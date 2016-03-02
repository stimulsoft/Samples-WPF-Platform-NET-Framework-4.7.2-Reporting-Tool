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
    internal class StiDeflaterOutputStream : Stream
    {
        #region Fields
        private ICryptoTransform cryptoTransform;
        private readonly byte[] buffer;
        protected Stream baseOutputStream;
        internal StiDeflater deflater;
        private bool isClosed;

        public bool IsStreamOwner = true;
        #endregion

        #region Properties
        public bool CanPatchEntries
        {
            get 
            {
                return baseOutputStream.CanSeek; 
            }
        }

        public override bool CanRead
        {
            get
            {
                return false;
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
                return baseOutputStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return baseOutputStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return baseOutputStream.Position;
            }
            set
            {
                
            }
        }
        #endregion

        #region Methods
        public virtual void Finish()
        {
            deflater.Finish();
            while (!deflater.IsFinished)
            {
                int len = deflater.Deflate(buffer, 0, buffer.Length);
                if (len <= 0) break;

                if (cryptoTransform != null)
                    EncryptBlock(buffer, 0, len);

                baseOutputStream.Write(buffer, 0, len);
            }

            if (!deflater.IsFinished)
                throw new Exception("Can't deflate all input?");

            baseOutputStream.Flush();

            if (cryptoTransform == null)
                return;

            cryptoTransform.Dispose();
            cryptoTransform = null;
        }

        protected void EncryptBlock(byte[] buffer, int offset, int length)
        {
            cryptoTransform.TransformBlock(buffer, 0, length, buffer, 0);
        }

        protected void Deflate()
        {
            while (!deflater.IsNeedingInput)
            {
                int deflateCount = deflater.Deflate(buffer, 0, buffer.Length);

                if (deflateCount <= 0)
                    break;

                if (cryptoTransform != null)
                    EncryptBlock(buffer, 0, deflateCount);

                baseOutputStream.Write(buffer, 0, deflateCount);
            }

            if (!deflater.IsNeedingInput)
                throw new Exception("DeflaterOutputStream can't deflate all input?");
        }
 
       
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("DeflaterOutputStream Seek not supported");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("DeflaterOutputStream SetLength not supported");
        }

        public override int ReadByte()
        {
            throw new NotSupportedException("DeflaterOutputStream ReadByte not supported");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("DeflaterOutputStream Read not supported");
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
                                               object state)
        {
            throw new NotSupportedException("DeflaterOutputStream BeginRead not currently supported");
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
                                                object state)
        {
            throw new NotSupportedException("BeginWrite is not supported");
        }

        public override void Flush()
        {
            deflater.Flush();
            Deflate();
            baseOutputStream.Flush();
        }

        public override void Close()
        {
            if (!isClosed)
            {
                isClosed = true;

                try
                {
                    Finish();
                    if (cryptoTransform != null)
                    {
                        cryptoTransform.Dispose();
                        cryptoTransform = null;
                    }
                }
                finally
                {
                    if (IsStreamOwner)
                        baseOutputStream.Close();
                }
            }
        }

        public override void WriteByte(byte value)
        {
            var b = new byte[1];
            b[0] = value;
            Write(b, 0, 1);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            deflater.SetInput(buffer, offset, count);
            Deflate();
        }
        #endregion

        internal StiDeflaterOutputStream(Stream baseOutputStream, StiDeflater deflater)
            : this(baseOutputStream, deflater, 512)
        {
        }

        internal StiDeflaterOutputStream(Stream baseOutputStream, StiDeflater deflater, int bufferSize)
        {
            if (baseOutputStream == null)
                throw new ArgumentNullException("baseOutputStream");
            if (baseOutputStream.CanWrite == false)
                throw new ArgumentException("Must support writing", "baseOutputStream");
            if (deflater == null)
                throw new ArgumentNullException("deflater");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            this.baseOutputStream = baseOutputStream;
            this.buffer = new byte[bufferSize];
            this.deflater = deflater;
        }
    }
}