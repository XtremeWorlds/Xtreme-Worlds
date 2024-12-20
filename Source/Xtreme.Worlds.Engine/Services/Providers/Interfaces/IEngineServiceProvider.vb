Imports Microsoft.Extensions.DependencyInjection

Namespace Services.Providers.Interfaces
    Public Interface IEngineServiceProvider
        Inherits IDisposable

        ReadOnly Property ServiceProvider As ServiceProvider
        ReadOnly Property Services As ServiceCollection
    End Interface
End Namespace
