using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SummerSunMVC.Models
{
    public abstract class SingleCompanyEquipmentTypeVM
    {
        public IEnumerable<SelectListItem> Companies { get; set; }
        public string SelectedCompany { get; set; }

        public IEnumerable<SelectListItem> EquipmentTypes { get; set; }
        public string SelectedEquipmentType { get; set; }
    }
    public class EquipmentReportViewModel : SingleCompanyEquipmentTypeVM
    {
        public IEnumerable<EquipmentAndPointsViewModel> Equipment { get; set; }
    }
    public class EquipmentPointRoleViewModel : EquipmentReportViewModel
    {
        public IEnumerable<SelectListItem> RoleTypes { get; set; }
        public string SelectedRoleType { get; set; }
    }
    public class EquipmentAndPointsViewModel
    {
        [Display(Name = "Name")]
        public string EquipmentName { get; set; }
        public string EquipmentId { get; set; }
        [Display(Name = "Type")]
        public string EquipmentType { get; set; }
        public IEnumerable<PointViewModel> PointsStatus { get; set; }

    }
    public class PointViewModel
    {
        [Display(Name = "Role")]
        public string PointRole { get; set; }
        public string PointId { get; set; }
        [Display(Name = "Unit")]
        public string UoM { get; set; }
        [Display(Name = "Last value")]
        public string LastValue { get; set; }
    }

}