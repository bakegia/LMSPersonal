using LMSfinal.Data;
using LMSfinal.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace LMSfinal.Services
{
    public interface IPricingService
    {
        Task<decimal> GetCurrentPriceAsync();
        Task SetNewPriceAsync(decimal newPrice, DateTime effectiveFrom);
        Task<decimal> CalculateTuitionAsync(int courseId);
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
            var now = DateTime.Now;
            var price = await _context.PricePerCreditHistories
                .Where(x => x.EffectiveFrom <= now && (x.EffectiveTo == null || x.EffectiveTo >= now))
                .OrderByDescending(x => x.EffectiveFrom)
                .Select(x => x.Price)
                .FirstOrDefaultAsync();

            return price;
        }

        public async Task<decimal> CalculateTuitionAsync(int courseId)
        {
            var currentPrice = await GetCurrentPriceAsync();
            var credits = await _context.Courses
                .Where(c => c.Id == courseId)
                .Select(c => c.Credits)
                .FirstOrDefaultAsync();

            return currentPrice * credits;
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