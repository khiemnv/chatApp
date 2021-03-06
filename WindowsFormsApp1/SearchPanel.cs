﻿//#define use_chromium
//#define use_gecko
//#define use_lstview

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WindowsFormsApp1
{

    public class SearchPanel
    {
        string cnnStr;
        public TableLayoutPanel m_tblLayout;
#if use_chromium
        class JsHandler
        {
            public EventHandler<string> OnTitleSelected;
            public void HandleJsCall(string arg)
            {
                //MessageBox.Show($"Value Provided From JavaScript: {arg.ToString()}", "C# Method Called");
                OnTitleSelected?.Invoke(this, arg);
            }
        }
        protected CefSharp.WinForms.ChromiumWebBrowser m_wb;
#elif use_gecko
        protected Gecko.GeckoWebBrowser m_wb;
#else
        ListView m_lstV;
#endif
        ToolStripStatusLabel m_sts;
        TrackBar m_trck;
        public Button m_acceptBtn;
        public SearchPanel(string cnnStr, bool bldSrchDb = false)
        {
            this.cnnStr = cnnStr;

            m_tblLayout = new TableLayoutPanel();
            m_tblLayout.Dock = DockStyle.Fill;

            var edt = new TextBox();
            edt.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            var btn = new Button();
            btn.Text = "Search";
            btn.AutoSize = true;
            btn.Anchor = AnchorStyles.Right;
            if (!bldSrchDb)
            {
                btn.Click += (s, e) =>
                {
                    OnSearch(edt.Text);
                };
            }
            else
            {
                btn.Click += (s, e) =>
                {
                    BuildSearchDb();
                };
            }
            m_acceptBtn = btn;

#if use_chromium
            var lst = new CefSharp.WinForms.ChromiumWebBrowser("");
            lst.Dock = DockStyle.Fill;
            JsHandler jsHandler = new JsHandler();
            jsHandler.OnTitleSelected += (s, e) =>
            {
                Debug.WriteLine("OnSelectTitle {0}", e);
                OnSelectTitle?.Invoke(s, Convert.ToUInt64(e));
            };
            lst.JavascriptObjectRepository.Register("jsHandler", jsHandler, true);
            m_wb = lst;
#elif use_gecko
            var lst = new Gecko.GeckoWebBrowser();
            lst.Dock = DockStyle.Fill;
            m_wb = lst;
#else
            var lst = new ListView();
            lst.View = View.Details;
            lst.FullRowSelect = true;
            lst.GridLines = true;
            lst.Dock = DockStyle.Fill;
            lst.ItemSelectionChanged += (s, e) =>
            {
                //UInt64 titleId = 0;
                foreach (ListViewItem li in lst.SelectedItems)
                {
                    //UInt64.TryParse(li.SubItems[1].Text, out titleId);
                    //OnSelectTitle(titleId);
                    OnSelectTitle?.Invoke(s, Convert.ToUInt64(li.SubItems[1].Tag));
                    break;
                }
            };
            m_lstV = lst;
#endif

            //var sts = new StatusBar
            //{
            //    Dock = DockStyle.Bottom,
            //    ShowPanels = false
            //};
            //m_sts = sts;

            //m_trck = new TrackBar();
            //m_trck.Maximum = 1000;
            //m_trck.Minimum = 500;
            //m_trck.Height = 30;
            //var flow = new FlowLayoutPanel();
            //flow.Dock = DockStyle.Fill;
            //flow.FlowDirection = FlowDirection.RightToLeft;
            //flow.Height = 30;
            //m_sts.Height = 50;
            //m_sts.Width = 300;
            //flow.Controls.Add(m_trck);
            //flow.Controls.Add(m_sts);

            int iRow = 0;
            m_tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            m_tblLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            m_tblLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            //m_tblLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            m_tblLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            m_tblLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            m_tblLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            m_tblLayout.Controls.Add(edt, 0, iRow);
            m_tblLayout.Controls.Add(btn, 1, iRow++);
            m_tblLayout.Controls.Add(lst, 0, iRow++);
            m_tblLayout.SetColumnSpan(lst, 2);
            //m_tblLayout.Controls.Add(sts, 0, iRow);
            //m_tblLayout.Controls.Add(m_trck, 1, iRow);
            m_tblLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;

            var statusStrip1 = new StatusStrip();
            statusStrip1.LayoutStyle = ToolStripLayoutStyle.Table;
            var toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel1.Anchor = AnchorStyles.Right;
            toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            var ToolStripTraceBarItem = new ToolStripTraceBarItem();
            ToolStripTraceBarItem.Anchor = AnchorStyles.Left;
            statusStrip1.Items.Add(ToolStripTraceBarItem);
            statusStrip1.Items.Add(toolStripStatusLabel1);
            m_tblLayout.Controls.Add(statusStrip1);
            m_tblLayout.SetColumnSpan(statusStrip1, 2);
            m_trck = (TrackBar)ToolStripTraceBarItem.Control;
            m_sts = toolStripStatusLabel1;
            m_trck.Maximum = 1000;
            m_trck.Minimum = 500;
            var cfg = ConfigMng.getInstance();
            m_trck.Value = cfg.m_srchMaxD;
            m_trck.ValueChanged += M_trck_ValueChanged;
            toolStripStatusLabel1.Text = "status";
        }

        private void M_trck_ValueChanged(object sender, EventArgs e)
        {
            var cfg = ConfigMng.getInstance();
            cfg.m_srchMaxD = m_trck.Value;
        }

        /// <summary>
        /// Adds trackbar to toolstrip stuff
        /// </summary>
        [
        ToolStripItemDesignerAvailability
            (ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)
        ]

        public class ToolStripTraceBarItem : ToolStripControlHost
        {
            public ToolStripTraceBarItem() : base(new TrackBar())
            {
            }
        }

        public event EventHandler<UInt64> OnSelectTitle;
        private void SelectTitle(int titleId)
        {

        }

        private void BuildSearchDb()
        {
            var srch = new SearchContent(cnnStr);
            srch.BuildSearchDb();
            srch.Close();
            return;
        }

        private void OnSearch(string txt)
        {
            if (txt == "") return;

            var begin = Environment.TickCount;
            var srch = new SearchContent(cnnStr);
            var res = srch.Find(txt);
            ShowSearchRes(res);
            m_sts.Text = string.Format("elapsed time: {0}(ms)", Environment.TickCount - begin);
            srch.Close();
        }

        public class MarkedTxt
        {
            public string txt;
            public int style;   //1->marked 
        }
        public class MarkedTitle
        {
            public string title;
            public UInt64 titleId;
            public List<MarkedTxt> txtLst;
        }


        List<MarkedTitle> convJsData(SrchResult res)
        {
            var tmr = new MyTimer("convJsData");
            var lst = new List<MarkedTitle>();
            foreach (var rec in res.recs)
            {
                var mt = new MarkedTitle();
                mt.titleId = rec.detail[0].titleId;
                mt.title = res.titles.Find((t) => t.ID == mt.titleId).title;
                //<par,[point]>
                var tDict = new Dictionary<UInt64, List<int[]>>();
                var parLst = new List<MyParagraph>();
                foreach (var w in rec.detail)
                {
                    var par = res.paragraphs.Find(p => p.ID == w.parId);
                    if (tDict.ContainsKey(par.ID))
                    {
                        tDict[par.ID].Add(new int[] { w.pos, w.content.Length });
                        tDict[par.ID].Sort((v1, v2) => v1[0] - v2[0]);
                    }
                    else
                    {
                        tDict[par.ID] = new List<int[]>
                        {
                            new int[] { w.pos, w.content.Length }
                        };
                        parLst.Add(par);
                    }
                }
                //split
                mt.txtLst = new List<MarkedTxt>();
                foreach (var p in tDict)
                {
                    string txt = parLst.Find(par => par.ID == p.Key).content;
                    //find before [.]
                    //find after [.]
                    var points = p.Value;
                    var b = txt.LastIndexOf('.', points[0][0]);
                    b = b == -1 ? 0 : b + 1;
                    var e = txt.IndexOf('.', points[points.Count - 1][0]);
                    e = e == -1 ? txt.Length - 1 : e;
                    int cur = b;
                    int n;
                    foreach (var point in p.Value)
                    {
                        // txt before [point]
                        var offset = point[0];
                        var len = point[1];

                        if (offset > cur)
                        {
                            n = offset - cur;
                            if (n > 0)
                            {
                                n = adjBegin(txt, offset, Math.Min(n, 50));
                            }

                            mt.txtLst.Add(new MarkedTxt()
                            {
                                txt = txt.Substring(offset - n, n)
                            });
                        }
                        mt.txtLst.Add(new MarkedTxt()
                        {
                            txt = txt.Substring(offset, len) + " ",
                            style = 1
                        });
                        cur = offset + len;
                    }
                    //[point] txt after 
                    n = e - cur;
                    if (n > 0)
                    {
                        n = adjEnd(txt, cur, Math.Min(n, 50));
                        mt.txtLst.Add(new MarkedTxt()
                        {
                            txt = txt.Substring(cur, n) + "\v"
                        });
                    }
                }

                lst.Add(mt);
            }
            return lst;
        }

        int adjBegin(string txt, int offset, int n)
        {
            //var pos = txt.IndexOf(',', offset - n, n);
            //if (pos == -1) { pos = txt.IndexOf(' ', offset - n, n); }
            //if (pos != -1) { n = offset - pos - 1; }

            var reg = new Regex("\\v\\s*|\"\\s*|,\\s*|\\s+");
            var mc = reg.Matches(txt.Substring(offset - n, n));
            var arr = new Match[4];
            for (var mi = mc.Count - 1; mi >= 0; mi--)
            {
                var m = mc[mi];
                if (m.Value[0] == '\v') { arr[0] = m; }
                else if (m.Value[0] == '"') { arr[1] = m; }
                else if (m.Value[0] == ',') { arr[2] = m; }
                else { arr[3] = m; }
            }
            foreach (var m in arr)
            {
                if (m != null) { n -= m.Index + m.Length; break; }
            }
            return n;
        }
        int adjEnd(string txt, int cur, int n)
        {
            //var pos = txt.LastIndexOf(',', cur + n, n);
            //if (pos == -1) { pos = txt.LastIndexOf(' ', cur + n, n); }
            //if (pos != -1) { n = pos - cur; }

            var reg = new Regex("\\v\\s*|\"\\s*|,\\s*|\\s+");
            var mc = reg.Matches(txt.Substring(cur, n));
            var arr = new int[4] { -1, -1, -1, -1 };
            for (var mi = 0; mi < mc.Count; mi++)
            {
                var m = mc[mi];
                if (m.Value[0] == '\v') { arr[0] = m.Index; }
                else if (m.Value[0] == '"') { arr[1] = m.Index; }
                else if (m.Value[0] == ',') { arr[2] = m.Index; }
                else { arr[3] = m.Index; }
            }
            foreach (var m in arr)
            {
                if (m >= 0) { n = m; break; }
            }
            return n;
        }

        string genJsTxt(List<MarkedTitle> lst)
        {
            Type[] knownTypes = new Type[] {
                    typeof(MarkedTxt),
                    typeof(MarkedTitle),
                };

            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings
            {
                IgnoreExtensionDataObject = true,
                EmitTypeInformation = EmitTypeInformation.AsNeeded,
                KnownTypes = knownTypes
            };
            var x = new DataContractJsonSerializer(lst.GetType(), settings);

            var mem = new MemoryStream();
            x.WriteObject(mem, lst);
            StreamReader sr = new StreamReader(mem);
            mem.Position = 0;
            string myStr = sr.ReadToEnd();
            sr.Dispose();
            mem.Dispose();
            return myStr;
        }
        private string genHtmlTxt(string jsTxt)
        {

#if use_chromium
            var path = ConfigMng.findTmpl("search.html");
#elif use_gecko
            var path = ConfigMng.findTmpl("search_gecko.html");
#endif
            var txt = File.ReadAllText(path);
            var htmlTxt = txt.Replace("var jsTxt = null", "var jsTxt = " + jsTxt);
            return htmlTxt;
        }
        void ShowSearchRes(SrchResult res)
        {
            var data = convJsData(res);
            var jsTxt = genJsTxt(data);
            string htmlTxt = genHtmlTxt(jsTxt);
            string filename = string.Format(@"{0}{1}", Path.GetTempPath(), "page.htm");
            File.WriteAllText(filename, htmlTxt);
#if use_chromium
            //create json data [title, texthtml]
            m_wb.Load(filename);
#elif use_gecko
            m_wb.Navigate(filename);
            m_wb.AddMessageEventListener("openTitle", OnOpenTitle);
#else
            var listView1 = m_lstV;
            listView1.Clear();
            listView1.Columns.Add("content");
            listView1.Columns.Add("title");
            listView1.Columns.Add("paragraph");
            listView1.Columns.Add("pos");
            listView1.Columns.Add("diff");
            listView1.Columns.Add("detail");
            listView1.GridLines = true;
            foreach (var rec in res.recs)
            {
                //var tempTxt = string.Format("{0} {1} {2} {3}", 
                //    string.Join(" ", rec.path.Select((v)=>v.content).ToArray()),
                //    rec.d, rec.w.titleId, rec.w.parId);
                var li = listView1.Items.Add(string.Join(" ", rec.detail.Select((v) => v.content)));
                var titleId = rec.detail[0].titleId;
                var sub = li.SubItems.Add(res.titles.Find((t) => t.ID == titleId).zTitle);
                sub.Tag = rec.detail[0].titleId;
                li.SubItems.Add(string.Join(" ", rec.detail.Select(v => v.parId)));
                li.SubItems.Add(string.Join(" ", rec.detail.Select(v => v.pos)));
                li.SubItems.Add(rec.d.ToString());
                var w = rec.detail[0];
                var parId = w.parId;
                var parTxt = res.paragraphs.Find((t) => t.ID == parId).content;
                var txt = parTxt.Substring(0, w.pos) + "[" + w.content + "]" + parTxt.Substring(w.pos + w.content.Length);
                li.SubItems.Add(parTxt);
            }
#endif
        }
        private void OnOpenTitle(string s)
        {
            OnSelectTitle?.Invoke(m_wb, Convert.ToUInt64(s));
        }
    }
}
