namespace Comlink.Core
{
    public class MessageChannel
    {
        private readonly IMessagePort _port1;
        private readonly IMessagePort _port2;

        public void Deconstruct(out IMessagePort port1, out IMessagePort port2)
        {
            port1 = _port1;
            port2 = _port2;
        }
    }
}