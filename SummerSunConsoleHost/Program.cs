using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using BuildingApi;
using Mono.Options;
using Ninject;

namespace SummerSun
{
    class Program
    {
        static void Main(string[] args)
        {
            string companySearch = "";
            string equipmentType = "Chiller";
            string pointRoleType = "ChillerStatus";
            int pageSize = 10;

            var commandLineParams = new OptionSet()
                .Add("c|company=", c => companySearch = c)
                .Add("e|equipmentType=", e => equipmentType = e)
                .Add("pr|pointRoleType=", pr => pointRoleType = pr)
                .Add("ps|pageSize=", ps => int.TryParse(ps, out pageSize));
            commandLineParams.Parse(args);
            IKernel kernel = new StandardKernel();
            kernel.Load(new SummerSunNinjectBindings());
            Console.WriteLine("INPUT: CompanySearch= {0} EquipmentType={1}  PointRoleType={2}  pageSize={3}", companySearch, equipmentType, pointRoleType, pageSize);

            var company = PromptToChooseCompany(kernel, companySearch);

            var sun = kernel.Get<EquipmentClient>();
            Console.WriteLine("Searching company {0} ...", company.Name);
            var equip = new Dictionary<string, Equipment>();
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                equip = sun.GetEquipmentAndPointRoles(equipmentType, company, 0, pageSize).ToDictionary(e => e.Id, e => e);
            }
            catch (HttpRequestException httpExc)
            {
                if (httpExc.Message.Contains("customer"))
                {
                    Console.WriteLine("The {0} customer does not actually exist in this environment.", company.Name);
                    return;
                }
            }
            watch.Stop();
            Console.WriteLine("Found {0} equipment with \"{1}\" in their type ({2}ms)", equip.Count(), equipmentType, watch.ElapsedMilliseconds);
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
                var pts = sun.GetPointsAndSummary(ptIds, company).ToList();
                watch.Stop();
                Console.WriteLine("Found {0} points on those equipment with \"{1}\" in role type ({2}ms)", pts.Count(), pointRoleType, watch.ElapsedMilliseconds); 
                foreach (var pt in pts)
                {
                    var ptId = pt.Id;
                    foreach (var role in equip.Values.Where(eq => eq.PointRoles != null && eq.PointRoles.Items != null).SelectMany(eq => eq.PointRoles.Items.Where(r => r.Point.Id == ptId)))
                    {
                        role2Point.Add(role.Id, pt);
                    }
                }
            }
            const string tablePattern = "|{0,-40}|{1,-40}|{2,-20}|{3,12}|{4,12}|{5,12}|";
            DrawTableLine(tablePattern);
            Console.WriteLine(tablePattern, '"' + equipmentType + '"', '"' + pointRoleType + '"', "Units", "Sample Count", "Minimum", "Maximum");
            DrawTableLine(tablePattern);

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
            DrawTableLine(tablePattern);
            Console.ReadKey();
        }

        private static void DrawTableLine(string tablePattern)
        {
            const string dashes20 = "--------------------";
            const string dashes40 = dashes20 + dashes20;
            const string dashes12 = "------------";
            Console.WriteLine(tablePattern, dashes40, dashes40, dashes20, dashes12, dashes12, dashes12);
        }

        private static Company PromptToChooseCompany(IKernel kernel, string companySearch)
        {
            var compRepo = kernel.Get<ICompanyProvider>();
            var allCompanies = compRepo.Get().ToList();
            int ci, num = 0;
            var compIndex = allCompanies.Where(c => c.Name.ToLowerInvariant().Contains(companySearch.ToLowerInvariant())).ToDictionary(c => ++num);
            if (compIndex.Count == 1)
            {
                return compIndex[1];
            }
            if (compIndex.Count == 0)
            {
                num = 0;
                compIndex = allCompanies.ToDictionary(c => ++num);
            }
            string input = null;
            while (!Int32.TryParse(input, out ci))
            {
                Console.WriteLine("Choose a company to view:");
                foreach(var num2Comp in compIndex)
                {
                    Console.WriteLine("{0,4}  {1}", num2Comp.Key, num2Comp.Value.Name);
                }
                Console.Write("Enter a company number:");
                input = Console.ReadLine();
            }
            return compIndex[ci];
        }
    }
}
