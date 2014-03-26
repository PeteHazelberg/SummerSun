using System;
using System.Globalization;
using Common.Logging;
using Microsoft.WindowsAzure;

namespace SummerSun
{
    public static class Settings
    {
        //public static string ComposeConnectionString(string version)
        //{
        //    //Log.Warn("RoleEnvironment.IsAvailable: " + RoleEnvironment.IsAvailable);
        //    //Log.Warn("RoleEnvironment.GetConfigurationSettingValue(FactDbHost): " + RoleEnvironment.GetConfigurationSettingValue("FactDbHost"));

        //    var connStr = GetSetting(settingKey: "FactDbConnectionStringTemplate",
        //                             defaultValueIfNotFound: "metadata=res://*/v((VERSION-NUMBER)).FactEntities.csdl|res://*/v((VERSION-NUMBER)).FactEntities.ssdl|res://*/v((VERSION-NUMBER)).FactEntities.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=(local);initial catalog=Advantage_Fact;User ID=AdvUser;Password=p@ssw0rd;MultipleActiveResultsets=True;&quot;",
        //                             validityCheck: (v => v.Contains("((VERSION-NUMBER))")),
        //                             warningMessageIfCheckFails: "The configuration setting for FactDbConnectionStringTemplate does not contain the string ((VERSION-NUMBER)) for us to replace with the requested version.");
        //    connStr = connStr.Replace("((VERSION-NUMBER))", version);

        //    var factDbHost = GetSetting(settingKey: "FactDbHost",
        //                                defaultValueIfNotFound: "(local)",
        //                                validityCheck: (v => !string.IsNullOrEmpty(v)),
        //                                warningMessageIfCheckFails: "May be unable to process requests because the configuration settings don't have an entry for FactDbHost.");
        //    connStr = connStr.Replace("(local)", factDbHost);
        //    //Log.Warn(connStr);
        //    return connStr;
        //}

        public static int Get(string settingKey, int defaultValueIfNotFound)
        {
            int i = -1;
            Get(settingKey,
                defaultValueIfNotFound.ToString(CultureInfo.InvariantCulture),
                str => Int32.TryParse(str, out i),
                settingKey + " in config app setting must be an integer.");
            return i;
        }

        public static string Get(string settingKey, string defaultValueIfNotFound)
        {
            return Get(settingKey, defaultValueIfNotFound, s => true, null);
        }

    public static string Get(string settingKey, string defaultValueIfNotFound, Predicate<string> validityCheck, string warningMessageIfCheckFails)
        {
            var settingValue = GetSettingFromFile(settingKey) ?? defaultValueIfNotFound;
            if (!validityCheck(settingValue) && !string.IsNullOrEmpty(warningMessageIfCheckFails))
            {
                Log.Warn(CultureInfo.InvariantCulture, m => m(warningMessageIfCheckFails));
            }
            return settingValue;
        }

        private delegate string GetSettingDelegate(string key);
        private static readonly GetSettingDelegate GetSettingFromFile = CloudConfigurationManager.GetSetting;
        private static readonly ILog Log = LogManager.GetLogger("Jci.Panoptix.HistoricalCollector.ConfigSettings");
    }
}
