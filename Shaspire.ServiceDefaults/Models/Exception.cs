namespace Shaspire.ServiceDefaults.Models;

public class NotFoundException : Exception
{
  public NotFoundException(string message) : base(message) { }
  public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

public class UnauthorizedException : Exception
{
  public UnauthorizedException(string message) : base(message) { }
  public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
}

public class ForbiddenException : Exception
{
  public ForbiddenException(string message) : base(message) { }
  public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
}

public class BadRequestException : Exception
{
  public BadRequestException(string message) : base(message) { }
  public BadRequestException(string message, Exception innerException) : base(message, innerException) { }
}