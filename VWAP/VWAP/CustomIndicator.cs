using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomIndicator
{
    public class CustomIndicator : IndicatorInterface
    {
        [Separator(Label = "Common")]
        public string Separator_Common;
        [Input(Name = "Calculation mode")]
        public CalculationMode Mode = CalculationMode.hlc3;

        public IndicatorBuffer ExtBufferVWAP = new IndicatorBuffer();
        double __ohlcvTotal, __volumeTotal;
        DateTime __sessionStartTime;

        public override void OnInit()
        {
            SetIndicatorShortName("VWAP");
            SetIndexBuffer(0, ExtBufferVWAP);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, Color.DarkMagenta);
            SetIndexLabel(0, "VWAP Line");
        }
        public override void OnCalculate(int index)
        {
            int i = index;
            double ohlcAvg = 0;
            double vol = Volume(i);

            if (Mode == CalculationMode.hl2)
                ohlcAvg = (High(i) + Low(i)) / 2;
            else
            if (Mode == CalculationMode.hlc3)
                ohlcAvg = (High(i) + Low(i) + Close(i)) / 3;
            else
            if(Mode == CalculationMode.ohlc4)
                ohlcAvg = (Open(i) + High(i) + Low(i) + Close(i)) / 4;
            else
            if(Mode == CalculationMode.hlcc4)
                ohlcAvg = (High(i) + Low(i) + Close(i) + Close(i)) / 4;            

            if (Time(i).Day != __sessionStartTime.Day)
            {
                __sessionStartTime = Time(i);
                __ohlcvTotal = 0;
                __volumeTotal = 0;
            }

            __ohlcvTotal += ohlcAvg * vol;
            __volumeTotal += vol;
            ExtBufferVWAP[i] = __ohlcvTotal / __volumeTotal;
        }

        public enum CalculationMode
        {
            hl2,
            hlc3,
            ohlc4,
            hlcc4
        }
    }
}
