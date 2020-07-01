using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Comlink.Json;

namespace Comlink
{
    public static class Comlink
    {
        public static Remote<T> Wrap<T>(Endpoint endpoint, Object? target = null) =>
            CreateProxy<T>(endpoint, new String[0], target);

        public static Remote<T> CreateProxy<T>(Endpoint endpoint, String[] path, Object? target = null)
        {
            target ??= new Action(() => { });

            return new Remote<T>(default);
        }

        public static Endpoint WindowEndpoint(IWindow window)
        {
            return new Endpoint(window);
        }

        public static void Expose<T>(T target, Endpoint endpoint)
        {
            endpoint.Message += message =>
            {
                try
                {
                    Object?[] args = message.ArgumentList?.Select(FromWireValue).ToArray() ?? (message.Value != null ? new[]{ FromWireValue(message.Value) } : new Object?[0]);

                    // NOTE(Chris Kruining) this uses the path provided by the message and converts it into MemberInfo for easy manipulation
                    (Object? owner, Object? value, MemberInfo? member) = message.Path?.Aggregate<String, (Object? owner, Object? value, MemberInfo?)?>(
                        (null, target, null),
                        (obj, prop) =>
                        {
                            MemberInfo? member = obj?.value?.GetType().GetMember(prop, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public).FirstOrDefault();
                            Object? value = null;

                            if (member is PropertyInfo property)
                            {
                                value = property.GetValue(obj?.value);
                            } 
                            return (obj?.value, value, member);
                        }
                    ) ?? throw new Exception($"Unable to access the path '{message.Path}'");

                    Object? returnValue = message.Type switch
                    {
                        MessageType.Get => value,
                        MessageType.Set => new Func<Boolean>(() =>
                        {
                            try
                            {
                                (member as PropertyInfo)?.SetValue(owner, args[0]);
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        }).Invoke(),
                        MessageType.Apply => (member as MethodInfo)?.Invoke(owner, args),
                        MessageType.Construct => new Proxyfied(Activator.CreateInstance(typeof(T), args)),
                        MessageType.Endpoint => new Func<Object?>(() =>
                        {
                            (MessagePort port1, MessagePort port2) = new MessageChannel();

                            Expose(target, port2);

                            return Transfer(port1, port1);
                        }).Invoke(),
                        MessageType.Release => null,
                        _ => throw new Exception("Unhandled message type"),
                    };

                    (WireValue wireValue, ITransferable[] transferables) = ToWireValue(returnValue);
                    wireValue.Id = message.Id;

                    endpoint.PostMessage(wireValue, transferables);
                }
                catch (Exception exception)
                {

                }
            };
        }

        private static ConditionalWeakTable<Object, ITransferable[]> _transferCache = new ConditionalWeakTable<Object, ITransferable[]>();

        public static T Transfer<T>(T target, params ITransferable[] transferables)
        {
            _transferCache.Add(target, transferables);

            return target;
        }

        private static readonly ProxyTransferHandler _handler = new ProxyTransferHandler();
        public static (WireValue, ITransferable[]) ToWireValue<T>(T value)
        {
            if (_handler.CanHandle(value))
            {
                (MessagePort, ITransferable[] transferables) message = _handler.Serialize(value);

                return (
                    new WireValue
                    {
                        Type = WireValueType.Handler,
                        Name = "proxy",
                        Value = message,
                    },
                    message.transferables
                );
            }

            _transferCache.TryGetValue(value, out ITransferable[]? transferables);
            return (
                new WireValue
                {
                    Type = WireValueType.Raw,
                    Value = value,
                },
                transferables ?? new ITransferable[0]
            );
        }

        public static Object? FromWireValue(WireValue wireValue) => wireValue.Type switch
        {
            WireValueType.Handler => _handler.deserialize((MessagePort)wireValue.Value!),
            WireValueType.Raw => wireValue.Value,
            _ => throw new Exception("Unhandled WireValue type")
        };
    }
}