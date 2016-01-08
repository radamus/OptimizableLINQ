using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Runtime;

namespace OptimizableLINQ
{

    internal class EmptyEnumerable<TElement>
    {
        private static volatile TElement[] instance;

        public static IEnumerable<TElement> Instance
        {
          get
          {
            if (EmptyEnumerable<TElement>.instance == null)
              EmptyEnumerable<TElement>.instance = new TElement[0];
            return (IEnumerable<TElement>) EmptyEnumerable<TElement>.instance;
          }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EmptyEnumerable()
        {
        }
    }

    public class VolatileIndex<TKey, TElement>
    {
        private IEqualityComparer<TKey> comparer;
        private VolatileIndex<TKey, TElement>.Grouping[] groupings;
        private VolatileIndex<TKey, TElement>.Grouping lastGrouping;
        private VolatileIndex<TKey, TElement>.Grouping exceptionGrouping;
        private KeyExceptionEnumerable keyExceptionGrouping;
        private int count;

        public int Count
        {
          [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] get
          {
            return this.count;
          }
        }

        public IEnumerable<VolatileIndexElement> this[Func<TKey> key]
        {
          get
          {
              try
              {
                  return (IEnumerable<VolatileIndexElement>)this.GetGrouping(key(), false) ?? EmptyEnumerable<VolatileIndexElement>.Instance;
              }
              catch (Exception e)
              {
                  keyExceptionGrouping.setKeyException(e);
                  return keyExceptionGrouping;
              }
          }
        }

        private VolatileIndex(IEqualityComparer<TKey> comparer)
        {
          if (comparer == null)
              comparer = (IEqualityComparer<TKey>) EqualityComparer<TKey>.Default;
          this.comparer = comparer;
          this.groupings = new VolatileIndex<TKey, TElement>.Grouping[7];
          this.exceptionGrouping = new VolatileIndex<TKey, TElement>.Grouping();
          this.exceptionGrouping.elements = new VolatileIndexElement[1];
        }

        internal static VolatileIndex<TKey, TElement> Create<TSource>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (elementSelector == null)
            {
                throw new ArgumentNullException("elementSelector");
            }
            VolatileIndex<TKey, TElement> lookup = new VolatileIndex<TKey, TElement>(comparer);
            lookup.keyExceptionGrouping = new KeyExceptionEnumerable(source.Select(s => elementSelector(s)));
            foreach (TSource source1 in source)
            {
                TElement element = elementSelector(source1);
                try
                {
                    lookup.GetGrouping(keySelector(source1), true).Add(new VolatileIndexElement(element));
                }
                catch (Exception e)
                {
                    lookup.exceptionGrouping.Add(new VolatileIndexElement(element, e));
                }
            }
            return lookup;
        }

        internal int InternalGetHashCode(TKey key)
        {
          if ((object) key != null)
            return this.comparer.GetHashCode(key) & int.MaxValue;
          return 0;
        }

        internal VolatileIndex<TKey, TElement>.Grouping GetGrouping(TKey key, bool create)
        {
          int hashCode = this.InternalGetHashCode(key);
          for (VolatileIndex<TKey, TElement>.Grouping grouping = this.groupings[hashCode % this.groupings.Length]; grouping != null; grouping = grouping.hashNext)
          {
            if (grouping.hashCode == hashCode && this.comparer.Equals(grouping.key, key))
              return grouping;
          }
          if (!create)
            return (VolatileIndex<TKey, TElement>.Grouping) null;
          if (this.count == this.groupings.Length)
            this.Resize();
          int index = hashCode % this.groupings.Length;
          VolatileIndex<TKey, TElement>.Grouping grouping1 = new VolatileIndex<TKey, TElement>.Grouping();
          grouping1.key = key;
          grouping1.hashCode = hashCode;
          grouping1.elements = new VolatileIndexElement[1];
          grouping1.hashNext = this.groupings[index];
          this.groupings[index] = grouping1;
          if (this.lastGrouping == null)
          {
            grouping1.next = grouping1;
          }
          else
          {
            grouping1.next = this.lastGrouping.next;
            this.lastGrouping.next = grouping1;
          }
          this.lastGrouping = grouping1;
          ++this.count;
          return grouping1;
        }

