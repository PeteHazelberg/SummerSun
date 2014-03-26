using System;
using System.Net;
using BuildingApi;
using Ninject.Modules;

namespace SummerSun
{
    class SummerSunNinjectBindings : NinjectModule
    {
        public override void Load()
        {
            var clientId = Settings.Get("JciClientId", null);
            var clientSecret = Settings.Get("JciClientSecret", null);
            var tokenEndpoint = Settings.Get("JciTokenEndpoint", "https://identity.johnsoncontrols.com/issue/oauth2/token",
                s => new Uri(s).IsAbsoluteUri,
                "JciTokenEndpoint in configuration settings should be the URL where JCI's tokens are issued from.");
            var buildingApiEndpoint = Settings.Get("JciBuildingApiEndpoint", "https://api.panoptix.com/building",
                s => new Uri(s).IsAbsoluteUri,
                "JciBuildingApiEndpoint in configuration settings should be the base URL of the Building API.");
            var companyApiEndpoint = Settings.Get("JciCompanyApiEndpoint", "https://api.panoptix.com/companies",
                s => new Uri(s).IsAbsoluteUri,
                "JciCompanyApiEndpoint in configuration settings should be the URL of the Companies API."); 
            
            // ReSharper disable once CSharpWarnings::CS0618
            IWebProxy proxy = WebProxy.GetDefaultProxy();

            Bind<ITokenProvider>().To<TokenClient>()
                .WithConstructorArgument("id", clientId)
                .WithConstructorArgument("secret", clientSecret)
                .WithConstructorArgument("endpoint", tokenEndpoint)
                .WithConstructorArgument("proxy", proxy);

            Bind<ICompanyProvider>().To<CompanyClient>().WithConstructorArgument("endpoint", companyApiEndpoint); ;
            this.Bind<EquipmentClient>().To<EquipmentClient>().WithConstructorArgument("buildingApiUrl", buildingApiEndpoint);
        }
    }
}
