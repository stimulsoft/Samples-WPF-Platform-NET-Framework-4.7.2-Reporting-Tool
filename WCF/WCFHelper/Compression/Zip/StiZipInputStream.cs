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
    internal class StiZipInputStream : StiInflaterInputStream
    {
        #region Fields
        private StiCrc32 crc = new StiCrc32();
        private StiZipEntry entry;
        private int flags;
        private ReaderDelegate internalReader;
        private int method;
        
        private long size;
        private delegate int ReaderDelegate(byte[] b, int offset, int length);
        #endregion

        #region Properties
        public bool CanDecompressEntry
        {
            get 
            { 
                return (entry != null) && entry.CanDecompress; 
            }
        }

        public override int Available
        {
            get 
            { 
                return (entry != null) ? 1 : 0; 
            }
        }

        public override long Length
        {
            get
            {
                if (entry != null)
                {
                    if (entry.Size >= 0)
                        return entry.Size;
                    else
                        throw new Exception("Length not available for the current entry");
                }
                else
                {
                    throw new InvalidOperationException("No current entry");
                }
            }
        }
        #endregion

        #region Methods
        internal StiZipEntry GetNextEntry()
        {
            if (crc == null)
                throw new InvalidOperationException("Closed.");

            if (entry != null)
                CloseEntry();

            int header = inputBuffer.ReadLeInt();

            if (header == StiZipConstants.CentralHeaderSignature ||
                header == StiZipConstants.EndOfCentralDirectorySignature ||
                header == StiZipConstants.CentralHeaderDigitalSignature ||
                header == StiZipConstants.ArchiveExtraDataSignature ||
                header == StiZipConstants.Zip64CentralFileHeaderSignature)
            {
                Close();
                return null;
            }

            if ((header == StiZipConstants.SpanningTempSignature) || (header == StiZipConstants.SpanningSignature))
                header = inputBuffer.ReadLeInt();

            if (header != StiZipConstants.LocalHeaderSignature)
                throw new Exception("Wrong Local header signature: 0x" + String.Format("{0:X}", header));

            ushort versionRequiredToExtract = (ushort)inputBuffer.ReadLeShort();

            flags = inputBuffer.ReadLeShort();
            method = inputBuffer.ReadLeShort();
            uint dostime = (uint)inputBuffer.ReadLeInt();
            int crc2 = inputBuffer.ReadLeInt();
            csize = inputBuffer.ReadLeInt();
            size = inputBuffer.ReadLeInt();
            int nameLen = inputBuffer.ReadLeShort();
            int extraLen = inputBuffer.ReadLeShort();

            bool isCrypted = (flags & 1) == 1;

            byte[] buffer = new byte[nameLen];
            inputBuffer.ReadRawBuffer(buffer, 0, buffer.Length);

            string name = StiZipConstants.ConvertToStringExt(flags, buffer);

            entry = new StiZipEntry(name, versionRequiredToExtract);
            entry.Flags = flags;

            entry.CompressionMethod = (StiCompressionMethod)method;

            if ((flags & 8) == 0)
            {
                entry.Crc = crc2 & 0xFFFFFFFFL;
                entry.Size = size & 0xFFFFFFFFL;
                entry.CompressedSize = csize & 0xFFFFFFFFL;

                entry.CryptoCheckValue = (byte)((crc2 >> 24) & 0xff);
            }
            else
            {
                if (crc2 != 0)
                    entry.Crc = crc2 & 0xFFFFFFFFL;

                if (size != 0)
                    entry.Size = size & 0xFFFFFFFFL;

                if (csize != 0)
                    entry.CompressedSize = csize & 0xFFFFFFFFL;

                entry.CryptoCheckValue = (byte)((dostime >> 8) & 0xff);
            }

            entry.DosTime = dostime;

            if (extraLen > 0)
            {
                var extra = new byte[extraLen];
                inputBuffer.ReadRawBuffer(extra, 0, extra.Length);
                entry.ExtraData = extra;
            }

            entry.ProcessExtraData(true);
            if (entry.CompressedSize >= 0)
                csize = entry.CompressedSize;

            if (entry.Size >= 0)
                size = entry.Size;

            if (method == (int)StiCompressionMethod.Stored && (!isCrypted && csize != size || (isCrypted && csize - StiZipConstants.CryptoHeaderSize != size)))
                throw new Exception("Stored, but compressed != uncompressed");

            if (entry.IsCompressionMethodSupported())
                internalReader = InitialRead;
            else
                internalReader = ReadingNotSupported;

            return entry;
        }

        private void ReadDataDescriptor()
        {
            if (inputBuffer.ReadLeInt() != StiZipConstants.DataDescriptorSignature)
                throw new Exception("Data descriptor signature not found");

            entry.Crc = inputBuffer.ReadLeInt() & 0xFFFFFFFFL;

            if (entry.LocalHeaderRequiresZip64)
            {
                csize = inputBuffer.ReadLeLong();
                size = inputBuffer.ReadLeLong();
            }
            else
            {
                csize = inputBuffer.ReadLeInt();
                size = inputBuffer.ReadLeInt();
            }
            entry.CompressedSize = csize;
            entry.Size = size;
        }

        private void CompleteCloseEntry(bool testCrc)
        {
            StopDecrypting();

            if ((flags & 8) != 0)
                ReadDataDescriptor();

            size = 0;

            if (testCrc && ((crc.Value & 0xFFFFFFFFL) != entry.Crc) && (entry.Crc != -1))
                throw new Exception("CRC mismatch");

            crc.Reset();

            if (method == (int)StiCompressionMethod.Deflated)
                inf.Reset();
            
            entry = null;
        }

        public void CloseEntry()
        {
            if (crc == null)
                throw new InvalidOperationException("Closed");
            if (entry == null)
                return;

            if (method == (int)StiCompressionMethod.Deflated)
            {
                if ((flags & 8) != 0)
                {
                    byte[] tmp = new byte[2048];

                    while (Read(tmp, 0, tmp.Length) > 0)
                    {
                    }
                    return;
                }

                csize -= inf.TotalIn;
                inputBuffer.Available += inf.input.AvailableBytes;
            }

            if ((inputBuffer.Available > csize) && (csize >= 0))
            {
                inputBuffer.Available = (int)(inputBuffer.Available - csize);
            }
            else
            {
                csize -= inputBuffer.Available;
                inputBuffer.Available = 0;
                while (csize != 0)
                {
                    long skipped = base.Skip(csize & 0xFFFFFFFFL);

                    if (skipped <= 0)
                        throw new Exception("Zip archive ends early.");

                    csize -= skipped;
                }
            }

            CompleteCloseEntry(false);
        }

        public override int ReadByte()
        {
            byte[] b = new byte[1];
            if (Read(b, 0, 1) <= 0)
                return -1;

            return b[0] & 0xff;
        }

        private int ReadingNotAvailable(byte[] destination, int offset, int count)
        {
            throw new InvalidOperationException("Unable to read from this stream");
        }

        private int ReadingNotSupported(byte[] destination, int offset, int count)
        {
            throw new Exception("The compression method for this entry is not supported");
        }

        private int InitialRead(byte[] destination, int offset, int count)
        {
            if (!CanDecompressEntry)
                throw new Exception("Library cannot extract this entry. Version required is (" + entry.Version + ")");

            inputBuffer.CryptoTransform = null;
            if ((method == (int)StiCompressionMethod.Deflated) && (inputBuffer.Available > 0))
                inputBuffer.SetInflaterInput(inf);

            internalReader = BodyRead;
            return BodyRead(destination, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Cannot be negative");
            if ((buffer.Length - offset) < count)
                throw new ArgumentException("Invalid offset/count combination");

            return internalReader(buffer, offset, count);
        }

        private int BodyRead(byte[] buffer, int offset, int count)
        {
            if (crc == null)
                throw new InvalidOperationException("Closed");
            if ((entry == null) || (count <= 0))
                return 0;
            if (offset + count > buffer.Length)
                throw new ArgumentException("Offset + count exceeds buffer size");

            bool finished = false;
            switch (method)
            {
                case (int)StiCompressionMethod.Deflated:
                    count = base.Read(buffer, offset, count);
                    if (count <= 0)
                    {
                        if (!inf.IsFinished)
                            throw new Exception("Inflater not finished!");

                        inputBuffer.Available = inf.input.AvailableBytes;

                        if ((flags & 8) == 0 && (inf.TotalIn != csize || inf.TotalOut != size))
                            throw new Exception("Size mismatch: " + csize + ";" + size + " <-> " + inf.TotalIn + ";" + inf.TotalOut);

                        inf.Reset();
                        finished = true;
                    }
                    break;

                case (int)StiCompressionMethod.Stored:
                    if ((count > csize) && (csize >= 0))
                        count = (int)csize;

                    if (count > 0)
                    {
                        count = inputBuffer.ReadClearTextBuffer(buffer, offset, count);
                        if (count > 0)
                        {
                            csize -= count;
                            size -= count;
                        }
                    }

                    if (csize == 0)
                        finished = true;
                    else
                    {
                        if (count < 0)
                            throw new Exception("EOF in stored block");
                    }

                    break;
            }

            if (count > 0)
            {
                crc.Update(buffer, offset, count);
            }

            if (finished)
            {
                CompleteCloseEntry(true);
            }

            return count;
        }

        public override void Close()
        {
            internalReader = ReadingNotAvailable;
            crc = null;
            entry = null;

            base.Close();
        }
        #endregion

        public StiZipInputStream(Stream baseInputStream)
            : base(baseInputStream, new StiInflater(true))
        {
            internalReader = ReadingNotAvailable;
        }
    }
}