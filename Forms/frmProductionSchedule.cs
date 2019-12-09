using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using cf01.ModuleClass;
using RUI.PC;
using cf_pad.MDL;
using cf_pad.CLS;

namespace cf_pad.Forms
{
    public partial class frmProductionSchedule : Form
    {
        DataTable dtPrd_dept = new DataTable();
        DataTable dtMo_item = new DataTable();
        DataTable dtWork_type = new DataTable();
        DataTable dtMachine_std = new DataTable();
        DataTable dtGroup = new DataTable();//組別
        DataTable dtProductionRecordslist = new DataTable();
        private string remote_db = DBUtility.remote_db;
        private string edit_type="Y";//控制當控件中當值發生變化時的操作
        private clsUtility.enumOperationType OperationType;
        private int Result = 0;
        private string _userid = DBUtility._user_id.ToUpper();
        private product_records objModel;
        private int record_id = -1;//未完成記錄的ID，若查找到，則說明未完成，在保存時，執行更新操作

        public frmProductionSchedule()
        {
            InitializeComponent();

            //clsControlInfoHelper controlInfo = new clsControlInfoHelper("frmProductionSchedule", this.Controls);
            //controlInfo.GenerateContorl();

            GetAllComboxData();
        }

        private void frmProductionSchedule_Load(object sender, EventArgs e)
        {
            InitComBoxs();

            //加載時讓條碼框獲得焦點
            txtBarCode.Focus();

            Font a = new Font("GB2312", 14);//GB2312为字体名称，1为字体大小dataGridView1.Font = a;
            dgvAndSingle.Font = a;
            dgvDetails.Font = a;
            dgvDetails.AutoGenerateColumns = false;
            if (cmbProductDept.Text=="302")
                lblStandard_per_qty.Text = "標準時產能";
        }

