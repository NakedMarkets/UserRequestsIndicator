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

        [Input(Name = "R. Quadratic Loopback")]
        public int RationalQuadraticLoopback = 8;
        [Input(Name = "R. Quadratic Weight")]
        public int RationalQuadraticWeight = 1;
        [Input(Name = "R. Quadratic StartBar")]
        public int RationalQuadraticStartBar = 25;

        [Input(Name = "Gaussian Loopback")]
        public int GaussianLoopback = 16;
        [Input(Name = "Gaussian StartBar")]
        public int GaussianStartBar = 25;

        [Input(Name = "Periodic Loopback")]
        public int PeriodicLoopback = 8;
        [Input(Name = "Periodic Period")]
        public int PeriodicPeriod = 100;
        [Input(Name = "Periodic StartBar")]
        public int PeriodicStartBar = 25;

        [Input(Name = "L. Periodic Loopback")]
        public int LocallyPeriodicLoopback = 8;
        [Input(Name = "L. Periodic Period")]
        public int LocallyPeriodicPeriod = 24;
        [Input(Name = "L. Periodic StartBar")]
        public int LocallyPeriodicStartBar = 25;

        public IndicatorBuffer RationalQuadraticBuffer = new IndicatorBuffer();
        public IndicatorBuffer GaussianBuffer = new IndicatorBuffer();
        public IndicatorBuffer PeriodicBuffer = new IndicatorBuffer();
        public IndicatorBuffer LocallyPeriodicBuffer = new IndicatorBuffer();

        public override void OnInit()
        {
            SetIndicatorShortName("Kernel Functions");
            Indicator_Separate_Window = false;

            SetIndexBuffer(0, RationalQuadraticBuffer);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, Color.Red, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(0, "Rational Quadratic Kernel");

            SetIndexBuffer(1, GaussianBuffer);
            SetIndexStyle(1, DrawingStyle.DRAW_LINE, Color.Yellow, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(1, "Gaussian Kernel");

            SetIndexBuffer(2, PeriodicBuffer);
            SetIndexStyle(2, DrawingStyle.DRAW_LINE, Color.Green, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(2, "Periodic Kernel");

            SetIndexBuffer(3, LocallyPeriodicBuffer);
            SetIndexStyle(3, DrawingStyle.DRAW_LINE, Color.Aqua, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(3, "Local Periodic Kernel");               
        }

        public override void OnCalculate(int index)
        {
            if (index + PeriodicPeriod >= Bars())
                return;

            RationalQuadraticBuffer[index] = GetRationalQuadratic(index, RationalQuadraticLoopback, RationalQuadraticWeight, RationalQuadraticStartBar);
            GaussianBuffer[index] = GetGaussian(index, GaussianLoopback, GaussianStartBar);
            PeriodicBuffer[index] = GetPeriodic(index, PeriodicLoopback, PeriodicPeriod, PeriodicStartBar);
            LocallyPeriodicBuffer[index] = GetLocallyPeriodic(index, LocallyPeriodicLoopback, LocallyPeriodicPeriod, LocallyPeriodicStartBar);
        }

        private double GetRationalQuadratic(int Index, int Loopback, int RelativeWeight, int StartAtBar)
        {
            double CurrentWeight = 0;
            double CumulativeWeight = 0;
            int b = 0;

            for (int i = Index; i < Index + StartAtBar + 1; i++)
            {
                double y = Close(i);
                double w = Math.Pow(1 + (Math.Pow(b, 2) / ((Math.Pow(Loopback, 2) * 2 * RelativeWeight))), -RelativeWeight);
                CurrentWeight += (y * w);
                CumulativeWeight += w;
                b++;
            }

            return CurrentWeight / CumulativeWeight;
        }

        private double GetGaussian(int Index, int Loopback, int StartAtBar)
        {
            double CurrentWeight = 0;
            double CumulativeWeight = 0;
            int b = 0;

            for (int i = Index; i < Index + StartAtBar + 1; i++)
            {
                double y = Close(i);
                double w = Math.Exp(-Math.Pow(b, 2) / (2 * Math.Pow(Loopback, 2)));
                CurrentWeight += (y * w);
                CumulativeWeight += w;
                b++;
            }

            return CurrentWeight / CumulativeWeight;
        }

        private double GetPeriodic(int Index, int Loopback, int Period, int StartAtBar)
        {
            double CurrentWeight = 0;
            double CumulativeWeight = 0;
            int b = 0;

            for (int i = Index; i < Index + StartAtBar + 1; i++)
            {
                double y = Close(i);
                double w = Math.Exp(-2 * Math.Pow(Math.Sin(Math.PI * b / Period), 2) / Math.Pow(Loopback, 2));
                CurrentWeight += (y * w);
                CumulativeWeight += w;
                b++;
            }

            return CurrentWeight / CumulativeWeight;
        }

        private double GetLocallyPeriodic(int Index, int Loopback, int Period, int StartAtBar)
        {
            double CurrentWeight = 0;
            double CumulativeWeight = 0;
            int b = 0;

            for (int i = Index; i < Index + StartAtBar + 1; i++)
            {
                double y = Close(i);                
                double w = Math.Exp(-2 * Math.Pow(Math.Sin(Math.PI * b / Period), 2) / Math.Pow(Loopback, 2)) * Math.Exp(-Math.Pow(b, 2) / (2 * Math.Pow(Loopback, 2)));
                CurrentWeight += (y * w);
                CumulativeWeight += w;
                b++;
            }

            return CurrentWeight / CumulativeWeight;
        }
    }
}
