using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using cf_pad.MDL;


namespace cf_pad.CLS
{
    public class clsCheckOutQty
    {
        private static string strConn = DBUtility.dgcf_pad_connectionString;
        private static string remote_tb=DBUtility.remote_db;
        private static string within_code=DBUtility.within_code;
        public static DataTable GetWipDetails(string in_dept, string out_dept, string mo_id, string item, string doc, string seq,string dat1,string dat2)
        {
            DataTable dtMaster = new DataTable();
            try
            {
                string strProc = @"usp_checkoutqty";
                SqlParameter[] paras = new SqlParameter[] {
                   new SqlParameter("@in_dept",in_dept),
                   new SqlParameter("@out_dept",out_dept),
                   new SqlParameter("@mo_id",mo_id),
                   new SqlParameter("@doc",doc),
                   new SqlParameter("@seq",seq),
                   new SqlParameter("@item",item),
                   new SqlParameter("@dat1",dat1),
                   new SqlParameter("@dat2",dat2)
                };

                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = strProc;
                    cmd.Parameters.AddRange(paras);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dtMaster);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtMaster;
        }


        public static DataTable GetWfDetails(int select_type,string vend_id,string mo_id, string item,string doc,string seq, string dat1, string dat2)
        {
            DataTable dtMaster = new DataTable();
            try
            {
                string strProc = @"usp_checkoutqty1";
                SqlParameter[] paras = new SqlParameter[] {
                   new SqlParameter("@select_type",select_type),
                   new SqlParameter("@vend_id",vend_id),
                   new SqlParameter("@mo_id",mo_id),
                   new SqlParameter("@item",item),
                   new SqlParameter("@doc",doc),
                   new SqlParameter("@seq",seq),
                   new SqlParameter("@dat1",dat1),
                   new SqlParameter("@dat2",dat2)
                };

                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = strProc;
                    cmd.Parameters.AddRange(paras);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dtMaster);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtMaster;
        }

