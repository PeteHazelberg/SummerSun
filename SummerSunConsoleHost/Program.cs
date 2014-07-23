using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using BuildingApi;
using Mono.Options;
using Ninject;
using Humanizer;
using Flurl;

namespace SummerSun
{
    class Program
    {
        static void Main(string[] args)
        {
            string companySearch = "";
            string equipmentType = "";
            string pointRoleType = "";
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
            Console.WriteLine("Searching company {0} ...", company.Name);

            var equip = new Dictionary<string, Equipment>();
            var sun = kernel.Get<EquipmentClient>();
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
                }
            }

            Point point = null;

            IDictionary<string, Point> role2Point = new Dictionary<string, Point>();
            if (equip2Roles.Any())
            {
                var ptIds = (from roleList in equip2Roles.Values from role in roleList select role.Point.Id).ToList();
                var pts = new List<Point>();
                var numOfRequests = 0;
                watch.Restart();
                for (int i = 0; i < ptIds.Count; i += MaxPointIdsInQueryString)
                {
                    numOfRequests++;
                    pts.AddRange(sun.GetPointsAndSummary(ptIds.GetRange(i, Math.Min(MaxPointIdsInQueryString, ptIds.Count - i)), company));
                }
                watch.Stop();
                Console.WriteLine("Found {0} points on those equipment with \"{1}\" in role type ({2}ms across {3} HTTP GET requests)", pts.Count(), pointRoleType, watch.ElapsedMilliseconds, numOfRequests); 
                foreach (var pt in pts)
                {
                    var ptId = pt.Id;
                    foreach (var role in equip.Values.Where(eq => eq.PointRoles != null && eq.PointRoles.Items != null).SelectMany(eq => eq.PointRoles.Items.Where(r => r.Point.Id == ptId)).Where(role => !role2Point.ContainsKey(role.Id)))
                    {
                        role2Point.Add(role.Id, pt);
                    }
                }
            }
            else
            {
                Console.WriteLine("No points found on those equipment with \"{0}\" in their role type.", pointRoleType);
            }
            
            const string tablePattern = "|{0,-30}|{1,-30}|{2,15}|{3,-15}|{4,-15}|";
            DrawTableLine(tablePattern);
            Console.WriteLine(tablePattern, '"' + equipmentType + '"', '"' + pointRoleType + '"', "Newest Value", "Units", "When");
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
                            point = role2Point[role.Id];
                            var fqr = point.GetAttribute("source", "id");
                            Console.WriteLine(tablePattern + " {5}", FormatEquipmentName(equip[eId]), FormatRoleTypeName(role),
                                ExtractNewestVal(point), ExtractUnits(point), ExtractNewestTimeStamp(point), fqr);
                        }
                        else
                        {
                            Console.WriteLine(tablePattern, FormatEquipmentName(equip[eId]), FormatRoleTypeName(role), " (not found)", " (not found)", " (not found)");
                        }
                    }
                }
                else
                    Console.WriteLine(tablePattern, FormatEquipmentName(equip[eId]), "  ---", string.Empty, string.Empty, string.Empty);
            }
            DrawTableLine(tablePattern);

            //var api = kernel.Get<ApiClient>();
            //var url = api.BaseUrl.AppendPathSegments(new[] { "building", "point", point.Id, "samples" }).SetQueryParams(new 
            //{
            //    _startTime = DateTime.UtcNow.AddDays(-1).ToString("s") + "Z",
            //    _interval = "auto"
            //});
            //var samps = api.Get<Sample>(url, company);
            //foreach (var samp in samps)
            //    Console.WriteLine(samp.Timestamp);

            Console.ReadKey();
        }

        private static string FormatEquipmentName(Equipment eq)
        {
            if (eq == null || eq.Name == null) { return string.Empty; }
            return eq.Name.Truncate(30);
        }
        private static string FormatRoleTypeName(PointRole role)
        {
            if (role == null || role.Type == null || role.Type.Id == null) { return string.Empty; }
            return role.Type.Id.Humanize(LetterCasing.Title).Truncate(30);
        }
        private static string ExtractNewestTimeStamp(Point pt)
        {
            if (pt == null || pt.SampleSummary == null || pt.SampleSummary.Newest == null) { return string.Empty; }
            return pt.SampleSummary.Newest.Timestamp.Humanize();
        }
        private static string ExtractNewestVal(Point pt)
        {
            if (pt == null || pt.SampleSummary == null || pt.SampleSummary.Newest == null) {  return string.Empty; }
            return pt.SampleSummary.Newest.Value.ToString("F1");
        }
        private static string ExtractUnits(Point pt)
        {
            if (pt == null) { return string.Empty; }
            if (pt.Units == null && pt.States == null) { return string.Empty; }
            var retVal = pt.Units == null ? pt.States.Id : pt.Units.Id;
            if (retVal.StartsWith("enumset", StringComparison.InvariantCultureIgnoreCase)) { retVal = retVal.Remove(0, 7); }
            return retVal.Humanize(LetterCasing.Title).Truncate(15);
        }

        private static void DrawTableLine(string tablePattern)
        {
            const string dashes15 = "---------------";
            const string dashes30 = dashes15 + dashes15;
            Console.WriteLine(tablePattern, dashes30, dashes30, dashes15, dashes15, dashes15, dashes15);
        }

        private static Company PromptToChooseCompany(IKernel kernel, string companySearch)
        {
            var compRepo = kernel.Get<ICompanyProvider>();
            var allCompanies = new List<Company>();
            try
            {
                allCompanies = compRepo.Get().ToList();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
                Environment.Exit(1);
            }
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
                    Console.WriteLine("{0,4}  {1,-40}    Id={2}", num2Comp.Key, num2Comp.Value.Name, num2Comp.Value.Id);
                }
                Console.Write("Enter a company number:");
                input = Console.ReadLine();
            }
            return compIndex[ci];
        }

        private const int MaxPointIdsInQueryString = 50;
    }

    public class ApiClient
    {
        private readonly string baseUrl;
        private readonly ITokenProvider tokens;
        public ApiClient(ITokenProvider tokenProvider, string buildingApiUrl)
        {
            this.tokens = tokenProvider;
            this.baseUrl = buildingApiUrl;
        }

        public string BaseUrl { get { return baseUrl; } }

        public IEnumerable<T> Get<T>(Url url, Company company, int offset = 0, int limit = 200)
        {
            var resp = HttpHelper.Get<Page<T>>(company, url.ToString(), tokens);
            return (resp == null || resp.Items == null) ? new List<T>() : resp.Items;
        }
    }
}
