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
    internal class StiInflaterHuffmanTree
    {
        #region Fields
        private short[] tree;
        public static StiInflaterHuffmanTree defLitLenTree;
        public static StiInflaterHuffmanTree defDistTree;
        #endregion

        #region Methods
        private void BuildTree(byte[] codeLengths)
        {
            int[] blCount = new int[16];
            int[] nextCode = new int[16];

            int bits = 0;
            int i = -1;
            while (++i < codeLengths.Length)
            {
                bits = codeLengths[i];
                if (bits > 0)
                    blCount[bits]++;
            }

            int code = 0;
            int treeSize = 512;
            bits = 0;
            while(++bits <= 15)
            {
                nextCode[bits] = code;
                code += blCount[bits] << (16 - bits);
                if (bits >= 10)
                {
                    int start = nextCode[bits] & 0x1ff80;
                    int end = code & 0x1ff80;
                    treeSize += (end - start) >> (16 - bits);
                }
            }

            tree = new short[treeSize];
            int treePtr = 512;
            for (bits = 15; bits >= 10; bits--)
            {
                int end = code & 0x1ff80;
                code -= blCount[bits] << (16 - bits);
                int start = code & 0x1ff80;
                for (i = start; i < end; i += 1 << 7)
                {
                    tree[StiDeflaterHuffman.BitReverse(i)] = (short)((-treePtr << 4) | bits);
                    treePtr += 1 << (bits - 9);
                }
            }

            i = -1;
            while(++i < codeLengths.Length)
            {
                bits = codeLengths[i];
                if (bits == 0) continue;

                code = nextCode[bits];
                int revcode = StiDeflaterHuffman.BitReverse(code);
                if (bits <= 9)
                {
                    do
                    {
                        tree[revcode] = (short)((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < 512);
                }
                else
                {
                    int subTree = tree[revcode & 511];
                    int treeLen = 1 << (subTree & 15);
                    subTree = -(subTree >> 4);
                    do
                    {
                        tree[subTree | (revcode >> 9)] = (short)((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < treeLen);
                }
                nextCode[bits] = code + (1 << (16 - bits));
            }
        }

        public int GetSymbol(StiStreamManipulator input)
        {
            int lookahead, symbol;
            if ((lookahead = input.PeekBits(9)) >= 0)
            {
                if ((symbol = tree[lookahead]) >= 0)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                int subtree = -(symbol >> 4);
                int bitlen = symbol & 15;
                if ((lookahead = input.PeekBits(bitlen)) >= 0)
                {
                    symbol = tree[subtree | (lookahead >> 9)];
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    int bits = input.AvailableBits;
                    lookahead = input.PeekBits(bits);
                    symbol = tree[subtree | (lookahead >> 9)];
                    if ((symbol & 15) <= bits)
                    {
                        input.DropBits(symbol & 15);
                        return symbol >> 4;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            else
            {
                int bits = input.AvailableBits;
                lookahead = input.PeekBits(bits);
                symbol = tree[lookahead];
                if (symbol >= 0 && (symbol & 15) <= bits)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    return -1;
                }
            }
        }
        #endregion

        static StiInflaterHuffmanTree()
        {
            try
            {
                byte[] codeLengths = new byte[288];
                int i = 0;
                while (i < 144)
                {
                    codeLengths[i++] = 8;
                }
                while (i < 256)
                {
                    codeLengths[i++] = 9;
                }
                while (i < 280)
                {
                    codeLengths[i++] = 7;
                }
                while (i < 288)
                {
                    codeLengths[i++] = 8;
                }
                defLitLenTree = new StiInflaterHuffmanTree(codeLengths);

                codeLengths = new byte[32];
                i = 0;
                while (i < 32)
                {
                    codeLengths[i++] = 5;
                }
                defDistTree = new StiInflaterHuffmanTree(codeLengths);
            }
            catch
            {
                throw new Exception("InflaterHuffmanTree: static tree length illegal");
            }
        }

        public StiInflaterHuffmanTree(byte[] codeLengths)
        {
            BuildTree(codeLengths);
        }
    }
}