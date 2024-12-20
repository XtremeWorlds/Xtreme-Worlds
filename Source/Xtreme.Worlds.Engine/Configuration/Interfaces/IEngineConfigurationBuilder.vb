Imports Microsoft.Extensions.Configuration

Namespace Configuration.Interfaces
    Public Interface IEngineConfigurationBuilder
        Inherits IConfigurationBuilder, IDisposable

        Sub LoadEnvironmentSettingsFiles()
        Sub LoadEnvironmentVariables(Optional prefix As String = "XW")
        Sub LoadSettingsFiles()
    End Interface
End Namespace