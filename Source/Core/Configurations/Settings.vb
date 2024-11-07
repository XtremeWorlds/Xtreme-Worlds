Imports System.IO
Imports System.Xml.Serialization

Public Class Settings
    Public Shared Language As String = "English"

    Public Shared Username As String = ""
    Public Shared SaveUsername As Boolean = True

    Public Shared MenuMusic As String = "menu.mid"
    Public Shared Music As Boolean = True
    Public Shared Sound As Boolean = True
    Public Shared MusicVolume As Single = 100.0F
    Public Shared SoundVolume As Single = 100.0F

    Public Shared MusicExt As String = ".mid"
    Public Shared SoundExt As String = ".ogg"

    Public Shared Resolution As Byte = 13
    Public Shared Vsync As Boolean = True
    Public Shared ShowNpcBar As Boolean = True
    Public Shared Fullscreen As Boolean = False
    Public Shared CameraWidth As Byte = 32
    Public Shared CameraHeight As Byte = 24
    Public Shared OpenAdminPanelOnLogin As Boolean = True
    Public Shared DynamicLightRendering As Boolean = True
    Public Shared ChannelState(ChatChannel.Count - 1) As Byte

    Public Shared Ip As String = "127.0.0.1"
    Public Shared Port As Integer = 7001

    <XmlIgnore()> Public Shared GameName As String = "XtremeWorlds"
    <XmlIgnore()> Public Shared Website As String = "https://xtremeworlds.com/"

    Public Shared Welcome As String = "Welcome to XtremeWorlds, enjoy your stay!"

    Public Shared TimeSpeed As Boolean = True

    Public Shared Autotile As Boolean = True

     Public Shared Sub Load()
        Dim configPath As String = Core.Path.Config()
        Dim configFile As String = IO.Path.Combine(configPath, "Settings.xml")

        Directory.CreateDirectory(configPath)

        If Not File.Exists(configFile) Then
            Try
                Using writer As New StreamWriter(File.Create(configFile))
                    Dim serializer As New XmlSerializer(GetType(Settings), New XmlRootAttribute("Settings"))
                    serializer.Serialize(writer, New Settings()) ' Serialize default settings
                End Using
            Catch ex As IOException
                Console.WriteLine(ex.Message)
            Catch ex As UnauthorizedAccessException
                Console.WriteLine(ex.Message)
            End Try
        End If

        Try
            Using reader As New StreamReader(configFile)
                Dim serializer As New XmlSerializer(GetType(Settings), New XmlRootAttribute("Settings"))
                Type.Setting = CType(serializer.Deserialize(reader), Settings)
            End Using
        Catch ex As Exception
            Type.Setting = New Settings() ' Default to new settings if reading fails
        End Try
    End Sub

    Public Shared Sub Save()
        Dim configPath As String = Core.Path.Config()
        Dim configFile As String = IO.Path.Combine(configPath, "Settings.xml")

        Directory.CreateDirectory(configPath)

        Try
            Using writer = New StreamWriter(configFile)
                Dim serializer As New XmlSerializer(GetType(Settings), New XmlRootAttribute("Settings"))
                serializer.Serialize(writer, Type.Setting)
            End Using
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub
End Class
