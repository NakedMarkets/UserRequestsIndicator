using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Hull
{
    public class Hull : IndicatorInterface
    {
        [Separator(Label = "Common")]
        public string Separator_Common;
        [Input(Name = "Period")]
        public int inpPeriod = 20;
        [Input(Name = "Divisor")]
        public double inpDivisor = 2.0;
        [Input(Name = "Apply to price")]
        public Applied_Price inpPrice;

        public IndicatorBuffer val = new IndicatorBuffer();
        public IndicatorBuffer valc = new IndicatorBuffer();

        public CHull iHull;

        public override void OnInit()
        {
            SetIndicatorShortName("Hull (" + inpPeriod + ")");
            Indicator_Separate_Window = false;
            SetIndexBuffer(0, val);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, Color.Transparent, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(0, "val");
            SetIndexColors(0, valc, Color.Silver, Color.Green, Color.DarkOrange);
            iHull = new CHull(inpPeriod, inpDivisor);
        }
        public override void OnCalculate(int index)
        {
            iHull.m_array.Clear();

            if (index != 0)
                return;
            
            for (int i = Bars() - 1 ; i > 0; i--)
            {
                iHull.m_array.Add(new sHullArrayStruct());
                int ReverseIndex = Bars() - i - 1;
                double _price = GetAppliedPrice(Symbol(), Period(), i, inpPrice);
                val[i] = iHull.Calculate(_price, ReverseIndex, Bars());
                valc[i] = (val[i] > val[i + 1]) ? 1 : (val[i] < val[i + 1]) ? 2 : valc[i + 1];
            }
        }

        public class sHullArrayStruct
        {
            public double value;
            public double value3;
            public double wsum1;
            public double wsum2;
            public double wsum3;
            public double lsum1;
            public double lsum2;
            public double lsum3;
        }

        public class CHull
        {
            int m_fullPeriod;
            int m_halfPeriod;
            int m_sqrtPeriod;
            double m_weight1;
            double m_weight2;
            double m_weight3;
            public List<sHullArrayStruct> m_array = new List<sHullArrayStruct>();
            public CHull(int period, double divisor)
            {
                m_fullPeriod = (int)(period > 1 ? period : 1);
                m_halfPeriod = (int)(m_fullPeriod > 1 ? m_fullPeriod / (divisor > 1 ? divisor : 1) : 1);
                m_sqrtPeriod = (int)Math.Sqrt(m_fullPeriod);
            }

            public double Calculate(double value, int i, int bars)
            {
                m_array[i].value = value;
                if (i > m_fullPeriod)
                {
                    m_array[i].wsum1 = m_array[i - 1].wsum1 + value * m_halfPeriod - m_array[i - 1].lsum1;
                    m_array[i].lsum1 = m_array[i - 1].lsum1 + value - m_array[i - m_halfPeriod].value;
                    m_array[i].wsum2 = m_array[i - 1].wsum2 + value * m_fullPeriod - m_array[i - 1].lsum2;
                    m_array[i].lsum2 = m_array[i - 1].lsum2 + value - m_array[i - m_fullPeriod].value;
                }
                else
                {
                    m_array[i].wsum1 = m_array[i].wsum2 =
                    m_array[i].lsum1 = m_array[i].lsum2 = m_weight1 = m_weight2 = 0;
                    for (int k = 0, w1 = m_halfPeriod, w2 = m_fullPeriod; w2 > 0 && i >= k; k++, w1--, w2--)
                    {
                        if (w1 > 0)
                        {
                            m_array[i].wsum1 += m_array[i - k].value * w1;
                            m_array[i].lsum1 += m_array[i - k].value;
                            m_weight1 += w1;
                        }
                        m_array[i].wsum2 += m_array[i - k].value * w2;
                        m_array[i].lsum2 += m_array[i - k].value;
                        m_weight2 += w2;
                    }
                }
                m_array[i].value3 = 2.0 * m_array[i].wsum1 / m_weight1 - m_array[i].wsum2 / m_weight2;

                if (i > m_sqrtPeriod)
                {
                    m_array[i].wsum3 = m_array[i - 1].wsum3 + m_array[i].value3 * m_sqrtPeriod - m_array[i - 1].lsum3;
                    m_array[i].lsum3 = m_array[i - 1].lsum3 + m_array[i].value3 - m_array[i - m_sqrtPeriod].value3;
                }
                else
                {
                    m_array[i].wsum3 =
                    m_array[i].lsum3 = m_weight3 = 0;
                    for (int k = 0, w3 = m_sqrtPeriod; w3 > 0 && i >= k; k++, w3--)
                    {
                        m_array[i].wsum3 += m_array[i - k].value3 * w3;
                        m_array[i].lsum3 += m_array[i - k].value3;
                        m_weight3 += w3;
                    }
                }
                return (m_array[i].wsum3 / m_weight3);
            }
        }
    }
}
