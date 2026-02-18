namespace RealEstateMap.Api.Options;

public sealed class DataSourceOptions
{
    public const string SectionName = "DataSource";

    public bool UseDatabase { get; set; }
    public bool FallbackToFakeData { get; set; } = true;
}
