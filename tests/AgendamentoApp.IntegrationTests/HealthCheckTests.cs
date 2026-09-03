using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace AgendamentoApp.IntegrationTests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public HealthCheckTests(WebApplicationFactory<global::Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_DeveRetornar200Ok_ComStatusHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"STATUS: {response.StatusCode} | BODY: {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task GetIndex_DeveRetornar200Ok_ComHtml()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
