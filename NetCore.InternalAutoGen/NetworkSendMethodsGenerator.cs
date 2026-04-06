using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.IO;

namespace NetCore.InternalAutoGen
{
    /// <summary>
    /// Generator covering INetworkSendMethods interface.
    /// </summary>
    [Generator]
    public class NetworkSendMethodsGenerator : IIncrementalGenerator
    {
        const string DirectTargetInterface = "ISendNetworkMessaging";
        const string TransportTargetInterface = "ITransportBasedSendNetworkMessaging";
        const string GenericArgumentStyle = "TTransport";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, static (s, _) => Execute(s));
        }

        static void Execute(SourceProductionContext source)
        {
            using (var storage1 = new StringWriter())
            {
                using var w = new IndentedTextWriter(storage1);
                /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
                /// .
                /// .                                               Direct sending
                /// .
                /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>

                w.WriteLine($"using System;");
                w.WriteLine($"using NetCore.Transports;");
                w.WriteLine($"using System.Runtime.CompilerServices;");
                w.WriteLine();
                w.WriteLine($"namespace {NetworkMethodHelpers.Namespace}");
                w.WriteLine("{");
                w.Indent++;

                string className = DirectTargetInterface.Substring(1) + "Extensions";
                w.WriteLine($"public static partial class {className}");
                w.WriteLine("{");
                w.Indent++;

                GenerateDirectSendingMethods(w, DirectTargetInterface, method: "Unreliable");
                InsertRegionSpacing(w);
                GenerateDirectSendingMethods(w, DirectTargetInterface, method: "Reliable");
                InsertRegionSpacing(w);
                GenerateDirectSendingMethods(w, DirectTargetInterface, method: "Sequential");
                InsertRegionSpacing(w);
                GenerateDirectSendingMethods(w, DirectTargetInterface, method: "Resilient");

                w.Indent--;
                w.WriteLine("}");
                w.Indent--;
                w.WriteLine("}");

                source.AddSource(className, storage1.ToString());
            }

            using (var storage2 = new StringWriter())
            {
                using var w = new IndentedTextWriter(storage2);
                /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
                /// .
                /// .                                           Sending via transports
                /// .
                /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
                w.WriteLine($"using System;");
                w.WriteLine($"using NetCore.Transports;");
                w.WriteLine($"using System.Runtime.CompilerServices;");
                w.WriteLine();
                w.WriteLine($"namespace {NetworkMethodHelpers.Namespace}");
                w.WriteLine("{");
                w.Indent++;

                string className = TransportTargetInterface.Substring(1) + "Extensions";
                w.WriteLine($"public static partial class {className}");
                w.WriteLine("{");
                w.Indent++;

                GenerateTransportBasedSendingMethods(w, TransportTargetInterface, method: "Unreliable", "IUnreliableTransport");
                InsertRegionSpacing(w);
                GenerateTransportBasedSendingMethods(w, TransportTargetInterface, method: "Reliable", "IReliableTransport");
                InsertRegionSpacing(w);
                GenerateTransportBasedSendingMethods(w, TransportTargetInterface, method: "Sequential", "ISequentialTransport");
                InsertRegionSpacing(w);
                GenerateTransportBasedSendingMethods(w, TransportTargetInterface, method: "Resilient", "IResilientTransport");

                w.Indent--;
                w.WriteLine("}");
                w.Indent--;
                w.WriteLine("}");

                source.AddSource($"{className}.g.cs", storage2.ToString());
            }

            // Simplifications:
            static void InsertRegionSpacing(IndentedTextWriter w)
            {
                for (int i = 0; i < 4; i++) w.WriteLine();
            }
        }

        static void GenerateDirectSendingMethods(IndentedTextWriter w, string directTarget, string method)
        {
            w.SplitAndWriteLine($$"""
            /// <inheritdoc cref="{{directTarget}}.Send{{method}}(scoped ReadOnlySpan<byte> datagram, ref Header, ref Flags)"/>
            public static void Send{{method}}(this {{directTarget}} target, scoped ReadOnlySpan<byte> datagram)
            {
                Header header = Header.Get();
                Flags flags = Flags.Get();
                target.Send{{method}}(datagram, ref header, ref flags);
            }

            /// <inheritdoc cref="{{directTarget}}.Send{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static void Send{{method}}(this {{directTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Header header)
            {
                Flags flags = Flags.Get();
                target.Send{{method}}(datagram, ref header, ref flags);
            }

            /// <inheritdoc cref="{{directTarget}}.Send{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static void Send{{method}}(this {{directTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Flags flags)
            {
                Header header = Header.Get();
                target.Send{{method}}(datagram, ref header, ref flags);
            }

            /// <inheritdoc cref="{{directTarget}}.Send{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            /// <param name="datagram"/>
            /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
            /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
            public static void Send{{method}}(this {{directTarget}} target, scoped ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null)
            {
                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                target.Send{{method}}(datagram, ref header, ref flags);
            }
            """);
        }

        static void GenerateTransportBasedSendingMethods(IndentedTextWriter w, string transportTarget, string method, string genericArgs)
        {
            w.SplitAndWriteLine($$"""
            /// <inheritdoc cref="{{transportTarget}}.Send{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static void Send{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                Header header = Header.Get();
                Flags flags = Flags.Get();
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
            }

            /// <inheritdoc cref="{{transportTarget}}.Send{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static void Send{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Header header) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                Flags flags = Flags.Get();
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
            }

            /// <inheritdoc cref="{{transportTarget}}.Send{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static void Send{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Flags flags) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                Header header = Header.Get();
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
            }

            /// <inheritdoc cref="{{transportTarget}}.Send{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            /// <param name="datagram"/>
            /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
            /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
            public static void Send{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
            }



            
            /// <inheritdoc cref="TrySend{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static bool TrySend{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                if (!target.Has{{method}}Transport<{{GenericArgumentStyle}}>())
                    return false;
            
                Header header = Header.Get();
                Flags flags = Flags.Get();
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
                return true;
            }

            /// <inheritdoc cref="TrySend{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static bool TrySend{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Header header) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                if (!target.Has{{method}}Transport<{{GenericArgumentStyle}}>())
                    return false;

                Flags flags = Flags.Get();
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
                return true;
            }

            /// <inheritdoc cref="TrySend{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static bool TrySend{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Flags flags) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                if (!target.Has{{method}}Transport<{{GenericArgumentStyle}}>())
                    return false;

                Header header = Header.Get();
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
                return true;
            }

            /// <inheritdoc cref="TrySend{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            public static bool TrySend{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                if (!target.Has{{method}}Transport<{{GenericArgumentStyle}}>())
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
                return true;
            }

            /// <inheritdoc cref="TrySend{{method}}{{{GenericArgumentStyle}}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
            /// <param name="datagram"/>
            /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
            /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
            public static bool TrySend{{method}}<{{GenericArgumentStyle}}>(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null) where {{GenericArgumentStyle}} : class, {{genericArgs}}
            {
                if (!target.Has{{method}}Transport<{{GenericArgumentStyle}}>())
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                target.Send{{method}}<{{GenericArgumentStyle}}>(datagram, ref header, ref flags);
                return true;
            }
            """);
        }
    }
}

