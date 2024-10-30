Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.Extensions.Configuration

Public Class MirageConfiguration
    Implements IDisposable

    Private isDisposed As Boolean
    Private ReadOnly configuration As IConfigurationRoot

    Public Sub New(Optional envPrefix As String = "MIRAGE")
        Dim currentEnvironment As String = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")

        Dim files As IEnumerable(Of String) = Directory.GetFiles(AppContext.BaseDirectory) _
            .Where(Function(name) name.Contains("appsettings")) _
            .Where(Function(name) Not name.Contains(".development")) _
            .Where(Function(name) Not name.Contains(".production")) _
            .Where(Function(name) name.EndsWith(".json"))

        Dim envfiles As IEnumerable(Of String) = Directory.GetFiles(AppContext.BaseDirectory) _
            .Where(Function(name) name.Contains("appsettings")) _
            .Where(Function(name) name.EndsWith($".{currentEnvironment}.json"))

        Dim builder As IConfigurationBuilder = New ConfigurationBuilder()

        For Each file As String In files
            Console.WriteLine($"Reading configuration file '{file}'...")
            builder = builder.AddJsonFile(file, optional:=True, reloadOnChange:=True)
        Next

        For Each file As String In envfiles
            Console.WriteLine($"Reading configuration file '{file}'...")
            builder = builder.AddJsonFile(file, optional:=True, reloadOnChange:=True)
        Next

        Console.WriteLine($"Reading configuration environment variables with prefix '{envPrefix}'...")
        builder = builder.AddEnvironmentVariables(envPrefix)

        Me.configuration = builder.Build()
    End Sub


    Protected Overrides Sub Finalize()
        Me.Dispose(disposing:=False)
        MyBase.Finalize()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.isDisposed Then
            If disposing Then

            End If

            Me.isDisposed = 1
        End If
    End Sub

    Public Function GetValue(Of ValueType)(key As String, defaultValue As ValueType) As ValueType
        Dim value As String = Me.configuration(key)

        If Not String.IsNullOrWhiteSpace(value) Then
            Try
                Return CType(Convert.ChangeType(value, GetType(ValueType)), ValueType)
            Catch ex As InvalidCastException
                Console.WriteLine($"[Error] Unable to read configuration value '{key}' as type of '{NameOf(ValueType)}'.")
                Return defaultValue
            End Try
        Else
            Console.WriteLine($"[Error] Unable to read configuration value '{key}' as it does not exist'.")
            Return defaultValue
        End If
    End Function

End Class