        //獲取生產部門、工作類型
        private void GetAllComboxData()
        {
            try
            {
                dtPrd_dept = clsProductionSchedule.GetAllPrd_dept();
                dtWork_type = clsProductionSchedule.GetWorkType();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitComBoxs()
        {
            //初始化生產部門
            cmbProductDept.DataSource = dtPrd_dept;
            cmbProductDept.DisplayMember = "int9loc";
            cmbProductDept.ValueMember = "int9loc";
            string userid_part=_userid.Substring(0, 3);
            if (userid_part == "BUT")
            {
                cmbProductDept.Text = "102";
            }
            else
            {
                if (userid_part == "ALY")
                    cmbProductDept.Text = "302";
                else
                {
                    if (userid_part == "BLK")
                        cmbProductDept.Text = "105";
                    else
                    {
                        if (userid_part == "BLP")
                            cmbProductDept.Text = "107";
                    }
                }
            }
            if (_userid == "BUK01")
                cmbProductDept.Text = "202";
            else
            {
                if (_userid == "BUK02")
                    cmbProductDept.Text = "203";
            }
            //初始化工作類型
            cmbWorkType.DataSource = dtWork_type;
            cmbWorkType.DisplayMember = "work_type_desc";
            cmbWorkType.ValueMember = "work_type_id";

            InitComBoxGroup();
            GetJobType();//提取工種
            //初始化班次、組別
            cmbOrder_class.Items.Add("白班");
            cmbOrder_class.Items.Add("夜班");
            cmbOrder_class.Text = "白班";

            SetControlVisible();//設置控件可見

            dteProdcutDate.Text = System.DateTime.Now.ToString("yyyy/MM/dd");

        }

        //提取工作組別
        private void InitComBoxGroup()
        {
            string strSql = "";
            strSql = " SELECT work_group,group_desc FROM work_group WHERE ( dep='" + cmbProductDept.Text.Trim() + "'" + " AND group_type='" + "1" + "') " + " OR dep='" + "000" + "' ";
            try
            {
                dtGroup = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                if (dtGroup.Rows.Count > 0)
                {
                    cmbGroup.DataSource = dtGroup;
                    cmbGroup.DisplayMember = "work_group";
                    cmbGroup.ValueMember = "work_group";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (cmbProductDept.Text == "105" || cmbProductDept.Text == "203")
                cmbWorkType.Text = "生產";
            if (cmbProductDept.Text == "105")
            {
                if (_userid == "BLK01")
                    cmbGroup.Text = "BC01";
                else
                {
                    if (_userid == "BLK02")
                        cmbGroup.Text = "BC02";
                    else
                    {
                        if (_userid == "BLK03")
                            cmbGroup.Text = "BC03";
                        else
                        {
                            if (_userid == "BLK04")
                                cmbGroup.Text = "BC04";
                            else
                            {
                                if (_userid == "BLK05")
                                {
                                    cmbGroup.Text = "BC05";
                                    cmbWorkType.Text = "選貨";
                                }
                            }
                        }
                    }
                }
            }
        }
        //檢查工種
        private bool CheckJobType()
        {
            string strSql = "";
            string job_type = txtJob_type.Text.Trim();
            if (cmbProductDept.Text.Trim() == "202")
            {
                if(cmbGroup.Text=="KB01" || cmbGroup.Text=="KF01")
                    job_type = cmbJob_type.SelectedValue.ToString().Trim();
            }
            DataTable dtJobType = new DataTable();
            strSql = " SELECT id FROM " + remote_db + "cd_type_work WHERE within_code='0000' AND dept_id='" + cmbProductDept.Text.Trim() + "' " + " and id='" + job_type + "'";
            try
            {
                dtJobType = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                if (dtJobType.Rows.Count == 0)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return true;
        }
        //提取工種
        private void GetJobType()
        {
            string strSql = "";
            DataTable dtJobType = new DataTable();
            strSql = " SELECT id,id+name AS type_name FROM " + remote_db + "cd_type_work WHERE within_code='0000' AND dept_id='" + cmbProductDept.Text.Trim() + "' ";
            try
            {
                dtJobType = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                cmbJob_type.DataSource = dtJobType;
                cmbJob_type.DisplayMember = "type_name";
                cmbJob_type.ValueMember = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void txtmo_id_Leave(object sender, EventArgs e)
        {
            cmbGoods_id.Text = "";
            txtgoods_desc.Text = "";
            if (txtmo_id.Text != "" && cmbProductDept.Text != "")
            {
                GetMo_itme("");
            }
        }

        //獲取制單編號資料，并綁定物料編號
        private void GetMo_itme(string item)
        {
            cmbGoods_id.Items.Clear();
            try
            {
                string dep = cmbProductDept.SelectedValue.ToString();
                if (dep == "104")//如果是104幫102加工的，則將部門改成102來提取記錄
                    dep = "102";
                dtMo_item = clsProductionSchedule.GetMo_dataById(txtmo_id.Text.Trim(), dep, item);
                if (dtMo_item.Rows.Count > 0)
                {
                    for (int i = 0; i < dtMo_item.Rows.Count; i++)
                    {
                        cmbGoods_id.Items.Add(dtMo_item.Rows[i]["goods_id"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //查詢未完成的記錄，並重新賦值，便於重新輸入完整資料
        private void get_prd_records(int con_type)
        {
            string last_date = System.DateTime.Now.AddDays(-7).ToString("yyyy/MM/dd");
            try
            {
                //獲取制單編號資料
                string sql = "";
                if (con_type != 8)
                {
                    sql =" Select a.*,rtrim(b.work_type_desc) as work_type_desc " +
                    " From product_records a with(nolock) " +
                    " Left outer join work_type b with(nolock) on a.prd_work_type=b.work_type_id ";
                    sql += " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'";
                    if (cmbProductDept.SelectedValue.ToString() == "302")//如果是合金部，則不顯示選貨的記錄
                        sql += " And a.prd_work_type <> " + "'" + "A03" + "'";
                    if (con_type == 1)//是否查找當日未完成標識
                    {
                        sql += " And a.prd_mo = " + "'" + txtmo_id.Text.ToString() + "'";
                        sql += " And a.prd_item = " + "'" + cmbGoods_id.Text.ToString() + "'";
                        sql += " Order By a.amtim Desc,a.crtim Desc ";
                    }
                    else
                    {
                        if (con_type == 2)//未完成的記錄，用生產時間査，不是安排時間，因為一生產就已經是當日的日期了
                        {
                            sql += " And a.prd_date >= " + "'" + last_date + "'";
                            sql += " And a.prd_start_time <> " + "'" + "" + "'" + " And a.prd_end_time = " + "'" + "" + "'";
                            sql += " Order By a.prd_date desc,a.amtim Desc ";
                        }
                        else
                        {
                            if (con_type == 3)//如果是查找當日所有記錄
                            {
                                sql += " And a.prd_date = " + "'" + dteProdcutDate.Text + "'";
                                sql += " Order By a.prd_date desc,a.amtim Desc ";
                            }
                            else
                            {
                                if (con_type == 4)//未開始生產的記錄
                                {
                                    sql += " And a.prd_pdate >= " + "'" + last_date + "'";
                                    sql += " And a.prd_start_time = " + "'" + "" + "'" + " And a.prd_end_time = " + "'" + "" + "'";
                                }
                                else
                                    if (con_type == 5)//當天完成的記錄，如果組別不為空，也按組別過濾
                                    {
                                        sql += " And a.prd_date = " + "'" + dteProdcutDate.Text + "'";
                                        if (cmbGroup.Text.Trim() != "")
                                            sql += " And a.prd_group = " + "'" + cmbGroup.Text.Trim() + "'";
                                        sql += " And a.prd_start_time <> " + "'" + "" + "'" + " And a.prd_end_time <> " + "'" + "" + "'";
                                        sql += " Order By a.prd_date desc,a.amtim Desc ";
                                    }
                                    else
                                    {
                                        if (con_type == 6)//按制單編號查詢未完成的記錄
                                        {
                                            sql += " And a.prd_mo like " + "'%" + txtSearchMo.Text + "%'";
                                            sql += " And a.prd_end_time = " + "'" + "" + "'";
                                            sql += " Order By a.amtim Desc,a.crtim Desc ";
                                        }
                                        else
                                        {
                                            if (con_type == 7)//按制單編號查詢所有記錄
                                            {
                                                sql += " And a.prd_mo like " + "'%" + txtSearchMo.Text + "%'";
                                                sql += " Order By a.amtim Desc,a.crtim Desc ";
                                            }
                                            else
                                            {
                                                if (con_type == 9)//按記錄號查詢生產記錄，並單記錄時使用
                                                {
                                                    sql += " And a.prd_id = " + "'" + record_id.ToString() + "'";
                                                    sql += " Order By a.prd_date desc,a.amtim Desc ";
                                                }
                                                else
                                                {
                                                    if (con_type == 10)//按條形碼查找制單狀態，類似1情形
                                                    {
                                                        sql += " And a.prd_mo = " + "'" + txtBarCodeMo.Text.Trim() + "'";
                                                        sql += " And a.prd_item = " + "'" + txtBarCodeItem.Text.Trim() + "'";
                                                        sql += " Order By a.amtim Desc,a.crtim Desc ";
                                                    }
                                                }
                                            }
                                        }
                                    }
                            }
                        }
                    }
                }
                else//獲取機器最後一次生產記錄
                {
                    string strsql_part;
                    strsql_part = " (Select Max(prd_id) AS prd_id " +
                        " From product_records with(nolock) Where "+
                        " prd_dep= '" + cmbProductDept.Text + "'"
                        + " and prd_date >='"+last_date+"'"
                        + " and prd_machine='" + txtSearch_Machine.Text + "'"
                        +" and prd_start_time <>'"+""+"' and prd_end_time <>'"+""+"'"
                        + " ) c ";
                    
                    sql = " Select a.*,rtrim(b.work_type_desc) as work_type_desc " +
                    " From product_records a with(nolock) " +
                    " Left outer join work_type b with(nolock) on a.prd_work_type=b.work_type_id " +
                    " Inner Join "+strsql_part + " on a.prd_id=c.prd_id";
                }


                dtProductionRecordslist = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);

                //清空並將查找到未完成的記錄填充到各文本框
                //clear_text_box();
                //chk_prd_no_complete();
                //fill_dg_prd();//將查詢到的記錄存入列表
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            //GBW004725
        }

        //默認時，將未完成的記錄填入
        private void chk_prd_no_complete()
        {
            record_id = -1;
            if (dtProductionRecordslist.Rows.Count > 0)
            {
                for (int i = 0; i < dtProductionRecordslist.Rows.Count; i++)
                {
                    if (dtProductionRecordslist.Rows[i]["prd_end_time"].ToString() == "" || dtProductionRecordslist.Rows[i]["prd_end_time"].ToString() == "00:00")
                        fill_exist_record(i);
                }
            }
        }
        //重新填入查找到的記錄
        private void fill_exist_record(int index)
        {
            record_id = Convert.ToInt32(dtProductionRecordslist.Rows[index]["prd_id"].ToString());//更新記錄序號
            dteProdcutDate.Text = dtProductionRecordslist.Rows[index]["prd_date"].ToString();
            cmbOrder_class.Text = dtProductionRecordslist.Rows[index]["prd_class"].ToString();
            cmbGroup.Text = dtProductionRecordslist.Rows[index]["prd_group"].ToString();
            mktPrdPdate.Text = dtProductionRecordslist.Rows[index]["prd_pdate"].ToString();
            txtMachine.Text = dtProductionRecordslist.Rows[index]["prd_machine"].ToString();
            txtProductNo.Text = dtProductionRecordslist.Rows[index]["prd_worker"].ToString();
            cmbWorkType.Text = dtProductionRecordslist.Rows[index]["work_type_desc"].ToString().Trim();
            dtpStart.Value = Convert.ToDateTime("2014/01/01 " + dtProductionRecordslist.Rows[index]["prd_start_time"].ToString());
            dtpEnd.Value = Convert.ToDateTime("2014/01/01 " + dtProductionRecordslist.Rows[index]["prd_end_time"].ToString());
            txtNormal_work.Text = (dtProductionRecordslist.Rows[index]["prd_normal_time"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_normal_time"].ToString() : "");
            txtAdd_work.Text = (dtProductionRecordslist.Rows[index]["prd_ot_time"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_ot_time"].ToString() : "");
            txtRow_qty.Text = (dtProductionRecordslist.Rows[index]["line_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["line_num"].ToString() : "");
            txtPer_Convert_qty.Text = (dtProductionRecordslist.Rows[index]["hour_run_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["hour_run_num"].ToString() : "");
            txtper_Standrad_qty.Text = (dtProductionRecordslist.Rows[index]["hour_std_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["hour_std_qty"].ToString() : "");
            txtPrd_qty.Text = (dtProductionRecordslist.Rows[index]["prd_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_qty"].ToString() : "");
            txtprd_weg.Text = (dtProductionRecordslist.Rows[index]["prd_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_weg"].ToString() : "");
            txtkgPCS.Text = (dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() : "");
            txtDifficulty_level.Text = dtProductionRecordslist.Rows[index]["difficulty_level"].ToString();
            txtMatItem.Text = dtProductionRecordslist.Rows[index]["mat_item"].ToString();
            txtMatDesc.Text = dtProductionRecordslist.Rows[index]["mat_item_desc"].ToString();
            txtMatLot.Text = dtProductionRecordslist.Rows[index]["mat_item_lot"].ToString();
            dtpReqEnd.Text = dtProductionRecordslist.Rows[index]["prd_req_time"].ToString();//預計完成時間，用每次計算的時間
            txtToDep.Text = dtProductionRecordslist.Rows[index]["to_dep"].ToString();
            txtPrd_Run_qty.Text = dtProductionRecordslist.Rows[index]["prd_run_qty"].ToString();
            txtWork_code.Text = dtProductionRecordslist.Rows[index]["work_code"].ToString();
            txtSpeed_lever.Text = dtProductionRecordslist.Rows[index]["speed_lever"].ToString();
            txtPack_num.Text = (dtProductionRecordslist.Rows[index]["pack_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["pack_num"].ToString() : "");
            txtStart_run.Text = (dtProductionRecordslist.Rows[index]["start_run"].ToString() !="0" ? dtProductionRecordslist.Rows[index]["start_run"].ToString():"0");
            txtEnd_run.Text = (dtProductionRecordslist.Rows[index]["end_run"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["end_run"].ToString() : "");
            txtSample_no.Text = (dtProductionRecordslist.Rows[index]["sample_no"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["sample_no"].ToString() : "");
            txtSample_weg.Text = (dtProductionRecordslist.Rows[index]["sample_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["sample_weg"].ToString() : "");
            txtJob_type.Text = dtProductionRecordslist.Rows[index]["job_type"].ToString();
            cmbJob_type.SelectedValue = txtJob_type.Text.ToUpper();
            txtWork_class.Text = dtProductionRecordslist.Rows[index]["work_class"].ToString();
            txtPrd_id_ref.Text = dtProductionRecordslist.Rows[index]["prd_id_ref"].ToString();
            if (txtDifficulty_level.Text.Trim() == "" && dteProdcutDate.Text == "302")//合金部難度設定為1
                txtDifficulty_level.Text = "1";
            setProdDate();//自動設定生產日期為當前日期
            if (cmbProductDept.Text == "105" && cmbGroup.Text == "")
            {
                if (_userid == "BLK01")
                    cmbGroup.Text = "BC01";
                else
                {
                    if (_userid == "BLK02")
                        cmbGroup.Text = "BC02";
                    else
                    {
                        if (_userid == "BLK03")
                            cmbGroup.Text = "BC03";
                        else
                        {
                            if (_userid == "BLK04")
                                cmbGroup.Text = "BC04";
                            else
                            {
                                if (_userid == "BLK05")
                                    cmbGroup.Text = "BC05";
                            }
                        }
                    }
                }
            }
        }
        //自動設定生產日期為當前日期
        private void setProdDate()
        {
            if (dtpStart.Text == "00:00" || dtpEnd.Text == "00:00")//若未有生產日期的，設定為當日生產的
            {
                if (cmbOrder_class.SelectedIndex == 0)//白班
                    dteProdcutDate.Text = System.DateTime.Now.ToString("yyyy/MM/dd");
                else//夜班
                {
                    string now_time=System.DateTime.Now.ToString("yyyy/MM/dd HH:mm").Substring(11,5);
                    if (string.Compare(now_time, "20:30") >= 0 && string.Compare(now_time, "23:59") <= 0)
                        dteProdcutDate.Text = System.DateTime.Now.ToString("yyyy/MM/dd");
                    else
                    {
                        if (string.Compare(now_time, "00:00") >= 0 && string.Compare(now_time, "08:30") <= 0)//如果是凌晨的，當前日期減一日
                            dteProdcutDate.Text = System.DateTime.Now.AddDays(-1).ToString("yyyy/MM/dd");
                        else
                        {
                            dteProdcutDate.Text = System.DateTime.Now.ToString("yyyy/MM/dd");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 綁定列表
        /// </summary>
        private void FillGrid()
        {
            if (dtProductionRecordslist.Rows.Count > 0)
            {
                dgvDetails.DataSource = dtProductionRecordslist;
                dgvDetails.Refresh();
            }
            else
            {
                dgvDetails.DataSource = null;
            }
        }
        //ToolStripButton click 事件集合
        private void ToolStripButtonEvents()
        {
            switch (OperationType)
            {
                case clsUtility.enumOperationType.Find:
                    {
                        get_prd_records(3);//查詢當日所有的記錄
                        FillGrid(); //將查詢到的記錄存入列表
                    }
                    break;
                case clsUtility.enumOperationType.Delete:
                    {
                        if (dgvDetails.Rows.Count > 0)
                        {
                            if (MessageBox.Show("確定要刪除嗎?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                int prd_id = record_id;
                                int re = clsProductionSchedule.DeleteProductionRecords(prd_id);
                                if (re > 0)
                                {
                                    MessageBox.Show("刪除成功!");

                                    get_prd_records(1);//查詢已錄入的記錄
                                    FillGrid(); //將查詢到的記錄存入列表

                                }
                                else
                                {
                                    MessageBox.Show("刪除失敗!");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("沒有要刪除的記錄。");
                        }
                    }
                    break;
                case clsUtility.enumOperationType.Cancel:
                    {
                        ClearAllText();
                    }
                    break;
                case clsUtility.enumOperationType.Save:
                    {
                        AfterSave();  //執行保存后的事件

                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 當保存執行完之後的事件
        /// </summary>
        private void AfterSave()
        {
            if (Result > 0)
            {
                string show_message = "保存成功!";
                try
                {
                    //更新物料每KG對應的數量表
                    if (cmbProductDept.Text.Trim() != "" && cmbGoods_id.Text.Trim() != "" && txtkgPCS.Text.Trim() != "" && cmbWorkType.Text.Trim() == "生產")
                    {
                        int kg_pcs_rate = get_kg_pcs_rate();
                        if (kg_pcs_rate != Convert.ToInt32(txtkgPCS.Text))
                        {
                            int re = clsProductionSchedule.InsertOrUpdateItem_rate(objModel, kg_pcs_rate);
                            if (re > 0)
                            {

                            }
                            else
                                MessageBox.Show("保存物料每KG轉換數量失败!");
                        }
                    }

                    get_prd_records(1);//查詢未完成的記錄
                    FillGrid();//將查詢到的記錄存入列表

                    if (cmbWorkType.Text.Trim() == "校模" && dtpEnd.Text != "00:00")//如果是校模儲存後，自動轉入生產狀態
                    {
                        record_id = -1;//變成新增狀態
                        txtProductNo.Text = "";
                        dtpStart.Value = dtpEnd.Value;//生產完成後，將校模完成時間作為生產開始時間DateTime.Now;
                        dtpEnd.Value = Convert.ToDateTime("2014/01/01 " + "00:00");
                        txtNormal_work.Text = "";
                        txtAdd_work.Text = "";
                        cmbWorkType.Text = "生產";
                        chkcont_work1.Checked = false;
                        chkcont_work2.Checked = false;
                        show_message = "保存成功，校模完成，自動轉入生產狀態，可以點擊儲存按鈕以保存記錄!";
                        txtPrd_id_ref.Text = "";//取消此單再續
                    }
                    chkcont_work1.Checked = false;
                    chkcont_work2.Checked = false;

                    

                    //ClearAllText();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                MessageBox.Show(show_message);
            }
            else
            {
                MessageBox.Show("保存失敗!");
            }
        }

        /// <summary>
        /// 物料編號值改變后觸發事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbGoods_id_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                get_data_details();
                if (cmbProductDept.Text == "302" || (cmbProductDept.Text == "202" && cmbGroup.Text=="KB01"))
                {
                    txtRow_qty.Text = "";
                    txtPer_Convert_qty.Text = "";
                    txtper_Standrad_qty.Text = "";
                    if (txtMachine.Text.Trim() != "")
                    {
                        GetMachine_std();//獲取機器的各項標準數據
                        fill_textbox_machine_std();//填充機器各項標準
                    }
                }
            }
        }


        //清空所有輸入值
        private void ClearAllText()
        {
            cmbOrder_class.Text = "";
            cmbGroup.Text = "";
            txtmo_id.Text = "";
            cmbGoods_id.Text = "";
            ClearPartOfText();
        }
        // 清空部份值
        private void ClearPartOfText()
        {
            // cmbProductDept.SelectedText = "";
            dteProdcutDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
            txtgoods_desc.Text = "";
            txtMachine.Text = "";
            txtkgPCS.Text = "";
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            txtProductNo.Text = "";
            cmbWorkType.Text = "";
            dtpStart.Value = Convert.ToDateTime("2014/01/01 " + "00:00");
            dtpEnd.Value = Convert.ToDateTime("2014/01/01 " + "00:00");
            chkcont_work1.Checked = false;
            chkcont_work2.Checked = false;
            txtNormal_work.Text = "";
            txtAdd_work.Text = "";
            txtRow_qty.Text = "";
            txtPer_Convert_qty.Text = "";
            txtper_Standrad_qty.Text = "";
            txtToDep.Text = "";
            txtMatItem.Text = "";
            txtMatDesc.Text = "";
            txtMatLot.Text = "";
            txtPrd_Run_qty.Text = "";
            txtStart_run.Text = "";
            txtEnd_run.Text = "";
            txtTotalQty.Text = "";
            txtSample_no.Text = "";
            txtSample_weg.Text = "";
            txtDifficulty_level.Text = "";
        }

        private void fill_plan_value()
        {
            txtgoods_desc.Text = "";
            for (int i = 0; i < dtMo_item.Rows.Count; i++)
            {
                if (cmbGoods_id.Text.ToString() == dtMo_item.Rows[i]["goods_id"].ToString())
                {
                    txtgoods_desc.Text = dtMo_item.Rows[i]["goods_name"].ToString();
                    if (dtMo_item.Rows[i]["prod_qty"].ToString() != "" && txtPrd_qty.Text == "")
                        txtPrd_qty.Text = Convert.ToInt32(dtMo_item.Rows[i]["prod_qty"]).ToString();
                    if (txtToDep.Text.Trim() == "")
                        txtToDep.Text = dtMo_item.Rows[i]["next_wp_id"].ToString();
                    if (txtMatItem.Text.Trim() == "")
                        txtMatItem.Text = dtMo_item.Rows[i]["mat_item"].ToString();
                    if (txtMatDesc.Text.Trim() == "")
                        txtMatDesc.Text = dtMo_item.Rows[i]["mat_item_desc"].ToString();
                    break;
                }
            }
        }

        //輸入格式驗證
        private bool valid_data()
        {
            if (chk_imput_status() == true)//檢查記錄是否已傳入新系統
                return false;
            if (cmbProductDept.Text == "")
            {
                MessageBox.Show("生產部門不能為空,請重新輸入!");
                cmbProductDept.Focus();
                cmbProductDept.SelectAll();
                return false;
            }
            if (cmbOrder_class.Text == "")
            {
                MessageBox.Show("班次不能為空,請重新輸入!");
                cmbOrder_class.Focus();
                cmbOrder_class.SelectAll();
                return false;
            }
            if (cmbGroup.Text == "")
            {
                MessageBox.Show("組別不能為空,請重新輸入!");
                cmbGroup.Focus();
                cmbGroup.SelectAll();
                return false;
            }
            if (txtmo_id.Text == "")
            {
                MessageBox.Show("制單編號不能為空,請重新輸入!");
                txtmo_id.Focus();
                txtmo_id.SelectAll();
                return false;
            }
            if (cmbGoods_id.Text == "")
            {
                MessageBox.Show("物料編號不能為空,請重新輸入!");
                cmbGoods_id.Focus();
                cmbGoods_id.SelectAll();
                return false;
            }
            if (txtPrd_qty.Text != "" && !Verify.StringValidating(txtPrd_qty.Text.Trim(), Verify.enumValidatingType.AllNumber))
            {
                MessageBox.Show("生產數量格式有誤,請重新輸入!");
                txtPrd_qty.Focus();
                txtPrd_qty.SelectAll();
                return false;
            }
            if(txtMachine.Text != "")
            {
                if (checkMachine(0) == false)
                {
                    txtMachine.Focus();
                    return false;
                }
            }
            //如果是完成的，就要做如下控制
            if (dtpStart.Text != "00:00" && dtpEnd.Text != "00:00")
            {
                if (txtMachine.Text == "")
                {
                    if (cmbWorkType.Text == "A01" || cmbWorkType.Text == "A02")
                    {
                        MessageBox.Show("生產機器不能為空，請重新輸入!");
                        txtmo_id.Focus();
                        return false;
                    }
                }
                if (chk_prd_worker(txtProductNo.Text.Trim()) == "")
                {
                    MessageBox.Show("生產工號不存在，請重新輸入!");
                    txtProductNo.Focus();
                    return false;
                }
                if (chkPrdRecords() == false)//檢查之前是否存在重複時間的記錄
                    return false;
            }
            if (string.Compare(dteProdcutDate.Text, System.DateTime.Now.ToString("yyyy/MM/dd")) > 0)
            {
                MessageBox.Show("生產日期不能大於當天日期，請重新輸入!");
                dteProdcutDate.Focus();
                return false;
            }
            if (txtprd_weg.Text != "" && !Verify.StringValidating(txtprd_weg.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("重量格式有誤,請重新輸入!");
                txtprd_weg.Focus();
                txtprd_weg.SelectAll();
                return false;
            }
            if (cmbWorkType.Text == "")
            {
                MessageBox.Show("工作類型不能為空,請重新輸入!");
                cmbWorkType.Focus();
                cmbWorkType.SelectAll();
                return false;
            }
            if (txtNormal_work.Text != "" && !Verify.StringValidating(txtNormal_work.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("正常班時間格式有誤,請重新輸入!");
                txtNormal_work.Focus();
                txtNormal_work.SelectAll();
                return false;
            }
            if (txtAdd_work.Text != "" && !Verify.StringValidating(txtAdd_work.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("加班時間格式有誤,請重新輸入!");
                txtAdd_work.Focus();
                txtAdd_work.SelectAll();
                return false;
            }
            if (txtRow_qty.Text != "" && !Verify.StringValidating(txtRow_qty.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("每行數格式有誤,請重新輸入!");
                txtRow_qty.Focus();
                txtRow_qty.SelectAll();
                return false;
            }
            if (txtPer_Convert_qty.Text.Trim() != ""
                &&txtPer_Convert_qty.Text.Trim() != "0" 
                && !Verify.StringValidating(txtPer_Convert_qty.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("每小時轉數格式有誤,請重新輸入!");
                txtPer_Convert_qty.Focus();
                txtPer_Convert_qty.SelectAll();
                return false;
            }
            if (txtper_Standrad_qty.Text.Trim() != ""
                && txtper_Standrad_qty.Text.Trim() != "0"
                && !Verify.StringValidating(txtper_Standrad_qty.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("每小時生產量格式有誤,請重新輸入!");
                txtper_Standrad_qty.Focus();
                txtper_Standrad_qty.SelectAll();
                return false;
            }
            if (txtkgPCS.Text != "" && !Verify.StringValidating(txtkgPCS.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("每KG對應數量的格式有誤,請重新輸入!");
                txtkgPCS.Focus();
                txtkgPCS.SelectAll();
                return false;
            }
            if (txtJob_type.Text.Trim() != "")
            {
                if (CheckJobType() == false)
                {
                    MessageBox.Show("工種不存在!");
                    txtJob_type.Focus();
                    txtJob_type.SelectAll();
                    return false;
                }
            }
            return true;
        }
        //檢查記錄是否已傳入新系統
        private bool chk_imput_status()
        {
            bool sresult = false;
            DataTable dtPrd;
            try
            {
                //獲取制單編號資料 COLLATE Chinese_PRC_CI_AS
                string sql = "";
                sql += " Select a.prd_id " +
                    " From product_records a with(nolock) " +
                    " Where a.prd_id = " + "'" + record_id + "' and a.transfer_flag='" + "Y" + "'";
                dtPrd = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                if (dtPrd.Rows.Count > 0)
                {
                    sresult = true;
                    MessageBox.Show("這筆記錄已傳入系統,不能再進行操作!");
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            return sresult;
            //GBW004725
        }

        //檢查之前是否存在同一時間範圍的生產時間
        private bool chkPrdRecords()
        {
            DataTable dtPrdRecords;
            string o_start_time, o_end_time;
            string show_message;
            dtPrdRecords=clsProductionSchedule.GetPrdRecords(record_id, cmbProductDept.Text.Trim(), txtmo_id.Text.Trim(), cmbGoods_id.Text.Trim(), dteProdcutDate.Text);
            if (dtPrdRecords.Rows.Count > 0)
            {
                for (int i = 0; i < dtPrdRecords.Rows.Count; i++)
                {
                    o_start_time = dtPrdRecords.Rows[0]["prd_start_time"].ToString();
                    o_end_time=dtPrdRecords.Rows[0]["prd_end_time"].ToString();
                    show_message = "這個時間範圍已存在同一生產單" + "\n" + "\n"
                         + "開始時間：" + o_start_time + " 結束時間：" + o_end_time + "\n" + "\n"+ "可能會導致重複生產單，是否繼續儲存？";
                    //開始時間是否在之前的時間範圍
                    if (string.Compare(dtpStart.Text, o_start_time) >= 0 && string.Compare(dtpStart.Text, o_end_time) <= 0)
                    {
                        if ((int)MessageBox.Show(show_message, "系統提示", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) != 6)
                            return false;//如果是不儲存
                    }
                    //結束時間是否在之前的時間範圍
                    if (string.Compare(dtpEnd.Text, o_start_time) >= 0 && string.Compare(dtpEnd.Text, o_end_time) <= 0)
                    {
                        if ((int)MessageBox.Show(show_message, "系統提示", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) != 6)
                            return false;//如果是不儲存
                    }
                }
            }
            return true;
        }
        private string chk_prd_worker(string wid)
        {
            DataTable dtWid;
            string dep1="",dep2="";
            string hrm1name = "";
            if (cmbProductDept.Text == "302")
            {
                dep1 = "PL6-01";
                dep2 = "PL6-99";
            }
            else
            {
                if (cmbProductDept.Text == "101" || cmbProductDept.Text == "102" || cmbProductDept.Text == "104")
                {
                    dep1 = "PL4-03";
                    dep2 = "PL4-14";
                }
                else
                {
                    if (cmbProductDept.Text == "105")
                    {
                        dep1 = "PL4-05";
                        dep2 = "PL5-07";
                    }
                    else
                    {
                        if (cmbProductDept.Text == "202" || cmbProductDept.Text == "203")
                        {
                            dep1 = "PL5-01";
                            dep2 = "PL5-99";
                        }
                    }
                }
            }
            try
            {
                //獲取制單編號資料 COLLATE Chinese_PRC_CI_AS
                string sql = "";
                sql += " Select a.hrm1wid,a.hrm1name " +
                    " From dgsql1.dghr.dbo.hrm01 a " +
                    " Where a.hrm1stat2 >="+"'"+dep1+"'"+" and a.hrm1stat2<="+"'"+dep2+"'"+" and a.hrm1wid = " + "'" + wid + "'";
                dtWid = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                if (dtWid.Rows.Count > 0)
                    hrm1name = dtWid.Rows[0]["hrm1name"].ToString();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            return hrm1name;
        }
        //獲取機器的各項標準數據
        private void GetMachine_std()
        {

            string strSql = "";
            string dep = cmbProductDept.SelectedValue.ToString().Trim();
            string machine_id = txtMachine.Text.Trim();
            string machine_id_part=(machine_id.Length >=3 ? machine_id.Substring(0,3):"");
            if (dep == "102" || dep == "104" || (dep == "105" && cmbGroup.Text != "BC01")
               || (dep == "105" && cmbGroup.Text == "BC01" && machine_id_part == "NTR"))
            {
                strSql = @" SELECT machine_id,machine_mul,machine_rate FROM machine_std 
                               WHERE dep='" + dep + "' AND machine_id ='" + machine_id + "' ";
                strSql = " SELECT machine_id,rows_count AS machine_mul,standard_qty AS machine_rate FROM " + remote_db + "cd_machine_standard " +
                               " WHERE dept_id='" + dep + "' AND machine_id ='" + machine_id + "' "+" AND state='0'";
            }
            else
            {
                if (dep == "302")
                {
                    string prd_code = "";
                    prd_code = (cmbGoods_id.Text.Length >= 18 ? cmbGoods_id.Text.Substring(2, 2) : "");
                    strSql = " SELECT machine_id,machine_mul,machine_rate,machine_std_qty FROM machine_std "+
                             " WHERE dep='" + dep + "' AND machine_id ='" + machine_id
                                + "' AND prd_code ='" + prd_code + "' ";
                }
                else
                {
                    if (dep == "202")
                    {
                        string Group = "";
                        if (cmbGroup.SelectedValue != null)
                            Group = cmbGroup.SelectedValue.ToString().Trim();
                        string JobType = txtJob_type.Text.Trim();
                        
                        if (Group == "KB01")
                        {
                            JobType = cmbJob_type.SelectedValue.ToString();
                            string PrdType = "";
                            string Art = "";
                            if (cmbGoods_id.Text.Length >= 18)
                            {
                                PrdType = cmbGoods_id.Text.Substring(2, 2);
                                Art = cmbGoods_id.Text.Substring(4, 7);
                            }
                            strSql = " SELECT machine_id,per_stele_qty AS machine_mul,standard_qty AS machine_rate FROM " + remote_db + "cd_machine_standard " +
                                " WHERE dept_id='" + dep + "' AND type_work ='" + JobType + "' AND goods_id ='" + Art + "' AND product_type ='" + PrdType +
                                "' AND machine2 ='" + Group + "' AND state='0'";
                        }
                        else
                        {
                            if (machine_id.Length >= 5 && machine_id.Substring(0, 5) != "K-L-W")
                                strSql = " SELECT machine_id,per_stele_qty AS machine_mul,standard_qty AS machine_rate FROM " + remote_db + "cd_machine_standard " +
                                  " WHERE dept_id='" + dep + "' AND machine_id ='" + machine_id + "' " + " AND state='0'" +
                                  " ORDER BY per_stele_qty";
                            else
                            {
                                int machine_gear = (txtSpeed_lever.Text != "" ? Convert.ToInt32(txtSpeed_lever.Text) : 1);
                                strSql = " SELECT machine_id,rows_count AS machine_mul,standard_qty AS machine_rate FROM " + remote_db + "cd_machine_standard " +
                                   " WHERE dept_id='" + dep + "' AND machine_id ='" + machine_id + "' " + " AND machine_gear='" + machine_gear + "'" + " AND state='0'";
                            }
                        }
                    }
                    if (dep == "203")
                    {
                        if (cmbGroup.Text.Trim() == "KD04")//選貨時標準照原來，暫未變
                            strSql = " SELECT machine_id,rows_count AS machine_mul,standard_qty AS machine_rate FROM " + remote_db + "cd_machine_standard " +
                               " WHERE dept_id='" + dep + "' " + " AND type_work='" + txtJob_type.Text.Trim() + "'" + " AND difficulty='" + txtDifficulty_level.Text.Trim() + "'" + " AND state='0'";
                        else
                        {
                            string PrdType = "";
                            string Art = "";
                            if (cmbGoods_id.Text.Length >= 18)
                            {
                                PrdType = cmbGoods_id.Text.Substring(2, 2);
                                Art = cmbGoods_id.Text.Substring(4, 7);
                            }
                            strSql = " SELECT machine_id,rows_count AS machine_mul,standard_qty AS machine_rate FROM " + remote_db + "cd_machine_standard " +
                               " WHERE dept_id='" + dep + "' " + " AND product_type='" + PrdType + "'" + " AND type_work='" + txtJob_type.Text.Trim() + "'" +
                               " AND goods_id='" + Art + "'" + " AND state='0'";
                        }

                    }
                    else
                    {
                        if (cmbProductDept.Text == "105" && cmbGroup.Text == "BC01" && machine_id_part != "NTR")//林口手碑組的標準
                        {
                            strSql = " SELECT type_work AS machine_id,rows_count AS machine_mul,standard_qty AS machine_rate FROM " + remote_db + "cd_machine_standard " +
                               " WHERE dept_id='" + dep + "' AND type_work ='" + txtJob_type.Text.Trim() + "' " + " AND state='0'";
                        }
                    }
                 }
            }
            try
            {
                dtMachine_std = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        //檢查機器代碼是否正確
        private bool checkMachine(int replace_type)//replace_type = 1 獲取機器難度
        {
            DataTable mac_tb;
            string strSql = " SELECT resource_id AS machine_id,standby2 FROM " + remote_db + "cd_resource" +
                   " WHERE department_id='" + cmbProductDept.SelectedValue.ToString() + "' AND resource_id ='" + txtMachine.Text.Trim() + "' ";
            try
            {
                mac_tb = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                if (mac_tb.Rows.Count == 0)
                {
                    MessageBox.Show("機器代碼不存在,請重新輸入!");
                    return false;
                }
                else
                {
                    if (replace_type==1)//獲取機器難度
                        txtDifficulty_level.Text = mac_tb.Rows[0]["standby2"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        //填充機器各項標準
        private void fill_textbox_machine_std()
        {
            txtRow_qty.Text = "";
            txtPer_Convert_qty.Text = "";
            txtper_Standrad_qty.Text = "";
            if (dtMachine_std.Rows.Count > 0)
            {
                txtRow_qty.Text = (dtMachine_std.Rows[0]["machine_mul"].ToString() != "" ? Convert.ToInt32(dtMachine_std.Rows[0]["machine_mul"]).ToString() : "1");
                txtPer_Convert_qty.Text = (dtMachine_std.Rows[0]["machine_rate"].ToString() != "" ? Convert.ToInt32(dtMachine_std.Rows[0]["machine_rate"]).ToString() : "0");
                count_hour_std_qty();//計算機器每小時標準生產數
            }
        }

        private void txtMachine_Leave(object sender, EventArgs e)
        {
            txtRow_qty.Text = "";
            txtPer_Convert_qty.Text = "";
            txtper_Standrad_qty.Text = "";
            if (txtMachine.Text.Trim() != "" && cmbProductDept.Text != "")
            {
                GetMachine_std();//獲取機器的各項標準數據
                fill_textbox_machine_std();//填充機器各項標準
            }
            if (cmbProductDept.Text == "102")
            {
                txtDifficulty_level.Text = "";
                checkMachine(1);//獲取機器難度
            }
        }

        private void cmbProductDept_TextChanged(object sender, EventArgs e)
        {
            ClearAllText();
            
        }

        /// <summary>
        /// 新的時間計算
        /// </summary>
        private void count_datetime()
        {
            if (dtpStart.Text.ToString() == "00:00" || dtpEnd.Text.ToString() == "00:00")
                return;
            txtNormal_work.Text = "";
            txtAdd_work.Text = "";
            string current_day = "2000/01/01";
            string str_start_time = "";
            string str_end_time = "";
            string ks_time, js_time;
            double sj = 0, normal_time = 0, ot_time = 0;
            int std_work_hour = 8;
            TimeSpan ts;
            ks_time = dtpStart.Text.ToString();//開始生產時間
            js_time = dtpEnd.Text.ToString();//結束生產時間
            if (cmbOrder_class.SelectedIndex == 0)//白班
            {
                string am_in_time = "08:00";//早上上班
                string am_out_time = "12:59";//早上下班
                string pm_in_time = "14:00";//下午上班
                string pm_out_time = "18:59";//下午下班
                string night_in_time = "19:00";//晚上上班
                double am_ext_time = 1.5, pm_ext_time = 1;//不是連班應扣除時間
                str_start_time = current_day + " " + ks_time + ":00";//開始生產時間   加上日期便於計算
                str_end_time = current_day + " " + js_time + ":00";//結束生產時間
                ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                if ((string.Compare(ks_time, am_in_time) >= 0 && string.Compare(ks_time, am_out_time) == -1 && string.Compare(js_time, am_out_time) == -1)//08:30~12:30
                   || (string.Compare(ks_time, pm_in_time) >= 0 && string.Compare(ks_time, pm_out_time) == -1 && string.Compare(js_time, pm_out_time) == -1))//14:00~18:00
                    normal_time = sj;
                else
                {//>=08:30   >=14:00<18:00
                    if (string.Compare(ks_time, am_in_time) >= 0 && string.Compare(ks_time, am_out_time) == -1 && string.Compare(js_time, pm_in_time) >= 0 && string.Compare(js_time, pm_out_time) == -1)
                    {
                        normal_time = sj - am_ext_time;//正常班時間
                        if (chkcont_work1.Checked == true)//中午連班
                            ot_time = am_ext_time;
                    }
                    else
                    {//>=08:30   >=19:00
                        if (string.Compare(ks_time, am_in_time) >= 0 && string.Compare(ks_time, am_out_time) == -1 && string.Compare(js_time, night_in_time) >= 0)
                        {
                            str_end_time = current_day + " " + night_in_time + ":00";//結束生產時間
                            ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                            sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                            normal_time = sj - (am_ext_time + pm_ext_time);
                            if (chkcont_work1.Checked == true)//中午連班
                                ot_time = am_ext_time;
                            if (chkcont_work2.Checked == true)//下午連班
                                ot_time = ot_time + pm_ext_time;
                            //加班時間
                            str_start_time = current_day + " " + night_in_time + ":00";//開始生產時間
                            str_end_time = current_day + " " + js_time + ":00";//結束生產時間
                            ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                            sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                            ot_time = ot_time + sj;
                        }
                        else
                        {//晚上加班時間//>=19:00   >=19:00
                            if (string.Compare(ks_time, night_in_time) >= 0 && string.Compare(js_time, night_in_time) >= 0)
                                ot_time = sj;
                            else
                            {//中午開始，晚上結束>=14:00    >=19:00
                                if (string.Compare(ks_time, pm_in_time) >= 0 && string.Compare(ks_time, pm_out_time) == -1 && string.Compare(js_time, night_in_time) >= 0)
                                {
                                    str_end_time = current_day + " " + night_in_time + ":00";//結束生產時間
                                    ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                                    sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                                    normal_time = sj - pm_ext_time;
                                    if (chkcont_work2.Checked == true)//下午連班
                                        ot_time = pm_ext_time;
                                    //加班時間
                                    str_start_time = current_day + " " + night_in_time + ":00";//開始生產時間
                                    str_end_time = current_day + " " + js_time + ":00";//結束生產時間
                                    ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                                    sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                                    ot_time = ot_time + sj;
                                }
                            }
                        }
                    }
                }
            }
            else //夜班
            {
                string night_start_work_time = "19:00";
                string night_end_work_time = "08:30";
                string ot_start_time = "04:30";
                string day_start_time = "00:00";
                string day_end_time = "24:00";
                string next_day = "2000/01/02";
                string secon_data = ":00";
                string act_ot_start_time = "";//加班開始時間
                string act_ot_end_time = "";//加班結束時間
                bool ot_flag = false;//加班標識
                str_start_time = current_day + " " + ks_time + secon_data;
                str_end_time = current_day + " " + js_time + secon_data;
                if (string.Compare(ks_time, night_start_work_time) >= 0 && string.Compare(ks_time, day_end_time) <= 0
                    && string.Compare(js_time, night_start_work_time) >= 0 && string.Compare(js_time, day_end_time) <= 0)
                    str_end_time = current_day + " " + js_time + secon_data;//開始、結束時間在19:00 ~ 24:00之間 正常班時段
                else
                {
                    if (string.Compare(ks_time, night_start_work_time) >= 0 && string.Compare(ks_time, day_end_time) <= 0
                        && string.Compare(js_time, day_start_time) >= 0 && string.Compare(js_time, ot_start_time) <= 0)
                        str_end_time = next_day + " " + js_time + secon_data;//開始時間在19:00 ~ 24:00;結束時間在00:00~04:30之間 正常班時段
                    else
                    {
                        if (string.Compare(ks_time, day_start_time) >= 0 && string.Compare(ks_time, ot_start_time) <= 0
                        && string.Compare(js_time, day_start_time) >= 0 && string.Compare(js_time, ot_start_time) <= 0)
                            str_end_time = current_day + " " + js_time + secon_data;//開始、結束時間在00:00 ~ 04:30之間  正常班時段
                        else
                        {
                            if (string.Compare(ks_time, night_start_work_time) >= 0 && string.Compare(ks_time, day_end_time) <= 0
                                && string.Compare(js_time, ot_start_time) >= 0 && string.Compare(js_time, night_end_work_time) <= 0)
                            //開始時間在19:00 ~ 24:00;結束時間在04:00~08:30之間 正常班時段，有加班
                            {
                                str_end_time = next_day + " " + ot_start_time + secon_data;
                                ot_flag = true;
                                act_ot_start_time = next_day + " " + ot_start_time + secon_data;
                                act_ot_end_time = next_day + " " + js_time + secon_data;
                            }
                            else
                            {
                                if (string.Compare(ks_time, day_start_time) >= 0 && string.Compare(ks_time, ot_start_time) <= 0
                                && string.Compare(js_time, ot_start_time) >= 0 && string.Compare(js_time, night_end_work_time) <= 0)
                                //開始時間在00:00 ~ 04:30;結束時間在04:30~08:30之間 正常班時段，有加班
                                {
                                    str_start_time = next_day + " " + ks_time + secon_data;
                                    str_end_time = next_day + " " + ot_start_time + secon_data;
                                    ot_flag = true;
                                    act_ot_start_time = next_day + " " + ot_start_time + secon_data;
                                    act_ot_end_time = next_day + " " + js_time + secon_data;
                                }
                                else
                                {
                                    if (string.Compare(ks_time, ot_start_time) >= 0 && string.Compare(ks_time, night_end_work_time) <= 0
                                    && string.Compare(js_time, ot_start_time) >= 0 && string.Compare(js_time, night_end_work_time) <= 0)
                                    //開始、結束時間在04:30~08:30之間 沒有正常班，只有加班
                                    {
                                        str_start_time = next_day + " " + ks_time + secon_data;//將正常班開始、結束時間設定成一齊
                                        str_end_time = next_day + " " + ks_time + secon_data;
                                        ot_flag = true;
                                        act_ot_start_time = next_day + " " + ks_time + secon_data;
                                        act_ot_end_time = next_day + " " + js_time + secon_data;
                                    }
                                    else
                                    {
                                        str_end_time = current_day + " " + js_time + secon_data;
                                    }
                                }
                            }
                        }
                    }
                }
                ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                normal_time = sj;
                if (ot_flag == true)
                {
                    ts = Convert.ToDateTime(act_ot_end_time) - Convert.ToDateTime(act_ot_start_time); //結束 - 開始 時間
                    ot_time = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                }
                //if (sj <= std_work_hour)
                //    normal_time = sj;
                //else
                //{
                //    normal_time = std_work_hour;
                //    ot_time = sj - std_work_hour;
                //}

            }
            if (normal_time != 0)
                txtNormal_work.Text = Math.Round(normal_time, 3).ToString();
            if (ot_time != 0)
                txtAdd_work.Text = Math.Round(ot_time, 3).ToString();
        }

        private void chkcont_wrok1_Click(object sender, EventArgs e)
        {
            count_req_time();//計算預計完成時間
            count_datetime();
        }

        private void chkcont_work2_Click(object sender, EventArgs e)
        {
            count_req_time();//計算預計完成時間
            count_datetime();
        }

        /// <summary>
        /// DataGridView 的單元格點擊事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        //填充各種控件
        private void fill_textbox(int Rrow)
        {
            if (Rrow < 0)
                return;
            string item;
            if (dtProductionRecordslist.Rows.Count > 0)
            {
                edit_type = "N";//控件不作為編輯
                fill_exist_record(Rrow);
                chkcont_work1.Checked = false;
                chkcont_work2.Checked = false;
                txtmo_id.Text = dtProductionRecordslist.Rows[Rrow]["prd_mo"].ToString();
                item = dtProductionRecordslist.Rows[Rrow]["prd_item"].ToString();
                cmbGoods_id.Items.Clear();
                cmbGoods_id.Items.Add(item);
                //GetMo_itme(item);
                cmbGoods_id.Text = item;//物料編號
                txtgoods_desc.Text = GetItemDesc(item);//獲取物料描述
                get_total_prd_qty();//顯示單的總完成數量
                txtgoods_desc.Focus();
                SetControlVisible();//設置控件為可見狀態
            }
        }
        //獲取物料描述
        private string GetItemDesc(string item)
        {
            string desc="";
            DataTable dtitem = new DataTable();
            try
            {
                dtitem = clsProductionSchedule.GetItemDesc(item);
                if (dtitem.Rows.Count > 0)
                {
                    desc = dtitem.Rows[0]["name"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return desc;
        }
        private void get_total_prd_qty()
        {
            DataTable db_show_qty = clsProductionSchedule.get_total_prd_qty(cmbProductDept.SelectedValue.ToString(), txtmo_id.Text.ToString(), cmbGoods_id.Text.ToString());
            if (db_show_qty.Rows.Count > 0)
                txtTotalQty.Text = db_show_qty.Rows[0]["prd_qty"].ToString();
        }
        

        //獲取物料的每公斤對應數量
        private int get_kg_pcs_rate()
        {
            DataTable dtItem_kg_pcs = null;
            int kg_pcs_rate = 0;
            try
            {
                //獲取制單編號資料
                string sql = " select dep,mat_item,rate,cr_date  from item_rate ";
                sql += " Where dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'";
                sql += " And mat_item = " + "'" + cmbGoods_id.Text.ToString() + "'";

                dtItem_kg_pcs = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                if (dtItem_kg_pcs.Rows.Count > 0)
                    kg_pcs_rate = Convert.ToInt32(dtItem_kg_pcs.Rows[0]["rate"].ToString());
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            return kg_pcs_rate;
        }
        private void fill_txt_kg_pcs()
        {
            if (txtkgPCS.Text == "" && cmbProductDept.Text != "" && cmbGoods_id.Text != "")
            {
                txtkgPCS.Text = get_kg_pcs_rate().ToString();
                txtkgPCS.Text = (txtkgPCS.Text.ToString() != "0" ? txtkgPCS.Text : "");
            }
        }
        private void txtProductNo_Leave(object sender, EventArgs e)
        {
            if (txtProductNo.Text.Trim() != "")
                txtProductNo.Text = txtProductNo.Text.PadLeft(10, '0');
        }

        private void count_hour_std_qty()
        {
            if (txtRow_qty.Text != "" && txtPer_Convert_qty.Text != "")
            {
                if (cmbProductDept.Text == "302" || (cmbProductDept.Text == "202" && cmbGroup.SelectedValue.ToString()=="KB01"))
                    txtper_Standrad_qty.Text = txtPer_Convert_qty.Text;
                else
                    txtper_Standrad_qty.Text = (Convert.ToInt32(txtRow_qty.Text) * Convert.ToInt32(txtPer_Convert_qty.Text)).ToString();
                count_req_time();//預計完成時間
            }
        }

        private void btnCount_time_Click(object sender, EventArgs e)
        {
            double hour_num = 0;
            if (txtPrd_qty.Text != "" && txtper_Standrad_qty.Text != "" && dtpStart.Text != "00:00")
            {
                hour_num = Math.Round(Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtper_Standrad_qty.Text), 3);
                dtpEnd.Value = Convert.ToDateTime(dteProdcutDate.Text + " " + dtpStart.Text).AddHours(hour_num);
                count_datetime();
            }
        }
        private void count_req_time()
        {
            double hour_num = 0;
            string am_start_time = "08:30";
            string finish_work_noon1="12:30";//,finish_work_noon2="14:00";//中午下班時間 12:30~14:00
            string finish_work_afternoon1 = "18:00";//, finish_work_afternoon2 = "19:00";//下午下班時間 18:00~19:00
            string finish_work_time;
            dtpReqEnd.Text = "";
            if (txtPrd_qty.Text != "" && txtper_Standrad_qty.Text != "" && dtpStart.Text != "00:00")
            {
                hour_num = Math.Round(Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtper_Standrad_qty.Text), 3);
                finish_work_time = Convert.ToDateTime(dteProdcutDate.Text + " " + dtpStart.Text).AddHours(hour_num).ToString("yyyy/MM/dd HH:mm".Substring(11,5));
                //當開始時間是從08:30,完成時間是在12:30~18:00之間的
                if (string.Compare(dtpStart.Text, am_start_time) >= 0
                    && string.Compare(dtpStart.Text, finish_work_noon1) <= 0
                    && string.Compare(finish_work_time, finish_work_noon1) > 0)
                {
                    if (chkcont_work1.Checked == false)
                        hour_num = hour_num + 1.5;
                }

                finish_work_time = Convert.ToDateTime(dteProdcutDate.Text + " " + dtpStart.Text).AddHours(hour_num).ToString("yyyy/MM/dd HH:mm".Substring(11, 5));
                if (string.Compare(finish_work_time, finish_work_afternoon1) > 0
                    && string.Compare(dtpStart.Text, finish_work_afternoon1) <= 0)
                {
                    if (chkcont_work2.Checked == false)
                        hour_num = hour_num + 1;
                }
                dtpReqEnd.Value = Convert.ToDateTime(dteProdcutDate.Text + " " + dtpStart.Text).AddHours(hour_num);
            }
        }
        private void btnCount_qty_Click(object sender, EventArgs e)
        {
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            float normal_time, ot_time;
            int std_qty = (txtper_Standrad_qty.Text !=""?Convert.ToInt32(txtper_Standrad_qty.Text):0);
            normal_time = (txtNormal_work.Text.ToString() != "" ? Convert.ToSingle(txtNormal_work.Text) : 0);
            ot_time = (txtAdd_work.Text.ToString() != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
            txtPrd_qty.Text = Convert.ToInt32(((normal_time + ot_time) * std_qty)).ToString();
            if (cmbProductDept.Text != "203")//不是203部門，自動計算生產重量
                count_prd_weg();
        }

        private void txtBarCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    DataTable dtBarCode = clsPublicOfPad.BarCodeToItem(txtBarCode.Text);
                    txtBarCode.Text = "";
                    if (dtBarCode.Rows.Count > 0)
                    {
                        string barcode_type = dtBarCode.Rows[0]["barcode_type"].ToString();
                        if (barcode_type == "2")//從生產計劃中提取的條形碼
                        {
                            txtBarCodeMo.Text = dtBarCode.Rows[0]["mo_id"].ToString();
                            txtBarCodeItem.Text = dtBarCode.Rows[0]["goods_id"].ToString();
                            get_prd_records(10);//按條形碼查找制單狀態
                            FillGrid(); //將查詢到的記錄存入列表
                            if (dgvDetails.Rows.Count > 0)
                                fill_textbox(0);//填充各種控件
                        }
                    }
                    else
                        return;
                    break;
            }
        }

        
        private void txtgoods_desc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtgoods_desc.Text = "";
        }

        private void txtmo_id_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtmo_id.Text = "";
        }

        private void txtMachine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtMachine.Text = "";
        }

        private void txtRow_qty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtRow_qty.Text = "";
        }

        private void txtPer_Convert_qty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtPer_Convert_qty.Text = "";
        }

        private void txtper_Standrad_qty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtper_Standrad_qty.Text = "";
        }

        private void txtProductNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtProductNo.Text = "";
        }

        private void cmbWorkType_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                cmbWorkType.Text = "";
        }


        private void get_data_details()
        {
            ClearPartOfText(); //清空文本框內容
            fill_plan_value();//首先將計劃單帶出數量、描述
            get_prd_records(1);//查詢已錄入的記錄
            chk_prd_no_complete();//檢查是否有未完成的記錄，默認帶出來
            FillGrid();//將查詢到的記錄存入列表
            fill_txt_kg_pcs();//提取物料每公斤對應數量
            if (cmbProductDept.Text != "203")//不是203部門，自動計算生產重量
                count_prd_weg();
            else
                count_prd_qty();//是203部門，自動計算生產數量
            get_total_prd_qty();//獲取總完成數量
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtPrd_qty.Text != "" && txtprd_weg.Text != "" && txtprd_weg.Text != "0")
                txtkgPCS.Text = Convert.ToInt32(Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtprd_weg.Text)).ToString();
        }

        private void BTNNOCOMP_Click(object sender, EventArgs e)
        {
            get_prd_records(2);//查詢已錄入的記錄
            FillGrid(); //將查詢到的記錄存入列表
            if (dgvDetails.Rows.Count > 0)
                fill_textbox(0);//填充各種控件
        }

        private void BTNNOSTART_Click(object sender, EventArgs e)
        {
            get_prd_records(4);//未開始的記錄7天內未開始生產的記錄
            FillGrid(); //將查詢到的記錄存入列表
            if (dgvDetails.Rows.Count > 0)
                fill_textbox(0);//填充各種控件
        }

        private void BTNCOMP_Click(object sender, EventArgs e)
        {
            get_prd_records(5);//當天完成的記錄
            FillGrid(); //將查詢到的記錄存入列表
            if (dgvDetails.Rows.Count > 0)
                fill_textbox(0);//填充各種控件
        }
        private void txtSearchMo_TextChanged(object sender, EventArgs e)
        {
            if (txtSearchMo.Text.Trim().Length >= 6)
            {
                //chkIsComplete.Checked = true;
                if (txtSearchMo.Text != "")
                {
                    if (this.chkIsComplete.Checked == false)//按制單編號查詢未完成的記錄
                        get_prd_records(6);
                    else//按制單編號查詢包括已完成的記錄
                        get_prd_records(7);
                    FillGrid(); //將查詢到的記錄存入列表
                    if (txtSearchMo.Text.Length >= 9)
                        fill_textbox(0);//填充各種控件
                }
            }
        }
        private void dgvDetails_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
                e.RowBounds.Location.Y,
                dgvDetails.RowHeadersWidth - 4,
                e.RowBounds.Height);

            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
                dgvDetails.RowHeadersDefaultCellStyle.Font,
                rectangle,
                dgvDetails.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                count_datetime();//計算生產時間
                count_alloy_std_hour_qty();//計算合金部的每小時標準碑數
            }
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                count_datetime();//計算生產時間
                count_alloy_std_hour_qty();//計算合金部的每小時標準碑數
            }
        }

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (chk_imput_status() == true)//檢查記錄是否已傳入新系統
                return;
            OperationType = clsUtility.enumOperationType.Delete;
            ToolStripButtonEvents();
        }

        private void btnFind_Click_1(object sender, EventArgs e)
        {
            OperationType = clsUtility.enumOperationType.Find;
            ToolStripButtonEvents();
        }

        private void txtRow_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                if (cmbProductDept.Text != "302")
                    count_hour_std_qty();//計算標準數量
                else
                    Cout_prd_qty_alloy();//計算實際生產數量(合金部)
            }
        }

        private void txtPer_Convert_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                if (cmbProductDept.Text != "302")
                    count_hour_std_qty();
        }

        private void txtkgPCS_TextChanged(object sender, EventArgs e)
        {
            
        }
        
        private void txtper_Standrad_qty_TextChanged(object sender, EventArgs e)
        {
            count_req_time();//預計完成時間
            //if (edit_type == "Y" && cmbProductDept.Text != "302")
            //{
                
            //}
        }
        private void Cout_prd_qty_alloy()
        {
            if (txtRow_qty.Text != "" && txtPrd_Run_qty.Text != "")
            {
                txtPrd_qty.Text = (Convert.ToInt32(txtRow_qty.Text) * Convert.ToInt32(txtPrd_Run_qty.Text)).ToString();
                count_prd_weg();//不是203部門，自動計算生產重量
            }
        }

        private void txtPrd_Run_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")//合金部 生產數 = 每碑數 * 實際碑數
            {
                Cout_prd_qty_alloy();
                count_alloy_std_hour_qty();//計算合金部的每小時標準碑數
            }
        }
        private void count_alloy_std_hour_qty()//計算合金部的每小時標準碑數
        {
            if (cmbProductDept.Text == "302" && (txtNormal_work.Text != "" || txtAdd_work.Text != ""))
            {
                float normal_work, add_work, Prd_Run_qty;
                normal_work = (txtNormal_work.Text != "" ? Convert.ToSingle(txtNormal_work.Text) : 0);
                add_work = (txtAdd_work.Text != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
                Prd_Run_qty = (txtPrd_Run_qty.Text != "" ? Convert.ToSingle(txtPrd_Run_qty.Text) : 0);
                if (normal_work + add_work != 0)
                    txtper_Standrad_qty.Text = Math.Round(Prd_Run_qty / (normal_work + add_work), 0).ToString();
            }
        }
        private void cmbProductDept_Leave(object sender, EventArgs e)
        {
            InitComBoxGroup();//初始化組別
            GetJobType();//綁定工種
            SetControlVisible();//設置控件可見
            if (cmbProductDept.Text == "105" || cmbProductDept.Text == "203")
                cmbWorkType.Text = "生產";
        }
        private void SetControlVisible()
        {
            bool t1, t2;
            t1 = true;
            t2 = true;
            string dep = cmbProductDept.Text.Trim();
            if (dep == "102" || dep == "105" || dep == "202" || dep == "203")
            {
                t1 = false;
                t2 = true;
                lblDifficulty_level.Location = new Point(3, 327);
                txtDifficulty_level.Location = new Point(53, 321);
            }
            else
            {
                t1 = true;
                t2 = false;
                lblDifficulty_level.Location = new Point(3, 751);
                txtDifficulty_level.Location = new Point(54, 737);
            }
            lblStart_run.Visible = t1;
            txtStart_run.Visible = t1;
            lblEnd_run.Visible = t1;
            txtEnd_run.Visible = t1;
            lblPrd_Run_qty.Visible = t1;
            txtPrd_Run_qty.Visible = t1;
            lblSpeed_lever.Visible = t2;
            txtSpeed_lever.Visible = t2;
            lblJob_type.Visible = t2;
            txtJob_type.Visible = t2;
            cmbJob_type.Visible = t1;
            txtWork_class.Visible = t2;
            lblWork_class.Visible = t2;
            txtWork_code.Visible = t1;
            lblWork_code.Visible = t1;
            if (dep == "202")//如果是202的工種，則顯示下拉框
            {
                if (cmbGroup.Text == "KB01" || cmbGroup.Text == "KF01")
                {
                    txtJob_type.Visible = t1;
                    cmbJob_type.Visible = t2;
                }
            }
            else
            {
                if (dep == "203")//如果是203
                {
                }
            }
        }
        private void txtStart_run_TextChanged(object sender, EventArgs e)
        {
            
        }
        private void count_run_qty()//計算實際碑數  合金部使用
        {
            if (edit_type == "Y" && cmbProductDept.Text == "302")//當是在編輯狀態且302部門時
            {
                txtPrd_Run_qty.Text = "";
                if (txtEnd_run.Text != "")
                {
                    txtPrd_Run_qty.Text = (Convert.ToInt32(txtEnd_run.Text) - Convert.ToInt32((txtStart_run.Text != "" ? txtStart_run.Text : "0"))).ToString();
                    count_alloy_std_hour_qty();
                }
            }
        }

        private void txtEnd_run_TextChanged(object sender, EventArgs e)
        {
            
        }
        //此單再續時，將上一次結束的碑數，當做為今次的開始數
        private void get_last_run_qty()
        {
            DataTable db_last_run_qty = new DataTable();
            string sql = "";
            sql += " Select end_run From product_records a with(nolock)" +
                " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'" +
                " And a.prd_mo = " + "'" + txtmo_id.Text.ToString() + "'" +
                " And a.prd_item = " + "'" + cmbGoods_id.Text.ToString() + "'" +
                " And a.prd_work_type = '" + "A02" + "'" +
                " And a.prd_start_time <> '' " + " And a.prd_end_time <> '' "+
                " Order by prd_date Desc,prd_end_time Desc";
            db_last_run_qty = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            if(db_last_run_qty.Rows.Count > 0)
                txtStart_run.Text = db_last_run_qty.Rows[0]["end_run"].ToString();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            if (record_id == -1)
            {
                MessageBox.Show("原單記錄不存在!");
                return;
            }
            txtPrd_id_ref.Text = record_id.ToString();
            record_id = -1;//重新設定為新單狀態
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            dtpStart.Text = "00:00";
            dtpEnd.Text = "00:00";
            dtpReqEnd.Text = "00:00";
            txtNormal_work.Text = "";
            txtAdd_work.Text = "";
            txtStart_run.Text = "";
            txtEnd_run.Text = "";
            txtPrd_Run_qty.Text = "";
            txtProductNo.Text = "";
            txtTotalQty.Text = "";
            setProdDate();//自動設定生產日期為當前日期
            get_last_run_qty();//獲取最後一次的碑數
            get_total_prd_qty();//顯示單的總完成數量
            if (cmbProductDept.Text == "302")//302部門的，要將標準時能清空
                txtper_Standrad_qty.Text = "";
        }

        private void dtpEnd_MouseDown(object sender, MouseEventArgs e)
        {
            if (dtpEnd.Text == "00:00" &&dtpStart.Text != "00:00")
                dtpEnd.Value = System.DateTime.Now;
            clsUtility.Call_imput();
        }

        private void txtMachine_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtMachine.Text == "")
            {
                if (cmbProductDept.Text == "102")
                {
                    txtMachine.Text = "NBY-";
                    if (_userid == "BUT01")//萬能機
                        txtMachine.Text = "NBY-";
                    else
                    {
                        if (_userid == "BUT02")//雞眼
                            txtMachine.Text = "NDG-";
                    }
                }
                else
                {
                    if (cmbProductDept.Text == "302")//合金
                        txtMachine.Text = "ABY-";
                    else
                    {
                        if (cmbProductDept.Text == "203")//扣部--裝嵌
                            txtMachine.Text = "K-I-S-";
                        else
                        {
                            if (cmbProductDept.Text == "105")
                            {
                                if (cmbGroup.Text == "BC01")//手碑
                                    txtMachine.Text = "NFS-";
                                else
                                    txtMachine.Text = "NTR-";//林機
                            }
                            else
                            {
                                if (cmbProductDept.Text == "104")
                                    txtMachine.Text = "NCJ-";
                                else
                                {
                                    if (cmbProductDept.Text == "202")
                                    {
                                        if (cmbGroup.Text == "KB01" || cmbGroup.Text == "KF01")
                                            txtMachine.Text = "K-M-A-";
                                        else
                                        {
                                            if (cmbGroup.Text == "KC03")
                                                txtMachine.Text = "K-L-W-";
                                            else
                                                txtMachine.Text = "K-J-E-";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                    
                txtMachine.SelectionStart = txtMachine.Text.Length;
            }
            clsUtility.Call_imput();
        }

        private void dtpStart_MouseDown(object sender, MouseEventArgs e)
        {
            if (dtpStart.Text == "00:00")
                dtpStart.Value = System.DateTime.Now;
            clsUtility.Call_imput();
        }

        private void txtPrd_qty_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput();
        }

        private void txtprd_weg_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput();
        }

        private void txtmo_id_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtProductNo_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtStart_run_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtEnd_run_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtNormal_work_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtAdd_work_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtDifficulty_level_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtWork_code_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtSpeed_lever_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtSearch_Machine.Text == "")
            {
                if (_userid == "BUT01")
                    txtSearch_Machine.Text = "NBY-";
                else
                    if (_userid == "BUT02")
                        txtSearch_Machine.Text = "NDG-";
                    else
                        if (_userid == "ALY01")
                            txtSearch_Machine.Text = "ABY-";
                txtSearch_Machine.SelectionStart = txtSearch_Machine.Text.Length;
            }
            clsUtility.Call_imput(); 
        }

        private void txtSearchMo_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtSearch_Machine_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void btnMachine_Click(object sender, EventArgs e)
        {
            if (txtSearch_Machine.Text.Trim() != "")
            {
                get_prd_records(8);//按機器編號查詢最後一筆記錄
                FillGrid(); //將查詢到的記錄存入列表
            }
        }

        private void txtMatLot_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtMatLot.Text.Trim() == "")
            {
                txtMatLot.Text = "HWH";
                txtMatLot.SelectionStart = txtMatLot.Text.Length;
            }
        }

        private void cmbProductDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProductDept.Text == "302")
                lblStandard_per_qty.Text = "標準時產能";
            else
                lblStandard_per_qty.Text = "每小時標準數";
            if (cmbProductDept.Text == "105" || cmbProductDept.Text == "104")
                cmbWorkType.Text="生產";
        }

        private void txtNormal_work_Leave(object sender, EventArgs e)
        {
            count_alloy_std_hour_qty();
        }

        private void txtAdd_work_Leave(object sender, EventArgs e)
        {
            count_alloy_std_hour_qty();
        }

        private void txtStart_run_Leave(object sender, EventArgs e)
        {
            count_run_qty();//計算實際碑數  合金部使用
        }

        private void txtEnd_run_Leave(object sender, EventArgs e)
        {
            count_run_qty();//計算實際碑數  合金部使用
        }

        private void txtper_Standrad_qty_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        //檢查是否有已分單的MO
        private DataTable chkDistMo(int chk_type, string prd_mo_sub, string prd_item_sub)
        {
            DataTable dtJoin = new DataTable();
            try
            {
                //獲取分單資料
                string sql = "";
                if (chk_type == 1)//檢查是否有已分單的MO
                {
                    sql = " Select a.prd_id,a.prd_date,a.prd_mo,a.prd_item,a.prd_id_ref,b.prd_mo_sub,b.prd_item_sub,b.prd_qty_sub " +
                        " From product_records a with(nolock) " +
                        " Inner Join product_records_dist_mo b on a.prd_id_ref=b.prd_id_ref " +
                        " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'" +
                        " And b.prd_mo_sub = " + "'" + prd_mo_sub + "'" +
                        " And b.prd_item_sub = " + "'" + prd_item_sub + "'";
                }
                else//檢查是否有未完成的MO,若有則提示是否要分單
                {
                    if (chk_type == 2)
                    {
                        sql = " Select a.prd_id,a.prd_date,a.prd_mo,a.prd_item,a.prd_id_ref " +
                        " From product_records a with(nolock) " +
                        " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'" +
                        " And a.prd_item=" + "'" + prd_item_sub + "'" +
                        " And a.prd_end_time =" + "'" + "" + "'";
                    }
                    else
                    {
                        if (chk_type == 3 || chk_type == 4)
                        {
                            sql = " Select a.prd_id,a.prd_date,a.prd_mo,a.prd_item,a.prd_qty,a.prd_id_ref,b.prd_mo_sub,b.prd_item_sub,b.prd_qty_sub " +
                        " From product_records a with(nolock) " +
                        " Inner Join product_records_dist_mo b with(nolock) on a.prd_id_ref=b.prd_id_ref " +
                        " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'";
                            if (chk_type == 3)//按制單編號查詢分單
                            {
                                if (rdbSearch1.Checked == true)
                                    sql += " And a.prd_mo = " + "'" + prd_mo_sub + "'";
                                if (rdbSearch2.Checked == true)
                                    sql += " And b.prd_mo_sub = " + "'" + prd_mo_sub + "'";
                            }
                            else//按記錄號查詢分單
                            {
                                sql += " And a.prd_id_ref = " + "'" + prd_mo_sub + "'";
                            }
                        }
                    }
                }
                sql += " Order By a.prd_date desc,a.prd_end_time,a.crtim";
                dtJoin = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            return dtJoin;
        }

        private void btnMo_search_Click(object sender, EventArgs e)
        {
            dgvAndSingle.DataSource = chkDistMo(3, txtMo_search.Text, "");
        }

        private void rdbSearch1_Click(object sender, EventArgs e)
        {
            btnMo_search_Click(sender, e);
        }

        private void rdbSearch2_Click(object sender, EventArgs e)
        {
            btnMo_search_Click(sender, e);
        }

        private void dgvAndSingle_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            record_id = (int)dgvAndSingle.Rows[e.RowIndex].Cells["colSer_qo"].Value;
            get_prd_records(9);//按制單編號查詢未完成的記錄
            FillGrid();
            
        }

        private void btnKgPcs_Click(object sender, EventArgs e)
        {
            pnlKgPcs.Visible = true;
            txtSample_no.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            convert_kg_pcs();
            pnlKgPcs.Visible = false;
        }
        //計算每KG對應數量
        private void convert_kg_pcs()
        {
            txtkgPCS.Text = "";
            if (txtSample_no.Text != "" && txtSample_weg.Text != "" && Convert.ToSingle(txtSample_weg.Text) != 0)
                txtkgPCS.Text = Math.Round(Convert.ToInt32(txtSample_no.Text) / (Convert.ToSingle(txtSample_weg.Text) / 1000), 0).ToString();
            //計算生產數量或重量
            if (cmbProductDept.Text != "203")
                count_prd_weg();
            else
                count_prd_qty();
        }

        //計算生產數量
        private void count_prd_qty()
        {
            txtPrd_qty.Text = "";
            if (txtprd_weg.Text != "" && txtkgPCS.Text != "")
                txtPrd_qty.Text = Math.Round((Convert.ToSingle(txtprd_weg.Text) * Convert.ToSingle(txtkgPCS.Text)), 0).ToString();
        }
        //計算生產重量
        private void count_prd_weg()
        {
            txtprd_weg.Text = "";
            if (txtPrd_qty.Text != "" && txtkgPCS.Text != "")
                txtprd_weg.Text = Math.Round((Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtkgPCS.Text)), 2).ToString();
        }
        private void txtkgPCS_Leave(object sender, EventArgs e)
        {
            if (cmbProductDept.Text != "203")//不是203部門，自動計算生產重量
                count_prd_weg();
            else
                count_prd_qty();//是203部門，自動計算生產數量
        }

        private void txtSample_no_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void txtSample_weg_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!valid_data())
                return;
            product_records objModel = new product_records();
            objModel.prd_dep = cmbProductDept.SelectedValue.ToString();
            objModel.prd_date = dteProdcutDate.Text.ToString();
            objModel.prd_pdate = (mktPrdPdate.Text.Replace(" ", "") == "//" ? dteProdcutDate.Text.ToString() : mktPrdPdate.Text.ToString());
            objModel.prd_mo = txtmo_id.Text.Trim();
            objModel.prd_item = cmbGoods_id.Text.ToString().Trim();
            objModel.prd_qty = (txtPrd_qty.Text != "" ? Convert.ToInt32(txtPrd_qty.Text) : 0);
            objModel.prd_weg = (txtprd_weg.Text != "" ? Convert.ToSingle(txtprd_weg.Text) : 0);
            objModel.prd_machine = txtMachine.Text.Trim();
            objModel.prd_work_type = cmbWorkType.SelectedValue.ToString();
            objModel.prd_worker = txtProductNo.Text.Trim();
            objModel.prd_class = cmbOrder_class.Text.Trim();
            objModel.prd_group = cmbGroup.Text.Trim();
            objModel.prd_start_time = (dtpStart.Text.Trim() != "00:00" ? dtpStart.Text.Trim() : "");
            objModel.prd_end_time = (dtpEnd.Text.Trim() != "00:00" ? dtpEnd.Text.Trim() : "");
            objModel.prd_req_time = (dtpReqEnd.Text.Trim() != "00:00" ? dtpReqEnd.Text.Trim() : "");
            objModel.prd_normal_time = (txtNormal_work.Text != "" ? Convert.ToSingle(txtNormal_work.Text) : 0);
            objModel.prd_ot_time = (txtAdd_work.Text != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
            objModel.line_num = (txtRow_qty.Text != "" ? Convert.ToInt32(txtRow_qty.Text) : 0);
            objModel.hour_run_num = (txtPer_Convert_qty.Text != "" ? Convert.ToInt32(txtPer_Convert_qty.Text) : 0);
            objModel.hour_std_qty = (txtper_Standrad_qty.Text != "" ? Convert.ToInt32(txtper_Standrad_qty.Text) : 0);
            objModel.kg_pcs = (txtkgPCS.Text != "" ? Convert.ToInt32(txtkgPCS.Text) : 0);
            objModel.mat_item = txtMatItem.Text.Trim();
            objModel.mat_item_lot = txtMatLot.Text.Trim();
            objModel.mat_item_desc = txtMatDesc.Text.Trim();
            objModel.to_dep = txtToDep.Text.Trim();
            objModel.crusr = _userid;
            objModel.crtim = DateTime.Now;
            objModel.amusr = _userid;
            objModel.amtim = DateTime.Now;
            objModel.prd_id = record_id;
            objModel.difficulty_level = txtDifficulty_level.Text.Trim();
            if (objModel.prd_dep == "302")//合金部默認難度為1
                objModel.difficulty_level = "1";
            objModel.pack_num = (txtPack_num.Text != "" ? Convert.ToInt32(txtPack_num.Text) : 1);
            objModel.work_code = txtWork_code.Text;
            objModel.job_type = txtJob_type.Text.ToUpper();
            if (cmbProductDept.Text.Trim() == "202")
            {
                if (cmbGroup.Text == "KB01" || cmbGroup.Text == "KF01")
                    objModel.job_type = cmbJob_type.SelectedValue.ToString().ToUpper();
            }
            objModel.prd_run_qty = (txtPrd_Run_qty.Text != "" ? Convert.ToSingle(txtPrd_Run_qty.Text) : 0);
            objModel.speed_lever = (txtSpeed_lever.Text != "" ? Convert.ToInt32(txtSpeed_lever.Text) : 0);
            objModel.start_run = (txtStart_run.Text != "" ? Convert.ToInt32(txtStart_run.Text) : 0);
            objModel.end_run = (txtEnd_run.Text != "" ? Convert.ToInt32(txtEnd_run.Text) : 0);
            objModel.prd_id_ref = (txtPrd_id_ref.Text != "" ? Convert.ToInt32(txtPrd_id_ref.Text) : 0);
            objModel.sample_no = (txtSample_no.Text != "" ? Convert.ToInt32(txtSample_no.Text) : 0);
            objModel.sample_weg = Math.Round((txtSample_weg.Text != "" ? Convert.ToDecimal(txtSample_weg.Text) : 0), 4);
            objModel.work_class = txtWork_class.Text.Trim();
            //迎合選貨時用到的變量
            objModel.ok_qty = 0;
            objModel.ok_weg = 0;
            objModel.no_ok_qty = 0;
            objModel.no_ok_weg = 0;
            objModel.per_hour_std_qty = 0;
            objModel.member_no = 1;
            objModel.actual_pack_num = 0;
            objModel.actual_qty = 0;
            objModel.actual_weg = 0;
            objModel.conf_flag = "";
            objModel.conf_time = Convert.ToDateTime("1900/01/01");
            if (objModel.prd_dep == "105" && objModel.prd_item.Substring(14, 4) == "NEP0"
                && objModel.prd_start_time != "" && objModel.prd_end_time != "")//林口部，將NEP的直接加入磅貨中，當作組裝批量輸入
            {
                objModel.conf_flag = "Y";
                objModel.actual_qty = objModel.prd_qty;
                objModel.actual_weg = Convert.ToDecimal(objModel.prd_weg);
                objModel.conf_time = DateTime.Now;
            }
            
            try
            {
                if (record_id == -1)
                {
                    record_id = clsPublicOfPad.GenNo("frmProductionSchedule");//自動產生序列號
                    if (record_id > 0)
                    {
                        objModel.prd_id = record_id;
                        if (txtPrd_id_ref.Text.Trim() != "" && Convert.ToInt32(txtPrd_id_ref.Text) > 0)//此單再續的，需要用回舊的記錄號
                        {
                            objModel.prd_id_ref = Convert.ToInt32(txtPrd_id_ref.Text);
                        }
                        Result = clsProductionSchedule.AddProductionRecords(objModel);
                    }
                    else
                    {
                        MessageBox.Show("儲存記錄失敗!");
                        return;
                    }


                }
                else
                {
                    Result = clsProductionSchedule.UpdateProductionRecords(objModel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            OperationType = clsUtility.enumOperationType.Save;
            ToolStripButtonEvents();
            //txtBarCode.Focus();
        }

        private void GetMat_Desc()
        {

            txtMatDesc.Text = "";
            string strSql = "";
            DataTable dtMatDesc = new DataTable();
            strSql = " SELECT name FROM " + remote_db + "it_goods " +
                           " WHERE within_code='" + "0000" + "' AND id ='" + txtMatItem.Text.Trim() + "' ";
            try
            {
                dtMatDesc = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                if (dtMatDesc.Rows.Count > 0)
                    txtMatDesc.Text = dtMatDesc.Rows[0]["name"].ToString().Trim();
                else
                    MessageBox.Show("物料編號不存在!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtMatItem_Leave(object sender, EventArgs e)
        {
            GetMat_Desc();
        }

        private void txtJob_type_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
            if (txtJob_type.Text.Trim() == "")
            {
                if (cmbProductDept.Text == "203" && cmbGroup.Text.Trim()=="KD04")//選貨用舊標準
                    txtJob_type.Text = "KA";
                else
                {
                    if (cmbProductDept.Text == "105" && cmbGroup.Text=="BC01")
                        txtJob_type.Text = "B";
                }
                txtJob_type.SelectionStart = txtJob_type.Text.Length;
            }
        }

        private void txtSpeed_lever_Leave(object sender, EventArgs e)
        {
            if (txtMachine.Text.Trim() != "" && cmbProductDept.Text == "202" && txtSpeed_lever.Text.Trim() !="")
            {
                GetMachine_std();//獲取機器的各項標準數據
                fill_textbox_machine_std();//填充機器各項標準
            }
        }

        private void txtJob_type_Leave(object sender, EventArgs e)
        {
            string Group = cmbGroup.Text.Trim();
            if ((cmbProductDept.Text == "105" && Group == "BC01") || (cmbProductDept.Text == "203" && cmbGoods_id.Text != "" && txtJob_type.Text != "")
                || (cmbProductDept.Text == "202" && Group == "KB01" && cmbGoods_id.Text != "" && txtJob_type.Text != ""))
            {
                GetMachine_std();//獲取機器的各項標準數據
                fill_textbox_machine_std();//填充機器各項標準
            }

        }

        private void txtDifficulty_level_Leave(object sender, EventArgs e)
        {
            //203選貨的標準暫未改變
            if (txtDifficulty_level.Text.Trim() != "" && cmbProductDept.Text == "203" && cmbGroup.Text.Trim() != "KD04")
            {
                GetMachine_std();//獲取機器的各項標準數據
                fill_textbox_machine_std();//填充機器各項標準
            }
        }

        private void txtSample_no_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                convert_kg_pcs();
        }

        private void txtSample_weg_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                convert_kg_pcs();
        }

        private void txtWork_class_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput(); 
        }

        private void cmbGroup_Leave(object sender, EventArgs e)
        {
            string Group = "";
            Group = cmbGroup.Text.Trim();
            if (cmbProductDept.Text == "105" && Group == "BC05")
                cmbWorkType.Text = "選貨";
            else if (cmbProductDept.Text == "202")
            {
                if (Group == "KB01" || Group == "KF01")
                    SetControlVisible();//設置控件可見
                if (Group == "KB01")//202 KB01組別的標準是不同算法的
                {
                    GetMachine_std();//獲取機器的各項標準數據
                    fill_textbox_machine_std();//填充機器各項標準
                }
            }
        }

        private void cmbOrder_class_Leave(object sender, EventArgs e)
        {
            setProdDate();//自動設定生產日期為當前日期
        }


        private void dtpStart_Leave(object sender, EventArgs e)
        {
            count_req_time();//預計完成時間
        }

        private void txtPrd_qty_Leave(object sender, EventArgs e)
        {
            count_req_time();//預計完成時間
            if (cmbProductDept.Text != "203")//不是203部門，自動計算生產重量
                count_prd_weg();
        }

        private void txtprd_weg_Leave(object sender, EventArgs e)
        {
            if (cmbProductDept.Text == "203")//是203部門，自動計算生產數量
                count_prd_qty();
        }

        private void cmbOrder_class_SelectedValueChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                count_datetime();//計算生產時間
                count_alloy_std_hour_qty();//計算合金部的每小時標準碑數
            }
        }

        private void dgvDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            fill_textbox(e.RowIndex);//填充各種控件
        }

        private void dgvDetails_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            edit_type = "Y";
        }

        private void cmbWorkType_Leave(object sender, EventArgs e)
        {
            string work_type = "";
            work_type = cmbWorkType.SelectedValue.ToString().Trim();
            if (work_type == "A01" || work_type == "A04" || work_type == "A05" || work_type == "A06" || work_type == "A07" || work_type == "A10")
                txtPrd_qty.Text = "0";
        }

        private void cmbJob_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                string Group = cmbGroup.Text.Trim();
                if ((cmbProductDept.Text == "105" && Group == "BC01") || (cmbProductDept.Text == "203" && cmbGoods_id.Text != "" && txtJob_type.Text != "")
                    || (cmbProductDept.Text == "202" && Group == "KB01" && cmbGoods_id.Text != "" && cmbJob_type.SelectedValue != null && cmbJob_type.SelectedValue != ""))
                {
                    GetMachine_std();//獲取機器的各項標準數據
                    fill_textbox_machine_std();//填充機器各項標準
                }
            }
        }


    }
}
