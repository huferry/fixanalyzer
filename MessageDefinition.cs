namespace FixAnalyzer
{
    public class MessageDefinition
    {
        private readonly string name;

        public MessageDefinition(string name, string fixmate_file)
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }
    }
}