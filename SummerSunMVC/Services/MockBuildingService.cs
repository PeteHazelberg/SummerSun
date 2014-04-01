using BuildingApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SummerSunMVC.Services
{
    public class MockBuildingService : IBuildingService
    {
        public IEnumerable<Company> GetCompanies()
        { 
            List<Company> companies = new List<Company>();

            Company c = new Company() { Id = "1234", Name = "JCI"};
            companies.Add(c);
            c = new Company() { Id = "5678", Name = "Autodesk"};
            companies.Add(c);
            c = new Company() { Id = "9101112", Name = "Midgard"};
            companies.Add(c);
            
            return companies;
        }

        public IEnumerable<EquipmentType> GetEquipmentTypes()
        { 
            List<EquipmentType> types = new List<EquipmentType>();

            EquipmentType type = new EquipmentType()
            {
                Name = "EletricMeter",
                Id = "EletricMeter",
                PointRoleTypes = new EntityLink[1]
            };
            type.PointRoleTypes[0] = new EntityLink() { Id = "IntervalDemand" };
            types.Add(type);

            type = new EquipmentType()
            {
                Name = "Chiller",
                Id = "Chiller",
                PointRoleTypes = new EntityLink[1]
            };
            type.PointRoleTypes[0] = new EntityLink() { Id = "Actual_kWPerTon" };
            types.Add(type);

            return types;
        }

        // Let's ingnore the company for now
        // Mocking more data as I go...
        public IEnumerable<Equipment> GetEquipmentByCompany(string equipmentType, Company company)
        {
            List<Equipment> equipmentList = new List<Equipment>();

            Equipment eq = new Equipment()
            {
                Name = "Chiller A",
                Type = new EntityLink() { Id = "Chiller" },
                Id = "1",
                PointRoles = new Page<PointRole>()
            };
            List<PointRole> l =  new List<PointRole>();
            l.Add(new PointRole(){ 
                Type = new EntityLink() { Id = "Actual_kWPerTon"},
                Point = new EntityLink() { Id = "1" },
            });
            eq.PointRoles.Items = l; 
            equipmentList.Add(eq);

            eq = new Equipment()
            {
                Name = "Chiller B",
                Type = new EntityLink() { Id = "Chiller" },
                Id = "2",
                PointRoles = new Page<PointRole>()
            };
            l = new List<PointRole>();
            l.Add(new PointRole() { 
                Type = new EntityLink() { Id = "Actual_kWPerTon" },
                Point = new EntityLink() { Id = "1" },
            });
            eq.PointRoles.Items = l;
            equipmentList.Add(eq);

            eq = new Equipment()
            {
                Name = "Eletric Meter Main office",
                Type = new EntityLink() { Id = "EletricMeter" },
                Id = "3",
                PointRoles = new Page<PointRole>()
            };
            l = new List<PointRole>();
            l.Add(new PointRole() { 
                Type = new EntityLink() { Id = "IntervalDemand" }, 
                Point = new EntityLink() { Id = "1" },
            });
            eq.PointRoles.Items = l;
            equipmentList.Add(eq);

            return equipmentList.Where(e => e.Type.Id == equipmentType);
        }

        public IEnumerable<Point> GetPointsSummary(IEnumerable<string> ids, Company c)
        {
            List<Point> pointList = new List<Point>();


            return pointList;
        }

    }
}