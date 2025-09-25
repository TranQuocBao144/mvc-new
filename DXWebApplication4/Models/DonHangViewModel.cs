using System;

namespace DXWebApplication4.Models
{
    public class DonHangViewModel
    {
        public int IDDH { get; set; }
        public DateTime NgayDat { get; set; }
        public string KhachHang { get; set; }
        public string Color { get; set; }
        public string URL { get; set; }
        public string Caption { get; set; }

        public int XXS { get; set; }
        public int XSM { get; set; }
        public int SM { get; set; }
        public int MED { get; set; }
        public int LRG { get; set; }
        public int XLG { get; set; }
        public int XXL { get; set; }

        public int TongSoLuong
        {
            get
            {
                return XXS + XSM + SM + MED + LRG + XLG + XXL;
            }
        }
    }
}
