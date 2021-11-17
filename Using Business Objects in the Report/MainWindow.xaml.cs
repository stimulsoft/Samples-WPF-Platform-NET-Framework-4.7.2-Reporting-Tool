using BusinessObjects;
using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Using_Business_Objects_in_the_Report
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // How to Activate
            //Stimulsoft.Base.StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnO...";
            //Stimulsoft.Base.StiLicense.LoadFromFile("license.key");
            //Stimulsoft.Base.StiLicense.LoadFromStream(stream);

            InitializeComponent();
        }

        private void btDesignIEnumerable_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.RegData("EmployeeIEnumerable", CreateBusinessObjectsIEnumerable.GetEmployees());
            report.Load("Reports/BusinessObjects_IEnumerable.mrt");
            report.DesignV2WithWpf();
        }

        private void btPreviewIEnumerable_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.RegData("EmployeeIEnumerable", CreateBusinessObjectsIEnumerable.GetEmployees());
            report.Load("Reports/BusinessObjects_IEnumerable.mrt");
            report.ShowWithWpf();
        }

        private void btDesignITypedList_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.RegData("EmployeeITypedList", CreateBusinessObjectsITypedList.GetEmployees());
            report.Load("Reports/BusinessObjects_ITypedList.mrt");
            report.DesignV2WithWpf();
        }

        private void btPreviewITypedList_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.RegData("EmployeeITypedList", CreateBusinessObjectsITypedList.GetEmployees());
            report.Load("Reports/BusinessObjects_ITypedList.mrt");
            report.ShowWithWpf();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
