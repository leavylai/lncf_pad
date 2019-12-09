using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using cf_pad.CLS;
using cf_pad.Reports;

namespace cf_pad.Forms
{
    public partial class frmShowPlan : Form
    {
        private DataTable dtDataForPrint = new DataTable();
        private DataTable dtParts = new DataTable();
        private DataTable dtPlanDetails = new DataTable();

        public frmShowPlan()
        {
            InitializeComponent();
        }

        private void frmShowPlan_Load(object sender, EventArgs e)
        {
            Font a = new Font("GB2312", 14);

            this.gvDetails.BestFitColumns();
        }


        private void txtMo_id_v_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txtMo_id_v.Text == "")
                    {
                        MessageBox.Show("請輸入查詢條件！");
                        return;
                    }
                    GetProductionPlan(txtMo_id_v.Text.Trim());
                    break;
                default:
                    break;
            }
        }

        private void GetProductionPlan(string pMo_id)
        {
            DataTable dtPlanMostly = CLS.clsShowProductionPlan.GetProductionPlanMostly(pMo_id);
            if (dtPlanMostly.Rows.Count > 0)
            {
                txtId.Text = dtPlanMostly.Rows[0]["id"].ToString();
                txtId_ver.Text = dtPlanMostly.Rows[0]["ver"].ToString();
                txtBill_date.Text = string.IsNullOrEmpty(dtPlanMostly.Rows[0]["bill_date"].ToString()) ? "" : Convert.ToDateTime(dtPlanMostly.Rows[0]["bill_date"]).ToString("yyyy/MM/dd");
                txtOrder_no.Text = dtPlanMostly.Rows[0]["order_no"].ToString();
                txtMo_id.Text = dtPlanMostly.Rows[0]["mo_id"].ToString();
                txtMo_ver.Text = dtPlanMostly.Rows[0]["mo_ver"].ToString();
                txtOrder_qty.Text = Convert.ToInt32(dtPlanMostly.Rows[0]["order_qty"]).ToString();
                txtQty_unit.Text = dtPlanMostly.Rows[0]["goods_unit"].ToString();
                txtMerchandiser.Text = dtPlanMostly.Rows[0]["name"].ToString();
                txtCustomer_id.Text = dtPlanMostly.Rows[0]["customer_id"].ToString();
                txtCustomer_name.Text = dtPlanMostly.Rows[0]["cust_name"].ToString();
                txtPlanbackDate.Text = string.IsNullOrEmpty(dtPlanMostly.Rows[0]["f_production_date"].ToString()) ? "" : Convert.ToDateTime(dtPlanMostly.Rows[0]["f_production_date"]).ToString("yyyy/MM/dd");
                txtDelivery_date.Text = string.IsNullOrEmpty(dtPlanMostly.Rows[0]["t_production_date"].ToString()) ? "" : Convert.ToDateTime(dtPlanMostly.Rows[0]["t_production_date"]).ToString("yyyy/MM/dd");
                txtProduction_remark.Text = dtPlanMostly.Rows[0]["production_remark"].ToString();
                txtPlate_remark.Text = dtPlanMostly.Rows[0]["plate_remark"].ToString();
                txtMo_remark.Text = dtPlanMostly.Rows[0]["free"].ToString();
                txtRemark.Text = dtPlanMostly.Rows[0]["remark"].ToString();
                txtCreate_by.Text = dtPlanMostly.Rows[0]["create_by"].ToString();
                txtCreate_date.Text = string.IsNullOrEmpty(dtPlanMostly.Rows[0]["create_date"].ToString()) ? "" : Convert.ToDateTime(dtPlanMostly.Rows[0]["create_date"]).ToString("yyyy/MM/dd");
                txtCheck_by.Text = dtPlanMostly.Rows[0]["check_by"].ToString();
                txtCheck_date.Text = string.IsNullOrEmpty(dtPlanMostly.Rows[0]["check_date"].ToString()) ? "" : Convert.ToDateTime(dtPlanMostly.Rows[0]["check_date"]).ToString("yyyy/MM/dd");
                txtState.Text = dtPlanMostly.Rows[0]["prd_State"].ToString();

                dtPlanDetails = CLS.clsShowProductionPlan.GetProductionPlanDetails(txtId.Text.Trim());
                if (dtPlanDetails.Rows.Count > 0)
                {
                    //dgvPlanDetails.AutoGenerateColumns = false;
                    //dgvPlanDetails.DataSource = dtPlanDetails;
                    //dgvPlanDetails.Refresh();
                    dtPlanDetails.Columns.Add("check_value", System.Type.GetType("System.Boolean"));
                    gridControl1.DataSource = dtPlanDetails;

                    //FillTextBox(dgvPlanDetails.CurrentRow.Index);
                }
                this.tabControl1.SelectedTab = tabPage2;
            }
            else
            {
                MessageBox.Show("未查詢到數據，請重新輸入條件后查詢。");
                txtMo_id_v.Focus();
                txtMo_id_v.SelectAll();
                ClearText();
            }
        }

        private void dgvPlanDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;

            FillTextBox(e.RowIndex);
        }

        /// <summary>
        /// 填充生產明細 
        /// </summary>
        /// <param name="Index"></param>
        private void FillTextBox(int Index)
        {
            txtProd_state.Text = dtPlanDetails.Rows[Index]["prod_state"].ToString();
            txtHold.Text = dtPlanDetails.Rows[Index]["hold"].ToString();
            txtFlag.Text = dtPlanDetails.Rows[Index]["flag"].ToString();
            txtWp_dept.Text = dtPlanDetails.Rows[Index]["wp_id"].ToString();
            txtGoods_id.Text = dtPlanDetails.Rows[Index]["goods_id"].ToString();
            txtNext_wp_dept.Text = dtPlanDetails.Rows[Index]["next_wp_id"].ToString();
            txtGoods_name.Text = dtPlanDetails.Rows[Index]["goods_name"].ToString();
            txtS_qty.Text = dtPlanDetails.Rows[Index]["s_qty"].ToString();
            txtProd_qty.Text = dtPlanDetails.Rows[Index]["prod_qty"].ToString();
            txtObligate_qty.Text = dtPlanDetails.Rows[Index]["obligate_qty"].ToString();
            txtComplete_qty.Text = dtPlanDetails.Rows[Index]["c_qty_ok"].ToString();
            txtComplete_sec_qty.Text = dtPlanDetails.Rows[Index]["c_sec_qty_ok"].ToString();
            txtPre_dept.Text = dtPlanDetails.Rows[Index]["pre_dept"].ToString();
            txtF_complete_date.Text = dtPlanDetails.Rows[Index]["f_complete_date"].ToString();

            string strImagePath = DBUtility.imagePath + CLS.clsShowProductionPlan.GetImagePath(dtPlanDetails.Rows[Index]["goods_id"].ToString());
            if (File.Exists(strImagePath))
            {
                picBox.Image = Image.FromFile(strImagePath);
            }
            else
            {
                picBox.Image = null;
            }
        }

        private void ClearText()
        {
            /******* tabPage1*******/
            txtId.Text = "";
            txtId_ver.Text = "";
            txtBill_date.Text = "";
            txtOrder_no.Text = "";
            txtMo_id.Text = "";
            txtMo_ver.Text = "";
            txtOrder_qty.Text = "";
            txtQty_unit.Text = "";
            txtMerchandiser.Text = "";
            txtCustomer_id.Text = "";
            txtCustomer_name.Text = "";
            txtPlanbackDate.Text = "";
            txtDelivery_date.Text = "";
            txtProduction_remark.Text = "";
            txtPlate_remark.Text = "";
            txtMo_remark.Text = "";
            txtRemark.Text = "";
            txtCreate_by.Text = "";
            txtCreate_date.Text = "";
            txtCheck_by.Text = "";
            txtCheck_date.Text = "";
            txtState.Text = "";

            /******* tabPage2*******/
            txtProd_state.Text = "";
            txtHold.Text = "";
            txtFlag.Text = "";
            txtWp_dept.Text = "";
            txtGoods_id.Text = "";
            txtNext_wp_dept.Text = "";
            txtGoods_name.Text = "";
            txtS_qty.Text = "";
            txtProd_qty.Text = "";
            txtObligate_qty.Text = "";
            txtComplete_qty.Text = "";
            txtComplete_sec_qty.Text = "";
            txtPre_dept.Text = "";
            txtF_complete_date.Text = "";
            picBox.Image = null;
        }


        /// <summary>
        /// 生成打印數據
        /// </summary>
        private void GenerateDataForPrint()
        {
            dtDataForPrint.Rows.Clear();
            dtParts.Rows.Clear();
            string Mo_id = txtMo_id.Text.Trim();

            DataTable dtTransfer = clsPrdTransfer.GetTransferInfo(Mo_id);
            for (int i = 0; i < dtTransfer.Rows.Count; i++)
            {
                //if (gvDetails.GetDataRow(i)["check_value"].ToString()=="True")
                //{
                string wp_id = dtTransfer.Rows[i]["in_dept"].ToString();
                string mater_id = dtTransfer.Rows[i]["goods_id"].ToString();

                DataTable dtTempData = clsPrdTransfer.GetPOData(Mo_id, wp_id, mater_id);
                if (dtTempData.Rows.Count > 0)
                {
                    if (dtDataForPrint.Rows.Count > 0)
                    {
                        for (int j = 0; j < dtTempData.Rows.Count; j++)
                        {
                            //dtDataForPrint.Rows.Add(dtTempData.Rows[j].ItemArray);
                            dtDataForPrint.ImportRow(dtTempData.Rows[j]);
                        }
                    }
                    else
                    {
                        dtDataForPrint = dtTempData;
                    }
                }
                //}
            }

            ///配件名稱及顏色
            if (dtDataForPrint.Rows.Count > 0)
            {
                //配件名稱及顏色
                DataTable dtTempParts = clsPrdTransfer.GetPartsOfColor(Mo_id);
                if (dtParts.Rows.Count > 0)
                {
                    for (int j = 0; j < dtTempParts.Rows.Count; j++)
                    {
                        // dtParts.Rows.Add(dtTempParts.Rows[j].ItemArray);
                        dtParts.ImportRow(dtTempParts.Rows[j]);
                    }
                }
                else
                {
                    dtParts = dtTempParts;
                }
            }
        }

        private void gvDetails_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            int RowIndex = gvDetails.FocusedRowHandle;
            txtProd_state.Text = gvDetails.GetRowCellValue(RowIndex, "prod_state").ToString();
            txtHold.Text = gvDetails.GetRowCellValue(RowIndex, "hold").ToString();
            txtFlag.Text = gvDetails.GetRowCellValue(RowIndex, "flag").ToString();
            txtWp_dept.Text = gvDetails.GetRowCellValue(RowIndex, "wp_id").ToString();
            txtGoods_id.Text = gvDetails.GetRowCellValue(RowIndex, "goods_id").ToString();
            txtNext_wp_dept.Text = gvDetails.GetRowCellValue(RowIndex, "next_wp_id").ToString();
            txtGoods_name.Text = gvDetails.GetRowCellValue(RowIndex, "goods_name").ToString();
            txtS_qty.Text = gvDetails.GetRowCellValue(RowIndex, "s_qty").ToString();
            txtProd_qty.Text = gvDetails.GetRowCellValue(RowIndex, "prod_qty").ToString();
            txtObligate_qty.Text = gvDetails.GetRowCellValue(RowIndex, "obligate_qty").ToString();
            txtComplete_qty.Text = gvDetails.GetRowCellValue(RowIndex, "c_qty_ok").ToString();
            txtComplete_sec_qty.Text = gvDetails.GetRowCellValue(RowIndex, "c_sec_qty_ok").ToString();
            txtPre_dept.Text = gvDetails.GetRowCellValue(RowIndex, "pre_dept").ToString();
            txtF_complete_date.Text = gvDetails.GetRowCellValue(RowIndex, "f_complete_date").ToString();

            string strImagePath = DBUtility.imagePath + CLS.clsShowProductionPlan.GetImagePath(gvDetails.GetRowCellValue(RowIndex, "goods_id").ToString());
            if (File.Exists(strImagePath))
            {
                picBox.Image = Image.FromFile(strImagePath);
            }
            else
            {
                picBox.Image = null;
            }
        }

        private void gvCheck_Click(object sender, EventArgs e)
        {
            int RowIndex = gvDetails.FocusedRowHandle;
            if (RowIndex >= 0)
            {
                //首先获取到多选列的值
                string val = gvDetails.GetDataRow(RowIndex)["check_value"].ToString();
                if (val == "True")
                {
                    gvDetails.GetDataRow(RowIndex)["check_value"] = false;  //如果是已选中就设置未选中的值為 false
                }
                else
                {
                    gvDetails.GetDataRow(RowIndex)["check_value"] = true;   //如果是未选中就设置选中的值為 true
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetProductionPlan(txtMo_id_v.Text.Trim());
        }


    }
}
