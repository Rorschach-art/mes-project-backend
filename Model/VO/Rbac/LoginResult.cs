using System.ComponentModel;

namespace Model.VO.Rbac
{
    public class LoginResult
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Description("账号")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// 用户名
        /// </summary>
        [Description("用户名")]
        public string? Username { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Description("邮箱")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// 电话号码
        /// </summary>
        [Description("电话号码")]
        public string? Phone { get; set; }
        /// <summary>
        /// Token
        /// </summary>
        [Description("Token")]
        public string? Token { get; set; }=string.Empty; 
        /// <summary>
        /// 用于刷新的Token
        /// </summary>
        [Description("RefreshToken")]
        public string? RefreshToken { get; set; }=string.Empty;
        /// <summary>
        /// 用户头像
        /// </summary>
        [Description("用户头像")]
        public string? Avatar { get; set; }
    }
}
