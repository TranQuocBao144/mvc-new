using DXWebApplication4.Models;
using DXWebApplication4.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DXWebApplication4.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: Product
        public async Task<ActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            ViewBag.Sizes = new SelectList(_productService.GetSizes(), "IDSize", "TenSize");
            ViewBag.Colors = new SelectList(_productService.GetColors(), "IDColor", "TenColor");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sizes = new SelectList(_productService.GetSizes(), "IDSize", "TenSize");
                ViewBag.Colors = new SelectList(_productService.GetColors(), "IDColor", "TenColor");
                return View(model);
            }

            var files = Request.Files.AllKeys
                .Select(k => Request.Files[k])
                .Where(f => f != null && f.ContentLength > 0);

            try
            {
                await _productService.CreateProductAsync(model, files);
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Lỗi khi lưu sản phẩm: " + ex.Message;
                ViewBag.Sizes = new SelectList(_productService.GetSizes(), "IDSize", "TenSize");
                ViewBag.Colors = new SelectList(_productService.GetColors(), "IDColor", "TenColor");
                return View(model);
            }
        }

        // GET: Product/Details/5
        public async Task<ActionResult> Details(int id)
        {
            ViewBag.Sizes = _productService.GetSizes().ToList();
            ViewBag.Colors = _productService.GetColors().ToList();
            var product = await _productService.GetDetailsAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Index");
            }
            return View(product);
        }
        [HttpGet]
        public async Task<ActionResult> DeleteImage(int idImage, int productId)
        {
            if (idImage <= 0) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            await _productService.DeleteImageAsync(idImage);

            return RedirectToAction("Details", new { id = productId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateVariant(int? SelectedVariantId, int IDPro, int? SelectedSize, int? SelectedColor, string TenPro)
        {
            if (!SelectedSize.HasValue || !SelectedColor.HasValue)
            {
                TempData["Error"] = "Bạn cần chọn Size và Color.";
                return RedirectToAction("Details", new { id = IDPro });
            }

            var files = Request.Files.AllKeys
                .Select(k => Request.Files[k])
                .Where(f => f != null && f.ContentLength > 0);

            try
            {
                await _productService.UpdateVariantAsync(SelectedVariantId, IDPro, SelectedSize.Value, SelectedColor.Value, files,TenPro);
                TempData["Success"] = "Cập nhật biến thể thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật biến thể: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = IDPro });
        }
    }
}
