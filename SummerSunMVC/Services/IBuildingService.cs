using BuildingApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SummerSunMVC.Services
{
    public interface IBuildingService
    {
        IEnumerable<Company> GetCompanies();
        IEnumerable<EquipmentType> GetEquipmentTypes();
        IEnumerable<Equipment> GetEquipmentByCompany(string equipmentType, Company company);

        IEnumerable<Point> GetPointsSummary(IEnumerable<string> ids, Company c);
    }
}