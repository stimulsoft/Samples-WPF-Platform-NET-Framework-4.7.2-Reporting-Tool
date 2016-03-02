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
    internal class StiInflaterInputBuffer
    {
        #region Fields
        private readonly Stream inputStream;
        private byte[] internalClearText;
        private int clearTextLength;
        private byte[] clearText;
        internal int RawLength;
        private readonly byte[] rawData;
        public int Available;
        #endregion

        #region Properties
        private ICryptoTransform cryptoTransform;
        public ICryptoTransform CryptoTransform
        {
            set
            {
                cryptoTransform = value;
                if (cryptoTransform != null)
                {
                    if (rawData == clearText)
                    {
                        if (internalClearText == null)
                            internalClearText = new byte[4096];

                        clearText = internalClearText;
                    }

                    clearTextLength = RawLength;

                    if (Available > 0)
                        cryptoTransform.TransformBlock(rawData, RawLength - Available, Available, clearText, RawLength - Available);
                }
                else
                {
                    clearText = rawData;
                    clearTextLength = RawLength;
                }
            }
        }
        #endregion

        #region Methods
        public void SetInflaterInput(StiInflater inflater)
        {
            if (Available > 0)
            {
                inflater.SetInput(clearText, clearTextLength - Available, Available);
                Available = 0;
            }
        }

        public void Fill()
        {
            RawLength = 0;
            int toRead = rawData.Length;

            while (toRead > 0)
            {
                int count = inputStream.Read(rawData, RawLength, toRead);
                if (count <= 0)
                {
                    if (RawLength == 0)
                        throw new Exception("Unexpected EOF");
                    break;
                }

                RawLength += count;
                toRead -= count;
            }

            clearTextLength = cryptoTransform != null ? cryptoTransform.TransformBlock(rawData, 0, RawLength, clearText, 0) : RawLength;
            Available = clearTextLength;
        }

        public int ReadRawBuffer(byte[] outBuffer, int offset, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            int currentOffset = offset;
            int currentLength = length;

            while (currentLength > 0)
            {
                if (Available <= 0)
                {
                    Fill();
                    if (Available <= 0)
                        return 0;
                }

                int toCopy = Math.Min(currentLength, Available);
                Array.Copy(rawData, RawLength - Available, outBuffer, currentOffset, toCopy);
                currentOffset += toCopy;
                currentLength -= toCopy;
                Available -= toCopy;
            }

            return length;
        }

        public int ReadClearTextBuffer(byte[] outBuffer, int offset, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            int currentOffset = offset;
            int currentLength = length;

            while (currentLength > 0)
            {
                if (Available <= 0)
                {
                    Fill();
                    if (Available <= 0)
                        return 0;
                }

                int toCopy = Math.Min(currentLength, Available);
                Array.Copy(clearText, clearTextLength - Available, outBuffer, currentOffset, toCopy);
                currentOffset += toCopy;
                currentLength -= toCopy;
                Available -= toCopy;
            }

            return length;
        }

        public int ReadLeByte()
        {
            if (Available <= 0)
            {
                Fill();

                if (Available <= 0)
                    throw new Exception("EOF in header");
            }

            byte result = (byte)(rawData[RawLength - Available] & 0xff);
            Available -= 1;

            return result;
        }

        public int ReadLeShort()
        {
            return ReadLeByte() | (ReadLeByte() << 8);
        }

        public int ReadLeInt()
        {
            return ReadLeShort() | (ReadLeShort() << 16);
        }

        public long ReadLeLong()
        {
            return (uint)ReadLeInt() | ((long)ReadLeInt() << 32);
        }
        #endregion

        public StiInflaterInputBuffer(Stream stream, int bufferSize)
        {
            inputStream = stream;

            if (bufferSize < 1024)
                bufferSize = 1024;

            rawData = new byte[bufferSize];
            clearText = rawData;
        }
    }
}