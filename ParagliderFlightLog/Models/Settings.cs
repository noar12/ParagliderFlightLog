using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace ParagliderFlightLog.Models
{
    public class Settings
    { 
        private readonly string m_SettingsFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Settings.json");
        public void Build()
        {
            if (File.Exists(m_SettingsFile))
            {
                // read json settings and build settings with its content
                string jsonString = File.ReadAllText(m_SettingsFile);
                Settings settings = JsonSerializer.Deserialize<Settings>(jsonString) ?? new Settings();
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
