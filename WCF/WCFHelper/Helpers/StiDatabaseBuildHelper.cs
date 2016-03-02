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
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;

using Stimulsoft.Report.Dictionary;
using System.Text;
using WCFHelper.Helpers;
using Stimulsoft.Report;
using System.Collections.Generic;

namespace WCFHelper
{
    public static class StiDatabaseBuildHelper
    {
        #region class SettingsTestConnection
        public class SettingsTestConnection
        {
            public string ConnectionString;
            public StiSqlAdapterService Adapter;
        }
        #endregion

        #region class SettingsRetrieveColumns
        public class SettingsRetrieveColumns
        {
            public string Name;
            public string Alias;
            public string NameInSource;
            public string ConnectionString;
            public string PromptUserNameAndPassword;
            public string SqlCommand;

            public StiSqlAdapterService adapter;
            public StiSqlSource dataSource;
            public DbConnection connection;

            public StiReport Report;
        }
        #endregion

        #region Input
        public static class Input
        {
            public static SettingsTestConnection ParseTestConnection(string xml)
            {
                var settings = new SettingsTestConnection();

                using (var stringReader = new System.IO.StringReader(StiSLEncodingHelper.DecodeString(xml)))
                using (var tr = new XmlTextReader(stringReader))
                {
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
                                    case "TypeAdapter":
                                        string typeStr = tr.ReadString();
                                        if (!string.IsNullOrEmpty(typeStr))
                                            settings.Adapter = Stimulsoft.Base.StiActivator.CreateObject(typeStr) as StiSqlAdapterService;
                                        break;

                                    case "ConnectionString":
                                        settings.ConnectionString = tr.ReadString();
                                        break;
                                }
                            }
                        }
                    }
                }

                return settings;
            }

            #region BuildObjects
            public static StiDatabase ParseBuildObjects(string xml)
            {
                StiDatabase result = null;

                string decodeString = StiSLEncodingHelper.DecodeString(xml);
                using (var stringReader = new System.IO.StringReader(decodeString))
                using (var tr = new XmlTextReader(stringReader))
                {
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
                                string str = tr.ReadString();

                                // BrightEye
                                //if (str.StartsWith("MEScontrol.Reporting"))
                                //    str = typeof(MesDatabase).ToString();

                                result = Stimulsoft.Base.StiActivator.CreateObject(str) as StiDatabase;
                                if (result == null) break;

                                if (result is StiXmlDatabase)
                                {
                                    result = GetXmlDatabase(tr);
                                }
                                else if (result is StiSqlDatabase)
                                {
                                    result = GetSqlDatabase(tr, result as StiSqlDatabase);
                                }

                                break;
                            }
                        }
                    }
                }

                return result;
            }

            private static StiDatabase GetXmlDatabase(XmlTextReader tr)
            {
                var xmlDatabase = new StiXmlDatabase();

                while (tr.Read())
                {
                    if (tr.Depth == 1)
                    {
                        switch (tr.Name)
                        {
                            case "Name":
                                xmlDatabase.Name = tr.ReadString();
                                break;

                            case "Alias":
                                xmlDatabase.Alias = tr.ReadString();
                                break;

                            case "PathData":
                                xmlDatabase.PathData = tr.ReadString();
                                break;

                            case "PathSchema":
                                xmlDatabase.PathSchema = tr.ReadString();
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return xmlDatabase;
            }

            private static StiDatabase GetSqlDatabase(XmlTextReader tr, StiSqlDatabase database)
            {
                StiSqlDatabase sqlDatabase = database;

                while (tr.Read())
                {
                    if (tr.Depth == 1)
                    {
                        switch (tr.Name)
                        {
                            case "Name":
                                sqlDatabase.Name = tr.ReadString();
                                break;

                            case "Alias":
                                sqlDatabase.Alias = tr.ReadString();
                                break;

                            case "ConnectionString":
                                sqlDatabase.ConnectionString = tr.ReadString();
                                break;

                            case "PromptUserNameAndPassword":
                                sqlDatabase.PromptUserNameAndPassword = (tr.ReadString() == "1");
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return sqlDatabase;
            }
            #endregion

            #region ParseRetrieveColumns
            public static SettingsRetrieveColumns ParseRetrieveColumns(string xml)
            {
                var settings = new SettingsRetrieveColumns();

                string decodeString = StiSLEncodingHelper.DecodeString(xml);
                using (var stringReader = new System.IO.StringReader(decodeString))
                using (var tr = new XmlTextReader(stringReader))
                {
                    tr.Read();
                    if (tr.Name == "XmlResult")
                    {
                        while (tr.Read())
                        {
                            if (tr.Depth == 0)
                                break;
                            else
                            {
                                switch (tr.Name)
                                {
                                    case "Report":
                                        settings.Report = new StiReport();
                                        settings.Report.LoadFromString(tr.ReadString());
                                        break;

                                    case "DataAdapterType":
                                        string typeStr = tr.ReadString();

                                        // BrightEye
                                        //if (typeStr.StartsWith("MEScontrol.Reporting"))
                                        //    typeStr = typeof(MesDataAdapterService).ToString();

                                        settings.adapter = Stimulsoft.Base.StiActivator.CreateObject(typeStr) as StiSqlAdapterService;
                                        settings.dataSource = Stimulsoft.Base.StiActivator.CreateObject(settings.adapter.GetDataSourceType()) as StiSqlSource;

                                        settings.connection = CreateDataAdapterTypeByName(settings.adapter.GetType().Name);
                                        break;

                                    case "Name":
                                        settings.Name = tr.ReadString();
                                        break;

                                    case "Alias":
                                        settings.Alias = tr.ReadString();
                                        break;

                                    case "NameInSource":
                                        settings.NameInSource = tr.ReadString();
                                        break;

                                    case "ConnectionString":
                                        settings.ConnectionString = tr.ReadString();
                                        break;

                                    case "PromptUserNameAndPassword":
                                        settings.PromptUserNameAndPassword = tr.ReadString();
                                        break;

                                    case "SqlCommand":
                                        settings.SqlCommand = tr.ReadString();
                                        break;
                                }
                            }
                        }
                    }
                }

                return settings;
            }

            private static DbConnection CreateDataAdapterTypeByName(string name)
            {
                DbConnection result = null;
                switch (name)
                {
                    case "StiSqlAdapterService":
                        result = new SqlConnection();
                        break;

                    case "StiOleDbAdapterService":
                        result = new System.Data.OleDb.OleDbConnection();
                        break;

                    case "StiOdbcAdapterService":
                        result = new System.Data.Odbc.OdbcConnection();
                        break;

                    case "StiMSAccessAdapterService":
                        result = new System.Data.OleDb.OleDbConnection();
                        break;

                    case "StiDB2AdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("IBM.Data.DB2.DB2Connection") as DbConnection;
                        break;

                    case "StiDotConnectUniversalAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Devart.Data.Universal.UniConnection") as DbConnection;
                        break;

                    case "StiEffiProzAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.EffiProz.EfzConnection") as DbConnection;
                        break;

                    case "StiFirebirdAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("FirebirdSql.Data.FirebirdClient.FbConnection") as DbConnection;
                        break;

                    case "StiInformixAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("IBM.Data.Informix.IfxConnection") as DbConnection;
                        break;

                    case "StiMySqlAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("MySql.Data.MySqlClient.MySqlConnection") as DbConnection;
                        break;

                    case "StiOracleAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.OracleClient.OracleConnection") as DbConnection;
                        break;

                    case "StiOracleODPAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Oracle.DataAccess.Client.OracleConnection") as DbConnection;
                        break;

                    case "StiPostgreSQLAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Npgsql.NpgsqlConnection") as DbConnection;
                        break;

                    case "StiSqlCeAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.SqlServerCe.SqlCeConnection") as DbConnection;
                        break;

                    case "StiSQLiteAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("System.Data.SQLite.SQLiteConnection") as DbConnection;
                        break;

                    case "StiSybaseAdsAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Advantage.Data.Provider.AdsConnection") as DbConnection;
                        break;

                    case "StiSybaseAseAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("Sybase.Data.AseClient.AseConnection") as DbConnection;
                        break;

                    case "StiUniDirectAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("CoreLab.UniDirect.UniConnection") as DbConnection;
                        break;

                    case "StiVistaDBAdapterService":
                        result = Stimulsoft.Base.StiActivator.CreateObject("VistaDB.Provider.VistaDBConnection") as DbConnection;
                        break;

                    // BrightEye
                    //case "MesDataAdapterService":
                    //    settings.connection = new MesConnection(ServerHost.Current.Domain);
                    //    continue;
                }

                if (result != null)
                    return result;

                // Если не удалось найти тип из стандартных, вызываем событие, чтобы пользователь сам указал тип
                if (CreateCustomDataAdapterType != null)
                {
                    var args = new StiCustomDataAdapterTypeEventArgs(name);
                    CreateCustomDataAdapterType(null, args);
                    return args.Connection;
                }

                return null;
            }
            #endregion

            #region Events
            public static StiCustomDataAdapterTypeEventHandlers CreateCustomDataAdapterType;
            #endregion
        }
        #endregion

        #region Output
        public static class Output
        {
            public static string ParseBuildObjects(StiDatabaseInformation info)
            {
                using (var str = new System.IO.StringWriter())
                using (var writer = new XmlTextWriter(str))
                {
                    var hashTypes = new Dictionary<Type, int>();
                    int typeIdent = 0;
                    writer.WriteStartElement("Result");

                    #region Tables
                    if (info.Tables.Count > 0)
                    {
                        writer.WriteStartElement("Tables");
                        foreach (DataTable table in info.Tables)
                        {
                            string tableName = CheckName(table.TableName);
                            writer.WriteStartElement(tableName);

                            foreach (DataColumn column in table.Columns)
                            {
                                // Берем сокращенный вариант типа
                                int ident;
                                if (hashTypes.ContainsKey(column.DataType))
                                    ident = hashTypes[column.DataType];
                                else
                                {
                                    ident = typeIdent;
                                    hashTypes.Add(column.DataType, ident);
                                    typeIdent++;
                                }

                                writer.WriteStartElement(CheckName(column.ColumnName));
                                writer.WriteAttributeString("id", ident.ToString());
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    #endregion

                    #region Views
                    if (info.Views.Count > 0)
                    {
                        writer.WriteStartElement("Views");
                        foreach (DataTable table in info.Views)
                        {
                            string tableName = CheckName(table.TableName);
                            writer.WriteStartElement(tableName);

                            foreach (DataColumn column in table.Columns)
                            {
                                // Берем сокращенный вариант типа
                                int ident;
                                if (hashTypes.ContainsKey(column.DataType))
                                    ident = hashTypes[column.DataType];
                                else
                                {
                                    ident = typeIdent;
                                    hashTypes.Add(column.DataType, ident);
                                    typeIdent++;
                                }

                                writer.WriteStartElement(CheckName(column.ColumnName));
                                writer.WriteAttributeString("id", ident.ToString());
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    #endregion

                    #region StoredProcedures
                    if (info.StoredProcedures.Count > 0)
                    {
                        writer.WriteStartElement("StoredProcedures");
                        foreach (DataTable table in info.StoredProcedures)
                        {
                            if (table.TableName.IndexOfAny(new char[] { '~', '(', ')' }) != -1) continue;

                            string tableName = CheckName(table.TableName);
                            writer.WriteStartElement(tableName);

                            foreach (DataColumn column in table.Columns)
                            {
                                // Берем сокращенный вариант типа
                                int ident;
                                if (hashTypes.ContainsKey(column.DataType))
                                    ident = hashTypes[column.DataType];
                                else
                                {
                                    ident = typeIdent;
                                    hashTypes.Add(column.DataType, ident);
                                    typeIdent++;
                                }

                                writer.WriteStartElement(CheckName(column.ColumnName));
                                writer.WriteAttributeString("id", ident.ToString());
                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    #endregion

                    #region Сохраняем кэш типов
                    writer.WriteStartElement("hash");
                    foreach (var pair in hashTypes)
                    {
                        writer.WriteStartElement("i" + pair.Value.ToString());
                        writer.WriteString(pair.Key.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    #endregion

                    writer.WriteEndElement();

                    hashTypes.Clear();
                    hashTypes = null;

                    return StiSLEncodingHelper.EncodeString(str.ToString());
                }
            }

            public static string ParseRetrieveColumns(StiDataColumnsCollection columns)
            {
                if (columns == null || columns.Count == 0) return " ";

                using (var str = new System.IO.StringWriter())
                using (var writer = new XmlTextWriter(str))
                {
                    writer.WriteStartElement("Result");

                    #region Columns
                    writer.WriteStartElement("Columns");
                    foreach (StiDataColumn column in columns)
                    {
                        string columnName = CheckName(column.Name);

                        writer.WriteStartElement(columnName);
                        writer.WriteValue(column.Type.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    #endregion

                    writer.WriteEndElement();
                    return StiSLEncodingHelper.EncodeString(str.ToString());
                }
            }
        }
        #endregion

        #region Methods.Helpers
        public static string CheckName(string name)
        {
            var builder = new StringBuilder(name);
            builder.Replace(" ", "_x0020_");
            builder.Replace("@", "_x0040_");
            builder.Replace("~", "_x007e_");
            builder.Replace("$", "_x0024_");
            builder.Replace("#", "_x0023_");
            builder.Replace("%", "_x0025_");
            builder.Replace("&", "_x0026_");
            builder.Replace("*", "_x002A_");
            builder.Replace("^", "_x005E_");
            builder.Replace("(", "_x0028_");
            builder.Replace(")", "_x0029_");
            builder.Replace("!", "_x0021_");

            return builder.ToString();
        }
        #endregion
    }
}