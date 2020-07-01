using System;

namespace Comlink
{
    public class Remote<T> : Proxy<T>
    {
        private Boolean _isProxyReleased = false;

        public Remote(T target) : base(target)
        {
        }

        public override Object? Get(T target, String property)
        {
            ThrowIfProxyReleased();



            throw new NotImplementedException();
        }

        public override Boolean Set(T target, String property, Object? value)
        {
            throw new NotImplementedException();
        }

        public override Boolean Apply(T target, String property, Object?[] args)
        {
            throw new NotImplementedException();
        }

        public override T Construct(T target, Object?[] args)
        {
            throw new NotImplementedException();
        }

        private void ThrowIfProxyReleased()
        {
            if (_isProxyReleased)
            {
                throw new Exception("Proxy has been released and is not useable");
            }
        }
    }
}