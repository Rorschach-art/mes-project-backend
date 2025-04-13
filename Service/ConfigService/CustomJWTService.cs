using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using InterFace;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Model.Entity.RBAC;
using Model.Other;
using Model.VO.Rbac;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SqlSugar;

namespace Service.ConfigService
{
    public class CustomJwtService :BaseService, ICustomJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly Lazy<RSAParameters> _privateKey;
        private readonly Lazy<RSAParameters> _publicKey;
        private static readonly Lazy<RSA> RsaInstance =
            new(
                () =>
                {
                    var rsa = RSA.Create();
                    return rsa;
                },
                LazyThreadSafetyMode.ExecutionAndPublication
            );

        public CustomJwtService(IOptionsMonitor<JwtSettings> jwt,ISqlSugarClient sqlSugar, IHttpContextAccessor contextAccessor) : base( sqlSugar,  contextAccessor)
        {
            _jwtSettings =
                jwt.CurrentValue
                ?? throw new ArgumentNullException(nameof(jwt), "JWT设置不能为空。");
            if (string.IsNullOrEmpty(_jwtSettings.SecurityKey))
                throw new InvalidOperationException("JWT私钥未配置");
            if (string.IsNullOrEmpty(_jwtSettings.PublicKey))
                throw new InvalidOperationException("JWT公钥未配置");

            // 使用Lazy<T>缓存私钥和公钥
            _privateKey = new Lazy<RSAParameters>(
                LoadRsaPrivateKey,
                LazyThreadSafetyMode.ExecutionAndPublication
            );
            _publicKey = new Lazy<RSAParameters>(
                LoadRsaPublicKey,
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        public async Task<TokenResult> GetTokenAsync(LoginResult login)
        {

                try
                {
                    if (login == null)
                        throw new ArgumentNullException(nameof(login), "登录信息不能为空。");
                    var user = await this.SqlSugar.Queryable<UserInfo>().FirstAsync(x => x.Code == login.Code)??new UserInfo();
                    var accessClaims = CreateClaims(user);
                    var refreshClaims = CreateRefreshClaims(login.Code);

                    RsaInstance.Value.ImportParameters(_privateKey.Value);
                    var signingCredentials = new SigningCredentials(
                        new RsaSecurityKey(RsaInstance.Value),
                        SecurityAlgorithms.RsaSha256
                    );

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var accessToken = CreateToken(
                        accessClaims,
                        DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                        signingCredentials
                    );
                    var refreshToken = CreateToken(
                        refreshClaims,
                        DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                        signingCredentials
                    );

                    var result = new TokenResult
                    {
                        AccessToken = tokenHandler.WriteToken(accessToken),
                        RefreshToken = tokenHandler.WriteToken(refreshToken),
                    };

                    Console.WriteLine(
                        $"[{DateTime.UtcNow}] GetTokenAsync completed for {login.Code}"
                    );
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.UtcNow}] Error in GetTokenAsync: {ex}");
                    throw;
                }
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(token))
                    throw new ArgumentNullException(nameof(token), "令牌不能为空。");

                RsaInstance.Value.ImportParameters(_publicKey.Value);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new RsaSecurityKey(RsaInstance.Value),
                };

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var principal = tokenHandler.ValidateToken(
                        token,
                        validationParameters,
                        out var validatedToken
                    );
                    if (validatedToken is not JwtSecurityToken)
                        throw new SecurityTokenException("无效的令牌格式。");
                    return principal;
                }
                catch (SecurityTokenExpiredException ex)
                {
                    throw new SecurityTokenException("令牌已过期。", ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.UtcNow}] Error in ValidateTokenAsync: {ex}");
                    throw new SecurityTokenException("令牌验证失败。", ex);
                }
            });
        }

        public async Task<TokenResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                ClaimsPrincipal? principal =
                    await ValidateTokenAsync(refreshToken)
                    ?? throw new SecurityTokenException("无效的刷新令牌。");
                var codeClaim =
                    principal.FindFirst("Code")?.Value
                    ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                if (string.IsNullOrEmpty(codeClaim))
                    throw new SecurityTokenException("无效的刷新令牌：缺少Code或Sub声明。"); 
                var user = await SqlSugar.Queryable<UserInfo>().FirstAsync(x => x.Code == codeClaim)??new UserInfo();
                var accessClaims = CreateClaims(user);
                var refreshClaims = CreateRefreshClaims(codeClaim);

                // 使用缓存的私钥
                RsaInstance.Value.ImportParameters(_privateKey.Value);
                var signingCredentials = new SigningCredentials(
                    new RsaSecurityKey(RsaInstance.Value),
                    SecurityAlgorithms.RsaSha256
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var newAccessToken = CreateToken(
                    accessClaims,
                    DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    signingCredentials
                );
                var newRefreshToken = CreateToken(
                    refreshClaims,
                    DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                    signingCredentials
                );

                var result = new TokenResult
                {
                    AccessToken = tokenHandler.WriteToken(newAccessToken),
                    RefreshToken = tokenHandler.WriteToken(newRefreshToken),
                };
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.UtcNow}] Error in RefreshTokenAsync: {ex}");
                throw;
            }
        }

        private static ClaimsIdentity CreateClaims(UserInfo res)
        {
            var claims = new[]
            {
                new Claim("Code", res.Code ?? string.Empty),
                new Claim("Username", res.Username ?? string.Empty),
                new Claim("Email", res.Email ?? string.Empty),
                new Claim("Phone", res.Phone ?? string.Empty),
                new Claim("Id", res.Id.ToString() ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, res.Code ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64
                ),
            };
            return new ClaimsIdentity(claims);
        }

        private static ClaimsIdentity CreateRefreshClaims(string code)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, code ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64
                ),
            };
            return new ClaimsIdentity(claims);
        }

        private SecurityToken CreateToken(
            ClaimsIdentity subject,
            DateTime expires,
            SigningCredentials credentials
        )
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var descriptor = new SecurityTokenDescriptor
                {
                    Subject = subject,
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    Expires = expires,
                    SigningCredentials = credentials,
                };
                return tokenHandler.CreateToken(descriptor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.UtcNow}] Error in CreateToken: {ex}");
                throw;
            }
        }

        private RSAParameters LoadRsaPrivateKey()
        {
            using var reader = new StringReader(_jwtSettings.SecurityKey);
            var pemReader = new PemReader(reader);
            var keyObject = pemReader.ReadObject();

            if (keyObject is AsymmetricCipherKeyPair keyPair)
                keyObject = keyPair.Private;

            if (keyObject is RsaPrivateCrtKeyParameters privateKeyParams)
                return DotNetUtilities.ToRSAParameters(privateKeyParams);

            throw new InvalidOperationException("无效的RSA私钥格式。");
        }

        private RSAParameters LoadRsaPublicKey()
        {
            using var reader = new StringReader(_jwtSettings.PublicKey);
            var pemReader = new PemReader(reader);
            var keyObject = pemReader.ReadObject();

            if (keyObject is RsaKeyParameters publicKeyParams)
                return DotNetUtilities.ToRSAParameters(publicKeyParams);

            throw new InvalidOperationException("无效的RSA公钥格式。");
        }
    }
}
