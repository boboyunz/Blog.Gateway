using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Blog.Core.Common.Helper
{
    #region 返回值对象
    /// <summary>
    ///  Redis  帮助类
    /// </summary>
    public class RabbitMQHelper
    {
        public class ExchangeType
        {
            public const string direct = "direct";
            public const string fanout = "fanout";
            public const string topic = "topic";

            //fanout、direct、topic
        }
        #region  获得实例 初始化

        private static RabbitMQHelper _handler = null;

        public static RabbitMQHelper GetInstance()
        {
            if (_handler == null)
            {
                _handler = RabbitMQHelper.GetInstanceByVH("");
            }
            return _handler;
        }

        public static RabbitMQHelper GetInstanceByVH(string VirtualHost)
        {
            string RabbitHost = Appsettings.GetValue("RabbitmqDB:Host");
            string RabbitPort = Appsettings.GetValue("RabbitmqDB:Port"); 
            string RabbitVirtualHost = Appsettings.GetValue("RabbitmqDB:VirtualHost");  
            string RabbitUserName = Appsettings.GetValue("RabbitmqDB:uid");  
            string Rabbitpassword = Appsettings.GetValue("RabbitmqDB:pwd");
            if (!string.IsNullOrEmpty(VirtualHost)) RabbitVirtualHost = VirtualHost;
            RabbitMQHelper _handler2 = new RabbitMQHelper();

            ConnectionFactory factory = new ConnectionFactory();

            factory.UserName = RabbitUserName;
            factory.Password = Rabbitpassword;
            factory.VirtualHost = RabbitVirtualHost;
            factory.Port = RabbitPort.Cof_ToSafeInt();
            factory.HostName = RabbitHost;


            //ExecutorService service = Executors.newFixedThreadPool(500);
            //factory.setSharedExecutor(service);


            //factory.UserName = "admin"; 
            //factory.Password = "1qaz#edc";
            //factory.VirtualHost = "/";
            //factory.Port = 5672;
            //factory.HostName = "39.100.109.103";

            _handler2.iconnection = factory.CreateConnection();
            _handler2.imodel = _handler2.iconnection.CreateModel();

            //_handler.rds = new NewLife.Caching.FullRedis(string.Format("{0}:{1}", redishost, redisport), redispassword, redisdatabase);


            return _handler2;
        }
        public RabbitMQHelper()
        { }
        public IConnection iconnection;
        public IModel imodel;
        #endregion

        #region  方法
        public void StopReceive()
        {
            try
            {
                //imodel.QueueDeclare(queue, true, false, false, null);
                imodel.Close();
            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("RabbitMQHelper-StopReceive", ee);
            }
        }
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Data"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public void Publish(string RoutingKey, string Data)
        {
            try
            {
                //imodel.QueueDeclare(queue, true, false, false, null);
                imodel.BasicPublish("", RoutingKey, null, Encoding.UTF8.GetBytes(Data));
            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("RabbitMQHelper-Publish", ee);
            }
        }
        public void Publish(string ExchangeName, string RoutingKey, string Data)
        {
            try
            {
                //imodel.QueueDeclare(queue, true, false, false, null);
                imodel.BasicPublish(ExchangeName, RoutingKey,  null, Encoding.UTF8.GetBytes(Data));
            }
            catch (System.Exception ee)
            {
                InfluxdbHelper.GetInstance().AddLog("RabbitMQHelper-Publish", ee);
            }
        }
        #endregion


        #region 接收消息事件

        public void DirectAcceptExchangeEvent(string ExchangeName, string QueueName, string RoutingKey,  Action<string, string, byte[]> Dealer)
        {
            DirectAcceptExchangeEvent(ExchangeName, QueueName, RoutingKey, "direct", Dealer);
        }
        /// <summary>
        /// 基于事件的，当消息到达时触发事件，获取数据
        /// </summary>
        public  void DirectAcceptExchangeEvent(string ExchangeName,string QueueName,string RoutingKey,string type,Action< string ,string  ,byte[]> Dealer)
        {
            try
            {
                IModel channel = iconnection.CreateModel();
                
                channel.ExchangeDeclare(ExchangeName, type, durable: true, autoDelete: false, arguments: null);
                channel.QueueDeclare(QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                channel.QueueBind(QueueName, ExchangeName, RoutingKey);
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received +=new EventHandler<BasicDeliverEventArgs>(
                    (a, b) => {                        
                        var msgBody = b.Body.ToArray();
                        Dealer(b.Exchange, b.RoutingKey, msgBody);
                    }                    
                    );
                channel.BasicConsume(QueueName, true, consumer);

                

            }
            catch (Exception ex)
            {
                InfluxdbHelper.GetInstance().AddLog("RabbitMQHelper-DirectAcceptExchangeEvent", ex);
                
            }

        }

        
     
        #endregion
    }
    #endregion
}
