using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Specialized;
using System.Runtime.Remoting.Messaging;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Forms;
using System.Xml.Linq;
//using cf_pad.MDL;


namespace cf_pad
{
	/// <summary>
	/// ADO.NET数据库操作基础类。
	/// </summary>
	public abstract class DBUtility
	{
		public static string _user_id = "ADMIN";//此變量用於保存當前登入的用戶ID
		public static string _language = "0";//此變量用於保存當前登入的語言
		public static string remote_db = "lnerp1.cferp.dbo.";//遠程數據庫名稱
        public static string remote_db_hr = "lnfs1.hr_db.dbo.";//遠程數據庫名稱JX HR
		public static string pad_db = "dgcf_pad.dbo.";//PAD用的數據庫
		public static string get_query_id = "";
		public static string imagePath = @"\\192.168.3.12\cf_artwork\Artwork\";
		public static string EncryptKey = "!cf~1965";
        public static string within_code = "0000";
		private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        
		//数据库连接字符串
		public static string connectionString = System.Configuration.ConfigurationManager.AppSettings["conn_string_dgsql1"]; // DGSQL1 
		//public static string connectionString = GetConnectionString("DGSQL1"); // DGSQL1 
		public static string conn_str_dgerp2 = System.Configuration.ConfigurationManager.AppSettings["conn_string_dgerp2"];  // DGERP2 
		public static string dgcf_pad_connectionString = System.Configuration.ConfigurationManager.AppSettings["conn_db_for_wm"]; // For PAD
		//public static string conn_str_dgerp2 = GetConnectionString("DGERP2");  // DGERP2 
		

