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
using System.Text;

namespace WCFHelper.Compression
{
    internal class StiZipNameTransform : IStiNameTransform
    {
        #region static Fields
        private static readonly char[] InvalidEntryChars;
        private static readonly char[] InvalidEntryCharsRelaxed;
        #endregion

        #region Methods
        public string TransformDirectory(string name)
        {
            name = TransformFile(name);
            if (name.Length > 0)
            {
                if (!name.EndsWith("/"))
                    name += "/";
            }
            else
                throw new Exception("Cannot have an empty directory name");

            return name;
        }

        public string TransformFile(string name)
        {
            if (name != null)
            {
                string lowerName = name.ToLower();
                if (Path.IsPathRooted(name))
                    name = name.Substring(Path.GetPathRoot(name).Length);

                name = name.Replace(@"\", "/");

                while ((name.Length > 0) && (name[0] == '/'))
                {
                    name = name.Remove(0, 1);
                }

                name = MakeValidName(name, '_');
            }
            else
            {
                name = string.Empty;
            }

            return name;
        }

        static string MakeValidName(string name, char replacement)
        {
            int index = name.IndexOfAny(InvalidEntryChars);
            if (index > 0)
            {
                StringBuilder builder = new StringBuilder(name);

                while (index >= 0)
                {
                    builder[index] = replacement;
                    index = (index >= name.Length) ? -1 : name.IndexOfAny(InvalidEntryChars, index + 1);
                }

                name = builder.ToString();
            }

            return name;
        }
        #endregion

        public StiZipNameTransform()
        {
        }

        static StiZipNameTransform()
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            int howMany = invalidPathChars.Length + 2;

            InvalidEntryCharsRelaxed = new char[howMany];
            Array.Copy(invalidPathChars, 0, InvalidEntryCharsRelaxed, 0, invalidPathChars.Length);
            InvalidEntryCharsRelaxed[howMany - 1] = '*';
            InvalidEntryCharsRelaxed[howMany - 2] = '?';

            howMany = invalidPathChars.Length + 4;
            InvalidEntryChars = new char[howMany];
            Array.Copy(invalidPathChars, 0, InvalidEntryChars, 0, invalidPathChars.Length);
            InvalidEntryChars[howMany - 1] = ':';
            InvalidEntryChars[howMany - 2] = '\\';
            InvalidEntryChars[howMany - 3] = '*';
            InvalidEntryChars[howMany - 4] = '?';
        }
    }
}