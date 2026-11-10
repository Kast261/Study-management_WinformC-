using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class CauHoiImportService : ICauHoiImportService
    {
        private readonly CauHoiImportDAL _repository;

        public CauHoiImportService() : this(new CauHoiImportDAL()) { }

        public CauHoiImportService(CauHoiImportDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public KetQuaNhapCauHoi Nhap(int taiKhoanID, DeThi deThi, IList<CauHoi> questions)
        {
            if (questions == null || questions.Count == 0) throw new ArgumentException("Chưa có câu hỏi để nhập.", nameof(questions));
            return _repository.Nhap(taiKhoanID, deThi, questions);
        }
    }
}
