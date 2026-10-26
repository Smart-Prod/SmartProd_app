namespace SmartProd.API.Server.Utils;

public sealed class ConflictException(string message) : Exception(message);
