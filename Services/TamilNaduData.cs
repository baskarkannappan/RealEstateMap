using System.Collections.Generic;

namespace RealEstateMap.Services;

public static class TamilNaduData
{
    public static readonly Dictionary<string, List<string>> DistrictsAndCities = new()
    {
        { "Chennai", new List<string> { "Adyar", "Anna Nagar", "T. Nagar", "Velachery", "Tambaram", "Ambattur", "Guindy", "Mylapore" } },
        { "Coimbatore", new List<string> { "RS Puram", "Gandhipuram", "Peelamedu", "Saravanampatti", "Pollachi", "Mettupalayam" } },
        { "Madurai", new List<string> { "Anna Nagar", "K.Pudur", "Sellur", "Thiruparankundram", "Tallakulam" } },
        { "Trichy", new List<string> { "Srirangam", "Thillai Nagar", "Woraiyur", "K.K. Nagar" } },
        { "Salem", new List<string> { "Fairlands", "Hasthampatti", "Suramangalam", "Ammapet" } },
        { "Tirunelveli", new List<string> { "Palayamkottai", "Melapalayam", "Pettai" } },
        { "Erode", new List<string> { "Perundurai", "Bhavani", "Gobichettipalayam" } },
        { "Vellore", new List<string> { "Katpadi", "Sathuvachari", "Gudiyatham" } },
        { "Thanjavur", new List<string> { "Pattukkottai", "Kumbakonam" } },
        { "Dindigul", new List<string> { "Palani", "Oddanchatram" } },
        { "Kanchipuram", new List<string> { "Sriperumbudur", "Chengalpattu" } },
        { "Tiruppur", new List<string> { "Avinashi", "Dharapuram", "Udumalaipettai" } },
        { "Thoothukudi", new List<string> { "Kovilpatti", "Tiruchendur" } },
        { "Karur", new List<string> { "Kulithalai", "Aravakurichi" } },
        { "Namakkal", new List<string> { "Rasipuram", "Tiruchengode" } },
        { "Cuddalore", new List<string> { "Chidambaram", "Panruti", "Virudhachalam" } },
        { "Villupuram", new List<string> { "Tindivanam", "Gingee" } },
        { "Nagapattinam", new List<string> { "Velankanni", "Vedaranyam" } },
        { "Ramanathapuram", new List<string> { "Rameswaram", "Paramakudi" } },
        { "Sivagangai", new List<string> { "Karaikudi", "Devakottai" } },
        { "Dharmapuri", new List<string> { "Palacode", "Pennagaram" } },
        { "Krishnagiri", new List<string> { "Hosur", "Denkanikottai" } },
        { "Nilgiris", new List<string> { "Ooty", "Coonoor", "Gudalur" } },
        { "Ariyalur", new List<string> { "Jayankondam", "Sendurai" } },
        { "Perambalur", new List<string> { "Veppanthattai", "Alathur" } },
        { "Pudukkottai", new List<string> { "Aranthangi", "Illuppur" } },
        { "Tenkasi", new List<string> { "Sankarankovil", "Alangulam" } },
        { "Kallakurichi", new List<string> { "Sankarapuram", "Ulundurpet" } },
        { "Mayiladuthurai", new List<string> { "Sirkali", "Kuthalam" } },
        { "Ranipet", new List<string> { "Arakkonam", "Walajapet" } },
        { "Chengalpattu", new List<string> { "Pallavaram", "Madhuranthakam" } },
        { "Tirupathur", new List<string> { "Vaniyambadi", "Ambur" } }
    };
}
