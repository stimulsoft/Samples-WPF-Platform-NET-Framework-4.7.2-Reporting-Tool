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

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Linq;
using System.Reflection;

using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Export;

namespace WCFHelper
{
    public static class StiSLRenderingReportHelper
    {
        #region Methods.RenderingInteractions
        public static string RenderingInteractions(string xml, DataSet previewDataSet)
        {
            var drillDownContainer = DecodeXmlRenderingInteractions(xml);

            string result = null;
            switch (drillDownContainer.TypeDrillDown)
            {
                case "DrillDownPage":
                    result = StartDrillDownPage(drillDownContainer, previewDataSet);
                    break;

                case "Sorting":
                    result = StartSorting(drillDownContainer, previewDataSet);
                    break;

                case "Collapsing":
                    result = StartCollapsing(drillDownContainer, previewDataSet);
                    break;
            }

            return result;
        }

        private static StiDrillDownContainer DecodeXmlRenderingInteractions(string xml)
        {
            var drillDownContainer = new StiDrillDownContainer();
            using (var stringReader = new System.IO.StringReader(StiSLEncodingHelper.DecodeString(xml)))
            using (var tr = new XmlTextReader(stringReader))
            {
                tr.Read();

                if (tr.Name == "XmlResult")
                {
                    while (tr.Read())
                    {
                        string name = tr.Name;

                        switch (name)
                        {
                            case "TypeDD":
                                drillDownContainer.TypeDrillDown = tr.ReadString();
                                break;
                            case "Report":
                                drillDownContainer.Report.LoadFromString(tr.ReadString());
                                break;
                            case "DDPName":
                                drillDownContainer.DrillDownPageName = tr.ReadString();
                                break;
                            case "CollIndex":
                                drillDownContainer.CollapsingIndex = int.Parse(tr.ReadString());
                                break;
                            case "IsCollapsed":
                                drillDownContainer.IsCollapsed = (tr.ReadString() == "1") ? true : false;
                                break;
                            case "PIndex":
                                drillDownContainer.PageIndex = int.Parse(tr.ReadString());
                                break;
                            case "CIndex":
                                drillDownContainer.CompIndex = int.Parse(tr.ReadString());
                                break;

                            case "DDParameters":
                                while (tr.Read())
                                {
                                    if (tr.Depth == 2)
                                    {
                                        drillDownContainer.DrillDownParameters.Add(tr.Name, tr.ReadString());
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                break;

                            case "DBandName":
                                drillDownContainer.DataBandName = tr.ReadString();
                                break;
                            case "DBandColumns":
                                drillDownContainer.DataBandColumns = tr.ReadString().Split(new char[] { ',' });
                                break;
                            case "DBandColStr":
                                drillDownContainer.DataBandColumnString = tr.ReadString();
                                break;
                            case "SDirec":
                                drillDownContainer.SortingDirection = ((StiInteractionSortDirection)int.Parse(tr.ReadString()));
                                break;
                            case "ControlPress":
                                drillDownContainer.IsControlPress = (tr.ReadString() == "1") ? true : false;
                                break;

                            case "ICollStates":
                                drillDownContainer.InteractionCollapsingStates = new System.Collections.Hashtable();
                                System.Collections.Hashtable currentTable = null;
                                string compName = string.Empty;

                                while (tr.Read())
                                {
                                    if (tr.Depth == 1)
                                    {
                                        break;
                                    }
                                    else if (tr.Depth == 2)
                                    {
                                        compName = tr.Name;
                                        if (!drillDownContainer.InteractionCollapsingStates.ContainsKey(compName))
                                        {
                                            drillDownContainer.InteractionCollapsingStates.Add(compName, new System.Collections.Hashtable());
                                        }
                                        currentTable = drillDownContainer.InteractionCollapsingStates[compName] as System.Collections.Hashtable;
                                    }
                                    else if (tr.Depth == 3)
                                    {
                                        int keyValue = int.Parse(tr.Name.Remove(0, 1));
                                        switch (tr.ReadString())
                                        {
                                            case "1":
                                                currentTable.Add(keyValue, true);
                                                break;
                                            case "0":
                                                currentTable.Add(keyValue, false);
                                                break;
                                            default:
                                                currentTable.Add(keyValue, null);
                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                return drillDownContainer;
            }
        }

        private static string StartDrillDownPage(StiDrillDownContainer drillDownContainer, System.Data.DataSet ds)
        {
            drillDownContainer.Report.RegData(ds);

            drillDownContainer.Report.Compile();
            drillDownContainer.Report.Render(false);
            StiReport compileReport = StiActivator.CreateObject(drillDownContainer.Report.CompiledReport.GetType()) as StiReport;

            compileReport.RegData(drillDownContainer.Report.DataStore);
            compileReport.RegBusinessObject(drillDownContainer.Report.BusinessObjectsStore);

            #region Drill-Down Page
            StiPage drillDownPage = null;

            foreach (StiPage newPage in compileReport.Pages)
            {
                if (newPage.Name == drillDownContainer.DrillDownPageName)
                {
                    newPage.Enabled = true;
                    newPage.Skip = false;
                    drillDownPage = newPage;
                }
                else
                    newPage.Enabled = false;
            }

            #region Clear any reference to drill-down page from other components in report
            //We need do this because during report rendering drill-down pages is skipped
            var comps = compileReport.GetComponents();
            foreach (StiComponent comp in comps)
            {
                if (comp.Interaction != null && comp.Interaction.DrillDownEnabled && comp.Interaction.DrillDownPageGuid == drillDownPage.Guid)
                {
                    comp.Interaction.DrillDownPage = null;
                }
            }
            #endregion

            #region Set DrillDownParameters
            StiPage renderedPage = drillDownContainer.Report.RenderedPages[drillDownContainer.PageIndex];
            StiComponent interactionComp = renderedPage.Components[drillDownContainer.CompIndex];

            if (interactionComp != null && interactionComp.DrillDownParameters != null)
            {
                foreach (var entry in interactionComp.DrillDownParameters)
                {
                    compileReport[entry.Key] = entry.Value;
                }
            }
            #endregion
            #endregion

            compileReport.IsInteractionRendering = true;

            bool error = false;
            try
            {
                compileReport.Render(false);
            }
            catch
            {
                error = true;
            }

            if (error)
            {
                return null;
            }
            else
            {
                compileReport.IsInteractionRendering = false;
                return CheckReportOnInteractions(compileReport, false);
            }
        }

        private static string StartCollapsing(StiDrillDownContainer drillDownContainer, System.Data.DataSet ds)
        {
            drillDownContainer.Report.RegData(ds);

            drillDownContainer.Report.Compile();
            drillDownContainer.Report.CompiledReport.InteractionCollapsingStates = drillDownContainer.InteractionCollapsingStates;
            drillDownContainer.Report.Render(false);

            StiReport compileReport = drillDownContainer.Report.CompiledReport == null ? drillDownContainer.Report : drillDownContainer.Report.CompiledReport;
            StiComponent interactionComp = compileReport.RenderedPages[drillDownContainer.PageIndex].Components[drillDownContainer.CompIndex];

            if (interactionComp != null)
            {
                if (compileReport.InteractionCollapsingStates == null)
                    compileReport.InteractionCollapsingStates = new Hashtable();

                var list = compileReport.InteractionCollapsingStates[interactionComp.Name] as Hashtable;
                if (list == null)
                {
                    list = new Hashtable();
                    compileReport.InteractionCollapsingStates[interactionComp.Name] = list;
                }
                list[drillDownContainer.CollapsingIndex] = drillDownContainer.IsCollapsed;

                #region Render Report
                try
                {
                    drillDownContainer.Report.IsInteractionRendering = true;
                    compileReport.IsInteractionRendering = true;
                    compileReport.Render(false);
                }
                finally
                {
                    compileReport.IsInteractionRendering = false;
                }
                #endregion

                return CheckReportOnInteractions(compileReport, false);
            }
            else
            {
                return " ";
            }
        }

        private static string StartSorting(StiDrillDownContainer drillDownContainer, System.Data.DataSet ds)
        {
            drillDownContainer.Report.RegData(ds);

            drillDownContainer.Report.Compile();
            drillDownContainer.Report.Render(false);

            StiReport compileReport = drillDownContainer.Report.CompiledReport == null ? drillDownContainer.Report : drillDownContainer.Report.CompiledReport;

            StiComponent interactionComp = compileReport.RenderedPages[drillDownContainer.PageIndex].Components[drillDownContainer.CompIndex];
            interactionComp.Interaction.SortingDirection = drillDownContainer.SortingDirection;

            StiDataBand dataBand = null;
            if (interactionComp is Stimulsoft.Report.Components.Table.IStiTableComponent)
            {
                dataBand = ((Stimulsoft.Report.Components.Table.IStiTableCell)interactionComp).TableTag as StiDataBand;
            }
            else
            {
                dataBand = compileReport.GetComponentByName(drillDownContainer.DataBandName) as StiDataBand;
            }

            if (dataBand != null)
            {
                string sort = (drillDownContainer.SortingDirection == StiInteractionSortDirection.Descending) ? "ASC" : "DESC";
                dataBand.Sort = new string[] { sort, drillDownContainer.DataBandColumnString };

                #region Set Sorting
                if (dataBand.Sort == null || dataBand.Sort.Length == 0)
                {
                    dataBand.Sort = StiSortHelper.AddColumnToSorting(dataBand.Sort, drillDownContainer.DataBandColumnString, true);
                }
                else
                {
                    int sortIndex = StiSortHelper.GetColumnIndexInSorting(dataBand.Sort, drillDownContainer.DataBandColumnString);

                    if (drillDownContainer.IsControlPress)
                    {
                        if (sortIndex == -1)
                        {
                            dataBand.Sort = StiSortHelper.AddColumnToSorting(dataBand.Sort, drillDownContainer.DataBandColumnString, true);
                        }
                        else
                        {
                            dataBand.Sort = StiSortHelper.ChangeColumnSortDirection(dataBand.Sort, drillDownContainer.DataBandColumnString);
                        }
                    }
                    else
                    {
                        if (sortIndex != -1)
                        {
                            StiInteractionSortDirection direction = StiSortHelper.GetColumnSortDirection(dataBand.Sort, drillDownContainer.DataBandColumnString);

                            if (direction == StiInteractionSortDirection.Ascending) direction = StiInteractionSortDirection.Descending;
                            else direction = StiInteractionSortDirection.Ascending;

                            dataBand.Sort = StiSortHelper.AddColumnToSorting(new string[0],
                                drillDownContainer.DataBandColumnString, direction == StiInteractionSortDirection.Ascending);
                        }
                        else
                        {
                            dataBand.Sort = StiSortHelper.AddColumnToSorting(new string[0], drillDownContainer.DataBandColumnString, true);
                        }
                    }
                }
                #endregion

                #region Render Report
                try
                {
                    drillDownContainer.Report.IsInteractionRendering = true;
                    drillDownContainer.Report.CompiledReport.IsInteractionRendering = true;
                    drillDownContainer.Report.Render(false);

                    RefreshInteractions(drillDownContainer.Report);
                }
                finally
                {
                    drillDownContainer.Report.CompiledReport.IsInteractionRendering = false;
                    interactionComp = drillDownContainer.Report.CompiledReport.RenderedPages[drillDownContainer.PageIndex].Components[drillDownContainer.CompIndex];
                    interactionComp.Interaction.SortingDirection = drillDownContainer.SortingDirection;
                }
                #endregion

                return CheckReportOnInteractions(drillDownContainer.Report, false);
            }

            return null;
        }

        public static string CheckReportOnInteractions(StiReport report, bool useBaseReport)
        {
            var listComps = new List<StiComponent>();

            #region Search Components
            bool isSort = false;
            foreach (StiPage page in report.RenderedPages)
            {
                foreach (StiComponent comp in page.Components)
                {
                    var interaction = comp.Interaction;
                    var bandInteraction = comp.Interaction as StiBandInteraction;

                    if (interaction != null)
                    {
                        if ((!string.IsNullOrEmpty(interaction.SortingColumn) ||
                            interaction.DrillDownEnabled && !string.IsNullOrEmpty(interaction.DrillDownPageGuid) ||
                            (bandInteraction != null && (bandInteraction.CollapsingEnabled || bandInteraction.SelectionEnabled))))
                        {
                            comp.Guid = string.Format("Guid_{0}", Guid.NewGuid().ToString().Replace("-", ""));

                            if (!isSort && interaction.SortingDirection != StiInteractionSortDirection.None)
                            {
                                isSort = true;
                            }

                            listComps.Add(comp);
                        }
                    }
                }
            }
            #endregion

            #region Check Variables
            bool isRequestFromUser = false;
            foreach (StiVariable variable in report.Dictionary.Variables)
            {
                if (variable.RequestFromUser)
                {
                    isRequestFromUser = true;
                    break;
                }
            }
            #endregion

            #region Set Sorting
            if (!isSort)
            {
                foreach (StiComponent comp in listComps)
                {
                    if (!string.IsNullOrEmpty(comp.Interaction.SortingColumn))
                    {
                        comp.Interaction.SortingDirection = StiInteractionSortDirection.Ascending;
                        break;
                    }
                }
            }
            #endregion

            using (var str = new System.IO.StringWriter())
            using (var writer = new XmlTextWriter(str))
            {
                writer.WriteStartElement("XmlResult");

                writer.WriteStartElement("Report");
                writer.WriteValue(report.SaveDocumentToString());
                writer.WriteEndElement();

                if (useBaseReport && (listComps.Count > 0 || isRequestFromUser))
                {
                    writer.WriteStartElement("BaseReport");
                    writer.WriteValue(report.SaveToString());
                    writer.WriteEndElement();
                }

                if (listComps.Count > 0)
                {
                    writer.WriteStartElement("Comps");

                    foreach (StiComponent comp in listComps)
                    {
                        writer.WriteStartElement(comp.Guid);

                        #region StiInteraction
                        StiInteraction interaction = comp.Interaction;
                        if (interaction is StiBandInteraction)
                        {
                            writer.WriteStartElement("StiBandInteraction");

                            #region StiBandInteraction
                            StiBandInteraction bandInteraction = interaction as StiBandInteraction;

                            if (bandInteraction.CollapseGroupFooter)
                            {
                                writer.WriteStartElement("CollGroupFooter");
                                writer.WriteValue(bandInteraction.CollapseGroupFooter ? "1" : "0");
                                writer.WriteEndElement();
                            }

                            if (bandInteraction.CollapsingEnabled)
                            {
                                writer.WriteStartElement("CollEnabled");
                                writer.WriteValue(bandInteraction.CollapsingEnabled ? "1" : "0");
                                writer.WriteEndElement();
                            }

                            if (bandInteraction.SelectionEnabled)
                            {
                                writer.WriteStartElement("SelEnabled");
                                writer.WriteValue(bandInteraction.SelectionEnabled ? "1" : "0");
                                writer.WriteEndElement();
                            }
                            #endregion
                        }
                        else
                        {
                            writer.WriteStartElement("StiInteraction");
                        }

                        if (!string.IsNullOrEmpty(comp.Page.Guid))
                        {
                            writer.WriteStartElement("PGuid");
                            writer.WriteValue(comp.Page.Guid);
                            writer.WriteEndElement();
                        }

                        if (interaction.DrillDownEnabled)
                        {
                            writer.WriteStartElement("DDEnabled");
                            writer.WriteValue(interaction.DrillDownEnabled ? "1" : "0");
                            writer.WriteEndElement();
                        }

                        if (!string.IsNullOrEmpty(interaction.DrillDownPageGuid))
                        {
                            writer.WriteStartElement("DDPGuid");
                            writer.WriteValue(interaction.DrillDownPageGuid);
                            writer.WriteEndElement();
                        }

                        #region Sort
                        if (!string.IsNullOrEmpty(interaction.SortingColumn))
                        {
                            writer.WriteStartElement("SCol");
                            writer.WriteValue(interaction.SortingColumn);
                            writer.WriteEndElement();
                        }

                        if (interaction.SortingDirection != StiInteractionSortDirection.None)
                        {
                            writer.WriteStartElement("SDirect");
                            writer.WriteValue((int)interaction.SortingDirection);
                            writer.WriteEndElement();
                        }

                        if (interaction.SortingEnabled)
                        {
                            writer.WriteStartElement("SEnabled");
                            writer.WriteValue(interaction.SortingEnabled ? "1" : "0");
                            writer.WriteEndElement();
                        }

                        writer.WriteStartElement("SIndex");
                        writer.WriteValue(interaction.SortingIndex);
                        writer.WriteEndElement();
                        #endregion

                        writer.WriteEndElement();
                        #endregion

                        #region CollapsingIndex
                        StiContainer container = comp as StiContainer;
                        if (container != null)
                        {

                            writer.WriteStartElement("CollapsingIndex");
                            writer.WriteValue(container.CollapsingIndex.ToString());
                            writer.WriteEndElement();

                            writer.WriteStartElement("CollapsedValue");
                            writer.WriteValue(Stimulsoft.Report.Engine.StiDataBandV2Builder.IsCollapsed(container, false) ? "1" : "0");
                            writer.WriteEndElement();
                        }
                        #endregion

                        #region DrillDownParameters
                        if (comp.DrillDownParameters != null && comp.DrillDownParameters.Count > 0)
                        {
                            writer.WriteStartElement("DrillDownParameters");
                            foreach (string key in comp.DrillDownParameters.Keys)
                            {
                                writer.WriteStartElement(key);
                                writer.WriteValue(comp.DrillDownParameters[key]);
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                        #endregion

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement(); // End Comps
                }

                #region InteractionCollapsingStates
                StiReport currentReport = report.CompiledReport == null ? report : report.CompiledReport;
                if (currentReport.InteractionCollapsingStates != null && currentReport.InteractionCollapsingStates.Count > 0)
                {
                    writer.WriteStartElement("InteractionCollapsingStates");
                    foreach (string key in currentReport.InteractionCollapsingStates.Keys)
                    {
                        Hashtable list = currentReport.InteractionCollapsingStates[key] as Hashtable;
                        if (list != null)
                        {
                            writer.WriteStartElement("_" + key);
                            foreach (object key1 in list.Keys)
                            {
                                writer.WriteStartElement("_" + key1.ToString());
                                writer.WriteValue(list[key1].ToString());
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                    }
                    writer.WriteEndElement();
                }
                #endregion

                writer.WriteEndElement();

                return StiSLEncodingHelper.EncodeString(str.ToString());
            }
        }
        #endregion

        #region Methods.RequestFromUser
        public static string RequestFromUserRenderReport(string xml, DataSet previewDataSet)
        {
            StiReport report = DecodeXmlRequestFromUser(xml, previewDataSet);

            if (report.CompilerResults.Errors.Count > 0)
            {
                return GetErrorListXml(report);
            }

            bool error = false;
            try
            {
                report.Render(false);
            }
            catch
            {
                error = true;
            }

            if (!error)
            {
                return CheckReportOnInteractions(report, false);
            }

            return null;
        }

        public static string PrepareRequestFromUserVariables(string xml, DataSet previewDataSet)
        {
            StiReport report = DecodeXmlPrepareRequestFromUserVariables(xml, previewDataSet);

            string result = string.Empty;
            if (report.CompiledReport != null)
            {
                report.CompiledReport.Dictionary.Connect();
                Stimulsoft.Report.Engine.StiVariableHelper.FillItemsOfVariables(report.CompiledReport != null ? report.CompiledReport : report);

                result = GetPrepareRequestFromUserVariablesXml(report.CompiledReport);
                report.CompiledReport.Dictionary.Disconnect();
            }

            return result;
        }

        private static StiReport DecodeXmlPrepareRequestFromUserVariables(string xml, System.Data.DataSet previewDataSet)
        {
            var report = new StiReport();

            using (var stringReader = new System.IO.StringReader(StiSLEncodingHelper.DecodeString(xml)))
            using (var tr = new XmlTextReader(stringReader))
            {
                tr.Read();
                if (tr.Name == "XmlResult")
                {
                    while (tr.Read())
                    {
                        if (tr.Name == "Report")
                        {
                            report.LoadFromString(tr.ReadString());
                            //report.Dictionary.DataSources.Clear();
                            //report.Dictionary.Databases.Clear();
                            //report.Dictionary.DataStore.Clear();

                            if (previewDataSet != null) report.RegData(previewDataSet);
                            report.Dictionary.Synchronize();

                            try
                            {
                                report.Compile();
                            }
                            catch
                            {
                                return report;
                            }

                            break;
                        }
                    }
                }

                return report;
            }
        }

        private static string GetPrepareRequestFromUserVariablesXml(StiReport compileReport)
        {
            using (var str = new System.IO.StringWriter())
            using (var writer = new XmlTextWriter(str))
            {
                writer.WriteStartElement("XmlResult");

                #region Variables
                writer.WriteStartElement("Variables");

                foreach (StiVariable variable in compileReport.Dictionary.Variables)
                {
                    List<StiDialogInfoItem> infos = variable.DialogInfo.GetDialogInfoItems(variable.Type);
                    if (infos != null && infos.Count > 0)
                    {
                        string varName = StiDatabaseBuildHelper.CheckName(variable.Name);

                        writer.WriteStartElement(varName);
                        writer.WriteAttributeString("count", infos.Count.ToString());

                        foreach (StiDialogInfoItem info in infos)
                        {
                            writer.WriteStartElement("Key");
                            writer.WriteValue(info.KeyObject.ToString());
                            writer.WriteEndElement();

                            string keyObjectTo = (info.KeyObjectTo != null) ? info.KeyObjectTo.ToString() : string.Empty;
                            writer.WriteStartElement("KeyTo");
                            writer.WriteValue(keyObjectTo);
                            writer.WriteEndElement();

                            writer.WriteStartElement("Value");
                            writer.WriteValue(info.Value);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
                #endregion

                writer.WriteEndElement();

                return StiSLEncodingHelper.EncodeString(str.ToString());
            }
        }

        private static StiReport DecodeXmlRequestFromUser(string xml, System.Data.DataSet ds)
        {
            var report = new StiReport();

            using (var stringReader = new System.IO.StringReader(StiSLEncodingHelper.DecodeString(xml)))
            using (var tr = new XmlTextReader(stringReader))
            {
                tr.Read();
                if (tr.Name == "XmlResult")
                {
                    while (tr.Read())
                    {
                        string name = tr.Name;

                        switch (name)
                        {
                            case "Report":
                                report.LoadFromString(tr.ReadString());
                                if (ds != null) report.RegData("Demo", ds);
                                try
                                {
                                    report.Compile();
                                }
                                catch
                                {
                                    return report;
                                }
                                break;

                            case "RequestFromUser":
                                string variableName = string.Empty;
                                StiRequestFromUserType? variableType = null;
                                while (tr.Read())
                                {
                                    if (tr.Depth == 1)
                                    {
                                        break;
                                    }
                                    else if (tr.Depth == 2)
                                    {
                                        if (tr.NodeType == System.Xml.XmlNodeType.Element)
                                        {
                                            variableName = tr.Name;
                                            variableType = (StiRequestFromUserType)int.Parse(tr.GetAttribute("Type"));
                                        }
                                    }
                                    else if (tr.Depth == 3)
                                    {
                                        ParseVariable(report, variableName, variableType.Value, tr);
                                    }
                                }
                                break;
                        }
                    }
                }

                return report;
            }
        }

        private static void ParseVariable(StiReport report, string variableName, StiRequestFromUserType type, XmlTextReader tr)
        {
            var currentReport = (report.CompiledReport != null) ? report.CompiledReport : report;
            var variable = report.Dictionary.Variables[variableName];
            if (variable == null) return;

            Range range = null;
            IStiList list = null;
            object value = null;

            #region Create Type
            switch (type)
            {
                case StiRequestFromUserType.ListBool:
                    list = new BoolList();
                    break;
                case StiRequestFromUserType.ListChar:
                    list = new CharList();
                    break;
                case StiRequestFromUserType.ListDateTime:
                    list = new DateTimeList();
                    break;
                case StiRequestFromUserType.ListTimeSpan:
                    list = new TimeSpanList();
                    break;
                case StiRequestFromUserType.ListDecimal:
                    list = new DecimalList();
                    break;
                case StiRequestFromUserType.ListFloat:
                    list = new FloatList();
                    break;
                case StiRequestFromUserType.ListDouble:
                    list = new DoubleList();
                    break;
                case StiRequestFromUserType.ListByte:
                    list = new ByteList();
                    break;
                case StiRequestFromUserType.ListShort:
                    list = new ShortList();
                    break;
                case StiRequestFromUserType.ListInt:
                    list = new IntList();
                    break;
                case StiRequestFromUserType.ListLong:
                    list = new LongList();
                    break;
                case StiRequestFromUserType.ListGuid:
                    list = new GuidList();
                    break;
                case StiRequestFromUserType.ListString:
                    list = new StringList();
                    break;
                case StiRequestFromUserType.RangeByte:
                    range = new ByteRange();
                    break;
                case StiRequestFromUserType.RangeChar:
                    range = new CharRange();
                    break;
                case StiRequestFromUserType.RangeDateTime:
                    range = new DateTimeRange();
                    break;
                case StiRequestFromUserType.RangeDecimal:
                    range = new DecimalRange();
                    break;
                case StiRequestFromUserType.RangeDouble:
                    range = new DoubleRange();
                    break;
                case StiRequestFromUserType.RangeFloat:
                    range = new FloatRange();
                    break;
                case StiRequestFromUserType.RangeGuid:
                    range = new GuidRange();
                    break;
                case StiRequestFromUserType.RangeInt:
                    range = new IntRange();
                    break;
                case StiRequestFromUserType.RangeLong:
                    range = new LongRange();
                    break;
                case StiRequestFromUserType.RangeShort:
                    range = new ShortRange();
                    break;
                case StiRequestFromUserType.RangeString:
                    range = new StringRange();
                    break;
                case StiRequestFromUserType.RangeTimeSpan:
                    range = new TimeSpanRange();
                    break;
            }
            #endregion

            bool isFind = false;
            int iType = (int)type;
            do
            {
                if (tr.Depth == 3)
                {
                    string tmp = tr.ReadString();

                    #region List & Value
                    if (tr.Name == "Value")
                    {
                        #region List
                        if (iType >= 0 && iType <= (int)StiRequestFromUserType.ListString)
                        {
                            switch (type)
                            {
                                case StiRequestFromUserType.ListBool:
                                    list.AddElement(bool.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListChar:
                                    list.AddElement(char.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListDateTime:
                                    list.AddElement(DateTime.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListTimeSpan:
                                    list.AddElement(TimeSpan.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListDecimal:
                                    list.AddElement(decimal.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListFloat:
                                    list.AddElement(float.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListDouble:
                                    list.AddElement(double.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListByte:
                                    list.AddElement(byte.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListShort:
                                    list.AddElement(short.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListInt:
                                    list.AddElement(int.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListLong:
                                    list.AddElement(long.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListGuid:
                                    list.AddElement(Guid.Parse(tmp));
                                    break;
                                case StiRequestFromUserType.ListString:
                                    list.AddElement(tmp);
                                    break;
                            }
                            continue;
                        }
                        #endregion

                        #region Nullablealue
                        else if ((int)type >= (int)StiRequestFromUserType.ValueNullableBool)
                        {
                            isFind = true;
                            if (string.IsNullOrEmpty(tmp))
                            {
                                variable.ValueObject = null;
                                break;
                            }
                        }
                        #endregion

                        #region Value
                        isFind = true;
                        switch (type)
                        {
                            case StiRequestFromUserType.ValueBool:
                            case StiRequestFromUserType.ValueNullableBool:
                                variable.ValueObject = bool.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableByte:
                            case StiRequestFromUserType.ValueByte:
                                variable.ValueObject = byte.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableChar:
                            case StiRequestFromUserType.ValueChar:
                                variable.ValueObject = char.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableDateTime:
                            case StiRequestFromUserType.ValueDateTime:
                                variable.ValueObject = DateTime.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableDecimal:
                            case StiRequestFromUserType.ValueDecimal:
                                variable.ValueObject = decimal.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableDouble:
                            case StiRequestFromUserType.ValueDouble:
                                variable.ValueObject = double.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableFloat:
                            case StiRequestFromUserType.ValueFloat:
                                variable.ValueObject = float.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableGuid:
                            case StiRequestFromUserType.ValueGuid:
                                variable.ValueObject = Guid.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableInt:
                            case StiRequestFromUserType.ValueInt:
                                variable.ValueObject = int.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableLong:
                            case StiRequestFromUserType.ValueLong:
                                variable.ValueObject = long.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableSbyte:
                            case StiRequestFromUserType.ValueSbyte:
                                variable.ValueObject = sbyte.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableShort:
                            case StiRequestFromUserType.ValueShort:
                                variable.ValueObject = short.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableTimeSpan:
                            case StiRequestFromUserType.ValueTimeSpan:
                                variable.ValueObject = TimeSpan.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableUint:
                            case StiRequestFromUserType.ValueUint:
                                variable.ValueObject = uint.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableUlong:
                            case StiRequestFromUserType.ValueUlong:
                                variable.ValueObject = ulong.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueNullableUshort:
                            case StiRequestFromUserType.ValueUshort:
                                variable.ValueObject = ushort.Parse(tmp);
                                break;
                            case StiRequestFromUserType.ValueString:
                                variable.ValueObject = tmp;
                                break;
                            case StiRequestFromUserType.ValueImage:
                                variable.ValueObject = tmp;
                                break;
                        }

                        #region Reflection
                        FieldInfo fi = currentReport.GetType().GetField(variable.Name);
                        if (fi != null)
                        {
                            var initBy = variable.InitBy;
                            variable.InitBy = StiVariableInitBy.Value;

                            fi.SetValue(currentReport, variable.ValueObject);

                            variable.InitBy = initBy;
                        }
                        #endregion

                        break;
                        #endregion
                    }
                    #endregion

                    #region Range
                    else if (tr.Name == "From" || tr.Name == "To")
                    {
                        switch (type)
                        {
                            case StiRequestFromUserType.RangeByte:
                                if (tr.Name == "To")
                                    range.ToObject = byte.Parse(tmp);
                                else
                                    range.FromObject = byte.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeChar:
                                if (tr.Name == "To")
                                    range.ToObject = char.Parse(tmp);
                                else
                                    range.FromObject = char.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeDateTime:
                                if (tr.Name == "To")
                                    range.ToObject = DateTime.Parse(tmp);
                                else
                                    range.FromObject = DateTime.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeDecimal:
                                if (tr.Name == "To")
                                    range.ToObject = decimal.Parse(tmp);
                                else
                                    range.FromObject = decimal.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeDouble:
                                if (tr.Name == "To")
                                    range.ToObject = double.Parse(tmp);
                                else
                                    range.FromObject = double.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeFloat:
                                if (tr.Name == "To")
                                    range.ToObject = float.Parse(tmp);
                                else
                                    range.FromObject = float.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeGuid:
                                if (tr.Name == "To")
                                    range.ToObject = Guid.Parse(tmp);
                                else
                                    range.FromObject = Guid.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeInt:
                                if (tr.Name == "To")
                                    range.ToObject = int.Parse(tmp);
                                else
                                    range.FromObject = int.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeLong:
                                if (tr.Name == "To")
                                    range.ToObject = long.Parse(tmp);
                                else
                                    range.FromObject = long.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeShort:
                                if (tr.Name == "To")
                                    range.ToObject = short.Parse(tmp);
                                else
                                    range.FromObject = short.Parse(tmp);
                                break;
                            case StiRequestFromUserType.RangeString:
                                if (tr.Name == "To")
                                    range.ToObject = tmp;
                                else
                                    range.FromObject = tmp;
                                break;
                            case StiRequestFromUserType.RangeTimeSpan:
                                if (tr.Name == "To")
                                    range.ToObject = TimeSpan.Parse(tmp);
                                else
                                    range.FromObject = TimeSpan.Parse(tmp);
                                break;
                        }
                    }
                    #endregion
                }
                else if (tr.Depth == 2)
                    break;

                if (isFind) break;
            }
            while (tr.Read());

            StiReport compileReport = report.CompiledReport;
            if (compileReport != null)
            {
                var field = compileReport.GetType().GetField(variableName);
                if (field != null)
                {
                    if (range != null)
                        field.SetValue(compileReport, range);
                    else if (list != null)
                        field.SetValue(compileReport, list);
                    else if (value != null)
                        field.SetValue(compileReport, value);
                }
            }
        }
        #endregion

        #region Methods.Interactive
        public static string InteractiveDataBandSelection(string xml, DataSet previewDataSet)
        {
            var helper = DecodeXmlDataBandSelection(xml);
            if (previewDataSet != null) helper.Report.RegData(previewDataSet);

            bool error = false;
            helper.Report.IsInteractionRendering = true;

            try
            {
                helper.Report.Compile();

                if (helper.Report.CompiledReport != null)
                {
                    int index = -1;
                    while(++index < helper.DataBandNames.Length)
                    {
                        StiDataBand band = helper.Report.CompiledReport.GetComponentByName(helper.DataBandNames[index]) as StiDataBand;
                        if (band != null)
                            band.SelectedLine = helper.SelectedLines[index];
                    }
                }

                helper.Report.Render(false);
                RefreshInteractions(helper.Report);
            }
            catch
            {
                error = true;
            }
            helper.Report.IsInteractionRendering = false;

            string result = null;
            if (error || helper.Report.CompilerResults.Errors.Count > 0)
                result = GetErrorListXml(helper.Report);
            else
                result = CheckReportOnInteractions(helper.Report, true);

            return result;
        }

        private static StiDataBandSelectionContainer DecodeXmlDataBandSelection(string xml)
        {
            var helper = new StiDataBandSelectionContainer();

            System.IO.StringReader stringReader = new System.IO.StringReader(StiSLEncodingHelper.DecodeString(xml));
            XmlTextReader tr = new XmlTextReader(stringReader);

            tr.Read();
            if (tr.Name == "XmlResult")
            {
                while (tr.Read())
                {
                    string name = tr.Name;

                    switch (name)
                    {
                        case "Report":
                            helper.Report.LoadFromString(tr.ReadString());
                            break;
                        case "SelectedLines":
                            int count = int.Parse(tr.GetAttribute("Count"));
                            helper.DataBandNames = new string[count];
                            helper.SelectedLines = new int[count];

                            tr.Read();
                            for (int index = 0; index < count; index++)
                            {
                                // Name
                                helper.DataBandNames[index] = tr.ReadString();
                                tr.Read();

                                // SelectedLine
                                helper.SelectedLines[index] = int.Parse(tr.ReadString());
                                tr.Read();
                            }
                            break;
                    }
                }
            }

            tr.Close();
            stringReader.Close();
            stringReader.Dispose();
            tr = null;
            stringReader = null;

            return helper;
        }

        private static void RefreshInteractions(StiReport report)
        {
            if (report == null || report.RenderedPages.CacheMode) return;

            StiReport templateReport = report.CompiledReport == null ? report : report.CompiledReport;

            #region Create List of DataBand's
            List<StiDataBand> dataBands = new List<StiDataBand>();
            StiComponentsCollection comps = templateReport.GetComponents();
            foreach (StiComponent comp in comps)
            {
                StiDataBand dataBand = comp as StiDataBand;
                if (dataBand != null && dataBand.Sort != null && dataBand.Sort.Length > 0)
                {
                    dataBands.Add(dataBand);
                }
            }
            #endregion

            #region Create List of Interaction's
            Hashtable interactions = GetListOfInteractions(report);
            #endregion

            foreach (StiDataBand dataBand in dataBands)
            {
                List<StiInteraction> addedInteractions = new List<StiInteraction>();
                List<StiInteraction> list = interactions[dataBand.Name] as List<StiInteraction>;

                if (list != null)
                {
                    #region Reset current Interaction's states
                    foreach (StiInteraction interaction in list)
                    {
                        interaction.SortingIndex = 0;
                        interaction.SortingDirection = StiInteractionSortDirection.None;
                    }
                    #endregion

                    #region Process Interaction's for specified DataBand
                    int sortIndex = 1;
                    string sortStr = string.Empty;
                    bool isAsc = true;
                    int index = 0;
                    foreach (string str in dataBand.Sort)
                    {
                        #region Add sorting str
                        if (str != "ASC" && str != "DESC")
                        {
                            if (sortStr.Length == 0) sortStr = str;
                            else sortStr += "." + str;
                        }
                        #endregion

                        if (str == "ASC" || str == "DESC" || index == dataBand.Sort.Length - 1)
                        {
                            #region If Sorting string is not empty then process it
                            if (sortStr.Length > 0)
                            {
                                #region Try to Search sorting string in Interaction's
                                foreach (StiInteraction interaction in list)
                                {
                                    string str2 = interaction.GetSortColumnsString();
                                    if (str2 == sortStr)
                                    {
                                        #region We have finded sorting string
                                        if (isAsc) interaction.SortingDirection = StiInteractionSortDirection.Ascending;
                                        else interaction.SortingDirection = StiInteractionSortDirection.Descending;

                                        interaction.SortingIndex = sortIndex;
                                        list.Remove(interaction);
                                        addedInteractions.Add(interaction);
                                        break;
                                        #endregion
                                    }
                                }
                                #endregion

                                sortStr = string.Empty;
                                sortIndex++;

                                //if we don't have more Interaction's in our list for specified 
                                //DataBand then breaks search
                                if (list.Count == 0) break;
                            }
                            #endregion

                            if (str == "ASC") isAsc = true;
                            else isAsc = false;
                        }
                        index++;
                    }
                    #endregion

                    if (addedInteractions.Count == 1)
                    {
                        addedInteractions[0].SortingIndex = 0;
                    }
                }
            }
        }

        private static Hashtable GetListOfInteractions(StiReport report)
        {
            Hashtable interactions = new Hashtable();
            StiReport currentReport = (report.CompiledReport == null) ? report : report.CompiledReport;

            foreach (StiPage page in currentReport.RenderedPages)
            {
                StiComponentsCollection comps2 = page.GetComponents();
                foreach (StiComponent comp in comps2)
                {
                    if (comp is IStiInteraction)
                    {
                        StiInteraction interaction = ((IStiInteraction)comp).Interaction;
                        if (interaction != null && interaction.SortingEnabled)
                        {
                            string dataBandName = interaction.GetSortDataBandName();
                            List<StiInteraction> list = interactions[dataBandName] as List<StiInteraction>;
                            if (list == null)
                            {
                                list = new List<StiInteraction>();
                                interactions[dataBandName] = list;
                            }

                            list.Add(interaction);
                        }
                    }
                }
            }

            return interactions;
        }
        #endregion

        #region Methods.Helpers
        internal static string GetErrorListXml(StiReport report)
        {
            System.IO.StringWriter str = new System.IO.StringWriter();
            XmlTextWriter writer = new XmlTextWriter(str);

            writer.WriteStartElement("XmlResult");
            writer.WriteStartElement("ErrorList");

            foreach (CompilerError error in report.CompilerResults.Errors)
            {
                writer.WriteStartElement("Error");
                writer.WriteValue(error.ErrorText);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();

            string result = StiSLEncodingHelper.EncodeString(str.ToString());

            writer = null;
            str.Dispose();
            str = null;

            return result;
        }
        #endregion

        #region Methods.SaveReportScript.Input
        public static StiReport ParseXmlSaveReportScript(string xml)
        {
            var report = new StiReport();

            var stringReader = new System.IO.StringReader(xml);
            var tr = new XmlTextReader(stringReader);

            tr.Read();
            if (tr.Name == "XmlResult")
            {
                while (tr.Read())
                {
                    if (tr.Depth == 0)
                    {
                        break;
                    }
                    else
                    {
                        switch (tr.Name)
                        {
                            case "Report":
                                report.LoadFromString(StiSLEncodingHelper.DecodeString(tr.ReadString()));
                                break;

                            case "Script":
                                report.Script = StiSLEncodingHelper.DecodeString(tr.ReadString());
                                break;
                        }
                    }
                }
            }


            tr.Close();
            stringReader.Close();
            stringReader.Dispose();
            stringReader = null;
            tr = null;

            return report;
        }
        #endregion
    }
}