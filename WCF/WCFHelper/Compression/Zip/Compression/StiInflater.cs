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
    internal class StiInflater
    {
        #region Constants
        static readonly int[] CPLENS = {
                                           3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31,
                                           35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258
                                       };

        static readonly int[] CPLEXT = {
                                           0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
                                           3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
                                       };

        static readonly int[] CPDIST = {
                                           1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
                                           257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145,
                                           8193, 12289, 16385, 24577
                                       };

        static readonly int[] CPDEXT = {
                                           0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
                                           7, 7, 8, 8, 9, 9, 10, 10, 11, 11,
                                           12, 12, 13, 13
                                       };

        const int DECODE_HEADER = 0;
        const int DECODE_DICT = 1;
        const int DECODE_BLOCKS = 2;
        const int DECODE_STORED_LEN1 = 3;
        const int DECODE_STORED_LEN2 = 4;
        const int DECODE_STORED = 5;
        const int DECODE_DYN_HEADER = 6;
        const int DECODE_HUFFMAN = 7;
        const int DECODE_HUFFMAN_LENBITS = 8;
        const int DECODE_HUFFMAN_DIST = 9;
        const int DECODE_HUFFMAN_DISTBITS = 10;
        const int DECODE_CHKSUM = 11;
        const int FINISHED = 12;
        #endregion

        #region Fields
        private int mode;
        private int readAdler;
        private int neededBits;
        private int repLength;
        private int repDist;
        private int uncomprLen;
        private bool isLastBlock;
        internal long TotalOut;
        private long totalIn;
        private bool noHeader;

        internal StiStreamManipulator input;
        private StiOutputWindow outputWindow;
        private StiInflaterDynHeader dynHeader;
        private StiInflaterHuffmanTree litlenTree, distTree;
        private StiAdler32 adler;
        #endregion

        #region Methods
        public void Reset()
        {
            mode = noHeader ? DECODE_BLOCKS : DECODE_HEADER;
            totalIn = 0;
            TotalOut = 0;
            input.Reset();
            outputWindow.Reset();
            dynHeader = null;
            litlenTree = null;
            distTree = null;
            isLastBlock = false;
            adler.checksum = 1;
        }

        private bool DecodeHeader()
        {
            int header = input.PeekBits(16);
            if (header < 0)
                return false;
            input.DropBits(16);

            header = ((header << 8) | (header >> 8)) & 0xffff;
            if (header % 31 != 0)
                throw new Exception("Header checksum illegal");
            if ((header & 0x0f00) != (StiDeflater.DEFLATED << 8))
                throw new Exception("Compression Method unknown");

            if ((header & 0x0020) == 0)
            {
                mode = DECODE_BLOCKS;
            }
            else
            {
                mode = DECODE_DICT;
                neededBits = 32;
            }

            return true;
        }

        private bool DecodeDict()
        {
            while (neededBits > 0)
            {
                int dictByte = input.PeekBits(8);
                if (dictByte < 0)
                    return false;

                input.DropBits(8);
                readAdler = (readAdler << 8) | dictByte;
                neededBits -= 8;
            }

            return false;
        }

        private bool DecodeHuffman()
        {
            int free = outputWindow.GetFreeSpace();
            while (free >= 258)
            {
                int symbol;
                switch (mode)
                {
                    case DECODE_HUFFMAN:
                        while (((symbol = litlenTree.GetSymbol(input)) & ~0xff) == 0)
                        {
                            outputWindow.Write(symbol);
                            if (--free < 258)
                                return true;
                        }

                        if (symbol < 257)
                        {
                            if (symbol < 0)
                            {
                                return false;
                            }
                            else
                            {
                                distTree = null;
                                litlenTree = null;
                                mode = DECODE_BLOCKS;
                                return true;
                            }
                        }

                        try
                        {
                            repLength = CPLENS[symbol - 257];
                            neededBits = CPLEXT[symbol - 257];
                        }
                        catch
                        {
                            throw new Exception("Illegal rep length code");
                        }
                        goto case DECODE_HUFFMAN_LENBITS;

                    case DECODE_HUFFMAN_LENBITS:
                        if (neededBits > 0)
                        {
                            mode = DECODE_HUFFMAN_LENBITS;
                            int i = input.PeekBits(neededBits);
                            if (i < 0)
                                return false;

                            input.DropBits(neededBits);
                            repLength += i;
                        }
                        mode = DECODE_HUFFMAN_DIST;
                        goto case DECODE_HUFFMAN_DIST;

                    case DECODE_HUFFMAN_DIST:
                        symbol = distTree.GetSymbol(input);
                        if (symbol < 0)
                            return false;

                        try
                        {
                            repDist = CPDIST[symbol];
                            neededBits = CPDEXT[symbol];
                        }
                        catch
                        {
                            throw new Exception("Illegal rep dist code");
                        }

                        goto case DECODE_HUFFMAN_DISTBITS;

                    case DECODE_HUFFMAN_DISTBITS:
                        if (neededBits > 0)
                        {
                            mode = DECODE_HUFFMAN_DISTBITS;
                            int i = input.PeekBits(neededBits);
                            if (i < 0)
                                return false;

                            input.DropBits(neededBits);
                            repDist += i;
                        }

                        outputWindow.Repeat(repLength, repDist);
                        free -= repLength;
                        mode = DECODE_HUFFMAN;
                        break;

                    default:
                        throw new Exception("Inflater unknown mode");
                }
            }

            return true;
        }

        private bool DecodeChksum()
        {
            while (neededBits > 0)
            {
                int chkByte = input.PeekBits(8);
                if (chkByte < 0)
                    return false;

                input.DropBits(8);
                readAdler = (readAdler << 8) | chkByte;
                neededBits -= 8;
            }

            if ((int)adler.Value != readAdler)
                throw new Exception("Adler chksum doesn't match: " + (int)adler.Value + " vs. " + readAdler);

            mode = FINISHED;
            return false;
        }

        private bool Decode()
        {
            switch (mode)
            {
                case DECODE_HEADER:
                    return DecodeHeader();

                case DECODE_DICT:
                    return DecodeDict();

                case DECODE_CHKSUM:
                    return DecodeChksum();

                case DECODE_BLOCKS:
                    if (isLastBlock)
                    {
                        if (noHeader)
                        {
                            mode = FINISHED;
                            return false;
                        }
                        else
                        {
                            input.SkipToByteBoundary();
                            neededBits = 32;
                            mode = DECODE_CHKSUM;
                            return true;
                        }
                    }

                    int type = input.PeekBits(3);
                    if (type < 0)
                        return false;
                    input.DropBits(3);

                    if ((type & 1) != 0)
                        isLastBlock = true;

                    switch (type >> 1)
                    {
                        case StiDeflaterConstants.STORED_BLOCK:
                            input.SkipToByteBoundary();
                            mode = DECODE_STORED_LEN1;
                            break;

                        case StiDeflaterConstants.STATIC_TREES:
                            litlenTree = StiInflaterHuffmanTree.defLitLenTree;
                            distTree = StiInflaterHuffmanTree.defDistTree;
                            mode = DECODE_HUFFMAN;
                            break;

                        case StiDeflaterConstants.DYN_TREES:
                            dynHeader = new StiInflaterDynHeader();
                            mode = DECODE_DYN_HEADER;
                            break;

                        default:
                            throw new Exception("Unknown block type " + type);
                    }

                    return true;

                case DECODE_STORED_LEN1:
                    {
                        if ((uncomprLen = input.PeekBits(16)) < 0)
                        {
                            return false;
                        }
                        input.DropBits(16);
                        mode = DECODE_STORED_LEN2;
                    }
                    goto case DECODE_STORED_LEN2;

                case DECODE_STORED_LEN2:
                    {
                        int nlen = input.PeekBits(16);
                        if (nlen < 0)
                            return false;

                        input.DropBits(16);
                        if (nlen != (uncomprLen ^ 0xffff))
                            throw new Exception("broken uncompressed block");
                        mode = DECODE_STORED;
                    }
                    goto case DECODE_STORED;

                case DECODE_STORED:
                    {
                        int more = outputWindow.CopyStored(input, uncomprLen);
                        uncomprLen -= more;
                        if (uncomprLen == 0)
                        {
                            mode = DECODE_BLOCKS;
                            return true;
                        }

                        return !input.IsNeedingInput;
                    }

                case DECODE_DYN_HEADER:
                    if (!dynHeader.Decode(input))
                        return false;

                    litlenTree = dynHeader.BuildLitLenTree();
                    distTree = dynHeader.BuildDistTree();
                    mode = DECODE_HUFFMAN;
                    goto case DECODE_HUFFMAN;

                case DECODE_HUFFMAN:
                case DECODE_HUFFMAN_LENBITS:
                case DECODE_HUFFMAN_DIST:
                case DECODE_HUFFMAN_DISTBITS:
                    return DecodeHuffman();

                case FINISHED:
                    return false;

                default:
                    throw new Exception("Inflater.Decode unknown mode");
            }
        }
        
        public void SetInput(byte[] buffer, int index, int count)
        {
            input.SetInput(buffer, index, count);
            totalIn += (long)count;
        }
        
        public int Inflate(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "count cannot be negative");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "offset cannot be negative");
            if (offset + count > buffer.Length)
                throw new ArgumentException("count exceeds buffer bounds");

            if (count == 0)
            {
                if (!IsFinished)
                    Decode();
                return 0;
            }

            int bytesCopied = 0;

            do
            {
                if (mode != DECODE_CHKSUM)
                {
                    int more = outputWindow.CopyOutput(buffer, offset, count);
                    if (more > 0)
                    {
                        adler.Update(buffer, offset, more);
                        offset += more;
                        bytesCopied += more;
                        TotalOut += (long)more;
                        count -= more;

                        if (count == 0)
                            return bytesCopied;
                    }
                }
            } while (Decode() || ((outputWindow.GetAvailable() > 0) && (mode != DECODE_CHKSUM)));

            return bytesCopied;
        }
        #endregion

        #region Properties
        public bool IsNeedingDictionary
        {
            get
            {
                return mode == DECODE_DICT && neededBits == 0;
            }
        }

        public bool IsFinished
        {
            get
            {
                return mode == FINISHED && outputWindow.GetAvailable() == 0;
            }
        }

        public long TotalIn
        {
            get
            {
                return totalIn - (long)input.AvailableBytes;
            }
        }
        #endregion

        public StiInflater(bool noHeader)
        {
            this.noHeader = noHeader;
            this.adler = new StiAdler32();
            input = new StiStreamManipulator();
            outputWindow = new StiOutputWindow();
            mode = noHeader ? DECODE_BLOCKS : DECODE_HEADER;
        }
    }
}