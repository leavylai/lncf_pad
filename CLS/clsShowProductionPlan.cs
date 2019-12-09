using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace cf_pad.CLS
{
    public class clsShowProductionPlan
    {
        /// <summary>
        /// 獲取生產計劃主表記錄
        /// </summary>
        /// <param name="pMo_id">頁數</param>
        /// <returns></returns>
        public static DataTable GetProductionPlanMostly(string pMo_id)
        {
            DataTable dtPlanMostly = new DataTable();
            try
            {
                string strSql = @" SELECT a.id,a.ver,a.bill_date,a.bill_origin,a.create_by
                                        ,a.create_date,a.update_by,a.update_date,a.check_by,a.check_date
                                        ,a.bill_type,a.remark,a.update_count,a.mo_id,a.mo_ver
                                        ,a.order_no,a.so_sequence_id,a.so_ver,a.f_production_date,a.t_production_date
                                        ,a.customer_id,c.goods_id,c.order_qty,c.goods_unit,c.plate_remark,b.name as cust_name
                                        ,d.production_remark,d.free,e.name, (Case a.state When '0' Then '未批准' When '1' Then '已批准' When '3' Then '生產中' When '4' Then '生產完成' Else '未知狀態' End)as prd_State
                                FROM jo_bill_mostly  as a with(nolock)
                                INNER JOIN it_customer b on a.within_code=b.within_code and a.customer_id=b.id
                                LEFT OUTER JOIN so_order_details c with(nolock) on a.within_code=c.within_code and a.mo_id=c.mo_id and a.order_no=c.id and a.so_sequence_id=c.sequence_id
                                LEFT OUTER JOIN so_order_special_info d with(nolock) on c.within_code=d.within_code and c.id=d.id and c.ver=d.ver and c.sequence_id=d.upper_sequence
                                INNER JOIN cd_personnel e on a.within_code=e.within_code and a.merchandiser=e.id
                                WHERE A.within_code='0000' AND A.mo_id='" + pMo_id + "' ";
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtPlanMostly);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtPlanMostly;
        }

        /// <summary>
        /// 獲取生產計劃明細記錄
        /// </summary>
        /// <param name="pId">生產計劃ID</param>
        /// <returns></returns>
        public static DataTable GetProductionPlanDetails(string pId)
        {
            DataTable dtPlanDetails = new DataTable();
            try
            {
                string strSql = @"SELECT a.hold,a.flag,a.goods_id,CONVERT(decimal(28,0),a.color_qty)as color_qty,CONVERT(decimal(28,0),a.s_qty) as s_qty,CONVERT(decimal(28,0),a.prod_qty) as prod_qty
                                        ,CONVERT(decimal(28,0),a.OBLIGATE_QTY) as obligate_qty,a.goods_unit,a.wp_id,d.name as wp_name
                                        ,a.next_wp_id,'' as next_wp_name,CONVERT(decimal(28,0),a.order_qty) as order_qty,CONVERT(decimal(28,0),a.c_qty_ok) as c_qty_ok
                                        ,CONVERT(decimal(28,2),a.c_sec_qty_ok) as c_sec_qty_ok,a.EQUIPMENT_ID,a.EQUIPMENT_name
                                        ,a.shading_color,a.shading_color_state,a.flevel,a.qty_on_hand,a.remark,a.t_production_date
                                        ,CONVERT(varchar(10),a.t_complete_date,111) as t_complete_date,CONVERT(varchar(10),a.f_complete_date,111) as f_complete_date,CONVERT(decimal(28,0),a.predept_rechange_qty) as predept_rechange_qty
                                        ,CONVERT(decimal(28,2),a.predept_rechange_sec_qty) as predept_rechange_sec_qty,CONVERT(varchar(10),a.predept_rechange_date,111) as predept_rechange_date
                                        ,a.pre_dept,'' as pre_dept_name,a.vendor_id,b.name as goods_name,b.do_color,c.name as vendor_name,'' as prod_state
                                FROM jo_bill_goods_details AS a  
                                INNER JOIN it_goods b on a.within_code=b.within_code and a.goods_id=b.id
                                LEFT JOIN it_vendor c on a.within_code=c.within_code and a.vendor_id=c.id
                                LEFT JOIN cd_department d on a.within_code=d.within_code and a.wp_id=d.id
                                WHERE a.within_code='0000' AND a.id='" + pId + "' ORDER BY  CONVERT(int, a.flag) ";

                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtPlanDetails);
                }

                if (dtPlanDetails.Rows.Count > 0)
                {
                    DataTable dtDept = clsPrdTransfer.GetAllDepartment();
                    for (int i = 0; i < dtPlanDetails.Rows.Count; i++)
                    {
                        //下一部門
                        string strNext_wp_dept = dtPlanDetails.Rows[i]["next_wp_id"].ToString();
                        if (strNext_wp_dept != "")
                        {
                            DataRow[] dr1 = dtDept.Select("id='" + strNext_wp_dept + "'");
                            dtPlanDetails.Rows[i]["next_wp_name"] = dr1[0]["name"].ToString();
                        }

                        //上部門
                        string strPre_dept = dtPlanDetails.Rows[i]["pre_dept"].ToString();
                        if (strPre_dept != "")
                        {
                            DataRow[] dr2 = dtDept.Select("id='" + strPre_dept + "'");
                            dtPlanDetails.Rows[i]["pre_dept_name"] = dr2[0]["name"].ToString();
                        }

                        int complete_qty = clsUtility.FormatNullableInt32(dtPlanDetails.Rows[i]["c_qty_ok"]);
                        int prod_qty = clsUtility.FormatNullableInt32(dtPlanDetails.Rows[i]["prod_qty"]);
                        int obligate_qty = clsUtility.FormatNullableInt32(dtPlanDetails.Rows[i]["OBLIGATE_QTY"]);
                        if ((prod_qty + obligate_qty) > 0 || complete_qty > 0)  //判斷生產狀態
                        {
                            if (complete_qty >= (prod_qty + obligate_qty))
                            {
                                dtPlanDetails.Rows[i]["prod_state"] = "完成";
                            }
                            else
                            {
                                dtPlanDetails.Rows[i]["prod_state"] = "未完成";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtPlanDetails;
        }

        /// <summary>
        /// 獲取圖樣
        /// </summary>
        /// <param name="pGoods_id">貨品編號</param>
        /// <returns></returns>
        public static string GetImagePath(string pGoods_id)
        {
            string strImagePath = "";
            DataTable dtTemp = new DataTable();
            try
            {
                string strSql = @"SELECT b.sequence_id AS art_id,b.picture_name
                                  FROM  dbo.it_goods a 
                                  LEFT JOIN dbo.cd_pattern_details b ON a.within_code=b.within_code AND a.blueprint_id=b.id                          
                                  WHERE a.within_code='0000' and a.id='" + pGoods_id + "' ";
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtTemp);
                }

                if (dtTemp.Rows.Count > 0)
                {
                    strImagePath = dtTemp.Rows[0]["picture_name"].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return strImagePath;
        }



    }
}
