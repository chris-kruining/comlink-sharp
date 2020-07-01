using System;
using System.Text.Json;
using Comlink.Json;

namespace Comlink
{

    public enum MessageType
    {
        Get,
        Set,
        Apply,
        Construct,
        Endpoint,
        Release,
    }
    public interface IGetMessage
    {
        static MessageType Type = MessageType.Get;

        String? Id { get; set; }
        String[] Path { get; set; }
    }
    public interface ISetMessage
    {
        static MessageType Type = MessageType.Set;

        String? Id { get; set; }
        String[] Path { get; set; }
        WireValue? Value { get; set; }
    }
    public interface IApplyMessage
    {
        static MessageType Type = MessageType.Apply;

        String? Id { get; set; }
        String[] Path { get; set; }
        WireValue[]? ArgumentList { get; set; }
    }
    public interface IConstructMessage
    {
        static MessageType Type = MessageType.Construct;

        String? Id { get; set; }
        String[] Path { get; set; }
        WireValue[]? ArgumentList { get; set; }
    }
    public interface IEndpointMessage
    {
        static MessageType Type = MessageType.Endpoint;

        String? Id { get; set; }
    }
    public interface IReleaseMessage
    {
        static MessageType Type = MessageType.Release;

        String? Id { get; set; }
        String[] Path { get; set; }
    }
    public class Message : IGetMessage, ISetMessage, IApplyMessage, IConstructMessage, IEndpointMessage, IReleaseMessage
    {
        public MessageType Type { get; set; }
        public String Id { get; set; }
        public String[]? Path { get; set; }
        public WireValue[]? ArgumentList { get; set; }
        public WireValue? Value { get; set; }

        public void Deconstruct(out String id, out MessageType type, out String[] path)
        {
            id = Id;
            type = Type;
            path = Path;
        }

        public static Message FromJson(String json) => JsonSerializer.Deserialize<Message>(json, Options.Default) ?? throw new Exception("Invalid message received, could not deserialize json to instance of 'Message'");
    }
}