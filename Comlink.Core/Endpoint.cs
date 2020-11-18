using System;
using System.Text.Json;
using Comlink.Core.Json;

namespace Comlink.Core
{
    public class Endpoint
    {
        private readonly IWindow _window;

        public Endpoint(IWindow window)
        {
            _window = window;
            _window.Message += json => Message?.Invoke(global::Comlink.Message.FromJson(json));
        }

        public void PostMessage(dynamic message, params ITransferable[] transferables)
        {
            _window.PostMessage(JsonSerializer.Serialize(message, Options.Default));
        }

        public event Action<Message> Message;
    }
}