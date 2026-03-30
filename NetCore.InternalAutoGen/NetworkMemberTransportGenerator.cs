using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;

namespace NetCore.InternalAutoGen
{
    /// <summary>
    /// Generator covering INetworkSendMethods interface.
    /// </summary>
    [Generator]
    public class NetworkMemberTransportsGenerator : IIncrementalGenerator
    {
        const string TargetMember = "NetworkMember";
        const string TargetNamespace = NetworkMethodHelpers.Namespace;
        const string GenericArgumentStyle = "TTransport";
        public void Initialize(IncrementalGeneratorInitializationContext context) => context.RegisterSourceOutput(context.CompilationProvider, static (s, _) => Execute(s));
        static void Execute(SourceProductionContext source)
        {
            using var storage = new StringWriter();
            using var w = new IndentedTextWriter(storage);

            w.WriteLine($"using NetCore;");
            w.WriteLine($"using NetCore.Transports;");
            w.WriteLine($"using System.Diagnostics.CodeAnalysis;");
            w.WriteLine();
            w.WriteLine($"namespace {TargetNamespace}");
            w.WriteLine("{");
            w.Indent++;

            w.WriteLine($"/// <inheritdoc cref=\"{TargetNamespace}.{TargetMember}\"/>");
            w.WriteLine($"public abstract partial class {TargetMember}");
            w.WriteLine("{");
            w.Indent++;

            Execute(w, method: "General", type: "ITransport", genericArgs: "IUnreliableTransport, IReliableTransport, ISequentialTransport, IResilientTransport", "Transports");
            InsertRegionSpacing(w);
            Execute(w, method: "Unreliable", type: "IUnreliableTransport", genericArgs: "IUnreliableTransport", "UnreliableTransports");
            InsertRegionSpacing(w);
            Execute(w, method: "Reliable", type: "IReliableTransport", genericArgs: "IReliableTransport", "ReliableTransports");
            InsertRegionSpacing(w);
            Execute(w, method: "Sequential", type: "ISequentialTransport", genericArgs: "ISequentialTransport", "SequentialTransports");
            InsertRegionSpacing(w);
            Execute(w, method: "Resilient", type: "IResilientTransport", genericArgs: "IResilientTransport", "ResilientTransports");

            w.Indent--;
            w.WriteLine("}");
            w.Indent--;
            w.WriteLine("}");

            source.AddSource($"{TargetMember}.Transports.g.cs", storage.ToString());
            static void InsertRegionSpacing(IndentedTextWriter w)
            {
                for (int i = 0; i < 4; i++) w.WriteLine();
            }
        }

