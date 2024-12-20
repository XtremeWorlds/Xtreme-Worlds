Imports System.IO
Imports Microsoft.Extensions.Configuration
Imports Xtreme.Worlds.Engine.Configuration.Interfaces

Namespace Configuration
    Public Class EngineConfigurationBuilder
        Implements IEngineConfigurationBuilder

        Private _isDisposed As Boolean
        Private ReadOnly _properties As New Dictionary(Of String, Object)()
        Private ReadOnly _sources As New List(Of IConfigurationSource)()

        Public ReadOnly Property Properties As IDictionary(Of String, Object) Implements IConfigurationBuilder.Properties
            Get
                Return Me._properties
            End Get
        End Property

        Public ReadOnly Property Sources As IList(Of IConfigurationSource) Implements IConfigurationBuilder.Sources
            Get
                Return Me._sources
            End Get
        End Property

        Public Function Add(source As IConfigurationSource) As IConfigurationBuilder Implements IConfigurationBuilder.Add
            Me._sources.Add(source)
            Return Me
        End Function

        Public Function Build() As IConfigurationRoot Implements IConfigurationBuilder.Build
            Dim engineConfig As New EngineConfiguration()

            For Each source As IConfigurationSource In Me._sources
                Dim provider As IConfigurationProvider = source.Build(Me)

                Dim configuration As IConfiguration = New ConfigurationRoot(New List(Of IConfigurationProvider) From {provider})

                For Each kvp As KeyValuePair(Of String, String) In configuration.AsEnumerable()
                    engineConfig(kvp.Key) = kvp.Value
                Next
            Next

            Return engineConfig
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me._isDisposed Then
                If disposing Then
                    ' Dispose of managed resources here if needed.
                End If
                Me._isDisposed = True
            End If
        End Sub

        Private Sub LoadJsonFiles(filePaths As IEnumerable(Of String))
            For Each file As String In filePaths
                Dim unused = Me.AddJsonFile(file, optional:=True, reloadOnChange:=True)
            Next
        End Sub

        Public Sub LoadSettingsFiles() Implements IEngineConfigurationBuilder.LoadSettingsFiles
            Dim files As IEnumerable(Of String) = Directory.GetFiles(AppContext.BaseDirectory) _
                                                   .Where(Function(name) name.Contains("appsettings") AndAlso name.EndsWith(".json")) _
                                                   .Where(Function(name) Not name.Contains(".development") AndAlso Not name.Contains(".production"))

            Me.LoadJsonFiles(files)
        End Sub

        Public Sub LoadEnvironmentSettingsFiles() Implements IEngineConfigurationBuilder.LoadEnvironmentSettingsFiles
            Dim currentEnvironment As String = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant()

            If String.IsNullOrWhiteSpace(currentEnvironment) Then currentEnvironment = "production"

            Dim files As IEnumerable(Of String) = Directory.GetFiles(AppContext.BaseDirectory) _
                                                    .Where(Function(name) name.Contains("appsettings") AndAlso name.EndsWith($".{currentEnvironment}.json"))

            Me.LoadJsonFiles(files)
        End Sub

        Public Sub LoadEnvironmentVariables(Optional prefix As String = "XW") Implements IEngineConfigurationBuilder.LoadEnvironmentVariables
            Dim unused = Me.AddEnvironmentVariables(prefix)
        End Sub
    End Class
End Namespace
