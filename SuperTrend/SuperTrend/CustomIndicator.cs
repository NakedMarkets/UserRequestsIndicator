using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperTrend
{
    public class SuperTrend : IndicatorInterface
    {
        [Separator(Label = "Common")]
        public string Separator_Common;
        [Input(Name = "Period")]
        public int inpPeriod = 10;
        [Input(Name = "Multiplier")]
        public double Multiplier = 3;
        [Input(Name = "Show Filling")]
        public bool Show_Filling;

        public IndicatorBuffer Filled_a = new IndicatorBuffer();
        public IndicatorBuffer Filled_b = new IndicatorBuffer();
        public IndicatorBuffer SuperTrendBuffer = new IndicatorBuffer();
        public IndicatorBuffer ColorBuffer = new IndicatorBuffer();
        public IndicatorBuffer Atr = new IndicatorBuffer();
        public IndicatorBuffer Up = new IndicatorBuffer();
        public IndicatorBuffer Down = new IndicatorBuffer();
        public IndicatorBuffer Middle = new IndicatorBuffer();
        public IndicatorBuffer trend = new IndicatorBuffer();

        int changeOfTrend;
        int flag;
        int flagh;

        public override void OnInit()
        {
            SetIndicatorShortName("SuperTrend");
            Indicator_Separate_Window = false;            
            SetIndexBuffer(0, Filled_a);
            SetIndexBuffer(1, Filled_b);
            SetIndexBuffer(2, SuperTrendBuffer);
            SetIndexStyle(2, DrawingStyle.DRAW_LINE, Color.Transparent);
            SetIndexLabel(2, "SuperTrend");
            SetIndexColors(2, ColorBuffer, Color.Green, Color.Red);
            SetIndexBuffer(3, Atr);
            SetIndexBuffer(4, Up);
            SetIndexBuffer(5, Down);
            SetIndexBuffer(6, Middle);
            SetIndexBuffer(7, trend);
        }
        public override void OnCalculate(int index)
        {
            if (index != 0)
                return;

            for (int i = Bars(); i >= 0; i--)
            {
                Atr[i] = iATR(Symbol(), Period(), inpPeriod, i);

                Middle[i] = (High(i) + Low(i)) / 2;
                Up[i] = Middle[i] + (Multiplier * Atr[i]);
                Down[i] = Middle[i] - (Multiplier * Atr[i]);

                if (Close(i) > Up[i + 1])
                {
                    trend[i] = 1;
                    if (trend[i + 1] == -1) changeOfTrend = 1;

                }
                else if (Close(i) < Down[i + 1])
                {
                    trend[i] = -1;
                    if (trend[i + 1] == 1) changeOfTrend = 1;
                }
                else if (trend[i + 1] == 1)
                {
                    trend[i] = 1;
                    changeOfTrend = 0;
                }
                else if (trend[i + 1] == -1)
                {
                    trend[i] = -1;
                    changeOfTrend = 0;
                }

                if (trend[i] < 0 && trend[i + 1] > 0)
                {
                    flag = 1;
                }
                else
                {
                    flag = 0;
                }

                if (trend[i] > 0 && trend[i + 1] < 0)
                {
                    flagh = 1;
                }
                else
                {
                    flagh = 0;
                }

                if (trend[i] > 0 && Down[i] < Down[i + 1])
                    Down[i] = Down[i + 1];

                if (trend[i] < 0 && Up[i] > Up[i + 1])
                    Up[i] = Up[i + 1];

                if (flag == 1)
                    Up[i] = Middle[i] + (Multiplier * Atr[i]);

                if (flagh == 1)
                    Down[i] = Middle[i] - (Multiplier * Atr[i]);


                if (trend[i] == 1)
                {
                    SuperTrendBuffer[i] = Down[i];
                    if (changeOfTrend == 1)
                    {
                        SuperTrendBuffer[i + 1] = SuperTrendBuffer[i + 2];
                        changeOfTrend = 0;
                    }
                    ColorBuffer[i] = -1.0;
                }
                else if (trend[i] == -1)
                {
                    SuperTrendBuffer[i] = Up[i];
                    if (changeOfTrend == 1)
                    {
                        SuperTrendBuffer[i + 1] = SuperTrendBuffer[i + 2];
                        changeOfTrend = 0;
                    }
                    ColorBuffer[i] = 1.0;
                }
            }
        }
    }
}
