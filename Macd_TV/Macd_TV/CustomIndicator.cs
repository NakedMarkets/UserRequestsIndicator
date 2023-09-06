using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MACD_TV
{
    public class MACD_TV : IndicatorInterface
    {
        [Separator(Label = "Common")]
        public string Separator_Common;
        [Input(Name = "Fast Length")]
        public int FastEMAPeriod = 12;
        [Input(Name = "Slow Length")]
        public int SlowEMAPeriod = 26;
        [Input(Name = "Signal Smoothing")]
        public int SMAPeriod = 9;
        [Input(Name = "Oscillator MA Type")]
        public MA_Method MethodMASource;

        public IndicatorBuffer FastEMA = new IndicatorBuffer();
        public IndicatorBuffer SlowEMA = new IndicatorBuffer();
        public IndicatorBuffer _MACD = new IndicatorBuffer();
        public IndicatorBuffer SMA = new IndicatorBuffer();
        public IndicatorBuffer Diff = new IndicatorBuffer();
        public IndicatorBuffer ColorBuffer = new IndicatorBuffer();

        private double LastDiffValue = 0;
        private Color col_grow_above = ColorTranslator.FromHtml("#26A69A");
        private Color col_fall_above = ColorTranslator.FromHtml("#B2DFDB");
        private Color col_grow_below = ColorTranslator.FromHtml("#FFCDD2");
        private Color col_fall_below = ColorTranslator.FromHtml("#FF5252");

        public override void OnInit()
        {
            SetIndicatorShortName("MACD (TV version)");
            Indicator_Separate_Window = true;

            SetLevel(0, Color.DarkGray, LineStyle.STYLE_DOT);
            SetIndexBuffer(0, _MACD);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, ColorTranslator.FromHtml("#2962FF"), LineStyle.STYLE_SOLID);
            SetIndexLabel(0, "MACD");
            SetIndexBuffer(1, SMA);
            SetIndexStyle(1, DrawingStyle.DRAW_LINE, ColorTranslator.FromHtml("#FF6D00"), LineStyle.STYLE_SOLID);
            SetIndexLabel(1, "Signal");
            SetIndexBuffer(2, Diff);
            SetIndexStyle(2, DrawingStyle.DRAW_HISTOGRAM, Color.Transparent);
            SetIndexLabel(2, "Histogram");
            SetIndexColors(2, ColorBuffer, col_grow_above, col_fall_above, col_grow_below, col_fall_below);
        }

        public override void OnCalculate(int index)
        {
            double k, sum;

            FastEMA[index] = GetMA(Symbol(), Period(), index, 0, FastEMAPeriod, MA_Method.MODE_EMA, Applied_Price.PRICE_CLOSE, FastEMA[index + 1]);
            SlowEMA[index] = GetMA(Symbol(), Period(), index, 0, SlowEMAPeriod, MA_Method.MODE_EMA, Applied_Price.PRICE_CLOSE, SlowEMA[index + 1]);
            _MACD[index] = FastEMA[index] - SlowEMA[index];

            sum = 0;
            for (int i = index; i < index + SMAPeriod; i++)
                sum = sum + _MACD[i];

            SMA[index] = sum / SMAPeriod;
            Diff[index] = _MACD[index] - SMA[index];

            if (Diff[index] > 0)            
                ColorBuffer[index] = Diff[index] > LastDiffValue ? - 1 : 1;            
            else
                ColorBuffer[index] = Diff[index] > LastDiffValue ? 2 : 3;

            LastDiffValue = Diff[index];
        }

    }
}
