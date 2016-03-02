#region Copyright (C) 2003-2012 Stimulsoft
/*
{*******************************************************************}
{																	}
{	Stimulsoft Reports  											}
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
using System.Text;

namespace WCFHelper.Compression
{
    internal static class StiZipConstants
    {
        #region Fields const
        public const int VersionMadeBy = 45;        
        public const int VersionZip64 = 45;

        public const int LocalHeaderBaseSize = 30;
        public const int Zip64DataDescriptorSize = 24;
        public const int DataDescriptorSize = 16;
        public const int CentralHeaderBaseSize = 46;
        public const int CryptoHeaderSize = 12;

        public const int LocalHeaderSignature = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);
        public const int SpanningSignature = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
        public const int SpanningTempSignature = 'P' | ('K' << 8) | ('0' << 16) | ('0' << 24);
        public const int DataDescriptorSignature = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
        public const int CentralHeaderSignature = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);
        public const int Zip64CentralFileHeaderSignature = 'P' | ('K' << 8) | (6 << 16) | (6 << 24);
        public const int Zip64CentralDirLocatorSignature = 'P' | ('K' << 8) | (6 << 16) | (7 << 24);
        public const int ArchiveExtraDataSignature = 'P' | ('K' << 8) | (6 << 16) | (7 << 24);
        public const int CentralHeaderDigitalSignature = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);
        public const int EndOfCentralDirectorySignature = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
        #endregion

        #region Methods
        public static string ConvertToString(byte[] data, int count)
        {
            if (data == null)
                return string.Empty;

            return Encoding.UTF8.GetString(data, 0, count);
        }

        public static string ConvertToStringExt(int flags, byte[] data)
        {
            if (data == null)
                return string.Empty;

            if ((flags & (int)StiGeneralBitFlags.UnicodeText) != 0)
                return Encoding.UTF8.GetString(data, 0, data.Length);
            else
                return ConvertToString(data, data.Length);
        }

        public static byte[] ConvertToArray(string str)
        {
            return (str == null) ? new byte[0] : Encoding.UTF8.GetBytes(str);
        }

        public static byte[] ConvertToArray(int flags, string str)
        {
            if (str == null)
                return new byte[0];

            return ((flags & (int)StiGeneralBitFlags.UnicodeText) != 0) ? Encoding.UTF8.GetBytes(str) : ConvertToArray(str);
        }
        #endregion
    }
}