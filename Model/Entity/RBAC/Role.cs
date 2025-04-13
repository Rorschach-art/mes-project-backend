using SqlSugar;

namespace Model.Entity.RBAC;

public class Role : AuditedEntity
{
    /// <summary>
    /// 角色代码（唯一）
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = false, UniqueGroupNameList = ["role"])]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 角色名称（唯一）
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = false, UniqueGroupNameList = ["role"])]
    public string Name { get; set; }= null!;

    /// <summary>
    /// 角色描述
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string Description { get; set; }= null!;
}