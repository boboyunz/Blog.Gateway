using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Blog.Core.Common.Helper
{
    /// <summary>
    /// 通用的历史数据缓存
    /// 用于为数据对象提供一个线程安全的数据缓冲池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HisDatacatch<T> : IDisposable// where T : new()
    {
        //public CarhisDataDs ds =null ;

        public DateTime HisDataTime = DateTime.Now;

        public ConcurrentQueue<T> Hisdatalist1;
        public ConcurrentQueue<T> Hisdatalist2;
        /// <summary>
        /// 方向开关 true-指向Hisdatalist1 ；false-指向Hisdatalist2  
        /// </summary>
        public volatile bool HisdataPoint = true;
        
             
        public HisDatacatch()
        {

            HisDataTime = DateTime.Now;
            //ds = new CarhisDataDs();
            Hisdatalist1 = new ConcurrentQueue<T>();
            Hisdatalist2 = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// 添加单个历史数据对象
        /// </summary>
        /// <param name="data"></param>
        public void Add(T data)
        {
            if (HisdataPoint)
            {
                Hisdatalist1.Enqueue(data);
            }
            else
            {
                Hisdatalist2.Enqueue(data);
            }

        }

        /// <summary>
        /// 添加多个个历史数据对象
        /// </summary>
        /// <param name="data"></param>
        public void Add(List<T> datas)
        {
            if (HisdataPoint)
            {
                datas.ForEach(t => Hisdatalist1.Enqueue(t));
                
            }
            else
            {
                datas.ForEach(t => Hisdatalist2.Enqueue(t));
                
            }
            
        }
        /// <summary>
        /// 改变缓存方向
        /// </summary>
        public void ChangeCatch()
        {
            HisdataPoint = !HisdataPoint;
            // Interlocked.Exchange(ref HisDataTime, DateTime.Now);
            HisDataTime = DateTime.Now;
        }

        /// <summary>
        /// 返回历史数据辅助缓存 将要存放到数据库内
        /// </summary>
        public ConcurrentQueue<T> ToSaveCatch
        {
            get
            {
                if (HisdataPoint) return Hisdatalist2;
                else return Hisdatalist1;
            }
            set
            {
                if (HisdataPoint) Hisdatalist2 = value;
                else Hisdatalist1 = value;
            }
        }
        /// <summary>
        /// 返回历史数据的主缓存 新到的数据存放的地方
        /// </summary>
        public ConcurrentQueue<T> ToInsertCatch
        {
            get
            {
                if (HisdataPoint) return Hisdatalist1;
                else return Hisdatalist2;
            }
        }
        /// <summary>
        /// 清理存储列表
        /// </summary>
        public void ToSaveCatchClear()
        {
            if (HisdataPoint) Hisdatalist2 = new ConcurrentQueue<T>();
            else Hisdatalist1 = new ConcurrentQueue<T>();
        }
        /// <summary>
        /// 收集全部的列表
        /// </summary>
        /// <returns></returns>
        public List<T> GetAllDataAndClear()
        {
            List<T> adl = new List<T>();
            if (Hisdatalist1.Cof_CheckAvailable())
            {
                adl.Cof_AddRange(Hisdatalist1.ToList());
                Hisdatalist1.Clear();
            }
            if (Hisdatalist2.Cof_CheckAvailable())
            {
                adl.Cof_AddRange(Hisdatalist2.ToList());
                Hisdatalist2.Clear();
            }
            return adl;
        }
        /// <summary>
        /// 返回当前存放历史数据的数据量
        /// </summary>
        public int ToInsertCatchLength
        {
            get
            {
                if (HisdataPoint) return Hisdatalist1.Count;
                else return Hisdatalist2.Count;
            }
        }
        /// <summary>
        /// 返回辅助缓存的数据量
        /// </summary>
        public int ToSaveCatchLength
        {
            get
            {
                if (HisdataPoint) return (Hisdatalist2 == null ? 0 : Hisdatalist2.Count);
                else return (Hisdatalist1 == null ? 0 : Hisdatalist1.Count);
            }
        }




        #region 释放资源
        private bool disposed = false;


        /// <summary>
        /// 释放对象资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///	释放对象的实例变量。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {


            if (!this.disposed)
            {
                if (disposing)
                {
                    if (Hisdatalist1 != null && Hisdatalist1.Count > 0) Hisdatalist1.Clear();
                    if (Hisdatalist2 != null && Hisdatalist2.Count > 0) Hisdatalist2.Clear();
                    // Release managed resources	
                    //  ds.Dispose();
                }
                disposed = true;

            }

        }


        ~HisDatacatch()
        {
            Dispose(false);
        }

        #endregion

    }


    
}
