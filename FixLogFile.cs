namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public enum LogDirection
    {
        In,
        Out
    };

    public abstract class FixLogFile
    {
        private readonly List<LogLine> lines = new List<LogLine>();

        public IEnumerable<LogLine> Lines
        {
            get { return this.lines; }
        }

        protected void AddLine(LogLine line)
        {
            this.lines.Add(line);
        }

        protected void ClearLines()
        {
            this.lines.Clear();
        }

        public static FixLogFile FromCmfLog(string fileName)
        {
            return new CmfLogFile(fileName);
        }

        public static FixLogFile FromDatabase(Gateway gateway, DbSettings settings, TimeSpan from)
        {
            return new DbLogFile(gateway, settings, from);
        }

        public abstract void Refresh();

        public virtual bool IsChanged()
        {
            return false;
        }
    }

    public class LogLine
    {
        private readonly LogDirection direction;
        private readonly string fixml;
        private DateTime datetime;
        private FixInterpreter interpreter;
        private FixMessage msg;

        public LogLine(string logline, LogDirection direction, string fixml)
        {
            this.fixml = fixml;
            this.direction = direction;
            Parse(logline);
        }

        public LogLine(DateTime timestamp, LogDirection direction, string fixml)
        {
            this.fixml = fixml;
            this.direction = direction;
            this.datetime = timestamp;
        }

        public DateTime TimeStamp
        {
            get { return this.datetime; }
        }

        public string Fixml
        {
            get { return this.fixml; }
        }

        public LogDirection Direction
        {
            get { return this.direction; }
        }

        public FixMessage FixMessage
        {
            get
            {
                if (this.msg == null)
                    this.msg = new FixMessage(this.fixml);
                return this.msg;
            }
        }

        public string Interpretation
        {
            get
            {
                if (this.interpreter == null)
                    this.interpreter = FixInterpreter.FromMessage(FixMessage);
                return this.interpreter.Interpretation;
            }
        }

        private void Parse(string logline)
        {
            // CMF timestamp
            Regex regex = new Regex("cpp:\\d+\\s(.+)UTC");
            string[] splits = regex.Split(logline);
            if (splits.Length > 1)
            {
                this.datetime = ParseDateTime(splits[1].Trim());
            }
            else // retail fix service timestamp
            {
                Match match = Regex.Match(logline, @"\[(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})\.(\d{2,3})");
                // try again
                if (!match.Success)
                {
                    match = Regex.Match(logline, @"\;(\d{4})-(\d{2})-(\d{2})\;(\d{2}):(\d{2}):(\d{2})\.(\d{2,3})");
                }

                if (match.Success)
                {
                    try
                    {
                        this.datetime = new DateTime(
                            Int32.Parse(match.Groups[1].Value),
                            Int32.Parse(match.Groups[2].Value),
                            Int32.Parse(match.Groups[3].Value),
                            Int32.Parse(match.Groups[4].Value),
                            Int32.Parse(match.Groups[5].Value),
                            Int32.Parse(match.Groups[6].Value),
                            Int32.Parse(match.Groups[7].Value)
                            );
                    }
                    catch (Exception)
                    {
                        this.datetime = new DateTime(0);
                    }
                }
            }
        }

        private DateTime ParseDateTime(string log)
        {
            // separate date from time
            string[] parts = log.Split(' ');

            string[] dateStr = parts[0].Split('/');
            string[] timeStr = parts[1].Split(':');

            try
            {
                return new DateTime(
                    Int32.Parse(dateStr[2]),
                    Int32.Parse(dateStr[1]),
                    Int32.Parse(dateStr[0]),
                    Int32.Parse(timeStr[0]),
                    Int32.Parse(timeStr[1]),
                    Int32.Parse(timeStr[2]));
            }
            catch
            {
                return new DateTime(0);
            }
        }

        public override string ToString()
        {
            return this.datetime.ToString("dd-MMM-yyyy | hh:mm:ss | ") + this.direction.ToString() +
                   " | " + this.fixml.Replace((char) 1, ':');
        }

        public string ToJson()
        {
            string time = "\"Time\":\"" + this.datetime.ToString("yyyy-MM-dd hh:mm:ss") + "\"";
            string dir = "\"Dir\":\"" + this.direction.ToString() + "\"";
            string interp = "\"Interpret\":\"" + Interpretation + "\"";
            string fix = this.msg.ToJson();

            return "{" + time + "," + dir + "," + fix + "," + interp + "}";
        }

        public override bool Equals(object obj)
        {
            return (obj is LogLine) && obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    internal class CmfLogFile : FixLogFile
    {
        private readonly string filename;
        private FileInfo lastFileStatus;

        public CmfLogFile(string fileName)
        {
            this.filename = fileName;
            Refresh();
        }

        public override sealed void Refresh()
        {
            this.lastFileStatus = new FileInfo(this.filename);
            ClearLines();
            FileStream file = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(file);
            while (!reader.EndOfStream)
            {
                ReadLog(reader.ReadLine(), reader);
            }
            file.Close();
        }

        public override bool IsChanged()
        {
            FileInfo info = new FileInfo(this.filename);
            return info.Length > this.lastFileStatus.Length;
        }

        private void ReadLog(string content, StreamReader reader)
        {
            string[] excluded = {"A", "1", "0", "2", "4", "5"};
            if ((content.IndexOf("UTC Message out", System.StringComparison.Ordinal) > 0) || content.Contains("CEST Message out ("))
            {
                LogLine line = new LogLine(content, LogDirection.Out, reader.ReadLine());
                if ((line.FixMessage[35] != null) &&
                    (!excluded.Contains(line.FixMessage[35].Value))
                    )
                {
                    AddLine(line);
                }
            } 
                // reading Topline ProRealTime or Retail Fix Service
            else if (Regex.IsMatch(content, "8\\=FIX.{50,1000}") &&
                     (!Regex.IsMatch(content, "\\[ACCEPTOR") || !Regex.IsMatch(content, @"\[Warn\]")))
            {
                LogDirection dir = Regex.IsMatch(content, "\\;Out") || Regex.IsMatch(content, "toApp\\:")
                    ? LogDirection.Out
                    : LogDirection.In;
                Match match = Regex.Match(content, "8\\=FIX.{50,1000}");
                // left out the incoming 'j' message
                if (match.Success)
                {
                    LogLine line = new LogLine(content, dir, match.Value);
                    if (!excluded.Contains(line.FixMessage.TagValue(35)))
                    {
                        AddLine(line);
                    }
                }
            }
            else if ((content.IndexOf("UTC msg in (", System.StringComparison.Ordinal) > 0) || content.Contains("CEST msg in ("))
            {
                LogLine line = new LogLine(content, LogDirection.In, reader.ReadLine());
                if ((line.FixMessage[35] != null) &&
                    (!excluded.Contains(line.FixMessage[35].Value))
                    )
                {
                    AddLine(line);
                }
            }
        }

        public override string ToString()
        {
            FileInfo file = new FileInfo(this.filename);
            return string.Format("CMF log file: {0} - File date/time: {1}", file.Name, file.LastWriteTime);
        }
    }

    internal class DbLogFile : FixLogFile
    {
        private readonly TimeSpan from;
        private readonly Gateway gateway;
        private readonly DbSettings settings;

        public DbLogFile(Gateway gateway, DbSettings settings, TimeSpan from)
        {
            this.gateway = gateway;
            this.from = from;
            this.settings = settings;
            Refresh();
        }

        public override sealed void Refresh()
        {
            ClearLines();
            DateTime timestamp = DateTime.Now.Subtract(this.@from);
            CmfLog cmfLog = new CmfLog(this.gateway, this.settings);
            DataTable log = cmfLog.GetLogDataSet(timestamp);
            foreach (DataRow row in log.Rows)
            {
                AddLine(new LogLine(row.Field<DateTime>("submit_time"), LogDirection.Out, row.Field<string>("msg")));
            }
        }

        public override string ToString()
        {
            return string.Format("Database log for {0} - Since {1}", this.gateway, DateTime.Now.Subtract(this.@from));
        }
    }
}