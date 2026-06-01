using CommunicationHub.Backend.Core.AI.Plugins;
using CommunicationHub.Backend.Core.BC;
using CommunicationHub.Backend.Core.Models;
using CommunicationHub.Backend.Core.Search;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace CommunicationHub.Backend.Core.AI;

/// <summary>
/// Creates request-scoped <see cref="Kernel"/> instances with the correct plugins
/// and AOAI connector for each incoming request.
/// A factory is used (rather than a singleton Kernel) so that request-scoped
/// dependencies like <see cref="TenantContext"/> can be injected into plugins.
/// </summary>
public interface IKernelFactory
{
    Kernel CreateKernel(TenantContext ctx, ISearchClient searchClient, IBcApiClient bcClient);
}

public sealed class KernelFactory(
    IOptions<CopilotOptions> options,
    Azure.Core.TokenCredential credential) : IKernelFactory
{
    public Kernel CreateKernel(TenantContext ctx, ISearchClient searchClient, IBcApiClient bcClient)
    {
        var opts = options.Value;

        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: opts.ChatDeployment,
                endpoint: opts.AoaiEndpoint,
                credentials: credential);

        var kernel = builder.Build();

        // Register request-scoped plugins
        kernel.Plugins.AddFromObject(new BcPlugin(bcClient, ctx), "BcPlugin");
        kernel.Plugins.AddFromObject(new SearchPlugin(searchClient, ctx), "SearchPlugin");

        return kernel;
    }
}

/// <summary>Configuration for <see cref="CopilotOrchestrator"/> and <see cref="KernelFactory"/>.</summary>
public sealed class CopilotOptions
{
    public const string Section = "Copilot";
    public required string AoaiEndpoint { get; init; }
    public string ChatDeployment { get; init; } = "gpt-4.1-eu";
    public string EmbeddingDeployment { get; init; } = "text-embedding-3-large-eu";
    public string ApiVersion { get; init; } = "2024-12-01-preview";
}
