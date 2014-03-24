using System;
using Ninject.Modules;
using SummerSun;

namespace SummerSunConsoleHost
{
    class SummerSunNinjectBindings : NinjectModule
    {
        public override void Load()
        {
            var apiEndpoint = Settings.Get("PanoptixBuildingApiEndpoint", "https://dev-apiproxy.panoptix.com/building",
                s => new Uri(s).IsAbsoluteUri,
                "PanoptixBuildingApiEndpoint in configuration settings should be the base URL of the Building API.");
            this.Bind<SummaryBuilder>().To<SummaryBuilder>().WithConstructorArgument("buildingApiUrl", apiEndpoint);
        }
    }
}
