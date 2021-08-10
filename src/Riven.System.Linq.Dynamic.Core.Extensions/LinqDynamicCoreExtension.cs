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



        /// <summary>
        /// 根据筛选条件对象创建筛选
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="query">查询器</param>
        /// <param name="queryConditions">查询筛选条件</param>
        /// <returns>返回增加了筛选条件的查询器</returns>
        public static IQueryable<T> Where<T>(this IQueryable<T> query, IEnumerable<QueryCondition> queryConditions)
        {

            // 判断筛选条件是否正确
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

            // 拼接表达式
            var predicateStringBuilder = new StringBuilder(); // 表达式字符串
            var argList = new List<object>(); // 参数集合
            var argIndex = 0;  // 参数计数器
         
            var i = 0;
            foreach (var condition in queryConditions)
            {
                var predicate = QueryPredicate.Create(condition, ref argIndex);
                if (predicate != null)
                {
                    if (i > 0)
                    {
                        predicateStringBuilder.Append(" and ");
                    }

                    predicateStringBuilder.Append($"({predicate.PredicateString})");
                    argList.AddRange(predicate.Args);
                    i++;
                }
            }

            return query.Where(predicateStringBuilder.ToString(), argList.ToArray());
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
