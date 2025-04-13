using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterFace
{
    public interface ISqlSugerService
    {
        /// <summary>
        /// 创建数据库表
        /// 如果数据库不存在，则会创建数据库
        /// </summary>
        Task<bool> CreateTableAsync();
    }
}
