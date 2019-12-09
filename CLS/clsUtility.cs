using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

namespace cf_pad.CLS
{
    public class clsUtility
    {
        public enum enmOrderType
        {
            PurchaseOrder,
            ArrivedGoods
        }

        public enum enumOperationType
        {
            Add = 0,
            Update = 1,
            Find = 2,
            Delete = 3,
            Cancel = 4,
            Save = 5,
            FindBrand = 6,
            FindCustomer = 7,
            Load = 8,
            Print = 9,
            ExportToExcel = 10,
            PreView = 11
        }

        /// <summary>
        /// 根據不同操作控制按鈕的狀態是否可用
        /// </summary>
        /// <param name="toolstrip"></param>
        /// <param name="OperationType"></param>
        public static void EnableToolStripButton(ToolStrip toolstrip, enumOperationType OperationType)
        {
            foreach (var ct in toolstrip.Items)
            {
                if (ct.GetType() == typeof(ToolStripButton))
                {
                    ToolStripButton tsbtn = (ToolStripButton)ct;

                    switch (OperationType)
                    {
                        case enumOperationType.Add:
                            {
                                if (tsbtn.Name != "BTNCANCEL" && tsbtn.Name != "BTNSAVE" && tsbtn.Name != "BTNEXIT")
                                {
                                    tsbtn.Enabled = false;
                                }
                                else
                                {
                                    tsbtn.Enabled = true;
                                }
                            }
                            break;
                        case enumOperationType.Update:
                            {
                                if (tsbtn.Name != "BTNCANCEL" && tsbtn.Name != "BTNSAVE" && tsbtn.Name != "BTNEXIT")
                                {
                                    tsbtn.Enabled = false;
                                }
                                else
                                {
                                    tsbtn.Enabled = true;
                                }
                            }
                            break;
                        case enumOperationType.Cancel:
                            {
                                if (tsbtn.Name != "BTNCANCEL" && tsbtn.Name != "BTNSAVE")
                                {
                                    tsbtn.Enabled = true;
                                }
                                else
                                {
                                    tsbtn.Enabled = false;
                                }
                            }
                            break;
                        case enumOperationType.Save:
                            {
                                if (tsbtn.Name != "BTNSAVE" && tsbtn.Name != "BTNCANCEL")
                                {
                                    tsbtn.Enabled = true;
                                }
                                else
                                {
                                    tsbtn.Enabled = false;
                                }
                            }
                            break;
                        case enumOperationType.Load:
                            {
                                if (tsbtn.Name != "BTNSAVE" && tsbtn.Name != "BTNCANCEL")
                                {
                                    tsbtn.Enabled = true;
                                }
                                else
                                {
                                    tsbtn.Enabled = false;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public static bool CheckDate(string strDate)
        {
            if (strDate.Replace(" ", "").Length == 15 || strDate.Replace(" ", "").Length == 10)
            {
                //int intIndex = strDate.Replace(" ", "").IndexOf("/");

                if (strDate.Substring(0, 4).Length > 4 || strDate.Substring(0, 4).Length < 4)
                {
                    return false;
                }

                if (Convert.ToInt32(strDate.Substring(5, 2)) > 12 || Convert.ToInt32(strDate.Substring(5, 2)) == 00)
                {
                    MessageBox.Show("月份應該在1~12之間,請輸入正確的月份。");
                    return false;
                }

                switch (strDate.Substring(5, 2))
                {
                    case "01":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 31 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("1月份為31天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "02":
                        int intYear = Convert.ToInt32(strDate.Substring(0, 4));
                        if ((intYear % 4 == 0 && intYear % 100 != 0) || intYear % 400 == 0)
                        {
                            if (Convert.ToInt32(strDate.Substring(8, 2)) > 29 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                            {
                                MessageBox.Show("2月份為29天，請輸入正確的天數。");
                                return false;
                            }
                        }
                        else
                        {
                            if (Convert.ToInt32(strDate.Substring(8, 2)) > 28 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                            {
                                MessageBox.Show("2月份為28天，請輸入正確的天數。");
                                return false;
                            }
                        }
                        break;
                    case "03":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 31 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("3月份為31天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "04":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 30 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("4月份為30天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "05":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 31 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("5月份為31天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "06":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 30 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("6月份為30天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "07":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 31 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("7月份為31天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "08":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 31 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("8月份為31天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "09":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 30 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("9月份為30天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "10":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 31 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("10月份為31天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "11":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 30 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("11月份為30天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    case "12":
                        if (Convert.ToInt32(strDate.Substring(8, 2)) > 31 || Convert.ToInt32(strDate.Substring(8, 2)) == 00)
                        {
                            MessageBox.Show("12月份為31天，請輸入正確的天數。");
                            return false;
                        }
                        break;
                    default:
                        break;
                }

                if (strDate.Replace(" ", "").Length == 15)
                {
                    if (Convert.ToInt32(strDate.Substring(11, 2)) > 24 || Convert.ToInt32(strDate.Substring(11, 2)) == 00)
                    {
                        MessageBox.Show("請輸入正確的小時數。");
                        return false;
                    }

                    if (Convert.ToInt32(strDate.Substring(14, 2)) > 59)
                    {
                        MessageBox.Show("請輸入正確的分鐘數。");
                        return false;
                    }
                }
            }
            else
            {
                MessageBox.Show("請輸入正確的日期格式 ：'yyyy/MM/dd HH:mm'。");
                return false;
            }
            return true;
        }

        public string TranslateenmOrderTypeToString(enmOrderType pOrderType)
        {
            switch (pOrderType)
            {
                case enmOrderType.PurchaseOrder:
                    return "purchaseorder";
                case enmOrderType.ArrivedGoods:
                    return "arrivedgoods";
                default:
                    return "false";
            }
        }

        public string TranslateenmOperationTypeToString(enumOperationType pOperationType)
        {
            switch (pOperationType)
            {
                case enumOperationType.Find:
                    return "query";
                case enumOperationType.Delete:
                    return "delete";
                case enumOperationType.Add:
                    return "add";
                case enumOperationType.Update:
                    return "edit";
                default:
                    return "false";
            }
        }


        public string TrimEndDecimal(decimal pValue)
        {
            return TrimEndDecimal(pValue.ToString());
        }

        public string TrimEndDecimal(string pValue)
        {
            if (pValue.IndexOf('.') > 0)
            {
                return pValue.TrimEnd('0').TrimEnd('.');
            }
            else
            {
                return pValue;
            }
        }

        public DateTime FormatDate1(string pDate)
        {
            DateTime dtmDate = Convert.ToDateTime(pDate);
            return dtmDate;
        }

        public string FormatDate(DateTime pDate)
        {
            string strReturn = "";
            strReturn = pDate.Year.ToString() + "-" +
                        pDate.Month.ToString().PadLeft(2, '0') + "-" +
                        pDate.Day.ToString().PadLeft(2, '0');
            return strReturn;
        }

        /// <summary>
        /// 計算時間段的時間差
        /// </summary>
        /// <param name="DateTime1"></param>
        /// <param name="DateTime2"></param>
        /// <returns></returns>
        private string DateCha(DateTime DateTime1, DateTime DateTime2)
        {
            string str = null;
            TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            str = ts.Days.ToString() + "天" + ts.Hours.ToString() + "小时" + ts.Minutes.ToString() + "分钟" + ts.Seconds.ToString() + "秒";
            return str;
        }

        /// <summary>
        /// 对于可能出现DBNull的字段转换成String时调用。
        /// </summary>
        /// <param name="pInput"></param>
        /// <returns></returns>
        public static string FormatNullableString(object pInput)
        {
            if (pInput == DBNull.Value || pInput == null)
            {
                return "";
            }
            else
            {
                return pInput.ToString();
            }
        }

        /// <summary>
        /// 对于可能出现DBNull的字段转换成bool时调用。
        /// </summary>
        /// <param name="pInput"></param>
        /// <returns></returns>
        public static bool FormatNullableBool(object pInput)
        {
            if (pInput == DBNull.Value)
            {
                return false;
            }
            else
            {
                return Convert.ToBoolean(pInput);
            }
        }

        /// <summary>
        /// 对于可能出现DBNull的字段转换成Float时调用。
        /// </summary>
        /// <param name="pInput"></param>
        /// <returns></returns>
        public static float FormatNullableFloat(object pInput)
        {
            if (pInput == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToSingle(pInput);
            }
        }

        /// <summary>
        /// 对于可能出现DBNull的字段转换成Int时调用。
        /// </summary>
        /// <param name="pInput"></param>
        /// <returns></returns>
        public static Int32 FormatNullableInt32(object pInput)
        {
            if (pInput == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(pInput);
            }
        }


        /// <summary>
        /// 对于可能出现DBNull的字段转换成decimal时调用。
        /// </summary>
        /// <param name="pInput"></param>
        /// <returns></returns>
        public static decimal FormatNullableDecimal(object pInput)
        {
            if (pInput == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToDecimal(pInput);
            }
        }

        /// <summary>
        /// 对于可能出现DBNull的字段转换成DateTime时调用。
        /// </summary>
        /// <param name="pInput"></param>
        /// <returns></returns>
        public static DateTime FormatNullableDateTime(object pInput)
        {
            if (pInput == DBNull.Value)
            {
                return Convert.ToDateTime("1900/01/01");
            }
            else
            {
                return Convert.ToDateTime(pInput);
            }
        }


        /// <summary>
        /// 此方法用于确认用户输入的不是恶意信息
        /// </summary>
        /// <param name="pText">用户输入信息</param>
        /// <param name="pMaxLength">输入的最大长度</param>
        /// <returns>处理后的输入信息</returns>
        public string InputText(string pText, int pMaxLength)
        {
            pText = pText.Trim();
            if (string.IsNullOrEmpty(pText))
            {
                return string.Empty;
            }
            if (pText.Length > pMaxLength)
            {
                pText = pText.Substring(0, pMaxLength);
            }
            // 2个或以上的空格
            pText = Regex.Replace(pText, "[\\s]{2,}", " ");
            //pText = Regex.Replace(pText, "(<[b|B][r|R]/*>)+|(<[p|P](.|\\n)*?>)", "\n");	//<br> html换行符
            //pText = Regex.Replace(pText, "(\\s*&[n|N][b|B][s|S][p|P];\\s*)+", " ");	//&nbsp;   html空格符
            //pText = Regex.Replace(pText, "<(.|\\n)*?>", string.Empty);	// 任何其他的标签
            //pText = pText.Replace("'", "''");// 单引号
            return pText;
        }

        public string OutputText(object pValue, string pOriginalValue)
        {
            if (pValue == DBNull.Value)
            {
                return pOriginalValue;
            }
            else
            {
                return pValue.ToString().Trim();
            }
        }

        public int OutputText(object pValue, int OriginalValue)
        {
            if (pValue == DBNull.Value)
            {
                return OriginalValue;
            }
            else
            {
                return Convert.ToInt32(pValue);
            }
        }

        public decimal OutputText(object pValue, decimal OriginalValue)
        {
            if (pValue == DBNull.Value)
            {
                return OriginalValue;
            }
            else
            {
                return Convert.ToDecimal(pValue);
            }
        }

        public DateTime OutputText(object pValue, DateTime OriginalValue)
        {
            if (pValue == DBNull.Value)
            {
                return OriginalValue;
            }
            else
            {
                return Convert.ToDateTime(pValue);
            }
        }

        public bool OutputText(object pValue, bool OriginalValue)
        {
            if (pValue == DBNull.Value)
            {
                return OriginalValue;
            }
            else
            {
                return Convert.ToBoolean(pValue);
            }
        }

        /// <summary>
        /// 將數字轉換成千分位
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static string NumberConvert(object objValue)
        {
            string strResult = Convert.ToInt32(objValue).ToString();
            if (strResult.Length > 3)
            {
                return strResult = Convert.ToInt32(objValue).ToString("0,000");
            }
            else
            {
                return strResult;
            }
        }

        public static void Call_imput()
        {
            dynamic file = "C:\\Program Files\\Common Files\\microsoft shared\\ink\\TabTip.exe";
            if (System.IO.File.Exists(file))
                System.Diagnostics.Process.Start(file);//"TabTip.exe"
        }

        /// <summary>
        /// 可輸入整數和小數
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public static void Input_Decimal(TextBox obj, KeyPressEventArgs e)//
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46)      //小数点
            {
                if (obj.Text.Length <= 0)
                    e.Handled = true;   //小数点不能在第一位
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse(obj.Text, out oldf);
                    b2 = float.TryParse(obj.Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

            //按的是回車鍵
            if (e.KeyChar == 13)
            {
                SendKeys.Send("{TAB}");
            }
        }

        /// <summary>
        /// 只能輸入整數
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public static void Input_Int(TextBox obj, KeyPressEventArgs e)
        //public static void Input_Int(KeyPressEventArgs e)
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            if (e.KeyChar == 46)
                e.Handled = true;

            //按的是回車鍵
            if (e.KeyChar == 13)
            {
                SendKeys.Send("{TAB}");
            }
            //obj.SelectAll();
        }


        /// <summary>
        /// Test to show launching on screen board (osk.exe).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"c:\Temp\OSK.exe");
            }
            catch (Exception error)
            {
                string err = error.ToString();
            }
        }


        public static void StartImput()
        {
            string osk = "C:\\hvk\\hvk.exe";
            if (!File.Exists(osk))
            {
                osk = "C:\\Program Files\\Common Files\\microsoft shared\\ink\\TabTip.exe";
                if (!File.Exists(osk))
                {
                    osk = null;
                    string windir = Environment.GetEnvironmentVariable("WINDIR");
                    //string osk = null;

                    if (osk == null)
                    {
                        osk = Path.Combine(Path.Combine(windir, "sysnative"), "osk.exe");
                        if (!File.Exists(osk))
                            osk = null;
                    }

                    else
                    {
                        osk = Path.Combine(Path.Combine(windir, "system32"), "osk.exe");
                        if (!File.Exists(osk))
                        {
                            osk = null;
                        }
                    }

                    if (osk == null)
                        osk = "osk.exe";
                }
            }
            System.Diagnostics.Process.Start(osk);
        }
        //將日期時間轉換成字符型
        public static string changeDateTimeToChar(DateTime dateValue)
        {
            string result = "";
            result = dateValue.ToString("yyyy/MM/dd yy:MM:dd");
            return result;
        }
        //將日期時間轉換成字符型
        public static string changeDateToChar(DateTime dateValue)
        {
            string result = "";
            result = dateValue.ToString("yyyy/MM/dd");
            return result;
        }
    }
}
