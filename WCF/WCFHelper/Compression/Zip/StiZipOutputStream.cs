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
using System.Collections.Generic;
using System.IO;

namespace WCFHelper.Compression
{
    internal class StiZipOutputStream : StiDeflaterOutputStream
    {
        #region Fields
        private readonly StiCrc32 crc = new StiCrc32();
        private long crcPatchPos = -1;
        private StiZipEntry curEntry;
        private StiCompressionMethod curMethod = StiCompressionMethod.Deflated;
        private int defaultCompressionLevel = StiDeflater.DEFAULT_COMPRESSION;
        private List<StiZipEntry> entries = new List<StiZipEntry>();
        private long offset;
        private bool patchEntryHeader;
        private long size;
        private long sizePatchPos = -1;
        private byte[] zipComment = new byte[0];
        private StiUseZip64 useZip64 = StiUseZip64.Dynamic;
        #endregion

        #region Methods
        public void SetLevel(int level)
        {
            deflater.SetLevel(level);
            defaultCompressionLevel = level;
        }

        private void WriteLeShort(int value)
        {
            unchecked
            {
                baseOutputStream.WriteByte((byte)(value & 0xff));
                baseOutputStream.WriteByte((byte)((value >> 8) & 0xff));
            }
        }

        private void WriteLeInt(int value)
        {
            unchecked
            {
                WriteLeShort(value);
                WriteLeShort(value >> 16);
            }
        }

        private void WriteLeLong(long value)
        {
            unchecked
            {
                WriteLeInt((int)value);
                WriteLeInt((int)(value >> 32));
            }
        }

        internal void PutNextEntry(StiZipEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");
            if (entries == null)
                throw new InvalidOperationException("StiZipOutputStream was finished");

            if (curEntry != null)
                CloseEntry();

            if (entries.Count == int.MaxValue)
                throw new Exception("Too many entries for Zip file");

            var method = entry.CompressionMethod;
            int compressionLevel = defaultCompressionLevel;

            entry.Flags &= (int)StiGeneralBitFlags.UnicodeText;
            patchEntryHeader = false;
            bool headerInfoAvailable = true;

            if (method == StiCompressionMethod.Stored)
            {
                entry.Flags &= ~8;
                if (entry.CompressedSize >= 0)
                {
                    if (entry.Size < 0)
                    {
                        entry.Size = entry.CompressedSize;
                    }
                    else if (entry.Size != entry.CompressedSize)
                    {
                        throw new Exception("Method STORED, but compressed size != size");
                    }
                }
                else
                {
                    if (entry.Size >= 0)
                        entry.CompressedSize = entry.Size;
                }

                if (entry.Size < 0 || entry.Crc < 0)
                {
                    if (CanPatchEntries)
                    {
                        headerInfoAvailable = false;
                    }
                    else
                    {
                        method = StiCompressionMethod.Deflated;
                        compressionLevel = 0;
                    }
                }
            }

            if (method == StiCompressionMethod.Deflated)
            {
                if (entry.Size == 0)
                {
                    entry.CompressedSize = entry.Size;
                    entry.Crc = 0;
                    method = StiCompressionMethod.Stored;
                }
                else if ((entry.CompressedSize < 0) || (entry.Size < 0) || (entry.Crc < 0))
                {
                    headerInfoAvailable = false;
                }
            }

            if (headerInfoAvailable == false)
            {
                if (CanPatchEntries == false)
                    entry.Flags |= 8;
                else
                    patchEntryHeader = true;
            }

            entry.Offset = offset;
            entry.CompressionMethod = method;

            curMethod = method;
            sizePatchPos = -1;

            if ((useZip64 == StiUseZip64.On) || ((entry.Size < 0) && (useZip64 == StiUseZip64.Dynamic)))
                entry.forceZip64 = true;

            WriteLeInt(StiZipConstants.LocalHeaderSignature);

            WriteLeShort(entry.Version);
            WriteLeShort(entry.Flags);
            WriteLeShort((byte)method);
            WriteLeInt((int)entry.DosTime);

            if (headerInfoAvailable)
            {
                WriteLeInt((int)entry.Crc);
                if (entry.LocalHeaderRequiresZip64)
                {
                    WriteLeInt(-1);
                    WriteLeInt(-1);
                }
                else
                {
                    WriteLeInt((int)entry.CompressedSize);
                    WriteLeInt((int)entry.Size);
                }
            }
            else
            {
                if (patchEntryHeader)
                {
                    crcPatchPos = baseOutputStream.Position;
                }
                WriteLeInt(0);

                if (patchEntryHeader)
                {
                    sizePatchPos = baseOutputStream.Position;
                }

                if (entry.LocalHeaderRequiresZip64 && patchEntryHeader)
                {
                    WriteLeInt(-1);
                    WriteLeInt(-1);
                }
                else
                {
                    WriteLeInt(0);
                    WriteLeInt(0);
                }
            }

            byte[] name = StiZipConstants.ConvertToArray(entry.Flags, entry.Name);

            if (name.Length > 0xFFFF)
                throw new Exception("Entry name too long.");

            StiZipExtraData ed = new StiZipExtraData(entry.ExtraData);

            if (entry.LocalHeaderRequiresZip64 && (headerInfoAvailable || patchEntryHeader))
            {
                ed.StartNewEntry();
                if (headerInfoAvailable)
                {
                    ed.AddLeLong(entry.Size);
                    ed.AddLeLong(entry.CompressedSize);
                }
                else
                {
                    ed.AddLeLong(-1);
                    ed.AddLeLong(-1);
                }
                ed.AddNewEntry(1);

                if (!ed.Find(1))
                    throw new Exception("Internal error cant find extra data");

                if (patchEntryHeader)
                    sizePatchPos = ed.CurrentReadIndex;
            }
            else
            {
                ed.Delete(1);
            }

            byte[] extra = ed.GetEntryData();

            WriteLeShort(name.Length);
            WriteLeShort(extra.Length);

            if (name.Length > 0)
                baseOutputStream.Write(name, 0, name.Length);
            if (entry.LocalHeaderRequiresZip64 && patchEntryHeader)
                sizePatchPos += baseOutputStream.Position;
            if (extra.Length > 0)
                baseOutputStream.Write(extra, 0, extra.Length);

            offset += StiZipConstants.LocalHeaderBaseSize + name.Length + extra.Length;

            curEntry = entry;
            crc.Reset();
            if (method == StiCompressionMethod.Deflated)
            {
                deflater.Reset();
                deflater.SetLevel(compressionLevel);
            }
            size = 0;
        }

