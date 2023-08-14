using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static IndicatorInterfaceCSharp.IndicatorInterface;

namespace CustomIndicator
{
    public class CustomIndicator : IndicatorInterface
    {
        [Separator(Label = "Common")]
        public string Separator_Common;
        [Input(Name = "Length EMA Fast")]
        public int emaFastLen = 30;
        [Input(Name = "Length EMA Slow")]
        public int emaSlowLen = 60;
        [Input(Name = "Margin EMA - ATR Length")]
        public int emaMarginATRLen = 60;
        [Input(Name = "Margin EMA - ATR Multiplier")]
        public double emaMarginATRMult = 0.3;

        public IndicatorBuffer emaFast = new IndicatorBuffer();
        public IndicatorBuffer emaSlow = new IndicatorBuffer();
        public IndicatorBuffer ColorBufferFast = new IndicatorBuffer();
        public IndicatorBuffer ColorBufferSlow = new IndicatorBuffer();
        public IndicatorBuffer AtrBuffer = new IndicatorBuffer();

        private Color BullColor = Color.Green;
        private Color BearColor = Color.DarkRed;
        private Color NeutralColor = Color.Yellow;

        public override void OnInit()
        {
            SetIndicatorShortName("Andres Line");
            Indicator_Separate_Window = false;
            SetIndexBuffer(0, emaFast);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, BullColor, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(0, "EMA Fast");
            SetIndexColors(0, ColorBufferFast, BullColor, NeutralColor, BearColor);

            SetIndexBuffer(1, emaSlow);
            SetIndexStyle(1, DrawingStyle.DRAW_LINE, BearColor, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(1, "EMA Slow");
            SetIndexColors(1, ColorBufferSlow, BullColor, NeutralColor, BearColor);
        }
        public override void OnCalculate(int index)
        {
            if (index + emaSlowLen >= Bars())
                return;

            emaFast[index] = GetMA(Symbol(), Period(), index, 0, emaFastLen, MA_Method.MODE_EMA, Applied_Price.PRICE_CLOSE, emaFast[index + 1]);
            emaSlow[index] = GetMA(Symbol(), Period(), index, 0, emaSlowLen, MA_Method.MODE_EMA, Applied_Price.PRICE_CLOSE, emaSlow[index + 1]);

            double emaDiff = emaFast[index] - emaSlow[index];
            double ATRValue = GetATR(index);

            bool emaBull = emaDiff > emaMarginATRMult * ATRValue;
            bool emaBear = emaDiff < -emaMarginATRMult * ATRValue;

            ColorBufferFast[index] = emaBull ? -1 : emaBear ? 1 : 0;
            ColorBufferSlow[index] = emaBull ? -1 : emaBear ? 1 : 0;

        }

        private double GetATR(int index)
        {
            double P_High, P_Low, prevclose, sum;

            P_High = High(index);
            P_Low = Low(index);

            if (index == Bars() - 1)
                AtrBuffer[index] = P_High - P_Low;
            else
            {
                prevclose = Close(index + 1);
                AtrBuffer[index] = Math.Max(P_High, prevclose) - Math.Min(P_Low, prevclose);
            }

            sum = 0;
            for (int i = 0; i < emaMarginATRLen; i++)
                sum = sum + AtrBuffer[index + i];

            return sum / emaMarginATRLen;
        }
    }
}
