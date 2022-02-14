using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Vibrant.InfluxDB;
//using Vibrant.InfluxDB.Client;

using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using InfluxData.Net.InfluxDb.Enums;
using InfluxData.Net.InfluxDb.Models.Responses;
using Blog.Core.Common.DB;
using Microsoft.Extensions.Configuration;
using Magicodes.ExporterAndImporter.Excel;
using OfficeOpenXml.Table;
using Magicodes.ExporterAndImporter.Core;
//using System.Web.Services.Discovery;

namespace Blog.Core.Common.Helper
{
    public class InfluxdbHelper
    {

        #region  获得实例 初始化

        private static InfluxdbHelper _handler = null;

        public static InfluxdbHelper GetInstance()
        {
            if (_handler == null)
            {
                //"redisdatabase": "13",
                //"redishost": "159.226.111.112",
                //"redisport": "9075",
                //"redispassword": "ibalife"

                IConfiguration configobj = TxtToolBLL.GetConfiguration();
                string Influxdburl = configobj["Influxdb:Endpoint"];  //"http://localhost:8086"
                string InfluxdbUID = configobj["Influxdb:uid"];
                string InfluxdbPWD = configobj["Influxdb:pwd"];
                string InfluxdbName = configobj["Influxdb:dbname"];


                _handler = new InfluxdbHelper();
                if (!string.IsNullOrEmpty(InfluxdbName)) _handler.DBname = InfluxdbName;

                //_handler.influxClient = new InfluxClient(new Uri(Influxdburl), InfluxdbUID, InfluxdbPWD);

                _handler.influxClient = new InfluxDbClient(Influxdburl, InfluxdbUID, InfluxdbPWD, InfluxDbVersion.Latest);
                _handler.initsysLoger();
            }
            return _handler;
        }
        public InfluxdbHelper()
        { }
        public InfluxDbClient influxClient;
        #endregion

        #region 基础方法

        /// <summary>
        /// 默认的数据库
        /// </summary>
        public string DBname = "mndata";
        /// <summary>
        /// 连续查询的时间间隔
        /// </summary>
        public List<int> ContinuousDic = new List<int> { 30, 60, 300, 600, 1800, 3600 };

        /// <summary>
        ///  创建 表格 连续查询
        /// </summary>
        /// <param name="db">数据库名字</param>
        /// <param name="tbname">表名</param>
        /// <param name="valuetag">对一个字段计算平均值</param>
        public async Task CreatTableContinuousQuery(string db, string tableName, string valuetag)
        {
            try
            {
                var resultck = await IfContinuousExits(db, tableName);
                if (resultck == false)
                    foreach (var DsItem in ContinuousDic)
                    {
                        string queries = tableName + "_" + DsItem.ToString();
                        var cqParams = new CqParams()
                        {
                            DbName = db,
                            CqName = queries, // CQ name
                            Downsamplers = new List<string>()
                            {
                                string.Format("mean({0}) AS value", valuetag),
                            },
                            DsSerieName = queries, // new (downsample) serie name
                            SourceSerieName = tableName, // source serie name to get data from
                            Interval = DsItem.ToString() + "s"
                            //,FillType = FillType.Null
                            // you can also add a list of tags to keep in the DS serie here
                        };
                        var cresult = await influxClient.ContinuousQuery.CreateContinuousQueryAsync(cqParams);
                        //string ContinuousQuerys = string.Format(" BEGIN SELECT mean({3}) INTO {0} FROM {1} GROUP BY time({2}s) END ", queries, tableName, DsItem, valuetag);
                        //var cresult = await influxClient.
                        // CreateContinuousQuery(db, queries, ContinuousQuerys);
                    }
            }
            catch (System.Exception ee)
            {
                string mm = ee.Message;
            }
        }
        /// <summary>
        /// 获取需要定期执行的语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="valuetag"></param>
        /// <returns></returns>
        public string[] CreatContinuousQuerySQL(string tableName, string valuetag)
        {
            List<string> sql = new List<string>();
            foreach (var DsItem in ContinuousDic)
            {
                string queries = tableName + "_" + DsItem.ToString();
                sql.Add(string.Format(@" SELECT mean({3}) as value INTO {0} FROM {1} GROUP BY time({2}s) ", queries, tableName, DsItem, valuetag));
            }
            return sql.ToArray();
        }


