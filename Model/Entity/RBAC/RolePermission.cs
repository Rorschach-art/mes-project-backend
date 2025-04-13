using SqlSugar;

namespace Model.Entity.RBAC;

public class RolePermission : AuditedEntity
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnDataType = "uuid", IsNullable = false)]
    public Guid RoleId { get; set; }

    /// <summary>
    /// 权限ID
    /// </summary>
    [SugarColumn(ColumnDataType = "uuid", IsNullable = false)]
    public Guid PermissionId { get; set; }
}