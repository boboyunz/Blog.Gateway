using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
namespace Blog.Core.Common.Helper
{
    public class TextLogerBll
    {
        public static void LogWriter(string logName, int logtype = 6)
        {
            InfluxdbHelper.GetInstance().AddInfoLog(logtype, "TextLoger", "system", logName);
        }

        public static void LogWriter(string logName, System.Exception e, int logtype = 1)
        {
            InfluxdbHelper.GetInstance().AddErrLog(logtype, "TextLoger", "system", logName, e);

        }

        public static void LogVehicleWriter(string logName, System.Exception e, string JLYID, int logtype = 1)
        {
            InfluxdbHelper.GetInstance().AddErrLog(1, $"TextLoger.{JLYID}", "system", logName, e);

        }


        public static void LogVehicleWriter(string Data, string JLYID, int logtype = 6)
        {
            InfluxdbHelper.GetInstance().AddInfoLog(logtype, $"TextLoger.{JLYID}", "system", Data);

        }


        public static void LogVehicleWriter(byte[] Data, string JLYID, int logtype = 6)
        {

            LogVehicleWriter(TxtToolBLL.GetBytesHexStr(Data), JLYID, logtype);

        }
    }
}
