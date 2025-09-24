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
            string bucketName = "doanmusic-c1235.appspot.com"; // chỉnh lại cho đúng
            string serviceAccountPath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/devexpress-5b0d4-firebase-adminsdk-fbsvc-45eb70e0ae.json");
            _firebaseService = new FirebaseService(bucketName, serviceAccountPath);
        }

        // GET: Product/Create
        public ActionResult Create()
        {
            ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FormCollection form)
        {
            string productName = form["ProductName"];
            string productSizeId = form["ProductSize"];

            HttpPostedFileBase productImage = Request.Files["ProductImage"];
            HttpPostedFileBase productDocument = Request.Files["ProductDocument"];
            
            // Test với HTML input
            HttpPostedFileBase testImage = Request.Files["TestImage"];
            HttpPostedFileBase testDocument = Request.Files["TestDocument"];

            // Debug tất cả files được gửi
            var allFiles = Request.Files.AllKeys;
            var filesInfo = string.Join(", ", allFiles.Select(key => $"{key}: {(Request.Files[key] != null && Request.Files[key].ContentLength > 0 ? Request.Files[key].FileName : "null")}"));

            TempData["Debug"] = $"Start Create - ProductName={productName}, ProductSizeId='{productSizeId}' (type: {productSizeId?.GetType().Name}), " +
                                $"All Files: [{filesInfo}], " +
                                $"DevExImage={(productImage != null ? productImage.FileName : "null")}, " +
                                $"DevExDoc={(productDocument != null ? productDocument.FileName : "null")}, " +
                                $"TestImage={(testImage != null ? testImage.FileName : "null")}, " +
                                $"TestDoc={(testDocument != null ? testDocument.FileName : "null")}";

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productSizeId))
            {
                ModelState.AddModelError("", "Product Name và Size là bắt buộc.");
                ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                return View();
            }

            try
            {
                // 1. Thêm Product
                var product = new Product { TenPro = productName };
                _db.Products.Add(product);

                int rowsProduct = await _db.SaveChangesAsync();
                TempData["Debug"] += $" | Saved Product: Rows={rowsProduct}, IDPro={product.IDPro}, TenPro={product.TenPro}";

                // 2. Lấy Size và Color mặc định
                Size sizeEntity = null;
                
                // Thử parse như ID trước
                if (int.TryParse(productSizeId, out int sizeId))
                {
                    sizeEntity = _db.Sizes.FirstOrDefault(s => s.IDSize == sizeId);
                    TempData["Debug"] += $" | Tried to find size by ID: {sizeId}, Found: {(sizeEntity != null ? "Yes" : "No")}";
                }
                
                // Nếu không parse được ID, thử tìm theo tên
                if (sizeEntity == null)
                {
                    sizeEntity = _db.Sizes.FirstOrDefault(s => s.TenSize == productSizeId);
                    TempData["Debug"] += $" | Tried to find size by name: '{productSizeId}', Found: {(sizeEntity != null ? "Yes" : "No")}";
                }
                
                if (sizeEntity == null)
                {
                    TempData["Error"] = $"Size '{productSizeId}' không hợp lệ.";
                    ViewBag.Sizes = new SelectList(_db.Sizes.ToList(), "IDSize", "TenSize");
                    return View();
                }

                // Lấy color mặc định (màu đầu tiên hoặc tạo màu mặc định)
                var defaultColor = _db.Colors.FirstOrDefault();
                if (defaultColor == null)
                {
                    defaultColor = new Color { TenColor = "Default" };
                    _db.Colors.Add(defaultColor);
                    await _db.SaveChangesAsync();
                }

                // 3. Thêm ProductVariant
                var variant = new ProductVariant
                {
                    IDPro = product.IDPro,
                    IDSize = sizeEntity.IDSize,
                    IDColor = defaultColor.IDColor
                };
                _db.ProductVariants.Add(variant);

                int rowsVariant = await _db.SaveChangesAsync();
                TempData["Debug"] += $" | Saved Variant: Rows={rowsVariant}, IDVariant={variant.IDVariant}";

                // 4. Upload Image - Thử nhiều cách
                HttpPostedFileBase imageFile = null;
                
                // Ưu tiên test image nếu có
                if (testImage != null && testImage.ContentLength > 0)
                {
                    imageFile = testImage;
                    TempData["Debug"] += $" | Using TestImage: {testImage.FileName}";
                }
                else if (productImage != null && productImage.ContentLength > 0)
                {
                    imageFile = productImage;
                    TempData["Debug"] += $" | Using DevExImage: {productImage.FileName}";
                }
                else
                {
                    // Thử tìm file image với các tên khác nhau
                    var possibleImageNames = new[] { "ProductImage", "ProductImage_0", "ProductImage$0" };
                    foreach (var name in possibleImageNames)
                    {
                        var file = Request.Files[name];
                        if (file != null && file.ContentLength > 0)
                        {
                            imageFile = file;
                            TempData["Debug"] += $" | Found image with name: {name}";
                            break;
                        }
                    }
                }

                if (imageFile != null)
                {
                    var imageUrl = await _firebaseService.UploadFileAsync(imageFile.InputStream, imageFile.FileName, "images");
                    var image = new Image
                    {
                        IDVariant = variant.IDVariant,
                        URL = imageUrl
                    };
                    _db.Images.Add(image);
                    TempData["Debug"] += $" | Added Image: {imageUrl} (from file: {imageFile.FileName})";
                }
                else
                {
                    TempData["Debug"] += $" | No image file found";
                }

                // 5. Upload Document - Thử nhiều cách
                HttpPostedFileBase documentFile = null;
                
                // Ưu tiên test document nếu có
                if (testDocument != null && testDocument.ContentLength > 0)
                {
                    documentFile = testDocument;
                    TempData["Debug"] += $" | Using TestDocument: {testDocument.FileName}";
                }
                else if (productDocument != null && productDocument.ContentLength > 0)
                {
                    documentFile = productDocument;
                    TempData["Debug"] += $" | Using DevExDocument: {productDocument.FileName}";
                }
                else
                {
                    // Thử tìm file document với các tên khác nhau
                    var possibleDocNames = new[] { "ProductDocument", "ProductDocument_0", "ProductDocument$0" };
                    foreach (var name in possibleDocNames)
                    {
                        var file = Request.Files[name];
                        if (file != null && file.ContentLength > 0)
                        {
                            documentFile = file;
                            TempData["Debug"] += $" | Found document with name: {name}";
                            break;
                        }
                    }
                }

                if (documentFile != null)
                {
                    var noteUrl = await _firebaseService.UploadFileNoteAsync(documentFile.InputStream, documentFile.FileName, "notes");
                    var note = new Note
                    {
                        IDPro = product.IDPro,
                        NoiDung = noteUrl
                    };
                    _db.Notes.Add(note);
                    TempData["Debug"] += $" | Added Note: {noteUrl} (from file: {documentFile.FileName})";
                }
                else
                {
                    TempData["Debug"] += $" | No document file found";
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
                return View();
            }
        }

        // GET: Product/Index
        public ActionResult Index()
        {
            var products = _db.Products.Include(p => p.ProductVariants).ToList();
            return View(products);
        }
    }
}
