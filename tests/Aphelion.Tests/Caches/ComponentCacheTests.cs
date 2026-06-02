using Aphelion.Caches;
using Aphelion.Core;
using Xunit;

namespace Aphelion.Tests.Caches;

public class ComponentCacheTests : IDisposable
{
    public ComponentCacheTests()
    {
        ComponentCache.Clear();
    }

    public void Dispose()
    {
        ComponentCache.Clear();
    }

    [Fact]
    public void Register_AddsToUninitialized()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Register(component);

        Assert.Empty(ComponentCache.Components);
    }

    [Fact]
    public void Update_MovesFromUninitializedToComponents()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Register(component);
        ComponentCache.Update();

        Assert.Single(ComponentCache.Components);
        Assert.Contains(component, ComponentCache.Components);
    }

    [Fact]
    public void Update_CallsStartOnAddedComponents()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Register(component);
        ComponentCache.Update();

        Assert.True(component.IsStarted);
    }

    [Fact]
    public void Unregister_RemovesComponentAfterUpdate()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Register(component);
        ComponentCache.Update();
        Assert.Contains(component, ComponentCache.Components);

        ComponentCache.Unregister(component);
        ComponentCache.Update();

        Assert.Empty(ComponentCache.Components);
    }

    [Fact]
    public void Update_CallsStopOnRemovedComponents()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Register(component);
        ComponentCache.Update();

        ComponentCache.Unregister(component);
        ComponentCache.Update();

        Assert.True(component.IsStopped);
    }

    private class TestComponent : BaseComponent
    {
        public bool IsStarted { get; private set; }
        public bool IsStopped { get; private set; }

        public override void Start()
        {
            IsStarted = true;
        }

        public override void Stop()
        {
            IsStopped = true;
        }
    }
}