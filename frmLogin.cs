using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using cf_pad.CLS;

namespace cf_pad
{
    public partial class frmLogin : Form
    {
        private string strBarCode = "";
        public frmLogin()
        {
            InitializeComponent();
        }

        //自定義屬性
        private bool _isPass;
        public bool isPass
        {
            get
            {
                return _isPass;
            }
            set
            {
                _isPass = value;
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.isPass = false;
            //txtUserid.Text = DBUtility.GetAppConfig("userid");
            //txtUserName.Text = DBUtility.GetAppConfig("userName");

            string ilang = DBUtility.GetAppConfig("language");//獲取默認的語言
            int i = Convert.ToInt16(ilang);
            cmbLanguage.Text = cmbLanguage.Items[i].ToString();


            //設置默認的獲得焦點的控件
            this.ActiveControl = txtBarCode;  //設置獲得點的控件必須與txtPassword.Focus()一起使用否則不起作用
            //txtPassword.Focus();
            //this.AcceptButton = btn_ok;//設置btn_ok按鈕響應Enter鍵           

        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            string userid = txtUserid.Text.Trim();
            string pwd = txtPassword.Text.Trim();

            if (userid != "")
            {
                if (this.isPass != true)  //用戶直接按確定鈕
                {
                    txtUserName.Text = clsUser.IsExistUser(userid);
                    if (txtUserName.Text != "")
                    {
                        this.isPass = true;
                    }
                }
                if (this.isPass) //用戶帳號正確
                {
                    if (clsUser.GetUserInfo(userid, pwd)) //傳兩個參數,以判斷當前用戶密碼是否正確
                    {
                        
                        DBUtility._user_id = userid;  //2014-08-15 因取消SaveLoginInfo()而增加此行代碼
                        DBUtility._language = cmbLanguage.SelectedIndex.ToString(); //設置用戶登入語言臨時公共變量   2014-08-01 因取消SaveLoginInfo()而增加此行代碼
                        frmMainForPad.isRunMain = true;
                        //關閉Login窗體
                        this.Close();
                        
                    }
                    else
                    {
                        txtPassword.Focus();
                        txtPassword.SelectAll();
                    }
                }
                else
                {
                    txtUserName.Text = "";
                    txtUserid.Focus();
                    txtUserid.SelectAll();
                }
            }
            else
            {
                MessageBox.Show("用戶帳號不可為空！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUserid.Focus();
            }
        }

        private void SaveLoginInfo()
        {
            //保存用戶信息
            string strUserid = txtUserid.Text;
            string strUserName = txtUserName.Text;
            string strLanguage = cmbLanguage.SelectedIndex.ToString();
            if (strUserid != DBUtility.GetAppConfig("userid"))
            {
                DBUtility.UpdateAppConfig("userid", strUserid);

            }
            else
            {
                DBUtility._user_id = strUserid; //基類表單調用
            }
            if (strUserName != DBUtility.GetAppConfig("userName"))
            {
                DBUtility.UpdateAppConfig("userName", strUserName);
            }
            if (strLanguage != DBUtility.GetAppConfig("language")) //暫時不用,改為在權限設置中設定
            {
                DBUtility.UpdateAppConfig("language", cmbLanguage.SelectedIndex.ToString());
            }


            //判斷當前用戶選擇的語言與數據中保存的語言如不同，則更新數據中該用戶的登入語言
            string strSql = String.Format("Select language From  dbo.tb_sy_user where uname='{0}'", strUserid);
            DataTable dt = DBUtility.GetDataTable(strSql);
            DataRow[] dr = dt.Select();
            string strlang = dr[0]["language"].ToString().Trim();
            dt.Dispose();
            if (strlang != strLanguage)
            {
                try
                {
                    clsUser.UpdateUserLanguage(strLanguage, strUserid);
                    DBUtility._language = strLanguage; //設置用戶登入語言臨時公共變量                
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtUserid_Validated(object sender, EventArgs e)
        {
            string strUserid = txtUserid.Text.Trim();
            if (strUserid != "")  //輸入的帳號是否為空
            {
                string UserName = clsUser.IsExistUser(strUserid);
                if (UserName != "")  //返回的用戶名不為空，說明用戶存在
                {
                    txtUserName.Text = UserName;
                    this.isPass = true;
                }
                else
                {
                    txtUserName.Text = "";
                    txtUserid.Text = "";
                    txtUserid.Focus();
                    this.isPass = false;
                }
            }
            else
            {
                txtUserName.Text = "";
                this.isPass = false;
            }
        }

        private void txtPassword_MouseDown(object sender, MouseEventArgs e)
        {
            //clsUtility.Call_imput();
            //clsUtility.StartImput();
        }

        private void txtUserid_MouseDown(object sender, MouseEventArgs e)
        {
            //clsUtility.Call_imput();
            
        }

        private void txtBarCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    //掃描制單編號，物料編號

                    strBarCode = txtBarCode.Text.Trim().ToUpper();
                    int barcode_length=strBarCode.Length;
                    int cno = 0;
                    barcode_length = strBarCode.Length;
                    if (barcode_length >= 10)
                    {
                        cno = strBarCode.IndexOf("/");
                        txtUserid.Text = strBarCode.Substring(0, cno);
                        txtPassword.Text = strBarCode.Substring(cno + 1, barcode_length - (cno + 1));
                        btn_ok_Click(sender, e);
                    }
                    txtBarCode.Text = "";
                    break;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btn_ok_Click(null, new EventArgs());
        }

        private void btnImput_Click(object sender, EventArgs e)
        {
            clsUtility.StartImput();
        }



    }
}
