Imports Core

Module [Loop]
    Sub GameLoop()
        Dim i As Integer
        Dim tmr1000 As Integer, tick As Integer, fogtmr As Integer, chattmr As Integer
        Dim tmpfps As Integer, tmplps As Integer, walkTimer As Integer, frameTime As Integer
        Dim tmrweather As Integer, barTmr As Integer
        Dim tmr25 As Integer, tmr500 As Integer, tmr250 As Integer, tmrconnect As Integer, TickFPS As Integer
        Dim fadetmr As Integer, rendertmr As Integer
        Dim animationtmr(1) As Integer

        ' Main game loop
        While GameState.InGame = True Or GameState.InMenu = True
            If GameState.GameDestroyed = True Then End

            tick = GetTickCount()
            GameState.ElapsedTime = tick - frameTime ' Set the time difference for time-based movement
            
            frameTime = tick
            
            'SyncLock loadLock
            '    TextureCounter = 0
            'End SyncLock

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

                If GameState.ShowAnimTimer < tick Then
                    GameState.ShowAnimLayers = Not GameState.ShowAnimLayers
                    GameState.ShowAnimTimer = tick + 500
                End If

                For layer = 0 To 1
                    If animationtmr(layer) < tick Then
                        For x = 0 To MyMap.MaxX
                            For y = 0 To MyMap.MaxY
                                If IsValidMapPoint(x, y) Then
                                    On Error GoTo mapsync
                                    If MyMap.Tile(x, y).Data1 > 0 And (MyMap.Tile(x, y).Type = TileType.Animation Or MyMap.Tile(x, y).Type2 = TileType.Animation) Then
                                        Dim sprite As Integer = Type.Animation(MyMap.Tile(x, y).Data1).Sprite(layer)

                                        If sprite > 0 Then
                                            Dim GfxInfo As GameClient.GfxInfo = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Animations, sprite))

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
                        If EventChat = 1 Then
                            EventChat = 0
                        End If
                    End If
                End If

                ' screenshake
                If GameState.ShakeTimerEnabled Then
                    If GameState.ShakeTimer < tick Then
                        If GameState.ShakeCount < 10 Then
                            If GameState.LastDir = 0 Then
                                GameState.LastDir = 1
                            Else
                                GameState.LastDir = 0
                            End If
                        Else
                            GameState.ShakeCount = 0
                            GameState.ShakeTimerEnabled = 0
                        End If

                        GameState.ShakeCount += 1

                        GameState.ShakeTimer = tick + 50
                    End If
                End If

                ' check if we need to end the CD icon
                If GameState.NumSkills > 0 Then
                    For i = 1 To MAX_PLAYER_SKILLS
                        If Type.Player(GameState.MyIndex).Skill(i).Num > 0 Then
                            If Type.Player(GameState.MyIndex).Skill(i).CD > 0 Then
                                If Type.Player(GameState.MyIndex).Skill(i).CD + (Type.Skill(Type.Player(GameState.MyIndex).Skill(i).Num).CdTime * 1000) < tick Then
                                    Type.Player(GameState.MyIndex).Skill(i).CD = 0
                                End If
                            End If
                        End If
                    Next
                End If

                ' check if we need to unlock the player's skill casting restriction
                If GameState.SkillBuffer > 0 Then
                    If GameState.SkillBufferTimer + (Type.Skill(Type.Player(GameState.MyIndex).Skill(GameState.SkillBuffer).Num).CastTime * 1000) < tick Then
                        GameState.SkillBuffer = 0
                        GameState.SkillBufferTimer = 0
                    End If
                End If

                ' check if we need to unlock the pets's Skill casting restriction
                If PetSkillBuffer > 0 Then
                    If PetSkillBufferTimer + (Type.Skill(Type.Pet(Type.Player(GameState.MyIndex).Pet.Num).Skill(PetSkillBuffer)).CastTime * 1000) < tick Then
                        PetSkillBuffer = 0
                        PetSkillBufferTimer = 0
                    End If
                End If

                If GameState.CanMoveNow Then
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

                    For i = 1 To GameState.CurrentEvents
                        ProcessEventMovement(i)
                    Next

                    walkTimer = tick + 25 ' edit this value to change WalkTimer
                End If

                ' chat timer
                If chattmr < tick Then
                    ' scrolling
                    If GameState.ChatButtonUp Then
                        ScrollChatBox(0)
                    End If

                    If GameState.ChatButtonDown Then
                        ScrollChatBox(1)
                    End If

                    chattmr = tick + 50
                End If

                ' fog scrolling
                If fogtmr < tick Then
                    If GameState.CurrentFogSpeed > 0 Then
                        ' move
                        GameState.FogOffsetX = GameState.FogOffsetX - 1
                        GameState.FogOffsetY = GameState.FogOffsetY - 1

                        ' reset
                        If GameState.FogOffsetX < -255 Then GameState.FogOffsetX = 1
                        If GameState.FogOffsetY < -255 Then GameState.FogOffsetY = 1
                        fogtmr = tick + 255 - GameState.CurrentFogSpeed
                    End If
                End If

                If tmr500 < tick Then
                    ' animate waterfalls
                    Select Case GameState.WaterfallFrame
                        Case 0
                            GameState.WaterfallFrame = 1
                        Case 1
                            GameState.WaterfallFrame = 2
                        Case 2
                            GameState.WaterfallFrame = 0
                    End Select

                    ' animate autotiles
                    Select Case GameState.AutoTileFrame
                        Case 0
                            GameState.AutoTileFrame = 1
                        Case 1
                            GameState.AutoTileFrame = 2
                        Case 2
                            GameState.AutoTileFrame = 0
                    End Select

                    ' animate textbox
                    If GameState.chatShowLine = "|" Then
                        GameState.chatShowLine = ""
                    Else
                        GameState.chatShowLine = "|"
                    End If

                    tmr500 = tick + 500
                End If

                ' elastic bars
                If barTmr < tick Then
                    SetBarWidth(GameState.BarWidth_GuiHP_Max, GameState.BarWidth_GuiHP)
                    SetBarWidth(GameState.BarWidth_GuiSP_Max, GameState.BarWidth_GuiSP)
                    SetBarWidth(GameState.BarWidth_GuiEXP_Max, GameState.BarWidth_GuiEXP)
                    For i = 1 To MAX_MAP_NPCS
                        If Type.MyMapNPC(i).Num > 0 Then
                            SetBarWidth(GameState.BarWidth_NpcHP_Max(i), GameState.BarWidth_NpcHP(i))
                        End If
                    Next

                    For i = 1 To MAX_PLAYERS
                        If IsPlaying(i) And GetPlayerMap(i) = GetPlayerMap(GameState.MyIndex) Then
                            SetBarWidth(GameState.BarWidth_PlayerHP_Max(i), GameState.BarWidth_PlayerHP(i))
                            SetBarWidth(GameState.BarWidth_PlayerSP_Max(i), GameState.BarWidth_PlayerSP(i))
                        End If
                    Next

                    ' reset timer
                    barTmr = tick + 10
                End If

                ' Change map animation
                If tmr250 < tick Then
                    If GameState.MapAnim = 0 Then
                        GameState.MapAnim = 1
                    Else
                        GameState.MapAnim = 0
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
                    If GameState.chatShowLine = "|" Then
                        GameState.chatShowLine = ""
                    Else
                        GameState.chatShowLine = "|"
                    End If

                    tmr500 = tick + 500
                End If

                If tmr25 < tick Then
                    PlayMusic(Settings.MenuMusic)
                    tmr25 = tick + 25
                End If
            End If

            If tmrweather < tick Then
                ProcessWeather()
                tmrweather = tick + 50
            End If

            If fadetmr < tick Then
                If GameState.FadeType <> 2 Then
                    If GameState.FadeType = 1 Then
                        If GameState.FadeAmount = 255 Then
                        Else
                            GameState.FadeAmount = GameState.FadeAmount + 5
                        End If
                    ElseIf GameState.FadeType = 0 Then
                        If GameState.FadeAmount = 0 Then
                            GameState.UseFade = 0
                        Else
                            GameState.FadeAmount = GameState.FadeAmount - 5
                        End If
                    End If
                End If
                fadetmr = tick + 30
            End If
            
            Gui.ResizeGUI()
            
            ' Signal that loading is complete
            SyncLock GameClient.LoadLock
                if GameClient.IsLoading Then
                    GameClient.isLoading = 0
                End If
            End SyncLock
        End While
    End Sub
End Module