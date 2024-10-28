Imports Mirage.Sharp.Asfw
Imports Core

Module Player
#Region "Database"
    Sub ClearPlayers()
        Dim i As Integer

        ReDim Type.Account(MAX_PLAYERS)
        ReDim Type.Player(MAX_PLAYERS)

        For i = 1 To MAX_PLAYERS
            ClearAccount(i)
            ClearPlayer(i)
        Next
    End Sub

    Sub ClearAccount(index As Integer)
        Type.Account(index).Login = ""
    End Sub

    Sub ClearPlayer(index As Integer)
        ClearAccount(index)

        Type.Player(index).Name = ""
        Type.Player(index).Attacking = 0
        Type.Player(index).AttackTimer = 0
        Type.Player(index).Job = 1
        Type.Player(index).Dir = 0
        Type.Player(index).Access = AccessType.Player

        ReDim Type.Player(index).Equipment(EquipmentType.Count - 1)
        For y = 0 To EquipmentType.Count - 1
            Type.Player(index).Equipment(y) = -1
        Next

        Type.Player(index).Exp = 0
        Type.Player(index).Level = 0
        Type.Player(index).Map = 1
        Type.Player(index).MapGetTimer = 0
        Type.Player(index).Moving = 0
        Type.Player(index).Pk = 0
        Type.Player(index).Points = 0
        Type.Player(index).Sprite = 0

        ReDim Type.Player(index).Inv(MAX_INV)
        For x = 1 To MAX_INV
            Type.Player(index).Inv(x).Num = 0
            Type.Player(index).Inv(x).Value = 0
        Next

        ReDim Type.Player(index).Skill(MAX_PLAYER_SKILLS)
        For x = 1 To MAX_PLAYER_SKILLS
            Type.Player(index).Skill(x).Num = 0
            Type.Player(index).Skill(x).CD = 0
        Next

        ReDim Type.Player(index).Stat(StatType.Count - 1)
        For x = 0 To StatType.Count - 1
            Type.Player(index).Stat(x) = 0
        Next

        Type.Player(index).Steps = 0

        ReDim Type.Player(index).Vital(VitalType.Count - 1)
        For x = 0 To VitalType.Count - 1
            Type.Player(index).Vital(x) = 0
        Next

        Type.Player(index).X = 0
        Type.Player(index).XOffset = 0
        Type.Player(index).Y = 0
        Type.Player(index).YOffset = 0

        ReDim Type.Player(index).Hotbar(MAX_Hotbar)
        ReDim Type.Player(index).GatherSkills(ResourceType.Count - 1)
        ReDim Type.Player(index).GatherSkills(ResourceType.Count - 1)

        Type.Player(index).Pet.Num = 0
        Type.Player(index).Pet.Health = 0
        Type.Player(index).Pet.Mana = 0
        Type.Player(index).Pet.Level = 0

        ReDim Type.Player(index).Pet.Stat(StatType.Count - 1)
        For x = 0 To StatType.Count - 1
            Type.Player(index).Pet.Stat(x) = 0
        Next

        ReDim Type.Player(index).Pet.Skill(4)
        For x = 0 To 4
            Type.Player(index).Pet.Skill(x) = 0
        Next

        Type.Player(index).Pet.X = 0
        Type.Player(index).Pet.Y = 0
        Type.Player(index).Pet.Dir = 0
        Type.Player(index).Pet.MaxHp = 0
        Type.Player(index).Pet.MaxMp = 0
        Type.Player(index).Pet.Alive = 0
        Type.Player(index).Pet.AttackBehaviour = 0
        Type.Player(index).Pet.Exp = 0
        Type.Player(index).Pet.Tnl = 0
    End Sub
#End Region

