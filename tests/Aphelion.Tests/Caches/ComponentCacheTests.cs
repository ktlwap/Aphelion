using Aphelion.Caches;
using Aphelion.Core;
using Xunit;

namespace Aphelion.Tests.Caches;

public class ComponentCacheTests : IDisposable
{
    public ComponentCacheTests()
    {
        ComponentCache.Instance.Value!.Clear();
    }

    public void Dispose()
    {
        ComponentCache.Instance.Value!.Clear();
    }

    [Fact]
    public void Register_AddsToUninitialized()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Instance.Value!.Register(component);

        Assert.Empty(ComponentCache.Instance.Value!.Components);
    }

    [Fact]
    public void Update_MovesFromUninitializedToComponents()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Instance.Value!.Register(component);
        ComponentCache.Instance.Value!.Update();

        Assert.Single(ComponentCache.Instance.Value!.Components);
        Assert.Contains(component, ComponentCache.Instance.Value!.Components);
    }

    [Fact]
    public void Update_CallsStartOnAddedComponents()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Instance.Value!.Register(component);
        ComponentCache.Instance.Value!.Update();

        Assert.True(component.IsStarted);
    }

    [Fact]
    public void Unregister_RemovesComponentAfterUpdate()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Instance.Value!.Register(component);
        ComponentCache.Instance.Value!.Update();
        Assert.Contains(component, ComponentCache.Instance.Value!.Components);

        ComponentCache.Instance.Value!.Unregister(component);
        ComponentCache.Instance.Value!.Update();

        Assert.Empty(ComponentCache.Instance.Value!.Components);
    }

    [Fact]
    public void Update_CallsStopOnRemovedComponents()
    {
        var go = GameObject.Instantiate("Test");
        var component = new TestComponent { GameObject = go, Transform = go.Transform };

        ComponentCache.Instance.Value!.Register(component);
        ComponentCache.Instance.Value!.Update();

        ComponentCache.Instance.Value!.Unregister(component);
        ComponentCache.Instance.Value!.Update();

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