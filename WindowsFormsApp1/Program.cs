//#define use_gecko
//#define use_chromium
//#define use_rtb

using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
