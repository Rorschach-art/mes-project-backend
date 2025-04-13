using System.Security.Claims;
using Model.VO.Rbac;

namespace InterFace
{
    public interface ICustomJwtService
    {
        Task<TokenResult> GetTokenAsync(LoginResult login);
        Task<ClaimsPrincipal> ValidateTokenAsync(string token);
        Task<TokenResult> RefreshTokenAsync(string refreshToken);
        
    }
}
