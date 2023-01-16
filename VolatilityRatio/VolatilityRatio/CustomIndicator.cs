using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace VolatilityRatio
{
    public class VolatilityRatio : IndicatorInterface
    {
        [Separator(Label = "Common")]
        public string Separator_Common;
        [Input(Name = "Volatility Period")]
        public int inpPeriod = 25;
        [Input(Name = "Apply to price")]
        public Applied_Price ApplyToPriceParameter;

        public IndicatorBuffer val = new IndicatorBuffer();
        public IndicatorBuffer valc = new IndicatorBuffer();

        public cStdDevVolatilityRatio iVolatilityRatio;
        public override void OnInit()
        {
            SetIndicatorShortName("Volatility Ratio ("+inpPeriod+")");
            Indicator_Separate_Window = true;
            SetIndexBuffer(0, val);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, Color.DarkBlue);
            SetIndexLabel(0, "Volatility");
            SetIndexColors(0, valc, Color.Gray, Color.LawnGreen, Color.OrangeRed);
            iVolatilityRatio = new cStdDevVolatilityRatio(inpPeriod);
        }

        public override void OnCalculate(int index)
        {            
            iVolatilityRatio.m_array.Clear();

            if (index != 0)
                return;

            for (int i = index; i < Bars(); i++)
            {
                iVolatilityRatio.m_array.Add(new sStdDevVolatilityRatioStruct());
                int ReverseIndex = Bars() - i - 1;
                double _price = GetAppliedPrice(Symbol(), Period(), ReverseIndex, ApplyToPriceParameter);
                val[ReverseIndex] = iVolatilityRatio.Calculate(_price, i, Bars());
                valc[ReverseIndex] = (val[ReverseIndex] > 1) ? 1 : (val[ReverseIndex] < 1) ? 2 : 0;
            }
        }

        public class sStdDevVolatilityRatioStruct
        {
            public double price;
            public double price2;
            public double sum;
            public double sum2;
            public double sumd;
            public double deviation;
        }

        public class cStdDevVolatilityRatio
        {
            public int m_period;
            public List<sStdDevVolatilityRatioStruct> m_array = new List<sStdDevVolatilityRatioStruct>();

            public cStdDevVolatilityRatio(int period)
            {
                m_period = (period > 1) ? period : 1;
            }

            public double Calculate(double price, int i, int bars)
            {
                m_array[i].price = price;
                m_array[i].price2 = price * price;

                if (i > m_period)
                {
                    m_array[i].sum = m_array[i - 1].sum + m_array[i].price - m_array[i - m_period].price;
                    m_array[i].sum2 = m_array[i - 1].sum2 + m_array[i].price2 - m_array[i - m_period].price2;
                }
                else
                {
                    m_array[i].sum = m_array[i].price;
                    m_array[i].sum2 = m_array[i].price2;
                    for (int k = 1; k < m_period && i >= k; k++)
                    {
                        m_array[i].sum += m_array[i - k].price;
                        m_array[i].sum2 += m_array[i - k].price2;
                    }
                }

                m_array[i].deviation = (Math.Sqrt((m_array[i].sum2 - m_array[i].sum * m_array[i].sum / (double)m_period) / (double)m_period));

                if (i > m_period)
                    m_array[i].sumd = m_array[i - 1].sumd + m_array[i].deviation - m_array[i - m_period].deviation;
                else
                {
                    m_array[i].sumd = m_array[i].deviation;
                    for (int k = 1; k < m_period && i >= k; k++)
                        m_array[i].sumd += m_array[i - k].deviation;
                }

                double deviationAverage = m_array[i].sumd / (double)m_period;
                return (deviationAverage != 0 ? m_array[i].deviation / deviationAverage : 1);
            }
        }

    }
}
