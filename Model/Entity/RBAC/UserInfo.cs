using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace Model.Entity.RBAC
{
    /// <summary>
    /// 用户信息表
    /// </summary>
    [SugarTable("UserInfo")] // 指定对应的数据库表名
    public class UserInfo : AuditedEntity
    {

        /// <summary>
        /// 用户编码
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = false, UniqueGroupNameList = [ "UniqueCode"])]
        public string Code { get; set; } = null!;

        /// <summary>
        /// 密码
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = false)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// 用户名
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = true)]
        public string? Username { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 20, IsNullable = true)]
        public string? IdCard { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = false, UniqueGroupNameList = ["UniqueCode"])]
        public string Email { get; set; } = null!;

        /// <summary>
        /// 电话号码
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 20, IsNullable = true)]
        public string? Phone { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true)]
        public string? Address { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true)]
        public string? Avatar { get; set; }
    }
}
