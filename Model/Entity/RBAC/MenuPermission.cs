using SqlSugar;

namespace Model.Entity.RBAC;

public class MenuPermission : AuditedEntity
{
    /// <summary>
    /// 菜单ID
    /// </summary>
    [SugarColumn(ColumnDataType = "uuid", IsNullable = false)]
    public Guid MenuId { get; set; }

    /// <summary>
    /// 权限ID
    /// </summary>
    [SugarColumn(ColumnDataType = "uuid", IsNullable = false)]
    public Guid PermissionId { get; set; }
}