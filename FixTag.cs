namespace FixAnalyzer
{
    public class FixTag
    {
        private TagDefinition def;
        private readonly int pos;
        private readonly int tag;
        private readonly string value;
        private readonly FixTagValueEnricher enricher;

        public FixTag(int tag, string value)
        {
            this.tag = tag;
            this.value = value;
            this.enricher = FixTagValueEnricher.GetEnricher(tag);
        }

        public FixTag(int tag, string value, int position)
        {
            this.tag = tag;
            this.value = value;
            this.pos = position;
            this.enricher = FixTagValueEnricher.GetEnricher(tag);
        }

        public TagDefinition Definition
        {
            get
            {
                if (this.def == null)
                    this.def = new TagDefinition(this.tag);
                return this.def;
            }
        }

        public int Tag
        {
            get { return this.tag; }
        }

        public string Value
        {
            get
            {
                return this.enricher.Enrich(this.value);
            }
        }

        public string ValueMeaning
        {
            get { return Definition.ValueMeaning(this.value); }
        }

        public int Position
        {
            get { return this.pos; }
        }

        public override string ToString()
        {
            return this.tag.ToString() + " [" + Definition.FieldName + "]";
        }

        public string ToJson()
        {
            string jtag = "\"Tag\":" + this.tag.ToString();
            string jvalue = "\"Value\":\"" + this.value + "\"";
            string jname = "\"Name\":\"" + Definition.FieldName + "\"";
            return "{" + jtag + "," + jvalue + "," + jname + "}";
        }
    }
}