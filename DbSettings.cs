namespace FixAnalyzer
{
    using System.Collections.Generic;

    public class DbSettings
    {
        public string Desc;
        public string Name;
        public string Password;
        public string User;

        private DbSettings(string desc, string name, string user, string password)
        {
            this.Desc = desc;
            this.Name = name;
            this.User = user;
            this.Password = password;
        }

        public override string ToString()
        {
            return this.Desc;
        }

        public static IEnumerable<DbSettings> GetAll()
        {
            return new DbSettings[]
            {
                new DbSettings("T7", "TOT7", "lees", "read"),
                new DbSettings("T4", "TOT4", "lees", "read"),
                new DbSettings("Alex Schaduw", "TOPS", "lees", "read"),
                new DbSettings("Alex Acceptatie 1", "TOA3", "lees", "read")
            };
        }
    }
}