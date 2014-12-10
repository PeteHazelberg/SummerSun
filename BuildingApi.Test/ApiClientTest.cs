using System;
using System.Linq;
using System.Net;
using Flurl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BuildingApi
{
    [TestClass]
    public class ApiClientTest
    {
        [TestMethod]
        public void AsyncApiWalkthrough()
        {
            var tokenProvider = GetTokenProvider();
            var apiClient = new ApiClient(tokenProvider, "https://dev-apiproxy.panoptix.com/");

            var companies = apiClient.GetCompaniesAsync().Result;
            var myTestCompany = companies.FirstOrDefault(x => x.Name == "BASAPI Integration Test Customer 2");
            Assert.IsNotNull(myTestCompany);

            var point = new Point
            {
                Name = ("AsyncApiWalkthroughTest" + Guid.NewGuid()).Substring(0, 50)
            };

            var url = apiClient.BaseUrl.AppendPathSegment("building").AppendPathSegment("points");
            var writtenPoint = apiClient.PostAsync(url, myTestCompany, point).Result;
            Assert.IsNotNull(writtenPoint);
            Assert.IsNotNull(writtenPoint.Id);

            url = apiClient.BaseUrl.AppendPathSegment("building").AppendPathSegment("points").AppendPathSegment(writtenPoint.Id);
            var readPoint = apiClient.GetAsync<Point>(url, myTestCompany).Result;
            Assert.AreEqual(writtenPoint.Id, readPoint.Id);

            url = apiClient.BaseUrl.AppendPathSegment("building").AppendPathSegment("points").SetQueryParam("id", writtenPoint.Id);
            var pageOfPoints = apiClient.GetPageAsync<Point>(url, myTestCompany).Result;
            Assert.AreEqual(1, pageOfPoints.Count());

            url = apiClient.BaseUrl.AppendPathSegment("building").AppendPathSegment("points").SetQueryParam("id", writtenPoint.Id);
            var allPoints = apiClient.GetAllAsync<Point>(url, myTestCompany).Result;
            Assert.AreEqual(1, allPoints.Count());

            url = apiClient.BaseUrl.AppendPathSegment("building").AppendPathSegment("points").SetQueryParam("id", writtenPoint.Id);
            HttpHelper.DeleteAsync(url, tokenProvider.Get(myTestCompany, scope : "panoptix.write")).Wait();
        }

        private static ITokenProvider GetTokenProvider()
        {
            return new ClientCredentialsTokenClient(
                id : "", 
                secret : "",
                endpoint : "https://ims-dev.johnsoncontrols.com/issue/oauth2/token", 
                proxy: WebProxy.GetDefaultProxy());
        }
    }
}
