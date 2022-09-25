using System;
using ParagliderFlightLog.DataModels;

namespace ParagliderFlightLog // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Settings settings = new Settings();
            settings.Build();
            FlightLogDB db = new FlightLogDB(settings);
            LogFlyDB logFlyDB = new LogFlyDB(settings);

            logFlyDB.LoadLogFlyDB("Logfly.db");
            db = logFlyDB.BuildFlightLogDB();


        }
    }
}