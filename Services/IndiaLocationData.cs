using System.Collections.Generic;
using System.Linq;

namespace RealEstateMap.Services
{
    public static class IndiaLocationData
    {
        public class StateInfo
        {
            public string Name { get; set; }
            public List<DistrictInfo> Districts { get; set; } = new();
        }

        public class DistrictInfo
        {
            public string Name { get; set; }
            public List<string> Cities { get; set; } = new();
        }

        public static List<StateInfo> States { get; } = new List<StateInfo>
        {
            new StateInfo
            {
                Name = "Tamil Nadu",
                Districts = new List<DistrictInfo>
                {
                    new DistrictInfo { Name = "Chennai", Cities = new List<string> { "Adyar", "Anna Nagar", "Besant Nagar", "Mylapore", "T. Nagar", "Velachery", "Tambaram", "Avadi" } },
                    new DistrictInfo { Name = "Coimbatore", Cities = new List<string> { "RS Puram", "Gandhipuram", "Peelamedu", "Pollachi" } },
                    new DistrictInfo { Name = "Madurai", Cities = new List<string> { "Anna Nagar", "K.K. Nagar", "Melur" } }
                }
            },
            new StateInfo
            {
                Name = "Karnataka",
                Districts = new List<DistrictInfo>
                {
                    new DistrictInfo { Name = "Bengaluru", Cities = new List<string> { "Indiranagar", "Koramangala", "HSR Layout", "Whitefield", "Jayanagar" } },
                    new DistrictInfo { Name = "Mysuru", Cities = new List<string> { "Gokulam", "Jayalakshmipuram", "Vidyaranyapuram" } }
                }
            },
            new StateInfo
            {
                Name = "Maharashtra",
                Districts = new List<DistrictInfo>
                {
                    new DistrictInfo { Name = "Mumbai", Cities = new List<string> { "Andheri", "Bandra", "Colaba", "Dadar", "Juhu" } },
                    new DistrictInfo { Name = "Pune", Cities = new List<string> { "Kothrud", "Hadapsar", "Baner", "Viman Nagar" } }
                }
            }
            // Additional states can be added here
        };

        public static IEnumerable<string> GetStates() => States.Select(s => s.Name);
        
        public static IEnumerable<string> GetDistricts(string stateName) => 
            States.FirstOrDefault(s => s.Name == stateName)?.Districts.Select(d => d.Name) ?? Enumerable.Empty<string>();

        public static IEnumerable<string> GetCities(string stateName, string districtName) =>
            States.FirstOrDefault(s => s.Name == stateName)?
                  .Districts.FirstOrDefault(d => d.Name == districtName)?
                  .Cities ?? Enumerable.Empty<string>();
    }
}
