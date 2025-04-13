using SqlSugar;

namespace Model.Entity
{
    public abstract class AuditedEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, ColumnDataType = "uuid")]
        public Guid Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDataType = "timestamp", IsNullable = true, InsertServerTime = true)]
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 创建用户ID
        /// </summary>

        [SugarColumn(ColumnDataType = "uuid", IsNullable = true)]
        public Guid? CreateUserId { get; set; }

        /// <summary>
        /// 创建用户名
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = true)]
        public string? CreateUserName { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(ColumnDataType = "timestamp", IsNullable = true, UpdateServerTime = true)]
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新用户ID
        /// </summary>
        [SugarColumn(ColumnDataType = "uuid", IsNullable = true)]
        public Guid? UpdateUserId { get; set; }

        /// <summary>
        /// 更新用户名
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = true)]
        public string? UpdateUserName { get; set; }

        /// <summary>
        /// 是否已删除
        /// </summary>
        [SugarColumn(ColumnDataType = "boolean", IsNullable = false, DefaultValue = "false")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        [SugarColumn(ColumnDataType = "timestamp", IsNullable = true)]
        public DateTime? DeletedTime { get; set; }

        /// <summary>
        /// 删除用户ID
        /// </summary>
        [SugarColumn(ColumnDataType = "uuid", IsNullable = true)]
        public Guid? DeletedUserId { get; set; }

        /// <summary>
        /// 删除用户名
        /// </summary>
        [SugarColumn(ColumnDataType = "varchar", Length = 100, IsNullable = true)]
        public string? DeletedUserName { get; set; }
    }
}
