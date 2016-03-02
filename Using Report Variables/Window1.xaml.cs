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

namespace Variables
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private StiReport stiReport1 = new StiReport();

        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();
        }

        private void buttonDesign_Click(object sender, RoutedEventArgs e)
        {
            stiReport1.Load("..\\..\\Variables.mrt");
            stiReport1.DesignWithWpf();
        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            stiReport1.Load("..\\..\\Variables.mrt");
            stiReport1.Compile();
            //Set Variables
            stiReport1["Name"] = tbName.Text;
            stiReport1["Surname"] = tbSurname.Text;
            stiReport1["Email"] = tbEmail.Text;
            stiReport1["Address"] = tbAddress.Text;
            stiReport1["Sex"] = rbMale.IsChecked.GetValueOrDefault();

            stiReport1.ShowWithWpf();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
