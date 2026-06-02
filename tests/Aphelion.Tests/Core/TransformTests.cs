using System.Numerics;
using Aphelion.Caches;
using Aphelion.Core;
using Xunit;

namespace Aphelion.Tests.Core;

public class TransformTests : IDisposable
{
    public TransformTests()
    {
        GameObjectCache.Instance.Value!.Clear();
    }

    public void Dispose()
    {
        GameObjectCache.Instance.Value!.Clear();
    }

    [Fact]
    public void Transform_InitialValues_AreCorrect()
    {
        var go = GameObject.Instantiate("TestObject");
        var transform = go.Transform;

        Assert.Equal(Vector2.Zero, transform.Position);
        Assert.Equal(Vector2.One, transform.Scale);
        Assert.Equal(0f, transform.Rotation);
        Assert.Equal(go, transform.GameObject);
    }

    [Fact]
    public void Position_SetAndGet_ReturnsCorrectValue()
    {
        var go = GameObject.Instantiate("TestObject");
        var transform = go.Transform;
        var newPosition = new Vector2(10f, 20f);

        transform.Position = newPosition;

        Assert.Equal(newPosition, transform.Position);
    }

    [Fact]
    public void Scale_SetAndGet_ReturnsCorrectValue()
    {
        var go = GameObject.Instantiate("TestObject");
        var transform = go.Transform;
        var newScale = new Vector2(2f, 2f);

        transform.Scale = newScale;

        Assert.Equal(newScale, transform.Scale);
    }

    [Fact]
    public void Rotation_SetAndGet_ReturnsCorrectValue()
    {
        var go = GameObject.Instantiate("TestObject");
        var transform = go.Transform;
        var newRotation = 90f;

        transform.Rotation = newRotation;

        Assert.Equal(newRotation, transform.Rotation);
    }
}