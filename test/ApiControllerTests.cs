using System.Net;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace Tests;

public class ApiControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    public ApiControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateDefaultClient();
    }

    [Fact]
    public async Task Unauthenticated_request_to_anonymous_endpoint_returns_ok()
    {
        // Assert
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/api/anonymous/testname", UriKind.Relative)
        };
    
        // Act
        HttpResponseMessage response = await _client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Unauthenticated_request_to_authenticated_endpoint_returns_redirect()
    {
        // Assert
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/api/authenticated/testname", UriKind.Relative)
        };
    
        // Act
        HttpResponseMessage response = await _client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task Authenticated_request_to_autheticated_endpoint_returns_ok()
    {
        // Assert
        HttpClient client = _factory.GetAuthenticatedClient();

        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/api/authenticated/testname", UriKind.Relative)
        };
    
        // Act
        HttpResponseMessage response = await client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Unauthenticated_request_to_anonymous_antiforgery_endpoint_without_tokens_returns_bad_request()
    {
        // Assert
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/api/anonymous/antiforgery/testname", UriKind.Relative)
        };
    
        // Act
        HttpResponseMessage response = await _client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);   
    }

    [Fact]
    public async Task Unauthenticated_request_to_anonymous_antiforgery_endpoint_with_tokens_returns_ok()
    {
        // Assert
        AntiforgeryTokens tokens = await _factory.GetAntiforgeryTokensAsync();

        CookieContainerHandler cookieHandler = new();
        cookieHandler.Container.Add(
            _factory.Server.BaseAddress,
            new Cookie(tokens.CookieName, tokens.CookieValue));

        HttpClient client = _factory.CreateDefaultClient(cookieHandler);

        client.DefaultRequestHeaders.Add(tokens.HeaderName, tokens.RequestToken);
        
        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/api/anonymous/antiforgery/testname", UriKind.Relative)
        };
    
        // Act
        HttpResponseMessage response = await client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);   
    }

// TODO: Break up login part so it's clear what's happening on stack overflow post
    [Fact]
    public async Task Authenticated_request_to_authenticated_antiforgery_endpoint_with_tokens_returns_ok()
    {
        // Assert
        AntiforgeryTokens tokens = await _factory.GetAntiforgeryTokensAsync();

        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        CookieContainerHandler cookieHandler = new();
        cookieHandler.Container.Add(
            _factory.Server.BaseAddress,
            new Cookie(tokens.CookieName, tokens.CookieValue));

        // HttpClient client = _factory.GetAuthenticatedClient(cookieHandler);
        HttpClient client = _factory.CreateDefaultClient(cookieHandler);

        Uri uri = new($"{client.BaseAddress!.AbsoluteUri}login");

        Dictionary<string, string> postData = new()
        {
            { "Input.UserName", "testname" },
            { "Input.Password", "password" },
            { tokens!.FormFieldName, tokens.RequestToken }
        };

        HttpContent formContent = new FormUrlEncodedContent(postData);

        HttpResponseMessage loginResponse = await client.PostAsync(uri, formContent, cancellationToken);

        List<string> cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        client.DefaultRequestHeaders.Add(tokens.HeaderName, tokens.RequestToken);
        client.DefaultRequestHeaders.Add("Cookie", cookies);

        HttpRequestMessage message = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("/api/authenticated/antiforgery/testname", UriKind.Relative)
        };
    
        // Act
        HttpResponseMessage response = await client.SendAsync(message);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);   
    }
}