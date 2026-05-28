using System.ComponentModel.DataAnnotations;
using LMSfinal.Models.EF;

namespace LMSfinal.Models.ViewModels.Admin
{
    public class PricingHistoryViewModel
    {
        public decimal CurrentPrice { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal NewPrice { get; set; }

        public DateTime NewEffectiveFrom { get; set; } = DateTime.Now;

        public List<PricePerCreditHistory> History { get; set; } = new();
    }
}