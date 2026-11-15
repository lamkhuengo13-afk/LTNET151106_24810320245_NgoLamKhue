using System.Data;
using Library.BUS;
using Library.DTO;

namespace Library.GUI;
public class MainForm : Form
{
    private readonly LibraryBUS bus = new();
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView readersGrid = Grid(), booksGrid = Grid(), loansGrid = Grid(), overdueGrid = Grid(), statsGrid = Grid();
    private readonly TextBox readerName = new(), readerEmail = new(), bookName = new(), bookGenre = new();
    private readonly DateTimePicker readerExpiry = new() { Format = DateTimePickerFormat.Short };
    private readonly NumericUpDown bookStock = new() { Minimum = 0, Maximum = 100000 };
    private readonly ComboBox readerFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
    private readonly ComboBox statusFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly ComboBox loanReader = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
    private readonly ComboBox loanBook = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240 };
    private readonly DateTimePicker dueDate = new() { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(14) };
    private readonly NumericUpDown loanQty = new() { Minimum = 1, Maximum = 100, Value = 1 };
    private int selectedReader, selectedBook;

    public MainForm()
    {
        Text = "LIBRARY MANAGEMENT SYSTEM - BAI TAP 6.6";
        Width = 1180; Height = 760; StartPosition = FormStartPosition.CenterScreen;
        Controls.Add(tabs);
        BuildReadersTab(); BuildBooksTab(); BuildLoansTab(); BuildOverdueTab(); BuildStatsTab();
        LoadAll();
    }
    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
    private static TabPage Page(string title) => new(title);
    private static FlowLayoutPanel Bar() => new() { Dock = DockStyle.Top, Height = 82, Padding = new Padding(8), AutoSize = false, WrapContents = true };
    private static Button Btn(string text, Action action) { var b = new Button { Text = text, AutoSize = true, Height = 32, Margin = new Padding(4) }; b.Click += (_,_) => action(); return b; }
    private static void AddField(FlowLayoutPanel p, string label, Control c) { p.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(3,8,0,0) }); c.Width = Math.Max(c.Width,130); p.Controls.Add(c); }
    private void BuildReadersTab()
    {
        var page=Page("Độc giả"); var bar=Bar();
        AddField(bar,"Họ tên",readerName); AddField(bar,"Email",readerEmail); AddField(bar,"Hết hạn thẻ",readerExpiry);
        bar.Controls.Add(Btn("Thêm",()=>SaveReader(false))); bar.Controls.Add(Btn("Sửa dòng chọn",()=>SaveReader(true))); bar.Controls.Add(Btn("Xóa",DeleteReader)); bar.Controls.Add(Btn("Làm mới",()=>RefreshReaders()));
        readersGrid.SelectionChanged += (_,_) => { if(readersGrid.CurrentRow?.DataBoundItem is DataRowView r) { selectedReader=Convert.ToInt32(r["MaDocGia"]); readerName.Text=Convert.ToString(r["HoTen"]); readerEmail.Text=Convert.ToString(r["Email"]); if(r["NgayHetHanThe"]!=DBNull.Value) readerExpiry.Value=Convert.ToDateTime(r["NgayHetHanThe"]); } };
        page.Controls.Add(readersGrid); page.Controls.Add(bar); tabs.TabPages.Add(page);
    }
    private void BuildBooksTab()
    {
        var page=Page("Đầu sách"); var bar=Bar();
        AddField(bar,"Tên sách",bookName); AddField(bar,"Thể loại",bookGenre); AddField(bar,"Số lượng",bookStock);
        bar.Controls.Add(Btn("Thêm",()=>SaveBook(false))); bar.Controls.Add(Btn("Sửa dòng chọn",()=>SaveBook(true))); bar.Controls.Add(Btn("Xóa",DeleteBook)); bar.Controls.Add(Btn("Làm mới",()=>RefreshBooks()));
        booksGrid.SelectionChanged += (_,_) => { if(booksGrid.CurrentRow?.DataBoundItem is DataRowView r) { selectedBook=Convert.ToInt32(r["MaSach"]); bookName.Text=Convert.ToString(r["TenSach"]); bookGenre.Text=Convert.ToString(r["TheLoai"]); bookStock.Value=Math.Max(0,Convert.ToInt32(r["SoLuongCon"])); } };
        page.Controls.Add(booksGrid); page.Controls.Add(bar); tabs.TabPages.Add(page);
    }
    private void BuildLoansTab()
    {
        var page=Page("Mượn / Trả sách"); var bar=Bar();
        AddField(bar,"Độc giả",loanReader); AddField(bar,"Đầu sách",loanBook); AddField(bar,"Số lượng",loanQty); AddField(bar,"Hạn trả",dueDate);
        bar.Controls.Add(Btn("Lập phiếu mượn",CreateLoan)); bar.Controls.Add(Btn("Trả phiếu đang chọn",ReturnLoan));
        AddField(bar,"Lọc độc giả",readerFilter); AddField(bar,"Trạng thái",statusFilter);
        bar.Controls.Add(Btn("Lọc",RefreshLoans)); bar.Controls.Add(Btn("Tất cả",()=>{readerFilter.SelectedIndex=0;statusFilter.SelectedIndex=0;RefreshLoans();}));
        loansGrid.SelectionChanged += (_,_) => { };
        page.Controls.Add(loansGrid); page.Controls.Add(bar); tabs.TabPages.Add(page);
    }
    private void BuildOverdueTab() { var p=Page("Cảnh báo quá hạn"); p.Controls.Add(overdueGrid); p.Controls.Add(new Label{Text="Các phiếu đã quá hạn và chưa được trả",Dock=DockStyle.Top,Height=34,TextAlign=ContentAlignment.MiddleLeft}); tabs.TabPages.Add(p); }
    private void BuildStatsTab() { var p=Page("Thống kê theo thể loại"); p.Controls.Add(statsGrid); p.Controls.Add(Btn("Cập nhật thống kê",RefreshStats).AlsoDockTop()); tabs.TabPages.Add(p); }
    private void LoadAll() { try { RefreshReaders(); RefreshBooks(); LoadLoanOptions(); RefreshLoans(); RefreshOverdue(); RefreshStats(); } catch(Exception ex) { ShowError(ex); } }
    private void RefreshReaders() => readersGrid.DataSource=bus.GetReaders();
    private void RefreshBooks() => booksGrid.DataSource=bus.GetBooks();
    private void LoadLoanOptions()
    {
        var rd=bus.GetReaderOptions(); loanReader.DataSource=rd.Copy(); loanReader.DisplayMember="HoTen"; loanReader.ValueMember="MaDocGia";
        var filterTable = rd.Clone();
        var allRow = filterTable.NewRow(); allRow["MaDocGia"] = DBNull.Value; allRow["HoTen"] = "Tất cả độc giả"; filterTable.Rows.Add(allRow);
        foreach (DataRow row in rd.Rows) filterTable.ImportRow(row);
        readerFilter.DataSource = filterTable; readerFilter.DisplayMember = "HoTen"; readerFilter.ValueMember = "MaDocGia"; readerFilter.SelectedIndex = 0;
        var bk=bus.GetBookOptions(); loanBook.DataSource=bk; loanBook.DisplayMember="TenSach"; loanBook.ValueMember="MaSach";
        statusFilter.Items.Clear(); statusFilter.Items.AddRange(new object[]{"Tất cả","Đang mượn","Đã trả","Quá hạn"}); statusFilter.SelectedIndex=0;
    }
    private void RefreshLoans() { int? id = readerFilter.SelectedIndex <= 0 || readerFilter.SelectedValue == DBNull.Value ? null : Convert.ToInt32(readerFilter.SelectedValue); string? st = statusFilter.SelectedIndex <= 0 ? null : Convert.ToString(statusFilter.SelectedItem); loansGrid.DataSource = bus.GetLoans(id, st); }
    private void RefreshOverdue() => overdueGrid.DataSource=bus.GetOverdue();
    private void RefreshStats() => statsGrid.DataSource=bus.GetGenreStats();
    private void SaveReader(bool edit) { try { bus.SaveReader(new DocGiaDTO{MaDocGia=edit?selectedReader:0,HoTen=readerName.Text.Trim(),Email=string.IsNullOrWhiteSpace(readerEmail.Text)?null:readerEmail.Text.Trim(),NgayHetHanThe=readerExpiry.Value.Date}); RefreshReaders(); LoadLoanOptions(); MessageBox.Show("Đã lưu độc giả."); } catch(Exception ex){ShowError(ex);} }
    private void DeleteReader() { if(selectedReader==0)return; if(MessageBox.Show("Xóa độc giả đang chọn?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return; try{bus.DeleteReader(selectedReader);RefreshReaders();LoadLoanOptions();}catch(Exception ex){ShowError(ex);} }
    private void SaveBook(bool edit) { try { bus.SaveBook(new DauSachDTO{MaSach=edit?selectedBook:0,TenSach=bookName.Text.Trim(),TheLoai=string.IsNullOrWhiteSpace(bookGenre.Text)?null:bookGenre.Text.Trim(),SoLuongCon=(int)bookStock.Value});RefreshBooks();LoadLoanOptions();MessageBox.Show("Đã lưu đầu sách.");}catch(Exception ex){ShowError(ex);} }
    private void DeleteBook() { if(selectedBook==0)return; if(MessageBox.Show("Xóa đầu sách đang chọn?","Xác nhận",MessageBoxButtons.YesNo)!=DialogResult.Yes)return; try{bus.DeleteBook(selectedBook);RefreshBooks();LoadLoanOptions();}catch(Exception ex){ShowError(ex);} }
    private void CreateLoan() { try { if(loanReader.SelectedValue==null||loanBook.SelectedValue==null)throw new InvalidOperationException("Cần có độc giả và đầu sách."); int id=bus.CreateLoan(Convert.ToInt32(loanReader.SelectedValue),dueDate.Value.Date,Convert.ToInt32(loanBook.SelectedValue),(int)loanQty.Value);MessageBox.Show($"Lập phiếu mượn thành công. Mã phiếu: {id}");RefreshBooks();LoadLoanOptions();RefreshLoans();RefreshOverdue();}catch(Exception ex){ShowError(ex);} }
    private void ReturnLoan() { if(loansGrid.CurrentRow==null)return; int id=Convert.ToInt32(loansGrid.CurrentRow.Cells["MaPhieu"].Value); try{bus.ReturnLoan(id);MessageBox.Show("Đã trả sách và hoàn lại tồn kho.");RefreshBooks();LoadLoanOptions();RefreshLoans();RefreshOverdue();RefreshStats();}catch(Exception ex){ShowError(ex);} }
    private static void ShowError(Exception ex)=>MessageBox.Show(ex.Message,"Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);
}
internal sealed class ComboItem
{
    public string Text {get;} public int? Value {get;}
    public ComboItem(string text,int? value){Text=text;Value=value;}
    public override string ToString()=>Text;
}
internal static class ControlExtensions
{
    public static Control AlsoDockTop(this Control c){c.Dock=DockStyle.Top;return c;}
}