        internal void CloseEntry()
        {
            if (curEntry == null)
                throw new InvalidOperationException("No open entry");
            if (curMethod == StiCompressionMethod.Deflated)
                base.Finish();

            long csize = (curMethod == StiCompressionMethod.Deflated) ? deflater.TotalOut : size;

            if (curEntry.Size < 0)
            {
                curEntry.Size = size;
            }
            else if (curEntry.Size != size)
                throw new Exception("size was " + size + ", but I expected " + curEntry.Size);

            if (curEntry.CompressedSize < 0)
            {
                curEntry.CompressedSize = csize;
            }
            else if (curEntry.CompressedSize != csize)
                throw new Exception("compressed size was " + csize + ", but I expected " + curEntry.CompressedSize);

            if (curEntry.Crc < 0)
            {
                curEntry.Crc = crc.Value;
            }
            else if (curEntry.Crc != crc.Value)
                throw new Exception("crc was " + crc.Value + ", but I expected " + curEntry.Crc);

            offset += csize;

            if (patchEntryHeader)
            {
                patchEntryHeader = false;

                long curPos = baseOutputStream.Position;
                baseOutputStream.Seek(crcPatchPos, SeekOrigin.Begin);
                WriteLeInt((int)curEntry.Crc);

                if (curEntry.LocalHeaderRequiresZip64)
                {
                    if (sizePatchPos == -1)
                        throw new Exception("Entry requires zip64 but this has been turned off");

                    baseOutputStream.Seek(sizePatchPos, SeekOrigin.Begin);
                    WriteLeLong(curEntry.Size);
                    WriteLeLong(curEntry.CompressedSize);
                }
                else
                {
                    WriteLeInt((int)curEntry.CompressedSize);
                    WriteLeInt((int)curEntry.Size);
                }
                baseOutputStream.Seek(curPos, SeekOrigin.Begin);
            }

            if ((curEntry.Flags & 8) != 0)
            {
                WriteLeInt(StiZipConstants.DataDescriptorSignature);
                WriteLeInt(unchecked((int)curEntry.Crc));

                if (curEntry.LocalHeaderRequiresZip64)
                {
                    WriteLeLong(curEntry.CompressedSize);
                    WriteLeLong(curEntry.Size);
                    offset += StiZipConstants.Zip64DataDescriptorSize;
                }
                else
                {
                    WriteLeInt((int)curEntry.CompressedSize);
                    WriteLeInt((int)curEntry.Size);
                    offset += StiZipConstants.DataDescriptorSize;
                }
            }

            entries.Add(curEntry);
            curEntry = null;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (curEntry == null)
                throw new InvalidOperationException("No open entry.");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Cannot be negative");
            if ((buffer.Length - offset) < count)
                throw new ArgumentException("Invalid offset/count combination");

            crc.Update(buffer, offset, count);
            size += count;

            switch (curMethod)
            {
                case StiCompressionMethod.Deflated:
                    base.Write(buffer, offset, count);
                    break;

                case StiCompressionMethod.Stored:
                    baseOutputStream.Write(buffer, offset, count);
                    break;
            }
        }
        #endregion

