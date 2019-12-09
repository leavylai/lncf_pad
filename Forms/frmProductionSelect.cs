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
    public partial class frmProductionSelect : Form
    {
        DataTable dtPrd_dept = new DataTable();
        DataTable dtMo_item = new DataTable();
        DataTable dtWork_type = new DataTable();
        DataTable dtMachine_std = new DataTable();
        DataTable dtProductionRecordslist = new DataTable();
        DataTable dtWorker = new DataTable();
        DataTable dtGroup = new DataTable();//組別
        private string edit_type = "Y";//控制當控件中當值發生變化時的操作
        private clsUtility.enumOperationType OperationType;
        private int Result = 0;
        private string _userid = DBUtility._user_id;
        private product_records objModel;
        private int record_id = -1;//未完成記錄的ID，若查找到，則說明未完成，在保存時，執行更新操作

        public frmProductionSelect()
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

            Font a = new Font("GB2312", 20);//GB2312为字体名称，1为字体大小dataGridView1.Font = a;
            dgvDetails.Font = a;
            dgvWorker.Font = a;
            dgvWorker1.Font = a;
            dgvWorker2.Font = a;
            dgvDetails.AutoGenerateColumns = false;

            get_prd_worker();//初始化dtWorker表，不然中間會出錯
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
            if (_userid.Substring(0, 3) == "ALY")
                cmbProductDept.Text = "302";
            else
            {
                if (_userid.Substring(0, 3) == "BLK")
                    cmbProductDept.Text = "105";
            }

            //初始化工作類型
            cmbWorkType.DataSource = dtWork_type;
            cmbWorkType.DisplayMember = "work_type_desc";
            cmbWorkType.ValueMember = "work_type_id";
            cmbWorkType.Text = "選貨";
            cmbWorkType.SelectedValue = "A03";
            InitComBoxGroup();
            //初始化班次、組別
            cmbOrder_class.Items.Add("白班");
            cmbOrder_class.Items.Add("夜班");
            cmbOrder_class.Text = "白班";
            dteProdcutDate.Text = System.DateTime.Now.ToString("yyyy/MM/dd");
        }

        private void GetGroups()
        {
            string strSql = "";
            strSql = " SELECT work_group,group_desc FROM work_group WHERE ( dep='" + cmbProductDept.Text.Trim() + "'" + " AND group_type='" + "2" + "') " + " OR dep='" + "000" + "' ";
            try
            {
                dtGroup = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitComBoxGroup()
        {
            GetGroups();

            if (dtGroup.Rows.Count > 0)
            {
                cmbGroup.DataSource = dtGroup;
                cmbGroup.DisplayMember = "work_group";
                cmbGroup.ValueMember = "work_group";
            }

            cmbWorkType.Text = "選貨";
            if (cmbProductDept.Text == "302")
                txtmWeg1.Text = "1.4";
            else
            {
                if (cmbProductDept.Text == "105")
                    txtmWeg1.Text = "0";
                else
                    txtmWeg1.Text = "0";
            }
            txtmWeg2.Text = "0";
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
                dtMo_item = clsProductionSchedule.GetMo_dataById(txtmo_id.Text.Trim(), cmbProductDept.SelectedValue.ToString(), item);
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
            try
            {
                string prd_work_type = cmbWorkType.SelectedValue.ToString().Trim();
                //獲取制單編號資料
                string sql = "";
                sql += " Select a.*,rtrim(b.work_type_desc) as work_type_desc ";
                sql += " From product_records a ";
                sql += " Left outer join work_type b on a.prd_work_type=b.work_type_id ";
                sql += " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'";
                sql += " And a.prd_work_type = " + "'" + prd_work_type + "'";
                if (con_type == 1)//是否查找當日未完成標識
                {
                    sql += " And a.prd_mo = " + "'" + txtmo_id.Text.ToString() + "'";
                    sql += " And a.prd_item = " + "'" + cmbGoods_id.Text.ToString() + "'";
                }
                else
                {
                    if (con_type == 2)//未完成的記錄
                    {
                        //sql += " And a.prd_date = " + "'" + dteProdcutDate.Text + "'";
                        sql += " And a.prd_start_time <> " + "'" + "" + "'" + " And a.prd_end_time = " + "'" + "" + "'";
                    }
                    else
                    {
                        if (con_type == 3)//如果是查找當日所有記錄
                            sql += " And a.prd_date = " + "'" + dteProdcutDate.Text + "'";
                        else
                        {
                            if (con_type == 4)//未開始生產的記錄
                            {
                                //sql += " And a.prd_date = " + "'" + dteProdcutDate.Text + "'";
                                sql += " And a.prd_start_time = " + "'" + "" + "'" + " And a.prd_end_time = " + "'" + "" + "'";
                            }
                            else
                            {
                                if (con_type == 5)//當天完成的記錄
                                {
                                    sql += " And a.prd_date = " + "'" + dteProdcutDate.Text + "'";
                                    sql += " And a.prd_start_time <> " + "'" + "" + "'" + " And a.prd_end_time <> " + "'" + "" + "'";
                                }
                                else
                                {
                                    if (con_type == 6)//按制單編號查詢未完成的記錄
                                    {
                                        sql += " And a.prd_mo like " + "'%" + txtSearchMo.Text + "%'";
                                        sql += " And a.prd_end_time = " + "'" + "" + "'";
                                    }
                                    else
                                    {
                                        if (con_type == 7)//按制單編號查詢所有記錄
                                        {
                                            sql += " And a.prd_mo like " + "'%" + txtSearchMo.Text + "%'";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                sql += " Order By a.prd_date desc,a.prd_end_time,a.crtim ";
                dtProductionRecordslist = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            //GBW004725
        }

        //獲取進度表的工號及姓名
        private void get_prd_worker()
        {
            try
            {
                //獲取制單編號資料 COLLATE Chinese_PRC_CI_AS
                string sql = "";
                sql += " Select a.prd_worker,b.hrm1name " +
                    " From product_records_worker a " +
                    " Left Join dgsql1.dghr.dbo.hrm01 b on a.prd_worker=b.hrm1wid  COLLATE Chinese_PRC_CI_AS" +
                    " Where a.prd_id = " + "'" + (txtPrd_id.Text != "" ? Convert.ToInt32(txtPrd_id.Text) : 0) + "'";
                dtWorker = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                dgvWorker.DataSource = dtWorker;
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            //GBW004725
        }
        //取組別的當日最後的完成時間作為下次的開始時間
        private void get_last_prd_end_time()
        {

            try
            {
                string strsql_part;
                string sql;
                string last_date = System.DateTime.Now.ToString("yyyy/MM/dd");
                DataTable dtLastTime = new DataTable();
                strsql_part = " (Select Max(prd_id) AS prd_id " +
                    " From product_records Where " +
                    " prd_dep= '" + cmbProductDept.Text + "'"
                    + " and prd_date ='" + last_date + "'"
                    + " and prd_group='" + cmbGroup.Text + "'"
                    + " and prd_start_time <>'" + "" + "' and prd_end_time <>'" + "" + "'"
                    + " ) c ";

                sql = " Select a.*,rtrim(b.work_type_desc) as work_type_desc " +
                " From product_records a " +
                " Left outer join work_type b on a.prd_work_type=b.work_type_id " +
                " Inner Join " + strsql_part + " on a.prd_id=c.prd_id";
                dtLastTime = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                if (dtLastTime.Rows.Count > 0)//取組別的當日最後的完成時間作為下次的開始時間
                    dtpStart.Text = dtLastTime.Rows[0]["prd_end_time"].ToString();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }
        private string chk_prd_worker(string wid)
        {
            DataTable dtWid;
            string hrm1name = "";
            try
            {
                //獲取制單編號資料 COLLATE Chinese_PRC_CI_AS
                string sql = "";
                sql += " Select a.hrm1wid,a.hrm1name " +
                    " From dgsql1.dghr.dbo.hrm01 a " +
                    " Where a.hrm1wid = " + "'" + wid + "'";
                dtWid = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                if (dtWid.Rows.Count > 0)
                    hrm1name = dtWid.Rows[0]["hrm1name"].ToString();
                else
                    MessageBox.Show("工號不存在!");
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            return hrm1name;
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
                    {
                        fill_exist_record(i);
                        break;
                    }
                }
            }
        }
        //重新填入查找到的記錄
        private void fill_exist_record(int index)
        {
            record_id = Convert.ToInt32(dtProductionRecordslist.Rows[index]["prd_id"].ToString());//更新記錄序號
            txtPrd_id.Text = record_id.ToString();
            dteProdcutDate.Text = dtProductionRecordslist.Rows[index]["prd_date"].ToString();
            cmbOrder_class.Text = dtProductionRecordslist.Rows[index]["prd_class"].ToString();
            cmbGroup.Text = dtProductionRecordslist.Rows[index]["prd_group"].ToString();

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
            txtPer_hour_std_qty.Text = (dtProductionRecordslist.Rows[index]["per_hour_std_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["per_hour_std_qty"].ToString() : "");
            txtPrd_qty.Text = (dtProductionRecordslist.Rows[index]["prd_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_qty"].ToString() : "");
            txtprd_weg.Text = (dtProductionRecordslist.Rows[index]["prd_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_weg"].ToString() : "");
            txtkgPCS.Text = (dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() : "");
            txtDifficulty_level.Text = dtProductionRecordslist.Rows[index]["difficulty_level"].ToString();
            txtMatItem.Text = dtProductionRecordslist.Rows[index]["mat_item"].ToString();
            txtMatDesc.Text = dtProductionRecordslist.Rows[index]["mat_item_desc"].ToString();
            txtMatLot.Text = dtProductionRecordslist.Rows[index]["mat_item_lot"].ToString();
            dtpReqEnd.Text = dtProductionRecordslist.Rows[index]["prd_req_time"].ToString();
            txtToDep.Text = dtProductionRecordslist.Rows[index]["to_dep"].ToString();
            txtPrd_Run_qty.Text = dtProductionRecordslist.Rows[index]["prd_run_qty"].ToString();
            txtWork_code.Text = dtProductionRecordslist.Rows[index]["work_code"].ToString();
            txtSpeed_lever.Text = dtProductionRecordslist.Rows[index]["speed_lever"].ToString();
            txtPack_num.Text = (dtProductionRecordslist.Rows[index]["pack_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["pack_num"].ToString() : "");
            txtStart_run.Text = (dtProductionRecordslist.Rows[index]["start_run"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["start_run"].ToString() : "0");
            txtEnd_run.Text = (dtProductionRecordslist.Rows[index]["end_run"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["end_run"].ToString() : "");
            txtOk_qty.Text = (dtProductionRecordslist.Rows[index]["ok_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["ok_qty"].ToString() : "");
            txtOk_weg.Text = (dtProductionRecordslist.Rows[index]["ok_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["ok_weg"].ToString() : "");
            txtNook_qty.Text = (dtProductionRecordslist.Rows[index]["no_ok_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["no_ok_qty"].ToString() : "");
            txtNook_weg.Text = (dtProductionRecordslist.Rows[index]["no_ok_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["no_ok_weg"].ToString() : "");
            txtSample_no.Text = (dtProductionRecordslist.Rows[index]["sample_no"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["sample_no"].ToString() : "");
            txtSample_weg.Text = (dtProductionRecordslist.Rows[index]["sample_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["sample_weg"].ToString() : "");
            txtTotMember.Text = (dtProductionRecordslist.Rows[index]["member_no"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["member_no"].ToString() : "");
            txtPrd_id_ref.Text = (dtProductionRecordslist.Rows[index]["prd_id_ref"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_id_ref"].ToString() : "");
            txtActual_weg.Text = (dtProductionRecordslist.Rows[index]["actual_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["actual_weg"].ToString() : "");
            txtActual_qty.Text = (dtProductionRecordslist.Rows[index]["actual_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["actual_qty"].ToString() : "");
        }

        /// <summary>
        /// 綁定列表
        /// </summary>
        private void FillGrid()
        {
            if (dtProductionRecordslist.Rows.Count > 0)
            {
                dgvDetails.DataSource = dtProductionRecordslist;
                if (dgvDetails.Rows.Count > 0)
                    fill_exist_record(0);//填充各種控件
                //dgvDetails.Refresh();
            }
            else
            {
                dgvDetails.DataSource = null;
                dgvDetails.Refresh();
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
                                int prd_id = Convert.ToInt32(txtPrd_id.Text);
                                int re = clsProductionSchedule.DeleteProductionRecords(prd_id);
                                if (re > 0)
                                {
                                    MessageBox.Show("刪除成功!");
                                    get_prd_records(1);//查詢已錄入的記錄
                                    FillGrid(); //將查詢到的記錄存入列表
                                    ClearAllText();
                                    ClearPartOfText();
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
                if (cmbProductDept.Text == "302")
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
            txtPrd_id.Text = "";
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
            txtWorker.Text = "";
            txtTotMember.Text = "";
            txtSample_no.Text = "";
            txtSample_weg.Text = "";
            txtOk_weg.Text = "";
            txtOk_qty.Text = "";
            txtNook_weg.Text = "";
            txtNook_qty.Text = "";
            txtkgPCS.Text = "";
            txtOkqty_All.Text = "";
            txtNookqty_All.Text = "";
            cmbGroup.Text = "";
            if (cmbGroup.SelectedIndex > 0)
                cmbGroup.SelectedIndex = 0;//選擇為空值
            txtPer_hour_std_qty.Text = "";
            txtWork_code.Text = "";
            txtPrd_id_ref.Text = "";
            txtActual_weg.Text = "";
            txtActual_qty.Text = "";
            txtPack_num.Text = "";
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
                    if (txtMatLot.Text.Trim() == "")
                        txtMatLot.Text = "HWH";
                    break;
                }
            }
        }

        //輸入格式驗證
        private bool valid_data()
        {
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
            if (cmbWorkType.Text == "")
            {
                MessageBox.Show("工作類型不能為空,請重新輸入!");
                cmbWorkType.Focus();
                cmbWorkType.SelectAll();
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
            if (cmbWorkType.Text.Trim() == "A03")
            {
                if (txtPrd_qty.Text != "" && !Verify.StringValidating(txtPrd_qty.Text.Trim(), Verify.enumValidatingType.AllNumber))
                {
                    MessageBox.Show("生產數量格式有誤,請重新輸入!");
                    txtPrd_qty.Focus();
                    txtPrd_qty.SelectAll();
                    return false;
                }
                if (txtMachine.Text != "")
                {
                    if (checkMachine() == false)
                    {
                        txtMachine.Focus();
                        return false;
                    }
                }
                //如果是完成的，就要做如下控制
                if (dtpStart.Text != "00:00" && dtpEnd.Text != "00:00" && cmbGroup.Text != "AB99")
                {
                    if (dtWorker.Rows.Count == 0)
                    {
                        MessageBox.Show("選貨工號不能為空，請重新輸入!");
                        txtWorker.Focus();
                        return false;
                    }
                    if (txtPrd_qty.Text == "" || txtPrd_qty.Text == "0")
                    {
                        MessageBox.Show("數量不能為零,請重新輸入!");
                        txtPrd_qty.Focus();
                        txtPrd_qty.SelectAll();
                        return false;
                    }
                }
                if (string.Compare(dteProdcutDate.Text, System.DateTime.Now.ToString("yyyy/MM/dd")) > 0)
                {
                    MessageBox.Show("生產日期不能大於當天日期，請重新輸入!");
                    dteProdcutDate.Focus();
                    return false;
                }

                if (txtprd_weg.Text != "" && txtprd_weg.Text != "0" && !Verify.StringValidating(txtprd_weg.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
                {
                    MessageBox.Show("重量格式有誤,請重新輸入!");
                    txtprd_weg.Focus();
                    txtprd_weg.SelectAll();
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
                if (txtPer_Convert_qty.Text != "" && !Verify.StringValidating(txtPer_Convert_qty.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
                {
                    MessageBox.Show("每小時轉數格式有誤,請重新輸入!");
                    txtPer_Convert_qty.Focus();
                    txtPer_Convert_qty.SelectAll();
                    return false;
                }
                if (txtper_Standrad_qty.Text != "" && !Verify.StringValidating(txtper_Standrad_qty.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
                {
                    MessageBox.Show("每小時生產量格式有誤,請重新輸入!");
                    txtper_Standrad_qty.Focus();
                    txtper_Standrad_qty.SelectAll();
                    return false;
                }
                if (txtkgPCS.Text != "" && txtkgPCS.Text != "0" && !Verify.StringValidating(txtkgPCS.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
                {
                    MessageBox.Show("每KG對應數量的格式有誤,請重新輸入!");
                    txtkgPCS.Focus();
                    txtkgPCS.SelectAll();
                    return false;
                }
            }
            return true;
        }

        //獲取機器的各項標準數據
        private void GetMachine_std()
        {

            string strSql = "";
            string prd_code = "";
            prd_code = (cmbGoods_id.Text.Length >= 18 ? cmbGoods_id.Text.Substring(2, 2) : "");
            if (cmbProductDept.Text == "102")
                strSql = @" SELECT machine_id,machine_mul,machine_rate FROM machine_std 
                               WHERE dep='" + cmbProductDept.SelectedValue.ToString() + "' AND machine_id ='" + txtMachine.Text.Trim() + "' ";
            else
                if (cmbProductDept.Text == "302")
                    strSql = @" SELECT machine_id,machine_mul,machine_rate,machine_std_qty FROM machine_std 
                               WHERE dep='" + cmbProductDept.SelectedValue.ToString() + "' AND machine_id ='" + txtMachine.Text.Trim()
                                + "' AND prd_code ='" + prd_code + "' ";
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
        private bool checkMachine()
        {
            DataTable mac_tb;
            string strSql = @" SELECT machine_id FROM machine_tb
                               WHERE dep='" + cmbProductDept.SelectedValue.ToString() + "' AND machine_id ='" + txtMachine.Text.Trim() + "' ";
            try
            {
                mac_tb = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                if (mac_tb.Rows.Count == 0)
                {
                    MessageBox.Show("機器代碼輸入不正確,請重新輸入!");
                    return false;
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
            if (dtMachine_std.Rows.Count > 0)
            {
                txtRow_qty.Text = dtMachine_std.Rows[0]["machine_mul"].ToString();
                txtPer_Convert_qty.Text = dtMachine_std.Rows[0]["machine_rate"].ToString();
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
            txtNormal_work.Text = "";
            txtAdd_work.Text = "";
            if (dtpStart.Text.ToString() == "00:00" || dtpEnd.Text.ToString() == "00:00")
                return;
            string str_start_time = "";
            string str_end_time = "";
            string am_in_time = "08:00";//早上上班
            string am_out_time = "12:59";//早上下班
            string pm_in_time = "14:00";//下午上班
            string pm_out_time = "18:59";//下午下班
            string night_in_time = "19:00";//晚上上班
            string ks_time, js_time;
            double am_ext_time = 1.5, pm_ext_time = 1;//不是連班應扣除時間
            ks_time = dtpStart.Text.ToString();//開始生產時間
            js_time = dtpEnd.Text.ToString();//結束生產時間
            double sj = 0, normal_time = 0, ot_time = 0;
            TimeSpan ts;
            str_start_time = dteProdcutDate.Text.ToString() + " " + ks_time + ":00";//開始生產時間   加上日期便於計算
            str_end_time = dteProdcutDate.Text.ToString() + " " + js_time + ":00";//結束生產時間
            ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
            sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
            if ((string.Compare(ks_time, am_in_time) >= 0 && string.Compare(ks_time, am_out_time) == -1 && string.Compare(js_time, am_out_time) == -1)//08:30~12:30
               || (string.Compare(ks_time, pm_in_time) >= 0 && string.Compare(ks_time, pm_out_time) == -1 && string.Compare(js_time, pm_out_time) == -1))//14:00~18:00
                normal_time = sj;
            else//>=08:30   >=14:00<18:00
                if (string.Compare(ks_time, am_in_time) >= 0 && string.Compare(ks_time, am_out_time) == -1 && string.Compare(js_time, pm_in_time) >= 0 && string.Compare(js_time, pm_out_time) == -1)
                {
                    normal_time = sj - am_ext_time;//正常班時間
                    if (chkcont_work1.Checked == true)//中午連班
                        ot_time = am_ext_time;
                }
                else//>=08:30   >=19:00
                    if (string.Compare(ks_time, am_in_time) >= 0 && string.Compare(ks_time, am_out_time) == -1 && string.Compare(js_time, night_in_time) >= 0)
                    {
                        str_end_time = dteProdcutDate.Text.ToString() + " " + night_in_time + ":00";//結束生產時間
                        ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                        sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                        normal_time = sj - (am_ext_time + pm_ext_time);
                        if (chkcont_work1.Checked == true)//中午連班
                            ot_time = am_ext_time;
                        if (chkcont_work2.Checked == true)//下午連班
                            ot_time = ot_time + pm_ext_time;
                        //加班時間
                        str_start_time = dteProdcutDate.Text.ToString() + " " + night_in_time + ":00";//開始生產時間
                        str_end_time = dteProdcutDate.Text.ToString() + " " + js_time + ":00";//結束生產時間
                        ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                        sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                        ot_time = ot_time + sj;
                    }
                    else//晚上加班時間//>=19:00   >=19:00
                        if (string.Compare(ks_time, night_in_time) >= 0 && string.Compare(js_time, night_in_time) >= 0)
                            ot_time = sj;
                        else//中午開始，晚上結束>=14:00    >=19:00
                            if (string.Compare(ks_time, pm_in_time) >= 0 && string.Compare(ks_time, pm_out_time) == -1 && string.Compare(js_time, night_in_time) >= 0)
                            {
                                str_end_time = dteProdcutDate.Text.ToString() + " " + night_in_time + ":00";//結束生產時間
                                ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                                sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                                normal_time = sj - pm_ext_time;
                                if (chkcont_work2.Checked == true)//下午連班
                                    ot_time = pm_ext_time;
                                //加班時間
                                str_start_time = dteProdcutDate.Text.ToString() + " " + night_in_time + ":00";//開始生產時間
                                str_end_time = dteProdcutDate.Text.ToString() + " " + js_time + ":00";//結束生產時間
                                ts = Convert.ToDateTime(str_end_time) - Convert.ToDateTime(str_start_time); //結束 - 開始 時間
                                sj = Convert.ToSingle(ts.TotalHours.ToString());//數值型的時間
                                ot_time = ot_time + sj;
                            }
            //if (Convert.ToDateTime(dtp_prd_date.Text).DayOfWeek.ToString() != "Saturday" && Convert.ToDateTime(dtp_prd_date.Text).DayOfWeek.ToString() != "Sunday")
            //{
            if (normal_time != 0)
                txtNormal_work.Text = Math.Round(normal_time, 3).ToString();
            if (ot_time != 0)
                txtAdd_work.Text = Math.Round(ot_time, 3).ToString();
            //}else//如果是禮拜六、日，則全當作加班時間
            //if(normal_time + ot_time != 0)
            //    txt_ot_time.Text = Math.Round(normal_time + ot_time,3).ToString();
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
        private void dgvDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            fill_textbox(e.RowIndex);
        }
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
            }
        }


        //獲取物料描述
        private string GetItemDesc(string item)
        {
            string desc = "";
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
        //獲取所有已選貨的數量
        private void get_total_prd_qty()
        {
            DataTable db_show_qty = new DataTable();

            string sql = "";
            sql += " Select sum(prd_qty) as prd_qty From product_records a" +
                " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'" +
                " And a.prd_mo = " + "'" + txtmo_id.Text.ToString() + "'" +
                " And a.prd_item = " + "'" + cmbGoods_id.Text.ToString() + "'" +
                " And a.prd_work_type = '" + "A03" + "'" +
                " And a.prd_start_time <> '' " + " And a.prd_end_time <> '' ";
            db_show_qty = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
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
            string finish_work_noon1 = "12:30";//,finish_work_noon2="14:00";//中午下班時間 12:30~14:00
            string finish_work_afternoon1 = "18:00";//, finish_work_afternoon2 = "19:00";//下午下班時間 18:00~19:00
            string finish_work_time;
            dtpReqEnd.Text = "";
            if (txtPrd_qty.Text != "" && txtper_Standrad_qty.Text != "" && txtper_Standrad_qty.Text != "0" && dtpStart.Text != "00:00")
            {
                hour_num = Math.Round(Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtper_Standrad_qty.Text), 3);
                finish_work_time = Convert.ToDateTime(dteProdcutDate.Text + " " + dtpStart.Text).AddHours(hour_num).ToString("yyyy/MM/dd HH:mm".Substring(11, 5));
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
            int std_qty = (txtper_Standrad_qty.Text != "" ? Convert.ToInt32(txtper_Standrad_qty.Text) : 0);
            normal_time = (txtNormal_work.Text.ToString() != "" ? Convert.ToSingle(txtNormal_work.Text) : 0);
            ot_time = (txtAdd_work.Text.ToString() != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
            txtPrd_qty.Text = Convert.ToInt32(((normal_time + ot_time) * std_qty)).ToString();
            count_kg_pcs();//計算每KG對應數量
        }

        private void txtBarCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    string goods_id = "";
                    DataTable dtBarCode = clsPublicOfPad.BarCodeToItem(txtBarCode.Text);
                    txtBarCode.Text = "";
                    if (dtBarCode.Rows.Count > 0)
                    {
                        string barcode_type = dtBarCode.Rows[0]["barcode_type"].ToString();

                        if (barcode_type == "2")//從生產計劃中提取的條形碼
                        {
                            txtmo_id.Text = dtBarCode.Rows[0]["mo_id"].ToString();
                            goods_id = dtBarCode.Rows[0]["goods_id"].ToString();
                            GetMo_itme(goods_id);
                            cmbGoods_id.Text = goods_id;
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

        private void txtProductNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtProductNo.Text = "";
        }

        private void get_data_details()
        {
            ClearPartOfText(); //清空文本框內容
            fill_plan_value();//首先將計劃單帶出數量、描述
            get_prd_records(1);//查詢已錄入的記錄
            chk_prd_no_complete();//檢查是否有未完成的記錄，默認帶出來
            get_prd_worker();//獲取選貨工號
            FillGrid();//將查詢到的記錄存入列表
            fill_txt_kg_pcs();//提取物料每公斤對應數量
            count_kg_pcs();//換算重量
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

        private void BTNREFRESHMO_Click(object sender, EventArgs e)
        {
            get_prd_records(1);//查詢已錄入的記錄
            FillGrid(); //將查詢到的記錄存入列表
            if (dgvDetails.Rows.Count > 0)
                fill_textbox(0);//填充各種控件
        }

        private void BTNNOSTART_Click(object sender, EventArgs e)
        {
            get_prd_records(4);//未開始的記錄
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
                if (this.chkIsComplete.Checked == false)
                    get_prd_records(6);//按制單編號查詢未完成的記錄
                else
                    get_prd_records(7);//按制單編號查詢包括已完成的記錄
                FillGrid(); //將查詢到的記錄存入列表
                if (txtSearchMo.Text.Length >= 9)
                    fill_textbox(0);//填充各種控件
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

        private void dgvDetails_Leave(object sender, EventArgs e)
        {
            edit_type = "Y";//控件作為編輯
        }

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                count_datetime();
                count_hour_std_qty();//計算每人每小時標準數
                count_req_time();//預計完成時間
            }
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                count_datetime();
                count_hour_std_qty();//計算每人每小時標準數
            }
        }

        private void btnExit_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
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
                count_hour_std_qty();//計算標準數量
                if (cmbProductDept.Text == "302")
                    Cout_prd_qty_alloy();//計算實際生產數量(合金部)
            }
        }

        private void txtPer_Convert_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_hour_std_qty();
        }

        private void txtper_Standrad_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_req_time();//預計完成時間
        }
        private void Cout_prd_qty_alloy()
        {
            if (txtRow_qty.Text != "" && txtPrd_Run_qty.Text != "")
            {
                txtPrd_qty.Text = (Convert.ToInt32(txtRow_qty.Text) * Convert.ToInt32(txtPrd_Run_qty.Text)).ToString();
            }
        }

        private void txtPrd_Run_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")//合金部 生產數 = 每碑數 * 實際碑數
                Cout_prd_qty_alloy();
        }

        private void cmbProductDept_Leave(object sender, EventArgs e)
        {
            InitComBoxGroup();//初始化組別
        }

        private void txtStart_run_TextChanged(object sender, EventArgs e)
        {
            count_run_qty();//計算實際碑數  合金部使用
        }
        private void count_run_qty()//計算實際碑數  合金部使用
        {
            if (edit_type == "Y" && cmbProductDept.Text == "302")//當是在編輯狀態且302部門時
            {
                txtPrd_Run_qty.Text = "";
                if (txtEnd_run.Text != "")
                    txtPrd_Run_qty.Text = (Convert.ToInt32(txtEnd_run.Text) - Convert.ToInt32((txtStart_run.Text != "" ? txtStart_run.Text : "0"))).ToString();
            }
        }

        private void txtEnd_run_TextChanged(object sender, EventArgs e)
        {
            count_run_qty();//計算實際碑數  合金部使用
        }
        //此單再續時，將上一次結束的碑數，當做為今次的開始數
        private void get_last_run_qty()
        {
            DataTable db_last_run_qty = new DataTable();
            string sql = "";
            sql += " Select end_run From product_records a" +
                " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'" +
                " And a.prd_mo = " + "'" + txtmo_id.Text.ToString() + "'" +
                " And a.prd_item = " + "'" + cmbGoods_id.Text.ToString() + "'" +
                " And a.prd_work_type = '" + "A02" + "'" +
                " And a.prd_start_time <> '' " + " And a.prd_end_time <> '' " +
                " Order by prd_date Desc,prd_end_time Desc";
            db_last_run_qty = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            if (db_last_run_qty.Rows.Count > 0)
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
            txtPrd_id.Text = record_id.ToString();
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            dtpStart.Text = "00:00";
            dtpEnd.Text = "00:00";
            dtpReqEnd.Text = "00:00";
            txtNormal_work.Text = "";
            txtAdd_work.Text = "";
            txtStart_run.Text = "";
            txtEnd_run.Text = "";
            txtTotalQty.Text = "";
            txtOk_weg.Text = "";
            txtNook_weg.Text = "";
            txtOk_qty.Text = "";
            txtNook_qty.Text = "";
            dteProdcutDate.Text =System.DateTime.Now.ToString("yyyy/MM/dd");
            get_last_run_qty();//獲取最後一次的碑數
            get_last_prd_end_time();//查詢組別當日最後的完成時間，作為開始時間
            get_group_member();//獲取組別的成員
            get_total_prd_qty();//顯示單的總完成數量

        }

        private void dtpEnd_MouseDown(object sender, MouseEventArgs e)
        {
            if (dtpEnd.Text == "00:00" && dtpStart.Text != "00:00")
                dtpEnd.Value = System.DateTime.Now;
        }

        private void txtMachine_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtMachine.Text == "")
            {
                if (_userid == "BUT01")
                    txtMachine.Text = "NBY-";
                else
                    if (_userid == "BUT02")
                        txtMachine.Text = "NDG-";
                    else
                        if (_userid == "ALY01")
                            txtMachine.Text = "ABY-";
            }
            // System.Diagnostics.Process.Start("osk.exe"); 
        }

        private void dtpStart_MouseDown(object sender, MouseEventArgs e)
        {
            if (dtpStart.Text == "00:00")
                dtpStart.Value = System.DateTime.Now;
        }

        private void txtWorker_Leave(object sender, EventArgs e)
        {
            if (txtWorker.Text.Trim() != "")
                txtWorker.Text = txtWorker.Text.PadLeft(10, '0');
        }

        private void txtWorker_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13 && txtWorker.Text.Trim() != "")
            {
                string hrm1name;
                txtWorker.Text = txtWorker.Text.PadLeft(10, '0');
                hrm1name = chk_prd_worker(txtWorker.Text);
                if (hrm1name != "")
                {
                    add_prd_worker(txtWorker.Text, hrm1name);
                }
            }
        }
        //加入生產工號
        private void add_prd_worker(string wid, string hrm1name)
        {
            bool sresult = false;
            int count_member = dtWorker.Rows.Count;
            for (int i = 0; i < dtWorker.Rows.Count; i++)
            {
                if (dtWorker.Rows[i]["prd_worker"].ToString() == txtWorker.Text)
                {
                    sresult = true;
                    break;
                }
            }
            if (sresult == false)
            {
                DataRow dr = null;
                dr = dtWorker.NewRow();
                dr["prd_worker"] = wid;
                dr["hrm1name"] = hrm1name;
                dtWorker.Rows.Add(dr);
                txtWorker.Text = "";
                txtTotMember.Text = (count_member + 1).ToString();
            }
            count_hour_std_qty();//計算每人每小時標準數
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (valid_data())
            {
                product_records objModel = new product_records();
                objModel.prd_dep = cmbProductDept.SelectedValue.ToString();
                objModel.prd_pdate = dteProdcutDate.Text.ToString();//計劃日期=生產日期
                objModel.prd_date = dteProdcutDate.Text.ToString();
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
                objModel.per_hour_std_qty = (txtPer_hour_std_qty.Text != "" ? Convert.ToInt32(txtPer_hour_std_qty.Text) : 0);
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
                objModel.pack_num = (txtPack_num.Text != "" ? Convert.ToInt32(txtPack_num.Text) : 1);
                objModel.work_code = txtWork_code.Text;
                objModel.job_type = "";
                objModel.work_class = "";
                objModel.prd_run_qty = (txtPrd_Run_qty.Text != "" ? Convert.ToSingle(txtPrd_Run_qty.Text) : 0);
                objModel.speed_lever = (txtSpeed_lever.Text != "" ? Convert.ToInt32(txtSpeed_lever.Text) : 0);
                objModel.start_run = (txtStart_run.Text != "" ? Convert.ToInt32(txtStart_run.Text) : 0);
                objModel.end_run = (txtEnd_run.Text != "" ? Convert.ToInt32(txtEnd_run.Text) : 0);
                objModel.ok_qty = (txtOk_qty.Text != "" ? Convert.ToInt32(txtOk_qty.Text) : 0);
                objModel.ok_weg = Math.Round((txtOk_weg.Text != "" ? Convert.ToDecimal(txtOk_weg.Text) : 0), 4);
                objModel.no_ok_qty = (txtNook_qty.Text != "" ? Convert.ToInt32(txtNook_qty.Text) : 0);
                objModel.no_ok_weg = Math.Round((txtNook_weg.Text != "" ? Convert.ToDecimal(txtNook_weg.Text) : 0), 4);
                objModel.sample_no = (txtSample_no.Text != "" ? Convert.ToInt32(txtSample_no.Text) : 0);
                objModel.sample_weg = Math.Round((txtSample_weg.Text != "" ? Convert.ToDecimal(txtSample_weg.Text) : 0), 4);
                objModel.member_no = (txtTotMember.Text.Trim() != "" ? Convert.ToInt32(txtTotMember.Text) : 1);
                objModel.prd_id_ref = (txtPrd_id_ref.Text != "" ? Convert.ToInt32(txtPrd_id_ref.Text) : 0);
                objModel.actual_pack_num = (txtPack_num.Text != "" ? Convert.ToInt32(txtPack_num.Text) : 1);
                objModel.actual_qty = (txtActual_qty.Text != "" ? Convert.ToInt32(txtActual_qty.Text) : 0);
                objModel.actual_weg = (txtActual_weg.Text != "" ? Convert.ToInt32(txtActual_weg.Text) : 0);
                if (objModel.prd_dep == "302" && objModel.actual_qty != 0 || objModel.actual_weg != 0)//這個應該是合金部選貨時更新
                {
                    objModel.conf_flag = "Y";
                    objModel.conf_time = DateTime.Now;
                }
                else
                {
                    if (objModel.prd_dep == "105"
                && objModel.prd_start_time != "" && objModel.prd_end_time != "")//林口部，將NEP的直接加入磅貨中，當作組裝批量輸入
                    {
                        objModel.conf_flag = "Y";
                        objModel.actual_qty = objModel.prd_qty;
                        objModel.actual_weg = Convert.ToDecimal(objModel.prd_weg);
                        objModel.conf_time = DateTime.Now;
                    }
                    else
                    {
                        objModel.conf_flag = "";
                        objModel.conf_time = Convert.ToDateTime("1900/01/01");
                    }
                }
                
                try
                {
                    if (record_id == -1)
                    {

                        record_id = clsPublicOfPad.GenNo("frmProductionSchedule");//自動產生序列號

                        if (record_id > 0)
                        {
                            objModel.prd_id = record_id;
                            if (txtPrd_id_ref.Text == "")//此單再續的，需要用回舊的記錄號
                            {
                                txtPrd_id_ref.Text = record_id.ToString();
                                objModel.prd_id_ref = record_id;
                            }
                            Result = clsProductionSchedule.AddProductionRecords(objModel);
                        }
                        else
                        {
                            MessageBox.Show("儲存記錄失敗!");
                            return;
                        }

                        txtPrd_id.Text = record_id.ToString().Trim();
                    }
                    else
                    {
                        Result = clsProductionSchedule.UpdateProductionRecords(objModel);
                    }
                    if (record_id > 0)
                    {
                        //檢查是否存在工號,若有則先刪除
                        if (clsProductionSchedule.DeletePrdWorker(record_id) > 0)
                        {
                            //重新儲存工號
                            for (int i = 0; i < dtWorker.Rows.Count; i++)
                            {
                                if (dtWorker.Rows[i]["prd_worker"].ToString() != "")
                                    Result = clsProductionSchedule.AddPrdWorker(record_id, dtWorker.Rows[i]["prd_worker"].ToString(), _userid, DateTime.Now);
                            }
                        }
                        MessageBox.Show("保存成功!");
                    }
                    else
                        MessageBox.Show("獲取記錄號失敗,不能更新工號表!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                OperationType = clsUtility.enumOperationType.Save;
                ToolStripButtonEvents();
                txtBarCode.Focus();
            }
        }
        //計算每KG對應數量
        private void count_kg_pcs()
        {
            txtkgPCS.Text = "";
            if (txtSample_no.Text != "" && txtSample_weg.Text != "" && Convert.ToSingle(txtSample_weg.Text) != 0)
                txtkgPCS.Text = Math.Round(Convert.ToInt32(txtSample_no.Text) / (Convert.ToSingle(txtSample_weg.Text)), 0).ToString();
            count_select_qty();//計算良品數量
        }
        private void count_select_qty()
        {
            if (txtkgPCS.Text != "" && txtkgPCS.Text != "0")
            {
                if (txtOk_weg.Text != "")
                    txtOk_qty.Text = Math.Round(Convert.ToSingle(txtOk_weg.Text) * Convert.ToInt32(txtkgPCS.Text), 0).ToString();
                if (txtNook_weg.Text != "")
                    txtNook_qty.Text = Math.Round(Convert.ToSingle(txtNook_weg.Text) * Convert.ToInt32(txtkgPCS.Text), 0).ToString();
            }
            add_prd_qty();//生產數量=良品+不良品
        }
        //生產數量=良品+不良品
        private void add_prd_qty()
        {
            txtPrd_qty.Text = ((txtOk_qty.Text != "" ? Convert.ToInt32(txtOk_qty.Text) : 0) + (txtNook_qty.Text != "" ? Convert.ToInt32(txtNook_qty.Text) : 0)).ToString();
            txtprd_weg.Text = ((txtOk_weg.Text != "" ? Convert.ToSingle(txtOk_weg.Text) : 0) + (txtNook_weg.Text != "" ? Convert.ToSingle(txtNook_weg.Text) : 0)).ToString();
            count_hour_std_qty();//計算每人每小時標準數
        }

        private void cmbGroup_Leave(object sender, EventArgs e)
        {
            //get_group_member();//獲取組別的成員
            //get_last_prd_end_time();//查詢組別當日最後的完成時間，作為開始時間
            //if (cmbGroup.Text == "AB99")
            //{
            //    dtpStart.Value = System.DateTime.Now;
            //    dtpEnd.Value = dtpStart.Value;
            //}
        }
        //獲取組別的成員
        private void get_group_member()
        {
            if (txtmo_id.Text == "" || cmbGoods_id.Text == "")
                return;
            DataTable dtMember = new DataTable();

            int count = dtWorker.Rows.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                dtWorker.Rows.RemoveAt(i);
            }
            txtTotMember.Text = "";
            dtMember = GetWorkerByGroup(cmbProductDept.SelectedValue.ToString(), cmbGroup.Text.ToString());
            if (dtMember.Rows.Count > 0)
            {
                for (int i = 0; i < dtMember.Rows.Count; i++)
                {
                    add_prd_worker(dtMember.Rows[i]["prd_worker"].ToString(), dtMember.Rows[i]["hrm1name"].ToString());
                }
            }
        }

        private DataTable GetWorkerByGroup(string prod_dept, string group)
        {
            string sql = "";
            DataTable dtWorker = new DataTable();
            try
            {
                sql += " Select a.prd_worker,b.hrm1name From product_group_member a " +
               " Left Join dgsql1.dghr.dbo.hrm01 b on a.prd_worker=b.hrm1wid  COLLATE Chinese_PRC_CI_AS " +
               " Where a.prd_dep = " + "'" + prod_dept + "'" +
               " And a.prd_group = " + "'" + group + "'";
                dtWorker = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtWorker;
        }

        //計算每人每小時標準數
        private void count_hour_std_qty()
        {
            int qty = (txtPrd_qty.Text != "" ? Convert.ToInt32(txtPrd_qty.Text) : 0);
            int rs = (txtTotMember.Text != "" ? Convert.ToInt32(txtTotMember.Text) : 1);
            float tt = (txtNormal_work.Text != "" ? Convert.ToSingle(txtNormal_work.Text) : 0) + (txtAdd_work.Text != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
            txtper_Standrad_qty.Text = "";
            txtPer_hour_std_qty.Text = "";
            if (txtNormal_work.Text != "" || txtAdd_work.Text != "")
            {
                txtper_Standrad_qty.Text = Math.Round(qty / rs / tt, 0).ToString();
                txtPer_hour_std_qty.Text = txtper_Standrad_qty.Text;
            }
            //count_req_time();//預計完成時間
        }

        private void txtNormal_work_Leave(object sender, EventArgs e)
        {
            count_hour_std_qty();//計算每人每小時標準數
        }

        private void txtAdd_work_Leave(object sender, EventArgs e)
        {
            count_hour_std_qty();//計算每人每小時標準數
        }

        private void txtTotMember_Leave(object sender, EventArgs e)
        {
            count_hour_std_qty();//計算每人每小時標準數
        }

        private void txtOkqty_All_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_ok_weg();
        }
        private void count_ok_weg()
        {
            txtOk_weg.Text = "";
            if (txtOkqty_All.Text != "" && txtmWeg1.Text != "")
                txtOk_weg.Text = (Convert.ToSingle(txtOkqty_All.Text) - Convert.ToSingle(txtmWeg1.Text)).ToString();
            count_select_qty();//計算良品數量
        }
        private void count_nook_weg()
        {
            txtNook_weg.Text = "";
            if (txtNookqty_All.Text != "" && txtmWeg2.Text != "")
                txtNook_weg.Text = (Convert.ToSingle(txtNookqty_All.Text) - Convert.ToSingle(txtmWeg2.Text)).ToString();
            count_select_qty();//計算良品數量
        }

        private void txtNookqty_All_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_nook_weg();
        }

        private void txtmWeg1_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_ok_weg();
        }

        private void txtmWeg2_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_nook_weg();
        }

        private void cmbGroup_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                get_group_member();//獲取組別的成員
                get_last_prd_end_time();//查詢組別當日最後的完成時間，作為開始時間
                if (cmbGroup.Text == "AB99")
                {
                    dtpStart.Value = System.DateTime.Now;
                    dtpEnd.Value = dtpStart.Value;
                }
            }
        }

        private void dgvWorker_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (dtWorker.Rows[0]["prd_worker"].ToString() == "")
                txtTotMember.Text = (Convert.ToInt32(txtTotMember.Text) - 1).ToString();
        }

        private void btnDeleteMember_Click(object sender, EventArgs e)
        {
            if (dgvWorker.Rows.Count > 0)
            {
                dtWorker.Rows.RemoveAt(dgvWorker.CurrentCell.RowIndex);
                txtTotMember.Text = dgvWorker.Rows.Count.ToString();
            }
        }

        private void btnKgPcsHide_Click(object sender, EventArgs e)
        {
            pnlKgPcs.Visible = false;
        }

        private void btnKgPcsShow_Click(object sender, EventArgs e)
        {
            pnlKgPcs.Visible = true;
            txtSample_no.Focus();
        }

        private void btnWorkerShow_Click(object sender, EventArgs e)
        {
            pnlWorker.Visible = true;
        }

        private void btnWorkerHide_Click(object sender, EventArgs e)
        {
            pnlWorker.Visible = false;
        }

        private void txtSample_no_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_kg_pcs();
        }

        private void txtSample_weg_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                count_kg_pcs();
        }

        private void txtOk_weg_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                txtOk_qty.Text = "";
                count_select_qty();//計算良品數量
            }
        }

        private void txtOk_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                add_prd_qty();//生產數量=良品+不良品
        }

        private void txtNook_weg_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                txtNook_qty.Text = "";
                count_select_qty();//計算不良品數量
            }
        }

        private void txtNook_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
                add_prd_qty();//生產數量=良品+不良品
        }

        private void txtPrd_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                //count_kg_pcs();  ////計算每KG對應數量
                count_hour_std_qty();//計算每人每小時標準數
                count_req_time();//預計完成時間
            }
        }

        private void txtmo_id_MouseDown(object sender, MouseEventArgs e)
        {
            clsUtility.Call_imput();
        }

        private void txtActual_weg_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                if (txtkgPCS.Text != "" && txtActual_weg.Text != "0")
                    txtActual_qty.Text = Math.Round(Convert.ToSingle(txtActual_weg.Text) * Convert.ToInt32(txtkgPCS.Text), 0).ToString();
            }
        }

        private void btnInsertWorker1_Click(object sender, EventArgs e)
        {
            if (dgvWorker2.Rows.Count > 0)
            {
                if (cmbGroup1.Text != " " && cmbGroup2.Text != " ")
                {
                    if (cmbGroup1.Text != cmbGroup2.Text)
                    {
                        product_group_member objGroupMember = new product_group_member();
                        objGroupMember.prd_dep = cmbProductDept.SelectedValue.ToString();
                        objGroupMember.prd_group_In = cmbGroup1.Text.ToString();
                        objGroupMember.prd_group_Out = cmbGroup2.Text.ToString();
                        objGroupMember.prd_worker = dgvWorker2.CurrentRow.Cells["colWorker_no2"].Value.ToString();
                        objGroupMember.crusr = DBUtility._user_id;
                        objGroupMember.crtim = DateTime.Now;

                        int Result = clsProductionSchedule.UpdateWorkerGroup(objGroupMember);
                        if (Result > 0)
                        {
                            GetGroup1();
                            dgvWorker2.Rows.RemoveAt(dgvWorker2.CurrentRow.Index);
                        }
                        else
                        {
                            MessageBox.Show("成員組別變更失敗！");
                        }
                    }
                }
            }
        }

        private void btnInsertWorker2_Click(object sender, EventArgs e)
        {
            if (dgvWorker1.Rows.Count > 0)
            {
                if (cmbGroup2.Text != " " && cmbGroup1.Text != " ")
                {
                    if (cmbGroup1.Text != cmbGroup2.Text)
                    {
                        product_group_member objGroupMember = new product_group_member();
                        objGroupMember.prd_dep = cmbProductDept.SelectedValue.ToString();
                        objGroupMember.prd_group_In = cmbGroup2.Text.ToString();
                        objGroupMember.prd_group_Out = cmbGroup1.Text.ToString();
                        objGroupMember.prd_worker = dgvWorker1.CurrentRow.Cells["colWorker_no1"].Value.ToString();
                        objGroupMember.crusr = DBUtility._user_id;
                        objGroupMember.crtim = DateTime.Now;

                        int Result = clsProductionSchedule.UpdateWorkerGroup(objGroupMember);
                        if (Result > 0)
                        {
                            GetGroup2();
                            dgvWorker1.Rows.RemoveAt(dgvWorker1.CurrentRow.Index);
                        }
                        else
                        {
                            MessageBox.Show("成員組別變更失敗！");
                        }
                    }
                }
            }
        }

        private void btnDelWorker_Click(object sender, EventArgs e)
        {
            if (dgvWorker1.Rows.Count > 0)
            {
                int Result = clsProductionSchedule.DelWorkerGroup(cmbProductDept.SelectedValue.ToString(), cmbGroup1.Text.ToString(), dgvWorker1.CurrentRow.Cells["colWorker_no1"].Value.ToString());
                if (Result > 0)
                {
                    dgvWorker1.Rows.RemoveAt(dgvWorker1.CurrentRow.Index);
                    dgvWorker1.Refresh();
                }
                else
                {
                    MessageBox.Show("刪除失敗！");
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage3)
            {
                GetGroups();
                DataTable dtGroup1 = dtGroup.Copy();
                cmbGroup1.DataSource = dtGroup1;
                cmbGroup1.DisplayMember = "work_group";
                cmbGroup1.ValueMember = "work_group";

                DataTable dtGroup2 = dtGroup.Copy();
                cmbGroup2.DataSource = dtGroup2;
                cmbGroup2.DisplayMember = "work_group";
                cmbGroup2.ValueMember = "work_group";
            }
        }

        private void cmbGroup1_TextChanged(object sender, EventArgs e)
        {
            GetGroup1();
        }

        private void cmbGroup2_TextChanged(object sender, EventArgs e)
        {
            GetGroup2();
        }

        private void GetGroup1()
        {
            DataTable dtworker = GetWorkerByGroup(cmbProductDept.SelectedValue.ToString(), cmbGroup1.Text.ToString());
            dgvWorker1.DataSource = dtworker;
        }

        private void GetGroup2()
        {
            DataTable dtworker = GetWorkerByGroup(cmbProductDept.SelectedValue.ToString(), cmbGroup2.Text.ToString());
            dgvWorker2.DataSource = dtworker;
        }

        private void btnAddWorkerNo_Click(object sender, EventArgs e)
        {
            if (cmbGroup1.Text != " ")
            {
                if (txtWorker_no.Text != "")
                {
                    string strWorker_no = AutoCompletion(txtWorker_no.Text.Trim());

                    bool IsAdd = true;
                    for (int i = 0; i < dgvWorker1.Rows.Count; i++)
                    {
                        if (strWorker_no == dgvWorker1.Rows[i].Cells["colWorker_no1"].Value.ToString())
                        {
                            IsAdd = false;
                            break;
                        }
                    }

                    if (IsAdd)
                    {
                        int Result = clsProductionSchedule.AddWorkerGroup(cmbProductDept.SelectedValue.ToString(), cmbGroup1.Text.ToString(), strWorker_no);
                        if (Result > 0)
                        {
                            GetGroup1();
                            txtWorker_no.Text = "";
                        }
                        else
                        {
                            MessageBox.Show("添加工號失敗！");
                        }
                    }
                    else
                    {
                        MessageBox.Show("此工號已存在！");
                    }
                }
            }
        }

        private string AutoCompletion(string strNo)
        {
            string strNew_No = "";
            string zero = "";
            for (int i = 0; i < 9; i++)
            {
                if ((zero + strNo).Length == 10)
                {
                    strNew_No = zero + strNo;
                    break;
                }
                zero += "0";
            }

            return strNew_No;
        }

    }
}
