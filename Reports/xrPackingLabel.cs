using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace cf_pad.Reports
{
    public partial class xrPackingLabel : DevExpress.XtraReports.UI.XtraReport
    {
        public xrPackingLabel()
        {
            InitializeComponent();
        }

        private void Detail_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {

        }

    }
}
