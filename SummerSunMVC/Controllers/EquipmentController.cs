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
    public class EquipmentController : Controller
    {
        private readonly IBuildingService _buildingService;

        public EquipmentController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }
        //
        // GET: /Equipment/
        public ActionResult Index(string selectedCompany, string selectedEquipmentType)
        {
            // TO DO
            // Refactor to reuse PointController logic for companies and types
            var viewModel = new EquipmentReportViewModel();

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
            
            // Retrieve data for the partial view
            var company = companies.FirstOrDefault(c => c.Id == selectedCompany);
            viewModel.Equipment = _retrievePointsStatus(company, selectedEquipmentType);
            
            return View(viewModel);
        }

        private IEnumerable<EquipmentAndPointsViewModel> _retrievePointsStatus(Company company, string type)
        {
            var eqListVM = new List<EquipmentAndPointsViewModel>();
            if (company == null)
            {
                return eqListVM;
            }

            var equipList = _buildingService.GetEquipmentByCompany(type, company);
            var equip2PointsMap = new Dictionary<string,Dictionary<string, PointViewModel>>();

            foreach (var item in equipList)
            {
                var pointsMap = new Dictionary<string, PointViewModel>();
                // Does the equipment API support paging ??
                foreach (var ptr in item.PointRoles.Items)
	            {
		            var model = new PointViewModel
                    {
                        PointRole = ptr.Type.Id,
                        PointId = ptr.Point.Id
                    };
                    pointsMap.Add(ptr.Point.Id, model);
	            }
                equip2PointsMap.Add(item.Id, pointsMap);
            }

            if (equip2PointsMap.Count == 0)
            {
                return eqListVM;
            }
            // Get all point ids across equipment
            var pointIdsList = new List<string>();
            foreach (var equip in equip2PointsMap)
                pointIdsList.AddRange(equip.Value.Keys.ToList());
            
            var pointsInfo = _buildingService.GetPointsSummary(pointIdsList, company);
            // ?? Linq here ??
            foreach (var equip in equip2PointsMap)
            {
                foreach (var item in pointsInfo)
                {
                    var pointStatus = equip.Value.Select(pts => pts.Key == item.Id);
                    if (equip.Value.ContainsKey(item.Id))
                    {
                        equip.Value[item.Id].UoM = item.Units.Id ?? "-";
                        equip.Value[item.Id].LastValue = string.Format("{0:0.##}",item.SampleSummary.MaxValue);
                    }
                }
            }
            // build the final viewmodel
            foreach (var item in equipList)
            {
                // Skip equipment without points
                if (item.PointRoles.Items.Count() > 0)
                {
                    var eq = new EquipmentAndPointsViewModel()
                    {
                        EquipmentName = item.Name,
                        EquipmentId = item.Id,
                        EquipmentType = item.Name
                    };
                    if (equip2PointsMap.ContainsKey(item.Id))
                        eq.PointsStatus = equip2PointsMap[item.Id].Values.ToList();
                    eqListVM.Add(eq);
                } 
            }
            return eqListVM;
        }

	}
}