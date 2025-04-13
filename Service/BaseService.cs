using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Service;

public abstract class BaseService(ISqlSugarClient sqlSugar,IHttpContextAccessor contextAccessor)
{
    protected  ISqlSugarClient SqlSugar { get; init; }= sqlSugar;
    private  IHttpContextAccessor HttpContextAccessor { get; init; }=contextAccessor;
    protected Guid UserId
    {
        get
        {
            var userId = HttpContextAccessor.HttpContext?.User.FindFirstValue("UserId");
            return userId is null ? Guid.Empty : Guid.Parse(userId);
        }
    }
    protected string UserName
    {
        get
        {
            var userName = HttpContextAccessor.HttpContext?.User.FindFirstValue("UserName");
            return userName ?? string.Empty;
        }
    }
}