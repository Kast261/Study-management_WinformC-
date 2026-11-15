namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    // Trạng thái độc lập với WinForms để kiểm tra các chuỗi Activate/Deactivate.
    public sealed class GiamSatPhongThi
    {
        public int SoLan { get; private set; }
        public bool DangThi { get; private set; }
        public bool BiDung => SoLan >= 3;
        private bool coTieuDiem;

        public void BatDau(int soLanDaLuu)
        {
            SoLan = System.Math.Max(0, System.Math.Min(3, soLanDaLuu));
            DangThi = true;coTieuDiem = true;
        }
        public void KichHoat() { coTieuDiem = true; }
        public bool RoiCuaSo(bool hopThoaiNoiBo)
        {
            if (!DangThi || BiDung || !coTieuDiem || hopThoaiNoiBo) return false;
            coTieuDiem = false;SoLan++;return true;
        }
        public void KetThuc() { DangThi = false; }
    }
}
