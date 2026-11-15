using Microsoft.Data.SqlClient;
namespace Library.DAL;
internal static class Db
{
    internal const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=LibraryDB;Integrated Security=True;TrustServerCertificate=True;";
    internal static SqlConnection Open()
    {
        var cn = new SqlConnection(ConnectionString);
        cn.Open();
        return cn;
    }
}
