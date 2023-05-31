using Stimulsoft.Base;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Design;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.WpfDesign;
using System.Windows;

namespace Using_a_Custom_Data_Adapter
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

            //Clearing standard data adapters, if necessary
            StiOptions.Services.Databases.Clear();

            //Adding a Custom PostgreSQL data adapter
            StiOptions.Services.Databases.Add(new CustomPostgreSQLDatabase());
            StiOptions.Services.DataAdapters.Add(new CustomPostgreSQLAdapterService());
        }

        private void ButtonDesign_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            
            //Adding a connection to the report from code
            var database = new CustomPostgreSQLDatabase("CustomData1", "Server=127.0.0.1; Port=5432; Database=myDataBase; User Id=myUsername; Password=myPassword;");
            report.Dictionary.Databases.Add(database);

            report.DesignV2WithWpf();
        }
    }
}
