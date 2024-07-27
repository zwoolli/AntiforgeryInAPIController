using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace Tests;

/// <summary>
/// A class representing a factory for creating instances of the application.
/// </summary>
/// <typeparam name="TProgram">A type in the entry point assembly of the application</typeparam>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<Program> where TProgram : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomWebApplicationFactory<typeparamref name="TProgram"/>"/> class.
    /// </summary>
    public CustomWebApplicationFactory() : base()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
        ClientOptions.HandleCookies = true;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAntiforgeryTokenResource();
    }

    /// <summary>
    /// Configures a test authentication scheme to use when accessing a route requiring authroization
    /// </summary>
    /// <returns>
    /// An <see cref="HttpClient"/> with the test scheme authorization headers
    /// </returns>
    public HttpClient GetAuthenticatedClient(CookieContainerHandler? cookieHandler = default)
    {
        cookieHandler ??= new();

        string testScheme = "TestScheme";

        HttpClient client = WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestAuthenticationScheme(testScheme);
        })
        .CreateDefaultClient(cookieHandler);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: testScheme);

        return client;
    }

    /// <summary>
    /// Gets a set of valid antiforgery tokens for the application as an asynchronous operation.
    /// </summary>
    /// <param name="httpClientFactory">
    /// An optional delegate to a method to provide <see cref="HttpClient"/> to use to obtain the response.
    /// </param>
    /// <param name="cancellationToken">
    /// The optional <see cref="CancellationToken"/> to use for the HTTP request to obtain the response.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get a set of valid
    /// antiforgery (CSRF/XSRF) tokens to use for HTTP POST requests to the test server
    /// </returns>
    public async Task<AntiforgeryTokens> GetAntiforgeryTokensAsync(
        Func<HttpClient>? httpClientFactory = null,
        bool isAuthenticated = false,
        CancellationToken cancellationToken = default)
    {
        using HttpClient httpClient = isAuthenticated
            ? GetAuthenticatedClient()
            : httpClientFactory?.Invoke() ?? CreateDefaultClient();

        AntiforgeryTokens? tokens = await httpClient.GetFromJsonAsync<AntiforgeryTokens>(
            AntiforgeryTokenController.GetTokensUri,
            cancellationToken);

        return tokens!;
    }
}