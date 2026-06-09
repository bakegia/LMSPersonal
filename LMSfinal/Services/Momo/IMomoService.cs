using LMSfinal.Models;
using LMSfinal.Models.Momo;

namespace LMSfinal.Services.Momo
{
    public interface IMomoService 
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo model);
        MomoExcuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
