using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace register
{
    public class CnnInfo
    {
        public string cnnStr;
    }

    public class CurrentState
    {
        public List<string> selectedTitles = new List<string>();
        public List<string> expandedNodes = new List<string>();
        public string readingTitle;

        public bool AddSelectedTitle(string path)
        {
            if (selectedTitles.IndexOf(path) != 1)
            {
                selectedTitles.Add(path);
                return true;
            }
            return false;
        }
        public bool RmSelectedTitle(string path)
        {
            return selectedTitles.Remove(path);
        }
        public bool AddColapsedNode(string path)
        {
            if (expandedNodes.IndexOf(path) != 1)
            {
                expandedNodes.Add(path);
                return true;
            }
            return false;
        }
        public bool RmColapsedNode(string path)
        {
            return expandedNodes.Remove(path);
        }
        public void Reset()
        {
            selectedTitles.Clear();
            expandedNodes.Clear();
        }
    }

    [DataContract(Name = "config")]
    public class ConfigMng
    {
        [DataMember(Name = "cnnInfo")]
        public CnnInfo m_cnnInfo;
        [DataMember(Name = "curSts")]
        public CurrentState m_curSts;
        [DataMember(Name = "wndSize")]
        public Size m_wndSize;
        [DataMember(Name = "wndPos")]
        public Point m_wndPos;
        [DataMember(Name = "srchWndSize")]
        public Size m_srchWndSize;
        [DataMember(Name = "srchWndPos")]
        public Point m_srchWndPos;
        [DataMember(Name = "srchMaxResult")]
        public int m_srchMaxRes;
        [DataMember(Name = "srchMaxDistance")]
        public int m_srchMaxD;

        [DataMember(Name = "fontFamily")]
        public string m_fontFamily;
        [DataMember(Name = "fontSize")]
        public float m_fontSize;



        //public lContentProvider m_content;

        //static ConfigMng m_instance;
        //static string m_cfgPath = @"..\..\..\config.xml";

        ConfigMng()
        {
            m_cnnInfo = new CnnInfo();
            m_curSts = new CurrentState();
            m_fontFamily = "Arial";
            m_fontSize = 12;
            m_srchMaxRes = 1000;
            m_srchMaxD = 100;
        }

        //static XmlObjectSerializer createSerializer()
        //{
        //    Type[] knownTypes = new Type[] {
        //        typeof(CnnInfo),
        //        typeof(CurrentState),
        //        };

        //    var settings = new DataContractJsonSerializerSettings
        //    {
        //        IgnoreExtensionDataObject = true,
        //        EmitTypeInformation = EmitTypeInformation.AsNeeded,
        //        KnownTypes = knownTypes
        //    };
        //    return new DataContractJsonSerializer(typeof(ConfigMng), settings);
        //}

        //public static ConfigMng getInstance()
        //{
        //    string cfgPath = m_cfgPath;
        //    if (m_instance == null)
        //    {
        //        XmlObjectSerializer sz = createSerializer();
        //        if (File.Exists(cfgPath))
        //        {
        //            XmlReader xrd = XmlReader.Create(cfgPath);
        //            xrd.Read();
        //            xrd.ReadToFollowing("config");
        //            var obj = sz.ReadObject(xrd, false);
        //            xrd.Close();
        //            m_instance = (ConfigMng)obj;
        //            if (m_instance.m_curSts == null)
        //            {
        //                m_instance.m_curSts = new CurrentState();
        //            }
        //            if (m_instance.m_cnnInfo == null)
        //            {
        //                m_instance.m_cnnInfo = new CnnInfo();
        //            }

        //            //check cnnstr
        //            try
        //            {
        //                string cnnStr = m_instance.m_cnnInfo.cnnStr;
        //                var cnn = new OleDbConnection(cnnStr);
        //                cnn.Open();
        //                cnn.Close();
        //            }
        //            catch
        //            {
        //                m_instance.m_cnnInfo.cnnStr = null;
        //            }

        //            //chk srch setting
        //            if (m_instance.m_srchMaxRes == 0) { m_instance.m_srchMaxRes = 1000; }
        //            if (m_instance.m_srchMaxD == 0) { m_instance.m_srchMaxD = 1000; }
        //        }
        //        else
        //        {
        //            m_instance = new ConfigMng();
        //        }

        //        m_instance.m_content = lOleDbContentProvider.getInstance(null);
        //    }
            
        //    return m_instance;
        //}
        //public void SaveConfig()
        //{
        //    var sz = createSerializer();
        //    XmlWriterSettings settings = new XmlWriterSettings();
        //    settings.Indent = true;
        //    settings.IndentChars = "\t";
        //    settings.Encoding = Encoding.Unicode;

        //    XmlWriter xwriter;
        //    xwriter = XmlWriter.Create(m_cfgPath, settings);
        //    xwriter.WriteStartElement("config");
        //    sz.WriteObjectContent(xwriter, this);
        //    xwriter.WriteEndElement();
        //    xwriter.Close();
        //}

        public static string FindTmpl(string tmpl)
        {
            var path = Environment.CurrentDirectory;
            while (path != null)
            {
                if (File.Exists(path + "\\" + tmpl)) {
                    return (path + "\\" + tmpl);
                }
                path = Path.GetDirectoryName(path);
            }
            return null;
        }

        class MyCfgItem
        {
            public string key;
            public string value;
        }
        static List<MyCfgItem> s_cfgItems;
        public static object CfgRead(string key)
        {
            try
            {
                if (s_cfgItems == null)
                {
                    string path = FindTmpl("cfg.json");
                    s_cfgItems = JsonConvert.DeserializeObject<List<MyCfgItem>>(File.ReadAllText(path));
                }
                var obj = s_cfgItems.Find(x=>x.key == key);
                if (obj != null)
                {
                    return obj.value;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        public static void CfgWrite(string key, string value)
        {
            string path = FindTmpl("cfg.json");
            if (s_cfgItems == null)
            {
                if (path == null)
                {
                    path = Directory.GetCurrentDirectory() + "\\cfg.json";
                    s_cfgItems = new List<MyCfgItem>();
                }
                else
                {
                    s_cfgItems = JsonConvert.DeserializeObject<List<MyCfgItem>>(File.ReadAllText(path));
                }
            }
            var obj = s_cfgItems.Find(x => x.key == key);
            if (obj == null)
            {
                obj = new MyCfgItem() { key = key };
                s_cfgItems.Add(obj);
            }
            obj.value = value;
            File.WriteAllText(path,JsonConvert.SerializeObject(s_cfgItems));
        }
        public static Font GetFont()
        {
            Font f = (Font)CfgRead("font");
            return (f != null) ? f : new Font("Arial", 10);
        }
        public static Button CrtButton()
        {
            var btn = new Button();
            btn.Font = GetFont();
            return btn;
        }
        public static Label CrtLabel()
        {
            var ctrl = new Label();
            ctrl.Font = GetFont();
            return ctrl;
        }
        public static CheckedListBox CrtCheckedListBox()
        {
            var ctrl = new CheckedListBox();
            ctrl.Font = GetFont();
            return ctrl;
        }
        public static TextBox CrtTextBox()
        {
            var ctrl = new TextBox();
            ctrl.Font = GetFont();
            return ctrl;
        }
        public static CheckBox CrtCheckBox()
        {
            var ctrl = new CheckBox();
            ctrl.Font = GetFont();
            return ctrl;
        }
        public static ComboBox CrtComboBox()
        {
            var ctrl = new ComboBox();
            ctrl.Font = GetFont();
            return ctrl;
        }
        public static DataGridView CrtDGV()
        {
            var ctrl = new DataGridView();
            ctrl.Font = GetFont();
            return ctrl;
        }
        public static MenuStrip CrtMenuStrip()
        {
            var ctrl = new MenuStrip();
            ctrl.Font = GetFont();
            return ctrl;
        }
        public static ToolStripMenuItem CrtStripMI()
        {
            var ctrl = new ToolStripMenuItem();
            ctrl.Font = GetFont();
            return ctrl;
        }

        public static string GetDateFormat() { return "yyyy-MM-dd"; }
        public static string GetDisplayDateFormat() { return "dd/MM/yyyy"; }
        public static bool parseDisplayDate(string txt, out DateTime dt) {
            //txt = "dd/MM/yyyy"
            bool ret = false;
            dt = DateTime.Now;
            do
            {
                var arr = txt.Split('/');
                if (arr.Length != 3) break;
                //year 1-9999, month 1-12, day
                int y, m, d;
                if (!int.TryParse(arr[2], out y)) break;
                if (!int.TryParse(arr[1], out m)) break;
                if (!int.TryParse(arr[0], out d)) break;
                try
                {
                    dt = new DateTime(y, m, d);
                    ret = true;
                }
                catch
                {
                    Debug.Assert(false, "invalid date string");
                }
            } while (false);
            return ret;
        }
    }
}
