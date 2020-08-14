namespace System.Linq
{
    /// <summary>
    /// 查询操作类型
    /// </summary>
    public enum QueryOperator
    {


        /// <summary>
        /// 等于
        /// </summary>
        Equal = 0,
        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual = 1,

        /// <summary>
        /// 大于
        /// </summary>
        Greater = 2,
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterEqual = 3,

        /// <summary>
        /// 小于
        /// </summary>
        Less = 4,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessEqual = 5,

        /// <summary>
        /// 以xx开头
        /// </summary>
        StartsWith = 6,
        /// <summary>
        /// 以xx结尾
        /// </summary>
        EndsWith = 7,

        /// <summary>
        /// 存在于 [A, B, C, ...] 中
        /// </summary>
        In = 8,
        /// <summary>
        /// 不存在 [A, B, C, ...] 中
        /// </summary>
        NotIn = 9,
        /// <summary>
        /// 在 xxxx 中包含 a
        /// </summary>
        Contains = 10,

        /// <summary>
        /// 在 A 与B之间
        /// </summary>
        Between = 11,
        /// <summary>
        /// 在 A 与 B 之间,并包含  A 
        /// </summary>
        BetweenEqualStart = 12,
        /// <summary>
        /// 在 A 与B之间,并包含B
        /// </summary>
        BetweenEqualEnd = 13,
        /// <summary>
        /// 在 A 与B之间,并包含 A 与B
        /// </summary>
        BetweenEqualStartAndEnd = 14,

    }
}
