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
    public class clsTakeMoSample
    {

        private static String remote_db = DBUtility.remote_db;
        private static String within_code=DBUtility.within_code;

        //獲取倉庫發料的記錄
        public static DataTable GetFlRec(string loc, string mo_id, string goods_id)
        {
            DataTable dtFlRec = new DataTable();

            string strSql = @" SELECT a.id,a.location_id,a.mo_id,a.goods_id,a.lot_no,a.obligate_mo_id,a.issue_qty,Convert(INT,a.qty) AS qty,a.obligate_qty" +
                ",b.name AS goods_name" +
                " From mrp_st_details_lot a " +
                    " Left Join it_goods b ON a.within_code=b.within_code AND a.goods_id=b.id" +
                    " WHERE a.within_code='" + within_code + "'  And a.location_id = '" + loc + "' And a.mo_id = '" + mo_id +
                    "' AND a.goods_id='" + goods_id + "'";
            dtFlRec = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtFlRec;
        }
        //從生產計劃中獲取倉庫發料的記錄
        public static DataTable GetFlRecFromJo(string id,string seq)
        {
            DataTable dtFlRec = new DataTable();
            string strSql = "";
            strSql = @" SELECT c.mrp_id,c.location,a.mo_id,b.goods_id,c.lot_no,c.obligate_mo_id,c.materiel_id,Convert(INT,c.fl_qty) AS fl_qty" +
                ",d.name AS goods_name,a.id,c.sequence_id As jo_sub_sequence_id,a.order_no,a.so_sequence_id,Convert(INT,b.order_qty) As order_qty " +
                ",Convert(INT,c.i_qty) AS i_qty"+
                " From jo_bill_mostly a " +
                " Inner Join jo_bill_goods_details b on a.within_code=b.within_code and a.id=b.id and a.ver=b.ver" +
                " Inner Join jo_bill_materiel_details c on b.within_code=c.within_code and b.id=c.id and b.ver=c.ver and b.sequence_id=c.upper_sequence" +
                " Left Join it_goods d ON c.within_code=d.within_code AND c.materiel_id=d.id" + "'";


            strSql = @" SELECT a.id As mrp_id,b.ii_location As location,b.mo_id,b.goods_id,b.lot_no,b.obligate_mo_id,b.goods_id As materiel_id,Convert(INT,b.issues_qty) AS fl_qty" +
                ",c.name AS goods_name,a.id,b.sequence_id As jo_sub_sequence_id,a.id As order_no,b.sequence_id As so_sequence_id,Convert(INT,b.issues_qty) As order_qty " +
                ",Convert(INT,b.issues_qty) AS i_qty" +
                " From so_issues_mostly a " +
                " Inner Join so_issues_details b on a.within_code=b.within_code and a.id=b.id" +
                " Left Join it_goods c ON b.within_code=c.within_code AND b.goods_id=c.id" +
                " WHERE a.within_code='" + within_code + "' And a.id='" + id + "' And b.sequence_id='" + seq + "'";

            dtFlRec = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtFlRec;
        }
        public static int GetRate(string goods_id)
        {
            int result = 0;
            string strSql = @"Select rate from it_coding where within_code='" + within_code + "' AND id='" + goods_id + "' AND basic_unit='PCS' AND unit_code='KG'";
            DataTable dtRate = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            if (dtRate.Rows.Count > 0)
                result = (dtRate.Rows[0]["rate"].ToString() != "" ? Convert.ToInt32(dtRate.Rows[0]["rate"]) : 0);
            return result;
        }
        //產生新的發料單號
        public static string AddNewDoc(takemosample objModel)
        {
            int result = 0;
            string strSql = "";
            string strSql_f = "";
            string id = "";
            string id1 = "";
            string bill_id = "SA02";
            string bill_code = "";
            string bill_text1 = "ADI";
            string dat = objModel.con_date;//"2000/01/01";
            string year_month = dat.Substring(2, 2) + dat.Substring(5, 2);
            id1 = "ADI" + year_month;
            strSql_f = "Select bill_code From sys_bill_max_separate Where within_code='" + within_code + "' AND bill_id='" + bill_id +
            "' AND year_month='" + year_month + "' AND bill_text1='" + bill_text1 + "'";
            DataTable tbGenNo = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql_f);
            if (tbGenNo.Rows.Count > 0)
            {
                bill_code = tbGenNo.Rows[0]["bill_code"].ToString();
                bill_code = id1 + (Convert.ToInt32(bill_code.Substring(7, 6)) + 1).ToString().PadLeft(6, '0');
                strSql += string.Format(@"Update sys_bill_max_separate Set bill_code='{0}' Where within_code='{1}' AND bill_id='{2}' AND year_month='{3}' AND bill_text1='{4}'"
                    , bill_code, within_code, bill_id, year_month,bill_text1);
            }
            else
            {
                bill_code = id1 + "000001";
                strSql += string.Format(@"INSERT INTO sys_bill_max_separate (within_code,bill_id,year_month,bill_code,bill_text1,bill_text2,bill_text3,bill_text4,bill_text5) " +
                    " VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')"
                    , within_code, bill_id, year_month, bill_code,bill_text1,"","","","");
            }
            id = bill_code;
            int update_count = 1;
            //插入發貨記錄主表
            strSql += string.Format(@"INSERT INTO so_issues_mostly(" +
                "within_code,id,issues_date,issues_type,it_customer,name,separate_issues,update_count,transfers_state" +
                ",state,create_date,create_by,update_date,update_by,group_number,type,servername) " +
                " VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',GETDATE(),'{10}',GETDATE(),'{11}'" +
                ",'{12}','{13}','{14}')"
                , within_code, id, objModel.con_date, "0", objModel.it_customer, objModel.it_customer, "1", update_count, "0"
                , "0", objModel.crusr, objModel.crusr, "0", bill_text1, "hkserver.cferp.dbo");
            if (strSql != "")
                result = clsPublicOfGeo.ExecuteSqlUpdate(strSql);
            if (result == 0)
                id = "";
            return id;

        }
        //查找單據狀態
        public static DataTable checkDocStatus(string id)
        {
            DataTable dtId = new DataTable();
            string strSql = "Select a.state From so_issues_mostly a Where a.within_code='" + within_code + "' and a.id='" + id + "'";
            dtId = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtId;
        }
        //檢查單據是否有不同的倉庫號
        public static DataTable checkDocDiffLoc(string id,string adn_id,string seq)
        {
            DataTable dtId = new DataTable();
            string strSql = "Select a.ii_location,a.carton_code,a.sequence_id From so_issues_details a Where a.within_code='" + within_code + "'";
            if (id != "")
                strSql += " and a.id='" + id + "'";
            if (adn_id != "")
                strSql += " and a.order_id='" + adn_id + "'";
            if (seq != "")
                strSql += " and a.so_sequence_id='" + seq + "'";
            dtId = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtId;
        }
        //提取單據明細表
        public static DataTable loadDocSubRec(string id,string seq)
        {
            DataTable dtId = new DataTable();
            string strSql = "Select a.id,a.obligate_mo_id,a.lot_no,Convert(int,a.issues_qty) AS fl_qty" +
                ",Convert(numeric(18,2),a.sec_qty) AS fl_weg,a.order_id As mrp_id,a.so_sequence_id As jo_sub_sequence_id " +
                " From so_issues_details a " +
                " Where a.within_code='" + within_code + "' and a.id='" + id + "' and a.sequence_id='" + seq + "'";

            dtId = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtId;
        }
        //查找MO庫存
        public static DataTable checkMoStore(string loc,string mo_id,string goods_id,string lot_no)
        {
            DataTable dtMoStore = new DataTable();
            string strSql = "Select a.location_id,a.mo_id,a.goods_id,a.lot_no,a.qty,a.sec_qty "+
                " From st_details_lot a "+
                " Where a.within_code='" + within_code +
                "' and a.location_id='" + loc + "' and a.carton_code='" + loc + "'";
            if (mo_id != "")
                strSql += " and a.mo_id='" + mo_id + "'";
            if (goods_id != "")
                strSql += " and a.goods_id='" + goods_id + "'";
            if (lot_no != "")
                strSql += " and a.lot_no='" + lot_no + "'";
            strSql += " and (qty <> 0 or sec_qty <>0)";
            dtMoStore = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtMoStore;
        }
        //插入發貨記錄到明細表
        public static string AddStoreRec(List<takemosample> listDetail)
        {
            int result = 0;
            string strSql = "";
            string unit = "PCS";
            int exchange_rate = 1;
            string state = "0";
            string transfers_state = "0";
            string sub_seq = "";
            string sec_unit = "KG";
            string shipment_suit = "0";
            int piece_num = 0;
            string package_no = "";
            string s_address = "P";
            sub_seq = GetSeq("so_issues_details", listDetail[0].id, 4);//獲取明細表記錄序號

            for (int i = 0; i < listDetail.Count; i++)
            {
                if (i > 0)
                    sub_seq = (Convert.ToInt32(sub_seq.Substring(0, 4)) + 1).ToString().PadLeft(4, '0') + "h";
                strSql += string.Format(@"INSERT INTO so_issues_details(" +
                    " within_code,id,sequence_id,mo_id,goods_id,goods_name,contract_cid,customer_goods,issues_qty,issues_unit,unit_rate,sec_qty,sec_unit" +
                    ",exchange_rate,piece_num,package_no,ii_location,carton_code,lot_no,obligate_mo_id,transfers_state,fact_qty,shipment_suit,state" +
                    ",order_id,so_sequence_id,s_address) " +
                    " VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}'" +
                    ",'{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}')"
                    , within_code, listDetail[0].id, sub_seq, listDetail[i].line_mo_id, listDetail[0].goods_id, listDetail[0].goods_name
                    , listDetail[i].contract_cid, listDetail[i].customer_goods, listDetail[i].line_qty, unit, 1, listDetail[i].line_weg
                    , sec_unit, exchange_rate, piece_num, package_no, listDetail[i].loc, listDetail[i].loc, listDetail[i].line_lot_no
                    , listDetail[i].line_mo_id, transfers_state, listDetail[i].line_qty, shipment_suit, state, listDetail[i].mrp_id
                    , listDetail[i].ref_sequence_id, s_address);
            }
            if (strSql != "")
                result = clsPublicOfGeo.ExecuteSqlUpdate(strSql);
            if (result == 0)
                sub_seq = "";
            return sub_seq;
        }
        private static string GetSeq(string tb_name, string id, int seq_len)
        {
            string seq = "";
            string strSql;
            strSql = "Select Max(sequence_id) AS max_seq From " + tb_name + " Where within_code='" + within_code + "' AND id='" + id + "'";
            DataTable tbGenNo = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            if (tbGenNo.Rows[0]["max_seq"].ToString() == "")
            {
                if (seq_len == 4)
                    seq = "0001";
                else
                    if (seq_len == 5)
                        seq = "00001";
            }
            else
                seq = (Convert.ToInt32(tbGenNo.Rows[0]["max_seq"].ToString().Substring(0, seq_len)) + 1).ToString().PadLeft(seq_len, '0');
            seq = seq + "h";
            return seq;
        }
        private static string GetSub_Seq_id(string sub_seq)
        {
            string sub_seq_id = "";
            sub_seq_id = (Convert.ToInt32(sub_seq.Substring(0, 5)) + 1).ToString().PadLeft(5, '0') + "h";
            return sub_seq_id;

        }

        //查找制單發貨資料
        public static DataTable findDocRec(string id,string loc,string mo_id,string goods_id)
        {
            DataTable dtId = new DataTable();
            string strSql = "Select a.id,Convert(Varchar(20),a.issues_date,111) AS inventory_date,a.state,a.create_by,Convert(Varchar(20),a.create_date,120) AS create_date" +
                ",b.sequence_id,b.mo_id,b.goods_id,b.lot_no As ii_lot_no,b.obligate_mo_id,Convert(INT,b.issues_qty) AS i_amount,Convert(numeric(18,2),b.sec_qty) AS i_weight" +
                ",b.ii_location As inventory_issuance,b.ii_location As inventory_receipt,Convert(Varchar(20),a.check_date,120) AS check_date,c.name AS goods_name" +
                " From so_issues_mostly a " +
                " Inner Join so_issues_details b on a.within_code=b.within_code and a.id=b.id" +
                " Left Join it_goods c on b.within_code=c.within_code and b.goods_id=c.id" +
                " Where a.within_code='" + within_code + "'";

            if (id != "")
                strSql += " and a.id='" + id + "'";
            if (loc != "")
                strSql += " and b.ii_location='" + loc + "'";
            if (mo_id != "")
                strSql += " and b.mo_id='" + mo_id + "'";
            if (goods_id != "")
                strSql += " and b.goods_id='" + goods_id + "'";
            strSql += " Order By a.id,b.sequence_id";
            dtId = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtId;
        }


        //查找制單發貨資料
        public static DataTable findDocRec_Sub(string id, string loc, string goods_id, string state)
        {
            DataTable dtId = new DataTable();
            string strSql = "";
            strSql = "Select a.id,a.state" +
                ",b.sequence_id,b.mo_id,b.goods_id,b.lot_no As ir_lot_no,b.obligate_mo_id,Convert(INT,b.issues_qty) AS i_amount,Convert(numeric(18,2),b.sec_qty) AS i_weight" +
                ",b.ii_location As inventory_issuance,b.ii_location As inventory_receipt" +
                " From so_issues_mostly a " +
                " Inner Join so_issues_details b on a.within_code=b.within_code and a.id=b.id" +
                " Where a.within_code='" + within_code + "' And a.type='ADI' ";

            if (id != "")
                strSql += " and a.id='" + id + "'";
            if (loc != "")
                strSql += " and b.ii_location='" + loc + "'";
            if (goods_id != "")
                strSql += " and b.goods_id='" + goods_id + "'";
            if (state == "0")
                strSql += " and a.state='" + state + "'";
            strSql += " Order By a.id,b.sequence_id";
            dtId = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtId;
        }


        //查找單據是否已批準
        public static DataTable findDocDetails(string loc, string dat,string state,string tdep,string crusr)
        {
            DataTable dtId = new DataTable();
            string status="0";
            if (state == "0")
                status = "0";
            else
                status = "1";
            string strSql = "Select a.id,Convert(Varchar(20),a.issues_date,111) AS inventory_date,a.state,a.create_by,Convert(Varchar(20),a.create_date,120) AS create_date" +
                ",b.sequence_id,b.mo_id,b.goods_id,b.lot_no As ii_lot_no,b.obligate_mo_id,Convert(INT,b.issues_qty) AS i_amount,Convert(numeric(18,2),b.sec_qty) AS i_weight" +
                ",b.ii_location As inventory_issuance,b.ii_location As inventory_receipt,Convert(Varchar(20),a.check_date,120) AS check_date,c.name AS goods_name" +
                ",b.order_id,b.so_sequence_id,b.s_address" +
                " From so_issues_mostly a " +
                " Inner Join so_issues_details b on a.within_code=b.within_code and a.id=b.id" +
                " Left Join it_goods c on b.within_code=c.within_code and b.goods_id=c.id" +
                " Where a.within_code='" + within_code + "' And a.type='ADI' ";


            if (loc != "")
                strSql += " and b.ii_location='" + loc + "'";
            if (dat != "")
            {
                string dat2 = Convert.ToDateTime(dat).AddDays(1).ToString("yyyy/MM/dd");
                strSql += " and a.issues_date >='" + dat + "' and a.issues_date < '" + dat2 + "'";
            }
            if (tdep != "")
                strSql += " and b.ii_location='" + tdep + "'";
            if (crusr != "")
                strSql += " and a.create_by='" + crusr + "'";
            strSql += " and a.state='" + status + "'";
            strSql += " Order By a.id,b.sequence_id";
            dtId = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtId;
        }

        //刪除發貨記錄
        public static int DelStoreRec(string id,string seq)
        {
            int result = 0;
            string strSql = "";
            strSql += string.Format(@"Delete From so_issues_details where within_code='{0}' and id='{1}' and sequence_id='{2}'"
                , within_code, id, seq);
            

            if (strSql != "")
                result = clsPublicOfGeo.ExecuteSqlUpdate(strSql);
            return result;
        }

        public static DataTable loadLoc()
        {
            DataTable dtLoc = new DataTable();
            string strSql = @" select * from int09 where int9loc>='8' and int9loc<'9' ";
            dtLoc = clsPublicOfPad.ExecuteSqlReturnDataTable(strSql);
            dtLoc.Rows.Add();
            dtLoc.DefaultView.Sort="int9loc";
            return dtLoc;
        }


        //查找庫存記錄
        public static DataTable findStore(string loc,string mo_id, string goods_id)
        {
            string strSql = "";
            strSql = "Select a.mo_id,a.location_id,a.goods_id,a.lot_no,Convert(INT,a.qty) AS qty,Convert(numeric(18,2),a.sec_qty) AS sec_qty " +
                " From st_details_lot a Where a.within_code='" + within_code +
                "' and a.location_id='" + loc + "' and a.mo_id='" + mo_id + "' and a.goods_id='" + goods_id + "'" +
                " and (qty <>0 or sec_qty <>0 )";
            strSql += " order by a.location_id,a.goods_id,a.mo_id,a.lot_no";
            DataTable dtStore = new DataTable();
            dtStore = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtStore;
        }

        //從生產計劃中提取是否有QC流程
        public static DataTable GetWipQc(string mo_id, string seq)
        {
            DataTable dtWipQc = new DataTable();

            string strSql = @" SELECT a.mo_id,b.wp_id,b.next_wp_id,b.goods_id" +
                " From jo_bill_mostly a " +
                " Inner Join jo_bill_goods_details b on a.within_code=b.within_code and a.id=b.id and a.ver=b.ver" +
                " WHERE a.within_code='" + within_code + "' And a.mo_id='" + mo_id + "' And b.sequence_id='" + seq + "'";
            dtWipQc = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtWipQc;
        }

        //從生產計劃中獲取倉庫發料的記錄QC
        public static DataTable GetFlRecFromJoQc(string mo_id, string seq)
        {
            DataTable dtFlRec = new DataTable();

            string strSql = @" SELECT c.mrp_id,c.location,a.mo_id,b.goods_id,c.lot_no,c.obligate_mo_id,c.materiel_id,Convert(INT,b.prod_qty) AS fl_qty" +
                ",d.name AS goods_name,a.id,c.sequence_id As jo_sub_sequence_id,a.order_no,a.so_sequence_id,Convert(INT,b.order_qty) As order_qty " +
                ",Convert(INT,c.i_qty) AS i_qty" +
                " From jo_bill_mostly a " +
                " Inner Join jo_bill_goods_details b on a.within_code=b.within_code and a.id=b.id and a.ver=b.ver" +
                " Left Join jo_bill_materiel_details c on b.within_code=c.within_code and b.id=c.id and b.ver=c.ver and b.sequence_id=c.upper_sequence" +
                " Left Join it_goods d ON c.within_code=d.within_code AND c.materiel_id=d.id" +
                " WHERE a.within_code='" + within_code + "' And a.mo_id='" + mo_id + "' And b.sequence_id='" + seq + "'";
            dtFlRec = clsPublicOfGeo.ExecuteSqlReturnDataTable(strSql);
            return dtFlRec;
        }


    }
}
