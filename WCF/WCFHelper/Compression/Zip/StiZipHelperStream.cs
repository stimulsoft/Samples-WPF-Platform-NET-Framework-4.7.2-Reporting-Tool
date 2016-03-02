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
using System.Text;

namespace WCFHelper.Compression
{
    internal class StiZipHelperStream : Stream
    {
        #region Fields
        public bool IsStreamOwner = false;
        private Stream stream;
        #endregion

        #region Base Stream Methods
        public override bool CanRead
        {
            get 
            {
                return stream.CanRead; 
            }
        }

        public override bool CanSeek
        {
            get 
            { 
                return stream.CanSeek; 
            }
        }

        public override bool CanTimeout
        {
            get 
            { 
                return stream.CanTimeout; 
            }
        }

        public override long Length
        {
            get 
            { 
                return stream.Length; 
            }
        }

        public override long Position
        {
            get 
            { 
                return stream.Position; 
            }
            set 
            { 
                stream.Position = value; 
            }
        }

        public override bool CanWrite
        {
            get 
            { 
                return stream.CanWrite; 
            }
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        override public void Close()
        {
            Stream toClose = stream;
            stream = null;

            if (IsStreamOwner && (toClose != null))
            {
                IsStreamOwner = false;
                toClose.Close();
            }
        }
        #endregion

        #region Methods
        private void WriteZip64EndOfCentralDirectory(long noOfEntries, long sizeEntries, long centralDirOffset)
        {
            long centralSignatureOffset = stream.Position;
            WriteLEInt(StiZipConstants.Zip64CentralFileHeaderSignature);
            WriteLELong(44);
            WriteLEShort(StiZipConstants.VersionMadeBy);
            WriteLEShort(StiZipConstants.VersionZip64);
            WriteLEInt(0);
            WriteLEInt(0);
            WriteLELong(noOfEntries);
            WriteLELong(noOfEntries);
            WriteLELong(sizeEntries);
            WriteLELong(centralDirOffset);

            WriteLEInt(StiZipConstants.Zip64CentralDirLocatorSignature);
            WriteLEInt(0);
            WriteLELong(centralSignatureOffset);
            WriteLEInt(1);
        }

        internal void WriteEndOfCentralDirectory(long noOfEntries, long sizeEntries, long startOfCentralDirectory, byte[] comment)
        {
            if ((noOfEntries >= 0xffff) ||
                (startOfCentralDirectory >= 0xffffffff) ||
                (sizeEntries >= 0xffffffff))
            {
                WriteZip64EndOfCentralDirectory(noOfEntries, sizeEntries, startOfCentralDirectory);
            }

            WriteLEInt(StiZipConstants.EndOfCentralDirectorySignature);
            WriteLEShort(0);
            WriteLEShort(0);


            if (noOfEntries >= 0xffff)
            {
                WriteLEUshort(0xffff);
                WriteLEUshort(0xffff);
            }
            else
            {
                WriteLEShort((short)noOfEntries);
                WriteLEShort((short)noOfEntries);
            }

            if (sizeEntries >= 0xffffffff)
            {
                WriteLEUint(0xffffffff);
            }
            else
            {
                WriteLEInt((int)sizeEntries);
            }


            if (startOfCentralDirectory >= 0xffffffff)
            {
                WriteLEUint(0xffffffff);
            }
            else
            {
                WriteLEInt((int)startOfCentralDirectory);
            }

            int commentLength = (comment != null) ? comment.Length : 0;

            if (commentLength > 0xffff)
                throw new Exception(string.Format("Comment length({0}) is too long can only be 64K", commentLength));

            WriteLEShort(commentLength);

            if (commentLength > 0)
            {
                Write(comment, 0, comment.Length);
            }
        }

        #region LE value reading/writing
        public int ReadLEShort()
        {
            int byteValue1 = stream.ReadByte();

            if (byteValue1 < 0)
            {
                throw new EndOfStreamException();
            }

            int byteValue2 = stream.ReadByte();
            if (byteValue2 < 0)
            {
                throw new EndOfStreamException();
            }

            return byteValue1 | (byteValue2 << 8);
        }

        public int ReadLEInt()
        {
            return ReadLEShort() | (ReadLEShort() << 16);
        }

        public void WriteLEShort(int value)
        {
            stream.WriteByte((byte)(value & 0xff));
            stream.WriteByte((byte)((value >> 8) & 0xff));
        }

        public void WriteLEUshort(ushort value)
        {
            stream.WriteByte((byte)(value & 0xff));
            stream.WriteByte((byte)(value >> 8));
        }

        public void WriteLEInt(int value)
        {
            WriteLEShort(value);
            WriteLEShort(value >> 16);
        }

        public void WriteLEUint(uint value)
        {
            WriteLEUshort((ushort)(value & 0xffff));
            WriteLEUshort((ushort)(value >> 16));
        }

        public void WriteLELong(long value)
        {
            WriteLEInt((int)value);
            WriteLEInt((int)(value >> 32));
        }

        public void WriteLEUlong(ulong value)
        {
            WriteLEUint((uint)(value & 0xffffffff));
            WriteLEUint((uint)(value >> 32));
        }
        #endregion
        #endregion

        public StiZipHelperStream(Stream stream)
        {
            this.stream = stream;
        }
    }
}