        public List<KeyValuePair<string, string>> CreatContinuousQuerySQL2(string dbname, string tableName, string valuetag)
        {
            if (string.IsNullOrEmpty(dbname)) dbname = DBname;
            List<KeyValuePair<string, string>> sql = new List<KeyValuePair<string, string>>();
            foreach (var DsItem in ContinuousDic)
            {
                string queries = tableName + "_" + DsItem.ToString();
                sql.Add(new KeyValuePair<string, string>(queries, string.Format(@"CREATE CONTINUOUS QUERY {0} ON {4} BEGIN SELECT mean({3}) as value INTO {0} FROM {1} GROUP BY time({2}s) END", queries, tableName, DsItem, valuetag, dbname)));
            }
            return sql;
        }
        public async Task CreatContinuousQuerySQL3(string dbname, List<KeyValuePair<string, string>> CQlist)
        {
            if (string.IsNullOrEmpty(dbname)) dbname = DBname;
            var cqForTestDb = await influxClient.ContinuousQuery.GetContinuousQueriesAsync(dbname);

            CQlist = CQlist.Where(d => cqForTestDb.Where(c => c.Name == d.Key).Count() == 0).ToList();

            await influxClient.Client.QueryAsync(CQlist.Select(d => d.Value), dbname);
        }
        /// <summary>
        /// 创建 表格 连续查询
        /// </summary>
        /// <param name="tbname">表名</param>
        /// <param name="valuetag">对一个字段计算平均值</param>
        /// <returns></returns>
        public async Task CreatTableContinuousQuery(string tbname, string valuetag)
        {
            await CreatTableContinuousQuery(DBname, tbname, valuetag);
        }
        /// <summary>
        /// 判断是否针 对表 创建 连续查询
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        private async Task<bool> IfContinuousExits(string db, string tableName)
        {
            var cqForTestDb = await influxClient.ContinuousQuery.GetContinuousQueriesAsync(db);

            if (cqForTestDb != null)
            {
                return cqForTestDb.Where(d => d.Name.IndexOf(tableName) >= 0).Count() > 0;
            }
            return false;
        }

        public async Task IfContinuousExits(string tableName)
        {
            await IfContinuousExits(DBname, tableName);
        }



        /// <summary>
        /// 写入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <param name="Dlist"></param>
        /// <returns></returns>
        public async Task WriteAsync(string db, List<Point> Dlist)
        {
            try
            {
                await influxClient.Client.WriteAsync(Dlist, db).ConfigureAwait(false);
            }
            catch (System.Exception ee)
            {
                string mm = ee.Message;
            }
        }
       

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="Dlist"></param>
        /// <returns></returns>
        public async Task WriteAsync(List<Point> Dlist)
        {
            try
            {
                await influxClient.Client.WriteAsync(Dlist, DBname);
            }
            catch (System.Exception ee)
            {

                InfluxdbHelper.GetInstance().AddLog("InfluxdbHelper-WriteAsync2  " +
                    Dlist.Select(d => string.Format("{0}-{1}-{2}",
                    d.Name,
                    d.Tags.Select(tag => tag.Key + tag.Value.ToString()).Cof_ToString_WithSplit(),
                    d.Fields.Select(tag => tag.Key + tag.Value.ToString()).Cof_ToString_WithSplit())).Cof_ToString_WithSplit('|'), ee);
                string mm = ee.Message;
            }
        }


        #endregion

        #region 通用日志查询

