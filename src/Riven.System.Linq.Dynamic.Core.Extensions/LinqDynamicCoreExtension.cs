using System;
using System.Linq;
using System.Collections;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    /// <summary>
    /// 对System.Linq.Dynamic.Core的扩展函数
    /// </summary>
    public static class LinqDynamicCoreExtensions
    {
        const char FieldSplitChar = ',';
        const char ArraySplitChar = '|';
        static char[] FieldSplitChars { get; set; }
        static char[] ArraySplitChars { get; set; }

        static LinqDynamicCoreExtensions()
        {
            FieldSplitChars = new char[] { FieldSplitChar };
            ArraySplitChars = new char[] { ArraySplitChar };
        }

        #region 筛选

        /// <summary>
        /// 根据筛选条件对象创建筛选
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">查询器</param>
        /// <param name="queryConditions">查询筛选条件</param>
        /// <returns>返回增加了筛选条件的查询器</returns>
        public static IQueryable<T> Where<T>(this IQueryable<T> query, IEnumerable<QueryCondition> queryConditions)
        {
            if (query == null
                || queryConditions == null
                || queryConditions.Count() == 0)
            {
                return query;
            }

            var conditions = queryConditions.Where(o => !string.IsNullOrWhiteSpace(o.Field));
            if (conditions.Count() == 0)
            {
                return query;
            }

            var filterTextBuilder = new StringBuilder();
            var args = new List<object>();


            var condationList = conditions.ToList();
            var latestIndex = condationList.Count - 1;
            for (int i = 0; i < condationList.Count; i++)
            {
                var condition = condationList[i];
                var filterText = condition.ToString(args);
                if (string.IsNullOrWhiteSpace(filterText))
                {
                    continue;
                }
                filterTextBuilder.Append("(");
                filterTextBuilder.Append(filterText);
                filterTextBuilder.Append(")");
                if (i != latestIndex)
                {
                    filterTextBuilder.Append(" and ");
                }
            }

            return query.Where(
                filterTextBuilder.ToString(),
                args.ToArray()
                );
        }


        /// <summary>
        /// 筛选存在筛选数组中的数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">查询器</param>
        /// <param name="condition">筛选条件</param>
        /// <returns>返回增加了查询条件的查询器</returns>
        public static IQueryable<T> WhereIn<T>(this IQueryable<T> query, QueryCondition condition)
        {
            if (query == null
                || condition == null
                || string.IsNullOrWhiteSpace(condition.Field)
                || string.IsNullOrWhiteSpace(condition.Value)
                || condition.Operator != QueryOperator.In)
            {
                return query;
            }


            var args = new List<object>();
            var filterString = condition.ToString(args);

            if (string.IsNullOrEmpty(filterString))
            {
                return query;
            }

            return query.Where(filterString, args);
        }

        /// <summary>
        /// 筛选不存在筛选数组中的数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">查询器</param>
        /// <param name="condition">筛选条件</param>
        /// <returns>返回增加了查询条件的查询器</returns>
        public static IQueryable<T> WhereNotIn<T>(this IQueryable<T> query, QueryCondition condition)
        {
            if (query == null
                || condition == null
                || string.IsNullOrWhiteSpace(condition.Field)
                || string.IsNullOrWhiteSpace(condition.Value)
                || condition.Operator != QueryOperator.NotIn)
            {
                return query;
            }

            var args = new List<object>();
            var filterString = condition.ToString(args);

            if (string.IsNullOrEmpty(filterString))
            {
                return query;
            }

            return query.Where(filterString, args);
        }


        /// <summary>
        /// 查询表达式转字符串，并将值添加到参数数组中
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ToString(this QueryCondition condition, List<object> args)
        {
            var result = string.Empty;

            // 值等于空或空字符串
            if (string.IsNullOrWhiteSpace(condition.Value))
            {
                // 如果要跳过为空
                if (condition.SkipValueIsNull)
                {
                    return string.Empty;
                }

                // 如果操作类型不是 "相等" 或 "不相等"
                if (condition.Operator == QueryOperator.Equal
                    || condition.Operator == QueryOperator.NotEqual)
                {
                    switch (condition.Operator)
                    {
                        case QueryOperator.Equal:
                            result = $"{condition.Field} == null";
                            break;
                        case QueryOperator.NotEqual:
                            result = $"{condition.Field} != null";
                            break;
                    }
                }
                return result;
            }


            // 值不为空时
            switch (condition.Operator)
            {
                case QueryOperator.Equal:
                    result = $"{condition.Field} == @{args.Count}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.NotEqual:
                    result = $"{condition.Field} != @{args.Count}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.Greater:
                    result = $"{condition.Field} > @{args.Count}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.GreaterEqual:
                    result = $"{condition.Field} >= @{args.Count}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.Less:
                    result = $"{condition.Field} < @{args.Count}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.LessEqual:
                    result = $"{condition.Field} <= @{args.Count}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.StartsWith:
                    result = $"{condition.Field}.StartsWith(@{args.Count})";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.EndsWith:
                    result = $"{condition.Field}.EndsWith(@{args.Count})";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.Contains:
                    result = $"{condition.Field}.Contains(@{args.Count})";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.Between:
                    {
                        var values = condition.Value?.Split(
                           ArraySplitChars,
                           StringSplitOptions.RemoveEmptyEntries
                        );
                        if (values == null || values.Length != 2)
                        {
                            throw new ArgumentException(
                                    $"Incorrect number of filter values after splitting: {condition.Value}"
                                );
                        }
                        result = $"{condition.Field} > @{args.Count} and {condition.Field} < @{args.Count + 1}";
                        args.Add(values[0]);
                        args.Add(values[1]);
                    }
                    break;
                case QueryOperator.BetweenEqualStart:
                    {
                        var values = condition.Value?.Split(
                           ArraySplitChars,
                           StringSplitOptions.RemoveEmptyEntries
                        );
                        if (values == null || values.Length != 2)
                        {
                            throw new ArgumentException(
                                    $"Incorrect number of filter values after splitting: {condition.Value}"
                                );
                        }
                        result = $"{condition.Field} >= @{args.Count} and {condition.Field} < @{args.Count + 1}";
                        args.Add(values[0]);
                        args.Add(values[1]);
                    }
                    break;
                case QueryOperator.BetweenEqualEnd:
                    {
                        var values = condition.Value?.Split(
                           ArraySplitChars,
                           StringSplitOptions.RemoveEmptyEntries
                        );
                        if (values == null || values.Length != 2)
                        {
                            throw new ArgumentException(
                                    $"Incorrect number of filter values after splitting: {condition.Value}"
                                );
                        }

                        result = $"{condition.Field} > @{args.Count} and {condition.Field} <= @{args.Count + 1}";
                        args.Add(values[0]);
                        args.Add(values[1]);
                    }
                    break;
                case QueryOperator.BetweenEqualStartAndEnd:
                    {
                        var values = condition.Value?.Split(
                           ArraySplitChars,
                           StringSplitOptions.RemoveEmptyEntries
                        );
                        if (values == null || values.Length != 2)
                        {
                            throw new ArgumentException(
                                    $"Incorrect number of filter values after splitting: {condition.Value}"
                                );
                        }
                        result = $"{condition.Field} >= @{args.Count} and {condition.Field} <= @{args.Count + 1}";
                        args.Add(values[0]);
                        args.Add(values[1]);
                    }
                    break;
                case QueryOperator.In:
                    {
                        var values = condition.Value.Split(
                                ArraySplitChars,
                                StringSplitOptions.RemoveEmptyEntries
                        );

                        if (values == null || values.Length == 0)
                        {
                            return string.Empty;
                        }

                        var inFilter = new StringBuilder();
                        var latestIndex = values.Length - 1;
                        for (int i = 0; i < values.Length; i++)
                        {
                            inFilter.Append($"{condition.Field} == @{args.Count}");
                            if (i != latestIndex)
                            {
                                inFilter.Append(" or ");
                            }
                            args.Add(values[i]);
                        }

                        result = inFilter.ToString();
                    }
                    break;
                case QueryOperator.NotIn:
                    {
                        var conditionTmp = new QueryCondition()
                        {
                            Field = condition.Field,
                            Operator = QueryOperator.In,
                            SkipValueIsNull = true,
                            Value = condition.Value,
                        };

                        result = conditionTmp.ToString(args)
                             .Replace(" == ", " != ")
                             .Replace(" or ", " and ");
                    }
                    break;
                default:
                    throw new ArgumentException($"Unsupported filter operation type: {condition.Operator}");
            }

            return result;
        }

        #endregion


        #region 排序

        /// <summary>
        /// 根据排序条件进行排序
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">查询器</param>
        /// <param name="sortConditions">排序条件</param>
        /// <returns>返回增加了排序条件的查询器</returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, IEnumerable<SortCondition> sortConditions)
        {
            if (query == null || sortConditions == null || sortConditions.Count() == 0)
            {
                return query;
            }


            var orderedSortConditions = sortConditions
                .Where(o => o.Type != SortType.None)
                .OrderBy(o => o.Order)
                .ToList();

            if (orderedSortConditions.Count == 0)
            {
                return query;
            }

            var ordersStr = new StringBuilder();
            for (int i = 0; i < orderedSortConditions.Count; i++)
            {

                if (i > 0)
                {
                    ordersStr.Append(" ,");
                }

                ordersStr.Append(orderedSortConditions[i].Field);
                ordersStr.Append(
                       orderedSortConditions[i].Type == SortType.Asc ? " asc" : " desc"
                    ); ;
            }

            return query.OrderBy(ordersStr.ToString());

        }

        #endregion
    }
}
