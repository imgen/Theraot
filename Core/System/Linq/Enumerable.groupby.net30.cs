#if NET20 || NET30

using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return GroupBy(source, keySelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            LinqCheck.SourceAndKeySelector(source, keySelector);

            return CreateGroupByIterator(source, keySelector, comparer);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return GroupBy(source, keySelector, elementSelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            LinqCheck.SourceAndKeyElementSelectors(source, keySelector, elementSelector);

            return CreateGroupByIterator(source, keySelector, elementSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, elementSelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            LinqCheck.GroupBySelectors(source, keySelector, elementSelector, resultSelector);

            return CreateGroupByIterator(source, keySelector, elementSelector, resultSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
        {
            return GroupBy(source, keySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            LinqCheck.SourceAndKeyResultSelectors(source, keySelector, resultSelector);

            return CreateGroupByIterator(source, keySelector, resultSelector, comparer);
        }

        private static IEnumerable<IGrouping<TKey, TSource>> CreateGroupByIterator<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            var groups = new Dictionary<TKey, List<TSource>>(comparer);
            var nullList = new List<TSource>();
            int counter = 0;
            int nullCounter = -1;

            foreach (TSource element in source)
            {
                TKey key = keySelector(element);
                if (ReferenceEquals(key, null))
                {
                    nullList.Add(element);
                    if (nullCounter == -1)
                    {
                        nullCounter = counter;
                        counter++;
                    }
                }
                else
                {
                    List<TSource> group;
                    if (!groups.TryGetValue(key, out group))
                    {
                        group = new List<TSource>();
                        groups.Add(key, group);
                        counter++;
                    }
                    group.Add(element);
                }
            }

            counter = 0;
            foreach (var group in groups)
            {
                if (counter == nullCounter)
                {
                    yield return new Grouping<TKey, TSource>(default(TKey), nullList);
                    counter++;
                }

                yield return new Grouping<TKey, TSource>(group.Key, group.Value);
                counter++;
            }

            if (counter == nullCounter)
            {
                yield return new Grouping<TKey, TSource>(default(TKey), nullList);
                // counter++;
            }
        }

        private static IEnumerable<IGrouping<TKey, TElement>> CreateGroupByIterator<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            var groups = new Dictionary<TKey, List<TElement>>(comparer);
            var nullList = new List<TElement>();
            int counter = 0;
            int nullCounter = -1;

            foreach (TSource item in source)
            {
                TKey key = keySelector(item);
                TElement element = elementSelector(item);
                if (ReferenceEquals(key, null))
                {
                    nullList.Add(element);
                    if (nullCounter == -1)
                    {
                        nullCounter = counter;
                        counter++;
                    }
                }
                else
                {
                    List<TElement> group;
                    if (!groups.TryGetValue(key, out group))
                    {
                        group = new List<TElement>();
                        groups.Add(key, group);
                        counter++;
                    }
                    group.Add(element);
                }
            }

            counter = 0;
            foreach (var group in groups)
            {
                if (counter == nullCounter)
                {
                    yield return new Grouping<TKey, TElement>(default(TKey), nullList);
                    counter++;
                }

                yield return new Grouping<TKey, TElement>(group.Key, group.Value);
                counter++;
            }

            if (counter == nullCounter)
            {
                yield return new Grouping<TKey, TElement>(default(TKey), nullList);
                // counter++;
            }
        }

        private static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            IEnumerable<IGrouping<TKey, TElement>> groups = GroupBy(source, keySelector, elementSelector, comparer);

            foreach (IGrouping<TKey, TElement> group in groups)
            {
                yield return resultSelector(group.Key, group);
            }
        }

        private static IEnumerable<TResult> CreateGroupByIterator<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            IEnumerable<IGrouping<TKey, TSource>> groups = GroupBy(source, keySelector, comparer);

            foreach (IGrouping<TKey, TSource> group in groups)
            {
                yield return resultSelector(group.Key, group);
            }
        }
    }
}

#endif