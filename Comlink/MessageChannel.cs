namespace Comlink
{
    public class MessageChannel
    {
        private readonly MessagePort _port1;
        private readonly MessagePort _port2;

        public void Deconstruct(out MessagePort port1, out MessagePort port2)
        {
            port1 = _port1;
            port2 = _port2;
        }
    }
}