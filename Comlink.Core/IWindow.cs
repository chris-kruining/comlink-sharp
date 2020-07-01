using System;

namespace Comlink
{
    public interface IWindow
    {
        public event Action<String> Message;
        public void PostMessage(String message);
    }
}