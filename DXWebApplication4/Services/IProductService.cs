using DXWebApplication4.Models;
using DXWebApplication4.Repository;
using DXWebApplication4.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        Task DeleteImageAsync(int idImage);

        IEnumerable<Size> GetSizes();
        IEnumerable<Color> GetColors();
        Task UpdateVariantAsync(
             int? variantId,
             int productId,
             int? sizeId,
             int? colorId,
             IEnumerable<HttpPostedFileBase> files, string tenPro = null);
    }
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly FirebaseService _firebase;

        public ProductService(IProductRepository productRepo)
        {
            _productRepo = productRepo;

            var bucketName = ConfigurationManager.AppSettings["Firebase:BucketName"];
            var serviceAccountPath = System.Web.HttpContext.Current.Server.MapPath(
                ConfigurationManager.AppSettings["Firebase:ServiceAccountPath"]
            );

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
        public async Task DeleteImageAsync(int idImage)
        {
            var image = await _productRepo.GetImageByIdAsync(idImage);
            if (image != null)
            {
                _productRepo.DeleteImage(image);
                await _productRepo.SaveAsync();
            }
        }
        public async Task UpdateVariantAsync(int? variantId, int productId, int? sizeId, int? colorId, IEnumerable<HttpPostedFileBase> files, string tenPro)
        {

            var product = await _productRepo.GetDetailsAsync(productId)
                ?? throw new Exception("Sản phẩm không tồn tại.");

            ProductVariant variant = null;
            if (!string.IsNullOrWhiteSpace(tenPro))
            {
                product.TenPro = tenPro;
                await _productRepo.SaveAsync(); 
            }

            if (variantId.HasValue)
            {
                variant = await _productRepo.GetVariantByIdAsync(variantId.Value)
                    ?? throw new Exception("Biến thể không tồn tại.");
            }
            else
            {
                variant = new ProductVariant { IDPro = productId };
                await _productRepo.AddVariantAsync(variant);
            }

            if (sizeId.HasValue) variant.IDSize = sizeId.Value;
            if (colorId.HasValue) variant.IDColor = colorId.Value;

            if (files != null)
            {
                foreach (var file in files.Where(f => f != null && f.ContentLength > 0))
                {
                    var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                    if (new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(ext))
                    {
                        var uniqueName = $"{Guid.NewGuid()}{ext}";
                        var imageUrl = await _firebase.UploadFileAsync(file.InputStream, uniqueName, "images");
                        await _productRepo.AddImageAsync(new Image { IDVariant = variant.IDVariant, URL = imageUrl });
                    }
                    else
                    {
                        throw new Exception("Chỉ hỗ trợ upload file ảnh.");
                    }
                }
            }

            await _productRepo.SaveAsync();
        }
    }
}
