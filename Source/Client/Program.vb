Imports System.Collections.Concurrent
Imports System.IO
Imports System.Threading
Imports Core
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class GameClient
    Inherits Game

    Public Shared Graphics As GraphicsDeviceManager
    Public Shared SpriteBatch As SpriteBatch
    Public Shared ReadOnly TextureCache As New ConcurrentDictionary(Of String, Texture2D)()
    Public Shared ReadOnly GfxInfoCache As New ConcurrentDictionary(Of String, GfxInfo)()
    Public Shared TextureCounter As Integer
    Public Shared LoadingCompleted As ManualResetEvent = New ManualResetEvent(False)
    
    Public ReadOnly MultiplyBlendState As New BlendState()

    ' Queue to maintain FIFO order of batches
    Public Shared Batches As New ConcurrentDictionary(Of Integer, RenderBatch)()
    Public Shared ReadOnly BatchLock As New Object()
    
    Private Shared _gameFps As Integer
    Private Shared ReadOnly FpsLock As New Object()

    ' Safely set FPS with a lock
    Public Shared Sub SetFps(newFps As Integer)
        SyncLock FpsLock
            _gameFps = newFps
        End SyncLock
    End Sub

    ' Safely get FPS with a lock
    Public Shared Function GetFps() As Integer
        SyncLock FpsLock
            Return _gameFps
        End SyncLock
    End Function

    Public Class RenderBatch
        Public Property Texture As Texture2D
        Public Property TextureCounter As Integer
        Public Property Font as SpriteFont
        Public Property Commands As New List(Of RenderCommand)()
    End Class
    
    ' ManualResetEvent to signal when loading is complete
    Public Shared IsLoading As Boolean = True
    Public Shared ReadOnly LoadLock As New Object()
    
    ' State tracking variables
    ' Shared keyboard and mouse states for cross-thread access
    Public Shared CurrentKeyboardState As KeyboardState
    Public Shared PreviousKeyboardState As KeyboardState

    Public Shared CurrentMouseState As MouseState
    Public Shared PreviousMouseState As MouseState
    
    ' Keep track of the key states to avoid repeated input
    Public Shared ReadOnly KeyStates As New Dictionary(Of Keys, Boolean)
    
    ' Define a dictionary to store the last time a key was processed
    Public Shared KeyRepeatTimers As New Dictionary(Of Keys, DateTime)

    ' Minimum interval (in milliseconds) between repeated key inputs
    Private Const KeyRepeatInterval As Byte = 100

    ' Lock object to ensure thread safety
    Public Shared ReadOnly InputLock As New Object()

    ' Track the previous scroll value to compute delta
    Private Shared ReadOnly ScrollLock As New Object()

    Private elapsedTime As TimeSpan = TimeSpan.Zero

    Private Shared TilesetWindow As RenderTarget2D
    Private EditorAnimation_Anim1 As RenderTarget2D
    Private EditorAnimation_Anim2 As RenderTarget2D
    Private Shared RenderTarget As RenderTarget2D
    Public Shared TransparentTexture As Texture2D

    ' Ensure this class exists to store graphic info
    Public Class GfxInfo
        Public Width As Integer
        Public Height As Integer
    End Class

    Public Shared Function GetGfxInfo(key As String) As GfxInfo
        ' Check if the key does not end with ".gfxext" and append if needed
        If Not key.EndsWith(GameState.GfxExt, StringComparison.OrdinalIgnoreCase) Then
            key &= GameState.GfxExt
        End If

        ' Retrieve the texture
        Dim texture = GameClient.GetTexture(key)

        Dim result As GfxInfo = Nothing
        If Not GfxInfoCache.TryGetValue(key, result) Then
            ' Log or handle the case where the key is not found in the cache
            Debug.WriteLine($"Warning: GfxInfo for key '{key}' not found in cache.")
            Return Nothing
        End If

        Return result
    End Function

    Public Sub New()
        GetResolutionSize(Settings.Resolution, GameState.ResolutionWidth, GameState.ResolutionHeight)

        Graphics = New GraphicsDeviceManager(Me)

        ' Set basic properties for GraphicsDeviceManager
        With Graphics
            .IsFullScreen = Settings.FullScreen
            .PreferredBackBufferWidth = GameState.ResolutionWidth
            .PreferredBackBufferHeight = GameState.ResolutionHeight
            .SynchronizeWithVerticalRetrace = Settings.Vsync
            .PreferHalfPixelOffset = True
            .PreferMultiSampling = True
        End With

        ' Add handler for PreparingDeviceSettings
        AddHandler Graphics.PreparingDeviceSettings, Sub(sender, args)
            args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents
            args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8
        End Sub

#If DEBUG Then
        Me.IsMouseVisible = True
