using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    /// <summary>
    /// A plugin filter that hosts a binding in its options, the shape
    /// enabled by [BindingProperty].
    /// </summary>
    public class BindingOptionHost : IPositionedPipelineElement<IDeviceReport>
    {
        [BindingProperty("Action")]
        public PluginSettingStore? Action { set; get; }

        public PipelinePosition Position => PipelinePosition.PreTransform;
        public event Action<IDeviceReport?>? Emit;

        public void Consume(IDeviceReport? report) => Emit?.Invoke(report);
    }

    public class BindingOptionInjectionTest
    {
        private sealed class FakeMouseButtonHandler : IMouseButtonHandler
        {
            public void MouseDown(MouseButton button) { }
            public void MouseUp(MouseButton button) { }
        }

        /// <summary>
        /// A profile constructs a filter with its binding service manager, and the
        /// filter then builds the binding stored in its option the way every plugin
        /// does - parameterless, with no provider in sight. The binding must receive
        /// the same pointer instance the output mode flushes; before the provider was
        /// inherited it resolved null on every platform.
        /// </summary>
        [Fact]
        public void Binding_Option_Inherits_Provider_From_Host_Construction()
        {
            RegisterPluginType(typeof(BindingOptionHost));

            var pointer = new FakeMouseButtonHandler();
            // mirrors DriverDaemon.CreateBindingHandler
            var services = new ServiceManager();
            services.AddService<IMouseButtonHandler>(() => pointer);

            var hostStore = new PluginSettingStore(typeof(BindingOptionHost));
            hostStore[nameof(BindingOptionHost.Action)].SetValue(new PluginSettingStore(typeof(MouseBinding)));

            var host = Assert.IsType<BindingOptionHost>(
                hostStore.Construct<IPositionedPipelineElement<IDeviceReport>>(services));
            Assert.NotNull(host.Action);

            // what a plugin does in [OnDependencyLoad]
            var binding = host.Action.Construct<IBinding>();
            var mouseBinding = Assert.IsType<MouseBinding>(binding);

            Assert.Same(pointer, mouseBinding.Pointer);
        }

        private static void RegisterPluginType(Type type)
        {
            var field = typeof(PluginManager).GetField("pluginTypes", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            var pluginTypes = Assert.IsType<ConcurrentBag<TypeInfo>>(field!.GetValue(AppInfo.PluginManager));
            var typeInfo = type.GetTypeInfo();
            if (!pluginTypes.Contains(typeInfo))
                pluginTypes.Add(typeInfo);
        }
    }
}
