using Stimulsoft.Report;
using System.Windows;

namespace Editing_a_Report_Template_in_the_Designer
{
    /// <summary>
    /// Interaction logic for DesignerWindow.xaml
    /// </summary>
    public partial class DesignerWindow : Window
    {
        public DesignerWindow()
        {
            InitializeComponent();
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            StiDesignerControl1.Report = new StiReport();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var report = new StiReport();
            report.Load(@"Reports\SimpleList.mrt");
            StiDesignerControl1.Report = report;
        }
    }
}
