using SqlSugar;

namespace Model.Entity.RBAC;

public class UserRole : AuditedEntity
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnDataType = "uuid", IsNullable = false)]
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnDataType = "uuid", IsNullable = false)]
    public Guid RoleId { get; set; }
}