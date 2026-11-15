using System.Data;
using Microsoft.Data.SqlClient;
namespace Library.DAL;
public class PhieuMuonDAL
{
    public DataTable GetFiltered(int? docGiaId, string? status)
    {
        using var cn = Db.Open();
        const string sql = @"SELECT p.MaPhieu,p.MaDocGia,d.HoTen,p.NgayMuon,p.HanTra,
            CASE WHEN p.TrangThai=N'Đang mượn' AND p.HanTra < GETDATE() THEN N'Quá hạn' ELSE p.TrangThai END AS TrangThai
            FROM dbo.PhieuMuon p JOIN dbo.DocGia d ON d.MaDocGia=p.MaDocGia
            WHERE (@dg IS NULL OR p.MaDocGia=@dg)
              AND (@tt IS NULL OR (CASE WHEN p.TrangThai=N'Đang mượn' AND p.HanTra < GETDATE() THEN N'Quá hạn' ELSE p.TrangThai END)=@tt)
            ORDER BY p.NgayMuon DESC";
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.Add("@dg", SqlDbType.Int).Value = (object?)docGiaId ?? DBNull.Value;
        cmd.Parameters.Add("@tt", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(status) ? DBNull.Value : status;
        using var da = new SqlDataAdapter(cmd); var dt = new DataTable(); da.Fill(dt); return dt;
    }
    public DataTable GetOverdue()
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand(@"SELECT p.MaPhieu,d.HoTen,p.NgayMuon,p.HanTra,
            DATEDIFF(DAY,p.HanTra,GETDATE()) AS SoNgayQuaHan
            FROM dbo.PhieuMuon p JOIN dbo.DocGia d ON d.MaDocGia=p.MaDocGia
            WHERE p.HanTra < GETDATE() AND p.TrangThai <> N'Đã trả'
            ORDER BY p.HanTra", cn);
        using var rd = cmd.ExecuteReader(); var dt = new DataTable(); dt.Load(rd); return dt;
    }
    public DataTable GetReaders()
    {
        using var cn = Db.Open(); using var da = new SqlDataAdapter("SELECT MaDocGia,HoTen FROM dbo.DocGia ORDER BY HoTen", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }
    public int CreateLoan(int readerId, DateTime due, int bookId, int quantity)
    {
        using var cn = Db.Open(); using var tx = cn.BeginTransaction();
        try
        {
            using (var check = new SqlCommand("SELECT NgayHetHanThe FROM dbo.DocGia WHERE MaDocGia=@id", cn, tx))
            {
                check.Parameters.Add("@id", SqlDbType.Int).Value = readerId;
                var expiry = check.ExecuteScalar();
                if (expiry == null || expiry == DBNull.Value || Convert.ToDateTime(expiry).Date < DateTime.Today)
                    throw new InvalidOperationException("Thẻ độc giả đã hết hạn hoặc không tồn tại.");
            }
            using (var stock = new SqlCommand("UPDATE dbo.DauSach SET SoLuongCon=SoLuongCon-@sl WHERE MaSach=@sach AND SoLuongCon>=@sl", cn, tx))
            {
                stock.Parameters.Add("@sl", SqlDbType.Int).Value = quantity;
                stock.Parameters.Add("@sach", SqlDbType.Int).Value = bookId;
                if (stock.ExecuteNonQuery() != 1) throw new InvalidOperationException("Sách không tồn tại hoặc số lượng còn không đủ.");
            }
            int id;
            using (var p = new SqlCommand("INSERT dbo.PhieuMuon(MaDocGia,HanTra,TrangThai) VALUES(@dg,@han,N'Đang mượn'); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx))
            {
                p.Parameters.Add("@dg", SqlDbType.Int).Value = readerId;
                p.Parameters.Add("@han", SqlDbType.DateTime).Value = due;
                id = (int)p.ExecuteScalar()!;
            }
            using (var ct = new SqlCommand("INSERT dbo.ChiTietMuon(MaPhieu,MaSach,SoLuong) VALUES(@p,@s,@sl)", cn, tx))
            {
                ct.Parameters.Add("@p", SqlDbType.Int).Value = id;
                ct.Parameters.Add("@s", SqlDbType.Int).Value = bookId;
                ct.Parameters.Add("@sl", SqlDbType.Int).Value = quantity;
                ct.ExecuteNonQuery();
            }
            tx.Commit(); return id;
        }
        catch { tx.Rollback(); throw; }
    }
    public void ReturnLoan(int loanId)
    {
        using var cn = Db.Open(); using var tx = cn.BeginTransaction();
        try
        {
            using (var update = new SqlCommand("UPDATE dbo.PhieuMuon SET TrangThai=N'Đã trả' WHERE MaPhieu=@id AND TrangThai<>N'Đã trả'", cn, tx))
            {
                update.Parameters.Add("@id", SqlDbType.Int).Value = loanId;
                if (update.ExecuteNonQuery() != 1) throw new InvalidOperationException("Phiếu không tồn tại hoặc đã được trả trước đó.");
            }
            using (var restore = new SqlCommand(@"UPDATE s SET s.SoLuongCon=s.SoLuongCon+c.SoLuong
                FROM dbo.DauSach s JOIN dbo.ChiTietMuon c ON c.MaSach=s.MaSach WHERE c.MaPhieu=@id", cn, tx))
            {
                restore.Parameters.Add("@id", SqlDbType.Int).Value = loanId; restore.ExecuteNonQuery();
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }
    public DataTable GetGenreStats()
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand(@"SELECT s.TheLoai, SUM(c.SoLuong) AS TongLuotMuon
            FROM dbo.ChiTietMuon c JOIN dbo.DauSach s ON s.MaSach=c.MaSach
            GROUP BY s.TheLoai ORDER BY TongLuotMuon DESC", cn);
        using var rd = cmd.ExecuteReader(); var dt = new DataTable(); dt.Load(rd); return dt;
    }
    public DataTable GetBooks()
    {
        using var cn = Db.Open(); using var da = new SqlDataAdapter("SELECT MaSach,TenSach,SoLuongCon FROM dbo.DauSach WHERE SoLuongCon>0 ORDER BY TenSach", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }
}
