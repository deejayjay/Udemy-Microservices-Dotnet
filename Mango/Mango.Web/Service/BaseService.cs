using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service;

public class BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) : IBaseService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MangoAPI");
            var message = new HttpRequestMessage();
            message.Headers.Add("Accept", "application/json");

            // Add token
            if(withBearer)
            {
                var token = _tokenProvider.GetToken();
                message.Headers.Add("Authorization", $"Bearer {token}");
            }

            message.RequestUri = new Uri(requestDto.Url);

            if (requestDto.Data is not null)
            {
                message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
            }

            message.Method = requestDto.ApiType switch
            {
                ApiType.POST => HttpMethod.Post,
                ApiType.PUT => HttpMethod.Put,
                ApiType.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get
            };

            var response = await client.SendAsync(message);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    return new ResponseDto { IsSuccess = false, Message = "Not Found" };
                case HttpStatusCode.Forbidden:
                    return new ResponseDto { IsSuccess = false, Message = "Access Denied" };
                case HttpStatusCode.Unauthorized:
                    return new ResponseDto { IsSuccess = false, Message = "Unauthorized" };
                case HttpStatusCode.InternalServerError:
                    return new ResponseDto { IsSuccess = false, Message = "Internal Server Error" };
                
                default:
                    var content = await response.Content.ReadAsStringAsync();
                    var responseDto = JsonConvert.DeserializeObject<ResponseDto>(content);
                    return responseDto;
            }
        }
        catch (Exception e)
        {
            var responseDto = new ResponseDto
            { 
                Message = e.Message,
                IsSuccess = false
            };

            return responseDto;
        }
    }
}
