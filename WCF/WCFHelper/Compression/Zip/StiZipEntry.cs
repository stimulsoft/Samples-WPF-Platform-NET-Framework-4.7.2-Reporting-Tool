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
using System.IO;

namespace WCFHelper.Compression
{
    internal class StiZipEntry
    {
        #region  enum StiKnown
        [Flags]
        private enum StiKnown : byte
        {
            None = 0,
            Size = 0x01,
            CompressedSize = 0x02,
            Crc = 0x04,
            Time = 0x08,
            ExternalAttributes = 0x10,
        }
        #endregion

        #region Fields
        private StiKnown known;
        private int externalFileAttributes = -1;
        private ushort versionMadeBy;
        private ushort versionToExtract;
        private uint dosTime;
        internal bool forceZip64;

        public int Flags;
        public long ZipFileIndex = -1;
        public long Offset;
        internal byte CryptoCheckValue;
        internal string Name;
        #endregion

        #region Properties
        internal bool IsUnicodeText
        {
            set
            {
                if (value)
                    Flags |= (int)StiGeneralBitFlags.UnicodeText;
                else
                    Flags &= ~(int)StiGeneralBitFlags.UnicodeText;
            }
        }

        internal int ExternalFileAttributes
        {
            get
            {
                if ((known & StiKnown.ExternalAttributes) == 0)
                    return -1;
                else
                    return externalFileAttributes;
            }
            set
            {
                externalFileAttributes = value;
                known |= StiKnown.ExternalAttributes;
            }
        }
        
        internal bool HasDosAttributes(int attributes)
        {
            bool result = false;
            if ((known & StiKnown.ExternalAttributes) != 0)
            {
                if (((HostSystem == (int)StiHostSystemID.Msdos) ||
                      (HostSystem == (int)StiHostSystemID.WindowsNT)) &&
                     (ExternalFileAttributes & attributes) == attributes)
                {
                    result = true;
                }
            }

            return result;
        }

        internal int HostSystem
        {
            get
            {
                return (versionMadeBy >> 8) & 0xff;
            }
        }

        internal int Version
        {
            get
            {
                if (versionToExtract != 0)
                {
                    return versionToExtract;
                }
                else
                {
                    int result = 10;
                    if (CentralHeaderRequiresZip64)
                    {
                        result = StiZipConstants.VersionZip64;
                    }
                    else if (StiCompressionMethod.Deflated == compMethod)
                    {
                        result = 20;
                    }
                    else if (IsDirectory)
                    {
                        result = 20;
                    }
                    else if (HasDosAttributes(0x08))
                    {
                        result = 11;
                    }

                    return result;
                }
            }
        }

        internal bool CanDecompress
        {
            get
            {
                return (Version <= StiZipConstants.VersionMadeBy) &&
                       ((Version == 10) || (Version == 11) || (Version == 20) || (Version == 45)) && IsCompressionMethodSupported();
            }
        }

        public bool LocalHeaderRequiresZip64
        {
            get
            {
                bool result = forceZip64;

                if (!result)
                {
                    ulong trueCompressedSize = compressedSize;
                    result = ((this.size >= uint.MaxValue) || (trueCompressedSize >= uint.MaxValue)) &&
                        ((versionToExtract == 0) || (versionToExtract >= StiZipConstants.VersionZip64));
                }

                return result;
            }
        }

        public bool CentralHeaderRequiresZip64
        {
            get
            {
                return LocalHeaderRequiresZip64 || (Offset >= uint.MaxValue);
            }
        }

        public long DosTime
        {
            get
            {
                if ((known & StiKnown.Time) == 0)
                {
                    return 0;
                }
                else
                {
                    return dosTime;
                }
            }
            set
            {
                unchecked
                {
                    dosTime = (uint)value;
                }

                known |= StiKnown.Time;
            }
        }

        public DateTime DateTime
        {
            get
            {
                int sec = Math.Min(59, (int)(2 * (dosTime & 0x1f)));
                int min = Math.Min(59, (int)((dosTime >> 5) & 0x3f));
                int hrs = Math.Min(23, (int)((dosTime >> 11) & 0x1f));
                int mon = Math.Max(1, Math.Min(12, (int)((dosTime >> 21) & 0xf)));
                uint year = ((dosTime >> 25) & 0x7f) + 1980;
                int day = Math.Max(1, Math.Min(DateTime.DaysInMonth((int)year, mon), (int)((dosTime >> 16) & 0x1f)));
                return new DateTime((int)year, mon, day, hrs, min, sec);
            }
            set
            {
                uint year = (uint)value.Year;
                uint month = (uint)value.Month;
                uint day = (uint)value.Day;
                uint hour = (uint)value.Hour;
                uint minute = (uint)value.Minute;
                uint second = (uint)value.Second;

                if (year < 1980)
                {
                    year = 1980;
                    month = 1;
                    day = 1;
                    hour = 0;
                    minute = 0;
                    second = 0;
                }
                else if (year > 2107)
                {
                    year = 2107;
                    month = 12;
                    day = 31;
                    hour = 23;
                    minute = 59;
                    second = 59;
                }

                DosTime = ((year - 1980) & 0x7f) << 25 |
                          (month << 21) |
                          (day << 16) |
                          (hour << 11) |
                          (minute << 5) |
                          (second >> 1);
            }
        }

        private ulong size;
        public long Size
        {
            get
            {
                return (known & StiKnown.Size) != 0 ? (long)size : -1L;
            }
            set
            {
                this.size = (ulong)value;
                this.known |= StiKnown.Size;
            }
        }

