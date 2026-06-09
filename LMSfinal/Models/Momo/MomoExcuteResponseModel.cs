
namespace LMSfinal.Models.Momo
{
    public class MomoExcuteResponseModel
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
        public string FullName { get; internal set; }
        public DateTime DatePaid { get; internal set; }
    }
}
