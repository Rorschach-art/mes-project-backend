using System.Linq.Expressions;
using InterFace;
using Microsoft.AspNetCore.Http;
using Model.DTO.Rbac;
using Model.Entity.RBAC;
using Model.Other;
using Model.VO.Rbac;
using SqlSugar;

namespace Service.Rbac
{
    /// <summary>
    /// 用户信息服务类
    /// </summary>
    public class UserInfoService(ISqlSugarClient sqlSugarClient,IHttpContextAccessor contextAccessor) :BaseService(sqlSugarClient,contextAccessor), IUserInfoService
    {

        /// <summary>
        /// 用户登录方法
        /// </summary>
        /// <param name="arg">登录输入参数，包含用户标识和密码</param>
        /// <returns>格式化的登录结果响应</returns>
        public async Task<FormattedResponse<LoginResult>> UserLoginAsync(LoginInput arg)
        {
            try
            {
                // 验证Code是否为空
                if (string.IsNullOrWhiteSpace(arg.Code))
                {
                    return FormattedResponse<LoginResult>.Error("请输入账号信息", 200);
                }
                // 验证密码是否为空
                if (string.IsNullOrWhiteSpace(arg.Password))
                {
                    return FormattedResponse<LoginResult>.Error("请输入密码", 200);
                }
                // 根据Code的类型查询用户信息
                var queryCondition = GetQueryCondition(arg.Code);
                var userInfo = await SqlSugar
                    .Queryable<UserInfo>()
                    .Where(queryCondition)
                    .FirstAsync();

                // 如果用户不存在，返回错误
                if (userInfo == null)
                {
                    return FormattedResponse<LoginResult>.Error("用户名不存在", 200);
                }

                // 验证密码
                if (!SecurityUtility.VerifyPassword(arg.Password, userInfo.Password))
                {
                    return FormattedResponse<LoginResult>.Error("密码错误", 200);
                }

                // 返回成功响应
                return FormattedResponse<LoginResult>.Success(
                    "登录成功",
                    new LoginResult
                    {
                        Code = userInfo.Code,
                        Username = userInfo.Username,
                        Email = userInfo.Email,
                        Phone = userInfo.Phone,
                        Avatar= userInfo.Avatar,
                        
                    }
                );
            }
            catch (Exception ex)
            {
                return FormattedResponse<LoginResult>.Error(
                    "发生错误",
                    500,
                    messageDetail: ex.Message
                );
            }
        }

        /// <summary>
        /// 获取查询用户信息的条件
        /// </summary>
        /// <param name="code">用户标识，可以是用户名、邮箱、电话或身份证号</param>
        /// <returns>查询用户信息的条件表达式</returns>
        private static Expression<Func<UserInfo, bool>> GetQueryCondition(string code)
        {
            return code switch
            {
                // 如果Code是用户名
                _
                    when !SecurityUtility.IsEmail(code)
                        && !SecurityUtility.IsChinesePhone(code)
                        && !SecurityUtility.IsIdCardWithCheck(code) => x => x.Code == code,

                // 如果Code是邮箱
                _ when SecurityUtility.IsEmail(code) => x => x.Email == code,

                // 如果Code是电话
                _ when SecurityUtility.IsChinesePhone(code) => x => x.Phone == code,

                // 如果Code是身份证号
                _ when SecurityUtility.IsIdCardWithCheck(code) => x => x.IdCard == code,

                // 默认情况（理论上不会进入）
                _ => x => false,
            };
        }

        /// <summary>
        /// 用户注册方法
        /// </summary>
        /// <param name="arg">注册输入参数</param>
        /// <returns>格式化的注册结果响应</returns>
        public async Task<FormattedResponse<RegisterResult>> UserRegisterAsync(RegisterInput arg)
        {
            try
            {
                // 1. 验证输入参数
                var validationErrors = ValidateInput(arg);
                if (validationErrors.Count != 0)
                    return FormattedResponse<RegisterResult>.Error(validationErrors.First(), 200);

                // 2. 检查用户是否已存在
                if (await CheckUserExistenceAsync(arg))
                    return FormattedResponse<RegisterResult>.Error("用户信息已存在", 200);

                // 3. 准备用户数据
                var userInfo = PrepareUserInfo(arg);
                 userInfo.CreateUserId = userInfo.Id;
                // 4. 插入数据库
                var insertResult = await SqlSugar.Insertable(userInfo).ExecuteCommandAsync();

                // 根据插入结果返回成功或失败的响应
                return insertResult == 1
                    ? FormattedResponse<RegisterResult>.Success(
                        "注册成功,请记住您的账号,或者用Email和Phone以及身份证号码登录",
                        new RegisterResult
                        {
                            Code = userInfo.Code,
                            Username = userInfo.Username,
                            Email = userInfo.Email,
                            Phone = userInfo.Phone,
                            Address = userInfo.Address,
                            IdCard = userInfo.IdCard,
                            Avatar = userInfo.Avatar,
                        }
                    )
                    : FormattedResponse<RegisterResult>.Error("注册失败,请重试", 200);
            }
            catch (Exception ex)
            {
                // 异常处理，返回错误响应
                return FormattedResponse<RegisterResult>.Error(
                    "发生错误",
                    500,
                    messageDetail: ex.Message
                );
            }
        }

