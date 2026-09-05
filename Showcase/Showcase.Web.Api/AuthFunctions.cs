using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Showcase.Web.Api;

/// <summary>
/// FaLoginForm demo — a fake auth check, no real user store. "demo"/"password" is
/// the one combination that succeeds; everything else fails.
/// </summary>
public sealed class AuthFunctions
{
    [Function("Login")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
    {
        var request = await req.ReadFromJsonAsync<LoginRequest>();
        var success = request?.Username == "demo" && request?.Password == "password";

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new LoginResult(success, success ? null : "Incorrect username or password."));
        return response;
    }
}
