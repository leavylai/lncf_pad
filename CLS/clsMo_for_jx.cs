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
    public class clsMo_for_jx
    {
        /// <summary>
        /// 工序卡(frmOrderProCard)中獲取生產計劃資料
        /// </summary>
        /// <param name="wp_id"></param>
        /// <param name="mo_id"></param>
        /// <param name="goods_id"></param>
        /// <returns></returns>
        public static DataTable GetGoods_DetailsById(string wp_id, string mo_id, string goods_id)
        {
            DataTable dtGoods = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @" SELECT b.wp_id, a.mo_id, b.goods_id, c.name, b.prod_qty, b.OBLIGATE_QTY, a.bill_date
                                               ,b.t_complete_date, b.next_wp_id, d.name AS next_wp_name, a.check_date, b.goods_unit
                                               ,a.customer_id, e.brand_id, e.get_color_sample
                                               ,e.goods_unit AS order_unit, f.production_remark, f.nickle_free, f.plumbum_free
                                               ,a.remark, convert(int,g.base_qty) as base_qty, g.unit_code, convert(int,g.rate) AS base_rate
                                               ,g.basic_unit, b.vendor_id, CONVERT(Decimal(10), b.c_sec_qty_ok) as c_sec_qty_ok, dp.name as get_color_sample_name
                                               ,a.within_code, a.id, a.ver, b.sequence_id, c.blueprint_id, CONVERT(Decimal(10),b.predept_rechange_qty) AS predept_rechange_qty, c.color
                                        FROM  dbo.jo_bill_mostly a with(nolock)
                                        INNER JOIN dbo.jo_bill_goods_details b with(nolock)ON  a.within_code = b.within_code AND  a.id = b.id AND  a.ver = b.ver 
                                        INNER JOIN dbo.it_goods c with(nolock) ON  b.within_code = c.within_code AND  b.goods_id = c.id
                                        INNER JOIN dbo.cd_department d ON b.within_code=d.within_code And b.next_wp_id=d.id
                                        LEFT JOIN dbo.so_order_details e with(nolock) ON a.within_code=e.within_code AND a.mo_id=e.mo_id AND a.so_sequence_id=e.sequence_id
                                        LEFT OUTER JOIN dbo.cd_department dp ON e.within_code=dp.within_code and e.get_color_sample=dp.id
                                        LEFT OUTER JOIN dbo.so_order_special_info f with(nolock) ON e.within_code=f.within_code AND e.id=f.id AND e.sequence_id=f.upper_sequence AND e.ver=f.ver
                                        LEFT OUTER JOIN dbo.it_coding g with(nolock) On b.within_code=g.within_code AND b.goods_id=g.id
                                        WHERE  b.wp_id = @wp_id AND  a.mo_id = @mo_id ";
                    if (goods_id != "")
                    {
                        strSQL += " AND b.goods_id=@goods_id ";
                        cmd.Parameters.Add(new SqlParameter("@goods_id", goods_id));
                    }
                    cmd.Parameters.Add(new SqlParameter("@wp_id", wp_id));
                    cmd.Parameters.Add(new SqlParameter("@mo_id", mo_id));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtGoods);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtGoods;
        }

        /// <summary>
        /// 從OC中獲取訂單數量
        /// </summary>
        /// <param name="mo_id"></param>
        /// <returns></returns>
        public static DataTable GetOrderQty(string mo_id)
        {
            DataTable dtQty = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @" SELECT a.order_qty,convert(int,a.order_qty*b.rate) as order_qty_pcs,a.goods_unit
                                        FROM dbo.so_order_details a with(nolock) 
                                        LEFT OUTER JOIN dbo.it_coding b with(nolock) On a.within_code=b.within_code AND a.goods_unit=b.unit_code
                                        WHERE  a.within_code = '" + "0000" + "'" + " AND  a.mo_id = @mo_id and b.id='" + "*" + "'";
                    cmd.Parameters.Add(new SqlParameter("@mo_id", mo_id));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtQty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtQty;
        }

        /// <summary>
        /// 從OC中獲取訂單數量(Bom 用量)
        /// </summary>
        /// <param name="mo_id"></param>
        /// <returns></returns>
        public static DataTable GetOrderQtyBasedOnBom(string mo_id, string goods_id)
        {
            DataTable dtQty = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @" SELECT a.order_qty*b.dosage as order_qty,convert(int,a.order_qty*c.rate*b.dosage) as order_qty_pcs,a.goods_unit
                                        FROM dbo.so_order_details as a 
                                        LEFT OUTER JOIN  so_order_bom as b with(nolock) on a.within_code=b.within_code AND a.id =b.id AND a.sequence_id =b.upper_sequence
                                        LEFT OUTER JOIN dbo.it_coding c with(nolock) On a.within_code=c.within_code AND a.goods_unit=c.unit_code
                                        WHERE  a.within_code = '" + "0000" + "'" + " AND  a.mo_id = @mo_id and c.id='" + "*" + "' AND b.goods_id =@goods_id ";
                    cmd.Parameters.Add(new SqlParameter("@mo_id", mo_id));
                    cmd.Parameters.Add(new SqlParameter("@goods_id", goods_id));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtQty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtQty;
        }

        /// <summary>
        /// 工序卡(frmOrderProCard)中獲取物料編號、圖樣的資料
        /// </summary>
        /// <param name="goods_item"></param>
        /// <returns></returns>
        public static DataTable GetGoods_ArtWork(string goods_item)
        {
            DataTable dtArt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @" SELECT b.sequence_id AS art_id,b.picture_name,c.name AS color_name,c.do_color
                                        FROM  dbo.it_goods a 
                                        LEFT JOIN dbo.cd_pattern_details b ON a.within_code=b.within_code AND a.blueprint_id=b.id
                                        LEFT JOIN dbo.cd_color c ON a.within_code=c.within_code AND a.color=c.id
                                        WHERE  a.within_code=@within_code and a.id = @goods_id ";
                    cmd.Parameters.Add(new SqlParameter("@within_code", "0000"));
                    cmd.Parameters.Add(new SqlParameter("@goods_id", goods_item));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtArt);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtArt;
        }

        /// <summary>
        /// 根據產品類型、圖樣代號，尺寸獲取模具存放位置
        /// </summary>
        /// <param name="prd_item">根據傳入的產品編號截取以上條件</param>
        /// <returns>String</returns>
        public static DataTable GetPosition(string prd_item)
        {
            DataTable dtPosition = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @" SELECT id,mould_no FROM cd_mould_position 
                                      WHERE products_type=@products_type AND pattern_id=@pattern_id AND measurement=@measurement ";
                    cmd.Parameters.Add(new SqlParameter("@products_type", prd_item.Substring(2, 2)));
                    cmd.Parameters.Add(new SqlParameter("@pattern_id", prd_item.Substring(4, 7)));
                    cmd.Parameters.Add(new SqlParameter("@measurement", prd_item.Substring(11, 3)));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtPosition);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtPosition;
        }

        //獲取電鍍顏色
        public static DataTable GetColor(string mo_id, string goods_id, string next_wp_id)
        {
            DataTable dtColor = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @"SELECT a.mo_id, c.sequence_id,c.materiel_id, b.goods_id,e.name AS color_desc, d.do_color, b.next_wp_id,b.wp_id
                        From jo_bill_mostly a
                        Inner Join jo_bill_goods_details b on a.within_code=b.within_code and a.id=b.id and a.ver=b.ver
                        Inner Join jo_bill_materiel_details c on b.within_code=c.within_code AND b.id=c.id AND b.ver=c.ver AND b.sequence_id=c.upper_sequence
                        Inner Join it_goods d on b.within_code=d.within_code and b.goods_id=d.id
                        Left Join cd_color e on d.within_code=e.within_code and d.color=e.id
                        Where a.within_code=@within_code and a.mo_id=@mo_id and c.materiel_id=@goods_id and b.wp_id=@next_wp_id";
                    cmd.Parameters.Add(new SqlParameter("@within_code", "0000"));
                    cmd.Parameters.Add(new SqlParameter("@mo_id", mo_id));
                    cmd.Parameters.Add(new SqlParameter("@goods_id", goods_id));
                    cmd.Parameters.Add(new SqlParameter("@next_wp_id", next_wp_id));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtColor);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtColor;
        }

        /// <summary>
        /// 單位轉換
        /// </summary>
        /// <param name="basic_unit"></param>
        /// <param name="UnitValue"></param>
        /// <returns></returns>
        public static double UnitConversionRate(string basic_unit, double UnitValue)
        {
            double resultValue;
            resultValue = 0;
            DataTable dtUnitRate = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @" SELECT rate
                                        FROM  dbo.it_coding a
                                        WHERE  a.within_code=@within_code and a.id=@id and a.unit_code = @basic_unit ";
                    cmd.Parameters.Add(new SqlParameter("@within_code", "0000"));
                    cmd.Parameters.Add(new SqlParameter("@id", "*"));
                    cmd.Parameters.Add(new SqlParameter("@basic_unit", basic_unit));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtUnitRate);
                    if (dtUnitRate.Rows.Count > 0)
                    {
                        resultValue = UnitValue * Convert.ToDouble(dtUnitRate.Rows[0]["rate"].ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return resultValue;
        }

        public static DataTable CheckIsLock(string wp_id, string mo_id, string goods_id)
        {
            DataTable dtLockInfo = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(DBUtility.connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @" SELECT apr_usr,apr_tim FROM mo_for_jx 
                                        WHERE mo_id=@mo_id AND wp_id=@wp_id AND goods_id=@goods_id
                                              AND apr_usr IS NOT NULL  AND apr_tim IS NOT NULL ";
                    cmd.Parameters.Add(new SqlParameter("@wp_id", wp_id));
                    cmd.Parameters.Add(new SqlParameter("@mo_id", mo_id));
                    cmd.Parameters.Add(new SqlParameter("@goods_id", goods_id));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtLockInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtLockInfo;
        }

        public static int AddMo_for_jx(Mo_for_jx pModel)
        {
            int Result = -1;

            string strIsExsit = DBUtility.ExecuteSqlReturnObject(string.Format("SELECT mo_id FROM mo_for_jx WHERE mo_id='{0}' AND wp_id='{1}' AND goods_id='{2}'", pModel.mo_id, pModel.wp_id, pModel.goods_id));
            if (strIsExsit == "")
            {
                string strSQL = @"INSERT INTO mo_for_jx( mo_date, mo_id, wp_id, goods_id ,goods_name, prod_qty, rmk, cr_usr, cr_tim,order_qty,chk_dat,t_dat,next_wp_id,next_wp_name,ver,color_desc,do_color)
                                                     VALUES(@mo_date, @mo_id, @wp_id, @goods_id ,@goods_name, @prod_qty, @rmk, @cr_usr, @cr_tim,@order_qty,@chk_dat,@t_dat,@next_wp_id,@next_wp_name,@ver,@color_desc,@do_color)";
                SqlParameter[] paras = new SqlParameter[] { 
                       new SqlParameter("@mo_date",pModel.mo_date),
                       new SqlParameter("@mo_id",pModel.mo_id),
                       new SqlParameter("@wp_id",pModel.wp_id),
                       new SqlParameter("@goods_id",pModel.goods_id),
                       new SqlParameter("@goods_name",pModel.goods_name),
                       new SqlParameter("@prod_qty",pModel.prod_qty),
                       new SqlParameter("@rmk",pModel.rmk),
                       new SqlParameter("@cr_usr",pModel.cr_usr),
                       new SqlParameter("@cr_tim",pModel.cr_tim),
                       new SqlParameter("@order_qty",pModel.order_qty),
                       new SqlParameter("@chk_dat",pModel.check_date),
                       new SqlParameter("@t_dat",pModel.t_complete_date),
                       new SqlParameter("@next_wp_id",pModel.next_wp_id),
                       new SqlParameter("@next_wp_name",pModel.next_wp_name),
                       new SqlParameter("@ver",pModel.ver),
                       new SqlParameter("@color_desc",pModel.color_desc),
                       new SqlParameter("@do_color",pModel.do_color)
                    };
                Result = DBUtility.ExecuteNonQuery(strSQL, paras, false);
            }
            else
            {
                MessageBox.Show("該條數據已存在，請重新輸入信息。");
            }

            return Result;
        }

        public static int DelMo_for_jxLock(string wp_id, string mo_id, string goods_id)
        {
            int Result = -1;

            string strSQL = @" DELETE FROM mo_for_jx WHERE mo_id=@mo_id AND wp_id=@wp_id AND goods_id=@goods_id ";
            SqlParameter[] paras = new SqlParameter[] { 
                new SqlParameter("@mo_id",mo_id),
                new SqlParameter("@wp_id",wp_id),
                new SqlParameter("@goods_id",goods_id)
            };
            Result = DBUtility.ExecuteNonQuery(strSQL, paras, false);

            return Result;
        }

        public static int UpdateMo_for_jxLock(Mo_for_jx pModel)
        {
            int Result = -1;


            string strSQL = "";
            SqlParameter[] paras = null;
            if (pModel.apr_usr != null && pModel.apr_tim != null)
            {
                strSQL += @"  UPDATE mo_for_jx SET apr_usr=@apr_usr, apr_tim=@apr_tim 
                                         WHERE  mo_id=@mo_id AND wp_id=@wp_id AND goods_id=@goods_id";
                paras = new SqlParameter[] { 
                    new SqlParameter("@apr_usr",pModel.apr_usr),
                    new SqlParameter("@apr_tim",pModel.apr_tim),
                    new SqlParameter("@mo_id",pModel.mo_id),
                    new SqlParameter("@wp_id",pModel.wp_id),
                    new SqlParameter("@goods_id",pModel.goods_id)
                };
            }
            else
            {
                strSQL += @"  UPDATE mo_for_jx SET apr_usr=NULL, apr_tim=NULL 
                                         WHERE  mo_id=@mo_id AND wp_id=@wp_id AND goods_id=@goods_id";
                paras = new SqlParameter[] { 
                    new SqlParameter("@mo_id",pModel.mo_id),
                    new SqlParameter("@wp_id",pModel.wp_id),
                    new SqlParameter("@goods_id",pModel.goods_id)
                };
            }
            Result = DBUtility.ExecuteNonQuery(strSQL, paras, false);

            return Result;
        }

        public static int UpdateMo_for_jx(Mo_for_jx pModel)
        {
            int Result = -1;

            string strSQL = @"  UPDATE mo_for_jx SET prod_qty=@prod_qty ,am_usr=@am_usr, am_tim=@am_tim,rmk=@rmk,order_qty=@order_qty
                                        ,t_dat=@t_dat,chk_dat=@chk_dat,next_wp_id=@next_wp_id,next_wp_name=@next_wp_name,color_desc=@color_desc,do_color=@do_color
                                WHERE  mo_id=@mo_id AND wp_id=@wp_id AND goods_id=@goods_id";
            SqlParameter[] paras = new SqlParameter[] { 
                new SqlParameter("@prod_qty",pModel.prod_qty),
                new SqlParameter("@am_usr",pModel.am_usr),
                new SqlParameter("@am_tim",pModel.am_tim),
                new SqlParameter("@mo_id",pModel.mo_id),
                new SqlParameter("@wp_id",pModel.wp_id),
                new SqlParameter("@rmk",pModel.rmk),
                new SqlParameter("@goods_id",pModel.goods_id),
                new SqlParameter("@order_qty",pModel.order_qty),
                new SqlParameter("@t_dat",pModel.t_complete_date),
                new SqlParameter("@chk_dat",pModel.check_date),
                new SqlParameter("@next_wp_id",pModel.next_wp_id),
                new SqlParameter("@next_wp_name",pModel.next_wp_name),
                new SqlParameter("@color_desc",pModel.color_desc),
                new SqlParameter("@do_color",pModel.do_color)
            };
            Result = DBUtility.ExecuteNonQuery(strSQL, paras, false);

            return Result;
        }

        public static DataTable GetMo_for_jx(string pMo_id, string pWp_id, string pGoods_id)
        {
            DataTable dtMo_for_jx = new DataTable();

            string strSQL = @"SELECT mo_date, mo_id, wp_id, goods_id ,goods_name, prod_qty, rmk, cr_usr,chk_dat,order_qty,t_dat,next_wp_id,next_wp_name,cr_tim, am_usr, am_tim FROM mo_for_jx ";
            if (pWp_id != "")
            {
                strSQL += " WHERE wp_id LIKE'%" + pWp_id + "%' ";
            }
            if (pMo_id != "" && pWp_id == "")
            {
                strSQL += " WHERE mo_id LIKE'%" + pMo_id + "%' ";
            }
            else
            {
                if (pMo_id != "")
                {
                    strSQL += " OR mo_id LIKE'%" + pMo_id + "%' ";
                }
            }
            if (pGoods_id != "" && pMo_id == "" && pWp_id == "")
            {
                strSQL += " WHERE goods_id LIKE'%" + pGoods_id + "%' ";
            }
            else
            {
                if (pGoods_id != "")
                {
                    strSQL += " OR goods_id LIKE'%" + pGoods_id + "%' ";
                }
            }
            dtMo_for_jx = DBUtility.GetDataTable(strSQL);

            return dtMo_for_jx;
        }

        /// <summary>
        /// 根據傳入字符串返回條碼值
        /// </summary>
        /// <param name="pBarCodeSource"></param>
        /// <returns></returns>
        public static string ReturnBarCode(string pBarCodeSource)
        {
            string BarCode = "";
            try
            {
                DataTable dtBarCode = new DataTable();
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    string strSQL = @"select dbo.StrToCode128B(rtrim(@ItemCode)) AS item_barcode ";
                    cmd.Parameters.Add(new SqlParameter("@ItemCode", pBarCodeSource));
                    cmd.CommandText = strSQL;
                    cmd.Connection = conn;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtBarCode);
                }

                if (dtBarCode.Rows.Count > 0)
                {
                    BarCode = dtBarCode.Rows[0]["item_barcode"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return BarCode;
        }


    }
}
