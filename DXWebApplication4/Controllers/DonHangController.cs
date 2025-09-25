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


        // Index hiển thị danh sách
        public ActionResult Index()
        {
            return View(GetData());
        }

        // Partial view cho Grid
        [ValidateInput(false)]
        public ActionResult GridViewPartial()
        {
            var query = from dh in db.DonHangs
                        join ctdh in db.CTDHs on dh.IDDH equals ctdh.IDDH into gj1
                        from ctdh in gj1.DefaultIfEmpty()
                        join pv in db.ProductVariants on ctdh.IDVariant equals pv.IDVariant into gj2
                        from pv in gj2.DefaultIfEmpty()
                        join c in db.Colors on pv.IDColor equals c.IDColor into gj3
                        from c in gj3.DefaultIfEmpty()
                        join s in db.Sizes on pv.IDSize equals s.IDSize into gj4
                        from s in gj4.DefaultIfEmpty()
                        select new
                        {
                            IDDH = (int?)dh.IDDH,
                            NgayDat = dh.NgayDat,
                            KhachHang = dh.KhachHang,
                            Mau = c.TenColor,
                            Size = s.TenSize,
                            SoLuong = (int?)ctdh.SoLuong
                        };

            var data = query.GroupBy(x => new { x.IDDH, x.NgayDat, x.KhachHang, x.Mau })
                .Select(g => new DonHangViewModel
                {
                    IDDH = g.Key.IDDH ?? 0,
                    NgayDat = g.Key.NgayDat,
                    KhachHang = g.Key.KhachHang,
                    Color = g.Key.Mau,
                    XXS = g.Where(x => x.Size == "XXS").Sum(x => x.SoLuong) ?? 0,
                    XSM = g.Where(x => x.Size == "XSM").Sum(x => x.SoLuong) ?? 0,
                    SM = g.Where(x => x.Size == "SM").Sum(x => x.SoLuong) ?? 0,
                    MED = g.Where(x => x.Size == "MED").Sum(x => x.SoLuong) ?? 0,
                    LRG = g.Where(x => x.Size == "LRG").Sum(x => x.SoLuong) ?? 0,
                    XLG = g.Where(x => x.Size == "XLG").Sum(x => x.SoLuong) ?? 0,
                    XXL = g.Where(x => x.Size == "XXL").Sum(x => x.SoLuong) ?? 0,
                })
                .Where(d => (d.XXS + d.XSM + d.SM + d.MED + d.LRG + d.XLG + d.XXL) > 0)
                .ToList();
            return PartialView("_GridViewPartial", GetData());
        }

        private List<DonHangViewModel> GetData()
        {
            var query = from dh in db.DonHangs
                        join ctdh in db.CTDHs on dh.IDDH equals ctdh.IDDH into gj1
                        from ctdh in gj1.DefaultIfEmpty()
                        join pv in db.ProductVariants on ctdh.IDVariant equals pv.IDVariant into gj2
                        from pv in gj2.DefaultIfEmpty()
                        join c in db.Colors on pv.IDColor equals c.IDColor into gj3
                        from c in gj3.DefaultIfEmpty()
                        join s in db.Sizes on pv.IDSize equals s.IDSize into gj4
                        from s in gj4.DefaultIfEmpty()
                        select new
                        {
                            IDDH = (int?)dh.IDDH,
                            NgayDat = dh.NgayDat,
                            KhachHang = dh.KhachHang,
                            Mau = c.TenColor,
                            Size = s.TenSize,
                            SoLuong = (int?)ctdh.SoLuong
                        };

            var data = query
                .GroupBy(x => new { x.IDDH, x.NgayDat, x.KhachHang, x.Mau })
                .Select(g => new DonHangViewModel
                {
                    IDDH = g.Key.IDDH ?? 0,
                    NgayDat = g.Key.NgayDat,
                    KhachHang = g.Key.KhachHang,
                    Color = g.Key.Mau,
                    XXS = g.Where(x => x.Size == "XXS").Sum(x => x.SoLuong) ?? 0,
                    XSM = g.Where(x => x.Size == "XSM").Sum(x => x.SoLuong) ?? 0,
                    SM = g.Where(x => x.Size == "SM").Sum(x => x.SoLuong) ?? 0,
                    MED = g.Where(x => x.Size == "MED").Sum(x => x.SoLuong) ?? 0,
                    LRG = g.Where(x => x.Size == "LRG").Sum(x => x.SoLuong) ?? 0,
                    XLG = g.Where(x => x.Size == "XLG").Sum(x => x.SoLuong) ?? 0,
                    XXL = g.Where(x => x.Size == "XXL").Sum(x => x.SoLuong) ?? 0,
                })
                .Where(d => (d.XXS + d.XSM + d.SM + d.MED + d.LRG + d.XLG + d.XXL) > 0)
                .ToList();

            return data;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialAddNew(DonHang item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.DonHangs.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewData["EditError"] = ex.Message;
                }
            }

            return GridViewPartial(); // gọi lại hàm chung
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialUpdate(DonHang item)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var modelItem = db.DonHangs.FirstOrDefault(it => it.IDDH == item.IDDH);
                    if (modelItem != null)
                    {
                        db.Entry(modelItem).CurrentValues.SetValues(item);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    ViewData["EditError"] = ex.Message;
                }
            }

            return GridViewPartial();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GridViewPartialDelete(int IDDH)
        {
            try
            {
                var item = db.DonHangs.FirstOrDefault(it => it.IDDH == IDDH);
                if (item != null)
                {
                    db.DonHangs.Remove(item);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ViewData["EditError"] = ex.Message;
            }

            return GridViewPartial();
        }
    }
}