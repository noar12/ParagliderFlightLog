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

        static LogBookCommand()
        {
            s_ImportIGC_Command = new RoutedUICommand("Import an IGC file in the DB","ImportIGC",typeof(LogBookCommand));
        }
        public static RoutedUICommand ImportIGC_Command => s_ImportIGC_Command;
    }
}
