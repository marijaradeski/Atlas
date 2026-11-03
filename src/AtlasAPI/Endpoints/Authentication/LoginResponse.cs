namespace AtlasAPI.Endpoints.Authentication;

public sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc);
