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
    internal enum StiUseZip64
    {
        Off,
        On,
        Dynamic,
    }

    internal enum StiCompressionMethod
    {
        Stored = 0,
        Deflated = 8,
        Deflate64 = 9,
        BZip2 = 11,
        WinZipAES = 99
    }

    [Flags]
    internal enum StiGeneralBitFlags
    {
        Encrypted = 0x0001,
        Method = 0x0006,
        Descriptor = 0x0008,
        ReservedPKware4 = 0x0010,
        Patched = 0x0020,
        StrongEncryption = 0x0040,
        Unused7 = 0x0080,
        Unused8 = 0x0100,
        Unused9 = 0x0200,
        Unused10 = 0x0400,
        UnicodeText = 0x0800,
        EnhancedCompress = 0x1000,
        HeaderMasked = 0x2000,
        ReservedPkware14 = 0x4000,
        ReservedPkware15 = 0x8000
    }

    internal enum StiHostSystemID
    {
        Msdos = 0,
        Amiga = 1,
        OpenVms = 2,
        Unix = 3,
        VMCms = 4,
        AtariST = 5,
        OS2 = 6,
        Macintosh = 7,
        ZSystem = 8,
        Cpm = 9,
        WindowsNT = 10,
        MVS = 11,
        Vse = 12,
        AcornRisc = 13,
        Vfat = 14,
        AlternateMvs = 15,
        BeOS = 16,
        Tandem = 17,
        OS400 = 18,
        OSX = 19,
        WinZipAES = 99,
    }

    internal enum StiTestStrategy
    {
        FindFirstError,
        FindAllErrors
    }

    internal enum StiTestOperation
    {
        Initialising,
        EntryHeader,
        EntryData,
        EntryComplete,
        MiscellaneousTests,
        Complete,
    }

    internal enum StiFileUpdateMode
    {
        Safe,
        Direct,
    }
}