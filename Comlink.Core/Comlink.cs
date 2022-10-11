using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Comlink.Core
{
    public static class Comlink
    {
        public static Symbol CreateEndpoint { get; } = new("Comlink.endpoint");
        public static Symbol ReleaseProxy { get; } = new("Comlink.releaseProxy");

        public static dynamic Wrap<T>(IEndpoint endpoint, T target = default) => CreateProxy(endpoint, Array.Empty<PropertyAccessor>(), target);

        public static Proxy<T> CreateProxy<T>(IEndpoint endpoint, PropertyAccessor[] path, T target = default)
        {
            Boolean isProxyReleased = false;

            Proxy<T>? proxy = null;
            proxy = new Proxy<T>(target, new Proxy<T>.Arguments
            {
                Get = async (_target, property) =>
                {
                    ThrowIfProxyReleased(isProxyReleased);

                    if (property == ReleaseProxy)
                    {
                        return (Action)(async () =>
                        {
                            await RequestResponseMessage(endpoint, new Message
                            {
                                Type = MessageType.Release,
                                Path = path.Select(p => p.ToString()),
                            });

                            endpoint.Close();
                            isProxyReleased = true;
                        });
                    }

                    if (property == "then")
                    {
                        if (!path.Any())
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            return ValueTask.FromResult(proxy);
                        }

                        IMessage message = new Message
                        {
                            Type = MessageType.Get,
                            Path = path.Select(p => p.ToString()),
                        };
                        return await RequestResponseMessage(endpoint, message).FromWireValue<ValueTask?>();
                    }

                    return CreateProxy<T>(endpoint, path.Append(property));
                },
                Set = async (_target, property, value) =>
                {
                    ThrowIfProxyReleased(isProxyReleased);

                    (IWireValue wireValue, ITransferable[] transferables) = ToWireValue(value);

                    IMessage message = new Message
                    {
                        Type = MessageType.Set,
                        Path = path.Append(property).Select(p => p.ToString()),
                        Value = wireValue,
                    };
                    return await RequestResponseMessage(endpoint, message, transferables).FromWireValue<Boolean>();
                },
                Apply = async (_target, property, args) =>
                {
                    ThrowIfProxyReleased(isProxyReleased);

                    PropertyAccessor last = path.Length > 0 ? path[^1] : property;
                    IMessage message;

                    if (last == CreateEndpoint)
                    {
                        message = new Message
                        {
                            Type = MessageType.Endpoint,
                        };
                        return await RequestResponseMessage(endpoint, message).FromWireValue<ValueTask?>();
                    }

                    if (last == "bind")
                    {
                        return CreateProxy<T>(endpoint, path[..^2]);
                    }

                    (IEnumerable<WireValue> arguments, IEnumerable<ITransferable> transferables) = ProcessArguments(args);

                    message = new Message
                    {
                        Type = MessageType.Apply,
                        Path = path.Append(property).Select(p => p.ToString()),
                        ArgumentList = arguments,
                    };
                    return await RequestResponseMessage(endpoint, message, transferables.ToArray()).FromWireValue<Object?>();
                },
                Construct = async (_target, args) =>
                {
                    ThrowIfProxyReleased(isProxyReleased);

                    (IEnumerable<WireValue> arguments, IEnumerable<ITransferable> transferables) = ProcessArguments(args);
                    IMessage message = new Message
                    {
                        Type = MessageType.Construct,
                        Path = path.Select(p => p.ToString()),
                        ArgumentList = arguments,
                    };

                    return await RequestResponseMessage(endpoint, message, transferables.ToArray()).FromWireValue<T>();
                },
            });

            return proxy;
        }

        public static IEndpoint WindowEndpoint(IWindow window)
        {
            return new Endpoint(window);
        }

        public static void Expose<T>(T target, IEndpoint endpoint)
        {
            endpoint.Message += message =>
            {
                if (message.Type == MessageType.Raw)
                {
                    return;
                }
                
                Object?[] args = message.ArgumentList?.Select(FromWireValue).ToArray() 
                    ?? (message.Value != null ? new[]{ FromWireValue(message.Value) } : Array.Empty<Object?>());

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
                ) ?? (null, null, null);
                // ) ?? throw new Exception($"Unable to access the path '{message.Path}'");

                Object? returnValue;

                try
                {
                    returnValue = message.Type switch
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
                        })(),
                        MessageType.Apply => (member as MethodInfo)?.Invoke(owner, args),
                        MessageType.Construct => Proxy(Activator.CreateInstance(typeof(T), args)),
                        MessageType.Endpoint => new Func<Object?>(() =>
                        {
                            (IMessagePort port1, IMessagePort port2) = new MessageChannel();

                            Expose(target, port2);

                            return Transfer(port1, port1);
                        })(),
                        MessageType.Release => null,
                        _ => throw new Exception("Unhandled message type"),
                    };
                }
                catch(Exception e)
                {
                    returnValue = e;
                }

                (IWireValue wireValue, ITransferable[] transferables) = ToWireValue(returnValue);
                IMessage response = new Message
                {
                    Id = message.Id,
                    Type = message.Type,
                    Value = wireValue.Value,
                };

                endpoint.PostMessage(response, transferables);
            };
        }

        private static ConditionalWeakTable<Object?, ITransferable[]> _transferCache = new();
        public static T Transfer<T>(T target, params ITransferable[] transferables)
        {
            _transferCache.Add(target!, transferables);

            return target;
        }

        public static Proxy<TValue> Proxy<TValue>(TValue target) => new(target, new Proxy<TValue>.Arguments());

        public static readonly IDictionary<String, ITransferHandler> _handlers = new Dictionary<String, ITransferHandler>
        {
            { "proxy", new ProxyTransferHandler() },
            { "throw", new ThrowTransferHandler() }, 
        };
        public static (WireValue Value, ITransferable[] Transferables) ToWireValue<T>(T value)
        {
            if (_handlers.FirstOrDefault(h => h.Value.CanHandle(value)) is ITransferHandler<T, Object?> handler)
            {
                (Object? v, ITransferable[] transfers) = handler.Serialize(value);

                return (
                    new WireValue
                    {
                        Type = IHandlerWireValue.Type,
                        Name = handler.Name,
                        Value = v,
                    },
                    transfers
                );
            }

            ITransferable[]? transferables = null;
            
            if (value != null)
            {
                _transferCache.TryGetValue(value, out transferables);
            }
            
            return (
                new WireValue
                {
                    Type = IRawWireValue.Type,
                    Value = value,
                },
                transferables ?? Array.Empty<ITransferable>()
            );
        }

        public static Object? FromWireValue(Object? value) => value switch
        {
            IWireValue wireValue => wireValue?.Type switch
            {
                WireValueType.Handler => _handlers[wireValue.Name ?? ""].Deserialize(wireValue.Value),
                WireValueType.Raw when wireValue.Value is JsonElement jsonElement => HandleJsonValues(jsonElement),
                _ => throw new Exception("Unhandled WireValue type"),
            },
            JsonElement jsonElement => HandleJsonValues(jsonElement),
            _ => value,
        };
        private static Object? HandleJsonValues(JsonElement jsonElement) => jsonElement.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => jsonElement.TryGetInt64(out long l) ? l : jsonElement.GetDouble(),
            JsonValueKind.String => jsonElement.TryGetDateTime(out DateTime datetime) ? (Object?)datetime : jsonElement.GetString(),
            _ => jsonElement,
        };

        private static (IEnumerable<WireValue>, IEnumerable<ITransferable>) ProcessArguments(IEnumerable<Object?> arguments)
        {
            (IEnumerable<WireValue> Value, IEnumerable<ITransferable> Transferables) seed = (Array.Empty<WireValue>(), Array.Empty<ITransferable>());
            return arguments.Select(ToWireValue).Aggregate(seed, (result, argument) => (result.Value.Append(argument.Value), result.Transferables.Concat(argument.Transferables)));
        }

        private static void ThrowIfProxyReleased(Boolean released)
        {
            if (released)
            {
                throw new Exception("Proxy has been released and is not useable");
            }
        }
        
        private static ValueTask<Object?> RequestResponseMessage(IEndpoint endpoint, IMessage message, ITransferable[]? transferables = null)
        {
            // Set up async handling
            TaskCompletionSource<Object?> tcs = new();

            // Set up the message id
            String id = Guid.NewGuid().ToString();
            message.Id = id;

            // Kick off actual message logic
            endpoint.Message += Handler;
            endpoint.Start();
            endpoint.PostMessage(message, transferables);

            return new ValueTask<Object?>(tcs.Task);

            void Handler(IMessage response)
            {
                if (response.Id != id)
                {
                    return;
                }

                endpoint.Message -= Handler;
                tcs.SetResult(response.Value);
            }
        }
    }

    public class Symbol : IEquatable<Symbol>
    {
        private readonly Object _object = new Object();
        public String? Description { get; }

        public Symbol(String? description = null)
        {
            Description = description;
        }

        public Boolean Equals(Symbol? other) => other?._object == _object;
        public static Boolean operator ==(Symbol? o1, Symbol? o2) => EqualityComparer<Symbol>.Default.Equals(o1, o2);
        public static Boolean operator !=(Symbol? o1, Symbol? o2) => !(o1 == o2);

        public override Int32 GetHashCode() => _object.GetHashCode();

        public override Boolean Equals(Object? obj) => Equals(obj as Symbol);
        public override String ToString() => $"symbol({Description})";

        // static methods to support symbol registry
        private static readonly Dictionary<String, Symbol> GlobalSymbols = new Dictionary<String, Symbol>(StringComparer.Ordinal);

        public static Symbol For(String key) => GlobalSymbols[key] ??= new Symbol(key);

        public static String? KeyFor(Symbol s) => GlobalSymbols.FirstOrDefault(a => a.Value == s).Key;

        // Well-known ECMAScript symbols
        private const String ns = "Symbol.";
        public static Symbol HasInstance => For(ns + "hasInstance");
        public static Symbol IsConcatSpreadable => For(ns + "isConcatSpreadable");
        public static Symbol Iterator => For(ns + "iterator");
        public static Symbol Match => For(ns + "match");
        public static Symbol Replace => For(ns + "replace");
        public static Symbol Search => For(ns + "search");
        public static Symbol Species => For(ns + "species");
        public static Symbol Split => For(ns + "split");
        public static Symbol ToPrimitive => For(ns + "toPrimitive");
        public static Symbol ToStringTag => For(ns + "toStringTag");
        public static Symbol Unscopables => For(ns + "unscopables");
    }

    public static class Extensions
    {
        public static T[] Append<T>(this T[] subject, T item) => subject.ToList().Append(item).ToArray();

        public static async ValueTask<T> FromWireValue<T>(this ValueTask<Object?> valueTask) => (T)Comlink.FromWireValue(await valueTask)!;
    }
}