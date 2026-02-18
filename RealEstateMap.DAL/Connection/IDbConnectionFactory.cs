using System.Data;

namespace RealEstateMap.DAL.Connection;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
