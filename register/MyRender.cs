using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace register
{
    public class MyLine
    {
        public string text;
        public bool bold;
        public int leftIndent;
    }
    class MyRender
    {
        public static void PrintRtb(RichTextBox rtb, List<MyLine> lines)
        {
            rtb.Clear();
            var old = rtb.Font;
            foreach(MyLine line in lines)
            {
                if (line.bold)
                {
                    rtb.SelectionFont = new Font(old, FontStyle.Bold);
                }
                rtb.SelectedText = line.text+"\n";
            }
        }
    }
}