#Region "Movement"
    Sub CheckMovement()
        If IsTryingToMove() AndAlso CanMove() Then
            ' Check if player has the shift key down for running
            If GameState.VbKeyShift Then
                Type.Player(GameState.MyIndex).Moving = MovementType.Walking
            Else
                Type.Player(GameState.MyIndex).Moving = MovementType.Running               
            End If

            Select Case GetPlayerDir(GameState.MyIndex)
                Case DirectionType.Up
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).YOffset = GameState.PicY
                    SetPlayerY(GameState.MyIndex, GetPlayerY(GameState.MyIndex) - 1)
                Case DirectionType.Down
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).YOffset = GameState.PicY * -1
                    SetPlayerY(GameState.MyIndex, GetPlayerY(GameState.MyIndex) + 1)
                Case DirectionType.Left
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).XOffset = GameState.PicX
                    SetPlayerX(GameState.MyIndex, GetPlayerX(GameState.MyIndex) - 1)
                Case DirectionType.Right
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).XOffset = GameState.PicX * -1
                    SetPlayerX(GameState.MyIndex, GetPlayerX(GameState.MyIndex) + 1)
                Case DirectionType.UpLeft
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).XOffset = GameState.PicX
                    SetPlayerX(GameState.MyIndex, GetPlayerX(GameState.MyIndex) - 1)
                    Type.Player(GameState.MyIndex).YOffset = GameState.PicY
                    SetPlayerY(GameState.MyIndex, GetPlayerY(GameState.MyIndex) - 1)
                Case DirectionType.UpRight
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).XOffset = GameState.PicX * -1
                    SetPlayerX(GameState.MyIndex, GetPlayerX(GameState.MyIndex) + 1)
                    Type.Player(GameState.MyIndex).YOffset = GameState.PicY
                    SetPlayerY(GameState.MyIndex, GetPlayerY(GameState.MyIndex) - 1)
                Case DirectionType.DownLeft
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).XOffset = GameState.PicX
                    SetPlayerX(GameState.MyIndex, GetPlayerX(GameState.MyIndex) - 1)
                    Type.Player(GameState.MyIndex).YOffset = GameState.PicY * -1
                    SetPlayerY(GameState.MyIndex, GetPlayerY(GameState.MyIndex) + 1)
                Case DirectionType.DownRight
                    SendPlayerMove()
                    Type.Player(GameState.MyIndex).XOffset = GameState.PicX * -1
                    SetPlayerX(GameState.MyIndex, GetPlayerX(GameState.MyIndex) + 1)
                    Type.Player(GameState.MyIndex).YOffset = GameState.PicY * -1
                    SetPlayerY(GameState.MyIndex, GetPlayerY(GameState.MyIndex) + 1)
            End Select

            If Type.Player(GameState.MyIndex).XOffset = 0 And Type.Player(GameState.MyIndex).YOffset = 0 Then
                If MyMap.Tile(GetPlayerX(GameState.MyIndex), GetPlayerY(GameState.MyIndex)).Type = TileType.Warp Or MyMap.Tile(Global.Core.Commands.GetPlayerX(GameState.MyIndex), GetPlayerY(GameState.MyIndex)).Type2 = Global.Core.Enum.TileType.Warp Then
                    GameState.GettingMap = True
                End If
            End If

        End If
    End Sub

    Function IsTryingToMove() As Boolean

        If GameState.DirUp Or GameState.DirDown Or GameState.DirLeft Or GameState.DirRight Then
            IsTryingToMove = True
        End If

    End Function

    Function CanMove() As Boolean
        Dim d As Integer

        CanMove = True

        If Type.Player(GameState.MyIndex).XOffset <> 0 Or Type.Player(GameState.MyIndex).YOffset <> 0 Then
            CanMove = False
            Exit Function
        End If

        If HoldPlayer = True Then
            CanMove = False
            Exit Function
        End If

        If GameState.GettingMap = True Then
            CanMove = False
            Exit Function
        End If

        ' Make sure they aren't trying to move when they are already moving
        If Type.Player(GameState.MyIndex).Moving <> 0 Then
            CanMove = False
            Exit Function
        End If

        ' Make sure they haven't just casted a skill
        If GameState.SkillBuffer > 0 Then
            CanMove = False
            Exit Function
        End If

        ' make sure they're not stunned
        If GameState.StunDuration > 0 Then
            CanMove = False
            Exit Function
        End If

        If InEvent Then
            CanMove = False
            Exit Function
        End If

        If InTrade Then
            CanMove = False
            Exit Function
        End If

        If Not GameState.inSmallChat Then
            CanMove = False
            Exit Function
        End If

        If GameState.InShop > 0 Then
            CloseShop
        End If

        If GameState.InBank Then
            CloseBank()
        End If

        d = GetPlayerDir(GameState.MyIndex)

        Select Case d
            Case DirectionType.Up

                If GetPlayerY(GameState.MyIndex) <= 0 Then
                    GameState.DirUp = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Down)
                    Exit Function
                End If

            Case DirectionType.Down

                If GetPlayerY(GameState.MyIndex) >= MyMap.MaxY Then
                    GameState.DirDown = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Up)
                    Exit Function
                End If

            Case DirectionType.Left

                If GetPlayerX(GameState.MyIndex) <= 0 Then
                    GameState.DirLeft = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Right)
                    Exit Function
                End If

            Case DirectionType.Right

                If GetPlayerX(GameState.MyIndex) >= MyMap.MaxX Then
                    GameState.DirRight = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Left)
                    Exit Function
                End If

            Case DirectionType.UpLeft

                 If GetPlayerY(GameState.MyIndex) <= 0 And GetPlayerX(GameState.MyIndex) <= 0 Then
                    GameState.DirUp = False
                    GameState.DirDown = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Down)
                    GameState.DirLeft = False
                    GameState.DirRight = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Right)
                    Exit Function
                End If

                If GetPlayerY(GameState.MyIndex) <= 0 Then
                    GameState.DirUp = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Down)
                    Exit Function
                End If

                If GetPlayerX(GameState.MyIndex) <= 0 Then
                    GameState.DirLeft = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Right)
                    Exit Function
                End If

            Case DirectionType.UpRight

                If GetPlayerY(GameState.MyIndex) >= MyMap.MaxY And GetPlayerX(GameState.MyIndex) >= MyMap.MaxX Then
                    GameState.DirUp = False
                    GameState.DirDown = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Down)
                    GameState.DirRight = False
                    GameState.DirLeft = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Left)
                    Exit Function
                End If

                If GetPlayerY(GameState.MyIndex) <= 0 Then
                    GameState.DirUp = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Down)
                    Exit Function
                End If

                If GetPlayerX(GameState.MyIndex) <= 0 Then
                    GameState.DirLeft = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Right)
                    Exit Function
                End If

            Case DirectionType.DownLeft

                If GetPlayerY(GameState.MyIndex) >= MyMap.MaxY And GetPlayerX(GameState.MyIndex) <= 0 Then
                    GameState.DirDown = False
                    GameState.DirUp = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Up)
                    GameState.DirLeft = False
                    GameState.DirRight = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Right)
                    Exit Function
                End If

                If GetPlayerY(GameState.MyIndex) <= 0 Then
                    GameState.DirDown = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Up)
                    Exit Function
                End If

                If GetPlayerX(GameState.MyIndex) <= 0 Then
                    GameState.DirLeft = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Right)
                    Exit Function
                End If

            Case DirectionType.DownRight

                If GetPlayerY(GameState.MyIndex) >= MyMap.MaxY And GetPlayerX(GameState.MyIndex) >= MyMap.MaxX Then
                    GameState.DirDown = False
                    GameState.DirUp = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Up)
                    GameState.DirRight = False
                    GameState.DirLeft = True
                    SetPlayerDir(GameState.MyIndex, DirectionType.Left)
                    Exit Function
                End If

                If GetPlayerY(GameState.MyIndex) >= MyMap.MaxY Then
                    GameState.DirDown = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Up)
                    Exit Function
                End If

                If GetPlayerX(GameState.MyIndex) >= MyMap.MaxX Then
                    GameState.DirRight = False
                    SetPlayerDir(GameState.MyIndex, DirectionType.Left)
                    Exit Function
                End If

        End Select

        ' Check for cardinal movements if no diagonal movements
        If GameState.DirUp Then
            SetPlayerDir(GameState.MyIndex, DirectionType.Up)
            If GetPlayerY(GameState.MyIndex) > 0 Then
                If CheckDirection(DirectionType.Up) Then
                    CanMove = False
                    If d <> DirectionType.Up Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Up > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        End If

        If GameState.DirDown Then
            SetPlayerDir(GameState.MyIndex, DirectionType.Down)
            If GetPlayerY(GameState.MyIndex) < MyMap.MaxY Then
                If CheckDirection(DirectionType.Down) Then
                    CanMove = False
                    If d <> DirectionType.Down Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Down > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        End If

        If GameState.DirLeft Then
            SetPlayerDir(GameState.MyIndex, DirectionType.Left)
            If GetPlayerX(GameState.MyIndex) > 0 Then
                If CheckDirection(DirectionType.Left) Then
                    CanMove = False
                    If d <> DirectionType.Left Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Left > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        End If

        If GameState.DirRight Then
            SetPlayerDir(GameState.MyIndex, DirectionType.Right)
            If GetPlayerX(GameState.MyIndex) < MyMap.MaxX Then
                If CheckDirection(DirectionType.Right) Then
                    CanMove = False
                    If d <> DirectionType.Right Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Right > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        End If

        ' Check for diagonal movements first
        If GameState.DirUp And GameState.DirRight Then
            SetPlayerDir(GameState.MyIndex, DirectionType.UpRight)
            If GetPlayerY(GameState.MyIndex) > 0 And GetPlayerX(GameState.MyIndex) < MyMap.MaxX Then
                If CheckDirection(DirectionType.UpRight) Then
                    CanMove = False
                    If d <> DirectionType.UpRight Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Up > 0 And MyMap.Right > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        ElseIf GameState.DirUp And GameState.DirLeft Then
            SetPlayerDir(GameState.MyIndex, DirectionType.UpLeft)
            If GetPlayerY(GameState.MyIndex) > 0 And GetPlayerX(GameState.MyIndex) > 0 Then
                If CheckDirection(DirectionType.UpLeft) Then
                    CanMove = False
                    If d <> DirectionType.UpLeft Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Up > 0 And MyMap.Left > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        ElseIf GameState.DirDown And GameState.DirRight Then
            SetPlayerDir(GameState.MyIndex, DirectionType.DownRight)
            If GetPlayerY(GameState.MyIndex) < MyMap.MaxY And GetPlayerX(GameState.MyIndex) < MyMap.MaxX Then
                If CheckDirection(DirectionType.DownRight) Then
                    CanMove = False
                    If d <> DirectionType.DownRight Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Down > 0 And MyMap.Right > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        ElseIf GameState.DirDown And GameState.DirLeft Then
            SetPlayerDir(GameState.MyIndex, DirectionType.DownLeft)
            If GetPlayerY(GameState.MyIndex) < MyMap.MaxY And GetPlayerX(GameState.MyIndex) > 0 Then
                If CheckDirection(DirectionType.DownLeft) Then
                    CanMove = False
                    If d <> DirectionType.DownLeft Then
                        SendPlayerDir()
                    End If
                    Exit Function
                End If
            ElseIf MyMap.Down > 0 And MyMap.Left > 0 Then
                SendPlayerRequestNewMap()
                CanMove = False
                Exit Function
            End If
        End If

    End Function

    Function CheckDirection(direction As Byte) As Boolean
        Dim x As Integer, y As Integer
        Dim i As Integer

        ' check directional blocking
        If IsDirBlocked(MyMap.Tile(GetPlayerX(GameState.MyIndex), GetPlayerY(GameState.MyIndex)).DirBlock, direction) Then
            CheckDirection = True
            Exit Function
        End If

        Select Case direction
            Case DirectionType.Up
                x = GetPlayerX(GameState.MyIndex)
                y = GetPlayerY(GameState.MyIndex) - 1
            Case DirectionType.Down
                x = GetPlayerX(GameState.MyIndex)
                y = GetPlayerY(GameState.MyIndex) + 1
            Case DirectionType.Left
                x = GetPlayerX(GameState.MyIndex) - 1
                y = GetPlayerY(GameState.MyIndex)
            Case DirectionType.Right
                x = GetPlayerX(GameState.MyIndex) + 1
                y = GetPlayerY(GameState.MyIndex)
            Case DirectionType.UpLeft
                x = GetPlayerX(GameState.MyIndex) - 1
                y = GetPlayerY(GameState.MyIndex) - 1
            Case DirectionType.UpRight
                x = GetPlayerX(GameState.MyIndex) + 1
                y = GetPlayerY(GameState.MyIndex) - 1
            Case DirectionType.DownLeft
                x = GetPlayerX(GameState.MyIndex) - 1
                y = GetPlayerY(GameState.MyIndex) + 1
            Case DirectionType.DownRight
                x = GetPlayerX(GameState.MyIndex) + 1
                y = GetPlayerY(GameState.MyIndex) + 1
        End Select

        ' Check to see if the map tile is blocked or not
        If MyMap.Tile(x, y).Type = TileType.Blocked Or MyMap.Tile(x, y).Type2 = TileType.Blocked Then
            CheckDirection = True
            Exit Function
        End If

        ' Check to see if the map tile is tree or not
        If MyMap.Tile(x, y).Type = TileType.Resource Or MyMap.Tile(x, y).Type2 = TileType.Resource Then
            CheckDirection = True
            Exit Function
        End If

        ' Check to see if a player is already on that tile
        If MyMap.Moral > 0 Then
            If Type.Moral(MyMap.Moral).PlayerBlock Then
                For i = 1 To MAX_PLAYERS
                    If IsPlaying(i) Then
                        If Type.Player(i).X = x And Type.Player(i).Y = y Then
                            CheckDirection = True
                            Exit Function
                        End If
                    End If
                Next
            End If

            ' Check to see if a npc is already on that tile
            If Type.Moral(MyMap.Moral).NPCBlock Then
                For i = 1 To MAX_MAP_NPCS
                    If Type.MyMapNPC(i).Num > 0 And Type.MyMapNPC(i).X = x And Type.MyMapNPC(i).Y = y Then
                        CheckDirection = True
                        Exit Function
                    End If
                Next
            End If
        End If

        For i = 1 To GameState.CurrentEvents
           If MapEvents(i).Visible = 1 Then
               If MapEvents(i).X = x And MapEvents(i).Y = y Then
                   If MapEvents(i).WalkThrough = 0 Then
                        CheckDirection = True
                        Exit Function
                    End If
                End If
            End If
        Next

    End Function

    Sub ProcessPlayerMovement(index As Integer)
        ' Check if player is walking or running, and if so process moving them over
        Select Case Type.Player(index).Moving
            Case MovementType.Walking
                GameState.MovementSpeed = (GameState.ElapsedTime / 1000.0) * GameState.WalkSpeed * GameState.SizeX ' Adjust speed by elapsed time
            Case MovementType.Running
                GameState.MovementSpeed = (GameState.ElapsedTime / 1000.0) * GameState.RunSpeed * GameState.SizeX ' Adjust speed by elapsed time
            Case Else
                Exit Sub
        End Select

        ' Adjust speed for diagonal movement
        If GetPlayerDir(index) = DirectionType.UpRight OrElse GetPlayerDir(index) = DirectionType.UpLeft OrElse GetPlayerDir(index) = DirectionType.DownRight OrElse GetPlayerDir(index) = DirectionType.DownLeft Then
            GameState.MovementSpeed = Math.Sqrt(GameState.MovementSpeed)
        End If

        GameState.MovementSpeed = Math.Round(GameState.MovementSpeed)

        ' Update player offsets based on direction
        Select Case GetPlayerDir(index)
            Case DirectionType.Up
                Type.Player(index).YOffset -= GameState.MovementSpeed
                If Type.Player(index).YOffset < 0 Then Type.Player(index).YOffset = 0
            Case DirectionType.Down
                Type.Player(index).YOffset += GameState.MovementSpeed
                If Type.Player(index).YOffset > 0 Then Type.Player(index).YOffset = 0
            Case DirectionType.Left
                Type.Player(index).XOffset -= GameState.MovementSpeed
                If Type.Player(index).XOffset < 0 Then Type.Player(index).XOffset = 0
            Case DirectionType.Right
                If GetPlayerX(index) >= MyMap.MaxX Then
                    SetPlayerDir(index, DirectionType.Left)
                    Exit Sub
                End If

                Type.Player(index).XOffset += GameState.MovementSpeed
                If Type.Player(index).XOffset > 0 Then Type.Player(index).XOffset = 0
            Case DirectionType.UpRight
                Type.Player(index).XOffset += GameState.MovementSpeed
                Type.Player(index).YOffset -= GameState.MovementSpeed
                If Type.Player(index).XOffset > 0 Then Type.Player(index).XOffset = 0
                If Type.Player(index).YOffset < 0 Then Type.Player(index).YOffset = 0
            Case DirectionType.UpLeft
                Type.Player(index).XOffset -= GameState.MovementSpeed
                Type.Player(index).YOffset -= GameState.MovementSpeed
                If Type.Player(index).XOffset < 0 Then Type.Player(index).XOffset = 0
                If Type.Player(index).YOffset < 0 Then Type.Player(index).YOffset = 0
            Case DirectionType.DownRight
                Type.Player(index).XOffset += GameState.MovementSpeed
                Type.Player(index).YOffset += GameState.MovementSpeed
                If Type.Player(index).XOffset > 0 Then Type.Player(index).XOffset = 0
                If Type.Player(index).YOffset > 0 Then Type.Player(index).YOffset = 0
            Case DirectionType.DownLeft
                Type.Player(index).XOffset -= GameState.MovementSpeed
                Type.Player(index).YOffset += GameState.MovementSpeed
                If Type.Player(index).XOffset < 0 Then Type.Player(index).XOffset = 0
                If Type.Player(index).YOffset > 0 Then Type.Player(index).YOffset = 0
        End Select

        ' Check if completed walking over to the next tile
        If Type.Player(index).Moving > 0 Then
            If (Type.Player(index).XOffset = 0 And Type.Player(index).YOffset = 0) Then
                Type.Player(index).Moving = 0
                If Type.Player(index).Steps = 1 Then
                    Type.Player(index).Steps = 3
                Else
                    Type.Player(index).Steps = 1
                End If
            End If
        End If

    End Sub


