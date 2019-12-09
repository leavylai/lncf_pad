using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cf_pad.MDL
{
    /// <summary>
    /// 產品QC記錄
    /// </summary>
    public class product_qc_records
    {
        public int id;
        public string seq_no;
        public string dep_no;
        public string prd_date;
        public string mo_no;
        public string mat_item;
        public string mat_color;
        public int lot_qty;
        public int facade_actual_qty;
        public int size_actual_qty;
        public string actual_size;
        public string mat_logo;
        public string oth_desc;
        public int no_pass_qty;
        public string check_result;
        public string do_result;
        public string crusr;
        public string qc_date;
        public string qc_no_ok;
        public string qc_ok;
        public DateTime crtim;
        public string amusr;
        public DateTime amtim;
    }

    /// <summary>
    ///產品報告 
    /// </summary>
    public class product_records
    {
        public int prd_id;
        public string prd_dep;
        public string prd_date;
        public string prd_mo;
        public string prd_item;
        public int req_prd_qty;
        public int prd_qty;
        public float prd_weg;
        public string prd_machine;
        public string machine_desc;
        public string prd_work_type;
        public string work_type_decs;
        public string prd_worker;
        public string prd_class;
        public string prd_group;
        public string prd_start_time;
        public string prd_end_time;
        public string prd_req_time;
        public float prd_normal_time;
        public float prd_ot_time;
        public int line_num;
        public int hour_run_num;
        public int hour_std_qty;
        public int per_hour_std_qty;
        public int kg_pcs;
        public string qc_flag;
        public string mat_item;
        public string mat_item_desc;
        public string mat_item_lot;
        public float actual_qty;
        public decimal actual_weg;
        public int actual_pack_num;
        public string conf_flag;
        public DateTime conf_time;
        public string imput_flag;
        public string transfer_flag;
        public DateTime transfer_time;
        public string crusr;
        public DateTime crtim;
        public string amusr;
        public DateTime amtim;
        public int temp_qty;
        public int pageIndex;
        public string difficulty_level;
        public string to_dep;
        public float prd_run_qty;
        public int speed_lever;
        public string work_code;
        public int pack_num;
        public int start_run;
        public int end_run;
        public int ok_qty;
        public decimal ok_weg;
        public int no_ok_qty;
        public decimal no_ok_weg;
        public int prd_id_ref;
        public int sample_no;
        public decimal sample_weg;
        public int member_no;
        public string job_type;
        public string work_class;
        public string prd_pdate;
        public string arrange_flag;
        public string prd_owndep;
    }
    public class mdlProductWorker
    {
        public int prdId;
        public string prdWorker;
        public string crusr;
        public string crtim;
    }
    /// <summary>
    /// 移交單主表
    /// </summary>
    public class jo_materiel_con_mostly
    {
        public string id;
        public DateTime con_date;
        public DateTime check_date;
        public string out_dept;
        public string in_dept;
        public string stock_type;
        public string state;
        public string imput_flag;
        public DateTime imput_time;
        public string crusr;
        public DateTime crtim;
        public List<jo_materiel_con_details> lsDetails = new List<jo_materiel_con_details>();
    }

    /// <summary>
    /// 移交單明細表
    /// </summary>
    public class jo_materiel_con_details
    {
        public string id;
        public string sequence_id;
        public string mo_id;
        public string goods_id;
        public string lot_no;
        public float con_qty;
        public float sec_qty;
        public float package_num;
        public float actual_qty;
        public double actual_weg;
        public float actual_pack;
        public float sample_no;
        public double sample_weg;
        public string conf_flag;
        public string crusr;
        public DateTime crtim;
        public int merge_id;
        public float total_qty;
        public float total_weg;
        public float dif_qty;
        public double dif_weg;
        public string dif_flag;
        public string barcode_lot;

    }

    public class Mo_for_jx
    {
        public string mo_date;
        public string mo_id;
        public string wp_id;
        public string goods_id;
        public string goods_name;
        public decimal prod_qty;
        public string rmk;
        public string cr_usr;
        public DateTime cr_tim;
        public string am_usr;
        public DateTime am_tim;
        public string apr_usr;
        public DateTime apr_tim;
        public string bill_date;
        public string check_date;
        public int order_qty;
        public string t_complete_date;
        public string next_wp_id;
        public string next_wp_name;
        public string brand_id;
        public string get_color_sample;
        public string order_unit;
        public string production_remark;
        public string nickle_free;
        public string plumbum_free;
        public string remark;
        public string base_qty;
        public string unit_code;
        public string base_rate;
        public string basic_unit;
        public string customer_id;
        public string goods_unit;
        public string within_code;
        public string ver;
        public string id;
        public string sequence_id;
        public string blueprint_id;
        public string color;
        public string predept_rechange_qty;
        public int Reserve_qty; //預留數量
        public string get_color_sample_name;  //取色辦名稱
        public string Vendor_id;
        public int c_sec_qty_ok;     //完成重量
        public string color_desc;
        public string do_color;
    }

    /// <summary>
    ///組成員 
    /// </summary>
    public class product_group_member
    {
        public string prd_dep;
        public string prd_group_In;
        public string prd_group_Out;
        public string prd_worker;
        public string crusr;
        public DateTime crtim;
    }


    public class checkoutqty
    {
        public string id;
        public string sequence_id;
        public string wf_id;
        public string wf_seq;
        public string mo_id;
        public string goods_id;
        public string vend_id;
        public string check_flag_wip;
        public string check_flag_wf;
        public string crusr;
        public DateTime crtim;
    }
    public class product_ipqc
    {
        public int id;
        public string doc_id;
        public string doc_seq;
        public string seq_no;
        public string seq_id;
        public string dep_no;
        public string mo_source;
        public string qc_date;
        public string prd_date;
        public string mo_no;
        public string mat_item;

        //public string mat_logo;
        //public string mat_color;
        //public string qc_ok;
        //public string ng_rmk;
        //public string qc_no_ok;
        //public string oth_desc;
        //public string do_result;

        public int lot_qty;
        public string iqc_result;
        public string iqc_state;
        public string qc_remark;
        public string crusr;
        public string vendor;
        public string vendor_id;
        public int check_qty;
        public string update_count;
        public string transfers_state;
        public string adobt_level;
        public string state;
        public string approved_by;
        public int check_times;
        public string waster_modality;
        public string sequence_id;
        public string adopt_standard;
        public string aql_standard;
        public string aql_sample;
        public int accept_qty;
        public int reject_qty;
        public string qc_doc_id;
        public string qc_by;
        public string unqualified_iqc_seq;
        public string unqualified_category;
        public string not_ok_rmk;

    }

    public class storeissue
    {
        public string id;
        public string con_date;
        public string mo_id;
        public string goods_id;
        public string loc;
        public string t_dep;
        public string lot_no;
        public string obligate_mo_id;
        public string ref_id;
        public string ref_sequence_id;
        public string jo_sequence_id;
        public string line_lot_no;
        public string line_mo_id;
        public int line_qty;
        public double line_weg;
        public int qty;
        public double weg;
        public string order_no;
        public string so_sequence_id;
        public int order_qty;
        public string mrp_id;
        public string remark;
        public string crusr;
    }
    public class takemosample
    {
        public string id;
        public string con_date;
        public string it_customer;
        public string mo_id;
        public string goods_id;
        public string goods_name;
        public string loc;
        public string contract_cid;
        public string lot_no;
        public string obligate_mo_id;
        public string customer_goods;
        public string ref_sequence_id;
        public string line_lot_no;
        public string line_mo_id;
        public int line_qty;
        public double line_weg;
        public int qty;
        public double weg;
        public string order_no;
        public string so_sequence_id;
        public int order_qty;
        public string mrp_id;
        public string crusr;
    }
    public class product_transfer_jx_details
    {
        public string Transfer_date;
        public string Prd_dep;
        public string Prd_item;
        public string Prd_mo;
        public int Transfer_flag;
        public decimal Transfer_qty;
        public decimal Transfer_weg;
        public string To_dep;
        public string Crusr;
        public string Crtim;
    }
}
