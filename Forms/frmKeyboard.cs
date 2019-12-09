using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;


using System.Windows.Forms;
using cf_pad.CLS;
using CFPublic;
//using System.Reflection;
//using DevExpress.XtraEditors;
//using DevExpress.XtraGrid;

namespace cf_pad.Forms
{
    public partial class frmKeyboard : Form
    {

        //public BarCodeHook BarCode = new BarCodeHook();

        BardCodeHooK BarCode = new BardCodeHooK();

        //private ScanerHook listener = new ScanerHook();
        public frmKeyboard()
        {
            InitializeComponent();
            BarCode.BarCodeEvent += new BardCodeHooK.BardCodeDeletegate(BarCode_BarCodeEvent);
            //listener.ScanerEvent += Listener_ScanerEvent;
            //BarCode.BarCodeEvent += new BarCodeHook.BarCodeDelegate(BarCode_BarCodeEvent);

        }

        //public delegate void ShowInfoDelegate(BarCodeHook.BarCodes barCode);
        //void ShowInfo(BarCodeHook.BarCodes barCode)
        //{
        //    textBox_barCode.Text = barCode.BarCode;
        //    buttonX2.Focus();
        //}
        //public void BarCode_BarCodeEvent(BarCodeHook.BarCodes barCode)
        //{
        //    ShowInfo(barCode);
        //}