        /// <summary>
        /// 验证注册输入参数
        /// </summary>
        /// <param name="arg">注册输入参数</param>
        /// <returns>验证错误列表，如果没有错误则为空列表</returns>
        private static List<string> ValidateInput(RegisterInput arg)
        {
            var errors = new List<string>();

            // 验证各个字段是否为空或无效
            if (string.IsNullOrWhiteSpace(arg.Password))
                errors.Add("请输入密码");
            if (string.IsNullOrWhiteSpace(arg.Email))
                errors.Add("请输入邮箱");
            if (string.IsNullOrWhiteSpace(arg.Phone))
                errors.Add("请输入手机号");
            if (string.IsNullOrWhiteSpace(arg.Address))
                errors.Add("请输入地址");
            if (string.IsNullOrWhiteSpace(arg.IdCard))
                errors.Add("请输入身份证号");
            if (string.IsNullOrWhiteSpace(arg.Username))
                errors.Add("请输入真实姓名");
            if (string.IsNullOrWhiteSpace(arg.Avatar))
                errors.Add("请上传头像");

            return errors;
        }

        /// <summary>
        /// 检查用户是否已存在的方法
        /// </summary>
        /// <param name="arg">注册输入参数</param>
        /// <returns>如果用户已存在则返回true，否则返回false</returns>
        private async Task<bool> CheckUserExistenceAsync(RegisterInput arg)
        {
            var query = SqlSugar.Queryable<UserInfo>();

            // 检查邮箱、手机号和身份证号是否已存在于数据库中
            return await query.AnyAsync(x => x.Email == arg.Email)
                || await query.AnyAsync(x => x.Phone == arg.Phone)
                || await query.AnyAsync(x => x.IdCard == arg.IdCard);
        }

        /// <summary>
        /// 准备用户数据的方法
        /// </summary>
        /// <param name="arg">注册输入参数</param>
        /// <returns>填充了用户信息的UserInfo对象</returns>
        private static UserInfo PrepareUserInfo(RegisterInput arg)
        {
            return new UserInfo
            {
                Id = SequentialGuidGenerator.Instance.GenerateSequentialGuid(),
                Code = SnowflakeIdWorker.Instance.NextId().ToString(),
                Password = SecurityUtility.GeneratePasswordHash(arg.Password),
                Username = arg.Username,
                IdCard = arg.IdCard,
                Email = arg.Email,
                Phone = arg.Phone,
                Address = arg.Address,
                Avatar = arg.Avatar,
                CreateTime = DateTime.UtcNow,
                CreateUserName = arg.Username,
            };
        }

        /// <summary>
        /// 异步创建密码并格式化响应结果
        /// </summary>
        /// <param name="length">密码长度，默认为16位</param>
        /// <returns>返回一个任务，该任务将生成一个格式化的响应结果，包含创建的密码</returns>
        public async Task<FormattedResponse<string>> PasswordCreateAsync(int length = 16)
        {
            // 输入验证
            if (length is <= 0 or > 128) // 假设最大密码长度为 128
            {
                return FormattedResponse<string>.Error(
                    "无效的密码长度，请确保长度在 1 到 128 之间",
                    200
                );
            }

            try
            {
                // 如果 GenerateRandomPassword 是同步方法，直接调用即可
                var password = await GeneratePasswordAsync(length);

                // 返回成功响应
                return FormattedResponse<string>.Success("密码创建成功", password);
            }
            catch (Exception ex)
            {
                // 捕获异常并返回错误响应
                return FormattedResponse<string>.Error(
                    $"密码生成失败",
                    500,
                    messageDetail: ex.Message
                );
            }
        }

        // 封装密码生成逻辑
        private static Task<string> GeneratePasswordAsync(int length)
        {
            // 如果 GenerateRandomPassword 是同步方法，可以改为以下实现：
            // return Task.FromResult(SecurityUtility.GenerateRandomPassword(length));
            return Task.Run(() => SecurityUtility.GenerateRandomPassword(length));
        }
    }
}
