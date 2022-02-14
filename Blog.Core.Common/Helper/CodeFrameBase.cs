using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using SqlSugar;
namespace Blog.Core.Common.Helper
{
    #region Model
    [Serializable]
    public class ModelFrameClass
    {
        [SugarColumn(IsIgnore = true)]
        public int Index { get; set; }

    }


    [Serializable]
    public class QueryFrameClass<T>
    {
        /// <summary>
        /// 第几页 从0开始
        /// </summary>
        public int page { get; set; } = 0;
        /// <summary>
        /// 每页记录数量
        /// </summary>
        public int pageSize { get; set; } = 20;
        /// <summary>
        /// 查询字段
        /// </summary>
        public string columns { get; set; } = "";
        /// <summary>
        /// 排序方式
        /// </summary>
        public string OrderBy { get; set; } = "";
        /// <summary>
        /// 最前多少条
        /// </summary>
        public string TopNum { get; set; } = "";
        /// <summary>
        /// 添加其他参数数据
        /// </summary>
        /// <param name="condition"></param>
        public virtual void AddOtherWhere(List<Expression<Func<T, bool>>> condition)
        {
            return;
        }
    }
    #endregion
}
