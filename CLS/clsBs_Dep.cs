using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace cf_pad.CLS
{
    public class clsBs_Dep
    {

        /// <summary>
        ///dgerp2.cferp.cd_department 
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllDepartment()
        {
            DataTable dtDept = new DataTable();
            try
            {
                string strSql = @" SELECT id,name FROM cd_department";
                dtDept = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtDept;
        }

        public static DataTable GetAll_WH()
        {
            DataTable dtDept = new DataTable();
            try
            {
                string strSql = @"SELECT id,name FROM cd_productline WHERE within_code ='0000' AND storehouse_group='DG' AND type='01' and state<>'2'";
                dtDept = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtDept;
        }

        /// <summary>
        /// 返回電鍍部門
        /// </summary>
        /// <returns></returns>
        public static DataTable Get_Plate_Dept()
        {
            DataTable dt = new DataTable();
            try
            {
                string strSql = @"SELECT id,name FROM cd_department where within_code='0000' and op_dept ='1'";
                dt = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dt;            
        }
    }
}
