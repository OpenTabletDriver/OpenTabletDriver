using System.Collections.ObjectModel;

namespace OpenTabletDriver.UI;

public sealed class CompositeDisposable : Collection<IDisposable>, IDisposable
{
    public void Dispose()
    {
        this.ForEach(disposable => disposable.Dispose());
    }

    public static CompositeDisposable From(IDisposable disposable)
    {
        var composite = new CompositeDisposable();
        composite.Add(disposable);
        return composite;
    }

    public static CompositeDisposable From(IDisposable disposable1, IDisposable disposable2)
    {
        var composite = new CompositeDisposable();
        composite.Add(disposable1);
        composite.Add(disposable2);
        return composite;
    }

    public static CompositeDisposable From(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
    {
        var composite = new CompositeDisposable();
        composite.Add(disposable1);
        composite.Add(disposable2);
        composite.Add(disposable3);
        return composite;
    }

    public static CompositeDisposable From(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4)
    {
        var composite = new CompositeDisposable();
        composite.Add(disposable1);
        composite.Add(disposable2);
        composite.Add(disposable3);
        composite.Add(disposable4);
        return composite;
    }

    public static CompositeDisposable From(params IDisposable[] disposables)
    {
        var composite = new CompositeDisposable();
        composite.AddRange(disposables);
        return composite;
    }
}

public static class CompositeDisposableExtensions
{
    public static void DisposeWith(this IDisposable disposable, CompositeDisposable composite)
    {
        composite.Add(disposable);
    }
}
