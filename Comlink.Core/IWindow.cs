using System;

namespace Comlink.Core
{
    public interface IWindow
    {
        public void Start();
        public event Action<String> Message;
        public void PostMessage(String message, params ITransferable[]? transferables);
    }
}