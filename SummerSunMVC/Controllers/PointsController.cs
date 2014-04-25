using BuildingApi;
using SummerSunMVC.Models;
using SummerSunMVC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Flurl;
using System;

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
            var viewModel = new EquipmentPointRoleViewModel();
            
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
            viewModel.Equipment = _retrievePointsStatus(company, selectedEquipmentType, selectedRoleType);

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

        public ActionResult Details(EquipmentPointRoleViewModel Model, string pointId)
        {
            if (!string.IsNullOrEmpty(pointId))
            {
                ViewBag.AccessToken = _buildingService.GetAccessToken(Model.SelectedCompany);
                ViewBag.pointUrl = _buildingService.APIBaseUrl.AppendPathSegment("building/points").AppendPathSegment(pointId).ToString();
            }
            return View();
        }

        private List<EquipmentAndPointsViewModel> _retrievePointsStatus(Company company, string type, string roleType)
        {
            if (company == null)
            {
                return new List<EquipmentAndPointsViewModel>();
            }

            // TO DO
            // Let's assume to build a table with a single point per row
            var equipList = _buildingService.GetEquipmentByCompany(type, company);
            var viewModelMap = new Dictionary<string, EquipmentAndPointsViewModel>();

            foreach (var item in equipList)
            {
                PointRole pt = item.PointRoles.Items.FirstOrDefault(r => r.Type.Id == roleType);
                // For now let's filter out equipment without the requested pointrole.
                // Not sure if we need them or not
                if (pt != null)
                {
                    var model = new EquipmentAndPointsViewModel
                    {
                        EquipmentName = item.Name,
                        EquipmentId = item.Id,
                        EquipmentType = item.Type.Id,
                    };
                    var list = new List<PointViewModel>();
                    var m = new PointViewModel() { PointRole = pt.Type.Id, PointId = pt.Point.Id };
                    list.Add(m);
                    model.PointsStatus = list;
                    viewModelMap.Add(model.PointsStatus.First().PointId, model);
                }
            }

            if (viewModelMap.Count == 0)
            {
                return new List<EquipmentAndPointsViewModel>();
            }

            var pointsInfo = _buildingService.GetPointsSummary(viewModelMap.Keys, company);
            foreach (var item in pointsInfo)
            {
                if (viewModelMap.ContainsKey(item.Id) && viewModelMap[item.Id].PointsStatus.Count() > 0)
                {
                    viewModelMap[item.Id].PointsStatus.First().UoM = item.Units.Id ?? item.States.Id;
                    // At this point GetPointsSummary does not return the nested Newest field
                    // To be improved
                    if (item.SampleSummary.Newest != null)
                    {
                        viewModelMap[item.Id].PointsStatus.First().LastValue = string.Format("{0:0.##}", item.SampleSummary.Newest.val);
                        viewModelMap[item.Id].PointsStatus.First().TimeStampLastValue = DateTime.ParseExact(item.SampleSummary.Newest.ts, "u", System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }

            return viewModelMap.Values.ToList();
        }
    }
}