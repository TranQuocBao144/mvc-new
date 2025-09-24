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
    }
}