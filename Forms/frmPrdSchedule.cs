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
using CFPublic;
using cf_pad.MDL;
using cf_pad.CLS;

namespace cf_pad.Forms
{
    public partial class frmPrdSchedule : Form
    {
        BardCodeHooK BarCode = new BardCodeHooK();
        DataTable dtPrd_dept = new DataTable();
        DataTable dtMo_item = new DataTable();
        DataTable dtWork_type = new DataTable();
        DataTable dtMachine_std = new DataTable();
        DataTable dtGroup = new DataTable();//組別
        DataTable dtProductionRecordslist = new DataTable();
        DataTable dtWorker = new DataTable();
        private string remote_db = DBUtility.remote_db;
        private string remote_db_hr = DBUtility.remote_db_hr;
        private string edit_type="Y";//控制當控件中當值發生變化時的操作
        private clsUtility.enumOperationType OperationType;
        private int Result = 0;
        private string _userid = DBUtility._user_id.ToUpper();
        private product_records objModel;
        private int record_id = -1;//未完成記錄的ID，若查找到，則說明未完成，在保存時，執行更新操作
        private int BarCodeMinLength = 4;//這個為測試用，如果是正常的制單條碼，長度為10


        public frmPrdSchedule()
        {
            InitializeComponent();

            //clsControlInfoHelper controlInfo = new clsControlInfoHelper("frmProductionSchedule", this.Controls);
            //controlInfo.GenerateContorl();
            GetAllComboxData();

            ////暫時取消屏幕撲捉的輸入(2019/11/22日)
            //BarCode.BarCodeEvent += new BardCodeHooK.BardCodeDeletegate(BarCode_BarCodeEvent);
            
        }
        //private delegate void ShowInfoDelegate(BardCodeHooK.BarCodes barCode);

        //////暫時取消屏幕撲捉的輸入(2019/11/22日)
        //private void ShowInfo(BardCodeHooK.BarCodes barCode)
        //{
        //    if (this.InvokeRequired)
        //    {
        //        this.BeginInvoke(new ShowInfoDelegate(ShowInfo), new object[] { barCode });
        //    }
        //    else
        //    {
        //        string strBarCode1 = "";
        //        strBarCode1 = barCode.IsValid ? barCode.BarCode : "";//是否为扫描枪输入，如果为true则是 否则为键盘输入
        //        strBarCode1 = strBarCode1.Replace("\r\n", "").Replace("'", "").Replace("\0", "").Replace("\r", "");

        //        //textBox7.Text += barCode.KeyName;
        //        if (strBarCode1.Length > BarCodeMinLength)
        //        {
        //            txtBarCode.Text = strBarCode1.Trim().ToUpper();

        //            //MessageBox.Show(strBarCode);
        //            if (strBarCode1.Length == 5)//如果掃描的是工號// && barcode.Substring(0, 5) == "00000"
        //            {
        //                txtProductNo.Text = strBarCode1;
        //                txtBarCode.Text = "";
        //                txtProductNo.Focus();
        //            }
        //            else
        //            {
        //                doBarCode();
        //                txtBarCode.Focus();
        //                //MessageBox.Show(barCode.IsValid.ToString());
        //            }
        //        }
        //    }
        //}
        ////暫時取消屏幕撲捉的輸入(2019/11/22日)
        //void BarCode_BarCodeEvent(BardCodeHooK.BarCodes barCode)
        //{

        //    ShowInfo(barCode);
        //}

        private void frmProductionSchedule_Load(object sender, EventArgs e)
        {
            BarCode.Start();
            InitComBoxs();

            //加載時讓條碼框獲得焦點
            //txtBarCode.Focus();

            Font a = new Font("GB2312", 12);//GB2312为字体名称，1为字体大小dataGridView1.Font = a;
            dgvAndSingle.Font = a;
            dgvDetails.Font = a;
            dgvDetails.AutoGenerateColumns = false;
            dgvWorker.Font = a;
            dgvWorker.AutoGenerateColumns = false;
            if (cmbProductDept.SelectedValue.ToString() == "J01")
                lblStandard_per_qty.Text = "標準時產能";

            //tabControl1.SizeMode   =   TabSizeMode.Fixed;
            //tabControl1.ItemSize   =   new   Size(0,1);


            //this.tabControl1.Region = new Region(new RectangleF(this.tabPage1.Left, this.tabPage1.Top, this.tabPage1.Width, this.tabPage1.Height));
            
        }

        //獲取生產部門、工作類型
        private void GetAllComboxData()
        {
            dtPrd_dept = clsProductionSchedule.GetAllPrd_dept();
            dtWork_type = clsProductionSchedule.GetWorkType();
        }

        private void InitComBoxs()
        {
            //初始化生產部門
            cmbProductDept.DataSource = dtPrd_dept;
            cmbProductDept.DisplayMember = "int9desc";
            cmbProductDept.ValueMember = "int9loc";
            string userid_part=_userid.Substring(0, 3);
            if (userid_part == "BUT01")
            {
                cmbProductDept.SelectedValue = "J03";
            }
            else
            {
                if (userid_part == "ALY")
                    cmbProductDept.SelectedValue = "J01";
                else
                {
                    if (userid_part == "BUT02")
                        cmbProductDept.SelectedValue = "104";
                    else
                    {
                        if (userid_part == "BLP")
                            cmbProductDept.SelectedValue = "J07";
                    }
                }
            }
            //初始化工作類型
            cmbWorkType.DataSource = dtWork_type;
            cmbWorkType.DisplayMember = "work_type_desc";
            cmbWorkType.ValueMember = "work_type_id";

            InitComBoxGroup();
            //初始化班次、組別
            cmbOrder_class.Items.Add("白班");
            cmbOrder_class.Items.Add("夜班");
            cmbOrder_class.Text = "白班";

            SetControlVisible();//設置控件可見

            dteProdcutDate.Text = clsUtility.changeDateToChar(System.DateTime.Now);


            cmbSearch.Items.Add("制單編號--可部分輸入");
            cmbSearch.Items.Add("機器--最近一次的生產");
            cmbSearch.Items.Add("當日未完成的制單");
            //cmbSearch.Items.Add("未開始生產的制單");
            cmbSearch.Items.Add("當日已完成的制單");
            cmbSearch.Items.Add("按工號查詢");
            //將工號綁定到dgvWorker中
            dtWorker.Columns.Add("prd_worker", typeof(string)); //数据类型为 文本
            dtWorker.Columns.Add("workerName", typeof(string)); //数据类型为 文本
            dgvWorker.DataSource = dtWorker;

        }
        //提取工作組別
        private void InitComBoxGroup()
        {
            string strSql = "";
            string dep = cmbProductDept.SelectedValue.ToString().Trim();
            strSql = " SELECT work_group,group_desc FROM work_group WHERE ( dep='" + dep + "'" + " AND group_type='" + "1" + "') " + " OR dep='" + "000" + "' ";
            dtGroup = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            if (dtGroup.Rows.Count > 0)
            {
                cmbGroup.DataSource = dtGroup;
                cmbGroup.DisplayMember = "work_group";
                cmbGroup.ValueMember = "work_group";
            }
            if (dep == "J07")
                cmbWorkType.SelectedValue = "A02";
            else
                cmbWorkType.SelectedValue = "A01";


            getJobType();
            //getJobTimes();//噴油排板次數
        }
        private void getJobType()
        {
            string dep = cmbProductDept.SelectedValue.ToString().Trim();
            DataTable dtJobType = clsProductionSchedule.getJobType(dep);
            cmbJob_type.DataSource = dtJobType;
            cmbJob_type.DisplayMember = "job_desc";
            cmbJob_type.ValueMember = "job_type";
        }
        //噴油排板的次數
        private void getJobTimes()
        {
            if (txtWipDep.Text.Trim() == "510")
            {
                string dep="";
                string jobType=(cmbJob_type.SelectedValue != null ? cmbJob_type.SelectedValue.ToString() : "");
                if (jobType == "J0702" || jobType == "J0703")
                    dep = "5101";
                else
                    dep = "5102";
                DataTable dtJobTimes = clsProductionSchedule.getJobType(dep);
                cmbDifficultyLevel.DataSource = dtJobTimes;
                cmbDifficultyLevel.DisplayMember = "job_desc";
                cmbDifficultyLevel.ValueMember = "job_type";
            }
        }
        private void getWipDep()
        {
            txtWipDep.Text = clsPublicOfPad.getDepJx(cmbProductDept.SelectedValue.ToString().Trim(), "");
        }

        //提取噴油的排板規格、標準
        private void getWorkCodeList()
        {
            if (txtWipDep.Text.Trim() == "510")//如果是噴油部門，則綁定排板的標準
            {
                DataTable dtMacStd = clsProductionSchedule.getWorkCodeList(txtWipDep.Text.Trim());
                if (dtMacStd.Rows.Count > 0)
                {
                    cmbWorkCode.DataSource = dtMacStd;
                    cmbWorkCode.DisplayMember = "machine_desc";
                    cmbWorkCode.ValueMember = "machine_id";
                }
            }
        }
        //提取噴油的排板規格、標準
        private void getWorkCodeStd()
        {
            string dep = txtWipDep.Text.Trim();
            string machine_id = cmbWorkCode.SelectedValue != null ? cmbWorkCode.SelectedValue.ToString() : "";
            txtRow_qty.Text = clsProductionSchedule.getWorkCodeStd(dep, machine_id);
        }
        private void txtmo_id_Leave(object sender, EventArgs e)
        {
            cmbGoods_id.Text = "";
            txtgoods_desc.Text = "";
            cmbGoods_id.Items.Clear();

            string dep = txtWipDep.Text.Trim();//cmbProductDept.SelectedValue.ToString();
            if (dep == "104")//如果是104幫102加工的，則將部門改成102來提取記錄
                dep = "102";
            dtMo_item = clsProductionSchedule.getItemByMo(txtmo_id.Text.Trim(), dep);
            if (dtMo_item.Rows.Count > 0)
            {
                for (int i = 0; i < dtMo_item.Rows.Count; i++)
                {
                    cmbGoods_id.Items.Add(dtMo_item.Rows[i]["goods_id"].ToString());
                }
            }
        }

