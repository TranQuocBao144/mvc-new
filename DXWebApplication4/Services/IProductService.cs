using DXWebApplication4.Models;
using DXWebApplication4.Repository;
using DXWebApplication4.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DXWebApplication4.Services
{
    public interface IProductService
    {
        Task CreateProductAsync(ProductViewModel model,IEnumerable<HttpPostedFileBase> files);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetDetailsAsync(int id);
        IEnumerable<Size> GetSizes();
        IEnumerable<Color> GetColors();
    }
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly FirebaseService _firebase;

        public ProductService(IProductRepository productRepo)
        {
            _productRepo = productRepo;

            // config Firebase tại đây, controller không cần biết
            string bucketName = "doanmusic-c1235.appspot.com";
            string serviceAccountPath = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/doanmusic-c1235-firebase-adminsdk-siavi-faba5c718c.json");
            _firebase = new FirebaseService(bucketName, serviceAccountPath);
        }

        public async Task<IEnumerable<Product>> GetAllAsync() => await _productRepo.GetAllAsync();
        public async Task<Product> GetDetailsAsync(int id) => await _productRepo.GetDetailsAsync(id);

        public IEnumerable<Size> GetSizes() => _productRepo.GetSizes();
        public IEnumerable<Color> GetColors() => _productRepo.GetColors();

        public async Task CreateProductAsync(ProductViewModel model, IEnumerable<HttpPostedFileBase> files)
        {
            var sizeEntity = int.TryParse(model.ProductSize, out var sizeId)
                ? _productRepo.GetSizes().FirstOrDefault(s => s.IDSize == sizeId)
                : _productRepo.GetSizes().FirstOrDefault(s => s.TenSize == model.ProductSize);

            var colorEntity = int.TryParse(model.ProductColor, out var colorId)
                ? _productRepo.GetColors().FirstOrDefault(c => c.IDColor == colorId)
                : _productRepo.GetColors().FirstOrDefault(c => c.TenColor == model.ProductColor);

            if (sizeEntity == null || colorEntity == null)
                throw new Exception("Size hoặc Color không hợp lệ.");

            var product = new Product { TenPro = model.ProductName };
            await _productRepo.AddAsync(product);

            var variant = new ProductVariant
            {
                IDPro = product.IDPro,
                IDSize = sizeEntity.IDSize,
                IDColor = colorEntity.IDColor
            };
            await _productRepo.AddVariantAsync(variant);

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var ct = (file.ContentType ?? string.Empty).ToLowerInvariant();

                if (ct.StartsWith("image/") || new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(ext))
                {
                    var imageUrl = await _firebase.UploadFileAsync(file.InputStream, file.FileName, "images");
                    await _productRepo.AddImageAsync(new Image { IDVariant = variant.IDVariant, URL = imageUrl });
                }
                else if (new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" }.Contains(ext))
                {
                    var noteUrl = await _firebase.UploadFileNoteAsync(file.InputStream, file.FileName, "notes");
                    await _productRepo.AddNoteAsync(new Note { IDPro = product.IDPro, NoiDung = noteUrl });
                }
            }
        }

    }
}
