# Description
I have an ASP.NET Core API controller endpoint that requires:

 - an authenticated user, and
 - validation of antiforgery tokens

I want to perform an integration test on this endpoint.

# Problem
I am unable to send a request that has both an authenticated user and the necessary antiforgery tokens/cookies and authentication cookies. Therefore, the endpoint continues to return a `Bad Request` before reaching the handler. 

# Code
To help walk through this question, I have created a [sample application](https://github.com/zwoolli/AntiforgeryInAPIController) to demonstrate the issue I'm having. 
## API Controller Endpoints
The sample application has an [API controller](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/src/Controllers/ApiController.cs) with four POST endpoints:

**1. Unauthenticated (anonymous) endpoint - antiforgery validation NOT required**
```
[AllowAnonymous]
[HttpPost("Anonymous/{name}")]
public IActionResult AnonymousPost(string name)
{
    return Ok(name);
}
```
**2. Authenticated endpoint - antiforgery validation NOT required**
```
[HttpPost("Authenticated/{name}")]
public IActionResult AuthenticatedPost(string name)
{
    return Ok(name);
}
```
**3. Unauthenticated (anonymous) endpoint - antiforgery validation required**
```
[AllowAnonymous]
[ValidateAntiForgeryToken]
[HttpPost("Anonymous/Antiforgery/{name}")]
public IActionResult AnonymousAntiforgeryPost(string name)
{
    return Ok(name);
}
```
**4. Authenticated endpoint - antiforgery validation required**
```
[ValidateAntiForgeryToken]
[HttpPost("Authenticated/Antiforgery/{name}")]
public IActionResult AuthenticatedAntiforgeryPost(string name)
{
    return Ok(name);
}
```
Endpoint #4, which requires an authenticated user as well as validation of antiforgery tokens, is the endpoint I am unable to successfully test.

## Authentication
The application uses cookie authentication and requires an authenticated user.
```
// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.LoginPath = new PathString("/Login");
    }).AddTwoFactorRememberMeCookie();

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```
The test project configures a [test authentication scheme](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/test/IWebHostBuilderExtensions.cs#L14)
```
public static IWebHostBuilder ConfigureTestAuthenticationScheme(this IWebHostBuilder builder, string scheme)
{
    ArgumentNullException.ThrowIfNull(builder);

    return builder.ConfigureTestServices(services =>
    {
        services.AddAuthentication(defaultScheme: "TestScheme")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
    });
}
```
Where [`TestAuthHandler`](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/test/TestAuthHandler.cs) inherits from [`AuthenticatonHandler`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationhandler-1?view=aspnetcore-8.0) and overrides the `HandleAuthenticateAsync` method.
```
protected override Task<AuthenticateResult> HandleAuthenticateAsync()
{
    Claim[] claims = 
    [
        new Claim(ClaimTypes.Name, "testuser"),
        new Claim(ClaimTypes.NameIdentifier, "testuser")
    ];
    ClaimsIdentity identity = new (claims, "Test");
    ClaimsPrincipal principal = new (identity);
    AuthenticationTicket ticket = new (principal, "TestScheme");

    AuthenticateResult result = AuthenticateResult.Success(ticket);

    return Task.FromResult(result);
}
```
An [authenticated client](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/test/CustomWebApplicationFactory.cs#L34) can be created for the test as follows
```
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
```
## Antiforgery
The [`ValidateAntiForgeryToken`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.validateantiforgerytokenattribute?view=aspnetcore-8.0) attribute is used on the two endpoints (#3 & #4) that require antiforgery token validation.

The test project adds an [`AntiforgeryController`](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/test/AntiforgeryTokenController.cs) to the `IWebHostBuilder`, which returns a JSON object containing valid antiforgery tokens.
```
public static IWebHostBuilder ConfigureAntiforgeryTokenResource(this IWebHostBuilder builder)
{
    ArgumentNullException.ThrowIfNull(builder);

    return builder.ConfigureTestServices((services) =>
    {
        services.AddControllers()
                .AddApplicationPart(typeof(AntiforgeryTokenController).Assembly);
    });
}
```
```
public IActionResult GetAntiforgeryTokens(
    [FromServices] IAntiforgery antiforgery,
    [FromServices] IOptions<AntiforgeryOptions> options)
{
    ArgumentNullException.ThrowIfNull(antiforgery);
    ArgumentNullException.ThrowIfNull(options);

    AntiforgeryTokenSet tokens = antiforgery.GetTokens(HttpContext);

    AntiforgeryTokens model = new()
    {
        CookieName = options.Value!.Cookie!.Name!,
        CookieValue = tokens.CookieToken!,
        FormFieldName = options.Value.FormFieldName,
        HeaderName = tokens.HeaderName!,
        RequestToken = tokens.RequestToken!
    };

    return Json(model);
}
```
The [`CustomWebApplicationFactory`](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/test/CustomWebApplicationFactory.cs#L64) provides a method, `GetAntiForgeryTokensAsync`, for pinging the `AntiforgeryTokenController` within a test method.
```
public async Task<AntiforgeryTokens> GetAntiforgeryTokensAsync(
    Func<HttpClient>? httpClientFactory = null,
    CancellationToken cancellationToken = default)
{
    using HttpClient httpClient = httpClientFactory?.Invoke() ?? CreateDefaultClient();

    AntiforgeryTokens? tokens = await httpClient.GetFromJsonAsync<AntiforgeryTokens>(
        AntiforgeryTokenController.GetTokensUri,
        cancellationToken);

    return tokens!;
}
```
## [Integration Tests](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/test/ApiControllerTests.cs)
I am able to successfully test the first three endpoints, however, when I need to test the fourth endpoint, requiring both an authenticated user and antiforgery token validation, a `Bad Request` is returned.

**1. Testing unauthenticated (anonymous) endpoint - antiforgery validation NOT required**

```
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
```

**2. Testing authenticated endpoint - antiforgery validation NOT required**

```
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
```

**3. Testing unauthenticated (anonymous) endpoint - antiforgery validation required**
```
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
```
**4. Testing authenticated endpoint - antiforgery validation required**

[I had read](https://dasith.me/2020/02/03/integration-test-aspnetcore-api-with-csrf/) the browser automatically extracts any cookie from the server response headers and attaches them to the next request. For the validation to succeed with CSRF, this needs to be simulated. Therefore, this test calls a [`GetAuthenticationCookies`](https://github.com/zwoolli/AntiforgeryInAPIController/blob/main/test/ApiControllerTests.cs#L145) method, which logs into the application and extracts the authentication cookies from the response.
```
public async Task<List<string>> GetAuthenticationCookies(CookieContainerHandler cookieHandler, AntiforgeryTokens tokens)
{
    CancellationToken cancellationToken = new CancellationTokenSource().Token;

    HttpClient client = _factory.CreateDefaultClient(cookieHandler);

    Uri uri = new($"{client.BaseAddress!.AbsoluteUri}login");

    Dictionary<string, string> postData = new()
    {
        { "Input.UserName", "testuser" },
        { "Input.Password", "password" },
        { tokens!.FormFieldName, tokens.RequestToken }
    };

    HttpContent formContent = new FormUrlEncodedContent(postData);

    HttpResponseMessage response = await client.PostAsync(uri, formContent, cancellationToken);

    return response.Headers.GetValues("Set-Cookie").ToList();
}
```
The test then adds those cookies to the clients request headers in addition to antiforgery tokens.

```
public async Task Authenticated_request_to_authenticated_antiforgery_endpoint_with_tokens_returns_ok()
{
    // Assert
    AntiforgeryTokens tokens = await _factory.GetAntiforgeryTokensAsync();

    CookieContainerHandler cookieHandler = new();
    cookieHandler.Container.Add(
        _factory.Server.BaseAddress,
        new Cookie(tokens.CookieName, tokens.CookieValue));

    HttpClient client = _factory.GetAuthenticatedClient(cookieHandler);

    List<string> cookies = await GetAuthenticationCookies(cookieHandler, tokens);

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
```
Unfortunately, this test returns a `Bad Request`, and never reaches the handler. 

# Question
How do you perform an integration test on an endpoint that requires both authentication and validation of antiforgery tokens?