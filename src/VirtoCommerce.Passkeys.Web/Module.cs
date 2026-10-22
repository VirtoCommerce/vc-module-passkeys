using GraphQL.MicrosoftDI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Passkeys.Core;
using VirtoCommerce.Passkeys.Core.Models;
using VirtoCommerce.Passkeys.Core.Services;
using VirtoCommerce.Passkeys.Data.Services;
using VirtoCommerce.Passkeys.Xapi;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security.SignInMethods;
using VirtoCommerce.Platform.Security.OpenIddict;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.Passkeys.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        // Register services
        serviceCollection.AddOptions<PasskeyOptions>().Bind(Configuration.GetSection("Passkey"));
        serviceCollection.AddScoped<IPasskeyService, PasskeyService>();
        serviceCollection.AddScoped<ISignInMethodProvider, PasskeySignInMethodProvider>();
        serviceCollection.AddGrantTypeHandler<PasskeyGrantTypeHandler>(ModuleConstants.Security.GrantType);

        // Register GraphQL schema
        _ = new GraphQLBuilder(serviceCollection, builder =>
        {
            builder.AddSchema(serviceCollection, typeof(XapiAssemblyMarker));
        });

        serviceCollection.AddSingleton<ScopedSchemaFactory<XapiAssemblyMarker>>();
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        // Register partial GraphQL schema
        appBuilder.UseScopedSchema<XapiAssemblyMarker>("passkeys");
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