		/// <summary>
		/// 执行查询语句，返回DataTable
		/// </summary>
		/// <param name="SQLString"></param>
		/// <returns></returns>
		public static DataTable GetDataTable(string SQLString)
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				DataTable dt = new DataTable();
				try
				{
					connection.Open();
					using (SqlDataAdapter da = new SqlDataAdapter(SQLString, connection))
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

		/// <summary>
		/// 執行SQL 語句，返回string
		/// </summary>
		/// <param name="strSQL"></param>
		/// <returns></returns>
		public static String ExecuteSqlReturnObject(string strSQL)
		{
			string objStrValue = "";
			try
			{
				DataTable dts = ExecuteSqlReturnDataTable(strSQL);
				if (dts.Rows.Count > 0)
				{
					objStrValue = dts.Rows[0][0].ToString();
				}
			}
			catch (Exception ex)
			{

				MessageBox.Show(ex.Message);
			}
			return objStrValue;
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
				using (SqlConnection conn = new SqlConnection(connectionString))
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

		/// <summary>
		/// 執行sql 語句或存儲過程，返回結果 int 
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
				using (SqlConnection conn = new SqlConnection(connectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand();
					cmd.Connection = conn;
					cmd.CommandText = strSql;
					cmd.Parameters.AddRange(paras);
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

		/// <summary>
		/// 執行存儲過程，返回DataTable
		/// </summary>
		/// <param name="proce"></param>
		/// <param name="paras"></param>
		/// <returns></returns>
		public static DataTable ExecuteProcedure(string proce, SqlParameter[] paras)
		{
			DataTable dtData = new DataTable();
			try
			{
				using (SqlConnection conn = new SqlConnection(connectionString))
				{
					conn.Open();
					SqlCommand cmd = new SqlCommand();

					cmd.Connection = conn;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = proce;
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

		#region  加解密函數

		public static string GeoEncrypt(string strEncrypt)
		{
			//函數說明：傳入用戶密碼(原碼),返回加密之後的字串
			//參數：as_code(輸入的密碼原碼).
			//返回值：ls_EncryptPass 加密之後的字串
			//ChingFung可以寫一個類似的函數，將加密之後的字串與目前資料庫中保存的密碼去比較,如果相等就表示輸入的密碼正確。
			//定義函數使用到的變數
			string ls_TempString, ls_Work, ls_EncryptPass, ls_DecryptString, ls_EncryptString, as_code;
			int li_Length, li_Position, li_Multiplier, li_Offset, li_Count;
			as_code = strEncrypt;// 輸入的密碼
			//--將輸入的密碼轉化為小寫字元
			ls_TempString = as_code.ToLower();
			//定義加密解密的字串,這一些字元是固定的.不允許修改.
			//以雙引號開始,同樣以雙引號結束，比如："123456",表示123456這幾個字元.

			ls_DecryptString = " !" + "\"" + "#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[" + "\\" + "]^_`abcdefghijklmnopqrstuvwxyz{|}~";
			ls_EncryptString = "~{[}u;Ce83KX%:VIm!|gs]_aL-QEOpx<UlzZjBq6#1($" + "\\" + "\"" + "FS5H0'cM&>Po.NGA*Jr)Y" + " " + "Dv/t9kd?^fni,hR2Wy=`+4T@7wb";


			//取得輸入的密碼長度
			li_Length = as_code.Trim().Length;
			//--根據不同的密碼長度得到不同的加密方法的字元長度倍數
			if (1 <= li_Length && li_Length <= 3)
				li_Multiplier = 1;
			else
				if (4 <= li_Length && li_Length <= 6)
					li_Multiplier = 2;
				else
					if (7 <= li_Length && li_Length <= 9)
						li_Multiplier = 3;
					else
						li_Multiplier = 4;
			ls_EncryptPass = "";//先將保存加密之後字串清空。

			//以下為迴圈密碼每一位元字元,對每一位元字元進行加密
			for (li_Count = 1; li_Count <= li_Length; li_Count++)
			{
				li_Offset = li_Count * li_Multiplier;
				//取密碼中的第li_count位元的字元，長度為1
				//使用方法：Mid(需要取值的字串,開始位置，長度)
				ls_Work = as_code.Substring(li_Count - 1, 1);//SUBSTR(as_code, li_Count, 1);
				//取到ls_work字元在ls_decryptstring中的第一個位置
				//使用方法：Pos(用來查找的字串，需要查找的字串)
				li_Position = ls_DecryptString.IndexOf(ls_Work) + 1;//substring(ls_Work,ls_DecryptString);
				li_Position = li_Position + li_Offset;
				//Mod是取模函數，即取到Li_positon除以95之後的餘數
				li_Position = li_Position % 95;//Mod(li_Position, 95);
				//將li_position值加1 ，相當於li_postion = li_postion + 1
				li_Position = li_Position + 1;
				//取到ls_EncryptString中第li_Position開始的1位元字元
				ls_Work = ls_EncryptString.Substring(li_Position - 1, 1);//SUBSTR(ls_EncryptString, li_Position, 1);
				//將加密之後的字元相加,得到加密結果字串.
				ls_EncryptPass = ls_EncryptPass + ls_Work;
				//重新設置加密方法的字元長度倍數
				if (1 <= li_Multiplier && li_Multiplier <= 3)
					li_Multiplier = li_Multiplier + 1;
				else
					li_Multiplier = 1;
			}
			//--將加密之後的字元返回
			return ls_EncryptPass;
		}

		//密鈅為8位長度
		//   public static string EncryptKey = "!cf~1965";
		//   private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
		/// <summary>
		/// 加密字符串
		/// </summary>
		/// <param name="encryptString">待加密的字符串</param>
		/// <param name="encryptKey">加密密钥,要求为8位</param>
		/// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
		public static string Encrypt(string encryptString) //, string encryptKey)
		{
			try
			{

				byte[] rgbKey = Encoding.UTF8.GetBytes(EncryptKey.Substring(0, 8));
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
				DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Convert.ToBase64String(mStream.ToArray());
			}
			catch
			{
				return encryptString;
			}
		}


		/// <summary>
		/// 解密字符串
		/// </summary>
		/// <param name="decryptString">待解密的字符串</param>
		/// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
		/// <returns>解密成功返回解密后的字符串，失败返源串</returns>
		public static string Decrypt(string decryptString) //, string decryptKey)
		{
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(EncryptKey);
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Convert.FromBase64String(decryptString);
				DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Encoding.UTF8.GetString(mStream.ToArray());
			}
			catch
			{
				//未加密的字串解密則返回原字串                
				return decryptString;
			}
		}
		#endregion

		#region 讀取或更新系統配置信息

		/// <summary>
		/// 返回數據庫的配置信息：True--已存在成功的配置信息;False--不存在成功的配置信息        
		/// </summary>        
		/// <returns>返回布樂值</returns>     
		public static bool GetDatabaseSeting()
		{
			bool result = false;
			SqlConnection conn = new SqlConnection(connectionString);
			if (conn == null)
			{
				return false;
			}
			try
			{
				conn.Open();
				if (conn.State == ConnectionState.Open)
				{
					result = true;
					conn.Close();
				}
				else
				{
					result = false;
				}
			}
			catch (SqlException)
			{
				//throw new Exception(E.Message);
				MessageBox.Show("請配置數據連接信息！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
				result = false;
			}
			finally
			{
				if (conn != null)
				{
					conn.Dispose();
					conn = null;
				}
			}
			return result;
		}


		///<summary> 
		///返回＊.exe.config文件中appSettings配置节的value项  
		///</summary> 
		///<param name="strKey"></param> 
		///<returns></returns> 
		public static string GetAppConfig(string strKey)
		{
			foreach (string key in ConfigurationManager.AppSettings)
			{
				if (key == strKey)
				{
					return ConfigurationManager.AppSettings[strKey];
				}
			}
			return null;
		}

		///<summary>  
		///在＊.exe.config文件中appSettings配置节增加一对键、值对  
		///</summary>  
		///<param name="newKey"></param>  
		///<param name="newValue"></param>  
		public static void UpdateAppConfig(string newKey, string newValue)
		{
			bool isModified = false;
			foreach (string key in ConfigurationManager.AppSettings)
			{
				if (key == newKey)
				{
					isModified = true;
				}
			}
			// Open App.Config of executable  
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			// You need to remove the old settings object before you can replace it  
			if (isModified)
			{
				config.AppSettings.Settings.Remove(newKey);
			}
			// Add an Application Setting.  
			config.AppSettings.Settings.Add(newKey, newValue);
			// Save the changes in App.config file.  
			config.Save(ConfigurationSaveMode.Modified);
			// Force a reload of a changed section.  
			ConfigurationManager.RefreshSection("appSettings");
		}
		#endregion


		

	} //DBUtility
}

