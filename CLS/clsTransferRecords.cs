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
    public class clsTransferRecords
    {
        /// <summary>
        /// 添加移交單記錄
        /// </summary>
        /// <param name="lsModel"></param>
        /// <returns></returns>
        public static int AddTransferRescords(List<TransferRecords> lsModel)
        {
            int Result = 0;
            try
            {
                string strSql = "";
                for (int i = 0; i < lsModel.Count; i++)
                {
                    DataTable dtIsExist = IsExist(lsModel[i].tran_id, lsModel[i].seq_no);
                    if (dtIsExist.Rows.Count <= 0)
                    {
                        strSql += string.Format(@" INSERT INTO transfer_records
                                                          (tran_id, seq_no, tran_date, in_loc, out_loc
                                                           , prd_mo, prd_item, prd_lot, tran_qty, tran_weg, crusr, crtim)
                                                    VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',{8},{9},'{10}','{11}') "
                                                               , lsModel[i].tran_id, lsModel[i].seq_no, lsModel[i].tran_date, lsModel[i].in_loc, lsModel[i].out_loc
                                                               , lsModel[i].prd_mo, lsModel[i].prd_item, lsModel[i].prd_lot, lsModel[i].tran_qty, lsModel[i].tran_weg, lsModel[i].crusr, lsModel[i].crtim);
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
                    MessageBox.Show("你要保存的數據已存在，請重新選擇要保存的數據");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        /// <summary>
        ///根據條件判斷移交數據是否已存在 
        /// </summary>
        /// <param name="pTrans_id">移交單號</param>
        /// <param name="pSeq_no">序號</param>
        /// <returns></returns>
        public static DataTable IsExist(string pTrans_id, string pSeq_no)
        {
            DataTable dtIsExist = new DataTable();

            try
            {
                string strSql = @" SELECT tran_id, seq_no FROM transfer_records WHERE tran_id='" + pTrans_id + "' AND seq_no='" + pSeq_no + "' ";
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
        /// 獲取移交單記錄
        /// </summary>
        /// <param name="pIn_dept">收貨部門</param>
        /// <param name="pTrans_id">移交單號</param>
        /// <param name="pTran_date">移交日期</param>
        /// <returns></returns>
        public static DataTable GetTransferInfo(string pIn_dept, string pTrans_id, string pTran_date)
        {
            DataTable dtTransInfo = new DataTable();
            try
            {
                string strSql = @"SELECT  a.id,a.con_date,a.in_dept,a.out_dept,b.goods_id,b.sequence_id
                                        ,c.name as goods_desc,b.mo_id,b.lot_no,CONVERT(decimal(28,0), b.con_qty)as con_qty ,CONVERT(decimal(28,2), b.sec_qty) as sec_qty
                                FROM jo_materiel_con_mostly AS a with(nolock)
                                LEFT JOIN jo_materiel_con_details b with(nolock) on a.within_code=b.within_code and a.id=b.id 
                                LEFT JOIN it_goods c on b.goods_id=c.id ";

                if (pIn_dept != "")
                {
                    strSql += " WHERE a.in_dept='" + pIn_dept + "' ";

                    if (pTrans_id != "")
                    {
                        strSql += " AND a.id='" + pTrans_id + "' ";
                    }

                    if (pTran_date != "")
                    {
                        strSql += " AND a.con_date='" + pTran_date + "' ";
                    }
                }
                else
                {
                    if (pTrans_id != "")
                    {
                        strSql += " WHERE a.id='" + pTrans_id + "' ";

                        if (pTran_date != "")
                        {
                            strSql += " AND a.con_date='" + pTran_date + "' ";
                        }
                    }
                    else
                    {
                        if (pTran_date != "")
                        {
                            strSql += " WHERE a.con_date='" + pTran_date + "' ";
                        }
                    }
                }

                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtTransInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtTransInfo;
        }
    }
}
