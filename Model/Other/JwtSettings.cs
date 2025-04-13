using System.ComponentModel;

namespace Model.Other
{
    /// <summary>
    /// 包含JWT（JSON Web Token）配置的类
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// 用于加密JWT的密钥
        /// </summary>
        [Description("用于加密JWT的密钥")]
        public string SecurityKey { get; set; } = null!;

        /// <summary>
        /// 用于验证JWT签名的公钥
        /// </summary>
        [Description("用于验证JWT签名的公钥")]
        public string PublicKey { get; set; } = null!;

        /// <summary>
        /// JWT的发行者标识
        /// </summary>
        [Description("JWT的发行者标识")]
        public string Issuer { get; set; } = null!;

        /// <summary>
        /// JWT的受众标识
        /// </summary>
        [Description("JWT的受众标识")]
        public string Audience { get; set; } = null!;

        /// <summary>
        /// JWT的过期时间（分钟）
        /// </summary>
        [Description("JWT的过期时间（分钟）")]
        public int ExpirationMinutes { get; set; }

        /// <summary>
        /// 刷新令牌的过期时间（天）
        /// </summary>
        [Description("刷新令牌的过期时间（天）")]
        public int RefreshTokenExpirationDays { get; set; }
    }
}
