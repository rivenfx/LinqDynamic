using System;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    /// <summary>
    /// 排序条件
    /// </summary>
    public class SortCondition
    {
        /// <summary>
        /// 字段
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// 顺序,值越小越靠前
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 排序类型 <see cref="SortType"/>
        /// </summary>
        public SortType Type { get; set; }
    }
}
