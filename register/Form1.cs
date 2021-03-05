using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
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
        List<MyUser> m_users;
        List<MyProgram> m_programs;
        List<MyGroup> m_groups;
        Indexer m_idx;
        string m_db;
        string m_cnnStr;
        Dictionary<string, MyNode> m_nodes;  //<zUserFb><MyNode>

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

            List<MyProgram> progs = getProg();
            m_programs = progs;

            progCmb.Items.AddRange(progs.Select(i => i.zName).ToArray());
            progCmb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            progCmb.AutoCompleteSource = AutoCompleteSource.ListItems;

            m_users = getUsers();
            m_groups = m_cp.GetAllGroups();

            userCmb.KeyUp += UserCmb_KeyUp; ;
            //textBox1.TextChanged += TextBox1_TextChanged;
            //textBox1.KeyDown += TextBox1_KeyDown;

            progCmb.DropDownStyle = ComboBoxStyle.DropDownList;
            progCmb.SelectedIndexChanged += ProgCmb_SelectedIndexChanged;

            treeView1.NodeMouseClick += TreeView1_NodeMouseClick;
            addBtn.Click += AddBtn_Click;

            splitContainer1.Dock = DockStyle.Fill;
            progCmb.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            treeView1.Dock = DockStyle.Fill;

            richTextBox1.Anchor = AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom|AnchorStyles.Top;

            m_treeMng = new MyTree();
            m_treeMng.m_tree = treeView1;
            m_treeMng.m_treeStyle = MyTree.TreeStyle.check;
            m_treeMng.InitTree();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var txt = optTxt.Text;
            Regex reg = new Regex(@"^(\d)( \((.*)\))?");
            var m = reg.Match(txt);
            if (!m.Success)
            {
                MessageBox.Show("Invalid option");
                return;
            }

            var nOpt = int.Parse(m.Groups[1].Value);
            var zOpt = m.Groups[3].Value;
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

            MyTree.MyTitle title = new MyTree.MyTitle();
            MyGroup grp = m_groups.Find(x => x.nGroup == user.nGroup);
            title.title = user.zUserFb;
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
            } else
            {
                PreviewReport(e.Node);
            }
        }

        void PreviewReport(TreeNode tnode)
        {
            //string 
        }

        private void ProgCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            var titleLst = BuildTree();
            m_treeMng.Clear();
            m_treeMng.AddTitles(titleLst);
        }

        void getCnnStr()
        {
#if DEBUG
            m_db = @"C:\Users\Onsiter\Google Drive\CBV\DTPTXX\tools\PTXX_NB.accdb";
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
            //var zDb = ConfigMng.findTmpl("");
            var zDb = @"C:\Users\Onsiter\Google Drive\CBV\DTPTXX\tools\PTXX_NB.accdb";
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
            var prog = m_programs.Find(x => x.zName == zProg);
            List<MyReg> regs = m_cp.GetRegs(prog);
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
        public List<MyTree.MyTitle> BuildTree()
        {
            var groups = m_groups;
            var regs = getRegister();

            List<MyTree.MyTitle> titleLst = new List<MyTree.MyTitle>();

            Dictionary<UInt64, MyUser> userDict = new Dictionary<ulong, MyUser>();
            foreach (MyUser user in m_users)
            {
                userDict.Add(user.ID, user);
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
    }
}