        public string GetSysComLogqueryStr(SysComLog_query query)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(query.LsubType))
                sb.AppendFormat(@" and LsubType='{0}' ", query.LsubType);
            if (!string.IsNullOrEmpty(query.UserID))
                sb.AppendFormat(@" and UserID='{0}' ", query.UserID);
            if (!string.IsNullOrEmpty(query.SysID))
                sb.AppendFormat(@" and SysID='{0}' ", query.SysID);
            if (!string.IsNullOrEmpty(query.PrjID))
                sb.AppendFormat(@" and PrjID='{0}' ", query.PrjID);
            if (!string.IsNullOrEmpty(query.TimeStart))
                sb.AppendFormat(@" and time>'{0}' ", query.TimeStart);
            if (!string.IsNullOrEmpty(query.TimeEnd))
                sb.AppendFormat(@" and time<'{0}' ", query.TimeEnd);
            if (!string.IsNullOrEmpty(query.KeyData))
                sb.AppendFormat(@" and KeyData =~ /{0}/ ", query.KeyData);
            return sb.ToString();

        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<List<SysComLog>> QuerySysComLogData(SysComLog_query query, string db)
        {
            string pageing = "";
            if (query.page > 0)
            {
                pageing = string.Format(" limit {0} offset {1}", query.pageSize, (query.page - 1) * query.pageSize);
            }
            if (string.IsNullOrEmpty(db)) db = "systemlog";
            string TabelName = GetTableName(query.Ltype.Cof_ToSafeInt());

            string queryString = string.Format(@"select * from ""{0}"" where 1=1  {1}  order by time desc {2}",
                 TabelName,
                 GetSysComLogqueryStr(query),
                 pageing
                 );

            //从指定库中查询数据
            var response = await influxClient.Client.QueryAsync(queryString, db);
            //得到Serie集合对象（返回执行多个查询的结果）
            var series = response.ToList();
            if (!series.Cof_CheckAvailable()) return null;
            //取出第一条命令的查询结果，是一个集合
            var list = series[0].Values;
            if (!list.Cof_CheckAvailable()) return null;
            List<SysComLog> listR = new List<SysComLog>();
            foreach (var Litem in list)
            {
                var pd = new SysComLog()
                {
                    LogTime = Convert.ToDateTime(Litem[0]).AddHours(-8),
                    KeyShow = Litem[1].Cof_SafeToString(),
                    LogData = Litem[2].Cof_SafeToString(),
                    LsubType = Litem[3].Cof_SafeToString().Cof_ToSafeInt(),
                    Ltype = Litem[4].Cof_SafeToString().Cof_ToSafeInt(),
                    PrjID = Litem[5].Cof_SafeToString(),
                    SysID = Litem[6].Cof_SafeToString(),
                    UserID = Litem[7].Cof_SafeToString()
                };
                listR.Add(pd);
            }
            return listR.OrderByDescending(d => d.LogTime).ToList();
        }
        /// <summary>
        /// 统计数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<long> QuerySysComLog_Count(SysComLog_query query, string db)
        {
            if (string.IsNullOrEmpty(db)) db = "systemlog";
            string TabelName = GetTableName(query.Ltype.Cof_ToSafeInt());

            string queryString = string.Format(@"select count(*) from {0}  where 1=1  {1} ", TabelName, GetSysComLogqueryStr(query));

            long total = 0;
            //从指定库中查询数据
            var response = await influxClient.Client.QueryAsync(queryString, db);
            if (response == null || response.Count() == 0) return total;

            //得到Serie集合对象（返回执行多个查询的结果）
            var series = response.ToList();
            //取出第一条命令的查询结果，是一个集合
            var list = series[0].Values;
            total = Convert.ToInt64(list[0][1]);
            return total;

        }
        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<PageModelCom<SysComLog>> QuerySysComLog(SysComLog_query query, string db)
        {
            var rstobj = new PageModelCom<SysComLog> { page = query.page, PageSize = query.pageSize };
            rstobj.dataCount = await QuerySysComLog_Count(query, db);
            if (rstobj.dataCount == 0) return rstobj;
            rstobj.data = await QuerySysComLogData(query, db);
            return rstobj;
        }



        #endregion

