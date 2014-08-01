using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace BuildingApi
{
    [TestClass]
    public class EntityTest
    {
        [TestMethod]
        public void AttributesRoundTrip()
        {
            var expected = new Entity {Id = "someId", Name = "someName"};
            expected.SetAttribute("source", "id", "site:device/container.object");
            expected.SetAttribute("source", "name", "someName");
            expected.SetAttribute("metasys", "firmwareVersion", "6.1.0.1203");

            var onTheWire = HttpHelper.SerializeObject(expected);
            const string json = @"{""name"":""someName"",""attributes"":{""source"":{""attributes"":{""id"":""site:device/container.object"",""name"":""someName""}},""metasys"":{""attributes"":{""firmwareVersion"":""6.1.0.1203""}}},""id"":""someId""}";
            Assert.AreEqual(json, onTheWire);

            var actual = HttpHelper.DeserializeObject<Entity>(onTheWire);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Attributes);
            Assert.AreEqual("someId", actual.Id);
            Assert.IsNotNull("site:device/container.object", actual.GetAttribute("source", "id"));
            Assert.IsNotNull("someName", actual.GetAttribute("source", "name"));
            Assert.IsNotNull("6.1.0.1203", actual.GetAttribute("metasys", "firmwareVersion"));
        }
    }
}
