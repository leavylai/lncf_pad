using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;//反射

namespace cf_pad
{
    public partial class frmMainForPad : Form
    {
        public static frmMainForPad pMainWin;
        public string user_id;
        public string user_name;
        public static bool isRunMain;
        public frmMainForPad()
        {
            InitializeComponent();
            pMainWin = this;
        }

        private void RunNewForm(string path_formname)
        {
            string formname = "";
            formname = path_formname.Substring(path_formname.LastIndexOf(".") + 1, (path_formname.Length - (path_formname.LastIndexOf(".") + 1)));

            if (checkChildFrmExist(formname) == false)
            {
                Assembly asb = Assembly.GetExecutingAssembly();//得到当前的程序集
                Form f = (Form)asb.CreateInstance("cf_pad." + path_formname);//利用反射，根据数据库中的字段值创建窗体对象 
                //f.MdiParent = this;
                f.WindowState = FormWindowState.Maximized;
                //f.Show();
                f.ShowDialog();
            }
        }
        private static bool checkChildFrmExist(string childFrmName)
        {
            bool isExist_flag = false;
            foreach (Form childFrm in frmMainForPad.pMainWin.MdiChildren)
            {
                if (childFrm.Name == childFrmName)
                {
                    if (childFrm.WindowState == FormWindowState.Minimized)
                    {
                        childFrm.WindowState = FormWindowState.Maximized;
                    }
                    childFrm.Activate();
                    isExist_flag = true;
                    break;
                }
            }
            return isExist_flag;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            RunNewForm("Forms.frmPrdSchedule");//生產進度表(新)
            //RunNewForm("Forms.frmPrdSchedule");//生產進度表
        }


        private void btnShowPlan_Click(object sender, EventArgs e)
        {
            RunNewForm("Forms.frmShowPlan");//查看生產計劃
            //RunNewForm("Forms.frmPrdSchedule");//查看生產計劃
        }

        private void btnPrdSelect_Click(object sender, EventArgs e)
        {
            RunNewForm("Forms.frmPrdSelect");//選貨
        }


    }
}
