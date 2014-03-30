using BuildingApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;

namespace SummerSunMVC.App_Start
{
    // It does not seems right....
    // TO DO figure out how to leverage IoC
    public class BuildingAPIClient
    {
        private static ICompanyProvider _companyProviderInstance = null;
        private static ITokenProvider _tokenProvider = null;
        private static EquipmentClient _equipmentClient = null;
        private static readonly TypesClient _typesClient = null;

        static BuildingAPIClient ()
	    {
            string clientId = ConfigurationManager.AppSettings["JciClientId"];
            string clientSecret = ConfigurationManager.AppSettings["JciClientSecret"];
            string tokenEndpoint = ConfigurationManager.AppSettings["JciTokenEndpoint"];
            string companyApiEndpoint = ConfigurationManager.AppSettings["JciCompanyApiEndpoint"];
            string buildingApiEndpoint = ConfigurationManager.AppSettings["JciBuildingApiEndpoint"];
            IWebProxy proxy = WebProxy.GetDefaultProxy();

            _tokenProvider = new TokenClient(clientId,clientSecret, tokenEndpoint, proxy);
            _companyProviderInstance = new CompanyClient(companyApiEndpoint, _tokenProvider);
            _equipmentClient = new EquipmentClient(_tokenProvider, buildingApiEndpoint);
            _typesClient = new TypesClient(_tokenProvider, buildingApiEndpoint);
	    }
        
        public static ICompanyProvider CompanyProvider
        {
            get 
            { 
                return _companyProviderInstance; 
            }
        }
        public static EquipmentClient EquipmentClient
        {
            get 
            { 
                return _equipmentClient; 
            }
        }

        public static TypesClient TypesClient
        {
            get
            {
                return _typesClient;
            }
        }

    }
}