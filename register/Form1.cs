using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace register
{
    public partial class Form1 : Form
    {
        MyTree m_treeMng;
        lOleDbContentProvider m_cp;
        List<MyUser> p_users;
        List<MyUser> m_users
        {
            get { return p_users; }
            set
            {
                p_users = value;
                OnUpdateUsers();
            }
        }

        private void OnUpdateUsers()
        {
            m_idx = null;
        }

        List<MyProgram> m_programs;
        List<MyGroup> m_groups;
        Indexer m_idx;
        string m_db;
        string m_cnnStr;
        Dictionary<string, MyNode> m_nodes;  //<zUserFb><MyNode>

        MyTree m_progTreeMng;
        public Form1()
        {
            InitializeComponent();
            Load += onLoad;
        }

        private void onLoad(object sender, EventArgs e)
        {
            getCnnStr();

            m_cp = new lOleDbContentProvider();
            m_cp.initCnn(m_cnnStr);


            userCmb.KeyUp += UserCmb_KeyUp; ;
            //textBox1.TextChanged += TextBox1_TextChanged;
            //textBox1.KeyDown += TextBox1_KeyDown;

            progCmb.DropDownStyle = ComboBoxStyle.DropDownList;
            progCmb.SelectedIndexChanged += ProgCmb_SelectedIndexChanged;

            treeView1.NodeMouseClick += TreeView1_NodeMouseClick;
            addBtn.Click += AddBtn_Click;
            treeView1.MouseDoubleClick += TreeView1_MouseDoubleClick;

            splitContainer1.Dock = DockStyle.Fill;
            progCmb.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            treeView1.Dock = DockStyle.Fill;

            richTextBox1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;

            splitContainer2.Dock = DockStyle.Fill;

            //init user tree
            m_treeMng = new MyTree();
            m_treeMng.m_tree = treeView1;
            m_treeMng.m_treeStyle = MyTree.TreeStyle.check;
            m_treeMng.InitTree();

            //ToolStripMenuItem mi = new ToolStripMenuItem("refresh programs");
            //menuStrip1.Items.Add(mi);
            //mi.Click += (s,ev) => {RefreshDb(); };

            tagLstBx.Click += TagLstBx_Click;
            tagLstBx.DoubleClick += TagLstBx_DoubleClick;
            tagLstBx.ItemCheck += TagLstBx_SelectedIndexChanged;

            RefreshTagLst();

            //int prog tree
            m_progTreeMng = new MyTree();
            m_progTreeMng.m_tree = programTV;
            m_progTreeMng.m_treeStyle = MyTree.TreeStyle.check;
            m_progTreeMng.InitTree();
            var programTV_menu = new ContextMenuStrip();
            programTV.ContextMenuStrip = programTV_menu;
            var refreshProgMi = programTV_menu.Items.Add("refresh programs");
            refreshProgMi.Click += (s,ev) => {RefreshPrograms(); };

            var statiProgMi = programTV_menu.Items.Add("static");
            statiProgMi.Click += StaticProgMi_Click;
            programTV.NodeMouseClick += ProgramTV_NodeMouseClick;
            var ProgMiCopy = programTV_menu.Items.Add("copy");
            ProgMiCopy.Click += ProgMiCopy_Click;
            var newProgMi = programTV.ContextMenuStrip.Items.Add("new program");
            newProgMi.Click += OnCrtNewProg;

            treeView1.ContextMenuStrip = new ContextMenuStrip();
            var regmi = treeView1.ContextMenuStrip.Items.Add("refresh");
            regmi.Click += RegMiRefresh_Click;

            //load data
            RefreshPrograms();
            
            m_users = getUsers();
            m_groups = m_cp.GetAllGroups();

            var checkOutMid = fileToolStripMenuItem.DropDownItems.Add("CheckOut");
            checkOutMid.Click += OnCheckOut;
            var checkInMi = fileToolStripMenuItem.DropDownItems.Add("CheckIn");
            checkInMi.Click += OnCheckIn;

            //default add button
            this.AcceptButton = addBtn;
        }

        private void OnCrtNewProg(object sender, EventArgs e)
        {
            //show input dlg
            var inputDlg = new inputProg();
            inputDlg.path = "";
            if (programTV.SelectedNode != null)
            {
                inputDlg.path = programTV.SelectedNode.FullPath.Replace("\\","/");
            }
            inputDlg.name = string.Format("new prog {0}-{1}", DateTime.Now.Day,DateTime.Now.Month);
            inputDlg.date = DateTime.Now;
            var ret = inputDlg.ShowDialog();
            if (ret == DialogResult.OK)
            {
                //check duplicate
                var newProg = new MyProgram()
                {
                    zPath = inputDlg.path,
                    zName = inputDlg.name,
                    startDate = inputDlg.date
                };
                if (! m_progTreeMng.PathExist(newProg.zPath + "/" + newProg.zName))
                {
                    //add new prog
                    var bOK = m_cp.AddProg(newProg);
                    if (bOK)
                    {
                        //add to log
                        m_cp.LogsSaveAdd(newProg);

                        //update gui
                        var title = Prog2Title(newProg);
                        m_programs.Add(newProg);
                        m_progTreeMng.Add(title);

                        //update combo
                        progCmb.Items.Insert(0, newProg.zName);
                        progCmb.SelectedIndex = 0;
                    }
                }
            }
        }

        private async void OnCheckIn(object sender, EventArgs e)
        {
            CheckInAsync();
        }
        private async void OnCheckOut(object sender, EventArgs e)
        {
            await CheckOutAsync();
        }
        private async Task CheckOutAsync()
        {

            ContentSync sync = new ContentSync();
            List<UInt64>ids = await sync.GetIdsAsync("logs");
            
            
            //check out
            List<UInt64>oldIds  = m_cp.GetIds("logs");
            List<UInt64> newIds;
            newIds = ids.FindAll(x=>!oldIds.Contains(x));
            List<object> objs = await sync.GetObjAsync("logs", newIds);
            List<MyLog> checkOutLst = new List<MyLog>();
            foreach(var obj in objs)
            {
                checkOutLst.Add(obj as MyLog);
            }
            m_cp.AddLogs(checkOutLst);
        }
        private async Task CheckInAsync()
        {
            ContentSync sync = new ContentSync();
            //List<UInt64> oldIds = null;
            //var task1 =  sync.GetIdsAsync("logs");
            //Task.Run(async ()=> oldIds = await sync.GetIdsAsync("logs"));
            //task1.RunSynchronously();
            //task1.Wait();
            //List<UInt64> oldIds = task1.Result;
            List<UInt64> oldIds = await sync.GetIdsAsync("logs");
            List<UInt64> ids = m_cp.GetIds("logs");
            //check in

            List<UInt64> newIds = ids.FindAll(x => !oldIds.Contains(x));

            List<MyLog> checkInLst = m_cp.GetLogs(newIds);
            await sync.PutLogAsync("logs", checkInLst);
            //var task2 = sync.PutLogAsync("logs", checkInLst);
            //task2.RunSynchronously();
            //task2.Wait();
        }

        private void RegMiRefresh_Click(object sender, EventArgs e)
        {
            var zProg = progCmb.Text;
            if (zProg != "")
            {
                RefreshRegTree();
            }
        }

        private void ProgMiCopy_Click(object sender, EventArgs e)
        {
            if (programTV.SelectedNode == null)
            {
                return;
            }
            string path = (programTV.SelectedNode.Tag) as string;
            MyTree.MyTitle title = m_progTreeMng.GetTitle(path);
            if (title != null)
            {
                Clipboard.SetText(title.title);
            }
        }

        private void ProgramTV_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string path = (e.Node.Tag) as string;
            MyTree.MyTitle title = m_progTreeMng.GetTitle(path);
            if (title != null)
            {
                progCmb.Text = title.title;
            }
        }

        private void StaticProgMi_Click(object sender, EventArgs e)
        {
            List<MyTree.MyTitle> lst = m_progTreeMng.GetSelectedTitle();
            List<MyReg> regLst = new List<MyReg>();
            foreach (MyTree.MyTitle title in lst)
            {
                var progID = title.ID;
                var regs = m_cp.GetRegs(progID);
                regLst.AddRange(regs);
            }
            CrtAndRenderRpt(regLst);
        }

        private void CrtAndRenderRpt(List<MyReg> regLst)
        {
            Dictionary <UInt64,MyUser> uDict = new Dictionary<ulong, MyUser>();
            foreach (MyUser user in m_users)
            {
                uDict.Add(user.ID,user);
            }

            int maxOpt = 0;
            Dictionary<int, List<MyReg>> tDict = new Dictionary<int, List<MyReg>>();
            foreach (MyReg reg in regLst)
            {
                var uID = reg.userID;
                if (!uDict.ContainsKey(uID)) { continue;}

                var nOpt = reg.nStatus;
                var zOpt = reg.zNote;
                if (nOpt > maxOpt) { maxOpt = nOpt; }
                if (tDict.ContainsKey(nOpt))
                {
                }
                else
                {
                    tDict[nOpt] = new List<MyReg>();
                }
                tDict[nOpt].Add(reg);
            }


            List<MyLine> lines = new List<MyLine>();
            lines.Add(new MyLine
            {
                text = string.Format("{0}\t{1}", "option", "count"),
                bold = true
            });
            for (int i = 0; i <= maxOpt; i++)
            {
                if (tDict.ContainsKey(i))
                {
                    //display
                    lines.Add(new MyLine
                    {
                        text = string.Format("{0}\t{1}", i, tDict[i].Count)
                    });
                }
            }
            MyRender.PrintRtb(richTextBox1, lines);
        }

        private void ProgramTV_DoubleClick(object sender, EventArgs e)
        {

        }

        private void UpdateProgTV()
        {
            //throw new NotImplementedException();
            var titleLst = BuildProgramTree();
            m_progTreeMng.Clear();
            m_progTreeMng.AddTitles(titleLst);
        }

        private List<MyTree.MyTitle> BuildProgramTree()
        {
            var progs = m_programs;
            List<MyTree.MyTitle> titleLst = new List<MyTree.MyTitle>();
            foreach (MyProgram prog in m_programs)
            {
                MyTree.MyTitle title = Prog2Title(prog);
                titleLst.Add(title);
            }
            return titleLst;
        }
        private MyTree.MyTitle Prog2Title(MyProgram prog)
        {
            MyTree.MyTitle title = new MyTree.MyTitle();
            title.title = prog.zName;
            title.path = string.Format("{0}/{1}", prog.zPath, prog.zName);
            title.ID = prog.ID;
            return title;
        }

        private void TreeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //throw new NotImplementedException();
            //refresh user list
            var zProg = progCmb.Text;
            if (zProg != "")
            {
                RefreshRegTree();
            }
        }

        private void TagLstBx_DoubleClick(object sender, EventArgs e)
        {
            RefreshTagLst();
        }
        void RefreshTagLst()
        {
            tagLstBx.Items.Clear();
            var tags = m_cp.GetAllTags();
            foreach (var tag in tags)
            {
                tagLstBx.Items.Add(tag);
            }
        }
        private void TagLstBx_SelectedIndexChanged(object sender, ItemCheckEventArgs e)
        {
            List<string> tags = new List<string>();
            //filter user by tag
            for (int i = 0; i < tagLstBx.Items.Count; i++)
            {
                if (i == e.Index)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        tags.Add(tagLstBx.Items[i].ToString());
                    }
                }
                else if (tagLstBx.GetItemChecked(i))
                {
                    tags.Add(tagLstBx.Items[i].ToString());
                }
            }
            if (tags.Count == 0)
            {
                //chinh thuc
                m_users = m_cp.GetAllUsers();
                var zProg = progCmb.Text;
                if (zProg != "")
                {
                    RefreshRegTree();
                }
            }
            else
            {
                m_users = m_cp.GetAllUsers(tags);
                var zProg = progCmb.Text;
                if (zProg != "")
                {
                    RefreshRegTree();
                }
            }
        }

        private void TagLstBx_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void RefreshPrograms()
        {
            //m_cp.clear();   //clear content provider
            List<MyProgram> progs = getProg();
            m_programs = progs;
            UpdateProgramCmb();
            UpdateProgTV();
        }
        void UpdateProgramCmb()
        {
            progCmb.Items.Clear();
            progCmb.Items.AddRange(m_programs.Select(i => i.zName).ToArray());
        }

        bool ParseOpt(string txt, out string zOpt, out int nOpt)
        {
            Regex reg = new Regex(@"^(\d+)( \((.*)\))?");
            var m = reg.Match(txt);
            if (!m.Success)
            {
                MessageBox.Show("Invalid option");
                zOpt = null;
                nOpt = 0;
                return false;
            }
            nOpt = int.Parse(m.Groups[1].Value);
            zOpt = m.Groups[3].Value;
            return true;
        }
        private void AddBtn_Click(object sender, EventArgs e)
        {
            int nOpt;
            string zOpt;
            var txt = optTxt.Text;
            bool v = ParseOpt(txt, out zOpt, out nOpt);
            if (!v) { return; }

            var zUserFb = userCmb.Text;
            MyUser user = m_users.Find(x => x.zUserFb == zUserFb);
            var zProg = progCmb.Text;
            MyProgram prog = m_programs.Find(x => x.zName == zProg);
            MyReg myReg = new MyReg();
            myReg.userID = user.ID;
            myReg.programID = prog.ID;
            myReg.nStatus = nOpt;
            myReg.zNote = zOpt;
            int ret = m_cp.AddUpdateReg(myReg);

            //add to ricktext
            richTextBox1.SelectedText = user.zUserFb + string.Format("({0}-{1})\n",nOpt, zOpt);
            richTextBox1.ScrollToCaret();

            MyTree.MyTitle title = new MyTree.MyTitle();
            MyGroup grp = m_groups.Find(x => x.nGroup == user.nGroup);
            title.title = user.zUserFb;
            title.content = string.Format("{0} ({1})", nOpt, zOpt);
            title.path = string.Format("register/{0}/{1}", grp.zGroup, user.zUserFb);
            var nRet = m_treeMng.Add(title);
            if (nRet == 1)
            {
                title.path = string.Format("un-register/{0}/{1}", grp.zGroup, user.zUserFb);
                m_treeMng.Remove(title);
            }
        }

        private void TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string path = (e.Node.Tag) as string;
            MyTree.MyTitle title = m_treeMng.GetTitle(path);
            if (title != null)
            {
                var zopt = title.content;
                var user = m_users.Find(x => x.zUserFb == title.title);
                userCmb.Text = title.title;
                optTxt.Text = zopt;
            }
            else
            {
                PreviewReport(e.Node);
            }
        }

        void PreviewReport(TreeNode tnode)
        {
            //string
            string path = tnode.Tag as string;
            List<MyTree.MyTitle> lst = m_treeMng.GetAllChilds(path);
            Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();
            int maxOpt = 0;
            foreach (MyTree.MyTitle title in lst)
            {
                MyUser user = m_users.Find(x => x.zUserFb == title.title);
                int nOpt = 0;
                string zOpt = "";
                if (title.content != null)
                {
                    ParseOpt(title.content, out zOpt, out nOpt);
                }
                if (dict.ContainsKey(nOpt))
                {
                    dict[nOpt].Add(user.zUserFb + " " + zOpt);
                }
                else
                {
                    dict[nOpt] = new List<string>();
                    dict[nOpt].Add(user.zUserFb + " " + zOpt);
                    maxOpt = maxOpt < nOpt ? nOpt : maxOpt;
                }
            }
            richTextBox1.Clear();
            var old = richTextBox1.Font;
            for (int nOpt = 0; nOpt <= maxOpt; nOpt++)
            {
                if (!dict.ContainsKey(nOpt)) continue;

                var titles = dict[nOpt];
                //var nOpt = p.Key;

                richTextBox1.SelectionFont = new Font(old, FontStyle.Bold);
                richTextBox1.SelectedText = string.Format("{0} ({1})\n", nOpt, titles.Count);
                richTextBox1.SelectionFont = old;
                string txt = string.Join("\n", titles);
                richTextBox1.SelectedText = txt + "\n";
            }
        }

        private void ProgCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshRegTree();
        }

        private void RefreshRegTree()
        {
            var titleLst = BuildRegTree();
            m_treeMng.Clear();
            m_treeMng.AddTitles(titleLst);
        }

        void getCnnStr()
        {
#if DEBUG
            m_db = ConfigMng.cfgRead("zDb") as string;
#else
            m_db = ConfigMng.findTmpl("PTXX_NB.accdb");
#endif
            if (m_db != null)
            {
                var cnnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=<zDb>;";
                m_cnnStr = cnnStr.Replace("<zDb>", m_db);
            }
            else
            {
                OpenDbDlg();
                #if DEBUG
                ConfigMng.cfgWrite("zDb",m_db);
                #endif
            }
        }
        bool OpenDbDlg()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select database file";
            ofd.Filter = "access files (*.accdb)|*.accdb";
            //ofd.InitialDirectory = Directory.GetCurrentDirectory();
            //ofd.Multiselect = true;
            var ret = ofd.ShowDialog();
            if (ret == DialogResult.OK)
            {
                m_db = ofd.FileName;
                var cnnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=<db>;";
                m_cnnStr = cnnStr.Replace("<db>", m_db);
                return true;
            }
            return false;
        }

        private void UserCmb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                UserCmb_TextChanged(sender, e);
            }
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            //if (m_idx == null)
            //{
            //    m_idx = new Indexer();
            //    var tLst = new List<string>();
            //    foreach(var i in m_users)
            //    {
            //        tLst.Add(i.zUserFb);
            //    }
            //    m_idx.init(tLst);
            //}
            //var res = m_idx.Find(userCmb.Text);
            //if (res != null)
            //{
            //    AutoCompleteStringCollection sourceName = new AutoCompleteStringCollection();
            //    sourceName.AddRange(res.items);
            //    textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //    textBox1.AutoCompleteSource  = AutoCompleteSource.CustomSource;
            //    textBox1.AutoCompleteCustomSource  = sourceName;
            //}
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        bool m_busy = false;
        BindingSource m_bs;
        private void UserCmb_TextChanged(object sender, EventArgs e)
        {
            if (m_busy) { return; }
            m_busy = true;
            if (m_idx == null)
            {
                m_idx = new Indexer();
                var tLst = new List<string>();
                foreach (var i in m_users)
                {
                    tLst.Add(i.zUserFb);
                }
                m_idx.init(tLst);
                m_bs = new BindingSource();
                userCmb.DataSource = m_bs;
            }
            var oldTxt = userCmb.Text;
            var res = m_idx.Find(userCmb.Text);
            if (res != null)
            {
                m_bs.DataSource = res.items;
                //userCmb.DataSource = res.items;
                userCmb.DroppedDown = true;
                Cursor.Current = Cursors.Default;
                userCmb.Text = oldTxt;
                userCmb.SelectionStart = oldTxt.Length;
                userCmb.SelectionLength = 0;

                if( res.items.Length == 1)
                {
                    userCmb.Text = res.items[0];
                    userCmb.DroppedDown = false;
                }
            }
            m_busy = false;
            //if (m_maps == null) { 
            //    m_maps = new Dictionary<string, string>();

            //    foreach (var i in userCmb.Items)
            //    {
            //        string val = i.ToString();
            //        string key = genKey(val);
            //        m_col.Add(val);
            //        m_col.Add(key);
            //        try
            //        {
            //            m_maps.Add(key, val);
            //        }
            //        catch { }
            //    }
            //userCmb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //userCmb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            //userCmb.AutoCompleteCustomSource = m_col;
            //}

        }

        private List<MyUser> getUsers()
        {
            return m_cp.GetAllUsers();
        }

        private List<MyProgram> getProg()
        {
            return m_cp.GetAllProgs();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void openDbToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, MyUser> userD;
            var content = new lOleDbContentProvider();
            ConfigMng.cfgRead("zDb");
            var zDb = ConfigMng.cfgRead("zDb") as string;
            var cnnStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=<zDb>;";
            content.initCnn(cnnStr.Replace("<zDb>", zDb));
            var userLst = content.GetAllUsers();
            userD = new Dictionary<string, MyUser>();
            foreach (MyUser tuser in userLst)
            {
                userD.Add(tuser.zFb, tuser);
            }

            var arr = optTxt.Lines;
            int s = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                string line = arr[i];
                switch (s)
                {
                    case 0:
                        if (userD.ContainsKey(line))
                        {
                            s = 1;
                        }
                        else
                        {
                            s = -1;
                        }
                        break;
                }
            }
        }

        private Dictionary<string, MyUser> getUserDict()
        {
            throw new NotImplementedException();
        }

        public class lFireBaseContentProvider
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        List<MyReg> getRegister()
        {
            var zProg = progCmb.Text;
            return getRegister(new List<string> { zProg });
        }
        List<MyReg> getRegister(List<string> progLst)
        {
            List<MyReg> regs = new List<MyReg>();
            foreach (string zProg in progLst)
            {
                var prog = m_programs.Find(x => x.zName == zProg);
                regs.AddRange(m_cp.GetRegs(prog));
            }
            return regs;
        }

        public class MyNode
        {
            public string zTitle;
            public string zPath;
            public object data;
            public List<MyNode> childs;
            public enum eNodeType
            {
                none,
                eReg,
                eUser
            };
            public eNodeType eType;
        }
        public List<MyTree.MyTitle> BuildRegTree()
        {
            var groups = m_groups;
            var regs = getRegister();

            List<MyTree.MyTitle> titleLst = new List<MyTree.MyTitle>();

            Dictionary<UInt64, MyUser> userDict = new Dictionary<ulong, MyUser>();
            foreach (MyUser user in m_users)
            {
                if (userDict.ContainsKey(user.ID))
                {

                }
                else
                {

                    userDict.Add(user.ID, user);
                }
            }

            foreach (MyReg reg in regs)
            {
                if (!userDict.ContainsKey(reg.userID))
                {
                    continue;
                }
                reg.user = userDict[reg.userID];
                MyGroup grp = groups.Find(x => x.nGroup == reg.user.nGroup);
                MyTree.MyTitle title = new MyTree.MyTitle();
                title.title = reg.user.zUserFb;
                title.path = string.Format("register/{0}/{1}", grp.zGroup, title.title);
                title.content = string.Format("{0} ({1})", reg.nStatus, reg.zNote);
                titleLst.Add(title);

                userDict.Remove(reg.userID);
            }

            foreach (MyUser user in userDict.Values)
            {
                MyGroup grp = groups.Find(x => x.nGroup == user.nGroup);
                MyTree.MyTitle title = new MyTree.MyTitle();
                title.title = user.zUserFb;
                title.path = string.Format("un-register/{0}/{1}", grp.zGroup, title.title);
                titleLst.Add(title);
            }

            return titleLst;
        }

        //tree
        //register
        //unregister
        //  group 1
        //  group 2
        void updateTree(MyNode root)
        {
            treeView1.Nodes.Clear();
            foreach (MyNode child in root.childs)
            {
                var newNode = treeView1.Nodes.Add(child.zTitle);
                newNode.Tag = child;
                addNode(newNode, child.childs);
            }
        }

        void addNode(TreeNode parent, List<MyNode> childs)
        {
            foreach (MyNode child in childs)
            {
                var newNode = parent.Nodes.Add(child.zTitle);
                newNode.Tag = child;
                if (child.childs != null)
                {
                    addNode(newNode, child.childs);
                }
            }
        }

        void moveNode(MyNode node)
        {
            MyNode root = m_nodes["root"];
            MyNode regNode = m_nodes["register"];
            MyNode unregNode = m_nodes["un-register"];
            MyReg reg = node.data as MyReg;
            MyGroup grp = m_groups.Find(x => x.nGroup == reg.user.nGroup);
            MyNode grpNode;

            //register
            grpNode = regNode.childs.Find(x => x.zTitle == grp.zGroup);
            if (grpNode != null)
            {
                if (!grpNode.childs.Contains(node))
                {
                    grpNode.childs.Add(node);
                }
            }
            else
            {
                grpNode = new MyNode() { zTitle = grp.zGroup };
                regNode.childs.Add(grpNode);
                grpNode.childs.Add(node);
            }

            //un-register
            grpNode = unregNode.childs.Find(x => x.zTitle == grp.zGroup);
            if (grpNode != null)
            {
                if (grpNode.childs.Contains(node))
                {
                    grpNode.childs.Remove(node);
                    if (grpNode.childs.Count == 0)
                    {
                        unregNode.childs.Remove(grpNode);
                    }
                }
            }

            updateTree(root);
        }

        private void openFormToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string vbs;
