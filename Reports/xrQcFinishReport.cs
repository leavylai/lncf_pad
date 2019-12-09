using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace cf_pad.Reports
{
    public partial class xrQcFinishReport : DevExpress.XtraReports.UI.XtraReport
    {
        public xrQcFinishReport()
        {
            InitializeComponent();
        }    

        private void lblSeq_no_TextChanged(object sender, EventArgs e)
        {
            string strResult = GetCurrentColumnValue("qc_result").ToString();
            if (strResult == "True")
            {
                chk_ok.Checked = true;
            }
            else
            {
                chk_ok.Checked = false;
            }
            if (strResult == "False")
            {
                chk_ng.Checked = true;
            }
            else
            {
                chk_ng.Checked = false;
            }
        }

    }
}
