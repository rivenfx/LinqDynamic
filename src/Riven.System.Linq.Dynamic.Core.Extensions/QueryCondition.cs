using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    /// <summary>
    /// 查询条件
    /// </summary>
    public class QueryCondition
    {
        public QueryCondition()
        {
            SkipValueIsNull = true;
        }

        /// <summary>
        /// 字段名称 
        /// xxx
        /// xxx.xxx
        /// (多字段使用 “,”分隔, 例如： xxx,xxx 或 or:xxx,xxx)
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 值
        /// 数组值使用 | 分隔： 1|2|3|..
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 查询类型
        /// </summary>
        public QueryOperator Operator { get; set; }


        /// <summary>
        /// 为true时,<see cref="Value"/>为空则跳过此条件
        /// 为false时,<see cref="Value"/>为空不跳过此条件
        /// </summary>
        public bool SkipValueIsNull { get; set; }

        public virtual string JoinString()
        {
            return Field.StartsWith("or|") ? "or" : "and";
        }
    }
}