        public static DataTable GetWipItem(string out_dep, string in_dep, string mo_id, string mat_item)
        {

            string strSql = "";
            DataTable dtItem = new DataTable();
            strSql = " SELECT b.goods_id " +
                    " FROM " +DBUtility.remote_db + "jo_bill_mostly a " +
                    " Inner Join " + DBUtility.remote_db + "jo_bill_goods_details b ON a.within_code=b.within_code AND a.id=b.id AND a.ver=b.ver" +
                    " Inner Join " + DBUtility.remote_db + "jo_bill_materiel_details c ON b.within_code=c.within_code AND b.id=c.id AND b.ver=c.ver AND b.sequence_id=c.upper_sequence" +
                    " WHERE a.within_code='" + "0000" + "' AND a.mo_id ='" + mo_id + "' AND b.wp_id='" + in_dep +
                    "' AND c.materiel_id='" + mat_item + "'";
            try
            {
                dtItem = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                if (dtItem.Rows.Count == 0)
                    MessageBox.Show("通過條碼提取的物料編號不存在!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtItem;

        }
        
        public static int UpdateCheckOutQty(int upd_flag,List<checkoutqty> detailsList)
        {
            int result = 0;
            string strSql = "";
            string strSql1 = "";
            string within_code = DBUtility.within_code;
            if (upd_flag == 1 || upd_flag == 2 || upd_flag == 3)//更新移交單：1--全部確認；2--分批確認；3--未開外發單
            {
                for (int i = 0; i < detailsList.Count; i++)
                {
                    strSql1 = @" SELECT id FROM jo_mat_check_out1 WHERE within_code='" + within_code + "' AND id='" + detailsList[i].id +
                        "' AND sequence_id='" + detailsList[i].sequence_id + "'";
                    DataTable dtDetail = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql1);
                    if (dtDetail.Rows.Count <= 0)
                    {
                        strSql += string.Format(@"INSERT INTO jo_mat_check_out1( 
                        within_code,id,sequence_id,mo_id,goods_id,wf_id,wf_seq,check_flag,crusr,crtim,vendor_id)
                        VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}',GETDATE(),'{9}') "
                            , within_code, detailsList[i].id, detailsList[i].sequence_id, detailsList[i].mo_id, detailsList[i].goods_id, detailsList[i].wf_id
                            , detailsList[i].wf_seq, detailsList[i].check_flag_wip, detailsList[i].crusr, detailsList[i].vend_id);
                    }
                    else
                    {
                        strSql += string.Format(@"UPDATE jo_mat_check_out1 SET mo_id='{0}',goods_id='{1}',wf_id='{2}',wf_seq='{3}',check_flag='{4}'
                        ,vendor_id='{5}',crusr='{6}',crtim=GETDATE()
                        WHERE within_code='{7}' AND id='{8}' AND sequence_id='{9}' "
                            , detailsList[i].mo_id, detailsList[i].goods_id, detailsList[i].wf_id, detailsList[i].wf_seq, detailsList[i].check_flag_wip
                            , detailsList[i].vend_id, detailsList[i].crusr, within_code, detailsList[i].id, detailsList[i].sequence_id);
                    }
                }
            }
            if (upd_flag == 1 || upd_flag ==2 || upd_flag==4)//更新外發單：1--全部確認；2--分批確認；4--暫停外發
            {
                strSql1 = @" SELECT id FROM jo_mat_check_out2 WHERE within_code='" + within_code + "' AND id='" + detailsList[0].wf_id +
                    "' AND sequence_id='" + detailsList[0].wf_seq + "'";
                DataTable dtHead = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql1);
                if (dtHead.Rows.Count <= 0)
                {
                    strSql += string.Format(@"INSERT INTO jo_mat_check_out2( 
                        within_code,id,sequence_id,check_flag,crusr,crtim)
                        VALUES ('{0}','{1}','{2}','{3}','{4}',GETDATE()) "
                        , within_code, detailsList[0].wf_id, detailsList[0].wf_seq, detailsList[0].check_flag_wf, detailsList[0].crusr);
                }
                else
                {
                    strSql += string.Format(@"UPDATE jo_mat_check_out2 SET check_flag='{0}',crusr='{1}',crtim=GETDATE()
                        WHERE within_code='{2}' AND id='{3}' AND sequence_id='{4}' "
                        , detailsList[0].check_flag_wf, detailsList[0].crusr, within_code, detailsList[0].wf_id, detailsList[0].wf_seq);
                }
            }
            if(strSql!="")
                result = clsPublicOfPad.ExecuteSqlUpdate(strSql);
            return result;

        }


        public static int DeleteCheckOutQty(checkoutqty objDetails)
        {
            int result = 0;
            string strSql = "";
            string within_code = DBUtility.within_code;
            if (objDetails.id != "" && objDetails.sequence_id != "")
            {
                strSql += string.Format(@"DELETE FROM jo_mat_check_out1 WHERE within_code='{0}' AND id='{1}' AND sequence_id='{2}' "
                    , within_code, objDetails.id, objDetails.sequence_id);
            }
            if (objDetails.wf_id != "" && objDetails.wf_seq != "")
            {
                strSql += string.Format(@"DELETE FROM jo_mat_check_out2 WHERE within_code='{0}' AND id='{1}' AND sequence_id='{2}' "
                    , within_code, objDetails.wf_id, objDetails.wf_seq);
            }
            if (strSql != "")
                result = clsPublicOfPad.ExecuteSqlUpdate(strSql);
            return result;

        }

        public static int UpdateIpqc_NG(product_ipqc objModel)
        {
            string strSql = "";
            int result = 0;
            //外發iqc主表
            strSql += string.Format(
                @"Update dbo.op_iqc_mostly Set qc_result='{0}',remark='{1}',final_solution='{2}',check_person='{3}',qc_state='{4}',
                unqualified_iqc_seq='{5}',unqualified_category='{6}' Where within_code='0000' AND id='{7}' ",
                objModel.iqc_result, objModel.qc_remark, objModel.state, objModel.qc_by, objModel.iqc_state,
                    objModel.unqualified_iqc_seq, objModel.unqualified_category, objModel.doc_id);
            //外發iqc從表只更新外觀這一種類型
            //保存明細的處理方法objModel.qc_date
            strSql += string.Format(
                @"Update dbo.op_iqc_details Set disposal_method='{0}' Where within_code='0000' AND id='{1}' AND waster_modality='001' ",
                objModel.qc_date, objModel.doc_id);
            
            //objModel.mat_item 為外發入庫單據號
            strSql += string.Format(
                @"Update dbo.op_outpro_in_detail Set iqc_result='{0}',iqc_state='{1}' Where within_code='0000' AND id='{2}' AND sequence_id='{3}' ",
                objModel.iqc_result, objModel.iqc_state, objModel.mat_item, objModel.sequence_id);
            result = clsPublicOfGeo.ExecuteSqlUpdate(strSql);            
            return result;
        }
        
        public static int UpdateIpqc(product_ipqc objModel)
        {
            int result = 0;
            string strSql = "";
            string gen_no = "";// GenIqcNo(objModel.qc_date);
            gen_no=ExistIpqc(objModel.doc_id, objModel.doc_seq);
            if (gen_no == "")
            {
                //產生QC單號
                string strSql_f = "";
                string id1 = "";
                string bill_id = "QC05";
                string dat = objModel.qc_date;//"2000/01/01";
                string year_month = dat.Substring(2, 2) + dat.Substring(5, 2);
                id1 = "IQC" + year_month;
                strSql_f = "Select bill_code From sys_bill_max_separate Where within_code='" + within_code + "' AND bill_id='" + bill_id +
                "' AND year_month='" + year_month + "'";
                DataTable tbGenNo = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql_f);
                if (tbGenNo.Rows.Count > 0)
                {
                    gen_no = tbGenNo.Rows[0]["bill_code"].ToString();
                    gen_no = id1 + (Convert.ToInt32(gen_no.Substring(7, 5)) + 1).ToString().PadLeft(5, '0');
                    strSql += string.Format(@"Update sys_bill_max_separate Set bill_code='{0}' Where within_code='{1}' AND bill_id='{2}' AND year_month='{3}'"
                        , gen_no, within_code, bill_id, year_month);
                }
                else
                {
                    gen_no = id1 + "00001";
                    strSql += string.Format(@"INSERT INTO sys_bill_max_separate (within_code,bill_id,year_month,bill_code) " +
                        " VALUES ('{0}','{1}','{2}','{3}')"
                        , within_code, bill_id, year_month, gen_no);
                }
                //插入QC記錄主表
                strSql += string.Format(@"INSERT INTO op_iqc_mostly(" +
                    "within_code,id,sequence_id,vendor,vendor_id,goods_id,bill_id,qc_date,issues_qty,check_qty,qc_result,qc_state,update_count,transfers_state" +
                    ",create_date,create_by,update_date,update_by,check_person,adobt_level,state,approved_by,approved_date,check_times,mo_id,remark,final_solution) " +
                    " VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}'" +
                    ",GETDATE(),'{14}',GETDATE(),'{15}', '{16}', '{17}','{18}','{19}',GETDATE(),'{20}','{21}','{22}','{23}')"
                    , within_code, gen_no, objModel.doc_seq, objModel.vendor, objModel.vendor_id, objModel.mat_item
                    , objModel.doc_id, objModel.qc_date, objModel.lot_qty, objModel.lot_qty, objModel.iqc_result, objModel.iqc_state
                    , objModel.update_count, objModel.transfers_state, objModel.crusr, objModel.crusr, objModel.qc_by
                    , objModel.adobt_level, objModel.state, objModel.crusr, objModel.check_times, objModel.mo_no, objModel.qc_remark, objModel.not_ok_rmk);
                //插入QC記錄明細表
                strSql += string.Format(@"INSERT INTO op_iqc_details(" +
                    "within_code,id,sequence_id,waster_modality,transfers_state,check_qty,adopt_standard,aql_standard,aql_sample,accept_qty,reject_qty,disposal_method) " +
                    " VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')"
                    , within_code, gen_no, objModel.sequence_id, objModel.waster_modality, objModel.transfers_state, objModel.check_qty
                    , objModel.adopt_standard, objModel.aql_standard, objModel.aql_sample, objModel.accept_qty, objModel.reject_qty, objModel.not_ok_rmk);
            }
            else
            {
                //更新QC記錄主表
                strSql += string.Format(@"UPDATE op_iqc_mostly SET " +
                    "vendor='{0}',vendor_id='{1}',goods_id='{2}',bill_id='{3}',qc_date='{4}',issues_qty='{5}',check_qty='{6}'"+
                    ",qc_result='{7}',qc_state='{8}',update_count='{9}',transfers_state='{10}'" +
                    ",create_date=GETDATE(),create_by='{11}',update_date=GETDATE(),update_by='{12}',check_person='{13}'" +
                    ",adobt_level='{14}',state='{15}',approved_by='{16}',approved_date=GETDATE(),check_times='{17}'" +
                    ",mo_id='{18}',remark='{19}',final_solution='{20}' " +
                    " WHERE within_code='{21}' AND id='{22}'"
                    , objModel.vendor, objModel.vendor_id, objModel.mat_item
                    , objModel.doc_id, objModel.qc_date, objModel.lot_qty, objModel.lot_qty, objModel.iqc_result, objModel.iqc_state
                    , objModel.update_count, objModel.transfers_state, objModel.crusr, objModel.crusr, objModel.qc_by
                    , objModel.adobt_level, objModel.state, objModel.crusr, objModel.check_times, objModel.mo_no, objModel.qc_remark                    
                    ,objModel.not_ok_rmk, within_code, gen_no);
                //更新QC記錄明細表
                strSql += string.Format(@"UPDATE op_iqc_details SET " +
                    "waster_modality='{0}',transfers_state='{1}',check_qty='{2}',adopt_standard='{3}',aql_standard='{4}'"+
                    ",aql_sample='{5}',accept_qty='{6}',reject_qty='{7}',disposal_method='{8}'" +
                    " WHERE within_code='{9}' AND id='{10}' AND sequence_id='{11}' "
                    , objModel.waster_modality, objModel.transfers_state, objModel.check_qty
                    , objModel.adopt_standard, objModel.aql_standard, objModel.aql_sample
                    , objModel.accept_qty, objModel.reject_qty, objModel.not_ok_rmk, within_code, gen_no, objModel.sequence_id);
            }
            //更新外發收貨記錄表標識
            strSql += string.Format(@"UPDATE op_outpro_in_detail SET iqc_result='{0}',iqc_state='{1}'" +
                " WHERE within_code='{2}' AND id='{3}' AND sequence_id='{4}'"
                , objModel.iqc_result, objModel.iqc_state, within_code, objModel.doc_id, objModel.doc_seq);
            if (strSql != "")
                result = clsPublicOfGeo.ExecuteSqlUpdate(strSql);
            return result;
        }


        public static int CancelIpqc(product_ipqc objModel)
        {
            int result = 0;
            string strSql = "";
            string strSql_f = "";

            strSql_f = String.Format("Select id From op_iqc_mostly Where within_code='{0}' AND id='{1}'", within_code, objModel.qc_doc_id);
            DataTable tbIqc = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql_f);
            if (tbIqc.Rows.Count > 0)
            {
                strSql += string.Format(@"UPDATE op_outpro_in_detail SET iqc_result=null,iqc_state=null" +
                    " WHERE within_code='{0}' AND id='{1}' AND sequence_id='{2}'"
                    , within_code, objModel.doc_id, objModel.doc_seq);
                strSql += string.Format(@"UPDATE " + remote_tb + "op_iqc_mostly SET state='{0}' " +
                    " WHERE within_code='{1}' AND id='{2}'"
                    , objModel.state, within_code, objModel.qc_doc_id);
                if (strSql != "")
                    result = clsPublicOfGeo.ExecuteSqlUpdate(strSql);
            }
            return result;
        }


        private static string ExistIpqc(string doc_id, string doc_seq)
        {
            string result = "";
            string strSql = "";
            strSql = String.Format("Select id From op_iqc_mostly Where within_code='{0}' AND bill_id='{1}' AND sequence_id='{2}' AND state<>'2' ", within_code, doc_id, doc_seq);
            DataTable tbIpqc = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            if (tbIpqc.Rows.Count > 0)
                result = tbIpqc.Rows[0]["id"].ToString();
            return result;
        }
        private static string GenIqcNo(string dat1)
        {
            string strSql="";
            string id1 = "";//, id2 = "";
            string result="";
            string gen_no = "";
            const string bill_id = "QC05";
            string bill_code = "";
            string dat = "2000/01/01";
            string year_month=dat.Substring(2,2)+dat.Substring(5,2);
            id1 = "IQC" + year_month;
            //id2 = id1 + "ZZZZZZZZ";
            //strSql = "Select Max(id) AS max_id From " + remote_tb + "op_iqc_mostly Where within_code='"+
            //    within_code + "' AND id>='" + id1 + "' AND id<='" + id2 + "'";
            //DataTable tbGenNo = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            //if (tbGenNo.Rows.Count > 0)
            //{
            //    gen_no= tbGenNo.Rows[0]["max_id"].ToString();
            //    if (gen_no == "")
            //        result = id1 + "00001";
            //    else
            //        result = id1 + (Convert.ToInt32(gen_no.Substring(7, 5)) + 1).ToString().PadLeft(5, '0');
            //}
            //return result;


            strSql = String.Format("Select bill_code From sys_bill_max_separate Where within_code='{0}' AND bill_id='{1}' AND year_month='{2}'", within_code, bill_id, year_month);
            DataTable tbGenNo = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            if (tbGenNo.Rows.Count > 0)
            {
                gen_no = tbGenNo.Rows[0]["bill_code"].ToString();
                bill_code = id1 + (Convert.ToInt32(gen_no.Substring(7, 5)) + 1).ToString().PadLeft(5, '0');
                strSql = string.Format(@"Update {0}sys_bill_max_separate Set bill_code='{1}' Where within_code='{2}' AND bill_id='{3}' AND year_month='{4}'", remote_tb, bill_code, within_code, bill_id, year_month);
            }
            else
            {
                bill_code = id1 + "00001";
                strSql = string.Format(@"INSERT INTO {0}sys_bill_max_separate (within_code,bill_id,year_month,bill_code) " +
                    " VALUES ('{1}','{2}','{3}','{4}')",remote_tb, within_code, bill_id, year_month, bill_code);
            }
            if (clsPublicOfPad.ExecuteSqlUpdate(strSql) > 0)
                result = bill_code;
            else
                result = "";
            return result;
        }
    }
}
