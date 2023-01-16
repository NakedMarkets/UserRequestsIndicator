using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSL_Channel_Chart
{
    public class SSL_Channel_Chart : IndicatorInterface
    {
        [Separator(Label = "Common")]
        public string Separator_Common;
        [Input(Name = "MA Method")]
        public MA_Method MaMethodParameter;
        [Input(Name = "Lb")]
        public int Lb = 10;

        public IndicatorBuffer ssld = new IndicatorBuffer();
        public IndicatorBuffer sslu = new IndicatorBuffer();
        public IndicatorBuffer Hlv = new IndicatorBuffer();

        public override void OnInit()
        {
            SetIndicatorShortName("SSL Channel Chart");
            Indicator_Separate_Window = false;
            SetIndexBuffer(0, ssld);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, Color.Orange);
            SetIndexLabel(0, "Bears");
            SetIndexBuffer(1, sslu);
            SetIndexStyle(1, DrawingStyle.DRAW_LINE, Color.DarkBlue);
            SetIndexLabel(1, "Bulls");
            SetIndexBuffer(2, Hlv);
            SetIndexStyle(2, DrawingStyle.DRAW_NONE, Color.AliceBlue);
        }
        public override void OnCalculate(int index)
        {
            if (index != 0)
                return;

            for (int i = Bars() - Lb; i >= 0; i--)
            {
                double MAHigh = iMA(Symbol(), Period(), Lb, 0, MaMethodParameter, Applied_Price.PRICE_HIGH, i);
                double MALow = iMA(Symbol(), Period(), Lb, 0, MaMethodParameter, Applied_Price.PRICE_LOW, i);

                Hlv[i] = Hlv[i + 1];

                if (Close(i) > MAHigh) Hlv[i] = 1;
                if (Close(i) < MALow) Hlv[i] = -1;

                if (Hlv[i] == -1)
                {
                    ssld[i] = MAHigh;
                    sslu[i] = MALow;
                }
                else
                {
                    ssld[i] = MALow;
                    sslu[i] = MAHigh;
                }

            }

        }
    }
}
