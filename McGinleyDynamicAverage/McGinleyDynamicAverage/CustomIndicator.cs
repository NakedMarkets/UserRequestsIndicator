using IndicatorInterfaceCSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [Input(Name = "Period")]
        public int inpPeriod = 14;
        [Input(Name = "Apply to price")]
        public Applied_Price inpPrice;
        [Input(Name = "Calculation type")]
        public enMcgType inpType;

        public IndicatorBuffer val = new IndicatorBuffer();
        public IndicatorBuffer valc = new IndicatorBuffer();
        public double mcgMultiplier;

        public override void OnInit()
        {
            SetIndicatorShortName("McGinley dynamic (" + inpPeriod + ")");
            Indicator_Separate_Window = false;
            SetIndexBuffer(0, val);
            SetIndexStyle(0, DrawingStyle.DRAW_LINE, Color.Transparent, LineStyle.STYLE_SOLID, 2);
            SetIndexLabel(0, "val");
            SetIndexColors(0, valc, Color.Silver, Color.Green, Color.DarkOrange);            
        }
        public override void OnCalculate(int index)
        {
            mcgMultiplier = (inpType == enMcgType.mcg_original) ? 1.0 : 0.6;

            if (index != 0)
                return;

            for (int i = Bars(); i >= 0; i--)
            {
                double _price = GetAppliedPrice(Symbol(), Period(), i, inpPrice);

                if (i < Bars() && inpPeriod > 1)
                {
                    double _pow = inpPeriod * mcgMultiplier * Math.Pow((val[i + 1] != 0 && _price != 0 ? _price / val[i + 1] : 0), 4);
                    val[i] = (_pow != 0) ? val[i + 1] + (_price - val[i + 1]) / _pow : _price;
                }
                else
                    val[i] = _price;

                valc[i] = (i > 0) ? (val[i] > val[i + 1]) ? 1 : (val[i] < val[i + 1]) ? 2 : valc[i + 1] : 0;
            }
        }

        public enum enMcgType
        {
            [Description("Original formula")]
            mcg_original,
            [Description("Improved formula")]
            mcg_faster
        }
    }
}
