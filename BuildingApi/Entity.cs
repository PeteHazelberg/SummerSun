using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace BuildingApi
{
    public class Entity : EntityLink
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public JObject Attributes { get; set; }
        
        /// <summary>
        /// Extracts an individual attribute value out of the relatively complex Attributes JSON structure.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetAttribute(string @group, string name)
        {
            if (Attributes == null
                || Attributes[@group] == null
                || Attributes[@group]["attributes"] == null
                || Attributes[@group]["attributes"][name] == null) return string.Empty;

            return Attributes[@group]["attributes"].Value<string>(name);
        }

        /// <summary>
        /// Updates the Attributes JSON structure so that POST-ing this entity will include an update to this user-defined attribute.
        /// </summary>
        /// <param name="group">the name of the group of attribute (use camel-casing here, as the server will camel-case when retrieving the JSON later) containing the attribute to be updated</param>
        /// <param name="name">the attribute name (use camel-casing here, as the server will camel-case when retrieving the JSON later) </param>
        /// <param name="value"></param>
        public void SetAttribute(string @group, string name, string value)
        {
            if (Attributes == null)
                Attributes = new JObject();

            if (((IDictionary<string, JToken>)Attributes).ContainsKey(@group))
            {
                Attributes[@group]["attributes"][name] = value;
                return;
            }
            var dict = new Dictionary<string, string>(); // case insensitive comparer);
            dict[name] = value;
            var fixedContainer = new Dictionary<string, JObject>();
            fixedContainer["attributes"] = JObject.FromObject(dict);
            Attributes[@group] = JObject.FromObject(fixedContainer);
        }
    }
}
