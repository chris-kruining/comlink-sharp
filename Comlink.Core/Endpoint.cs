using System;
using System.Text.Json;
using Comlink.Core.Json;

namespace Comlink.Core
{
    public interface IEndpoint
    {
        public event Action<IMessage>? Message;
        public void PostMessage(dynamic message, params ITransferable[]? transferables);
        public void Start();
        public void Close();
    }

    public class Endpoint: IEndpoint
    {
        private readonly IWindow _window;

        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = Options.Default;

        public Endpoint(IWindow window)
        {
            _window = window;
            _window.Message += json => Message?.Invoke(Core.Message.FromJson(json));
        }

        public void PostMessage(dynamic message, params ITransferable[]? transferables)
        {
            String serializedMessage = JsonSerializer.Serialize(message, JsonSerializerOptions);

            _window.PostMessage(serializedMessage, transferables);
        }

        public void Start()
        {
            _window.Start();
        }

        public void Close()
        {

        }

        public event Action<IMessage>? Message;
    }
}