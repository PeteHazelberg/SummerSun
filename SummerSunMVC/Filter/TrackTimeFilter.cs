using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SummerSunMVC.Filter
{
    public class TrackTimeFilter : ActionFilterAttribute
    {
        private Stopwatch _stopWatch;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TrackTimeFilter));

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            _stopWatch.Stop();
            Log(filterContext.RouteData, _stopWatch.ElapsedMilliseconds);
        }

        private void Log(RouteData routeData, long time)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            _logger.Info(string.Format("{0}.{1} --> {2} (ms)", controllerName, actionName, time));
        }
    }
}