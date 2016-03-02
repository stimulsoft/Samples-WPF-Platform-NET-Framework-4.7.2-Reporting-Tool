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
    internal class StiDeflaterHuffman
    {
        #region class StiTree
        private class StiTree
        {
            #region Fields
            public readonly short[] freqs;
            public byte[] length;
            public readonly int minNumCodes;
            public int numCodes;
            private short[] codes;
            private readonly int[] bl_counts;
            private readonly int maxLength;
            private readonly StiDeflaterHuffman dh;
            #endregion

            #region Methods
            public void Reset()
            {
                int index = -1;
                while(++index < freqs.Length)
                {
                    freqs[index] = 0;
                }

                codes = null;
                length = null;
            }

            public void WriteSymbol(int code)
            {
                dh.pending.WriteBits(codes[code] & 0xffff, length[code]);
            }

            public void SetStaticCodes(short[] staticCodes, byte[] staticLengths)
            {
                codes = staticCodes;
                length = staticLengths;
            }

            public void BuildCodes()
            {
                int[] nextCode = new int[maxLength];
                int code = 0;
                codes = new short[freqs.Length];
                
                int bits = -1;
                while(++bits < maxLength)
                {
                    nextCode[bits] = code;
                    code += bl_counts[bits] << (15 - bits);
                }

                int i = -1;
                while(++i < numCodes)
                {
                    bits = length[i];
                    if (bits > 0)
                    {
                        codes[i] = BitReverse(nextCode[bits - 1]);
                        nextCode[bits - 1] += 1 << (16 - bits);
                    }
                }
            }

            public void BuildTree()
            {
                int numSymbols = freqs.Length;

                int[] heap = new int[numSymbols];
                int heapLen = 0;
                int maxCode = 0;

                int n = -1;
                while(++n < numSymbols)
                {
                    int freq = freqs[n];
                    if (freq == 0) continue;

                    int pos = heapLen++;
                    int ppos;
                    while (pos > 0 && freqs[heap[ppos = (pos - 1) / 2]] > freq)
                    {
                        heap[pos] = heap[ppos];
                        pos = ppos;
                    }
                    heap[pos] = n;
                    maxCode = n;
                }

                while (heapLen < 2)
                {
                    int node = maxCode < 2 ? ++maxCode : 0;
                    heap[heapLen++] = node;
                }

                numCodes = Math.Max(maxCode + 1, minNumCodes);

                int numLeafs = heapLen;
                int[] childs = new int[4 * heapLen - 2];
                int[] values = new int[2 * heapLen - 1];
                int numNodes = numLeafs;

                int i = -1;
                while(++i < heapLen)
                {
                    int node = heap[i];
                    childs[2 * i] = node;
                    childs[2 * i + 1] = -1;
                    values[i] = freqs[node] << 8;
                    heap[i] = i;
                }

                do
                {
                    int first = heap[0];
                    int last = heap[--heapLen];

                    int ppos = 0;
                    int path = 1;

                    while (path < heapLen)
                    {
                        if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
                            path++;

                        heap[ppos] = heap[path];
                        ppos = path;
                        path = path * 2 + 1;
                    }

                    int lastVal = values[last];
                    while ((path = ppos) > 0 && values[heap[ppos = (path - 1) / 2]] > lastVal)
                    {
                        heap[path] = heap[ppos];
                    }
                    heap[path] = last;

                    int second = heap[0];

                    last = numNodes++;
                    childs[2 * last] = first;
                    childs[2 * last + 1] = second;
                    int mindepth = Math.Min(values[first] & 0xff, values[second] & 0xff);
                    values[last] = lastVal = values[first] + values[second] - mindepth + 1;

                    ppos = 0;
                    path = 1;

                    while (path < heapLen)
                    {
                        if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
                            path++;

                        heap[ppos] = heap[path];
                        ppos = path;
                        path = ppos * 2 + 1;
                    }

                    while ((path = ppos) > 0 && values[heap[ppos = (path - 1) / 2]] > lastVal)
                    {
                        heap[path] = heap[ppos];
                    }
                    heap[path] = last;
                } while (heapLen > 1);

                if (heap[0] != childs.Length / 2 - 1)
                    throw new Exception("Heap invariant violated");

                BuildLength(childs);
            }

            public int GetEncodedLength()
            {
                int len = 0;
                int i = -1;
                while(++i < freqs.Length)
                {
                    len += freqs[i] * length[i];
                }

                return len;
            }

            public void CalcBLFreq(StiTree blTree)
            {
                int curlen = -1;
                int i = 0;

                while (i < numCodes)
                {
                    int count = 1;
                    int nextlen = length[i];
                    int max_count;
                    int min_count;
                    if (nextlen == 0)
                    {
                        max_count = 138;
                        min_count = 3;
                    }
                    else
                    {
                        max_count = 6;
                        min_count = 3;
                        if (curlen != nextlen)
                        {
                            blTree.freqs[nextlen]++;
                            count = 0;
                        }
                    }
                    curlen = nextlen;
                    i++;

                    while (i < numCodes && curlen == length[i])
                    {
                        i++;
                        if (++count >= max_count)
                            break;
                    }

                    if (count < min_count)
                    {
                        blTree.freqs[curlen] += (short)count;
                    }
                    else if (curlen != 0)
                    {
                        blTree.freqs[REP_3_6]++;
                    }
                    else if (count <= 10)
                    {
                        blTree.freqs[REP_3_10]++;
                    }
                    else
                    {
                        blTree.freqs[REP_11_138]++;
                    }
                }
            }

            public void WriteTree(StiTree blTree)
            {
                int curlen = -1;
                int i = 0;
                while (i < numCodes)
                {
                    int count = 1;
                    int nextlen = length[i];
                    int max_count;
                    int min_count;
                    if (nextlen == 0)
                    {
                        max_count = 138;
                        min_count = 3;
                    }
                    else
                    {
                        max_count = 6;
                        min_count = 3;
                        if (curlen != nextlen)
                        {
                            blTree.WriteSymbol(nextlen);
                            count = 0;
                        }
                    }
                    curlen = nextlen;
                    i++;

                    while (i < numCodes && curlen == length[i])
                    {
                        i++;
                        if (++count >= max_count) break;
                    }

                    if (count < min_count)
                    {
                        while (count-- > 0)
                            blTree.WriteSymbol(curlen);
                    }
                    else if (curlen != 0)
                    {
                        blTree.WriteSymbol(REP_3_6);
                        dh.pending.WriteBits(count - 3, 2);
                    }
                    else if (count <= 10)
                    {
                        blTree.WriteSymbol(REP_3_10);
                        dh.pending.WriteBits(count - 3, 3);
                    }
                    else
                    {
                        blTree.WriteSymbol(REP_11_138);
                        dh.pending.WriteBits(count - 11, 7);
                    }
                }
            }

            private void BuildLength(int[] childs)
            {
                length = new byte[freqs.Length];
                int numNodes = childs.Length / 2;
                int numLeafs = (numNodes + 1) / 2;
                int overflow = 0;

                int i = -1;
                while(++i < maxLength)
                {
                    bl_counts[i] = 0;
                }

                int[] lengths = new int[numNodes];
                lengths[numNodes - 1] = 0;

                for (i = numNodes - 1; i >= 0; i--)
                {
                    if (childs[2 * i + 1] != -1)
                    {
                        int bitLength = lengths[i] + 1;
                        if (bitLength > maxLength)
                        {
                            bitLength = maxLength;
                            overflow++;
                        }
                        lengths[childs[2 * i]] = lengths[childs[2 * i + 1]] = bitLength;
                    }
                    else
                    {
                        int bitLength = lengths[i];
                        bl_counts[bitLength - 1]++;
                        length[childs[2 * i]] = (byte)lengths[i];
                    }
                }


                if (overflow == 0) return;

                int incrBitLen = maxLength - 1;
                do
                {
                    while (bl_counts[--incrBitLen] == 0)
                    {

                    }

                    do
                    {
                        bl_counts[incrBitLen]--;
                        bl_counts[++incrBitLen]++;
                        overflow -= 1 << (maxLength - 1 - incrBitLen);
                    } while (overflow > 0 && incrBitLen < maxLength - 1);
                } while (overflow > 0);

                bl_counts[maxLength - 1] += overflow;
                bl_counts[maxLength - 2] -= overflow;

                int nodePtr = 2 * numLeafs;
                for (int bits = maxLength; bits != 0; bits--)
                {
                    int n = bl_counts[bits - 1];
                    while (n > 0)
                    {
                        int childPtr = 2 * childs[nodePtr++];
                        if (childs[childPtr + 1] == -1)
                        {
                            length[childs[childPtr]] = (byte)bits;
                            n--;
                        }
                    }
                }
            }
            #endregion

            public StiTree(StiDeflaterHuffman dh, int elems, int minCodes, int maxLength)
            {
                this.dh = dh;
                minNumCodes = minCodes;
                this.maxLength = maxLength;
                freqs = new short[elems];
                bl_counts = new int[maxLength];
            }
        }
        #endregion

        #region Consts
        private const int BUFSIZE = 1 << (StiDeflaterConstants.DEFAULT_MEM_LEVEL + 6);
        private const int LITERAL_NUM = 286;
        private const int DIST_NUM = 30;
        private const int BITLEN_NUM = 19;
        private const int REP_3_6 = 16;
        private const int REP_3_10 = 17;
        private const int REP_11_138 = 18;
        private const int EOF_SYMBOL = 256;
        #endregion

        #region Fields
        public StiDeflaterPending pending;
        private readonly StiTree literalTree;
        private readonly StiTree distTree;
        private readonly StiTree blTree;
        private readonly short[] d_buf;
        private readonly byte[] l_buf;
        private int last_lit;
        private int extra_bits;

        private static readonly int[] BL_ORDER = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };
        private static readonly byte[] bit4Reverse = { 0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15 };
        private static readonly short[] staticLCodes;
        private static readonly byte[] staticLLength;
        private static readonly short[] staticDCodes;
        private static readonly byte[] staticDLength;
        #endregion               
        
        #region Methods
        public void Reset()
        {
            last_lit = 0;
            extra_bits = 0;
            literalTree.Reset();
            distTree.Reset();
            blTree.Reset();
        }

        public void SendAllTrees(int blTreeCodes)
        {
            blTree.BuildCodes();
            literalTree.BuildCodes();
            distTree.BuildCodes();
            pending.WriteBits(literalTree.numCodes - 257, 5);
            pending.WriteBits(distTree.numCodes - 1, 5);
            pending.WriteBits(blTreeCodes - 4, 4);

            int rank = -1;
            while(++rank < blTreeCodes)
            {
                pending.WriteBits(blTree.length[BL_ORDER[rank]], 3);
            }

            literalTree.WriteTree(blTree);
            distTree.WriteTree(blTree);
        }

        public void CompressBlock()
        {
            int i = -1;
            while(++i < last_lit)
            {
                int litlen = l_buf[i] & 0xff;
                int dist = d_buf[i];
                if (dist-- != 0)
                {
                    int lc = Lcode(litlen);
                    literalTree.WriteSymbol(lc);
                    int bits = (lc - 261) / 4;

                    if (bits > 0 && bits <= 5)
                        pending.WriteBits(litlen & ((1 << bits) - 1), bits);

                    int dc = Dcode(dist);
                    distTree.WriteSymbol(dc);
                    bits = dc / 2 - 1;

                    if (bits > 0)
                        pending.WriteBits(dist & ((1 << bits) - 1), bits);
                }
                else
                {
                    literalTree.WriteSymbol(litlen);
                }
            }

            literalTree.WriteSymbol(EOF_SYMBOL);
        }

        public void FlushStoredBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
        {
            pending.WriteBits((StiDeflaterConstants.STORED_BLOCK << 1) + (lastBlock ? 1 : 0), 3);
            pending.AlignToByte();
            pending.WriteShort(storedLength);
            pending.WriteShort(~storedLength);
            pending.WriteBlock(stored, storedOffset, storedLength);
            Reset();
        }

        public void FlushBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
        {
            literalTree.freqs[EOF_SYMBOL]++;

            literalTree.BuildTree();
            distTree.BuildTree();

            literalTree.CalcBLFreq(blTree);
            distTree.CalcBLFreq(blTree);

            blTree.BuildTree();

            int blTreeCodes = 4;
            int i = 19;
            while(--i > blTreeCodes)
            {
                if (blTree.length[BL_ORDER[i]] > 0)
                    blTreeCodes = i + 1;
            }

            int opt_len = 14 + blTreeCodes * 3 + blTree.GetEncodedLength() + literalTree.GetEncodedLength() + distTree.GetEncodedLength() + extra_bits;

            int static_len = extra_bits;

            i = -1;
            while(++i < LITERAL_NUM)
            {
                static_len += literalTree.freqs[i] * staticLLength[i];
            }

            i = -1;
            while(++i < DIST_NUM)
            {
                static_len += distTree.freqs[i] * staticDLength[i];
            }

            if (opt_len >= static_len)
                opt_len = static_len;

            if (storedOffset >= 0 && storedLength + 4 < opt_len >> 3)
            {
                FlushStoredBlock(stored, storedOffset, storedLength, lastBlock);
            }
            else if (opt_len == static_len)
            {

                pending.WriteBits((StiDeflaterConstants.STATIC_TREES << 1) + (lastBlock ? 1 : 0), 3);
                literalTree.SetStaticCodes(staticLCodes, staticLLength);
                distTree.SetStaticCodes(staticDCodes, staticDLength);
                CompressBlock();
                Reset();
            }
            else
            {
                pending.WriteBits((StiDeflaterConstants.DYN_TREES << 1) + (lastBlock ? 1 : 0), 3);
                SendAllTrees(blTreeCodes);
                CompressBlock();
                Reset();
            }
        }

        public bool IsFull()
        {
            return last_lit >= BUFSIZE;
        }

        public bool TallyLit(int literal)
        {
            d_buf[last_lit] = 0;
            l_buf[last_lit++] = (byte)literal;
            literalTree.freqs[literal]++;
            return IsFull();
        }

        public bool TallyDist(int distance, int length)
        {
            d_buf[last_lit] = (short)distance;
            l_buf[last_lit++] = (byte)(length - 3);

            int lc = Lcode(length - 3);
            literalTree.freqs[lc]++;
            if (lc >= 265 && lc < 285)
                extra_bits += (lc - 261) / 4;

            int dc = Dcode(distance - 1);
            distTree.freqs[dc]++;
            if (dc >= 4)
                extra_bits += dc / 2 - 1;

            return IsFull();
        }

        public static short BitReverse(int toReverse)
        {
            return (short)(bit4Reverse[toReverse & 0xF] << 12 |
                            bit4Reverse[(toReverse >> 4) & 0xF] << 8 |
                            bit4Reverse[(toReverse >> 8) & 0xF] << 4 |
                            bit4Reverse[toReverse >> 12]);
        }

        private static int Lcode(int length)
        {
            if (length == 255)
                return 285;

            int code = 257;
            while (length >= 8)
            {
                code += 4;
                length >>= 1;
            }

            return code + length;
        }

        private static int Dcode(int distance)
        {
            int code = 0;
            while (distance >= 4)
            {
                code += 2;
                distance >>= 1;
            }
            return code + distance;
        }
        #endregion

        static StiDeflaterHuffman()
        {
            staticLCodes = new short[LITERAL_NUM];
            staticLLength = new byte[LITERAL_NUM];

            int i = 0;
            while (i < 144)
            {
                staticLCodes[i] = BitReverse((0x030 + i) << 8);
                staticLLength[i++] = 8;
            }

            while (i < 256)
            {
                staticLCodes[i] = BitReverse((0x190 - 144 + i) << 7);
                staticLLength[i++] = 9;
            }

            while (i < 280)
            {
                staticLCodes[i] = BitReverse((0x000 - 256 + i) << 9);
                staticLLength[i++] = 7;
            }

            while (i < LITERAL_NUM)
            {
                staticLCodes[i] = BitReverse((0x0c0 - 280 + i) << 8);
                staticLLength[i++] = 8;
            }

            staticDCodes = new short[DIST_NUM];
            staticDLength = new byte[DIST_NUM];

            i = -1;
            while(++i < DIST_NUM)
            {
                staticDCodes[i] = BitReverse(i << 11);
                staticDLength[i] = 5;
            }
        }

        public StiDeflaterHuffman(StiDeflaterPending pending)
        {
            this.pending = pending;

            literalTree = new StiTree(this, LITERAL_NUM, 257, 15);
            distTree = new StiTree(this, DIST_NUM, 1, 15);
            blTree = new StiTree(this, BITLEN_NUM, 4, 7);

            d_buf = new short[BUFSIZE];
            l_buf = new byte[BUFSIZE];
        }
    }
}