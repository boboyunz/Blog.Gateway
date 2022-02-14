using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text;
using Minio;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
namespace Blog.Core.Common.Helper
{
    public class MinioHelper : IDisposable
    {

        #region  获得实例 初始化

        private static MinioHelper _handler = null;

        public static MinioHelper GetInstance()
        {
            if (_handler == null)
            {
                _handler = new MinioHelper();
                string ippoint = Appsettings.GetValue("Minio:Endpoint");
                string uid = Appsettings.GetValue("Minio:uid");
                string pwd = Appsettings.GetValue("Minio:pwd");

                _handler.minioClient = new MinioClient(ippoint, uid, pwd);
            }
            return _handler;
        }
       
        public MinioClient minioClient;
        #endregion

        #region 构造函数
        private bool _alreadyDispose = false;
        public MinioHelper()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        ~MinioHelper()
        {
            Dispose(); ;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDispose) return;
            _alreadyDispose = true;
        }
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion




        public async Task<JToken> UploadFile(Stream FileStream, string FileName,string BucketName,string ContentType)
        {
            JToken Jobj = JToken.Parse("{}");
            Jobj["result"] = 0;
            await minioClient.PutObjectAsync(BucketName, FileName, FileStream, FileStream.Length, ContentType);
            Jobj["result"] = 1;
            return Jobj;
        }

    }



}
