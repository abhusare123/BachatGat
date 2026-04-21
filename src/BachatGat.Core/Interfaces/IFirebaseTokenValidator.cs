namespace BachatGat.Core.Interfaces;

public record FirebaseTokenInfo(
    string Uid,
    string? PhoneNumber,
    string? Email,
    string? Name,
    string SignInProvider);

public interface IFirebaseTokenValidator
{
    Task<FirebaseTokenInfo?> ValidateAsync(string idToken);
}
