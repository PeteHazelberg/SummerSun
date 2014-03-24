using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Options;
using Ninject;
using SummerSun;
using SummerSun.Api;

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
            kernel.Load(new Jci.Panoptix.Cda.Building.BuildingStorageNinjectModule());
            kernel.Load(new SummerSunNinjectBindings());

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
            var equip2Roles = new Dictionary<string, IList<PointRole>>();
            var pointId2Equip = new Dictionary<string, string>();
            foreach (var e in equip.Values)
            {
                foreach (var r in e.PointRoles.Items.Where(r => r.Type.Id.ToLowerInvariant().Contains(pointRoleType.ToLowerInvariant())))
                {
                    if (equip2Roles.ContainsKey(e.Id))
                     {
                         var roleList = equip2Roles[e.Id];
                         roleList.Add(r);
                         equip2Roles[e.Id] = roleList;
                    }                       
                    else
                    {
                        equip2Roles.Add(e.Id, new List<PointRole> { r });
                    }
                    pointId2Equip.Add(r.Point.Id, e.Id);
                }
            }
            IDictionary<string, Point> role2Point = new Dictionary<string, Point>();
            if (equip2Roles.Any())
            {
                var ptIds = new HashSet<string>((from roleList in equip2Roles.Values from role in roleList select role.Point.Id));
                watch.Restart();
                var pts = sun.GetPoints(ToCustomerId(companyId), ptIds).ToList();
                watch.Stop();
                Console.WriteLine("Found {0} points ({1}ms)", pts.Count(), watch.ElapsedMilliseconds); 
                foreach (var pt in pts)
                {
                    var ptId = pt.Id;
                    foreach (var eq in equip.Values)
                    {
                        if (eq != null && eq.PointRoles != null && eq.PointRoles.Items != null)
                        {
                            foreach (var role in eq.PointRoles.Items.Where(r => r.Point.Id == ptId))
                            {
                                role2Point.Add(role.Id, pt); 
                            }
                        }
                    }
                };
            }
            const string tablePattern = "|{0,-40}|{1,-40}|{2,-12}|{3,12}|{4,12}|{5,12}|";
            const string dashes20 = "--------------------";
            const string dashes40 = dashes20 + dashes20;
            const string dashes12 = "------------";
            Console.WriteLine(tablePattern, dashes40, dashes40, dashes12, dashes12, dashes12, dashes12);
            Console.WriteLine(tablePattern, equipmentType, pointRoleType, "Units", "Sample Count", "Minimum", "Maximum");
            Console.WriteLine(tablePattern, dashes40, dashes40, dashes12, dashes12, dashes12, dashes12); 
            foreach (var eId in equip.Keys)
            {
                if (equip2Roles.ContainsKey(eId))
                {
                    var roles = equip2Roles[eId];
                    foreach (var role in roles)
                    {
                        if (role2Point.ContainsKey(role.Id))
                        {
                            var pt = role2Point[role.Id];
                            Console.WriteLine(tablePattern, equip[eId].Name, role.Type.Id, pt.Units.Id ?? pt.States.Id,
                                pt.SampleSummary.Count, pt.SampleSummary.MinValue.ToString("F1"), pt.SampleSummary.MaxValue.ToString("F1"));
                        }
                        else
                        {
                            Console.WriteLine(tablePattern, equip[eId].Name, role.Id, "(not found)", "(not found)", "(not found)", "(not found)");
                        }
                    }
                }
                else
                    Console.WriteLine(tablePattern, equip[eId].Name, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            }
            Console.WriteLine(tablePattern, dashes40, dashes40, dashes12, dashes12, dashes12, dashes12); 
            Console.ReadKey();
        }

        private static string PromptToChooseCompany(IKernel kernel, string companyId)
        {
            var compRepo = kernel.Get<Jci.Panoptix.Cda.Building.Storage.ICompanyRepository>();
            var companies = compRepo.Get().ToList();
            var compIndex = new Dictionary<int, Jci.Panoptix.Cda.Building.Company>(companies.Count());
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
