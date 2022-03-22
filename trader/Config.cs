using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trader
{
    public class Config
    {
        private string configName = "config.ini";

        private string configPath = "";

        private FileIniDataParser parser;

        private IniData data;

        public Config()
        {
            this.configPath = Directory.GetCurrentDirectory() + "\\" + configName;

            parser = new FileIniDataParser();
            if (!File.Exists(this.configPath))
            {
                var f = File.Create(this.configPath);
                f.Dispose();
                f.Close();
            }

            this.data = parser.ReadFile(this.configPath);
        }

        public void WriteData(string key, string value)
        {
            data["Data"][key] = value;
            parser.WriteFile(this.configPath, data);
        }

        public string GetData(string key)
        {
            return data["Data"][key];
        }
    }
}
