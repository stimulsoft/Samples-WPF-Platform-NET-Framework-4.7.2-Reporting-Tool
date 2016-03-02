#region Copyright (C) 2003-2012 Stimulsoft
/*
{*******************************************************************}
{																	}
{	Stimulsoft Reports.Wpf											}
{	                         										}
{																	}
{	Copyright (C) 2003-2012 Stimulsoft     							}
{	ALL RIGHTS RESERVED												}
{																	}
{	The entire contents of this file is protected by U.S. and		}
{	International Copyright Laws. Unauthorized reproduction,		}
{	reverse-engineering, and distribution of all or any portion of	}
{	the code contained in this file is strictly prohibited and may	}
{	result in severe civil and criminal penalties and will be		}
{	prosecuted to the maximum extent possible under the law.		}
{																	}
{	RESTRICTIONS													}
{																	}
{	THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES			}
{	ARE CONFIDENTIAL AND PROPRIETARY								}
{	TRADE SECRETS OF Stimulsoft										}
{																	}
{	CONSULT THE END USER LICENSE AGREEMENT FOR INFORMATION ON		}
{	ADDITIONAL RESTRICTIONS.										}
{																	}
{*******************************************************************}
*/
#endregion Copyright (C) 2003-2012 Stimulsoft

using System.Linq;
using System.Windows;
using System.Xml;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.WpfDesign;
using Stimulsoft.Report.Viewer;
using Stimulsoft.Report.Wpf;
using Stimulsoft.Base.Localization;
using Microsoft.Win32;
using System.Data;
using System;
using Stimulsoft.Report.WCFService;
using System.Threading.Tasks;
using WCF_WpfDesigner.ServiceReference1;

namespace WCF_WpfDesigner
{
    public partial class MainWindow
    {
        #region Fields
        private StiProgressInformation progress;
        private StiWpfDesignerControl designer;
        #endregion

        #region Handlers

