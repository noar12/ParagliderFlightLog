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


        static LogBookCommand()
        {
            s_ImportIGC_Command = new RoutedUICommand("Import an IGC file in the DB","ImportIGC",typeof(LogBookCommand));
            s_ImportLogFlyDbCommand = new RoutedUICommand("Import flight, site and glider from a LogFly DB", "ImportLogFlyDb", typeof(LogBookCommand));
            s_RemoveFlightCommand = new RoutedUICommand("Remove a flight from the data base", "RemoveFlight", typeof(LogBookCommand));
            s_EditSiteCommand = new RoutedUICommand("Edit a site in the data base", "EditSite", typeof(LogBookCommand));

        }
        public static RoutedUICommand ImportIGC_Command => s_ImportIGC_Command;
        public static RoutedUICommand ImportLogFlyDbCommand => s_ImportLogFlyDbCommand;
        public static RoutedUICommand RemoveFlightCommand => s_RemoveFlightCommand;
        public static RoutedUICommand EditSiteCommand => s_EditSiteCommand;

    }
}
