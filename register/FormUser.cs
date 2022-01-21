using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace register
{
    public partial class FormUser : Form
    {
        public lOleDbContentProvider m_cp;

        TextBox zUser;
        TextBox zFb;
        TextBox zZalo;
        DateTimePicker birthDate;
        TextBox nGroup;
        CheckedListBox tags;
        TextBox id;

        TreeView tv;

        MyUser m_user;
        MyCmb m_useCmb;
        InputPanel m_inputPanel;
        public FormUser()
        {
            InitializeComponent();
            InitCtrl();
            Load += LoadData;
        }

        private void InitCtrl()
        {
            SplitContainer sc = new SplitContainer();
            sc.Dock = DockStyle.Fill;
            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.AutoSize });
            tlp.ColumnStyles.Add(new ColumnStyle() { SizeType = SizeType.Percent,Width=100 });
            var iCol = 0;
            var iRow = 0;
            var lbl = new Label();

            m_inputPanel = new UserInputPanel();
            int lastRow = 0;
            foreach (var ctrl in m_inputPanel.m_inputsCtrls)
            {
                tlp.Controls.Add(ctrl.m_panel, ctrl.m_pos.X, ctrl.m_pos.Y);
                tlp.SetColumnSpan(ctrl.m_panel, ctrl.m_size.Width);
                tlp.SetRowSpan(ctrl.m_panel, ctrl.m_size.Height);
                lastRow = Math.Max(lastRow, ctrl.m_pos.Y);
                iRow++;
            }

            lbl = ConfigMng.CrtLabel(); lbl.Text = "Tags";
            tags = ConfigMng.CrtCheckedListBox();
            tags = ConfigMng.CrtCheckedListBox();
            tags.MultiColumn =  true;
            tags.Dock = DockStyle.Fill;
            tags.Height = 200;

            tlp.Controls.Add(lbl, iCol, ++iRow);
            //tlp.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            tlp.Controls.Add(tags, iCol + 1, iRow);

            var btn = ConfigMng.CrtButton();
            btn.Text = "Add/Update";
            btn.AutoSize = true;
            btn.Click += AddUpdateUser;
            tlp.Controls.Add(btn, iCol, ++iRow);

            sc.Panel2.Controls.Add (tlp);
            //sc.Panel2.Controls.Add (btn);
            this.Controls.Add( sc);

            //left panel
            tv = new TreeView();
            tv.NodeMouseDoubleClick += (s,e)=>{
                m_user = (MyUser)e.Node.Tag;
                ShowSelectedUser();
            };
            tv.Dock = DockStyle.Fill;
            sc.Panel1.Controls.Add(tv);

            m_useCmb = new MyCmb();
            var cmb = m_useCmb.userCmb;
            cmb.Dock = DockStyle.Top;
            sc.Panel1.Controls.Add(cmb);

            m_useCmb.OnSelectUser += (s,e)=>
            {
                m_user = m_useCmb.GetUser();
                ShowSelectedUser();
            };
            
            //menu context
            var mc = new ContextMenuStrip();
            this.ContextMenuStrip = mc;
            var newUser = mc.Items.Add("clear");
            newUser.Click += NewUser_Click;

            this.FormClosed += FormUser_Resize;

            //restore size
            {
                var location = ConfigMng.CfgRead("UserWndLocation") as string;
                var wndSize = ConfigMng.CfgRead("UserWndSize") as string;
                if (location != null)
                {
                    this.Location = JsonConvert.DeserializeObject<Point>(location);
                    this.Size = JsonConvert.DeserializeObject<Size>(wndSize);
                }
            }
        }

        private void FormUser_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                ConfigMng.CfgWrite("UserWndLocation", JsonConvert.SerializeObject(this.Location));
                ConfigMng.CfgWrite("UserWndSize", JsonConvert.SerializeObject(this.Size));
            }
        }

        private void NewUser_Click(object sender, EventArgs e)
        {
            foreach (var ctrl in m_inputPanel.m_inputsCtrls)
            {
                ctrl.Text = "";
            }
            for (int i = 0; i < tags.Items.Count; i++)
            {
                tags.SetItemChecked(i, false);
            }
        }

        #region edit_user
        TreeNode m_editingNode;
        private void ShowSelectedUser()
        {
            MyUser u = m_user;
            m_inputPanel.SetObject<MyUser>(u);

            var tagLst = m_cp.GetUserTags(u);
            for(int i = 0;i< tags.Items.Count;i++)
            {
                string z = tags.Items[i].ToString();
                if (tagLst.Contains(z))
                {
                    tags.SetItemChecked(i, true);
                } else
                {
                    tags.SetItemChecked(i, false);
                }
            }
        }

        ToolTip mNotify = new ToolTip();
        private void AddUpdateUser(object sender, EventArgs e)
        {            
            List<string> tagLst = new List<string>();
            string tooltipText = "";
            foreach( var i in tags.CheckedItems)
            {
                tagLst.Add(i.ToString());
            }
            var obj = m_inputPanel.GetObject<MyUser>();
            if (obj.ID == 0)
            {
                m_user = obj;
                m_cp.AddUser(m_user);
                var n = tv.Nodes.Add(m_user.zUserFb);
                n.Tag = m_user;
                n.EnsureVisible();
                tooltipText = "added new";
                m_cp.UpdateUserTag(m_user, tagLst);
                m_useCmb.OnUpdateUsers();
            }
            else
            {
                var user = obj;
                var ret = m_cp.UpdateUser(user);

                var n = tv.Nodes.Find(user.ID.ToString(),true);
                n[0].Text = user.zUserFb;
                n[0].Tag = user;
                m_user = user;
                n[0].EnsureVisible();
                
                var nchg = m_cp.UpdateUserTag(m_user, tagLst);
                ret += nchg;
                if (ret > 0)
                { 
                    tooltipText = "updated";
                    m_useCmb.OnUpdateUsers();
                }
                else
                {
                    tooltipText = "no change";
                }
            }
            mNotify.Show(tooltipText,(IWin32Window)sender,1000);
        }
        #endregion
        private void LoadData(object sender, EventArgs e)
        {
            var tagLst = m_cp.GetAllTags();
            foreach(string ztag in tagLst)
            {
                tags.Items.Add(ztag);
            }

            var userLst = m_cp.GetAllUsers();
            foreach (MyUser u in userLst)
            {
                var n = tv.Nodes.Add(u.ID.ToString(),u.zUserFb);
                n.Tag = u;
            }
            m_useCmb.m_users = userLst;
        }
    }
}
