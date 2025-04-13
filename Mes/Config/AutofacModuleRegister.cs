using Autofac;
using InterFace;
using Service;
using System.Reflection;
using SqlSugar;
using Model.Entity;
using Model.Other;
using Service.ConfigService;

namespace Mes.Config
{
    public class AutofacModuleRegister(IConfiguration configuration) : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            var interfacAssembly = Assembly.Load("InterFace");
            var serviceAssembly = Assembly.Load("Service");

            // 注册 IHttpContextAccessor 为单例
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();

            // 注册 CustomJWTService 为单例
            builder.RegisterType<CustomJwtService>().As<ICustomJwtService>().SingleInstance();

            // 注册 ISqlSugarClient 为单例
            builder.Register(c => ConfigureSqlSugar(configuration, c.Resolve<IHttpContextAccessor>()))
                .As<ISqlSugarClient>()
                .SingleInstance();

            // 将数据服务 ISqlSugerService 注册为单例
            builder.RegisterType<SqlSugarService>().As<ISqlSugerService>().SingleInstance();

            // 注册其他服务为 Scoped
            builder.RegisterAssemblyTypes(interfacAssembly, serviceAssembly)
                .Where(t => t.IsClass 
                            && t.Name.EndsWith("Service") 
                            && t.Name != "SqlSugerService" 
                            && t.Name != "CustomJwtService") // 排除 CustomJwtService
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }

        private static SqlSugarClient ConfigureSqlSugar(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            try
            {
                // 获取数据库连接字符串
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentException("配置中的数据库连接字符串不能为空或 null", nameof(configuration));
                // 创建 SqlSugarClient 实例
                SqlSugarClient db = new(
                    new ConnectionConfig
                    {
                        ConnectionString = connectionString,
                        DbType = DbType.PostgreSQL,
                        IsAutoCloseConnection = true,
                        InitKeyType = InitKeyType.Attribute,
                    }
                );
                //过滤软删除
                db.QueryFilter.AddTableFilter<AuditedEntity>(it => it.IsDeleted == false);
                //配置AOP拦截器---创建   更新    软删除
                db.Aop.DataExecuting =(_,  dataInfo) =>
                {
                    var userId = Guid.Empty;
                    var userName= string.Empty;
                    if (contextAccessor.HttpContext != null)
                    {
                        var claimId = contextAccessor.HttpContext.User.Claims.FirstOrDefault(p => p.Type.Equals("Id"));
                        if(claimId==null) return;
                        userId = Guid.Parse(claimId.Value);
                        var claimUserName = contextAccessor.HttpContext.User.Claims.FirstOrDefault(p => p.Type.Equals("Username"));
                        userName = claimUserName != null ? claimUserName.Value : string.Empty;
                    }
                    switch (dataInfo)
                    {
                        case { OperationType: DataFilterType.InsertByObject, EntityValue: AuditedEntity objIns }:
                            objIns.CreateTime = DateTime.Now;
                            objIns.CreateUserId = userId;
                            objIns.CreateUserName = userName;
                            objIns.Id = SequentialGuidGenerator.Instance.GenerateSequentialGuid();
                            break;

                        case { OperationType: DataFilterType.UpdateByObject, EntityValue: AuditedEntity objUpd }:
                            objUpd.UpdateTime = DateTime.Now;
                            objUpd.UpdateUserId = userId;
                            objUpd.UpdateUserName = userName;
                            break;

                        case { OperationType: DataFilterType.DeleteByObject, EntityValue: AuditedEntity objDel }:
                            objDel.IsDeleted = true;
                            objDel.DeletedTime = DateTime.Now;
                            objDel.DeletedUserId = userId;
                            objDel.DeletedUserName = userName;
                            break;
                    }
                };
                // 返回配置好的 SqlSugarClient 实例
                return db;
            }
            catch (Exception ex)
            {
                // 如果配置 SqlSugar 失败，抛出异常
                throw new InvalidOperationException("配置 SqlSugar 失败", ex);
            }
        }
    }
}
