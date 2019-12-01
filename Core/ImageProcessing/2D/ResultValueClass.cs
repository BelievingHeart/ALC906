﻿using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Runtime.InteropServices;
 using System.Text;

//引用命名空间

namespace Demon
{
    /// <summary>
    /// 对应于每个结果的测量值， 在检测结果相关参数进行设定的时候，
    /// 只给用户界面显示 string name, double tlower, double tupper, double tbias
    /// </summary>
    public class ResultValueClass
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <param name="key">键</param>
        /// <param name="def">值</param>
        /// <param name="retval">stringbulider对象</param>
        /// <param name="size">字节大小</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        #region "设定用参数"
        /// <summary>
        /// 标记字符串
        /// </summary>
        public string nameStr { set; get; }
        /// <summary>
        /// 设定的上限值，下面类同
        /// </summary>
        public double upper { get; set; }
        public double lower { get; set; }

        /// <summary>
        /// 乘数因子
        /// </summary>
        public double mulFactor { get; set; }

        public double mulFactor2 { get; set; }
        /// <summary>
        /// 每个测量结果， 都做一个偏置值， 设定参数
        /// </summary>
        public double bias { get; set; }

        public double bias2 { get; set; }
        #endregion

        #region "结果性参数"
        /// <summary>
        /// 用于访问的测量值
        /// </summary>
        public double measureValue { get; set; }

        /// <summary>
        /// 用于标定是第次存储的结果
        /// </summary>
        public int index { get; set; }
        #endregion


        public ResultValueClass(string name, double tlower, double tupper, double tbias, double tbias2 = 0)
        {
            nameStr = name; upper = tupper; lower = tlower;bias = tbias;bias2 = tbias2;
            mulFactor = 1; mulFactor2 = 1;
        }

        public static List<string> OnGuiHeaderStr()
        {
            List<string> headerStr = new List<string>();
            headerStr.Add("测试项");
            headerStr.Add("lower");
            headerStr.Add("upper");
            headerStr.Add("MulFactor");
            headerStr.Add("MulFactor2");
            headerStr.Add("bias");
            headerStr.Add("bias2");

            return headerStr;
        }

        /// <summary>
        /// 将GUI 界面的结果写入相关
        /// </summary>
        /// <param name="valueStr"></param>
        public void OnGetGuiData(List<string> valueStr)
        {
            if (valueStr[0] != nameStr)
                return;
            if (valueStr.Count() != OnGuiHeaderStr().Count)
                return;
            int tIndex = 1;
            lower = Double.Parse(valueStr[tIndex++]);
            upper = Double.Parse(valueStr[tIndex++]);
            mulFactor = Double.Parse(valueStr[tIndex++]);
            mulFactor2 = Double.Parse(valueStr[tIndex++]);
            bias = Double.Parse(valueStr[tIndex++]);
            bias2 = Double.Parse(valueStr[tIndex++]);
        }

        public List<string> OnShownGuiData()
        {
            List<string> valueStr = new List<string>();
            valueStr.Add(nameStr);
            valueStr.Add(lower.ToString());
            valueStr.Add(upper.ToString());
            valueStr.Add(mulFactor.ToString());
            valueStr.Add(mulFactor2.ToString());
            valueStr.Add(bias.ToString());
            valueStr.Add(bias2.ToString());
            return valueStr;
        }


        public bool isOK { get { return (measureValue <= upper) && (lower <= measureValue); } }

        public override string ToString()
        {
            return string.Format("{0}={1:F6}, {2}", nameStr, measureValue, isOK);
        }


        public void WriteIni(string fileName)
        {
            if (nameStr == "")
                return;

            string upperStr = upper.ToString();
            WritePrivateProfileString(nameStr, "UPPER", upperStr, fileName);

            string lowStr = lower.ToString();
            WritePrivateProfileString(nameStr, "LOWER", lowStr, fileName);

            WritePrivateProfileString(nameStr, "MulFactor", mulFactor.ToString(), fileName);
            WritePrivateProfileString(nameStr, "MulFactor2", mulFactor2.ToString(), fileName);

            string biasStr = bias.ToString();
            WritePrivateProfileString(nameStr, "BIAS", biasStr, fileName);


            string biasStr2 = bias2.ToString();
            WritePrivateProfileString(nameStr, "BIAS2", biasStr2, fileName);
        }

        public void ReadIni(string fileName)
        {
            if (nameStr == "")
                return;

            StringBuilder temp = new StringBuilder(1024);
            GetPrivateProfileString(nameStr, "UPPER", "", temp, 1024, fileName);
            string upperStr = temp.ToString();
            if (upperStr != "")
                upper = Double.Parse(upperStr);

            GetPrivateProfileString(nameStr, "LOWER", "", temp, 1024, fileName);
            string lowStr = temp.ToString();
            if (lowStr != "")
                lower = Double.Parse(lowStr);

            GetPrivateProfileString(nameStr, "MulFactor", "", temp, 1024, fileName);
            string factorStr = temp.ToString();
            if (factorStr != "")
                mulFactor = Double.Parse(factorStr);
            else
                mulFactor = 1;

            GetPrivateProfileString(nameStr, "MulFactor2", "", temp, 1024, fileName);
            string factorStr2 = temp.ToString();
            if (factorStr2 != "")
                mulFactor2 = Double.Parse(factorStr2);
            else
                mulFactor2 = 1;


            GetPrivateProfileString(nameStr, "BIAS", "", temp, 1024, fileName);
            string biasStr = temp.ToString();
            if (biasStr != "")
                bias = Double.Parse(biasStr);

            GetPrivateProfileString(nameStr, "BIAS2", "", temp, 1024, fileName);
            biasStr = temp.ToString();
            if (biasStr != "")
                bias2 = Double.Parse(biasStr);
        }
    }
}