        #region 通用系统日志
        /// <summary>
        /// 获得日志名字 syslog+ Ltype(X4)
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string GetTableName( int LType)
        {
            return $"syslog{LType.ToString("X4")}";
        }
        private InfluxdbLog<SysComLog> sysloger ;
        private string syslogDB = "systemlog";
        private void initsysLoger()
        {
            sysloger = new InfluxdbLog<SysComLog>();
            sysloger.initial(SysComLog.GetTableName, 
               (t) =>{
                    try
                    {
                        WriteAsync(syslogDB, t.Select(d => d.toPoint()).ToList()).Wait();
                    }
                    catch (System.Exception ee)
                    {
                        Console.Out.WriteLineAsync($"InfluxdbHelper .SysComLog.save.error:{ee.Message},{ee.StackTrace}").Wait();
                 
                    }
                }
                );
        }
        /// <summary>
        /// 添加log
        /// </summary>
        /// <param name="log"></param>
        public void AddLog(SysComLog log)
        {
            sysloger.AddLog(log);
        }
        /// <summary>
        /// 批量添加Log
        /// </summary>
        /// <param name="log"></param>
        public void AddLog(List<SysComLog> log)
        {
            sysloger.AddLog(log);
        }
        /// <summary>
        /// 通过名称和错误对象 增加日志
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="e"></param>
        public void AddLog(string logName, System.Exception e)
        {
            SysComLog logobjErr = new SysComLog { Ltype = 1, LsubType = 2, KeyShow = e.Message, UserID = "ErrorMsg", LogTime = DateTime.Now, LogDataShow = $"{logName}----{e.StackTrace}" };
            sysloger.AddLog(logobjErr);
        }
        /// <summary>
        /// 添加错误日志
        /// </summary>
        /// <param name="ltype">主类型</param>
        /// <param name="lsubType">辅助类型</param>
        /// <param name="Keydata">关键字</param>
        /// <param name="SysName">系统名称</param>
        /// <param name="logName">日志名称</param>
        /// <param name="e"></param>
        public void AddErrLog(int ltype , int lsubType, string Keydata,string SysName ,string logName, System.Exception e)
        {
            SysComLog logobjErr = new SysComLog { Ltype = ltype, LsubType = lsubType, KeyShow = Keydata, SysID= SysName, UserID = "ErrorMsg", LogTime = DateTime.Now, LogDataShow = $"{logName}--{e.Message}--{e.StackTrace}" };
            sysloger.AddLog(logobjErr);
        }
        public void AddErrLog(int lsubType, string Keydata, string SysName, string logName, System.Exception e)
        {
            SysComLog logobjErr = new SysComLog { Ltype = 1, LsubType = lsubType, KeyShow = Keydata, SysID = SysName, UserID = "ErrorMsg", LogTime = DateTime.Now, LogDataShow = $"{logName}--{e.Message}--{e.StackTrace}" };
            sysloger.AddLog(logobjErr);
        }
        /// <summary>
        /// 增加调试日志
        /// </summary>
        /// <param name="lsubType"></param>
        /// <param name="Keydata"></param>
        /// <param name="SysName"></param>
        /// <param name="logdata"></param>
        public void AddDebugLog( int lsubType, string Keydata, string SysName, string logdata )
        {
            SysComLog logobjErr = new SysComLog { Ltype = 2, LsubType = lsubType, KeyShow = Keydata, SysID = SysName, UserID = "DebugMsg", LogTime = DateTime.Now, LogDataShow = logdata };
            sysloger.AddLog(logobjErr);
        }

        public void AddInfoLog(int lsubType, string Keydata, string SysName, string logdata)
        {
            SysComLog logobjErr = new SysComLog { Ltype = 4, LsubType = lsubType, KeyShow = Keydata, SysID = SysName, UserID = "DebugMsg", LogTime = DateTime.Now, LogDataShow = logdata };
            sysloger.AddLog(logobjErr);
        }
        /// <summary>
        /// 增加数据采集日志
        /// </summary>
        /// <param name="lsubType"></param>
        /// <param name="TaskID"></param>
        /// <param name="Keydata"></param>
        /// <param name="SysName"></param>
        /// <param name="logdata"></param>
        public void AddDataCollectLog(int lsubType,string TaskID, string Keydata, string SysName, string logdata)
        {
            SysComLog logobjErr = new SysComLog { Ltype = 5, LsubType = lsubType, KeyShow = Keydata, SysID = SysName, UserID = "DataCollect", PrjID= TaskID, LogTime = DateTime.Now, LogDataShow = logdata };
            sysloger.AddLog(logobjErr);
        }
        #endregion

