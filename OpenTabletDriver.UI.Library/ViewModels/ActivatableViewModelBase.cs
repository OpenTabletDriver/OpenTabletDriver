namespace OpenTabletDriver.UI.ViewModels;

public abstract class ActivatableViewModelBase : ViewModelBase
{
    private Action<CompositeDisposable>? _whenActivatedAction;
    private CompositeDisposable? _whenActivatedDisposables;
    private bool _isActivated;

    public event EventHandler? Activated;
    public event EventHandler? Deactivated;

    public virtual void OnActivated()
    {
        _isActivated = true;
        InvokeActivated();
        Activated?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnDeactivated()
    {
        _isActivated = false;
        DisposeObjects();
        Deactivated?.Invoke(this, EventArgs.Empty);
    }

    protected void WhenActivated(Action<CompositeDisposable> whenActivated)
    {
        _whenActivatedAction = whenActivated;

        // Dispose if there's some disposables left over
        DisposeObjects();
        // Invoke if we're currently navigated to
        InvokeActivated();
    }

    private void InvokeActivated()
    {
        if (_whenActivatedAction is not null && _isActivated)
        {
            _whenActivatedDisposables = new CompositeDisposable();
            _whenActivatedAction(_whenActivatedDisposables);
        }
    }

    private void DisposeObjects()
    {
        var whenNavigatedToDisposables = Interlocked.Exchange(ref _whenActivatedDisposables, null);
        whenNavigatedToDisposables?.Dispose();
    }
}
