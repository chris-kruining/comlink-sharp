using System;
using System.Collections.Generic;
using System.Text.Json;
using Comlink.Core.Json;

namespace Comlink.Core
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
        IEnumerable<String>? Path { get; set; }
    }
    public interface ISetMessage
    {
        static MessageType Type = MessageType.Set;

        String? Id { get; set; }
        IEnumerable<String>? Path { get; set; }
        IWireValue? Value { get; set; }
    }
    public interface IApplyMessage
    {
        static MessageType Type = MessageType.Apply;

        String? Id { get; set; }
        IEnumerable<String>? Path { get; set; }
        IEnumerable<IWireValue>? ArgumentList { get; set; }
    }
    public interface IConstructMessage
    {
        static MessageType Type = MessageType.Construct;

        String? Id { get; set; }
        IEnumerable<String>? Path { get; set; }
        IEnumerable<IWireValue>? ArgumentList { get; set; }
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
        IEnumerable<String>? Path { get; set; }
    }
    public interface IMessage : IGetMessage, ISetMessage, IApplyMessage, IConstructMessage, IEndpointMessage, IReleaseMessage
    {
        MessageType Type { get; set; }
        new String? Id { get; set; }
        new IWireValue? Value { get; set; }
        new IEnumerable<String>? Path { get; set; }
        new IEnumerable<IWireValue>? ArgumentList { get; set; }
    }
    public class Message : IMessage
    {
        public MessageType Type { get; set; }
        public String? Id { get; set; }
        public IEnumerable<String>? Path { get; set; }
        public IEnumerable<IWireValue>? ArgumentList { get; set; }
        public IWireValue? Value { get; set; }

        public void Deconstruct(out String? id, out MessageType type, out IEnumerable<String>? path)
        {
            id = Id;
            type = Type;
            path = Path;
        }

        public static IMessage FromJson(String json) => JsonSerializer.Deserialize<Message>(json, Options.Default) ?? throw new Exception("Invalid message received, could not deserialize json to instance of 'Message'");
    }
}