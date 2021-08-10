using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System.Linq
{
    public class QueryPredicate
    {
        public const char FieldSplitChar = ',';
        public static char[] FieldSplitChars { get; } = new char[] { FieldSplitChar };
        public static char[] ArraySplitChar { get; } = new char[] { '|' };


        public string PredicateString { get; protected set; }

        public object[] Args { get; protected set; }


        protected QueryPredicate()
        {

        }

        /// <summary>
        /// 创建表达式对象
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static QueryPredicate Create(QueryCondition condition, ref int index)
        {
            var args = new List<object>();

            var predicateString = CreatePredicateString(condition, ref index, ref args);
            if (string.IsNullOrWhiteSpace(predicateString))
            {
                return null;
            }
            var predicate = new QueryPredicate();
            predicate.PredicateString = predicateString;
            predicate.Args = args.ToArray();

            return predicate;
        }

        /// <summary>
        /// 生成字段过滤表达式
        /// </summary>
        /// <param name="condition">表达式</param>
        /// <param name="index">参数索引</param>
        /// <param name="args">参数集合</param>
        /// <returns>过滤表达式</returns>
        public static string CreatePredicateString(QueryCondition condition, ref int index, ref List<object> args)
        {
            // 多字段列
            if (condition.Field.Contains(FieldSplitChar))
            {
                return CreatePredicateString(
                    QueryConditionAsEnumerable(condition),
                    condition.JoinString(),
                    ref index,
                    ref args
                    );
            }


            var predicateString = string.Empty;
            // 值等于空或空字符串
            if (string.IsNullOrWhiteSpace(condition.Value))
            {

                // 如果要跳过为空
                if (condition.SkipValueIsNull)
                {
                    return predicateString;
                }

                // 如果操作类型不是 "相等" 或 "不相等"
                if (condition.Operator == QueryOperator.Equal
                    || condition.Operator == QueryOperator.NotEqual)
                {
                    switch (condition.Operator)
                    {
                        case QueryOperator.Equal:
                            predicateString = $"{condition.Field} == null";
                            break;
                        case QueryOperator.NotEqual:
                            predicateString = $"{condition.Field} != null";
                            break;

                    }
                }
                return predicateString;
            }

            // 值不为空时
            switch (condition.Operator)
            {
                case QueryOperator.Equal:
                    predicateString = $"{condition.Field} == @{index++}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.NotEqual:
                    predicateString = $"{condition.Field} != @{index++}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.Greater:
                    predicateString = $"{condition.Field} > @{index++}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.GreaterEqual:
                    predicateString = $"{condition.Field} >= @{index++}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.Less:
                    predicateString = $"{condition.Field} < @{index++}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.LessEqual:
                    predicateString = $"{condition.Field} <= @{index++}";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.StartsWith:
                    predicateString = $"{condition.Field}.StartsWith(@{index++})";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.EndsWith:
                    predicateString = $"{condition.Field}.EndsWith(@{index++})";
                    args.Add(condition.Value);
                    break;
                case QueryOperator.In:
                    predicateString = CreatePredicateStringWhereIn(condition, ref index, ref args);
                    break;
                case QueryOperator.NotIn:
                    predicateString = CreatePredicateStringWhereNotIn(condition, ref index, ref args);
                    break;
                case QueryOperator.Contains:
                    predicateString = $"{condition.Field}.Contains(@{index++})";
                    args.Add(condition.Value);
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

                        predicateString = $"{condition.Field} > @{index++} and {condition.Field} < @{index++}";
                        args.Add(values[0]);
                        args.Add(values[1]);
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
                        predicateString = $"{condition.Field} >= @{index++} and {condition.Field} < @{index++}";
                        args.Add(values[0]);
                        args.Add(values[1]);
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
                        predicateString = $"{condition.Field} > @{index++} and {condition.Field} <= @{index++}";
                        args.Add(values[0]);
                        args.Add(values[1]);
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
                        predicateString = $"{condition.Field} >= @{index++} and {condition.Field} <= @{index++}";
                        args.Add(values[0]);
                        args.Add(values[1]);
                    }
                    break;
                default:
                    throw new ArgumentException($"Unsupported filter operation type: {condition.Operator}");
            }

            return $" {predicateString} ";
        }

        /// <summary>
        /// 生成多字段过滤表达式
        /// </summary>
        /// <param name="conditions">表达式迭代器</param>
        /// <param name="join">拼接的参数，"or" 或 "and" </param>
        /// <param name="index">参数索引</param>
        /// <param name="args">参数集合</param>
        /// <returns>过滤表达式</returns>
        public static string CreatePredicateString(IEnumerable<QueryCondition> conditions, string join, ref int index, ref List<object> args)
        {
            var filterBuilder = new StringBuilder();

            var i = 0;
            foreach (var item in conditions)
            {
                if (i > 0)
                {
                    filterBuilder.Append(join);
                }
                filterBuilder.Append(
                    CreatePredicateString(item, ref index, ref args)
                    );
                args.Add(item.Value);
                i++;
            }

            return filterBuilder.ToString();
        }

        /// <summary>
        /// 生成 in 字段过滤表达式
        /// </summary>
        /// <param name="condition">表达式</param>
        /// <param name="index">参数索引</param>
        /// <param name="args">参数集合</param>
        /// <returns>过滤表达式</returns>
        public static string CreatePredicateStringWhereIn(QueryCondition condition, ref int index, ref List<object> args)
        {
            if (condition == null
              || string.IsNullOrWhiteSpace(condition.Field)
              || string.IsNullOrWhiteSpace(condition.Value)
              || condition.Operator != QueryOperator.In)
            {
                return string.Empty;
            }

            var values = condition.Value.Split(
                    ArraySplitChar,
                    StringSplitOptions.RemoveEmptyEntries
                );
            if (values == null || values.Length == 0)
            {
                return string.Empty;
            }

            var inFilter = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    inFilter.Append(" or ");
                }
                inFilter.Append($"{condition.Field} == @{index++}");
                args.Add(values[i]);
            }


            return inFilter.ToString();
        }

        /// <summary>
        /// 生成 not in 字段过滤表达式
        /// </summary>
        /// <param name="condition">表达式</param>
        /// <param name="index">参数索引</param>
        /// <param name="args">参数集合</param>
        /// <returns>过滤表达式</returns>
        public static string CreatePredicateStringWhereNotIn(QueryCondition condition, ref int index, ref List<object> args)
        {
            if (condition == null
                || string.IsNullOrWhiteSpace(condition.Field)
                || string.IsNullOrWhiteSpace(condition.Value)
                || condition.Operator != QueryOperator.NotIn)
            {
                return string.Empty;
            }

            var values = condition.Value.Split(
                    ArraySplitChar,
                    StringSplitOptions.RemoveEmptyEntries
                );
            if (values == null || values.Length == 0)
            {
                return string.Empty;
            }

            var notInFilter = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    notInFilter.Append(" and ");
                }
                notInFilter.Append($"{condition.Field} != @{index++}");
                args.Add(values[i]);
            }


            return notInFilter.ToString();
        }


        /// <summary>
        /// 多字段筛选条件转多条件
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IEnumerable<QueryCondition> QueryConditionAsEnumerable(QueryCondition condition)
        {
            var fieldString = condition.Field
                .Split(ArraySplitChar, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .LastOrDefault();

            var fields = fieldString
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
    }
}
