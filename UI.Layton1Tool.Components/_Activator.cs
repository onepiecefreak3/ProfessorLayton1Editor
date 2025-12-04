using CrossCutting.Core.Contract.Bootstrapping;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.DependencyInjection.DataClasses;
using CrossCutting.Core.Contract.EventBrokerage;
using UI.Layton1Tool.Components.Contract;

namespace UI.Layton1Tool.Components;

public class Layton1ToolComponentsActivator : IComponentActivator
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
        kernel.Register<IAnimationStateManager, AnimationStateManager>(ActivationScope.Unique);

        kernel.Register<IComponentFactory, ComponentFactory>(ActivationScope.Unique);

        kernel.RegisterToSelf<AnimationPlayer>();
        kernel.RegisterToSelf<AnimationViewer>();
        kernel.RegisterToSelf<GlyphElement>();
        kernel.RegisterToSelf<ZoomableCharacterInfo>();
        kernel.RegisterToSelf<ZoomablePaddedGlyph>();

        kernel.RegisterConfiguration<Layton1ToolComponentsConfiguration>();
    }

    public void AddMessageSubscriptions(IEventBroker broker)
    {
    }

    public void Configure(IConfigurator configurator)
    {
    }
}