using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Jci.Panoptix.Cda.Building;
using Jci.Panoptix.Cda.Building.Storage;
using Mono.Options;
using Ninject;
using SummerSun;

namespace SummerSunConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string companyId = null;
            string equipmentType = "Chiller";
            string pointRoleType = "ChillerStatus";
            int pageSize = 10;

            var commandLineParams = new OptionSet()
                .Add("c|companyId=", c => companyId = c)
                .Add("e|equipmentType=", e => equipmentType = e)
                .Add("pr|pointRoleType=", pr => pointRoleType = pr)
                .Add("ps|pageSize=", ps => int.TryParse(ps, out pageSize));
            commandLineParams.Parse(args);
            IKernel kernel = new StandardKernel();
            kernel.Load(new BuildingStorageNinjectModule());
            kernel.Load(new NinjectBindings());

            if (companyId == null)
            {
                companyId = PromptToChooseCompany(kernel, companyId);
            }
            Console.WriteLine("Company={0}  EquipmentType={1}  PointRoleType={2}  pageSize={3}", companyId, equipmentType, pointRoleType, pageSize);

            var sun = kernel.Get<SummaryBuilder>();
            Console.WriteLine("Searching...");
            var watch = new Stopwatch();
            watch.Start();
            var equip = sun.GetEquipmentAndPointRoles(ToCustomerId(companyId), equipmentType, null, 0, pageSize).ToDictionary(e => e.Id, e => e);
            watch.Stop();
            Console.WriteLine("Found {0} equipment ({1}ms)", equip.Count(), watch.ElapsedMilliseconds);
            var equip2PointId = new Dictionary<string, string>();
            var pointId2Equip = new Dictionary<string, string>();
            foreach (var e in equip.Values)
            {
                foreach (var r in e.PointRoles.Items.Where(r => String.Equals(r.Role.Id, pointRoleType, StringComparison.InvariantCultureIgnoreCase)))
                {
                    equip2PointId.Add(e.Id, r.Point.Id);
                    pointId2Equip.Add(r.Point.Id, e.Id);
                }
            }
            watch.Restart();
            var equip2Point = sun.GetPoints(ToCustomerId(companyId), equip2PointId.Values).ToDictionary(k => pointId2Equip[k.Id], point => point);
            watch.Stop();
            Console.WriteLine("Found {0} points ({1}ms)", equip2Point.Count(), watch.ElapsedMilliseconds);
            const string tablePattern = "|{0,-20}|{1,-12}|{2,12}|{3,12}|{4,12}|";
            const string dashes20 = "--------------------";
            const string dashes12 = "------------";
            Console.WriteLine(tablePattern, dashes20, dashes12, dashes12, dashes12, dashes12);
            Console.WriteLine(tablePattern, equipmentType, "Units", "#", "Minimum", "Maximum");
            Console.WriteLine(tablePattern, dashes20, dashes12, dashes12, dashes12, dashes12);
            foreach (var id in equip.Keys)
            {
                if (equip2PointId.ContainsKey(id))
                {
                    var pt = equip2Point[id];
                    Console.WriteLine(tablePattern, equip[id].Name, pt.Units.Id ?? pt.States.Id,
                        pt.SampleSummary.Count, pt.SampleSummary.MinValue, pt.SampleSummary.MaxValue);
                }
                else
                    Console.WriteLine(tablePattern, equip[id].Name, string.Empty, string.Empty, string.Empty, string.Empty);
            }
            Console.WriteLine(tablePattern, dashes20, dashes12, dashes12, dashes12, dashes12);
            Console.ReadKey();
        }

        private static string PromptToChooseCompany(IKernel kernel, string companyId)
        {
            var compRepo = kernel.Get<ICompanyRepository>();
            var companies = compRepo.Get().ToList();
            var compIndex = new Dictionary<int, Company>(companies.Count());
            int ci;
            for (var i = 1; i < companies.Count(); i++)
            {
                compIndex.Add(i, companies[i]);
            }
            string input = null;
            while (!Int32.TryParse(input, out ci))
            {
                Console.WriteLine("Choose a company to view:");
                for (var i = 1; i < companies.Count(); i++)
                {
                    Console.WriteLine("{0,4}  {1}", i, companies[i].Name);
                }
                input = Console.ReadLine();
            }
            companyId = compIndex[ci].Id;
            return companyId;
        }

        public static Guid ToCustomerId(string companyId)
        {
            var data = companyId.Replace("_", "/").Replace("-", "+") + "==";
            var id = Convert.FromBase64String(data);
            return new Guid(id);
        }
    }
}
