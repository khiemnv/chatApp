using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace register
{

    public class InputPanel
    {
        protected string m_tblName;
        protected TableInfo m_tblInfo;
        public List<lInputCtrl> m_inputsCtrls;

        public lInputCtrl crtInputCtrl(TableInfo tblInfo, string colName, Point pos, Size size)
        {
            return crtInputCtrl(tblInfo, colName, pos, size, lSearchCtrl.SearchMode.match);
        }
        public lInputCtrl crtInputCtrl(TableInfo tblInfo, string colName, Point pos, Size size, lSearchCtrl.SearchMode mode)
        {
            int iCol = tblInfo.getColIndex(colName);
            if (iCol != -1)
            {
                return crtInputCtrl(tblInfo, iCol, pos, size, mode);
            }
            return null;
        }
        public lInputCtrl crtInputCtrl(TableInfo tblInfo, int iCol, Point pos, Size size)
        {
            return crtInputCtrl(tblInfo, iCol, pos, size, lSearchCtrl.SearchMode.match);
        }
        public lInputCtrl crtInputCtrl(TableInfo tblInfo, int iCol, Point pos, Size size, lSearchCtrl.SearchMode mode)
        {
            TableInfo.lColInfo col = tblInfo.m_cols[iCol];
            switch (col.m_type)
            {
                case TableInfo.lColInfo.lColType.text:
                case TableInfo.lColInfo.lColType.uniqueText:
                    lInputCtrlText textCtrl = new lInputCtrlText(col.m_field, col.m_alias, lSearchCtrl.ctrlType.text, pos, size);
                    textCtrl.m_mode = mode;
                    textCtrl.m_colInfo = col;
                    return textCtrl;
                case TableInfo.lColInfo.lColType.dateTime:
                    lInputCtrlDate dateCtrl = new lInputCtrlDate(col.m_field, col.m_alias, lSearchCtrl.ctrlType.dateTime, pos, size);
                    return dateCtrl;
                case TableInfo.lColInfo.lColType.num:
                    lInputCtrlNum numCtrl = new lInputCtrlNum(col.m_field, col.m_alias, lSearchCtrl.ctrlType.num, pos, size);
                    return numCtrl;
                case TableInfo.lColInfo.lColType.currency:
                    lInputCtrlCurrency currencyCtrl = new lInputCtrlCurrency(col.m_field, col.m_alias, lSearchCtrl.ctrlType.currency, pos, size);
                    return currencyCtrl;
                case TableInfo.lColInfo.lColType.map:
                    lInputCtrlEnum enumCtrl = new lInputCtrlEnum(col.m_field, col.m_alias, lSearchCtrl.ctrlType.map, pos, size);
                    return enumCtrl;
            }
            return null;
        }

        public T GetObject<T>() where T : new()
        {
            T item = new T();

            foreach (lInputCtrl ctrl in m_inputsCtrls)
            {
                PropertyInfo property = GetProperty(typeof(T), ctrl.m_fieldName);
                
                DateTime dt;
                object val;
                if (ctrl.m_type == lSearchCtrl.ctrlType.dateTime)
                {
                    ConfigMng.parseDisplayDate(ctrl.Text, out dt);
                    val = dt;
                }
                else
                {
                    val = ctrl.Text;
                }

                if (property != null)
                {
                    property.SetValue(item, ChangeType(val, property.PropertyType), null);
                }

                FieldInfo field = typeof(T).GetField(ctrl.m_fieldName);
                if(field != null)
                {
                    field.SetValue(item, ChangeType(val, field.FieldType));
                }
            }

            return item;
        }
        public void SetObject<T>(T item)
        {
            foreach (lInputCtrl ctrl in m_inputsCtrls)
            {
                PropertyInfo property = GetProperty(typeof(T), ctrl.m_fieldName);

                if (property != null)
                {
                    ctrl.Text = property.GetValue(item).ToString();
                    return;
                }

                FieldInfo field = typeof(T).GetField(ctrl.m_fieldName);
                if(field != null)
                {
                    var obj = field.GetValue(item);
                    if (obj != null)
                    {
                        ctrl.Text = obj.ToString();
                    }
                }
            }
        }
        private PropertyInfo GetProperty(Type type, string attributeName)
        {
            PropertyInfo property = type.GetProperty(attributeName);

            if (property != null)
            {
                return property;
            }

            var pros = type.GetProperties();
            return pros.FirstOrDefault(x => attributeName.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
        }
        public object ChangeType(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            }

            return Convert.ChangeType(value, type);
        }
    }
    public class lSearchParam
    {
        public string key;
        public string val;
        public DbType type;
    }
    public class lSearchCtrl : IDisposable
    {
        public enum ctrlType
        {
            text,
            dateTime,
            num,
            currency,
            map
        };
        public TableInfo.lColInfo m_colInfo;
        [DataMember(Name = "field", EmitDefaultValue = false)]
        public string m_fieldName;
        public string m_alias;
        public ctrlType m_type;
        [DataMember(Name = "pos", EmitDefaultValue = false)]
        public Point m_pos;
        [DataMember(Name = "size", EmitDefaultValue = false)]
        public Size m_size;

        [DataContract(Name = "SeachMode")]
        public enum SearchMode
        {
            [EnumMember]
            like,
            [EnumMember]
            match
        };

        [DataMember(Name = "mode", EmitDefaultValue = false)]
        public SearchMode m_mode = SearchMode.like;

        public FlowLayoutPanel m_panel = new FlowLayoutPanel();
        public CheckBox m_label = ConfigMng.CrtCheckBox();

        public lSearchCtrl() { }
        public lSearchCtrl(string fieldName, string alias, ctrlType type, Point pos, Size size)
        {
            m_fieldName = fieldName;
            m_alias = alias;
            m_type = type;
            m_pos = pos;
            m_size = size;

            m_label.Text = alias;
#if fit_txt_size
            m_label.AutoSize = true;
#else
            m_label.Width = 100;
#endif
            m_label.TextAlign = ContentAlignment.MiddleLeft;
            m_panel.AutoSize = true;
#if true
            m_panel.BorderStyle = BorderStyle.FixedSingle;
#endif
        }

        public virtual void updateInsertParams(List<string> exprs, List<lSearchParam> srchParams) { }
        public virtual void updateSearchParams(List<string> exprs, List<lSearchParam> srchParams) { }
        public virtual string getSearchParams() { return null; }
        public virtual void LoadData() { }
        protected virtual void valueChanged(object sender, EventArgs e)
        {
            m_label.Checked = true;
        }

        #region dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~lSearchCtrl()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_panel.Dispose();
                m_label.Dispose();
            }
        }
        #endregion
    };
    public class lInputCtrl : lSearchCtrl
    {
        protected new Label m_label;
        public lInputCtrl() { }
        public lInputCtrl(string fieldName, string alias, ctrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_label = ConfigMng.CrtLabel();
            m_label.Text = alias + " : ";
            m_label.TextAlign = ContentAlignment.MiddleLeft;
            m_panel.BorderStyle = BorderStyle.None;
        }
        public virtual bool ReadOnly { get; set; }
        public virtual string Text { get; set; }
        public event EventHandler<string> EditingCompleted;
        protected virtual void onEditingCompleted()
        {
            if (EditingCompleted != null) { EditingCompleted(this, Text); }
        }
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        m_label.Dispose();
        //    }
        //    base.Dispose();
        //}

    }
    public class lInputCtrlText : lInputCtrl
    {
        protected TextBox m_text;
        ComboBox m_combo;
        string m_value
        {
            get
            {
                if (m_text != null) return m_text.Text;
                else return m_combo.Text;
            }
        }
        public lInputCtrlText(string fieldName, string alias, ctrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_text = ConfigMng.CrtTextBox();
            m_text.Width = 200;

            m_panel.Controls.AddRange(new Control[] { m_label, m_text });
        }
        public override void updateInsertParams(List<string> exprs, List<lSearchParam> srchParams)
        {
            exprs.Add(m_fieldName);
            srchParams.Add(
                new lSearchParam()
                {
                    key = string.Format("@{0}", m_fieldName),
                    val = m_value
                }
            );
        }
#if use_auto_complete
        lDataSync m_autoCompleteData;
#endif
        public override void LoadData()
        {

        }


        private void M_combo_Validated(object sender, EventArgs e)
        {

        }
        public override bool ReadOnly
        {
            get
            {
                return m_text.ReadOnly;
            }

            set
            {
                m_text.ReadOnly = value;
                m_text.TabStop = !value;
            }
        }
        public override string Text
        {
            get
            {
                return m_value;
            }
            set
            {
                if (m_text != null)
                {
                    m_text.Text = value;
                }
            }

        }
    }
    public class lInputCtrlDate : lInputCtrl
    {
        private DateTimePicker m_date = new DateTimePicker();
        public lInputCtrlDate(string fieldName, string alias, ctrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
#if fit_txt_size
            int w = ConfigMng.getWidth(ConfigMng.getDateFormat()) + 20;
#else
            int w = 100;
#endif
            m_date.Width = w;
            m_date.Format = DateTimePickerFormat.Custom;
            m_date.CustomFormat = ConfigMng.GetDisplayDateFormat();
            m_date.Font = ConfigMng.GetFont();

            m_panel.Controls.AddRange(new Control[] { m_label, m_date });
        }

        public override void updateInsertParams(List<string> exprs, List<lSearchParam> srchParams)
        {
            string zStartDate = m_date.Value.ToString(ConfigMng.GetDateFormat());
            exprs.Add(m_fieldName);
            srchParams.Add(
                new lSearchParam()
                {
                    key = string.Format("@{0}", m_fieldName),
                    val = string.Format("{0} 00:00:00", zStartDate),
                    type = DbType.Date
                }
            );
        }
    }
    [DataContract(Name = "InputCtrlNum")]
    public class lInputCtrlNum : lInputCtrlText
    {
        public lInputCtrlNum(string fieldName, string alias, ctrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_text.KeyPress += onKeyPress;
        }
        private void onKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
    [DataContract(Name = "InputCtrlCurrency")]
    public class lInputCtrlCurrency : lInputCtrl
    {
        private TextBox m_val = ConfigMng.CrtTextBox();
        //private Label m_lab = ConfigMng.crtLabel();
        public lInputCtrlCurrency(string fieldName, string alias, ctrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
#if fit_txt_size
            int w = ConfigMng.getWidth("000,000,000,000");
#else
            int w = 100;
#endif
            m_val.Width = w;
            m_val.RightToLeft = RightToLeft.Yes;
            m_val.KeyPress += onKeyPress;
            m_val.KeyUp += onKeyUp;
            m_val.Validated += M_val_Validated;
            m_panel.Controls.AddRange(new Control[] { m_label, m_val });
        }

        private void M_val_Validated(object sender, EventArgs e)
        {
            onEditingCompleted();
        }

        private void onKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        int selectStart;
        private void onKeyUp(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (e.KeyCode == Keys.Back)
            {
                //0,000 ->0000
                string txt = tb.Text;
                char[] buff = new char[txt.Length];
                int s = 0;
                int i = txt.Length - 1;
                for (; i >= 0; i--)
                {
                    char ch = txt[i];
                    s++;
                    if (ch == ',') { s = 0; }
                    if (s == 4)
                    {
                        string newVal = "";
                        if (i > 0) newVal = txt.Substring(0, i);
                        newVal = newVal + new string(buff, i + 1, txt.Length - i - 1);
                        selectStart = tb.SelectionStart - 1;
                        chgTxt(tb, newVal);
                        return;
                    }
                    buff[i] = ch;
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                //0,000 ->0000
                string txt = tb.Text;
                char[] buff = new char[txt.Length];
                int s = 0;
                int i = txt.Length - 1;
                for (; i >= 0; i--)
                {
                    char ch = txt[i];
                    s++;
                    if (ch == ',') { s = 0; }
                    if (s == 4)
                    {
                        string newVal = "";
                        newVal = txt.Substring(0, i + 1);
                        newVal = newVal + new string(buff, i + 2, txt.Length - i - 2);
                        selectStart = tb.SelectionStart;
                        chgTxt(tb, newVal);
                        return;
                    }
                    buff[i] = ch;
                }
            }

            selectStart = tb.SelectionStart;
            chgTxt(tb, tb.Text);
        }

        private void chgTxt(TextBox tb, string val)
        {
            Int64 amount = 0;
            //display in 000,000
            char[] buff = new char[64];
            Debug.Assert(val.Length < 48, "currency too long");
            int j = 63;
            for (int i = val.Length; i > 0; i--)
            {
                char ch = val[i - 1];
                if (ch >= '0' && ch <= '9')
                {
                    amount = amount * 10 + (ch - '0');
                    if (j % 4 == 0)
                    {
                        buff[j] = ',';
                        j--;
                    }
                    buff[j] = ch;
                    j--;
                }
            }
            string newVal = new string(buff, j + 1, 63 - j);
            tb.Text = newVal;

            selectStart += newVal.Length - val.Length;
            if (selectStart >= 0) { tb.Select(selectStart, 0); }

            //update size

        }

        void getInputRange(out string val)
        {
            val = m_val.Text.Replace(",", "");
            if (val == "") val = "0";
        }
        public override bool ReadOnly
        {
            get
            {
                return m_val.ReadOnly;
            }

            set
            {
                m_val.ReadOnly = value;
                m_val.TabStop = !value;
            }
        }
        public override string Text
        {
            get
            {
                return m_val.Text;
            }

            set
            {
                m_val.Text = value;
            }
        }
        public override void updateInsertParams(List<string> exprs, List<lSearchParam> srchParams)
        {
            string val;
            getInputRange(out val);
            srchParams.Add(
                new lSearchParam()
                {
                    key = "@" + m_fieldName,
                    val = val,
                    type = DbType.UInt64
                }
            );
            exprs.Add(m_fieldName);
        }
    }
    [DataContract(Name = "InputCtrlEnum")]
    public class lInputCtrlEnum : lInputCtrl
    {
        ComboBox m_combo;
        public lInputCtrlEnum(string fieldName, string alias, ctrlType type, Point pos, Size size)
            : base(fieldName, alias, type, pos, size)
        {
            m_combo = ConfigMng.CrtComboBox();
            m_combo.Width = 100;
            m_panel.Controls.AddRange(new Control[] { m_label, m_combo });
        }
        public class comboItem
        {
            public string name;
            public int val;
        }
        public void init(List<comboItem> arr)
        {
            var dt = new DataTable();
            dt.Columns.Add("name");
            dt.Columns.Add("val");
            foreach (var item in arr)
            {
                var newRow = dt.NewRow();
                newRow[0] = item.name;
                newRow[1] = item.val;
                dt.Rows.Add(newRow);
            }
            m_combo.DataSource = dt;
            m_combo.DisplayMember = "name";
            m_combo.ValueMember = "val";
        }
        public override void updateInsertParams(List<string> exprs, List<lSearchParam> srchParams)
        {
            string zVal = m_combo.SelectedValue.ToString();
            exprs.Add(m_fieldName);
            srchParams.Add(
                new lSearchParam()
                {
                    key = string.Format("@{0}", m_fieldName),
                    val = zVal,
                    type = DbType.Int16
                }
            );
        }
        public override string Text
        {
            get
            {
                return m_combo.SelectedItem.ToString();
            }

            set
            {
                m_combo.Text = value;
            }
        }
    }
    public class UserInputPanel : InputPanel
    {
        public UserInputPanel()
        {
            m_tblInfo = new UserTableInfo();
            m_tblName = "users";

            m_inputsCtrls = new List<lInputCtrl> {
                crtInputCtrl(m_tblInfo, "ID"        , new Point(0, 0), new Size(2, 1)),
                crtInputCtrl(m_tblInfo, "zUser"     , new Point(0, 1), new Size(2, 1)),
                crtInputCtrl(m_tblInfo, "zFb"       , new Point(0, 2), new Size(2, 1)),
                crtInputCtrl(m_tblInfo, "zZalo"     , new Point(0, 3), new Size(2, 1)),
                crtInputCtrl(m_tblInfo, "birthDate" , new Point(0, 4), new Size(2, 1)),
                crtInputCtrl(m_tblInfo, "nGroup"    , new Point(0, 5), new Size(2, 1)),
                crtInputCtrl(m_tblInfo, "zPhapDanh" , new Point(0, 6), new Size(2, 1)),
            };
            m_inputsCtrls[0].ReadOnly = true;
        }

    }
}
