using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RoleColorGen
{
    public partial class Form1 : Form
    {
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 ITEM1 = 1000;
        public const Int32 ITEM2 = 1001;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);


        public Form1()
        {
            InitializeComponent();
        }

        public struct HSV { public float h; public float s; public float v; }

        HSV MakeColor(Color color)
        {
            var newColor = new HSV();
            newColor.h = color.GetHue();
            newColor.s = color.GetSaturation();
            newColor.v = color.GetBrightness();
            return newColor;
        }

        HSV MakeColor(float h, float s, float v)
        {
            var newColor = new HSV();
            newColor.h = h;
            newColor.s = s;
            newColor.v = v;
            return newColor;
        }

        static double RGBChannelFromHue(double m1, double m2, double h)
        {
            h = (h + 1d) % 1d;
            if (h < 0) h += 1;
            if (h * 6 < 1) return m1 + (m2 - m1) * 6 * h;
            else if (h * 2 < 1) return m2;
            else if (h * 3 < 2) return m1 + (m2 - m1) * 6 * (2d / 3d - h);
            else return m1;
        }

        static public Color ColorFromHSV(HSV hsv)
        {
            if (hsv.s == 0)
            { int L = (int)hsv.v; return Color.FromArgb(255, L, L, L); }

            double min, max, h;
            h = hsv.h / 360d;

            max = hsv.v < 0.5d ? hsv.v * (1 + hsv.s) : (hsv.v + hsv.s) - (hsv.v * hsv.s);
            min = (hsv.v * 2d) - max;

            Color c = Color.FromArgb(255, (int)(255 * RGBChannelFromHue(min, max, h + 1 / 3d)),
                                          (int)(255 * RGBChannelFromHue(min, max, h)),
                                          (int)(255 * RGBChannelFromHue(min, max, h - 1 / 3d)));
            return c;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            textBox1.Text = ConvertHex(colorDialog1.Color);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog2.ShowDialog();
            textBox2.Text = ConvertHex(colorDialog2.Color);
        }

        private void MixColors()
        {
            panel1.BackColor = ConvertColor(textBox1.Text);
            panel2.BackColor = ConvertColor(textBox2.Text);

            var color1 = MakeColor(panel1.BackColor);
            var color2 = MakeColor(panel2.BackColor);

            panel3.BackColor = ColorFromHSV(MakeColor((color1.h + color2.h) / 2, (color1.s + color2.s) / 2, (color1.v + color2.v) / 2));
            textBox3.Text = ConvertHex(panel3.BackColor);

            textBoxResult.Text = "";

            for (var i = 0; i < numericUpDown1.Value - 1; i++)
            {
                var h = color1.h + (color2.h - color1.h) * i / (int)(numericUpDown1.Value - 1);
                var s = color1.s + (color2.s - color1.s) * i / (int)(numericUpDown1.Value - 1);
                var v = color1.v + (color2.v - color1.v) * i / (int)(numericUpDown1.Value - 1);
                textBoxResult.Text += ConvertHex(ColorFromHSV(MakeColor(h, s, v))) + " ";
            }
            textBoxResult.Text += ConvertHex(panel2.BackColor);
        }

        private Color ConvertColor(string str)
        {
            try
            {
                return ColorTranslator.FromHtml(str);
            }
            catch
            {
                return Color.White;
            }
        }

        private static String ConvertHex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            MixColors();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            MixColors();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            MixColors();
        }

        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_SYSCOMMAND)
            {
                switch (msg.WParam.ToInt32())
                {
                    case ITEM1:
                        MessageBox.Show("RoleColorGen is a small utility that allows you to generate rainbow like role colors for your Discord server. Simply choose 2 colors, set the number of the roles, and bum!","What's this?", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    case ITEM2:
                        var ver = Assembly.GetExecutingAssembly().GetName().Version;
                        MessageBox.Show("RoleColorGen v" + ver.Major + "." + ver.Minor + " by Mors. Greetings from 2018.","About", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    default:
                        break;
                }
            }
            base.WndProc(ref msg);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IntPtr MenuHandle = GetSystemMenu(this.Handle, false);
            InsertMenu(MenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(MenuHandle, 6, MF_BYPOSITION, ITEM1, "What's this?");
            InsertMenu(MenuHandle, 7, MF_BYPOSITION, ITEM2, "About");
            MixColors();
        }
    }
}
