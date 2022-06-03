using System;
using ParagliderFlightLog.DataModels;

namespace ParagliderFlightLog // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FlightLogDB db = new FlightLogDB();
            LogFlyDB logFlyDB = new LogFlyDB();

            logFlyDB.LoadLogFlyDB("Logfly.db");
            db = logFlyDB.BuildFlightLogDB();


        }
    }
}