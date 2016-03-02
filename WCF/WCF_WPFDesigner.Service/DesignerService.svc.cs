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

using System.IO;
using System.ServiceModel.Activation;
using System.Data;
using Stimulsoft.Report;
using WCFHelper;

namespace WCF_WPFDesigner.Service
{
    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DesignerService : IDesignerService
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
                previewDataSet.ReadXmlSchema(@"d:\Data\Demo.xsd");
                previewDataSet.ReadXml(@"d:\Data\Demo.xml");
            }
        }
        #endregion

        #region Method.Load & Save
        public byte[] LoadReport()
        {
            var report = new StiReport();
            report.Load("Reports\\Labels.mrt");

            return report.SaveToByteArray();
        }

        public bool SaveReport(byte[] buffer)
        {
            var fileStream = new FileStream("g:\\Report2.mrt", FileMode.CreateNew);
            fileStream.Write(buffer, 0, buffer.Length);
            fileStream.Flush();
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;

            return true;
        }
        #endregion

        #region Methods.LoadConfiguration
        public string LoadConfiguration()
        {
            return StiSLDesignerHelper.LoadConfiguration();
        }
        #endregion

        #region Methods.Viewer
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

        public byte[] ExportDocument(string xml)
        {
            return StiSLExportHelper.StartExport(xml);
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
        #endregion

        #region Methods.Designer
        public string RenderReport(string xml)
        {
            InvokePreviewDataSet();
            return StiSLDesignerHelper.RenderReport(xml, PreviewDataSet);
        }

        public string TestConnection(string xml)
        {
            return StiSLDesignerHelper.TestConnection(xml);
        }

        public string BuildObjects(string xml)
        {
            return StiSLDesignerHelper.BuildObjects(xml);
        }

        public string RetrieveColumns(string xml)
        {
            return StiSLDesignerHelper.RetrieveColumns(xml);
        }

        #region GoogleDocs
        public string GoogleDocsGetDocuments(string xml)
        {
            return StiSLDesignerHelper.GoogleDocsGetDocuments(xml);
        }

        public string GoogleDocsCreateCollection(string xml)
        {
            return StiSLDesignerHelper.GoogleDocsCreateCollection(xml);
        }

        public string GoogleDocsDelete(string xml)
        {
            return StiSLDesignerHelper.GoogleDocsDelete(xml);
        }

        public string GoogleDocsOpen(string xml)
        {
            return StiSLDesignerHelper.GoogleDocsOpen(xml);
        }

        public string GoogleDocsSave(string xml)
        {
            return StiSLDesignerHelper.GoogleDocsSave(xml);
        }
        #endregion
        #endregion

        #region Methods.ReportScript
        public string OpenReportScript(string xml)
        {
            return StiSLDesignerHelper.OpenReportScript(xml);
        }

        public string SaveReportScript(string xml)
        {
            return StiSLDesignerHelper.SaveReportScript(xml);
        }

        public string CheckReport(string xml)
        {
            return StiSLDesignerHelper.CheckReport(xml);
        }
        #endregion

        public DesignerService()
        {
            // Connect only if you use the additional database.
            //StiOptions.Dictionary.DataAdapters.TryToLoadDB2Adapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadFirebirdAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadMySqlAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadDotConnectUniversalAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadOracleClientAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadOracleODPAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadPostgreSQLAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSqlCeAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSQLiteAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadVistaDBAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadUniDirectAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSybaseADSAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadSybaseASEAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadInformixAdapter = true;
            //StiOptions.Dictionary.DataAdapters.TryToLoadEffiProzAdapter = true;
        }
    }
}