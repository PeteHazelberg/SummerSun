using BuildingApi;
using SummerSunMVC.Models;
using SummerSunMVC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Flurl;

namespace SummerSunMVC.Controllers
{
    public class PointsController : Controller
    {
        private readonly IBuildingService _buildingService;

        public PointsController(IBuildingService service)
        {
            _buildingService = service;
        }

        public ActionResult Index(string selectedCompany, string selectedEquipmentType, string selectedRoleType)
        {
            var viewModel = new SingleCompanyPointStatusVM();
            
            // Retrieve Companies
            var companies = _buildingService.GetCompanies().ToList();
            var companieslist = companies.Select(comp => new SelectListItem
            {
                Value = comp.Id,
                Text = comp.Name
            }).ToList();
            viewModel.Companies = companieslist;
            viewModel.SelectedCompany = selectedCompany;

            // Retrieve Available Equipment Type and Role
            var types = _buildingService.GetEquipmentTypes().ToList();
            var listItem = types.Select(type => new SelectListItem
            {
                Value = type.Id,
                Text = type.Name
            }).ToList();
            viewModel.EquipmentTypes = listItem;
            viewModel.SelectedEquipmentType = selectedEquipmentType;
            
            // If an equipmentType has been selected populate the Role accordenly
            var roles = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(selectedEquipmentType))
                foreach (var item in types.First(t => t.Id == selectedEquipmentType).PointRoleTypes)
                    roles.Add(new SelectListItem { Value = item.Id, Text = item.Id });
            viewModel.RoleTypes = roles;
            viewModel.SelectedRoleType = selectedRoleType;
            
            // Retrieve data for the partial view
            var company = companies.FirstOrDefault(c => c.Id == selectedCompany);
            viewModel.PointsStatus = _retrievePointsStatus(company, selectedEquipmentType, selectedRoleType);

            return View(viewModel);
        }
  
        public ActionResult GetSupportedRoles(string selectedEquipmentType)
        {
            IEnumerable<EquipmentType> types = _buildingService.GetEquipmentTypes();
            var roles = new List<SelectListItem>();
            foreach (var item in types.First(t => t.Id == selectedEquipmentType).PointRoleTypes)
                roles.Add(new SelectListItem { Value = item.Id, Text = item.Id });   

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(SingleCompanyPointStatusVM Model, string pointId)
        {
            if (!string.IsNullOrEmpty(pointId))
            {
                ViewBag.AccessToken = _buildingService.GetAccessToken(Model.SelectedCompany);
                ViewBag.pointUrl = _buildingService.APIBaseUrl.AppendPathSegment("building/points").AppendPathSegment(pointId).ToString();
            }
            return View();
        }

        private List<PointStatus> _retrievePointsStatus(Company company, string type, string roleType)
        {
            if (company == null)
            {
                return new List<PointStatus>();
            }

            // TO DO
            // Let's assume to build a table with a single point per row
            var equipList = _buildingService.GetEquipmentByCompany(type, company);
            var viewModelMap = new Dictionary<string, PointStatus>();

            foreach (var item in equipList)
            {
                PointRole pt = item.PointRoles.Items.FirstOrDefault(r => r.Type.Id == roleType);
                // For now let's filter out equipment without the requested pointrole.
                // Not sure if we need them or not
                if (pt != null)
                {
                    var model = new PointStatus
                    {
                        EquipmentName = item.Name,
                        EquipmentId = item.Id,
                        EquipmentType = item.Type.Id,
                        PointRole = pt.Type.Id,
                        PointId = pt.Point.Id
                    };
                    viewModelMap.Add(model.PointId, model);
                }
            }

            if (viewModelMap.Count == 0)
            {
                return new List<PointStatus>();
            }

            var pointsInfo = _buildingService.GetPointsSummary(viewModelMap.Keys, company);
            foreach (var item in pointsInfo)
            {
                if (viewModelMap.ContainsKey(item.Id))
                {
                    viewModelMap[item.Id].UoM = item.Units.Id ?? item.States.Id;
                    // TO DO
                    // To be replaced with last value
                    viewModelMap[item.Id].LastValue = item.SampleSummary.MaxValue.ToString();
                }
            }

            return viewModelMap.Values.ToList();
        }
    }
}