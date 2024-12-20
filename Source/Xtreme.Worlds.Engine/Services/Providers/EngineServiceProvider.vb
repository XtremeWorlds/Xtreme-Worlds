Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.DependencyInjection
Imports Xtreme.Worlds.Engine.Configuration.Interfaces
Imports Xtreme.Worlds.Engine.Services.Providers.Interfaces

Namespace Services.Providers
    Public Class EngineServiceProvider
        Implements IEngineServiceProvider

        Public ReadOnly Property Services As ServiceCollection Implements IEngineServiceProvider.Services
        Private _serviceProvider As ServiceProvider
        Private _isDisposed As Boolean

        Public Sub New(ByRef configurationBuilder As IEngineConfigurationBuilder)
            Me.Services = New ServiceCollection()

            Dim configuration As IEngineConfiguration = CType(configurationBuilder.Build(), IEngineConfiguration)
            Dim unused1 = Me.Services.AddSingleton(Of IEngineConfiguration)(configuration)
            Dim unused2 = Me.Services.AddSingleton(Of IConfiguration)(configuration)
        End Sub

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

        Public ReadOnly Property ServiceProvider As ServiceProvider Implements IEngineServiceProvider.ServiceProvider
            Get
                If Me._serviceProvider Is Nothing Then
                    If Me.Services IsNot Nothing Then
                        Me._serviceProvider = Me.Services.BuildServiceProvider()
                    Else
                        Throw New NullReferenceException(NameOf(Me.Services))
                    End If
                End If

                Return Me._serviceProvider
            End Get
        End Property
    End Class

End Namespace