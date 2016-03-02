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
    internal class StiInflaterDynHeader
    {
        #region Constants
        const int LNUM = 0;
        const int DNUM = 1;
        const int BLNUM = 2;
        const int BLLENS = 3;
        const int LENS = 4;
        const int REPS = 5;

        static readonly int[] repMin = { 3, 3, 11 };
        static readonly int[] repBits = { 2, 3, 7 };

        static readonly int[] BL_ORDER = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
        #endregion

        #region Fields
        private byte[] blLens;
        private byte[] litdistLens;

        private StiInflaterHuffmanTree blTree;

        private int mode;
        private int lnum, dnum, blnum, num;
        private int repSymbol;
        private byte lastLen;
        private int ptr;
        #endregion

        #region Methods
        public bool Decode(StiStreamManipulator input)
        {
        decode_loop:
            for (; ; )
            {
                switch (mode)
                {
                    case LNUM:
                        lnum = input.PeekBits(5);
                        if (lnum < 0)
                        {
                            return false;
                        }
                        lnum += 257;
                        input.DropBits(5);
                        mode = DNUM;
                        goto case DNUM;
                    case DNUM:
                        dnum = input.PeekBits(5);
                        if (dnum < 0)
                        {
                            return false;
                        }
                        dnum++;
                        input.DropBits(5);
                        num = lnum + dnum;
                        litdistLens = new byte[num];
                        mode = BLNUM;
                        goto case BLNUM;
                    case BLNUM:
                        blnum = input.PeekBits(4);
                        if (blnum < 0)
                        {
                            return false;
                        }
                        blnum += 4;
                        input.DropBits(4);
                        blLens = new byte[19];
                        ptr = 0;
                        mode = BLLENS;
                        goto case BLLENS;
                    case BLLENS:
                        while (ptr < blnum)
                        {
                            int len = input.PeekBits(3);
                            if (len < 0)
                            {
                                return false;
                            }
                            input.DropBits(3);
                            blLens[BL_ORDER[ptr]] = (byte)len;
                            ptr++;
                        }
                        blTree = new StiInflaterHuffmanTree(blLens);
                        blLens = null;
                        ptr = 0;
                        mode = LENS;
                        goto case LENS;
                    case LENS:
                        {
                            int symbol;
                            while (((symbol = blTree.GetSymbol(input)) & ~15) == 0)
                            {
                                litdistLens[ptr++] = lastLen = (byte)symbol;

                                if (ptr == num)
                                    return true;
                            }

                            if (symbol < 0)
                                return false;

                            if (symbol >= 17)
                                lastLen = 0;
                            else
                            {
                                if (ptr == 0)
                                    throw new Exception();
                            }
                            repSymbol = symbol - 16;
                        }
                        mode = REPS;
                        goto case REPS;
                    case REPS:
                        {
                            int bits = repBits[repSymbol];
                            int count = input.PeekBits(bits);
                            if (count < 0)
                                return false;

                            input.DropBits(bits);
                            count += repMin[repSymbol];

                            if (ptr + count > num)
                                throw new Exception();
                            while (count-- > 0)
                            {
                                litdistLens[ptr++] = lastLen;
                            }

                            if (ptr == num)
                                return true;
                        }
                        mode = LENS;
                        goto decode_loop;
                }
            }
        }

        public StiInflaterHuffmanTree BuildLitLenTree()
        {
            byte[] litlenLens = new byte[lnum];
            Array.Copy(litdistLens, 0, litlenLens, 0, lnum);
            return new StiInflaterHuffmanTree(litlenLens);
        }

        public StiInflaterHuffmanTree BuildDistTree()
        {
            byte[] distLens = new byte[dnum];
            Array.Copy(litdistLens, lnum, distLens, 0, dnum);
            return new StiInflaterHuffmanTree(distLens);
        }
        #endregion

        public StiInflaterDynHeader()
        {
        }
    }
}