#End If

        Content.RootDirectory = "Content"

        ' Hook into the Exiting event to handle window close
        AddHandler Me.Exiting, AddressOf OnWindowClose
        AddHandler Graphics.DeviceReset, AddressOf OnDeviceReset
    End Sub
    
    Protected Overrides Sub Initialize()
        Window.Title = Settings.GameName

        ' Create the RenderTarget2D with the same size as the screen
        RenderTarget = New RenderTarget2D(
            Graphics.GraphicsDevice,
            Graphics.GraphicsDevice.PresentationParameters.BackBufferWidth,
            Graphics.GraphicsDevice.PresentationParameters.BackBufferHeight,
            False,
            Graphics.GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24)
        
        InitializeMultiplyBlendState()
        
        ' Apply changes to GraphicsDeviceManager
        Try
            Graphics.ApplyChanges()
        Catch ex As Exception
            Debug.WriteLine($"GraphicsDevice initialization failed: {ex.Message}")
            Throw
        End Try

        MyBase.Initialize()
    End Sub

    Sub InitializeMultiplyBlendState()
        With MultiplyBlendState
            .ColorSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.DestinationColor
            .ColorDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.Zero
            .ColorBlendFunction = BlendFunction.Add
            .AlphaSourceBlend = Microsoft.Xna.Framework.Graphics.Blend.One
            .AlphaDestinationBlend = Microsoft.Xna.Framework.Graphics.Blend.Zero
            .AlphaBlendFunction = BlendFunction.Add
        End With
    End Sub

    Public Class RenderCommand
        Public Property Type As Byte
        Public Property Path As String
        Public Property Text As String
        Public Property sRect As Rectangle
        Public Property dRect As Rectangle
        Public Property X As Integer
        Public Property Y As Integer
        Public Property Color As Color
        Public Property Color2 As Color
        Public Property EntityID As Integer
        Public Property TextureID As Integer
    End Class
    
    Private Sub LoadFonts()
        For i = 1 To FontType.Count - 1
            Fonts(i) = LoadFont(Core.Path.Fonts, i)
        Next
    End Sub
    
    ' Method to center the window using GraphicsAdapter
    Private Sub CenterWindow()
        ' Get the primary display's resolution
        Dim displayMode As DisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode

        Dim screenWidth As Integer = displayMode.Width
        Dim screenHeight As Integer = displayMode.Height

        Dim windowWidth As Integer = graphics.PreferredBackBufferWidth
        Dim windowHeight As Integer = graphics.PreferredBackBufferHeight

        ' Calculate centered position
        Dim posX As Integer = (screenWidth - windowWidth) / 2
        Dim posY As Integer = (screenHeight - windowHeight) / 2

        ' Set the new window position
        Window.Position = New Point(posX, posY)
    End Sub
    
    Protected Overrides Sub LoadContent()
        SpriteBatch = New SpriteBatch(GraphicsDevice)

        TransparentTexture = New Texture2D(GraphicsDevice, 1, 1)
        TransparentTexture.SetData(New Color() {Color.White})

        LoadFonts()
        
        ' Signal that loading is complete
        loadingCompleted.Set()
    End Sub

    Public Function LoadFont(path As String, font As [Enum].FontType) As SpriteFont
        Return Content.Load(Of SpriteFont)(IO.Path.Combine(path, font))
    End Function

    Public Shared Sub EnqueueText(ByRef text As String, path As String, x As Integer, y As Integer, 
                           font As FontType, frontColor As Color, backColor As Color,
                           Optional entityID As Integer = 0)
        
        SyncLock batchLock
            TextureCounter += 1
            
            ' Create the new render command
            Dim newCommand = New RenderCommand With {
                    .Type = RenderType.Font,
                    .Path = path,
                    .Text = text,
                    .X = x,
                    .Y = y,
                    .Color = frontColor,
                    .Color2 = backColor,
                    .EntityID = entityID,
                    .TextureID = GenerateUniqueTextureID(.Path, TextureCounter)
                    }
            
            ' Try to update an existing batch with the same TextCounter
            If Not UpdateBatches(newCommand) Then
                ' Create a new batch if no matching batch was found
                Dim batch = New RenderBatch() With {
                        .Font = Fonts(font),
                        .TextureCounter = TextureCounter
                        }
                batch.Commands.Add(newCommand)

                ' Enqueue the new batch
                batches.TryAdd(TextureCounter, batch)
            End If
        End SyncLock
    End Sub

    ' Method to enqueue textures and manage duplicates
    Public Shared Sub EnqueueTexture(ByRef path As String, dX As Integer, dY As Integer,
                              sX As Integer, sY As Integer, dW As Integer, dH As Integer,
                              Optional sW As Integer = 1, Optional sH As Integer = 1,
                              Optional alpha As Byte = 255, Optional red As Byte = 255,
                              Optional green As Byte = 255, Optional blue As Byte = 255,
                              Optional entityID As Integer = 0)

        ' Create destination and source rectangles
        Dim dRect As New Rectangle(dX, dY, dW, dH)
        Dim sRect As New Rectangle(sX, sY, sW, sH)
        Dim color As New Color(red, green, blue, alpha)

        path = EnsureFileExtension(path)

        ' Retrieve the texture
        Dim texture = GetTexture(path)
        If texture Is Nothing Then
            Return
        End If

        SyncLock batchLock
            TextureCounter += 1
            
            ' Create a new render command
            Dim newCommand = New RenderCommand With {
                    .Type = RenderType.Texture,
                    .Path = path,
                    .dRect = dRect,
                    .sRect = sRect,
                    .Color = color,
                    .EntityID = entityID,
                    .TextureID = GenerateUniqueTextureID(.Path, TextureCounter)
                    }
            
            ' Try to update an existing batch with the same TextureID
            If Not UpdateBatches(newCommand) Then
                ' Create a new batch if no matching batch was found
                Dim batch = New RenderBatch() With {
                        .Texture = texture,
                        .TextureCounter = TextureCounter
                        }
                batch.Commands.Add(newCommand)
                batches.TryAdd(TextureCounter, batch)
            End If
        End SyncLock
    End Sub
    
    Public Shared Sub RenderTexture(ByRef path As String, dX As Integer, dY As Integer,
                              sX As Integer, sY As Integer, dW As Integer, dH As Integer,
                              Optional sW As Integer = 1, Optional sH As Integer = 1,
                              Optional alpha As Byte = 255, Optional red As Byte = 255,
                              Optional green As Byte = 255, Optional blue As Byte = 255)

        ' Create destination and source rectangles
        Dim dRect As New Rectangle(dX, dY, dW, dH)
        Dim sRect As New Rectangle(sX, sY, sW, sH)
        Dim color As New Color(red, green, blue, alpha)

        path = EnsureFileExtension(path)

        ' Retrieve the texture
        Dim texture = GameClient.GetTexture(path)
        If texture Is Nothing Then
            Return
        End If

        SpriteBatch.Draw(texture, dRect, sRect, Color)
    End Sub
    
    Private Shared Function GenerateUniqueTextureID(path As String, index As Integer) As Integer
        Dim pathHash = path.GetHashCode() ' Generate a hash from the path
        Dim uniqueID = Math.Abs(pathHash + index) ' Ensure the ID is non-negative

        Return uniqueID
    End Function
    
    Private Shared Function UpdateBatches(newCommand As RenderCommand) As Boolean
        Dim batchToUpdate As RenderBatch = Nothing
        Dim matchingCommand As RenderCommand
 
        SyncLock BatchLock
            ' Iterate over each batch in the dictionary
            For Each key As Integer In Batches.Keys.ToList()
                ' Try to dequeue a batch that matches the command's texture ID or add back if not updated
                If Batches.TryGetValue(key, batchToUpdate) Then
                    ' Search for an existing command in the dequeued batch.
                    matchingCommand = batchToUpdate.Commands.FirstOrDefault(
                        Function(cmd) cmd.EntityID = newCommand.EntityID)

                    If matchingCommand IsNot Nothing Then
                        If matchingCommand.EntityID > 0 And Not Gui.Windows(matchingCommand.EntityID).Visible = True
                            Batches.TryRemove(key, batchToUpdate)
                            Continue For
                        End If
                    End If
                    
                    ' Search for an existing command in the dequeued batch.
                    matchingCommand = batchToUpdate.Commands.FirstOrDefault(
                        Function(cmd) cmd.TextureID = newCommand.TextureID)

                    If matchingCommand IsNot Nothing Then
                        ' Update the matching command's properties.
                        With matchingCommand
                            .sRect = newCommand.sRect
                            .dRect = newCommand.dRect
                            .X = newCommand.X
                            .Y = newCommand.Y
                            .Color = newCommand.Color
                            .Color2 = newCommand.Color2
                            .Text = newCommand.Text
                        End With

                        ' Update texture if necessary.
                        If newCommand.Type = RenderType.Texture Then
                            batchToUpdate.Texture = GetTexture(newCommand.Path)
                        End If
                        
                        Continue For
                    End If
                End If
            Next
        End SyncLock
        Return False
    End Function
    
    Public Shared Function GetTexture(path As String) As Texture2D
        If Not GameClient.TextureCache.ContainsKey(path) Then
            Dim texture = GameClient.LoadTexture(path)
            return texture
        End If
        
        Return TextureCache(path)
    End Function
    
    Public Shared Function LoadTexture(path As String) As Texture2D
        Try
            Using stream As New FileStream(path, FileMode.Open)
                Dim texture = Texture2D.FromStream(GameClient.Graphics.GraphicsDevice, stream)
                
                ' Cache graphics information
                Dim gfxInfo As New GfxInfo With {
                        .Width = texture.Width,
                        .Height = texture.Height
                        }
                GfxInfoCache.TryAdd(path, gfxInfo)
                
                TextureCache(path) = texture

                Return texture
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error loading texture from {path}: {ex.Message}")
            Return Nothing
        End Try
    End Function
    
    Protected Overrides Sub Draw(gameTime As GameTime)
        Graphics.GraphicsDevice.Clear(Color.Black)

        SyncLock LoadLock
            If IsLoading = True then Exit Sub
        End SyncLock
        
        SpriteBatch.Begin()
        If GameState.InGame = True Then
            Render_Game()
        Else
            Render_Menu()
        End If
        SpriteBatch.End()

        MyBase.Draw(gameTime)
    End Sub
    
    ' Render method to iterate over the batches and draw them.
    Private Sub RenderBatches()
        SyncLock BatchLock
            For Each batch In Batches.Values
                For Each renderCommand In batch.Commands.ToArray()
                    Select Case renderCommand.Type
                        Case RenderType.Texture
                            If batch.Texture IsNot Nothing Then
                                SpriteBatch.Draw(batch.Texture, renderCommand.dRect, renderCommand.sRect, renderCommand.Color)
                            End If

                        Case RenderType.Font
                            If batch.Font IsNot Nothing Then
                                SpriteBatch.DrawString(batch.Font, renderCommand.Text, New Vector2(renderCommand.X + 1, renderCommand.Y + 1), renderCommand.Color2,
                                                       0.0F, Vector2.Zero, 10 / 16.0F, SpriteEffects.None, 0.0F)
                                SpriteBatch.DrawString(batch.Font, renderCommand.Text, New Vector2(renderCommand.X, renderCommand.Y), renderCommand.Color,
                                                       0.0F, Vector2.Zero, 10 / 16.0F, SpriteEffects.None, 0.0F)
                            End If
                    End Select
                Next
            Next
        End SyncLock
    End Sub
    
    Protected Overrides Sub Update(gameTime As GameTime)
        ' Ignore input if the window is minimized or inactive
        If Not IsActive OrElse Window.ClientBounds.Width = 0 Or Window.ClientBounds.Height = 0 Then
            ResetInputStates()
            MyBase.Update(gameTime)
            Return
        End If

        UpdateMouseCache()
        UpdateKeyCache()
        ProcessInputs()

        If IsKeyStateActive(Keys.F12)
            TakeScreenshot()
        End If
        
        SetFps(_gameFps + 1)
        elapsedTime += gameTime.ElapsedGameTime
        
        If elapsedTime.TotalSeconds >= 1 Then
            Console.WriteLine("FPS: " & GetFps())    
            SetFps(0)
            elapsedTime = TimeSpan.Zero
        End If

        MyBase.Update(gameTime)
    End Sub
    
    ' Reset keyboard and mouse states
    Private Sub ResetInputStates()
        SyncLock InputLock
            CurrentKeyboardState = New KeyboardState()
            PreviousKeyboardState = New KeyboardState()
            CurrentMouseState = New MouseState()
            PreviousMouseState = New MouseState()
        End SyncLock
    End Sub

    Private Shared Sub UpdateKeyCache()
        SyncLock InputLock
            ' Get the current keyboard state
            Dim keyboardState As KeyboardState = Keyboard.GetState()

            ' Update the previous and current states
            PreviousKeyboardState = currentKeyboardState
            CurrentKeyboardState = keyboardState
        End SyncLock
    End Sub
    
    Private Shared Sub UpdateMouseCache()
        SyncLock InputLock
            ' Get the current mouse state
            Dim mouseState As MouseState = Mouse.GetState()

            ' Update the previous and current states
            PreviousMouseState = CurrentMouseState
            CurrentMouseState = mouseState
        End SyncLock
    End Sub
    
    Public Shared Function GetMouseScrollDelta() As Integer
        SyncLock ScrollLock
            ' Calculate the scroll delta between the previous and current states
            Return CurrentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue
        End SyncLock
    End Function
    
    Public Shared Function IsKeyStateActive(key As Keys) As Boolean
        SyncLock InputLock
            If CanProcessKey(key) = True Then
                ' Check if the key is down in the current keyboard state
                Return CurrentKeyboardState.IsKeyDown(key)
            End If
        End SyncLock
    End Function

    Public Shared Function GetMousePosition() As Tuple(Of Integer, Integer)
        SyncLock InputLock
            ' Return the current mouse position as a Tuple
            Return New Tuple(Of Integer, Integer)(CurrentMouseState.X, CurrentMouseState.Y)
        End SyncLock
    End Function

    Public Shared Function IsMouseButtonDown(button As MouseButton) As Boolean
        SyncLock InputLock
            Select Case button
                Case MouseButton.Left
                    Return CurrentMouseState.LeftButton = ButtonState.Pressed
                Case MouseButton.Right
                    Return CurrentMouseState.RightButton = ButtonState.Pressed
                Case MouseButton.Middle
                    Return CurrentMouseState.MiddleButton = ButtonState.Pressed
                Case Else
                    Return False
            End Select
        End SyncLock
    End Function
    
    Public Shared Function IsMouseButtonUp(button As MouseButton) As Boolean
        SyncLock InputLock
            Select Case button
                Case MouseButton.Left
                    Return CurrentMouseState.LeftButton = ButtonState.Released
                Case MouseButton.Right
                    Return CurrentMouseState.RightButton = ButtonState.Released
                Case MouseButton.Middle
                    Return CurrentMouseState.MiddleButton = ButtonState.Released
                Case Else
                    Return False
            End Select
        End SyncLock
    End Function
    
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

            ' Check for action keys
            GameState.VbKeyControl = GameClient.CurrentKeyboardState.IsKeyDown(Keys.LeftControl)
            GameState.VbKeyShift = GameClient.CurrentKeyboardState.IsKeyDown(Keys.LeftShift)

            ' Handle Escape key to toggle menus
            If GameClient.IsKeyStateActive(Keys.Escape) Then
                If GameState.InMenu = True Then Exit Sub

                ' Hide options screen
                If Gui.Windows(Gui.GetWindowIndex("winOptions")).Visible = True Then
                    Gui.HideWindow(Gui.GetWindowIndex("winOptions"))
                    Gui.CloseComboMenu()
                    Exit Sub
                End If

                ' hide/show chat window
                If Gui.Windows(Gui.GetWindowIndex("winChat")).Visible = True Then
                    Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).Text = ""
                    HideChat()
                    Exit Sub
                End If

                If Gui.Windows(Gui.GetWindowIndex("winEscMenu")).Visible = True Then
                    Gui.HideWindow(Gui.GetWindowIndex("winEscMenu"))
                    Exit Sub
                Else
                    ' show them
                    If Gui.Windows(Gui.GetWindowIndex("winChat")).Visible = False Then
                        Gui.ShowWindow(Gui.GetWindowIndex("winEscMenu"), True)
                        Exit Sub
                    End If
                End If
            End If

            If GameClient.CurrentKeyboardState.IsKeyDown(Keys.Space) Then
                CheckMapGetItem()
            End If

            If GameClient.CurrentKeyboardState.IsKeyDown(Keys.Insert) Then
                SendRequestAdmin()
            End If

            HandleMouseInputs()
            HandleActiveWindowInput()
            HandleTextInput()

            If GameState.InGame = True Then
                ' Check for movement keys
                GameState.DirUp = GameClient.CurrentKeyboardState.IsKeyDown(Keys.W) Or GameClient.CurrentKeyboardState.IsKeyDown(Keys.Up)
                GameState.DirDown = GameClient.CurrentKeyboardState.IsKeyDown(Keys.S) Or GameClient.CurrentKeyboardState.IsKeyDown(Keys.Down)
                GameState.DirLeft = GameClient.CurrentKeyboardState.IsKeyDown(Keys.A) Or GameClient.CurrentKeyboardState.IsKeyDown(Keys.Left)
                GameState.DirRight = GameClient.CurrentKeyboardState.IsKeyDown(Keys.D) Or GameClient.CurrentKeyboardState.IsKeyDown(Keys.Right)

                HandleHotbarInput()

                If Gui.Windows(Gui.GetWindowIndex("winEscMenu")).Visible = True Then Exit Sub

                If GameClient.CurrentKeyboardState.IsKeyDown(Keys.I) Then
                    ' hide/show inventory
                    If Not Gui.Windows(Gui.GetWindowIndex("winChat")).Visible = True Then Gui.btnMenu_Inv()
                End If

                If GameClient.CurrentKeyboardState.IsKeyDown(Keys.C) Then
                    ' hide/show char
                    If Not Gui.Windows(Gui.GetWindowIndex("winChat")).Visible = True Then Gui.btnMenu_Char()
                End If

                If GameClient.CurrentKeyboardState.IsKeyDown(Keys.K) Then
                    ' hide/show skills
                    If Not Gui.Windows(Gui.GetWindowIndex("winChat")).Visible = True Then Gui.btnMenu_Skills()
                End If

                If GameClient.CurrentKeyboardState.IsKeyDown(Keys.Enter) Then
                    If Gui.Windows(Gui.GetWindowIndex("winChatSmall")).Visible = True Then
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
            If Gui.ActiveWindow > 0 AndAlso Gui.Windows(Gui.ActiveWindow).Visible Then
                ' Check if an active control exists.
                If Gui.Windows(Gui.ActiveWindow).ActiveControl > 0 Then
                    ' Get the active control.
                    Dim activeControl = Gui.Windows(Gui.ActiveWindow).Controls(Gui.Windows(Gui.ActiveWindow).ActiveControl)

                    ' Check if the Enter key is active and can be processed.
                    If GameClient.IsKeyStateActive(Keys.Enter) Then
                        ' Handle Enter: Call the control's callback or activate a new control.
                        If activeControl.CallBack(EntState.Enter) IsNot Nothing Then
                            activeControl.CallBack(EntState.Enter).Invoke()
                        Else
                            ' If no callback, activate a new control.
                            If Gui.ActivateControl() = 0 Then
                                Gui.ActivateControl(0, False)
                            End If
                        End If
                    End If

                    ' Check if the Tab key is active and can be processed
                    If GameClient.IsKeyStateActive(Keys.Tab) Then
                        ' Handle Tab: Switch to the next control.
                        If Gui.ActivateControl() = 0 Then
                            Gui.ActivateControl(0, False)
                        End If
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
                If GameClient.CurrentKeyboardState.IsKeyDown(Keys.D0 + i) Then
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
                If GameClient.IsKeyStateActive(key) Then
                    ' Handle Backspace key separately
                    If key = Keys.Back Then
                        Dim activeControl = Gui.GetActiveControl()

                        If activeControl IsNot Nothing AndAlso Not activeControl.Locked AndAlso activeControl.Text.Length > 0 Then
                            ' Modify the text and update it back in the window
                            activeControl.Text = activeControl.Text.Substring(0, activeControl.Text.Length - 1)
                            Gui.UpdateActiveControl(activeControl)
                        End If
                        Continue For ' Move to the next key
                    End If

                    ' Convert key to a character, considering Shift key
                    Dim character As Nullable(Of Char) = ConvertKeyToChar(key, GameClient.CurrentKeyboardState.IsKeyDown(Keys.LeftShift))

                    ' If the character is valid, update the active control's text
                    If character.HasValue Then
                        Dim activeControl = Gui.GetActiveControl()

                        If activeControl IsNot Nothing AndAlso Not activeControl.Locked AndAlso activeControl.Enabled Then
                            ' Append character to the control's text
                            activeControl.Text &= character.Value
                            Gui.UpdateActiveControl(activeControl)
                            Continue For ' Move to the next key
                        End If
                    End If

                    KeyStates.Remove(key)
                    KeyRepeatTimers.Remove(key)
                End If
            Next
        End SyncLock
    End Sub

    ' Check if the key can be processed (with interval-based repeat logic)
    Private Shared Function CanProcessKey(key As Keys) As Boolean
        SyncLock InputLock
            Dim now = DateTime.Now
            If CurrentKeyboardState.IsKeyDown(key) Then
                If Not KeyRepeatTimers.ContainsKey(key) OrElse (now - KeyRepeatTimers(key)).TotalMilliseconds >= KeyRepeatInterval Then
                    ' If the key is released, remove it from KeyStates and reset the timer
                    KeyStates.Remove(key)
                    KeyRepeatTimers.Remove(key)
                    KeyRepeatTimers(key) = now ' Update the timer for the key
                    Return True
                End If
            End If
            Return False
        End SyncLock
    End Function

    ' Convert a key to a character (if possible)
    Private Shared Function ConvertKeyToChar(key As Keys, shiftPressed As Boolean) As Char
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

        ' Handle the "/" character (typically mapped to OemQuestion)
        If key = Keys.OemQuestion Then
            Return If(shiftPressed, "?"c, "/"c)
        End If

        ' Ignore unsupported keys (e.g., function keys, control keys)
        Return Nothing
    End Function

    Private Shared Sub HandleMouseInputs()
        HandleMouseClick(MouseButton.Left)
        HandleScrollWheel()
    End Sub
    
    Private Shared Sub HandleScrollWheel()
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

    Private Shared _leftMouseHandled As Boolean = False

    Private Shared Sub HandleMouseClick(button As MouseButton)
        SyncLock GameClient.InputLock
            Dim currentTime As Integer = Environment.TickCount

            ' Handle MouseMove event when the mouse moves
            If GameClient.CurrentMouseState.X <> GameClient.PreviousMouseState.X OrElse
           GameClient.CurrentMouseState.Y <> GameClient.PreviousMouseState.Y Then
                Gui.HandleInterfaceEvents(EntState.MouseMove)
            End If

            ' Check for MouseDown event (button pressed)
            If GameClient.IsMouseButtonDown(button) Then
                If Not _leftMouseHandled Then
                    Gui.HandleInterfaceEvents(EntState.MouseDown)
                    GameState.LastLeftClickTime = currentTime ' Track time for double-click detection
                    _leftMouseHandled = True ' Set flag to indicate the left mouse button has been processed
                End If
            Else
                _leftMouseHandled = False ' Reset flag when the button is released
            End If

            ' Check for MouseUp event (button released)
            If Not GameClient.IsMouseButtonUp(button) Then
                Gui.HandleInterfaceEvents(EntState.MouseUp)
            End If

            ' Double-click detection for left button
            If GameClient.IsMouseButtonDown(button) AndAlso
           currentTime - GameState.LastLeftClickTime <= GameState.DoubleClickTImer Then
                Gui.HandleInterfaceEvents(EntState.DblClick)
                GameState.LastLeftClickTime = 0 ' Reset double-click timer
            End If

            ' In-game interactions for left click
            If GameState.InGame = True Then
                If GameClient.IsMouseButtonDown(button) AndAlso Not _leftMouseHandled Then
                    If PetAlive(GameState.MyIndex) AndAlso IsInBounds() Then
                        PetMove(GameState.CurX, GameState.CurY)
                    End If
                    CheckAttack(True)
                    PlayerSearch(GameState.CurX, GameState.CurY, 0)
                    _leftMouseHandled = True ' Set flag after handling in-game interactions
                End If

                ' Right-click interactions
                If GameClient.IsMouseButtonDown(MouseButton.Right) And GameState.InGame = True Then
                    If GameState.VbKeyShift = True Then
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

    Private Shared Sub HandleRightClickMenu()
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
    
    Private Sub OnWindowClose(ByVal sender As Object, ByVal e As EventArgs)
        DestroyGame()
        End
    End Sub
    
    Private Sub OnDeviceReset()
        Console.WriteLine("Device Reset")
    End Sub

    Public Sub TakeScreenshot()
        ' Set the render target to our RenderTarget2D
        GameClient.Graphics.GraphicsDevice.SetRenderTarget(GameClient.RenderTarget)

        ' Clear the render target with a transparent background
        GameClient.Graphics.GraphicsDevice.Clear(Color.Transparent)

        ' Draw everything to the render target
        Draw(New GameTime()) ' Assuming Draw handles your game rendering

        ' Reset the render target to the back buffer (main display)
        GameClient.Graphics.GraphicsDevice.SetRenderTarget(Nothing)

        ' Save the contents of the RenderTarget2D to a PNG file
        Dim timestamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
        Using stream As New FileStream($"screenshot_{timestamp}.png", FileMode.Create)
            GameClient.RenderTarget.SaveAsPng(stream, 
                                              GameClient.RenderTarget.Width, 
                                              GameClient.RenderTarget.Height)
        End Using
    End Sub
    
    ' Draw a filled rectangle with an optional outline
    Public Shared Sub DrawRectangle(position As Vector2, size As Vector2, fillColor As Color, outlineColor As Color, outlineThickness As Single)
        ' Create a 1x1 white texture for drawing
        Dim whiteTexture As New Texture2D(SpriteBatch.GraphicsDevice, 1, 1)

        whiteTexture.SetData(New Color() {Color.White})

        ' Draw the filled rectangle
        SpriteBatch.Draw(whiteTexture, New Rectangle(position.ToPoint(), size.ToPoint()), fillColor)

        '        Draw the outline if thickness > 0
        If outlineThickness > 0 Then
            ' Create the four sides of the outline
            Dim left As New Rectangle(position.ToPoint(), New Point(CInt(outlineThickness), CInt(size.Y)))
            Dim top As New Rectangle(position.ToPoint(), New Point(CInt(size.X), CInt(outlineThickness)))
            Dim right As New Rectangle(New Point(CInt(position.X + size.X - outlineThickness), CInt(position.Y)), New Point(CInt(outlineThickness), CInt(size.Y)))
            Dim bottom As New Rectangle(New Point(CInt(position.X), CInt(position.Y + size.Y - outlineThickness)), New Point(CInt(size.X), CInt(outlineThickness)))

            ' Draw the outline rectangles
            SpriteBatch.Draw(whiteTexture, left, outlineColor)
            SpriteBatch.Draw(whiteTexture, top, outlineColor)
            SpriteBatch.Draw(whiteTexture, right, outlineColor)
            SpriteBatch.Draw(whiteTexture, bottom, outlineColor)
        End If

        ' Dispose the texture to free memory
        whiteTexture.Dispose()
    End Sub

    ''' <summary>
    ''' Draws a rectangle with a fill color and an outline.
    ''' </summary>
    ''' <param name="rect">The Rectangle to be drawn.</param>
    ''' <param name="fillColor">The color to fill the rectangle.</param>
    ''' <param name="outlineColor">The color of the outline.</param>
    ''' <param name="outlineThickness">The thickness of the outline.</param>
    Public Shared Sub DrawRectangleWithOutline(
        rect As Rectangle,
        fillColor As Color,
        outlineColor As Color,
        outlineThickness As Single)

        ' Create a 1x1 white texture
        Dim whiteTexture As New Texture2D(SpriteBatch.GraphicsDevice, 1, 1)
        whiteTexture.SetData(New Color() {Color.White})

        ' Draw the filled rectangle
        SpriteBatch.Draw(whiteTexture, rect, fillColor)

        ' Draw the outline if thickness > 0
        If outlineThickness > 0 Then
            ' Define outline rectangles (left, top, right, bottom)
            Dim left As New Rectangle(rect.Left, rect.Top, CInt(outlineThickness), rect.Height)
            Dim top As New Rectangle(rect.Left, rect.Top, rect.Width, CInt(outlineThickness))
            Dim right As New Rectangle(rect.Right - CInt(outlineThickness), rect.Top, CInt(outlineThickness), rect.Height)
            Dim bottom As New Rectangle(rect.Left, rect.Bottom - CInt(outlineThickness), rect.Width, CInt(outlineThickness))

            ' Draw the outline rectangles
            SpriteBatch.Draw(whiteTexture, left, outlineColor)
            SpriteBatch.Draw(whiteTexture, top, outlineColor)
            SpriteBatch.Draw(whiteTexture, right, outlineColor)
            SpriteBatch.Draw(whiteTexture, bottom, outlineColor)
        End If

        ' Dispose the texture after use
        whiteTexture.Dispose()
    End Sub

    Public Shared Sub DrawSelectionRectangle()
        Dim selectionRect As New Rectangle(
            GameState.EditorTileSelStart.X * GameState.PicX, GameState.EditorTileSelStart.Y * GameState.PicY,
            GameState.EditorTileWidth * GameState.PicX, GameState.EditorTileHeight * GameState.PicY
        )

        ' Begin the sprite batch and draw a semi-transparent overlay (optional)
        SpriteBatch.Begin()
        SpriteBatch.Draw(TilesetWindow, selectionRect, Color.Red * 0.4F)
        SpriteBatch.End()
    End Sub

    Private Sub DrawOutlineRectangle(x As Integer, y As Integer, width As Integer, height As Integer, color As Color, thickness As Single)
        Dim whiteTexture As New Texture2D(SpriteBatch.GraphicsDevice, 1, 1)

        SpriteBatch.Begin()

        ' Define four rectangles for the outline
        Dim left As New Rectangle(x, y, thickness, height)
        Dim top As New Rectangle(x, y, width, thickness)
        Dim right As New Rectangle(x + width - thickness, y, thickness, height)
        Dim bottom As New Rectangle(x, y + height - thickness, width, thickness)

        ' Draw the outline
        SpriteBatch.Draw(whiteTexture, left, color)
        SpriteBatch.Draw(whiteTexture, top, color)
        SpriteBatch.Draw(whiteTexture, right, color)
        SpriteBatch.Draw(whiteTexture, bottom, color)
        SpriteBatch.End()
    End Sub

    Public Shared Function QbColorToXnaColor(qbColor As Integer) As Color
        Select Case qbColor
            Case ColorType.Black
                Return Color.Black
            Case ColorType.Blue
                Return Color.Blue
            Case ColorType.Green
                Return Color.Green
            Case ColorType.Cyan
                Return Color.Cyan
            Case ColorType.Red
                Return Color.Red
            Case ColorType.Magenta
                Return Color.Magenta
            Case ColorType.Brown
                Return Color.Brown
            Case ColorType.Gray
                Return Color.LightGray
            Case ColorType.DarkGray
                Return Color.Gray
            Case ColorType.BrightBlue
                Return Color.LightBlue
            Case ColorType.BrightGreen
                Return Color.LightGreen
            Case ColorType.BrightCyan
                Return Color.LightCyan
            Case ColorType.BrightRed
                Return Color.LightCoral
            Case ColorType.Pink
                Return Color.Orchid
            Case ColorType.Yellow
                Return Color.Yellow
            Case ColorType.White
                Return Color.White
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(qbColor), "Invalid QbColor value.")
        End Select
    End Function
    
    Friend Shared Sub DrawEmote(x2 As Integer, y2 As Integer, sprite As Integer)
        Dim rec As Rectangle
        Dim x As Integer, y As Integer, anim As Integer

        If sprite < 1 Or sprite > GameState.NumEmotes Then Exit Sub
        If GameState.ShowAnimLayers = 1 Then
            anim = 1
        Else
            anim = 0
        End If

        With rec
            .Y = 0
            .Height = GameState.PicX
            .X = anim * (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Emotes, sprite)).Width / 2)
            .Width = (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Emotes, sprite)).Width / 2)
        End With

        x = ConvertMapX(x2)
        y = ConvertMapY(y2) - (GameState.PicY + 16)

        RenderTexture(System.IO.Path.Combine(Core.Path.Emotes, sprite), x, y, rec.X, rec.Y, rec.Width, rec.Height)
    End Sub

    Friend Sub DrawDirections(x As Integer, y As Integer)
        Dim rec As Rectangle, i As Integer

        ' render grid
        rec.Y = 24
        rec.X = 0
        rec.Width = 32
        rec.Height = 32

        RenderTexture(IO.Path.Combine(Core.Path.Misc, "Direction"), ConvertMapX(x * GameState.PicX), ConvertMapY(y * GameState.PicY), rec.X, rec.Y, rec.Width,
                     rec.Height, rec.Width, rec.Height)

        ' render dir blobs
        For i = 1 To 4
            rec.X = (i - 1) * 8
            rec.Width = 8

            ' find out whether render blocked or not
            If Not IsDirBlocked(MyMap.Tile(x, y).DirBlock, CByte(i)) Then
                rec.Y = 8
            Else
                rec.Y = 16
            End If
            rec.Height = 8

            RenderTexture(IO.Path.Combine(Core.Path.Misc, "Direction"), ConvertMapX(x * GameState.PicX) + GameState.DirArrowX(i), ConvertMapY(y * GameState.PicY) + GameState.DirArrowY(i), rec.X, rec.Y, rec.Width, rec.Height, rec.Width, rec.Height)
        Next
    End Sub

    Friend Shared Sub DrawPaperdoll(x2 As Integer, y2 As Integer, sprite As Integer, anim As Integer, spritetop As Integer)
        Dim rec As Rectangle
        Dim x As Integer, y As Integer
        Dim width As Integer, height As Integer

        If sprite < 1 Or sprite > GameState.NumPaperdolls Then Exit Sub

        With rec
            .Y = spritetop * GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Paperdolls, sprite)).Height / 4
            .Height = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Paperdolls, sprite)).Height / 4
            .X = anim * GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Paperdolls, sprite)).Width / 4
            .Width = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Paperdolls, sprite)).Width / 4
        End With

        x = ConvertMapX(x2)
        y = ConvertMapY(y2)
        width = (rec.Right - rec.Left)
        height = (rec.Bottom - rec.Top)

        RenderTexture(System.IO.Path.Combine(Core.Path.Paperdolls, sprite), x, y, rec.X, rec.Y, rec.Width, rec.Height)
    End Sub

    Friend Shared Sub DrawNPC(MapNpcNum As Integer)
        Dim anim As Byte
        Dim x As Integer
        Dim y As Integer
        Dim sprite As Integer, spriteLeft As Integer
        Dim rect As Rectangle
        Dim attackSpeed As Integer = 1000

        ' Check if NPC exists
        If MyMapNPC(MapNpcNum).Num = 0 Then Exit Sub

        ' Ensure NPC is within the tile view range
        If MyMapNPC(MapNpcNum).X < GameState.TileView.Left Or MyMapNPC(MapNpcNum).X > GameState.TileView.Right Then Exit Sub
        If MyMapNPC(MapNpcNum).Y < GameState.TileView.Top Or MyMapNPC(MapNpcNum).Y > GameState.TileView.Bottom Then Exit Sub

        ' Stream NPC if not yet loaded
        StreamNpc(MyMapNPC(MapNpcNum).Num)

        ' Get the sprite of the NPC
        sprite = Type.NPC(MyMapNPC(MapNpcNum).Num).Sprite

        ' Validate sprite
        If sprite < 1 Or sprite > GameState.NumCharacters Then Exit Sub

        ' Reset animation frame
        anim = 0

        ' Check for attacking animation
        If MyMapNPC(MapNpcNum).AttackTimer + (attackSpeed / 2) > GetTickCount() AndAlso MyMapNPC(MapNpcNum).Attacking = 1 Then
            anim = 3
        Else
            ' Walking animation based on direction
            Select Case MyMapNPC(MapNpcNum).Dir
                Case DirectionType.Up
                    If MyMapNPC(MapNpcNum).YOffset > 8 Then anim = MyMapNPC(MapNpcNum).Steps
                Case DirectionType.Down
                    If MyMapNPC(MapNpcNum).YOffset < -8 Then anim = MyMapNPC(MapNpcNum).Steps
                Case DirectionType.Left
                    If MyMapNPC(MapNpcNum).XOffset > 8 Then anim = MyMapNPC(MapNpcNum).Steps
                Case DirectionType.Right
                    If MyMapNPC(MapNpcNum).XOffset < -8 Then anim = MyMapNPC(MapNpcNum).Steps
            End Select
        End If

        ' Reset attacking state if attack timer has passed
        With MyMapNPC(MapNpcNum)
            If .AttackTimer + attackSpeed < GetTickCount() Then
                .Attacking = 0
                .AttackTimer = 0
            End If
        End With

        ' Set sprite sheet position based on direction
        Select Case MyMapNPC(MapNpcNum).Dir
            Case DirectionType.Up
                spriteLeft = 3
            Case DirectionType.Right
                spriteLeft = 2
            Case DirectionType.Down
                spriteLeft = 0
            Case DirectionType.Left
                spriteLeft = 1
        End Select

        ' Create the rectangle for rendering the sprite
        rect = New Rectangle(anim * (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, sprite)).Width / 4), spriteLeft * (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, sprite)).Height / 4),
                              GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, sprite)).Width / 4, GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, sprite)).Height / 4)

        ' Calculate X and Y coordinates for rendering
        x = MyMapNPC(MapNpcNum).X * GameState.PicX + MyMapNPC(MapNpcNum).XOffset - ((GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, sprite)).Width / 4 - 32) / 2)

        If GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, sprite)).Height / 4 > 32 Then
            ' Larger sprites need an offset for height adjustment
            y = MyMapNPC(MapNpcNum).Y * GameState.PicY + MyMapNPC(MapNpcNum).YOffset - (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, sprite)).Height / 4 - 32)
        Else
            ' Normal sprite height
            y = MyMapNPC(MapNpcNum).Y * GameState.PicY + MyMapNPC(MapNpcNum).YOffset
        End If

        ' Draw shadow and NPC sprite
        'DrawShadow(x, y + 16)
        DrawCharacterSprite(sprite, x, y, rect)
    End Sub

    Friend Shared Sub DrawMapItem(itemNum As Integer)
        Dim srcrec As Rectangle, destrec As Rectangle
        Dim picNum As Integer
        Dim x As Integer, y As Integer
        StreamItem(MyMapItem(itemNum).Num)

        picNum = Type.Item(MyMapItem(itemNum).Num).Icon

        If picNum < 1 Or picNum > GameState.NumItems Then Exit Sub

        With MyMapItem(itemNum)
            If .X < GameState.TileView.Left Or .X > GameState.TileView.Right Then Exit Sub
            If .Y < GameState.TileView.Top Or .Y > GameState.TileView.Bottom Then Exit Sub
        End With

        srcrec = New Rectangle(0, 0, GameState.PicX, GameState.PicY)
        destrec = New Rectangle(ConvertMapX(MyMapItem(itemNum).X * GameState.PicX), ConvertMapY(MyMapItem(itemNum).Y * GameState.PicY), GameState.PicX, GameState.PicY)

        x = ConvertMapX(MyMapItem(itemNum).X * GameState.PicX)
        y = ConvertMapY(MyMapItem(itemNum).Y * GameState.PicY)

        RenderTexture(IO.Path.Combine(Core.Path.Items, picNum), x, y, srcrec.X, srcrec.Y, srcrec.Width, srcrec.Height, srcrec.Width, srcrec.Height)
    End Sub

    Friend Shared Sub DrawCharacterSprite(sprite As Integer, x2 As Integer, y2 As Integer, sRECT As Rectangle)
        Dim x As Integer
        Dim y As Integer

        If sprite < 1 Or sprite > GameState.NumCharacters Then Exit Sub

        x = ConvertMapX(x2)
        y = ConvertMapY(y2)

        RenderTexture(IO.Path.Combine(Core.Path.Characters, sprite), x, y, sRECT.X, sRECT.Y, sRECT.Width, sRECT.Height, sRECT.Width, sRECT.Height)
    End Sub
    
    Friend Shared Sub DrawBlood(index As Integer)
        Dim srcrec As Rectangle
        Dim destrec As Rectangle
        Dim x As Integer
        Dim y As Integer

        With Blood(index)
            If .X < GameState.TileView.Left Or .X > GameState.TileView.Right Then Exit Sub
            If .Y < GameState.TileView.Top Or .Y > GameState.TileView.Bottom Then Exit Sub

            ' check if we should be seeing it
            If .Timer + 20000 < GetTickCount() Then Exit Sub

            x = ConvertMapX(Blood(index).X * GameState.PicX)
            y = ConvertMapY(Blood(index).Y * GameState.PicY)

            srcrec = New Rectangle((.Sprite - 1) * GameState.PicX, 0, GameState.PicX, GameState.PicY)
            destrec = New Rectangle(ConvertMapX(.X * GameState.PicX), ConvertMapY(.Y * GameState.PicY), GameState.PicX, GameState.PicY)

            GameClient.RenderTexture(IO.Path.Combine(Core.Path.Misc, "Blood"), x, y, srcrec.X, srcrec.Y, srcrec.Width, srcrec.Height)

        End With
    End Sub

    Public Shared Sub DrawBars()
        Dim Left As Long, Top As Long, Width As Long, Height As Long
        Dim tmpX As Long, tmpY As Long, barWidth As Long, i As Long, NpcNum As Long

        ' dynamic bar calculations
        Width = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Bars")).Width
        Height = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Bars")).Height / 4

        ' render npc health bars
        For i = 1 To MAX_MAP_NPCS
            NpcNum = Type.MyMapNPC(i).Num
            ' exists?
            If NpcNum > 0 Then
                ' alive?
                If Type.MyMapNPC(i).Vital(VitalType.HP) > 0 And Type.MyMapNPC(i).Vital(VitalType.HP) < Type.NPC(NpcNum).HP Then
                    ' lock to npc
                    tmpX = Type.MyMapNPC(i).X * GameState.PicX + Type.MyMapNPC(i).XOffset + 16 - (Width / 2)
                    tmpY = Type.MyMapNPC(i).Y * GameState.PicY + Type.MyMapNPC(i).YOffset + 35

                    ' calculate the width to fill
                    If Width > 0 Then GameState.BarWidth_NpcHP_Max(i) = ((Type.MyMapNPC(i).Vital(VitalType.HP) / Width) / (Type.NPC(NpcNum).HP / Width)) * Width

                    ' draw bar background
                    Top = Height * 3 ' HP bar background
                    Left = 0
                    GameClient.RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, Width, Height, Width, Height)

                    ' draw the bar proper
                    Top = 0 ' HP bar
                    Left = 0
                    RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, GameState.BarWidth_NpcHP(i), Height, GameState.BarWidth_NpcHP(i), Height)
                End If
            End If
        Next

        For i = 1 To MAX_PLAYERS
            If GetPlayerMap(i) = GetPlayerMap(i) Then
                If GetPlayerVital(i, VitalType.HP) > 0 And GetPlayerVital(i, VitalType.HP) < GetPlayerMaxVital(i, VitalType.HP) Then
                    ' lock to Player
                    tmpX = GetPlayerX(i) * GameState.PicX + Type.Player(i).XOffset + 16 - (Width / 2)
                    tmpY = GetPlayerY(i) * GameState.PicY + Type.Player(i).YOffset + 35

                    ' calculate the width to fill
                    If Width > 0 Then GameState.BarWidth_PlayerHP_Max(i) = ((GetPlayerVital(i, VitalType.HP) / Width) / (GetPlayerMaxVital(i, VitalType.HP) / Width)) * Width

                    ' draw bar background
                    Top = Height * 3 ' HP bar background
                    Left = 0
                    GameClient.RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, Width, Height, Width, Height)

                    ' draw the bar proper
                    Top = 0 ' HP bar
                    Left = 0
                    GameClient.RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, GameState.BarWidth_PlayerHP(i), Height, GameState.BarWidth_PlayerHP(i), Height)
                End If

                If GetPlayerVital(i, VitalType.SP) > 0 And GetPlayerVital(i, VitalType.SP) < GetPlayerMaxVital(i, VitalType.SP) Then
                    ' lock to Player
                    tmpX = GetPlayerX(i) * GameState.PicX + Type.Player(i).XOffset + 16 - (Width / 2)
                    tmpY = GetPlayerY(i) * GameState.PicY + Type.Player(i).YOffset + 35 + Height

                    ' calculate the width to fill
                    If Width > 0 Then GameState.BarWidth_PlayerSP_Max(i) = ((GetPlayerVital(i, VitalType.SP) / Width) / (GetPlayerMaxVital(i, VitalType.SP) / Width)) * Width

                    ' draw bar background
                    Top = Height * 3 ' SP bar background
                    Left = 0
                    RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, Width, Height, Width, Height)

                    ' draw the bar proper
                    Top = Height * 1 ' SP bar
                    Left = 0
                    RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, GameState.BarWidth_PlayerSP(i), Height, GameState.BarWidth_PlayerSP(i), Height)
                End If

                If GameState.SkillBuffer > 0 Then
                    If Type.Skill(Type.Player(i).Skill(GameState.SkillBuffer).Num).CastTime > 0 Then
                        ' lock to player
                        tmpX = GetPlayerX(i) * GameState.PicX + Type.Player(i).XOffset + 16 - (Width / 2)
                        tmpY = GetPlayerY(i) * GameState.PicY + Type.Player(i).YOffset + 35 + Height

                        ' calculate the width to fill
                        If Width > 0 Then barWidth = (GetTickCount() - GameState.SkillBufferTimer) / ((Type.Skill(Type.Player(i).Skill(GameState.SkillBuffer).Num).CastTime * 1000)) * Width

                        ' draw bar background
                        Top = Height * 3 ' cooldown bar background
                        Left = 0
                        RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, Width, Height, Width, Height)

                        ' draw the bar proper
                        Top = Height * 2 ' cooldown bar
                        Left = 0
                        RenderTexture(IO.Path.Combine(Core.Path.Misc, "Bars"), ConvertMapX(tmpX), ConvertMapY(tmpY), Left, Top, barWidth, Height, barWidth, Height)
                    End If
                End If
            End If
        Next
    End Sub

    Friend Sub DrawEyeDropper()
        SpriteBatch.Begin()

        ' Define rectangle parameters.
        Dim position As New Vector2(ConvertMapX(GameState.CurX * GameState.PicX), ConvertMapY(GameState.CurY * GameState.PicY))
        Dim size As New Vector2(GameState.PicX, GameState.PicX)
        Dim fillColor As Color = Color.Transparent  ' No fill
        Dim outlineColor As Color = Color.Cyan      ' Cyan outline
        Dim outlineThickness As Integer = 1         ' Thickness of outline

        ' Draw the rectangle with an outline.
        DrawRectangle(position, size, fillColor, outlineColor, outlineThickness)
        SpriteBatch.End()
    End Sub

    Friend Shared Sub DrawGrid()
        ' Use a single Begin/End pair to improve performance
        SpriteBatch.Begin()

        ' Iterate over the tiles in the visible range
        For x = GameState.TileView.Left - 1 To GameState.TileView.Right + 1
            For y = GameState.TileView.Top - 1 To GameState.TileView.Bottom + 1
                If IsValidMapPoint(x, y) Then
                    ' Calculate the tile position and size
                    Dim posX As Integer = ConvertMapX((x - 1) * GameState.PicX)
                    Dim posY As Integer = ConvertMapY((y - 1) * GameState.PicY)
                    Dim rectWidth As Integer = GameState.PicX
                    Dim rectHeight As Integer = GameState.PicY

                    ' Draw the transparent rectangle as the tile background
                    SpriteBatch.Draw(GameClient.TransparentTexture, New Rectangle(posX, posY, rectWidth, rectHeight), Color.Transparent)

                    ' Define the outline color and thickness
                    Dim outlineColor As Color = Color.White
                    Dim thickness As Integer = 1

                    ' Draw the tile outline (top, bottom, left, right)
                    SpriteBatch.Draw(GameClient.TransparentTexture, New Rectangle(posX, posY, rectWidth, thickness), outlineColor) ' Top
                    SpriteBatch.Draw(GameClient.TransparentTexture, New Rectangle(posX, posY + rectHeight - thickness, rectWidth, thickness), outlineColor) ' Bottom
                    SpriteBatch.Draw(GameClient.TransparentTexture, New Rectangle(posX, posY, thickness, rectHeight), outlineColor) ' Left
                    SpriteBatch.Draw(GameClient.TransparentTexture, New Rectangle(posX + rectWidth - thickness, posY, thickness, rectHeight), outlineColor) ' Right
                End If
            Next
        Next

        SpriteBatch.End()
    End Sub
    
    Friend Shared Sub DrawTarget(x2 As Integer, y2 As Integer)
        Dim rec As Rectangle
        Dim x As Integer, y As Integer
        Dim width As Integer, height As Integer

        With rec
            .Y = 0
            .Height = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Target")).Height
            .X = 0
            .Width = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Target")).Width / 2
        End With
        x = ConvertMapX(x2 + 4)
        y = ConvertMapY(y2 - 32)
        width = (rec.Right - rec.Left)
        height = (rec.Bottom - rec.Top)

        RenderTexture(IO.Path.Combine(Core.Path.Misc, "Target"), x, y, rec.X, rec.Y, rec.Width, rec.Height, rec.Width, rec.Height)
    End Sub

    Friend Shared Sub DrawHover(x2 As Integer, y2 As Integer)
        Dim rec As Rectangle
        Dim x As Integer, y As Integer
        Dim width As Integer, height As Integer

        With rec
            .Y = 0
            .Height = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Target")).Height
            .X = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Target")).Width / 2
            .Width = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Target")).Width / 2 + GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Misc, "Target")).Width / 2
        End With

        x = ConvertMapX(x2 + 4)
        y = ConvertMapY(y2 - 32)
        width = (rec.Right - rec.Left)
        height = (rec.Bottom - rec.Top)

        RenderTexture(IO.Path.Combine(Core.Path.Misc, "Target"), x, y, rec.X, rec.Y, rec.Width, rec.Height, rec.Width, rec.Height)
    End Sub
    
    Public Shared Sub DrawChatBubble(ByVal Index As Long)
        Dim theArray() As String, x As Long, y As Long, i As Long, MaxWidth As Long, x2 As Long, y2 As Long, Color As Integer, tmpNum As Long

        With ChatBubble(Index)
            ' exit out early
            If .Target = 0 Then Exit Sub

            Color = .Color

            ' calculate position
            Select Case .TargetType
                Case TargetType.Player
                    ' it's a player
                    If Not GetPlayerMap(.Target) = GetPlayerMap(GameState.MyIndex) Then Exit Sub

                    ' it's on our map - get co-ords
                    x = ConvertMapX((Type.Player(.Target).X * 32) + Type.Player(.Target).XOffset) + 16
                    y = ConvertMapY((Type.Player(.Target).Y * 32) + Type.Player(.Target).YOffset) - 32
                Case TargetType.Event
                    x = ConvertMapX(MyMap.Event(.Target).X * 32) + 16
                    y = ConvertMapY(MyMap.Event(.Target).Y * 32) - 16
                Case Else
                    Exit Sub
            End Select

            ' word wrap
            WordWrap(.Msg, FontType.Georgia, GameState.ChatBubbleWidth, theArray)

            ' find max width
            tmpNum = UBound(theArray)

            For i = 1 To tmpNum
                If GetTextWidth(theArray(i), FontType.Georgia) > MaxWidth Then MaxWidth = GetTextWidth(theArray(i), FontType.Georgia)
            Next

            ' calculate the new position
            x2 = x - (MaxWidth \ 2)
            y2 = y - (UBound(theArray) * 12)

            ' render bubble - top left
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2 - 9, y2 - 5, 0, 0, 9, 5, 9, 5)

            ' top right
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2 + MaxWidth, y2 - 5, 119, 0, 9, 5, 9, 5)

            ' top
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2, y2 - 5, 9, 0, MaxWidth, 5, 5, 5)

            ' bottom left
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2 - 9, y, 0, 19, 9, 6, 9, 6)

            ' bottom right
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2 + MaxWidth, y, 119, 19, 9, 6, 9, 6)

            ' bottom - left half
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2, y, 9, 19, (MaxWidth \ 2) - 5, 6, 6, 6)

            ' bottom - right half
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2 + (MaxWidth \ 2) + 6, y, 9, 19, (MaxWidth \ 2) - 5, 6, 9, 6)

            ' left
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2 - 9, y2, 0, 6, 9, (UBound(theArray) * 12), 9, 6)

            ' right
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2 + MaxWidth, y2, 119, 6, 9, (UBound(theArray) * 12), 9, 6)

            ' center
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x2, y2, 9, 5, MaxWidth, (UBound(theArray) * 12), 9, 5)

            ' little pointy bit
            RenderTexture(IO.Path.Combine(Core.Path.Gui, 33), x - 5, y, 58, 19, 11, 11, 11, 11)

            ' render each line centralized
            tmpNum = UBound(theArray)

            For i = 1 To tmpNum
                RenderText(theArray(i), x - (theArray(i).Length / 2) - (GetTextWidth(theArray(i)) / 2), y2, QbColorToXnaColor(.Color), Microsoft.Xna.Framework.Color.Black)
                y2 = y2 + 12
            Next

            ' check if it's timed out - close it if so
            If .Timer + 5000 < GetTickCount() Then
                .Active = 0
            End If
        End With
    End Sub

    Friend Shared Sub DrawPlayer(index As Integer)
        Dim anim As Byte, x As Integer, y As Integer
        Dim spritenum As Integer, spriteleft As Integer
        Dim attackspeed As Integer
        Dim rect As Rectangle

        spritenum = GetPlayerSprite(index)

        If index < 1 Or index > MAX_PLAYERS Then Exit Sub
        If spritenum <= 0 Or spritenum > GameState.NumCharacters Then Exit Sub

        ' speed from weapon
        If GetPlayerEquipment(index, EquipmentType.Weapon) > 0 Then
            attackspeed = Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Speed
        Else
            attackspeed = 1000
        End If

        ' Reset frame
        anim = 0

        ' Check for attacking animation
        If Type.Player(index).AttackTimer + (attackspeed / 2) > GetTickCount() Then
            If Type.Player(index).Attacking = 1 Then
                anim = 3
            End If
        Else
            ' If not attacking, walk normally
            Select Case GetPlayerDir(index)
                Case DirectionType.Up

                    If (Type.Player(index).YOffset > 8) Then anim = Type.Player(index).Steps
                Case DirectionType.Down

                    If (Type.Player(index).YOffset < -8) Then anim = Type.Player(index).Steps
                Case DirectionType.Left

                    If (Type.Player(index).XOffset > 8) Then anim = Type.Player(index).Steps
                Case DirectionType.Right

                    If (Type.Player(index).XOffset < -8) Then anim = Type.Player(index).Steps
                Case DirectionType.UpRight
                    If (Type.Player(index).XOffset < -8) Then anim = Type.Player(index).Steps
                    If (Type.Player(index).YOffset > 8) Then anim = Type.Player(index).Steps

                Case DirectionType.UpLeft
                    If (Type.Player(index).XOffset > 8) Then anim = Type.Player(index).Steps
                    If (Type.Player(index).YOffset > 8) Then anim = Type.Player(index).Steps

                Case DirectionType.DownRight
                    If (Type.Player(index).XOffset < -8) Then anim = Type.Player(index).Steps
                    If (Type.Player(index).YOffset < -8) Then anim = Type.Player(index).Steps

                Case DirectionType.DownLeft
                    If (Type.Player(index).XOffset > 8) Then anim = Type.Player(index).Steps
                    If (Type.Player(index).YOffset < -8) Then anim = Type.Player(index).Steps

            End Select

        End If

        ' Check to see if we want to stop making him attack
        With Type.Player(index)
            If .AttackTimer + attackspeed < GetTickCount() Then
                .Attacking = 0
                .AttackTimer = 0
            End If

        End With

        ' Set the left
        Select Case GetPlayerDir(index)
            Case DirectionType.Up
                spriteleft = 3
            Case DirectionType.Right
                spriteleft = 2
            Case DirectionType.Down
                spriteleft = 0
            Case DirectionType.Left
                spriteleft = 1
            Case DirectionType.UpRight
                spriteleft = 2
            Case DirectionType.UpLeft
                spriteleft = 1
            Case DirectionType.DownLeft
                spriteleft = 1
            Case DirectionType.DownRight
                spriteleft = 2
        End Select

        ' Calculate the X
        x = Type.Player(index).X * GameState.PicX + Type.Player(index).XOffset - ((GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spritenum)).Width / 4 - 32) / 2)

        ' Is the player's height more than 32..?
        If (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spritenum)).Height) > 32 Then
            ' Create a 32 pixel offset for larger sprites
            y = GetPlayerY(index) * GameState.PicY + Type.Player(index).YOffset - ((GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spritenum)).Height / 4) - 32)
        Else
            ' Proceed as normal
            y = GetPlayerY(index) * GameState.PicY + Type.Player(index).YOffset
        End If

        rect = New Rectangle((anim) * (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spritenum)).Width / 4), spriteleft * (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spritenum)).Height / 4),
                               (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spritenum)).Width / 4), (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spritenum)).Height / 4))

        ' render the actual sprite
        'DrawShadow(x, y + 16)
        DrawCharacterSprite(spritenum, x, y, rect)

        'check for paperdolling
        For i = 1 To EquipmentType.Count - 1
            If GetPlayerEquipment(index, i) > 0 Then
                If Type.Item(GetPlayerEquipment(index, i)).Paperdoll > 0 Then
                    DrawPaperdoll(x, y, Type.Item(GetPlayerEquipment(index, i)).Paperdoll, anim, spriteleft)
                End If
            End If
        Next

        ' Check to see if we want to stop showing emote
        With Type.Player(index)
            If .EmoteTimer < GetTickCount() Then
                .Emote = 0
                .EmoteTimer = 0
            End If
        End With

        'check for emotes
        If Type.Player(GameState.MyIndex).Emote > 0 Then
            DrawEmote(x, y, Type.Player(GameState.MyIndex).Emote)
        End If
    End Sub
    
    Friend Sub DrawEvents()
        If MyMap.EventCount <= 0 Then Exit Sub ' Exit early if no events

        For i = 1 To MyMap.EventCount
            Dim x = ConvertMapX(MyMap.Event(i).X * GameState.PicX)
            Dim y = ConvertMapY(MyMap.Event(i).Y * GameState.PicY)

            ' Skip event if there are no pages
            If MyMap.Event(i).PageCount <= 0 Then
                DrawOutlineRectangle(x, y, GameState.PicX, GameState.PicY, Color.Blue, 0.6F)
                Continue For
            End If

            ' Render event based on its graphic type
            Select Case MyMap.Event(i).Pages(1).GraphicType
                Case 0 ' Text Event
                    Dim tX = x + (GameState.PicX \ 2) - 4
                    Dim tY = y + (GameState.PicY \ 2) - 7
                    RenderText("E", tX, tY, Color.Green, Color.Black)

                Case 1 ' Character Graphic
                    RenderCharacterGraphic(MyMap.Event(i), x, y)

                Case 2 ' Tileset Graphic
                    RenderTilesetGraphic(MyMap.Event(i), x, y)

                Case Else
                    ' Draw fallback outline rectangle if graphic type is unknown
                    DrawOutlineRectangle(x, y, GameState.PicX, GameState.PicY, Color.Blue, 0.6F)
            End Select
        Next
    End Sub

    Public Shared Sub RenderCharacterGraphic(eventData As EventStruct, x As Integer, y As Integer)
        ' Get the graphic index from the event's first page
        Dim gfxIndex As Integer = eventData.Pages(1).Graphic

        ' Validate the graphic index to ensure it�s within range
        If gfxIndex <= 0 OrElse gfxIndex > GameState.NumCharacters Then Exit Sub

        ' Get animation details (frame index and columns) from the event
        Dim frameIndex As Integer = eventData.Pages(1).GraphicX ' Example frame index
        Dim columns As Integer = eventData.Pages(1).GraphicY ' Example column count

        ' Calculate the frame size (assuming square frames for simplicity)
        Dim frameWidth As Integer = GetGfxInfo(IO.Path.Combine(Core.Path.Characters, gfxIndex)).Width \ columns
        Dim frameHeight As Integer = frameWidth ' Adjust if non-square frames

        ' Calculate the source rectangle for the current frame
        Dim column As Integer = frameIndex Mod columns
        Dim row As Integer = frameIndex \ columns
        Dim sourceRect As New Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight)

        ' Define the position on the map where the graphic will be drawn
        Dim position As New Vector2(x, y)

        RenderTexture(IO.Path.Combine(Core.Path.Characters, gfxIndex), position.X, position.Y, sourceRect.X, sourceRect.Y, frameWidth, frameHeight, sourceRect.Width, sourceRect.Height)
    End Sub

    Private Sub RenderTilesetGraphic(eventData As EventStruct, x As Integer, y As Integer)
        Dim gfxIndex = eventData.Pages(1).Graphic

        If gfxIndex > 0 AndAlso gfxIndex <= GameState.NumTileSets Then
            ' Define source rectangle from tileset graphics
            Dim srcRect As New Rectangle(
                eventData.Pages(1).GraphicX * 32,
                eventData.Pages(1).GraphicY * 32,
                eventData.Pages(1).GraphicX2 * 32,
                eventData.Pages(1).GraphicY2 * 32
            )

            ' Adjust position if the tile is larger than 32x32
            If srcRect.Height > 32 Then y -= GameState.PicY

            ' Define destination rectangle
            Dim destRect As New Rectangle(x, y, srcRect.Width, srcRect.Height)

            RenderTexture(IO.Path.Combine(Core.Path.Tilesets, gfxIndex), destRect.X, destRect.Y, srcRect.X, srcRect.Y, destRect.Width, destRect.Height, srcRect.Width, srcRect.Height)
        Else
            ' Draw fallback outline if the tileset graphic is invalid
            DrawOutlineRectangle(x, y, GameState.PicX, GameState.PicY, Color.Blue, 0.6F)
        End If
    End Sub

    Friend Shared Sub DrawEvent(id As Integer) ' draw on map, outside the editor
        Dim x As Integer, y As Integer, width As Integer, height As Integer, sRect As Rectangle, anim As Integer, spritetop As Integer

        If MapEvents(id).Visible = False Then
            Exit Sub
        End If

        Select Case MapEvents(id).GraphicType
            Case 0
                Exit Sub
            Case 1
                If MapEvents(id).Graphic <= 0 Or MapEvents(id).Graphic > GameState.NumCharacters Then Exit Sub

                ' Reset frame
                If MapEvents(id).Steps = 3 Then
                    anim = 0
                ElseIf MapEvents(id).Steps = 1 Then
                    anim = 2
                End If

                Select Case MapEvents(id).Dir
                    Case DirectionType.Up
                        If (MapEvents(id).YOffset > 8) Then anim = MapEvents(id).Steps
                    Case DirectionType.Down
                        If (MapEvents(id).YOffset < -8) Then anim = MapEvents(id).Steps
                    Case DirectionType.Left
                        If (MapEvents(id).XOffset > 8) Then anim = MapEvents(id).Steps
                    Case DirectionType.Right
                        If (MapEvents(id).XOffset < -8) Then anim = MapEvents(id).Steps
                End Select

                ' Set the left
                Select Case MapEvents(id).ShowDir
                    Case DirectionType.Up
                        spritetop = 3
                    Case DirectionType.Right
                        spritetop = 2
                    Case DirectionType.Down
                        spritetop = 0
                    Case DirectionType.Left
                        spritetop = 1
                End Select

                If MapEvents(id).WalkAnim = 1 Then anim = 0
                If MapEvents(id).Moving = 0 Then anim = MapEvents(id).GraphicX

                width = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, MapEvents(id).Graphic)).Width / 4
                height = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, MapEvents(id).Graphic)).Height / 4

                Dim gfxInfo = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, MapEvents(id).Graphic))
                sRect = New Rectangle(
                    anim * (gfxInfo.Width / 4),
                    spritetop * (gfxInfo.Height / 4),
                    gfxInfo.Width / 4,
                    gfxInfo.Height / 4
                    )

                ' Calculate the X
                x = MapEvents(id).X * GameState.PicX + MapEvents(id).XOffset - ((GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, MapEvents(id).Graphic)).Width / 4 - 32) / 2)

                ' Is the player's height more than 32..?
                If (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, MapEvents(id).Graphic)).Height * 4) > 32 Then
                    ' Create a 32 pixel offset for larger sprites
                    y = MapEvents(id).Y * GameState.PicY + MapEvents(id).YOffset - ((GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, MapEvents(id).Graphic)).Height / 4) - 32)
                Else
                    ' Proceed as normal
                    y = MapEvents(id).Y * GameState.PicY + MapEvents(id).YOffset
                End If
                ' render the actual sprite
                DrawCharacterSprite(MapEvents(id).Graphic, x, y, sRect)
            Case 2
                If MapEvents(id).Graphic < 1 Or MapEvents(id).Graphic > GameState.NumTileSets Then Exit Sub
                If MapEvents(id).GraphicY2 > 0 Or MapEvents(id).GraphicX2 > 0 Then
                    With sRect
                        .X = MapEvents(id).GraphicX * 32
                        .Y = MapEvents(id).GraphicY * 32
                        .Width = MapEvents(id).GraphicX2 * 32
                        .Height = MapEvents(id).GraphicY2 * 32
                    End With
                Else
                    With sRect
                        .X = MapEvents(id).GraphicY * 32
                        .Height = .Top + 32
                        .Y = MapEvents(id).GraphicX * 32
                        .Width = .Left + 32
                    End With
                End If
                x = MapEvents(id).X * 32
                y = MapEvents(id).Y * 32
                x = x - ((sRect.Right - sRect.Left) / 2)
                y = y - (sRect.Bottom - sRect.Top) + 32

                If MapEvents(id).GraphicY2 > 1 Then
                    RenderTexture(IO.Path.Combine(Core.Path.Tilesets, MapEvents(id).Graphic), ConvertMapX(MapEvents(id).X * GameState.PicX), ConvertMapY(MapEvents(id).Y * GameState.PicY) - GameState.PicY, sRect.Left, sRect.Top, sRect.Width, sRect.Height)
                Else
                    RenderTexture(IO.Path.Combine(Core.Path.Tilesets, MapEvents(id).Graphic), ConvertMapX(MapEvents(id).X * GameState.PicX), ConvertMapY(MapEvents(id).Y * GameState.PicY), sRect.Left, sRect.Top, sRect.Width, sRect.Height)
                End If
        End Select

    End Sub

    Friend Sub Render_Game()
        Dim x As Integer, y As Integer, i As Integer

        If GameState.GettingMap Then Exit Sub

        UpdateCamera()

        If GameState.NumPanoramas > 0 And MyMap.Panorama > 0 Then
            DrawPanorama(MyMap.Panorama)
        End If

        If GameState.NumParallax > 0 And MyMap.Parallax > 0 Then
            DrawParallax(MyMap.Parallax)
        End If

        ' Draw lower tiles
        If GameState.NumTileSets > 0 Then
            For x = GameState.TileView.Left - 1 To GameState.TileView.Right + 1
                For y = GameState.TileView.Top - 1 To GameState.TileView.Bottom + 1
                    If IsValidMapPoint(x, y) Then
                        DrawMapLowerTile(x, y)
                    End If
                Next
            Next
        End If

        ' events
        If GameState.MyEditorType <> EditorType.Map Then
            If GameState.CurrentEvents > 0 And GameState.CurrentEvents <= MyMap.EventCount Then
                For i = 1 To GameState.CurrentEvents
                    If MapEvents(i).Position = 0 Then
                        GameClient.DrawEvent(i)
                    End If
                Next
            End If
        End If

        ' blood
        For i = 0 To Byte.MaxValue
            GameClient.DrawBlood(i)
        Next

        ' Draw out the items
        If GameState.NumItems > 0 Then
            For i = 1 To MAX_MAP_ITEMS
                If MyMapItem(i).Num > 0 Then
                    GameClient.DrawMapItem(i)
                End If
            Next
        End If

        ' draw animations
        If GameState.NumAnimations > 0 Then
            For i = 0 To Byte.MaxValue
                If AnimInstance(i).Used(0) Then
                    DrawAnimation(i, 0)
                End If
            Next
        End If

        ' Y-based render. Renders Players, Npcs and Resources based on Y-axis.
        For y = 0 To MyMap.MaxY
            If GameState.NumCharacters > 0 Then
                ' Players
                For i = 1 To MAX_PLAYERS
                    If IsPlaying(i) And GetPlayerMap(i) = GetPlayerMap(GameState.MyIndex) Then
                        If Type.Player(i).Y = y Then
                            GameClient.DrawPlayer(i)
                        End If

                        If PetAlive(i) Then
                            If Type.Player(i).Pet.Y = y Then
                                DrawPet(i)
                            End If
                        End If
                    End If
                Next

                For i = 1 To MAX_MAP_NPCS
                    If Type.MyMapNPC(i).Y = y Then
                        GameClient.DrawNPC(i)
                    End If
                Next

                If GameState.MyEditorType <> EditorType.Map Then
                    If GameState.CurrentEvents > 0 And GameState.CurrentEvents <= MyMap.EventCount Then
                        For i = 1 To GameState.CurrentEvents
                            If MapEvents(i).Position = 1 Then
                                If y = MapEvents(i).Y Then
                                    GameClient.DrawEvent(i)
                                End If
                            End If
                        Next
                    End If
                End If

                ' Draw the target icon
                If GameState.MyTarget > 0 Then
                    If GameState.MyTargetType = TargetType.Player Then
                        GameClient.DrawTarget(Type.Player(GameState.MyTarget).X * 32 - 16 + Type.Player(GameState.MyTarget).XOffset, Type.Player(GameState.MyTarget).Y * 32 + Type.Player(GameState.MyTarget).YOffset)
                    ElseIf GameState.MyTargetType = TargetType.NPC Then
                        GameClient.DrawTarget(MyMapNPC(GameState.MyTarget).X * 32 - 16 + MyMapNPC(GameState.MyTarget).XOffset, MyMapNPC(GameState.MyTarget).Y * 32 + MyMapNPC(GameState.MyTarget).YOffset)
                    ElseIf GameState.MyTargetType = TargetType.Pet Then
                        GameClient.DrawTarget(Type.Player(GameState.MyTarget).Pet.X * 32 - 16 + Type.Player(GameState.MyTarget).Pet.XOffset, (Type.Player(GameState.MyTarget).Pet.Y * 32) + Type.Player(GameState.MyTarget).Pet.YOffset)
                    End If
                End If

                For i = 1 To MAX_PLAYERS
                    If IsPlaying(i) Then
                        If Type.Player(i).Map = Type.Player(GameState.MyIndex).Map Then
                            If GameState.CurX = Type.Player(i).X And GameState.CurY = Type.Player(i).Y Then
                                If GameState.MyTargetType = TargetType.Player And GameState.MyTarget = i Then

                                Else
                                    GameClient.DrawHover(Type.Player(i).X * 32 - 16, Type.Player(i).Y * 32 + Type.Player(i).YOffset)
                                End If
                            End If

                        End If
                    End If
                Next
            End If

            ' Resources
            If GameState.NumResources > 0 Then
                If GameState.ResourcesInit Then
                    If GameState.ResourceIndex > 0 Then
                        For i = 0 To GameState.ResourceIndex
                            If MyMapResource(i).Y = y Then
                                DrawMapResource(i)
                            End If
                        Next
                    End If
                End If
            End If
        Next

        ' animations
        If GameState.NumAnimations > 0 Then
            For i = 0 To Byte.MaxValue
                If AnimInstance(i).Used(1) Then
                    DrawAnimation(i, 1)
                End If
            Next
        End If

        If GameState.NumProjectiles > 0 Then
            For i = 1 To MAX_PROJECTILES
                If Type.MapProjectile(Type.Player(GameState.MyIndex).Map, i).ProjectileNum > 0 Then
                    DrawProjectile(i)
                End If
            Next
        End If

        If GameState.CurrentEvents > 0 And GameState.CurrentEvents <= MyMap.EventCount Then
            For i = 1 To GameState.CurrentEvents
                If MapEvents(i).Position = 2 Then
                    GameClient.DrawEvent(i)
                End If
            Next
        End If

        If GameState.NumTileSets > 0 Then
            For x = GameState.TileView.Left - 1 To GameState.TileView.Right + 1
                For y = GameState.TileView.Top - 1 To GameState.TileView.Bottom + 1
                    If IsValidMapPoint(x, y) Then
                        DrawMapUpperTile(x, y)
                    End If
                Next
            Next
        End If

        DrawWeather()
        DrawThunderEffect()
        DrawMapTint()

        ' Draw out a square at mouse cursor
        If GameState.MapGrid = 1 And GameState.MyEditorType = EditorType.Map Then
            GameClient.DrawGrid()
        End If
        
        For i = 1 To MAX_PLAYERS
            If IsPlaying(i) And GetPlayerMap(i) = GetPlayerMap(GameState.MyIndex) Then
                DrawPlayerName(i)
                If PetAlive(i) Then
                    DrawPlayerPetName(i)
                End If
            End If
        Next

        If GameState.CurrentEvents > 0 AndAlso MyMap.EventCount >= GameState.CurrentEvents Then
            For i = 1 To GameState.CurrentEvents
                If MapEvents(i).Visible = True Then
                    If MapEvents(i).ShowName = 1 Then
                        DrawEventName(i)
                    End If
                End If
            Next
        End If

        For i = 1 To MAX_MAP_NPCS
            If Type.MyMapNPC(i).Num > 0 Then
                DrawNPCName(i)
            End If
        Next

        DrawFog()
        DrawPicture()

        For i = 1 To Byte.MaxValue
            DrawActionMsg(i)
        Next

        For i = 1 To Byte.MaxValue
            If ChatBubble(i).Active Then
                GameClient.DrawChatBubble(i)
            End If
        Next

        If GameState.Bfps Then
            Dim fps As String = "FPS: " & GameClient.GetFps()
            Call RenderText(fps, GameState.Camera.Left - 24, GameState.Camera.Top + 60, Microsoft.Xna.Framework.Color.Yellow, Microsoft.Xna.Framework.Color.Black)
        End If

        ' draw cursor, player X and Y locations
        If GameState.BLoc Then
            Dim Cur As String = "Cur X: " & GameState.CurX & " Y: " & GameState.CurY
            Dim Loc As String = "loc X: " & GetPlayerX(GameState.MyIndex) & " Y: " & GetPlayerY(GameState.MyIndex)
            Dim Map As String = " (Map #" & GetPlayerMap(GameState.MyIndex) & ")"

            Call RenderText(Cur, GameState.DrawLocX, GameState.DrawLocY + 105, Microsoft.Xna.Framework.Color.Yellow, Microsoft.Xna.Framework.Color.Black)
            Call RenderText(Loc, GameState.DrawLocX, GameState.DrawLocY + 120, Microsoft.Xna.Framework.Color.Yellow, Microsoft.Xna.Framework.Color.Black)
            Call RenderText(Map, GameState.DrawLocX, GameState.DrawLocY + 135, Microsoft.Xna.Framework.Color.Yellow, Microsoft.Xna.Framework.Color.Black)
        End If

        DrawMapName()

        GameClient.DrawBars()
        DrawMapFade()
        Gui.Render()
        GameClient.RenderTexture(IO.Path.Combine(Core.Path.Misc, "Cursor"), GameState.CurMouseX, GameState.CurMouseY, 0, 0, 16, 16, 32, 32)
    End Sub

    Friend Sub Render_Menu()
        Gui.DrawMenuBG()
        Gui.Render()
        GameClient.RenderTexture(IO.Path.Combine(Core.Path.Misc, "Cursor"), GameState.CurMouseX, GameState.CurMouseY, 0, 0, 16, 16, 32, 32)
    End Sub
End Class
