Imports Client.GameClient
Imports Core
Imports System.IO

Module General
    Public Client As New GameClient()
    Public Random As New Random()

    Friend Function GetTickCount() As Integer
        Return Environment.TickCount
    End Function

    Sub Startup()
        InMenu = True
        ClearGameData()
        LoadGame()
        Client.LoadingCompleted.WaitOne()
        GameLoop()
    End Sub

    Friend Sub LoadGame()
        Started = True
        Settings.Load()
        Languages.Load()
        Input.Load()
        CheckAnimations()
        CheckCharacters()
        CheckEmotes()
        CheckTilesets()
        CheckFogs()
        CheckItems()
        CheckPanoramas()
        CheckPaperdolls()
        CheckParallax()
        CheckPictures()
        CheckProjectile()
        CheckResources()
        CheckSkills()
        ChecKInterface()
        CheckGradients()
        CheckDesigns()
        InitializeBASS()
        InitNetwork()
        InitInterface()
        Ping = -1
    End Sub

    Friend Sub CheckAnimations()
        NumAnimations = GetFileCount(Core.Path.Animations)
    End Sub

    Friend Sub CheckCharacters()
        NumCharacters = GetFileCount(Core.Path.Characters)
    End Sub

    Friend Sub CheckEmotes()
        NumEmotes = GetFileCount(Core.Path.Emotes)
    End Sub

    Friend Sub CheckTilesets()
        NumTileSets = GetFileCount(Core.Path.Tilesets)
    End Sub

    Friend Sub CheckFogs()
        NumFogs = GetFileCount(Core.Path.Fogs)
    End Sub

    Friend Sub CheckItems()
        NumItems = GetFileCount(Core.Path.Items)
    End Sub

    Friend Sub CheckPanoramas()
        NumPanoramas = GetFileCount(Core.Path.Panoramas)
    End Sub

    Friend Sub CheckPaperdolls()
        NumPaperdolls = GetFileCount(Core.Path.Paperdolls)
    End Sub

    Friend Sub CheckParallax()
        NumParallax = GetFileCount(Core.Path.Parallax)
    End Sub

    Friend Sub CheckPictures()
        NumPictures = GetFileCount(Core.Path.Pictures)
    End Sub

    Friend Sub CheckProjectile()
        NumProjectiles = GetFileCount(Core.Path.Projectiles)
    End Sub

    Friend Sub CheckResources()
        NumResources = GetFileCount(Core.Path.Resources)
    End Sub

    Friend Sub CheckSkills()
        NumSkills = GetFileCount(Core.Path.Skills)
    End Sub

    Friend Sub CheckInterface()
        NumInterface = GetFileCount(Core.Path.Gui)
    End Sub

    Friend Sub CheckGradients()
        NumGradients = GetFileCount(Core.Path.Gradients)
    End Sub

    Friend Sub CheckDesigns()
        NumDesigns = GetFileCount(Core.Path.Designs)
    End Sub


    Function GetResolutionSize(Resolution As Byte, ByRef Width As Integer, ByRef Height As Integer)
        Select Case Resolution
            Case 1
                Width = 1920
                Height = 1080
            Case 2
                Width = 1680
                Height = 1050
            Case 3
                Width = 1600
                Height = 900
            Case 4
                Width = 1440
                Height = 900
            Case 5
                Width = 1440
                Height = 1050
            Case 6
                Width = 1366
                Height = 768
            Case 7
                Width = 1360
                Height = 1024
            Case 8
                Width = 1360
                Height = 768
            Case 9
                Width = 1280
                Height = 1024
            Case 10
                Width = 1280
                Height = 800
            Case 11
                Width = 1280
                Height = 768
            Case 12
                Width = 1280
                Height = 720
            Case 13
                Width = 1120
                Height = 864
            Case 14      
                Width = 1024
                Height = 768
        End Select
    End Function

    Friend Sub ClearGameData()
        ClearMap()
        ClearMapNPCs()
        ClearMapItems()
        ClearNpcs()
        ClearResources()
        ClearItems()
        ClearShops()
        ClearSkills()
        ClearAnimations()
        ClearProjectile()
        ClearPets()
        ClearJobs()
        ClearMorals()
        ClearBanks()
        ClearParty()

        For i = 1 To MAX_PLAYERS
            ClearPlayer(i)
        Next

        ClearAnimInstances()
        ClearAutotiles()

        ' clear chat
        For i = 1 To CHAT_LINES
            Chat(i).text = ""
        Next
    End Sub

    Friend Function GetFileCount(folderName As String) As Integer
        Dim folderPath As String = IO.Path.Combine(Core.Path.Graphics, folderName)
        If Directory.Exists(folderPath) Then
            Return Directory.GetFiles(folderPath, "*.png").Length ' Adjust for other formats if needed
        Else
            Console.WriteLine($"Folder not found: {folderPath}")
            Return 0
        End If
    End Function

    Friend Sub CacheMusic()
        ReDim MusicCache(Directory.GetFiles(Core.Path.Music, "*" & Type.Setting.MusicExt).Count)
        Dim files As String() = Directory.GetFiles(Core.Path.Music, "*" & Type.Setting.MusicExt)
        Dim maxNum As String = Directory.GetFiles(Core.Path.Music, "*" & Type.Setting.MusicExt).Count
        Dim counter As Integer = 0

        For Each FileName In files
            counter = counter + 1
            ReDim Preserve MusicCache(counter)

            MusicCache(counter) = IO.Path.GetFileName(FileName)
        Next
    End Sub

    Friend Sub CacheSound()
        ReDim SoundCache(Directory.GetFiles(Core.Path.Sounds, "*" & Type.Setting.SoundExt).Count)
        Dim files As String() = Directory.GetFiles(Core.Path.Sounds, "*" & Type.Setting.SoundExt)
        Dim maxNum As String = Directory.GetFiles(Core.Path.Sounds,  "*" & Type.Setting.SoundExt).Count
        Dim counter As Integer = 0

        For Each FileName In files
            counter = counter + 1
            ReDim Preserve SoundCache(counter)

            SoundCache(counter) = IO.Path.GetFileName(FileName)
        Next
    End Sub

    Sub GameInit()
        ' Send a request to the server to open the admin menu if the user wants it.
        If Type.Setting.OpenAdminPanelOnLogin = 1 Then
            If GetPlayerAccess(MyIndex) > 0 Then
                SendRequestAdmin()
            End If
        End If
    End Sub

    Friend Function ConnectToServer(i As Integer) As Boolean
        Dim until As Integer
        ConnectToServer = False

        ' Check to see if we are already connected, if so just exit
        If Socket.IsConnected() Then
            ConnectToServer = True
            Exit Function
        End If

        If i = 4 Then Exit Function
        until = GetTickCount() + 3500

        Connect()

        ' Wait until connected or a few seconds have passed and report the server being down
        Do While (Not Socket.IsConnected()) And (GetTickCount() <= until)

        Loop

        ' return value
        If Socket.IsConnected() Then
            ConnectToServer = True
        End If

        If Not ConnectToServer Then
            ConnectToServer(i + 1)
        End If

    End Function

    Friend Sub DestroyGame()
        ' break out of GameLoop
        InGame = False
        FreeBASS
        End
    End Sub

    Friend Function GetExceptionInfo(ex As Exception) As String
        Dim result As String
        Dim hr As Integer = Runtime.InteropServices.Marshal.GetHRForException(ex)
        result = ex.GetType.ToString & "(0x" & hr.ToString("X8") & "): " & ex.Message & Environment.NewLine & ex.StackTrace & Environment.NewLine
        Dim st As StackTrace = New StackTrace(ex, True)
        For Each sf As StackFrame In st.GetFrames
            If sf.GetFileLineNumber() > 0 Then
                result &= "Line:" & sf.GetFileLineNumber() & " Filename: " & IO.Path.GetFileName(sf.GetFileName) & Environment.NewLine
            End If
        Next
        Return result
    End Function

    Sub GameLoop()
        Dim i As Integer
        Dim tmr1000 As Integer, tick As Integer, fogtmr As Integer, chattmr As Integer
        Dim tmpfps As Integer, tmplps As Integer, walkTimer As Integer, frameTime As Integer
        Dim tmrweather As Integer, barTmr As Integer
        Dim tmr25 As Integer, tmr500 As Integer, tmr250 As Integer, tmrconnect As Integer, TickFPS As Integer
        Dim fadetmr As Integer, rendertmr As Integer
        Dim animationtmr(1) As Integer

        ' Main game loop
        While InGame Or InMenu
            If GameDestroyed Then End

            tick = GetTickCount()
            ElapsedTime = tick - frameTime ' Set the time difference for time-based movement

            frameTime = tick

            DirUp = VbKeyUp
            DirDown = VbKeyDown
            DirLeft = VbKeyLeft
            DirRight = VbKeyRight

            ' Convert adjusted coordinates to game world coordinates
            CurX = TileView.Left + Math.Floor((Client.MouseCache("X") + Camera.Left) / PicX)
            CurY = TileView.Top + Math.Floor((Client.MouseCache("Y") + Camera.Top) / PicY)

            ' Store raw mouse coordinates for interface interactions
            CurMouseX = Client.MouseCache("X")
            CurMouseY = Client.MouseCache("Y")

            If GameStarted() Then
                'Calculate FPS
                If tmr1000 < tick Then
                    GetPing()
                    tmr1000 = tick + 1000
                End If

                If tmr25 < tick Then
                    PlayMusic(MyMap.Music)
                    tmr25 = tick + 25
                End If

                If ShowAnimTimer < tick Then
                    ShowAnimLayers = Not ShowAnimLayers
                    ShowAnimTimer = tick + 500
                End If

                For layer As Integer = 0 To 1
                    If animationtmr(layer) < tick Then
                        For x = 0 To MyMap.MaxX
                            For y = 0 To MyMap.MaxY
                                If IsValidMapPoint(x, y) Then
                                    On Error GoTo mapsync
                                    If MyMap.Tile(x, y).Data1 > 0 And (MyMap.Tile(x, y).Type = TileType.Animation Or MyMap.Tile(x, y).Type2 = TileType.Animation) Then
                                        Dim sprite As Integer = Type.Animation(MyMap.Tile(x, y).Data1).Sprite(layer)

                                        If sprite > 0 Then
                                            Dim GfxInfo As GameClient.GfxInfo = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Animations, sprite))

                                            ' Get dimensions and column count from controls and graphic info
                                            Dim totalWidth As Integer = GfxInfo.Width
                                            Dim totalHeight As Integer = GfxInfo.Height
                                            Dim columns As Integer = Type.Animation(MyMap.Tile(x, y).Data1).Frames(layer)
                                            Dim frameWidth As Integer

                                            ' Calculate frame dimensions
                                            If columns > 0 Then
                                                frameWidth = totalWidth / columns
                                            End If

                                            Dim frameHeight As Integer = frameWidth
                                            Dim rows As Integer

                                            If frameHeight > 0 Then
                                                rows = totalHeight / frameHeight
                                            End If

                                            Dim frameCount As Integer = rows * columns

                                            animationtmr(layer) = tick + Type.Animation(MyMap.Tile(x, y).Data1).LoopTime(layer) * frameCount * Type.Animation(MyMap.Tile(x, y).Data1).LoopCount(layer)
                                            CreateAnimation(MyMap.Tile(x, y).Data1, x, y)
                                        Else
                                            StreamAnimation(MyMap.Tile(x, y).Data1)
                                        End If
                                    End If
                                End If
                            Next
                        Next
