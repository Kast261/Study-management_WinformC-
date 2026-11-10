using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface IBaoCaoService
    {
        DataSet TaoDuLieuBangDiem(DataTable results, string tenHocVien, string nguoiLap);
    }
}
