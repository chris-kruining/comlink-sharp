using System;

namespace Comlink.Core
{
    public interface ITransferHandler
    {
        String Name { get; }
        Boolean CanHandle(dynamic? value);
        Object? Deserialize(Object? value);
    }
    public interface ITransferHandler<TTarget, TSerialized> : ITransferHandler
    {
        (TSerialized, ITransferable[]) Serialize(TTarget value);
        TTarget Deserialize(TSerialized value);
    }

    public class ProxyTransferHandler : ITransferHandler<Proxy<Object?>, IMessagePort>
    {
        public String Name { get; } = "proxy";

        public Boolean CanHandle(dynamic? value) => value is Proxy<Object?>;

        public (IMessagePort, ITransferable[]) Serialize(Proxy<Object?> value)
        {
            (IMessagePort port1, IMessagePort port2) = new MessageChannel();

            Comlink.Expose(value, port1);

            return (port2, new ITransferable[] { port2 });
        }

        public Proxy<Object?> Deserialize(IMessagePort value)
        {
            value.Start();

            return Comlink.Wrap<Object?>(value);
        }
        public Object? Deserialize(Object? value) => Deserialize((IMessagePort)value!);
    }

    public class ThrowTransferHandler : ITransferHandler<Exception, Exception>
    {
        public String Name { get; } = "trow";

        public Boolean CanHandle(dynamic? value) => value is Exception;

        public (Exception, ITransferable[]) Serialize(Exception value) => (value, Array.Empty<ITransferable>());

        public Exception Deserialize(Exception value) => value;
        public Object? Deserialize(Object? value) => Deserialize((Exception)value!);
    }
}