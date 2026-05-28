using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Services
{
    public interface IPricingService
    {
        Task<decimal> GetCurrentPriceAsync();
        Task SetNewPriceAsync(decimal newPrice, DateTime effectiveFrom);
    }

    public class PricingService : IPricingService
    {
        private readonly ApplicationDbContext _context;

        public PricingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetCurrentPriceAsync()
        {
            var price = await _context.PricePerCreditHistories
                .Where(x => x.EffectiveTo == null)
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => x.Price)
                .FirstOrDefaultAsync();

            return price;
        }

        public async Task SetNewPriceAsync(decimal newPrice, DateTime effectiveFrom)
        {
            var current = await _context.PricePerCreditHistories
                .Where(x => x.EffectiveTo == null)
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefaultAsync();

            if (current != null)
            {
                current.EffectiveTo = effectiveFrom;
            }

            _context.PricePerCreditHistories.Add(new PricePerCreditHistory
            {
                Price = newPrice,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = null
            });

            await _context.SaveChangesAsync();
        }
    }
}