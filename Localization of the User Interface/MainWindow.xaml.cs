using Stimulsoft.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Localization_of_the_User_Interface
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

            foreach (var fileName in Directory.GetFiles(@"Localization"))
            {
                var label = new Label();
                label.Padding = new Thickness(0, 2, 0, 2);
                label.Content = fileName;
                ComboBoxLocalizations.Items.Add(label);

                if (fileName.EndsWith("en.xml"))
                    ComboBoxLocalizations.SelectedItem = label;
            }
        }

        private void LoadLocalization()
        {
            var fileName = (string)((Label)ComboBoxLocalizations.SelectedItem).Content;
            StiOptions.Localization.Load(fileName);
        }

        private void ButtonShow_Click(object sender, RoutedEventArgs e)
        {
            LoadLocalization();

            var report = new StiReport();
            report.Load(@"Reports\SimpleList.mrt");
            report.ShowWithWpf();
        }

        private void ButtonDesign_Click(object sender, RoutedEventArgs e)
        {
            LoadLocalization();

            var report = new StiReport();
            report.Load(@"Reports\SimpleList.mrt");
            report.DesignV2WithWpf();
        }
    }
}
