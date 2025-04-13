using System.ComponentModel;

namespace Model.VO.Rbac;


public class MenuResult
{
    
    /// <summary>
    /// 菜单Id
    /// </summary>
    [Description("菜单Id")]
    public Guid Id { get; set; } 
    /// <summary>
    /// 菜单代码
    /// </summary>
    [Description("菜单代码")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 菜单名称（唯一）
    /// </summary>
    [Description("菜单名称")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 菜单图标
    /// </summary>
    [Description("菜单图标")]
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 菜单URL
    /// </summary>
    [Description("菜单URL")]
    public string Url { get; set; } = null!;
    /// <summary>
    /// 父级菜单Id
    /// </summary>
    [Description("父级菜单Id")]
    public Guid ParentId { get; set; }

    /// <summary>
    /// 菜单子项
    /// </summary>
    [Description("菜单子项")]
    public List<MenuResult> Children { get; set; } = [];
}

