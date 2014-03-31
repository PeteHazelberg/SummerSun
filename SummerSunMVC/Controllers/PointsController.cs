using BuildingApi;
using SummerSunMVC.Models;
using SummerSunMVC.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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
            viewModel.PointsStatus = _retrievePoints(company, selectedEquipmentType, selectedRoleType);

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

        public ActionResult Details(string pointId)
        { 
            return View();
        }

        private List<PointStatus> _retrievePoints(Company company, string type, string roleType)
        {
            if (company == null)
            {
                return new List<PointStatus>();
            }

            // TO DO
            // Look into mappers if needed
            var equipList = _buildingService.GetEquipmentByCompany(type, company);
            var viewModelList = new List<PointStatus>();
            foreach (var item in equipList)
            {
                PointRole pt = item.PointRoles.Items.FirstOrDefault(r => r.Type.Id == roleType);
                var model = new PointStatus
                {
                    EquipmentName = item.Name,
                    EquipmentId = item.Id,
                    EquipmentType = item.Type.Id,
                    PointRole = pt != null ? pt.Type.Id : "Not Available",
                    PointId = pt != null ? pt.Id : string.Empty
                };
                viewModelList.Add(model);
            }
            return viewModelList;
        }
    }
}