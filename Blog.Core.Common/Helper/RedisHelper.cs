using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NewLife.Caching;
using NewLife.Caching.Models;
using Blog.Core.Common.Helper;
using Microsoft.Extensions.Configuration;
namespace Blog.Core.Common.Helper
{
    #region 返回值对象
    /// <summary>
    ///  Redis  帮助类
    /// </summary>
    public class RedisHelper
    {

        #region  获得实例 初始化
       
        private static RedisHelper _handler = null;

        public static RedisHelper GetInstance()
        {
            if (_handler == null || _handler.init==false)
            {
                //"Redis": {
                //    "databaseId": "13",
                //    "host": "47.92.123.100",
                //    "port": "8145",
                //    "password": "1qaz@wsx"
                //  },
                IConfiguration configobj = TxtToolBLL.GetConfiguration();
             
                int redisdatabase = configobj["Redis:databaseId"].Cof_ToSafeInt();
                string redishost = configobj["Redis:host"];
                string redisport = configobj["Redis:port"];
                string redispassword = configobj["Redis:password"];

                _handler = new RedisHelper();
                _handler.rds = new NewLife.Caching.FullRedis(string.Format("{0}:{1}", redishost, redisport), redispassword, redisdatabase);
                _handler.init = true;
                if (_handler.rds == null) _handler.init = false;
            }
            return _handler;
        }
        private volatile bool init = false;
        public RedisHelper()
        { }
        public FullRedis rds;
        #endregion

        #region  方法

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="Key">数值</param>
        /// <param name="Data">数据</param>
        /// <param name="expire">过期时间单位秒</param>
        /// <returns></returns>
        public bool Set(string Key, string Data, int expire)
        {
            return rds.Set<string>(Key, Data, expire);
        }
        /// <summary>
        /// 设置对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="Data"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public bool Set<T>(string Key, T Data,int expire)
        {
            return rds.Set<T>(Key, Data, expire);
        }

        /// <summary>
        /// 提取字符串
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string Get(string Key)
        {
            return rds.Get<string>(Key);
        }
        /// <summary>
        /// 提取对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <returns></returns>
        public T Get<T>(string Key)
        {
            return rds.Get<T>(Key);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public int Del(string Key)
        {
            return rds.Remove(Key);
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="Keys"></param>
        /// <returns></returns>
        public int Del(string[] Keys)
        {
            return rds.Remove(Keys);
        }
        /// <summary>
        /// 替换对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Keys"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public T Replace<T>(string Keys,T obj)
        {
            return rds.Replace<T>(Keys, obj);
        }

        /// <summary>
        /// 获得列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <returns></returns>
        public IList<T> Getlist<T>(string Key)
        {
            return rds.GetList<T>(Key);
        }
        /// <summary>
        /// 设置列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="lst"></param>
        /// <param name="expire"></param>
        /// <returns></returns>       
        public int ListAdd<T>(string Key, params T[] Tobj)
        {
            return rds.RPUSH<T>(Key, Tobj);
        }
        /// <summary>
        /// 数据字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Keys"></param>
        /// <returns></returns>
        public IDictionary<string, T> GetDic<T>(string[] Keys)
        {
            if (Keys == null || Keys.Length == 0) return null;          
            return rds.GetAll<T>(Keys);            
        }
        /// <summary>
        /// 根据文件夹名字 获得全部数据
        /// "Tagdata:T*"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Patten">可以用* ?  例如  Tagdata:T*</param>
        /// <returns></returns>
        public IDictionary<string, T> SearchDic<T>(string Patten)
        {
            //string[] keys = rds.Search(Patten);
            try
            {
                //通过SearchModel 实现了分页查询
                var schm = new SearchModel { Pattern = Patten , Position=0, Count=100000};
                return GetDic<T>(rds.Search(schm).ToArray());
            }
            catch (System.Exception ee)
            {
                TextLogerBll.LogWriter("RedisHelper.SearchDic", ee);
                return null;
            }
        }

        public IDictionary<string, T> SearchList<T>(string Patten)
        {
            //string[] keys = rds.Search(Patten);
            try
            {
                //通过SearchModel 实现了分页查询
                var schm = new SearchModel { Pattern = Patten, Position = 0, Count = 100000 };
                
                return GetDic<T>(rds.Search(schm).ToArray());
            }
            catch (System.Exception ee)
            {
                TextLogerBll.LogWriter("RedisHelper.SearchDic", ee);
                return null;
            }
        }
        /// <summary>
        /// 设置字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dic"></param>
        /// <param name="expire"></param>
        public void SetDic<T>(Dictionary<string,T> dic,  int expire=-1)
        {
            Dictionary<string, T> dicbuf = new Dictionary<string, T>();
            foreach (var ditem in dic)
            {
                dicbuf.Add(ditem.Key, ditem.Value);
                if (dicbuf.Count > 1000)
                {
                    rds.SetAll<T>(dicbuf, expire);
                    dicbuf = new Dictionary<string, T>();
                }
            }
            if (dicbuf.Count > 0)
            {
                rds.SetAll<T>(dicbuf, expire);
                dicbuf =null;
            }

        }
        /// <summary>
        /// 原子操作 增加
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="addvalue"></param>
        /// <returns></returns>
        public long Increment(string Key, long addvalue)
        {
            return rds.Increment(Key, addvalue);
        }
        /// <summary>
        /// 原子操作  减少
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Delvalue"></param>
        /// <returns></returns>
        public long Decrement(string Key, long Delvalue)
        {
            return rds.Decrement(Key, Delvalue);
        }


        /// <summary>
        /// 向一个队里中推送一个消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="Msg"></param>
        /// <returns></returns>
        public int PushToList<T>(string Key, T Msg)
        {
            if (rds == null) return 0;
            return rds.RPUSH<T>(Key, Msg);
        }
        /// <summary>
        /// 从列表中取得数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <returns></returns>
        public T POPFromList<T>(string Key)
        {
            if (rds == null) return default;
            return rds.LPOP<T>(Key);
        }
        #endregion
    }
    #endregion
}