mapsync:

                    End If
                Next

                For i = 0 To Byte.MaxValue
                    CheckAnimInstance(i)
                Next

                If tick > EventChatTimer Then
                    If EventText = "" Then
                        If EventChat = True Then
                            EventChat = False
                        End If
                    End If
                End If

                ' screenshake
                If ShakeTimerEnabled Then
                    If ShakeTimer < tick Then
                        If ShakeCount < 10 Then
                            If LastDir = 0 Then
                                LastDir = 1
                            Else
                                LastDir = 0
                            End If
                        Else
                            ShakeCount = 0
                            ShakeTimerEnabled = False
                        End If

                        ShakeCount += 1

                        ShakeTimer = tick + 50
                    End If
                End If

                ' check if we need to end the CD icon
                If NumSkills > 0 Then
                    For i = 1 To MAX_PLAYER_SKILLS
                        If Type.Player(MyIndex).Skill(i).Num > 0 Then
                            If Type.Player(MyIndex).Skill(i).CD > 0 Then
                                If Type.Player(MyIndex).Skill(i).CD + (Type.Skill(Type.Player(MyIndex).Skill(i).Num).CdTime * 1000) < tick Then
                                    Type.Player(MyIndex).Skill(i).CD = 0
                                End If
                            End If
                        End If
                    Next
                End If

                ' check if we need to unlock the player's skill casting restriction
                If SkillBuffer > 0 Then
                    If SkillBufferTimer + (Type.Skill(Type.Player(MyIndex).Skill(SkillBuffer).Num).CastTime * 1000) < tick Then
                        SkillBuffer = 0
                        SkillBufferTimer = 0
                    End If
                End If

                ' check if we need to unlock the pets's Skill casting restriction
                If PetSkillBuffer > 0 Then
                    If PetSkillBufferTimer + (Type.Skill(Type.Pet(Type.Player(MyIndex).Pet.Num).Skill(PetSkillBuffer)).CastTime * 1000) < tick Then
                        PetSkillBuffer = 0
                        PetSkillBufferTimer = 0
                    End If
                End If

                If CanMoveNow Then
                    CheckMovement() ' Check if player is trying to move
                    CheckAttack()   ' Check to see if player is trying to attack
                End If

                ' Process input before rendering, otherwise input will be behind by 1 frame
                If walkTimer < tick Then
                    For i = 1 To MAX_PLAYERS
                        If IsPlaying(i) Then
                            ProcessPlayerMovement(i)
                            If PetAlive(i) Then
                                ProcessPetMovement(i)
                            End If
                        End If
                    Next

                    ' Process npc movements (actually move them)
                    For i = 1 To MAX_MAP_NPCS
                        If MyMap.NPC(i) > 0 Then
                            ProcessNpcMovement(i)
                        End If
                    Next

                    For i = 1 To CurrentEvents
                        ProcessEventMovement(i)
                    Next

                    walkTimer = tick + 25 ' edit this value to change WalkTimer
                End If

                ' chat timer
                If chattmr < tick Then
                    ' scrolling
                    If ChatButtonUp Then
                        ScrollChatBox(0)
                    End If

                    If ChatButtonDown Then
                        ScrollChatBox(1)
                    End If

                    chattmr = tick + 50
                End If

                ' fog scrolling
                If fogtmr < tick Then
                    If CurrentFogSpeed > 0 Then
                        ' move
                        FogOffsetX = FogOffsetX - 1
                        FogOffsetY = FogOffsetY - 1

                        ' reset
                        If FogOffsetX < -255 Then FogOffsetX = 1
                        If FogOffsetY < -255 Then FogOffsetY = 1
                        fogtmr = tick + 255 - CurrentFogSpeed
                    End If
                End If

                If tmr500 < tick Then
                    ' animate waterfalls
                    Select Case WaterfallFrame
                        Case 0
                            WaterfallFrame = 1
                        Case 1
                            WaterfallFrame = 2
                        Case 2
                            WaterfallFrame = 0
                    End Select

                    ' animate autotiles
                    Select Case AutoTileFrame
                        Case 0
                            AutoTileFrame = 1
                        Case 1
                            AutoTileFrame = 2
                        Case 2
                            AutoTileFrame = 0
                    End Select

                    ' animate textbox
                    If chatShowLine = "|" Then
                        chatShowLine = ""
                    Else
                        chatShowLine = "|"
                    End If

                    tmr500 = tick + 500
                End If

                ' elastic bars
                If barTmr < tick Then
                    SetBarWidth(BarWidth_GuiHP_Max, BarWidth_GuiHP)
                    SetBarWidth(BarWidth_GuiSP_Max, BarWidth_GuiSP)
                    SetBarWidth(BarWidth_GuiEXP_Max, BarWidth_GuiEXP)
                    For i = 1 To MAX_MAP_NPCS
                        If Type.MyMapNPC(i).Num > 0 Then
                            SetBarWidth(BarWidth_NpcHP_Max(i), BarWidth_NpcHP(i))
                        End If
                    Next

                    For i = 1 To MAX_PLAYERS
                        If IsPlaying(i) And GetPlayerMap(i) = GetPlayerMap(MyIndex) Then
                            SetBarWidth(BarWidth_PlayerHP_Max(i), BarWidth_PlayerHP(i))
                            SetBarWidth(BarWidth_PlayerSP_Max(i), BarWidth_PlayerSP(i))
                        End If
                    Next

                    ' reset timer
                    barTmr = tick + 10
                End If

                ' Change map animation
                If tmr250 < tick Then
                    If MapAnim = 0 Then
                        MapAnim = 1
                    Else
                        MapAnim = 0
                    End If
                    tmr250 = tick + 250
                End If

                If FadeInSwitch = True Then
                    FadeIn()
                End If

                If FadeOutSwitch = True Then
                    FadeOut()
                End If
            Else
                If tmr500 < tick Then
                    ' animate textbox
                    If chatShowLine = "|" Then
                        chatShowLine = ""
                    Else
                        chatShowLine = "|"
                    End If

                    tmr500 = tick + 500
                End If

                If tmr25 < tick Then
                    PlayMusic(Type.Setting.MenuMusic)
                    tmr25 = tick + 25
                End If
            End If

            If tmrweather < tick Then
                ProcessWeather()
                tmrweather = tick + 50
            End If

            If fadetmr < tick Then
                If FadeType <> 2 Then
                    If FadeType = 1 Then
                        If FadeAmount = 255 Then
                        Else
                            FadeAmount = FadeAmount + 5
                        End If
                    ElseIf FadeType = 0 Then
                        If FadeAmount = 0 Then
                            UseFade = False
                        Else
                            FadeAmount = FadeAmount - 5
                        End If
                    End If
                End If
                fadetmr = tick + 30
            End If

            ' Calculate fps
            If TickFPS < tick Then
                GameFps = Fps
                TickFPS = tick + 1000
                Fps = 0
            Else
                Fps += 1
            End If
            
            If InGame Then
                Client.Render_Game()
            Else
                Client.Render_Menu()
            End If
            
            ResizeGUI()
        End While
    End Sub

End Module