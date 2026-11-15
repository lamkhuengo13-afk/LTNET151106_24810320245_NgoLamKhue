using System.Data;
using Library.DTO;
using Library.DAL;
namespace Library.BUS;
public class LibraryBUS
{
    private readonly DocGiaDAL readers = new();
    private readonly DauSachDAL books = new();
    private readonly PhieuMuonDAL loans = new();
    public DataTable GetReaders() => readers.GetAll();
    public DataTable GetBooks() => books.GetAll();
    public DataTable GetLoans(int? readerId, string? status) => loans.GetFiltered(readerId, status);
    public DataTable GetOverdue() => loans.GetOverdue();
    public DataTable GetGenreStats() => loans.GetGenreStats();
    public DataTable GetReaderOptions() => loans.GetReaders();
    public DataTable GetBookOptions() => loans.GetBooks();
    public void SaveReader(DocGiaDTO x) { if (string.IsNullOrWhiteSpace(x.HoTen)) throw new ArgumentException("Họ tên độc giả không được để trống."); if (x.MaDocGia == 0) readers.Insert(x); else readers.Update(x); }
    public void DeleteReader(int id) => readers.Delete(id);
    public void SaveBook(DauSachDTO x) { if (string.IsNullOrWhiteSpace(x.TenSach)) throw new ArgumentException("Tên sách không được để trống."); if (x.SoLuongCon < 0) throw new ArgumentException("Số lượng không thể âm."); if (x.MaSach == 0) books.Insert(x); else books.Update(x); }
    public void DeleteBook(int id) => books.Delete(id);
    public int CreateLoan(int readerId, DateTime due, int bookId, int quantity) { if (due.Date < DateTime.Today) throw new ArgumentException("Hạn trả phải từ hôm nay trở đi."); if (quantity <= 0) throw new ArgumentException("Số lượng mượn phải lớn hơn 0."); return loans.CreateLoan(readerId,due,bookId,quantity); }
    public void ReturnLoan(int id) => loans.ReturnLoan(id);
}
