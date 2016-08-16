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

namespace BusinessObjects
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();            
        }

        private void btDesignIEnumerable_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();
            report.RegData("EmployeeIEnumerable", CreateBusinessObjectsIEnumerable.GetEmployees());
            report.Load("..\\BusinessObjects_IEnumerable.mrt");
            report.DesignWithWpf();
        }

        private void btPreviewIEnumerable_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();
            report.RegData("EmployeeIEnumerable", CreateBusinessObjectsIEnumerable.GetEmployees());
            report.Load("..\\BusinessObjects_IEnumerable.mrt");
            report.ShowWithWpf();
        }

        private void btPreviewITypedList_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();
            report.RegData("EmployeeITypedList", CreateBusinessObjectsITypedList.GetEmployees());
            report.Load("..\\BusinessObjects_ITypedList.mrt");
            report.ShowWithWpf();
        }

        private void btDesignITypedList_Click(object sender, RoutedEventArgs e)
        {
            StiReport report = new StiReport();
            report.RegData("EmployeeITypedList", CreateBusinessObjectsITypedList.GetEmployees());
            report.Load("..\\BusinessObjects_ITypedList.mrt");
            report.DesignWithWpf();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
