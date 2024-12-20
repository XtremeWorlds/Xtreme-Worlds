Imports System.Threading
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.Primitives
Imports Xtreme.Worlds.Engine.Configuration.Interfaces

Namespace Configuration
    Public Class EngineConfiguration
        Implements IEngineConfiguration

        Private ReadOnly _configDictionary As New Dictionary(Of String, String)
        Private ReadOnly _configSections As New Dictionary(Of String, Dictionary(Of String, String))
        Private ReadOnly _providers As New List(Of IConfigurationProvider)()
        Private _reloadToken As IChangeToken

        Default Public Property Item(key As String) As String Implements IConfiguration.Item
            Get
                Return If(Me._configDictionary.ContainsKey(key), Me._configDictionary(key), String.Empty)
            End Get
            Set(value As String)
                Me._configDictionary(key) = value
            End Set
        End Property

        Public ReadOnly Property Providers As IEnumerable(Of IConfigurationProvider) Implements IConfigurationRoot.Providers
            Get
                Return Me._providers
            End Get
        End Property

        Public Sub Reload() Implements IConfigurationRoot.Reload
            For Each provider As IConfigurationProvider In Me._providers
                provider.Load()
            Next

            Me._reloadToken = New CancellationChangeToken(CancellationToken.None)
        End Sub

        Public Function GetSection(key As String) As IConfigurationSection Implements IConfiguration.GetSection
            Return If(Me._configSections.ContainsKey(key),
                New EngineConfigurationSection(key, key, Nothing, Me._configSections(key)),
                New EngineConfigurationSection(key, key, Nothing))
        End Function

        Public Function GetChildren() As IEnumerable(Of IConfigurationSection) Implements IConfiguration.GetChildren
            Return Me._configSections.Select(Function(pair) New EngineConfigurationSection(pair.Key, pair.Key, Nothing, pair.Value))
        End Function

        Public Function GetReloadToken() As IChangeToken Implements IConfiguration.GetReloadToken
            Return Me._reloadToken
        End Function

        Public Function GetValue(Of ValueType)(key As String, defaultValue As ValueType) As ValueType Implements IEngineConfiguration.GetValue
            Dim value As String = Me(key)

            If Not String.IsNullOrWhiteSpace(value) Then
                Try
                    Return CType(Convert.ChangeType(value, GetType(ValueType)), ValueType)
                Catch ex As InvalidCastException
                    Return defaultValue
                End Try
            Else
                Return defaultValue
            End If
        End Function
    End Class
End Namespace
