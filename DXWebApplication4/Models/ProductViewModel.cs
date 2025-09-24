using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace DXWebApplication4.Models
{
    public class ProductViewModel
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Kích thước là bắt buộc.")]
        public string ProductSize { get; set; }

        [Required(ErrorMessage = "Màu sắc là bắt buộc.")]
        public string ProductColor { get; set; }

        public IEnumerable<HttpPostedFileBase> TestImages { get; set; }
        public IEnumerable<HttpPostedFileBase> TestDocuments { get; set; }
    }
}