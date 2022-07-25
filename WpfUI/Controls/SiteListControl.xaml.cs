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
using WpfUI.ViewModels;

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
        }
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ICollection<SiteViewModel>), typeof(SiteListControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(SiteViewModel), typeof(SiteListControl), new PropertyMetadata(null));

        //private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{

        //}
        public ICollection<SiteViewModel> Source
        {
            get { return (ICollection<SiteViewModel>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public SiteViewModel SelectedItem
        {
            get { return (SiteViewModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private void AddSite_Click(object sender, RoutedEventArgs e)
        {
            Forms.AddSiteForm AddSiteForm = new Forms.AddSiteForm();
            AddSiteForm.DataContext = Source;
            AddSiteForm.ShowDialog();
            
        }
    }
}
