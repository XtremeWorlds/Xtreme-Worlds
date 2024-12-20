Imports System.Threading
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.Primitives
Imports Xtreme.Worlds.Engine.Configuration.Interfaces

Namespace Configuration
    Public Class EngineConfigurationSection
        Implements IEngineConfigurationSection

        Private ReadOnly _children As New Dictionary(Of String, String)
        Private _reloadToken As IChangeToken
        Private _cancellationTokenSource As CancellationTokenSource

        Public Sub New(key As String, path As String, value As String, Optional children As Dictionary(Of String, String) = Nothing)
            Me.Key = key
            Me.Path = path
            Me.Value = value

            If children IsNot Nothing Then
                Me._children = children
            End If

            Me._cancellationTokenSource = New CancellationTokenSource()
            Me._reloadToken = New CancellationChangeToken(Me._cancellationTokenSource.Token)
        End Sub

        Public ReadOnly Property Key As String Implements IConfigurationSection.Key

        Public ReadOnly Property Path As String Implements IConfigurationSection.Path

        Public Property Value As String Implements IConfigurationSection.Value

        Default Public Property Item(key As String) As String Implements IConfiguration.Item
            Get
                Dim value As String = Nothing
                Return If(Me._children.TryGetValue(key, value), value, Nothing)
            End Get
            Set(value As String)
                Me._children(key) = value
                Me.TriggerReload()
            End Set
        End Property

        Public Function GetSection(key As String) As IConfigurationSection Implements IConfigurationSection.GetSection
            Dim value As String = Nothing
            Return If(Me._children.TryGetValue(key, value),
                      New EngineConfigurationSection(key, Me.Path & "." & key, value),
                      New EngineConfigurationSection(key, Me.Path & "." & key, Nothing))
        End Function

        Public Function GetChildren() As IEnumerable(Of IConfigurationSection) Implements IConfigurationSection.GetChildren
            Return Me._children.Select(Function(pair) New EngineConfigurationSection(pair.Key, Me.Path & "." & pair.Key, pair.Value))
        End Function

        Public Function GetReloadToken() As IChangeToken Implements IConfigurationSection.GetReloadToken
            Return Me._reloadToken
        End Function

        Private Sub TriggerReload()
            If Me._cancellationTokenSource IsNot Nothing Then
                Me._cancellationTokenSource.Cancel()

                Me._cancellationTokenSource = New CancellationTokenSource()
                Me._reloadToken = New CancellationChangeToken(Me._cancellationTokenSource.Token)
            End If
        End Sub
    End Class
End Namespace
