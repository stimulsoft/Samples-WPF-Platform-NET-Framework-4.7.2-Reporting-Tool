using Microsoft.Win32;
using Stimulsoft.Report;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Printing_a_Report_from_Code
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SaveFileDialog saveFileDialog = new SaveFileDialog();

        public MainWindow()
        {
            // How to Activate
            //Stimulsoft.Base.StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnO...";
            //Stimulsoft.Base.StiLicense.LoadFromFile("license.key");
            //Stimulsoft.Base.StiLicense.LoadFromStream(stream);

            InitializeComponent();
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.Load(@"Reports\TwoSimpleLists.mrt");
            report.PrintWithWpf();

            MessageBox.Show("The print action is complete.", "Print Report");
        }
    }
}
