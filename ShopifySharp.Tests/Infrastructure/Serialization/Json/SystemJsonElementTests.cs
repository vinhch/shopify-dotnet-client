using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using ShopifySharp.Infrastructure.Serialization.Json;
using Xunit;

namespace ShopifySharp.Tests.Infrastructure.Serialization.Json;

[Trait("Category", "Serialization"), TestSubject(typeof(SystemJsonElement))]
public class SystemJsonElementTests
{
    #region Constructor

    [Fact]
    public void Constructor_UsingJsonDocument_AssignsRootElement()
    {
        // Setup
        const string json = """{"foo":"bar"}""";
        var doc = JsonDocument.Parse(json);

        // Act
        var node = new SystemJsonElement(doc);

        // Assert
        node.GetRawObject().ValueKind.Should().Be(JsonValueKind.Object);
        node.GetProperty("foo")
            .GetRawObject()
            .Should()
            .BeOfType<JsonElement>()
            .Subject
            .GetString()
            .Should()
            .Be("bar");
    }

    [Fact]
    public void Constructor_UsingElement_AndOptionalDocument_AssignsElement()
    {
        // Setup
        var doc = JsonDocument.Parse("[1,2,3]");

        // Act
        var node = new SystemJsonElement(doc.RootElement, doc);

        // Assert
        node.ValueType.Should().Be(JsonValueType.Array);
        node.GetArrayLength().Should().Be(3);
    }

    #endregion

    #region object? GetRawObject()

    [Fact]
    public void GetRawObject_ShouldReturnTheJsonElement()
    {
        // Setup
        const string expectedValue = "bar";
        var doc = JsonDocument.Parse($$"""{"foo":"{{expectedValue}}"}""");
        var node = new SystemJsonElement(doc);

        // Act
        var result = node.GetRawObject();

        // Assert
        result.Should().BeOfType<JsonElement>().Which.GetProperty("foo").GetString().Should().Be(expectedValue);
    }

    #endregion

    #region JsonValueType ValueType { get }

    [Theory]
    [InlineData(" null ", JsonValueType.Null)]
    [InlineData(" true ", JsonValueType.True)]
    [InlineData(" false ", JsonValueType.False)]
    [InlineData(""" "some string" """, JsonValueType.String)]
    [InlineData(" 123 ", JsonValueType.Number)]
    [InlineData(""" ["an array"] """, JsonValueType.Array)]
    [InlineData(""" {"foo":"bar"} """, JsonValueType.Object)]
    public void ValueType_ShouldBeAssignedAccordingToElementValueKind(string json, JsonValueType jsonValueType)
    {
        // Setup
        var doc = JsonDocument.Parse(json);

        // Act
        var node = new SystemJsonElement(doc);

        // Assert
        node.ValueType.Should().Be(jsonValueType);
    }

    [Fact]
    public void ValueKind_WhenElementValueKindIsUndefined_ShouldUseJsonValueTypeUndefined()
    {
        // Setup
        // The only way to get a JsonElement with a value of Undefined is to use default
        JsonElement element = default;

        // Act
        var node = new SystemJsonElement(element);

        // Assert
        node.ValueType.Should().Be(JsonValueType.Undefined);
    }

    #endregion

    #region bool TryGetProperty(string propertyName, out IJsonNode node)

    [Fact]
    public void TryGetProperty_WhenPropertyExists_ShouldReturnTrueAndSetNode()
    {
        // Setup
        var doc = JsonDocument.Parse("{\"num\":123}");
        var node = new SystemJsonElement(doc);

        // Act
        var result = node.TryGetProperty("num", out var child);

        // Assert
        result.Should().BeTrue();
        child.ValueType.Should().Be(JsonValueType.Number);
        child.GetRawObject().Should().BeOfType<JsonElement>().Which.GetInt32().Should().Be(123);
    }

    #endregion

    #region IJsonNode GetProperty(string propertyName)

    [Fact]
    public void TryGetProperty_WhenPropertyDoesNotExist_ShouldReturnFalseAndNull()
    {
        // Setup
        var doc = JsonDocument.Parse("""{"foo":"bar"}""");
        var node = new SystemJsonElement(doc);

        // ACt
        var result = node.TryGetProperty("x", out var child);

        // Assert
        result.Should().BeFalse();
        child.Should().BeNull();
    }

    [Fact]
    public void GetProperty_WhenPropertyDoesNotExist_ShouldThrows()
    {
        // Setup
        var doc = JsonDocument.Parse("{}");
        var node = new SystemJsonElement(doc);

        // Act
        var act = () => node.GetProperty("some-property-name");

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    #endregion

    #region IJsonNodeObjectEnumerator EnumerateObject()

    [Fact]
    public void EnumerateObject_ShouldReturnProperties()
    {
        // Setup
        var doc = JsonDocument.Parse("""{"a":1,"b":true,"c":"foo"}""");
        var node = new SystemJsonElement(doc);
        var props = new List<JsonValueType>();

        // Act
        var enumerator = node.EnumerateObject();
        foreach (var x in enumerator)
        {
            props.Add(x.ValueType);
        }

        // Assert
        props.Should().ContainInOrder(JsonValueType.Number, JsonValueType.True, JsonValueType.String);
    }

    #endregion

    #region string GetRawText()

    [Fact]
    public void GetRawText_ShouldReturnOriginalJson()
    {
        // Setup
        const string json = """{"foo":"bar"}""";
        var doc = JsonDocument.Parse(json);
        var node = new SystemJsonElement(doc);

        // Act
        var rawText = node.GetRawText();

        // Assert
        rawText.Should().Be(json);
    }

    #endregion

    #region int GetArrayLength()

    [Theory]
    [InlineData("[]", 0)]
    [InlineData("[10, 20, 30]", 3)]
    public void GetArrayLength_ShouldReturnCorrectLength(string json, int expectedLength)
    {
        // Setup
        var doc = JsonDocument.Parse(json);
        var node = new SystemJsonElement(doc);

        // Act
        var arrayLength = node.GetArrayLength();

        // Assert
        arrayLength.Should().Be(expectedLength);
    }

    #endregion

    #region int GetPropertyCount()

    [Fact]
    public void GetPropertyCount_ReturnsNumberOfProperties()
    {
        // Setup
        var doc = JsonDocument.Parse("""{"foo":"bar","bat":"baz"}""");
        var node = new SystemJsonElement(doc);

        // Act
        var propertyCount = node.GetPropertyCount();

        // Assert
        propertyCount.Should().Be(2);
    }

    #endregion

    #region Dispose()

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldOnlyDisposeTheJsonDocumentOnce()
    {
        // Setup
        var doc = JsonDocument.Parse("{}");
        var node = new SystemJsonElement(doc);

        // Act
        node.Dispose();
        var disposalAction = () =>
        {
            for (var i = 0; i < 4; i++)
                node.Dispose();
        };

        // Assert
        disposalAction.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldDisposeTheJsonDocument()
    {
        // Setup
        var doc = JsonDocument.Parse("{}");
        var node = new SystemJsonElement(doc);

        // Act
        node.Dispose();
        var act = () => node.GetRawText();

        // Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    #endregion
}
