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

namespace WCFHelper
{
    #region StiSLExportType
    internal enum StiSLExportType
    {
        Csv,
        Dbf,
        Dif,
        Excel,
        Excel2007,
        ExcelXml,
        Html,
        Html5,
        Mht,
        Bmp,
        Gif,
        Jpeg,
        Emf,
        Pcx,
        Png,
        Svg,
        Svgz,
        Tiff,
        Ods,
        Odt,
        Pdf,
        Rtf,
        Sylk,
        Text,
        Word2007,
        Xps,
        Ppt2007,
        Xml
    }
    #endregion

    #region StiRequestFromUserType
    internal enum StiRequestFromUserType
    {
        ListBool,
        ListChar,
        ListDateTime,
        ListTimeSpan,
        ListDecimal,
        ListFloat,
        ListDouble,
        ListByte,
        ListShort,
        ListInt,
        ListLong,
        ListGuid,
        ListString,
        RangeChar,
        RangeDateTime,
        RangeDouble,
        RangeFloat,
        RangeDecimal,
        RangeGuid,
        RangeByte,
        RangeShort,
        RangeInt,
        RangeLong,
        RangeString,
        RangeTimeSpan,
        ValueBool,
        ValueChar,
        ValueDateTime,
        ValueFloat,
        ValueDouble,
        ValueDecimal,
        ValueGuid,
        ValueImage,
        ValueString,
        ValueTimeSpan,
        ValueShort,
        ValueInt,
        ValueLong,
        ValueSbyte,
        ValueUshort,
        ValueUint,
        ValueUlong,
        ValueByte,
        ValueNullableBool,
        ValueNullableChar,
        ValueNullableDateTime,
        ValueNullableFloat,
        ValueNullableDouble,
        ValueNullableDecimal,
        ValueNullableGuid,
        ValueNullableTimeSpan,
        ValueNullableShort,
        ValueNullableInt,
        ValueNullableLong,
        ValueNullableSbyte,
        ValueNullableUshort,
        ValueNullableUint,
        ValueNullableUlong,
        ValueNullableByte
    }
    #endregion
}