using Newtonsoft.Json;
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UserServer.Services;

namespace UserServer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SetupRemotingService();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            HttpConfiguration config = GlobalConfiguration.Configuration;
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();
        }

        private void SetupRemotingService()
        {
            var remotingTcpChannel = new TcpChannel(8001);

            ChannelServices.RegisterChannel(
              remotingTcpChannel,
              false);

            RemotingConfiguration.RegisterWellKnownServiceType(
              typeof(Services.UserServices),
              "RemoteUserServices",
              WellKnownObjectMode.SingleCall);

            Console.WriteLine("User Services server has started at: tcp://127.0.0.1/RemoteUserServices");

            //Thread unregisterChannelThread = new Thread(() => UnregisterChannel(remotingTcpChannel));
            //unregisterChannelThread.Start();
        }


        private static void UnregisterChannel(TcpChannel remotingTcpChannel)
        {
            ChannelServices.UnregisterChannel(remotingTcpChannel);
        }
    }
}
