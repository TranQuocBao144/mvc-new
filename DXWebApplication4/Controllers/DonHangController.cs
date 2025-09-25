
using DXWebApplication4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DXWebApplication4.Controllers
{
    public class DonHangController : Controller
    {
        private QLBanHangEntities1 db = new QLBanHangEntities1();


        public ActionResult Index()
        {
            var query = db.DonHangs.OrderBy(dh => dh.IDDH);
            var DonHangs = query.ToList();

            if (!DonHangs.Any())
            {
                ViewBag.ThongBao = "Khong co don hang nao";
            }
            return View(DonHangs);
        }
        public ActionResult GridViewPartial()
        {
            var model = db.DonHangs.OrderBy(d => d.IDDH).ToList();
            return PartialView("_GridViewPartial", model);
        }
        public ActionResult Details(int id)
        {
            var query = from dh in db.DonHangs
                        join ctdh in db.CTDHs on dh.IDDH equals ctdh.IDDH
                        join pv in db.ProductVariants on ctdh.IDVariant equals pv.IDVariant
                        join c in db.Colors on pv.IDColor equals c.IDColor
                        join s in db.Sizes on pv.IDSize equals s.IDSize
                        join img in db.Images on pv.IDVariant equals img.IDVariant into imgs
                        from i in imgs.DefaultIfEmpty()
                        where dh.IDDH == id
                        select new
                        {
                            dh.IDDH,
                            dh.NgayDat,
                            dh.KhachHang,
                            Mau = c.TenColor,
                            Size = s.TenSize,
                            SoLuong = ctdh.SoLuong,
                            URL = i.URL
                        };

            var grouped = query
                .GroupBy(x => new { x.IDDH, x.NgayDat, x.KhachHang, x.Mau, x.URL })
                .Select(g => new DonHangViewModel
                {
                    IDDH = g.Key.IDDH,
                    NgayDat = g.Key.NgayDat,
                    KhachHang = g.Key.KhachHang,
                    Color = g.Key.Mau,
                    URL = g.Key.URL,
                    XXS = g.Where(x => x.Size == "XXS").Sum(x => (int?)x.SoLuong) ?? 0,
                    XSM = g.Where(x => x.Size == "XSM").Sum(x => (int?)x.SoLuong) ?? 0,
                    SM = g.Where(x => x.Size == "SM").Sum(x => (int?)x.SoLuong) ?? 0,
                    MED = g.Where(x => x.Size == "MED").Sum(x => (int?)x.SoLuong) ?? 0,
                    LRG = g.Where(x => x.Size == "LRG").Sum(x => (int?)x.SoLuong) ?? 0,
                    XLG = g.Where(x => x.Size == "XLG").Sum(x => (int?)x.SoLuong) ?? 0,
                    XXL = g.Where(x => x.Size == "XXL").Sum(x => (int?)x.SoLuong) ?? 0,
                })
                .ToList();

            return View(grouped);
        }
    }
}
