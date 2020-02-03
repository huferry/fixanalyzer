namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Windows.Forms;
    using SourceGrid;
    using SourceGrid.Cells;
    using SourceGrid.Selection;

    public partial class FormLogFile : Form
    {
        private const string MarkChanged = "[changed] ";

        private readonly SourceGrid.Cells.Views.Cell evenRow = new SourceGrid.Cells.Views.Cell
        {
            BackColor = Color.FromArgb(40, Color.LightGreen)
        };

        private readonly List<Filter> filters = new List<Filter>();
        private readonly FixLogFile logfile;
        private readonly IFixMessageViewer viewer;
        private IEnumerable<LogLine> lines;

        public FormLogFile()
        {
            InitializeComponent();
            InitGrid();
        }


        public FormLogFile(IFixMessageViewer viewer, FixLogFile logFile)
        {
            this.logfile = logFile;
            this.viewer = viewer;
            InitializeComponent();
            InitGrid();
            this.lines = logFile.Lines;
            LoadLines();

            IOrderedEnumerable<string> qryClOrdId = this.lines
                .Where(x => x.FixMessage[11] != null)
                .GroupBy(x => x.FixMessage[11].Value)
                .Select(x => x.Key)
                .OrderBy(x => x);

            this.comboBoxClOrdId.Items.Add("");
            foreach (string id in qryClOrdId)
            {
                if (!this.comboBoxClOrdId.Items.Contains(id))
                    this.comboBoxClOrdId.Items.Add(id);
            }

            IObservable<Filter> filterChanged = Observable
                .FromEventPattern<EventArgs>(this.textBoxMessageType, "TextChanged")
                .Select(x => ((TextBox) x.Sender).Text.ToUpper())
                .DistinctUntilChanged()
                .Select(x => new FitlerMessageType {Value = x} as Filter)
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Merge(Observable.FromEventPattern<EventArgs>(
                    this.comboBoxClOrdId, "SelectedIndexChanged").Select(
                        x =>
                            new FilterClOrdId
                            {
                                Value = ((System.Windows.Forms.ComboBox) x.Sender).SelectedItem.ToString()
                            } as Filter))
                .Merge(Observable.FromEventPattern<EventArgs>(
                    this.textBoxKeywords, "TextChanged").Select(
                        x => ((TextBox) x.Sender).Text.ToUpper())
                    .Throttle(TimeSpan.FromMilliseconds(700))
                    .DistinctUntilChanged()
                    .Select(x => new FilterKeywords {Value = x} as Filter))
                .ObserveOn(Scheduler.CurrentThread);

            this.grid1.Selection.SelectionChanged += Selection_SelectionChanged;

            filterChanged.Subscribe(HandleFilterChanged);
        }

        private void Selection_SelectionChanged(object sender, RangeRegionChangedEventArgs e)
        {
            int x = (sender as IGridSelection).ActivePosition.Row;
            if (x > -1)
            {
                LogLine line = (LogLine) this.grid1[x, 0].Tag;
                this.viewer.ViewMessage(line.FixMessage);
            }
        }


        public void InitGrid()
        {
            this.grid1.BorderStyle = BorderStyle.FixedSingle;
            this.grid1.ColumnsCount = 4;
            this.grid1.FixedRows = 1;
            this.grid1.Rows.Insert(0);
            this.grid1[0, 0] = new SourceGrid.Cells.ColumnHeader("Date");
            this.grid1[0, 1] = new SourceGrid.Cells.ColumnHeader("Time");
            this.grid1[0, 2] = new SourceGrid.Cells.ColumnHeader("In/Out");
            this.grid1[0, 3] = new SourceGrid.Cells.ColumnHeader("FIX");

            this.grid1.AutoScroll = false;
            this.grid1.HScrollBar.Visible = false;
            grid1_Resize(this.grid1, null);
        }

        private void HandleFilterChanged(Filter filter)
        {
            List<Filter> qryExistingFilter = this.filters
                .Where(x => x.GetType() == filter.GetType())
                .ToList();
            foreach (Filter f in qryExistingFilter)
            {
                this.filters.Remove(f);
            }

            this.filters.Add(filter);

            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(LoadLines));
            }
        }

        public void CellEvent(CellContext c, EventArgs e)
        {
            Close();
        }

        private IEnumerable<LogLine> FilteredLines()
        {
            return this.lines
                .Where(line => this.filters.All(filter => filter.Pass(line.FixMessage)));
        }

        private void LoadLines()
        {
            Text = this.logfile.ToString();

            while (this.grid1.Rows.Count > 1)
            {
                this.grid1.Rows.Remove(1);
            }

            IEnumerable<LogLine> selected = FilteredLines();
            foreach (LogLine line in selected)
            {
                AddLineToGrid(line);
            }
        }

        private void AddLineToGrid(LogLine line)
        {
            int r = this.grid1.RowsCount;
            this.grid1.Rows.Insert(r);
            this.grid1[r, 0] = new SourceGrid.Cells.Cell(line.TimeStamp.ToShortDateString());
            this.grid1[r, 1] = new SourceGrid.Cells.Cell(line.TimeStamp.TimeOfDay.ToString());
            this.grid1[r, 2] = new SourceGrid.Cells.Cell(line.Direction == LogDirection.In ? "IN" : "OUT");
            this.grid1[r, 3] = new SourceGrid.Cells.Cell(line.Interpretation);
            this.grid1[r, 0].Tag = line;

            if (r%2 == 0)
            {
                for (int i = 0; i < 4; i++)
                    this.grid1[r, i].View = this.evenRow;
            }
        }

        private void UpdateChangesToGrid()
        {
            // is the selected row the last row?
            int[] selectedRows =
                this.grid1.Selection.GetSelectionRegion().GetRowsIndex().OrderBy(x => -x).ToArray();
            bool isLastRowSelected = selectedRows.Length == 1 && selectedRows[0] == this.grid1.RowsCount - 1;

            int[] displayedLines =
                this.grid1.GetCellsAtColumn(0)
                    .Select(c => ((c as ICell).Tag as LogLine))
                    .Where(t => t != null)
                    .Select(line => line.GetHashCode())
                    .ToArray();

            List<LogLine> newLines = FilteredLines()
                .Where(line => !displayedLines.Contains(line.GetHashCode()))
                .ToList();

            newLines.ForEach(AddLineToGrid);

            // when the last row is selected before the update, 
            // select the last row after the update too
            if (isLastRowSelected)
            {
                this.grid1.Selection.FocusRow(this.grid1.RowsCount - 1);
            }
        }

        private void FormLogFile_Shown(object sender, EventArgs e)
        {
            Timer checkUpdateTimer = new Timer {Interval = 5000, Enabled = true};
            checkUpdateTimer.Tick += (o, args) =>
            {
                if (this.logfile.IsChanged())
                {
                    string title = Text;
                    if (!title.StartsWith(MarkChanged))
                    {
                        title = MarkChanged + title;
                    }
                    Text = title;

                    if (this.checkBoxAutoUpdate.Checked)
                    {
                        button1_Click(null, null);
                    }
                }
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.logfile.Refresh();
            this.lines = this.logfile.Lines;
            UpdateChangesToGrid();
            Text = Text.Replace(MarkChanged, string.Empty);
        }

        private void grid1_Resize(object sender, EventArgs e)
        {
            this.grid1.Columns[0].Width = 80;
            this.grid1.Columns[1].Width = 80;
            this.grid1.Columns[2].Width = 50;
            this.grid1.Columns[3].Width = this.grid1.Width - 230;
        }

        private IEnumerable<LogLine> GetSelectedLines()
        {
            for (int i = 1; i < this.grid1.Rows.Count; i++)
            {
                LogLine line = (LogLine) this.grid1[i, 0].Tag;
                if (line.FixMessage[11] != null)
                    yield return line;
            }
        }

        private IEnumerable<FixMessage> GetSelectedMessages()
        {
            RangeRegion region = this.grid1.Selection.GetSelectionRegion();
            for (int i = 1; i < this.grid1.Rows.Count; i++)
            {
                if (region.ContainsRow(i))
                {
                    LogLine line = (LogLine) this.grid1[i, 0].Tag;
                    yield return line.FixMessage;
                }
            }
        }

        private void btnCopyAll_Click(object sender, EventArgs e)
        {
            string content = "";
            for (int i = 1; i < this.grid1.Rows.Count; i++)
            {
                LogLine line = (LogLine) this.grid1[i, 0].Tag;
                content += line.TimeStamp.ToLongTimeString() + "  " + line.Interpretation + "\r\n" + line.Fixml +
                           "\r\n\r\n";
            }
            Clipboard.SetText(content);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            // reading template
            string templateFile = Path.ChangeExtension(Application.ExecutablePath, ".html");
            StreamReader templateReader = new StreamReader(templateFile);
            string template = templateReader.ReadToEnd();
            string content = string.Empty;

            FileStream file = new FileStream("d:\\fixlog.html", FileMode.Create);
            StreamWriter writer = new StreamWriter(file);

            foreach (LogLine line in GetSelectedLines())
            {
                content += content == "" ? "" : ",";
                content += line.ToJson();
            }

            content = "{\"Logs\":[" + content + "]}";

            writer.WriteLine(template.Replace("#json_log#", content));

            writer.Close();
            file.Close();
            templateReader.Close();
        }

        private void buttonCompare_Click(object sender, EventArgs e)
        {
            FixMessage[] messages = GetSelectedMessages().ToArray();

            if (messages.Length == 1)
            {
                this.viewer.SetCompare(messages.First());
            }

            if (messages.Length > 1)
            {
                FixMessage[] items = messages.Take(2).ToArray();
                this.viewer.ViewCompare(items[0], items[1]);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FixMessage[] messages = GetSelectedMessages().ToArray();
            if (messages.Length == 1)
            {
                this.comboBoxClOrdId.Text =
                    messages.First().TagValue(11);
            }
        }
    }

    internal abstract class Filter
    {
        public string Value;

        public abstract bool Pass(FixMessage message);
    }

    internal class FilterClOrdId : Filter
    {
        public override bool Pass(FixMessage message)
        {
            return string.IsNullOrEmpty(this.Value) ||
                   (message[11] != null) && (message[11].Value == this.Value);
        }
    }

    internal class FitlerMessageType : Filter
    {
        public override bool Pass(FixMessage message)
        {
            string[] msgTypes =
                this.Value.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.ToUpperInvariant())
                    .ToArray();

            return string.IsNullOrEmpty(this.Value) ||
                   message[35] != null &&
                   msgTypes.Contains(message[35].Value.ToUpperInvariant());
        }
    }

    internal class FilterKeywords : Filter
    {
        public override bool Pass(FixMessage message)
        {
            string[] words = this.Value.ToUpper().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            string raw = message.Raw.ToUpper();
            int filter = words
                .Where(word => !raw.Contains(word))
                .Count();

            return filter == 0;
        }
    }
}