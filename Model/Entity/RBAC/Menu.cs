using SqlSugar;

namespace Model.Entity.RBAC;

public class Menu : AuditedEntity
{
    /// <summary>
    /// 菜单代码（唯一）
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = false, UniqueGroupNameList = ["Menu"])]
    public string Code { get; set; }= null!;

    /// <summary>
    /// 菜单名称（唯一）
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = false, UniqueGroupNameList = ["Menu"])]
    public string Name { get; set; }= null!;

    /// <summary>
    /// 父菜单ID
    /// </summary>
    [SugarColumn(ColumnDataType = "uuid", IsNullable = true)]
    public Guid ParentId { get; set; }=Guid.Empty;

    /// <summary>
    /// 菜单图标
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 菜单URL
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string Url { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDataType = "int", IsNullable = false, DefaultValue = "0")]
    public int Sort { get; set; }
}