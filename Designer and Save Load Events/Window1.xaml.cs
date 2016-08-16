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

namespace WpfDesigner_SaveLoad
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            InitializeComponent();

            StiOptions.Engine.GlobalEvents.SavingReportInDesigner += new Stimulsoft.Report.Design.StiSavingObjectEventHandler(GlobalEvents_SavingReportInDesigner);
            StiOptions.Engine.GlobalEvents.LoadingReportInDesigner += new Stimulsoft.Report.Design.StiLoadingObjectEventHandler(GlobalEvents_LoadingReportInDesigner);
        }

        private void GlobalEvents_LoadingReportInDesigner(object sender, Stimulsoft.Report.Design.StiLoadingObjectEventArgs e)
        {
            e.Processed = true;

            StiReport report = new StiReport();
            report.Load("..\\SimpleList.mrt");
            designerControl1.Report = report;
        }

        private void GlobalEvents_SavingReportInDesigner(object sender, Stimulsoft.Report.Design.StiSavingObjectEventArgs e)
        {
            if (designerControl1.Report == null) return;
            e.Processed = true;

            designerControl1.Report.Save("Report.mrt");
        }
    }
}