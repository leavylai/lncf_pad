using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using cf_pad.Forms;
using CFPublic;

namespace cf_pad
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new frmLogin());
            if (frmMainForPad.isRunMain == true)
            {
                //frmCfg.flag_readonly = true; //數據庫連接信息窗體用到此變量               
                Application.Run(new frmMainForPad());
            }

            //Application.Run(new frmPrdSchedule());
            //Application.Run(new frmKeyboard());
            //Application.Run(new frmGoodsTransferJx());
            //Application.Run(new frmIqcOp());
            //Application.Run(new frmTransferRecords());
            //Application.Run(new frmPackingLabel());
            //Application.Run(new frmTransfer());

            //student st = new student();
            //st.Code = "S01";
            //st.Name = "小王";
            //st.Sex = "女";
            //st.Age = 28;
            //MessageBox.Show(st.Sex1);

            //MessageBox.Show(st.jiafa(3, 2).ToString());
            //MessageBox.Show(st.jianfa(3, 2).ToString());

        }
    }
}
