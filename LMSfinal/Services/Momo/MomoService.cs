using LMSfinal.Models;
using LMSfinal.Models.Momo;

using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;


namespace LMSfinal.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfo model)
        {
            model.OrderInformation = "Khach hang: " + model.FullName + ". Noi dung" + model.OrderInformation;
            string endpoint = _options.Value.MomoApiUrl;

            // ĐÚNG THỨ TỰ THEO MOMO YÊU CẦU
            string rawData =
                $"accessKey={_options.Value.AccessKey}" +
                $"&amount={model.Amount}" +
                $"&extraData=" +
                $"&ipnUrl={_options.Value.NotifyUrl}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderInformation}" +
                $"&partnerCode={_options.Value.PartnerCode}" +
                $"&redirectUrl={_options.Value.ReturnUrl}" +
                $"&requestId={model.OrderId}" +
                $"&requestType={_options.Value.RequestType}";

            string signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.AccessKey,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,   // <= vẫn gửi notifyUrl đúng format MoMo
                ipnUrl = _options.Value.NotifyUrl,
                redirectUrl = _options.Value.ReturnUrl,
                orderId = model.OrderId,
                amount = model.Amount.ToString("F0"),
                orderInfo = model.OrderInformation,
                requestId = model.OrderId, // Cần truyền y hệt để mapping
                extraData = "",
                signature = signature
            };

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);
            var result = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);

            return result;
        }

        public MomoExcuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;
            return new MomoExcuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo,
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            byte[] hashBytes;

            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;


        }
    }
}
