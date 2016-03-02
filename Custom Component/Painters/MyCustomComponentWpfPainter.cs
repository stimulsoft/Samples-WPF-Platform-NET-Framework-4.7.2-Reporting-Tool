using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections;
using System.Text;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Report;
using Stimulsoft.Report.Wpf;
using Stimulsoft.Report.Units;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Components;
using Stimulsoft.Report.Dialogs;
using Stimulsoft.Report.Design;
using Stimulsoft.Report.Painters;
using Stimulsoft.Report.Events;

namespace CustomComponent
{
    public class MyCustomComponentWpfPainter : StiComponentWpfPainter
    {
        public virtual void PaintBackground(MyCustomComponent customComponent, DrawingContext dc, Rect rect)
        {
            if (customComponent.Brush is StiSolidBrush &&
                ((StiSolidBrush)customComponent.Brush).Color == System.Drawing.Color.Transparent &&
                customComponent.Report.Info.FillComponent &&
                customComponent.IsDesigning)
            {
                Color color = Colors.White;
                color.A = 150;

                Brush brush = new SolidColorBrush(color);

                dc.DrawRectangle(brush, null, rect);
            }
            else dc.DrawRectangle(StiBrushHelper.BrushToWpfBrush(customComponent.Brush), null, rect);
        }

        public override void Paint(StiComponent component, StiPaintEventArgs e)
        {
            DrawingContext dc = e.Context as DrawingContext;

            RectangleD rectD = component.GetPaintRectangle(true, false);
            Rect rect = new Rect(rectD.Left, rectD.Top, rectD.Width, rectD.Height);
            if (rect.Width > 0 && rect.Height > 0)
            {
                MyCustomComponent customComponent = component as MyCustomComponent;

                #region Fill rectangle
                PaintBackground(customComponent, dc, rect);
                #endregion

                #region Markers
                if (customComponent.HighlightState == StiHighlightState.Hide && customComponent.Border.Side != StiBorderSides.All)
                    PaintMarkers(customComponent, dc, rect);
                #endregion

                //Own drawing


                #region Border
                base.PaintBorder(customComponent, dc, rect, true, true);
                #endregion
            }

            PaintEvents(component, dc, rect);
            PaintConditions(component, dc, rect);
            PaintInteraction(component, dc);
        }
    }
}
