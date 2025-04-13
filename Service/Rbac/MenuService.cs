using InterFace;
using Microsoft.AspNetCore.Http;
using Model.Entity.RBAC;
using Model.VO.Rbac;
using SqlSugar;

namespace Service.Rbac;

public class MenuService(ISqlSugarClient sqlSugarClient, HttpContextAccessor contextAccessor)
    : BaseService(sqlSugarClient, contextAccessor), IMenuService
{
    /// <summary>
    /// 获取菜单列表
    /// </summary>
    /// <returns>树形菜单列表 </returns>
    public async Task<List<MenuResult>> GetMenuAsync()
    {
        if (UserId == Guid.Empty) return [];
        // 从数据库获取所有菜单项
        var list = await SqlSugar.Queryable<UserInfo, UserRole, Role, RolePermission, Permission, MenuPermission, Menu>
            (
                (user, userRole, role, rolePermission, permission, menuPermission, menu) => new object[]
                {
                    JoinType.Inner, user.Id == userRole.UserId,
                    JoinType.Inner, userRole.RoleId == role.Id,
                    JoinType.Inner, role.Id == rolePermission.RoleId,
                    JoinType.Inner, rolePermission.PermissionId == permission.Id,
                    JoinType.Inner, permission.Id == menuPermission.PermissionId,
                    JoinType.Inner, menuPermission.MenuId == menu.Id
                }
            )
            .Where((user, userRole, role, rolePermission, permission, menuPermission, menu) => user.Id == UserId)
            .OrderBy((user, userRole, role, rolePermission, permission, menuPermission, menu) => menu.Sort)
            .Select((user, userRole, role, rolePermission, permission, menuPermission, menu) => new MenuResult
            {
                Id = menu.Id,
                Code = menu.Code,
                Name = menu.Name,
                Icon = menu.Icon,
                Url = menu.Url,
                ParentId = menu.ParentId,
            })
            .ToListAsync();
        // 调用递归方法获取菜单列表
        return GetMenuTree(list, Guid.Empty);
    }

    /// <summary>
    /// 递归获取菜单列表及其子菜单
    /// </summary>
    /// <param name="list">所有菜单项的列表</param>
    /// <param name="parentId">当前处理的菜单的父ID</param>
    /// <returns>返回以当前父ID为上级的菜单列表</returns>
    private List<MenuResult> GetMenuTree(List<MenuResult> list, Guid parentId)
    {
        // 边界条件检查
        if (list is not { Count: not 0 })
        {
            return [];
        }

        // 过滤掉 ParentId 为 null 的项，并按 ParentId 分组
        var menuGroups = list
            .Where(x => x.ParentId == Guid.Empty)
            .GroupBy(x => x.ParentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 返回结果
        return GetChildren(parentId);
        // 核心逻辑：获取指定 parentId 的菜单项并设置其子菜单
        List<MenuResult> GetChildren(Guid id)
        {
            if (!menuGroups.TryGetValue(id, out var children))
            {
                return [];
            }

            foreach (var menu in children)
            {
                // 递归设置子菜单
                menu.Children = GetChildren(menu.Id);
            }
            return children;
        }
    }
}