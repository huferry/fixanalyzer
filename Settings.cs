using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;

namespace FixAnalyzer
{
    public class Settings
    {
        static private Settings instance = Load();

        public LogDialog LogDialog = new LogDialog();

        static private string GetFileName()
        {
            return Application.ExecutablePath + ".config.xml";            
        }

        public void Save()
        {
            XmlSerializer serial = new XmlSerializer(typeof(Settings));
            FileStream file = new FileStream(GetFileName(), FileMode.Create);
            try
            {
                serial.Serialize(file, this);
            }
            finally
            {
                file.Close();
            }
        }

        static private Settings Load()
        {
            XmlSerializer serial = new XmlSerializer(typeof(Settings));
            Settings settings = null;

            if (File.Exists(GetFileName()))
            {
                FileStream file = new FileStream(GetFileName(), FileMode.Open);
                try
                {
                    settings = (Settings)serial.Deserialize(file);
                }
                finally
                {
                    file.Close();
                }
            }
            if (settings == null)
                settings = new Settings();

            return settings;
        }

        static public Settings Instance
        {
            get { return instance; }
        }
    }

    public class LogDialog
    {
        public int LastEnvironment = -1;
        public int LastGateway = -1;
        public int LastTime = -1;
    }
}