        #region  通用查询数据方法
        /// <summary>
		/// 返回结果 分页查询
		/// </summary>
		/// <param name="query"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public async Task<PageModelCom<T>> QueryPageData<T>(PointQuery query, string db, string TabelName, Func<IList<object>, T> ParseFun)
        {
            var rstobj = new PageModelCom<T> { page = query.page, PageSize = query.pageSize };

            try
            {
                if (query.page > 0)
                {
                    rstobj.dataCount = await QueryDataCount(query, db, TabelName);
                    if (rstobj.dataCount == 0) return rstobj;
                }

                rstobj.data = await QueryData<T>(query, db, TabelName, ParseFun);

            }
            catch (System.Exception ee)
            {
                AddErrLog(3, "InfluxdbHelper.QueryPageData", "system", $"QueryPageData.Error:db={db},TabelName={TabelName},query={query.Cof_ToJsonString()}", ee);
                string sss = ee.Message;
            }


            return rstobj;
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<List<T>> QueryData<T>(PointQuery query, string db, string TabelName, Func<IList<object>, T> ParseFun)
        {
            string pageing = "";
            if (query.page > 0)
            {
                pageing = string.Format(" limit {0} offset {1}", query.pageSize, (query.page - 1) * query.pageSize);
            }
            if (string.IsNullOrEmpty(db)) db = DBname;
            string orderby = string.IsNullOrEmpty(query.orderBy) ? " time desc " : query.orderBy;


            string queryString = string.Format(@"select {4} from ""{0}"" where 1=1  {1}  order by {3} {2}",
                 TabelName,
                 query.ToQueryString(),
                 pageing,
                 orderby,
                 (string.IsNullOrEmpty(query.Columns)?"*": query.Columns)
                 );

            //从指定库中查询数据
            var response = await influxClient.Client.QueryAsync(new string[] { queryString }, db).ConfigureAwait(false);

            //await InfluxdbHelper.GetInstance().influxClient.Client.QueryAsync(queryString, db);
            //得到Serie集合对象（返回执行多个查询的结果）
            var series = response.ToList();
            if (!series.Cof_CheckAvailable()) return null;
            //取出第一条命令的查询结果，是一个集合
            var list = series[0].Values;
            if (!list.Cof_CheckAvailable()) return null;
            List<T> listR = new List<T>();
            foreach (var Litem in list)
            {
                var pd = ParseFun(Litem);
                listR.Add(pd);
            }
            return listR;
        }
        /// <summary>
        /// 统计数据数量
        /// </summary>
        /// <param name="query"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<int> QueryDataCount(PointQuery query, string db, string TabelName)
        {
            if (string.IsNullOrEmpty(db)) db = DBname;

            string queryString = string.Format(@"select count(*) from {0}  where 1=1  {1} ", TabelName, query.ToQueryString());

            int total = 0;
            //从指定库中查询数据
            var response = await influxClient.Client.QueryAsync(queryString, db).ConfigureAwait(false);
            if (response == null || response.Count() == 0) return total;

            //得到Serie集合对象（返回执行多个查询的结果）
            var series = response.ToList();
            //取出第一条命令的查询结果，是一个集合
            var list = series[0].Values;
            total = Convert.ToInt32(list[0][1]);
            return total;
        }
        #endregion
    }
    public enum SysComLogType
    {
        #region 通用日志类类型
        通用信息日志 = 1,
        通用错误日志 = 2,
        通用查询日志 = 3,

        #endregion

        #region 异常日志类类型
        #endregion

        #region 调试日志类型
        #endregion

        #region 报警日志类型
        #endregion     

        #region 信息日志类型
        信息日志TextLoger = 6,
        通信层日志 = 7,
        通信层WebSocket日志 = 8,
        网关信息日志 = 9,


        #region RPC 10-19
        RPC指令日志 = 10,
        #endregion


