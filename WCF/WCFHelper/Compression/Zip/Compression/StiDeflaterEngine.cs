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
    internal class StiDeflaterEngine : StiDeflaterConstants
    {
        #region Constants
        private const int TooFar = 4096;
        #endregion

        #region Fields
        private int ins_h;
        private readonly short[] head;
        private readonly short[] prev;

        private int matchStart;
        private int matchLen;
        private bool prevAvailable;
        private int blockStart;
        private int strstart;
        private int lookahead;
        private readonly byte[] window;

        public StiDeflateStrategy Strategy;
        private int max_chain, max_lazy, niceLength, goodLength;
        private int compressionFunction;
        private byte[] inputBuf;
        private int inputOff;
        private int inputEnd;
        internal int TotalIn;

        private readonly StiDeflaterPending pending;
        private readonly StiDeflaterHuffman huffman;
        private readonly StiAdler32 adler;
        #endregion

        #region Properties
        public int Adler
        {
            get
            {
                return unchecked((int)adler.Value);
            }
        }
        #endregion        

        #region Methods
        public bool Deflate(bool flush, bool finish)
        {
            bool progress;
            do
            {
                FillWindow();
                bool canFlush = flush && (inputOff == inputEnd);
                switch (compressionFunction)
                {
                    case DEFLATE_STORED:
                        progress = DeflateStored(canFlush, finish);
                        break;

                    case DEFLATE_FAST:
                        progress = DeflateFast(canFlush, finish);
                        break;

                    case DEFLATE_SLOW:
                        progress = DeflateSlow(canFlush, finish);
                        break;

                    default:
                        throw new InvalidOperationException("unknown compressionFunction");
                }
            } while (pending.IsFlushed && progress);

            return progress;
        }

        public void SetInput(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (inputOff < inputEnd)
                throw new InvalidOperationException("Old input was not completely processed");

            int end = offset + count;

            if ((offset > end) || (end > buffer.Length))
                throw new ArgumentOutOfRangeException("count");

            inputBuf = buffer;
            inputOff = offset;
            inputEnd = end;
        }

        public bool NeedsInput()
        {
            return (inputEnd == inputOff);
        }

        public void Reset()
        {
            huffman.Reset();
            adler.checksum = 1;
            blockStart = strstart = 1;
            lookahead = 0;
            TotalIn = 0;
            prevAvailable = false;
            matchLen = MIN_MATCH - 1;

            int i = -1;
            while(++i < HASH_SIZE)
            {
                head[i] = 0;
            }

            i = -1;
            while(++i < WSIZE)
            {
                prev[i] = 0;
            }
        }

        public void ResetAdler()
        {
            adler.checksum = 1;
        }

        public void SetLevel(int level)
        {
            if ((level < 0) || (level > 9))
                throw new ArgumentOutOfRangeException("level");

            goodLength = GOOD_LENGTH[level];
            max_lazy = MAX_LAZY[level];
            niceLength = NICE_LENGTH[level];
            max_chain = MAX_CHAIN[level];

            if (COMPR_FUNC[level] == compressionFunction)
                return;

            switch (compressionFunction)
            {
                case DEFLATE_STORED:
                    if (strstart > blockStart)
                    {
                        huffman.FlushStoredBlock(window, blockStart, strstart - blockStart, false);
                        blockStart = strstart;
                    }
                    UpdateHash();
                    break;

                case DEFLATE_FAST:
                    if (strstart > blockStart)
                    {
                        huffman.FlushBlock(window, blockStart, strstart - blockStart, false);
                        blockStart = strstart;
                    }
                    break;

                case DEFLATE_SLOW:
                    if (prevAvailable)
                        huffman.TallyLit(window[strstart - 1] & 0xff);

                    if (strstart > blockStart)
                    {
                        huffman.FlushBlock(window, blockStart, strstart - blockStart, false);
                        blockStart = strstart;
                    }
                    prevAvailable = false;
                    matchLen = MIN_MATCH - 1;
                    break;
            }

            compressionFunction = COMPR_FUNC[level];
        }

        public void FillWindow()
        {
            if (strstart >= WSIZE + MAX_DIST)
                SlideWindow();

            while (lookahead < MIN_LOOKAHEAD && inputOff < inputEnd)
            {
                int more = 2 * WSIZE - lookahead - strstart;
                if (more > inputEnd - inputOff)
                    more = inputEnd - inputOff;

                Array.Copy(inputBuf, inputOff, window, strstart + lookahead, more);
                adler.Update(inputBuf, inputOff, more);

                inputOff += more;
                TotalIn += more;
                lookahead += more;
            }

            if (lookahead >= MIN_MATCH)
                UpdateHash();
        }

        private void UpdateHash()
        {
            ins_h = (window[strstart] << HASH_SHIFT) ^ window[strstart + 1];
        }

        private int InsertString()
        {
            short match;
            int hash = ((ins_h << HASH_SHIFT) ^ window[strstart + (MIN_MATCH - 1)]) & HASH_MASK;
            prev[strstart & WMASK] = match = head[hash];
            head[hash] = unchecked((short)strstart);
            ins_h = hash;

            return match & 0xffff;
        }

        private void SlideWindow()
        {
            Array.Copy(window, WSIZE, window, 0, WSIZE);
            matchStart -= WSIZE;
            strstart -= WSIZE;
            blockStart -= WSIZE;

            int i = -1;
            while(++i < HASH_SIZE)
            {
                int m = head[i] & 0xffff;
                head[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
            }

            i = -1;
            while(++i < WSIZE)
            {
                int m = prev[i] & 0xffff;
                prev[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
            }
        }

        private bool FindLongestMatch(int curMatch)
        {
            int chainLength = max_chain;
            int niceLength = this.niceLength;
            short[] prev = this.prev;
            int scan = strstart;
            int best_end = strstart + matchLen;
            int best_len = Math.Max(matchLen, MIN_MATCH - 1);

            int limit = Math.Max(strstart - MAX_DIST, 0);

            int strend = strstart + MAX_MATCH - 1;
            byte scan_end1 = window[best_end - 1];
            byte scan_end = window[best_end];

            if (best_len >= goodLength)
                chainLength >>= 2;
            if (niceLength > lookahead)
                niceLength = lookahead;

            do
            {
                if (window[curMatch + best_len] != scan_end ||
                    window[curMatch + best_len - 1] != scan_end1 ||
                    window[curMatch] != window[scan] ||
                    window[curMatch + 1] != window[scan + 1])
                {
                    continue;
                }

                int match = curMatch + 2;
                scan += 2;

                while (
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    (scan < strend))
                {
                }

                if (scan > best_end)
                {
                    matchStart = curMatch;
                    best_end = scan;
                    best_len = scan - strstart;

                    if (best_len >= niceLength) break;

                    scan_end1 = window[best_end - 1];
                    scan_end = window[best_end];
                }

                scan = strstart;
            } while ((curMatch = (prev[curMatch & WMASK] & 0xffff)) > limit && --chainLength != 0);

            matchLen = Math.Min(best_len, lookahead);
            return matchLen >= MIN_MATCH;
        }

        private bool DeflateStored(bool flush, bool finish)
        {
            if (!flush && (lookahead == 0))
                return false;

            strstart += lookahead;
            lookahead = 0;

            int storedLength = strstart - blockStart;

            if ((storedLength >= MAX_BLOCK_SIZE) || (blockStart < WSIZE && storedLength >= MAX_DIST) || flush)
            {
                bool lastBlock = finish;
                if (storedLength > MAX_BLOCK_SIZE)
                {
                    storedLength = MAX_BLOCK_SIZE;
                    lastBlock = false;
                }

                huffman.FlushStoredBlock(window, blockStart, storedLength, lastBlock);
                blockStart += storedLength;

                return !lastBlock;
            }

            return true;
        }

        private bool DeflateFast(bool flush, bool finish)
        {
            if (lookahead < MIN_LOOKAHEAD && !flush)
                return false;

            while (lookahead >= MIN_LOOKAHEAD || flush)
            {
                if (lookahead == 0)
                {
                    huffman.FlushBlock(window, blockStart, strstart - blockStart, finish);
                    blockStart = strstart;
                    return false;
                }

                if (strstart > 2 * WSIZE - MIN_LOOKAHEAD)
                    SlideWindow();

                int hashHead;
                if (lookahead >= MIN_MATCH &&
                    (hashHead = InsertString()) != 0 &&
                    Strategy != StiDeflateStrategy.HuffmanOnly &&
                    strstart - hashHead <= MAX_DIST &&
                    FindLongestMatch(hashHead))
                {

                    bool full = huffman.TallyDist(strstart - matchStart, matchLen);

                    lookahead -= matchLen;
                    if (matchLen <= max_lazy && lookahead >= MIN_MATCH)
                    {
                        while (--matchLen > 0)
                        {
                            ++strstart;
                            InsertString();
                        }
                        ++strstart;
                    }
                    else
                    {
                        strstart += matchLen;
                        if (lookahead >= MIN_MATCH - 1)
                            UpdateHash();
                    }
                    matchLen = MIN_MATCH - 1;
                    if (!full) continue;
                }
                else
                {
                    huffman.TallyLit(window[strstart] & 0xff);
                    ++strstart;
                    --lookahead;
                }

                if (huffman.IsFull())
                {
                    bool lastBlock = finish && (lookahead == 0);
                    huffman.FlushBlock(window, blockStart, strstart - blockStart, lastBlock);
                    blockStart = strstart;

                    return !lastBlock;
                }
            }

            return true;
        }

        private bool DeflateSlow(bool flush, bool finish)
        {
            if (lookahead < MIN_LOOKAHEAD && !flush)
                return false;

            while (lookahead >= MIN_LOOKAHEAD || flush)
            {
                if (lookahead == 0)
                {
                    if (prevAvailable)
                        huffman.TallyLit(window[strstart - 1] & 0xff);

                    prevAvailable = false;
                    huffman.FlushBlock(window, blockStart, strstart - blockStart, finish);
                    blockStart = strstart;
                    return false;
                }

                if (strstart >= 2 * WSIZE - MIN_LOOKAHEAD)
                    SlideWindow();

                int prevMatch = matchStart;
                int prevLen = matchLen;
                if (lookahead >= MIN_MATCH)
                {
                    int hashHead = InsertString();

                    if (Strategy != StiDeflateStrategy.HuffmanOnly && hashHead != 0 && strstart - hashHead <= MAX_DIST && FindLongestMatch(hashHead))
                    {
                        if (matchLen <= 5 && (Strategy == StiDeflateStrategy.Filtered || (matchLen == MIN_MATCH && strstart - matchStart > TooFar)))
                            matchLen = MIN_MATCH - 1;
                    }
                }

                if ((prevLen >= MIN_MATCH) && (matchLen <= prevLen))
                {
                    huffman.TallyDist(strstart - 1 - prevMatch, prevLen);
                    prevLen -= 2;
                    do
                    {
                        strstart++;
                        lookahead--;
                        if (lookahead >= MIN_MATCH)
                            InsertString();
                    } while (--prevLen > 0);

                    strstart++;
                    lookahead--;
                    prevAvailable = false;
                    matchLen = MIN_MATCH - 1;
                }
                else
                {
                    if (prevAvailable)
                        huffman.TallyLit(window[strstart - 1] & 0xff);
                    prevAvailable = true;
                    strstart++;
                    lookahead--;
                }

                if (huffman.IsFull())
                {
                    int len = strstart - blockStart;
                    if (prevAvailable) len--;

                    bool lastBlock = (finish && (lookahead == 0) && !prevAvailable);
                    huffman.FlushBlock(window, blockStart, len, lastBlock);
                    blockStart += len;

                    return !lastBlock;
                }
            }

            return true;
        }
        #endregion

        public StiDeflaterEngine(StiDeflaterPending pending)
        {
            this.pending = pending;
            huffman = new StiDeflaterHuffman(pending);
            adler = new StiAdler32();

            window = new byte[2 * WSIZE];
            head = new short[HASH_SIZE];
            prev = new short[WSIZE];

            blockStart = strstart = 1;
        }
    }
}