using InterFace;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Model.DTO.Rbac;
using Model.Other;
using Model.VO.Rbac;

namespace Mes.Controllers
{
    /// <summary>
    /// 用户控制器类，负责处理与用户相关的HTTP请求
    /// </summary>
    /// <param name="userInfoService">用户信息服务接口，用于执行用户相关的业务逻辑</param>
    public class UserController(IUserInfoService userInfoService, ICustomJwtService jWtService)
        : BaseController
    {
        // 保存用户信息服务接口的实例，以便在类的方法中使用
        private readonly IUserInfoService _userInfoService = userInfoService;
        private readonly ICustomJwtService _jWtService = jWtService;

         /// <summary>
        /// 用户登录方法，处理用户的登录请求
        /// </summary>
        /// <param name="arg">登录输入模型，包含用户登录所需的信息</param>
        /// <returns>返回一个格式化的响应，包含登录结果信息</returns>
        [HttpPost]
        [AllowAnonymous]
        [EndpointSummary("用户登录")]
        [EndpointDescription("登录服务接口")]
        [EnableRateLimiting("LoginLimiter")]
        public async Task<FormattedResponse<LoginResult>> UserLoginAsync([FromBody] LoginInput arg)
        {
            try
            {
                // 调用用户信息服务的登录方法，并返回登录结果
                var result = await _userInfoService.UserLoginAsync(arg);
                if (!result.IsSuccess) return result;
                //查看HTTP请求头是否有jwtToken
                if (
                    Request.Headers.TryGetValue(
                        "Authorization",
                        out var value
                    )
                )
                {
                    // 提取令牌
                    var token = value.ToString().Replace("Bearer ", "");

                    // 验证令牌
                    var result1 = await _jWtService.ValidateTokenAsync(token);
                    if (result1.Identity is { IsAuthenticated: true })
                    {
                        result.Data.Token = token;
                    }
                }
                else
                {
                    var token = await _jWtService.GetTokenAsync(result.Data);
                    result.Data.Token = token.AccessToken;
                    result.Data.RefreshToken = token.RefreshToken;
                }
                return result;
            }
            catch (Exception ex)
            {
                return FormattedResponse<LoginResult>.Error(
                    "登录失败",
                    500,
                    messageDetail: ex.Message
                );
            }
        }

        /// <summary>
        /// 异步处理用户注册请求
        /// </summary>
        /// <param name="arg">包含用户注册所需信息的实体</param>
        /// <returns>返回一个包含注册结果的格式化响应</returns>
        [HttpPost]
        [EndpointSummary("用户注册")]
        [EndpointDescription("注册服务接口")]
        public async Task<FormattedResponse<RegisterResult>> UserRegisterAsync(
            [FromBody] RegisterInput arg
        )
        {
            // 调用用户信息服务的用户注册方法，并等待结果
            return await _userInfoService.UserRegisterAsync(arg);
        }

        [HttpPost]
        [AllowAnonymous]
        [EndpointSummary("生成密码")]
        [EndpointDescription("密码服务接口")]
        public async Task<FormattedResponse<string>> PasswordCreateAsync()
        {
            return await _userInfoService.PasswordCreateAsync();
        }
        public record TokenRefresh { public string RefreshToken { get; set;}=default!;}
        [HttpPost]
        [EndpointSummary("刷新Token")]
        [EndpointDescription("Token服务接口")]
        public async Task<FormattedResponse<TokenResult>> RefreshTokenAsync([FromBody] TokenRefresh tokenRefresh )
        {
            try
            {
                var result = await _jWtService.RefreshTokenAsync(tokenRefresh.RefreshToken);
                return FormattedResponse<TokenResult>.Success("刷新成功", result);
            }
            catch (Exception ex)
            {
                return FormattedResponse<TokenResult>.Error("刷新失败", 500, ex.Message);
            }
        }
    }
}
