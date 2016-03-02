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
using System.Collections;
using System.Collections.Generic;

using Stimulsoft.Report;
using Stimulsoft.Report.Components;

namespace WCFHelper
{
    internal class StiDrillDownContainer
    {
        #region Fields
        public string TypeDrillDown;

        public string DrillDownPageName;
        public Dictionary<string, object> DrillDownParameters;
        public int PageIndex;
        public int CompIndex;
        public StiReport Report;

        public string DataBandName;
        public string[] DataBandColumns;
        public string DataBandColumnString;
        public StiInteractionSortDirection SortingDirection;
        public bool IsControlPress;
        public int CollapsingIndex;
        public bool IsCollapsed;
        public Hashtable InteractionCollapsingStates;
        #endregion

        public StiDrillDownContainer()
        {
            TypeDrillDown = string.Empty;

            DrillDownPageName = string.Empty;
            DrillDownParameters = new Dictionary<string, object>();
            Report = new StiReport();

            DataBandName = string.Empty;
            DataBandColumnString = string.Empty;
            SortingDirection = StiInteractionSortDirection.None;
        }
    }
}