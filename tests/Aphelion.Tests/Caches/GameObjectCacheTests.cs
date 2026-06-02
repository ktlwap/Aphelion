using Aphelion.Caches;
using Aphelion.Core;
using Xunit;

namespace Aphelion.Tests.Caches;

public class GameObjectCacheTests : IDisposable
{
    public GameObjectCacheTests()
    {
        GameObjectCache.Clear();
    }

    public void Dispose()
    {
        GameObjectCache.Clear();
    }

    [Fact]
    public void Update_MovesFromUninitializedToGameObjects()
    {
        var go = GameObject.Instantiate("Test");

        GameObjectCache.Update();

        Assert.Single(GameObjectCache.GameObjects);
        Assert.Contains(go, GameObjectCache.GameObjects);
    }

    [Fact]
    public void Unregister_RemovesGameObjectAfterUpdate()
    {
        var go = GameObject.Instantiate("TestUnregister");
        GameObjectCache.Update();
        var objects = GameObjectCache.GameObjects.ToList();
        Assert.Contains(go, objects);

        GameObject.Destroy(go);
        GameObjectCache.Update();

        Assert.Empty(GameObjectCache.GameObjects);
    }
}