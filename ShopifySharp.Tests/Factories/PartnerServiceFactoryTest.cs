#nullable enable
using System;
using FakeItEasy;
using FluentAssertions;
using JetBrains.Annotations;
using ShopifySharp.Credentials;
using ShopifySharp.Factories;
using Xunit;

namespace ShopifySharp.Tests.Factories;

[TestSubject(typeof(PartnerServiceFactory))]
[Trait("Category", "Factories")]
public class PartnerServiceFactoryTest
{
    private const long OrganizationId = 123L;
    private const string AccessToken = "some-access-token";

    private readonly IServiceProvider _serviceProvider = A.Fake<IServiceProvider>();
    private readonly PartnerServiceFactory _sut;

    public PartnerServiceFactoryTest()
    {
        _sut = new PartnerServiceFactory(_serviceProvider);
    }

    [Fact]
    public void Create_ReturnsTheService()
    {
        // Act
        var service = _sut.Create(OrganizationId, AccessToken);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<PartnerService>().And.BeAssignableTo<IPartnerService>();
    }

    [Fact]
    public void Create_WhenGivenShopifyPartnerApiCredentials_ReturnsTheService()
    {
        // Setup
        var credentials = new ShopifyPartnerApiCredentials(OrganizationId, AccessToken);

        // Act
        var service = _sut.Create(credentials);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<PartnerService>().And.BeAssignableTo<IPartnerService>();
    }
}
