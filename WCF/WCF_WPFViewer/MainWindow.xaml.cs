#region Copyright (C) 2003-2013 Stimulsoft
/*
{*******************************************************************}
{																	}
{	Stimulsoft Cloud Reports 										}
{																	}
{	Copyright (C) 2003-2013 Stimulsoft     							}
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
#endregion Copyright (C) 2003-2013 Stimulsoft

using System.Windows;
using Stimulsoft.Report;
using Stimulsoft.Report.Wpf;
using Microsoft.Win32;
using Stimulsoft.Base.Localization;
using System.Windows.Controls;
using System.Threading.Tasks;
using System;

namespace WCF_WPFViewer
{
    public partial class MainWindow
    {
        #region Fields
        private StiInteractionType interactionType;
        private StiProgressInformation progress;
        #endregion

        #region Handlers

        #region ExportDocument
        private string exportFilter;

        private Task<byte[]> ExportDocumentRask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.ExportDocument(xml);
            });
        }


        private async void WCFService_WCFExportDocument(object sender, Stimulsoft.Report.Events.StiWCFExportEventArgs e)
        {
            exportFilter = e.Filter;
            progress.Start("Export Report...");

            try
            {
                var result = await ExportDocumentRask(e.Xml);
                progress.Hide();

                if (result != null)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = string.Format("Export Document (*.{0})|*.{0}", exportFilter)
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
            catch (Exception)
            {
                progress.Hide();
            }
        }

        #endregion

        #region buttonLoad_Click

        private Task<string> LoadReportTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.LoadReportAsync(xml);
            });
        }

        private async void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (cbReports.SelectedItem != null)
            {
                progress.Start(StiLocalization.Get("DesignerFx", "LoadingDocument") + "...");

                try
                {
                    string result = await LoadReportTask(((ComboBoxItem)cbReports.SelectedItem).Content.ToString());

                    progress.Hide();
                    if (result != null && result.Length > 2)
                        viewerControl.ApplyRenderedReport(result);
                }
                catch
                {
                    progress.Hide();
                }
            }
        }

        #endregion

        #region RenderingInteractions

        private Task<string> RenderingInteractionsTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.RenderingInteractions(xml);
            });
        }

        private async void WCFService_WCFRenderingInteractions(object viewer, Stimulsoft.Report.Events.StiWCFRenderingInteractionsEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "CompilingReport") + "...");

            interactionType = e.InteractionType;

            try
            {
                var result = await RenderingInteractionsTask(e.Xml);
                if (result != null && result.Length > 2)
                {
                    switch (interactionType)
                    {
                        case StiInteractionType.Collapsing:
                            viewerControl.ApplyChangesAfterCollapsing(result);
                            break;

                        case StiInteractionType.DrillDownPage:
                            viewerControl.ApplyChangesAfterDrillDownPage(result);
                            break;

                        case StiInteractionType.Sorting:
                            viewerControl.ApplyChangesAfterSorting(result);
                            break;
                    }
                }
            }
            catch
            {

            }

            progress.Hide();
        }

        #endregion

        #region RequestFromUserRenderReport

        private Task<string> RequestFromUserRenderReportTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
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
                    viewerControl.ApplyRenderedReport(result, true);
            }
            catch
            {

            }

            progress.Hide();
        }

        #endregion

        #region WCFPrepareRequestFromUserVariables

        private Task<string> PrepareRequestFromUserVariablesTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.PrepareRequestFromUserVariables(xml);
            });
        }

        private async void WCFService_WCFPrepareRequestFromUserVariables(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            var service = new ServiceReference1.ViewerServiceClient();
            try
            {
                var result = await PrepareRequestFromUserVariablesTask(e.Xml);
                viewerControl.ApplyResultAfterPrepareRequestFromUserVariables(null, result);
            }
            catch (Exception ex)
            {
                viewerControl.ApplyResultAfterPrepareRequestFromUserVariables(ex, null);
            }
        }

        #endregion

        #region WCFInteractiveDataBandSelection

        private Task<string> InteractiveDataBandSelectionTask(string xml)
        {
            return Task.Run(() =>
            {
                var service = new ServiceReference1.ViewerServiceClient();
                return service.InteractiveDataBandSelection(xml);
            });
        }

        private async void WCFService_WCFInteractiveDataBandSelection(object sender, Stimulsoft.Report.Events.StiWCFEventArgs e)
        {
            progress.Start(StiLocalization.Get("DesignerFx", "LoadingReport") + "...");

            try
            {
                var result = await InteractiveDataBandSelectionTask(e.Xml);
                this.viewerControl.ApplyChangesAfterInteractiveDataBandSelection(result);
                progress.Hide();
            }
            catch (Exception ex)
            {
                progress.Hide();
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        #endregion

        #endregion

        public MainWindow()
        {
            StiOptions.Wpf.CurrentTheme = StiOptions.Wpf.Themes.Office2013Theme;
            StiOptions.WCFService.UseWCFService = true;
            progress = new StiProgressInformation(this);
            progress.IsDialog = false;
            progress.IsMarquee = true;
            // Viewer
            Stimulsoft.Report.StiOptions.WCFService.WCFExportDocument += WCFService_WCFExportDocument;
            Stimulsoft.Report.StiOptions.WCFService.WCFRequestFromUserRenderReport += WCFService_WCFRequestFromUserRenderReport;

            // Interactions
            Stimulsoft.Report.StiOptions.WCFService.WCFRenderingInteractions += WCFService_WCFRenderingInteractions;
            Stimulsoft.Report.StiOptions.WCFService.WCFInteractiveDataBandSelection += WCFService_WCFInteractiveDataBandSelection;

            // Prepare RequestFromUser Variables
            Stimulsoft.Report.StiOptions.WCFService.WCFPrepareRequestFromUserVariables += WCFService_WCFPrepareRequestFromUserVariables;

            InitializeComponent();
        }
    }
}