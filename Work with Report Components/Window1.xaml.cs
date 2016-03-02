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
using Stimulsoft.Report.Components;

namespace Work_with_Components
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
            stiReport1.Load("..\\..\\Work with Components.mrt");
            stiReport1.DesignWithWpf();
        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            stiReport1.Load("..\\..\\Work with Components.mrt");
            ((StiText)stiReport1.Pages["Page1"].Components["Text1"]).Text.Value =
                textBox1.Text;

            stiReport1.ShowWithWpf();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
