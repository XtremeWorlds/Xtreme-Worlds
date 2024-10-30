Imports System.Collections.Concurrentt
Imports Core
Imports System.IO
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Input

Module General
    ' Keep track of the key states to avoid repeated input
    Private ReadOnly KeyStates As New Dictionary(Of Keys, Boolean)
    
    ' Define a dictionary to store the last time a key was processed
    Private KeyRepeatTimers As New Dictionary(Of Keys, DateTime)

    ' Minimum interval (in milliseconds) between repeated key inputs
    Private Const KeyRepeatInterval As Byte = 100
    
    Public Client As New GameClient()
    Public State As New GameState()
    Public Random As New Random()
    Public Gui As New Gui()

    Friend Function GetTickCount() As Integer
        Return Environment.TickCount
    End Function

    Sub Startup()
        GameState.InMenu = True
        ClearGameData()
        LoadGame()
        GameClient.LoadingCompleted.WaitOne()
        GameLoop()
    End Sub

    Friend Sub LoadGame()
        Settings.Load()
        Languages.Load()
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
        Gui.InitInterface()
        GameState.Ping = -1
    End Sub

    Friend Sub CheckAnimations()
        GameState.NumAnimations = GetFileCount(Core.Path.Animations)
    End Sub

    Friend Sub CheckCharacters()
        GameState.NumCharacters = GetFileCount(Core.Path.Characters)
    End Sub

    Friend Sub CheckEmotes()
        GameState.NumEmotes = GetFileCount(Core.Path.Emotes)
    End Sub

    Friend Sub CheckTilesets()
        GameState.NumTileSets = GetFileCount(Core.Path.Tilesets)
    End Sub

    Friend Sub CheckFogs()
        GameState.NumFogs = GetFileCount(Core.Path.Fogs)
    End Sub

    Friend Sub CheckItems()
        GameState.NumItems = GetFileCount(Core.Path.Items)
    End Sub

    Friend Sub CheckPanoramas()
        GameState.NumPanoramas = GetFileCount(Core.Path.Panoramas)
    End Sub

    Friend Sub CheckPaperdolls()
        GameState.NumPaperdolls = GetFileCount(Core.Path.Paperdolls)
    End Sub

    Friend Sub CheckParallax()
        GameState.NumParallax = GetFileCount(Core.Path.Parallax)
    End Sub

    Friend Sub CheckPictures()
        GameState.NumPictures = GetFileCount(Core.Path.Pictures)
    End Sub

    Friend Sub CheckProjectile()
        GameState.NumProjectiles = GetFileCount(Core.Path.Projectiles)
    End Sub

    Friend Sub CheckResources()
        GameState.NumResources = GetFileCount(Core.Path.Resources)
    End Sub

    Friend Sub CheckSkills()
        GameState.NumSkills = GetFileCount(Core.Path.Skills)
    End Sub

    Friend Sub CheckInterface()
        GameState.NumInterface = GetFileCount(Core.Path.Gui)
    End Sub

    Friend Sub CheckGradients()
        GameState.NumGradients = GetFileCount(Core.Path.Gradients)
    End Sub

    Friend Sub CheckDesigns()
        GameState.NumDesigns = GetFileCount(Core.Path.Designs)
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
        ReDim MusicCache(Directory.GetFiles(Core.Path.Music, "*" & Settings.MusicExt).Count)
        Dim files As String() = Directory.GetFiles(Core.Path.Music, "*" & Settings.MusicExt)
        Dim maxNum As String = Directory.GetFiles(Core.Path.Music, "*" & Settings.MusicExt).Count
        Dim counter As Integer = 0

        For Each FileName In files
            counter = counter + 1
            ReDim Preserve MusicCache(counter)

            MusicCache(counter) = IO.Path.GetFileName(FileName)
        Next
    End Sub

    Friend Sub CacheSound()
        ReDim SoundCache(Directory.GetFiles(Core.Path.Sounds, "*" & Settings.SoundExt).Count)
        Dim files As String() = Directory.GetFiles(Core.Path.Sounds, "*" & Settings.SoundExt)
        Dim maxNum As String = Directory.GetFiles(Core.Path.Sounds,  "*" & Settings.SoundExt).Count
        Dim counter As Integer = 0

        For Each FileName In files
            counter = counter + 1
            ReDim Preserve SoundCache(counter)

            SoundCache(counter) = IO.Path.GetFileName(FileName)
        Next
    End Sub

    Sub GameInit()
        ' Send a request to the server to open the admin menu if the user wants it.
        If Settings.OpenAdminPanelOnLogin = 1 Then
            If GetPlayerAccess(GameState.MyIndex) > 0 Then
                SendRequestAdmin()
            End If
        End If
    End Sub

    Friend Sub DestroyGame()
        ' break out of GameLoop
        GameState.InGame = False
        GameState.InMenu = False
        FreeBASS
        End
    End Sub
    
    Public Sub ProcessInputs()
        SyncLock GameClient.InputLock
            ' Get the mouse position from the cache
            Dim mousePos As Tuple(Of Integer, Integer) = GameClient.GetMousePosition()
            Dim mouseX As Integer = mousePos.Item1
            Dim mouseY As Integer = mousePos.Item2

            ' Convert adjusted coordinates to game world coordinates
            GameState.CurX = GameState.TileView.Left + Math.Floor((mouseX + GameState.Camera.Left) / GameState.PicX)
            GameState.CurY = GameState.TileView.Top + Math.Floor((mouseY + GameState.Camera.Top) / GameState.PicY)

            ' Store raw mouse coordinates for interface interactions
            GameState.CurMouseX = mouseX
            GameState.CurMouseY = mouseY

            ' Check for movement keys
            GameState.DirUp = GameClient.IsKeyStateActive(Keys.W) Or GameClient.IsKeyStateActive(Keys.Up)
            GameState.DirDown = GameClient.IsKeyStateActive(Keys.S) Or GameClient.IsKeyStateActive(Keys.Down)
            GameState.DirLeft = GameClient.IsKeyStateActive(Keys.A) Or GameClient.IsKeyStateActive(Keys.Left)
            GameState.DirRight = GameClient.IsKeyStateActive(Keys.D) Or GameClient.IsKeyStateActive(Keys.Right)

            ' Check for action keys
            GameState.VbKeyControl = GameClient.IsKeyStateActive(Keys.LeftControl)
            GameState.VbKeyShift = GameClient.IsKeyStateActive(Keys.LeftShift)

            ' Handle Escape key to toggle menus
            If GameClient.IsKeyStateActive(Keys.Escape) Then
                If GameState.InMenu = True Then Exit Sub

                ' Hide options screen
                If Gui.Windows(Gui.GetWindowIndex("winOptions")).Window.Visible = True
                    Gui.HideWindow(Gui.GetWindowIndex("winOptions"))
                    Gui.CloseComboMenu()
                    Exit Sub
                End If

                ' hide/show chat window
                If Gui.Windows(Gui.GetWindowIndex("winChat")).Window.Visible = True
                    Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).Text = ""
                    HideChat()
                    Exit Sub
                End If

                If Gui.Windows(Gui.GetWindowIndex("winEscMenu")).Window.Visible = True
                    Gui.HideWindow(Gui.GetWindowIndex("winEscMenu"))
                    Exit Sub
                Else
                    ' show them
                    If Gui.Windows(Gui.GetWindowIndex("winChat")).Window.Visible = False Then
                        Gui.ShowWindow(Gui.GetWindowIndex("winEscMenu"), True)
                        Exit Sub
                    End If
                End If
            End If
            
            If GameClient.IsKeyStateActive(Keys.Space) Then
                CheckMapGetItem()
            End If
            
            If GameClient.IsKeyStateActive(Keys.Insert) Then
                SendRequestAdmin()
            End If
            
            HandleHotbarInput()
            HandleMouseInputs()
            HandleActiveWindowInput()
            HandleTextInput()
            
            If GameState.InGame = True Then
                If Gui.Windows(Gui.GetWindowIndex("winEscMenu")).Window.Visible = True Then Exit Sub
            
                If GameClient.IsKeyStateActive(Keys.I) Then
                    ' hide/show inventory
                    If Not Gui.Windows(Gui.GetWindowIndex("winChat")).Window.Visible = True Then Gui.btnMenu_Inv
                End If
                
                If GameClient.IsKeyStateActive(Keys.C) Then
                    ' hide/show char
                    If Not Gui.Windows(Gui.GetWindowIndex("winChat")).Window.Visible = True Then Gui.btnMenu_Char
                End If
            
                If GameClient.IsKeyStateActive(Keys.K) Then
                    ' hide/show skills
                    If Not Gui.Windows(Gui.GetWindowIndex("winChat")).Window.Visible = True Then Gui.btnMenu_Skills
                End If
            
                If GameClient.IsKeyStateActive(Keys.Enter)
                    If Gui.Windows(Gui.GetWindowIndex("winChatSmall")).Window.Visible = True
                        ShowChat()
                        GameState.inSmallChat = 0
                        Exit Sub
                    End If

                    HandlePressEnter()
                End If
            End If
        End SyncLock
    End Sub
    
    Private Sub HandleActiveWindowInput()
        Dim key As Keys

        SyncLock GameClient.InputLock
            ' Check if there is an active window and that it is visible.
            If Gui.ActiveWindow > 0 AndAlso Gui.Windows(Gui.ActiveWindow).Window.Visible = True
                ' Check if an active control exists.
                If Gui.Windows(Gui.ActiveWindow).ActiveControl > 0 Then
                    ' Get the active control.
                    Dim activeControl = Gui.Windows(Gui.ActiveWindow).Controls(Gui.Windows(Gui.ActiveWindow).ActiveControl)

                    ' Check if the Enter key is active and can be processed.
                    key = Keys.Enter
                    If CanProcessKey(key) AndAlso GameClient.IsKeyStateActive(key) Then
                        ' Handle Enter: Call the control's callback or activate a new control.
                        If activeControl.CallBack(EntState.Enter) IsNot Nothing Then
                            activeControl.CallBack(EntState.Enter) = Nothing
                        Else
                            Dim n As Integer = Gui.ActivateControl()
                            If n = 0 Then Gui.ActivateControl(n, False)
                        End If
                    End If

                    ' Check if the Tab key is active and can be processed.
                    key = Keys.Tab
                    If CanProcessKey(key) AndAlso GameClient.IsKeyStateActive(key) Then
                        ' Handle Tab: Switch to the next control.
                        Dim n As Integer = Gui.ActivateControl()
                        If n = 0 Then Gui.ActivateControl(n, False)
                    End If
                End If
            End If
        End SyncLock
    End Sub
    
    ' Handles the hotbar key presses using KeyboardState
    Private Sub HandleHotbarInput()
        If GameState.inSmallChat Then
            ' Iterate through hotbar slots and check for corresponding keys
            For i = 1 To MAX_HOTBAR
                ' Check if the corresponding hotbar key is pressed
                If  GameClient.IsKeyStateActive(Keys.D0 + i) Then
                    SendUseHotbarSlot(i)
                    Exit Sub ' Exit once the matching slot is used
                End If
            Next
        End If
    End Sub

    Private Sub HandleTextInput()
        SyncLock GameClient.InputLock
            ' Iterate over all pressed keys
            For Each key As Keys In GameClient.CurrentKeyboardState.GetPressedKeys()
                If GameClient.IsKeyStateActive(key) AndAlso CanProcessKey(key) Then
                    ' Handle Backspace key separately
                    If key = Keys.Back Then
                        Dim activeControl = Gui.GetActiveControl()

                        If activeControl.HasValue AndAlso Not activeControl.Value.Locked AndAlso activeControl.Value.Text.Length > 0 Then
                            ' Modify the text inside the struct and update it back in the window
                            Dim modifiedControl = activeControl.Value
                            modifiedControl.Text = modifiedControl.Text.Substring(0, modifiedControl.Text.Length - 1)

                            ' Save the modified control back into the window
                            Gui.UpdateActiveControl(modifiedControl)
                        End If
                        Continue For ' Move to the next key
                    End If

                    ' Convert key to a character, considering Shift key
                    Dim character As Nullable(Of Char) = ConvertKeyToChar(key, GameClient.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))

                    ' If the character is valid, update the active control's text
                    If character.HasValue Then
                        Dim activeControl = Gui.GetActiveControl()

                        If activeControl.HasValue AndAlso Not activeControl.Value.Locked AndAlso activeControl.Value.Text.Length < activeControl.Value.Length Then
                            ' Modify the control's text
                            Dim modifiedControl = activeControl.Value
                            modifiedControl.Text &= character.Value

                            ' Save the modified control back into the window
                            Gui.UpdateActiveControl(modifiedControl)
                        End If
                    End If
                ElseIf KeyStates.ContainsKey(key) Then
                    ' If the key is released, remove it from KeyStates and reset the timer
                    KeyStates.Remove(key)
                    KeyRepeatTimers.Remove(key)
                End If
            Next
        End SyncLock
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
        HandleMouseClick(MouseButton.Left)
        HandleMouseClick(MouseButton.Right)
        HandleScrollWheel()
    End Sub
    
    Private Sub HandleScrollWheel()
        SyncLock GameClient.InputLock
            ' Handle scroll wheel (assuming delta calculation happens elsewhere)
            Dim scrollValue = GameClient.GetMouseScrollDelta()
            If scrollValue > 0 Then
                ScrollChatBox(0) ' Scroll up
            ElseIf scrollValue < 0 Then
                ScrollChatBox(1) ' Scroll down
            End If
            
            if scrollvalue <> 0 Then
                Gui.HandleInterfaceEvents(EntState.MouseScroll)
            End If
        End SyncLock
    End Sub
    
    Private Sub HandleMouseClick(button As MouseButton)
        SyncLock GameClient.InputLock
            Dim currentTime As Integer = Environment.TickCount

            ' Handle MouseMove event when the mouse moves
            If GameClient.CurrentMouseState.X <> GameClient.PreviousMouseState.X OrElse
               GameClient.CurrentMouseState.Y <> GameClient.PreviousMouseState.Y Then
                Gui.HandleInterfaceEvents(EntState.MouseMove)
            End If

            ' Check for MouseDown event (button pressed)
            If GameClient.IsMouseButtonDown(button) Then
                Gui.HandleInterfaceEvents(EntState.MouseDown)
                GameState.LastLeftClickTime = currentTime ' Track time for double-click detection
            End If
            
            ' Check for MouseUp event (button released)
            If Not GameClient.IsMouseButtonUp(button) Then
                Gui.HandleInterfaceEvents(EntState.MouseUp)
            End If

            ' Double-click detection for left button
            If button = MouseButton.Left AndAlso 
               currentTime - GameState.LastLeftClickTime <= GameState.DoubleClickTImer Then
                Gui.HandleInterfaceEvents(EntState.DblClick)
                GameState.LastLeftClickTime = 0 ' Reset double-click timer
            End If

            ' In-game interactions for left click
            If GameState.InGame = True Then
                If button = MouseButton.Left Then
                    If PetAlive(GameState.MyIndex) AndAlso IsInBounds() Then
                        PetMove(GameState.CurX, GameState.CurY)
                    End If
                    CheckAttack(True)
                    PlayerSearch(GameState.CurX, GameState.CurY, 0)
                End If

                ' Right-click interactions
                If button = MouseButton.Right and GameState.InGame = True Then
                    If GameState.VbKeyShift Then
                        ' Admin warp if Shift is held and the player has moderator access
                        If GetPlayerAccess(GameState.MyIndex) >= AccessType.Moderator Then
                            AdminWarp(GameClient.CurrentMouseState.X, GameClient.CurrentMouseState.Y)
                        End If
                    Else
                        ' Handle right-click menu
                        HandleRightClickMenu()
                    End If
                End If
            End If
        End SyncLock
    End Sub
    
    Private Sub HandleRightClickMenu()
        ' Loop through all players and display the right-click menu for the matching one
        For i = 1 To MAX_PLAYERS
            If IsPlaying(i) AndAlso GetPlayerMap(i) = GetPlayerMap(GameState.MyIndex) Then
                If GetPlayerX(i) = GameState.CurX AndAlso GetPlayerY(i) = GameState. CurY Then
                    ' Use current mouse state for the X and Y positions
                    ShowPlayerMenu(i, GameClient.CurrentMouseState.X, GameClient.CurrentMouseState.Y)
                End If
            End If
        Next

        ' Perform player search at the current cursor position
        PlayerSearch(GameState.CurX, GameState.CurY, 1)
    End Sub
    
    Public Function IsEq(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To EquipmentType.Count - 1
            If GetPlayerEquipment(GameState.MyIndex, i) Then
                With tempRec
                .Top = StartY + GameState.EqTop + (GameState.PicY * ((i - 1) \ GameState.EqColumns))
                .bottom = .Top + GameState.PicY
                .Left = StartX + GameState.EqLeft + ((GameState.EqOffsetX + GameState.PicX) * (((i - 1) Mod GameState.EqColumns)))
                .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsEq = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Public Function IsInv(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_INV
            If GetPlayerInv(GameState.MyIndex, i) > 0 Then
                With tempRec
                    .Top = StartY + GameState.InvTop + ((GameState.InvOffsetY + GameState.PicY) * ((i - 1) \ GameState.InvColumns))
                    .bottom = .Top + GameState.PicY
                    .Left = StartX + GameState.InvLeft + ((GameState.InvOffsetX + GameState.PicX) * (((i - 1) Mod GameState.InvColumns)))
                    .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsInv = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Public Function IsSkill(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_PLAYER_SKILLS
            If Type.Player(GameState.MyIndex).Skill(i).Num Then
                With tempRec
                    .Top = StartY + GameState.SkillTop + ((GameState.SkillOffsetY + GameState.PicY) * ((i - 1) \ GameState.SkillColumns))
                    .bottom = .Top + GameState.PicY
                    .Left = StartX + GameState.SkillLeft + ((GameState.SkillOffsetX + GameState.PicX) * (((i - 1) Mod GameState.SkillColumns)))
                    .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsSkill = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Public Function IsBank(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_BANK
            If GetBank(GameState.MyIndex, i) > 0 Then
                With tempRec
                    .Top = StartY + GameState.BankTop + ((GameState.BankOffsetY + GameState.PicY) * ((i - 1) \ GameState.BankColumns))
                    .bottom = .Top + GameState.PicY
                    .Left = StartX + GameState.BankLeft + ((GameState.BankOffsetX + GameState.PicX) * (((i - 1) Mod GameState.BankColumns)))
                    .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsBank = i
                        Exit Function
                    End If
                End If
            End If
        
        Next

    End Function

    Public Function IsShop(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_TRADES
            With tempRec
                .Top = StartY + GameState.ShopTop + ((GameState.ShopOffsetY + GameState.PicY) * ((i - 1) \ GameState.ShopColumns))
                .bottom = .Top + GameState.PicY
                .Left = StartX + GameState.ShopLeft + ((GameState.ShopOffsetX + GameState.PicX) * (((i - 1) Mod GameState.ShopColumns)))
                .Right = .Left + GameState.PicX
            End With

            If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                    IsShop = i
                    Exit Function
                End If
            End If
        Next
    End Function

    Public Function IsTrade(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_INV
            With tempRec
                .Top = StartY + GameState.TradeTop + ((GameState.TradeOffsetY + GameState.PicY) * ((i - 1) \ GameState.TradeColumns))
                .bottom = .Top + GameState.PicY
                .Left = StartX + GameState.TradeLeft + ((GameState.TradeOffsetX + GameState.PicX) * (((i - 1) Mod GameState.TradeColumns)))
                .Right = .Left + GameState.PicX
            End With

            If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                    IsTrade = i
                    Exit Function
                End If
            End If
        Next
    End Function

End Module