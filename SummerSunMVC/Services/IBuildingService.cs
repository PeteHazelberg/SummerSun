using BuildingApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SummerSunMVC.Services
{
    public interface IBuildingService
    {
        string APIBaseUrl { get; }
        IEnumerable<Company> GetCompanies();
        string GetAccessToken(string companyName);
        string GetAccessToken(Company c);
        IEnumerable<EquipmentType> GetEquipmentTypes();
        IEnumerable<Equipment> GetEquipmentByCompany(string equipmentType, Company company);
        IEnumerable<Point> GetPointsSummary(IEnumerable<string> ids, Company c);
    }
}