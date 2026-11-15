using System.Data;
using Microsoft.Data.SqlClient;
using Library.DTO;
namespace Library.DAL;
public class DocGiaDAL
{
    public DataTable GetAll()
    {
        using var cn = Db.Open();
        using var da = new SqlDataAdapter("SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM dbo.DocGia ORDER BY MaDocGia DESC", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }
    public int Insert(DocGiaDTO x)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("INSERT dbo.DocGia(HoTen,Email,NgayHetHanThe) VALUES(@ten,@email,@het);", cn);
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = x.HoTen;
        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = (object?)x.Email ?? DBNull.Value;
        cmd.Parameters.Add("@het", SqlDbType.DateTime).Value = (object?)x.NgayHetHanThe ?? DBNull.Value;
        return cmd.ExecuteNonQuery();
    }
    public int Update(DocGiaDTO x)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("UPDATE dbo.DocGia SET HoTen=@ten, Email=@email, NgayHetHanThe=@het WHERE MaDocGia=@id", cn);
        cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 100).Value = x.HoTen;
        cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = (object?)x.Email ?? DBNull.Value;
        cmd.Parameters.Add("@het", SqlDbType.DateTime).Value = (object?)x.NgayHetHanThe ?? DBNull.Value;
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = x.MaDocGia;
        return cmd.ExecuteNonQuery();
    }
    public int Delete(int id)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("DELETE dbo.DocGia WHERE MaDocGia=@id", cn);
        cmd.Parameters.Add("@id", SqlDbType.Int).Value = id; return cmd.ExecuteNonQuery();
    }
}
