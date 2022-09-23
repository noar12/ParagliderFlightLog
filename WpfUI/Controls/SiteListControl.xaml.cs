using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ParagliderFlightLog.ViewModels;

namespace WpfUI.Controls
{
    /// <summary>
    /// Interaction logic for SiteListControl.xaml
    /// </summary>
    public partial class SiteListControl : UserControl
    {
        
        public SiteListControl()
        {
            
            
            InitializeComponent();
            CommandManager.RegisterClassCommandBinding(typeof(SiteListControl),
                new CommandBinding(LogBookCommand.EditSiteCommand, OnEditSites,
                (s, e) => { e.CanExecute = SitesGrid.SelectedItems.Count == 1; }));
        }
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(MainViewModel), typeof(SiteListControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(SiteViewModel), typeof(SiteListControl), new PropertyMetadata(null));


        public int SiteUseCount
        {
            get { return (int)GetValue(SiteUseCountProperty); }
            set { SetValue(SiteUseCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SiteUseCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SiteUseCountProperty =
            DependencyProperty.Register("SiteUseCount", typeof(int), typeof(SiteListControl), new PropertyMetadata(null));


        //private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}
        public MainViewModel Source
        {
            get { return (MainViewModel)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public SiteViewModel SelectedItem
        {
            get { return (SiteViewModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private void AddSite_Click(object sender, RoutedEventArgs e)
        {
            SiteViewModel svm = new SiteViewModel();
            Forms.SiteForm AddSiteForm = new Forms.SiteForm(svm);


            if (AddSiteForm.ShowDialog() == true)
            {
                Source.AddSite(svm);
            }


        }

        private void OnEditSites(object sender, RoutedEventArgs e)
        {
            Forms.SiteForm EditSiteForm = new Forms.SiteForm(SelectedItem);

            if (EditSiteForm.ShowDialog() == true)
            {
                Source.EditSite(SelectedItem);
            }

            
            
        }

        private void SitesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SiteUseCount = SelectedItem.SiteUseCount;
        }
    }
}