        #region Methods override
        public override void Finish()
        {
            if (entries == null)
                return;
            if (curEntry != null)
                CloseEntry();

            long numEntries = entries.Count;
            long sizeEntries = 0;

            foreach (var entry in entries)
            {
                WriteLeInt(StiZipConstants.CentralHeaderSignature);
                WriteLeShort(StiZipConstants.VersionMadeBy);
                WriteLeShort(entry.Version);
                WriteLeShort(entry.Flags);
                WriteLeShort((short)entry.CompressionMethod);
                WriteLeInt((int)entry.DosTime);
                WriteLeInt((int)entry.Crc);

                if (entry.forceZip64 || (entry.CompressedSize >= uint.MaxValue))
                    WriteLeInt(-1);
                else
                    WriteLeInt((int)entry.CompressedSize);

                if (entry.forceZip64 || (entry.Size >= uint.MaxValue))
                    WriteLeInt(-1);
                else
                    WriteLeInt((int)entry.Size);

                byte[] name = StiZipConstants.ConvertToArray(entry.Flags, entry.Name);

                if (name.Length > 0xffff)
                    throw new Exception("Name too long.");

                StiZipExtraData ed = new StiZipExtraData(entry.ExtraData);

                if (entry.CentralHeaderRequiresZip64)
                {
                    ed.StartNewEntry();
                    if (entry.forceZip64 || (entry.Size >= 0xffffffff))
                        ed.AddLeLong(entry.Size);

                    if (entry.forceZip64 || (entry.CompressedSize >= 0xffffffff))
                        ed.AddLeLong(entry.CompressedSize);

                    if (entry.Offset >= 0xffffffff)
                        ed.AddLeLong(entry.Offset);

                    ed.AddNewEntry(1);
                }
                else
                {
                    ed.Delete(1);
                }

                byte[] extra = ed.GetEntryData();
                byte[] entryComment = (entry.Comment != null) ? StiZipConstants.ConvertToArray(entry.Flags, entry.Comment) : new byte[0];

                if (entryComment.Length > 0xffff)
                    throw new Exception("Comment too long.");

                WriteLeShort(name.Length);
                WriteLeShort(extra.Length);
                WriteLeShort(entryComment.Length);
                WriteLeShort(0);
                WriteLeShort(0);

                if (entry.ExternalFileAttributes != -1)
                {
                    WriteLeInt(entry.ExternalFileAttributes);
                }
                else
                {
                    if (entry.IsDirectory)
                        WriteLeInt(16);
                    else
                        WriteLeInt(0);
                }

                if (entry.Offset >= uint.MaxValue)
                    WriteLeInt(-1);
                else
                    WriteLeInt((int)entry.Offset);

                if (name.Length > 0)
                    baseOutputStream.Write(name, 0, name.Length);
                if (extra.Length > 0)
                    baseOutputStream.Write(extra, 0, extra.Length);
                if (entryComment.Length > 0)
                    baseOutputStream.Write(entryComment, 0, entryComment.Length);

                sizeEntries += StiZipConstants.CentralHeaderBaseSize + name.Length + extra.Length + entryComment.Length;
            }

            using (var zhs = new StiZipHelperStream(baseOutputStream))
            {
                zhs.WriteEndOfCentralDirectory(numEntries, sizeEntries, offset, zipComment);
            }

            entries = null;
        }
        #endregion

        public StiZipOutputStream(Stream baseOutputStream)
            : base(baseOutputStream, new StiDeflater(StiDeflater.DEFAULT_COMPRESSION, true))
        {
        }
    }
}