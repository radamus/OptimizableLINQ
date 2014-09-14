using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBQL.Environment
{
    public class Binder<T>
    {
        public String Name { get; set; }
        public T Value { get; set; }
    }
    public abstract class ENVS<BV> 
    {
        public class BindingInfo<T>
        {            
            public T Value { get; internal set; }
            public int Section { get; internal set; }
            public bool Found { get; internal set; }

            public static readonly BindingInfo<T> NOTFOUND = new BindingInfo<T>() { Found = false, Section = -1, Value = default(T) };
        }

        public class ENV
        {
            public Dictionary<String, Binder<BV>> binders = new Dictionary<String, Binder<BV>>();
        }

        private Stack<ENV> sections = new Stack<ENV>();
        public int Size { get { return sections.Count; } }
        public virtual void DestroyEnv()
        {
            if (Size <= 0)
            {
                throw new IndexOutOfRangeException("insufficient envs size: " + Size);
            }
            ENV section = sections.Pop();
            
        }

        public virtual void CreateEnv()
        {
            sections.Push(new ENV());
        }

        public virtual void PushBinder(Binder<BV> binder)
        {
            if(!sections.Peek().binders.ContainsKey(binder.Name))
                sections.Peek().binders.Add(binder.Name, binder);
        }

        public virtual void PushBinders(Binder<BV>[] binders)
        {
            foreach (Binder<BV> binder in binders)
            {
                this.PushBinder(binder);
            }
        }

        public virtual BindingInfo<BV> Bind(String name)
        {
            int index = 1;
            foreach (ENV section in sections)
            {
                if (section.binders.ContainsKey(name))
                {
                    Binder<BV> b = section.binders[name];
                    return new BindingInfo<BV>() { Value = b.Value, Section = sections.Count - index , Found = true };
                }
                index++;
            }
            
            return BindingInfo<BV>.NOTFOUND;

        }


       
    }
}
