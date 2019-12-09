using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using cf_pad.MDL;

namespace cf_pad.CLS
{
	public class clsUser
	{

		/// <summary>
		/// 查找用戶是否存在:非空--存在;空--不存在        
		/// </summary>
		/// <param name="strUserID"></param>
		/// <returns></returns>
		public static string IsExistUser(string userid)
		{
			string cUserid = userid;
			string username = "";
			bool flag = true;
			if (cUserid == "")
			{
				MessageBox.Show("用戶帳號不可為空！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
				flag = false;
				username = "";
			}

			if (flag) //帳號是否為為空
			{

				string strSql = "Select user_id,user_name From dbo.sys_user Where user_id =@uname and typeid ='U'";
				SqlConnection con = new SqlConnection(DBUtility.conn_str_dgerp2);
				try
				{
					con.Open();
					SqlCommand cmd = new SqlCommand();
					cmd.CommandText = strSql;
					cmd.Connection = con;
					SqlParameter Uname = new SqlParameter("@uname", SqlDbType.VarChar, 50);
					Uname.Value = cUserid;
					cmd.Parameters.Add(Uname);
					SqlDataReader dr = cmd.ExecuteReader();
					if (dr.Read())
					{
						username = dr.GetString(dr.GetOrdinal("user_name"));
					}
					else
					{
						MessageBox.Show("用戶帳號不正確！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
						username = "";
					}
					cmd.Dispose();
					dr.Dispose();
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString(), "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
					username = "";
				}
				finally
				{
					con.Close();
				}
			}
			else
			{
				username = "";
			}
			userid = username;
			return userid;
		}

		/// <summary>
		/// 登錄    
		/// </summary>
		/// <param name="strUserID">用戶帳號</param>
		/// <param name="strpassword">帳號密碼</param>
		/// <returns></returns>
		public static bool GetUserInfo(string userid, string password)
		{
			string cUserid = userid;

			bool result = false;
			string strSql = "Select user_id,user_name,password From dbo.sys_user Where user_id =@uname and typeid ='U'";
			SqlConnection con = new SqlConnection(DBUtility.conn_str_dgerp2);
			try
			{
				con.Open();
				SqlCommand cmd = new SqlCommand();
				cmd.CommandText = strSql;
				cmd.Connection = con;
				SqlParameter Uname = new SqlParameter("@uname", SqlDbType.VarChar, 50);
				Uname.Value = cUserid;
				cmd.Parameters.Add(Uname);
				SqlDataReader dr = cmd.ExecuteReader();
				if (dr.Read())
				{
					if (DBUtility.GeoEncrypt(password) == dr.GetString(dr.GetOrdinal("password")))
					{
						result = true;
						DataTable dt = DBUtility.GetDataTable("select language from dbo.tb_sy_user where uname='" + userid + "'");//2013-08-21更改
						DBUtility._language = dt.Rows[0]["language"].ToString();                                                  //2013-08-21更改
					}
					else
					{
						result = false;
						MessageBox.Show("密碼不正確！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				else
				{
					result = false;
				}
				cmd.Dispose();
				dr.Dispose();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
				result = false;
			}
			finally
			{
				con.Close();
			}
			return result;
		}

		public static DataTable QueryUserInfo(string UserID, string UserName)
		{
			using (SqlConnection connection = new SqlConnection(DBUtility.connectionString))
			{
				DataTable dt = new DataTable();

				string strSQL = @" SELECT A.Uid,A.Uname,A.Uname_desc,B.Rid,B.Rname ,
					(CASE WHEN A.language='0' THEN '繁體中文' WHEN A.language='1' THEN '簡體中文' 
						  WHEN A.language='2' THEN 'English' ELSE NULL END)AS Language " +
								 "FROM dbo.tb_sy_user A left join tb_sy_role B on A.rid = b.rid ";
				if (UserID != "" && UserName != "")
				{
					strSQL += " WHERE A.Uname LIKE " + "'%" + UserID + "%'";
					strSQL += " AND A.Uname_desc LIKE " + "'%" + UserName + "%'";
				}
				if (UserName != "" && UserID == "")
				{
					strSQL += " WHERE A.Uname_desc LIKE " + "'%" + UserName + "%'";
				}
				if (UserName == "" && UserID != "")
				{
					strSQL += " WHERE A.Uname LIKE " + "'%" + UserID + "%'";
				}
				try
				{
					connection.Open();
					using (SqlDataAdapter da = new SqlDataAdapter(strSQL, connection))
					{
						da.Fill(dt);
					}
				}
				catch (SqlException ex)
				{
					throw new Exception(ex.Message);
				}
				return dt;
			}
		}

		public static int AddUserInfo(mdlUser modelUser)
		{
			int Result = -1;
			try
			{
				using (SqlConnection conn = new SqlConnection(DBUtility.connectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand();
					string strSQL = @"INSERT INTO tb_sy_user(Uname, Uname_desc, Pwd, Rid, language) VALUES (@Uname, @Uname_desc, @Pwd, @Rid, @language)";

					SqlParameter[] paras = new SqlParameter[] { 
					   new SqlParameter("@Uname",modelUser.Uname),
						new SqlParameter("@Uname_desc",modelUser.Uname_Desc),
						 new SqlParameter("@Pwd",modelUser.Pwd),
						  new SqlParameter("@Rid",modelUser.Role.Rid),
						   new SqlParameter("@language",modelUser.Language)
					};
					cmd.Parameters.AddRange(paras);
					cmd.CommandText = strSQL;
					cmd.Connection = conn;
					Result = cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return Result;
		}

		public static int DelUserInfo(int uid)
		{
			int Result = -1;
			try
			{
				using (SqlConnection conn = new SqlConnection(DBUtility.connectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand();
					string strSQL = "DELETE FROM tb_sy_user WHERE Uid=@Uid ";

					cmd.Parameters.Add(new SqlParameter("@Uid", uid));
					cmd.CommandText = strSQL;
					cmd.Connection = conn;

					Result = cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return Result;
		}

		public static int UpdateUserInfo(mdlUser modelUser)
		{
			int Result = -1;
			try
			{
				using (SqlConnection conn = new SqlConnection(DBUtility.connectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand();
					string strSQL = @" UPDATE tb_sy_user SET Rid =@Rid, language=@language  WHERE Uid=@Uid AND Uname=@Uname";

					SqlParameter[] paras = new SqlParameter[] { 
					   new SqlParameter("@Uid",modelUser.Uid),
						new SqlParameter("@Uname",modelUser.Uname),
						  new SqlParameter("@Rid",modelUser.Role.Rid),
						   new SqlParameter("@language",modelUser.Language)
					};
					cmd.Parameters.AddRange(paras);
					cmd.CommandText = strSQL;
					cmd.Connection = conn;
					Result = cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return Result;
		}

		public static int UpdateUserLanguage(string pLanguage,string pUname)
		{
			int Result = -1;
			try
			{
				using (SqlConnection conn = new SqlConnection(DBUtility.connectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand();
					string strSQL = @" Update dbo.tb_sy_user set language =@language  Where uname=@uname ";

					SqlParameter[] paras = new SqlParameter[] { 
					   new SqlParameter("@uname",pUname),
					   new SqlParameter("@language",pLanguage)
					};
					cmd.Parameters.AddRange(paras);
					cmd.CommandText = strSQL;
					cmd.Connection = conn;
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
		///根据用户ID查询用户权限 
		/// </summary>
		/// <param name="UserId"></param>
		/// <param name="UserName"></param>
		/// <returns> sqlserver=dgerp2</returns>
		public static DataTable GetUserById(string UserId, string UserName)
		{
			using (SqlConnection connection = new SqlConnection(DBUtility.conn_str_dgerp2))
			{
				DataTable dt = new DataTable();

				string strSQL = "SELECT user_id AS Uname,user_name AS Uname_desc,password FROM  sys_user ";
				if (UserId != "")
				{
					strSQL += " WHERE user_id LIKE " + "'%" + UserId + "%'";
				}
				if (UserName != "")
				{
					strSQL += " OR user_id LIKE " + "'%" + UserName + "%'";
				}
				try
				{
					connection.Open();
					using (SqlDataAdapter da = new SqlDataAdapter(strSQL, connection))
					{
						da.Fill(dt);
					}
				}
				catch (SqlException ex)
				{
					throw new Exception(ex.Message);
				}
				return dt;
			}
		}

	}
}
