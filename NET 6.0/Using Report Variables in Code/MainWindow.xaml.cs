using Stimulsoft.Report;
using System.Windows;

namespace Using_Report_Variables_in_Code
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

        private void ButtonShow_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.Load(@"Reports\Variables.mrt");

            // Required to prepare the in compilation mode
            report.Compile();

            // Set Variables
            report["Name"] = TextBoxName.Text;
            report["Surname"] = TextBoxSurname.Text;
            report["Email"] = TextBoxEmail.Text;
            report["Address"] = TextBoxAddress.Text;
            report["Sex"] = RadioButtonMale.IsChecked.GetValueOrDefault();

            report.ShowWithWpf();
        }
    }
}
