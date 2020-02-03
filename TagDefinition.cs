namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class TagDefinition
    {
        public static string FixmatePath = "\\fixmate\\en\\";
        public static string FixVersion = "FIX.4.4";
        private static string exe_dir = "";

        private string dataType;
        private string description = "";
        private string fieldName = "";
        private List<MessageDefinition> messages = new List<MessageDefinition>();
        private int tag;
        private Dictionary<string, string> validValues = new Dictionary<string, string>();
        private string xmlName;

        public TagDefinition(int tag)
        {
            this.tag = tag;
            try
            {
                Stream fixmatefile = GetFixmateTagFile(tag);
                StreamReader reader = new StreamReader(fixmatefile);
                ReadFile(reader.ReadToEnd());
                fixmatefile.Close();
            }
            catch
            {
            }
        }

        public string FieldName
        {
            get { return this.fieldName; }
        }

        public string XmlName
        {
            get { return this.xmlName; }
        }

        public string DataType
        {
            get { return this.dataType; }
        }

        public string Description
        {
            get { return this.description; }
        }

        public int Tag
        {
            get { return this.tag; }
        }

        public IDictionary<string, string> ValidValues
        {
            get { return this.validValues; }
        }

        public IEnumerable<MessageDefinition> Messages
        {
            get { return this.messages; }
        }

        private static string GetTagFile(int tag)
        {
            if (exe_dir.Length == 0)
            {
                exe_dir = Directory.GetCurrentDirectory();
            }
            string path = exe_dir + FixmatePath +
                          FixVersion + "\\tag" + tag.ToString() + ".html";

            return path;
        }

        private static Stream GetFixmateTagFile(int tag)
        {
            string file = GetTagFile(tag);
            return new FileStream(file, FileMode.Open);
        }

        private void ReadFile(string content)
        {
            // get the content between <tr> and </tr>
            string[] split1 = content.Split(new string[] {"<tr xmlns=\"\">"}, StringSplitOptions.None);
            if (split1.Length > 1)
            {
                string[] split2 = split1[1].Split(new string[] {"<hr>"}, StringSplitOptions.None);
                if (split2.Length > 1)
                {
                    ReadAllData(split2[0]);
                }
            }

            // read links - used in messages
            string[] splits = content.Split(new string[] {"Used in messages:", "Used in components:"},
                StringSplitOptions.None);
            if (splits.Length > 2)
            {
                ReadUsedInMessages(splits[1]);
            }

            // read used in components
            if (this.tag == 54)
            {
                FixmateLink.GetUsedInComponents(content);
            }
        }

        private void ReadAllData(string data)
        {
            // read description
            string s = data;
            int index = 0;
            while (s.Length > 0)
            {
                string field;
                int i = s.IndexOf("</td>");

                if (index == 5)
                    i = s.IndexOf("<table>");

                if ((i > 0) && (index < 6))
                {
                    field = s.Substring(0, i + 5);
                    s = s.Remove(0, i + 5);
                }
                else
                {
                    field = s;
                    s = "";
                }
                ReadData(field, index++);
            }
        }

        private void ReadUsedInMessages(string data)
        {
            Regex regex = new Regex(">(\\w+)<");
            Regex link = new Regex("href=\"(.+)\\?");
            string[] splits = regex.Split(data);
            int i = 0;
            while (i < splits.Length - 1)
            {
                string[] link_splits = link.Split(splits[i]);
                if (link_splits.Length > 1)
                {
                    this.messages.Add(new MessageDefinition(splits[i + 1], link_splits[1]));
                }
                i += 2;
            }
        }

        private void ReadData(string data, int index)
        {
            string value = ParseData(data);
            switch (index)
            {
                case 0:
                    if (Int16.Parse(value) != this.tag)
                    {
                        throw new Exception("error in parsing fixmate file");
                    }
                    break;
                case 1:
                    this.fieldName = value;
                    break;
                case 2:
                    this.xmlName = value;
                    break;
                case 3:
                    this.dataType = value;
                    break;
                case 4:
                    this.description = value;
                    break;
                case 6:
                    ReadValidValues(data);
                    break;
            }
        }

        private void ReadValidValues(string values)
        {
            if (values.Length > 0)
            {
                this.validValues.Clear();

                List<string> val_splits = new List<string>();

                string iter = values;

                while (iter.Length > 0)
                {
                    int idx = iter.IndexOf('>');
                    if (idx > -1)
                    {
                        iter = iter.Remove(0, idx + 1);
                    }

                    string s = "";
                    while ((iter.Length > 0) && (iter[0] != '<'))
                    {
                        s += iter[0];
                        iter = iter.Substring(1);
                    }

                    s = s.Replace('=', ' ').Trim();

                    if (s.Length > 0)
                        val_splits.Add(s);
                }

                int i = 0;
                while (i < val_splits.Count - 1)
                {
                    this.validValues.Add(val_splits[i], val_splits[i + 1]);
                    i += 2;
                }
            }
        }

        private string ParseData(string data)
        {
            Regex regex = new Regex(">(\\w|@)");

            string[] split1 = regex.Split(data);

            if (split1.Length > 1)
            {
                string value = data.Remove(0, split1[0].Length + 1);
                int i = value.IndexOf("<");
                return value.Substring(0, i);
            }

            return "";
        }

        public string ValueMeaning(string value)
        {
            if (this.validValues.Count == 0)
            {
                return "";
            }
            if (this.validValues.Keys.Contains(value))
            {
                return this.validValues[value];
            }
            else
                return "invalid";
        }
    }
}