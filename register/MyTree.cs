using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace register
{
    //using
    //m_treeStyle = TreeStyle.check;
    //initTree(); //set state imagine
    public class MyTree
    {
        public enum TreeStyle
        {
            check,
            radio,
        }
        public TreeStyle m_treeStyle;

        public TreeView m_tree;
        Dictionary<string, Node> m_nodeDict;    //path, node
        #region tree

        public void Clear()
        {
            m_nodeDict.Clear();
            m_tree.Nodes.Clear();
        }
        public void AddTitles(List<MyTitle> titles)
        {
            foreach (var title in titles)
            {
                var tNode = addRow('T', 1,
                    title.path, title.title);
                tNode.title = title;
                RenderNode(tNode);
            }
        }
        public MyTitle GetTitle(string path)
        {
            Node node = m_nodeDict[path];
            return node.title;
        }
        public int Remove(string path)
        {
            if (m_nodeDict.ContainsKey(path))
            {
                Node child = m_nodeDict[path];
                m_nodeDict.Remove(path);
                int idx = path.LastIndexOf('/');
                string parentPath = path.Substring(0,idx);
                Node parent = m_nodeDict[parentPath];
                parent.tnode.Nodes.Remove(child.tnode);

                if (parent.tnode.Nodes.Count == 0)
                {
                    Remove(parentPath);
                }
                return 1;
            }

            return 0;
        }
        public int Remove(MyTitle title)
        {
            return Remove(title.path);
        }
        public int Add(MyTitle title)
        {
            if (!m_nodeDict.ContainsKey(title.path))
            {
                var tNode = addRow('T', 1,title.path, title.title);
                tNode.title = title;
                RenderNode(tNode);
                return 1;
            }
            return 0;
        }
        void RenderNode(Node node)
        {
            var tDict = m_nodeDict;
            var arr = node.title.path.Split(new char[] { '\\', '/' });
            string path = arr[0];
            Node parent;
            parent = tDict[path];
            if (parent.tnode == null)
            {
                parent.tnode = m_tree.Nodes.Add(parent.name);
                parent.tnode.Tag = path;
                parent.tnode.StateImageIndex = 0;
            }

            Node child;
            for (int j = 1; j < arr.Length; j++)
            {
                path = path + "/" + arr[j];
                child = tDict[path];
                if (child.tnode == null)
                {
                    child.tnode = parent.tnode.Nodes.Add(child.name);
                    child.tnode.Tag = path;
                    child.tnode.StateImageIndex = 0;
                }
                parent = child;
            }
        }

        int getNo(string txt)
        {
            Regex reg = new Regex("([IVXLC]+)|một|hai|ba|bốn|năm|sáu|bảy|tám|chín|mười|mươi");
            var mc = reg.Matches(txt);
            //tap I
            string zNo = txt.Substring(4);
            switch (zNo)
            {
                case "I": return 1;
                case "II": return 2;
                case "III": return 3;
                case "IV": return 4;
                case "V": return 5;
                case "VI": return 6;
                case "VII": return 7;
                case "VIII": return 8;
                case "IX": return 9;
            }
            return 0;
        }
        Node addRow(char type, UInt64 size, string name, string dir = "")
        {
            var tDict = m_nodeDict;
            var arr = name.Split(new char[] { '\\', '/' });
            //arr[0] = dir;
            string path = arr[0];
            Node parent;
            //m_nodeDict = new Dictionary<string, Node>();
            if (tDict.ContainsKey(path))
            {
                parent = tDict[path];
            }
            else
            {
                parent = new Node() { id = path, name = path };
                tDict.Add(path, parent);
            }
            Node child;
            for (int j = 1; j < arr.Length; j++)
            {
                path = path + "/" + arr[j];
                if (tDict.ContainsKey(path))
                {
                    child = tDict[path];
                }
                else
                {
                    child = new Node() { id = path, name = arr[j] };
                    tDict.Add(path, child);
                    parent.childs.Add(child);
                }

                parent.size++;
                parent = child;
            }
            parent.size = size;
            parent.type = type;
            return parent;
        }
        TreeNode CreateTreeNode(Node node, TreeNode newNode = null)
        {
            //if (node.type != 'T') { node.size = (UInt64)node.childs.Count; }
            string name = node.size == 0 ? node.name :
                string.Format("{0} ({1})", node.name, node.size);
            if (newNode == null) newNode = new TreeNode(name);
            else newNode.Name = name;

            newNode.Tag = node.id;
            newNode.StateImageIndex = 0;
#if RightClickCMS
            if (node.title != null)
            {
                ContextMenuStrip contextMenuStrip;
                contextMenuStrip = new ContextMenuStrip();
                contextMenuStrip.Items.Add("Speech").Click += (s, e)=>{ SpeechTitle(node.title); };
                contextMenuStrip.Items.Add("Stop").Click += (s, e) => { SpeechStop(); };
                newNode.ContextMenuStrip = contextMenuStrip;
            }
#endif
            node.tnode = newNode;
            return newNode;
        }
        void renderTree(Node root)
        {
            var tree = m_tree;
            m_tree.Nodes.Clear();
            var tnRoot = CreateTreeNode(root);
            Queue<KeyValuePair<Node, TreeNode>> q = new Queue<KeyValuePair<Node, TreeNode>>();
            q.Enqueue(new KeyValuePair<Node, TreeNode>(root, tnRoot));
            while (q.Count > 0)
            {
                var rec = q.Dequeue();
                foreach (Node child in rec.Key.childs)
                {
                    var tnChild = CreateTreeNode(child);
                    rec.Value.Nodes.Add(tnChild);
                    q.Enqueue(new KeyValuePair<Node, TreeNode>(child, tnChild));
                }
            }

            tree.Nodes.Add(tnRoot);
        }
        void updateTree(Node parent, Node child)
        {
            var tnParent = parent.tnode;
            child.tnode = CreateTreeNode(child);
            tnParent.Nodes.Add(child.tnode);
        }
        private ImageList CrtChkBoxImg()
        {
            var lst = new ImageList();
            for (int i = 0; i < 3; i++)
            {
                // Create a bitmap which holds the relevent check box style
                // see http://msdn.microsoft.com/en-us/library/ms404307.aspx and http://msdn.microsoft.com/en-us/library/system.windows.forms.checkboxrenderer.aspx

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(16, 16);
                System.Drawing.Graphics chkGraphics = System.Drawing.Graphics.FromImage(bmp);
                switch (i)
                {
                    // 0,1 - offset the checkbox slightly so it positions in the correct place
                    case 0:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                        break;
                    case 1:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
                        break;
                    case 2:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 1), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
                        break;
                }

                lst.Images.Add(bmp);
            }
            return lst;
        }
        private ImageList CrtRadBtnImg()
        {
            var lst = new ImageList();
            for (int i = 0; i < 2; i++)
            {
                Bitmap bmp = new Bitmap(16, 16);
                Graphics chkGraphics = Graphics.FromImage(bmp);
                switch (i)
                {
                    // 0,1 - offset the checkbox slightly so it positions in the correct place
                    case 0:
                        RadioButtonRenderer.DrawRadioButton(chkGraphics, new Point(0, 1), System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedNormal);
                        break;
                    case 1:
                        RadioButtonRenderer.DrawRadioButton(chkGraphics, new Point(0, 1), System.Windows.Forms.VisualStyles.RadioButtonState.CheckedNormal);
                        break;
                }

                lst.Images.Add(bmp);
            }
            return lst;
        }
        public void InitTree()
        {
            m_tree.CheckBoxes = false;
            m_tree.StateImageList = new ImageList();
            switch (m_treeStyle)
            {
                case TreeStyle.check:
                    m_tree.StateImageList = CrtChkBoxImg();
                    break;
                case TreeStyle.radio:
                    m_tree.StateImageList = CrtRadBtnImg();
                    break;
            }
            
            m_nodeDict = new Dictionary<string, Node>();
        }
        protected void OnNodeMouseClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        {
            //base.OnNodeMouseClick(e);

            // is the click on the checkbox?  If not, discard it
            TreeViewHitTestInfo info = m_tree.HitTest(e.X, e.Y);
            if (info == null)
            {
                return;
            }
            else if (info.Location == TreeViewHitTestLocations.Label)
            {
                string key = (string)e.Node.Tag;

                var tNode = m_nodeDict[key];
                //var title = tNode.title;
                //if (title == null) { return; }

                //avoid re display title
                DisplayNode(tNode);
            }
            else if (info.Location == System.Windows.Forms.TreeViewHitTestLocations.StateImage)
            {
                TreeNode tn = e.Node;
                var bChk = Check(tn);
                Check(tn, !bChk);
            }
            else
            {
                return;
            }
        }

        private void DisplayNode(Node node)
        {
            //var lst = getAllTitleR(node);
            //string jsTxt = TitlesLstToJson(lst);
            //string htmlTxt = GenHtmlTxt(jsTxt);
            //UpdateWB(htmlTxt);
        }

        void DisplayTitle2(MyTitle title)
        {
            //if (m_curTitle == title.path) return;

            //DisplayTitle(title);
            //if (isEditing)
            //{
            //    BeginEditTitle(title);
            //}

            //m_curTitle = title.path;
        }

        private void Tree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //if (e.Action != TreeViewAction.Unknown)
            switch (m_treeStyle)
            {
                case TreeStyle.check:
                    updateChkBoxState(e);
                    break;
                case TreeStyle.radio:
                    UpdateRadBtnState(e);
                    break;
            }

            OnSelectedChg();
        }
        private void updateChkBoxState(TreeViewEventArgs e)
        {
            // check/uncheck tree nodes
            var val = Check(e.Node);
            var lst = new List<TreeNode>();
            lst.Add(e.Node);
            while (lst.Count > 0)
            {
                var node = lst[0];
                lst.RemoveAt(0);
                var parent = node.Parent;
                if (parent != null)
                {
                    CheckParentNode(parent, val);
                    lst.Add(parent);
                }
            }
            lst.Add(e.Node);
            while (lst.Count > 0)
            {
                var node = lst[0];
                lst.RemoveAt(0);
                if (Check(node) != val) { Check(node, val); }
                lst.AddRange(node.Nodes.Cast<TreeNode>());
            }
        }
        private void UpdateRadBtnState(TreeViewEventArgs e)
        {
            var val = Check(e.Node);
            if (!val)
            {
                Check(e.Node, true);
                Check(e.Node, 1);
            }
            else
            {
                //uncheck other node
                foreach (TreeNode node in m_tree.Nodes)
                {
                    if (Check(node)) { Check(node, 0); Check(node, false); }
                }
                Check(e.Node, true);
                Check(e.Node, 1);
            }
        }
        public virtual void OnSelectedChg() { }
        public class Node
        {
            public MyTitle title;
            public string id;
            public char type;
            public UInt64 size;
            public string name;
            public List<Node> childs = new List<Node>();

            public TreeNode tnode;
        }

        private List<TreeNode> GetAllLeafs(TreeNode parent)
        {
            var lst = new List<TreeNode>();
            var q = new List<TreeNode>();
            q.Add(parent);
            while (q.Count > 0)
            {
                var n = q[0];
                q.RemoveAt(0);
                if (n.Nodes.Count == 0) { lst.Add(n); }
                else
                {
                    q.AddRange(n.Nodes.Cast<TreeNode>().ToList());
                }
            }
            return lst;
        }

        private void CheckParentNode(TreeNode parent, bool val)
        {
            int i = 0;
            var childLst = parent.Nodes;
            for (; i < childLst.Count; i++)
            {
                var child = childLst[i];
                if (!(Check(child) == val)) { break; }    //child not checked
            }
            if (i == childLst.Count)
            {
                if (Check(parent) != val) { Check(parent, val); }
                Check(parent, val ? 1 : 0);
            }
            else
            {
                if (Check(parent) != false) { Check(parent, false); }
                Check(parent, 2);
            }
        }

        private void UpdateParent(TreeNode node, int idx)
        {
            int state = idx;
            foreach (TreeNode child in node.Nodes)
            {
                if (child.StateImageIndex != idx)
                {
                    state = 2; //gray
                    break;
                }
            }
            node.StateImageIndex = state;

            if (node.Parent != null)
            {
                UpdateParent(node.Parent, idx);
            }
        }

        private bool Check(TreeNode node, bool val)
        {
            int idx = val ? 1 : 0;
            node.StateImageIndex = idx;

            if (node.Nodes.Count > 0)
            {
                UpdateChilds(node, idx);
            }

            if (node.Parent != null)
            {
                UpdateParent(node.Parent, idx);
            }

            return idx == 1;
        }
        protected bool Check(TreeNode node, int idx = -1)
        {
            if (idx == -1)
            {
                return node.StateImageIndex == 1;
            }
            else
            {
                node.StateImageIndex = idx;
                return idx == 1;
            }
        }
        private void UpdateChilds(TreeNode node, int idx)
        {
            foreach (TreeNode i in node.Nodes)
            {
                i.StateImageIndex = idx;
                if (i.Nodes.Count > 0) { UpdateChilds(i, idx); }
            }
        }
        #endregion
        public class MyTitle
        {
            public string zID;
            public string title;
            public string path;
            public string content;
            public string type; // [group/msg/user]

            public string likes;    //user1, user2, ...
            public UInt64 ID;
            public UInt64 groupID;
            public string groupPath;
            public int ord;
            public UInt64 parentID;
            public Dictionary<UInt64, MyTitle> childs;
        }
    }
}
