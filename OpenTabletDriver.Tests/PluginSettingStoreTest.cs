using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class BindingOptions
    {
        [BindingProperty("Action")]
        public PluginSettingStore? Action { set; get; }

        [Property("Message")]
        public string? Message { set; get; }
    }

    public class PluginSettingStoreTest
    {
        private const string MissingBindingPath = "SampleBinding.SampleKeyBinding, SampleBinding";

        private static PropertyInfo ActionProperty =>
            typeof(BindingOptions).GetProperty(nameof(BindingOptions.Action))!;

        [Fact]
        public void BindingProperty_Is_Created_As_A_Setting()
        {
            var store = new PluginSettingStore(typeof(BindingOptions));

            Assert.Contains(store.Settings, s => s.Property == nameof(BindingOptions.Action));
        }

        [Fact]
        public void Binding_Value_Round_Trips_Through_PluginSetting()
        {
            var store = new PluginSettingStore(typeof(KeyBinding));
            store[nameof(KeyBinding.Key)].SetValue("A");

            var setting = new PluginSetting(ActionProperty, store);
            var restored = setting.GetValueOrDefault<PluginSettingStore>(ActionProperty);

            Assert.NotNull(restored);
            Assert.Equal(store.Path, restored!.Path);
            Assert.Equal("A", restored[nameof(KeyBinding.Key)].GetValue<string>());
        }

        [Fact]
        public void HumanReadableString_Falls_Back_To_Path_When_Type_Is_Missing()
        {
            var store = CreateStoreWithMissingPath();

            Assert.Equal(MissingBindingPath, store.GetHumanReadableString());
        }

        /// <summary>
        /// A binding that itself holds a binding option stores that option as JSON.
        /// The summary is what the binding row's button and the binding editor pane
        /// show, so it has to read as a binding rather than as the raw JSON that
        /// interpolating the JToken produced.
        /// </summary>
        [Fact]
        public void HumanReadableString_Summarises_Nested_Binding_Option_As_Binding()
        {
            var store = new PluginSettingStore(typeof(BindingOptions));
            store[nameof(BindingOptions.Action)].SetValue(CreateKeyBindingStore("A"));

            string text = store.GetHumanReadableString();

            Assert.StartsWith(typeof(BindingOptions).FullName, text);
            // the nested binding is summarised as a binding, with its own settings
            Assert.Contains("Key Binding", text);
            Assert.Contains(nameof(KeyBinding.Key), text);
            Assert.Contains("A", text);
            // ...and not the serialized store
            Assert.DoesNotContain("\"Path\"", text);
            Assert.DoesNotContain("\"Settings\"", text);
        }

        [Fact]
        public void HumanReadableString_Keeps_Settings_When_Type_Is_Missing()
        {
            var store = CreateStoreWithMissingPath();
            store[nameof(KeyBinding.Key)].SetValue("A");

            string text = store.GetHumanReadableString();

            Assert.StartsWith(MissingBindingPath, text);
            Assert.Contains(nameof(KeyBinding.Key), text);
            Assert.Contains("A", text);
        }

        [Fact]
        public void Construct_Does_Not_Throw_And_Keeps_Settings_When_Type_Is_Missing()
        {
            var store = CreateStoreWithMissingPath();
            store[nameof(KeyBinding.Key)].SetValue("A");

            Assert.Null(store.Construct<IBinding>());

            Assert.Equal("A", store[nameof(KeyBinding.Key)].GetValue<string>());
            Assert.Single(store.Settings);
        }

        [Fact]
        public void ApplySettings_Restores_Binding_Property()
        {
            var store = new PluginSettingStore(typeof(BindingOptions));
            var binding = new PluginSettingStore(typeof(KeyBinding));
            binding[nameof(KeyBinding.Key)].SetValue("A");
            store[nameof(BindingOptions.Action)].SetValue(binding);

            var target = new BindingOptions();
            store.ApplySettings(target);

            Assert.NotNull(target.Action);
            Assert.Equal(binding.Path, target.Action!.Path);
            Assert.Equal("A", target.Action[nameof(KeyBinding.Key)].GetValue<string>());
        }

        [Fact]
        public void Settings_Are_Never_Null_After_Deserialization()
        {
            var json = new JObject
            {
                ["Path"] = typeof(BindingOptions).FullName,
                ["Settings"] = JValue.CreateNull()
            };

            var store = json.ToObject<PluginSettingStore>();

            Assert.NotNull(store);
            Assert.NotNull(store!.Settings);
            Assert.Empty(store.Settings);
        }

        /// <summary>
        /// A null entry ("Settings": [null], corrupt or hand-edited settings.json) is
        /// deserializable, and used to NRE every reader of Settings - ApplySettings (so
        /// Construct, so the daemon's whole SetSettings), the indexer and the summary.
        /// </summary>
        [Fact]
        public void Null_Setting_Entries_Are_Dropped_On_Deserialization()
        {
            var json = new JObject
            {
                ["Path"] = typeof(BindingOptions).FullName,
                ["Settings"] = new JArray { JValue.CreateNull() }
            };

            var store = json.ToObject<PluginSettingStore>();

            Assert.NotNull(store);
            Assert.Empty(store!.Settings);
            store.ApplySettings(new BindingOptions());
            store.GetHumanReadableString();
            _ = store[nameof(BindingOptions.Message)];
        }

        /// <summary>
        /// A stored value whose shape no longer matches the property - the corrupt-value
        /// case documented in sample-plugin/README.md - used to throw out of ApplySettings,
        /// which aborts Construct and with it the daemon's entire settings application.
        /// </summary>
        [Fact]
        public void ApplySettings_Skips_A_Value_That_Does_Not_Match_The_Property()
        {
            var store = new PluginSettingStore(typeof(BindingOptions));
            store[nameof(BindingOptions.Action)].SetValue(5);

            var target = new BindingOptions();
            store.ApplySettings(target);

            Assert.Null(target.Action);
            // the unreadable value is kept, so it can be fixed or become valid again
            Assert.Equal(JTokenType.Integer, store[nameof(BindingOptions.Action)].Value!.Type);
        }

        private static PluginSettingStore CreateStoreWithMissingPath()
        {
            return new PluginSettingStore(MissingBindingPath, new ObservableCollection<PluginSetting>());
        }

        private static PluginSettingStore CreateKeyBindingStore(string key)
        {
            var store = new PluginSettingStore(typeof(KeyBinding));
            store[nameof(KeyBinding.Key)].SetValue(key);
            return store;
        }
    }
}
