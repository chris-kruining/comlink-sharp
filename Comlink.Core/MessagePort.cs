namespace Comlink
{
    public class MessagePort : Endpoint, ITransferable
    {
        public MessagePort(IWindow window) : base(window)
        {
        }
    }
}