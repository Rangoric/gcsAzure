using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using gcsShared.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utilities.Jwt;

namespace gcsAzure
{

  public static class RulesetEndpoints
  {
    [FunctionName("ruleset-post")]
    public static async Task<HttpResponseMessage> Post(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ruleset")]HttpRequest req,
        [CosmosDB(
                databaseName: "GCS",
                collectionName: "Rulesets",
                ConnectionStringSetting = "Ruleset-Database")]
                IAsyncCollector<Ruleset> rulesetCollection,
                ILogger log)

    {
      var (isValid, claims, actorProfile) = await JwtSecurity.IsValid(req, new[] { "admin" });

      if (!isValid)
      {
        return new HttpResponseMessage(HttpStatusCode.Forbidden);
      }

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      Ruleset data = JsonConvert.DeserializeObject<Ruleset>(requestBody);

      if (data != null)
      {
        await rulesetCollection.AddAsync(data);
        var message = new HttpResponseMessage(HttpStatusCode.OK);
        return message;
      }

      return new HttpResponseMessage(HttpStatusCode.BadRequest);
    }
    [FunctionName("rulesets-get")]
    public static async Task<HttpResponseMessage> GetMany(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rulesets")]HttpRequest req,
      [CosmosDB(
                databaseName: "GCS",
                collectionName: "Rulesets",
                ConnectionStringSetting = "Ruleset-Database",
                SqlQuery = "SELECT * FROM c order by c.lastModified desc")]
                IEnumerable<Ruleset> rulesets, ILogger log)
    {
      var (isValid, claims, actorProfile) = await JwtSecurity.IsValid(req, new[] { "admin" });

      if (!isValid)
      {
        return new HttpResponseMessage(HttpStatusCode.Forbidden);
      }

      var message = new HttpResponseMessage(HttpStatusCode.OK);
      message.Content = new StringContent(JsonConvert.SerializeObject(rulesets.ToList()));
      return message;
    }
  }

}