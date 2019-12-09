using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cf_pad.MDL
{
    /// <summary>
    /// 用戶信息
    /// </summary>
    public class mdlUser
    {

        public int Uid;
        public string Uname;
        public string Uname_Desc;
        public string Pwd;
        public string Language;
        public Role Role;
    }

    /// <summary>
    /// 角色
    /// </summary>
    public class Role
    {
        public int Rid;
        public string Rname;
    }

    /// <summary>
    /// 用戶對按鈕權限
    /// </summary>
    public class mdlUserPopedom
    {
        private string usr_No;

        public string Usr_No
        {
            get { return usr_No; }
            set { usr_No = value; }
        }
        private string window_Id;

        public string Window_Id
        {
            get { return window_Id; }
            set { window_Id = value; }
        }
        private byte c1_State;

        public byte C1_State
        {
            get { return c1_State; }
            set { c1_State = value; }
        }
        private string c1_Id;

        public string C1_Id
        {
            get { return c1_Id; }
            set { c1_Id = value; }
        }
        private string c1_Text;

        public string C1_Text
        {
            get { return c1_Text; }
            set { c1_Text = value; }
        }
        private byte c2_State;

        public byte C2_State
        {
            get { return c2_State; }
            set { c2_State = value; }
        }
        private string c2_Id;

        public string C2_Id
        {
            get { return c2_Id; }
            set { c2_Id = value; }
        }
        private string c2_Text;

        public string C2_Text
        {
            get { return c2_Text; }
            set { c2_Text = value; }
        }
        private byte c3_State;

        public byte C3_State
        {
            get { return c3_State; }
            set { c3_State = value; }
        }
        private string c3_Id;

        public string C3_Id
        {
            get { return c3_Id; }
            set { c3_Id = value; }
        }
        private string c3_Text;

        public string C3_Text
        {
            get { return c3_Text; }
            set { c3_Text = value; }
        }
        private byte c4_State;

        public byte C4_State
        {
            get { return c4_State; }
            set { c4_State = value; }
        }
        private string c4_Id;

        public string C4_Id
        {
            get { return c4_Id; }
            set { c4_Id = value; }
        }
        private string c4_Text;

        public string C4_Text
        {
            get { return c4_Text; }
            set { c4_Text = value; }
        }
        private byte c5_State;

        public byte C5_State
        {
            get { return c5_State; }
            set { c5_State = value; }
        }
        private string c5_Id;

        public string C5_Id
        {
            get { return c5_Id; }
            set { c5_Id = value; }
        }
        private string c5_Text;

        public string C5_Text
        {
            get { return c5_Text; }
            set { c5_Text = value; }
        }
        private byte c6_State;

        public byte C6_State
        {
            get { return c6_State; }
            set { c6_State = value; }
        }
        private string c6_Id;

        public string C6_Id
        {
            get { return c6_Id; }
            set { c6_Id = value; }
        }
        private string c6_Text;

        public string C6_Text
        {
            get { return c6_Text; }
            set { c6_Text = value; }
        }
        private byte c7_State;

        public byte C7_State
        {
            get { return c7_State; }
            set { c7_State = value; }
        }
        private string c7_Id;

        public string C7_Id
        {
            get { return c7_Id; }
            set { c7_Id = value; }
        }
        private string c7_Text;

        public string C7_Text
        {
            get { return c7_Text; }
            set { c7_Text = value; }
        }
        private byte c8_State;

        public byte C8_State
        {
            get { return c8_State; }
            set { c8_State = value; }
        }
        private string c8_Id;

        public string C8_Id
        {
            get { return c8_Id; }
            set { c8_Id = value; }
        }
        private string c8_Text;

        public string C8_Text
        {
            get { return c8_Text; }
            set { c8_Text = value; }
        }
        private byte c9_State;

        public byte C9_State
        {
            get { return c9_State; }
            set { c9_State = value; }
        }
        private string c9_Id;

        public string C9_Id
        {
            get { return c9_Id; }
            set { c9_Id = value; }
        }
        private string c9_Text;

        public string C9_Text
        {
            get { return c9_Text; }
            set { c9_Text = value; }
        }
        private byte c10_State;

        public byte C10_State
        {
            get { return c10_State; }
            set { c10_State = value; }
        }
        private string c10_Id;

        public string C10_Id
        {
            get { return c10_Id; }
            set { c10_Id = value; }
        }
        private string c10_Text;

        public string C10_Text
        {
            get { return c10_Text; }
            set { c10_Text = value; }
        }

        private byte c11_State;

        public byte C11_State
        {
            get { return c11_State; }
            set { c11_State = value; }
        }
        private string c11_Id;

        public string C11_Id
        {
            get { return c11_Id; }
            set { c11_Id = value; }
        }
        private string c11_Text;

        public string C11_Text
        {
            get { return c11_Text; }
            set { c11_Text = value; }
        }
        private byte c12_State;

        public byte C12_State
        {
            get { return c12_State; }
            set { c12_State = value; }
        }
        private string c12_Id;

        public string C12_Id
        {
            get { return c12_Id; }
            set { c12_Id = value; }
        }
        private string c12_Text;

        public string C12_Text
        {
            get { return c12_Text; }
            set { c12_Text = value; }
        }
        private byte c13_State;

        public byte C13_State
        {
            get { return c13_State; }
            set { c13_State = value; }
        }
        private string c13_Id;

        public string C13_Id
        {
            get { return c13_Id; }
            set { c13_Id = value; }
        }
        private string c13_Text;

        public string C13_Text
        {
            get { return c13_Text; }
            set { c13_Text = value; }
        }
        private byte c14_State;

        public byte C14_State
        {
            get { return c14_State; }
            set { c14_State = value; }
        }
        private string c14_Id;

        public string C14_Id
        {
            get { return c14_Id; }
            set { c14_Id = value; }
        }
        private string c14_Text;

        public string C14_Text
        {
            get { return c14_Text; }
            set { c14_Text = value; }
        }
        private byte c15_State;

        public byte C15_State
        {
            get { return c15_State; }
            set { c15_State = value; }
        }
        private string c15_Id;

        public string C15_Id
        {
            get { return c15_Id; }
            set { c15_Id = value; }
        }
        private string c15_Text;

        public string C15_Text
        {
            get { return c15_Text; }
            set { c15_Text = value; }
        }
        private byte c16_State;

        public byte C16_State
        {
            get { return c16_State; }
            set { c16_State = value; }
        }
        private string c16_Id;

        public string C16_Id
        {
            get { return c16_Id; }
            set { c16_Id = value; }
        }
        private string c16_Text;

        public string C16_Text
        {
            get { return c16_Text; }
            set { c16_Text = value; }
        }
        private byte c17_State;

        public byte C17_State
        {
            get { return c17_State; }
            set { c17_State = value; }
        }
        private string c17_Id;

        public string C17_Id
        {
            get { return c17_Id; }
            set { c17_Id = value; }
        }
        private string c17_Text;

        public string C17_Text
        {
            get { return c17_Text; }
            set { c17_Text = value; }
        }
        private byte c18_State;

        public byte C18_State
        {
            get { return c18_State; }
            set { c18_State = value; }
        }
        private string c18_Id;

        public string C18_Id
        {
            get { return c18_Id; }
            set { c18_Id = value; }
        }
        private string c18_Text;

        public string C18_Text
        {
            get { return c18_Text; }
            set { c18_Text = value; }
        }
        private byte c19_State;

        public byte C19_State
        {
            get { return c19_State; }
            set { c19_State = value; }
        }
        private string c19_Id;

        public string C19_Id
        {
            get { return c19_Id; }
            set { c19_Id = value; }
        }
        private string c19_Text;

        public string C19_Text
        {
            get { return c19_Text; }
            set { c19_Text = value; }
        }
        private byte c20_State;

        public byte C20_State
        {
            get { return c20_State; }
            set { c20_State = value; }
        }
        private string c20_Id;

        public string C20_Id
        {
            get { return c20_Id; }
            set { c20_Id = value; }
        }
        private string c20_Text;

        public string C20_Text
        {
            get { return c20_Text; }
            set { c20_Text = value; }
        }
        private char control_state;

        public char Control_state
        {
            get { return control_state; }
            set { control_state = value; }
        }
        private char curgroup_state;

        public char Curgroup_state
        {
            get { return curgroup_state; }
            set { curgroup_state = value; }
        }
        private char curdepartment_state;

        public char Curdepartment_state
        {
            get { return curdepartment_state; }
            set { curdepartment_state = value; }
        }
        public char curcompany_state;
    }
}
