using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using gcsAzure.Data;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Utilities.Jwt;

namespace gcsAzure
{

  public static class CharacterEndpoints
  {
    [FunctionName("character-post")]
    public static async Task<HttpResponseMessage> Post(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "character/{id}")] HttpRequest req,
      string id, [CosmosDB(
        databaseName: "GCS",
        collectionName: "Characters",
        ConnectionStringSetting = "Database")] IAsyncCollector<Character> outputCharacters, [CosmosDB(
        databaseName: "GCS",
        collectionName: "Characters",
        ConnectionStringSetting = "Database",
        SqlQuery = "SELECT * FROM c where c.id = {id} order by c.lastModified desc")] IEnumerable<Character> currentCharacters,
      ILogger log)

    {
      var (isValid, claims, actorProfile) = await JwtSecurity.IsValid(req, new[] { "admin" });

      if (!isValid)
      {
        return new HttpResponseMessage(HttpStatusCode.Forbidden);
      }

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

      if (string.IsNullOrWhiteSpace(requestBody))
      {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
      }

      var data = new Character
      {
        payload = requestBody,
        id = id,
        ownerID = actorProfile.ID,
        lastModified = DateTime.UtcNow.Ticks
      };

      var characters = currentCharacters.ToList();

      if (characters.Any() && characters.First().ownerID != data.ownerID)
      {
        return new HttpResponseMessage(HttpStatusCode.BadRequest);
      }

      await outputCharacters.AddAsync(data);
      var message = new HttpResponseMessage(HttpStatusCode.OK);
      message.Content = new StringContent(JsonConvert.SerializeObject(data));
      return message;
    }

    [FunctionName("characters-get")]
    public static async Task<HttpResponseMessage> GetMany(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "characters/{ownerID}")] HttpRequest req, [CosmosDB(
        databaseName: "GCS",
        collectionName: "Characters",
        ConnectionStringSetting = "Database",
        SqlQuery = "SELECT * FROM c where c.ownerID = {ownerID} order by c.lastModified desc")] IEnumerable<Character> characters,
      ILogger log)
    {
      var (isValid, claims, actorProfile) = await JwtSecurity.IsValid(req);

      if (!isValid)
      {
        return new HttpResponseMessage(HttpStatusCode.Forbidden);
      }

      var characterList = characters.ToList();

      if (characterList.Any())
      {
        if (characterList.First().ownerID != actorProfile.ID)
        {
          return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
      }
      log.LogInformation("OwnerID " + actorProfile.ID);
      var message = new HttpResponseMessage(HttpStatusCode.OK);
      message.Content = new StringContent(JsonConvert.SerializeObject(characterList));
      return message;
    }
  }

}