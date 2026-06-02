using Aphelion.Caches;
using Aphelion.Core;
using Xunit;

namespace Aphelion.Tests.Caches;

public class GameObjectCacheTests : IDisposable
{
    public GameObjectCacheTests()
    {
        GameObjectCache.Instance.Value!.Clear();
    }

    public void Dispose()
    {
        GameObjectCache.Instance.Value!.Clear();
    }

    [Fact]
    public void Update_MovesFromUninitializedToGameObjects()
    {
        var go = GameObject.Instantiate("Test");

        GameObjectCache.Instance.Value!.Update();

        Assert.Single(GameObjectCache.Instance.Value!.GameObjects);
        Assert.Contains(go, GameObjectCache.Instance.Value!.GameObjects);
    }

    [Fact]
    public void Unregister_RemovesGameObjectAfterUpdate()
    {
        var go = GameObject.Instantiate("TestUnregister");
        GameObjectCache.Instance.Value!.Update();
        var objects = GameObjectCache.Instance.Value!.GameObjects.ToList();
        Assert.Contains(go, objects);

        GameObject.Destroy(go);
        GameObjectCache.Instance.Value!.Update();

        Assert.Empty(GameObjectCache.Instance.Value!.GameObjects);
    }
}