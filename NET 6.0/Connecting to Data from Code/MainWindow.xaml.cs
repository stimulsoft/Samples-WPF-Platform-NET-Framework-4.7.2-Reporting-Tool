using Stimulsoft.Base;
using Stimulsoft.Report;
using System.Data;
using System.Windows;

namespace Connecting_to_Data_from_Code
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

        private void ShowReport(DataSet dataSet)
        {
            var report = new StiReport();
            report.Load(@"Reports\TwoSimpleLists.mrt");
            report.Dictionary.Databases.Clear();
            report.RegData("Demo", dataSet);
            report.ShowWithWpf();
        }

        private void ButtonXML_Click(object sender, RoutedEventArgs e)
        {
            var dataSet = new DataSet();
            dataSet.ReadXmlSchema(@"Data\Demo.xsd");
            dataSet.ReadXml(@"Data\Demo.xml");

            ShowReport(dataSet);
        }

        private void ButtonJSON_Click(object sender, RoutedEventArgs e)
        {
            var dataSet = StiJsonToDataSetConverterV2.GetDataSetFromFile(@"Data\Demo.json");

            ShowReport(dataSet);
        }
    }
}
