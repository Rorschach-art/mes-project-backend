using System.ComponentModel;

namespace Model.DTO.Rbac
{
    public class LoginInput
    {
        /// <summary>
        /// 用户账号
        /// </summary>
        [Description("用户账号")]
        public string Code { get; set; } = null!;

        /// <summary>
        /// 密码
        /// </summary>
        [Description("密码")]
        public string Password { get; set; } = null!;
    }
}
