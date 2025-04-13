using System.Reflection;
using InterFace;
using Microsoft.AspNetCore.Http;
using Model.Entity;
using SqlSugar;

namespace Service.ConfigService
{
    /// <summary>
    /// SqlSugar服务类，用于管理数据库操作
    /// </summary>
    /// <param name="sqlSugar">SqlSugar客户端接口，用于执行数据库操作</param>
    public class SqlSugarService(ISqlSugarClient sqlSugar,IHttpContextAccessor contextAccessor) :BaseService(sqlSugar, contextAccessor), ISqlSugerService
    {


        /// <summary>
        /// 创建数据库表
        /// 如果数据库不存在，则会创建数据库
        /// </summary>
        public async Task<bool> CreateTableAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    SqlSugar.DbMaintenance.CreateDatabase(); // 如果没有数据库则新建

                    // 使用反射获取所有继承了 AuditedEntity 的类
                    var entityTypes = Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(type =>
                            typeof(AuditedEntity).IsAssignableFrom(type)
                            && type is { IsAbstract: false, IsInterface: false }
                        )
                        .ToArray();

                    SqlSugar.CodeFirst.SetStringDefaultLength(100).BackupTable().InitTables(entityTypes);
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
