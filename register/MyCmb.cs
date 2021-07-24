using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace register
{
    class MyCmb
    {
        public EventHandler OnSelectUser;
        public ComboBox userCmb;
        bool m_busy = false;
        BindingSource m_bs;
        Indexer m_idx;
        List<MyUser> p_users;
        public List<MyUser> m_users
        {
            get { return p_users; }
            set
            {
                p_users = value;
                OnUpdateUsers();
            }
        }
        public MyUser GetUser()
        {
            string txt = userCmb.Text;
            return m_users.Find((u) => {return u.zUserFb == txt; });
        }

        private void OnUpdateUsers()
        {
            m_idx = null;
        }
        public MyCmb()
        {
            userCmb = ConfigMng.CrtComboBox();
            userCmb.KeyUp += Cmb_KeyUp;
            userCmb.SelectedIndexChanged += UserCmb_SelectedIndexChanged;
        }

        private void UserCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnSelectUser != null)
            {
                OnSelectUser(this,new EventArgs());
            }
        }

        private void Cmb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                UserCmb_TextChanged(sender, e);
            }
        }
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

        }

    }
}
