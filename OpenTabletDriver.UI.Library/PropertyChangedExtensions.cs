using System.ComponentModel;

namespace OpenTabletDriver.UI;

// we do it like this to avoid System.Linq.Expressions
internal static class PropertyChangedExtensions
{
    public static WeakRefPropertyChangedHandler<TSource, TTarget> HandlePropertyWeak<TSource, TTarget>(
        this TSource source,
        string sourceProperty,
        Func<TSource, TTarget> getter,
        Action<TSource, TTarget> action,
        bool invokeEventOnCreation = true)
            where TSource : class, INotifyPropertyChanged
    {
        return new WeakRefPropertyChangedHandler<TSource, TTarget>(source, sourceProperty, getter, action, invokeEventOnCreation);
    }

    public static StrongRefPropertyChangedHandler<TSource, TTarget> HandleProperty<TSource, TTarget>(
        this TSource source,
        string sourceProperty,
        Func<TSource, TTarget> getter,
        Action<TSource, TTarget> action,
        bool invokeOnCreation = true)
            where TSource : class, INotifyPropertyChanged
    {
        return new StrongRefPropertyChangedHandler<TSource, TTarget>(source, sourceProperty, getter, action, invokeOnCreation);
    }
}

internal class WeakRefPropertyChangedHandler<TSource, TTarget> : IDisposable
    where TSource : class, INotifyPropertyChanged
{
    private readonly WeakReference<TSource> _source;
    private readonly string _sourceProperty;
    private readonly Func<TSource, TTarget> _getter;
    private readonly Action<TSource, TTarget> _action;

    public WeakRefPropertyChangedHandler(TSource source, string sourceProperty, Func<TSource, TTarget> getter, Action<TSource, TTarget> action, bool invokeEventOnCreation)
    {
        _source = new WeakReference<TSource>(source);
        _sourceProperty = sourceProperty;
        _getter = getter;
        _action = action;

        source.PropertyChanged += HandlePropertyChanged;

        if (invokeEventOnCreation)
            action(source, getter(source));
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_source.TryGetTarget(out var source) && e.PropertyName == _sourceProperty)
            _action(source, _getter(source));
    }

    public void Dispose()
    {
        if (_source.TryGetTarget(out var source))
            source.PropertyChanged -= HandlePropertyChanged;
    }
}

internal class StrongRefPropertyChangedHandler<TSource, TTarget> : IDisposable
    where TSource : class, INotifyPropertyChanged
{
    private readonly TSource _source;
    private readonly string _sourceProperty;
    private readonly Func<TSource, TTarget> _getter;
    private readonly Action<TSource, TTarget> _action;

    public StrongRefPropertyChangedHandler(TSource source, string sourceProperty, Func<TSource, TTarget> getter, Action<TSource, TTarget> action, bool invokeEventOnCreation)
    {
        _source = source;
        _sourceProperty = sourceProperty;
        _getter = getter;
        _action = action;

        source.PropertyChanged += HandlePropertyChanged;

        if (invokeEventOnCreation)
            action(source, getter(source));
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == _sourceProperty)
            _action(_source, _getter(_source));
    }

    public void Dispose()
    {
        _source.PropertyChanged -= HandlePropertyChanged;
    }
}