        private delegate void ShowInfoDelegate(BardCodeHooK.BarCodes barCode);
        private void ShowInfo(BardCodeHooK.BarCodes barCode)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new ShowInfoDelegate(ShowInfo), new object[] { barCode });
            }
            else
            {
                //textBox1.Text = barCode.KeyName;
                //textBox2.Text = barCode.VirtKey.ToString();
                //textBox3.Text = barCode.ScanCode.ToString();
                //textBox4.Text = barCode.Ascll.ToString();
                //textBox5.Text = barCode.Chr.ToString();
                textBox6.Text = barCode.IsValid ? barCode.BarCode : "";//是否为扫描枪输入，如果为true则是 否则为键盘输入
                buttonX2.Focus();
                //textBox7.Text += barCode.KeyName;
                //textBox6.Text = textBox6.Text.Trim().ToUpper();
                //MessageBox.Show(barCode.IsValid.ToString());
            }
        }

        void BarCode_BarCodeEvent(BardCodeHooK.BarCodes barCode)
        {

            ShowInfo(barCode);
        }

        //private void Listener_ScanerEvent(ScanerHook.ScanerCodes codes)
        //{
        //    dgv_lst.Rows.Add(new object[] { codes.KeyDownCount, codes.Event.message, codes.Event.paramH, codes.Event.paramL, codes.CurrentChar, codes.Result, codes.isShift, codes.CurrentKey });
        //}

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //clsUtility.Input_Decimal(e);
            //txtmo_id.SelectAll();
            //objSelectAll(txtmo_id.Name.ToString());
            //TextBox obj = new TextBox txtmo_id();


            //foreach (object obj in this.Controls)
            //{
            //    if (obj is TextBox)
            //    {

            //        TextBox txt = (System.Windows.Forms.TextBox)obj;
            //        if (e.KeyChar == 13)
            //        {
            //            txt.SelectAll();
            //        }
            //        //txt.Text = "999";
            //        //clsUtility.Input_Decimal(txt, e);
                    
            //    }
            //    else
            //    {
            //        if (obj is ComboBox)
            //        {
            //            ComboBox txt = (System.Windows.Forms.ComboBox)obj;
            //            if (e.KeyChar == 13)
            //            {
            //                txt.SelectAll();
            //            }
            //        }
            //    }
                
            //}



            //object obj = new object();
            //TextBox txt = (System.Windows.Forms.TextBox)obj;
            //clsUtility.Input_Decimal(txt, e);
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //clsUtility.StartOSK();

            foreach (object obj in this.Controls)
            {
                if (obj is TextBox)
                {

                    TextBox txt = (System.Windows.Forms.TextBox)obj;

                    txt.SelectAll();
                    //txt.Text = "999";
                    //clsUtility.Input_Decimal(txt, e);

                }
                else
                {
                    if (obj is ComboBox)
                    {
                        ComboBox txt = (System.Windows.Forms.ComboBox)obj;

                        txt.SelectAll();
                    }
                }
            }
            //clsUtility.StartImput();
            //InitFormAllControlStyle(this);
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (textBox6.Text.Length > 0)
            {
                //MessageBox.Show("条码长度：" + textBox6.Text.Length + "\n条码内容：" + textBox6.Text, "系统提示");
            }
        }

        private void frmKeyboard_Load(object sender, EventArgs e)
        {
            BarCode.Start();
            //listener.Start();
        }

        private void frmKeyboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            BarCode.Stop();
        }

        private void frmKeyboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            //listener.Stop();
        }



        ///// <summary>
        ///// 初始化控件样式
        ///// </summary>
        ///// <param name="frm">控件所在窗体</param>
        //public void InitFormAllControlStyle(Form frm)
        //{
        //    try
        //    {
        //        //取得窗体属性
        //        Type FormType = frm.GetType();
        //        //取得控件
        //        FieldInfo[] fi = FormType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        //        foreach (FieldInfo info in fi)
        //        {
        //            //设置BarManege样式
        //            if (info.FieldType == typeof(DevExpress.XtraBars.BarManager))
        //            {
        //                DevExpress.XtraBars.BarManager barManege = (info.GetValue(frm)) as DevExpress.XtraBars.BarManager;
        //                barManege.AllowCustomization = false;
        //                barManege.AllowQuickCustomization = false;
        //                barManege.AllowShowToolbarsPopup = false;
        //            }
        //            else if (info.FieldType == typeof(TreeList))
        //            {
        //                TreeList tr = (info.GetValue(frm)) as TreeList;
        //                tr.OptionsView.ShowColumns = false; //是否显示选中的行
        //                tr.OptionsBehavior.Editable = false; //不可编辑
        //                tr.OptionsView.ShowHorzLines = false; //OptionsView提供对树状列表的显示选项,设置水平线是否显示
        //                tr.OptionsView.ShowIndicator = false; //节点的指示面板是否显示
        //                tr.OptionsView.ShowVertLines = false; //垂直线条是否显示
        //                //设置treeList的折叠样式为 +  - 号
        //                tr.LookAndFeel.UseDefaultLookAndFeel = false;
        //                tr.LookAndFeel.UseWindowsXPTheme = true;
        //                tr.OptionsSelection.InvertSelection = true; //聚焦的样式是否只适用于聚焦细胞或所有细胞除了聚焦对象，失去焦点后
        //                foreach (TreeListColumn column in tr.Columns)
        //                {
        //                    column.OptionsColumn.AllowSort = false;
        //                    column.OptionsFilter.AllowFilter = false;
        //                }
        //            }
        //            else if (info.FieldType == typeof(GridControl))
        //            {
        //                GridControl grd = (info.GetValue(frm)) as GridControl;
        //                var gv = (GridView)grd.MainView;
        //                gv.OptionsView.ShowGroupPanel = false;
        //                gv.OptionsFind.AlwaysVisible = false;

        //                //默认设置不可编辑
        //                gv.OptionsBehavior.Editable = false;

        //                gv.OptionsView.ShowIndicator = false;

        //                //禁用子表显示
        //                gv.OptionsDetail.EnableMasterViewMode = false;

        //                //设置标题和内容居中
        //                gv.Appearance.HeaderPanel.TextOptions.HAlignment = HorzAlignment.Center;
        //                gv.Appearance.ViewCaption.TextOptions.HAlignment = HorzAlignment.Center;

        //                //禁用列头的过滤器
        //                gv.OptionsCustomization.AllowFilter = false;

        //                //排序后选择第一行数据
        //                gv.EndSorting += gv_EndSorting;

        //                //列标题，右键禁用
        //                gv.OptionsMenu.EnableColumnMenu = false;
        //            }
        //            else if (info.FieldType == typeof(VGridControl))
        //            {
        //                VGridControl vgd = (info.GetValue(frm)) as VGridControl;
        //                //默认设置不可编辑
        //                vgd.OptionsBehavior.Editable = false;
        //            }
        //            else if (info.FieldType == typeof(PictureEdit))
        //            {
        //                PictureEdit pic = (info.GetValue(frm)) as PictureEdit;
        //                var picName = pic.Name.Substring(3);
        //                var picPath = ConfigHelper.ImagePath + picName + ".png";
        //                if (File.Exists(picPath))
        //                {
        //                    pic.Image = Image.FromFile(picPath);
        //                }
        //            }//日期控件默认最小时间为1990-1-1,最大值为2079-1-1
        //            else if (info.FieldType == typeof(DateEdit))
        //            {
        //                DateEdit dateCol = (info.GetValue(frm)) as DateEdit;
        //                dateCol.Properties.MinValue = new DateTime(1900, 1, 1);
        //                dateCol.Properties.MaxValue = new DateTime(2079, 1, 1);
        //            }
        //            else
        //            {
        //                //......
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}



    }
}
