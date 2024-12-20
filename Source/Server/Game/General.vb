Imports System.IO
Imports Core
Imports Core.Database
Imports Newtonsoft.Json.Linq
Imports Xtreme.Worlds.Engine.Configuration
Imports Xtreme.Worlds.Engine.Configuration.Interfaces

Module General
    Public Random As New Random()

    Public Configuration As IEngineConfiguration

    Friend ServerDestroyed As Boolean
    Friend MyIPAddress As String
    Friend myStopWatch As New Stopwatch()
    Friend shutDownTimer As New Stopwatch()
    Friend shutDownLastTimer As Integer
    Friend shutDownDuration As Integer

    Friend Function GetTimeMs() As Integer
        Return myStopWatch.ElapsedMilliseconds
    End Function

    Sub InitServer()
        Dim i As Integer, F As Integer, x As Integer
        Dim time1 As Integer, time2 As Integer

        myStopWatch.Start()

        Configuration = BuildEngineConfiguration()

        Settings.Load()

        Core.Time.Instance.GameSpeed = Settings.TimeSpeed

        Global.System.Console.Title = "XtremeWorlds Server"

        time1 = GetTimeMs()

        EKeyPair.GenerateKeys()
        InitNetwork()

        Call Global.System.Console.WriteLine("Creating Database...")
        CreateDatabase("mirage")

        Call Global.System.Console.WriteLine("Creating Tables...")
        CreateTables()

        Call Global.System.Console.WriteLine("Loading Character List...")

        Dim ids As Task(Of List(Of Int64)) = GetData("account")
        Dim data As JObject
        Dim player As New PlayerStruct()
        Core.Char = New CharList

        For Each id In ids.Result
            For i = 1 To MAX_CHARS
                data = SelectRowByColumn("id", id, "account", "character" & i.ToString())
                If data IsNot Nothing Then
                    player = JObject.FromObject(data).ToObject(Of PlayerStruct)()
                    Core.Char.Add(player.Name)
                End If
            Next
        Next

        ClearGameData()
        LoadGameData()

        Call Global.System.Console.WriteLine("Spawning Map Items...")
        SpawnAllMapsItems()
        Call Global.System.Console.WriteLine("Spawning Map NPCs...")
        SpawnAllMapNPCs()

        InitTime()

        UpdateCaption()
        time2 = GetTimeMs()

        Call Global.System.Console.Clear()
        Call Global.System.Console.WriteLine(" __   ___                        __          __        _     _     ")
        Call Global.System.Console.WriteLine(" \ \ / / |                       \ \        / /       | |   | |")
        Call Global.System.Console.WriteLine("  \ V /| |_ _ __ ___ _ __ ___   __\ \  /\  / /__  _ __| | __| |___ ")
        Call Global.System.Console.WriteLine("  > < | __| '__/ _ \ '_ ` _ \ / _ \ \/  \/ / _ \| '__| |/ _` / __|")
        Call Global.System.Console.WriteLine(" / . \| |_| | |  __/ | | | | |  __/\  /\  / (_) | |  | | (_| \__ \")
        Call Global.System.Console.WriteLine("/_/ \_\\__|_|  \___|_| |_| |_|\___| \/  \/ \___/|_|  |_|\__,_|___/")

        Call Global.System.Console.WriteLine("Initialization complete. Server loaded in " & time2 - time1 & "ms.")
        Call Global.System.Console.WriteLine("")
        Call Global.System.Console.WriteLine("Use /help for the available commands.")

        UpdateCaption()

        ' Start listener now that everything is loaded
        Socket.StartListening(Settings.Port, 5)

        ' Starts the server loop
        ServerLoop()

    End Sub

    Private Function BuildEngineConfiguration() As IEngineConfiguration
        Dim builder As IEngineConfigurationBuilder = New EngineConfigurationBuilder()

        builder.LoadSettingsFiles()
        builder.LoadEnvironmentSettingsFiles()
        builder.LoadEnvironmentVariables()

        Return CType(builder.Build(), IEngineConfiguration)
    End Function

    Private Function ConsoleEventCallback(eventType As Integer) As Boolean
        If eventType = 2 Then
            Call Global.System.Console.WriteLine("Console window closing, death imminent")
            'cleanup and close
            DestroyServer()
        End If
    End Function

    Private handler As ConsoleEventDelegate

    ' Keeps it from getting garbage collected
    ' Pinvoke
    Private Delegate Function ConsoleEventDelegate(eventType As Integer) As Boolean

    Function CountPlayersOnline() As Integer
        Dim count = 0
        For i = 1 To Socket.HighIndex()
            If Not IsPlaying(i) Then Continue For
            count += 1
        Next
        Return count
    End Function

    Sub UpdateCaption()
        Try
            Global.System.Console.Title = $"{Settings.GameName} <IP {MyIPAddress}:{Settings.Port}> ({CountPlayersOnline()} Players Online) - Current Errors: {ErrorCount} - Time: {Core.Time.Instance.ToString()}"
        Catch ex As Exception
            Global.System.Console.Title = $"{Settings.GameName}"
            Exit Sub
        End Try
    End Sub

    Sub DestroyServer()
        Socket.StopListening()

        Call Global.System.Console.WriteLine("Saving players online...")
        SaveAllPlayersOnline()

        Call Global.System.Console.WriteLine("Unloading players...")
        For i As Integer = 1 To MAX_PLAYERS
            SendLeftGame(i)
            LeftGame(i)
        Next

        DestroyNetwork()
        ClearGameData()

        Environment.Exit(0)
    End Sub

    Friend Sub ClearGameData()
        Dim i As Integer

        ' Init all the player sockets
        Call Global.System.Console.WriteLine("Clearing Players...")

        For i = 1 To MAX_PLAYERS
            ClearAccount(i)
            ClearPlayer(i)
        Next

        ClearParty()

        Call Global.System.Console.WriteLine("Clearing Jobs...") : ClearJobs()
        Call Global.System.Console.WriteLine("Clearing Morals...") : ClearMorals()
        Call Global.System.Console.WriteLine("Clearing Maps...") : ClearMaps()
        Call Global.System.Console.WriteLine("Clearing Map Items...") : ClearMapItems()
        Call Global.System.Console.WriteLine("Clearing Map NPC's...") : ClearAllMapNPCs()
        Call Global.System.Console.WriteLine("Clearing NPC's...") : ClearNPCs()
        Call Global.System.Console.WriteLine("Clearing Resources...") : ClearResources()
        Call Global.System.Console.WriteLine("Clearing Items...") : ClearItems()
        Call Global.System.Console.WriteLine("Clearing Shops...") : ClearShops()
        Call Global.System.Console.WriteLine("Clearing Skills...") : ClearSkills()
        Call Global.System.Console.WriteLine("Clearing Animations...") : ClearAnimations()
        Call Global.System.Console.WriteLine("Clearing Map Projectiles...") : ClearMapProjectile()
        Call Global.System.Console.WriteLine("Clearing Projectiles...") : ClearProjectile()
        Call Global.System.Console.WriteLine("Clearing Pets...") : ClearPets()
    End Sub

    Private Sub LoadGameData()
        Call Global.System.Console.WriteLine("Loading Jobs...") : LoadJobs()
        Call Global.System.Console.WriteLine("Loading Morals...") : LoadMorals()
        Call Global.System.Console.WriteLine("Loading Maps...") : LoadMaps()
        Call Global.System.Console.WriteLine("Loading Items...") : LoadItems()
        Call Global.System.Console.WriteLine("Loading NPCs...") : LoadNPCs()
        Call Global.System.Console.WriteLine("Loading Resources...") : LoadResources()
        Call Global.System.Console.WriteLine("Loading Shops...") : LoadShops()
        Call Global.System.Console.WriteLine("Loading Skills...") : LoadSkills()
        Call Global.System.Console.WriteLine("Loading Animations...") : LoadAnimations()
        Call Global.System.Console.WriteLine("Loading Switches...") : LoadSwitches()
        Call Global.System.Console.WriteLine("Loading Variables...") : LoadVariables()
        Call Global.System.Console.WriteLine("Spawning Global Events...") : SpawnAllMapGlobalEvents()
        Call Global.System.Console.WriteLine("Loading Projectiles...") : LoadProjectiles()
        Call Global.System.Console.WriteLine("Loading Pets...") : LoadPets()
    End Sub

    ' Used for checking validity of names
    Function IsNameLegal(sInput As String) As Boolean
        For Each ch As Char In sInput
            Dim asciiValue As Integer = AscW(ch)
            ' Check if character is a letter (A-Z, a-z), a digit (0-9), an underscore (_), or a space ( )
            If Not ((asciiValue >= 65 And asciiValue <= 90) Or
                (asciiValue >= 97 And asciiValue <= 122) Or
                (asciiValue = 95) Or
                (asciiValue = 32) Or
                (asciiValue >= 48 And asciiValue <= 57)) Then
                Return False
            End If
        Next
        Return True
    End Function


    Friend Sub CheckDir(path As String)
        If Not Directory.Exists(path) Then Directory.CreateDirectory(path)
    End Sub

    Sub ErrorHandler(sender As Object, args As UnhandledExceptionEventArgs)
        Dim e As Exception = DirectCast(args.ExceptionObject, Exception)
        Dim myFilePath As String = IO.Path.Combine(Core.Path.Logs, "Errors.log")

        Using sw As New StreamWriter(File.Open(myFilePath, FileMode.Append))
            sw.WriteLine(DateTime.Now)
            sw.WriteLine(GetExceptionInfo(e))
        End Using

        ErrorCount = ErrorCount + 1

        UpdateCaption()
    End Sub

    Friend Function GetExceptionInfo(ex As Exception) As String
        Dim Result As String
        Dim hr As Integer = Runtime.InteropServices.Marshal.GetHRForException(ex)
        Result = ex.GetType.ToString & "(0x" & hr.ToString("X8") & "): " & ex.Message & Environment.NewLine & ex.StackTrace & Environment.NewLine
        Dim st As New StackTrace(ex, True)
        For Each sf As StackFrame In st.GetFrames
            If sf.GetFileLineNumber() > 0 Then
                Result &= "Line:" & sf.GetFileLineNumber() & " Filename: " & IO.Path.GetFileName(sf.GetFileName) & Environment.NewLine
            End If
        Next
        Return Result
    End Function

    Friend Sub AddDebug(Msg As String)
        If DebugTxt = 1 Then
            Addlog(Msg, PACKET_LOG)
            Call Global.System.Console.WriteLine(Msg)
        End If
    End Sub

    Friend Sub CheckShutDownCountDown()
        If shutDownDuration > 0 Then
            Dim time As Integer = shutDownTimer.Elapsed.Seconds

            If shutDownLastTimer <> time Then
                If shutDownDuration - time <= 10 Then
                    NetworkSend.GlobalMsg("Server shutdown in " & (shutDownDuration - time) & " seconds!")
                    Call Global.System.Console.WriteLine("Server shutdown in " & (shutDownDuration - time) & " seconds!")

                    If shutDownDuration - time <= 1 Then
                        DestroyServer()
                    End If
                End If

                shutDownLastTimer = time
            End If
        End If
    End Sub

End Module