using DXWebApplication4.Models;
using DXWebApplication4.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

namespace DXWebApplication4.Controllers
{
    public class ProductController : Controller
    {
        private readonly QLBanHangEntities1 _db = new QLBanHangEntities1();
        private readonly FirebaseService _firebaseService;

        // Constructor - Giữ nguyên vấn đề cấu hình theo yêu cầu
        public ProductController()
        {
            string bucketName = "doanmusic-c1235.appspot.com";
            string serviceAccountPath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/doanmusic-c1235-firebase-adminsdk-siavi-faba5c718c.json");
            _firebaseService = new FirebaseService(bucketName, serviceAccountPath);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            // Tối ưu: Lấy danh sách từ DB chỉ một lần
            ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
            ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                return View(model);
            }

            try
            {
                int sizeId;
                var sizeEntity = int.TryParse(model.ProductSize, out sizeId)
                                 ? _db.Sizes.FirstOrDefault(s => s.IDSize == sizeId)
                                 : _db.Sizes.FirstOrDefault(s => s.TenSize == model.ProductSize);

                if (sizeEntity == null)
                {
                    TempData["Error"] = $"Size '{model.ProductSize}' không hợp lệ.";
                    ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                    ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                    return View(model);
                }

                int colorId;
                var selectedColor = int.TryParse(model.ProductColor, out colorId)
                                    ? _db.Colors.FirstOrDefault(c => c.IDColor == colorId)
                                    : _db.Colors.FirstOrDefault(c => c.TenColor == model.ProductColor);

                if (selectedColor == null)
                {
                    TempData["Error"] = $"Color '{model.ProductColor}' không hợp lệ.";
                    ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                    ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                    return View(model);
                }

                var product = new Product { TenPro = model.ProductName };
                _db.Products.Add(product);

                var variant = new ProductVariant
                {
                    IDPro = product.IDPro,
                    IDSize = sizeEntity.IDSize,
                    IDColor = selectedColor.IDColor
                };
                _db.ProductVariants.Add(variant);

                var imageFiles = new List<HttpPostedFileBase>();
                var documentFiles = new List<HttpPostedFileBase>();

                var allFiles = Request.Files.AllKeys.Select(key => Request.Files[key]).Where(f => f != null && f.ContentLength > 0);

                foreach (var file in allFiles)
                {
                    var ext = System.IO.Path.GetExtension(file.FileName)?.ToLowerInvariant();
                    var ct = (file.ContentType ?? string.Empty).ToLowerInvariant();

                    if (ct.StartsWith("image/") || new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(ext))
                    {
                        imageFiles.Add(file);
                    }
                    else if (new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" }.Contains(ext))
                    {
                        documentFiles.Add(file);
                    }
                }

                foreach (var imgFile in imageFiles)
                {
                    var imageUrl = await _firebaseService.UploadFileAsync(imgFile.InputStream, imgFile.FileName, "images");
                    var image = new Image { IDVariant = variant.IDVariant, URL = imageUrl };
                    _db.Images.Add(image);
                }

                foreach (var docFile in documentFiles)
                {
                    var noteUrl = await _firebaseService.UploadFileNoteAsync(docFile.InputStream, docFile.FileName, "notes");
                    var note = new Note { IDPro = product.IDPro, NoiDung = noteUrl };
                    _db.Notes.Add(note);
                }

                await _db.SaveChangesAsync();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                TempData["Error"] = "Lỗi khi lưu sản phẩm: " + ex.Message;
                ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                return View(model);
            }
        }

        public ActionResult Index()
        {
            var products = _db.Products.Include(p => p.ProductVariants).ToList();
            return View(products);
        }

        public ActionResult Details(int id)
        {
            var product = _db.Products
                .Include(p => p.ProductVariants.Select(v => v.Size))
                .Include(p => p.ProductVariants.Select(v => v.Color))
                .Include(p => p.ProductVariants.Select(v => v.Images))
                .Include(p => p.Notes)
                .FirstOrDefault(p => p.IDPro == id);

            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Index");
            }

            return View(product);
        }
    }
}