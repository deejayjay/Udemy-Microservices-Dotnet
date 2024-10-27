using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Mango.Services.OrderAPI.Utility;

public class BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor = accessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_accessor.HttpContext is not null)
        {
            var token = await _accessor.HttpContext.GetTokenAsync("access_token") ?? string.Empty;

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }


        return await base.SendAsync(request, cancellationToken);
    }
}
