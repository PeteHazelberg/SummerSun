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
            var clientId = Settings.Get("JciClientId", null, s => !string.IsNullOrEmpty(s), "You must specify a JCI-issued client id in the .config file.");
            var clientSecret = Settings.Get("JciClientSecret", null, s => !string.IsNullOrEmpty(s), "You must specify a JCI-issued client secret in the .config file.");
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new ArgumentException("JciClientId and/or JciClientSecret is not valid. Add them to the .config file settings section.");
            var tokenEndpoint = Settings.Get("JciTokenEndpoint", "https://identity.johnsoncontrols.com/issue/oauth2/token",
                s => new Uri(s).IsAbsoluteUri,
                "JciTokenEndpoint in configuration settings should be the URL where JCI's tokens are issued from.");
            var buildingApiEndpoint = Settings.Get("JciBuildingApiEndpoint", "https://api.panoptix.com/",
                s => new Uri(s).IsAbsoluteUri,
                "JciBuildingApiEndpoint in configuration settings should be the base URL of the Building API.");
            
            // ReSharper disable once CSharpWarnings::CS0618
            IWebProxy proxy = WebProxy.GetDefaultProxy();

            Bind<ITokenProvider>().To<TokenClient>()
                .WithConstructorArgument("id", clientId)
                .WithConstructorArgument("secret", clientSecret)
                .WithConstructorArgument("endpoint", tokenEndpoint)
                .WithConstructorArgument("proxy", proxy);

            Bind<ICompanyProvider>().To<CompanyClient>().WithConstructorArgument("endpoint", buildingApiEndpoint); ;
            this.Bind<EquipmentClient>().To<EquipmentClient>().WithConstructorArgument("buildingApiUrl", buildingApiEndpoint);
            this.Bind<ApiClient>().To<ApiClient>().WithConstructorArgument("buildingApiUrl", buildingApiEndpoint);
        }
    }
}
