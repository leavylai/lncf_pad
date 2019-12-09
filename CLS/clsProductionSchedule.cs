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
    public class clsProductionSchedule
    {
        private static String strConn = DBUtility.dgcf_pad_connectionString;
        private static string remote_db = DBUtility.remote_db;
        /// <summary>
        /// 查詢生產部門
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllPrd_dept()
        {
            DataTable dtDept = new DataTable();

            string strSql = @" select int9loc,int9loc+'--'+int9desc AS int9desc,wip_dep from int09 ";

            dtDept = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            dtDept.Rows.Add();
            dtDept.DefaultView.Sort = "int9loc";
            return dtDept;
        }

        /// <summary>
        /// 查詢工作類型
        /// </summary>
        /// <returns></returns>
        public static DataTable GetWorkType()
        {
            DataTable dtworktype = new DataTable();

            string strSql = @" select work_type_id,rtrim(work_type_desc) as work_type_desc from work_type ";

            dtworktype = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);

            return dtworktype;
        }

        /// <summary>
        /// 獲取相關制單物料信息
        /// </summary>
        /// <param name="mo_id"></param>
        /// <param name="prd_dept"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        //獲取制單資料
        public static DataTable GetMo_dataById(string mo_id, string prd_dept, string item)//(string mo_id, string fdep,string tdep, string item)//
        {
            DataTable dtmo_data = new DataTable();
            try
            {
                string strSql = @" SELECT a.mo_id,a.ver,b.sequence_id,b.wp_id,b.goods_id,c.name as goods_name,Convert(Int,b.prod_qty) AS prod_qty,b.next_wp_id,d.materiel_id AS mat_item,e.name AS mat_item_desc
                                   from jo_bill_mostly a 
                                   INNER join jo_bill_goods_details b on a.within_code=b.within_code and a.id=b.id and a.ver=b.ver
                                   INNER JOIN it_goods c on b.within_code=c.within_code and b.goods_id=c.id 
                                   INNER JOIN jo_bill_materiel_details d ON b.within_code=d.within_code and b.id=d.id and b.ver=d.ver and b.sequence_id=d.upper_sequence
                                   INNER JOIN it_goods e ON d.within_code=e.within_code and d.materiel_id=e.id
                                   WHERE a.within_code='0000'  And a.mo_id = '" + mo_id + "' And b.wp_id = '" + prd_dept + "' ";
                if (item != "")
                {
                    strSql += " And b.goods_id ='" + item + "'";
                }

//                string strSql = @" SELECT DISTINCT b.goods_id,c.name as name,b.prod_qty,b.next_wp_id,d.materiel_id AS mat_item,e.name AS mat_item_desc,b.sequence_id,b.ver
//                       from jo_bill_mostly a 
//                       INNER join jo_bill_goods_details b on a.within_code=b.within_code and a.id=b.id  and a.ver=b.ver
//                       INNER JOIN it_goods c on b.within_code=c.within_code and b.goods_id=c.id 
//                       INNER JOIN jo_bill_materiel_details d ON b.within_code=d.within_code and b.id=d.id and b.ver=d.ver and b.sequence_id=d.upper_sequence
//                       INNER JOIN it_goods e ON d.within_code=e.within_code and d.materiel_id=e.id
//                       WHERE a.within_code='0000'  And a.mo_id = '" + mo_id + "'";
//                if (fdep != "")
//                    strSql += " And b.wp_id = '" + fdep + "' ";
//                if (tdep != "")
//                    strSql += " And b.next_wp_id = '" + tdep + "' ";
//                if (item != "")
//                {
//                    strSql += " And b.goods_id ='" + item + "'";
//                }
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtmo_data);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtmo_data;
        }


        /// <summary>
        /// 獲取相關制單物料信息
        /// </summary>
        /// <param name="mo_id"></param>
        /// <param name="prd_dept"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        //獲取制單資料
        public static DataTable getItemByMo(string mo_id, string prd_dept)
        {
            DataTable dtmo_data = new DataTable();
            try
            {
                string strSql = @" SELECT a.mo_id,a.ver,b.sequence_id,b.wp_id,b.goods_id,c.name as goods_name,Convert(Int,b.prod_qty) AS prod_qty,b.next_wp_id
                                   from jo_bill_mostly a 
                                   INNER join jo_bill_goods_details b on a.within_code=b.within_code and a.id=b.id 
                                   INNER JOIN it_goods c on b.within_code=c.within_code and b.goods_id=c.id 
                                   WHERE a.within_code='0000'  And a.mo_id = '" + mo_id + "' And b.wp_id = '" + prd_dept + "' ";
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtmo_data);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtmo_data;
        }


        //從排期中獲取制單資料
        public static DataTable getDataFromArrangeByMo(string prd_dept,string mo_id, string goods_item)
        {
            string strSql = @"Select prd_dep,prd_mo,prd_item,arrange_qty,arrange_date,arrange_machine,prd_worker" +
            " From product_arrange " +
            " Where prd_dep='" + prd_dept + "' And prd_mo='" + mo_id + "' And prd_item='" + goods_item + "'";
            DataTable dtmo_data = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            return dtmo_data;
        }

        //獲取物料描述
        public static DataTable GetItemDesc(string item)
        {
            DataTable dtitem = new DataTable();
            try
            {
                string strSql = @" SELECT name from it_goods a WHERE a.within_code='0000'  And a.id = '" + item + "' ";
                using (SqlConnection conn = new SqlConnection(DBUtility.conn_str_dgerp2))
                {
                    SqlDataAdapter sda = new SqlDataAdapter(strSql, conn);
                    sda.Fill(dtitem);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtitem;
        }

        //獲取是否存在已生產的記錄
        public static DataTable GetPrdRecords(int rec_id, string dep, string mo, string item, string dat)
        {
            DataTable dtPrdRecords = new DataTable();

            string strSql = @" SELECT prd_start_time,prd_end_time From product_records a 
                         WHERE a.prd_dep=" + "'" + dep + "' " + "  And a.prd_mo = " + "'" + mo + "'" + "  And a.prd_item = " + "'" + item + "'" +
                        " and a.prd_id <> " + rec_id + " and a.prd_date=" + "'" + dat + "'";

            dtPrdRecords = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);

            return dtPrdRecords;
        }

        /// <summary>
        /// 添加制單編號資料
        /// </summary>
        /// <param name="prd_records"></param>
        /// <returns></returns>
        public static int AddProductionRecords(product_records prd_records)
        {
            int Result = 0;

            string strSql = @"insert into product_records (prd_id,prd_pdate,prd_date, prd_dep, prd_mo, prd_item, prd_qty
                                 ,prd_weg, prd_machine, prd_work_type, prd_worker, prd_class
                                 ,prd_group, prd_start_time, prd_end_time, prd_normal_time, prd_ot_time
                                 ,line_num,hour_run_num,hour_std_qty,kg_pcs,crtim,crusr,difficulty_level
                                 ,mat_item,mat_item_desc,mat_item_lot,to_dep,pack_num,work_code,prd_run_qty,speed_lever
                                 ,start_run,end_run,prd_id_ref,sample_no,sample_weg,job_type,work_class,amusr,amtim
                                 ,actual_qty,actual_weg,actual_pack_num,conf_flag,conf_time
                                 ,ok_qty,ok_weg,no_ok_qty,no_ok_weg,member_no,per_hour_std_qty,req_prd_qty,arrange_flag
                                 ,prd_req_time)
                                 Values(@prd_id,@prd_pdate,@prd_date, @prd_dep, @prd_mo, @prd_item, @prd_qty
                                 ,@prd_weg, @prd_machine, @prd_work_type, @prd_worker, @prd_class
                                 ,@prd_group, @prd_start_time, @prd_end_time, @prd_normal_time, @prd_ot_time
                                 ,@line_num, @hour_run_num, @hour_std_qty, @kg_pcs, @crtim, @crusr,@difficulty_level
                                 ,@mat_item,@mat_item_desc,@mat_item_lot,@to_dep,@pack_num,@work_code,@prd_run_qty,@speed_lever
                                 ,@start_run,@end_run,@prd_id_ref,@sample_no,@sample_weg,@job_type,@work_class,@amusr,@amtim
                                 ,@actual_qty,@actual_weg,@actual_pack_num,@conf_flag,@conf_time
                                 ,@ok_qty,@ok_weg,@no_ok_qty,@no_ok_weg,@member_no,@per_hour_std_qty,@req_prd_qty,@arrange_flag
                                 ,@prd_req_time)";

            SqlParameter[] paras = new SqlParameter[]{
                new SqlParameter("@prd_id",prd_records.prd_id),
                new SqlParameter("@prd_pdate",prd_records.prd_pdate),
                new SqlParameter("@prd_date",prd_records.prd_date),
                new SqlParameter("@prd_dep",prd_records.prd_dep),
                new SqlParameter("@prd_mo",prd_records.prd_mo),
                new SqlParameter("@prd_item",prd_records.prd_item),
                new SqlParameter("@prd_qty",prd_records.prd_qty),
                new SqlParameter("@prd_weg",Math.Round(prd_records.prd_weg,2)),
                new SqlParameter("@prd_machine",prd_records.prd_machine),
                new SqlParameter("@prd_work_type",prd_records.prd_work_type),
                new SqlParameter("@prd_worker",prd_records.prd_worker),
                new SqlParameter("@prd_class",prd_records.prd_class),
                new SqlParameter("@prd_group",prd_records.prd_group),
                new SqlParameter("@prd_start_time",prd_records.prd_start_time),
                new SqlParameter("@prd_end_time",prd_records.prd_end_time),
                new SqlParameter("@prd_normal_time",Math.Round(prd_records.prd_normal_time,3)),
                new SqlParameter("@prd_ot_time",Math.Round(prd_records.prd_ot_time,3)),
                new SqlParameter("@line_num",prd_records.line_num),
                new SqlParameter("@hour_run_num",prd_records.hour_run_num),
                new SqlParameter("@hour_std_qty",prd_records.hour_std_qty),
                new SqlParameter("@kg_pcs",prd_records.kg_pcs),
                new SqlParameter("@crtim",prd_records.crtim),
                new SqlParameter("@crusr",prd_records.crusr),
                new SqlParameter("@difficulty_level",prd_records.difficulty_level),
                new SqlParameter("@mat_item",prd_records.mat_item),
                new SqlParameter("@mat_item_desc",prd_records.mat_item_desc),
                new SqlParameter("@mat_item_lot",prd_records.mat_item_lot),
                new SqlParameter("@to_dep",prd_records.to_dep),
                new SqlParameter("@pack_num",prd_records.pack_num),
                new SqlParameter("@work_code",prd_records.work_code),
                new SqlParameter("@prd_run_qty",prd_records.prd_run_qty),
                new SqlParameter("@speed_lever",prd_records.speed_lever),
                new SqlParameter("@start_run",prd_records.start_run),
                new SqlParameter("@end_run",prd_records.end_run),
                new SqlParameter("@prd_id_ref",prd_records.prd_id_ref),
                new SqlParameter("@sample_no",prd_records.sample_no),
                new SqlParameter("@sample_weg",prd_records.sample_weg),
                new SqlParameter("@job_type",prd_records.job_type),
                new SqlParameter("@work_class",prd_records.work_class),
                new SqlParameter("@amtim",prd_records.amtim),
                new SqlParameter("@amusr",prd_records.amusr),
                new SqlParameter("@actual_qty",prd_records.actual_qty),
                new SqlParameter("@actual_weg",prd_records.actual_weg),
                new SqlParameter("@actual_pack_num",prd_records.actual_pack_num),
                new SqlParameter("@conf_flag",prd_records.conf_flag),
                new SqlParameter("@conf_time",prd_records.conf_time),
                new SqlParameter("@ok_qty",prd_records.ok_qty),
                new SqlParameter("@ok_weg",prd_records.ok_weg),
                new SqlParameter("@no_ok_qty",prd_records.no_ok_qty),
                new SqlParameter("@no_ok_weg",prd_records.no_ok_weg),
                new SqlParameter("@member_no",prd_records.member_no),
                new SqlParameter("@per_hour_std_qty",prd_records.per_hour_std_qty),
                new SqlParameter("@req_prd_qty",prd_records.req_prd_qty),
                new SqlParameter("@arrange_flag",prd_records.arrange_flag),
                new SqlParameter("@prd_req_time",prd_records.prd_req_time)
            };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(strSql, paras);

            return Result;
        }

        /// <summary>
        /// 編輯制單編號資料
        /// </summary>
        /// <param name="prd_records"></param>
        /// <returns></returns>
        public static int UpdateProductionRecords(product_records prd_records)
        {
            int Result = 0;

            string strSql = @"update product_records set prd_pdate = @prd_pdate,prd_date = @prd_date,prd_qty =@prd_qty,prd_weg=@prd_weg,prd_machine=@prd_machine,prd_work_type=@prd_work_type
                                ,prd_worker=@prd_worker,prd_class=@prd_class,prd_group=@prd_group,prd_start_time=@prd_start_time,prd_end_time=@prd_end_time
                                ,prd_normal_time=@prd_normal_time,prd_ot_time=@prd_ot_time,line_num=@line_num,hour_run_num=@hour_run_num,hour_std_qty=@hour_std_qty
                                ,kg_pcs=@kg_pcs,amtim=@amtim,amusr=@amusr,difficulty_level=@difficulty_level
                                ,mat_item=@mat_item,mat_item_desc=@mat_item_desc,mat_item_lot=@mat_item_lot,to_dep=@to_dep
                                ,pack_num=@pack_num,work_code=@work_code,prd_run_qty=@prd_run_qty,speed_lever=@speed_lever,start_run=@start_run,end_run=@end_run,prd_id_ref=@prd_id_ref
                                ,sample_no=@sample_no,sample_weg=@sample_weg,job_type=@job_type,work_class=@work_class
                                ,actual_qty=@actual_qty,actual_weg=@actual_weg,actual_pack_num=@actual_pack_num,conf_flag=@conf_flag,conf_time=@conf_time
                                ,ok_qty=@ok_qty,ok_weg=@ok_weg,no_ok_qty=@no_ok_qty,no_ok_weg=@no_ok_weg,member_no=@member_no,per_hour_std_qty=@per_hour_std_qty
                                ,req_prd_qty=@req_prd_qty,prd_req_time=@prd_req_time
                                Where prd_id = @prd_id";

            SqlParameter[] paras = new SqlParameter[] { 
                new SqlParameter("@prd_pdate",prd_records.prd_pdate),
                new SqlParameter("@prd_date",prd_records.prd_date),
                new SqlParameter("@prd_qty",prd_records.prd_qty),
                new SqlParameter("@prd_weg",Math.Round(prd_records.prd_weg,2)),
                new SqlParameter("@prd_machine",prd_records.prd_machine),
                new SqlParameter("@prd_work_type",prd_records.prd_work_type),
                new SqlParameter("@prd_worker",prd_records.prd_worker),
                new SqlParameter("@prd_class",prd_records.prd_class),
                new SqlParameter("@prd_group",prd_records.prd_group),
                new SqlParameter("@prd_start_time",prd_records.prd_start_time),
                new SqlParameter("@prd_end_time",prd_records.prd_end_time),
                new SqlParameter("@prd_normal_time",Math.Round(prd_records.prd_normal_time,3)),
                new SqlParameter("@prd_ot_time",Math.Round(prd_records.prd_ot_time,3)),
                new SqlParameter("@line_num",prd_records.line_num),
                new SqlParameter("@hour_run_num",prd_records.hour_run_num),
                new SqlParameter("@hour_std_qty",prd_records.hour_std_qty),
                new SqlParameter("@kg_pcs",prd_records.kg_pcs),
                new SqlParameter("@amtim",prd_records.amtim),
                new SqlParameter("@amusr",prd_records.amusr),
                new SqlParameter("@prd_id",prd_records.prd_id),
                new SqlParameter("@difficulty_level",prd_records.difficulty_level),
                new SqlParameter("@mat_item",prd_records.mat_item),
                new SqlParameter("@mat_item_desc",prd_records.mat_item_desc),
                new SqlParameter("@mat_item_lot",prd_records.mat_item_lot),
                new SqlParameter("@to_dep",prd_records.to_dep),
                new SqlParameter("@pack_num",prd_records.pack_num),
                new SqlParameter("@work_code",prd_records.work_code),
                new SqlParameter("@prd_run_qty",prd_records.prd_run_qty),
                new SqlParameter("@speed_lever",prd_records.speed_lever),
                new SqlParameter("@start_run",prd_records.start_run),
                new SqlParameter("@end_run",prd_records.end_run),
                new SqlParameter("@prd_id_ref",prd_records.prd_id_ref),
                new SqlParameter("@sample_no",prd_records.sample_no),
                new SqlParameter("@sample_weg",prd_records.sample_weg),
                new SqlParameter("@job_type",prd_records.job_type),
                new SqlParameter("@work_class",prd_records.work_class),
                new SqlParameter("@actual_qty",prd_records.actual_qty),
                new SqlParameter("@actual_weg",prd_records.actual_weg),
                new SqlParameter("@actual_pack_num",prd_records.actual_pack_num),
                new SqlParameter("@conf_flag",prd_records.conf_flag),
                new SqlParameter("@conf_time",prd_records.conf_time),
                new SqlParameter("@ok_qty",prd_records.ok_qty),
                new SqlParameter("@ok_weg",prd_records.ok_weg),
                new SqlParameter("@no_ok_qty",prd_records.no_ok_qty),
                new SqlParameter("@no_ok_weg",prd_records.no_ok_weg),
                new SqlParameter("@member_no",prd_records.member_no),
                new SqlParameter("@per_hour_std_qty",prd_records.per_hour_std_qty),
                new SqlParameter("@req_prd_qty",prd_records.req_prd_qty),
                new SqlParameter("@prd_req_time",prd_records.prd_req_time)
            };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(strSql, paras);

            return Result;
        }

        //更新磅貨數量、重量
        public static int UpdatePrdActualQty(product_records prd_records)
        {
            int Result = 0;

            string strSql = @"update product_records set actual_qty = @actual_qty,actual_weg =@actual_weg,mat_item=@mat_item,mat_item_lot=@mat_item_lot
                                ,to_dep=@to_dep,actual_pack_num=@actual_pack_num,sample_no=@sample_no,sample_weg=@sample_weg,conf_flag=@conf_flag,conf_time=@conf_time
                                  Where prd_id = @prd_id";

            SqlParameter[] paras = new SqlParameter[] { 
                new SqlParameter("@actual_qty",Math.Round(prd_records.actual_qty,2)),
                new SqlParameter("@actual_weg",Math.Round(prd_records.actual_weg,2)),
                new SqlParameter("@to_dep",prd_records.to_dep),
                new SqlParameter("@actual_pack_num",prd_records.actual_pack_num),
                new SqlParameter("@mat_item",prd_records.mat_item),
                new SqlParameter("@mat_item_lot",prd_records.mat_item_lot),
                new SqlParameter("@conf_flag",prd_records.conf_flag),
                new SqlParameter("@conf_time",prd_records.conf_time),
                new SqlParameter("@prd_id",prd_records.prd_id),
                new SqlParameter("@sample_no",prd_records.sample_no),
                new SqlParameter("@sample_weg",prd_records.sample_weg)
            };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(strSql, paras);

            return Result;
        }

        //如果是從生產計劃單中收貨，則要產生記錄號，並將記錄插入生產單中，生產類型為A03
        public static int InsertPrdActualQty(product_records prd_records)
        {
            int Result = 0;

            string strSql = @"insert into product_records (prd_id,prd_date,prd_pdate,prd_dep,prd_mo,prd_item,prd_work_type,actual_qty
                                ,actual_weg,mat_item,mat_item_desc,mat_item_lot,to_dep,sample_no,sample_weg,actual_pack_num,conf_flag,conf_time,crusr,crtim)
                                 Values
                                (@prd_id,@prd_date,@prd_pdate,@prd_dep,@prd_mo,@prd_item,@prd_work_type,@actual_qty
                                ,@actual_weg,@mat_item,@mat_item_desc,@mat_item_lot,@to_dep,@sample_no,@sample_weg,@actual_pack_num,@conf_flag,@conf_time,@crusr,@crtim)";

            SqlParameter[] paras = new SqlParameter[] { 
                new SqlParameter("@actual_qty",Math.Round(prd_records.actual_qty,2)),
                new SqlParameter("@actual_weg",Math.Round(prd_records.actual_weg,2)),
                new SqlParameter("@to_dep",prd_records.to_dep),
                new SqlParameter("@actual_pack_num",prd_records.actual_pack_num),
                new SqlParameter("@mat_item",prd_records.mat_item),
                new SqlParameter("@mat_item_desc",prd_records.mat_item_desc),
                new SqlParameter("@mat_item_lot",prd_records.mat_item_lot),
                new SqlParameter("@conf_flag",prd_records.conf_flag),
                new SqlParameter("@conf_time",prd_records.conf_time),
                new SqlParameter("@prd_id",prd_records.prd_id),
                new SqlParameter("@prd_date",prd_records.prd_date),
                new SqlParameter("@prd_pdate",prd_records.prd_pdate),
                new SqlParameter("@prd_dep",prd_records.prd_dep),
                new SqlParameter("@prd_mo",prd_records.prd_mo),
                new SqlParameter("@prd_item",prd_records.prd_item),
                new SqlParameter("@prd_work_type",prd_records.prd_work_type),
                new SqlParameter("@sample_no",prd_records.sample_no),
                new SqlParameter("@sample_weg",prd_records.sample_weg),
                new SqlParameter("@crusr",prd_records.crusr),
                new SqlParameter("@crtim",prd_records.crtim)
            };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(strSql, paras);

            return Result;
        }

        /// <summary>
        /// 更新物料每KG對應的數量表
        /// </summary>
        /// <param name="prd_records"></param>
        /// <param name="kg_pcs_rate"></param>
        /// <returns></returns>
        public static int InsertOrUpdateItem_rate(product_records prd_records, int kg_pcs_rate)
        {
            int Result = 0;
            string strSql = "";

            if (kg_pcs_rate > 0)
            {
                strSql += @" Update item_rate Set rate=@rate,cr_date=@cr_date
                               Where dep=@dep And mat_item=@mat_item ";
            }
            else
            {
                strSql += @"Insert into item_rate (dep, mat_item, rate, cr_date)
                                                values(@dep,@mat_item,@rate,@cr_date)";
            }
            SqlParameter[] paras = new SqlParameter[] { 
                new SqlParameter("@rate",prd_records.kg_pcs),
                new SqlParameter("@cr_date",prd_records.prd_date),
                new SqlParameter("@dep",prd_records.prd_dep),
                new SqlParameter("@mat_item",prd_records.prd_item)
              };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(strSql, paras);

            return Result;
        }

        /// <summary>
        /// 刪除制單編號的資料
        /// </summary>
        /// <param name="prd_id"></param>
        /// <param name="record_id"></param>
        /// <returns></returns>
        public static int DeleteProductionRecords(int prd_id)
        {
            int Result = 0;
            string strSql = "";
            strSql += string.Format(@" SET XACT_ABORT  ON ");
            strSql += string.Format(@" BEGIN TRANSACTION ");
            strSql += @" DELETE FROM product_records WHERE prd_id = '" + prd_id + "'";
            strSql += string.Format(@" Delete From product_records_worker Where prd_id='" + prd_id + "'");
            strSql += string.Format(@" COMMIT TRANSACTION ");

            Result = clsPublicOfPad.ExecuteSqlUpdate(strSql);
            return Result;
        }

        public static List<product_records> GetPrdMachineStatus(string dept, string worktype, int pageSize)
        {
            List<product_records> lsPms = new List<product_records>();
            DataTable dtBase = new DataTable();
            try
            {

                SqlParameter[] paras = new SqlParameter[]{
                   new SqlParameter("@dept",dept)
                };

                dtBase = clsPublicOfPad.ExecuteProcedure("Produce_Work_Status", paras);
                if (dtBase.Rows.Count > 0)
                {
                    int totalPages;
                    int r = 1;
                    int j = 0;
                    if ((dtBase.Rows.Count % pageSize) == 0)
                    {
                        totalPages = dtBase.Rows.Count / pageSize;
                    }
                    else
                    {
                        totalPages = (dtBase.Rows.Count / pageSize) + 1;
                    }

                    for (int i = 0; i < dtBase.Rows.Count; i++)
                    {
                        if (j == pageSize)
                        {
                            j = 0;
                            r = r + 1;
                        }
                        product_records objModel = new product_records();
                        objModel.prd_id = clsUtility.FormatNullableInt32(dtBase.Rows[i]["prd_id"]);
                        objModel.prd_dep = (dtBase.Rows[i]["prd_dep"].ToString() != null ? dtBase.Rows[i]["prd_dep"].ToString().Trim() : "");
                        objModel.prd_mo = (dtBase.Rows[i]["prd_mo"].ToString() != null ? dtBase.Rows[i]["prd_mo"].ToString().Trim() : "");
                        objModel.prd_item = (dtBase.Rows[i]["prd_item"].ToString() != null ? dtBase.Rows[i]["prd_item"].ToString().Trim() : "");
                        objModel.prd_qty = clsUtility.FormatNullableInt32(dtBase.Rows[i]["prd_qty"]);
                        objModel.prd_date = (dtBase.Rows[i]["prd_date"].ToString() != null ? dtBase.Rows[i]["prd_date"].ToString().Trim() : "");
                        objModel.prd_weg = clsUtility.FormatNullableFloat(dtBase.Rows[i]["prd_weg"]);
                        objModel.machine_desc = (dtBase.Rows[i]["machine_desc"].ToString() != null ? dtBase.Rows[i]["machine_desc"].ToString().Trim() : "");
                        objModel.prd_machine = (dtBase.Rows[i]["machine_id"].ToString() != null ? dtBase.Rows[i]["machine_id"].ToString().Trim() : "");
                        objModel.prd_work_type = (dtBase.Rows[i]["work_type_id"].ToString() != null ? dtBase.Rows[i]["work_type_id"].ToString().Trim() : "");
                        objModel.work_type_decs = (dtBase.Rows[i]["work_type_desc"].ToString() != null ? dtBase.Rows[i]["work_type_desc"].ToString().Trim() : "");
                        objModel.prd_worker = (dtBase.Rows[i]["prd_worker"].ToString() != null ? dtBase.Rows[i]["prd_worker"].ToString().Trim() : "");
                        objModel.prd_class = (dtBase.Rows[i]["prd_class"].ToString() != null ? dtBase.Rows[i]["prd_class"].ToString().Trim() : "");
                        objModel.prd_group = (dtBase.Rows[i]["prd_group"].ToString() != null ? dtBase.Rows[i]["prd_group"].ToString().Trim() : "");
                        objModel.prd_start_time = (dtBase.Rows[i]["prd_start_time"].ToString() != null ? dtBase.Rows[i]["prd_start_time"].ToString().Trim() : "");
                        objModel.prd_end_time = (dtBase.Rows[i]["prd_end_time"].ToString() != null ? dtBase.Rows[i]["prd_end_time"].ToString().Trim() : "");
                        objModel.prd_normal_time = clsUtility.FormatNullableFloat(dtBase.Rows[i]["prd_normal_time"]);
                        objModel.prd_ot_time = clsUtility.FormatNullableFloat(dtBase.Rows[i]["prd_ot_time"]);
                        objModel.line_num = clsUtility.FormatNullableInt32(dtBase.Rows[i]["line_num"]);
                        objModel.hour_run_num = clsUtility.FormatNullableInt32(dtBase.Rows[i]["hour_run_num"]);
                        objModel.hour_std_qty = clsUtility.FormatNullableInt32(dtBase.Rows[i]["hour_std_qty"]);
                        objModel.kg_pcs = clsUtility.FormatNullableInt32(dtBase.Rows[i]["kg_pcs"]);
                        objModel.qc_flag = (dtBase.Rows[i]["qc_flag"].ToString() != null ? dtBase.Rows[i]["qc_flag"].ToString().Trim() : "");
                        objModel.pageIndex = r;

                        lsPms.Add(objModel);

                        j++;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return lsPms;
        }

        //提取機器生產狀態
        public static DataTable GetPrdMachineStatus1(int select_type, string dept, string machine_id)
        {
            DataTable dtBase = new DataTable();
            try
            {

                SqlParameter[] paras = new SqlParameter[]{
                   new SqlParameter("@select_type",select_type)
                   ,new SqlParameter("@dep",dept)
                   ,new SqlParameter("@machine_id",machine_id)
                };

                dtBase = clsPublicOfPad.ExecuteProcedure("machinestatus", paras);//DBUtility.pad_db + 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtBase;
        }
        //提取生產記錄，用於磅貨使用
        public static DataTable GetProductQtyConfirm(string dep, string prd_date1, string prd_date2, string mo1, string item, string conf_date1, string conf_date2, string qc_flag, string conf_flag)
        {
            DataTable dtBase = new DataTable();
            try
            {
                SqlParameter[] paras = new SqlParameter[]{
                   new SqlParameter("@dep",dep)
                   ,new SqlParameter("@prd_date1",prd_date1)
                   ,new SqlParameter("@prd_date2",prd_date2)
                   ,new SqlParameter("@mo1",mo1)
                   ,new SqlParameter("@conf_date1",conf_date1)
                   ,new SqlParameter("@conf_date2",conf_date2)
                   ,new SqlParameter("@qc_flag",qc_flag)
                   ,new SqlParameter("@conf_flag",conf_flag)
                   ,new SqlParameter("@item",item)
                };

                dtBase = clsPublicOfPad.ExecuteProcedure("usp_ProductQtyConfirm", paras);//DBUtility.pad_db + 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtBase;
        }

        /// 更新工號表時，先檢查是否存在，若存在則先刪除，後重新插入
        /// </summary>
        /// <param name="prd_id"></param>
        /// <returns></returns>
        public static int DeletePrdWorker(int prd_id)
        {
            int Result = 0;
            try
            {
                string strSql1 = @" SELECT prd_id FROM product_records_worker 
                                   WHERE prd_id = '" + prd_id + "'";
                DataTable dtres = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql1);
                if (dtres.Rows.Count > 0)
                {
                    strSql1 = @" DELETE FROM product_records_worker WHERE prd_id = '" + prd_id + "'";
                    using (SqlConnection conn = new SqlConnection(strConn))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(strSql1, conn);
                        Result = cmd.ExecuteNonQuery();
                    }
                }
                else
                    Result = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        public static int AddPrdDefective(int prd_id, string seq, string defective_id, string oth_defective, string crusr, DateTime crtim)
        {
            int Result = 0;
            try
            {
                string strSql = @"insert into product_records_defective (prd_id, seq,defective_id,oth_defective,crusr,crtim)
                                                     Values(@prd_id, @seq,@defective_id,@oth_defective,@crusr,@crtim)";

                SqlParameter[] paras = new SqlParameter[]{
                    new SqlParameter("@prd_id",prd_id),
                    new SqlParameter("@seq",seq),
                    new SqlParameter("@defective_id",defective_id),
                    new SqlParameter("@oth_defective",oth_defective),
                    new SqlParameter("@crusr",crusr),
                    new SqlParameter("@crtim",crtim)
                };

                Result = clsPublicOfPad.ExecuteNonQuery(strSql, paras, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        //刪除次品記錄表
        public static int DeletePrdDefective(int prd_id)
        {
            int Result = 0;
            try
            {
                string strSql1 = @" SELECT prd_id FROM product_records_defective 
                                   WHERE prd_id = '" + prd_id + "'";
                DataTable dtres = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql1);
                if (dtres.Rows.Count > 0)
                {
                    strSql1 = @" DELETE FROM product_records_defective WHERE prd_id = '" + prd_id + "'";

                    Result = clsPublicOfPad.ExecuteNonQuery(strSql1, null, false);
                }
                else
                    Result = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return Result;
        }

        public static int AddPrdWorker(int prd_id, string prd_worker, string crusr, DateTime crtim)
        {
            int Result = 0;

            string strSql = @"insert into product_records_worker (prd_id, prd_worker,crusr,crtim)
                                                     Values(@prd_id, @prd_worker,@crusr,@crtim)";

            SqlParameter[] paras = new SqlParameter[]{
                new SqlParameter("@prd_id",prd_id),
                new SqlParameter("@prd_worker",prd_worker),
                new SqlParameter("@crusr",crusr),
                new SqlParameter("@crtim",crtim)
                };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(strSql, paras);

            return Result;
        }

        /// <summary>
        /// 新增組別成員
        /// </summary>
        /// <param name="prd_dep"></param>
        /// <param name="prd_group"></param>
        /// <param name="prd_worker"></param>
        /// <returns></returns>
        public static int AddWorkerGroup(string prd_dep, string prd_group, string prd_worker)
        {
            int Result = 0;

            string Sql = @"INSERT INTO product_group_member (prd_dep, prd_group, prd_worker, crusr, crtim) VALUES(@prd_dep, @prd_group, @prd_worker, @crusr, @crtim) ";
            SqlParameter[] paras = new SqlParameter[] { 
               new SqlParameter("@prd_dep",prd_dep),
               new SqlParameter("@prd_worker",prd_worker),
               new SqlParameter("@prd_group",prd_group),
               new SqlParameter("@crusr",DBUtility._user_id),
               new SqlParameter("@crtim",DateTime.Now)
            };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(Sql, paras);

            return Result;
        }


        /// <summary>
        /// 變更組員所屬組別
        /// </summary>
        /// <param name="objGroupMenber"></param>
        /// <returns></returns>
        public static int UpdateWorkerGroup(product_group_member objGroupMenber)
        {
            int Result = 0;

            string Sql = @" UPDATE product_group_member SET prd_group=@prd_group_In,crusr=@crusr,crtim=@crtim 
                            WHERE prd_dep=@prd_dep AND prd_group=@prd_group_Out AND prd_worker=@prd_worker ";
            SqlParameter[] paras = new SqlParameter[] { 
               new SqlParameter("@prd_dep",objGroupMenber.prd_dep),
               new SqlParameter("@prd_worker",objGroupMenber.prd_worker),
               new SqlParameter("@prd_group_In",objGroupMenber.prd_group_In),
               new SqlParameter("@prd_group_Out",objGroupMenber.prd_group_Out),
               new SqlParameter("@crusr",objGroupMenber.crusr),
               new SqlParameter("@crtim",objGroupMenber.crtim)
            };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(Sql, paras);

            return Result;
        }

        /// <summary>
        /// 刪除組別成員
        /// </summary>
        /// <param name="prd_dep"></param>
        /// <param name="prd_group"></param>
        /// <param name="prd_worker"></param>
        /// <returns></returns>
        public static int DelWorkerGroup(string prd_dep, string prd_group, string prd_worker)
        {
            int Result = 0;
            string strSql = @"DELETE FROM product_group_member WHERE prd_dep=@prd_dep AND prd_group=@prd_group AND prd_worker=@prd_worker ";

            SqlParameter[] paras = new SqlParameter[] { 
               new SqlParameter("@prd_dep",prd_dep),
               new SqlParameter("@prd_worker",prd_worker),
               new SqlParameter("@prd_group",prd_group)
            };
            Result = clsPublicOfPad.ExecuteNonQueryReturnInt(strSql, paras);

            return Result;
        }

        //獲取總生產數
        public static DataTable get_total_prd_qty(string dep,string mo,string item)
        {
            DataTable db_show_qty = new DataTable();

            string sql = "";
            sql += " Select sum(prd_qty) as prd_qty From product_records a with(nolock)"+
                " Where a.prd_dep = " + "'" + dep + "'"+
                " And a.prd_mo = " + "'" + mo + "'"+
                " And a.prd_item = " + "'" + item + "'"+
                " And a.prd_work_type = '"+"A02"+"'"+
                " And a.prd_start_time <> '' " + " And a.prd_end_time <> '' ";
            db_show_qty = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
            return db_show_qty;
        }


        //獲取物料的每公斤對應數量
        public static int get_kg_pcs_rate(string dep,string goods_item)
        {
            DataTable dtItem_kg_pcs = null;
            int kg_pcs_rate = 0;
            try
            {
                //獲取制單編號資料
                string sql = " select dep,mat_item,rate,cr_date  from item_rate ";
                sql += " Where dep = " + "'" + dep + "'";
                sql += " And mat_item = " + "'" + goods_item + "'";

                dtItem_kg_pcs = clsPublicOfPad.ExecuteSqlReturnDataTable(sql);
                if (dtItem_kg_pcs.Rows.Count > 0)
                    kg_pcs_rate = Convert.ToInt32(dtItem_kg_pcs.Rows[0]["rate"].ToString());
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
            return kg_pcs_rate;
        }

        //從外發申請中查找供應商編號、電鍍顏色
        public static DataTable GetVendorFromWp(string doc_id, string dep, string tdep, string mo_id, string item)
        {
            string strSql = "";
            string doc_type = "R";
            if (doc_id != "")
                doc_type = doc_id.Substring(0, 1);
            DataTable dtItem = new DataTable();
            strSql = "Select a.vendor_id,d.name AS vendor_name,f.name AS color_name,f.do_color,d.logogram" +
                    " FROM " + remote_db + "op_outpro_mostly a" +
                    " Inner Join " + remote_db + "op_outpro_goods_details b ON a.within_code=b.within_code AND a.id=b.id" +
                    " Inner Join " + remote_db + "op_outpro_materiel_details c ON b.within_code=c.within_code AND b.id=c.id AND b.sequence_id=c.upper_sequence" +
                    " Left Outer Join " + remote_db + "it_vendor d ON a.within_code=d.within_code AND a.vendor_id=d.id" +
                    " Inner Join " + remote_db + "it_goods e ON b.within_code=e.within_code AND b.goods_id=e.id" +
                    " Left Outer Join " + remote_db + "cd_color f ON e.within_code=f.within_code AND e.color=f.id" +
                    " WHERE a.within_code='" + "0000" + "'" + " AND a.department_id ='" + tdep + "' " + " AND b.mo_id ='" + mo_id + "' ";
            if (doc_type != "R")//如果是本部門發出的移交單，則從申請單的原料中提取供應商
                strSql += " AND c.materiel_id ='" + item + "' ";
            else
                strSql += " AND b.goods_id ='" + item + "' ";//如果是上部門退的移交單，則說明是已電好的成品，需再返電的
            try
            {
                dtItem = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
                if (dtItem.Rows.Count == 0)
                    MessageBox.Show("申請供商編號不存在!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return dtItem;
        }

        //從生產流程中獲取外發加工商
        public static DataTable GetVendDataFromWip(string mo_id, string in_dept, string goods_id)
        {
            string strSql = "";
            strSql = "Select e.name AS color_name,e.do_color,b.predept_rechange_qty AS rec_qty,b.predept_rechange_sec_qty AS rec_weg ";
            strSql += " From " + remote_db + "jo_bill_mostly a";
            strSql += " Inner Join " + remote_db + "jo_bill_goods_details b On a.within_code=b.within_code AND a.id=b.id";
            strSql += " Inner Join " + remote_db + "jo_bill_materiel_details c ON b.within_code=c.within_code AND b.id=c.id AND b.sequence_id=c.upper_sequence";
            strSql += " Inner Join " + remote_db + "it_goods d ON a.within_code=d.within_code AND b.goods_id=d.id";
            strSql += " Left Join " + remote_db + "cd_color e ON d.within_code=e.within_code AND d.color=e.id";
            strSql += " Where a.within_code='0000'" + " AND a.mo_id='" + mo_id +
                "' AND b.wp_id='" + in_dept + "' AND c.materiel_id='" + goods_id + "'";
            DataTable vtb = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            return vtb;
        }

        //從生產流程中獲取上部門的移交數
        public static DataTable GetRecQtyFromWip(string mo_id, string in_dept, string goods_id)
        {
            string strSql = "";
            strSql = "Select Convert(INT,b.prod_qty) AS pre_prd_qty,Convert(INT,b.c_qty_ok) AS pre_ok_qty,Convert(numeric(18, 2),b.c_sec_qty_ok) AS pre_ok_weg ";
            strSql += " From " + remote_db + "jo_bill_mostly a";
            strSql += " Inner Join " + remote_db + "jo_bill_goods_details b On a.within_code=b.within_code AND a.id=b.id";
            strSql += " Where a.within_code='0000'" + " AND a.mo_id='" + mo_id +
                "' AND b.next_wp_id='" + in_dept + "' AND b.goods_id='" + goods_id + "'";
            DataTable vtb = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            return vtb;
        }

        //獲取制單的庫存數量
        public static DataTable GetStQtyFromWip(string mo_id, string dep, string goods_id)
        {
            string strSql = "";
            strSql = "Select Convert(INT,Sum(a.qty)) AS st_qty,Convert(numeric(18, 2),Sum(a.sec_qty)) AS st_weg ";
            strSql += " From " + remote_db + "st_details_lot a";
            strSql += " Where a.within_code='0000'" + " AND a.mo_id='" + mo_id +
                "' AND a.location_id='" + dep + "' AND a.carton_code='" + dep + "' AND a.goods_id='" + goods_id + "'";
            DataTable dt = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            return dt;
        }

        public static int UpdateGoodsTransferJx(product_transfer_jx_details objModel)
        {
            int Result = 0;
            string strSql = "";
            string Prd_dep=objModel.Prd_dep;
            string Prd_mo=objModel.Prd_mo;
            string Prd_item=objModel.Prd_item;
            string Transfer_date = objModel.Transfer_date;
            int Transfer_flag = objModel.Transfer_flag;
            decimal Transfer_qty = objModel.Transfer_qty;
            decimal Transfer_weg = objModel.Transfer_weg;
            decimal In_qty = 0, In_weg = 0;
            decimal Out_qty = 0, Out_weg = 0;
            string In_date = "", Out_date = "";
            string Crusr = objModel.Crusr;
            string Crtim = objModel.Crtim;

            strSql += string.Format(@" SET XACT_ABORT  ON ");
            strSql += string.Format(@" BEGIN TRANSACTION ");
            strSql += string.Format(@"Insert Into product_transfer_jx_details (Transfer_date,Prd_dep,prd_item,prd_mo,Transfer_flag,transfer_qty,transfer_weg,to_dep,crusr,crtim) Values " +
                "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')"
                , Transfer_date, Prd_dep, Prd_item, Prd_mo, objModel.Transfer_flag, objModel.Transfer_qty, objModel.Transfer_weg, objModel.To_dep, objModel.Crusr, objModel.Crtim);

            DataTable dtStore = checkStore(Prd_dep, Prd_item, Prd_mo);
            if (dtStore.Rows.Count == 0)
            {
                if (Transfer_flag == 0)
                {
                    Out_date = Crtim;
                    Out_qty = Transfer_qty;
                    Out_weg = Transfer_weg;
                }
                else
                {
                    In_date = Crtim;
                    In_qty = Transfer_qty;
                    In_weg = Transfer_weg;
                }
                strSql += string.Format(@"Insert Into product_transfer_jx_summary (Prd_dep,Prd_item,Prd_mo,In_qty,In_weg,In_date,Out_qty,Out_weg,Out_date,Cpl_flag,crusr,crtim) Values " +
                "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')"
                , Prd_dep, Prd_item, Prd_mo, In_qty, In_weg, In_date, Out_qty, Out_weg, Out_date, "", Crusr, Crtim);
            }
            else
            {
                DataRow dr = dtStore.Rows[0];
                Out_date = dr["Out_date"].ToString();
                In_date = dr["In_date"].ToString();
                In_weg = dr["In_weg"].ToString() != "" ? Convert.ToDecimal(dr["In_weg"].ToString()) : 0;
                In_qty = dr["In_qty"].ToString() != "" ? Convert.ToDecimal(dr["In_qty"].ToString()) : 0;
                Out_weg = dr["Out_weg"].ToString() != "" ? Convert.ToDecimal(dr["Out_weg"].ToString()) : 0;
                Out_qty = dr["Out_qty"].ToString() != "" ? Convert.ToDecimal(dr["Out_qty"].ToString()) : 0;
                if (Transfer_flag == 0)
                {
                    Out_date = Crtim;
                    Out_qty = Out_qty + Transfer_qty;
                    Out_weg = Out_weg + Transfer_weg;
                }
                else
                {
                    In_date = Crtim;
                    In_qty = In_qty + Transfer_qty;
                    In_weg = In_weg + Transfer_weg;
                }
                strSql += string.Format(@"Update product_transfer_jx_summary Set In_qty='{0}',In_weg='{1}',Out_qty='{2}',Out_weg='{3}',In_date='{4}',Out_date='{5}',Cpl_flag='',Crusr='{6}',Crtim='{7}'" +
                " Where Prd_dep='{8}' And Prd_item='{9}' And Prd_mo='{10}'"
                , In_qty, In_weg, Out_qty, Out_weg, In_date, Out_date, Crusr, Crtim, Prd_dep, Prd_item, Prd_mo);
            }

            strSql += string.Format(@" COMMIT TRANSACTION ");

            Result = clsPublicOfPad.ExecuteSqlUpdate(strSql);
            return Result;

        }

        private static DataTable checkStore(string Prd_dep, string Prd_item, string Prd_mo)
        {
            string strSql = "Select * From product_transfer_jx_summary Where Prd_dep='" + Prd_dep + "' AND Prd_item='" + Prd_item + "' AND Prd_mo='" + Prd_mo + "'";
            DataTable dt = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            return dt;
        }
        public static DataTable CheckGoodsTransferJx(string Prd_dep,string Prd_item,string Prd_mo,int Transfer_flag)
        {
            string strSql = "Select * From product_transfer_jx_details Where Prd_dep='" + Prd_dep + "' AND Prd_item='" + Prd_item + "' AND Prd_mo='" + Prd_mo
                + "' AND Transfer_flag='" + Transfer_flag + "'";
            DataTable dt = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            return dt;
        }

        public static DataTable getJobType(string dep)
        {
            string strSql = "Select job_type,RTRIM(job_type)+'--'+RTRIM(job_desc) AS job_desc From job_type Where dep='" + dep + "'" + " ORDER BY job_type";
            DataTable dtJobType = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            DataRow dr2 = dtJobType.NewRow();
            dr2[0] = ""; //通过索引赋值
            dr2[1] = "";
            dtJobType.Rows.Add(dr2);
            dtJobType.DefaultView.Sort = "job_type";
            return dtJobType;
        }


        public static int updateProductWorker(List <mdlProductWorker> lsModel,int prd_id)
        {
            int Result = 0;
            string strSql = "";
            strSql += string.Format(@" SET XACT_ABORT  ON ");
            strSql += string.Format(@" BEGIN TRANSACTION ");
            strSql += string.Format(@" Delete From product_records_worker Where prd_id='" + prd_id + "'");
            for (int i = 0; i < lsModel.Count; i++)
            {
                strSql += string.Format(@" Insert Into product_records_worker (prd_id,prd_worker,crusr,crtim) Values " +
                    "('{0}','{1}','{2}','{3}')"
                    , lsModel[i].prdId, lsModel[i].prdWorker, lsModel[i].crusr, lsModel[i].crtim);
            }
            strSql += string.Format(@" COMMIT TRANSACTION ");

            Result = clsPublicOfPad.ExecuteSqlUpdate(strSql);
            return Result;
        }
        public static DataTable loadProductWorker(int prdId)
        {
            string strSql = "Select a.prd_worker,b.hrm1name AS workerName" +
                " From product_records_worker a" +
                " Left Join lnfs1.hr_db.dbo.hrm01 b ON a.prd_worker COLLATE chinese_taiwan_stroke_CI_AS=b.hrm1wid" +
                " Where a.prd_id='" + prdId + "'";
            DataTable dtWorker = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            return dtWorker;
        }

        public static DataTable getWorkCodeList(string dep)
        {
            string strSql = "";
            strSql = " SELECT machine_id,RTRIM(machine_desc) AS machine_desc FROM machine_std WHERE dep='" + dep + "' ";
            DataTable dtMacStd = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            DataRow dr2 = dtMacStd.NewRow();
            dr2[0] = ""; //通过索引赋值
            dr2[1] = "";
            dtMacStd.Rows.Add(dr2);
            dtMacStd.DefaultView.Sort = "machine_id";
            return dtMacStd;
        }
        public static string getWorkCodeStd(string dep,string machine_id)
        {
            string result = "";
            string strSql = " SELECT machine_std_qty FROM machine_std WHERE dep='" + dep + "' AND machine_id='" + machine_id + "'";
            DataTable dtMacStd = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            if (dtMacStd.Rows.Count > 0)
            {
                result = dtMacStd.Rows[0]["machine_std_qty"].ToString();
            }
            return result;
        }
        public static string getDefaultWorkCode(string dep, string sizeId1, string sizeId2)
        {
            string result = "";
            string strSql = "Select machine_id From machine_std Where dep='" + dep + "' And size_id>='" + sizeId1 + "' And size_id1<='" + sizeId2 + "'";
            DataTable dtWorkCode = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            if (dtWorkCode.Rows.Count > 0)
                result = dtWorkCode.Rows[0]["machine_id"].ToString();
            return result;
        }
    }
}
