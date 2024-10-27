Imports System.Collections.Concurrent
Imports Client.GameClient
Imports Core
Imports System.IO
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Xna.Framework.Input

Module General
    ' Keep track of the key states to avoid repeated input
    Private ReadOnly KeyStates As New Dictionary(Of Keys, Boolean)
    
    ' Define a dictionary to store the last time a key was processed
    Private KeyRepeatTimers As New Dictionary(Of Keys, DateTime)

    ' Minimum interval (in milliseconds) between repeated key inputs
    Private Const KeyRepeatInterval As Integer = 100
    
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
    
    Public Sub ProcessInputs()
        ' Get the mouse position from the cache
        Dim mousePos As Tuple(Of Integer, Integer) = GetMousePosition()
        Dim mouseX As Integer = mousePos.Item1
        Dim mouseY As Integer = mousePos.Item2

        ' Convert adjusted coordinates to game world coordinates
        CurX = TileView.Left + Math.Floor((mouseX + Camera.Left) / PicX)
        CurY = TileView.Top + Math.Floor((mouseY + Camera.Top) / PicY)

        ' Store raw mouse coordinates for interface interactions
        CurMouseX = mouseX
        CurMouseY = mouseY

        ' Check for movement keys
        DirUp = IsKeyStateActive(Keys.W) Or IsKeyStateActive(Keys.Up)
        DirDown = IsKeyStateActive(Keys.S) Or IsKeyStateActive(Keys.Down)
        DirLeft = IsKeyStateActive(Keys.A) Or IsKeyStateActive(Keys.Left)
        DirRight = IsKeyStateActive(Keys.D) Or IsKeyStateActive(Keys.Right)

        ' Check for action keys
        VbKeyControl = IsKeyStateActive(Keys.LeftControl)
        VbKeyShift = IsKeyStateActive(Keys.LeftShift)

        ' Handle Escape key to toggle menus
        If IsKeyStateActive(Keys.Escape) Then
            If InMenu Then Exit Sub

            ' Hide options screen
            If Windows(GetWindowIndex("winOptions")).Window.visible Then
                HideWindow(GetWindowIndex("winOptions"))
                CloseComboMenu()
                Exit Sub
            End If

            ' hide/show chat window
            If Windows(GetWindowIndex("winChat")).Window.Visible Then
                Windows(GetWindowIndex("winChat")).Controls(GetControlIndex("winChat", "txtChat")).Text = ""
                HideChat()
                Exit Sub
            End If

            If Windows(GetWindowIndex("winEscMenu")).Window.visible Then
                HideWindow(GetWindowIndex("winEscMenu"))
                Exit Sub
            Else
                ' show them
                If Windows(GetWindowIndex("winChat")).Window.Visible = False Then
                    ShowWindow(GetWindowIndex("winEscMenu"), True)
                    Exit Sub
                End If
            End If
        End If
        
        If IsKeyStateActive(Keys.Space) Then
            CheckMapGetItem()
        End If
        
        If IsKeyStateActive(Keys.Insert) Then
            SendRequestAdmin()
        End If
        
        HandleHotbarInput()
        HandleMouseInputs()
        HandleActiveWindowInput()
        HandleTextInput()
        
        If Windows(GetWindowIndex("winEscMenu")).Window.visible Then Exit Sub
        
        If IsKeyStateActive(Keys.I) Then
            ' hide/show inventory
            If Not Windows(GetWindowIndex("winChat")).Window.visible Then btnMenu_Inv
        End If
            
        If IsKeyStateActive(Keys.C) Then
            ' hide/show char
            If Not Windows(GetWindowIndex("winChat")).Window.visible Then btnMenu_Char
        End If
        
        If IsKeyStateActive(Keys.K) Then
            ' hide/show skills
            If Not Windows(GetWindowIndex("winChat")).Window.visible Then btnMenu_Skills
        End If
        
        If IsKeyStateActive(Keys.Enter)
            If Windows(GetWindowIndex("winChatSmall")).Window.Visible Then
                ShowChat()
                inSmallChat = False
                Exit Sub
            End If

            HandlePressEnter()
        End If
    End Sub
    
    Private Sub HandleActiveWindowInput()
        ' Check for active window
        If activeWindow > 0 AndAlso Windows(activeWindow).Window.Visible Then
            ' Check if an active control exists
            If Windows(activeWindow).ActiveControl > 0 Then
                ' Get the active control
                Dim activeControl = Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl)

                If IsKeyStateActive(Keys.Enter) Then
                    ' Handle Enter: Call the control’s callback or activate a new control
                    If Not activeControl.CallBack(EntState.Enter) Is Nothing Then
                        activeControl.CallBack(EntState.Enter) = Nothing
                    Else
                        Dim n As Integer = ActivateControl()

                        If n = 0 Then
                            ActivateControl(n, False)
                        End If
                    End If

                ElseIf IsKeyStateActive(Keys.Tab) Then
                    ' Handle Tab: Switch to the next control
                    Dim n As Integer = ActivateControl()
                    
                    
                    If n = 0 Then
                        ActivateControl(n, False)
                    End If
                End If
            End If
        End If
    End Sub

    
    ' Handles the hotbar key presses using KeyboardState
    Private Sub HandleHotbarInput()
        If inSmallChat Then
            ' Iterate through hotbar slots and check for corresponding keys
            For i = 1 To MAX_HOTBAR
                ' Check if the corresponding hotbar key is pressed
                If  IsKeyStateActive(Keys.D0 + i) Then
                    SendUseHotbarSlot(i)
                    Exit Sub ' Exit once the matching slot is used
                End If
            Next
        End If
    End Sub

    Private Sub HandleTextInput()
        ' Loop through all keys in the keyboard state
        For Each key As Keys In System.Enum.GetValues(GetType(Keys))
            Dim isKeyDown =  IsKeyStateActive(key)

            If isKeyDown Then
                ' Check if enough time has passed since the last key press processing
                If CanProcessKey(key) Then
                    ' Handle Backspace separately
                    If key = Keys.Back Then
                        HandleBackspace()
                        Continue For
                    End If

                    ' Convert key to character
                    Dim character As Nullable(Of Char) = ConvertKeyToChar(key, IsKeyStateActive(Keys.LeftShift))

                    ' If valid character and active control is available, add text
                    If character.HasValue AndAlso activeWindow > 0 AndAlso
                       Windows(activeWindow).Window.Visible AndAlso
                       Windows(activeWindow).ActiveControl > 0 Then

                        Dim control = Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl)

                        ' Ensure the control is not locked and text limit is respected
                        If Not control.Locked AndAlso control.Text.Length < control.Length Then
                            Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl).Text &= character.Value ' Add the character
                        End If
                    End If
                End If
            ElseIf KeyStates.ContainsKey(key) Then
                ' Remove the key from KeyStates when it is released
                KeyStates.Remove(key)
                KeyRepeatTimers.Remove(key) ' Reset the key's timer
            End If
        Next
    End Sub

    ' Check if the key can be processed (with interval-based repeat logic)
    Private Function CanProcessKey(key As Keys) As Boolean
        Dim now = DateTime.Now
        If Not KeyRepeatTimers.ContainsKey(key) OrElse (now - KeyRepeatTimers(key)).TotalMilliseconds >= KeyRepeatInterval Then
            KeyRepeatTimers(key) = now ' Update the timer for the key
            Return True
        End If
        Return False
    End Function
    
    ' Handle Backspace to remove the last character from the active control
    Private Sub HandleBackspace()
        If activeWindow > 0 AndAlso Windows(activeWindow).Window.Visible AndAlso
           Windows(activeWindow).ActiveControl > 0 Then

            ' Ensure the control is not locked
            If  Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl).Locked Then Exit Sub

            ' Remove the last character if the text is not empty
            If  Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl).Text.Length > 0 Then
                Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl).Text =  Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl).Text.Substring(0,  Windows(activeWindow).Controls(Windows(activeWindow).ActiveControl).Text.Length - 1)
            End If
        End If
    End Sub

    ' Convert a key to a character (if possible)
    Private Function ConvertKeyToChar(key As Keys, shiftPressed As Boolean) As Char?
        ' Handle alphabetic keys
        If key >= Keys.A AndAlso key <= Keys.Z Then
            Dim baseChar As Char = ChrW(AscW("A"c) + (key - Keys.A))
            Return If(shiftPressed, baseChar, Char.ToLower(baseChar))
        End If

        ' Handle numeric keys (0-9)
        If key >= Keys.D0 AndAlso key <= Keys.D9 Then
            Dim digit As Char = ChrW(AscW("0"c) + (key - Keys.D0))
            Return If(shiftPressed, GetShiftedDigit(digit), digit)
        End If

        ' Handle space key
        If key = Keys.Space Then Return " "c

        ' Ignore unsupported keys (e.g., function keys, control keys)
        Return Nothing
    End Function

    ' Get the shifted version of a digit key (for symbols)
    Private Function GetShiftedDigit(digit As Char) As Char
        Select Case digit
            Case "1"c : Return "!"c
            Case "2"c : Return "@"c
            Case "3"c : Return "#"c
            Case "4"c : Return "$"c
            Case "5"c : Return "%"c
            Case "6"c : Return "^"c
            Case "7"c : Return "&"c
            Case "8"c : Return "*"c
            Case "9"c : Return "("c
            Case "0"c : Return ")"c
            Case Else : Return digit
        End Select
    End Function
    
    Private Sub HandleMouseInputs()
        HandleLeftClick()
        HandleRightClick()
        HandleScrollWheel()
    End Sub
    
    Private Sub HandleScrollWheel()
        SyncLock InputLock
            ' Handle scroll wheel (assuming delta calculation happens elsewhere)
            Dim scrollValue = GetMouseScrollDelta()
            If scrollValue > 0 Then
                ScrollChatBox(0) ' Scroll up
            ElseIf scrollValue < 0 Then
                ScrollChatBox(1) ' Scroll down
            End If
            
            if scrollvalue <> 0 Then
                HandleInterfaceEvents(entState.MouseScroll)
            End If
        End SyncLock
    End Sub
    
    Private Sub HandleLeftClick()
        SyncLock InputLock
            Dim currentTime As Integer = Environment.TickCount
            
            ' Detect MouseMove event (when the mouse position changes)
            If PreviousMouseState.LeftButton = ButtonState.Released And CurrentMouseState.X <> PreviousMouseState.X OrElse
               CurrentMouseState.Y <> PreviousMouseState.Y Then
                HandleInterfaceEvents(EntState.MouseMove)
            End If
            
            ' Check if the left button has just been released after being pressed
            If CurrentMouseState.LeftButton = ButtonState.Pressed Then
                ' Detect MouseDown event (when a button is pressed)
                If CurrentMouseState.LeftButton = ButtonState.Pressed Then
                    HandleInterfaceEvents(EntState.MouseDown)
                End If

                ' Detect MouseUp event (when a button is released)
                If CurrentMouseState.LeftButton = ButtonState.Released Then
                    HandleInterfaceEvents(EntState.MouseUp)
                End If
                
                If PreviousMouseState.LeftButton = ButtonState.Released Then
                    ' Handle double-click logic
                    If currentTime - LastLeftClickTime <= DoubleClickTImer Then
                        HandleInterfaceEvents(EntState.DblClick)
                        LastLeftClickTime = 0 ' Reset to avoid consecutive double-clicks
                    Else
                        HandleInterfaceEvents(EntState.MouseDown)
                        LastLeftClickTime = currentTime ' Track time for future double-clicks
                    End If
                End If

                ' In-game interactions after a successful left-click
                If inGame Then
                    If PetAlive(MyIndex) AndAlso IsInBounds() Then
                        PetMove(CurX, CurY)
                    End If
                    CheckAttack(True)
                    PlayerSearch(CurX, CurY, 0)
                End If
            End If
        End SyncLock
    End Sub

    Private Sub HandleRightClick()
        SyncLock InputLock
            ' Check if the right button is pressed
            If CurrentMouseState.RightButton = ButtonState.Pressed Then
                If VbKeyShift Then
                    ' Admin warp if Shift is held and the player has moderator access
                    If GetPlayerAccess(MyIndex) >= AccessType.Moderator Then
                        AdminWarp(CurrentMouseState.X, CurrentMouseState.Y)
                    End If
                Else
                    ' Handle the right-click menu
                    HandleRightClickMenu()
                End If
            End If
        End SyncLock
    End Sub
    
    Private Sub HandleRightClickMenu()
        ' Loop through all players and display the right-click menu for the matching one
        For i = 1 To MAX_PLAYERS
            If IsPlaying(i) AndAlso GetPlayerMap(i) = GetPlayerMap(MyIndex) Then
                If GetPlayerX(i) = CurX AndAlso GetPlayerY(i) = CurY Then
                    ' Use current mouse state for the X and Y positions
                    ShowPlayerMenu(i, CurrentMouseState.X, CurrentMouseState.Y)
                End If
            End If
        Next

        ' Perform player search at the current cursor position
        PlayerSearch(CurX, CurY, 1)
    End Sub
    
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
            Client.TextureCounter = 0

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

            ' Render game or menu based on state
            If InGame Then
                Client.Render_Game()
            Else
                Client.Render_Menu()
            End If
            
            ResizeGUI()
            
            ' Signal that loading is complete
            SyncLock loadLock
                if IsLoading Then
                    isLoading = False
                End If
            End SyncLock
        End While
    End Sub

End Module