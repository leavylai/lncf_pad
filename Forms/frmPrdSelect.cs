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
    public partial class frmPrdSelect : Form
    {
        BardCodeHooK BarCode = new BardCodeHooK();
        DataTable dtPrd_dept = new DataTable();
        DataTable dtMo_item = new DataTable();
        DataTable dtWork_type = new DataTable();
        DataTable dtMachine_std = new DataTable();
        DataTable dtProductionRecordslist = new DataTable();
        DataTable dtWorker = new DataTable();
        DataTable dtDgvDefective = new DataTable();
        private string edit_type="Y";//控制當控件中當值發生變化時的操作
        private clsUtility.enumOperationType OperationType;
        private int Result = 0;
        private string _userid = DBUtility._user_id.ToUpper();//"PLA01";
        private product_records objModel;
        private int record_id = -1;//未完成記錄的ID，若查找到，則說明未完成，在保存時，執行更新操作
        public static string sent_dep = "";
        public static string sent_group = "";
        private int BarCodeMinLength = 10;
        private string remote_db = DBUtility.remote_db;
        public frmPrdSelect()
        {
            InitializeComponent();

            //clsControlInfoHelper controlInfo = new clsControlInfoHelper("frmProductionSchedule", this.Controls);
            //controlInfo.GenerateContorl();
            BarCode.BarCodeEvent += new BardCodeHooK.BardCodeDeletegate(BarCode_BarCodeEvent);
            GetAllComboxData();
        }

        private delegate void ShowInfoDelegate(BardCodeHooK.BarCodes barCode);

        private void ShowInfo(BardCodeHooK.BarCodes barCode)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowInfoDelegate(ShowInfo), new object[] { barCode });
            }
            else
            {
                //textBox1.Text = barCode.KeyName;
                //textBox2.Text = barCode.VirtKey.ToString();
                //textBox3.Text = barCode.ScanCode.ToString();
                //textBox4.Text = barCode.Ascll.ToString();
                //textBox5.Text = barCode.Chr.ToString();
                string strBarCode = "";

                strBarCode = barCode.IsValid ? barCode.BarCode : "";//是否为扫描枪输入，如果为true则是 否则为键盘输入
                
                strBarCode = strBarCode.Replace("\r\n", "").Replace("'", "").Replace("\0", "").Replace("\r", "");
                //textBox7.Text += barCode.KeyName;
                if (strBarCode.Length > BarCodeMinLength)
                {
                    txtBarCode.Text = strBarCode.Trim().ToUpper();

                    //MessageBox.Show(strBarCode);
                    doBarCode();
                    txtBarCode.Text = "";
                    //MessageBox.Show(barCode.IsValid.ToString());
                }
                txtBarCode.Focus();
            }
        }

        private void doBarCode()
        {
            //掃描制單編號，物料編號
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
                string goods_id = "";
                if (barcode_type == "2")//從生產計劃中提取的條形碼
                {
                    goods_id = dtBarCode.Rows[0]["goods_id"].ToString();
                    txtmo_id.Text = dtBarCode.Rows[0]["mo_id"].ToString();
                    cmbProductDept.Text = dtBarCode.Rows[0]["wp_id"].ToString();
                    cmbOwnDep.Text = cmbProductDept.Text;
                    InitComBoxGroup();
                }
                else
                {
                    string doc = dtBarCode.Rows[0]["doc_id"].ToString();
                    string seq = dtBarCode.Rows[0]["doc_seq"].ToString();
                    string strSql = "";
                    if (barcode_type == "11")
                    {
                        strSql = "Select a.in_dept,b.mo_id,b.goods_id From "+remote_db+"jo_materiel_con_mostly a" +
                            " Inner Join "+remote_db+"jo_materiel_con_details b ON a.within_code=b.within_code  AND a.id=b.id" +
                            " Where b.within_code='0000' AND b.id='" + doc + "' AND b.sequence_id='" + seq + "'";
                    }
                    else
                    {
                        strSql = "Select a.inventory_receipt AS in_dep,a.mo_id,a.goods_id From "+remote_db+"st_i_subordination a" +
                            " Where a.within_code='0000' AND a.id='" + doc + "' AND a.sequence_id='" + seq + "'";
                    }
                    DataTable dtTranMo = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                    if (dtTranMo.Rows.Count > 0)
                    {
                        goods_id = dtTranMo.Rows[0]["goods_id"].ToString();
                        txtmo_id.Text = dtTranMo.Rows[0]["mo_id"].ToString();
                    }
                }
                //cmbProductDept.Text = dtBarCode.Rows[0]["wp_id"].ToString();
                GetMo_itme(goods_id);
                cmbGoods_id.Text = goods_id;
                get_data_details();
                //edit_type = "Y";//轉為可編輯狀態
            }
        }

        void BarCode_BarCodeEvent(BardCodeHooK.BarCodes barCode)
        {

            ShowInfo(barCode);
        }


        private void frmProductionSchedule_Load(object sender, EventArgs e)
        {
            dtDgvDefective.Columns.Add("seq", typeof(string));
            dtDgvDefective.Columns.Add("defective_id", typeof(string));
            dtDgvDefective.Columns.Add("defective_cdesc", typeof(string));
            dtDgvDefective.Columns.Add("oth_defective", typeof(string));

            dtWorker.Columns.Add("prd_worker", typeof(string));
            dtWorker.Columns.Add("hrm1name", typeof(string));
            
            InitComBoxs();
            //get_prd_worker();//初始化工號表
            //加載時讓條碼框獲得焦點
            //txtBarCode.Focus();

            Font a = new Font("GB2312", 10);//GB2312为字体名称，1为字体大小dataGridView1.Font = a;
            dgvDetails.Font = a;
            dgvWorker.Font = a;
            dgvDefective.Font = a;
            dgvDetails.AutoGenerateColumns = false;

            BarCode.Start();
            
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
            if(_userid.Substring(0,3)=="ALY")
                cmbProductDept.Text = "302";
            else
                if (_userid.Substring(0, 3) == "BUT")
                    cmbProductDept.Text = "102";
                else
                    if (_userid.Substring(0, 3) == "BUK")
                        cmbProductDept.Text = "202";
                    else
                        if (_userid.Substring(0, 3) == "PLA")
                            cmbProductDept.Text = "501";
                        else
                            if (_userid.Substring(0, 3) == "BLK")
                                cmbProductDept.Text = "105";
            
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

            DataTable dtOwnDep = dtPrd_dept.Copy();
            cmbOwnDep.DataSource = dtOwnDep;
            cmbOwnDep.DisplayMember = "int9loc";
            cmbOwnDep.ValueMember = "int9loc";
            
        }
        private void InitComBoxGroup()
        {
            string group_type = "2";
            if (cmbProductDept.SelectedValue.ToString()=="203")
                group_type = "1";
            txtWork_code.Visible = true;
            cmbWork_code.Visible = false;
            lblWork_class.Visible = false;
            txtWork_class.Visible = false;
            panel5.Visible = true;
            txtmWeg1.Text = "0";//去皮
            txtmWeg2.Text = "0";

            string strSql = "";
            strSql = " SELECT work_group,group_desc FROM work_group WHERE ( dep='" + cmbProductDept.Text.Trim() + "'" + " AND group_type='" + group_type + "') " + " OR dep='" + "000" + "' ";
            DataTable dtGroup = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            if (dtGroup.Rows.Count > 0)
            {
                cmbGroup.DataSource = dtGroup;
                cmbGroup.DisplayMember = "work_group";
                cmbGroup.ValueMember = "work_group";
            }
            if (cmbProductDept.Text == "102")
            {
                cmbGroup.Text = "BA01";
            }
            else
            {
                if (cmbProductDept.Text == "302")
                {
                    txtmWeg1.Text = "1.4";//去皮
                    txtmWeg2.Text = "0";
                }
                else
                {
                    if (string.Compare(cmbProductDept.Text, "5") > 0 && string.Compare(cmbProductDept.Text, "599") < 0)
                    {
                        txtmWeg1.Text = "0";//去皮
                        txtmWeg2.Text = "0";
                        cmbGroup.Text = "T1";
                        txtWork_code.Visible = false;
                        cmbWork_code.Visible = true;
                        panel5.Visible = true;
                    }
                    else
                    {
                        if (cmbProductDept.Text == "105")
                        {
                            txtmWeg1.Text = "0";//去皮
                            txtmWeg2.Text = "0";
                            cmbGroup.Text = "BC05-01";
                            panel5.Visible = false;
                        }
                        else
                        {
                            if (string.Compare(cmbProductDept.Text, "7") > 0 && string.Compare(cmbProductDept.Text, "799") < 0)
                            {
                                txtmWeg1.Text = "0";//去皮
                                txtmWeg2.Text = "0";
                                cmbGroup.Text = "T1";
                                txtWork_code.Visible = false;
                                cmbWork_code.Visible = true;
                                panel5.Visible = false;
                            }
                            else
                            {
                                if (cmbProductDept.Text == "203")
                                {
                                    lblWork_class.Visible = true;
                                    txtWork_class.Visible = true;
                                }
                            }
                        }
                    }
                }
            }

            loadDefective();

            strSql = " SELECT dep,job_type FROM job_type Where dep='" + cmbProductDept.Text + "' And s_flag is null order by job_type";
            DataTable dtJob_type = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            cmbWork_code.DataSource = dtJob_type;
            cmbWork_code.DisplayMember = "job_type";
            cmbWork_code.ValueMember = "job_type";

        }

        private void loadDefective()
        {
            string strSql = " SELECT defective_id,defective_cdesc FROM defective_tb ";
            DataTable dtDefective = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            cmbDefective_id.DataSource = dtDefective;
            cmbDefective_id.DisplayMember = "defective_cdesc";
            cmbDefective_id.ValueMember = "defective_id";
        }
        private void txtmo_id_Leave(object sender, EventArgs e)
        {
            cmbGoods_id.Text = "";
            txtgoods_desc.Text = "";
            if (txtmo_id.Text != "" && cmbProductDept.Text != "")
            {
                GetMo_itme("");
                //設定第一個為默認的
                cmbGoods_id.SelectedIndex = 0;
                get_data_details();
                txtOk_qty.Focus();
            }
        }

        //獲取制單編號資料，并綁定物料編號
        private void GetMo_itme(string item)
        {
            cmbGoods_id.Items.Clear();
            string fdep, tdep;
            if (cmbProductDept.Text.Trim() == cmbOwnDep.Text.Trim())
            {
                fdep = cmbProductDept.Text.Trim();
                tdep = "";
            }
            else
            {
                fdep = "";
                tdep = cmbOwnDep.Text.Trim();
            }
            dtMo_item = clsProductionSchedule.GetMo_dataById(txtmo_id.Text.Trim(), fdep, item);//, tdep
            for (int i = 0; i < dtMo_item.Rows.Count; i++)
            {
                cmbGoods_id.Items.Add(dtMo_item.Rows[i]["goods_id"].ToString());
            }
            //cmbGoods_id.DataSource = dtMo_item;
            //cmbGoods_id.DisplayMember = "goods_id";
            //cmbGoods_id.ValueMember = "goods_id";

        }

        //查詢未完成的記錄，並重新賦值，便於重新輸入完整資料
        private void get_prd_records(int con_type)
        {
            try
            {
                //獲取制單編號資料
                string sql = "";
                sql += " Select a.*,rtrim(b.work_type_desc) as work_type_desc ";
                sql += " From product_records a with(nolock) ";
                sql += " Left outer join work_type b on a.prd_work_type=b.work_type_id ";
                sql += " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'";
                sql += " And a.prd_work_type = " + "'" + "A03" + "'";
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
                                        else
                                        {
                                            if (con_type == 8)//按記錄號提取記錄
                                            {
                                                sql += " And a.prd_id =' " + txtPrd_id.Text + "'";
                                            }
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

        //提取工號
        private void get_prd_worker()
        {
            //獲取制單編號資料 COLLATE Chinese_PRC_CI_AS
            string sql = "";
            sql += " Select a.prd_worker,b.hrm1name " +
                " From product_records_worker a with(nolock) " +
                " Left Join dgsql1.dghr.dbo.hrm01 b on a.prd_worker=b.hrm1wid  COLLATE Chinese_PRC_CI_AS" +
                " Where a.prd_id = " + "'" + (txtPrd_id.Text != "" ? Convert.ToInt32(txtPrd_id.Text) : 0) + "'";
            DataTable tempdtWorker = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            dtWorker.Clear();
            for (int i = 0; i < tempdtWorker.Rows.Count; i++)
            {
                DataRow dr = dtWorker.NewRow();
                dr["prd_worker"] = tempdtWorker.Rows[i]["prd_worker"].ToString();
                dr["hrm1name"] = tempdtWorker.Rows[i]["hrm1name"];
                dtWorker.Rows.Add(dr);
            }

            dgvWorker.DataSource = dtWorker;
        }
        //提取次品記錄
        private void get_prd_defective()
        {
            string sql = "";
            sql += " Select a.seq,a.defective_id,a.oth_defective,b.defective_cdesc " +
                " From product_records_defective a with(nolock) " +
                " Left Join defective_tb b on a.defective_id=b.defective_id" +
                " Where a.prd_id = " + "'" + (txtPrd_id.Text != "" ? Convert.ToInt32(txtPrd_id.Text) : 0) + "'";
            DataTable dtDefective = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            dtDgvDefective.Clear();
            for (int i = 0; i < dtDefective.Rows.Count; i++)
            {
                DataRow dr = dtDgvDefective.NewRow();
                dr["seq"] = dtDefective.Rows[i]["seq"].ToString();
                dr["defective_id"] = dtDefective.Rows[i]["defective_id"];
                dr["defective_cdesc"] = dtDefective.Rows[i]["defective_cdesc"];
                dr["oth_defective"] = dtDefective.Rows[i]["oth_defective"];
                dtDgvDefective.Rows.Add(dr);
            }
            dgvDefective.DataSource = dtDgvDefective;
        }
        //取組別的當日最後的完成時間作為下次的開始時間
        private void get_last_prd_end_time()
        {

            string strsql_part;
            string sql;
            string last_date = System.DateTime.Now.ToString("yyyy/MM/dd");
            DataTable dtLastTime = new DataTable();
            strsql_part = " (Select Max(prd_id) AS prd_id " +
                " From product_records with(nolock) Where " +
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
            else
                dtpStart.Value = Convert.ToDateTime("2014/01/01 " + "00:00");
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
            cmbOwnDep.Text = dtProductionRecordslist.Rows[index]["prd_owndep"].ToString();
            cmbOrder_class.Text = dtProductionRecordslist.Rows[index]["prd_class"].ToString();
            cmbGroup.Text = dtProductionRecordslist.Rows[index]["prd_group"].ToString();

            cmbWorkType.Text = dtProductionRecordslist.Rows[index]["work_type_desc"].ToString().Trim();
            dtpStart.Value = Convert.ToDateTime("2014/01/01 " + dtProductionRecordslist.Rows[index]["prd_start_time"].ToString());
            dtpEnd.Value = Convert.ToDateTime("2014/01/01 " + dtProductionRecordslist.Rows[index]["prd_end_time"].ToString());
            txtNormal_work.Text = (dtProductionRecordslist.Rows[index]["prd_normal_time"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_normal_time"].ToString() : "");
            txtAdd_work.Text = (dtProductionRecordslist.Rows[index]["prd_ot_time"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_ot_time"].ToString() : "");
            txtper_Standrad_qty.Text = (dtProductionRecordslist.Rows[index]["hour_std_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["hour_std_qty"].ToString() : "");
            txtPer_hour_std_qty.Text = (dtProductionRecordslist.Rows[index]["per_hour_std_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["per_hour_std_qty"].ToString() : "");
            txtPrd_qty.Text = (dtProductionRecordslist.Rows[index]["prd_qty"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_qty"].ToString() : "");
            txtprd_weg.Text = (dtProductionRecordslist.Rows[index]["prd_weg"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["prd_weg"].ToString() : "");
            txtkgPCS.Text = (dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["kg_pcs"].ToString() : "");
            dtpReqEnd.Text = dtProductionRecordslist.Rows[index]["prd_req_time"].ToString();
            txtWork_code.Text = dtProductionRecordslist.Rows[index]["work_code"].ToString();
            cmbWork_code.Text = txtWork_code.Text;
            txtWork_class.Text = dtProductionRecordslist.Rows[index]["work_class"].ToString();
            txtPack_num.Text = (dtProductionRecordslist.Rows[index]["pack_num"].ToString() != "0" ? dtProductionRecordslist.Rows[index]["pack_num"].ToString() : "");
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
            //if (edit_type == "Y")
            //{
            //    get_data_details();
            //}
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
            if (cmbProductDept.Text != "501")
                dteProdcutDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
            txtgoods_desc.Text = "";
            txtkgPCS.Text = "";
            txtPrd_qty.Text = "";
            txtprd_weg.Text = "";
            cmbWorkType.Text = "";
            dtpStart.Value = Convert.ToDateTime("2014/01/01 " + "00:00");
            dtpEnd.Value = Convert.ToDateTime("2014/01/01 " + "00:00");
            chkcont_work1.Checked = false;
            chkcont_work2.Checked = false;
            txtNormal_work.Text = "";
            txtAdd_work.Text = "";
            txtper_Standrad_qty.Text = "";
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
            txtPer_hour_std_qty.Text = "";
            txtWork_code.Text = "";
            txtPrd_id_ref.Text = "";
            txtActual_weg.Text = "";
            txtActual_qty.Text = "";
        }

        private void fill_plan_value()
        {
            txtgoods_desc.Text = "";
            for (int i = 0; i < dtMo_item.Rows.Count; i++)
            {
                if (cmbGoods_id.Text.ToString() == dtMo_item.Rows[i]["goods_id"].ToString())
                {
                    txtgoods_desc.Text = dtMo_item.Rows[i]["name"].ToString();
                    if (dtMo_item.Rows[i]["prod_qty"].ToString() != "" && txtPrd_qty.Text == "")
                    {
                        txtPrd_qty.Text = Convert.ToInt32(dtMo_item.Rows[i]["prod_qty"]).ToString();
                    }
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
            //if (cmbGroup.Text == "")
            //{
            //    MessageBox.Show("組別不能為空,請重新輸入!");
            //    cmbGroup.Focus();
            //    cmbGroup.SelectAll();
            //    return false;
            //}
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
            //如果是完成的，就要做如下控制
            if (dtpStart.Text != "00:00" && dtpEnd.Text != "00:00" && cmbGroup.Text != "AB99")
            {
                if (dtWorker.Rows.Count == 0 )
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
                if (string.Compare(cmbProductDept.Text.Trim(), "500") > 0 && string.Compare(cmbProductDept.Text, "599") < 0)
                {
                    if (txtOk_qty.Text == "" || txtOk_qty.Text == "0")
                    {
                        MessageBox.Show("良品數不能為空!");
                        txtOk_qty.Focus();
                        txtOk_qty.SelectAll();
                        return false;
                    }
                    if (!Verify.StringValidating(txtOk_qty.Text.Trim(), Verify.enumValidatingType.AllNumber))
                    {
                        MessageBox.Show("包數格式有誤,請重新輸入!");
                        txtOk_qty.Focus();
                        txtOk_qty.SelectAll();
                        return false;
                    }
                    if (txtNook_qty.Text != "" && !Verify.StringValidating(txtNook_qty.Text.Trim(), Verify.enumValidatingType.AllNumber))
                    {
                        MessageBox.Show("不良品數格式有誤,請重新輸入!");
                        txtOk_qty.Focus();
                        txtOk_qty.SelectAll();
                        return false;
                    }
                    if (cmbWork_code.Text == "")
                    {
                        MessageBox.Show("標準編碼不能為空,請重新輸入!");
                        cmbWork_code.Focus();
                        cmbWork_code.SelectAll();
                        return false;
                    }
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
            if (txtSample_no.Text != "" && !clsValidRule.IsNumeric(txtSample_no.Text))
            {
                MessageBox.Show("圍數個數格式有誤,請重新輸入!");
                txtSample_no.Focus();
                txtkgPCS.SelectAll();
                return false;
            }
            if (txtSample_weg.Text != "" && !clsValidRule.IsNumeric(txtSample_weg.Text))
            {
                MessageBox.Show("圍數重量格式有誤,請重新輸入!");
                txtSample_weg.Focus();
                txtSample_weg.SelectAll();
                return false;
            }
            if (txtkgPCS.Text != "" && !clsValidRule.IsNumeric(txtkgPCS.Text))
            {
                MessageBox.Show("每Kg個數格式有誤,請重新輸入!");
                txtkgPCS.Focus();
                txtkgPCS.SelectAll();
                return false;
            }
            if (txtOkqty_All.Text != "" && !clsValidRule.IsNumeric(txtOkqty_All.Text))
            {
                MessageBox.Show("良品毛重格式有誤,請重新輸入!");
                txtOkqty_All.Focus();
                txtOkqty_All.SelectAll();
                return false;
            }
            if (txtmWeg1.Text != "" && !clsValidRule.IsNumeric(txtmWeg1.Text))
            {
                MessageBox.Show("去皮格式有誤,請重新輸入!");
                txtmWeg1.Focus();
                txtmWeg1.SelectAll();
                return false;
            }
            if (txtNookqty_All.Text != "" && !clsValidRule.IsNumeric(txtNookqty_All.Text))
            {
                MessageBox.Show("不良品毛重格式有誤,請重新輸入!");
                txtNookqty_All.Focus();
                txtNookqty_All.SelectAll();
                return false;
            }
            if (txtmWeg2.Text != "" && !clsValidRule.IsNumeric(txtmWeg2.Text))
            {
                MessageBox.Show("去皮格式有誤,請重新輸入!");
                txtmWeg2.Focus();
                txtmWeg2.SelectAll();
                return false;
            }
            if (txtOk_weg.Text != "" && !clsValidRule.IsNumeric(txtOk_weg.Text))
            {
                MessageBox.Show("良品重量格式有誤,請重新輸入!");
                txtOk_weg.Focus();
                txtOk_weg.SelectAll();
                return false;
            }
            if (txtOk_qty.Text != "" && !clsValidRule.IsNumeric(txtOk_qty.Text))
            {
                MessageBox.Show("良品數量格式有誤,請重新輸入!");
                txtOk_qty.Focus();
                txtOk_qty.SelectAll();
                return false;
            }
            if (txtNook_weg.Text != "" && !clsValidRule.IsNumeric(txtNook_weg.Text))
            {
                MessageBox.Show("不良品重量格式有誤,請重新輸入!");
                txtNook_weg.Focus();
                txtNook_weg.SelectAll();
                return false;
            }
            if (txtNook_qty.Text != "" && !clsValidRule.IsNumeric(txtNook_qty.Text))
            {
                MessageBox.Show("不良品數量格式有誤,請重新輸入!");
                txtNook_qty.Focus();
                txtNook_qty.SelectAll();
                return false;
            }
            if (txtprd_weg.Text != "" && !clsValidRule.IsNumeric(txtprd_weg.Text))
            {
                MessageBox.Show("選貨重量格式有誤,請重新輸入!");
                txtprd_weg.Focus();
                txtprd_weg.SelectAll();
                return false;
            }
            if (txtPrd_qty.Text != "" && !clsValidRule.IsNumeric(txtPrd_qty.Text))
            {
                MessageBox.Show("選貨數量格式有誤,請重新輸入!");
                txtPrd_qty.Focus();
                txtPrd_qty.SelectAll();
                return false;
            }
            if (txtPack_num.Text != "" && !clsValidRule.IsNumeric(txtPack_num.Text))
            {
                MessageBox.Show("包數格式有誤,請重新輸入!");
                txtPack_num.Focus();
                txtPack_num.SelectAll();
                return false;
            }

            return true;
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
            string item;
            if (dgvDetails.Rows.Count > 0)
            {
                edit_type = "N";//控件不作為編輯
                fill_exist_record(e.RowIndex);
                get_prd_worker();//獲取工號
                get_prd_defective();//提取次品記錄
                txtmo_id.Text = dtProductionRecordslist.Rows[e.RowIndex]["prd_mo"].ToString();
                item = dtProductionRecordslist.Rows[e.RowIndex]["prd_item"].ToString();
                cmbGoods_id.Items.Clear();
                cmbGoods_id.Items.Add(item);
                //GetMo_itme(item);
                cmbGoods_id.Text = item;//物料編號
                txtgoods_desc.Text = GetItemDesc(item);//獲取物料描述
                get_total_prd_qty();//顯示單的總完成數量
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
        //獲取所有已選貨的數量
        private void get_total_prd_qty()
        {
            DataTable db_show_qty = new DataTable();

            string sql = "";
            sql += " Select sum(prd_qty) as prd_qty From product_records a with(nolock)"+
                " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'"+
                " And a.prd_mo = " + "'" + txtmo_id.Text.ToString() + "'"+
                " And a.prd_item = " + "'" + cmbGoods_id.Text.ToString() + "'"+
                " And a.prd_work_type = '"+"A03"+"'"+
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
            if (txtPrd_qty.Text != "" && txtper_Standrad_qty.Text != "" && txtper_Standrad_qty.Text != "0" && dtpStart.Text != "00:00")
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
            count_kg_pcs();//計算每KG對應數量
        }

        private void txtBarCode_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter)
            {
                if (txtBarCode.Text.Trim().Length > BarCodeMinLength)
                    doBarCode();
                txtBarCode.Text = "";
            }

        }

        
        private void txtgoods_desc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtgoods_desc.Text = "";
        }


        private void get_data_details()
        {
            ClearPartOfText(); //清空文本框內容
            fill_plan_value();//首先將計劃單帶出數量、描述
            get_prd_records(1);//查詢已錄入的記錄
            chk_prd_no_complete();//檢查是否有未完成的記錄，默認帶出來
            get_prd_worker();//獲取選貨工號
            get_prd_defective();//查詢次品記錄
            FillGrid();//將查詢到的記錄存入列表
            fill_txt_kg_pcs();//提取物料每公斤對應數量
            count_kg_pcs();//換算重量
            //如果是外发部门，则提取外发收货的数量当作选货数
            if (string.Compare(cmbOwnDep.Text, "5") > 0 && string.Compare(cmbOwnDep.Text, "599") < 0)
                get_plate_qty();
            get_total_prd_qty();//獲取總完成數量
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtPrd_qty.Text != "" && txtprd_weg.Text != "" && txtprd_weg.Text != "0")
                txtkgPCS.Text = Convert.ToInt32(Convert.ToSingle(txtPrd_qty.Text) / Convert.ToSingle(txtprd_weg.Text)).ToString();
        }
        //获取外发收货数量
        private void get_plate_qty()
        {
            string strSql = "";
            string dat = Convert.ToDateTime(dteProdcutDate.Text).AddDays(-3).ToString("yyyy/MM/dd");// System.DateTime.Now.AddDays(-3).ToString("yyyy/MM/dd");
            strSql = "Select b.t_ir_qty,b.sec_qty From "+remote_db+"op_outpro_in_mostly a "+
                " Inner Join "+remote_db+"op_outpro_in_detail b ON a.within_code=b.within_code AND a.id=b.id";
            strSql += " Where a.within_code='0000' AND a.dept_id='" + cmbProductDept.Text + "' And a.ir_date > '" + dat + "'" +
                " AND b.mo_id='" + txtmo_id.Text + "' AND b.goods_id='" + cmbGoods_id.Text + "'";
            strSql += " Order By a.ir_date Desc,b.id,b.sequence_id Desc";
            DataTable dtPrdPlate = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            if (dtPrdPlate.Rows.Count > 0)
            {
                txtPrd_qty.Text = (dtPrdPlate.Rows[0]["t_ir_qty"].ToString()!=""?Convert.ToInt32(dtPrdPlate.Rows[0]["t_ir_qty"]).ToString():"0");
                txtprd_weg.Text = (dtPrdPlate.Rows[0]["sec_qty"].ToString() != "" ? Math.Round(Convert.ToDecimal(dtPrdPlate.Rows[0]["sec_qty"]),2).ToString() : "0");
            }
        }

        private void BTNNOCOMP_Click(object sender, EventArgs e)
        {
            get_prd_records(2);//查詢已錄入的記錄
            FillGrid(); //將查詢到的記錄存入列表
        }

        private void BTNREFRESHMO_Click(object sender, EventArgs e)
        {
            get_prd_records(1);//查詢已錄入的記錄
            FillGrid(); //將查詢到的記錄存入列表
        }

        private void BTNNOSTART_Click(object sender, EventArgs e)
        {
            get_prd_records(4);//未開始的記錄
            FillGrid(); //將查詢到的記錄存入列表
        }

        private void BTNCOMP_Click(object sender, EventArgs e)
        {
            get_prd_records(5);//當天完成的記錄
            FillGrid(); //將查詢到的記錄存入列表
        }
        private void txtSearchMo_TextChanged(object sender, EventArgs e)
        {
            if (txtSearchMo.Text.Trim().Length>4)
            {
                //if (this.chkIsComplete.Checked == false)
                //    get_prd_records(6);//按制單編號查詢未完成的記錄
                //else
                //    get_prd_records(7);//按制單編號查詢包括已完成的記錄
                get_prd_records(7);//按制單編號查詢包括已完成的記錄
                FillGrid(); //將查詢到的記錄存入列表
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

        private void txtRow_qty_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                count_hour_std_qty();//計算標準數量
            }
        }

        private void txtPer_Convert_qty_TextChanged(object sender, EventArgs e)
        {
            if(edit_type =="Y")
                count_hour_std_qty();
        }

        private void txtPrd_qty_Leave(object sender, EventArgs e)
        {
            //count_kg_pcs();  ////計算每KG對應數量
            count_hour_std_qty();//計算每人每小時標準數
            count_req_time();//預計完成時間
        }

        private void cmbProductDept_Leave(object sender, EventArgs e)
        {
            edit_type = "N";
            InitComBoxGroup();//初始化組別
            ClearAllText();
            cmbOwnDep.Text = cmbProductDept.Text;
        }

        private void dtpEnd_MouseDown(object sender, MouseEventArgs e)
        {
            if (dtpEnd.Text == "00:00" &&dtpStart.Text != "00:00")
                dtpEnd.Value = System.DateTime.Now;
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
                hrm1name=chk_prd_worker(txtWorker.Text);
                if (hrm1name != "")
                {
                    add_prd_worker(txtWorker.Text, hrm1name);
                    dgvWorker.DataSource = dtWorker;
                }
            }
            txtmo_id_KeyPress(sender, e);
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
                DataRow dr = dtWorker.NewRow(); 
                dr["prd_worker"] = wid;
                dr["hrm1name"] = hrm1name;
                dtWorker.Rows.Add(dr);
                txtWorker.Text = "";
                txtTotMember.Text = (count_member+1).ToString();
            }
            count_hour_std_qty();//計算每人每小時標準數
        }


        private void txtSample_no_Leave(object sender, EventArgs e)
        {
            count_kg_pcs();
        }

        private void txtSample_weg_Leave(object sender, EventArgs e)
        {
            count_kg_pcs();
        }
        //計算每KG對應數量
        private void count_kg_pcs()
        {
            if (!clsValidRule.IsNumeric(txtSample_no.Text) || !clsValidRule.IsNumeric(txtSample_weg.Text))
                return;
            txtkgPCS.Text = "";
            if (txtSample_no.Text != "" && txtSample_weg.Text != "" && Convert.ToSingle(txtSample_weg.Text) != 0)
                txtkgPCS.Text = Math.Round(Convert.ToInt32(txtSample_no.Text) / (Convert.ToSingle(txtSample_weg.Text)/1000), 0).ToString();
            count_select_qty();//計算良品數量
        }
        private void txtOk_weg_Leave(object sender, EventArgs e)
        {
            txtOk_qty.Text = "";
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
            if (!clsValidRule.IsNumeric(txtOk_qty.Text) || !clsValidRule.IsNumeric(txtNook_qty.Text))
                return;
            //if (txtPrd_qty.Text == "" || txtPrd_qty.Text == "0")
                txtPrd_qty.Text = ((txtOk_qty.Text != "" ? Convert.ToInt32(txtOk_qty.Text) : 0) + (txtNook_qty.Text != "" ? Convert.ToInt32(txtNook_qty.Text) : 0)).ToString();
            if (!clsValidRule.IsNumeric(txtOk_weg.Text) || !clsValidRule.IsNumeric(txtNook_weg.Text))
                return;
            //if (txtprd_weg.Text == "" || txtprd_weg.Text == "0")
                txtprd_weg.Text = ((txtOk_weg.Text != "" ? Convert.ToSingle(txtOk_weg.Text) : 0) + (txtNook_weg.Text != "" ? Convert.ToSingle(txtNook_weg.Text) : 0)).ToString();
            count_hour_std_qty();//計算每人每小時標準數
        }
        private void txtNook_weg_Leave(object sender, EventArgs e)
        {
            txtNook_qty.Text = "";
            count_select_qty();//計算不良品數量
        }
        //獲取組別的成員
        private void get_group_member()
        {
            //if (txtmo_id.Text == "" || cmbGoods_id.Text == "")
            //    return;
            DataTable dtMember = new DataTable();
            string sql = "";
            //int count = dtWorker.Rows.Count;
            //for (int i = count - 1; i >= 0; i--)
            //{
            //    dtWorker.Rows.RemoveAt(i);
            //}

            dtWorker.Clear();
            sql += " Select a.prd_worker,b.hrm1name From product_group_member a " +
                " Left Join dgsql1.dghr.dbo.hrm01 b on a.prd_worker=b.hrm1wid  COLLATE Chinese_PRC_CI_AS " +
                " Where a.prd_dep = " + "'" + cmbProductDept.SelectedValue.ToString() + "'" +
                " And a.prd_group = " + "'" + cmbGroup.Text.ToString() + "'";
            dtMember = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            for (int i = 0; i < dtMember.Rows.Count; i++)
            {
                add_prd_worker(dtMember.Rows[i]["prd_worker"].ToString(), dtMember.Rows[i]["hrm1name"].ToString());
            }
            dgvWorker.DataSource = dtMember;
        }
        //計算每人每小時標準數
        private void count_hour_std_qty()
        {
            int qty=(txtPrd_qty.Text !="" ? Convert.ToInt32(txtPrd_qty.Text):0);
            int rs=(txtTotMember.Text !="" ? Convert.ToInt32(txtTotMember.Text):1);
            float tt=(txtNormal_work.Text !="" ? Convert.ToSingle(txtNormal_work.Text):0)+(txtAdd_work.Text !="" ? Convert.ToSingle(txtAdd_work.Text):0);
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

        private void txtOk_qty_Leave(object sender, EventArgs e)
        {
            add_prd_qty();//生產數量=良品+不良品
        }

        private void txtNook_qty_Leave(object sender, EventArgs e)
        {
            add_prd_qty();//生產數量=良品+不良品
        }
        private void count_ok_weg()
        {
            if (!clsValidRule.IsNumeric(txtOkqty_All.Text) || !clsValidRule.IsNumeric(txtmWeg1.Text))
                return;
            txtOk_weg.Text = "";
            if (txtOkqty_All.Text != "" && txtmWeg1.Text != "")
                txtOk_weg.Text = (Convert.ToSingle(txtOkqty_All.Text) - Convert.ToSingle(txtmWeg1.Text)).ToString();
            count_select_qty();//計算良品數量
        }
        private void count_nook_weg()
        {
            if (!clsValidRule.IsNumeric(txtNookqty_All.Text) || !clsValidRule.IsNumeric(txtmWeg2.Text))
                return;
            txtNook_weg.Text = "";
            if (txtNookqty_All.Text != "" && txtmWeg2.Text != "")
                txtNook_weg.Text = (Convert.ToSingle(txtNookqty_All.Text) - Convert.ToSingle(txtmWeg2.Text)).ToString();
            count_select_qty();//計算良品數量
        }

        private void dgvWorker_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (dtWorker.Rows[0]["prd_worker"].ToString() == "")
            //    txtTotMember.Text = (Convert.ToInt32(txtTotMember.Text) - 1).ToString();
        }

        private void btnDeleteMember_Click(object sender, EventArgs e)
        {
            if (dgvWorker.Rows.Count > 0)
            {
                dtWorker.Rows.RemoveAt(dgvWorker.CurrentCell.RowIndex);
                txtTotMember.Text = dgvWorker.Rows.Count.ToString();
            }
            dgvWorker.DataSource = dtWorker;
        }

        private void btnAddDefective_Click(object sender, EventArgs e)
        {
            if (cmbDefective_id.Text.Trim() == "" && txtOth_Defective.Text.Trim() == "")
                return;
            DataRow dr = dtDgvDefective.NewRow();
            if (cmbDefective_id.Text.Trim() != "")
            {
                dr["defective_id"] = cmbDefective_id.SelectedValue.ToString();
                dr["defective_cdesc"] = cmbDefective_id.Text;
            }
            dr["oth_defective"] = txtOth_Defective.Text;
            dtDgvDefective.Rows.Add(dr);
            txtOth_Defective.Text = "";
            cmbDefective_id.Text = "";
            dgvDefective.DataSource = dtDgvDefective;
        }

        private void btnDelDefective_Click(object sender, EventArgs e)
        {
            if (dgvDefective.Rows.Count > 0)
            {
                dtDgvDefective.Rows.RemoveAt(dgvDefective.CurrentCell.RowIndex);
            }
        }


        private void dtpEnd_Leave(object sender, EventArgs e)
        {
            count_datetime();
            count_hour_std_qty();//計算每人每小時標準數
        }

        private void dtpStart_Leave(object sender, EventArgs e)
        {
            count_datetime();
            count_hour_std_qty();//計算每人每小時標準數
            count_req_time();//預計完成時間
        }

        private void txtOkqty_All_Leave(object sender, EventArgs e)
        {
            count_ok_weg();
        }

        private void txtmWeg1_Leave(object sender, EventArgs e)
        {
            count_ok_weg();
        }

        private void txtNookqty_All_Leave(object sender, EventArgs e)
        {
            count_nook_weg();
        }

        private void txtmWeg2_Leave(object sender, EventArgs e)
        {
            count_nook_weg();
        }

        private void txtper_Standrad_qty_Leave(object sender, EventArgs e)
        {
            count_req_time();//預計完成時間
        }

        private void cmbGroup_Leave(object sender, EventArgs e)
        {
            get_group_member();//獲取組別的成員
            get_last_prd_end_time();//查詢組別當日最後的完成時間，作為開始時間
            if (cmbGroup.Text == "AB99")
            {
                dtpStart.Value = System.DateTime.Now;
                dtpEnd.Value = dtpStart.Value;
            }
        }

        private void cmbGoods_id_Leave(object sender, EventArgs e)
        {
            get_data_details();
            txtOk_qty.Focus();
        }

        private void txtprd_weg_Leave(object sender, EventArgs e)
        {
            if (cmbProductDept.SelectedValue.ToString() == "203")
            {
                if (txtkgPCS.Text != "" && txtprd_weg.Text != "")
                    txtPrd_qty.Text = Math.Round(Convert.ToDecimal(txtkgPCS.Text) * Convert.ToDecimal(txtprd_weg.Text), 0).ToString();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!valid_data())
                return;
            product_records  objModel = new product_records();
            objModel.prd_dep = cmbProductDept.SelectedValue.ToString();
            objModel.prd_owndep = cmbOwnDep.Text.ToString();
            objModel.prd_pdate = dteProdcutDate.Text.ToString();//計劃日期=生產日期
            objModel.prd_date = dteProdcutDate.Text.ToString();
            objModel.prd_mo = txtmo_id.Text.Trim();
            objModel.prd_item = cmbGoods_id.Text.ToString().Trim();
            objModel.prd_qty = (txtPrd_qty.Text != "" ? Convert.ToInt32(txtPrd_qty.Text) : 0);
            objModel.prd_weg = (txtprd_weg.Text != "" ? Convert.ToSingle(txtprd_weg.Text) : 0);
            objModel.prd_machine = "";
            objModel.prd_work_type = cmbWorkType.SelectedValue.ToString();
            objModel.prd_worker = "";
            objModel.prd_class = cmbOrder_class.Text.Trim();
            objModel.prd_group = cmbGroup.Text.Trim();
            objModel.prd_start_time = (dtpStart.Text.Trim() != "00:00" ? dtpStart.Text.Trim() : "");
            objModel.prd_end_time = (dtpEnd.Text.Trim() != "00:00" ? dtpEnd.Text.Trim() : "");
            objModel.prd_req_time = (dtpReqEnd.Text.Trim() != "00:00" ? dtpReqEnd.Text.Trim() : "");
            objModel.prd_normal_time = (txtNormal_work.Text != "" ? Convert.ToSingle(txtNormal_work.Text) : 0);
            objModel.prd_ot_time = (txtAdd_work.Text != "" ? Convert.ToSingle(txtAdd_work.Text) : 0);
            objModel.line_num = 0;
            objModel.hour_run_num = 0;
            objModel.hour_std_qty = (txtper_Standrad_qty.Text != "" ? Convert.ToInt32(txtper_Standrad_qty.Text) : 0);
            objModel.per_hour_std_qty = (txtPer_hour_std_qty.Text != "" ? Convert.ToInt32(txtPer_hour_std_qty.Text) : 0);
            objModel.kg_pcs = (txtkgPCS.Text != "" ? Convert.ToInt32(txtkgPCS.Text) : 0);
            objModel.mat_item = "";
            objModel.mat_item_lot = "";
            objModel.mat_item_desc = "";
            objModel.to_dep = "";
            objModel.crusr = _userid;
            objModel.crtim = DateTime.Now;
            objModel.amusr = _userid;
            objModel.amtim = DateTime.Now;
            objModel.prd_id = record_id;
            objModel.difficulty_level = "";
            objModel.pack_num = (txtPack_num.Text != "" ? Convert.ToInt32(txtPack_num.Text) : 0);
            if (string.Compare(objModel.prd_dep, "5") > 0 && string.Compare(objModel.prd_dep, "599") < 0)
                objModel.work_code = cmbWork_code.Text;
            else
                objModel.work_code = txtWork_code.Text;
            objModel.prd_run_qty = 0;
            objModel.speed_lever = 0;
            objModel.start_run = 0;
            objModel.end_run = 0;
            objModel.ok_qty = (txtOk_qty.Text != "" ? Convert.ToInt32(txtOk_qty.Text) : 0);
            objModel.ok_weg = Math.Round((txtOk_weg.Text != "" ? Convert.ToDecimal(txtOk_weg.Text) : 0), 4);
            objModel.no_ok_qty = (txtNook_qty.Text != "" ? Convert.ToInt32(txtNook_qty.Text) : 0);
            objModel.no_ok_weg = Math.Round((txtNook_weg.Text != "" ? Convert.ToDecimal(txtNook_weg.Text) : 0), 4);
            objModel.sample_no = (txtSample_no.Text != "" ? Convert.ToInt32(txtSample_no.Text) : 0);
            objModel.sample_weg = Math.Round((txtSample_weg.Text != "" ? Convert.ToDecimal(txtSample_weg.Text) : 0), 4);
            objModel.member_no = (txtTotMember.Text != "" ? Convert.ToInt32(txtTotMember.Text) : 1);
            objModel.prd_id_ref = (txtPrd_id_ref.Text != "" ? Convert.ToInt32(txtPrd_id_ref.Text) : 0);
            objModel.work_class = txtWork_class.Text;
            objModel.actual_qty = (txtActual_qty.Text != "" ? Convert.ToInt32(txtActual_qty.Text) : 0);
            objModel.actual_weg = (txtActual_weg.Text != "" ? Convert.ToDecimal(txtActual_weg.Text) : 0);
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
                            if (dgvWorker.Rows[i].Cells["prd_worker"].Value.ToString() != "")
                                Result = clsProductionSchedule.AddPrdWorker(record_id, dgvWorker.Rows[i].Cells["prd_worker"].Value.ToString(), _userid, DateTime.Now);
                        }
                    }
                    ////儲存次品記錄
                    //if (clsProductionSchedule.DeletePrdDefective(record_id) > 0)
                    //{
                        //儲存次品記錄
                        if (clsProductionSchedule.DeletePrdDefective(record_id) > 0)
                        {
                            //重新儲存工號
                            string seq;
                            for (int i = 0; i < dgvDefective.Rows.Count; i++)
                            {
                                seq = (i + 1).ToString().PadLeft(2, '0');
                                Result = clsProductionSchedule.AddPrdDefective(record_id, seq, dgvDefective.Rows[i].Cells["colDefective_id"].Value.ToString()
                                    , dgvDefective.Rows[i].Cells["colOth_defective"].Value.ToString(), _userid, DateTime.Now);
                            }
                        }
                    //}
                    MessageBox.Show("保存成功!");
                }
                else
                    MessageBox.Show("獲取記錄號失敗,不能更新工號表!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            get_prd_records(8);//按記錄號查詢記錄
            FillGrid(); //將查詢到的記錄存入列表
            OperationType = clsUtility.enumOperationType.Save;
            ToolStripButtonEvents();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            OperationType = clsUtility.enumOperationType.Delete;
            ToolStripButtonEvents();
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
            txtTotalQty.Text = "";
            txtOk_weg.Text = "";
            txtNook_weg.Text = "";
            txtOk_qty.Text = "";
            txtNook_qty.Text = "";
            dteProdcutDate.Text = System.DateTime.Now.ToString("yyyy/MM/dd");
            get_last_prd_end_time();//查詢組別當日最後的完成時間，作為開始時間
            get_group_member();//獲取組別的成員
            get_total_prd_qty();//顯示單的總完成數量
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            //OperationType = clsUtility.enumOperationType.Find;
            //ToolStripButtonEvents();


            if (btnFind.Text == "瀏覽(&B)")
            {
                btnFind.Text = "編輯(&B)";
                tabControl1.SelectedIndex = 1;
                edit_type = "N";
            }
            else
            {
                btnFind.Text = "瀏覽(&B)";
                tabControl1.SelectedIndex = 0;
                edit_type = "Y";
            }
            txtBarCode.Focus();
        }

        private void btnSetMemberGroup_Click(object sender, EventArgs e)
        {
            //sent_dep = cmbProductDept.Text;
            //sent_group = cmbGroup.Text;
            //frmPrdMemberGroup frmPrdMemberGroup = new frmPrdMemberGroup();
            //frmPrdMemberGroup.ShowDialog();

        }

        private void btnDefective_Click(object sender, EventArgs e)
        {
            //frmDefective frmDefective = new frmDefective();
            //frmDefective.ShowDialog();
            //loadDefective();
        }

        private void txtmo_id_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendKeys.Send("{tab}");
            }
        }

        private void frmProductionSelect_FormClosed(object sender, FormClosedEventArgs e)
        {
            BarCode.Stop();
        }

        private void txtActual_weg_TextChanged(object sender, EventArgs e)
        {
            if (edit_type == "Y")
            {
                if (Verify.StringValidating(txtkgPCS.Text.Trim(), Verify.enumValidatingType.AllNumber)
                    && Verify.StringValidating(txtActual_weg.Text.Trim(), Verify.enumValidatingType.AllNumber))
                    txtActual_qty.Text = Math.Round(Convert.ToSingle(txtActual_weg.Text) * Convert.ToInt32(txtkgPCS.Text), 0).ToString();
            }
        }

    }
}
