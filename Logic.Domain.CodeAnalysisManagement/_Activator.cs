using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using Logic.Domain.CodeAnalysisManagement.Contract;
using Logic.Domain.CodeAnalysisManagement.Contract.Level5;
using Logic.Domain.CodeAnalysisManagement.Layton1;
using Logic.Domain.CodeAnalysisManagement.Layton1.InternalContract.DataClasses;

namespace Logic.Domain.CodeAnalysisManagement;

public class CodeAnalysisManagementActivator : IComponentActivator
{
    public void Activating()
    {
    }

    public void Activated()
    {
    }

    public void Deactivating()
    {
    }

    public void Deactivated()
    {
    }

    public void Register(ICoCoKernel kernel)
    {
        kernel.Register<ITokenFactory<Layton1SyntaxToken>, Layton1ScriptFactory>(ActivationScope.Unique);
        kernel.Register<ILexer<Layton1SyntaxToken>, Layton1ScriptLexer>();
        kernel.Register<IBuffer<Layton1SyntaxToken>, TokenBuffer<Layton1SyntaxToken>>();
        kernel.Register<IBuffer<int>, StringBuffer>();

        kernel.Register<ILayton1ScriptParser, Layton1ScriptParser>(ActivationScope.Unique);
        kernel.Register<ILayton1ScriptComposer, Layton1ScriptComposer>(ActivationScope.Unique);
        kernel.Register<ILayton1ScriptWhitespaceNormalizer, Layton1ScriptWhitespaceNormalizer>(ActivationScope.Unique);

        kernel.Register<ILayton1SyntaxFactory, Layton1SyntaxFactory>();

        kernel.RegisterConfiguration<CodeAnalysisManagementConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}