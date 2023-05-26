using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf_Linq;

namespace Using_Linq_in_Reports
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

        private StiReport GetReport()
        {
            Item[] items = new Item[] { new Book{Id = 1, Price = 13.50, Genre = "Comedy", Author = "Jim Bob"},
                                        new Book{Id = 2, Price = 8.50, Genre = "Drama", Author = "John Fox"},
                                        new Movie{Id = 1, Price = 22.99, Genre = "Comedy", Director = "Phil Funk"},
                                        new Movie{Id = 1, Price = 13.40, Genre = "Action", Director = "Eddie Jones"}};

            var query1 = from i in items
                         where i.Price > 9.99
                         orderby i.Price
                         select i;

            var report = new StiReport();
            report.Load("Reports/Report.mrt");
            report.RegBusinessObject("MyData", "MyData", query1);

            return report;
        }

        private void ButtonShow_Click(object sender, RoutedEventArgs e)
        {
            var report = GetReport();
            report.ShowWithWpf();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var report = GetReport();
            report.DesignV2WithWpf();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
