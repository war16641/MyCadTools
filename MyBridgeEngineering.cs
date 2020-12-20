using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BRIDGEENGNEERING
{
    /// <summary>
    /// 这个类存放一些和桥梁工程相关的代码
    /// </summary>
    public static class MyBridgeEngineering
    {
        /// <summary>
        /// 从DK文本中获取里程数
        /// </summary>
        /// <param name="text"></param>
        /// <param name="mileage"></param>
        /// <returns></returns>
        public static bool read_mileage_from_text(string text, out double mileage)
        {
            Match m;
            mileage = 0.0;
            m = Regex.Match(text, @"[kK](?<kilo>\d+)\s*\+?\s*(?<number>\d*\.?\d*)");
            if (m == null) return false;
            try
            {
                if (m.Groups["number"].Length == 0)
                {
                    mileage = Convert.ToDouble(m.Groups["kilo"].Value) * 1000;
                }
                else
                {
                    mileage = Convert.ToDouble(m.Groups["kilo"].Value) * 1000 + Convert.ToDouble(m.Groups["number"].Value);
                }
            }
            catch (System.FormatException)//转化double识别
            {

                return false;
            }


            return true;
        }
    }

}
