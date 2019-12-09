using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using cf_pad.CLS;

namespace cf_pad.Forms
{
    public partial class frmMachineStdQty : Form
    {
        string strWhere = "";
        string lang_id = DBUtility._language;
        string user_id = DBUtility._user_id;
        string pad_db = DBUtility.pad_db;
       
        clsCommonUse commUse = new clsCommonUse();
        public frmMachineStdQty()
        {
            InitializeComponent();
        }

        private void BTNEXIT_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void count_std_qty()
        {
            if (txtLineNo.Text != "" && txtRunNo.Text != "")
                txtStdQty.Text = (Convert.ToInt32(txtLineNo.Text) * Convert.ToInt32(txtRunNo.Text)).ToString();
        }

        private void txtLineNo_Leave(object sender, EventArgs e)
        {
            count_std_qty();
        }

        private void txtRunNo_Leave(object sender, EventArgs e)
        {
            count_std_qty();
        }

        private void BTNFIND_Click(object sender, EventArgs e)
        {
            strWhere = " WHERE a.dep like '%" + txtFindDep.Text.Trim() + "%'" + " and a.machine_id like '%" + txtFindMachine.Text.Trim() + "%'";
            this.BindDataGridView(strWhere);
        }
        private void BindDataGridView(string strWhere)
        {
            string strSql = null;

            strSql = "SELECT a.dep,a.machine_id,a.machine_mul,a.machine_rate,a.machine_std_qty";
            strSql += " FROM " + pad_db + "machine_std a " + strWhere;
            strSql += " ORDER BY a.dep,a.machine_id";
            try
            {
                this.dgvDetails.DataSource = commUse.GetDataSet(strSql, "machine_std").Tables["machine_std"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "软件提示");
                throw ex;
            }
            if (dgvDetails.RowCount > 0)
            {
                FillControls();
            }
        }
        private void FillControls()
        {
            this.txtDep.Text = this.dgvDetails[0, this.dgvDetails.CurrentCell.RowIndex].Value.ToString().Trim();
            this.txtMachine.Text = this.dgvDetails[1, this.dgvDetails.CurrentCell.RowIndex].Value.ToString().Trim();
            this.txtLineNo.Text = this.dgvDetails[2, this.dgvDetails.CurrentCell.RowIndex].Value.ToString().Trim();
            this.txtRunNo.Text = this.dgvDetails[3, this.dgvDetails.CurrentCell.RowIndex].Value.ToString().Trim();
            this.txtStdQty.Text = this.dgvDetails[4, this.dgvDetails.CurrentCell.RowIndex].Value.ToString().Trim();
        }

        private void dgvDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvDetails.RowCount > 0)
            {
                FillControls();
            }
        }

        private void BTNSAVE_Click(object sender, EventArgs e)
        {
            string strCode = "";
            if (chk_data() == false)
                return;
            try
            {
                strCode = "UPDATE " + pad_db + "machine_std SET machine_mul=@machine_mul,machine_rate=@machine_rate,machine_std_qty=@machine_std_qty ";
                strCode += " WHERE dep = @dep AND machine_id=@machine_id ";

                ParametersAddValue();

                if (commUse.ExecDataBySql(strCode) > 0)
                {
                    MessageBox.Show("儲存成功！", "系統信息");
                    strWhere = " WHERE a.dep = " + "'" + txtDep.Text.Trim() + "'" + " and a.machine_id = " + "'" + txtMachine.Text.Trim() + "'";
                    this.BindDataGridView(strWhere);
                }
                else
                {
                    MessageBox.Show("儲存失敗！", "系統信息");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系統信息");
                throw ex;
            }
        }
        private void ParametersAddValue()
        {
            commUse.Cmd.Parameters.Clear();
            commUse.Cmd.Parameters.AddWithValue("@dep", txtDep.Text.Trim());
            commUse.Cmd.Parameters.AddWithValue("@machine_id", txtMachine.Text.Trim());
            commUse.Cmd.Parameters.AddWithValue("@machine_mul", Convert.ToInt32(txtLineNo.Text));
            commUse.Cmd.Parameters.AddWithValue("@machine_rate", Convert.ToInt32(txtRunNo.Text));
            commUse.Cmd.Parameters.AddWithValue("@machine_std_qty", Convert.ToInt32(txtStdQty.Text));
        }
        private bool chk_data()
        {
            bool chk_flag=true;
            if (txtLineNo.Text == "")
            {
                chk_flag = false;
                MessageBox.Show("行數無效！", "系統信息");
                txtLineNo.Focus();
            }
            if (txtRunNo.Text == "")
            {
                chk_flag = false;
                MessageBox.Show("轉數無效！", "系統信息");
                txtRunNo.Focus();
            }
            if (txtStdQty.Text == "")
            {
                chk_flag = false;
                MessageBox.Show("每小時標準數量無效！", "系統信息");
                txtStdQty.Focus();
            }
            return chk_flag;
        }

        private void frmMachineStdQty_Load(object sender, EventArgs e)
        {
            Font a = new Font("GB2312", 14);//GB2312为字体名称，1为字体大小dataGridView1.Font = a;
            dgvDetails.Font = a;
        }
    }
}