        private ulong compressedSize;
        public long CompressedSize
        {
            get
            {
                return (known & StiKnown.CompressedSize) != 0 ? (long)compressedSize : -1L;
            }
            set
            {
                this.compressedSize = (ulong)value;
                this.known |= StiKnown.CompressedSize;
            }
        }

        private uint crc;
        public long Crc
        {
            get
            {
                return (known & StiKnown.Crc) != 0 ? crc & 0xffffffffL : -1L;
            }
            set
            {
                if (((ulong)crc & 0xffffffff00000000L) != 0)
                    throw new ArgumentOutOfRangeException("value");

                this.crc = (uint)value;
                this.known |= StiKnown.Crc;
            }
        }

        private StiCompressionMethod compMethod = StiCompressionMethod.Deflated;
        internal StiCompressionMethod CompressionMethod
        {
            get
            {
                return compMethod;
            }
            set
            {
                if (!IsCompressionMethodSupported(value))
                    throw new NotSupportedException("Compression method not supported");
                this.compMethod = value;
            }
        }

        private byte[] extra;
        public byte[] ExtraData
        {
            get
            {
                return extra;
            }
            set
            {
                if (value == null)
                {
                    extra = null;
                }
                else
                {
                    if (value.Length > 0xffff)
                        throw new System.ArgumentOutOfRangeException("value");

                    extra = new byte[value.Length];
                    Array.Copy(value, 0, extra, 0, value.Length);
                }
            }
        }

        private string comment;
        public string Comment
        {
            get
            {
                return comment;
            }
            set
            {
                if ((value != null) && (value.Length > 0xffff))
                    throw new ArgumentOutOfRangeException("value", "cannot exceed 65535");

                comment = value;
            }
        }

        public bool IsDirectory
        {
            get
            {
                int nameLength = Name.Length;
                bool result =
                    ((nameLength > 0) &&
                    ((Name[nameLength - 1] == '/') || (Name[nameLength - 1] == '\\'))) ||
                    HasDosAttributes(16);

                return result;
            }
        }
        #endregion

        #region Methods
        internal void ProcessExtraData(bool localHeader)
        {
            StiZipExtraData extraData = new StiZipExtraData(this.extra);

            if (extraData.Find(0x0001))
            {
                if ((versionToExtract & 0xff) < StiZipConstants.VersionZip64)
                    throw new Exception("Zip64 Extended information found but version is not valid");

                forceZip64 = true;

                if (extraData.ValueLength < 4)
                    throw new Exception("Extra data extended Zip64 information length is invalid");

                if (localHeader || (size == uint.MaxValue))
                    size = (ulong)extraData.ReadLong();

                if (localHeader || (compressedSize == uint.MaxValue))
                    compressedSize = (ulong)extraData.ReadLong();

                if (!localHeader && (Offset == uint.MaxValue))
                    Offset = extraData.ReadLong();
            }
            else
            {
                if (((versionToExtract & 0xff) >= StiZipConstants.VersionZip64) &&
                    ((size == uint.MaxValue) || (compressedSize == uint.MaxValue)))
                    throw new Exception("Zip64 Extended information required but is missing.");
            }

            if (extraData.Find(10))
            {
                if (extraData.ValueLength < 8)
                    throw new Exception("NTFS Extra data invalid");

                extraData.ReadInt();

                while (extraData.UnreadCount >= 4)
                {
                    int ntfsTag = extraData.ReadShort();
                    int ntfsLength = extraData.ReadShort();
                    if (ntfsTag == 1)
                    {
                        if (ntfsLength >= 24)
                        {
                            long lastModification = extraData.ReadLong();
                            long lastAccess = extraData.ReadLong();
                            long createTime = extraData.ReadLong();

                            DateTime = System.DateTime.FromFileTime(lastModification);
                        }
                        break;
                    }
                    else
                    {
                        extraData.Skip(ntfsLength);
                    }
                }
            }
            else if (extraData.Find(0x5455))
            {
                int length = extraData.ValueLength;
                int flags = extraData.ReadByte();

                if (((flags & 1) != 0) && (length >= 5))
                {
                    int iTime = extraData.ReadInt();

                    DateTime = (new System.DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime() +
                                new TimeSpan(0, 0, 0, iTime, 0)).ToLocalTime();
                }
            }
        }

        public bool IsCompressionMethodSupported()
        {
            return IsCompressionMethodSupported(CompressionMethod);
        }
        #endregion

        #region Methods override
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Methods static
        internal static bool IsCompressionMethodSupported(StiCompressionMethod method)
        {
            return (method == StiCompressionMethod.Deflated || method == StiCompressionMethod.Stored);
        }
        #endregion

        public StiZipEntry(string name)
            : this(name, 0, StiZipConstants.VersionMadeBy, StiCompressionMethod.Deflated)
        {
        }

        internal StiZipEntry(string name, ushort versionRequiredToExtract)
            : this(name, versionRequiredToExtract, StiZipConstants.VersionMadeBy, StiCompressionMethod.Deflated)
        {
        }

        internal StiZipEntry(string name, ushort versionRequiredToExtract, ushort madeByInfo, StiCompressionMethod compMethod)
        {
            if (name == null)
                throw new System.ArgumentNullException("StiZipEntry name");
            if (name.Length > 0xffff)
                throw new ArgumentException("Name is too long", "name");
            if ((versionRequiredToExtract != 0) && (versionRequiredToExtract < 10))
                throw new ArgumentOutOfRangeException("versionRequiredToExtract");

            this.DateTime = System.DateTime.Now;
            this.Name = name;
            this.versionMadeBy = madeByInfo;
            this.versionToExtract = versionRequiredToExtract;
            this.compMethod = compMethod;
        }
    }
}