using Aphelion.Caches;
using Aphelion.Core;
using Xunit;

namespace Aphelion.Tests.Core;

public class GameObjectTests : IDisposable
{
    public GameObjectTests()
    {
        GameObjectCache.Instance.Value!.Clear();
        ComponentCache.Instance.Value!.Clear();
    }

    public void Dispose()
    {
        GameObjectCache.Instance.Value!.Clear();
        ComponentCache.Instance.Value!.Clear();
    }

    [Fact]
    public void Instantiate_CreatesGameObjectWithCorrectName()
    {
        var name = "TestObject";
        var go = GameObject.Instantiate(name);

        Assert.Equal(name, go.Name);
        Assert.NotNull(go.Transform);
        Assert.Equal(go, go.Transform.GameObject);
    }

    [Fact]
    public void Instantiate_ThrowsWhenNameInUse()
    {
        var name = "DuplicateName";
        GameObject.Instantiate(name);
        GameObjectCache.Instance.Value!.Update(); // Move to active list so IsNameAlreadyInUse sees it

        Assert.Throws<Exception>(() => GameObject.Instantiate(name));
    }

    [Fact]
    public void AddComponent_AddsAndRegistersComponent()
    {
        var go = GameObject.Instantiate("Test");
        var component = go.AddComponent<TestComponent>();

        Assert.Contains(component, go.Components);
        Assert.Equal(go, component.GameObject);
        Assert.Equal(go.Transform, component.Transform);
    }

    [Fact]
    public void AddComponent_ThrowsWhenAlreadyExists()
    {
        var go = GameObject.Instantiate("Test");
        go.AddComponent<TestComponent>();

        Assert.Throws<Exception>(() => go.AddComponent<TestComponent>());
    }

    [Fact]
    public void GetComponent_ReturnsComponent()
    {
        var go = GameObject.Instantiate("Test");
        var added = go.AddComponent<TestComponent>();

        var found = go.GetComponent<TestComponent>();

        Assert.Equal(added, found);
    }

    [Fact]
    public void GetComponent_ThrowsWhenNotFound()
    {
        var go = GameObject.Instantiate("Test");

        Assert.Throws<Exception>(() => go.GetComponent<TestComponent>());
    }

    [Fact]
    public void GetComponentNullSafe_ReturnsNullWhenNotFound()
    {
        var go = GameObject.Instantiate("Test");

        var found = go.GetComponentNullSafe<TestComponent>();

        Assert.Null(found);
    }

    [Fact]
    public void RemoveComponent_RemovesAndUnregisters()
    {
        var go = GameObject.Instantiate("Test");
        var component = go.AddComponent<TestComponent>();
        Assert.Contains(component, go.Components);

        go.RemoveComponent<TestComponent>();

        Assert.Empty(go.Components);
    }

    [Fact]
    public void Destroy_UnregistersGameObjectAndComponents()
    {
        var go = GameObject.Instantiate("Test");
        var component = go.AddComponent<TestComponent>();

        GameObjectCache.Instance.Value!.Update();
        ComponentCache.Instance.Value!.Update();

        Assert.Contains(go, GameObjectCache.Instance.Value!.GameObjects);
        Assert.Contains(component, ComponentCache.Instance.Value!.Components);

        GameObject.Destroy(go);

        GameObjectCache.Instance.Value!.Update();
        ComponentCache.Instance.Value!.Update();

        Assert.Empty(GameObjectCache.Instance.Value!.GameObjects);
        Assert.Empty(ComponentCache.Instance.Value!.Components);
    }

    private class TestComponent : BaseComponent
    {
    }

    private class AnotherComponent : BaseComponent
    {
    }
}