#if DEBUG
            vbs = @"C:\Users\Onsiter\Google Drive\CBV\DTPTXX\tools\dkthoikhoa.vbs";
#else
            vbs = ConfigMng.findTmpl("dkthoikhoa.vbs");
#endif
            Process scriptProc = new Process();
            scriptProc.StartInfo.WorkingDirectory = vbs.Substring(0, vbs.LastIndexOf("\\"));
            scriptProc.StartInfo.FileName = @"C:\Windows\SysWOW64\cscript.exe";
            scriptProc.StartInfo.UseShellExecute = false;
            scriptProc.StartInfo.Arguments = "dkthoikhoa.vbs";
            scriptProc.StartInfo.CreateNoWindow = false;
            scriptProc.StartInfo.Verb = "runas";
            scriptProc.StartInfo.WindowStyle = ProcessWindowStyle.Normal; //prevent console window from popping up
            scriptProc.Start();
            //scriptProc.StartInfo.WorkingDirectory = vbs.Substring(vbs.LastIndexOf("\\")); //<---very important 
            //scriptProc.WaitForExit(); // <-- Optional if you want program running until your script exit
            //scriptProc.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void programTV_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void usersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var userform = new FormUser();
            userform.m_cp = m_cp;
            userform.ShowDialog();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            //refresh user
            m_users = getUsers();
        }
    }
}