        #region RenderReport
        private Task<string> RenderReportTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RenderReport(xml);
            });
        }

        private async void WCFService_WCFRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                string result = await RenderReportTask(e.Xml);
                if (result != null && result.Length > 2)
                {
                    designer.ApplyRenderedReport(result);
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region TestConnection

        private Task<string> TestConnectionTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.TestConnection(xml);
            });
        }

        private async void WCFService_WCFTestConnection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "TestConnection") + "...");

            try
            {
                string result = await TestConnectionTask(e.Xml);

                if (result != null && result.Length > 2)
                {
                    ((IStiTestConnecting)sender).ApplyResultAfterTestConnection(result);
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region BuildObjects

        private Task<string> BuildObjectsTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.BuildObjects(xml);
            });
        }

        private async void WCFService_WCFBuildObjects(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("FormDictionaryDesigner", "RetrievingDatabaseInformation") + "...");

            try
            {
                string result = await BuildObjectsTask(e.Xml);

                if (!progress.IsBreaked)
                    ((StiSelectDataWindow)sender).ApplyResultAfterBuildObjects(result);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region RetrieveColumns

        private Task<string> RetrieveColumnsTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RetrieveColumns(xml);
            });
        }

        private async void WCFService_WCFRetrieveColumns(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("FormDictionaryDesigner", "RetrieveColumns") + "...");

            try
            {
                string result = await RetrieveColumnsTask(e.Xml);

                if (!progress.IsBreaked)
                {
                    ((StiDataStoreSourceEditWindow)sender).ApplyResultAfterRetrieveColumns(result);
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region Opening Report

        private Task<byte[]> LoadReportTask()
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.LoadReport();
            });
        }

        private async void WCFService_WCFOpeningReportInDesigner(object sender, Stimulsoft.Report.Events.StiWCFOpeningReportEventArgs e)
        {
            e.Handled = true;
            progress.Start(StiLocalization.Get("DesignerFx", "LoadingReport") + "...");

            try
            {
                var result = await LoadReportTask();

                if (result != null)
                {
                    var report = new StiReport();
                    report.Load(result);
                    designer.Report = report;
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region Saving Report

        private Task<bool> SaveReportTask(byte[] array)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.SaveReport(array);
            });
        }

        private async void GlobalEvents_SavingReportInDesigner(object sender, Stimulsoft.Report.Design.StiSavingObjectEventArgs e)
        {
            progress.Start(StiLocalization.Get("Report", "SavingReport") + "...");

            try
            {
                await SaveReportTask(designer.Report.SaveToByteArray());
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region Export Document

        private Task<byte[]> ExportDocumentTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.ExportDocument(xml);
            });
        }

        private async void WCFService_WCFExportDocument(object sender, Stimulsoft.Report.Events.StiWCFExportEventArgs e)
        {
            progress.Start("Export Report...");

            try
            {
                var result = await ExportDocumentTask(e.Xml);

                if (result != null)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = string.Format("Export Document (*.{0})|*.{0}", e.Filter)
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var stream = saveFileDialog.OpenFile();
                        stream.Write(result, 0, result.Length);

                        stream.Flush();
                        stream.Close();
                        stream.Dispose();
                        stream = null;
                    }
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region ReportCheck

        private Task<string> CheckReportTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.CheckReport(xml);
            });
        }

        private async void WCFService_WCFReportCheck(IStiCheckStatusControl sender, Stimulsoft.Report.Events.StiWCFReportCheckEventArgs e)
        {
            progress.Start("Report Check...");

            try
            {
                var result = await CheckReportTask(e.Xml);

                sender.ApplyResultAfterReportCheck(result);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }

        #endregion

        #region RenderingInteractions

        private Task<string> RenderingInteractionsTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RenderingInteractions(xml);
            });
        }

        private async void WCFService_WCFRenderingInteractions(object viewer, Stimulsoft.Report.Events.StiWCFRenderingInteractionsEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await RenderingInteractionsTask(e.Xml);

                if (result != null && result.Length > 2)
                {
                    ((StiWpfViewerControl)viewer).ApplyChangesAfterSorting(result);
                }
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region RequestFromUserRenderReport

        private Task<string> RequestFromUserRenderReportTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.RequestFromUserRenderReport(xml);
            });
        }

        private async void WCFService_WCFRequestFromUserRenderReport(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await RequestFromUserRenderReportTask(e.Xml);

                if (result != null && result.Length > 2)
                    ((StiWpfViewerControl)sender).ApplyRenderedReport(result, true);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #region WCFPrepareRequestFromUserVariables

        private Task<string> PrepareRequestFromUserVariablesTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.PrepareRequestFromUserVariables(xml);
            });
        }

        private async void WCFService_WCFPrepareRequestFromUserVariables(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start("Prepare Variables...");

            try
            {
                var result = await PrepareRequestFromUserVariablesTask(e.Xml);

                ((StiWpfViewerControl)sender).ApplyResultAfterPrepareRequestFromUserVariables(null, result);
            }
            catch (Exception ex)
            {
                ((StiWpfViewerControl)sender).ApplyResultAfterPrepareRequestFromUserVariables(ex, null);
            }

            progress.Hide();
        }

        #endregion

        #region WCFInteractiveDataBandSelection

        private Task<string> InteractiveDataBandSelectionTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.DesignerServiceClient();
                return service.InteractiveDataBandSelection(xml);
            });
        }

        private async void WCFService_WCFInteractiveDataBandSelection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            try
            {
                var result = await InteractiveDataBandSelectionTask(e.Xml);

                if (!this.progress.IsBreaked)
                    ((StiWpfViewerControl)sender).ApplyChangesAfterInteractiveDataBandSelection(result);
            }
            catch (Exception ex)
            {
                StiMessageBox.Show(this, ex.Message, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            progress.Hide();
        }
        #endregion

        #endregion

        #region Handlers

        private Task<string> LoadConfigurationTask()
        {
            return Task.Run(() =>
            {
                var service = new DesignerServiceClient();
                return service.LoadConfiguration();
            });
        }

        private async void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= MainWindowLoaded;

            try
            {
                var result = await LoadConfigurationTask();

                designer.ApplyLoadConfiguration(result, null);
                LoadReport();
            }
            catch (Exception ex)
            {
                designer.ApplyLoadConfiguration(null, ex);
            }
        }
        #endregion

        #region Methods
        private void LoadReport()
        {
            var dt = new DataSet();
            dt.ReadXml(@"d:\Data\\Demo.xml");
            //dt.ReadXmlSchema(@"d:\Data\Demo.xsd");

            var report = new StiReport();
            report.Load(@"g:\Report2.mrt");
            report.RegData("Demo", "Demo", dt);
            //report.RenderWithWpf(false);

            designer.Report = report;
        }
        #endregion

        public MainWindow()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            StiOptions.WCFService.UseWCFService = true;
            progress = new StiProgressInformation(this)
            {
                IsDialog = false, 
                IsMarquee = true
            };

            // Designer
            Stimulsoft.Report.StiOptions.WCFService.WCFRenderReport += WCFService_WCFRenderReport;
            Stimulsoft.Report.StiOptions.WCFService.WCFTestConnection += WCFService_WCFTestConnection;
            Stimulsoft.Report.StiOptions.WCFService.WCFBuildObjects += WCFService_WCFBuildObjects;
            Stimulsoft.Report.StiOptions.WCFService.WCFRetrieveColumns += WCFService_WCFRetrieveColumns;
            Stimulsoft.Report.StiOptions.WCFService.WCFOpeningReportInDesigner += WCFService_WCFOpeningReportInDesigner;
            Stimulsoft.Report.StiOptions.WCFService.WCFRequestFromUserRenderReport += WCFService_WCFRequestFromUserRenderReport;
            Stimulsoft.Report.StiOptions.WCFService.WCFReportCheck += WCFService_WCFReportCheck;
            Stimulsoft.Report.StiOptions.Engine.GlobalEvents.SavingReportInDesigner += GlobalEvents_SavingReportInDesigner;

            // Interactions
            Stimulsoft.Report.StiOptions.WCFService.WCFRenderingInteractions += WCFService_WCFRenderingInteractions;

            // Viewer
            Stimulsoft.Report.StiOptions.WCFService.WCFExportDocument += WCFService_WCFExportDocument;

            // Prepare RequestFromUser Variables
            Stimulsoft.Report.StiOptions.WCFService.WCFPrepareRequestFromUserVariables += WCFService_WCFPrepareRequestFromUserVariables;
            Stimulsoft.Report.StiOptions.WCFService.WCFInteractiveDataBandSelection += WCFService_WCFInteractiveDataBandSelection;

            InitializeComponent();

            this.designer = new StiWpfDesignerControl();
            this.Content = this.designer;
        }
    }
}