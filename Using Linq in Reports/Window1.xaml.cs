using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Wpf_Linq
{
    public partial class Window1 : Window
    {
        private StiReport report = new StiReport();
        public Window1()
        {
            // How to Activate
            //Stimulsoft.Base.StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnO...";
            //Stimulsoft.Base.StiLicense.LoadFromFile("license.key");
            //Stimulsoft.Base.StiLicense.LoadFromStream(stream);

            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            Stimulsoft.Report.Wpf.StiThemesHelper.LoadTheme(this);
            InitializeComponent();

            Item[] items = new Item[] { new Book{Id = 1, Price = 13.50, Genre = "Comedy", Author = "Jim Bob"}, 
                                        new Book{Id = 2, Price = 8.50, Genre = "Drama", Author = "John Fox"},  
                                        new Movie{Id = 1, Price = 22.99, Genre = "Comedy", Director = "Phil Funk"},
                                        new Movie{Id = 1, Price = 13.40, Genre = "Action", Director = "Eddie Jones"}};


            var query1 = from i in items
                         where i.Price > 9.99
                         orderby i.Price
                         select i;

            report.Load("..\\Report.mrt");

            report.RegBusinessObject("MyData", "MyData", query1);
        }



        private void btPreview_Click(object sender, RoutedEventArgs e)
        {
            report.ShowWithWpf();
        }

        private void btDesign_Click(object sender, RoutedEventArgs e)
        {
            report.DesignWithWpf();
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}