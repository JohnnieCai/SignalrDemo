using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SignalrServer
{
    public class ChatHub : Hub
    {
        public static Dictionary<string, List<string>> userDic = new Dictionary<string, List<string>>();

        #region 反注册用户
        /// <summary>
        /// 反注册用户
        /// </summary>
        /// <param name="userId">用户编号</param>
        public static void UnloadUser(string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    if (userDic.ContainsKey(userId))
                    {
                        userDic.Remove(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("UnloadUser执行错误:" + ex.Message);
            }
        }
        #endregion
        #region 用户服务端注册
        /// <summary>
        /// 用户服务端注册
        /// </summary>
        /// <param name="userId"></param>
        public void LoginIn(string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    string connectionId = Context.ConnectionId;
                    List<string> clientList = null;
                    if (userDic.ContainsKey(userId))
                    {
                        clientList = userDic[userId];
                        if (!clientList.Contains(connectionId))
                        {
                            clientList.Add(connectionId);
                        }
                    }
                    else
                    {
                        clientList = new List<string>();
                        clientList.Add(connectionId);

                        userDic.Add(userId, clientList);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("LoginIn执行错误:" + ex.Message);
            }
        }
        #endregion
        //public void SendHeartBeat()
        //    Clients.All.receiveHeartBeat()
        //}
        #region 向所有用户发送消息
        /// <summary>
        /// 向所有用户发送消息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        public void SendAllMsg(string name, string message)
        {
            // 客户端通过调用broadcastMessage来获取数据
            Clients.All.broadcastMessage(name, message);

            //Clients.Client("kkk").broadcastMessage(name, message);
        }
        #endregion
        #region 给指定用户发送未读消息数量
        /// <summary>
        /// 给指定用户发送未读消息数量
        /// </summary>
        /// <param name="msgNum"></param>
        public void SendMsg(int msgNum)
        {
            // 客户端通过调用broadcastMessage来获取数据
            Clients.All.receiveMessage(msgNum);

            //Clients.Client("kkk").broadcastMessage(name, message);
        }
        #endregion
        #region 给指定用户发送未读消息数量
        /// <summary>
        /// 给指定用户发送未读消息数量
        /// </summary>
        /// <param name="msgNum"></param>
        public void Send(string userIdStr)
        {
            if (!string.IsNullOrEmpty(userIdStr))
            {
                string[] idArray = userIdStr.Split(',');
                List<string> connList = new List<string>();
                string userId = "";
                for (int i = 0; i < idArray.Length; i++)
                {
                    userId = idArray[i];
                    if (userDic.ContainsKey(userId))
                    {
                        connList = userDic[userId];
                        for (int j = 0; j < connList.Count; j++)
                        {
                            Clients.Client(connList[j]).Update(1);
                        }
                    }
                }
            }
            // 客户端通过调用broadcastMessage来获取数据
            //Clients.All.Update(actionId);

            //Clients.Client("kkk").broadcastMessage(name, message);
        }
        #endregion

        string RootPath = System.AppDomain.CurrentDomain.BaseDirectory;
        #region 记录日志
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="args">参数信息(默认不记录byte[]数组的信息)</param>
        public void WriteLocalLog(string msg)
        {
            string customIP = GetClientEndPoint();
            //string serverPath =  HttpContext.Current.Server.MapPath("Logs");

            string logFullPath = Path.Combine(RootPath, "Logs");
            if (!Directory.Exists(logFullPath))
            {
                Directory.CreateDirectory(logFullPath);

                logFullPath = Path.Combine(logFullPath, DateTime.Now.ToString("yyyyMMddHH") + ".log");
                string Content = DateTime.Now.ToString() + " | " + customIP + " | " + msg;
                //    " | Method:" + methodName
                //For i As int = 0 To args.Length - 1
                //    Content += " | " + args(i).ToString()
                //Next
                //lock (methodName)
                //{
                WriteLogToFile(logFullPath, Content);
                //}
            }
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="logFullPath">文件全路径</param>
        /// <param name="content">要写入的内容</param>
        private void WriteLogToFile(string logFullPath, string content)
        {
            try
            {
                lock (logFullPath)
                {
                    using (StreamWriter sw = new StreamWriter(logFullPath, true))
                    {
                        sw.WriteLine(content);
                        sw.Close();
                        sw.Dispose();
                    }
                }
            }
            catch
            {
            }
        }
        #endregion
        #region 获取客户端IP和端口号
        /// <summary>
        /// 获取客户端IP和端口号
        /// </summary>
        /// <returns></returns>
        private string GetClientEndPoint()
        {
            try
            {
                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    return System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(',')[0];
                }
                else
                {
                    return System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch (Exception ex)
            {
                return "初始化无法获取IP," + ex.Message;
            }
        }
        #endregion

        /// <summary>
        /// 在连接上时
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            //向服务端写入一些数据
            WriteLocalLog("客户端连接ID:" + Context.ConnectionId);
            //Program.serverFrm.WriteToInfo("客户端连接ID:" + Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            //向服务端写入一些数据
            WriteLocalLog("客户端退出ID:" + Context.ConnectionId);
            //Program.serverFrm.WriteToInfo("客户端退出ID:" + Context.ConnectionId);
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            //向服务端写入一些数据
            WriteLocalLog("客户端退出ID:" + Context.ConnectionId);
            //Program.serverFrm.WriteToInfo("客户端退出ID:" + Context.ConnectionId);
            return base.OnDisconnected();
        }
    }
}