using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Blog.Core.Common.Helper
{
    public static class CommonExtFunction
    {
        #region 生成Guid Long
        private static int SHORT_MAX = 65535;
        private static int counter = 0;
        private static DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
        public static long GetNextGuid()
        {
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

        #region XML
        public static string Cof_GetNodeValue(this XmlDocument Xdoc, string Path)
        {
            XmlNode xd = Xdoc.SelectSingleNode(Path);
            if (xd != null) return xd.InnerText;
            return "";
        }
        #endregion

        #region 格式转换

        public static byte[] ToBigEndianBytes<T>(this int source)
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

        public static int ToLittleEndianInt(this byte[] source)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(source);

            if (source.Length == 2)
                return BitConverter.ToUInt16(source, 0);

            if (source.Length == 8)
                return (int)BitConverter.ToUInt64(source, 0);

            throw new ArgumentException("Unsupported Size");
        }

        public static byte[] Cof_ToBytes(this long D, int Len)
        {
            byte[] Dbytes = new byte[Len];
            TxtToolBLL.int2Byte(D, Dbytes, 0, Len, false);
            return Dbytes;
        }
        public static byte[] Cof_ToBytesX(this long D, int Len)
        {
            byte[] Dbytes = new byte[Len];
            TxtToolBLL.int2Byte(D, Dbytes, 0, Len, true);
            return Dbytes;
        }
        public static string GetTimeString_hms(this int SecondV)
        {
            return string.Format("{0}:{1}:{2}", SecondV / 3600, SecondV % 3600 / 60, SecondV % 60);

        }

        public static string GetTimeString_hm(this int SecondV)
        {
            return string.Format("{0}:{1}", SecondV / 3600, SecondV % 3600 / 60);


        }

        /// <summary>
        /// 泛型安全转换为字符串
        /// 如果为Not HasValue 则返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Tv"></param>
        /// <returns></returns>
        public static string Cof_SafeToString<T>(this T Tv)
        {
            if (Tv == null) return null;
            return Tv.ToString();
        }
        public static string Cof_SafeToStringInflux<T>(this T Tv)
        {
            if (Tv == null || Tv.ToString()=="null") return null;
            return Tv.ToString();
        }
        /// <summary>
        /// 针对Jtoken  过滤 " () [] | ,
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Tv"></param>
        /// <returns></returns>
        public static string Cof_SafeToStringJ<T>(this T Tv)
        {
            if (Tv == null) return "";
            return Tv.ToString().Replace("[]", "").Replace("\"", "").Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Replace("|", "");
        }
        /// <summary>
        /// 有值则转变字符串 没有则返回空字符串
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Cof_ToEmptyString<TEntity>(this TEntity? o) where TEntity : struct
        {
            if (o.HasValue) return o.ToString();
            return "";
        }
        public static string ToEmptyString<TEntity>(this TEntity? o) where TEntity : struct
        {
            if (o.HasValue) return o.ToString();
            return "";
        }
        /// <summary>
        /// 整形泛型Tostring null 返回 "0"
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Cof_ToZeroString(this int? o)
        {
            if (o.HasValue) return o.Value.ToString();
            else return "0";
        }
        public static int Cof_ToZeroValue(this int? o)
        {
            if (o.HasValue) return o.Value;
            else return 0;
        }
        public static T Cof_Value<T>(this T? o, T DefaultV) where T : struct
        {
            if (o.HasValue) return o.Value;
            else return DefaultV;
        }
        public static short Cof_ToZeroValue(this short? o)
        {
            if (o.HasValue) return o.Value;
            else return 0;
        }
        public static long Cof_ToZeroValue(this long? o)
        {
            if (o.HasValue) return o.Value;
            else return 0;
        }
        public static double Cof_ToZeroValue(this double? o)
        {
            if (o.HasValue) return o.Value;
            else return 0;
        }
        public static string Cof_ToFormatString(this decimal? o, string FormatS)
        {
            if (o.HasValue) return o.Value.ToString(FormatS);
            else return "0";
        }
        public static byte[] Cof_To4Bytes(this long D)
        {
            byte[] Dbytes = new byte[4];
            TxtToolBLL.int2Byte(D, Dbytes, 0, 4, false);
            return Dbytes;
        }
        public static byte[] Cof_To4Bytes(this int D)
        {
            byte[] Dbytes = new byte[4];
            TxtToolBLL.int2Byte(D, Dbytes, 0, 4, false);
            return Dbytes;
        }
        public static byte[] Cof_To2Bytes(this int D)
        {
            byte[] Dbytes = new byte[2];
            TxtToolBLL.int2Byte(D, Dbytes, 0, 2, false);
            return Dbytes;
        }
        public static byte[] Cof_To3Bytes(this int D)
        {
            byte[] Dbytes = new byte[3];
            TxtToolBLL.int2Byte(D, Dbytes, 0, 3, false);
            return Dbytes;
        }

        public static byte[] Cof_To3Bytes(this uint D)
        {
            byte[] Dbytes = new byte[3];
            TxtToolBLL.int2Byte(D, Dbytes, 0, 3, false);
            return Dbytes;
        }
        public static byte[] Cof_To2Bytes(this long D)
        {
            byte[] Dbytes = new byte[2];
            TxtToolBLL.int2Byte(D, Dbytes, 0, 2, false);
            return Dbytes;
        }
        public static bool Cof_ToSafeBool(this bool? o)
        {
            if (o.HasValue) return o.Value;
            else return false;
        }
        /// <summary>
        /// 转换成安全的整形
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static int Cof_ToSafeInt(this string S)
        {
            if (string.IsNullOrEmpty(S)) return 0;
            int Sint = 0;
            if (int.TryParse(S, out Sint)) return Sint;
            return 0;

        }
        public static int? Cof_ToSafeIntF(this string S)
        {
            if (string.IsNullOrEmpty(S)) return null;
            int Sint = 0;
            if (int.TryParse(S, out Sint)) return Sint;
            return null;

        }
        public static uint Cof_ToSafeUInt(this string S)
        {
            if (string.IsNullOrEmpty(S)) return 0;
            uint Sint = 0;
            if (uint.TryParse(S, out Sint)) return Sint;
            return 0;

        }
        /// <summary>
        /// 从BCD Byte 转成整数
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Cof_BCDByteToInt(this byte b)
        {
            return b.ToString("X2").Cof_ToSafeInt();

        }

        public static int Cof_ToSafeInt16(this string S)
        {
            if (string.IsNullOrEmpty(S)) return 0;
            int Sint = 0;
            try
            {
                return Convert.ToInt32(S, 16);
            }
            catch
            { }

            return 0;

        }
        /// <summary>
        /// 将小头模式INT转换成长整形
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long Cof_ToLongX(this byte[] Bs, int index, int length)
        {
            return TxtToolBLL.byte2Int(Bs, index, length, true);
        }
        public static long Cof_ToSafeLong(this string S)
        {
            if (string.IsNullOrEmpty(S)) return 0;
            long Sint = 0;
            if (long.TryParse(S, out Sint)) return Sint;
            return 0;

        }
        public static double Cof_ToSafeDouble(this string S)
        {
            if (string.IsNullOrEmpty(S)) return 0;
            double Sint = 0;
            if (double.TryParse(S, out Sint)) return Sint;
            return 0;

        }
        public static decimal Cof_ToSafeDecimal(this string S)
        {
            if (string.IsNullOrEmpty(S)) return 0;
            decimal Sint = 0;
            if (decimal.TryParse(S, out Sint)) return Sint;
            return 0;

        }
        /// <summary>
        /// 将字符串转换成整形后 放到一个byte中
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static byte Cof_ToSafeByte(this string S)
        {
            return Convert.ToByte(S.Cof_ToSafeInt());

        }
        #endregion

        #region Json

        /// <summary>
        /// 将对象转换为Json字符串
        /// datetime  格式   yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Cof_ToJsonString(this object o)
        {
            if (o == null) return "";
            Newtonsoft.Json.Converters.IsoDateTimeConverter isoFormat = new IsoDateTimeConverter();
            isoFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            return Newtonsoft.Json.JsonConvert.SerializeObject(o, isoFormat);
        }
        public static string Cof_ToJsonString2(this object o)
        {
            Newtonsoft.Json.Converters.IsoDateTimeConverter isoFormat = new IsoDateTimeConverter();
            isoFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";          
            return Newtonsoft.Json.JsonConvert.SerializeObject(o, isoFormat);
        }
        /// <summary>
        /// 将对象转换为Json字符串
        /// datetime  格式   yyyyMMddHHmmss
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Cof_ToJsonStringMin(this object o)
        {
            Newtonsoft.Json.Converters.IsoDateTimeConverter isoFormat = new IsoDateTimeConverter();
            isoFormat.DateTimeFormat = "yyyyMMddHHmmss";
            return Newtonsoft.Json.JsonConvert.SerializeObject(o, isoFormat);
        }

        /// <summary>
        /// 转换Json字符串获得指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="JsonString"></param>
        /// <returns></returns>
        public static T Cof_ObjectFromJsonString<T>(this string JsonString) where T : new()
        {
            if (string.IsNullOrEmpty(JsonString)) return new T();
            try
            {
                //Newtonsoft.Json.Converters.IsoDateTimeConverter isoFormat = new IsoDateTimeConverter();
                //isoFormat.DateTimeFormat = "{yyyy-MM-dd HH:mm:ss}";
                //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonString, isoFormat);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonString);
            }
            catch (System.Exception e)
            {
                TextLogerBll.LogWriter("Cof_FromJsonString", e);
                string msg = e.Message;
            }
            return new T();
        }
        public static T Cof_ObjectFromJsonStringNull<T>(this string JsonString) where T : new()
        {
            if (string.IsNullOrEmpty(JsonString)) return default;
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonString);
            }
            catch (System.Exception e)
            {
                TextLogerBll.LogWriter("Cof_FromJsonString", e);
                string msg = e.Message;
            }
            return default;
        }
        public static T Cof_ObjectFromJsonStringMin<T>(this string JsonString) where T : new()
        {
            if (string.IsNullOrEmpty(JsonString)) return new T();
            try
            {
                Newtonsoft.Json.Converters.IsoDateTimeConverter isoFormat = new IsoDateTimeConverter();
                isoFormat.DateTimeFormat = "yyyyMMddHHmmss";
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonString, isoFormat);
                //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonString);
            }
            catch (System.Exception e)
            {
                TextLogerBll.LogWriter("Cof_FromJsonString", e);
                string msg = e.Message;
            }
            return new T();
        }
        #endregion

        #region  数组

        /// <summary>
        /// 对队列按照时间属性进行分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Tlist"></param>
        /// <param name="Seconds"></param>
        /// <param name="getTime"></param>
        /// <returns></returns>
        public static List<List<T>> Cof_SplitByTimeDis<T>(this List<T> Tlist, int Seconds, Func<T, DateTime> getTime)
        {
            List<List<T>> result = new List<List<T>>();
            if (!Tlist.Cof_CheckAvailable()) return result;
            List<T> buf = new List<T>();
            for (int i = 0; i < Tlist.Count; i++)
            {

                if (i == 0) { buf.Add(Tlist[0]); }
                else
                {
                    T Lastobj = Tlist[i - 1];
                    T obj = Tlist[i];
                    if (getTime(obj) > getTime(Lastobj).AddSeconds(Seconds))
                    {
                        result.Add(buf);
                        buf = new List<T>();
                    }

                    buf.Add(obj);
                    if (i == Tlist.Count - 1)
                    {
                        result.Add(buf);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// 按照Bool变量分割
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Tlist"></param>
        /// <param name="Boolv"></param>
        /// <param name="getBool"></param>
        /// <returns></returns>
        public static List<List<T>> Cof_SplitByBool<T>(this List<T> Tlist, bool Boolv, Func<T, bool> getBool)
        {
            List<List<T>> result = new List<List<T>>();
            if (!Tlist.Cof_CheckAvailable()) return result;
            List<T> buf = new List<T>();
            for (int i = 0; i < Tlist.Count; i++)
            {
                T obj = Tlist[i];
                if (getBool(obj) == Boolv)
                {
                    buf.Add(obj);
                }
                else
                {
                    if (buf.Count > 0)
                    {
                        result.Add(buf);
                        buf = new List<T>();
                    }
                }
            }
            //最后一组
            if (buf.Count > 0)
            {
                result.Add(buf);
            }
            return result;
        }


        /// <summary>
        /// 按照规定的数量进行分组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Tlist"></param>
        /// <param name="Len"></param>
        /// <param name="Dealer"></param>
        /// <returns></returns>
        public static List<List<T>> Cof_SplitLen<T>(this List<T> Tlist, int Len)
        {
            List<List<T>> result = new List<List<T>>();
            if (!Tlist.Cof_CheckAvailable()) return result;
            List<T> buf = new List<T>();
            foreach (var item in Tlist)
            {
                buf.Add(item);
                if (buf.Count >= Len)
                {
                    result.Add(buf.Cof_clone());
                    buf = new List<T>();
                }
            }
            if (buf.Count > 0)
            {
                result.Add(buf);
                // buf = new List<T>();
            }
            return result;
        }

        /// <summary>
        /// 根据分割符连接成标准数组数据
        /// </summary>
        /// <typeparam name="TSource">类型</typeparam>
        /// <param name="Tlist">数据源</param>
        /// <param name="SplitChar">分隔符</param>
        /// <returns></returns>
        public static string Cof_ToString_WithSplit<TSource>(this IEnumerable<TSource> Tlist, char SplitChar)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = Tlist.Count(); i < len; i++)
            {
                sb.Append(Tlist.ElementAt(i).ToString()).Append(SplitChar);
            }
            return sb.ToString().TrimEnd(SplitChar);
        }
        public static string Cof_ToString_WithSplit<TSource>(this IEnumerable<TSource> Tlist, string  SplitString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = Tlist.Count(); i < len; i++)
            {
                sb.Append(Tlist.ElementAt(i).ToString());
                if (i!= len-1) sb.Append(SplitString);
                
            }
            return sb.ToString();
        }
        public static string Cof_ToString_Connect<TSource>(this IEnumerable<TSource> Tlist)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0, len = Tlist.Count(); i < len; i++)
            {
                sb.Append(Tlist.ElementAt(i).ToString());
            }
            return sb.ToString();
        }
        /// <summary>
        /// 根据分割符","连接成标准数组数据
        /// </summary>
        /// <typeparam name="TSource">类型</typeparam>
        /// <param name="Tlist">数据源</param>
        /// <returns></returns>
        public static string Cof_ToString_WithSplit<TSource>(this IEnumerable<TSource> Tlist)
        {
            return Cof_ToString_WithSplit(Tlist, ',');
        }

        public static IEnumerable<TSource> Cof_AddRangeNotFilter<TSource>(this IEnumerable<TSource> Tlist, IEnumerable<TSource> TlistSecond)
        {
            if (!TlistSecond.Cof_CheckAvailable()) return Tlist;
            return Tlist.Concat(TlistSecond);
           
        }

        public static IList<TSource> Cof_AddRange<TSource>(this IList<TSource> Tlist, IList<TSource> TlistSecond)
        {
            if (!TlistSecond.Cof_CheckAvailable()) return Tlist;
            foreach (TSource tv in TlistSecond)
            {
                if (!Tlist.Contains(tv)) Tlist.Add(tv);
            }
            return Tlist;
        }

        /// <summary>
        /// 判断数据不为null且项数大于0
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="Tlist"></param>
        /// <returns></returns>
        public static bool Cof_CheckAvailable<TSource>(this IEnumerable<TSource> Tlist)
        {
            return Tlist != null && Tlist.Count() > 0;
        }

        public static void Cof_Clear<TSource>(this List<TSource> Tlist)
        {
            if (Tlist != null && Tlist.Count() > 0) Tlist.Clear();
        }
        public static List<TSource> Cof_clone<TSource>(this List<TSource> Tlist)
        {
            if (Tlist == null || Tlist.Count() == 0) return null;
            List<TSource> a = new List<TSource>();
            a.AddRange(Tlist);
            return a;
            // Cof_ApendArray(Tlist);
            // return Tlist != null && Tlist.Count() > 0;
        }

        /// <summary>
        /// 列表Count 安全 null 返回0
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="Tlist"></param>
        /// <returns></returns>
        public static int Cof_Count<TSource>(this IEnumerable<TSource> Tlist)
        {
            if (Tlist != null) return Tlist.Count();
            return 0;
        }
        /// <summary>
        /// 对对象组进行分组处理
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="Tlist"></param>
        /// <param name="Period"></param>
        /// <param name="DealFun"></param>
        public static void Cof_SplitForPeriod<TSource>(this IEnumerable<TSource> Tlist, int Period, Action<List<TSource>> DealFun)
        {
            if (!Tlist.Cof_CheckAvailable()) return;
            int TlistLength = Tlist.Count();
            List<TSource> Lbuf = new List<TSource>();
            for (int i = 0; i < TlistLength; i++)
            {
                Lbuf.Add(Tlist.ElementAt(i));
                if (Lbuf.Count >= Period)
                {
                    DealFun(Lbuf);
                    Lbuf = new List<TSource>();

                }
            }
            if (Lbuf.Cof_CheckAvailable()) DealFun(Lbuf);
        }
        #endregion

        #region  DateTime


        /// <summary>
        /// 计算两个泛型时间之间的秒差
        /// </summary>
        /// <param name="D"></param>
        /// <param name="D2"></param>
        /// <returns></returns>
        public static int Cof_ToDisSecond(this DateTime? D, DateTime? D2)
        {
            if (D.HasValue && D2.HasValue)
            {
                return Convert.ToInt32(Math.Abs((D.Value - D2.Value).TotalSeconds));
            }

            return 0;
        }
        /// <summary>
        /// 当前时间与1970-1-1之间时间差的秒数 不考虑时区
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static long Cof_ToUTCSecond(this DateTime D)
        {
            if (D == DateTime.MinValue) return -1;
            if (D == DateTime.MaxValue) return -1;
            return Convert.ToInt64((D - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
        }
        public static int Cof_ToUTCSecondINT(this DateTime D)
        {
            if (D == DateTime.MinValue) return -1;
            if (D == DateTime.MaxValue) return -1;
            return Convert.ToInt32((D - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
        }
        public static string ConvertDateTimeToInt(this DateTime time)
        {
            string timeStamp = TxtToolBLL.GetUTCLongTicksByTime(time).ToString();
            timeStamp = timeStamp.Substring(0, timeStamp.Length - 7);
            return timeStamp;
        }
        /// <summary>
        /// 获取当月天数
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static int Cof_GetMonthDays(this DateTime D)
        {

            switch (D.Month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return 31;
                case 4:
                case 6:
                case 9:
                case 11:
                    return 30;
                case 2:
                    return D.Cof_Date_LastDay().Day;
                default:
                    return 30;
            }
        }
        public static int Cof_ToyyyyMMdd_Int(this DateTime D)
        {
            return int.Parse(D.Cof_ToyyyyMMdd());
        }

        /// <summary>
        /// 返回yyyyMM Int值
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static int Cof_ToyyyyMM_Int(this DateTime D)
        {

            return int.Parse(D.Cof_ToyyyyMM());
        }
        /// <summary>
        /// 当天零时到当前时间的分钟数 H*60 +m
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static int Cof_ToHHmm_Int(this DateTime D)
        {
            return D.Hour * 60 + D.Minute;
        }
        /// <summary>
        /// 如果出错则返回 DateTime.MinValue
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static DateTime Cof_ToSafeDateTime2(this string S)
        {
            if (string.IsNullOrEmpty(S)) return DateTime.MinValue;
            DateTime SDateTime = DateTime.Now;
            if (DateTime.TryParse(S, out SDateTime)) return SDateTime;
            return DateTime.MinValue;
        }
        ///// <summary>
        ///// 当前时间与1970-1-1之间时间差的秒数 不考虑时区
        ///// </summary>
        ///// <param name="D"></param>
        ///// <returns></returns>
        //public static long Cof_ToUTCSecond(this DateTime D)
        //{
        //    if (D == DateTime.MinValue) return -1;
        //    if (D == DateTime.MaxValue) return -1;
        //    return Convert.ToInt64((D - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
        //}
        /// <summary>
        /// 转换成安全的时间数据
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static DateTime? Cof_ToSafeDateTime(this string S)
        {
            if (string.IsNullOrEmpty(S)) return null;
            DateTime SDateTime = DateTime.Now;
            if (DateTime.TryParse(S, out SDateTime)) return SDateTime;
            return null;

        }
        public static DateTime Cof_Date_FirstDay(this DateTime D)
        {
            return D.AddDays((-1) * (D.Day - 1));
        }
        public static DateTime Cof_Date_LastDay(this DateTime D)
        {
            return D.AddMonths(1).AddDays((-1) * (D.Day)).Date;
        }
        /// <summary>
        /// 转换为yyyy-MM-dd HH:mm:ss 字符串
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static string Cof_ToString(this DateTime D)
        {

            return D.ToString("yyyy-MM-dd HH:mm:ss");
        }
        public static string Cof_ToString2(this DateTime D)
        {
            return D.ToString("yyyyMMddHHmmss");
        }
        public static string Cof_ToStringT(this DateTime D)
        {

            return D.ToString("yyyy-MM-ddTHH:mm:ss");
        }
        /// <summary>
        /// 转换为yyyyMMddHHmmss字符串
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static string Cof_ToStringMin(this DateTime D)
        {
            return D.ToString("yyyyMMddHHmmss");
        }
        public static string Cof_ToStringMM(this DateTime D)
        {

            return D.ToString("yyyy-MM-dd HH:mm");
        }
        /// <summary>
        /// 转换为yyyy-MM-dd 字符串
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static string Cof_ToDayString(this DateTime D)
        {

            return D.ToString("yyyy-MM-dd");
        }
        /// <summary>
        /// BCD 7 字节
        /// </summary>
        /// <param name="tt"></param>
        /// <returns></returns>
        public static byte[] Cof_To7byte(this DateTime tt)
        {
            //if (tt == null) return null;
            byte[] b = new byte[7];
            b[0] = (byte)Convert.ToInt32(tt.Year.ToString("0000").Substring(0, 2), 16);
            b[1] = (byte)Convert.ToInt32(tt.Year.ToString("0000").Substring(2, 2), 16);
            b[2] = (byte)Convert.ToInt32(tt.Month.ToString("00"), 16);
            b[3] = (byte)Convert.ToInt32(tt.Day.ToString("00"), 16);
            b[4] = (byte)Convert.ToInt32(tt.Hour.ToString("00"), 16);
            b[5] = (byte)Convert.ToInt32(tt.Minute.ToString("00"), 16);
            b[6] = (byte)Convert.ToInt32(tt.Second.ToString("00"), 16);
            return b;
        }
        /// <summary>
        /// BCD 6 字节数据
        /// </summary>
        /// <param name="tt"></param>
        /// <returns></returns>
        public static byte[] Cof_To6byte(this DateTime tt)
        {
            //if (tt == null) return null;
            byte[] b = new byte[6];
            b[0] = (byte)Convert.ToInt32(tt.Year.ToString("0000").Substring(2, 2), 16);
            b[1] = (byte)Convert.ToInt32(tt.Month.ToString("00"), 16);
            b[2] = (byte)Convert.ToInt32(tt.Day.ToString("00"), 16);
            b[3] = (byte)Convert.ToInt32(tt.Hour.ToString("00"), 16);
            b[4] = (byte)Convert.ToInt32(tt.Minute.ToString("00"), 16);
            b[5] = (byte)Convert.ToInt32(tt.Second.ToString("00"), 16);
            return b;
        }
        public static string Cof_ToyyyyMM(this DateTime D)
        {

            return D.ToString("yyyyMM");
        }
        public static string Cof_ToyyyyMMdd(this DateTime D)
        {

            return D.ToString("yyyyMMdd");
        }




        /// <summary>
        /// 从当前时区系统时间转换为Unix时间戳（Unix timestamp）
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static long Cof_ToUnixLong(this DateTime D)
        {
            return TxtToolBLL.GetUTCLongMsByTime(D);
        }
        /// <summary>
        /// Unix时间戳（Unix timestamp） 转换为当前时区的系统时间
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static DateTime Cof_ToUnixTime(this long D)
        {
            return TxtToolBLL.GetUTCtimeByLongMs(D);
        }
        /// <summary>
        /// 将时间转换为与1970-1-1之间的秒差距 不考虑时区
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static long Cof_ToLong(this DateTime D)
        {
            return TxtToolBLL.GetTimeLongValueSecond(D);
        }
        /// <summary>
        /// 将时间转换为与1970-1-1之间的毫秒差距 不考虑时区
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static long Cof_ToLongMs(this DateTime D)
        {
            return TxtToolBLL.GetTimeLongValueMilliseconds(D);
        }
        /// <summary>
        /// 将时间转换为与1970-1-1 0:0:0 相距秒数 不考虑时区
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static int Cof_ToLongINT(this DateTime D)
        {
            return TxtToolBLL.GetTimeLongValueSecondINT(D);
        }
        /// <summary>
        /// 将与1970-1-1之间的秒查转换为时间 不考虑时区
        /// </summary>
        /// <param name="V"></param>
        /// <returns></returns>
        public static DateTime Cof_UTCSecondToDateTime(this long V)
        {
            if (V < 0) return DateTime.MinValue;
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(V);
        }
        /// <summary>
        /// 将Long 转换为UTC时间 不考虑时区
        /// </summary>
        /// <param name="V"></param>
        /// <returns></returns>
        public static DateTime Cof_UTCMilliSecondToDateTime(this long V)
        {
            if (V < 0) return DateTime.MinValue;
            return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(V);
        }
        /// <summary>
        /// 将与1970-1-1之间的秒查转换为时间 不考虑时区
        /// </summary>
        /// <param name="V"></param>
        /// <returns></returns>
        public static DateTime Cof_UTCSecondToDateTime(this int V)
        {
            if (V < 0) return DateTime.MinValue;
            return new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(V);
        }
        /// <summary>
        /// 将与1970-1-1之间的秒查转换为时间 不考虑时区
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static long Cof_ToUTCLongSecond(this DateTime D)
        {
            return TxtToolBLL.GetTimeLongValueSecond(D);
        }
        public static byte[] Cof_ToUTCBytes(this DateTime D)
        {
            byte[] UTCbytes = new byte[8];
            TxtToolBLL.int2Byte(TxtToolBLL.GetUTCLongMsByTime(D), UTCbytes, 0, 8, false);
            return UTCbytes;
        }
        public static byte[] Cof_ToUTCBytesSecond(this DateTime D)
        {
            byte[] UTCbytes = new byte[8];
            TxtToolBLL.int2Byte(TxtToolBLL.GetUTCtimeValueSeconds(D), UTCbytes, 0, 8, false);
            return UTCbytes;
        }

        /// <summary>
        /// 转换为yyyy-MM-dd HH:mm:ss 字符串
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static string Cof_ToString(this DateTime? D)
        {
            if (D.HasValue) return D.Value.ToString("yyyy-MM-dd HH:mm:ss");
            return "";
        }
        /// <summary>
        /// 转换为 yyyyMMddHHmmss字符串
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static string Cof_ToString2(this DateTime? D)
        {
            if (D.HasValue) return D.Value.ToString("yyyyMMddHHmmss");
            return "";
        }
        /// <summary>
        /// 转换为yyyy-MM-dd 字符串
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static string Cof_ToDayString(this DateTime? D)
        {
            if (D.HasValue) return D.Value.ToString("yyyy-MM-dd");
            return "";
        }
        /// <summary>
        /// 转换为yyyyMMdd 字符串
        /// </summary>
        /// <param name="D"></param>
        /// <returns></returns>
        public static string Cof_ToDayString2(this DateTime? D)
        {
            if (D.HasValue) return D.Value.ToString("yyyyMMdd");
            return "";
        }
        public static byte[] Cof_ToBCD6Bytes(this DateTime D)
        {
            return TxtToolBLL.BCDbyteFromDateTime(D);
        }
        #endregion

        #region  String 
        /// <summary>
        /// 按照InfluxDB数据库的要求，不能为空 不能包含空格
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Cof_ToInfluxString(this string s)
        {
            return !string.IsNullOrEmpty(s) ? s.Replace(" ", "").Replace("\\", "") : "null";
        }
        public static string Cof_ToInfluxStringTag(this string s)
        {
            return !string.IsNullOrEmpty(s) ? s.Replace(" ", "").Replace("\\", "").Replace(",", "") : "null";
        }
        public static byte[] Cof_ToUTF8Bytes(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
        public static string Cof_ToUTF8String(this byte[] s)
        {
            return Encoding.UTF8.GetString(s);
        }
                
        public static string Cof_ToUTF8String(this byte[] s, int index, int count)
        {
            return Encoding.UTF8.GetString(s, index, count);
        }
        public static string Cof_ToBase64(this string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }
        public static string Cof_FromBase64(this string s)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }

        public static string Cof_FromBase64Hex(this string s)
        {
            return Convert.FromBase64String(s).Cof_ToHexStringS();
        }
        /// <summary>
        /// Hex字符串转换成byte数组后转为字符串 过滤不可见字符
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_ToHexString(this string S)
        {
            return TxtToolBLL.GetBytesByHexStringData(S).Cof_ToStringGB2312withFilter();
        }

        public static string Cof_ToStringOrEmpty(this object o)
        {
            if (o == null) return "";
            return o.ToString();

        }

        /// <summary>
        /// 将字段名称变为查询条件
        ///  1= "Equal" 2= "NotEmpty" 3= "ISEmpty" 4= "List" 5= "Min" 6= "Max" 7= "Day" 8= "Mon" 9= "Year" 10= "Columns" 11= "orderby" 12 "NotNull"
        /// </summary>
        /// <param name="s">字段名称</param>
        /// <param name="Type">查询类型</param>
        /// <returns></returns>
        public static string Cof_ToCondition(this string s, int Type)
        {
            string Result = s;
            switch (Type)
            {

                case 1: Result += "Equal"; break;
                case 2: Result += "NotEmpty"; break;
                case 3: Result += "ISEmpty"; break;
                case 4: Result += "List"; break;
                case 5: Result += "Min"; break;
                case 6: Result += "Max"; break;
                case 7: Result += "Day"; break;
                case 8: Result += "Mon"; break;
                case 9: Result += "Year"; break;
                case 10: Result = "Columns"; break;
                case 11: Result = "orderby"; break;
                case 12: Result += "NotNull"; break;
                case 13: Result += "IsNull"; break;



            }

            return Result;
        }
        /// <summary>
        /// 字符串是空或null
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool Cof_IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
        /// <summary>
        /// 不是null  empty 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool Cof_NotNullOrEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
        /// <summary>
        /// 不是null  empty 0
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool Cof_NotNullOrEmptyOrZero(this string s)
        {
            return !string.IsNullOrEmpty(s) && s != "0";
        }
        public static string Cof_SafeTrim(this string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Trim();
        }
        /// <summary>
        /// 字符串数据变成 list
        /// IsNullOrEmpty return null
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static List<string> Cof_ToList(this string S)
        {
            if (S.Cof_IsNullOrEmpty()) return null;
            return new List<string>(S.Split(','));
        }
        /// <summary>
        /// 将整形数据列表 "1,2,3..." 转换为List<int>
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static List<int> Cof_ToIntList(this string S)
        {
            if (S.Cof_IsNullOrEmpty()) return null;
            return S.Split(',').Select(d => d.Cof_ToSafeInt()).ToList();
        }
        public static List<uint> Cof_ToUIntList(this string S)
        {
            if (S.Cof_IsNullOrEmpty()) return null;
            return S.Split(',').Where(d => !string.IsNullOrEmpty(d)).Select(d => uint.Parse(d)).ToList();
        }
        /// <summary>
        /// 字符串数据变成 list
        /// </summary>
        /// <param name="S"></param>
        /// <param name="splitchar"></param>
        /// <returns></returns>
        public static List<string> Cof_ToList(this string S, char splitchar)
        {
            if (S.Cof_IsNullOrEmpty()) return null;
            return new List<string>(S.Split(splitchar));
        }
        /// <summary>
        /// 从左边取指令长度字符串
        /// </summary>
        /// <param name="S"></param>
        /// <param name="Len"></param>
        /// <returns></returns>
        public static string Cof_Safe_Left(this string S, int Len)
        {
            if (string.IsNullOrEmpty(S) || S.Length < Len) return S;
            return S.Substring(0, Len);
        }


        /// <summary>
        /// 只留下字母大小写数字和逗号 句号
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_FilterWord(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            return Regex.Replace(S, "[^[A-Za-z0-9\u4e00-\u9fa5,.]]*", "", RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 只留下汉子字母大小写数字
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_FilterWord3(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            return Regex.Replace(S, "[^[A-Za-z0-9\u4e00-\u9fa5]]*", "", RegexOptions.IgnoreCase);
        }
        /// <summary>
        ///  只留下字母大小写数字和逗号 如果为空返回null
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_FilterWord_null(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "null";
            string buf= Regex.Replace(S, "[^[A-Za-z0-9\u4e00-\u9fa5,]]*", "", RegexOptions.IgnoreCase);
            if (!string.IsNullOrEmpty(buf)) return buf;
            return "null";
        }
        /// <summary>
        /// 过滤不可见字符0x0-0x1F 0x20(空格) 0x7F（Del） 
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_FilterUnVisibleSpace(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            return Regex.Replace(S, @"[\u0000-\u001F\u0020\u007F\u00FF]", "");
        }
        public static string Cof_FilterUnVisible(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            return Regex.Replace(S, @"[\u0000-\u001F\u00FF]", "");
        }
        /// <summary>
        /// 过滤不可见字符0x0-0x1F 0x20(空格)  0x7F（Del） \u003a(:) \u0022(") \u007b({) \u007d (})
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_FilterUnVisibleForJson(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            return Regex.Replace(S, @"[\u0000-\u001F\u00FF\u007F\u0020\u003A\u0022\u007B\u007D]", "");
        }

        /// <summary>
        /// 只留下字母大小写数字中文
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_FilterWord4(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            return Regex.Replace(S, "[^[A-Za-z0-9\u4e00-\u9fa5]]*", "", RegexOptions.IgnoreCase);
        }

       
        /// <summary>
        /// 只留下字母大小写数字中文和 逗号 冒号 减号
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static string Cof_FilterWord2(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            return Regex.Replace(S, "[^[A-Za-z0-9\u4e00-\u9fa5,-:.]]*", "", RegexOptions.IgnoreCase);
        }

        public static string Cof_FilterUnVisibleSpaceNew(this string S)
        {
            if (string.IsNullOrEmpty(S)) return "";
            byte[] Sb = S.Cof_ToBytes().Where(d => d > 0x1f && d != 0x20 && d != 0x7f && d != 0xff).ToArray();
            if (!Sb.Cof_CheckAvailable()) return "";
            return Sb.Cof_ToStringGB2312();
        }
        /// <summary>
        /// 转换成GB2312Bytes
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static byte[] Cof_ToBytes(this string S)
        {
            if (string.IsNullOrEmpty(S)) return null;
            return System.Text.Encoding.Default.GetBytes(S);
        }
        public static byte[] Cof_ToBytesUtf8(this string S)
        {
            if (string.IsNullOrEmpty(S)) return null;
            return System.Text.Encoding.UTF8.GetBytes(S);
        }
        public static byte[] Cof_To2BytesX(this int D)
        {
            byte[] Dbytes = new byte[2];
            TxtToolBLL.int2Byte(D, Dbytes, 0, 2, true);
            return Dbytes;
        }
        /// <summary>
        /// 变成固定长度字节数组 不够的用0x0补足
        /// </summary>
        /// <param name="S"></param>
        /// <param name="LengthV"></param>
        /// <returns></returns>
        public static byte[] Cof_ToBytes_Length(this string S, int LengthV)
        {

            if (string.IsNullOrEmpty(S)) return new byte[LengthV];
            byte[] SB = S.Cof_ToBytes();
            byte[] SB2 = new byte[LengthV];
            SB.Cof_CopyTo(SB2, 0, 0, ((SB.Length >= SB2.Length) ? SB2.Length : SB.Length));
            return SB2;
        }
        /// <summary>
        /// Hex字符串转换成byte数组
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static byte[] Cof_ToHexBytes(this string S)
        {
            return TxtToolBLL.GetBytesFromHexString(S);
        }
        #endregion

        #region  Bytes
        public static void Cof_CopyToAll(this byte[] b, byte[] TargrtB, int TargetStart)
        {
            //if (TargrtB.Length < TargetStart + Length) return;
            //if (b.Length < SourceStart + Length) return;

            System.Array.Copy(b, 0, TargrtB, TargetStart, b.Length);

        }
        /// <summary>
        /// 转换成16进制字符串数据
        /// </summary>
        /// <param name="Bs"></param>
        /// <returns></returns>
        public static string Cof_ToHexStringData(this byte[] Bs)
        {
            return TxtToolBLL.GetBytesHexStrD(Bs);
        }
        /// <summary>
        /// 转变为GB2312字符串
        /// </summary>
        /// <param name="Bs"></param>
        /// <returns></returns>
        public static string Cof_ToStringGB2312(this byte[] Bs)
        {
            return System.Text.Encoding.Default.GetString(Bs);
        }
        public static string Cof_ToStringGB2312(this byte[] Bs, int index, int length)
        {
            return System.Text.Encoding.Default.GetString(Bs, index, length);
        }

        public static void Cof_CopyTo(this byte[] b, byte[] TargrtB, int SourceStart, int TargetStart, int Length)
        {
            //if (TargrtB.Length < TargetStart + Length) return;
            //if (b.Length < SourceStart + Length) return;

            System.Array.Copy(b, SourceStart, TargrtB, TargetStart, Length);

        }

        public static void Cof_CopyToALL(this byte[] b, byte[] TargrtB, int SourceStart, int TargetStart)
        {
            //if (TargrtB.Length < TargetStart + Length) return;
            //if (b.Length < SourceStart + Length) return;

            System.Array.Copy(b, SourceStart, TargrtB, TargetStart, b.Length);

        }
        /// <summary>
        /// 截取数据 
        /// b.Length <= SourceStart=null
        /// b.Length >= SourceStart + LengthV= byte[LengthV];
        /// b.Length <  SourceStart + LengthV=byte[b.Length - SourceStart];
        /// </summary>
        /// <param name="b"></param>
        /// <param name="SourceStart"></param>
        /// <param name="LengthV"></param>
        /// <returns></returns>
        public static byte[] Cof_FetchData(this byte[] b, int SourceStart, int LengthV)
        {
            if (b.Length <= SourceStart) return null;
            if (b.Length >= SourceStart + LengthV)
            {
                byte[] BB = new byte[LengthV];
                b.Cof_CopyTo(BB, SourceStart, 0, BB.Length);
                return BB;
            }
            else
            {
                byte[] BB = new byte[b.Length - SourceStart];
                b.Cof_CopyTo(BB, SourceStart, 0, BB.Length);
                return BB;
            }


        }
        /// <summary>
        /// 指定数据求和
        /// </summary>
        /// <param name="b"></param>
        /// <param name="SourceStart"></param>
        /// <param name="LengthV"></param>
        /// <returns></returns>
        public static long Cof_SumValue(this byte[] b, int SourceStart, int LengthV)
        {
            if (b.Length < SourceStart + LengthV) return 0;
            long result = 0;
            for (int i = 0; i < LengthV; i++)
            {
                result += b[SourceStart + i];
            }
            return result;
        }
        /// <summary>
        /// 转变成Hex字符串
        /// </summary>
        /// <param name="b"></param>
        /// <param name="SourceStart"></param>
        /// <param name="LengthV"></param>
        /// <returns></returns>
        public static string Cof_FetchDataToHexString(this byte[] b, int SourceStart, int LengthV)
        {
            byte[] BB = b.Cof_FetchData(SourceStart, LengthV);
            return TxtToolBLL.GetBytesHexStrD(BB);
        }
        /// <summary>
        /// 转变成16进制字符串 不带空格
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Cof_ToHexString(this byte[] b)
        {
            return TxtToolBLL.GetBytesHexStrD(b);
        }
        /// <summary>
        /// 转变成16进制字符串 带空格
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Cof_ToHexStringS(this byte[] b)
        {
            return TxtToolBLL.GetBytesHexStr(b);
        }
        /// <summary>
        /// 转换成字符串 过滤不可见字符
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Cof_ToStringGB2312withFilter(this byte[] Bs, int index, int length)
        {
            return System.Text.Encoding.Default.GetString(Bs, index, length).Cof_FilterUnVisibleSpace();
        }
        public static string Cof_ToStringGB2312withFilter(this byte[] Bs)
        {
            return System.Text.Encoding.Default.GetString(Bs, 0, Bs.Length).Cof_FilterUnVisibleSpace();

        }
        public static string Cof_ToStringUTF8withFilter(this byte[] Bs)
        {
            return System.Text.Encoding.UTF8.GetString(Bs, 0, Bs.Length).Cof_FilterUnVisibleSpace();

        }
        /// <summary>
        /// 将大头模式INT转换成长整形
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long Cof_ToLong(this byte[] Bs, int index, int length)
        {
            return TxtToolBLL.byte2Int(Bs, index, length, false);
        }
        /// <summary>
        /// 将大头模式数据转换成长整形
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ulong Cof_ToULongF(this byte[] Bs, int index, int length)
        {
            return TxtToolBLL.byte2Ulong(Bs, index, length, false);
        }
        /// <summary>
        /// 将小头模式数据转换成长整形
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static ulong Cof_ToULong(this byte[] Bs, int index, int length)
        {
            return TxtToolBLL.byte2Ulong(Bs, index, length, true);
        }
        /// <summary>
        /// 将数据转换成Long 再转换成UTC DateTime
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="bigOrsmall"></param>
        /// <returns></returns>
        public static DateTime Cof_ToLongUTC(this byte[] Bs, int index, int length, bool bigOrsmall)
        {
            return TxtToolBLL.GetUTCtimeValueByLong(TxtToolBLL.byte2Int(Bs, index, length, bigOrsmall));
        }
        public static DateTime Cof_ToLongUTCLocal(this byte[] Bs, int index, int length, bool bigOrsmall)
        {
            return TxtToolBLL.GetUTCtimeByLongMs(TxtToolBLL.byte2Int(Bs, index, length, bigOrsmall));
        }
        public static DateTime Cof_ToLongUTCSecond(this byte[] Bs, int index, int length, bool bigOrsmall)
        {
            return TxtToolBLL.GetTimeByLongSecond(TxtToolBLL.byte2Int(Bs, index, length, bigOrsmall));
        }
        /// <summary>
        /// 将数据转换成长整形
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="bigOrsmall"></param>
        /// <returns></returns>
        public static long Cof_ToLong(this byte[] Bs, int index, int length, bool bigOrsmall)
        {
            return TxtToolBLL.byte2Int(Bs, index, length, bigOrsmall);
        }

        public static int Cof_ToInt(this byte[] Bs, int index, int length)
        {
            long ii = TxtToolBLL.byte2Int(Bs, index, length, false);
            if (ii >= int.MaxValue) return 0;
            return Convert.ToInt32(ii);
        }
        /// <summary>
        /// 将数据转换成长整形
        /// </summary>
        /// <param name="Bs"></param>
        /// <param name="index"></param>
        /// <param name="length"></param>
        /// <param name="bigOrsmall"></param>
        /// <returns></returns>
        public static int Cof_ToInt(this byte[] Bs, int index, int length, bool bigOrsmall)
        {
            long ii = TxtToolBLL.byte2Int(Bs, index, length, bigOrsmall);
            if (ii >= int.MaxValue) return 0;
            return Convert.ToInt32(ii);
        }
        /// <summary>
        /// 将字节数组 列表连接起来
        /// </summary>
        /// <param name="Tlist"></param>
        /// <returns></returns>
        public static byte[] Cof_Join(this List<byte[]> Tlist)
        {
            if (!Tlist.Cof_CheckAvailable()) return null;
            List<byte> DataAll = new List<byte>();
            for (int i = 0; i < Tlist.Count; i++)
            {
                for (int k = 0; k < Tlist[i].Length; k++) DataAll.Add(Tlist[i][k]);
            }
            return DataAll.ToArray();
        }
        /// <summary>
        /// 添加Int值单byte
        /// </summary>
        /// <param name="Tlist"></param>
        /// <param name="ByteValue"></param>
        public static void Cof_AddByteByInt(this List<byte[]> Tlist, int ByteValue)
        {
            Tlist.Add(new byte[] { Convert.ToByte(ByteValue) });
        }

        #endregion

        #region 特殊计算
        /// <summary>
        /// 次数计算 I/P +(I%P>0?1:0)
        /// </summary>
        /// <param name="I"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        public static int Cof_ToIntTimes(this int I, int P)
        {
            return I / P + (I % P > 0 ? 1 : 0);
        }
        #endregion

        #region ICaching
        /// <summary>
        /// 从缓存里取数据，如果不存在则执行查询方法，
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="cache">ICaching </param>
        /// <param name="key">键值</param>
        /// <param name="GetFun">查询方法</param>
        /// <param name="timeSpanMin">有效期 单位分钟/param>
        /// <returns></returns>
        public static T Cof_GetICaching<T>(this ICaching cache, string key, Func<T> GetFun, int timeSpanMin) where T : class
        {
            var obj = cache.Get(key);
            obj = GetFun();
            if (obj == null)
            {
                obj = GetFun();
                cache.Set(key, obj, timeSpanMin);
            }
            return obj as T;
        }
        /// <summary>
        /// 异步从缓存里取数据，如果不存在则执行查询方法
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="cache">ICaching </param>
        /// <param name="key">键值</param>
        /// <param name="GetFun">查询方法</param>
        /// <param name="timeSpanMin">有效期 单位分钟/param>
        /// <returns></returns>
        public static async Task<T> Cof_AsyncGetICaching<T>(this ICaching cache, string key, Func<Task<T>> GetFun, int timeSpanMin) where T : class
        {
            var obj = cache.Get(key);
            if (obj == null)
            {
                obj = await GetFun();
                cache.Set(key, obj, timeSpanMin);
            }
            return obj as T;
        }
        #endregion

        #region HttpContext
        /// <summary>
        /// 返回请求上下文
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static async Task Cof_SendResponse(this HttpContext context, System.Net.HttpStatusCode code, string message, string ContentType = "text/html;charset=utf-8")
        {
            context.Response.StatusCode = (int)code;
            context.Response.ContentType = ContentType;
            await context.Response.WriteAsync(message);
        }
        #endregion

        #region Nacos NamingService
        private static readonly HttpClient httpclient = new HttpClient();
        private static string GetServiceUrl(Nacos.V2.INacosNamingService serv, string ServiceName, string Group, string apiurl)
        {
            try
            {
                var instance = serv.SelectOneHealthyInstance(ServiceName, Group).GetAwaiter().GetResult();
                var host = $"{instance.Ip}:{instance.Port}";
                if (instance.Metadata.ContainsKey("endpoint")) host = instance.Metadata["endpoint"];


                var baseUrl = instance.Metadata.TryGetValue("secure", out _)
                   ? $"https://{host}"
                   : $"http://{host}";

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    return "";
                }
                return $"{baseUrl}{apiurl}";
            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("Cof_NaoceGet.Err", ee);
            }
            return "";
        }
        public static async Task<string> Cof_NaoceGet(this Nacos.V2.INacosNamingService serv, string ServiceName, string Group, string apiurl, Dictionary<string, string> Parameters = null)
        {
            try
            {
                var url = GetServiceUrl(serv, ServiceName, Group, apiurl);
                if (string.IsNullOrEmpty(url)) return "";
                if (Parameters.Cof_CheckAvailable())
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var pitem in Parameters)
                    {
                        sb.Append($"{pitem.Key}={pitem.Value}&");
                    }
                    url = $"{url}?{sb.ToString().Trim('&')}";
                }
                httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await httpclient.GetAsync(url);
                return await result.Content.ReadAsStringAsync();

            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("Cof_NaoceGet.Err", ee);
            }
            return "";

        }

        public static async Task<string> Cof_NaocePostForm(this Nacos.V2.INacosNamingService serv, string ServiceName, string Group, string apiurl, Dictionary<string, string> Parameters)
        {
            try
            {
                var url = GetServiceUrl(serv, ServiceName, Group, apiurl);
                if (string.IsNullOrEmpty(url)) return "";

                var content = Parameters.Cof_CheckAvailable() ? new FormUrlEncodedContent(Parameters) : null;
                httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await httpclient.PostAsync(url, content);
                return await result.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();

            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("Cof_NaocePostForm.Err", ee);
            }
            return "";
        }
        public static async Task<string> Cof_NaocePostJson(this Nacos.V2.INacosNamingService serv, string ServiceName, string Group, string apiurl, string jSonData)
        {
            try
            {
                var url = GetServiceUrl(serv, ServiceName, Group, apiurl);
                if (string.IsNullOrEmpty(url)) return "";
                httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await httpclient.PostAsync(url, new StringContent(jSonData, Encoding.UTF8, "application/json"));
                return await result.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();

                //httpClient.BaseAddress = new Uri("https://www.testapi.com");
                //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("Cof_NaocePostJson.Err", ee);
            }
            return "";
        }

        public static async Task<string> Cof_NaocePostFile(this Nacos.V2.INacosNamingService serv, string ServiceName, string Group, string apiurl, Dictionary<string, byte[]> Parameters)
        {
            try
            {
                var url = GetServiceUrl(serv, ServiceName, Group, apiurl);
                if (string.IsNullOrEmpty(url)) return "";

                var content = new MultipartFormDataContent();               
                foreach (var pitem in Parameters)
                {
                    content.Add(new ByteArrayContent(pitem.Value), "files", pitem.Key);
                }
                httpclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = await httpclient.PostAsync(url, content);
                return await result.Content.ReadAsStringAsync();//.GetAwaiter().GetResult();

            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("Cof_NaocePostFile.Err", ee);
            }
            return "";
        }
        #endregion

    }
}
