using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utilities.Jwt;

namespace gcsAzure
{
  public static class ProfileEndpoints
  {
    [FunctionName("profile-get")]
    public static async Task<HttpResponseMessage> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profile")]HttpRequest req, ILogger log)
    {
      var (isValid, claims, actorProfile) = await JwtSecurity.IsValid(req);

      if (!isValid)
      {
        return new HttpResponseMessage(HttpStatusCode.Forbidden);
      }
      var message = new HttpResponseMessage(HttpStatusCode.OK);

      var claimsIdentity = claims.Identity as ClaimsIdentity;
      message.Content = new StringContent(JsonConvert.SerializeObject(actorProfile));
      return message;
    }

  }
}