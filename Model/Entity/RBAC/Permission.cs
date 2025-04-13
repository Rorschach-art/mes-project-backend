using SqlSugar;

namespace Model.Entity.RBAC;

public class Permission : AuditedEntity
{
    /// <summary>
    /// 权限代码（唯一）
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = false, UniqueGroupNameList = ["Permission"])]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 权限名称（唯一）
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = false, UniqueGroupNameList = ["Permission"])]
    public string Name { get; set; }= null!;

    /// <summary>
    /// 权限描述
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string Description { get; set; }= null!;
}