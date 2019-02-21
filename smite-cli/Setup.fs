namespace TIKSN.smite.cli

open TIKSN.Configuration
open TIKSN.DependencyInjection
open Autofac
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

type ConfigurationRootSetup() =
    inherit ConfigurationRootSetupBase()

type CompositionRootSetup(configurationRoot) =
    inherit AutofacPlatformCompositionRootSetupBase(configurationRoot)
    override this.ConfigureContainerBuilder(builder: ContainerBuilder) =
        ()
    override this.ConfigureOptions(services: IServiceCollection, configuration: IConfigurationRoot) =
        ()