#End Region

#Region "Attacking"
    Sub CheckAttack(Optional mouse As Boolean = False)
        Dim attackspeed As Integer, x As Integer, y As Integer
        Dim buffer As New ByteStream(4)

        If GameState.VbKeyControl Or mouse Then
            If GameState.MyIndex < 1 Or GameState.MyIndex > MAX_PLAYERS Then Exit Sub
            If InEvent = True Then Exit Sub
            If GameState.SkillBuffer > 0 Then Exit Sub ' currently casting a skill, can't attack
            If GameState.StunDuration > 0 Then Exit Sub ' stunned, can't attack

            ' speed from weapon
            If GetPlayerEquipment(GameState.MyIndex, EquipmentType.Weapon) > 0 Then
                attackspeed = Type.Item(GetPlayerEquipment(GameState.MyIndex, EquipmentType.Weapon)).Speed * 1000
            Else
                attackspeed = 1000
            End If

            If Type.Player(GameState.MyIndex).AttackTimer + attackspeed < GetTickCount() Then
                If Type.Player(GameState.MyIndex).Attacking = 0 Then

                    With Type.Player(GameState.MyIndex)
                        .Attacking = 1
                        .AttackTimer = GetTickCount()
                    End With

                    SendAttack()
                End If
            End If

            Select Case Type.Player(GameState.MyIndex).Dir
                Case DirectionType.Up
                    x = GetPlayerX(GameState.MyIndex)
                    y = GetPlayerY(GameState.MyIndex) - 1

                Case DirectionType.Down
                    x = GetPlayerX(GameState.MyIndex)
                    y = GetPlayerY(GameState.MyIndex) + 1

                Case DirectionType.Left
                    x = GetPlayerX(GameState.MyIndex) - 1
                    y = GetPlayerY(GameState.MyIndex)
                Case DirectionType.Right
                    x = GetPlayerX(GameState.MyIndex) + 1
                    y = GetPlayerY(GameState.MyIndex)

                Case DirectionType.UpRight
                    x = GetPlayerX(GameState.MyIndex) + 1
                    y = GetPlayerY(GameState.MyIndex) - 1

                Case DirectionType.UpLeft
                    x = GetPlayerX(GameState.MyIndex) - 1
                    y = GetPlayerY(GameState.MyIndex) - 1

                Case DirectionType.DownRight
                    x = GetPlayerX(GameState.MyIndex) + 1
                    y = GetPlayerY(GameState.MyIndex) + 1

                Case DirectionType.DownLeft
                    x = GetPlayerX(GameState.MyIndex) - 1
                    y = GetPlayerY(GameState.MyIndex) + 1
            End Select

            If GetTickCount() > Type.Player(GameState.MyIndex).EventTimer Then
                For i = 1 To GameState.CurrentEvents
                   If MapEvents(i).Visible = 1 Then
                       If MapEvents(i).X = x And MapEvents(i).Y = y Then
                            buffer = New ByteStream(4)
                            buffer.WriteInt32(ClientPackets.CEvent)
                            buffer.WriteInt32(i)
                            Socket.SendData(buffer.Data, buffer.Head)
                            buffer.Dispose()
                            Type.Player(GameState.MyIndex).EventTimer = GetTickCount() + 200
                        End If
                    End If
                Next
            End If
        End If

    End Sub

    Friend Sub PlayerCastSkill(skillslot As Integer)
        Dim buffer As New ByteStream(4)

        ' Check for subscript out of range
        If skillslot <= 0 Or skillslot > MAX_PLAYER_SKILLS Then Exit Sub

        If Type.Player(GameState.MyIndex).Skill(skillslot).CD > 0 Then
            AddText("Skill has not cooled down yet!", ColorType.BrightRed)
            Exit Sub
        End If

        ' Check if player has enough MP
        If GetPlayerVital(GameState.MyIndex, VitalType.SP) < Type.Skill(Type.Player(GameState.MyIndex).Skill(skillslot).Num).MpCost Then
            AddText("Not enough MP to cast " & Type.Skill(Type.Player(GameState.MyIndex).Skill(skillslot).Num).Name & ".", ColorType.BrightRed)
            Exit Sub
        End If

        If Type.Player(GameState.MyIndex).Skill(skillslot).Num > 0 Then
            If GetTickCount() > Type.Player(GameState.MyIndex).AttackTimer + 1000 Then
                If Type.Player(GameState.MyIndex).Moving = 0 Then
                    If MyMap.Moral > 0 Then
                        If Type.Moral(MyMap.Moral).CanCast Then
                            SendCast(skillSlot)
                        Else
                            AddText("Cannot cast here!", ColorType.BrightRed)
                        End If
                    End If
                Else
                    AddText("Cannot cast while walking!", ColorType.BrightRed)
                End If
            End If
        Else
            AddText("No skill here.", ColorType.BrightRed)
        End If

    End Sub

    Function FindSkill(skillNum As Integer) As Integer
        Dim i As Integer

        FindSkill = 0

        ' Check for subscript out of range
        If skillNum <= 0 Or skillNum > MAX_SKILLS Then
            Exit Function
        End If

        For i = 1 To MAX_PLAYER_SKILLS
            ' Check to see if the player has the skill
            If GetPlayerSkill(GameState.MyIndex, i) = skillNum Then
                FindSkill = i
                Exit Function
            End If
        Next

    End Function

