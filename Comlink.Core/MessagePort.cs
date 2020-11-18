namespace Comlink.Core
{
    public interface IMessagePort : IEndpoint, ITransferable
    {

    }

    public class MessagePort : Endpoint, IMessagePort
    {
        public MessagePort(IWindow window) : base(window)
        {
        }
    }
}