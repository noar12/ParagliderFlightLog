using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfUI
{
    public static class LogBookCommand
    {
        private static RoutedUICommand s_ImportIGC_Command;
        private static RoutedUICommand s_ImportLogFlyDbCommand;
        private static RoutedUICommand s_RemoveFlightCommand;
        private static RoutedUICommand s_EditSiteCommand;
        private static RoutedUICommand s_StatisticAnalyzeCommand;
        private static RoutedUICommand s_EditFlightCommand;

        static LogBookCommand()
        {
            s_ImportIGC_Command = new RoutedUICommand("Import an IGC file in the DB","ImportIGC",typeof(LogBookCommand));
            s_ImportLogFlyDbCommand = new RoutedUICommand("Import flight, site and glider from a LogFly DB", "ImportLogFlyDb", typeof(LogBookCommand));
            s_RemoveFlightCommand = new RoutedUICommand("Remove a flight from the data base", "RemoveFlight", typeof(LogBookCommand));
            s_EditSiteCommand = new RoutedUICommand("Edit a site in the data base", "EditSite", typeof(LogBookCommand));
            s_StatisticAnalyzeCommand = new RoutedUICommand("Analyze the flight", "StatisticAnalyze", typeof(LogBookCommand));
            s_EditFlightCommand = new RoutedUICommand("Edit the flight", "EditFlight", typeof(LogBookCommand));

        }
        public static RoutedUICommand ImportIGC_Command => s_ImportIGC_Command;
        public static RoutedUICommand ImportLogFlyDbCommand => s_ImportLogFlyDbCommand;
        public static RoutedUICommand RemoveFlightCommand => s_RemoveFlightCommand;
        public static RoutedUICommand EditSiteCommand => s_EditSiteCommand;
        public static RoutedUICommand StatisticAnalyzeCommand => s_StatisticAnalyzeCommand;
        public static RoutedUICommand EditFlightCommand => s_EditFlightCommand;

    }
}
