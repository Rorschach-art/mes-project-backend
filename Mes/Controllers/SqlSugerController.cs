using InterFace;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Other;

namespace Mes.Controllers
{
    public class SqlSugerController (ISqlSugerService sugerService): BaseController
    {
        [HttpPost]
        [EndpointSummary("SqlSuger服务")]
        [EndpointDescription("数据服务接口")]
        public async Task<FormattedResponse<bool>> CreateTableAsync()
        {
          var result=  await sugerService.CreateTableAsync();
            if(result) return FormattedResponse<bool>.Success("创建成功", result);
            else return FormattedResponse<bool>.Error("创建失败", 500);
        }
    }
}
