using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ParagliderFlightLog.DataModels
{
    public class Settings
    { 
        private readonly string m_SettingsFile = Path.Combine(Environment.CurrentDirectory, "Settings.json");
        public string DbPath { get; set; } = @"/home/noar/TestDB/ParagliderFlightLog.db";
        public void Build()
        {
            if (File.Exists(m_SettingsFile))
            {
                // read json settings and build settings with its content
                string jsonString = File.ReadAllText(m_SettingsFile);
                Settings settings = JsonSerializer.Deserialize<Settings>(jsonString) ?? new Settings();
                DbPath = settings.DbPath;
            }
            else
            {
                //write a json file with default value for the next time
                string jsonString = JsonSerializer.Serialize(this);
                File.WriteAllText(m_SettingsFile, jsonString);

            }
        }
    }
}