        #region 计算任务日志 20-49
        计算任务日志 = 20,
        计算任务错误日志 = 21,
        计算任务同步车辆信息 = 22,
        计算任务同步车辆错误 = 23,


        #endregion

        #region 用户操作日志  55 -79
        用户操作日志 = 55,
        #endregion


        #endregion
    }




    public class PointData
    {
        /// <summary>
        /// 有效性 0-有效  1-无效
        /// </summary>
        public int Dqty { get; set; }
        /// <summary>
        /// 数组值
        /// </summary>
        public decimal Dvalue { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Dtime { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public int Dtype { get; set; }
    }

  
    [Serializable]
    [ExcelExporter(Name = "历史数据", TableStyle = TableStyles.Light2, AutoFitAllColumn = true, MaxRowNumberOnASheet = 50000)]
    public class SysComLog
    {
        /// <summary>
        /// 主类型 1-err,2-debug,3-alarm,4-info,5-数据采集
        /// </summary>
        [ExporterHeader(Hidden = true)]
        public int Ltype { get; set; } = 0;
        [ExporterHeader(DisplayName = "类型", ColumnIndex = 2)]
        public string LtypeName
        {
            get
            {
                switch (Ltype)
                {
                    case 1: return "错误日志";
                    case 2: return "调试日志";
                    case 3: return "预警日志";
                    case 4: return "消息日志";
                    case 5: return "网关日志";
                    default: return "默认日志";
                }
            }
        }
        /// <summary>
        /// 子类型
        /// </summary>
        [ExporterHeader(DisplayName = "子类型", ColumnIndex = 3)]
        public int LsubType { get; set; } = 0;
        /// <summary>
        /// 用户编号
        /// </summary>
        [ExporterHeader(Hidden = true)]
        public string UserID { get; set; }
        /// <summary>
        /// 系统编号
        /// </summary>
        [ExporterHeader(Hidden = true)]
        public string SysID { get; set; }
        /// <summary>        
        ///  项目编号 也可以是任务编号
        /// </summary>
        [ExporterHeader(Hidden = true)]
        public string PrjID { get; set; }
        /// <summary>
        /// 关键字 只有数字、字母、中文、逗号
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [ExporterHeader(Hidden = true)]
        public string KeyData { get; set; }
        /// <summary>
        /// 用于录入数据
        /// </summary>
        [ExporterHeader(DisplayName = "关键字", ColumnIndex = 4)]
        public string KeyShow
        {
            get
            {
                return KeyData.Cof_FilterWord();
            }
            set
            {
                KeyData = value.Cof_FilterWord();
            }
        }
        /// <summary>
        /// 日志内容 经过编码 UTF8Bytes->hexString
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [ExporterHeader(Hidden = true)]
        public string LogData { get; set; }
        /// <summary>
        /// 日志显示 
        /// </summary>
        [ExporterHeader(DisplayName = "日志内容", ColumnIndex = 5)]
        public string LogDataShow
        {
            get
            {
                return LogData.Cof_ToHexBytes().Cof_ToUTF8String();
            }
            set
            {
                LogData = value.Cof_ToUTF8Bytes().Cof_ToHexStringData();
            }
        }
        /// <summary>
        /// 日志时间
        /// </summary> 
        [ExporterHeader(DisplayName = "时间", ColumnIndex = 1, Format = "yyyy-MM-dd HH:mm:ss", Width = 20)]
        public DateTime LogTime { get; set; }

        public Point toPoint()
        {
            //Point 中的值 不能为空 不能为空字符串  不能包括空格
            //对空格敏感 不能出现空格//.Replace("{","").Replace("}","").Replace("\"","")

            return new Point()
            {
                Name = GetTableName(this), // serie/measurement/table to write into
                Tags = new Dictionary<string, object>()
                {
                    { "Ltype", Ltype },
                    { "LsubType", LsubType },
                    { "UserID", (string.IsNullOrEmpty( UserID)?"null":UserID) },
                    { "SysID",(string.IsNullOrEmpty( SysID)?"null":SysID)  },
                    { "PrjID",(string.IsNullOrEmpty( PrjID)?"null":PrjID)  },
                    { "KeyData", (string.IsNullOrEmpty( KeyData)?"null":KeyData) }
                },
                Fields = new Dictionary<string, object>()
                {
                    { "LogData", LogData }
                },
                Timestamp = LogTime// optional (can be set to any DateTime moment)
            };
        }


        /// <summary>
        /// 获得日志名字 syslog+ Ltype(X4)
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string GetTableName(SysComLog log)
        {
            return $"syslog{log.Ltype.ToString("X4")}";
        }
    }

    [Serializable]
    public partial class SysComLog_query
    {
        /// <summary>
        /// 第几页 从1开始 小于1 则不分页
        /// </summary>
        public int page { get; set; } = 0;
        /// <summary>
        /// 每页记录数量
        /// </summary>
        public int pageSize { get; set; } = 20;
        /// <summary>
        /// 主类型 1-err,2-debug,3-alarm,4-info 必填
        /// </summary>
        public string Ltype { get; set; } 
        /// <summary>
        /// 子类型
        /// </summary>
        public string LsubType { get; set; } 
        /// <summary>
        /// 用户编号
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 系统编号
        /// </summary>
        public string SysID { get; set; }
        /// <summary>        /// 项目编号
        /// </summary>
        public string PrjID { get; set; }
        /// <summary>
        /// 关键字 只有数字、字母、中文、逗号
        /// </summary>        
        public string KeyData { get; set; }
        /// <summary>
        /// timestamp大于此时间
        /// </summary>
        public string TimeStart { get; set; }
        /// <summary>
        /// timestamp小于此时间
        /// </summary>
        public string TimeEnd { get; set; }
    }
    public class InfluxdbLog<T>
    {

        private HisDatacatch<T> LogCatch = null;
        private Func<T, string> GetTableNameFun = null;
        private Action<List<T>> SaveLogFun = null;
        private System.Threading.Timer timer;
        private System.Threading.TimerCallback timerDelegate;

        /// <summary>
        /// 添加log
        /// </summary>
        /// <param name="log"></param>
        public void AddLog(T log)
        {
            LogCatch.Add(log);
        }
        /// <summary>
        /// 批量添加Log
        /// </summary>
        /// <param name="log"></param>
        public void AddLog(List<T> log)
        {
            LogCatch.Add(log);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="GetTableName"></param>
        public void initial(Func<T, string> getTableName , Action<List<T>> saveLogFun)
        {
            LogCatch = new HisDatacatch<T>();
            GetTableNameFun = getTableName;
            SaveLogFun = saveLogFun;
            if (timer == null)
            {
                this.timerDelegate = new System.Threading.TimerCallback(this.MNReportData);
                this.timer = new System.Threading.Timer(timerDelegate, this, 20000, 20000);
            }
        }

        public void MNReportData(object e)
        {
            try
            {
                if (LogCatch.ToInsertCatchLength > 0) LogCatch.ChangeCatch();
                if (LogCatch.ToSaveCatchLength > 0)
                {
                    List<T> tosave = LogCatch.ToSaveCatch.ToList();
                    LogCatch.ToSaveCatchClear();
                    SaveLogFun(tosave);//将数据放到缓存中 统一存储
                    tosave.Clear();
                    tosave = null;
                }
            }
            catch (System.Exception ee1)
            {
                InfluxdbHelper.GetInstance().AddLog("MNReportData:", ee1);
                Console.Out.WriteLineAsync($"InfluxdbLog.MNReportData.error:{ee1.Message},{ee1.StackTrace}").Wait();
            }
        }
    }


    public interface PointClass
    {
        Point GetPoint();
    }
    public interface PointQuery
    {
        /// <summary>
		/// 第几页 从1开始 小于1 则不分页
		/// </summary>
		int page { get; set; }
        /// <summary>
        /// 每页记录数量
        /// </summary>
        int pageSize { get; set; }

        /// <summary>
        /// 排序条件
        /// </summary>
        string orderBy { get; set; }
        /// <summary>
        /// 字段
        /// </summary>
        string Columns { get; set; }

        string ToQueryString();
    }

}