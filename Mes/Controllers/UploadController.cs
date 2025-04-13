using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Model.Other;

namespace Mes.Controllers;

public class UploadController(IWebHostEnvironment hostEnvironment):BaseController
{
    private readonly IWebHostEnvironment _hostEnvironment = hostEnvironment; // 用于访问Web主机环境的接口
     private const long MaxFileSize = 524288000; // 最大文件大小500MB（以字节为单位）

     /// <summary>
     /// 处理文件上传请求，支持multipart/form-data请求类型。
     /// </summary>
     /// <returns>异步操作的任务，结果包含FormattedResponse对象。</returns>
     [HttpPost]
     [EndpointSummary("上传文件")]
     [EndpointDescription("上传文件服务接口")]
     [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)] // 设置请求表单的最大体积限制
     [RequestSizeLimit(MaxFileSize)] // 设置请求大小限制
     public async Task<FormattedResponse<List<string>>> UploadFile()
     {
         var request = HttpContext.Request; // 获取HTTP请求对象

         // 检查请求是否具有表单内容类型
         if (!request.HasFormContentType)
         {
             return FormattedResponse<List<string>>.Error("请求内容类型不正确", 400);
         }

         // 解析内容类型，获取multipart的边界值
         if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeader) ||
             string.IsNullOrEmpty(mediaTypeHeader.Boundary.Value))
         {
             return FormattedResponse<List<string>>.Error("请求内容类型不支持", 400);
         }

         // 创建一个MultipartReader来解析多部分表单数据
         var reader = new MultipartReader(mediaTypeHeader.Boundary.Value, request.Body);
         var section = await reader.ReadNextSectionAsync(); // 读取下一个部分

         // 设置上传路径，并检查路径是否存在
         var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "Images");
         if (!Directory.Exists(uploadPath))
         {
             Directory.CreateDirectory(uploadPath); // 如果路径不存在，则创建目录
         }

         var serverFilePathList = new List<string>(); // 存储上传成功文件的路径列表

         // 处理每个多部分表单数据部分
         while (section != null)
         {
             var header = section.GetContentDispositionHeader(); // 获取内容处置头信息

             // 检查部分是否包含文件
             if (header is not null && header.IsFileDisposition())
             {
                 var fileName = GetUniqueFileName(Path.GetExtension(header.FileName.Value!)); // 生成唯一文件名
                 var fileFullPath = Path.Combine(uploadPath, fileName); // 构建文件的完整路径

                 // 验证文件类型和大小
                 if (!IsValidFileType(Path.GetExtension(fileName)) || section.Body.Length > MaxFileSize)
                 {
                     return FormattedResponse<List<string>>.Error("不支持的文件类型或文件大小超出限制", 400);
                 }

                 try
                 {
                     // 保存文件到服务器
                     await using (var targetStream = System.IO.File.Create(fileFullPath))
                     {
                         await section.Body.CopyToAsync(targetStream); // 将文件内容复制到目标流
                     }
                     serverFilePathList.Add($"/Images/{fileName}"); // 将文件路径添加到列表中
                 }
                 catch (Exception ex)
                 {
                     return FormattedResponse<List<string>>.Error("文件上传失败", 500, ex.Message);
                 }
             }

             section = await reader.ReadNextSectionAsync(); // 读取下一个部分
         }

         // 如果没有成功上传的文件，则返回204 No Content
         if (serverFilePathList.Count == 0)
         {
             return FormattedResponse<List<string>>.Error("没有文件上传", 204);
         }

         // 返回上传文件路径列表
         return FormattedResponse<List<string>>.Success("上传成功",serverFilePathList);
     }

     /// <summary>
     /// 生成具有指定扩展名的唯一文件名。
     /// </summary>
     /// <param name="extension">文件扩展名。</param>
     /// <returns>带有提供的扩展名的唯一文件名。</returns>
     private static string GetUniqueFileName(string extension)
     {
         var fileName = Path.GetRandomFileName() + extension; // 生成随机文件名并附加扩展名
         return fileName;
     }

     /// <summary>
     /// 检查提供的文件扩展名是否被允许。
     /// </summary>
     /// <param name="extension">文件扩展名。</param>
     /// <returns>如果文件扩展名被允许，则返回 true；否则返回 false。</returns>
     private static bool IsValidFileType(string extension)
     {
         // 定义允许的文件类型扩展名
         var allowedExtensions = new[] { ".jpg", ".png", ".gif", ".txt", ".pdf" };
         return allowedExtensions.Contains(extension.ToLower()); // 检查扩展名是否在允许的列表中
     }
 }
/// <summary>
/// 静态类扩展
/// </summary>
 public static class ContentDispositionHeaderValueExtensions
 {
     /// <summary>
     /// 检查Content-Disposition头是否表示文件上传。
     /// </summary>
     /// <param name="header">Content-Disposition头。</param>
     /// <returns>如果头表示文件上传，则返回 true；否则返回 false。</returns>
     public static bool IsFileDisposition(this ContentDispositionHeaderValue? header)
     {
         return header != null && !string.IsNullOrEmpty(header.FileName.Value); // 检查是否存在文件名
     }
}