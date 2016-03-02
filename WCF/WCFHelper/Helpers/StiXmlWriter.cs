#region Copyright (C) 2003-2012 Stimulsoft
/*
{*******************************************************************}
{																	}
{	Stimulsoft Reports.SL											}
{	                         										}
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
using System.Text;
using System.Collections.Generic;
using System.Collections;
using Stimulsoft.Base.Drawing;

namespace WCFHelper
{
    public class StiXmlWriter
    {
        #region Fields
        public bool IsEncodeString = false;
        private StringBuilder builder;
        private List<string> headers = new List<string>();
        #endregion

        #region Methods
        public void WriteStartElement(string text)
        {
            headers.Add(text);

            builder.Append("<");
            builder.Append(text);
            builder.Append(">");
        }

        public void WriteEndElement()
        {
            int index = headers.Count - 1;
            string text = headers[index];
            headers.RemoveAt(index);

            builder.Append("</");
            builder.Append(text);
            builder.Append(">");
        }

        public void WriteStartElementAndContent(string name, string content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, int content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, bool content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content ? "1" : "0");
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, object content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, float content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndContent(string name, double content)
        {
            builder.Append("<" + name + ">");
            builder.Append(content);
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndEmptyContent(string name)
        {
            builder.Append("<" + name + ">");
            builder.Append("</" + name + ">");
        }

        public void WriteStartElementAndSimpleEndElement(string name)
        {
            builder.Append("<" + name + "/>");
        }

        public void WriteSimpleEndElement()
        {
            int index = headers.Count - 1;
            headers.RemoveAt(index);

            builder.Insert(builder.Length - 1, "/");
        }

        public void WriteString(string value)
        {
            builder.Append(value);
        }

        public void WriteInt(int value)
        {
            builder.Append(value);
        }

        #region WriteAttributeString
        public void WriteSimpleAttribute(string attr, int value)
        {
            string str = " " + attr + "=\"" + value + "\"";
            builder.Insert(builder.Length - 1, str);
        }
        #endregion

        public override string ToString()
        {
            return (this.IsEncodeString) ? StiSLEncodingHelper.EncodeString(builder.ToString()) : builder.ToString();
        }
        #endregion

        public StiXmlWriter()
        {
            builder = new StringBuilder();
        }
    }
}