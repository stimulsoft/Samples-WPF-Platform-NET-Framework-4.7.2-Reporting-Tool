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
    internal class StiOutputWindow
    {
        #region Constants
        private const int WindowMask = WindowSize - 1;
        private const int WindowSize = 1 << 15;
        #endregion

        #region Fields
        private readonly byte[] window = new byte[WindowSize];
        private int windowEnd;
        private int windowFilled;
        #endregion

        #region Methods
        public void Write(int value)
        {
            if (windowFilled++ == WindowSize)
                throw new InvalidOperationException("Window full");

            window[windowEnd++] = (byte)value;
            windowEnd &= WindowMask;
        }

        private void SlowRepeat(int repStart, int length)
        {
            while (length-- > 0)
            {
                window[windowEnd++] = window[repStart++];
                windowEnd &= WindowMask;
                repStart &= WindowMask;
            }
        }

        public void Repeat(int length, int distance)
        {
            if ((windowFilled += length) > WindowSize)
                throw new InvalidOperationException("Window full");

            int repStart = (windowEnd - distance) & WindowMask;
            int border = WindowSize - length;

            if ((repStart <= border) && (windowEnd < border))
            {
                if (length <= distance)
                {
                    Array.Copy(window, repStart, window, windowEnd, length);
                    windowEnd += length;
                }
                else
                {
                    while (length-- > 0)
                    {
                        window[windowEnd++] = window[repStart++];
                    }
                }
            }
            else
            {
                SlowRepeat(repStart, length);
            }
        }

        public int CopyStored(StiStreamManipulator input, int length)
        {
            length = Math.Min(Math.Min(length, WindowSize - windowFilled), input.AvailableBytes);
            int copied;

            int tailLen = WindowSize - windowEnd;
            if (length > tailLen)
            {
                copied = input.CopyBytes(window, windowEnd, tailLen);
                if (copied == tailLen)
                    copied += input.CopyBytes(window, 0, length - tailLen);
            }
            else
            {
                copied = input.CopyBytes(window, windowEnd, length);
            }

            windowEnd = (windowEnd + copied) & WindowMask;
            windowFilled += copied;

            return copied;
        }

        public int GetFreeSpace()
        {
            return WindowSize - windowFilled;
        }

        public int GetAvailable()
        {
            return windowFilled;
        }

        public int CopyOutput(byte[] output, int offset, int len)
        {
            int copyEnd = windowEnd;
            if (len > windowFilled)
                len = windowFilled;
            else
                copyEnd = (windowEnd - windowFilled + len) & WindowMask;

            int copied = len;
            int tailLen = len - copyEnd;

            if (tailLen > 0)
            {
                Array.Copy(window, WindowSize - tailLen, output, offset, tailLen);
                offset += tailLen;
                len = copyEnd;
            }

            Array.Copy(window, copyEnd - len, output, offset, len);
            windowFilled -= copied;
            if (windowFilled < 0)
                throw new InvalidOperationException();

            return copied;
        }

        public void Reset()
        {
            windowFilled = windowEnd = 0;
        }
        #endregion
    }
}