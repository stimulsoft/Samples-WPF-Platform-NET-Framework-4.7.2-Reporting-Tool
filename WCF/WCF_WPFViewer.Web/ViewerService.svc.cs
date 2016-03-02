#region Copyright (C) 2003-2012 Stimulsoft
/*
{*******************************************************************}
{																	}
{	Stimulsoft Reports.SL											}
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

using System.Data;
using System.ServiceModel.Activation;
using Stimulsoft.Report;
using WCFHelper;

namespace WCF_WPFViewer.Web
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ViewerService : IViewerService
    {
        #region PreviewDataSet
        private static DataSet previewDataSet = null;
        public static DataSet PreviewDataSet
        {
            get
            {
                return previewDataSet;
            }
            set
            {
                previewDataSet = value;
            }
        }

        private static void InvokePreviewDataSet()
        {
            if (previewDataSet == null)
            {
                previewDataSet = new DataSet();
                previewDataSet.ReadXmlSchema(@"c:\Users\Anton\Documents\Source Code\Data\Demo.xsd");
                previewDataSet.ReadXml(@"c:\Users\Anton\Documents\Source Code\Data\Demo.xml");
            }
        }
        #endregion

        #region Methods
        public string LoadReport(string reportName)
        {
            string result = null;

            if (!string.IsNullOrEmpty(reportName))
            {
                StiReport report = new StiReport();
                report.Load(@"c:\Users\Anton\Documents\Source Code\Stimulsoft\Stimulsoft.Reports.Samples.SWPF\WCF\WCF_WPFViewer.Web\Reports\MasterDetail.mrt");

                InvokePreviewDataSet();

                report.RegData(PreviewDataSet);
                report.Render(false);

                result = StiSLRenderingReportHelper.CheckReportOnInteractions(report, true);
            }

            return result;
        }

        public string RenderingInteractions(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.RenderingInteractions(xml, PreviewDataSet);
        }

        public string RequestFromUserRenderReport(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.RequestFromUserRenderReport(xml, PreviewDataSet);
        }

        public string PrepareRequestFromUserVariables(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.PrepareRequestFromUserVariables(xml, PreviewDataSet);
        }

        public string InteractiveDataBandSelection(string xml)
        {
            InvokePreviewDataSet();
            return StiSLRenderingReportHelper.InteractiveDataBandSelection(xml, PreviewDataSet);
        }

        public byte[] ExportDocument(string xml)
        {
            return StiSLExportHelper.StartExport(xml);
        }
        #endregion
    }
}