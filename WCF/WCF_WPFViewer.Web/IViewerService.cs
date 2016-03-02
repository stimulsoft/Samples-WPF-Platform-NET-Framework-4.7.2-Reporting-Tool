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

namespace WCF_WPFViewer.Web
{
    [ServiceContract]
    interface IViewerService
    {
        [OperationContract]
        string LoadReport(string reportName);
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
    }
}