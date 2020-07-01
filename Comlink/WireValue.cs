using System;

namespace Comlink
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
    public class WireValue : IRawWireValue, IHandlerWireValue
    {
        public WireValueType Type { get; set; }
        public String? Id { get; set; }
        public String? Name { get; set; }
        public Object? Value { get; set; }
    }
}