        static void Execute(IndentedTextWriter w, string method, string type, string genericArgs, string storage)
        {
            string lower = method.ToLower();
            w.SplitAndWriteLine($$"""
            /// <summary>
            /// Registers {{lower}} transport.
            /// </summary>
            public void Register{{method}}Transport<{{GenericArgumentStyle}}>({{GenericArgumentStyle}} transport) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                lock (_lock)
                {
                    if ({{storage}}.Remove(out {{GenericArgumentStyle}}? removed))
                        removed.InvokeDetach(); // TODO: Only terminate transport if it was removed from ALL other storages.

                    {{storage}}.Add(transport);
                    transport.InvokeAttach(this);
                }
            }

            /// <summary>
            /// Tries to remove <see cref="{{type}}"/> from the map of active transports.
            /// </summary>
            /// <typeparam name="{{GenericArgumentStyle}}">Type of transport to remove.</typeparam>
            /// <param name="transport">Transport which was removed just a moment ago.</param>
            /// <returns>
            /// <c>true</c> if transport was present, was removed and the instance is provided as <paramref name="transport"/>.
            /// <c>false</c> if transport was not present and thus - was not removed.
            /// </returns>
            public bool Remove{{method}}Transport<{{GenericArgumentStyle}}>([NotNullWhen(true)] out {{GenericArgumentStyle}}? transport) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                lock (_lock)
                {
                    if ({{storage}}.Remove(out transport))
                    {
                        transport.InvokeDetach(); // TODO: Only terminate transport if it was removed from ALL other storages.
                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// Tries to remove specific <paramref name="transport"/> from the map of active transports.
            /// </summary>
            /// <typeparam name="{{GenericArgumentStyle}}">Type of transport to remove.</typeparam>
            /// <param name="transport">Transport to remove.</param>
            /// <returns>
            /// <c>true</c> if transport was present and it was removed.
            /// <c>false</c> if transport was not present and thus - was not removed.
            /// </returns>
            public bool Remove{{method}}Transport<{{GenericArgumentStyle}}>({{GenericArgumentStyle}} transport) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                lock (_lock)
                {
                    if ({{storage}}.Remove(transport))
                        return false;

                    transport.InvokeDetach(); // TODO: Only terminate transport if it was removed from ALL other storages.
                    return true;
                }
            }

            /// <summary>
            /// Checks whether this <see cref="NetworkMember"/> manages any {{lower}} transports.
            /// </summary>
            public bool HasAny{{method}}Transport()
            {
                lock (_lock) return {{storage}}.Count == 0;
            }

            /// <summary>
            /// Checks whether this <see cref="NetworkMember"/> manages a specific {{lower}} transport.
            /// </summary>
            public bool Has{{method}}Transport<{{GenericArgumentStyle}}>() where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                lock (_lock) return {{storage}}.Has<{{GenericArgumentStyle}}>();
            }

            /// <summary>
            /// Tries to retrieve <see cref="{{type}}"/> under a given <typeparamref name="{{GenericArgumentStyle}}"/> type.
            /// </summary>
            /// <typeparam name="{{GenericArgumentStyle}}">Type of transport to look for.</typeparam>
            /// <param name="transport">Transport instance or <c>null</c> when not found.</param>
            /// <returns>
            /// <c>true</c> if found and <paramref name="transport"/> was provided.
            /// <c>false</c> if not found and <paramref name="transport"/> is null.
            /// </returns>
            public bool TryGet{{method}}Transport<{{GenericArgumentStyle}}>([NotNullWhen(true)] out {{GenericArgumentStyle}}? transport) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                lock (_lock) return {{storage}}.TryGet(out transport);
            }

            /// <summary>
            /// Retrieves <see cref="{{type}}"/> under a given <typeparamref name="{{GenericArgumentStyle}}"/> type.
            /// </summary>
            /// <typeparam name="{{GenericArgumentStyle}}">Type of transport to look for.</typeparam>
            /// <returns>Transport instance or <c>null</c> when not found.</returns>
            public {{GenericArgumentStyle}} Get{{method}}Transport<{{GenericArgumentStyle}}>() where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                lock (_lock) return {{storage}}.Get<{{GenericArgumentStyle}}>();
            }

            /// <summary>
            /// Iterates over all <see cref="{{type}}"/>s using a given <paramref name="action"/>.
            /// </summary>
            /// <param name="action">Action to use on all registered <see cref="{{type}}"/>s.</param>
            public void ForEach{{method}}Transport(TransportConsumer<{{type}}> action)
            {
                lock (_lock)
                {
                    foreach (var transport in {{storage}})
                    {
                        action(transport);
                    }
                }
            }

            /// <summary>
            /// Removes all <see cref="{{type}}"/>s
            /// and calls <see cref="ITransport.Detach(NetworkMember)"/> of all of them.
            /// </summary>
            /// <returns>
            /// <c>true</c> - all transports were removed successfully.
            /// <c>false</c> - some transports had issues executing <see cref="ITransport.Detach(NetworkMember)"/>.
            /// </returns>
            public bool Clear{{method}}Transports()
            {
                lock (_lock)
                {
                    bool anyFailed = false;
                    foreach (var transport in {{storage}})
                    {
                        anyFailed |= !transport.InvokeDetach();
                    }

                    return !anyFailed;
                }
            }
            """);
        }
    }
}
