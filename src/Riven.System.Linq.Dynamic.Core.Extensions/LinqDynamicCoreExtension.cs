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
        static char[] FieldSplitChars { get; set; }
        static char[] ArraySplitChar { get; set; }

        static LinqDynamicCoreExtensions()
        {
            FieldSplitChars = new char[] { FieldSplitChar };
            ArraySplitChar = new char[] { '|' };
        }


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


            foreach (var condition in conditions)
            {
                // 多字段列
                if (condition.Field.Contains(FieldSplitChar))
                {
                    query = query.Where(condition.AsEnumerable());
                    continue;
                }

                // 值等于空或空字符串
                if (string.IsNullOrWhiteSpace(condition.Value))
                {

                    // 如果要跳过为空
                    if (condition.SkipValueIsNull)
                    {
                        continue;
                    }

                    // 如果操作类型不是 "相等" 或 "不相等"
                    if (condition.Operator == QueryOperator.Equal
                        || condition.Operator == QueryOperator.NotEqual)
                    {
                        switch (condition.Operator)
                        {
                            case QueryOperator.Equal:
                                query = query.Where($"{condition.Field} == null", condition.Value);
                                break;
                            case QueryOperator.NotEqual:
                                query = query.Where($"{condition.Field} != null", condition.Value);
                                break;

                        }
                    }
                    continue;
                }

                // 值不为空时
                switch (condition.Operator)
                {
                    case QueryOperator.Equal:
                        query = query.Where($"{condition.Field} == @0", condition.Value);
                        break;
                    case QueryOperator.NotEqual:
                        query = query.Where($"{condition.Field} != @0", condition.Value);
                        break;
                    case QueryOperator.Greater:
                        query = query.Where($"{condition.Field} > @0", condition.Value);
                        break;
                    case QueryOperator.GreaterEqual:
                        query = query.Where($"{condition.Field} >= @0", condition.Value);
                        break;
                    case QueryOperator.Less:
                        query = query.Where($"{condition.Field} < @0", condition.Value);
                        break;
                    case QueryOperator.LessEqual:
                        query = query.Where($"{condition.Field} <= @0", condition.Value);
                        break;
                    case QueryOperator.StartsWith:
                        query = query.Where($"{condition.Field}.StartsWith(@0)", condition.Value);
                        break;
                    case QueryOperator.EndsWith:
                        query = query.Where($"{condition.Field}.EndsWith(@0)", condition.Value);
                        break;
                    case QueryOperator.In:
                        query = query.WhereIn(condition);
                        break;
                    case QueryOperator.NotIn:
                        query = query.WhereNotIn(condition);
                        break;
                    case QueryOperator.Contains:
                        query = query.Where($"{condition.Field}.Contains(@0)", condition.Value);
                        break;
                    case QueryOperator.Between:
                        {
                            var values = condition.Value?.Split(
                               ArraySplitChar,
                               StringSplitOptions.RemoveEmptyEntries
                            );
                            if (values == null || values.Length != 2)
                            {
                                throw new ArgumentException(
                                        $"Incorrect number of filter values after splitting: {condition.Value}"
                                    );
                            }

                            query = query.Where(
                                    $"{condition.Field} > @0 and {condition.Field} < @1",
                                    values[0],
                                    values[1]
                                );
                        }
                        break;
                    case QueryOperator.BetweenEqualStart:
                        {
                            var values = condition.Value?.Split(
                               ArraySplitChar,
                               StringSplitOptions.RemoveEmptyEntries
                            );
                            if (values == null || values.Length != 2)
                            {
                                throw new ArgumentException(
                                        $"Incorrect number of filter values after splitting: {condition.Value}"
                                    );
                            }
                            query = query.Where(
                                    $"{condition.Field} >= @0 and {condition.Field} < @1",
                                    values[0],
                                    values[1]
                                );
                        }
                        break;
                    case QueryOperator.BetweenEqualEnd:
                        {
                            var values = condition.Value?.Split(
                               ArraySplitChar,
                               StringSplitOptions.RemoveEmptyEntries
                            );
                            if (values == null || values.Length != 2)
                            {
                                throw new ArgumentException(
                                        $"Incorrect number of filter values after splitting: {condition.Value}"
                                    );
                            }
                            query = query.Where(
                                    $"{condition.Field} > @0 and {condition.Field} <= @1",
                                    values[0],
                                    values[1]
                                );
                        }
                        break;
                    case QueryOperator.BetweenEqualStartAndEnd:
                        {
                            var values = condition.Value?.Split(
                               ArraySplitChar,
                               StringSplitOptions.RemoveEmptyEntries
                            );
                            if (values == null || values.Length != 2)
                            {
                                throw new ArgumentException(
                                        $"Incorrect number of filter values after splitting: {condition.Value}"
                                    );
                            }
                            query = query.Where(
                                    $"{condition.Field} >= @0 and {condition.Field} <= @1",
                                    values[0],
                                    values[1]
                                );
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unsupported filter operation type: {condition.Operator}");
                }

            }


            return query;
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

            var values = condition.Value.Split(
                    ArraySplitChar,
                    StringSplitOptions.RemoveEmptyEntries
                );
            if (values == null || values.Length == 0)
            {
                return query;
            }

            var inFilter = new StringBuilder();
            var index = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    inFilter.Append(" or ");
                }
                inFilter.Append($"{condition.Field} == @{index++}");
            }


            return query.Where(inFilter.ToString(), values);
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

            var values = condition.Value.Split(
                    ArraySplitChar,
                    StringSplitOptions.RemoveEmptyEntries
                );
            if (values == null || values.Length == 0)
            {
                return query;
            }

            var inFilter = new StringBuilder();
            var index = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    inFilter.Append(" and ");
                }
                inFilter.Append($"{condition.Field} != @{index++}");
            }


            return query.Where(inFilter.ToString(), values);
        }

        /// <summary>
        /// 多字段表达式转表达式迭代器
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<QueryCondition> AsEnumerable(this QueryCondition condition)
        {
            var fields = condition.Field
                .Split(FieldSplitChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrWhiteSpace(o));

            foreach (var field in fields)
            {
                yield return new QueryCondition()
                {
                    Field = field,
                    Operator = condition.Operator,
                    SkipValueIsNull = condition.SkipValueIsNull,
                    Value = condition.Value
                };
            }

            yield break;
        }

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


        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="query">查询器</param>
        /// <param name="pageNumber">页码</param>
        /// <param name="pageSize">页面数据总量</param>
        /// <returns>返回增加了分页的查询器</returns>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query.Page(pageNumber, pageSize);
        }
    }
}