///// <inheritdoc cref="TrySend{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
//public static bool TrySend{{method}}(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram)
//{
//    if (!target.HasAny{{method}}Transport())
//        return false;

//    Header header = Header.Get();
//    Flags flags = Flags.Get();
//    target.Send{{method}}(datagram, ref header, ref flags);
//    return true;
//}

///// <inheritdoc cref="TrySend{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
//public static bool TrySend{{method}}(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Header header)
//{
//    if (!target.HasAny{{method}}Transport())
//    {
//        header.DisposeIfUnlocked();
//        return false;
//    }

//    Flags flags = Flags.Get();
//    target.Send{{method}}(datagram, ref header, ref flags);
//    return true;
//}

///// <inheritdoc cref="TrySend{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
//public static bool TrySend{{method}}(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Flags flags)
//{
//    if (!target.HasAny{{method}}Transport())
//    {
//        flags.DisposeIfUnlocked();
//        return false;
//    }

//    Header header = Header.Get();
//    target.Send{{method}}(datagram, ref header, ref flags);
//    return true;
//}

///// <inheritdoc cref="TrySend{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
//public static bool TrySend{{method}}(this {{transportTarget}} target, scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
//{
//    if (!target.HasAny{{method}}Transport())
//    {
//        header.DisposeIfUnlocked();
//        flags.DisposeIfUnlocked();
//        return false;
//    }

//    target.Send{{method}}(datagram, ref header, ref flags);
//    return true;
//}

///// <inheritdoc cref="TrySend{{method}}(scoped ReadOnlySpan{byte}, ref Header, ref Flags)"/>
///// <param name="datagram"/>
///// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
///// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
//public static bool TrySend{{method}}(this {{transportTarget}} target,
//
//
//, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null)
//{
//    if (!target.HasAny{{method}}Transport())
//        return false;

//    Header header = Header.Get();
//    headerSetup?.Invoke(ref header);
//    Flags flags = Flags.Get();
//    flagsSetup?.Invoke(ref flags);
//    target.Send{{method}}(datagram, ref header, ref flags);
//    return true;
//}