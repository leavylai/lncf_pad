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
    public class clsPrdTransfer
    {
        /// <summary>
        /// 查詢已確認收貨的移交單
        /// </summary>
        /// <param name="pMo_id"></param>
        /// <returns></returns>
        public static DataTable GetTransferInfo(string pMo_id)
        {
            DataTable dtTransfer = new DataTable();
            try
            {
                string strSql = @"SELECT distinct a.mo_id ,a.id ,a.sequence_id ,a.goods_id ,a.lot_no
                                         ,a.con_qty ,a.sec_qty ,a.package_num ,a.actual_qty ,a.actual_weg
                                         ,a.actual_pack ,convert(varchar(10),b.con_date,111)as con_date ,b.out_dept ,b.in_dept ,c.name as goods_name
                                  FROM jo_materiel_con_details as a
                                  INNER JOIN jo_materiel_con_mostly b on a.within_code=b.within_code and a.id=b.id
                                  LEFT JOIN dgsql2.dgcf_db.dbo.geo_it_goods c on a.goods_id=c.id  COLLATE Chinese_Taiwan_Stroke_CI_AS ";

                strSql += " WHERE a.mo_id='" + pMo_id + "' ";

                dtTransfer = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                //DataTable dtHadPrint = dtTemp.Copy();
                //DataTable dtNoPrint = dtTemp.Copy();
                //dtHadPrint.Rows.Clear();
                //dtNoPrint.Rows.Clear();
                //DataTable dtMoStatePrint=null;// clsMoStatePrint.GetMoStatePrintForTrans(pOut_dept, pIn_dept);
                //if (dtTemp.Rows.Count > 0 && dtMoStatePrint.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dtTemp.Rows.Count; i++)
                //    {
                //        bool IsRepeat = false;
                //        for (int j = 0; j < dtMoStatePrint.Rows.Count; j++)
                //        {
                //            if (dtTemp.Rows[i]["out_dept"].ToString() == dtMoStatePrint.Rows[j]["wp_id"].ToString() &&
                //                dtTemp.Rows[i]["mo_id"].ToString() == dtMoStatePrint.Rows[j]["mo_id"].ToString() &&
                //                dtTemp.Rows[i]["goods_id"].ToString() == dtMoStatePrint.Rows[j]["goods_id"].ToString() &&
                //                dtTemp.Rows[i]["in_dept"].ToString() == dtMoStatePrint.Rows[j]["next_wp_id"].ToString())
                //            {
                //                dtHadPrint.Rows.Add(dtTemp.Rows[i].ItemArray);
                //                IsRepeat = true;
                //            }
                //            else
                //            {
                //                if (j == dtMoStatePrint.Rows.Count - 1 && IsRepeat == false)
                //                {
                //                    dtNoPrint.Rows.Add(dtTemp.Rows[i].ItemArray);
                //                }
                //            }
                //        }
                //    }
                //}

                //if (IsPrint)  //顯示是否已打印數據
                //{
                //    dtTransfer = dtHadPrint;
                //}
                //else
                //{
                //    dtTransfer = dtNoPrint;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtTransfer;
        }

        /// <summary>
        /// 保存移交單記錄
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        public static int AddTransferInfo(jo_materiel_con_mostly objModel)
        {
            int Result = 0;
            try
            {
                string strSql = "";

                //先檢查主表是否已存在此條記錄，不存在則插入
                DataTable dtMostly = IsExistMostly(objModel.id);
                if (dtMostly.Rows.Count <= 0)
                {
                    strSql += string.Format(@" INSERT INTO jo_materiel_con_mostly 
                                                           (id, con_date, out_dept, in_dept, stock_type, state, crusr, crtim)
                                                     VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}') "
                                                          , objModel.id, objModel.con_date, objModel.out_dept, objModel.in_dept, objModel.stock_type, objModel.state
                                                          , objModel.crusr, objModel.crtim);

                    //查明細表插入
                    for (int i = 0; i < objModel.lsDetails.Count; i++)
                    {
                        strSql += string.Format(@" INSERT INTO jo_materiel_con_details
                                                               (id, sequence_id, mo_id, goods_id, lot_no, con_qty, sec_qty
                                                                , package_num, actual_qty, actual_weg, actual_pack, crusr, crtim)
                                                         VALUES('{0}','{1}','{2}','{3}','{4}',{5},{6},{7},{8},{9},{10},'{11}','{12}') "
                                                             , objModel.lsDetails[i].id, objModel.lsDetails[i].sequence_id, objModel.lsDetails[i].mo_id, objModel.lsDetails[i].goods_id
                                                             , objModel.lsDetails[i].lot_no, objModel.lsDetails[i].con_qty, objModel.lsDetails[i].sec_qty, objModel.lsDetails[i].package_num
                                                             , objModel.lsDetails[i].actual_qty, objModel.lsDetails[i].actual_weg, objModel.lsDetails[i].actual_pack, objModel.lsDetails[i].crusr, objModel.lsDetails[i].crtim);
                    }
                }

                if (strSql != "")
                {
                    using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = strSql;
                        Result = cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    MessageBox.Show("此移交單號已存在，請重新選擇要保存的數據");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        /// <summary>
        /// 儲存移交單明細記錄
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        public static int SaveTransferDetails(jo_materiel_con_details objModel)
        {
            int Result = 0;
            try
            {
                string strSql = "";

                //檢查是否已過數到系統，若是，則不能再儲存
                DataTable dtMostly = IsExistMostly(objModel.id);
                if (dtMostly.Rows.Count > 0)
                {
                    if (dtMostly.Rows[0]["imput_flag"].ToString() == "Y")
                    {
                        MessageBox.Show("此移交單已導入系統,不能再儲存!");
                        Result = 0;
                        return Result;
                    }
                }
                DataTable dtDetails = IsExistDetails(objModel.id, objModel.sequence_id, objModel.mo_id);
                if (dtDetails.Rows.Count <= 0)
                {
                    strSql = " INSERT INTO jo_materiel_con_details (within_code,id, sequence_id, mo_id, goods_id, lot_no, con_qty, sec_qty " +
                              ",package_num, actual_qty, actual_weg, actual_pack,sample_no,sample_weg,dif_qty,dif_weg,dif_flag,conf_flag, crusr, crtim) " +
                              " VALUES (@within_code,@id,@sequence_id,@mo_id,@goods_id,@lot_no,@con_qty,@sec_qty " +
                              ",@package_num,@actual_qty,@actual_weg,@actual_pack,@sample_no,@sample_weg,@dif_qty,@dif_weg,@dif_flag,@conf_flag,@crusr,@crtim)";
                }
                else
                {
                    strSql = " UPDATE jo_materiel_con_details SET mo_id=@mo_id,goods_id=@goods_id,lot_no=@lot_no,con_qty=@con_qty,sec_qty=@sec_qty " +
                              ",package_num=@package_num,actual_qty=@actual_qty,actual_weg=@actual_weg,actual_pack=@actual_pack" +
                              ",sample_no=@sample_no,sample_weg=@sample_weg,dif_qty=@dif_qty,dif_weg=@dif_weg,dif_flag=@dif_flag,conf_flag=@conf_flag,crusr=@crusr,crtim=@crtim " +
                              " Where within_code=@within_code and id=@id and sequence_id=@sequence_id";
                }
                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    conn.Open();
                    SqlParameter[] paras = new SqlParameter[]{
                        new SqlParameter("@within_code","0000"),
                        new SqlParameter("@id",objModel.id),
                        new SqlParameter("@sequence_id",objModel.sequence_id),
                        new SqlParameter("@mo_id",objModel.mo_id),
                        new SqlParameter("@goods_id",objModel.goods_id),
                        new SqlParameter("@lot_no",objModel.lot_no),
                        new SqlParameter("@con_qty",Convert.ToInt32(objModel.con_qty)),
                        new SqlParameter("@sec_qty",Math.Round(objModel.sec_qty,2)),
                        new SqlParameter("@package_num",objModel.package_num),
                        new SqlParameter("@actual_qty",objModel.actual_qty),
                        new SqlParameter("@actual_weg",objModel.actual_weg),
                        new SqlParameter("@actual_pack",objModel.actual_pack),
                        new SqlParameter("@sample_no",objModel.sample_no),
                        new SqlParameter("@sample_weg",objModel.sample_weg),
                        new SqlParameter("@conf_flag",objModel.conf_flag),
                        new SqlParameter("@dif_qty",objModel.dif_qty),
                        new SqlParameter("@dif_weg",objModel.dif_weg),
                        new SqlParameter("@dif_flag",objModel.dif_flag),
                        new SqlParameter("@crusr",objModel.crusr),
                        new SqlParameter("@crtim",objModel.crtim)
                    };
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = strSql;
                    cmd.Parameters.AddRange(paras);
                    Result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        /// <summary>
        /// 檢查是否已錄入完成明細記錄,並更新主表記錄
        /// </summary>
        /// <param name="save_type"></param>
        /// <param name="id"></param>
        /// <param name="usr"></param>
        /// <returns></returns>
        public static int SaveTransferMostly(int save_type, string id,string in_dept,string out_dept,string usr)
        {
            int Result = 0;
            try
            {
                string strProc = @"TransferMostlyConf ";
                SqlParameter[] paras = new SqlParameter[] {
                    new SqlParameter("@save_type",save_type),    
                    new SqlParameter("@id",id),
                    new SqlParameter("@in_dept",in_dept),
                    new SqlParameter("@out_dept",out_dept),
                    new SqlParameter("@usr",usr)
                };
                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = strProc;
                    cmd.Parameters.AddRange(paras);
                    cmd.CommandType = CommandType.StoredProcedure;
                    Result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return Result;
        }

        /// <summary>
        /// 存儲合併單據集合
        /// </summary>
        /// <param name="detailsList"></param>
        /// <returns></returns>
        public static int SaveTransferDetails(List<jo_materiel_con_details> detailsList, int pMerge_id)
        {
            int Result = 0;
            string strSql = "";
            try
            {
                for (int i = 0; i < detailsList.Count; i++)
                {

                    DataTable dtDetails = IsExistDetails(detailsList[i].id, detailsList[i].sequence_id, detailsList[i].mo_id);
                    if (dtDetails.Rows.Count <= 0)
                    {
                        strSql += string.Format(@"INSERT INTO jo_materiel_con_details( 
                                                                  within_code,id, sequence_id, mo_id, goods_id, lot_no
                                                                  , con_qty, sec_qty ,package_num, actual_qty, actual_weg, actual_pack
                                                                  , sample_no, sample_weg, conf_flag, crusr, crtim, merge_id, total_qty, total_weg) 
                                                        VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}' 
                                                                ,'{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}',GETDATE(),'{16}','{17}','{18}') "
                                                            , "0000", detailsList[i].id, detailsList[i].sequence_id, detailsList[i].mo_id, detailsList[i].goods_id, detailsList[i].lot_no
                                                            , detailsList[i].con_qty, detailsList[i].sec_qty, detailsList[i].package_num, detailsList[i].actual_qty, detailsList[i].actual_weg
                                                            , detailsList[i].actual_pack, detailsList[i].sample_no, detailsList[i].sample_weg, detailsList[i].conf_flag, detailsList[i].crusr
                                                            , pMerge_id, detailsList[i].total_qty, detailsList[i].total_weg);
                    }
                }

                if (strSql != "")
                {
                    using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = strSql;
                        Result = cmd.ExecuteNonQuery();
                    }

                    //if (Result > 0)
                    //{
                    //    for (int i = 0; i < detailsList.Count; i++)
                    //    {
                    //        int MostlyResult = SaveTransferMostly(1, detailsList[i].id, detailsList[i].crusr);
                    //    }
                    //}
                }
                else
                {
                    MessageBox.Show("單據已收貨，不能重複收貨！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return Result;
        }

        /// <summary>
        /// 刪除移交單明細
        /// </summary>
        /// <param name="cancelType"></param>
        /// <param name="id"></param>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static int DelTransferDetails(int cancelType, string id, string seq)
        {
            int Result = 0;
            try
            {
                string strSql = "";
                //檢查是否已過數到系統，若是，則不能再儲存
                DataTable dtDetails = IsExisMateriel_con_details(id, seq);
                if (dtDetails.Rows.Count > 0)
                {
                    if (dtDetails.Rows[0]["imput_flag"].ToString() == "Y")
                    {
                        MessageBox.Show("此筆記錄已導入系統,不能再取消!");
                        Result = 0;
                        return Result;
                    }
                }
                //cancelType=1取消收貨，2--取消圍數
                if (cancelType == 1)
                    strSql = " DELETE FROM  jo_materiel_con_details ";
                else
                    strSql = " UPDATE jo_materiel_con_details SET conf_flag='' ";
                strSql += " WHERE within_code=@within_code AND id=@id AND sequence_id=@seq ";
                strSql += " UPDATE jo_materiel_con_mostly SET id_state='' WHERE within_code=@within_code AND id=@id ";
                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    conn.Open();
                    SqlParameter[] paras = new SqlParameter[]{
                        new SqlParameter("@within_code","0000"),
                        new SqlParameter("@id",id),
                        new SqlParameter("@seq",seq)
                    };
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = strSql;
                    cmd.Parameters.AddRange(paras);
                    Result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        /// <summary>
        ///根據條件判斷移交單主表數據是否已存在 
        /// </summary>
        /// <param name="pTrans_id">移交單號</param>
        /// <returns></returns>
        /// 提取移交單主表中的記錄
        public static DataTable IsExistMostly(string pTrans_id)
        {
            DataTable dtIsExist = new DataTable();

            try
            {
                string strSql = @" SELECT id,imput_flag FROM jo_materiel_con_mostly WHERE within_code='" + "0000" + "'" + " and id='" + pTrans_id + "' ";
                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtIsExist);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtIsExist;
        }
        //檢查明細表中記錄是否已匯入Geo系統：imput_flag="Y"
        public static DataTable IsExisMateriel_con_details(string pTrans_id, string seq)
        {
            DataTable dtIsExist = new DataTable();

            try
            {
                string strSql = @" SELECT id,imput_flag FROM jo_materiel_con_details WHERE within_code='" + "0000" +"'" +
                    " and id='" + pTrans_id + "' " + " and sequence_id='" + seq + "' ";
                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtIsExist);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtIsExist;
        }
        /// <summary>
        ///根據條件判斷移交單明細表數據是否已存在 
        /// </summary>
        /// <param name="pTrans_id">移交單號</param>
        /// <param name="pSeq_no">序號</param>
        /// <returns></returns>
        public static DataTable IsExistDetails(string pTrans_id, string pSeq_no, string pMo_id)
        {
            DataTable dtIsExist = new DataTable();

            try
            {
                string strSql = @" SELECT id, sequence_id FROM jo_materiel_con_details WHERE id='" + pTrans_id + "' AND sequence_id='" + pSeq_no + "' AND mo_id='" + pMo_id + "'";
                using (SqlConnection conn = new SqlConnection(DBUtility.dgcf_pad_connectionString))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtIsExist);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtIsExist;
        }

        /// <summary>
        /// 獲取移交單主表信息
        /// </summary>
        /// <param name="pIn_dept">收貨部門</param>
        /// <param name="pTrans_id">移交單號</param>
        /// <param name="pTran_date">移交日期</param>
        /// <returns></returns>
        public static DataTable GetTransferMostly(string pTrans_id, string pIn_dept, string pOut_dept, string pTran_date_from, string pTran_date_to, int pQueryType)
        {
            DataTable dtMaster = new DataTable();
            try
            {
                #region SQL 語句
                //  string strSql = @" SELECT a.id,a.con_date,a.out_dept,a.in_dept,a.check_date,a.check_by,a.create_date,a.create_by
                //                          ,a.remark,a.state,a.handler,a.update_by,a.update_count,a.update_date,a.bill_origin,a.stock_type
                //                     FROM jo_materiel_con_mostly a  ";

                //  if (pQueryType == 0)
                //  {
                //      strSql += " WHERE a.state<> 0 ";
                //      strSql += " and a.id not in(select id from dgsql2.dgcf_pad.dbo.jo_materiel_con_mostly ) ";
                //  }
                //  if (pQueryType == 1)
                //  {
                //      strSql += " RIGHT JOIN dgsql2.dgcf_pad.dbo.jo_materiel_con_mostly b on a.id=b.id ";
                //      strSql += " WHERE a.state<> 0 ";
                //  }
                //  if (pQueryType == 2)
                //  {
                //      strSql += " WHERE a.state<> 0 ";
                //  }

                //  if (pTrans_id != "")
                //  {
                //      strSql += " AND a.id like '%" + pTrans_id + "%' ";
                //  }
                //  else
                //  {
                //      if (pIn_dept != "")
                //      {
                //          strSql += " AND a.in_dept='" + pIn_dept + "' ";
                //      }
                //      if (pOut_dept != "")
                //      {
                //          strSql += " AND a.out_dept='" + pOut_dept + "' ";
                //      }
                //  }
                //  if (pTran_date != "//")
                //  {
                //      strSql += " AND a.con_date='" + pTran_date + "' ";
                //  }
                #endregion

                string strProc = @"QueryTransferMostly ";
                SqlParameter[] paras = new SqlParameter[] {
                   new SqlParameter("@Trans_id",pTrans_id),
                   new SqlParameter("@In_dept",pIn_dept),
                   new SqlParameter("@Out_dept",pOut_dept),
                   new SqlParameter("@Con_date_from",pTran_date_from),
                   new SqlParameter("@Con_date_to",pTran_date_to),
                   new SqlParameter("@QueryType",pQueryType)
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

        public static DataTable GetTransferDetails(int select_type, string in_dept, string out_dept, string mo_id, string doc, string seq, string item, string fdat, string tdat, string merge_id)
        {
            DataTable dtMaster = new DataTable();
            try
            {
                string strProc = @"TransferMostlyQuery ";
                SqlParameter[] paras = new SqlParameter[] {
                   new SqlParameter("@select_type",select_type),
                   new SqlParameter("@in_dept",in_dept),
                   new SqlParameter("@out_dept",out_dept),
                   new SqlParameter("@mo_id",mo_id),
                   new SqlParameter("@doc",doc),
                   new SqlParameter("@seq",seq),
                   new SqlParameter("@item",item),
                   new SqlParameter("@fdat",fdat),
                   new SqlParameter("@tdat",tdat),
                   new SqlParameter("@merge_id",merge_id)
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

        /// <summary>
        /// 獲取移交單明細表信息
        /// </summary>
        /// <param name="pTrans_id"></param>
        /// <returns></returns>
        public static DataTable GetTransferDetails(string pTrans_id)
        {
            DataTable dtDetails = new DataTable();
            try
            {
                string strSql = @" SELECT  a.id,a.sequence_id,a.jo_id,a.goods_id,CONVERT(decimal(28,0), a.con_qty)as con_qty,a.unit_code,a.remark
                                           ,CONVERT(decimal(28,2), a.sec_qty) as sec_qty,a.sec_unit,a.mo_id,CONVERT(decimal(28,0), a.package_num)as package_num,a.lot_no,b.name as goods_desc
                                           ,CONVERT(decimal(28,0), a.con_qty)as actual_qty,CONVERT(decimal(28,2), a.sec_qty) as actual_weg,CONVERT(decimal(28,0), a.package_num)as actual_pack
                                    FROM jo_materiel_con_details as a with(nolock)
                                    LEFT OUTER JOIN it_goods b on a.goods_id=b.id
                                    WHERE a.within_code='0000'" + " and a.id='" + pTrans_id + "' ";

                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtDetails);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtDetails;
        }

        /// <summary>
        /// 獲取生產單信息
        /// </summary>
        /// <param name="pMo_id"></param>
        /// <param name="pGoods_id"></param>
        /// <returns></returns>
        public static DataTable GetPOData(string pMo_id, string pWp_id, string pMater_id)
        {
            DataTable dtPO = new DataTable();
            try
            {
                string strSql = @" SELECT  b.wp_id ,'' as prd_dept_name ,a.mo_id ,b.goods_id ,c.name as goods_name ,convert(varchar(10),b.t_complete_date,111)as t_complete_date
                                           , b.next_wp_id ,d.name AS next_wp_name ,convert(DECIMAL(18,0),b.order_qty)as order_qty ,convert(DECIMAL(18,0),b.prod_qty)as prod_qty ,b.goods_unit
                                           , a.customer_id ,a.remark ,f.production_remark ,g.do_color ,g.name as color_name,'' as picture_name ,a.ver ,b.sequence_id ,'' as BarCode
                                    FROM  dbo.jo_bill_mostly a with(nolock)
                                    INNER JOIN dbo.jo_bill_goods_details b with(nolock)ON  a.within_code = b.within_code AND  a.id = b.id AND  a.ver = b.ver
                                    INNER JOIN dbo.Jo_bill_materiel_details m with(nolock)ON b.within_code=m.within_code and b.id=m.id and b.ver=m.ver and b.sequence_id=m.upper_sequence
                                    INNER JOIN dbo.it_goods c with(nolock) ON  b.within_code = c.within_code AND  b.goods_id = c.id
                                    INNER JOIN dbo.cd_department d ON b.within_code=d.within_code And b.next_wp_id=d.id
                                    LEFT OUTER JOIN dbo.so_order_details e with(nolock) ON a.within_code=e.within_code AND a.mo_id=e.mo_id AND a.so_sequence_id=e.sequence_id
                                    LEFT OUTER JOIN dbo.so_order_special_info f with(nolock) ON e.within_code=f.within_code AND e.id=f.id AND e.sequence_id=f.upper_sequence AND e.ver=f.ver
                                    LEFT OUTER JOIN cd_color g with(nolock) on c.within_code=g.within_code and c.color=g.id
                                    WHERE a.mo_id='" + pMo_id + "'AND b.wp_id='" + pWp_id + "' AND m.materiel_id='" + pMater_id + "'";

                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtPO);
                }
                if (dtPO.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPO.Rows.Count; i++)
                    {
                        dtPO.Rows[i]["prd_dept_name"] = GetDept_name(dtPO.Rows[i]["wp_id"].ToString());  //獲得部門名稱

                        //取得圖片
                        DataTable dtArt = clsMo_for_jx.GetGoods_ArtWork(dtPO.Rows[i]["goods_id"].ToString());
                        if (dtArt.Rows.Count > 0)
                        {
                            dtPO.Rows[i]["picture_name"] = dtArt.Rows[0]["picture_name"];
                        }

                        //生成條碼內容
                        dtPO.Rows[i]["BarCode"] = clsMo_for_jx.ReturnBarCode(dtPO.Rows[i]["mo_id"].ToString() + "0" + dtPO.Rows[i]["ver"].ToString() + dtPO.Rows[i]["sequence_id"].ToString().Substring(2, 2));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtPO;
        }

        /// <summary>
        /// 獲取配件名稱及顏色
        /// </summary>
        /// <param name="pmo_id"></param>
        /// <returns></returns>
        public static DataTable GetPartsOfColor(string pmo_id)
        {
            DataTable dtParts = new DataTable();
            try
            {
                string strSql = @" SELECT distinct a.goods_id as part_goods_id ,b.name as part_goods_name ,c.picture_name ,e.name as color_name ,'' as Ser_no ,'' as mo_id
                                   FROM so_order_details d with(nolock)
                                   INNER JOIN  so_order_bom a with(nolock) on d.id=a.id and d.sequence_id = a.upper_sequence
                                   INNER JOIN it_goods b with(nolock) on a.within_code=b.within_code and a.goods_id=b.id
                                   LEFT OUTER JOIN dbo.cd_pattern_details c with(nolock) ON b.within_code=c.within_code AND b.blueprint_id=c.id
                                   LEFT OUTER JOIN dbo.cd_color e with(nolock) ON b.within_code=e.within_code and  b.color = e.id
                                   WHERE d.mo_id='" + pmo_id + "'";
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtParts);
                }
                if (dtParts.Rows.Count > 0)
                {
                    //過濾重複數據
                    DataView myDataView = new DataView(dtParts);
                    //此处可加任意数据项组合  
                    string[] strComuns = { "part_goods_id", "part_goods_name", "picture_name", "color_name", "Ser_no", "mo_id" };
                    dtParts = myDataView.ToTable(true, strComuns);

                    //添加序號
                    for (int i = 0; i < dtParts.Rows.Count; i++)
                    {
                        dtParts.Rows[i]["Ser_no"] = (i + 1);
                        dtParts.Rows[i]["mo_id"] = pmo_id;
                        dtParts.Rows[i]["part_goods_name"] = dtParts.Rows[i]["part_goods_name"] + "|" + dtParts.Rows[i]["color_name"];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtParts;
        }

        /// <summary>
        ///根據部門ID,取得部門名稱 
        /// </summary>
        /// <param name="pDept_id"></param>
        /// <returns></returns>
        public static string GetDept_name(string pDept_id)
        {
            string Dept_name = "";
            try
            {
                DataTable dtDept = new DataTable();
                string strSql = @" SELECT id,name FROM cd_department WHERE id ='" + pDept_id + "'";
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtDept);
                }
                if (dtDept.Rows.Count > 0)
                {
                    Dept_name = dtDept.Rows[0]["name"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Dept_name;
        }


        /// <summary>
        /// 獲取所有部門信息
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllDepartment()
        {
            DataTable dtDept = new DataTable();
            try
            {
                string strSql = @" SELECT id,name FROM cd_department";
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtDept);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtDept;
        }
        
    }
}
