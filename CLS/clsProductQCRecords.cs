using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cf_pad.MDL;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;

namespace cf_pad.CLS
{
	public class clsProductQCRecords
	{
		/// <summary>
		/// 添加工序產品QC報告
		/// </summary>
		/// <returns></returns>
		private static String strConn = DBUtility.dgcf_pad_connectionString;
		public static int AddProductQCRecords(product_qc_records prd_records)
		{
			int Result = 0;
			try
			{
				string strSQL = "";
				using (SqlConnection conn = new SqlConnection(strConn))
				{
					conn.Open();

					strSQL = @"INSERT INTO dbo.product_qc_records(id, dep_no, prd_date,qc_date, mo_no,
																mat_item, mat_color, lot_qty, facade_actual_qty, size_actual_qty, 
																actual_size, mat_logo, oth_desc, no_pass_qty, qc_no_ok,
																qc_ok,do_result,crusr,crtim,seq_no)
														  VALUES(@id, @dep_no, @prd_date,@qc_date, @mo_no,
																@mat_item, @mat_color, @lot_qty, @facade_actual_qty, @size_actual_qty, 
																@actual_size, @mat_logo, @oth_desc, @no_pass_qty, @qc_no_ok,
																@qc_ok, @do_result,@crusr,@crtim,@seq_no) ";

					SqlParameter[] paras = new SqlParameter[]{
					   new SqlParameter("@id",prd_records.id),
					   new SqlParameter("@dep_no",prd_records.dep_no),
					   new SqlParameter("@prd_date",prd_records.prd_date),
					   new SqlParameter("@qc_date",prd_records.qc_date),
					   new SqlParameter("@mo_no",prd_records.mo_no),
					   new SqlParameter("@mat_item",prd_records.mat_item),
					   new SqlParameter("@mat_color",prd_records.mat_color),
					   new SqlParameter("@lot_qty",prd_records.lot_qty),
					   new SqlParameter("@facade_actual_qty",prd_records.facade_actual_qty),
					   new SqlParameter("@size_actual_qty",prd_records.size_actual_qty),
					   new SqlParameter("@actual_size",prd_records.actual_size),
					   new SqlParameter("@mat_logo",prd_records.mat_logo),
					   new SqlParameter("@oth_desc",prd_records.oth_desc),
					   new SqlParameter("@no_pass_qty",prd_records.no_pass_qty),
					   new SqlParameter("@qc_no_ok",prd_records.qc_no_ok),
					   new SqlParameter("@qc_ok",prd_records.qc_ok),
					   new SqlParameter("@do_result",prd_records.do_result),
					   new SqlParameter("@crusr",prd_records.crusr),
					   new SqlParameter("@crtim",prd_records.crtim),
					   new SqlParameter("@seq_no",prd_records.seq_no)
					};
					strSQL += @" UPDATE product_records SET qc_flag='Y' WHERE prd_id=" + prd_records.id + "";

					SqlCommand cmd = new SqlCommand();
					cmd.Connection = conn;
					cmd.CommandText = strSQL;
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
		/// 更新已QC 的產品報告
		/// </summary>
		/// <param name="lsEntity"></param>
		/// <param name="ServerConn"></param>
		/// <returns></returns>
		public static int UpdateProductQCRecords(product_qc_records prd_records)
		{
			int Result = 0;
			try
			{
				string strSQL = "";
				using (SqlConnection conn = new SqlConnection(strConn))
				{
					conn.Open();

					strSQL =@" UPDATE dbo.product_qc_records 
								SET qc_date=@qc_date ,mat_color=@mat_color ,facade_actual_qty=@facade_actual_qty ,size_actual_qty=@size_actual_qty ,actual_size=@actual_size,
									mat_logo=@mat_logo ,no_pass_qty=@no_pass_qty ,oth_desc=@oth_desc ,do_result=@do_result ,amusr=@amusr,
									amtim=@amtim ,qc_no_ok=@qc_no_ok ,qc_ok=@qc_ok
							  WHERE id=@id and seq_no=@seq_no ";

					SqlParameter[] paras = new SqlParameter[]{
					   new SqlParameter("@qc_date",prd_records.qc_date),
					   new SqlParameter("@mat_color",prd_records.mat_color),
					   new SqlParameter("@facade_actual_qty",prd_records.facade_actual_qty),
					   new SqlParameter("@size_actual_qty",prd_records.size_actual_qty),
					   new SqlParameter("@actual_size",prd_records.actual_size),
					   new SqlParameter("@mat_logo",prd_records.mat_logo),
					   new SqlParameter("@oth_desc",prd_records.oth_desc),
					   new SqlParameter("@no_pass_qty",prd_records.no_pass_qty),
					   new SqlParameter("@qc_no_ok",prd_records.qc_no_ok),
					   new SqlParameter("@qc_ok",prd_records.qc_ok),
					   new SqlParameter("@do_result",prd_records.do_result),
					   new SqlParameter("@amusr",prd_records.amusr),
					   new SqlParameter("@amtim",prd_records.amtim),
					   new SqlParameter("@id",prd_records.id),
					   new SqlParameter("@seq_no",prd_records.seq_no)
					};

					SqlCommand cmd = new SqlCommand();
					cmd.Connection = conn;
					cmd.CommandText = strSQL;
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
		/// 獲取字樣
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
		public static String GetMat_Logo(string pId)
		{
			string strPatternName = "";
			try
			{
				DataTable dtMat_Logo = new DataTable();
				string strSql = "SELECT id , name FROM  cd_pattern WHERE id='" + pId + "'";
				using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
				{
					SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
					sda.Fill(dtMat_Logo);
				}
				strPatternName = dtMat_Logo.Rows[0]["name"].ToString();
			}
			catch (Exception ex)
			{

				MessageBox.Show(ex.Message);
			}

			return strPatternName;
		}

		/// <summary>
		///獲取實際尺寸
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
		public static String GetSize(string pId)
		{
			string strSizeName = "";
			try
			{
				DataTable dtSize = new DataTable();
				string strSql = "SELECT id , name FROM  cd_size  WHERE id='" + pId + "'";
				using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
				{
					SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
					sda.Fill(dtSize);
				}
				strSizeName = dtSize.Rows[0]["name"].ToString();
			}
			catch (Exception ex)
			{

				MessageBox.Show(ex.Message);
			}
			return strSizeName;
		}


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

        //提取檢驗人員的工號
        public static DataTable InitWorker(string dep1,string dep2,string job)
        {
            DataTable dtWid = new DataTable();
            try
            {
                //獲取制單編號資料 COLLATE Chinese_PRC_CI_AS
                string sql = "";
                sql += " Select a.hrm1wid,a.hrm1name " +
                    " From dgsql1.dghr.dbo.hrm01 a " +
                    " Where a.hrm1stat2 >='" + dep1 + "' AND a.hrm1stat2<='" + dep2 + "' AND a.hrm1state='1'";
                if (job != "")
                    sql += " And a.hrm1job='" + job + "'";
                dtWid = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                if (dtWid.Rows.Count > 0)
                {
                    dtWid.Rows.Add();
                    dtWid.DefaultView.Sort = "hrm1wid";


                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            return dtWid;
        }



	}
}
