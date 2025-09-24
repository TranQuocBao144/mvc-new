using DXWebApplication4.Models;
using DXWebApplication4.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DXWebApplication4.Controllers
{
    public class ProductController : Controller
    {
        private readonly QLBanHangEntities1 _db = new QLBanHangEntities1();
        private readonly FirebaseService _firebaseService;

        // Constructor
        public ProductController()
        {
            string bucketName = "doanmusic-c1235.appspot.com"; 
            string serviceAccountPath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/devexpress-5b0d4-firebase-adminsdk-fbsvc-45eb70e0ae.json");
            _firebaseService = new FirebaseService(bucketName, serviceAccountPath);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
            ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FormCollection form)
        {
            string productName = form["ProductName"];
            string productSizeId = form["ProductSize"];
            string productColorId = form["ProductColor"];

            HttpPostedFileBase productImage = Request.Files["ProductImage"];
            HttpPostedFileBase productDocument = Request.Files["ProductDocument"];
            
            
            HttpPostedFileBase testImage = Request.Files["TestImage"];
            HttpPostedFileBase testDocument = Request.Files["TestDocument"];

            
            var allFiles = Request.Files.AllKeys;
            var filesInfo = string.Join(", ", allFiles.Select(key => $"{key}: {(Request.Files[key] != null && Request.Files[key].ContentLength > 0 ? Request.Files[key].FileName : "null")}"));

            TempData["Debug"] = $"Start Create - ProductName={productName}, ProductSizeId='{productSizeId}' (type: {productSizeId?.GetType().Name}), ProductColorId='{productColorId}', " +
                                $"All Files: [{filesInfo}], " +
                                $"DevExImage={(productImage != null ? productImage.FileName : "null")}, " +
                                $"DevExDoc={(productDocument != null ? productDocument.FileName : "null")}, " +
                                $"TestImage={(testImage != null ? testImage.FileName : "null")}, " +
                                $"TestDoc={(testDocument != null ? testDocument.FileName : "null")}";

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productSizeId) || string.IsNullOrEmpty(productColorId))
            {
                ModelState.AddModelError("", "Product Name, Size và Color là bắt buộc.");
                ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                return View();
            }

            try
            {
               
                var product = new Product { TenPro = productName };
                _db.Products.Add(product);

                int rowsProduct = await _db.SaveChangesAsync();
                TempData["Debug"] += $" | Saved Product: Rows={rowsProduct}, IDPro={product.IDPro}, TenPro={product.TenPro}";

                
                Size sizeEntity = null;
                
                
                if (int.TryParse(productSizeId, out int sizeId))
                {
                    sizeEntity = _db.Sizes.FirstOrDefault(s => s.IDSize == sizeId);
                    TempData["Debug"] += $" | Tried to find size by ID: {sizeId}, Found: {(sizeEntity != null ? "Yes" : "No")}";
                }
                
                
                if (sizeEntity == null)
                {
                    sizeEntity = _db.Sizes.FirstOrDefault(s => s.TenSize == productSizeId);
                    TempData["Debug"] += $" | Tried to find size by name: '{productSizeId}', Found: {(sizeEntity != null ? "Yes" : "No")}";
                }
                
                if (sizeEntity == null)
                {
                    TempData["Error"] = $"Size '{productSizeId}' không hợp lệ.";
                    ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                    ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                    return View();
                }

                
                Color selectedColor = null;
                
                
                if (int.TryParse(productColorId, out int colorId))
                {
                    selectedColor = _db.Colors.FirstOrDefault(c => c.IDColor == colorId);
                    TempData["Debug"] += $" | Tried to find color by ID: {colorId}, Found: {(selectedColor != null ? "Yes" : "No")}";
                }
                
                
                if (selectedColor == null)
                {
                    selectedColor = _db.Colors.FirstOrDefault(c => c.TenColor == productColorId);
                    TempData["Debug"] += $" | Tried to find color by name: '{productColorId}', Found: {(selectedColor != null ? "Yes" : "No")}";
                }
                
                if (selectedColor == null)
                {
                    TempData["Error"] = $"Color '{productColorId}' không hợp lệ.";
                    ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                    ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                    return View();
                }

                var variant = new ProductVariant
                {
                    IDPro = product.IDPro,
                    IDSize = sizeEntity.IDSize,
                    IDColor = selectedColor.IDColor
                };
                _db.ProductVariants.Add(variant);

                int rowsVariant = await _db.SaveChangesAsync();
                TempData["Debug"] += $" | Saved Variant: Rows={rowsVariant}, IDVariant={variant.IDVariant}";

                var imageFiles = new System.Collections.Generic.List<HttpPostedFileBase>();

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var key = Request.Files.GetKey(i);
                    var file = Request.Files[i];
                    if (file == null || file.ContentLength <= 0) continue;
                    if (key == "TestImages" || key == "TestImage" || key == "ProductImage" || (key != null && key.StartsWith("ProductImage")))
                    {
                        imageFiles.Add(file);
                    }
                }

                if (imageFiles.Count == 0)
                {
                    if (testImage != null && testImage.ContentLength > 0) imageFiles.Add(testImage);
                    if (productImage != null && productImage.ContentLength > 0) imageFiles.Add(productImage);
                }

                foreach (var imgFile in imageFiles)
                {
                    var imageUrl = await _firebaseService.UploadFileAsync(imgFile.InputStream, imgFile.FileName, "images");
                    var image = new Image
                    {
                        IDVariant = variant.IDVariant,
                        URL = imageUrl
                    };
                    _db.Images.Add(image);
                    TempData["Debug"] += $" | Added Image: {imageUrl} (from file: {imgFile.FileName})";
                }
                if (imageFiles.Count == 0)
                {
                    TempData["Debug"] += " | No image files found";
                }

                var documentFiles = new System.Collections.Generic.List<HttpPostedFileBase>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var key = Request.Files.GetKey(i);
                    var file = Request.Files[i];
                    if (file == null || file.ContentLength <= 0) continue;
                    if (key == "TestDocuments" || key == "TestDocument" || key == "ProductDocument" || (key != null && key.StartsWith("ProductDocument")))
                    {
                        documentFiles.Add(file);
                    }
                }

                if (documentFiles.Count == 0)
                {
                    if (testDocument != null && testDocument.ContentLength > 0) documentFiles.Add(testDocument);
                    if (productDocument != null && productDocument.ContentLength > 0) documentFiles.Add(productDocument);
                }

                foreach (var docFile in documentFiles)
                {
                    var noteUrl = await _firebaseService.UploadFileNoteAsync(docFile.InputStream, docFile.FileName, "notes");
                    var note = new Note
                    {
                        IDPro = product.IDPro,
                        NoiDung = noteUrl
                    };
                    _db.Notes.Add(note);
                    TempData["Debug"] += $" | Added Note: {noteUrl} (from file: {docFile.FileName})";
                }
                if (documentFiles.Count == 0)
                {
                    TempData["Debug"] += " | No document files found";
                }

                int rowsFinal = await _db.SaveChangesAsync();
                TempData["Debug"] += $" | Final Save: Rows={rowsFinal}";

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi lưu sản phẩm: " + ex.ToString();
                ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                ViewBag.Colors = new SelectList(_db.Colors.ToList(), "IDColor", "TenColor");
                return View();
            }
        }

        // GET: Product/Index
        public ActionResult Index()
        {
            var products = _db.Products.Include(p => p.ProductVariants).ToList();
            return View(products);
        }

        // GET: Product/Details/5
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