        private void Resize()
        {
          int length = checked (this.count * 2 + 1);
          VolatileIndex<TKey, TElement>.Grouping[] groupingArray = new VolatileIndex<TKey, TElement>.Grouping[length];
          VolatileIndex<TKey, TElement>.Grouping grouping = this.lastGrouping;
          do
          {
            grouping = grouping.next;
            int index = grouping.hashCode % length;
            grouping.hashNext = groupingArray[index];
            groupingArray[index] = grouping;
          }
          while (grouping != this.lastGrouping);
          this.groupings = groupingArray;
        }

        public class VolatileIndexElement
        {
            public TElement Value;
            internal Exception e = null;
            public bool IsValid
            {
                get
                {
                    if (e == null) return true;
                    else throw e;
                }
            }

            internal VolatileIndexElement(TElement Value, Exception e = null) {
                this.Value = Value;
                this.e = e;
            }
        }

        internal class KeyExceptionEnumerable : IEnumerable<VolatileIndexElement>
        {

            private IEnumerable<TElement> source;
            private VolatileIndexElement vie;
            
            public KeyExceptionEnumerable(IEnumerable<TElement> source)
            {
                this.source = source;
                vie = new VolatileIndexElement(source.GetEnumerator().Current);
            }

            internal void setKeyException(Exception e) { vie.e = e; }


            IEnumerator<VolatileIndexElement> IEnumerable<VolatileIndexElement>.GetEnumerator()
            {
                foreach(TElement element in source)
                {
                    vie.Value = element;
                    yield return vie;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (TElement element in source)
                {
                    vie.Value = element;
                    yield return vie;
                }
            }
        }

        internal class Grouping : IGrouping<TKey, VolatileIndexElement>, IList<VolatileIndexElement>, ICollection<VolatileIndexElement>, IEnumerable<VolatileIndexElement>, IEnumerable
        {
            internal TKey key;
            internal int hashCode;
            internal VolatileIndexElement[] elements;
            internal int count;
            internal VolatileIndex<TKey, TElement>.Grouping hashNext;
            internal VolatileIndex<TKey, TElement>.Grouping next;

            public TKey Key
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")] get
                {
                    return this.key;
                }
            }

            int ICollection<VolatileIndexElement>.Count
            {
                get
                {
                    return this.count;
                }
            }

            bool ICollection<VolatileIndexElement>.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            VolatileIndexElement IList<VolatileIndexElement>.this[int index]
            {
                get
                {
                  if (index < 0 || index >= this.count)
                    throw new ArgumentOutOfRangeException("index");
                  return this.elements[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            public Grouping()
            {
            }

            internal void Add(VolatileIndexElement element)
            {
                if (this.elements.Length == this.count)
                    Array.Resize<VolatileIndexElement>(ref this.elements, checked(this.count * 2));
                this.elements[this.count] = element;
                ++this.count;
            }

            public IEnumerator<VolatileIndexElement> GetEnumerator()
            {
                for (int i = 0; i < this.count; ++i)
                    yield return this.elements[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator) this.GetEnumerator();
            }

            void ICollection<VolatileIndexElement>.Add(VolatileIndexElement item)
            {
                throw new NotSupportedException();
            }

            void ICollection<VolatileIndexElement>.Clear()
            {
                throw new NotSupportedException();
            }

            bool ICollection<VolatileIndexElement>.Contains(VolatileIndexElement item)
            {
                return Array.IndexOf<VolatileIndexElement>(this.elements, item, 0, this.count) >= 0;
            }

            void ICollection<VolatileIndexElement>.CopyTo(VolatileIndexElement[] array, int arrayIndex)
            {
                Array.Copy((Array) this.elements, 0, (Array) array, arrayIndex, this.count);
            }

            bool ICollection<VolatileIndexElement>.Remove(VolatileIndexElement item)
            {
                throw new NotSupportedException();
            }

            int IList<VolatileIndexElement>.IndexOf(VolatileIndexElement item)
            {
                return Array.IndexOf<VolatileIndexElement>(this.elements, item, 0, this.count);
            }

            void IList<VolatileIndexElement>.Insert(int index, VolatileIndexElement item)
            {
                throw new NotSupportedException();
            }

            void IList<VolatileIndexElement>.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
        }
    }
}

