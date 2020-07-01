using System;
using System.Dynamic;

namespace Comlink
{
    public class Proxyfied : Proxy<Object?>
    {
        public Proxyfied(Object? target) : base(target)
        {
        }

        public override Boolean Apply(Object target, String property, Object?[] args)
        {
            throw new NotImplementedException();
        }

        public override Object? Construct(Object target, Object?[] args)
        {
            throw new NotImplementedException();
        }

        public override Object? Get(Object target, String property)
        {
            throw new NotImplementedException();
        }

        public override Boolean Set(Object target, String property, Object? value)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Proxy<T> : DynamicObject
    {
        private readonly T _target;

        protected Proxy(T target)
        {
            _target = target;
        }

        public abstract Object? Get(T target, String property);
        public abstract Boolean Set(T target, String property, Object? value);
        public abstract Boolean Apply(T target, String property, Object?[] args);
        public abstract T Construct(T target, Object?[] args);

        public override Boolean TryGetMember(GetMemberBinder binder, out Object? result)
        {
            result = Get(_target, binder.Name);

            return true;
        }
        public override Boolean TrySetMember(SetMemberBinder binder, Object? value)
        {
            return Set(_target, binder.Name, value);
        }
        public override Boolean TryInvokeMember(InvokeMemberBinder binder, Object?[]? args, out Object? result)
        {
            try
            {
                result = Apply(_target, binder.Name, args ?? new Object?[0]);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public override Boolean TryCreateInstance(CreateInstanceBinder binder, Object?[]? args, out Object? result)
        {
            try
            {
                result = Construct(_target, args ?? new Object?[0]);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}