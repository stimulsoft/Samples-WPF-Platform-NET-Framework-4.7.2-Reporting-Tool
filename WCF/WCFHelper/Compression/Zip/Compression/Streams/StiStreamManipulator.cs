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

namespace WCFHelper.Compression
{
    internal class StiStreamManipulator
    {
        #region Fields
        private byte[] window_;
        private int windowStart_;
        private int windowEnd_;
        private uint buffer_;
        internal int AvailableBits;
        #endregion

        #region Properties
        public int AvailableBytes
        {
            get
            {
                return windowEnd_ - windowStart_ + (AvailableBits >> 3);
            }
        }

        public bool IsNeedingInput
        {
            get
            {
                return windowStart_ == windowEnd_;
            }
        }
        #endregion

        #region Methods
        public int PeekBits(int bitCount)
        {
            if (AvailableBits < bitCount)
            {
                if (windowStart_ == windowEnd_) return -1;

                buffer_ |= (uint)((window_[windowStart_++] & 0xff | (window_[windowStart_++] & 0xff) << 8) << AvailableBits);
                AvailableBits += 16;
            }

            return (int)(buffer_ & ((1 << bitCount) - 1));
        }

        public void DropBits(int bitCount)
        {
            buffer_ >>= bitCount;
            AvailableBits -= bitCount;
        }
                                
        public void SkipToByteBoundary()
        {
            buffer_ >>= (AvailableBits & 7);
            AvailableBits &= ~7;
        }        

        public int CopyBytes(byte[] output, int offset, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");
            if ((AvailableBits & 7) != 0)
                throw new InvalidOperationException("Bit buffer is not byte aligned!");

            int count = 0;
            while ((AvailableBits > 0) && (length > 0))
            {
                output[offset++] = (byte)buffer_;
                buffer_ >>= 8;
                AvailableBits -= 8;
                length--;
                count++;
            }

            if (length == 0) return count;

            int avail = windowEnd_ - windowStart_;
            if (length > avail)
                length = avail;

            Array.Copy(window_, windowStart_, output, offset, length);
            windowStart_ += length;

            if (((windowStart_ - windowEnd_) & 1) != 0)
            {
                buffer_ = (uint)(window_[windowStart_++] & 0xff);
                AvailableBits = 8;
            }

            return count + length;
        }

        public void Reset()
        {
            buffer_ = 0;
            windowStart_ = windowEnd_ = AvailableBits = 0;
        }

        public void SetInput(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Cannot be negative");
            if (windowStart_ < windowEnd_)
                throw new InvalidOperationException("Old input was not completely processed");

            int end = offset + count;

            if ((offset > end) || (end > buffer.Length))
                throw new ArgumentOutOfRangeException("count");

            if ((count & 1) != 0)
            {
                buffer_ |= (uint)((buffer[offset++] & 0xff) << AvailableBits);
                AvailableBits += 8;
            }

            window_ = buffer;
            windowStart_ = offset;
            windowEnd_ = end;
        }
        #endregion
    }
}