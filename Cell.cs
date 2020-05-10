using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku.NET
{
    public class Cell : Button
    {
        private static List<Color> ColorList = new List<Color>() { 
            Color.FromArgb(225),
            Color.Aqua,
            Color.Bisque,
            Color.Chocolate,
            Color.MediumPurple,
            Color.HotPink,
            Color.LightSteelBlue,
            Color.Thistle,
            Color.MediumAquamarine,
            Color.YellowGreen
        };

        const int blink = 100;
        const int blink_loop = 2;
        private bool settingDataFlag = false;
        public int Data
        {
            get
            {
                int val;
                if (int.TryParse(this.Text, out val))
                {
                    return val;
                }

                return 0;
            }
            set
            {
                if (value > 0)
                {
                    var targetColor = ColorList[value % 10];

                    //Task.Factory.StartNew(() =>
                    //{
                    //    settingDataFlag = true;
                    //    this.Text = value.ToString();

                    //    for (int i = 0; i < blink_loop; i++)
                    //    {
                    //        //this.BackColor = Color.LightSalmon;
                    //        this.BackColor = targetColor;
                    //        Thread.Sleep(blink);
                    //        this.BackColor = Color.LemonChiffon;
                    //        Thread.Sleep(blink);
                    //    }

                    //    //this.BackColor = ColorList[value.Value % 10];
                    //    this.BackColor = targetColor;
                    //    settingDataFlag = false;
                    //});

                    this.Text = value.ToString();
                    this.BackColor = targetColor;

                }
                else
                {
                    //Task.Factory.StartNew(() =>
                    //{

                    //    for (int i = 0; i < blink_loop; i++)
                    //    {
                    //        if (settingDataFlag == false)
                    //        {
                    //            break;
                    //        }

                    //        Thread.Sleep(blink);
                    //    }
                    //    this.Text = "";
                    //    this.BackColor = ColorList[0];
                    //});

                    this.Text = "";
                    this.BackColor = ColorList[0];
                }
            }
        }

        public void SetPosition(int val)
        {
            Line = (val - val % 10) / 10;
            Column = val % 10;
        }

        public int Line { get; private set; } = 0;

        public int Column { get; private set; } = 0;

    }
}
