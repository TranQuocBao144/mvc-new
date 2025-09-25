using DXWebApplication4.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXWebApplication4.Repository
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> GetDetailsAsync(int id);
        IEnumerable<Size> GetSizes();
        IEnumerable<Color> GetColors();
        Task AddVariantAsync(ProductVariant variant);
        Task AddImageAsync(Image image);
        Task AddNoteAsync(Note note);

    }

    public class ProductRepository : EfRepository<Product>, IProductRepository
    {
        public ProductRepository(QLBanHangEntities1 context) : base(context) { }

        public async Task<Product> GetDetailsAsync(int id)
        {
            return await _context.Products
            .Include("ProductVariants.Size")
            .Include("ProductVariants.Color")
            .Include("ProductVariants.Images")
            .Include("Notes")
            .FirstOrDefaultAsync(p => p.IDPro == id);

        }

        public IEnumerable<Size> GetSizes() => _context.Sizes.ToList();
        public IEnumerable<Color> GetColors() => _context.Colors.ToList();
        public async Task AddVariantAsync(ProductVariant variant)
        {
            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
        }

        public async Task AddImageAsync(Image image)
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
        }

        public async Task AddNoteAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }
    }
}
