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
using System.Collections;
using System.IO;

namespace WCFHelper.Compression
{
    internal sealed class StiZipExtraData : IDisposable
    {
        #region Fields
        private int index;
        private int readValueStart;
        private int readValueLength;

        private MemoryStream newEntry;
        private byte[] data;
        #endregion

        #region Methods
        public byte[] GetEntryData()
        {
            if (data.Length > ushort.MaxValue)
                throw new Exception("Data exceeds maximum length");

            return (byte[])data.Clone();
        }

        public bool Find(int headerID)
        {
            readValueStart = data.Length;
            readValueLength = 0;
            index = 0;

            int localLength = readValueStart;
            int localTag = headerID - 1;

            while ((localTag != headerID) && (index < data.Length - 3))
            {
                localTag = ReadShortInternal();
                localLength = ReadShortInternal();
                if (localTag != headerID)
                {
                    index += localLength;
                }
            }

            bool result = (localTag == headerID) && ((index + localLength) <= data.Length);

            if (result)
            {
                readValueStart = index;
                readValueLength = localLength;
            }

            return result;
        }
        
        public void AddEntry(int headerID, byte[] fieldData)
        {
            if ((headerID > ushort.MaxValue) || (headerID < 0))
                throw new ArgumentOutOfRangeException("headerID");

            int addLength = (fieldData == null) ? 0 : fieldData.Length;

            if (addLength > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("fieldData", "exceeds maximum length");

            int newLength = data.Length + addLength + 4;
            if (Find(headerID))
                newLength -= (ValueLength + 4);

            if (newLength > ushort.MaxValue)
                throw new Exception("Data exceeds maximum length");

            Delete(headerID);

            byte[] newData = new byte[newLength];
            data.CopyTo(newData, 0);
            int index = data.Length;
            data = newData;
            SetShort(ref index, headerID);
            SetShort(ref index, addLength);
            if (fieldData != null)
                fieldData.CopyTo(newData, index);
        }

        public void StartNewEntry()
        {
            newEntry = new MemoryStream();
        }

        public void AddNewEntry(int headerID)
        {
            byte[] newData = newEntry.ToArray();
            newEntry = null;
            AddEntry(headerID, newData);
        }
        
        public void AddLeShort(int toAdd)
        {
            unchecked
            {
                newEntry.WriteByte((byte)toAdd);
                newEntry.WriteByte((byte)(toAdd >> 8));
            }
        }

        public void AddLeInt(int toAdd)
        {
            unchecked
            {
                AddLeShort((short)toAdd);
                AddLeShort((short)(toAdd >> 16));
            }
        }

        public void AddLeLong(long toAdd)
        {
            unchecked
            {
                AddLeInt((int)(toAdd & 0xffffffff));
                AddLeInt((int)(toAdd >> 32));
            }
        }

        public bool Delete(int headerID)
        {
            bool result = false;

            if (Find(headerID))
            {
                result = true;
                int trueStart = readValueStart - 4;

                byte[] newData = new byte[data.Length - (ValueLength + 4)];
                Array.Copy(data, 0, newData, 0, trueStart);

                int trueEnd = trueStart + ValueLength + 4;
                Array.Copy(data, trueEnd, newData, trueStart, data.Length - trueEnd);
                data = newData;
            }

            return result;
        }

        public long ReadLong()
        {
            ReadCheck(8);
            return (ReadInt() & 0xffffffff) | (((long)ReadInt()) << 32);
        }

        public int ReadInt()
        {
            ReadCheck(4);

            int result = data[index] + (data[index + 1] << 8) +
                         (data[index + 2] << 16) + (data[index + 3] << 24);
            index += 4;
            return result;
        }

        public int ReadShort()
        {
            ReadCheck(2);
            int result = data[index] + (data[index + 1] << 8);
            index += 2;
            return result;
        }

        public int ReadByte()
        {
            int result = -1;
            if ((index < data.Length) && (readValueStart + readValueLength > index))
            {
                result = data[index];
                index += 1;
            }
            return result;
        }

        public void Skip(int amount)
        {
            ReadCheck(amount);
            index += amount;
        }

        private void ReadCheck(int length)
        {
            if ((readValueStart > data.Length) || (readValueStart < 4))
                throw new Exception("Find must be called before calling a Read method");
            if (index > readValueStart + readValueLength - length)
                throw new Exception("End of extra data");
        }

        private int ReadShortInternal()
        {
            if (index > data.Length - 2)
                throw new Exception("End of extra data");

            int result = data[index] + (data[index + 1] << 8);
            index += 2;
            return result;
        }

        private void SetShort(ref int index, int source)
        {
            data[index] = (byte)source;
            data[index + 1] = (byte)(source >> 8);
            index += 2;
        }

        public void Dispose()
        {
            if (newEntry != null)
                newEntry.Close();
        }
        #endregion

        #region Properties
        public int ValueLength
        {
            get 
            {
                return readValueLength; 
            }
        }

        public int CurrentReadIndex
        {
            get 
            { 
                return index; 
            }
        }

        public int UnreadCount
        {
            get
            {
                if ((readValueStart > data.Length) || (readValueStart < 4))
                    throw new Exception("Find must be called before calling a Read method");

                return readValueStart + readValueLength - index;
            }
        }
        #endregion        

        public StiZipExtraData(byte[] data)
        {
            this.data = (data == null) ? new byte[0] : data;
        }
    }
}