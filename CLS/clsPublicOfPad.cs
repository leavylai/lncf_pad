using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using cf_pad.MDL;

namespace cf_pad.CLS
{
    public class clsPublicOfPad
    {
        private static string strConn = DBUtility.dgcf_pad_connectionString;
        private static string remote_db = DBUtility.remote_db;
        public static int GenNo(string gen_id)
        {
            int gen_no = 0;
            DataTable dtGenNo = ExecuteSqlReturnDataTable("Select gen_no from gen_no where gen_id=" + "'" + gen_id + "'");
            if (dtGenNo.Rows.Count > 0)
            {
                if (updGenNo(1, gen_id) > 0)
                    gen_no = (int)dtGenNo.Rows[0]["gen_no"];
                else
                    gen_no = 0;
            }
            else
            {
                if (updGenNo(0, gen_id) > 0)
                    gen_no = 1;
                else
                    gen_no = 0;
            }
            return gen_no;
        }
        private static int updGenNo(int upd_type, string gen_id)
        {
            int Result = 0;
            try
            {
                string strSql = "";
                if (upd_type == 1)
                    strSql = "Update gen_no Set gen_no=gen_no+1 Where gen_id=@gen_id";
                else
                    strSql = "Insert Into gen_no (gen_id,gen_no) Values (@gen_id,2)";
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlParameter[] paras = new SqlParameter[]{
                        new SqlParameter("@gen_id",gen_id)
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
        ///執行存儲過程，返回dataTable 
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static DataTable ExecuteProcedure(string strSql, SqlParameter[] paras)
        {
            DataTable dtData = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = strSql;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(paras);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dtData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtData;
        }

        /// <summary>
        /// 執行SQL，返回 dataTable 類型
        /// </summary>
        /// <returns></returns>
        public static DataTable ExecuteSqlReturnDataTable(string strSQL)
        {
            DataTable dtData = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSQL, conn);
                    sda.Fill(dtData);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            return dtData;
        }

        //執行SQL語句或儲存過程，不返回值
        public static int ExecuteNonQuery(string strSql, SqlParameter[] paras, bool isProce)
        {
            int Result = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = strSql;
                    if (paras != null)
                    {
                        cmd.Parameters.AddRange(paras);
                    }
                    if (isProce)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                    }
                    Result = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        public static int ExecuteSqlUpdate(string strSql)
        {
            int result=0;
            SqlConnection conn = new SqlConnection(strConn);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = strSql;
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            return result;
        }
        /// <summary>
        /// 執行增、刪、改 SQL，返回 int 類型
        /// </summary>
        /// <returns></returns>
        public static int ExecuteNonQueryReturnInt(string strSql, SqlParameter[] paras)
        {
            int Result = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
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
        /// 執行存儲過程，返回DataSet
        /// </summary>
        /// <param name="proce"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static DataSet ExecuteProcedureReturnDataSet(string proce, SqlParameter[] paras, string TableName)
        {
            DataSet ds = new DataSet();
            try
            {
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = proce;
                    if (paras != null)
                    {
                        cmd.Parameters.AddRange(paras);
                    }
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    if (!string.IsNullOrEmpty(TableName))
                    {
                        sda.Fill(ds, TableName);
                    }
                    else
                    {
                        sda.Fill(ds);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return ds;
        }


        //通過BarCode查找物料編號、部門等資料
        public static DataTable BarCodeToItem(string org_barcode)
        {

            DataTable dtBarCode = new DataTable();
            dtBarCode.Columns.Add("barcode_type", typeof(string));
            dtBarCode.Columns.Add("mo_id", typeof(string));
            dtBarCode.Columns.Add("goods_id", typeof(string));
            dtBarCode.Columns.Add("wp_id", typeof(string));
            dtBarCode.Columns.Add("next_wp_id", typeof(string));
            dtBarCode.Columns.Add("doc_id", typeof(string));
            dtBarCode.Columns.Add("doc_seq", typeof(string));
            dtBarCode.Columns.Add("prod_qty", typeof(string));
            dtBarCode.Columns.Add("id", typeof(string));
            dtBarCode.Columns.Add("ver", typeof(int));
            dtBarCode.Columns.Add("sequence_id", typeof(string));
            DataRow dr = dtBarCode.NewRow();
            string barcode = org_barcode.Trim();
            int barcode_length = barcode.Length;

            if (barcode_length >= 13)
            {
                if (barcode_length == 16 || barcode_length == 17)//移交單的條碼/17為無生產流程的退單的條碼(RW)17
                {
                    if (barcode.Substring(0, 2) == "RW" || barcode.Substring(0, 2) == "TW")//無生產流程的退單貨無生產流程的移交單
                    {

                        dr["doc_id"] = barcode.Substring(0, 13);
                        dr["doc_seq"] = barcode.Substring(13, 4) + "h";
                    }
                    else//條形碼按單據編號查詢
                    {
                        dr["doc_id"] = barcode.Substring(0, 12);
                        dr["doc_seq"] = barcode.Substring(12, 4) + "h";
                    }
                    dr["barcode_type"] = "11";
                }
                else
                {
                    if ((barcode_length == 15 && barcode.Substring(0, 3) == "DAA") || (barcode_length == 15 && barcode.Substring(0, 3) == "DAB"))//倉庫發貨的條碼
                    {
                        dr["doc_id"] = barcode.Substring(0, 11);//貨倉發貨：條形碼按單據編號查詢
                        dr["doc_seq"] = barcode.Substring(11, 4) + "h";
                        dr["barcode_type"] = "12";
                    }
                    else//按制單編號查詢的條碼
                    {
                        string strSql = "";
                        string mo_id = "";
                        string seq_id = "";
                        DataTable dtItem = new DataTable();
                        mo_id = barcode.Substring(0, 9);
                        seq_id = "00" + barcode.Substring(11, 2) + "h";
                        if (barcode_length == 13)
                            strSql = " SELECT a.mo_id,b.goods_id,b.wp_id,b.next_wp_id,Convert(INT,b.prod_qty) AS prod_qty,a.id,a.ver,b.sequence_id" +
                                " FROM " + remote_db + "jo_bill_mostly a " +
                                    " Inner Join " + remote_db + "jo_bill_goods_details b ON a.within_code=b.within_code AND a.id=b.id AND a.ver=b.ver" +
                                           " WHERE a.within_code='" + "0000" + "' AND a.mo_id ='" + mo_id + "' " + " AND b.sequence_id='" + seq_id + "'";
                        else
                        {
                            string seq_id_part = "";
                            seq_id_part = "1000" + barcode.Substring(13, 2) + "h";
                            strSql = " SELECT a.mo_id,c.materiel_id AS goods_id,c.location AS wp_id,b.wp_id AS next_wp_id,Convert(INT,b.prod_qty) AS prod_qty,a.id,a.ver,b.sequence_id " +
                                    " FROM " +
                                    remote_db + "jo_bill_mostly a " +
                                    " Inner Join " + remote_db + "jo_bill_goods_details b ON a.within_code=b.within_code AND a.id=b.id AND a.ver=b.ver" +
                                    " Inner Join " + remote_db + "jo_bill_materiel_details c ON b.within_code=c.within_code AND b.id=c.id AND b.ver=c.ver AND b.sequence_id=c.upper_sequence" +
                                    " WHERE a.within_code='" + "0000" + "' AND a.mo_id ='" + mo_id + "' " + " AND b.sequence_id='" + seq_id + "'" + " AND c.sequence_id='" + seq_id_part + "'";
                        }
                        try
                        {
                            dtItem = ExecuteSqlReturnDataTable(strSql);
                            if (dtItem.Rows.Count == 0)
                                MessageBox.Show("通過條碼提取的物料編號不存在!");
                            else
                            {
                                DataRow pdr = dtItem.Rows[0];
                                dr["barcode_type"] = "2";//條形碼從計劃單中獲取制單資料
                                dr["mo_id"] = pdr["mo_id"].ToString();
                                dr["goods_id"] = pdr["goods_id"].ToString();
                                dr["wp_id"] = pdr["wp_id"].ToString();
                                dr["next_wp_id"] = pdr["next_wp_id"].ToString();
                                dr["prod_qty"] = pdr["prod_qty"].ToString();
                                dr["id"] = pdr["id"].ToString();
                                dr["ver"] = Convert.ToInt32(pdr["ver"]);
                                dr["sequence_id"] = pdr["sequence_id"].ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }

                        
                    }
                }
                dtBarCode.Rows.Add(dr);
            }
            else
                MessageBox.Show("未登記的條形碼!");
                        
            return dtBarCode;
        }


        

        /// <summary>
        /// 獲取用戶所屬部門
        /// </summary>
        /// <returns></returns>
        public static bool getUserDep(string uname, string dep)
        {
            bool result = true;
            string all_dep = "ALL";
            DataTable dtUserDep = new DataTable();
            string strSql = " SELECT * From dgcf_db.dbo.tb_sy_user_dep a Where uname='" + uname + "' And ( a.dep='" + dep + "' Or a.dep='ALL' )";
            try
            {
                dtUserDep=ExecuteSqlReturnDataTable(strSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (dtUserDep.Rows.Count == 0)
                result = false;
            return result;
        }

        /// <summary>
        /// 獲取WIP部門對應的JX部門
        /// </summary>
        /// <returns></returns>
        public static string getDepJx(string JxDep,string WipDep)
        {
            string result = "";
            string strSql = " SELECT * From int09 a Where int9loc>=''";
            if (JxDep != "")
                strSql += " And int9loc='" + JxDep + "'";
            if (WipDep != "")
                strSql += " And wip_dep='" + WipDep + "'";
            DataTable dt = ExecuteSqlReturnDataTable(strSql);
            if (dt.Rows.Count > 0)
            {
                if (JxDep != "")
                    result = dt.Rows[0]["wip_dep"].ToString().Trim();
                if (WipDep != "")
                    result = dt.Rows[0]["int9loc"].ToString().Trim();
            }
            return result;
        }

    }
}
