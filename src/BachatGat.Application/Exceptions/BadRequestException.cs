namespace BachatGat.Application.Exceptions;

public class BadRequestException(string? message = null) : Exception(message);
