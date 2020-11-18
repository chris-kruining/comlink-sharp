using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace Comlink.Core
{
    public class Proxy<T> : ProxyInternal<T>
    {
        public struct Arguments
        {
            public Func<T, PropertyAccessor, ValueTask<Object?>>? Get;
            public Func<T, PropertyAccessor, Object?, ValueTask<Boolean>>? Set;
            public Func<T, PropertyAccessor, Object?[], ValueTask<Object?>>? Apply;
            public Func<T, Object?[], ValueTask<T>>? Construct;
        }

        private readonly Arguments _arguments;

        public Proxy(T target, Arguments arguments) : base(target)
        {
            _arguments = arguments;
        }

        public override ValueTask<Object?> Get(T target, PropertyAccessor property)
        {
            // TODO(Chris Kruining) Add default implementation instead of throwing an exception
            Func<T, PropertyAccessor, ValueTask<Object?>> get = _arguments.Get ?? throw new NotImplementedException();

            return get.Invoke(target, property);
        }

        public override ValueTask<Boolean> Set(T target, PropertyAccessor property, Object? value)
        {
            // TODO(Chris Kruining) Add default implementation instead of throwing an exception
            Func<T, PropertyAccessor, Object?, ValueTask<Boolean>> set = _arguments.Set ?? throw new NotImplementedException();

            return set.Invoke(target, property, value);
        }

        public override ValueTask<Object?> Apply(T target, PropertyAccessor property, Object?[] args)
        {
            // TODO(Chris Kruining) Add default implementation instead of throwing an exception
            Func<T, PropertyAccessor, Object?[], ValueTask<Object?>> apply = _arguments.Apply ?? throw new NotImplementedException();

            return apply.Invoke(target, property, args);
        }

        public override ValueTask<T> Construct(T target, Object?[] args)
        {
            // TODO(Chris Kruining) Add default implementation instead of throwing an exception
            Func<T, Object?[], ValueTask<T>> construct = _arguments.Construct ?? throw new NotImplementedException();

            return construct.Invoke(target, args);
        }
    }

    public abstract class ProxyInternal<T> : DynamicObject
    {
        private readonly T _target;

        protected ProxyInternal(T target)
        {
            _target = target;
        }

        public abstract ValueTask<Object?> Get(T target, PropertyAccessor property);
        public abstract ValueTask<Boolean> Set(T target, PropertyAccessor property, Object? value);
        public abstract ValueTask<Object?> Apply(T target, PropertyAccessor property, Object?[] args);
        public abstract ValueTask<T> Construct(T target, Object?[] args);

        public override Boolean TryGetMember(GetMemberBinder binder, out Object? result)
        {
            result = Get(_target, binder.Name).GetAwaiter().GetResult();

            return true;
        }
        public override Boolean TrySetMember(SetMemberBinder binder, Object? value)
        {
            return Set(_target, binder.Name, value).GetAwaiter().GetResult();
        }
        public override Boolean TryInvokeMember(InvokeMemberBinder binder, Object?[]? args, out Object? result)
        {
            try
            {
                result = Apply(_target, binder.Name, args ?? new Object?[0]).GetAwaiter().GetResult();
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
                result = Construct(_target, args ?? Array.Empty<Object?>()).GetAwaiter().GetResult();
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }

    public class PropertyAccessor
    {
        private readonly String? _prop;
        private readonly Symbol? _symbol;

        public PropertyAccessor(String? prop = null, Symbol? symbol = null)
        {
            _prop = prop;
            _symbol = symbol;
        }

        public override String ToString() => _prop ?? _symbol?.ToString() ?? throw new Exception("Invalid state, both 'prop' and 'symbol' are null");

        public static implicit operator String?(PropertyAccessor property) => property._prop;
        public static implicit operator Symbol?(PropertyAccessor property) => property._symbol;

        public static implicit operator PropertyAccessor(String property) => new PropertyAccessor(property);
        public static implicit operator PropertyAccessor(Symbol property) => new PropertyAccessor(null, property);
    }
}