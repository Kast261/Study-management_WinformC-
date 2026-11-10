using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface ICauHoiImportService
    {
        KetQuaNhapCauHoi Nhap(int taiKhoanID, DeThi deThi, IList<CauHoi> questions);
    }
}
