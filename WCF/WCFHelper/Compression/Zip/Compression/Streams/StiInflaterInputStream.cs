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
using System.Security.Cryptography;

namespace WCFHelper.Compression
{
    internal class StiInflaterInputStream : Stream
    {
        #region Fields
        protected Stream baseInputStream;
        protected long csize;
        protected StiInflater inf;
        protected StiInflaterInputBuffer inputBuffer;
        private bool isClosed;
        public bool IsStreamOwner = true;
        #endregion

        #region Properties
        public virtual int Available
        {
            get 
            { 
                return inf.IsFinished ? 0 : 1; 
            }
        }

        public override bool CanRead
        {
            get 
            { 
                return baseInputStream.CanRead; 
            }
        }

        public override bool CanSeek
        {
            get 
            { 
                return false; 
            }
        }

        public override bool CanWrite
        {
            get 
            { 
                return false; 
            }
        }

        public override long Length
        {
            get 
            { 
                return inputBuffer.RawLength; 
            }
        }

        public override long Position
        {
            get 
            { 
                return baseInputStream.Position; 
            }
            set
            { 
            }
        }
        #endregion

        #region Methods
        public long Skip(long count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (baseInputStream.CanSeek)
            {
                baseInputStream.Seek(count, SeekOrigin.Current);
                return count;
            }

            int len = 2048;
            if (count < len)
                len = (int)count;

            byte[] tmp = new byte[len];

            return baseInputStream.Read(tmp, 0, tmp.Length);
        }

        protected void StopDecrypting()
        {
            inputBuffer.CryptoTransform = null;
        }

        protected void Fill()
        {
            inputBuffer.Fill();
            inputBuffer.SetInflaterInput(inf);
        }
        
        public override void Flush()
        {
            baseInputStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek not supported");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("InflaterInputStream SetLength not supported");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("InflaterInputStream Write not supported");
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException("InflaterInputStream WriteByte not supported");
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("InflaterInputStream BeginWrite not supported");
        }

        public override void Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                if (IsStreamOwner)
                    baseInputStream.Close();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (inf.IsNeedingDictionary)
                throw new Exception("Need a dictionary");

            int remainingBytes = count;
            while (true)
            {
                int bytesRead = inf.Inflate(buffer, offset, remainingBytes);
                offset += bytesRead;
                remainingBytes -= bytesRead;

                if (remainingBytes == 0 || inf.IsFinished) break;

                if (inf.input.IsNeedingInput)
                    Fill();
                else if (bytesRead == 0)
                    throw new Exception("Dont know what to do");
            }

            return count - remainingBytes;
        }
        #endregion

        public StiInflaterInputStream(Stream baseInputStream, StiInflater inflater, int bufferSize = 4096)
        {
            if (baseInputStream == null)
                throw new ArgumentNullException("baseInputStream");
            if (inflater == null)
                throw new ArgumentNullException("inflater");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            this.baseInputStream = baseInputStream;
            inf = inflater;

            inputBuffer = new StiInflaterInputBuffer(baseInputStream, bufferSize);
        }
    }
}