using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Utilities.Jwt
{
  public class ActorProfile
  {
    public string ID { get; set; }
    public string Name { get; set; }
    public string[] Roles { get; set; }
  }
  public class JwtSecurity
  {
    private static string authrizationConfigurationUrl = Environment.GetEnvironmentVariable("Authorization-Configuration-Url");
    private static string authorizationClientID = Environment.GetEnvironmentVariable("Authorization-ClientID");
    private static string authorizationIssuer = Environment.GetEnvironmentVariable("Authorization-Issuer");
    private static string authorizationMetadata = Environment.GetEnvironmentVariable("Authorization-Metadata");
    public JwtSecurity()
    {

    }

    private static string GetUserID(ClaimsPrincipal claimsPrincipal)
    {
      var identity = claimsPrincipal.Identity as ClaimsIdentity;
      var claims = identity.Claims.ToList();
      var subjectClaim = claims.First(t => t.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
      return subjectClaim.Value;
    }
    private static string GetUserName(ClaimsPrincipal claimsPrincipal)
    {
      var identity = claimsPrincipal.Identity as ClaimsIdentity;
      var claims = identity.Claims.ToList();
      var subjectClaim = claims.First(t => t.Type == (authorizationMetadata + "name")); ;
      return subjectClaim.Value;
    }

    private static AuthenticationHeaderValue GetHeader(HttpRequest request)
    {
      var headerValue = request.Headers.ContainsKey("Authorization")
        ? request.Headers["Authorization"]
        : request.Headers.ContainsKey("authorization")
          ? request.Headers["authorization"]
          : throw new Exception("No Token");

      var splitHeader = headerValue.ToString().Split(' ');
      return new AuthenticationHeaderValue(splitHeader[0], splitHeader[1]);
    }

    private static async Task<(ClaimsPrincipal, ActorProfile)> Authorize(HttpRequest request)
    {
      var header = GetHeader(request);

      var handler = new JwtSecurityTokenHandler();

      var documentRetriever = new HttpDocumentRetriever();
      documentRetriever.RequireHttps = true;
      var configuration = new ConfigurationManager<OpenIdConnectConfiguration>(
        authrizationConfigurationUrl,
        new OpenIdConnectConfigurationRetriever(),
        documentRetriever);
      var validationParameter = new TokenValidationParameters()
      {
        RequireSignedTokens = true,
        ValidAudience = authorizationClientID,
        ValidateAudience = true,
        ValidIssuer = authorizationIssuer,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKeys = (await configuration.GetConfigurationAsync(CancellationToken.None)).SigningKeys
      };

      SecurityToken securityToken;
      var claims = handler.ValidateToken(header.Parameter, validationParameter, out securityToken);
      var jwtToken = securityToken as JwtSecurityToken;
      var actorProfile = (jwtToken.Payload[authorizationMetadata + "app_metadata"] as JObject).ToObject<ActorProfile>();
      actorProfile.ID = jwtToken.Payload["sub"] as string;
      actorProfile.Name = jwtToken.Payload[authorizationMetadata + "name"] as string;
      return (claims, actorProfile);
    }
    public static async Task<(bool, ClaimsPrincipal, ActorProfile)> IsValid(HttpRequest request)
    {
      try
      {
        var (claims, actorProfile) = await Authorize(request);
        return (true, claims, actorProfile);
      }
      catch
      {
        return (false, null, null);
      }
    }
    public static async Task<(bool, ClaimsPrincipal, ActorProfile)> IsValid(HttpRequest request, string[] roles)
    {
      var (claims, actorProfile) = await Authorize(request);
      return (roles.Any(t => actorProfile.Roles.Contains(t)), claims, actorProfile);
    }
  }
}