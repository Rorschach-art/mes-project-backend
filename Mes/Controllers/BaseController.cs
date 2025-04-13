using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mes.Controllers
{ // 定义一个基础控制器类，处理API请求
    [Route("Mesapi/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class BaseController : ControllerBase
    {
        // 此处可以添加公共方法或属性，供继承自BaseController的控制器使用
    }
}
