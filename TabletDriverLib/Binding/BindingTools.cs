using TabletDriverPlugin;

namespace TabletDriverLib.Binding
{
    public static class BindingTools
    {
        public static IBinding GetBinding(string full)
        {
            if (!string.IsNullOrWhiteSpace(full))
            {
                var tokens = full.Split(", ", 2);
                var binding = PluginManager.ConstructObject<IBinding>(tokens[0]);
                if (binding != null)
                    binding.Property = tokens[1];
                return binding;
            }
            else
            {
                return null;
            }
        }
    }
}