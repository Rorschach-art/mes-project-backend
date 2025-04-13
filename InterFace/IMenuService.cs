using Model.VO.Rbac;

namespace InterFace;

public interface IMenuService
{
    Task<List<MenuResult>> GetMenuAsync();
}