        //獲取制單編號資料，并綁定物料編號
        private void GetMo_itme(string item)
        {

            //string dep = cmbProductDept.SelectedValue.ToString();
            //if (dep == "104")//如果是104幫102加工的，則將部門改成102來提取記錄
            //    dep = "102";
            //dtMo_item = clsProductionSchedule.GetMo_dataById(txtmo_id.Text.Trim(), dep, item);

            ////初始化生產部門
            //cmbGoods_id.DataSource = dtMo_item;
            //cmbGoods_id.DisplayMember = "goods_id";
            //cmbGoods_id.ValueMember = "goods_id";



            
        }

        //查詢未完成的記錄，並重新賦值，便於重新輸入完整資料
        private void get_prd_records(int con_type)
        {
            string last_date = dteProdcutDate.Text;// System.DateTime.Now.AddDays(-7).ToString("yyyy/MM/dd");
            string remote_db = DBUtility.remote_db;
            string within_code = DBUtility.within_code;
            //獲取制單編號資料
            string strSql = "";
            if (con_type != 8)
            {
                //strSql += "Select aa.*,bb.name AS prd_item_cdesc From ( ";
                strSql += " Select Top 100000 a.*,rtrim(b.work_type_desc) as work_type_desc,c.prd_worker AS prd_worker_d,a.prd_item AS prd_item_cdesc " +
                ",d.job_desc" +
                " From product_records a with(nolock) " +
                " Left join work_type b with(nolock) on a.prd_work_type=b.work_type_id " +
                " Left Join product_records_worker c with(nolock) On a.prd_id=c.prd_id " +
                " Left Join job_type d with(nolock) On a.job_type=d.job_type ";
                strSql += " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'";
                //,d.name AS prd_item_cdesc
                //+ "' And d.within_code='" + within_code
                //" Inner Join " + remote_db + "it_goods d with(nolock) On a.prd_item=d.id COLLATE chinese_taiwan_stroke_CI_AS";
                if (cmbProductDept.SelectedValue.ToString() == "302")//如果是合金部，則不顯示選貨的記錄
                    strSql += " And a.prd_work_type <> " + "'" + "A03" + "'";
                if (con_type == 1)//是否查找當日未完成標識
                {
                    strSql += " AND a.prd_id='" + record_id + "'";
                    strSql += " Order By a.amtim Desc,a.crtim Desc ";
                }
                else if (con_type == 2)//未完成的記錄，用生產時間査，不是安排時間，因為一生產就已經是當日的日期了
                {
                    strSql += " And a.prd_date = " + "'" + last_date + "'";
                    //strSql += " And a.prd_start_time <> " + "'" + "" + "'" + " And a.prd_end_time = " + "'" + "" + "'";
                    strSql += " And a.prd_end_time = " + "'" + "" + "'";// " And a.prd_start_time <> " + "'" + "" + "'" +
                    strSql += " Order By a.prd_date desc,a.amtim Desc ";
                }
                else if (con_type == 4)//如果是查找當日所有記錄
                {
                    strSql += " And a.prd_date = " + "'" + last_date + "'";
                    strSql += " And c.prd_worker = " + "'" + txtFindMo.Text.Trim() + "'";
                    strSql += " Order By a.prd_date desc,a.amtim Desc ";
                }
                else if (con_type == 5)//當天完成的記錄，如果組別不為空，也按組別過濾
                {
                    strSql += " And a.prd_date = " + "'" + last_date + "'";
                    if (cmbGroup.Text.Trim() != "")
                        strSql += " And a.prd_group = " + "'" + cmbGroup.Text.Trim() + "'";
                    strSql += " And a.prd_start_time <> " + "'" + "" + "'" + " And a.prd_end_time <> " + "'" + "" + "'";
                    strSql += " Order By a.prd_date desc,a.amtim Desc ";
                }
                else if (con_type == 9)//按記錄號查詢生產記錄，並單記錄時使用
                {
                    strSql += " And a.prd_id = " + "'" + record_id.ToString() + "'";
                    strSql += " Order By a.prd_date desc,a.amtim Desc ";
                }
                else if (con_type == 10)//按條形碼查找制單狀態，類似1情形，將未完成的排在前面
                {
                    strSql += " And a.prd_mo = " + "'" + txtFindMo.Text.Trim() + "'";
                    strSql += " And a.prd_item = " + "'" + txtBarCodeItem.Text.Trim() + "'";
                    //strSql += " Order By a.prd_end_time,a.prd_date Desc";//a.amtim Desc,a.crtim Desc ";
                    strSql += " Order By a.amtim Desc,a.crtim Desc ";
                }
                else if (con_type == 11)//按條形碼查找制單狀態，類似1情形，將未完成的排在前面
                {
                    strSql += " And a.prd_mo like " + "'%" + txtFindMo.Text.Trim() + "%'";
                    if (txtBarCodeItem.Text.Trim() != "")
                        strSql += " And a.prd_item = " + "'" + txtBarCodeItem.Text.Trim() + "'";
                    //strSql += " Order By a.prd_end_time,a.prd_date Desc";//a.amtim Desc,a.crtim Desc ";
                    strSql += " Order By a.amtim Desc,a.crtim Desc ";
                }
                //strSql += ") aa";
                //strSql += " Inner Join " + remote_db + "it_goods bb with(nolock) On aa.prd_item=bb.id COLLATE chinese_taiwan_stroke_CI_AS" +
                //    " Where bb.within_code='" + within_code + "'";
            }
            else//獲取機器最後一次生產記錄
            {
                string strsql_part;
                strsql_part = " (Select Max(prd_id) AS prd_id " +
                    " From product_records with(nolock) Where " +
                    " prd_dep= '" + cmbProductDept.SelectedValue.ToString().Trim() + "'"
                    + " and prd_date >='" + last_date + "'"
                    + " and prd_machine='" + txtFindMo.Text.Trim() + "'"
                    + " and prd_start_time <>'" + "" + "' and prd_end_time <>'" + "" + "'"
                    + " ) c ";

                strSql = " Select a.*,rtrim(b.work_type_desc) as work_type_desc " +
                " From product_records a with(nolock) " +
                " Left outer join work_type b with(nolock) on a.prd_work_type=b.work_type_id " +
                " Inner Join " + strsql_part + " on a.prd_id=c.prd_id";
            }


            dtProductionRecordslist = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            for (int i = 0; i < dtProductionRecordslist.Rows.Count; i++)
            {
                strSql = "Select name From it_goods Where within_code='" + within_code + "' And id='" + dtProductionRecordslist.Rows[i]["prd_item"].ToString() + "'";
                DataTable dt = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
                if (dt.Rows.Count > 0)
                    dtProductionRecordslist.Rows[i]["prd_item_cdesc"] = dt.Rows[0]["name"].ToString();
            }
            dgvDetails.DataSource = dtProductionRecordslist;
            dgvDetails.Refresh();
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
            string start_time = dtProductionRecordslist.Rows[index]["prd_start_time"].ToString();
            string end_time = dtProductionRecordslist.Rows[index]["prd_end_time"].ToString();
            dtpStart.Value = Convert.ToDateTime("2014/01/01 " + start_time);
            dtpEnd.Value = Convert.ToDateTime("2014/01/01 " + end_time);
            if (start_time=="" && end_time == "")
                lblMoStatus.Text = "未生產：沒有開始和結束時間，請錄入完整相關資料!";
            else
                if (start_time != "" && end_time == "")
                    lblMoStatus.Text = "生產中但未完成：沒有結束時間，請錄入完整相關資料!";
                else
                    if (start_time != "" && end_time != "")
                        lblMoStatus.Text = "已完成--若要繼續生產請點擊：繼續生產。";
                    else
                         if (start_time == "" && end_time != "")
                             lblMoStatus.Text = "生產中但未完成：沒有開始時間，請錄入完整相關資料!";
            txtNormal_work.Text = (dtProductionRecordslist.Rows[index]["prd_normal_time"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_normal_time"].ToString() : "");
            txtAdd_work.Text = (dtProductionRecordslist.Rows[index]["prd_ot_time"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_ot_time"].ToString() : "");
            txtRow_qty.Text = (dtProductionRecordslist.Rows[index]["line_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["line_num"].ToString() : "");
            txtPer_Convert_qty.Text = (dtProductionRecordslist.Rows[index]["hour_run_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["hour_run_num"].ToString() : "");
            txtper_Standrad_qty.Text = (dtProductionRecordslist.Rows[index]["hour_std_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["hour_std_qty"].ToString() : "");
            txtPrd_qty.Text = (dtProductionRecordslist.Rows[index]["prd_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_qty"].ToString() : "");
            txtReq_prd_qty.Text = (dtProductionRecordslist.Rows[index]["req_prd_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["req_prd_qty"].ToString() : "");
            txtprd_weg.Text = (dtProductionRecordslist.Rows[index]["prd_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_weg"].ToString() : "");
            txtkgPCS.Text = (dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() : "");
            txtActual_qty.Text = (dtProductionRecordslist.Rows[index]["actual_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["actual_qty"].ToString() : "");
            txtDifficulty_level.Text = dtProductionRecordslist.Rows[index]["difficulty_level"].ToString();
            getJobTimes();//綁定噴油的排板、噴油次數，再賦值：
            cmbDifficultyLevel.SelectedValue = txtDifficulty_level.Text;
            txtMatItem.Text = dtProductionRecordslist.Rows[index]["mat_item"].ToString();
            txtMatDesc.Text = dtProductionRecordslist.Rows[index]["mat_item_desc"].ToString();
            txtMatLot.Text = dtProductionRecordslist.Rows[index]["mat_item_lot"].ToString();
            dtpReqEnd.Text = dtProductionRecordslist.Rows[index]["prd_req_time"].ToString();//預計完成時間，用每次計算的時間
            txtToDep.Text = dtProductionRecordslist.Rows[index]["to_dep"].ToString();
            txtPrd_Run_qty.Text = dtProductionRecordslist.Rows[index]["prd_run_qty"].ToString();
            txtWork_code.Text = dtProductionRecordslist.Rows[index]["work_code"].ToString();
            cmbWorkCode.SelectedValue = txtWork_code.Text;
            txtSpeed_lever.Text = dtProductionRecordslist.Rows[index]["speed_lever"].ToString();
            txtPack_num.Text = (dtProductionRecordslist.Rows[index]["pack_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["pack_num"].ToString() : "");
            txtStart_run.Text = (dtProductionRecordslist.Rows[index]["start_run"].ToString() !="0" ? dtProductionRecordslist.Rows[index]["start_run"].ToString():"0");
            txtEnd_run.Text = (dtProductionRecordslist.Rows[index]["end_run"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["end_run"].ToString() : "");
            txtSample_no.Text = (dtProductionRecordslist.Rows[index]["sample_no"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["sample_no"].ToString() : "");
            txtSample_weg.Text = (dtProductionRecordslist.Rows[index]["sample_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["sample_weg"].ToString() : "");
            cmbJob_type.SelectedValue = dtProductionRecordslist.Rows[index]["job_type"].ToString();
            txtWork_class.Text = dtProductionRecordslist.Rows[index]["work_class"].ToString();
            txtPrd_id_ref.Text = dtProductionRecordslist.Rows[index]["prd_id_ref"].ToString();
            if (txtDifficulty_level.Text.Trim() == "" && dteProdcutDate.Text == "302")//合金部難度設定為1
                txtDifficulty_level.Text = "1";
            loadProductWorker();
            setProdDate();//自動設定生產日期為當前日期

        }
        //自動設定生產日期為當前日期
        private void setProdDate()
        {
            if (dtpStart.Text == "00:00" || dtpEnd.Text == "00:00")//若未有生產日期的，設定為當日生產的
            {
                if (cmbOrder_class.SelectedIndex == 0)//白班
                    dteProdcutDate.Text = clsUtility.changeDateToChar(System.DateTime.Now);
                else//夜班
                {
                    string now_time = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm").Substring(11, 5);
                    if (string.Compare(now_time, "20:30") >= 0 && string.Compare(now_time, "23:59") <= 0)
                        dteProdcutDate.Text = clsUtility.changeDateToChar(System.DateTime.Now);
                    else
                    {
                        if (string.Compare(now_time, "00:00") >= 0 && string.Compare(now_time, "08:30") <= 0)//如果是凌晨的，當前日期減一日
                            dteProdcutDate.Text = clsUtility.changeDateToChar(System.DateTime.Now.AddDays(-1));
                        else
                        {
                            dteProdcutDate.Text = clsUtility.changeDateToChar(System.DateTime.Now);
                        }
                    }
                }
            }
        }
        ///// <summary>
        ///// 綁定列表
        ///// </summary>
        //private void FillGrid()
        //{
        //    if (dtProductionRecordslist.Rows.Count > 0)
        //    {
        //        dgvDetails.DataSource = dtProductionRecordslist;
        //        dgvDetails.Refresh();
        //    }
        //    else
        //    {
        //        dgvDetails.DataSource = null;
        //    }
        //}
        //ToolStripButton click 事件集合
        private void ToolStripButtonEvents()
        {
            switch (OperationType)
            {
                case clsUtility.enumOperationType.Find:
                    {
                        get_prd_records(3);//查詢當日所有的記錄
                        //FillGrid(); //將查詢到的記錄存入列表
                    }
                    break;
                case clsUtility.enumOperationType.Delete:
                    {
                        
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
                //更新物料每KG對應的數量表
                if (cmbProductDept.SelectedValue.ToString().Trim() != "" && cmbGoods_id.Text.Trim() != "" && txtkgPCS.Text.Trim() != "" && cmbWorkType.SelectedValue.ToString().Trim() == "A02")
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

                get_prd_records(1);//查詢對應序號的記錄
                //FillGrid();//將查詢到的記錄存入列表
                fill_textbox(0);//填充各種控件
                edit_type = "Y";//設置可編輯狀態
                ////取消儲存後不自動轉換成生產(2018/09/05日取消)
                if (cmbWorkType.SelectedValue.ToString().Trim() == "A01" && dtpEnd.Text != "00:00")//如果是校模儲存後，自動轉入生產狀態
                {
                    record_id = -1;//變成新增狀態
                    txtProductNo.Text = "";
                    loadProductWorker();
                    dtpStart.Value = dtpEnd.Value.AddMinutes(1);//生產完成後，將校模完成時間作為生產開始時間DateTime.Now;
                    dtpEnd.Value = Convert.ToDateTime("2014/01/01 " + "00:00");
                    txtNormal_work.Text = "";
                    txtAdd_work.Text = "";
                    cmbWorkType.SelectedValue = "A02";//.Text = "生產";
                    chkcont_work1.Checked = false;
                    chkcont_work2.Checked = false;
                    show_message = "保存成功，校模完成，自動轉入生產狀態，可以點擊儲存按鈕以保存記錄!";
                    txtPrd_id_ref.Text = "";//取消此單再續
                }
                chkcont_work1.Checked = false;
                chkcont_work2.Checked = false;
                lblMoStatus.Text = show_message;
                //MessageBox.Show(show_message);
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
            dteProdcutDate.Text = clsUtility.changeDateToChar(System.DateTime.Now);
            cmbGoods_id.Items.Clear();
            txtgoods_desc.Text = "";
            txtMachine.Text = "";
            txtkgPCS.Text = "";
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            txtReq_prd_qty.Text = "";
            txtActual_qty.Text = "";
            txtProductNo.Text = "";
            cmbWorkType.SelectedValue = "A00";
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
            cmbDifficultyLevel.SelectedValue = "";
            cmbJob_type.SelectedValue = "";
            cmbWorkCode.SelectedValue = "";
            lblMoStatus.Text = "未定義";
            //dgvWorker.DataSource = null;
            loadProductWorker();
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
            //if (txtPrd_qty.Text != "" && !Verify.StringValidating(txtPrd_qty.Text.Trim(), Verify.enumValidatingType.AllNumber))
            if (!clsValidRule.IsNumeric(txtPrd_qty.Text))
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
            //如果是完成的，就要做如下控制
            if (dtpStart.Text != "00:00" && dtpEnd.Text != "00:00")
            {
                if (cmbWorkType.SelectedValue.ToString().Trim() == "A00")
                {
                    MessageBox.Show("不是有效的生產類型，請重新輸入!");
                    cmbWorkType.Focus();
                    return false;
                }
                if (txtMachine.Text == "" && txtWipDep.Text.Trim()!="510")//噴油部可以不用輸入機器
                {
                    if (cmbWorkType.SelectedValue.ToString().Trim() == "A01" || cmbWorkType.SelectedValue.ToString().Trim() == "A02")
                    {
                        MessageBox.Show("生產機器不能為空，請重新輸入!");
                        txtMachine.Focus();
                        return false;
                    }
                }
                if (chk_prd_worker(txtProductNo.Text.Trim()) == "")
                {
                    if (dgvWorker.Rows.Count == 0)
                    {
                        MessageBox.Show("生產工號不存在，請重新輸入!");
                        txtProductNo.Focus();
                        return false;
                    }
                }
                if ((txtNormal_work.Text != ""?Convert.ToDecimal(txtNormal_work.Text):0)==0
                    && (txtAdd_work.Text != "" ? Convert.ToDecimal(txtAdd_work.Text) : 0) == 0)
                {
                    MessageBox.Show("工時不存在,請重新輸入!");
                    txtNormal_work.Focus();
                    txtNormal_work.SelectAll();
                    return false;
                }
                if (cmbWorkType.SelectedValue.ToString().Trim() == "A02")//如果是生產，則要檢查數量不能為0
                {
                    if ((txtPrd_qty.Text != ""?Convert.ToDecimal(txtPrd_qty.Text):0)==0)
                    {
                        MessageBox.Show("生產數量不能為0,請重新輸入!");
                        txtPrd_qty.Focus();
                        txtPrd_qty.SelectAll();
                        return false;
                    }
                }
                if (chkPrdRecords() == false)//檢查之前是否存在重複時間的記錄
                    return false;
            }
            if (string.Compare(dteProdcutDate.Text, clsUtility.changeDateToChar(System.DateTime.Now)) > 0)
            {
                MessageBox.Show("生產日期不能大於當天日期，請重新輸入!");
                dteProdcutDate.Focus();
                return false;
            }
            //if (txtprd_weg.Text != "" && !Verify.StringValidating(txtprd_weg.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            if (!clsValidRule.IsNumeric(txtprd_weg.Text))
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

            if (!clsValidRule.IsNumeric(txtRow_qty.Text))//(txtRow_qty.Text != "" && !Verify.StringValidating(txtRow_qty.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
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
            if (!clsValidRule.IsNumeric(txtper_Standrad_qty.Text))//(txtper_Standrad_qty.Text.Trim() != ""
                //&& txtper_Standrad_qty.Text.Trim() != "0"
                //&& !Verify.StringValidating(txtper_Standrad_qty.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("每小時生產量格式有誤,請重新輸入!");
                txtper_Standrad_qty.Focus();
                txtper_Standrad_qty.SelectAll();
                return false;
            }
            if (!clsValidRule.IsNumeric(txtkgPCS.Text))//(txtkgPCS.Text != "" && !Verify.StringValidating(txtkgPCS.Text.Trim(), Verify.enumValidatingType.PositiveNumber))
            {
                MessageBox.Show("每KG對應數量的格式有誤,請重新輸入!");
                txtkgPCS.Focus();
                txtkgPCS.SelectAll();
                return false;
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
            dtPrdRecords=clsProductionSchedule.GetPrdRecords(record_id, cmbProductDept.SelectedValue.ToString().Trim(), txtmo_id.Text.Trim(), cmbGoods_id.Text.Trim(), dteProdcutDate.Text);
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
            string dep1 = "", dep2 = "ZZZ";
            string hrm1name = "";
            //if (cmbProductDept.SelectedValue.ToString().Trim() == "302")
            //{
            //    dep1 = "PL6-01";
            //    dep2 = "PL6-99";
            //}
            //else
            //{
            //    if (cmbProductDept.SelectedValue.ToString().Trim() == "101" || cmbProductDept.SelectedValue.ToString().Trim() == "102" || cmbProductDept.SelectedValue.ToString().Trim() == "104")
            //    {
            //        dep1 = "PL4-03";
            //        dep2 = "PL4-14";
            //    }
            //    else
            //    {
            //        if (cmbProductDept.SelectedValue.ToString().Trim() == "105")
            //        {
            //            dep1 = "PL4-05";
            //            dep2 = "PL4-05";
            //        }
            //        else
            //        {
            //            if (cmbProductDept.SelectedValue.ToString().Trim() == "202" || cmbProductDept.SelectedValue.ToString().Trim() == "203")
            //            {
            //                dep1 = "PL5-01";
            //                dep2 = "PL5-99";
            //            }
            //        }
            //    }
            //}
            //獲取制單編號資料 COLLATE Chinese_PRC_CI_AS
            string sql = "";
            sql += " Select a.hrm1wid,a.hrm1name " +
                " From " + remote_db_hr + "hrm01 a " +
                " Where a.hrm1stat2 >=" + "'" + dep1 + "'" + " and a.hrm1stat2<=" + "'" + dep2 + "'" + " and a.hrm1wid = " + "'" + wid + "'";
            dtWid = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            if (dtWid.Rows.Count > 0)
                hrm1name = dtWid.Rows[0]["hrm1name"].ToString();
            return hrm1name;
        }
        //獲取機器的各項標準數據
        private void GetMachine_std()
        {

            string strSql = "";
            string dep = cmbProductDept.SelectedValue.ToString().Trim();
            string machine_id = txtMachine.Text.Trim();
            strSql = "Select machine_id,machine_mul,machine_rate From machine_std Where dep='" + dep + "' AND machine_id='" + machine_id + "'";
            dtMachine_std = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);

        }


        //檢查機器代碼是否正確
        private bool checkMachine(int replace_type)//replace_type = 1 獲取機器難度
        {
            DataTable mac_tb;
            string strSql = "";
            //strSql = " SELECT resource_id AS machine_id,standby2 FROM " + remote_db + "cd_resource" +
            //       " WHERE department_id='" + cmbProductDept.SelectedValue.ToString() + "' AND resource_id ='" + txtMachine.Text.Trim() + "' ";
            strSql = "Select machine_id From machine_tb Where dep='" + cmbProductDept.SelectedValue.ToString() + "' AND machine_id='" + txtMachine.Text.Trim() + "'";
            mac_tb = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            if (mac_tb.Rows.Count == 0)
            {
                MessageBox.Show("機器代碼不存在,請重新輸入!");
                return false;
            }
            else
            {
                if (replace_type == 1)//獲取機器難度
                    txtDifficulty_level.Text = "0";// mac_tb.Rows[0]["standby2"].ToString();
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
            fillMachineVal();
        }
        private void fillMachineVal()
        {
            txtRow_qty.Text = "";
            txtPer_Convert_qty.Text = "";
            txtper_Standrad_qty.Text = "";
            if (txtMachine.Text.Trim() != "" && cmbProductDept.SelectedValue.ToString().Trim() != "")
            {
                GetMachine_std();//獲取機器的各項標準數據
                fill_textbox_machine_std();//填充機器各項標準
                if (cmbProductDept.SelectedValue.ToString().Trim() == "102")
                {
                    txtDifficulty_level.Text = "";
                    checkMachine(1);//獲取機器難度
                }
            }
        }

        /// <summary>
        /// 新的時間計算
        /// </summary>
        private void count_datetime()
        {
            if (dtpStart.Text.ToString() == "00:00" || dtpEnd.Text.ToString() == "00:00"
                || dtpStart.Text == "" || dtpEnd.Text=="")
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
                string pm_in_time = "13:30";//下午上班
                string pm_out_time = "17:30";//下午下班
                string night_in_time = "18:30";//晚上上班
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
            if (normal_time > 0)
                txtNormal_work.Text = Math.Round(normal_time, 3).ToString();
            else
                txtNormal_work.Text = "";
            if (ot_time > 0)
                txtAdd_work.Text = Math.Round(ot_time, 3).ToString();
            else
                txtAdd_work.Text = "";
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
                cmbGoods_id.Text = item;//物料編號
                txtgoods_desc.Text = GetItemDesc(item);//獲取物料描述
                get_total_prd_qty();//顯示單的總完成數量
                txtgoods_desc.Focus();
            }
            txtBarCode.Focus();
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
            int kg_pcs_rate = clsProductionSchedule.get_kg_pcs_rate(cmbProductDept.SelectedValue.ToString().Trim(), cmbGoods_id.Text.Trim());
            return kg_pcs_rate;
        }
        private void fill_txt_kg_pcs()
        {
            if (txtkgPCS.Text == "" && cmbProductDept.SelectedValue.ToString().Trim() != "" && cmbGoods_id.Text != "")
            {
                txtkgPCS.Text = get_kg_pcs_rate().ToString();
                txtkgPCS.Text = (txtkgPCS.Text.ToString() != "0" ? txtkgPCS.Text : "");
            }
        }
        private void txtProductNo_Leave(object sender, EventArgs e)
        {
            if (txtProductNo.Text.Trim() != "")
            {
                txtProductNo.Text = txtProductNo.Text.PadLeft(10, '0');
            }
        }

        private void count_hour_std_qty()
        {
            if (!clsValidRule.IsNumeric(txtRow_qty.Text))
            {
                //MessageBox.Show("每行(碑)數輸入不正確!");
                lblMoStatus.Text = "每行(碑)數輸入不正確!";
                txtRow_qty.SelectAll();
                txtRow_qty.Focus();
                //btnSetFocus.Focus();
                return;
            }
            if (!clsValidRule.IsNumeric(txtPer_Convert_qty.Text))
            {
                lblMoStatus.Text="每小時轉(碑)數輸入不正確!";
                txtPer_Convert_qty.SelectAll();
                txtPer_Convert_qty.Focus();
                return;
            }
            if (txtRow_qty.Text != "" && txtPer_Convert_qty.Text != "")
            {
                if (cmbProductDept.SelectedValue.ToString().Trim() == "302" || (cmbProductDept.SelectedValue.ToString().Trim() == "202" && cmbGroup.SelectedValue.ToString() == "KB01"))
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
            if (!clsValidRule.IsNumeric(txtper_Standrad_qty.Text))
            {
                MessageBox.Show("每小時標準數輸入不正確!");
                txtper_Standrad_qty.SelectAll();
                txtper_Standrad_qty.Focus();
                return;
            }
            double hour_num = 0;
            string am_start_time = "08:30";
            string finish_work_noon1="12:30";//,finish_work_noon2="14:00";//中午下班時間 12:30~14:00
            string finish_work_afternoon1 = "18:00";//, finish_work_afternoon2 = "19:00";//下午下班時間 18:00~19:00
            string finish_work_time;
            dtpReqEnd.Text = "";
            if (txtReq_prd_qty.Text != "" && txtper_Standrad_qty.Text != "" && txtper_Standrad_qty.Text != "0" && dtpStart.Text != "00:00")
            {
                hour_num = Math.Round(Convert.ToSingle(txtReq_prd_qty.Text) / Convert.ToSingle(txtper_Standrad_qty.Text), 3);
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
        //由每小時標準數量及時間，計算出生產數量
        private void btnCount_qty_Click(object sender, EventArgs e)
        {
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            float normal_time, ot_time;
            int std_qty = (txtper_Standrad_qty.Text !=""?Convert.ToInt32(txtper_Standrad_qty.Text):0);
            normal_time = (txtNormal_work.Text.ToString() != "" ? Convert.ToSingle(txtNormal_work.Text) : 0);
            ot_time = (txtAdd_work.Text.ToString() != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
            txtPrd_qty.Text = Convert.ToInt32(((normal_time + ot_time) * std_qty)).ToString();
            if (cmbProductDept.SelectedValue.ToString().Trim() != "203")//不是203部門，自動計算生產重量
                count_prd_weg();
        }

        private void txtBarCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtBarCode.Text.Trim().Length > BarCodeMinLength)
                    doBarCode();
            }
        }
        private void doBarCode()
        {
            string barcode = "";
            if (txtBarCode.Text.Trim().Length > 13)
                barcode = txtBarCode.Text.Substring(0, 13);
            else
                barcode = txtBarCode.Text.Trim();

            DataTable dtBarCode = clsPublicOfPad.BarCodeToItem(barcode);
            txtBarCode.Text = "";
            if (dtBarCode.Rows.Count > 0)
            {
                string barcode_type = dtBarCode.Rows[0]["barcode_type"].ToString();
                if (barcode_type == "2")//從生產計劃中提取的條形碼
                {
                    txtFindMo.Text = dtBarCode.Rows[0]["mo_id"].ToString();
                    txtBarCodeItem.Text = dtBarCode.Rows[0]["goods_id"].ToString();
                    txtWipDep.Text = dtBarCode.Rows[0]["wp_id"].ToString();
                    cmbProductDept.SelectedValue = clsPublicOfPad.getDepJx("", txtWipDep.Text);
                    getJobType();//查找部門對應的工種
                    getWorkCodeList();//主要是提取噴油部門的排板標準數
                    SetControlVisible();//設置控件可見
                    getMoDataSource();//從生產表或排期表或流程中獲取記錄
                    fill_txt_kg_pcs();//更新每Kg對應數量
                    //loadProductWorker();//獲取該記錄號的生產工號
                    edit_type = "Y";//控件為編輯狀態
                    
                }
            }
            txtBarCode.Focus();
            //MessageBox.Show(barcode);

            //txtMatItem.Focus();
            //}
            //else
            //    return;


            //break;
        }
        private void getMoDataSource()
        {

            get_prd_records(10);//按條形碼查找制單狀態
            if (dtProductionRecordslist.Rows.Count > 0)
            {
                //FillGrid(); //將查詢到的記錄存入列表
                int rowNo = 0;
                if (dgvDetails.Rows.Count > 0)
                {
                    //定位到未完成的記錄
                    for (int i = 0; i < dtProductionRecordslist.Rows.Count; i++)
                    {
                        if (dtProductionRecordslist.Rows[i]["prd_end_time"].ToString() == "" || dtProductionRecordslist.Rows[i]["prd_end_time"].ToString() == "00:00")
                        {
                            rowNo = i;
                            break;
                        }
                    }


                    fill_textbox(rowNo);//填充各種控件
                }
                
            }
            else
            {
                ClearPartOfText();
                record_id = -1;
                string goods_item = "";
                //如果在生產的記錄中找不到記錄,則在安排的計劃中查找
                DataTable dtArrange = clsProductionSchedule.getDataFromArrangeByMo(cmbProductDept.SelectedValue.ToString(), txtFindMo.Text.Trim(), txtBarCodeItem.Text);
                //從流程中提取物料描述、原料描述
                DataTable dtItem = clsProductionSchedule.GetMo_dataById(txtFindMo.Text.Trim(), txtWipDep.Text.Trim(), txtBarCodeItem.Text);
                if (dtItem.Rows.Count == 0)
                {
                    MessageBox.Show("該物料的流程記錄不存在!");
                    txtmo_id.SelectAll();
                    return;
                }
                DataRow drItem = dtItem.Rows[0];
                
                if (dtArrange.Rows.Count > 0)
                {
                    //fillDataFromArrange(dtArrange);
                    
                    DataRow dr = dtArrange.Rows[0];
                    txtmo_id.Text = dr["prd_mo"].ToString();
                    txtPrd_qty.Text = dr["arrange_qty"].ToString();
                    txtProductNo.Text = dr["prd_worker"].ToString();
                    txtMachine.Text = dr["arrange_machine"].ToString();
                    mktPrdPdate.Text = dr["arrange_date"].ToString();
                    goods_item = dr["prd_item"].ToString();
                    
                }
                else
                {
                    ////如果在安排的計劃中找不到，則直接在計劃單中查找
                    //fillDataFromWip(dtItem);
                    txtmo_id.Text = drItem["mo_id"].ToString();
                    txtPrd_qty.Text = drItem["prod_qty"].ToString();
                    goods_item = drItem["goods_id"].ToString();
                }

                
                cmbGoods_id.Items.Clear();
                cmbGoods_id.Items.Add(goods_item);
                cmbGoods_id.Text = goods_item;//物料編號
                InitComBoxGroup();//初始化組別
                
                //SetControlVisible();//設置控件可見
                fillMachineVal();//機器標準工時等資料
                countReqPrdQty();//計算待生產數量
                txtgoods_desc.Text = drItem["goods_name"].ToString();
                txtToDep.Text = drItem["next_wp_id"].ToString();
                txtMatItem.Text = drItem["mat_item"].ToString();
                txtMatDesc.Text = drItem["mat_item_desc"].ToString();
                getDefaultWorkCode();//設置噴油部排板默認的板數標準
                lblMoStatus.Text = "新的記錄：未生產，請錄入完整相關資料!";
                if (chkAutoSave.Checked == true)
                    btnSave_Click(null, new EventArgs());
            }
            //getJobTimes();//噴油排板、噴油次數
            
        }
        private void loadProductWorker()
        {
            dtWorker = clsProductionSchedule.loadProductWorker(record_id);
            dgvWorker.DataSource = dtWorker;
        }
        private void countReqPrdQty()
        {
            //從流程中提取物料描述、原料描述
            int prd_qty = 0;
            DataTable dtItem = clsProductionSchedule.GetMo_dataById(txtmo_id.Text.Trim(), txtWipDep.Text.Trim(),cmbGoods_id.Text.Trim());
            if (dtItem.Rows.Count > 0)
                prd_qty = dtItem.Rows[0]["prod_qty"] != "" ? Convert.ToInt32(dtItem.Rows[0]["prod_qty"]) : 0;
            get_total_prd_qty();//獲取總完成數量
            txtReq_prd_qty.Text = (prd_qty - (txtTotalQty.Text != "" ? Convert.ToInt32(txtTotalQty.Text) : 0)).ToString();
        }
        //設置噴油部默認的排板標準數
        private void getDefaultWorkCode()
        {
            
            if (txtWipDep.Text.Trim() == "510")
            {
                string jobType = cmbJob_type.SelectedValue != null ? cmbJob_type.SelectedValue.ToString().Trim() : "";
                if (jobType == "J0702" || jobType == "J0703")
                {
                    string sizeId1 = "", sizeId2 = "";
                    sizeId1 = cmbGoods_id.Text.Trim().Length >= 18 ? cmbGoods_id.Text.Trim().Substring(11, 3) : "";
                    sizeId2 = sizeId1;
                    if (string.Compare(sizeId2, "065") < 0)
                        sizeId1 = "";
                    cmbWorkCode.SelectedValue = clsProductionSchedule.getDefaultWorkCode(txtWipDep.Text.Trim(), sizeId1, sizeId2);

                    getWorkCodeStd();
                    countWorkCodeQty();
                }
            }
            else
            {
                cmbWorkCode.SelectedValue = "";
                txtRow_qty.Text = "";
                txtPrd_Run_qty.Text = "";
            }
        }
        private void get_data_details()
        {
            ClearPartOfText(); //清空文本框內容
            fill_plan_value();//首先將計劃單帶出數量、描述
            get_prd_records(1);//查詢已錄入的記錄
            //FillGrid();//將查詢到的記錄存入列表
            fill_txt_kg_pcs();//提取物料每公斤對應數量
            if (cmbProductDept.SelectedValue.ToString().Trim() != "203")//不是203部門，自動計算生產重量
                count_prd_weg();
            else
                count_prd_qty();//是203部門，自動計算生產數量
            get_total_prd_qty();//獲取總完成數量
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


        private void Cout_prd_qty_alloy()
        {
            if (!clsValidRule.IsNumeric(txtRow_qty.Text))
            {
                //MessageBox.Show("每行(碑)數輸入不正確!");
                txtRow_qty.SelectAll();
                //txtRow_qty.Focus();
                return;
            }
            if (!clsValidRule.IsNumeric(txtPrd_Run_qty.Text))
            {
                MessageBox.Show("實際碑數輸入不正確!");
                txtPrd_Run_qty.SelectAll();
                txtPrd_Run_qty.Focus();
                return;
            }
            if (txtRow_qty.Text != "" && txtPrd_Run_qty.Text != "")
            {
                txtPrd_qty.Text = (Convert.ToInt32(txtRow_qty.Text) * Convert.ToInt32(txtPrd_Run_qty.Text)).ToString();
                count_prd_weg();//不是203部門，自動計算生產重量
            }
        }

        private void count_alloy_std_hour_qty()//計算合金部的每小時標準碑數
        {
            if (!clsValidRule.IsNumeric(txtNormal_work.Text))
            {
                MessageBox.Show("正常班工時輸入不正確!");
                txtNormal_work.SelectAll();
                txtNormal_work.Focus();
                return;
            }
            if (!clsValidRule.IsNumeric(txtAdd_work.Text))
            {
                MessageBox.Show("加班工時輸入不正確!");
                txtAdd_work.SelectAll();
                txtAdd_work.Focus();
                return;
            }
            if (cmbProductDept.SelectedValue.ToString().Trim() == "302" && (txtNormal_work.Text != "" || txtAdd_work.Text != ""))
            {
                float normal_work, add_work, Prd_Run_qty;
                normal_work = (txtNormal_work.Text != "" ? Convert.ToSingle(txtNormal_work.Text) : 0);
                add_work = (txtAdd_work.Text != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
                Prd_Run_qty = (txtPrd_Run_qty.Text != "" ? Convert.ToSingle(txtPrd_Run_qty.Text) : 0);
                if (normal_work + add_work != 0)
                {
                    txtper_Standrad_qty.Text = Math.Round(Prd_Run_qty / (normal_work + add_work), 0).ToString();
                    count_req_time();//預計完成時間
                }
            }
        }
        private void cmbProductDept_Leave(object sender, EventArgs e)
        {
            ClearAllText();
            getWipDep();//獲取對應的Wip部門
            InitComBoxGroup();//初始化組別
            SetControlVisible();//設置控件可見
            getWorkCodeList();//主要是提取噴油部門的排板標準數
        }
        private void SetControlVisible()
        {
            bool t1, t2;
            t1 = true;
            t2 = true;
            string prdDep=cmbProductDept.SelectedValue.ToString().Trim();
            if (prdDep == "J07")
            {
                t1 = false;
                t2 = true;
                lblRow_qty.Location = new Point(560, 11);
                txtRow_qty.Location = new Point(636, 6);
                lblPrd_Run_qty.Location = new Point(16, 44);
                txtPrd_Run_qty.Location = new Point(92, 37);
                lblDifficulty_level.Location = new Point(320, 44);
                cmbDifficultyLevel.Location = new Point(366, 37);
                lblActual_qty.Location = new Point(592, 44);
                txtActual_qty.Location = new Point(636, 37);
                lblWork_code.Text = "選板規格:";
                lblRow_qty.Text = "每板粒數:";
                lblPrd_Run_qty.Text = "實際板數:";
                lblDifficulty_level.Text = "次數:";
                lblActual_qty.Text = "尾數:";
            }
            else
            {
                t1 = true;
                t2 = false;
                lblRow_qty.Location = new Point(7, 44);
                txtRow_qty.Location = new Point(92, 37);
                lblPrd_Run_qty.Location = new Point(560, 76);
                txtPrd_Run_qty.Location = new Point(636, 69);
                lblDifficulty_level.Location = new Point(592, 11);
                cmbDifficultyLevel.Location = new Point(636, 6);
                lblActual_qty.Location = new Point(820, 11);
                txtActual_qty.Location = new Point(875, 6);
                lblWork_code.Text = "標準編碼:";
                lblRow_qty.Text = "每行(碑)數:";
                lblPrd_Run_qty.Text = "實際碑數:";
                lblDifficulty_level.Text = "難度";
                lblActual_qty.Text = "實際數量:";
            }
            //lblDifficulty_level.Visible = t1;
            txtDifficulty_level.Visible = t1;
            cmbDifficultyLevel.Visible = t2;
            lblPer_hour_qty.Visible = t1;
            txtPer_Convert_qty.Visible = t1;
            lblStandard_per_qty.Visible = t1;
            txtper_Standrad_qty.Visible = t1;
            lblWork_class.Visible = t1;
            lblEnd_run.Visible = t1;
            txtEnd_run.Visible = t1;
            lblStart_run.Visible = t1;
            txtStart_run.Visible = t1;
            txtWork_code.Visible = t1;
            cmbWorkCode.Visible = t2;
            lblActual_qty.Visible = t2;
            txtActual_qty.Visible = t2;
            panel12.Visible = t1;
            if (prdDep=="102")
            {
                t1 = true;
                t2 = false;

                //lblStart_run.Location = new Point(5, 176);//開始碑數
                //txtStart_run.Location = new Point(59, 152);//開始碑數
                //lblEnd_run.Location = new Point(372, 176);//結束碑數
                //txtEnd_run.Location = new Point(432, 152);//結束碑數
                //lblPrd_Run_qty.Location = new Point(5, 249);//實際碑數
                //txtPrd_Run_qty.Location = new Point(59, 225);//實際碑數
                //lblWork_code.Location = new Point(5, 373);//標準編碼
                //txtWork_code.Location = new Point(59, 364);//標準編碼
                //lblDifficulty_level.Location = new Point(5, 318);
                //txtDifficulty_level.Location = new Point(59, 294);
                lblStart_run.Visible = t1;//開始碑數
                txtStart_run.Visible = t1;//開始碑數
                lblEnd_run.Visible = t1;//結束碑數
                txtEnd_run.Visible = t1;//結束碑數
                lblPrd_Run_qty.Visible = t1;//實際碑數
                txtPrd_Run_qty.Visible = t1;//實際碑數
                txtWork_code.Visible = t1;//標準編碼
                lblWork_code.Visible = t1;//標準編碼
                lblSpeed_lever.Visible = t2;//檔位
                txtSpeed_lever.Visible = t2;//檔位
                //lblJob_type.Visible = t2;//工种
                //cmbJob_type.Visible = t2;//工种
                txtWork_class.Visible = t2;//類別
                lblWork_class.Visible = t2;//類別
                if (cmbProductDept.SelectedValue.ToString().Trim() == "302")
                    lblStandard_per_qty.Text = "標準時產能";
                else
                    lblStandard_per_qty.Text = "每小時標準數";
            }
        }

        private void count_run_qty()//計算實際碑數  合金部使用
        {
            if (cmbProductDept.SelectedValue.ToString().Trim() == "302")//當是在編輯狀態且302部門時
            {
                if (!clsValidRule.IsNumeric(txtStart_run.Text))
                {
                    MessageBox.Show("結束碑數輸入不正確!");
                    txtStart_run.SelectAll();
                    txtStart_run.Focus();
                    return;
                }
                if (!clsValidRule.IsNumeric(txtEnd_run.Text))
                {
                    MessageBox.Show("開始碑數輸入不正確!");
                    txtEnd_run.SelectAll();
                    txtEnd_run.Focus();
                    return;
                }
                txtPrd_Run_qty.Text = "";
                int run_qty=0;
                
                if (txtStart_run.Text != "" && txtEnd_run.Text != "")
                {
                    run_qty=Convert.ToInt32(txtEnd_run.Text) - Convert.ToInt32(txtStart_run.Text);
                    run_qty = (run_qty > 0 ? run_qty : 0);
                }
                txtPrd_Run_qty.Text = run_qty.ToString();
                count_alloy_std_hour_qty();
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


        private void dtpEnd_MouseDown(object sender, MouseEventArgs e)
        {
            if (dtpEnd.Text == "00:00" &&dtpStart.Text != "00:00")
                dtpEnd.Value = System.DateTime.Now;
        }

        private void txtMachine_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtMachine.Text == "")
            {
                string prdDep = cmbProductDept.SelectedValue.ToString().Trim();
                if (prdDep == "J03")
                {
                    txtMachine.Text = "LNN-";
                    if (_userid == "BUT01")//萬能機
                        txtMachine.Text = "LNO-";
                    else
                    {
                        if (_userid == "BUT02")//雞眼
                            txtMachine.Text = "LNN-";
                    }
                }
                else if (prdDep == "J01")//合金
                    txtMachine.Text = "N-0";
                else if (prdDep == "203")//扣部--裝嵌
                    txtMachine.Text = "K-I-S-";
                else if (prdDep == "105" || prdDep == "125" || prdDep == "J05")
                    txtMachine.Text = "LNN-";
                else if (prdDep == "104" || prdDep == "124")
                    txtMachine.Text = "LNO-";
                else if (prdDep == "202")
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
                txtMachine.SelectionStart = txtMachine.Text.Length;
            }
        }

        private void dtpStart_MouseDown(object sender, MouseEventArgs e)
        {
            if (dtpStart.Text == "00:00")
                dtpStart.Value = System.DateTime.Now;
        }




        private void txtMatLot_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtMatLot.Text.Trim() == "")
            {
                txtMatLot.Text = "HWH";
                txtMatLot.SelectionStart = txtMatLot.Text.Length;
            }
        }

        private void txtNormal_work_Leave(object sender, EventArgs e)
        {
            count_alloy_std_hour_qty();
        }

        private void txtAdd_work_Leave(object sender, EventArgs e)
        {
            count_alloy_std_hour_qty();
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
                        " Inner Join product_records_dist_mo b on a.prd_id_ref=b.prd_id_ref " +
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
            //FillGrid();

        }
        //計算每KG對應數量
        private void convert_kg_pcs()
        {
            txtkgPCS.Text = "";
            if (txtSample_no.Text != "" && txtSample_weg.Text != "" && Convert.ToSingle(txtSample_weg.Text) != 0)
                txtkgPCS.Text = Math.Round(Convert.ToInt32(txtSample_no.Text) / (Convert.ToSingle(txtSample_weg.Text) / 1000), 0).ToString();
            //計算生產數量或重量
            if (cmbProductDept.SelectedValue.ToString().Trim() != "203")
                count_prd_weg();
            else
                count_prd_qty();
        }

        //計算生產數量
        private void count_prd_qty()
        {
            if (!clsValidRule.IsNumeric(txtprd_weg.Text))
            {
                MessageBox.Show("生產重量輸入不正確!");
                txtprd_weg.SelectAll();
                txtprd_weg.Focus();
                return;
            }
            if (!clsValidRule.IsNumeric(txtkgPCS.Text))
            {
                MessageBox.Show("每Kg個數輸入不正確!");
                txtkgPCS.SelectAll();
                txtkgPCS.Focus();
                return;
            }
            txtPrd_qty.Text = "";
            if (txtprd_weg.Text != "" && txtkgPCS.Text != "")
                txtPrd_qty.Text = Math.Round((Convert.ToSingle(txtprd_weg.Text) * Convert.ToSingle(txtkgPCS.Text)), 0).ToString();
        }
        //計算生產重量
        private void count_prd_weg()
        {
            if (!clsValidRule.IsNumeric(txtPrd_qty.Text))
            {
                MessageBox.Show("生產數量輸入不正確!");
                txtPrd_qty.SelectAll();
                txtPrd_qty.Focus();
                return;
            }
            if (!clsValidRule.IsNumeric(txtkgPCS.Text))
            {
                MessageBox.Show("每Kg個數輸入不正確!");
                txtkgPCS.SelectAll();
                txtkgPCS.Focus();
                return;
            }
            txtprd_weg.Text = "";
            if (txtPrd_qty.Text != "" && txtkgPCS.Text != "" && txtkgPCS.Text != "0")
                txtprd_weg.Text = Math.Round((Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtkgPCS.Text)), 2).ToString();
        }
        private void txtkgPCS_Leave(object sender, EventArgs e)
        {
            if (cmbProductDept.SelectedValue.ToString().Trim() != "203")//不是203部門，自動計算生產重量
                count_prd_weg();
            else
                count_prd_qty();//是203部門，自動計算生產數量
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
            if (txtMatItem.Text != "")
                GetMat_Desc();
        }


        private void txtSpeed_lever_Leave(object sender, EventArgs e)
        {
            if (txtMachine.Text.Trim() != "" && cmbProductDept.SelectedValue.ToString().Trim() == "202" && txtSpeed_lever.Text.Trim() !="")
            {
                GetMachine_std();//獲取機器的各項標準數據
                fill_textbox_machine_std();//填充機器各項標準
            }
        }


        private void txtDifficulty_level_Leave(object sender, EventArgs e)
        {
            //203選貨的標準暫未改變
            if (txtDifficulty_level.Text.Trim() != "" && cmbProductDept.SelectedValue.ToString().Trim() == "203" && cmbGroup.Text.Trim() != "KD04")
            {
                GetMachine_std();//獲取機器的各項標準數據
                fill_textbox_machine_std();//填充機器各項標準
            }
        }


        private void cmbGroup_Leave(object sender, EventArgs e)
        {
            string Group = "";
            Group = cmbGroup.Text.Trim();
            if (cmbProductDept.SelectedValue.ToString().Trim() == "105" && Group == "BC05")
                cmbWorkType.Text = "選貨";
            else if (cmbProductDept.SelectedValue.ToString().Trim() == "202")
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
            count_datetime();//計算生產時間
            count_alloy_std_hour_qty();//計算合金部的每小時標準碑數
        }

        private void txtprd_weg_Leave(object sender, EventArgs e)
        {
            if (cmbProductDept.SelectedValue.ToString().Trim() == "203")//是203部門，自動計算生產數量
                count_prd_qty();
        }

        private void dgvDetails_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            fill_textbox(e.RowIndex);//填充各種控件
        }

        private void dgvDetails_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            edit_type = "Y";
        }
        //由生產數量同重量計算出每公斤的數量
        private void btnCountKgPcs_Click(object sender, EventArgs e)
        {
            if (txtPrd_qty.Text != "" && txtprd_weg.Text != "" && txtprd_weg.Text != "0")
                txtkgPCS.Text = Convert.ToInt32(Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtprd_weg.Text)).ToString();
        }

        

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            txtBarCode.Focus();
            if (!valid_data())
                return;
            product_records objModel = new product_records();
            string prdDep = cmbProductDept.SelectedValue.ToString();
            objModel.prd_dep = prdDep;
            objModel.prd_date = dteProdcutDate.Text.ToString();
            objModel.prd_pdate = (mktPrdPdate.Text.Replace(" ", "") == "//" ? dteProdcutDate.Text.ToString() : mktPrdPdate.Text.ToString());
            objModel.prd_mo = txtmo_id.Text.Trim();
            objModel.prd_item = cmbGoods_id.Text.ToString().Trim();
            objModel.prd_qty = (txtPrd_qty.Text != "" ? Convert.ToInt32(txtPrd_qty.Text) : 0);
            objModel.prd_weg = (txtprd_weg.Text != "" ? Convert.ToSingle(txtprd_weg.Text) : 0);
            objModel.req_prd_qty = (txtReq_prd_qty.Text != "" ? Convert.ToInt32(txtReq_prd_qty.Text) : 0);
            objModel.prd_machine = txtMachine.Text.Trim();
            objModel.prd_work_type = cmbWorkType.SelectedValue.ToString();
            objModel.prd_worker = txtProductNo.Text.Trim();
            objModel.prd_class = cmbOrder_class.Text.Trim();
            objModel.prd_group = cmbGroup.Text.Trim();
            objModel.prd_start_time = (dtpStart.Text.Trim() != "00:00" ? dtpStart.Text.Trim() : "");
            objModel.prd_end_time = (dtpEnd.Text.Trim() != "00:00" ? dtpEnd.Text.Trim() : "");
            if (objModel.prd_start_time == "" && objModel.prd_start_time == "")//如果沒有輸入開始、結束時間，則標識為排單操作
                objModel.arrange_flag = "Y";
            else
                objModel.arrange_flag = "";
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
            
            objModel.pack_num = (txtPack_num.Text != "" ? Convert.ToInt32(txtPack_num.Text) : 1);
            if (prdDep == "J07")
            {
                objModel.work_code = cmbWorkCode.SelectedValue == null ? "" : cmbWorkCode.SelectedValue.ToString();
                objModel.difficulty_level = cmbDifficultyLevel.SelectedValue == null ? "" : cmbDifficultyLevel.SelectedValue.ToString();
            }
            else
            {
                objModel.work_code = txtWork_code.Text;
                objModel.difficulty_level = txtDifficulty_level.Text.Trim();
            }
            objModel.job_type = (cmbJob_type.SelectedValue != null ? cmbJob_type.SelectedValue.ToString() : "");
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
            objModel.actual_qty = (txtActual_qty.Text != "" ? Convert.ToInt32(txtActual_qty.Text) : 0);
            objModel.actual_weg = 0;
            objModel.conf_flag = "";
            objModel.conf_time = Convert.ToDateTime("1900/01/01");
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
            saveProductWorker();//更新工號
            OperationType = clsUtility.enumOperationType.Save;
            ToolStripButtonEvents();
            txtBarCode.Focus();
            //txtBarCode.Focus();
            //txtBarCode.Text = "";
        }
        //儲存後更新工號的記錄表
        private void saveProductWorker()
        {
            List<mdlProductWorker> lsProductWorker =new List<mdlProductWorker>();
            if (txtProductNo.Text != "")
            {
                if (checkExistWorker() == false)
                {
                    mdlProductWorker objProductWorker = new mdlProductWorker();
                    objProductWorker.prdId = record_id;
                    objProductWorker.prdWorker = txtProductNo.Text.Trim();
                    objProductWorker.crusr = _userid;
                    objProductWorker.crtim = clsUtility.changeDateTimeToChar(System.DateTime.Now);
                    lsProductWorker.Add(objProductWorker);
                }
            }
            for (int i = 0; i < dgvWorker.Rows.Count; i++)
            {
                mdlProductWorker objProductWorker = new mdlProductWorker();
                objProductWorker.prdId = record_id;
                objProductWorker.prdWorker = dgvWorker.Rows[i].Cells["colPrdWorker"].Value.ToString();
                objProductWorker.crusr = _userid;
                objProductWorker.crtim = clsUtility.changeDateTimeToChar(System.DateTime.Now);
                lsProductWorker.Add(objProductWorker);
            }
            if (lsProductWorker.Count > 0)
            {
                int result = 0;
                result = clsProductionSchedule.updateProductWorker(lsProductWorker, record_id);
            }
        }

        private void btnReDo_Click(object sender, EventArgs e)
        {
            if (record_id == -1)
            {
                MessageBox.Show("原單記錄不存在!");
                return;
            }
            txtPrd_id_ref.Text = record_id.ToString();
            record_id = -1;//重新設定為新單狀態
            edit_type = "Y";
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            txtReq_prd_qty.Text = "";
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
            cmbWorkType.SelectedValue = "A02";
            //txtFindMo.Text = txtmo_id.Text;
            //txtWipDep.Text = cmbProductDept.Text;
            //txtBarCodeItem.Text = cmbGoods_id.Text;
            setProdDate();//自動設定生產日期為當前日期
            get_last_run_qty();//獲取最後一次的碑數
            //get_total_prd_qty();//顯示單的總完成數量
            countReqPrdQty();//計算待生產數量
            if (cmbProductDept.SelectedValue.ToString().Trim() == "302")//302部門的，要將標準時能清空
                txtper_Standrad_qty.Text = "";
            //dgvWorker.DataSource = null;
            loadProductWorker();
            getDefaultWorkCode();//設置噴油部排板默認的板數標準
            lblMoStatus.Text = "繼續生產中，請輸入完整資料!";
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            ////以下2019/10/24日取消
            //if (chk_imput_status() == true)//檢查記錄是否已傳入新系統
            //    return;
            //OperationType = clsUtility.enumOperationType.Delete;
            //ToolStripButtonEvents();

            if (record_id > 0)
            {
                if (MessageBox.Show("確定要刪除嗎?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //int prd_id = record_id;
                    int re = clsProductionSchedule.DeleteProductionRecords(record_id);
                    if (re > 0)
                    {
                        MessageBox.Show("刪除成功!");
                        record_id = -1;
                        get_prd_records(1);//查詢已錄入的記錄
                        ClearAllText();
                        //FillGrid(); //將查詢到的記錄存入列表

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


        private void btnFindMo_Click(object sender, EventArgs e)
        {
            if (cmbProductDept.SelectedValue == null || cmbProductDept.SelectedValue.ToString() == "")
            {
                MessageBox.Show("請輸入查詢的部門!");
                return;
            }
            int selindex = 0;
            selindex = cmbSearch.SelectedIndex;
            switch (selindex)
            {
                case 0://制單編號--可部分輸入
                    if (txtFindMo.Text.Trim() != "")
                    {
                        txtBarCodeItem.Text = "";
                        get_prd_records(11);
                        //FillGrid(); //將查詢到的記錄存入列表
                        fill_textbox(0);//填充各種控件

                    }
                    break;
                case 1://機器--最近一次的生產
                    if (txtFindMo.Text.Trim() != "")
                        get_prd_records(8);//按機器編號查詢最後一筆記錄
                    break;
                case 2://當日未完成的制單
                    get_prd_records(2);
                    if (dgvDetails.Rows.Count > 0)
                        fill_textbox(0);//填充各種控件
                    break;
                case 3://當天完成的記錄
                    get_prd_records(5);
                    if (dgvDetails.Rows.Count > 0)
                        fill_textbox(0);//填充各種控件
                    break;
                case 4:
                    if (txtFindMo.Text.Trim() == "")
                        return;
                    txtFindMo.Text = txtFindMo.Text.PadLeft(10, '0');
                    get_prd_records(4);
                    if (dgvDetails.Rows.Count > 0)
                        fill_textbox(0);//填充各種控件
                    break;
            }

        }



        private void txtmo_id_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (txtProductNo.Text.Trim() != "")
                {
                    txtProductNo.Text = txtProductNo.Text.PadLeft(10, '0');
                    btnAddWorker_Click(sender, e);
                }
                SendKeys.Send("{tab}");
            }
        }

        private void tabControl1_Click(object sender, EventArgs e)
        {
            txtBarCode.Focus();
        }

        private void txtmo_id_KeyDown(object sender, KeyEventArgs e)
        {
            //clsUtility.Call_imput();
        }

        private void txtSample_no_Leave(object sender, EventArgs e)
        {
            convert_kg_pcs();
        }

        private void txtSample_weg_Leave(object sender, EventArgs e)
        {
            convert_kg_pcs();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (btnBrowse.Text == "瀏覽(&B)")
            {
                btnBrowse.Text = "編輯(&B)";
                tabControl1.SelectedIndex = 1;
                cmbSearch.SelectedIndex = 0;
                edit_type = "N";
            }
            else
            {
                btnBrowse.Text = "瀏覽(&B)";
                tabControl1.SelectedIndex = 0;
                edit_type = "Y";
            }
            txtBarCode.Focus();
        }

        private void cmbSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSearch.SelectedIndex == 1)
            {
                txtFindMo.SelectionStart = txtFindMo.Text.Length;
                txtFindMo.Focus();
            }
            else
            {
                txtFindMo.Text = "";
                if (cmbSearch.SelectedIndex > 1 && cmbSearch.SelectedIndex<4)
                {
                    btnFindMo_Click(sender, e);
                }
                txtBarCode.Focus();
            }
        }


        private void cmbGoods_id_Leave(object sender, EventArgs e)
        {
            txtFindMo.Text = txtmo_id.Text;
            txtBarCodeItem.Text = cmbGoods_id.Text;
            
            getMoDataSource();//從生產表或排期表或流程中獲取記錄

        }



        private void txtPer_Convert_qty_Leave(object sender, EventArgs e)
        {
            if (cmbProductDept.SelectedValue.ToString().Trim() != "302")
                count_hour_std_qty();
        }

        private void txtper_Standrad_qty_Leave(object sender, EventArgs e)
        {
            count_req_time();//預計完成時間
            //if (edit_type == "Y" && cmbProductDept.SelectedValue.ToString().Trim() != "302")
            //{

            //}
        }


        private void objSelectAll(string txt)
        {
            
            TextBox obj = new TextBox();
            foreach (Control c in panel8.Controls)
            {
                if (c is TextBox)
                {
                    TextBox tb = (TextBox)c;
                    if (tb.Name == txt)
                    {
                        
                        tb.SelectAll();
                        return;
                    }
                }
            }

        }

        private void txtPrd_qty_MouseDown(object sender, MouseEventArgs e)
        {
            txtPrd_qty.SelectAll();
        }

        private void txtProductNo_MouseDown(object sender, MouseEventArgs e)
        {
            txtProductNo.SelectAll();
        }

        private void txtNormal_work_MouseDown(object sender, MouseEventArgs e)
        {
            txtNormal_work.SelectAll();
        }

        private void txtAdd_work_MouseDown(object sender, MouseEventArgs e)
        {
            txtAdd_work.SelectAll();
        }

        private void txtprd_weg_MouseDown(object sender, MouseEventArgs e)
        {
            txtprd_weg.SelectAll();
        }

        private void txtPack_num_MouseDown(object sender, MouseEventArgs e)
        {
            txtPack_num.SelectAll();
        }

        private void txtStart_run_MouseDown(object sender, MouseEventArgs e)
        {
            txtStart_run.SelectAll();
        }

        private void txtEnd_run_MouseDown(object sender, MouseEventArgs e)
        {
            txtEnd_run.SelectAll();
        }

        private void txtPrd_Run_qty_MouseDown(object sender, MouseEventArgs e)
        {
            txtPrd_Run_qty.SelectAll();
        }

        private void txtDifficulty_level_MouseDown(object sender, MouseEventArgs e)
        {
            txtDifficulty_level.SelectAll();
        }

        private void txtWork_code_MouseDown(object sender, MouseEventArgs e)
        {
            txtWork_code.SelectAll();
        }

        private void txtWork_class_MouseDown(object sender, MouseEventArgs e)
        {
            txtWork_class.SelectAll();
        }

        private void txtSpeed_lever_MouseClick(object sender, MouseEventArgs e)
        {
            txtSpeed_lever.SelectAll();
        }

        private void cmbWorkType_Leave(object sender, EventArgs e)
        {
            string work_type="";
            work_type=cmbWorkType.SelectedValue.ToString().Trim();
            if (work_type == "A01" || work_type == "A04" || work_type == "A05" || work_type == "A06" || work_type == "A07" || work_type == "A10")
                txtPrd_qty.Text = "0";
        }

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                count_req_time();//預計完成時間
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

        private void txtPrd_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                if (!clsValidRule.IsNumeric(txtPrd_qty.Text))
                {
                    MessageBox.Show("生產數量輸入不正確!");
                    txtPrd_qty.SelectAll();
                    txtPrd_qty.Focus();
                    return;
                }
                //count_req_time();//預計完成時間  //2019/08/21日取消，預計時間改為由待生產數計算
                if (cmbProductDept.SelectedValue.ToString().Trim() != "203")//不是203部門，自動計算生產重量
                    count_prd_weg();
            }
        }

        private void btnImput_Click(object sender, EventArgs e)
        {
            clsUtility.StartImput();
        }

        private void txtRow_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                if (cmbProductDept.SelectedValue.ToString().Trim() == "302")
                    Cout_prd_qty_alloy();//計算實際生產數量(合金部)

                else
                {
                    if (txtWipDep.Text.Trim() == "510")
                        countWorkCodeQty();
                    else
                        count_hour_std_qty();//計算標準數量
                }
            }
        }

        private void txtPrd_Run_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")//合金部 生產數 = 每碑數 * 實際碑數
            {
                if (txtWipDep.Text.Trim() == "501")
                    countWorkCodeQty();
                else
                {
                    Cout_prd_qty_alloy();
                    count_alloy_std_hour_qty();//計算合金部的每小時標準碑數
                }
            }
        }


        private void txtStart_run_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")//合金部 生產數 = 每碑數 * 實際碑數
            {
                count_run_qty();//計算實際碑數  合金部使用
            }
        }

        private void txtEnd_run_TextChanged_1(object sender, EventArgs e)
        {
            if (edit_type == "Y")//合金部 生產數 = 每碑數 * 實際碑數
            {
                count_run_qty();//計算實際碑數  合金部使用
            }
        }

        private void frmPrdSchedule_FormClosed(object sender, FormClosedEventArgs e)
        {
            BarCode.Stop();
        }

        private void txtBarCode_TextChanged(object sender, EventArgs e)
        {
            //if (txtBarCode.Text.Trim().Length >= 13)
            //    doBarCode();
        }

        private void txtReq_prd_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                if (!clsValidRule.IsNumeric(txtReq_prd_qty.Text))
                {
                    MessageBox.Show("待生產數量輸入不正確!");
                    txtReq_prd_qty.SelectAll();
                    txtReq_prd_qty.Focus();
                    return;
                }
                count_req_time();//預計完成時間
                //if (cmbProductDept.SelectedValue.ToString().Trim() != "203")//不是203部門，自動計算生產重量
                //    count_prd_weg();
            }
        }

        private void cmbWorkCode_Leave(object sender, EventArgs e)
        {
            getWorkCodeStd();
            countWorkCodeQty();
        }
        private void countWorkCodeQty()
        {
            if (txtRow_qty.Text != "" && txtPrd_Run_qty.Text != "")
                txtPrd_qty.Text = (Convert.ToInt32(txtRow_qty.Text) * Convert.ToInt32(txtPrd_Run_qty.Text)).ToString();
        }

        private void btnAddWorker_Click(object sender, EventArgs e)
        {
            if (checkExistWorker() == true)
                return;
            string workerName = getWorkerName();
            if (workerName=="")
            {
                MessageBox.Show("工號不存在!");
                return;
            }
            //3、添加数据行
            DataRow dr2 = dtWorker.NewRow();
            dr2["prd_worker"] = txtProductNo.Text; //通过索引赋值
            dr2["workerName"] = workerName;
            dtWorker.Rows.Add(dr2);
            txtProductNo.Text = "";
        }
        private bool checkExistWorker()
        {
            bool result = false;
            string prdWorker = txtProductNo.Text.Trim();
            for (int i = 0; i < dgvWorker.Rows.Count; i++)
            {
                if (prdWorker == dgvWorker.Rows[i].Cells["colPrdWorker"].Value.ToString())
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        private string getWorkerName()
        {
            string result = "";
            string strSql = "Select hrm1name From lnfs1.hr_db.dbo.hrm01 Where hrm1wid='" + txtProductNo.Text + "'";
            DataTable dt = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            if (dt.Rows.Count > 0)
                result = dt.Rows[0]["hrm1name"].ToString();
            return result;
        }

        private void btnDeleteWorker_Click(object sender, EventArgs e)
        {
            if (dgvWorker.Rows.Count > 0)
            {
                int i = dgvWorker.CurrentRow.Index;
                dtWorker.Rows.RemoveAt(i);
            }
        }

        private void cmbJob_type_Leave(object sender, EventArgs e)
        {
            getJobTimes();//排板、噴油次數
            getDefaultWorkCode();
        }
    }
}
