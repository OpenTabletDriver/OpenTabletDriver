using System;
using System.Collections.Generic;
using Avalonia.Controls;
using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.UI.Controls;
using OpenTabletDriver.UI.Navigation;

namespace OpenTabletDriver.UI.Tests.Navigation;

public class NavigatorTests
{
    [Fact]
    public void CurrentIsNullByDefault()
    {
        var nav = CreateTestNavigator();
        nav.Current.Should().BeNull();
    }

    [Fact]
    public void Push()
    {
        var nav = CreateTestNavigator();
        using var navEvMon = nav.Monitor();

        nav.Push("test" as object); // cast to ensure it uses the right overload
        AssertText(nav.Current, "test");

        var expectedEvents = new List<OccurredEvent>()
            .AddStandardNavigationEvents(nav, null, CreateTestControl("test"), NavigationKind.Push)
            .ToArray();

        AssertEvents(navEvMon, expectedEvents);
    }

    [Fact]
    public void PushAfterPush()
    {
        var nav = CreateTestNavigator();
        using var navEvMon = nav.Monitor();

        nav.Push("test" as object);
        nav.Push("test2" as object);

        nav.CanGoBack.Should().BeTrue();
        AssertText(nav.Current, "test2");

        var expectedTest1 = CreateTestControl("test");
        var expectedTest2 = CreateTestControl("test2");

        var expectedEvents = new List<OccurredEvent>()
            .AddStandardNavigationEvents(nav, null, expectedTest1, NavigationKind.Push)
            .AddStandardNavigationEvents(nav, expectedTest1, expectedTest2, NavigationKind.Push)
            .ToArray();

        AssertEvents(navEvMon, expectedEvents);
    }

    [Fact]
    public void Pop()
    {
        var nav = CreateTestNavigator();
        using var navEvMon = nav.Monitor();

        nav.CanGoBack.Should().BeFalse();
        nav.Invoking(n => n.Pop())
            .Should().Throw<InvalidOperationException>("Navigator cannot go back when it has no history");

        nav.Push("test" as object);

        nav.CanGoBack.Should().BeFalse();
        nav.Invoking(n => n.Pop())
            .Should().Throw<InvalidOperationException>();

        nav.Push("test2" as object);

        nav.CanGoBack.Should().BeTrue();
        AssertText(nav.Current, "test2");
        nav.Invoking(n => n.Pop())
            .Should().NotThrow();

        nav.CanGoBack.Should().BeFalse();
        AssertText(nav.Current, "test");

        var expectedTest1 = CreateTestControl("test");
        var expectedTest2 = CreateTestControl("test2");

        var expectedEvents = new List<OccurredEvent>()
            .AddStandardNavigationEvents(nav, null, expectedTest1, NavigationKind.Push)
            .AddStandardNavigationEvents(nav, expectedTest1, expectedTest2, NavigationKind.Push)
            .AddStandardNavigationEvents(nav, expectedTest2, expectedTest1, NavigationKind.Pop)
            .ToArray();

        AssertEvents(navEvMon, expectedEvents);
    }

    [Fact]
    public void PopAfterThreeNexts()
    {
        var nav = CreateTestNavigator();
        nav.Push("test" as object);
        nav.Push("test2" as object);
        nav.Push("test3" as object);

        // Monitor late so we don't get the initial events
        using var navEvMon = nav.Monitor();

        nav.Pop();

        nav.CanGoBack.Should().BeTrue();
        AssertText(nav.Current, "test2");

        var expectedTest2 = CreateTestControl("test2");
        var expectedTest3 = CreateTestControl("test3");

        var expectedEvents = new List<OccurredEvent>()
            .AddStandardNavigationEvents(nav, expectedTest3, expectedTest2, NavigationKind.Pop)
            .ToArray();

        AssertEvents(navEvMon, expectedEvents);
    }

    [Fact]
    public void PushAsRoot()
    {
        var nav = CreateTestNavigator();
        using var navEvMon = nav.Monitor();

        nav.Push("test" as object, asRoot: true);

        nav.CanGoBack.Should().BeFalse();
        AssertText(nav.Current, "test");

        nav.Push("test2" as object);
        nav.Push("test3" as object, asRoot: true);

        nav.CanGoBack.Should().BeFalse();
        AssertText(nav.Current, "test3");

        var expectedTest1 = CreateTestControl("test");
        var expectedTest2 = CreateTestControl("test2");
        var expectedTest3 = CreateTestControl("test3");

        var expectedEvents = new List<OccurredEvent>()
            .AddStandardNavigationEvents(nav, null, expectedTest1, NavigationKind.PushAsRoot)
            .AddStandardNavigationEvents(nav, expectedTest1, expectedTest2, NavigationKind.Push)
            .AddStandardNavigationEvents(nav, expectedTest2, expectedTest3, NavigationKind.PushAsRoot)
            .ToArray();

        AssertEvents(navEvMon, expectedEvents);
    }

    [Fact]
    public void BackToRoot()
    {
        var nav = CreateTestNavigator();

        nav.Invoking(n => n.Pop(toRoot: true))
            .Should().Throw<InvalidOperationException>("Navigator cannot go back to root when it has no history");

        nav.Push("test" as object);

        using (var scopedNavEvMon = nav.Monitor())
        {
            nav.Pop(toRoot: true);
            scopedNavEvMon.OccurredEvents.Should().BeEmpty();
        }

        nav.Push("test2" as object);
        nav.Push("test3" as object);

        using (var scopedNavEvMon = nav.Monitor())
        {
            nav.Pop(toRoot: true);
            nav.CanGoBack.Should().BeFalse();
            AssertText(nav.Current, "test");

            var expectedTest1 = CreateTestControl("test");
            var expectedTest3 = CreateTestControl("test3");

            var expectedEvents = new List<OccurredEvent>()
                .AddStandardNavigationEvents(nav, expectedTest3, expectedTest1, NavigationKind.PopToRoot)
                .ToArray();
        }
    }

