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

using System.ServiceModel;

namespace WCF_WPFDesigner.Service
{
    [ServiceContract]
    public interface IDesignerService
    {
        // Viewer
        [OperationContract]
        byte[] LoadReport();

        [OperationContract]
        string RenderingInteractions(string xml);

        [OperationContract]
        string RequestFromUserRenderReport(string xml);

        [OperationContract]
        byte[] ExportDocument(string xml);

        [OperationContract]
        string PrepareRequestFromUserVariables(string xml);

        [OperationContract]
        string InteractiveDataBandSelection(string xml);

        //Designer
        [OperationContract]
        bool SaveReport(byte[] buffer);

        [OperationContract]
        string LoadConfiguration();

        [OperationContract]
        string RenderReport(string xml);

        [OperationContract]
        string TestConnection(string settings);

        [OperationContract]
        string BuildObjects(string settings);

        [OperationContract]
        string RetrieveColumns(string settings);

        // Designer.GoogleDocs
        [OperationContract]
        string GoogleDocsGetDocuments(string xml);

        [OperationContract]
        string GoogleDocsCreateCollection(string xml);

        [OperationContract]
        string GoogleDocsDelete(string xml);

        [OperationContract]
        string GoogleDocsOpen(string xml);

        [OperationContract]
        string GoogleDocsSave(string xml);

        // Designer.Script
        [OperationContract]
        string OpenReportScript(string xml);

        [OperationContract]
        string SaveReportScript(string xml);

        [OperationContract]
        string CheckReport(string xml);
    }
}