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
            lbl.Text = "Ho Ten";
            zUser = new TextBox();
            tlp.Controls.Add(lbl, iCol, iRow);
            tlp.Controls.Add(zUser, iCol+1, iRow);

            lbl = new Label() { Text = "facebook" };
            zFb = new TextBox();
            tlp.Controls.Add(lbl, iCol, ++iRow);
            tlp.Controls.Add(zFb, iCol + 1, iRow);

            lbl = new Label() { Text = "zalo" };
            zZalo = new TextBox();
            tlp.Controls.Add(lbl, iCol, ++iRow);
            tlp.Controls.Add(zZalo, iCol + 1, iRow);

            lbl = new Label() { Text = "Ngay Sinh" };
            birthDate = new DateTimePicker();
            tlp.Controls.Add(lbl, iCol, ++iRow);
            tlp.Controls.Add(birthDate, iCol + 1, iRow);

            lbl = new Label() { Text = "Nhom" };
            nGroup = new TextBox();
            tlp.Controls.Add(lbl, iCol, ++iRow);
            tlp.Controls.Add(nGroup, iCol + 1, iRow);

            lbl = new Label() { Text = "Tags" };
            tags = new CheckedListBox();
            tlp.Controls.Add(lbl, iCol, ++iRow);
            tlp.Controls.Add(tags, iCol + 1, iRow);

            lbl = new Label() { Text = "ID" };
            id = new TextBox();
            id.ReadOnly = true;
            tlp.Controls.Add(lbl, iCol, ++iRow);
            tlp.Controls.Add(id, iCol + 1, iRow);

            var btn = new Button() { Text = "Add/Update" };
            btn.Click += AddUpdateUser;
            tlp.Controls.Add(btn, iCol, ++iRow);

            sc.Panel2.Controls.Add (tlp);
            this.Controls.Add( sc);

            tv = new TreeView();
            tv.NodeMouseDoubleClick += ShowSelectedUser;
            tv.Dock = DockStyle.Fill;
            sc.Panel1.Controls.Add(tv);
        }

        private void ShowSelectedUser(object sender, TreeNodeMouseClickEventArgs e)
        {
            MyUser u = (MyUser)e.Node.Tag;
            zUser.Text = u.zUser;
            zFb.Text = u.zFb;
            id.Text = u.ID.ToString();
            nGroup.Text = u.nGroup.ToString();
            m_user = u;

            //birthDate.Value = u.birthDate
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

        private void AddUpdateUser(object sender, EventArgs e)
        {
            if (id.Text == "")
            {
                m_user = new MyUser();
                m_user.zUser = zUser.Text;
                m_user.zFb = zFb.Text;
                m_user.nGroup = int.Parse(nGroup.Text);
                m_cp.AddUser(m_user);
                var n = tv.Nodes.Add(m_user.zUserFb);
                n.Tag = m_user;
                n.EnsureVisible();
            }
            else
            {
                var old = m_user.zUserFb;

                var user = new MyUser();
                user.zUser = zUser.Text;
                user.zFb = zFb.Text;
                user.nGroup = int.Parse(nGroup.Text);
                user.ID= Convert.ToUInt64(id.Text);
                m_cp.UpdateUser(user);

                var n = tv.Nodes.Find(old,true);
                n[0].Text = user.zUserFb;
                n[0].Tag = user;
                m_user = user;
                n[0].EnsureVisible();
            }
            List<string> tagLst = new List<string>();
            foreach( var i in tags.CheckedItems)
            {
                tagLst.Add(i.ToString());
            }
            m_cp.UpdateUserTag(m_user, tagLst);
        }

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
                var n = tv.Nodes.Add(u.zUserFb);
                n.Tag = u;
            }
        }
    }
}
