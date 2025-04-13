using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.DTO.Rbac;
using Model.Other;
using Model.VO.Rbac;

namespace InterFace
{
    public interface IUserInfoService
    {
        /// <summary>
        /// 用户登录函数
        /// </summary>
        /// <param name="arg">登录所需的输入信息</param>
        /// <returns>返回一个格式化后的登录结果响应</returns>
        Task<FormattedResponse<LoginResult>> UserLoginAsync(LoginInput arg);

        /// <summary>
        /// 用户注册函数
        /// </summary>
        /// <param name="arg">注册所需的输入信息</param>
        /// <returns>返回一个格式化后的注册结果响应</returns>
        Task<FormattedResponse<RegisterResult>> UserRegisterAsync(RegisterInput arg);

        /// <summary>
        /// 异步创建密码并格式化响应结果
        /// </summary>
        /// <param name="lenth">密码长度，默认为16位</param>
        /// <returns>返回一个任务，该任务将生成一个格式化的响应结果，包含创建的密码</returns>
        Task<FormattedResponse<string>> PasswordCreateAsync(int lenth = 16);
    }
}
