namespace OpenTabletDriver.UI.Navigation;

public interface INavigatorFactory
{
    INavigator GetOrCreate(string navHostName);
}
