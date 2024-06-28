using Microsoft.AspNetCore.Mvc.Testing;

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
}