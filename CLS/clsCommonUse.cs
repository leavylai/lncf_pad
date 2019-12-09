using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Collections;

namespace cf_pad.CLS
{
  public  class clsCommonUse
    {
        #region 原DataBase 方法

        private SqlConnection m_Conn = null;
        private SqlCommand m_Cmd = null;

        /// <summary>
        /// 创建数据库连接和SqlCommand实例
        /// </summary>
        public clsCommonUse()
        {

            string strConn = DBUtility.connectionString;
            try
            {
                m_Conn = new SqlConnection(strConn);
                m_Cmd = new SqlCommand();
                m_Cmd.Connection = m_Conn;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public SqlConnection Conn
        {
            get { return m_Conn; }
        }

        public SqlCommand Cmd
        {
            get { return m_Cmd; }
        }

        /// <summary>
        /// 通过Transact-SQL语句提交数据
        /// </summary>
        /// <param name="strSql">Transact-SQL语句</param>
        /// <returns>受影响的行数</returns>
        public int ExecDataBySql(string strSql)
        {
            int intReturnValue;

            m_Cmd.CommandType = CommandType.Text;
            m_Cmd.CommandText = strSql;

            try
            {
                if (m_Conn.State == ConnectionState.Closed)
                {
                    m_Conn.Open();
                }

                intReturnValue = m_Cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                m_Conn.Close();//连接关闭，但不释放掉该对象所占的内存单元
            }

            return intReturnValue;
        }

        /// <summary>
        /// 通过Transact-SQL语句得到DataSet实例
        /// </summary>
        /// <param name="strSql">Transact-SQL语句</param>
        /// <param name="strTable">相关的数据表</param>
        /// <returns>DataSet实例的引用</returns>
        public DataSet GetDataSet(string strSql, string strTable)
        {
            DataSet ds = null;

            try
            {
                SqlDataAdapter sda = new SqlDataAdapter(strSql, m_Conn);
                ds = new DataSet();
                sda.Fill(ds, strTable);
            }
            catch (Exception e)
            {
                throw e;
            }

            return ds;
        }

        /// <summary>
        /// 通过Transact-SQL语句得到SqlDataReader实例
        /// </summary>
        /// <param name="strSql">Transact-SQL语句</param>
        /// <returns>SqlDataReader实例的引用</returns>
        public SqlDataReader GetDataReader(string strSql)
        {
            SqlDataReader sdr;

            m_Cmd.CommandType = CommandType.Text;
            m_Cmd.CommandText = strSql;

            try
            {
                if (m_Conn.State == ConnectionState.Closed)
                {
                    m_Conn.Open();
                }

                sdr = m_Cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception e)
            {
                throw e;
            }

            //sdr对象和m_Conn对象暂时不能关闭和释放掉，否则在调用时无法使用
            //待使用完毕sdr，再关闭sdr对象（同时会自动关闭关联的m_Conn对象）
            //m_Conn的关闭是指关闭连接通道，但连接对象依然存在
            //m_Conn的释放掉是指销毁连接对象
            return sdr;
        }

        #endregion

        #region DataGridView 、ComboBox控制
       
        /// <summary>
        /// 在DataGridView控件的指定位置插入行
        /// </summary>
        /// <param name="dgv">DataGridView控件</param>
        /// <param name="bs">BindingSource组件</param>
        /// <param name="dt">DataTable内存数据表</param>
        /// <param name="intPosIndex">指定位置的索引值</param>
        /// <returns>DataGridViewRow对象的引用</returns>
        public DataGridViewRow DataGridViewInsertRow(DataGridView dgv, BindingSource bs, DataTable dt, int intPosIndex)
        {
            DataGridViewRow dgvr = null;

            try
            {
                DataRow dr = dt.NewRow(); //基于某个DataTable的结构( 列结构仍然使用初始时产生的结构(如：sda.Fill(dt)) )，创建一个DataRow对象
                dt.Rows.InsertAt(dr, intPosIndex); //在数据源中插入新创建的DataRow对象
                bs.DataSource = dt;
                dgv.DataSource = bs;
                dgvr = dgv.Rows[intPosIndex];
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dgvr;
        }

        /// <summary>
        /// ComboBox或DataGridViewComboBoxColumn绑定到数据源
        /// </summary>
        /// <param name="obj">要绑定数据源的控件</param>
        /// <param name="strValueColumn">ValueMember属性要绑定的列名称</param>
        /// <param name="strTextColumn">DisplayMember属性要绑定的列名称</param>
        /// <param name="strSql">SQL查询语句</param>
        /// <param name="strTable">数据表的名称</param>
        public void BindComboBox(Object obj, string strValueColumn, string strTextColumn, string strSql, string strTable) //Component —替换—> Object
        {
            try
            {
                string strType = obj.GetType().ToString();
                strType = strType.Substring(strType.LastIndexOf(".") + 1);

                //判断控件的类型
                switch (strType)
                {
                    case "ComboBox":

                        ComboBox cbx = (ComboBox)obj;
                        cbx.BeginUpdate();
                        cbx.DataSource = GetDataSet(strSql, strTable).Tables[strTable];
                        cbx.DisplayMember = strTextColumn;
                        cbx.ValueMember = strValueColumn;
                        cbx.EndUpdate();
                        break;

                    case "DataGridViewComboBoxColumn":

                        DataGridViewComboBoxColumn dgvcbx = (DataGridViewComboBoxColumn)obj;
                        dgvcbx.DataSource = GetDataSet(strSql, strTable).Tables[strTable];
                        dgvcbx.DisplayMember = strTextColumn;
                        dgvcbx.ValueMember = strValueColumn;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
       
        #endregion

        #region 連接數據庫操作方法

        /// <summary>
        /// 此方法是取得存儲過程的要傳入的參數
        /// 並將其存放至數組中，並返回這個數組
        /// </summary>
        /// <param name="proc"> 傳入的存儲過程名</param>
        /// <returns></returns>
        public ArrayList getProcParameters(string proc)
        {
            SqlConnection myconn = new SqlConnection(DBUtility.connectionString);
            System.Data.SqlClient.SqlCommand mycomm = new SqlCommand("sp_sproc_columns", myconn);
            DataTable td = new DataTable();
            mycomm.CommandType = CommandType.StoredProcedure;

            //mycomm.Parameters.Add("@procedure_name", (object)proc);
            mycomm.Parameters.Add("@procedure_name", SqlDbType.NVarChar);//如果連接的是SQL2008，則可改為：SqlDbType.Text
            mycomm.Parameters["@procedure_name"].Value = (object)proc;
            System.Data.SqlClient.SqlDataAdapter myadapter = new SqlDataAdapter(mycomm);
            try
            {
                myadapter.Fill(td);
                myadapter.Dispose();
            }
            catch (SqlException ex)
            {
                myconn.Close();
                MessageBox.Show(ex.Message);
            }
            ArrayList al = new ArrayList();
            for (int i = 1; i < td.Rows.Count; i++)
            {
                al.Add(td.Rows[i][3].ToString());
            }

            return al;
        }
        public DataTable getDataProcedure(string proc, object[] parms)
        {
            SqlConnection myconn = new SqlConnection(DBUtility.connectionString);
            System.Data.SqlClient.SqlCommand mycomm = new SqlCommand(proc, myconn);
            mycomm.CommandType = CommandType.StoredProcedure;
            mycomm.CommandTimeout = 1800;//連接30分鐘

            ArrayList al = getProcParameters(proc);
            for (int i = 0; i < parms.Length; i++)
            {
                mycomm.Parameters.Add(new SqlParameter(al[i].ToString(), parms[i]));
            }

            SqlDataAdapter myAdapter = new SqlDataAdapter();
            myAdapter.SelectCommand = mycomm;

            DataTable d = new DataTable();
            try
            {
                myAdapter.Fill(d);
                myAdapter.Dispose();
            }
            catch (SqlException ex)
            {
                myconn.Close();
                MessageBox.Show(ex.Message);
            }
            return d;
        }


        //****************
        public DataSet getDataSet(string proc, object[] parms)
        {
            SqlConnection myconn = new SqlConnection(DBUtility.connectionString);
            System.Data.SqlClient.SqlCommand mycomm = new SqlCommand(proc, myconn);
            mycomm.CommandType = CommandType.StoredProcedure;
            mycomm.CommandTimeout = 1800;//連接30分鐘

            ArrayList al = getProcParameters(proc);
            for (int i = 0; i < parms.Length; i++)
            {
                mycomm.Parameters.Add(new SqlParameter(al[i].ToString(), parms[i]));
            }

            SqlDataAdapter myAdapter = new SqlDataAdapter();
            myAdapter.SelectCommand = mycomm;

            DataSet ds = new DataSet();
            try
            {
                myAdapter.Fill(ds);
                myAdapter.Dispose();
            }
            catch (SqlException ex)
            {
                myconn.Close();
                MessageBox.Show(ex.Message);
            }
            return ds;
        }
        #endregion
     
    }
}
