using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Stimulsoft.Report;
using System.IO;

namespace SqlParameters
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private string path = string.Empty;
        //private string path = "D:\\";
        private StiReport stiReport1 = new StiReport();

        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();
            
            stiReport1.Load("..\\StiReport1.mrt");

            stiReport1.Dictionary.DataStore.Clear();

            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.Data.OleDb.OleDbConnection connection =
                new System.Data.OleDb.OleDbConnection(
                "Provider=Microsoft.Jet.OLEDB.4.0;User ID=Admin;Data Source=" + path + "\\..\\..\\Data\\Nwind.mdb");
            
            stiReport1.RegData("NorthWind", connection);
            stiReport1.Compile();

            this.Background = Stimulsoft.Report.Wpf.StiThemesHelper.GetBrush("WorkBackgroundBrush", this, null);
            stiPreviewControl1.Report = stiReport1;
        }

        private void buttonDesign_Click(object sender, RoutedEventArgs e)
        {
            stiReport1.DesignWithWpf();
            stiReport1.Compile();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Set parameters
            stiReport1["@countryID"] = ((ListBoxItem)lbCountries.SelectedItem).Content as string;

            stiReport1.Render(false);            
            stiPreviewControl1.InvokeZoomOnePage();
        }
    }
}
