namespace FixAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public interface IFixMessageViewer
    {
        void ViewMessage(FixMessage message);
        void SetCompare(FixMessage message);
        void ViewCompare(FixMessage msg1, FixMessage msg2);
    }

    public class FixMessage
    {
        private const string TestFixMessage =
            "8=FIX.4.435=AB34=049=ALEXDERSTA56=QUODDERSTA11=40637833121=255=AEX461=OMXXXX167=MLEG207=LIFFE_K54=1762=SY60=20100208-08:08:5838=4344=-1215=EUR40=259=0528=A555=2600=AEX608=OPXXXX609=OPT610=201003612=1600.000000623=1624=1600=AEX608=OCXXXX609=OPT610=201003612=1600.000000623=1624=2";

        private readonly List<FixTag> fixTags = new List<FixTag>();
        private readonly string raw;

        public FixMessage(string msg)
        {
            Match match = Regex.Match(msg, "(8\\=FIX[T]{0,1}\\..{10,800})");
            if (match.Success)
            {
                string fixMsg = match.Groups[1].Value;
                char separator = FindSeparator(fixMsg);
                this.raw = fixMsg.Replace(separator, '|');
                string[] tags = fixMsg.Split(separator);
                this.fixTags.Clear();
                int pos = 0;
                foreach (string tag in tags)
                {
                    string[] split = tag.Split('=');
                    if (split.Length == 2)
                    {
                        try
                        {
                            int tagNumber = Int16.Parse(split[0]);
                            string value = split[1];
                            this.fixTags.Add(new FixTag(tagNumber, value, pos));
                        }
                        catch (Exception)
                        {
                        }
                    }
                    pos += tag.Length + 1;
                }
            }
            if (this[8] != null)
            {
                TagDefinition.FixVersion = this[8].Value;
            }
        }

        public IEnumerable<FixTag> Tags
        {
            get { return this.fixTags; }
        }

        public string Raw
        {
            get { return this.raw; }
        }

        public FixTag this[int tagNumber]
        {
            get
            {
                return this.fixTags
                    .FirstOrDefault(t => t.Tag == tagNumber);
            }
        }

        public static FixMessage Test()
        {
            return new FixMessage(TestFixMessage);
        }

        private char FindSeparator(string msg)
        {
            char separator = msg
                .FirstOrDefault(c => !Char.IsLetterOrDigit(c) && !Char.IsPunctuation(c) && (c != '='));
            return separator == default (char) ? (char) 1 : separator;
        }

        public string TagValue(int tagNumber)
        {
            FixTag tag = this[tagNumber];
            return tag == null ? "" : tag.Value;
        }

        public string ToJson()
        {
            string jtags = "";
            foreach (FixTag tag in this.fixTags)
            {
                jtags += jtags == "" ? "" : ",";
                jtags += tag.ToJson();
            }

            return "\"Tags\":[" + jtags + "]";
        }
    }
}