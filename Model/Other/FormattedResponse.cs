using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Other
{
    /// <summary>
    /// 表示一个格式化的 API 响应类，包含响应的成功状态、消息、数据、响应码和错误信息
    /// </summary>
    /// <typeparam name="T">响应数据的类型</typeparam>
    public class FormattedResponse<T>
    {
        /// <summary>
        /// 表示响应是否成功的布尔值
        /// </summary>
        [Description("表示响应是否成功的布尔值")]
        public bool IsSuccess { get; }

        /// <summary>
        /// 表示响应的消息
        /// </summary>
        [Description("表示响应的消息")]
        public string Message { get; }

        /// <summary>
        /// 表示响应的数据
        /// </summary>
        [Description("表示响应的数据")]
        public T Data { get; }

        /// <summary>
        /// 表示响应的状态码
        /// </summary>
        [Description("表示响应的状态码")]
        public int ResponseCode { get; }

        /// <summary>
        /// 表示错误的详细信息
        /// </summary>
        [Description("表示错误的详细信息")]
        public ErrorDetails ErrorInfo { get; }

        /// <summary>
        /// 私有构造函数，用于创建 FormattedResponse 实例
        /// </summary>
        /// <param name="success">响应是否成功</param>
        /// <param name="message">响应消息</param>
        /// <param name="data">响应数据</param>
        /// <param name="responseCode">响应状态码</param>
        /// <param name="error">错误信息</param>
        private FormattedResponse(
            bool success,
            string message,
            T data,
            int responseCode,
            ErrorDetails error
        )
        {
            IsSuccess = success;
            Message = message;
            Data = data;
            ResponseCode = responseCode;
            ErrorInfo = error;
        }

        /// <summary>
        /// 创建一个成功的 API 响应
        /// </summary>
        /// <param name="message">成功消息</param>
        /// <param name="data">返回的数据</param>
        /// <param name="responseCode">HTTP 状态码，默认为 200</param>
        /// <returns>FormattedResponse<T> 实例</returns>
        public static FormattedResponse<T> Success(string message, T data, int responseCode = 200)
        {
            return new FormattedResponse<T>(true, message, data, responseCode, default!);
        }

        /// <summary>
        /// 创建一个错误的 API 响应
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="responseCode">HTTP 状态码</param>
        /// <param name="code">错误代码 (可选)</param>
        /// <param name="messageDetail">详细错误信息 (可选)</param>
        /// <param name="target">发生错误的字段或目标 (可选)</param>
        /// <param name="details">更详细的错误信息，例如验证错误 (可选)</param>
        /// <param name="innerError">内部错误信息 (可选)</param>
        /// <param name="timestamp">错误发生的时间 (可选)</param>
        /// <param name="source">错误来源 (可选)</param>
        /// <param name="helpLink">指向错误详细信息的链接 (可选)</param>
        /// <returns>FormattedResponse<T> 实例</returns>
        public static FormattedResponse<T> Error(
            string message,
            int responseCode,
            string? code = null,
            string? messageDetail = null,
            string? target = null,
            Dictionary<string, string[]>? details = null,
            ErrorDetails? innerError = null,
            DateTimeOffset? timestamp = null,
            string? source = null,
            string? helpLink = null
        )
        {
            timestamp ??= DateTimeOffset.Now;
            return new FormattedResponse<T>(
                false,
                message,
                default!,
                responseCode,
                new ErrorDetails
                {
                    Code = code,
                    Message = messageDetail,
                    Target = target,
                    Details = details,
                    InnerError = innerError,
                    Timestamp = timestamp,
                    Source = source,
                    HelpLink = helpLink,
                }
            );
        }
    }

    public class ErrorDetails
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        [Description("错误代码")]
        public string? Code { get; set; }

        /// <summary>
        /// 详细错误信息
        /// </summary>
        [Description("详细错误信息")]
        public string? Message { get; set; }

        /// <summary>
        /// 发生错误的字段或目标 (例如：字段名称、资源 ID)
        /// </summary>
        [Description("发生错误的字段或目标 (例如：字段名称、资源 ID)")]
        public string? Target { get; set; }

        /// <summary>
        /// 更详细的错误信息，例如验证错误，可以包含每个字段的错误消息
        /// </summary>
        [Description("更详细的错误信息，例如验证错误，可以包含每个字段的错误消息")]
        public Dictionary<string, string[]>? Details { get; set; }

        /// <summary>
        /// 内部错误信息，用于表示更底层的错误
        /// </summary>
        [Description("内部错误信息，用于表示更底层的错误")]
        public ErrorDetails? InnerError { get; set; }

        /// <summary>
        /// 错误发生的时间
        /// </summary>
        [Description("错误发生的时间")]
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// 错误来源 (例如：服务名称、组件名称)
        /// </summary>
        [Description("错误来源 (例如：服务名称、组件名称)")]
        public string? Source { get; set; }

        /// <summary>
        /// 指向错误详细信息的帮助文档或链接
        /// </summary>
        [Description("指向错误详细信息的帮助文档或链接")]
        public string? HelpLink { get; set; }
    }
}
