namespace RealEstateMap.DAL.Configuration;

public sealed class DatabaseOptions
{
    public string ConnectionStringName { get; set; } = "RealEstateDb";
    public int DefaultCommandTimeoutSeconds { get; set; } = 30;
}
