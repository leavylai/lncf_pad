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
    public class clsPublicOfGeo
    {
        private static String strConn = DBUtility.conn_str_dgerp2;
        
        /// <summary>
        /// 執行SQL語句或儲存過程，不返回值
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="paras"></param>
        /// <param name="isProce"></param>
        /// <returns></returns>
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
                    cmd.CommandTimeout = 1200;//連接20分鐘
                    if (paras != null)
                    {
                        cmd.Parameters.AddRange(paras);
                    }
                    if (isProce)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;                   
                    }
                    //cmd.Dispose();
                    //conn.Close();
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
        /// 執行存儲過程，返回DataTable
        /// </summary>
        /// <param name="proce"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static DataTable ExecuteProcedureReturnTable(string proce, SqlParameter[] paras)
        {
            DataTable dtData = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(strConn))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();

                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 1800;//連接30分鐘
                    cmd.CommandText = proce;
                    if (paras != null)
                    {
                        cmd.Parameters.AddRange(paras);
                    }
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(dtData);
                    sda.Dispose();
                    //conn.Close();
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
                    sda.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            return dtData;
        }


        public static int ExecuteSqlUpdate(string strSql)
        {
            int result = 0;
            SqlConnection conn = new SqlConnection(strConn);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = strSql;
                result = cmd.ExecuteNonQuery();
                cmd.Dispose();
                conn.Close();
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
    }
}
