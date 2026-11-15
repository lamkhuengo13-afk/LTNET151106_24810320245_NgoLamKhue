using System.Data;
using Microsoft.Data.SqlClient;
using Library.DTO;
namespace Library.DAL;
public class DauSachDAL
{
    public DataTable GetAll()
    {
        using var cn = Db.Open();
        using var da = new SqlDataAdapter("SELECT MaSach, TenSach, TheLoai, SoLuongCon FROM dbo.DauSach ORDER BY MaSach DESC", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }
    public int Insert(DauSachDTO x)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("INSERT dbo.DauSach(TenSach,TheLoai,SoLuongCon) VALUES(@ten,@loai,@sl)", cn);
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 200).Value = x.TenSach;
        cmd.Parameters.Add("@loai", SqlDbType.NVarChar, 50).Value = (object?)x.TheLoai ?? DBNull.Value;
        cmd.Parameters.Add("@sl", SqlDbType.Int).Value = x.SoLuongCon; return cmd.ExecuteNonQuery();
    }
    public int Update(DauSachDTO x)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("UPDATE dbo.DauSach SET TenSach=@ten,TheLoai=@loai,SoLuongCon=@sl WHERE MaSach=@id", cn);
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 200).Value = x.TenSach;
        cmd.Parameters.Add("@loai", SqlDbType.NVarChar, 50).Value = (object?)x.TheLoai ?? DBNull.Value;
        cmd.Parameters.Add("@sl", SqlDbType.Int).Value = x.SoLuongCon;
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = x.MaSach; return cmd.ExecuteNonQuery();
    }
    public int Delete(int id)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("DELETE dbo.DauSach WHERE MaSach=@id", cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id; return cmd.ExecuteNonQuery();
    }
}
