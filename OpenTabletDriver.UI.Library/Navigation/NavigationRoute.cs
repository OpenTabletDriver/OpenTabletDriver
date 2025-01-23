namespace OpenTabletDriver.UI.Navigation;

// TODO: improve doc
/// <summary>
/// Represents a route that can be navigated to.
/// If Name is null, this route will act as a mapper from ObjectType to ViewType
/// </summary>
/// <param name="Host"></param>
/// <param name="Name"></param>
/// <param name="ObjectType"></param>
/// <param name="ViewType"></param>
public record NavigationRoute(
    string? Host,
    string? Name,
    Type ObjectType,
    Type ViewType
);
