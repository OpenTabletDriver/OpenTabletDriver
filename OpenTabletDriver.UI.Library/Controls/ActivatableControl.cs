using Avalonia;
using Avalonia.Controls;
using OpenTabletDriver.UI.ViewModels;

namespace OpenTabletDriver.UI.Controls;

public abstract class ActivatableUserControl : UserControl
{
    private Action<CompositeDisposable>? _whenActivatedAction;
    private CompositeDisposable? _whenActivatedDisposables;
    private bool _isActivated;
    private bool _isVisible;

    public event EventHandler? Activated;
    public event EventHandler? Deactivated;

    protected override void OnDataContextChanged(EventArgs e)
    {
        TryActivate();
        base.OnDataContextChanged(e);
    }

    protected sealed override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isVisible = true;
        TryActivate();
        base.OnAttachedToVisualTree(e);
    }

    protected sealed override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isVisible = false;
        DisposeObjects();
        if (DataContext is ActivatableViewModelBase vm)
            vm.OnDeactivated();

        Deactivated?.Invoke(this, EventArgs.Empty);
        base.OnDetachedFromVisualTree(e);
    }

    protected void WhenActivated(Action<CompositeDisposable> whenActivated)
    {
        _whenActivatedAction = whenActivated;

        // Dispose if there's some disposables left over
        DisposeObjects();
        // Invoke if we're currently navigated to
        InvokeActivated();
    }

    private void TryActivate()
    {
        if (!_isActivated && _isVisible && DataContext is ActivatableViewModelBase vm)
        {
            _isActivated = true;
            vm.OnActivated();
            InvokeActivated();
            Activated?.Invoke(this, EventArgs.Empty);
        }
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

    protected override Type StyleKeyOverride => typeof(ActivatableUserControl);
}
