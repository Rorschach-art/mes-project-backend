using System.ComponentModel;

namespace Model.VO.Rbac
{
    public class RegisterResult
    {
        /// <summary>
        /// 用户编码
        /// </summary>
        [Description("用户编码")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// 用户名
        /// </summary>
        [Description("用户名")]
        public string? Username { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        [Description("身份证号")]
        public string? IdCard { get; set; }

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
        /// 地址
        /// </summary>
        [Description("地址")]
        public string? Address { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        [Description("用户头像")]
        public string? Avatar { get; set; }
    }
}
