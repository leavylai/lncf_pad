using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.IO;
using System.Data;

namespace cf_pad.Reports
{
    public partial class xrPrdTransfer : DevExpress.XtraReports.UI.XtraReport
    {
        private DataTable dtPOInfo = new DataTable();
        private DataTable dtPartsInfo = new DataTable();

        public xrPrdTransfer(DataTable dtMain, DataTable dtParts)
        {
            InitializeComponent();
            dtPOInfo = dtMain;
            dtPartsInfo = dtParts;

            BindReport();
        }

        private void BindReport()
        {
            this.DataSource = dtPOInfo;
        }

        private void xrpbMain_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            ClearPicturebox();
            BindPartsReport(xrlblMo_id.Text);

            xrlblOrder_qty.Text = CLS.clsUtility.NumberConvert(xrlblOrder_qty.Text);
            xrlblProd_qty.Text = CLS.clsUtility.NumberConvert(xrlblProd_qty.Text);

            string art_path = DBUtility.imagePath + xrlblPbMain.Text.Trim();
            if (File.Exists(art_path))
            {
                xrpbMain.ImageUrl = art_path;
            }
            else
            {
                xrpbMain.ImageUrl = null;
            }
        }

        /// <summary>
        /// 綁定配件名稱及顏色
        /// </summary>
        /// <param name="pMo_id"></param>
        private void BindPartsReport(string pMo_id)
        {
            int picIndex = 0;
            xrTable1.Rows.Clear();
            for (int i = 0; i < dtPartsInfo.Rows.Count; i++)
            {
                if (pMo_id == dtPartsInfo.Rows[i]["mo_id"].ToString())
                {
                    //綁定單元格
                    XRTableCell tcSer_no = new XRTableCell();
                    tcSer_no.WidthF = 20;
                    tcSer_no.Text = dtPartsInfo.Rows[i]["Ser_no"].ToString() + ".";

                    XRTableCell tcGoods_id = new XRTableCell();
                    tcGoods_id.WidthF = 158;
                    tcGoods_id.Text = dtPartsInfo.Rows[i]["part_goods_id"].ToString();

                    XRTableCell tcGoods_name_color = new XRTableCell();
                    tcGoods_name_color.WidthF = 390;
                    tcGoods_name_color.Text = dtPartsInfo.Rows[i]["part_goods_name"].ToString();

                    XRTableRow tr = new XRTableRow();
                    tr.Cells.AddRange(new XRTableCell[] { tcSer_no, tcGoods_id, tcGoods_name_color });
                    xrTable1.Rows.Add(tr);

                    //綁定圖片
                    string art_path = DBUtility.imagePath + dtPartsInfo.Rows[i]["picture_name"].ToString();
                    switch (picIndex)
                    {
                        case 0:
                            xrpbpart1.ImageUrl = art_path;
                            break;
                        case 1:
                            xrpbpart2.ImageUrl = art_path;
                            break;
                        case 2:
                            xrpbpart3.ImageUrl = art_path;
                            break;
                        case 3:
                            xrpbpart4.ImageUrl = art_path;
                            break;
                        case 4:
                            xrpbpart5.ImageUrl = art_path;
                            break;
                        case 5:
                            xrpbpart6.ImageUrl = art_path;
                            break;
                        default:
                            break;
                    }
                    picIndex++;
                }
            }
        }

        private void ClearPicturebox()
        {
            xrpbMain.ImageUrl = null;
            xrpbpart1.ImageUrl = null;
            xrpbpart2.ImageUrl = null;
            xrpbpart3.ImageUrl = null;
            xrpbpart4.ImageUrl = null;
            xrpbpart5.ImageUrl = null;
            xrpbpart6.ImageUrl = null;
        }




    }
}
