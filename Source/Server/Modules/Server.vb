Imports System.Threading
Imports Core

Module Server
    Private consoleExit As Boolean
    Private threadConsole As Thread

    Sub Main()
        threadConsole = New Thread(New ThreadStart(AddressOf ConsoleThread))
        threadConsole.Start()

        AddHandler AppDomain.CurrentDomain.ProcessExit, AddressOf ProcessExitHandler

        ' Spin up the server on the main thread
        InitServer()
    End Sub

    Private Sub ProcessExitHandler(ByVal sender As Object, ByVal e As EventArgs)
        UpdateSavePlayers()
        consoleExit = 1
        threadConsole.Join()
    End Sub

    Private Sub ConsoleThread()
        Dim line As String, parts As String()

        Call Global.System.Console.WriteLine("Initializing Console Loop")

        While (Not consoleExit)
            Try
                line = Global.System.Console.ReadLine()
            Catch ex As Exception
                Exit While
            End Try

            parts = line.Split(" ") : If (parts.Length < 1) Then Continue While

            Select Case parts(0).ToLower()
                Case "/help"
#Region " Body "

                    Call Global.System.Console.WriteLine("/help, shows this message.")
                    Call Global.System.Console.WriteLine("/exit, closes down the server.")
                    Call Global.System.Console.WriteLine("/access, sets player access level, use with '/access name level goes from 1 for Player, to 5 to Owner.")
                    Call Global.System.Console.WriteLine("/kick, kicks user from server, use with '/kick name'")
                    Call Global.System.Console.WriteLine("/ban, bans user from server, use with '/ban name'")
                    Call Global.System.Console.WriteLine("/shutdown, shuts down the server after 60 seconds or a value you specify in seconds")

#End Region

                Case "/shutdown"
#Region "Body"
                    If parts.Length < 2 Then
                        shutDownDuration = 60
                    Else
                        shutDownDuration = parts(1)
                    End If

                    If shutDownTimer.IsRunning Then
                        shutDownTimer.Stop()
                        shutDownDuration = 0
                        Call Global.System.Console.WriteLine("Server shutdown has been cancelled!")
                        Call GlobalMsg("Server shutdown has been cancelled!")
                    Else
                        If shutDownTimer.ElapsedTicks > 0 Then
                            shutDownTimer.Restart()
                        Else
                            shutDownTimer.Start()
                        End If

                        Call Global.System.Console.WriteLine("Server shutdown in " & shutDownDuration & " seconds!")
                        Call GlobalMsg("Server shutdown in " & shutDownDuration & " seconds!")
                    End If
#End Region

                Case "/exit"

#Region " Body "

                    DestroyServer()

#End Region

                Case "/access"
#Region "Body"
                    If parts.Length < 3 Then Continue While

                    Dim Name As String = parts(1)
                    Dim Pindex As Integer = FindPlayer(Name)
                    Dim Access As Integer : Integer.TryParse(parts(2), Access)

                    If Not Pindex > 0 Then
                        Call Global.System.Console.WriteLine("Player name is empty or invalid. [Name not found]")
                    Else
                        Select Case Access
                            Case Core.AccessType.Player
                                SetPlayerAccess(Pindex, Access)
                                SendPlayerData(Pindex)
                                PlayerMsg(Pindex, "Your access has been set to Player!", ColorType.BrightCyan)
                                Call Global.System.Console.WriteLine("Successfully set the access level to " & Access & " for player " & Name)
                            Case AccessType.Moderator
                                SetPlayerAccess(Pindex, Access)
                                SendPlayerData(Pindex)
                                PlayerMsg(Pindex, "Your access has been set to Moderator!", ColorType.BrightCyan)
                                Call Global.System.Console.WriteLine("Successfully set the access level to " & Access & " for player " & Name)
                            Case AccessType.Mapper
                                SetPlayerAccess(Pindex, Access)
                                SendPlayerData(Pindex)
                                PlayerMsg(Pindex, "Your access has been set to Mapper!", ColorType.BrightCyan)
                                Call Global.System.Console.WriteLine("Successfully set the access level to " & Access & " for player " & Name)
                            Case AccessType.Developer
                                SetPlayerAccess(Pindex, Access)
                                SendPlayerData(Pindex)
                                PlayerMsg(Pindex, "Your access has been set to Developer!", ColorType.BrightCyan)
                                Call Global.System.Console.WriteLine("Successfully set the access level to " & Access & " for player " & Name)
                            Case AccessType.Owner
                                SetPlayerAccess(Pindex, Access)
                                SendPlayerData(Pindex)
                                PlayerMsg(Pindex, "Your access has been set to Owner!", ColorType.BrightCyan)
                                Call Global.System.Console.WriteLine("Successfully set the access level to " & Access & " for player " & Name)
                            Case Else
                                Call Global.System.Console.WriteLine("Failed to set the access level to " & Access & " for player " & Name)
                        End Select
                    End If

#End Region

                Case "/kick"
#Region "Body"
                    If parts.Length < 2 Then Continue While

                    Dim Name As String = parts(1)
                    Dim Pindex As Integer = FindPlayer(Name)
                    If Not Pindex > 0 Then
                        Call Global.System.Console.WriteLine("Player name is empty or invalid.")
                    Else
                        AlertMsg(Pindex, DialogueMsg.Kicked)
                        LeftGame(Pindex)
                    End If
#End Region

                Case "/ban"
#Region "Body"
                    If parts.Length < 2 Then Continue While

                    Dim Name As String = parts(1)
                    Dim Pindex As Integer = FindPlayer(Name)
                    If Not Pindex > 0 Then : Call Global.System.Console.WriteLine("Player name is empty or invalid. [Name not found]")
                    Else : ServerBanIndex(Pindex) : End If

#End Region

                Case "/timespeed"
#Region " Body "
                    If parts.Length < 2 Then Exit Sub

                    Dim speed As Double
                    Double.TryParse(parts(1), speed)
                    Core.Time.Instance.GameSpeed = speed
                    Type.Setting.TimeSpeed = speed
                    Settings.Save()
                    Call Global.System.Console.WriteLine("Set GameSpeed to " & Core.Time.Instance.GameSpeed & " secs per seconds")

#End Region

                Case "" : Continue While
                Case Else : Call Global.System.Console.WriteLine("Invalid Command. If you are unsure of the functions type '/help'.")
            End Select
        End While
    End Sub

End Module