using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace cf_pad.CLS
{
    public class clsValidRule
    {
        public clsValidRule()
        {
        }

        /// <summary>
        /// 验证邮箱
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEmail(string source)
        {
            return Regex.IsMatch(source, @"^[A-Za-z0-9](([_\.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\.([A-Za-z]{2,})$", RegexOptions.IgnoreCase);
        }

        //判斷日期格式的文本框輸入是否正確
        public static bool CheckDateFormat(string StrSource)
        {
            bool result_flag =true ;
            if (StrSource.ToString() != "" && StrSource.ToString().Length == 10)
            {
                string StrDat;
                StrDat = StrSource.Substring(0, 4) + "-" + StrSource.Substring(5, 2) + "-" + StrSource.Substring(8, 2);
                result_flag = Regex.IsMatch(StrDat,
                    @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]" +
                    @"|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|" +
                    @"1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?" +
                    @"2-(0?[1-9]|1\d|2[0-9]))|(((1[6-9]|[2-9]\d)(0[48]|[2468]" +
                    @"[048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$");
            }
            else
                result_flag = false;
            return result_flag;
        }
        //判斷日期時間格式的文本框輸入是否正確
        public static bool CheckDateTimeFormat(string StrSource)
        {
            bool result_flag = true;
            if (StrSource.ToString().Length != 16)
            {
                result_flag = false;
            }
            else
            {
                if (CheckDateFormat(StrSource.Substring(0, 10)) == false)
                    result_flag = false;
                else
                {
                    string StrDat;
                    StrDat = StrSource.Substring(11, 5);
                    result_flag = Regex.IsMatch(StrDat,
                        @"^((0?\d|1[0-9]|2[0-3])|((0?\d|1[0-9]|2[0-3]):(0?\d|[0-5]\d)(:(0?\d|[0-5]\d)){0,1}))$");
                }
            }
            return result_flag;
        }
        //判斷日期文本框中是否輸入內容
        public static bool CheckDateIsEmpty(string StrSource)
        {
            bool result_flag = true;
            if (StrSource.ToString().Trim() != "")
            {
                string Str = StrSource.ToString().Trim();
                string Str1;
                for (int i = 0; i < Str.Length; i++)
                {
                    Str1 = Str.Substring(i, 1).Trim();
                    if (Str1 != "" && Str1 != "/" && Str1 != "-" && Str1 != ":")
                    {
                        result_flag = false;
                        break;
                    }
                }
            }
            return result_flag;
        }
        //檢查是否是數值類型
        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
    }
    
}
