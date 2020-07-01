using System;

namespace Comlink
{
    public interface ITransferHandler<TTarget, TSerialized>
    {
        Boolean CanHandle(dynamic value);
        (TSerialized, ITransferable[]) Serialize(TTarget value);
        TTarget deserialize(TSerialized value);
    }

    public class ProxyTransferHandler : ITransferHandler<Object, MessagePort>
    {
        public Boolean CanHandle(dynamic? value) => value is Proxyfied;

        public (MessagePort, ITransferable[]) Serialize(Object value)
        {
            (MessagePort port1, MessagePort port2) = new MessageChannel();

            return (port2, new ITransferable[] { port2 });
        }

        public Object deserialize(MessagePort value)
        {
            throw new NotImplementedException();
        }
    }
}