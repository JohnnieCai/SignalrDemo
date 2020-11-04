
using Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: OwinStartup(typeof(SignalrServer.StartUp))]
namespace SignalrServer
{
    
    public class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapHubs();
        }
    }
}