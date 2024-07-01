
using System.ComponentModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;

namespace Tests;

/// <summary>
/// A class containing extension methods for <see cref="IWebHostBuilder"/ interface. This class cannot be inherited>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class IWebHostBuilderExtensions
{
    public static IWebHostBuilder ConfigureTestAuthenticationScheme(this IWebHostBuilder builder, string scheme)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(defaultScheme: scheme)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(scheme, options => { });
        });
    }

    /// <summary>
    /// Configures an HTTP GET resource for obtaining valid antiforgery tokens.
    /// </summary>
    /// <param name="builder">The <see cref="IWebHostBuilder"/> to configure.</param>
    /// <returns>
    /// The <see cref="IWebHostBuilder"/> specified by <paramref name="builder"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>
    /// </exception>
    public static IWebHostBuilder ConfigureAntiforgeryTokenResource(this IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ConfigureTestServices((services) =>
        {
            services.AddControllers()
                    .AddApplicationPart(typeof(AntiforgeryTokenController).Assembly);
        });
    }
}