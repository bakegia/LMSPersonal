using System.ComponentModel.DataAnnotations;

namespace LMSfinal.Models.EF
{
    public class PricePerCreditHistory
    {
        public int Id { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }
    }
}