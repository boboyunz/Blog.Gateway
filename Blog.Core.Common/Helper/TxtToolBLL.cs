namespace Blog.Core.Common.Helper
{
    using System;
    using System.Net;
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;  
    using Microsoft.Extensions.Configuration;
    
    using SqlSugar;
    /// <summary>
    /// TxtTool 的摘要说明。
    /// </summary>
    public class TxtToolBLL
    {
        

        #region 公共数据类型的定义
        /// <summary>
        /// 公共数据类型的定义
        /// </summary>
        /// <param name="Val"></param>
        /// <returns></returns>
        public static string GetDataBaseType(int Val)
        {
            switch (Val)
            {
                case 0: return "MySql";
                case 1: return "SqlServer";
                case 2: return "Sqlite";
                case 3: return "Oracle";
                case 4: return "Postgre";
                case 5: return "达梦";
                case 6: return "人大金仓";
                case 100: return "Doris";
            }
            return "";
        }
        #endregion      

        #region  配置文件缓存管理
        public static Dictionary<string, IConfiguration> AppSettingDic { get; set; }


        public static IConfiguration GetConfiguration(string ConfigFile = "appsettings.json")
        {
            if (AppSettingDic == null) AppSettingDic = new Dictionary<string, IConfiguration>();

            if (!AppSettingDic.ContainsKey(ConfigFile))
            {
                lock (AppSettingDic)
                {
                    IConfiguration cfgbuf = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile(ConfigFile)
                     .Build();

                    AppSettingDic.Add(ConfigFile, cfgbuf);
                    return cfgbuf;
                }

            }
            else
            {
                return AppSettingDic[ConfigFile];
            }
        }
        #endregion

        #region 构造函数
        public TxtToolBLL()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        #endregion

        #region GUID
        public static Random RandomObj = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0));
        /// <summary>
        /// 根据GUID获取16位的唯一字符串
        /// </summary>
        /// <param name=\"guid\"></param>
        /// <returns></returns>
        public static string GuidTo16String()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
                i *= ((int)b + 1);
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
        /// <summary>
        /// 根据GUID获取19位的唯一数字序列
        /// </summary>
        /// <returns></returns>
        public static long GuidToLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
        /// <summary>
        /// 生成22位唯一的数字 并发可用
        /// </summary>
        /// <returns></returns>
        public static string GenerateUniqueID()
        {
            System.Threading.Thread.Sleep(1); //保证yyyyMMddHHmmssffff唯一          
            string strUnique = DateTime.Now.ToString("yyyyMMddHHmmssffff") + RandomObj.Next(1000, 9999);
            return strUnique;
        }

        #endregion

        #region 生成Guid Long
        private static int SHORT_MAX = 65535;
        private static int counter = 0;
        private static DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
        public static long GetNextGuid()
        {
            System.Threading.Thread.Sleep(1);
            long now = Convert.ToInt64((DateTime.Now - dtZone).TotalMilliseconds);
            Interlocked.Increment(ref counter);
            if (counter >= SHORT_MAX) Interlocked.Exchange(ref counter, 0);
            return now * 65536 + counter;
        }

        public static long GetNextGuid(DateTime datet)
        {
            long now = Convert.ToInt64((datet - dtZone).TotalMilliseconds);
            Interlocked.Increment(ref counter);
            if (counter >= SHORT_MAX) Interlocked.Exchange(ref counter, 0);
            return now * 65536 + counter;
        }
        #endregion

        #region 文件操作函数
        public static byte[] ReadAllbytes(string FilePath)
        {
            byte[] Data = null;
            if (!File.Exists(FilePath)) return null;
            try
            {
                Data = File.ReadAllBytes(FilePath);
            }
            catch (System.Exception e)
            {
                string ss = e.Message;
            }
            return Data;
        }
        /// <summary>
        /// 读取文本文件
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string Reader(string FilePath)
        {
            StreamReader sr = null;
            string Lstr = string.Empty; ;
            if (File.Exists(FilePath))
            {
                //sr=File.OpenText(FilePath);
                sr = new StreamReader(FilePath, System.Text.Encoding.Default);
                //sr.StreamReader
                Lstr = sr.ReadToEnd();
                sr.Close();
            }


            return Lstr;

        }
        /// <summary>
        /// 写文本文件
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static bool Writer(string Content, string FilePath)
        {

            bool check = false;
            if (!File.Exists(FilePath)) return false;
            StreamWriter Sw;

            try
            {
                Sw = File.AppendText(FilePath);
                Sw.WriteLine(Content);
                check = true;
                Sw.Close();
            }
            catch
            {
                check = false;
            }

            return check;

        }
        public static bool CreatWriter(string Content, string FilePath)
        {

            bool check = false;

            StreamWriter Sw;

            try
            {
                if (!File.Exists(FilePath)) File.CreateText(FilePath);
                Sw = File.AppendText(FilePath);
                Sw.WriteLine(Content);
                check = true;
                Sw.Close();
            }
            catch
            {
                check = false;
            }

            return check;

        }
        public static bool Creater(string FilePath)
        {
            bool check = false;
            StreamWriter sc = null;
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }

                sc = File.CreateText(FilePath);
                check = true;
            }
            catch
            {
                check = false;
            }
            sc.Close();
            return check;



        }
        /// <summary>
        /// 创建任意各式的文件
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static bool CreaterFile(string FilePath)
        {
            bool check = false;
            System.IO.FileStream sc = null;
            try
            {
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }

                sc = File.Create(FilePath, 1024);
                check = true;
            }
            catch (System.Exception e)
            {
                string ss = e.Message;
                check = false;
            }
            sc.Close();
            return check;



        }
        /// <summary>
        /// 将字节数据写到文件的指定位置
        /// </summary>
        /// <param name="Content">数据</param>
        /// <param name="BeginWriteIndex">开始写入的位置</param>
        /// <param name="BeginIndex">数据开始读出的位置</param>
        /// <param name="Length">写入数据的长度</param>
        /// <param name="FilePath">文件路径和名称</param>
        /// <returns></returns>
        public static bool Writerbyte(byte[] Content, int BeginWriteIndex, int BeginIndex, int Length, string FilePath)
        {
            bool check = false;
            if (!File.Exists(FilePath)) return false;
            System.IO.FileStream Sw = File.OpenWrite(FilePath);
            Sw.Seek(BeginWriteIndex, System.IO.SeekOrigin.Begin);
            try
            {
                Sw.Write(Content, BeginIndex, Length);
                check = true;
            }
            catch
            {
                check = false;
            }
            Sw.Close();
            return check;
        }


        public static bool CopyBigFile(string source, string target, Func<byte[], byte[]> DealData)
        {

            bool check = false;
            //1)读

            //2)创建文件流
            try
            {
                using (FileStream fsRead = new FileStream(source, FileMode.Open))
                {

                    //4)创建写入流

                    using (FileStream fsWrite = new FileStream(target, FileMode.Create))
                    {

                        //缓冲区

                        byte[] byts = new byte[1024 * 1024 * 8];

                        //3)循环读取文件流

                        while (true)
                        {

                            int r = fsRead.Read(byts, 0, byts.Length);

                            if (r <= 0)
                            {

                                break;

                            }


                            if (DealData != null) byts = DealData(byts);
                            //5)写入文件
                            if (r < byts.Length) fsWrite.Write(byts, 0, r);
                            else fsWrite.Write(byts, 0, byts.Length);

                        }

                    }

                }
                check = true;
            }
            catch
            {
                check = false;
            }
            return check;
        }

        public static bool CopyBigFileAppend(string source, string target, Func<byte[], byte[]> DealData)
        {

            bool check = false;
            //1)读

            //2)创建文件流
            try
            {
                using (FileStream fsRead = new FileStream(source, FileMode.Open))
                {

                    //4)创建写入流

                    using (FileStream fsWrite = new FileStream(target, FileMode.Append))
                    {

                        //缓冲区

                        byte[] byts = new byte[1024 * 1024 * 8];

                        //3)循环读取文件流

                        while (true)
                        {

                            int r = fsRead.Read(byts, 0, byts.Length);

                            if (r <= 0)
                            {

                                break;

                            }


                            if (DealData != null) byts = DealData(byts);
                            //5)写入文件
                            if (r < byts.Length) fsWrite.Write(byts, 0, r);
                            else fsWrite.Write(byts, 0, byts.Length);

                        }

                    }

                }
                check = true;
            }
            catch
            {
                check = false;
            }
            return check;
        }
        /// <summary>
        /// 从文件指定位置读取数据
        /// </summary>
        /// <param name="Content">数据内容</param>
        /// <param name="BeginReadIndex">开始读取的位置</param>
        /// <param name="BeginIndex">数据开始写入的位置</param>
        /// <param name="Length">要读取的数据长度</param>
        /// <param name="FilePath">文件路径和名称</param>
        /// <returns></returns>
        public static bool Readbyte(byte[] Content, int BeginReadIndex, int BeginIndex, int Length, string FilePath)
        {
            bool check = false;
            if (!File.Exists(FilePath)) return false;

            System.IO.FileStream Sw = File.OpenRead(FilePath);
            Sw.Seek(BeginReadIndex, System.IO.SeekOrigin.Begin);
            try
            {
                Sw.Read(Content, BeginIndex, Length);
                check = true;
            }
            catch
            {
                check = false;
            }
            Sw.Close();
            return check;
        }


        /// <summary>
        /// 从指定的路径文件读取全部的数据
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static byte[] ReaderBytes(string FilePath)
        {

            if (!File.Exists(FilePath)) return null;
            return File.ReadAllBytes(FilePath);
        }

        #endregion

        #region 字符串分割函数
        /// <summary>
        /// 字符串分割函数
        /// </summary>
        /// <param name="source">被分割的数据</param>
        /// <param name="key">分割符</param>
        /// <returns></returns>
        public static string[] StringSplit(string source, string key)
        {
            //string[]  strarry=new string[100];
            int point1 = 0, keylenth = key.Length;
            ArrayList list = new ArrayList();


            while (source != string.Empty)
            {
                point1 = source.IndexOf(key);
                if (point1 > 0)
                {
                    list.Add(source.Substring(0, point1).Trim());

                    source = source.Substring(point1 + keylenth);
                }
                else
                {
                    list.Add(source);
                    source = string.Empty;
                }
            }
            string[] strarry = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                strarry[i] = list[i].ToString();
            }
            //System.Array.Copy(list,strarry,list.Count);
            return strarry;

        }

        /// <summary>
        /// 字符右截取
        /// </summary>
        /// <param name="source">被截取的数据</param>
        /// <param name="len">长度</param>
        /// <returns></returns>
        public static string StringRight(string source, int len)
        {
            return source.Substring(source.Length - len, len);
        }

        /// <summary>
        /// 字符左截取
        /// </summary>
        /// <param name="source">被截取的数据</param>
        /// <param name="len">长度</param>
        /// <returns></returns>
        public static string StringLeft(string source, int len)
        {
            return source.Substring(0, len);
        }
        #endregion

        #region XOR检验算法
        public static int XORCheck(string stringArray)
        {
            return XORCheck(System.Text.Encoding.Default.GetBytes(stringArray));
        }
        public static int XORCheck(byte[] byteArray)
        {

            int sscresult = 0;
            for (int i = 0; i < byteArray.Length; i++)
            {
                sscresult ^= (int)byteArray[i];
            }
            return sscresult;
        }
        /// <summary>
        /// 异或校验 初始值为0
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="StartIndex"></param>
        /// <param name="CheckLength"></param>
        /// <returns></returns>
        public static int XORCheck(byte[] byteArray, int StartIndex, int CheckLength)
        {
            if (byteArray.Length < StartIndex + CheckLength) return 0;
            int sscresult = 0;
            for (int i = StartIndex; i < CheckLength + StartIndex; i++)
            {
                sscresult ^= (int)byteArray[i];
            }
            return sscresult;
        }

        #endregion

        #region 字节数组的操作方法
        /// <summary>
        /// 字节数组进行比较
        /// </summary>
        /// <param name="source">字节数组1</param>
        /// <param name="StartIndex">开始比较索引</param>
        /// <param name="key">字节数组2</param>
        /// <param name="keyStartIndex">开始比较索引</param>
        /// <param name="Blength">总共比较的长度</param>
        /// <returns></returns>
        public static bool BinaryCompare(byte[] source, int StartIndex, byte[] key, int keyStartIndex, int Blength)
        {
            if (source.Length < Blength || key.Length < Blength) return false;
            if (source.Length - StartIndex < Blength || key.Length - keyStartIndex < Blength) return false;
            for (int i = 0; i < Blength; i++)
            {
                if (source[StartIndex + i] != key[keyStartIndex + i]) return false;
            }
            return true;
        }
        /// <summary>
        /// 字节数据查询
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="key">要查找的数据</param>
        /// <returns>返回-1说明没有找到，其他值为第一次出现时，第一个字节所在位置序号</returns>
        public static int BinarySearch(byte[] source, byte[] key)
        {
            try
            {
                bool checker = false;
                int index = -1;
                for (int i = 0; i < source.Length - key.Length; i++)
                {
                    checker = true;
                    for (int k = 0; k < key.Length; k++)
                    {
                        if (source[i + k] != key[k])
                        {
                            checker = false;
                            break;
                        }
                    }
                    if (checker == true)
                    {
                        index = i;
                        break;
                    }

                }
                return index;
            }
            catch
            {
                return -1;
            }
        }
        public static int BinarySearch(byte[] source, byte[] key, int StartIndex)
        {
            return BinarySearch(source, key, StartIndex, source.Length);
        }
        public static int BinarySearch(byte[] source, byte[] key, int StartIndex, int Offset)
        {
            try
            {
                bool checker = false;
                int index = -1;
                for (int i = StartIndex; i < Offset - key.Length; i++)
                {
                    checker = true;
                    for (int k = 0; k < key.Length; k++)
                    {
                        if (source[i + k] != key[k])
                        {
                            checker = false;
                            break;
                        }
                    }
                    if (checker == true)
                    {
                        index = i;
                        break;
                    }

                }
                return index;
            }
            catch
            {
                return -1;
            }
        }

     
        public static int BinarySearchs(byte[] source, List<byte[]> keys, int StartIndex, int Offset)
        {
            try
            {
                bool checker = false;
                int index = -1;
                for (int i = StartIndex; i < Offset; i++)
                {


                    foreach (var keyitem in keys)
                    {
                        checker = true;
                        for (int k = 0; k < keyitem.Length; k++)
                        {
                            if (i + k > source.Length - 1)
                            {
                                checker = false;
                                break;
                            }
                            if (source[i + k] != keyitem[k])
                            {
                                checker = false;
                                break;
                            }
                        }
                        if (checker) break;
                    }
                    if (checker == true)
                    {
                        index = i;
                        break;
                    }

                }
                return index;
            }
            catch
            {
                return -1;
            }
        }
        public static int BinarySearchLast(byte[] source, byte[] key, int StartIndex, int Offset)
        {
            try
            {
                bool checker = false;
                int index = -1;
                for (int i = Offset - 1, len = source.Length - Offset + key.Length - 1; i > len; i--)
                {
                    checker = true;
                    for (int k = 0; k < key.Length; k++)
                    {
                        if (source[i - k] != key[key.Length - k - 1])
                        {
                            checker = false;
                            break;
                        }
                    }
                    if (checker == true)
                    {
                        index = i;
                        break;
                    }

                }
                return index;
            }
            catch
            {
                return -1;
            }
        }


        public static string GetBytesHexStr(byte[] source)
        {
            if (source == null || source.Length == 0) return "";
            string strbuf = "";
            for (int i = 0; i < source.Length; i++)
            {
                strbuf += " " + ((int)source[i]).ToString("X2");
            }
            return strbuf;
        }
        public static string GetBytesHexdata(byte[] source)
        {
            if (source == null || source.Length == 0) return "";
            string strbuf = "";
            for (int i = 0; i < source.Length; i++)
            {
                strbuf += ",0x" + ((int)source[i]).ToString("X2");
            }
            return strbuf;
        }

        /// <summary>
        /// 没有空格
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetBytesHexStrD(byte[] source)
        {
            if (source == null || source.Length == 0) return "";
            string strbuf = "";
            for (int i = 0; i < source.Length; i++)
            {
                strbuf += ((int)source[i]).ToString("X2");
            }
            return strbuf;
        }
        #endregion

        #region  正则检验字符串正确性
        public static bool RegularCheck(string Source, string RegularStr)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(Source, RegularStr);
        }
        /// <summary>
        /// 检验是不是数字字符串
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static bool RegularCheck_09(string Source)
        {
            return RegularCheck(Source, @"^\d*$");
        }
        /// <summary>
        /// 检验是不是大小写字符 或者数字
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static bool RegularCheck_AZaz09(string Source)
        {
            return RegularCheck(Source, @"^[a-zA-Z\d]*$");
        }
        /// <summary>
        /// IP
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static bool RegularCheck_IP(string Source)
        {
            return RegularCheck(Source, @"^\d{2,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
        }
        /// <summary>
        /// 验证车牌号
        /// WJ京12345 （2012武警）
        /// KN12345 （军队车牌）
        /// 京B12345
        /// 京B123456 新能源
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static bool RegularCheck_CarPlate(string Source)
        {
            return RegularCheck(Source, @"^(([\u4e00-\u9fa5_A-Z]{1}[A-Z]{1})|(WJ[\u4e00-\u9fa5]{1})){1}([A-Z_0-9]{5,6}|([A-Z_0-9]{4}[\u4e00-\u9fa5]{1}))$");
        }
        /// <summary>
        /// 检查是否是域名
        /// </summary>
        /// <param name="IPs"></param>
        /// <returns></returns>
        public static string RegularCheckDNSToIP(string IPs)
        {
            if (RegularCheck_IP(IPs)) return IPs;
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(IPs);
                return hostEntry.AddressList[0].ToString();
            }
        }

        #endregion

        #region 整形变量与字节数组的相互转换
        public static byte[] int2Byte(long intValue, int bytesLength)
        {

            return int2Byte(intValue, bytesLength, true);
        }
        public static int byte2Int_I(byte[] b, int StartIndex, int Offset, bool BigOrSmall)
        {
            return Convert.ToInt32(byte2Int(b, StartIndex, Offset, BigOrSmall));
        }
        public static byte[] int2Byte(long intValue, int bytesLength, bool BigOrSmall)
        {
            byte[] b = new byte[bytesLength];
            if (BigOrSmall)
            {
                for (int i = 0; i < bytesLength; i++)
                {
                    b[i] = (byte)(intValue % 256);
                    intValue = intValue / 256;
                    if (intValue == 0) break;
                }
            }
            else
            {
                for (int i = 0; i < bytesLength; i++)
                {
                    b[bytesLength - 1 - i] = (byte)(intValue % 256);
                    intValue = intValue / 256;
                    if (intValue == 0) break;
                }
            }
            return b;
        }
        public static void int2Byte(long intValue, byte[] b, int StartIndex, int Offset)
        {

            int2Byte(intValue, b, StartIndex, Offset, true);


        }
        public static void int2Byte(long intValue, byte[] b, int StartIndex, int Offset, bool BigOrSmall)
        {
            if (BigOrSmall)
            {
                for (int i = 0; i < Offset; i++)
                {
                    b[i + StartIndex] = (byte)(intValue % 256);
                    intValue = intValue / 256;
                    //if (intValue == 0) break;
                }
            }
            else
            {
                for (int i = 0; i < Offset; i++)
                {
                    b[StartIndex + Offset - 1 - i] = (byte)(intValue % 256);
                    intValue = intValue / 256;
                    //if (intValue == 0) break;
                }
            }

        }
        public static int byte2Int(byte[] b)
        {

            return byte2Int(b, true);
        }
        public static int byte2Int(byte[] b, bool BigOrSmall)
        {
            int intValue = 0;
            if (BigOrSmall)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    intValue += (b[i] & 0xFF) << (8 * i);

                }
            }
            else
            {
                for (int i = 0; i < b.Length; i++)
                {
                    intValue += (b[i] & 0xFF) << (8 * (b.Length - 1 - i));

                }
            }
            return intValue;
        }


        public static long byte2Int(byte[] b, int StartIndex, int Offset)
        {
            return byte2Int(b, StartIndex, Offset, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="StartIndex"></param>
        /// <param name="Offset"></param>
        /// <param name="BigOrSmall"> true- 高位在后 false -高位在前 </param>
        /// <returns></returns>
        public static long byte2Int(byte[] b, int StartIndex, int Offset, bool BigOrSmall)
        {
            long intValue = 0;
            if (BigOrSmall)
            {
                for (int i = 0; i < Offset; i++)
                {

                    intValue += ((long)(b[i + StartIndex] & 0xFF)) << (8 * i);

                }
            }
            else
            {
                for (int i = 0; i < Offset; i++)
                {
                    //intValue1=b[i+StartIndex] & 0xFF;
                    //intValue1=intValue1<<(8*(Offset-1-i));
                    intValue += ((long)(b[i + StartIndex] & 0xFF)) << (8 * (Offset - 1 - i));
                    //intValue+=intValue1;

                }
            }
            return intValue;

        }
        public static ulong byte2Ulong(byte[] b, int StartIndex, int Offset, bool BigOrSmall)
        {
            ulong intValue = 0;
            if (BigOrSmall)
            {
                for (int i = 0; i < Offset; i++)
                {

                    intValue += ((ulong)(b[i + StartIndex] & 0xFF)) << (8 * i);

                }
            }
            else
            {
                for (int i = 0; i < Offset; i++)
                {
                    //intValue1=b[i+StartIndex] & 0xFF;
                    //intValue1=intValue1<<(8*(Offset-1-i));
                    intValue += ((ulong)(b[i + StartIndex] & 0xFF)) << (8 * (Offset - 1 - i));
                    //intValue+=intValue1;

                }
            }
            return intValue;

        }

        public static long Hexbyte2Int(byte[] b, int StartIndex, int Offset)
        {
            long intValue = 0;
            StringBuilder strb = new StringBuilder(string.Empty);
            for (int i = 0; i < Offset; i++)
            {

                if ((int)b[i + StartIndex] < 16) strb.Append("0");
                strb.Append(((int)b[i + StartIndex]).ToString("X"));

            }
            try
            {
                intValue = long.Parse(strb.ToString());
            }
            catch { }
            return intValue;

        }
        public static byte[] GetBytesFromHexString(string DataS)
        {
            DataS = DataS.Replace(" ", "");
            if (string.IsNullOrEmpty(DataS)) return null;
            byte[] JLYRecordDataB = new byte[DataS.Length / 2];
            TxtToolBLL.CopyStringTpbyte(ref JLYRecordDataB, 0, DataS);

            return JLYRecordDataB;
        }

        public static void CopyStringTpbyte(ref byte[] data, int index, string Cont)
        {
            if (string.IsNullOrEmpty(Cont)) return;
            int count = Cont.Length / 2;
            for (int i = 0; i < count; i++)
            {
                data[i + index] = (byte)Convert.ToInt32("0x" + Cont.Substring(i * 2, 2), 16);
            }
        }

        /// <summary>
        /// 从16进制字符串转换成bytes
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static byte[] GetBytesByHexStringData(string datas)
        {
            string msgDataStr = datas.Replace(" ", "");
            byte[] msgDatabytes = new byte[msgDataStr.Length / 2];
            CopyStringTpbyte(ref msgDatabytes, 0, msgDataStr);
            return msgDatabytes;
        }
        #endregion      

        #region BCD码解析

        public static byte[] BCDbytesFromString(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            int LengthV = value.Length / 2;
            List<byte> DataByteList = new List<byte>();
            for (int i = 0; i < LengthV; i++)
            {
                DataByteList.Add(Convert.ToByte(Convert.ToInt32(value.Substring(2 * i, 2), 16)));
            }
            return DataByteList.ToArray();
        }
        /// <summary>
        /// 根据数字字符串转变成指定长度的BCD数组
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ByteLen"></param>
        /// <returns></returns>
        public static byte[] BCDbyteFromString(string value, int ByteLen)
        {
            byte[] byetbuf = new byte[ByteLen];
            string valuestr = "00000000000000000000" + value;
            valuestr = valuestr.Substring(valuestr.Length - ByteLen * 2, ByteLen * 2);
            for (int i = 0; i < ByteLen; i++)
            {
                //byetbuf[i] = (byte)Convert.ToInt32(StringRight(valuestr, 2), 16);
                //if (!string.IsNullOrEmpty(valuestr)) valuestr = valuestr.Substring(0, valuestr.Length - 2);
                byetbuf[i] = (byte)Convert.ToInt32(valuestr.Substring(i * 2, 2), 16);

            }
            return byetbuf;
        }



        public static string BCDbytesToString(byte[] b, int StartIndex, int Offset)
        {
            if (b == null) return null;
            if (b.Length < StartIndex + Offset) return null;
            StringBuilder strb = new StringBuilder(string.Empty);
            for (int i = 0; i < Offset; i++)
            {
                strb.AppendFormat("{0:X2}", b[i + StartIndex]);
            }
            return strb.ToString();
        }
        public static byte[] BCDbyteFromInt(int value, int ByteLen)
        {
            byte[] byetbuf = new byte[ByteLen];
            string valuestr = IntToDECStr(value, ByteLen * 2);
            for (int i = 0; i < ByteLen; i++)
            {
                byetbuf[i] = (byte)Convert.ToInt32(StringRight(valuestr, 2), 16);
                if (!string.IsNullOrEmpty(valuestr)) valuestr = valuestr.Substring(0, valuestr.Length - 2);
            }
            return byetbuf;
        }
        /// <summary>
        /// 从整数转变成指定长度的BCD数据
        /// </summary>
        /// <param name="value"></param>
        /// <param name="ByteLen"></param>
        /// <returns></returns>
        public static byte[] BCDbyteFromIntLeft(int value, int ByteLen)
        {
            byte[] byetbuf = new byte[ByteLen];
            string valuestr = IntToDECStr(value, ByteLen * 2);
            for (int i = 0; i < ByteLen; i++)
            {
                byetbuf[i] = (byte)Convert.ToInt32(valuestr.Substring(0, 2), 16);
                if (!string.IsNullOrEmpty(valuestr)) valuestr = valuestr.Substring(2);
            }
            return byetbuf;
        }

        public static string BCDbyteToString(byte b)
        {
            return string.Format("{0:0}{1:0}", (int)b >> 4, (int)b & 15);
        }
        public static byte[] BCDbyteFromDateTime(DateTime tt)
        {
            if (tt == DateTime.MinValue || tt == DateTime.MaxValue) return null;
            byte[] b = new byte[6];
            b[0] = (byte)Convert.ToInt32(tt.Year.ToString("0000").Substring(2, 2), 16);
            b[1] = (byte)Convert.ToInt32(tt.Month.ToString("00"), 16);
            b[2] = (byte)Convert.ToInt32(tt.Day.ToString("00"), 16);
            b[3] = (byte)Convert.ToInt32(tt.Hour.ToString("00"), 16);
            b[4] = (byte)Convert.ToInt32(tt.Minute.ToString("00"), 16);
            b[5] = (byte)Convert.ToInt32(tt.Second.ToString("00"), 16);
            return b;

        }
        public static void BCDbyteFromDateTimeToBytes(DateTime tt, byte[] b, int StartIndex)
        {
            BCDbyteFromDateTimeToBytes(tt, b, StartIndex, 6);
        }
        /// <summary>
        /// 时间转换为6字节BCD数据放置到指定位置
        /// </summary>
        /// <param name="tt"></param>
        /// <param name="b"></param>
        /// <param name="StartIndex"></param>
        public static void BCDbyteFromDateTimeToBytes(DateTime tt, byte[] b, int StartIndex, int LengthV)
        {

            byte[] bt = BCDbyteFromDateTime(tt);
            if (bt == null) return;
            System.Array.Copy(bt, 0, b, StartIndex, LengthV);

        }


        public static string BCDTimebytesToString(byte[] b, int StartIndex)
        {
            return BCDTimebytesToString(b, StartIndex, 6);
        }
        public static string BCDTimebytesToString(byte[] b, int StartIndex, int Len)
        {
            StringBuilder strb = new StringBuilder(string.Empty);
            strb.Append(DateTime.Now.Year.ToString().Substring(0, 2));
            strb.AppendFormat("{0:0}{1:0}", (int)b[StartIndex + 0] >> 4, (int)b[StartIndex + 0] & 15);
            strb.Append("-");
            strb.AppendFormat("{0:0}{1:0}", (int)b[StartIndex + 1] >> 4, (int)b[StartIndex + 1] & 15);
            strb.Append("-");
            strb.AppendFormat("{0:0}{1:0}", (int)b[StartIndex + 2] >> 4, (int)b[StartIndex + 2] & 15);
            strb.Append(" ");
            if (StartIndex + 3 < b.Length && Len > 3)
            {
                strb.AppendFormat("{0:0}{1:0}", (int)b[StartIndex + 3] >> 4, (int)b[StartIndex + 3] & 15);
            }
            else
            {
                strb.AppendFormat("00");
            }
            strb.Append(":");
            if (StartIndex + 4 < b.Length && Len > 4)
            {
                strb.AppendFormat("{0:0}{1:0}", (int)b[StartIndex + 4] >> 4, (int)b[StartIndex + 4] & 15);
            }
            else
            {
                strb.AppendFormat("00");
            }
            strb.Append(":");
            if (StartIndex + 4 < b.Length && Len > 5)
            {
                strb.AppendFormat("{0:0}{1:0}", (int)b[StartIndex + 5] >> 4, (int)b[StartIndex + 5] & 15);
            }
            else
            {
                strb.AppendFormat("00");
            }
            return strb.ToString();
        }

        #endregion

        #region 格式转换函数
        public static byte[] ToBigEndianBytes<T>( int source)
        {
            byte[] bytes;

            var type = typeof(T);
            if (type == typeof(ushort))
                bytes = BitConverter.GetBytes((ushort)source);
            else if (type == typeof(ulong))
                bytes = BitConverter.GetBytes((ulong)source);
            else if (type == typeof(int))
                bytes = BitConverter.GetBytes(source);
            else
                throw new InvalidCastException("Cannot be cast to T");

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public static int ToLittleEndianInt( byte[] source)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(source);

            if (source.Length == 2)
                return BitConverter.ToUInt16(source, 0);

            if (source.Length == 8)
                return (int)BitConverter.ToUInt64(source, 0);

            throw new ArgumentException("Unsupported Size");
        }

        public static object ToInt(string s)
        {
            if (s == null || s == String.Empty)
                return System.DBNull.Value;
            else
                return Convert.ToInt32(s);
        }

        public static object ToInt(string s, string flag)
        {
            if (s == flag)
                return System.DBNull.Value;
            else
                return Convert.ToInt32(s);
        }

        public static object ToInt(string s, int DefaultValue)
        {
            if (s == null || s == String.Empty)
                return DefaultValue;
            else
                return Convert.ToInt32(s);
        }

        public static object ToString(string s)
        {
            if (s == null || s == String.Empty)
                return System.DBNull.Value;
            else
                return s;
        }
        public static object Tobool(string s)
        {
            if (s == null || s == String.Empty)
                return System.DBNull.Value;
            else
                return Convert.ToBoolean(s);
        }

        public static object ToString(string s, string flag)
        {
            if (s == flag)
                return System.DBNull.Value;
            else
                return s;
        }

        public static object ToDateTime(string s)
        {
            if (s == null || s == String.Empty)
                return System.DBNull.Value;
            else
                return Convert.ToDateTime(s);
        }


        public static object ToDateTime(string s, string flag)
        {
            if (s == flag)
                return System.DBNull.Value;
            else
                return Convert.ToDateTime(s);
        }

        public static object ToDecimal(string s)
        {
            if (s == null || s == String.Empty)
                return System.DBNull.Value;
            else
                return Convert.ToDecimal(s);
        }

        public static object ToDecimal(string s, string flag)
        {
            if (s == flag)
                return System.DBNull.Value;
            else
                return Convert.ToDecimal(s);
        }

        public static string IntToHEXStr(int v, int l)
        {
            string buf = string.Format("000000000000000000{0:x}", v);
            return buf.Substring(buf.Length - l, l);
        }
        public static string IntToDECStr(int v, int l)
        {
            string buf = string.Format("000000000000000000{0}", v);
            return buf.Substring(buf.Length - l, l);
        }
        #endregion            

        #region UTC 时间转换

        //一毫秒的 Tick 数: 10000
        public static long GetUTCLongTicksByTime(DateTime davev)
        {
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan TP = davev - dtZone;
            return Convert.ToInt64(TP.Ticks - TimeZoneInfo.Local.BaseUtcOffset.Ticks);
        }
        /// <summary>
        /// 从当前时区系统时间转换为Unix时间戳（Unix timestamp）
        /// 考虑时区因素  毫秒差
        /// </summary>
        /// <param name="davev"></param>
        /// <returns></returns>
        public static long GetUTCLongMsByTime(DateTime davev)
        {            
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan TP = davev - dtZone;
            return Convert.ToInt64(TP.TotalMilliseconds - TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds);
        }

        /// <summary>  
        /// DateTime时间格式转换为Unix时间戳格式 秒差 考虑了时区偏移 
        /// </summary>  
        /// <param name="time"> DateTime时间格式</param>  
        /// <returns>Unix时间戳格式</returns>  
        public static int GetUTCtimeValueSeconds(System.DateTime davev)
        {
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            TimeSpan TP = davev - dtZone;
            return Convert.ToInt32(TP.TotalSeconds - TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds);
        }
        /// <summary>
        /// Unix时间戳（Unix timestamp） 转换为当前时区的系统时间
        /// 将Unix时间戳 转变为本地时间 加上了时区便宜量
        /// </summary>
        /// <param name="davev"></param>
        /// <returns></returns>
        public static DateTime GetUTCtimeByLongMs(long davev)
        {
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            return dtZone.AddMilliseconds(davev + TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds);
        }
        /// <summary>
        /// 将Unix时间戳(秒差) 转变为本地时间 加上了时区便宜量
        /// </summary>
        /// <param name="Seconds"></param>
        /// <returns></returns>
        public static DateTime GetUTCtimeByLongS(long Seconds)
        {
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            return dtZone.AddSeconds(Seconds + TimeZoneInfo.Local.BaseUtcOffset.TotalSeconds);
        }


        /// <summary>
        /// 将时间转换为与1970-1-1 0:0:0 相距秒数 不考虑时区
        /// </summary>
        /// <param name="davev"></param>
        /// <returns></returns>
        public static long GetTimeLongValueSecond(DateTime dt)
        {            
            return Convert.ToInt64((dt - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);

        }
        /// <summary>
        /// 将时间转换为与1970-1-1 0:0:0 相距毫秒差 不考虑时区
        /// </summary>
        /// <param name="davev"></param>
        /// <returns></returns>
        public static long GetTimeLongValueMilliseconds(DateTime dt)
        {
            return Convert.ToInt64((dt- new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
        }
        /// <summary>
        /// 将时间转换为与1970-1-1 0:0:0 相距秒数 不考虑时区
        /// </summary>
        /// <param name="davev"></param>
        /// <returns></returns>
        public static int GetTimeLongValueSecondINT(DateTime davev)
        {
            return Convert.ToInt32(GetTimeLongValueSecond(davev));
        }
       
       
        /// <summary>
        /// 不考虑时区的问题，只考虑与1970-1-1之间的秒数差距
        /// </summary>
        /// <param name="davev"></param>
        /// <returns></returns>
        public static DateTime GetTimeByLongSecond(long davev)
        {
            return new System.DateTime(1970, 1, 1).AddSeconds(davev);
        }
        /// <summary>
        /// 不考虑时区的问题，只考虑与1970-1-1之间的毫秒差距
        /// </summary>
        /// <param name="davev"></param>
        /// <returns></returns>
        public static DateTime GetUTCtimeValueByLong(long davev)
        {
            return new System.DateTime(1970, 1, 1).AddMilliseconds(davev);
           
        }
        /// <summary>
        /// 将时间转换为UTC-Long数据 写到字节数组中
        /// </summary>
        /// <param name="b"></param>
        /// <param name="Index"></param>
        /// <param name="davev"></param>
        public static void WriteUTCtimeToBytes(byte[] b, int Index, DateTime davev)
        {

            long TLong = GetTimeLongValueMilliseconds(davev);
            //Convert.ToUInt64(TxtTool.GetUTCtimeValue(davev));
            TxtToolBLL.int2Byte(TLong, b, Index, 8, false);
        }
        #endregion

        #region 时间变量与字节数组的相互转换
        /// <summary>
        /// 1=yyyy-MM-dd HH:mm:ss 2=yyyy-MM-ddTHH:mm:ss 3=yyyy/MM/dd HH:mm:ss 4=yyyy年MM月dd日HH时mm分ss秒 5=yyyy年MM月dd日 6=yyyyMMddHHmmss 7=Unix时间戳(秒) 8=Unix时间戳(毫秒)
        /// </summary>
        /// <param name="S"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime? ParseStrToDateTime(string S, int format)
        {
            //{ label: 'yyyy-MM-dd HH:mm:ss',value: 1},
            //        { label: 'yyyy-MM-ddTHH:mm:ss',value: 2},
            //        { label: 'yyyy/MM/dd HH:mm:ss',value: 3},
            //        { label: 'yyyy年MM月dd日HH时mm分ss秒',value: 4},
            //        { label: 'yyyy年MM月dd日',value: 5},
            //        { label: 'yyyyMMddHHmmss',value: 6},
            //{ label: 'Unix时间戳(秒)',value: 7},
             //       { label: 'Unix时间戳(毫秒)',value: 8},
            if (string.IsNullOrEmpty(S)) return null;
            try
            {
                switch (format)
                {
                    case 1: return DateTime.Parse(S);
                    case 2: return DateTime.ParseExact(S.Replace(" ","").Replace("T",""), "yyyy-MM-ddHH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    case 3: return DateTime.ParseExact(S, "yyyy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    case 4: return DateTime.ParseExact(S, "yyyy年MM月dd日HH时mm分ss秒", System.Globalization.CultureInfo.CurrentCulture);
                    case 5: return DateTime.ParseExact(S, "yyyy年MM月dd日", System.Globalization.CultureInfo.CurrentCulture);
                    case 6: return DateTime.ParseExact(S, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                    case 7: return S.Cof_ToSafeLong().Cof_UTCSecondToDateTime();
                    case 8: return S.Cof_ToSafeLong().Cof_UTCMilliSecondToDateTime();
                }
            }
            catch { 
            }
            return null;
        }


        public static byte[] DateTime2BCDByte7(DateTime datetimeV)
        {

            if (datetimeV == DateTime.MinValue || datetimeV == DateTime.MaxValue) return null;
            byte[] b = new byte[7];

            b[0] = (byte)Convert.ToInt32((datetimeV.Year / 100).ToString("00"), 16);
            b[1] = (byte)Convert.ToInt32((datetimeV.Year % 100).ToString("00"), 16);
            b[2] = (byte)Convert.ToInt32(datetimeV.Month.ToString("00"), 16);
            b[3] = (byte)Convert.ToInt32(datetimeV.Day.ToString("00"), 16);
            b[4] = (byte)Convert.ToInt32(datetimeV.Hour.ToString("00"), 16);
            b[5] = (byte)Convert.ToInt32(datetimeV.Minute.ToString("00"), 16);
            b[6] = (byte)Convert.ToInt32(datetimeV.Second.ToString("00"), 16);
            return b;


        }
        public static byte[] DateTime2Byte7_809(DateTime datetimeV)
        {
            byte[] b = new byte[7];
            b[0] = (byte)datetimeV.Day;
            b[1] = (byte)datetimeV.Month;
            b[2] = (byte)(datetimeV.Year / 256);
            b[3] = (byte)(datetimeV.Year % 256);


            b[4] = (byte)datetimeV.Hour;
            b[5] = (byte)datetimeV.Minute;
            b[6] = (byte)datetimeV.Second;

            return b;
        }

        public static byte[] DateTime2Byte7(DateTime datetimeV)
        {
            byte[] b = new byte[7];
            b[0] = (byte)(datetimeV.Year / 100);
            b[1] = (byte)(datetimeV.Year % 100);
            b[2] = (byte)datetimeV.Month;
            b[3] = (byte)datetimeV.Day;
            b[4] = (byte)datetimeV.Hour;
            b[5] = (byte)datetimeV.Minute;
            b[6] = (byte)datetimeV.Second;

            return b;
        }
        public static void DateTime2Byte(DateTime datetimeV, byte[] b, int StartIndex, int length)
        {
            if (b == null || b.Length <= StartIndex) return;
            if (length < 7)
            {

                b[StartIndex] = (byte)(datetimeV.Year % 100);
                b[StartIndex + 1] = (byte)datetimeV.Month;
                b[StartIndex + 2] = (byte)datetimeV.Day;
                if (StartIndex + 3 < b.Length && length > 3)
                    b[StartIndex + 3] = (byte)datetimeV.Hour;
                if (StartIndex + 4 < b.Length && length > 4)
                    b[StartIndex + 4] = (byte)datetimeV.Minute;
                if (StartIndex + 5 < b.Length && length > 5)
                    b[StartIndex + 5] = (byte)datetimeV.Second;
            }

            if (length == 7)
            {
                b[StartIndex + 0] = (byte)(datetimeV.Year / 100);
                b[StartIndex + 1] = (byte)(datetimeV.Year % 100);
                b[StartIndex + 2] = (byte)datetimeV.Month;
                b[StartIndex + 3] = (byte)datetimeV.Day;
                b[StartIndex + 4] = (byte)datetimeV.Hour;
                b[StartIndex + 5] = (byte)datetimeV.Minute;
                b[StartIndex + 6] = (byte)datetimeV.Second;
            }


        }
        public static string DateTimeFromByte(byte[] b, int StartIndex, int length)
        {
            if (b == null || b.Length <= StartIndex) return "";

            StringBuilder strb = new StringBuilder(string.Empty);
            if (length < 7)
            {
                strb.Append(DateTime.Now.Year.ToString().Substring(0, 2));
                strb.AppendFormat("{0:00}", b[StartIndex]);
                strb.AppendFormat("-{0:00}", b[StartIndex + 1]);
                strb.AppendFormat("-{0:00}", b[StartIndex + 2]);
                if (StartIndex + 3 < b.Length && length > 3)
                    strb.AppendFormat(" {0:00}", b[StartIndex + 3]);
                if (StartIndex + 4 < b.Length && length > 4)
                    strb.AppendFormat(":{0:00}", b[StartIndex + 4]);
                if (StartIndex + 5 < b.Length && length > 5)
                    strb.AppendFormat(":{0:00}", b[StartIndex + 5]);
            }

            if (length == 7)
            {
                strb.AppendFormat("{0:00}", b[StartIndex + 0]);
                strb.AppendFormat("{0:00}", b[StartIndex + 1]);
                strb.AppendFormat("-{0:00}", b[StartIndex + 2]);
                strb.AppendFormat("-{0:00}", b[StartIndex + 3]);
                if (StartIndex + 4 < b.Length)
                    strb.AppendFormat(" {0:00}", b[StartIndex + 4]);
                if (StartIndex + 5 < b.Length)
                    strb.AppendFormat(":{0:00}", b[StartIndex + 5]);
                if (StartIndex + 6 < b.Length)
                    strb.AppendFormat(":{0:00}", b[StartIndex + 6]);
            }

            return strb.ToString();
        }
        #endregion

        #region 常用数据转换
        /// <summary>
        /// 根据月份数据yyyyMM获得次月第一天
        /// </summary>
        /// <param name="MonthName">yyyyMM</param>
        /// <returns></returns>
        public static string GetFirstDay(string MonthName)
        {
            return string.Format("{0}-{1}-01", MonthName.Substring(0, 4), MonthName.Substring(4));
        }
        /// <summary>
        /// 根据月份数据yyyyMM获得次月最后一天
        /// </summary>
        /// <param name="MonthName">yyyyMM</param>
        /// <returns></returns>
        public static string GetLastDay(string MonthName)
        {
            return DateTime.Parse(GetFirstDay(MonthName)).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
        }
        /// <summary>
        /// 获得里程显示字符串
        /// </summary>
        /// <param name="MeterV"></param>
        /// <returns></returns>
        public static string GetMilageStringByMeter(int MeterV)
        {
            return (MeterV / 1000).ToString("0.##");
        }
        /// <summary>
        /// 将秒数转换成 h:m:s 格式
        /// </summary>
        /// <param name="SecondV"></param>
        /// <returns></returns>
        public static string GetPersisStringBySecond(int SecondV)
        {
            StringBuilder sb = new StringBuilder();
            //if (SecondV >= 3600) 
            sb.AppendFormat("{0}:", SecondV / 3600);
            //if (SecondV >= 60) 
            sb.AppendFormat("{0}:", SecondV % 3600 / 60);
            sb.AppendFormat("{0}", SecondV % 60);

            return sb.ToString();
        }
        /// <summary>
        /// 将秒数转换成 h:m 格式
        /// </summary>
        /// <param name="SecondV"></param>
        /// <returns></returns>
        public static string GetPersisStringBySecond_hm(int SecondV)
        {
            StringBuilder sb = new StringBuilder();
            //if (SecondV >= 3600) 
            sb.AppendFormat("{0}:", SecondV / 3600);
            //if (SecondV >= 60) 
            sb.AppendFormat("{0}", SecondV % 3600 / 60);


            return sb.ToString();
        }
        public static string GetPersisStringBySecond_hm(long SecondV)
        {
            StringBuilder sb = new StringBuilder();
            //if (SecondV >= 3600) 
            sb.AppendFormat("{0}:", SecondV / 3600);
            //if (SecondV >= 60) 
            sb.AppendFormat("{0}", SecondV % 3600 / 60);


            return sb.ToString();
        }
        #endregion     

        #region Post
        public static string PostDataToUrlJson(Dictionary<string, string> headersdic, string MsgData, string Url)
        {
            try
            {
                if (string.IsNullOrEmpty(MsgData)) MsgData = "{}";
                //注意提交的编码 这边是需要改变的 这边默认的是Default：系统当前编码
                byte[] postData = Encoding.UTF8.GetBytes(MsgData);
                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                //Encoding myEncoding = Encoding.UTF8;
                request.Method = "POST";
                request.KeepAlive = false;
                request.AllowAutoRedirect = true;
                request.ContentType = "application/json";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
                request.ContentLength = postData.Length;

                if (headersdic.Cof_CheckAvailable())
                {
                    foreach (var item in headersdic)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }

                // 提交请求数据 
                System.IO.Stream outputStream = request.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();
               
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                string srcString = reader.ReadToEnd();             
                reader.Close();
                return srcString;
            }
            catch (SystemException ee)
            {
                return "error:" + ee.Message + ":" + ee.StackTrace;
            }
        }

        public static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }
        public static string PostDataToUrl_Mn(string MsgData, string Url, string Referer, string Host)
        {
            try
            {

                //注意提交的编码 这边是需要改变的 这边默认的是Default：系统当前编码
                byte[] postData = Encoding.UTF8.GetBytes(MsgData);
                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                //Encoding myEncoding = Encoding.UTF8;
                SetHeaderValue(request.Headers, "host", Host);
                request.Referer = Referer;
                request.Method = "POST";
                request.KeepAlive = true;
                request.AllowAutoRedirect = true;
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36";
                request.ContentLength = postData.Length;



                // 提交请求数据 
                System.IO.Stream outputStream = request.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();

                HttpWebResponse response;
                Stream responseStream;
                StreamReader reader;
                string srcString;
                response = request.GetResponse() as HttpWebResponse;
                responseStream = response.GetResponseStream();
                reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                string result = srcString;   //返回值赋值
                reader.Close();
                return result;
            }
            catch (SystemException ee)
            {
                return "error:" + ee.Message + ":" + ee.StackTrace;
            }
        }

        public static string PostDataToUrl(string MsgData, string Url)
        {
            try
            {

                //注意提交的编码 这边是需要改变的 这边默认的是Default：系统当前编码
                byte[] postData = Encoding.UTF8.GetBytes(MsgData);
                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                //Encoding myEncoding = Encoding.UTF8;
                request.Method = "POST";
                request.KeepAlive = false;
                request.AllowAutoRedirect = true;
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
                request.ContentLength = postData.Length;



                // 提交请求数据 
                System.IO.Stream outputStream = request.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();

                HttpWebResponse response;
                Stream responseStream;
                StreamReader reader;
                string srcString;
                response = request.GetResponse() as HttpWebResponse;
                responseStream = response.GetResponseStream();
                reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                string result = srcString;   //返回值赋值
                reader.Close();
                return result;
            }
            catch (SystemException ee)
            {
                return "error:" + ee.Message + ":" + ee.StackTrace;
            }
        }


        public static string GetHtmlStr(string url, string encoding)
        {

            string htmlStr = "";

            try

            {

                if (!String.IsNullOrEmpty(url))

                {

                    WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  

                    WebResponse response = request.GetResponse();           //创建WebResponse对象  

                    Stream datastream = response.GetResponseStream();       //创建流对象  

                    Encoding ec = Encoding.Default;

                    if (encoding == "UTF8")

                    {

                        ec = Encoding.UTF8;

                    }

                    else if (encoding == "Default")

                    {

                        ec = Encoding.Default;

                    }

                    StreamReader reader = new StreamReader(datastream, ec);

                    htmlStr = reader.ReadToEnd();                  //读取网页内容  

                    reader.Close();

                    datastream.Close();

                    response.Close();

                }

            }

            catch (System.Exception ee)
            {
                string ss = ee.Message;
            }

            return htmlStr;

        }

        public static string GetHTMLFromUrl(string Url)
        {
            try
            {

                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                //request.Method = "GET";
                request.KeepAlive = false;
                request.AllowAutoRedirect = true;
                request.ContentType = "text/html";
                request.Timeout = 20000;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";

                string srcString;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();


                StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                reader.Close();
                return srcString;
            }
            catch (SystemException ee)
            {
                TextLogerBll.LogWriter("GetDataFromUrl--" + Url, ee);
                return "error:" + ee.Message + ":" + ee.StackTrace;
            }
        }

        public static string GetDataFromUrlKeepAlive(string Url)
        {
            try
            {

                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                request.Method = "GET";
                request.KeepAlive = true;
                request.AllowAutoRedirect = true;
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";

                string srcString;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                reader.Close();
                return srcString;
            }
            catch (SystemException ee)
            {
                return "error:" + ee.Message + ":" + ee.StackTrace;
            }
        }
        public static string GetDataFromUrl(string Url)
        {
            try
            {

                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                request.Method = "GET";
                request.KeepAlive = false;
                request.AllowAutoRedirect = true;
                request.ContentType = "text";
                request.Timeout = 20000;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";

                string srcString;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                reader.Close();
                return srcString;
            }
            catch (SystemException ee)
            {
                TextLogerBll.LogWriter("GetDataFromUrl--" + Url, ee);
                return "error:" + ee.Message + ":" + ee.StackTrace;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受   
        }
        public static string PostDataToUrlGGJT(string MsgData, string Url, string APIname, string APItoken)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                //注意提交的编码 这边是需要改变的 这边默认的是Default：系统当前编码
                byte[] postData = Encoding.UTF8.GetBytes(MsgData);
                // 设置提交的相关参数 
                HttpWebRequest request = WebRequest.Create(Url) as HttpWebRequest;
                //Encoding myEncoding = Encoding.UTF8;
                request.ProtocolVersion = HttpVersion.Version11;
                request.Method = "POST";
                //request.KeepAlive = false;
                //request.AllowAutoRedirect = true;
                request.ContentType = "application/x-www-form-urlencoded";
                // request.ContentType = "form-data";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727; .NET CLR  3.0.04506.648; .NET CLR 3.5.21022; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729)";
                request.ContentLength = postData.Length;
                request.Headers["apiName"] = APIname;
                request.Headers["apiToken"] = APItoken;

                // 提交请求数据 
                System.IO.Stream outputStream = request.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Close();

                HttpWebResponse response;
                Stream responseStream;
                StreamReader reader;
                string srcString;
                response = request.GetResponse() as HttpWebResponse;
                responseStream = response.GetResponseStream();
                reader = new System.IO.StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                srcString = reader.ReadToEnd();
                string result = srcString;   //返回值赋值
                reader.Close();
                return result;
            }
            catch (SystemException ee)
            {
                return "error:" + ee.Message + ":" + ee.StackTrace + ",url:" + Url;
            }
        }
        #endregion    
    }
}