#End Region

#Region "Outgoing Traffic"

#End Region

#Region "Incoming Traffic"
    Sub Packet_PlayerHP(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        SetPlayerVital(GameState.MyIndex, VitalType.HP, buffer.ReadInt32)

        ' set max width
        If GetPlayerVital(GameState.MyIndex, VitalType.HP) > 0 Then
            GameState.BarWidth_GuiHP_Max = ((GetPlayerVital(GameState.MyIndex, VitalType.HP) / 209) / (GetPlayerMaxVital(GameState.MyIndex, VitalType.HP) / 209)) * 209
        Else
            GameState.BarWidth_GuiHP_Max = 0
        End If

        UpdateStats_UI()

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerSP(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        SetPlayerVital(GameState.MyIndex, VitalType.SP, buffer.ReadInt32)

        ' set max width
        If GetPlayerVital(GameState.MyIndex, VitalType.SP) > 0 Then
            GameState.BarWidth_GuiSP_Max = ((GetPlayerVital(GameState.MyIndex, VitalType.SP) / 209) / (GetPlayerMaxVital(GameState.MyIndex, VitalType.SP) / 209)) * 209
        Else
            GameState.BarWidth_GuiSP_Max = 0
        End If

        UpdateStats_UI()

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerStats(ByRef data() As Byte)
        Dim i As Integer, index As Integer
        Dim buffer As New ByteStream(data)

        index = buffer.ReadInt32

        For i = 1 To StatType.Count - 1
            SetPlayerStat(index, i, buffer.ReadInt32)
        Next

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerData(ByRef data() As Byte)
        Dim i As Integer, x As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32
        SetPlayerName(i, buffer.ReadString)
        SetPlayerJob(i, buffer.ReadInt32)
        SetPlayerLevel(i, buffer.ReadInt32)
        SetPlayerPoints(i, buffer.ReadInt32)
        SetPlayerSprite(i, buffer.ReadInt32)
        SetPlayerMap(i, buffer.ReadInt32)
        SetPlayerAccess(i, buffer.ReadInt32)
        SetPlayerPk(i, buffer.ReadInt32)

        For x = 1 To StatType.Count - 1
            SetPlayerStat(i, x, buffer.ReadInt32)
        Next

        For x = 1 To ResourceType.Count - 1
            Type.Player(i).GatherSkills(x).SkillLevel = buffer.ReadInt32
            Type.Player(i).GatherSkills(x).SkillCurExp = buffer.ReadInt32
            Type.Player(i).GatherSkills(x).SkillNextLvlExp = buffer.ReadInt32
        Next

        ' Check if the player is the client player
        If i = GameState.MyIndex Then
            ' Reset directions
            GameState.DirUp = False
            GameState.DirDown = False
            GameState.DirLeft = False
            GameState.DirRight = False

            ' set form
            With Windows(GetWindowIndex("winCharacter"))
                .Controls(GetControlIndex("winCharacter", "lblName")).Text = "Name"
                .Controls(GetControlIndex("winCharacter", "lblClass")).Text = "Class"
                .Controls(GetControlIndex("winCharacter", "lblLevel")).Text = "Level"
                .Controls(GetControlIndex("winCharacter", "lblGuild")).Text = "Guild"
                .Controls(GetControlIndex("winCharacter", "lblName2")).Text = GetPlayerName(GameState.MyIndex)
                .Controls(GetControlIndex("winCharacter", "lblClass2")).Text =Type.Job(GetPlayerJob(GameState.MyIndex)).Name
                .Controls(GetControlIndex("winCharacter", "lblLevel2")).Text = GetPlayerLevel(GameState.MyIndex)
                .Controls(GetControlIndex("winCharacter", "lblGuild2")).Text = "None"
                UpdateStats_UI()

                ' stats
                For x = 1 To StatType.Count - 1
                    .Controls(GetControlIndex("winCharacter", "lblStat_" & x)).Text = GetPlayerStat(GameState.MyIndex, x)
                Next

                ' points
                .Controls(GetControlIndex("winCharacter", "lblPoints")).Text = GetPlayerPoints(GameState.MyIndex)

                ' grey out buttons
                If GetPlayerPoints(GameState.MyIndex) = 0 Then
                    For x = 1 To StatType.Count - 1
                        .Controls(GetControlIndex("winCharacter", "btnGreyStat_" & x)).Visible = True
                    Next
                Else
                    For x = 1 To StatType.Count - 1
                        .Controls(GetControlIndex("winCharacter", "btnGreyStat_" & x)).Visible = False
                    Next
                End If
            End With
            GameState.PlayerData = True
        End If

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerMove(ByRef data() As Byte)
        Dim i As Integer, x As Integer, y As Integer
        Dim dir As Integer, n As Byte
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32
        x = buffer.ReadInt32
        y = buffer.ReadInt32
        dir = buffer.ReadInt32
        n = buffer.ReadInt32

        SetPlayerX(i, x)
        SetPlayerY(i, y)
        SetPlayerDir(i, dir)
        Type.Player(i).XOffset = 0
        Type.Player(i).YOffset = 0
        Type.Player(i).Moving = n

        Select Case GetPlayerDir(i)
            Case DirectionType.Up
                Type.Player(i).YOffset = GameState.PicY
            Case DirectionType.Down
                Type.Player(i).YOffset = GameState.PicY * -1
            Case DirectionType.Left
                Type.Player(i).XOffset = GameState.PicX
            Case DirectionType.Right
                Type.Player(i).XOffset = GameState.PicX * -1
        End Select

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerDir(ByRef data() As Byte)
        Dim dir As Integer, i As Integer
        Dim buffer As New ByteStream(data)
        i = buffer.ReadInt32
        dir = buffer.ReadInt32

        SetPlayerDir(i, dir)

        With Type.Player(i)
            .XOffset = 0
            .YOffset = 0
            .Moving = 0
        End With

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerExp(ByRef data() As Byte)
        Dim index As Integer, tnl As Integer
        Dim buffer As New ByteStream(data)

        index = buffer.ReadInt32
        SetPlayerExp(index, buffer.ReadInt32)

        tnl = buffer.ReadInt32
        GameState.NextlevelExp = tnl

        ' set max width
        If GetPlayerLevel(GameState.MyIndex) <= MAX_LEVEL Then
            If GetPlayerExp(GameState.MyIndex) > 0 Then
                GameState.BarWidth_GuiEXP_Max = ((GetPlayerExp(GameState.MyIndex) / 209) / (tnl / 209)) * 209
            Else
                GameState.BarWidth_GuiEXP_Max = 0
            End If
        Else
            GameState.BarWidth_GuiEXP_Max = 209
        End If

        ' Update GUI
        UpdateStats_UI()

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerXY(ByRef data() As Byte)
        Dim x As Integer, y As Integer, dir As Integer, index As Integer
        Dim buffer As New ByteStream(data)

        index = buffer.ReadInt32
        x = buffer.ReadInt32
        y = buffer.ReadInt32
        dir = buffer.ReadInt32

        SetPlayerX(index, x)
        SetPlayerY(index, y)
        SetPlayerDir(index, dir)

        ' Make sure they aren't walking
        Type.Player(index).Moving = 0
        Type.Player(index).XOffset = 0
        Type.Player(index).YOffset = 0

        buffer.Dispose()
    End Sub
#End Region

End Module