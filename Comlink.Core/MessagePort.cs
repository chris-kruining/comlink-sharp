namespace Comlink.Core
{
    public class MessagePort : Endpoint, ITransferable
    {
        public MessagePort(IWindow window) : base(window)
        {
        }
    }
}