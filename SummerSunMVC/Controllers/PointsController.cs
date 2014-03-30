using BuildingApi;
using SummerSunMVC.Models;
using SummerSunMVC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SummerSunMVC.Controllers
{
    public class PointsController : Controller
    {
        private IBuildingService _buildingService = null;

        public PointsController(IBuildingService service)
        {
            _buildingService = service;
        }

        public ActionResult Index(string selectedCompany, string selectedEquipmentType, string selectedRoleType)
        {
            SingleCompanyPointStatusVM viewModel = new SingleCompanyPointStatusVM();
            
            // Retrieve Companies
            IEnumerable<Company> companies = _buildingService.GetCompanies();
            List<SelectListItem> companieslist = new List<SelectListItem>();
            foreach (var item in companies)
                companieslist.Add(new SelectListItem { Value = item.Id, Text = item.Name });
            viewModel.Companies = companieslist;
            viewModel.SelectedCompany = selectedCompany;

            // Retrieve Available Equipment Type and Role
            IEnumerable<EquipmentType> types = _buildingService.GetEquipmentTypes();
            List<SelectListItem> listItem = new List<SelectListItem>();
            foreach (var item in types)
                listItem.Add(new SelectListItem { Value = item.Id, Text = item.Name });
            viewModel.EquipmentTypes = listItem;
            viewModel.SelectedEquipmentType = selectedEquipmentType;
            
            // If an equipmentType has been selected populate the Role accordenly
            List<SelectListItem> roles = new List<SelectListItem>();
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
            List<SelectListItem> roles = new List<SelectListItem>();
            
            foreach (var item in types.First(t => t.Id == selectedEquipmentType).PointRoleTypes)
             roles.Add(new SelectListItem {Value = item.Id, Text = item.Id});   
            
            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(string pointId)
        { 
            return View();
        }

        private List<PointStatus> _retrievePoints(Company company, string type, string roleType)
        {
            List<PointStatus> viewModelList = new List<PointStatus>();
            if (company != null)
            {
                // TO DO
                // Look into mappers if needed
                IEnumerable<Equipment> equipList = _buildingService.GetEquipmentByCompany(type, company);

                foreach (var item in equipList)
                {
                    PointRole pt = item.PointRoles.Items.FirstOrDefault(r => r.Type.Id == roleType);
                    PointStatus model = new PointStatus()
                    {
                        EquipmentName = item.Name,
                        EquipmentId = item.Id,
                        EquipmentType = item.Type.Id,
                        PointRole = pt != null ? pt.Type.Id : "Not Available",
                        PointId = pt != null ? pt.Id : string.Empty
                    };
                    viewModelList.Add(model);
                }
            }
            return viewModelList;
        }
    }
}