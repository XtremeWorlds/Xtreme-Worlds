Imports Microsoft.Extensions.Configuration

Namespace Configuration.Interfaces
    Public Interface IEngineConfiguration
        Inherits IConfigurationRoot, IConfiguration

        Function GetValue(Of ValueType)(key As String, defaultValue As ValueType) As ValueType
    End Interface
End Namespace