    [Fact]
    public void SingletonRoute()
    {
        var nav = CreateTestNavigator(sc => sc
            .AddSingleton(sc => new string("aaaa"))
            .AddSingletonRoute<string, TestControl>("test_route")
            .AddSingletonRoute<string, TestControl2>("other_nav", "test_route2")
            .AddNavigationMapping<NavigationMapNotFoundViewModel, TestControl3>());

        nav.Push("test_route");
        nav.Current.Should().BeOfType<TestControl>();

        var dummy = nav.Current as TestControl;
        var dummyContext = dummy!.DataContext;

        nav.Push("something" as object);
        nav.Pop();
        nav.Current.Should().BeSameAs(dummy);

        nav.Push("new_root" as object, asRoot: true);
        nav.Push("test_route");
        nav.Current.Should().NotBeSameAs(dummy, "the view should always return new instance when pushed")
            .And.BeOfType<TestControl>();

        ((Control)nav.Current!).DataContext.Should().BeSameAs(dummyContext, "the data context should be singleton");

        nav.Push("test_route2", asRoot: true);
        nav.Current.Should().BeOfType<TestControl3>("test_route2 is not a valid route for test_nav navigation host");
    }

    [Fact]
    public void TransientRoute()
    {
        var nav = CreateTestNavigator(sc => sc
            .AddTransient<string>(sc => new string("aaaa"))
            .AddTransientRoute<string, TestControl>("test_route")
            .AddTransientRoute<string, TestControl2>("other_nav", "test_route2")
            .AddNavigationMapping<NavigationMapNotFoundViewModel, TestControl3>());

        nav.Push("test_route");
        nav.Current.Should().BeOfType<TestControl>();

        var dummy = nav.Current as TestControl;
        var dummyContext = dummy!.DataContext;

        nav.Push("something" as object);
        nav.Pop();
        nav.Current.Should().BeSameAs(dummy, "Transient routes should return the same instance until popped out of the stack");

        nav.Push("new_root" as object, asRoot: true);
        nav.Push("test_route");
        nav.Current.Should().NotBeSameAs(dummy, "even if the route is singleton, the view should always return new when pushed")
            .And.BeOfType<TestControl>();

        ((Control)nav.Current!).DataContext.Should().NotBeSameAs(dummyContext, "the data context should be transient");

        nav.Push("test_route2", asRoot: true);
        nav.Current.Should().BeOfType<TestControl3>("'test_route2' is not a valid route for 'test_nav' navigation host");
    }

    // TODO: test cancelling behaviour

    private static INavigator CreateTestNavigator()
    {
        return CreateTestNavigator(sc => sc.AddNavigationMapping<string, TestControl>());
    }

    private static INavigator CreateTestNavigator(Action<IServiceCollection> configureServices)
    {
        var serviceCollection = new ServiceCollection()
            .UseNavigation<NavigatorFactory>();

        configureServices(serviceCollection);

        var serviceProvider = serviceCollection
            .BuildServiceProvider();

        var navFactory = serviceProvider
            .GetRequiredService<INavigatorFactory>();

        return navFactory.GetOrCreate("test_nav");
    }

    private static TestControl CreateTestControl(string text)
    {
        return new()
        {
            DataContext = text
        };
    }

    private static void AssertText(object? navObj, string text)
    {
        navObj.Should().BeOfType<TestControl>();
        ((TestControl)navObj!).Text.Should().Be(text);
    }

    private static void AssertEvents(IMonitor<INavigator> navEvMon, params OccurredEvent[] expectedEvents)
    {
        navEvMon.OccurredEvents.Should().BeEquivalentTo(expectedEvents, options => options
                .Using<TestControl>(ctx => ctx.Subject.Text.Should().BeEquivalentTo(ctx.Expectation.Text))
                .WhenTypeIs<TestControl>()
                .WithStrictOrdering()
                .Excluding(e => e.Sequence) // already accounted for by WithStrictOrdering
                .Excluding(e => e.TimestampUtc));
    }

    private class TestControl : ActivatableUserControl
    {
        public string? Text { get; private set; }

        protected override void OnDataContextChanged(EventArgs e)
        {
            Text = DataContext as string;
        }
    }

    private class TestControl2 : ActivatableUserControl
    {
        public string? Text { get; private set; }

        protected override void OnDataContextChanged(EventArgs e)
        {
            Text = DataContext as string;
        }
    }

    private class TestControl3 : ActivatableUserControl { }
}

internal static class OccurredEventListExtensions
{
    public static List<OccurredEvent> AddStandardNavigationEvents(
        this List<OccurredEvent> events,
        INavigator navigator,
        object? prev,
        object? curr,
       NavigationKind kind)
    {
        events.AddRange(new OccurredEvent[]
        {
            new OccurredEvent
            {
                EventName = nameof(INavigator.Navigating),
                Parameters = new object[]
                {
                    navigator,
                    new CancellableNavigationEventData(kind, prev, curr)
                }
            },
            new OccurredEvent
            {
                EventName = nameof(INavigator.Navigated),
                Parameters = new object[]
                {
                    navigator,
                    new NavigationEventData(kind, prev, curr)
                }
            }
        });

        return events;
    }
}
