//#define use_gecko
//#define use_chromium
//#define use_rtb

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            testCommon();
#if use_rtb
#elif use_gecko
            var fxLib = IntPtr.Size == 8 ? "Firefox64" : "Firefox86";
            Gecko.Xpcom.Initialize(fxLib);
#elif use_chromium
            var settings = new CefSharp.WinForms.CefSettings();
            CefSharp.Cef.Initialize(settings);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void testCommon()
        {
            string msg = "@Ánh Duyên Hạnh Bảo  c đã nhắn cho a Tú, nhưng có vẻ anh ý ko hoan hỷ, nên là c nghĩ là k nên can thiệp nhiều vào đời tư của anh ấy. còn về câu hỏi, c sẽ chuyển sang BNL rồi trả lời sau";
            int len = 50;
            var reg = new Regex("[\\s,\\.]");
            var i = Math.Min(len, msg.Length);
            var m = reg.Matches(msg.Substring(0, len - 1));
            int max_value = 0;
            int best_len = len;
            foreach(Match mi in m)
            {
                int ival = mi.Index;
                switch(mi.Value)
                {
                    case ".":
                        ival += 10000;
                        break;
                    case ",":
                        ival += 1000;
                        break;
                }
                if (ival > max_value)
                {
                    max_value = ival;
                    best_len = mi.Index;
                }
            }
            string ret = msg.Substring(0, best_len);
        }
    }
}
