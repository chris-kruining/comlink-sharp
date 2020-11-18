using System;

namespace Comlink.Core
{
    public enum WireValueType
    {
        Raw,
        Proxy,
        Throw,
        Handler,
    }
    public interface IRawWireValue
    {
        public static WireValueType Type = WireValueType.Raw;

        public String? Id { get; set; }
        public Object? Value { get; set; }
    }
    public interface IHandlerWireValue
    {
        public static WireValueType Type = WireValueType.Handler;

        public String? Id { get; set; }
        public String? Name { get; set; }
        public Object? Value { get; set; }
    }

    public interface IWireValue : IRawWireValue, IHandlerWireValue
    {
        WireValueType Type { get; set; }
        new String? Id { get; set; }
        new String? Name { get; set; }
        new Object? Value { get; set; }
    }

    public class WireValue : IWireValue
    {
        public WireValueType Type { get; set; }
        public String? Id { get; set; }
        public String? Name { get; set; }
        public Object? Value { get; set; }
    }
}