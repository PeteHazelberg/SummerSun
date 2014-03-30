using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SummerSunMVC.Models
{
    public class SingleCompanyPointStatusVM
    {
        public IEnumerable<SelectListItem> Companies { get; set; }
        public string SelectedCompany { get; set; }

        public IEnumerable<SelectListItem> EquipmentTypes { get; set; }
        public string SelectedEquipmentType { get; set; }
        public IEnumerable<SelectListItem> RoleTypes { get; set; }
        public string SelectedRoleType { get; set; }

        public IEnumerable<PointStatus> PointsStatus {get;set;}
    }

    // TO DO
    // Add Company and move in a seperate file... reusable for a Multi Customer view
    public class PointStatus
    {
        [Display(Name = "Name")]
        public string EquipmentName { get; set; }
        public string EquipmentId { get; set; }
        [Display(Name = "Type")]
        public string EquipmentType { get; set; }
        [Display(Name = "Role")]
        public string PointRole { get; set; }
        public string PointId { get; set; }
        [Display(Name = "Unit")]
        public string UoM { get; set; }
        [Display(Name = "Last value")]
        public string LastValue { get; set; }
    }
}