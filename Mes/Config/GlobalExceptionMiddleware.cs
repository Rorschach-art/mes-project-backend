using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Model.Other;

namespace Mes.Config
{
    public class GlobalExceptionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            FormattedResponse<object> response;
            HttpStatusCode statusCode;

            switch (exception)
            {
                case InvalidOperationException invEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response = FormattedResponse<object>.Error(
                        message: "操作无效",
                        responseCode: (int)statusCode,
                        code: "INVALID_OPERATION",
                        messageDetail: invEx.Message,
                        source: invEx.Source,
                        helpLink: "https://localhost:7092/scalar/v1"
                    );
                    break;

                case ArgumentNullException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    response = FormattedResponse<object>.Error(
                        message: "请求参数无效",
                        responseCode: (int)statusCode,
                        code: "INVALID_ARGUMENT",
                        messageDetail: argEx.Message,
                        target: argEx.ParamName,
                        source: argEx.Source
                    );
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    response = FormattedResponse<object>.Error(
                        message: "未经授权的访问",
                        responseCode: (int)statusCode,
                        code: "UNAUTHORIZED",
                        messageDetail: exception.Message,
                        source: exception.Source
                    );
                    break;

                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    response = FormattedResponse<object>.Error(
                        message: "资源未找到",
                        responseCode: (int)statusCode,
                        code: "NOT_FOUND",
                        messageDetail: exception.Message,
                        source: exception.Source
                    );
                    break;

                case JsonException jsonEx:
                    statusCode = HttpStatusCode.InternalServerError;
                    response = FormattedResponse<object>.Error(
                        message: "响应序列化失败",
                        responseCode: (int)statusCode,
                        code: "SERIALIZATION_ERROR",
                        messageDetail: jsonEx.Message,
                        source: jsonEx.Source
                    );
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    response = FormattedResponse<object>.Error(
                        message: "服务器内部错误",
                        responseCode: (int)statusCode,
                        code: "INTERNAL_SERVER_ERROR",
                        messageDetail: exception.Message,
                        source: exception.Source,
                        helpLink: "https://localhost:7092/scalar/v1"
                    );
                    break;
            }

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)statusCode;
            }

            var result = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(result);
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}