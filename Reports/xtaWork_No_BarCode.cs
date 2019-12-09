using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace cf_pad.Reports
{
    public partial class xtaWork_No_BarCode : DevExpress.XtraReports.UI.XtraReport
    {
        public xtaWork_No_BarCode()
        {
            InitializeComponent(); 
        }

        void BindImage()
        {
            string art_path = DBUtility.imagePath + GetCurrentColumnValue("picture_name");
            if (File.Exists(art_path))
            {
                xrPictureBox1.ImageUrl = art_path;
            }          
        }

        private void xrPictureBox1_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            BindImage();           
        }
    }
}
