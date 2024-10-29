Imports Mirage.Sharp.Asfw
Imports Core

Module Npc

#Region "Spawning"

    Sub SpawnAllMapNPCs()
        Dim i As Integer

        For i = 1 To MAX_MAPS
            SpawnMapNPCs(i)
        Next

    End Sub

    Sub SpawnMapNPCs(MapNum As Integer)
        Dim i As Integer

        For i = 1 To MAX_MAP_NPCS
            SpawnNpc(i, mapNum)
        Next

    End Sub

    Friend Sub SpawnNpc(MapNpcNum As Integer, mapNum As Integer)
        Dim buffer As New ByteStream(4)
        Dim npcNum As Integer
        Dim x As Integer
        Dim y As Integer
        Dim i = 0
        Dim spawned As Boolean

        If Type.Map(MapNum).NoRespawn Then Exit Sub

        npcNum = Type.Map(MapNum).NPC(MapNPCNum)

        If npcNum > 0 Then
            If Not Type.NPC(NPCNum).SpawnTime = Core.Time.Instance.TimeOfDay And Type.NPC(NPCNum).SpawnTime <> 0 Then
                ClearMapNPC(MapNpcNum, mapNum)
                SendMapNPCsToMap(MapNum)
                Exit Sub
            End If

            MapNPC(MapNum).NPC(MapNPCNum).Num = npcNum
            MapNPC(MapNum).NPC(MapNPCNum).Target = 0
            MapNPC(MapNum).NPC(MapNPCNum).TargetType = 0 ' clear

            For i = 1 To VitalType.Count - 1
                MapNPC(MapNum).NPC(MapNPCNum).Vital(i) = GetNpcMaxVital(npcNum, i)
            Next

            MapNPC(MapNum).NPC(MapNPCNum).Dir = Int(Rnd() * 4)

            'Check if theres a spawn tile for the specific npc
            For x = 0 To Type.Map(MapNum).MaxX
                For y = 0 To Type.Map(MapNum).MaxY
                    If Type.Map(MapNum).Tile(x, y).Type = TileType.NPCSpawn Then
                        If Type.Map(MapNum).Tile(x, y).Data1 = mapNpcNum Then
                            MapNPC(MapNum).NPC(MapNPCNum).X = x
                            MapNPC(MapNum).NPC(MapNPCNum).Y = y
                            MapNPC(MapNum).NPC(MapNPCNum).Dir = Type.Map(MapNum).Tile(x, y).Data2
                            spawned = 1
                            Exit For
                        End If
                    End If
                Next y
            Next x

            If Not spawned Then
                ' Well try 100 times to randomly place the sprite
                While i < 1000
                    x = Random.NextDouble(0, Type.Map(MapNum).MaxX)
                    y = Random.NextDouble(0, Type.Map(MapNum).MaxY)

                    If x > Type.Map(MapNum).MaxX Then x = Type.Map(MapNum).MaxX
                    If y > Type.Map(MapNum).MaxY Then y = Type.Map(MapNum).MaxY

                    ' Check if the tile is walkable
                    If NpcTileIsOpen(MapNum, x, y) Then
                        MapNPC(MapNum).NPC(MapNPCNum).X = x
                        MapNPC(MapNum).NPC(MapNPCNum).Y = y
                        spawned = 1
                        Exit While
                    End If
                    i += 1
                End While
            End If

            ' Didn't spawn, so now we'll just try to find a free tile
            If Not spawned Then
                For x = 0 To Type.Map(MapNum).MaxX
                    For y = 0 To Type.Map(MapNum).MaxY
                        If NpcTileIsOpen(MapNum, x, y) Then
                            MapNPC(MapNum).NPC(MapNPCNum).X = x
                            MapNPC(MapNum).NPC(MapNPCNum).Y = y
                            spawned = 1
                        End If
                    Next
                Next
            End If

            ' If we suceeded in spawning then send it to everyone
            If spawned Then
                buffer.WriteInt32(ServerPackets.SSpawnNpc)
                buffer.WriteInt32(MapNPCNum)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Num)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).X)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Y)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Dir)
                
                For i = 1 To VitalType.Count - 1
                    buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Vital(i))
                Next

                SendDataToMap(MapNum, buffer.Data, buffer.Head)
            End If

            SendMapNPCVitals(MapNum, mapNpcNum)
        End If

        buffer.Dispose()
    End Sub

#End Region

#Region "Movement"

    Friend Function NpcTileIsOpen(MapNum As Integer, x As Integer, y As Integer) As Boolean
        Dim i As Integer
        NpcTileIsOpen = 1

        If PlayersOnMap(MapNum) Then
            For i = 1 To Socket.HighIndex
                If GetPlayerMap(i) = mapNum And GetPlayerX(i) = x And GetPlayerY(i) = y Then
                    NpcTileIsOpen = 0
                    Exit Function
                End If
            Next
        End If

        For LoopI = 1 To MAX_MAP_NPCS
           If MapNPC(MapNum).NPC(LoopI).Num > 0 And MapNPC(MapNum).NPC(LoopI).X = x And MapNPC(MapNum).NPC(LoopI).Y = y Then
                NpcTileIsOpen = 0
                Exit Function
            End If
        Next

        If Type.Map(MapNum).Tile(x, y).Type <> TileType.NPCSpawn And Type.Map(MapNum).Tile(x, y).Type <> TileType.Item And Type.Map(MapNum).Tile(x, y).Type <> TileType.None And Type.Map(MapNum).Tile(x, y).Type2 <> TileType.NPCSpawn And Type.Map(MapNum).Tile(x, y).Type2 <> TileType.Item And Type.Map(MapNum).Tile(x, y).Type2 <> TileType.None Then
            NpcTileIsOpen = 0
        End If

    End Function

    Function CanNpcMove(MapNum As Integer, MapNPCNum As Integer, Dir As Byte) As Boolean
        Dim i As Integer
        Dim n As Integer, n2 As Integer
        Dim x As Integer
        Dim y As Integer

        ' Check for subscript out of range
       If MapNum <= 0 Or mapNum > MAX_MAPS Or MapNPCNum <= 0 Or MapNPCNum > MAX_MAP_NPCS Or Dir <= DirectionType.None Or Dir > DirectionType.Left Then
            Exit Function
        End If

        x = MapNPC(MapNum).NPC(MapNPCNum).X
        y = MapNPC(MapNum).NPC(MapNPCNum).Y
        CanNpcMove = 1

        Select Case Dir
            Case DirectionType.Up
                ' Check to make sure not outside of boundaries
                If y > 0 Then
                    n = Type.Map(MapNum).Tile(x, y - 1).Type
                    n2 = Type.Map(MapNum).Tile(x, y - 1).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.None And n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.None And n2 <> TileType.Item And n2 <> TileType.NPCSpawn Then
                        CanNpcMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = MapNPC(MapNum).NPC(MapNPCNum).X) And (GetPlayerY(i) = MapNPC(MapNum).NPC(MapNPCNum).Y - 1) Then
                                CanNpcMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (i <> MapNPCNum) And (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = MapNPC(MapNum).NPC(MapNPCNum).X) And (MapNPC(MapNum).NPC(i).Y = MapNPC(MapNum).NPC(MapNPCNum).Y - 1) Then
                            CanNpcMove = 0
                            Exit Function
                        End If
                    Next
                Else
                    CanNpcMove = 0
                End If

            Case DirectionType.Down
                ' Check to make sure not outside of boundaries
                If y < Type.Map(MapNum).MaxY Then
                    n = Type.Map(MapNum).Tile(x, y + 1).Type
                    n2 = Type.Map(MapNum).Tile(x, y + 1).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.None And n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.None And n2 <> TileType.Item And n2 <> TileType.NPCSpawn Then
                        CanNpcMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = MapNPC(MapNum).NPC(MapNPCNum).X) And (GetPlayerY(i) = MapNPC(MapNum).NPC(MapNPCNum).Y + 1) Then
                                CanNpcMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (i <> MapNPCNum) And (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = MapNPC(MapNum).NPC(MapNPCNum).X) And (MapNPC(MapNum).NPC(i).Y = MapNPC(MapNum).NPC(MapNPCNum).Y + 1) Then
                            CanNpcMove = 0
                            Exit Function
                        End If
                    Next
                Else
                    CanNpcMove = 0
                End If

            Case DirectionType.Left
                ' Check to make sure not outside of boundaries
                If x > 0 Then
                    n = Type.Map(MapNum).Tile(x - 1, y).Type
                    n2 = Type.Map(MapNum).Tile(x - 1, y).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.None And n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.None And n2 <> TileType.Item And n2 <> TileType.NPCSpawn Then
                        CanNpcMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = MapNPC(MapNum).NPC(MapNPCNum).X - 1) And (GetPlayerY(i) = MapNPC(MapNum).NPC(MapNPCNum).Y) Then
                                CanNpcMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (i <> MapNPCNum) And (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = MapNPC(MapNum).NPC(MapNPCNum).X - 1) And (MapNPC(MapNum).NPC(i).Y = MapNPC(MapNum).NPC(MapNPCNum).Y) Then
                            CanNpcMove = 0
                            Exit Function
                        End If
                    Next
                Else
                    CanNpcMove = 0
                End If

            Case DirectionType.Right
                ' Check to make sure not outside of boundaries
                If x < Type.Map(MapNum).MaxX Then
                    n = Type.Map(MapNum).Tile(x + 1, y).Type
                    n2 = Type.Map(MapNum).Tile(x + 1, y).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.None And n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.None And n2 <> TileType.Item And n2 <> TileType.NPCSpawn Then
                        CanNpcMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = MapNPC(MapNum).NPC(MapNPCNum).X + 1) And (GetPlayerY(i) = MapNPC(MapNum).NPC(MapNPCNum).Y) Then
                                CanNpcMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (i <> MapNPCNum) And (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = MapNPC(MapNum).NPC(MapNPCNum).X + 1) And (MapNPC(MapNum).NPC(i).Y = MapNPC(MapNum).NPC(MapNPCNum).Y) Then
                            CanNpcMove = 0
                            Exit Function
                        End If
                    Next
                Else
                    CanNpcMove = 0
                End If

        End Select

       If MapNPC(MapNum).NPC(MapNPCNum).SkillBuffer > 0 Then CanNpcMove = 0

    End Function

    Sub NpcMove(MapNum As Integer, MapNPCNum As Integer, Dir As Integer, Movement As Integer)
        Dim buffer As New ByteStream(4)

        ' Check for subscript out of range
       If MapNum <= 0 Or mapNum > MAX_MAPS Or MapNPCNum <= 0 Or MapNPCNum > MAX_MAP_NPCS Or Dir <= DirectionType.None Or Dir > DirectionType.Left Or Movement < 0 Or Movement > 2 Then
            Exit Sub
        End If

        MapNPC(MapNum).NPC(MapNPCNum).Dir = Dir

        Select Case Dir
            Case DirectionType.Up
                MapNPC(MapNum).NPC(MapNPCNum).Y = MapNPC(MapNum).NPC(MapNPCNum).Y - 1

                buffer.WriteInt32(ServerPackets.SNpcMove)
                buffer.WriteInt32(MapNPCNum)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).X)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Y)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Dir)
                buffer.WriteInt32(Movement)

                SendDataToMap(MapNum, buffer.Data, buffer.Head)
            Case DirectionType.Down
                MapNPC(MapNum).NPC(MapNPCNum).Y = MapNPC(MapNum).NPC(MapNPCNum).Y + 1

                buffer.WriteInt32(ServerPackets.SNpcMove)
                buffer.WriteInt32(MapNPCNum)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).X)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Y)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Dir)
                buffer.WriteInt32(Movement)

                SendDataToMap(MapNum, buffer.Data, buffer.Head)
            Case DirectionType.Left
                MapNPC(MapNum).NPC(MapNPCNum).X = MapNPC(MapNum).NPC(MapNPCNum).X - 1

                buffer.WriteInt32(ServerPackets.SNpcMove)
                buffer.WriteInt32(MapNPCNum)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).X)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Y)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Dir)
                buffer.WriteInt32(Movement)

                SendDataToMap(MapNum, buffer.Data, buffer.Head)
            Case DirectionType.Right
                MapNPC(MapNum).NPC(MapNPCNum).X = MapNPC(MapNum).NPC(MapNPCNum).X + 1

                buffer.WriteInt32(ServerPackets.SNpcMove)
                buffer.WriteInt32(MapNPCNum)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).X)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Y)
                buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Dir)
                buffer.WriteInt32(Movement)

                SendDataToMap(MapNum, buffer.Data, buffer.Head)
        End Select

        buffer.Dispose()
    End Sub

    Sub NpcDir(MapNum As Integer, MapNPCNum As Integer, Dir As Integer)
        Dim buffer As New ByteStream(4)

        ' Check for subscript out of range
       If MapNum <= 0 Or mapNum > MAX_MAPS Or MapNPCNum <= 0 Or MapNPCNum > MAX_MAP_NPCS Or Dir <= DirectionType.None Or Dir > DirectionType.Left Then
            Exit Sub
        End If

        MapNPC(MapNum).NPC(MapNPCNum).Dir = Dir

        buffer.WriteInt32(ServerPackets.SNpcDir)
        buffer.WriteInt32(MapNPCNum)
        buffer.WriteInt32(Dir)

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

#End Region

#Region "Npcombat"

    Friend Sub TryNpcAttackPlayer(MapNpcNum As Integer, index As Integer)

        Dim mapNum As Integer, npcnum As Integer, Damage As Integer, i As Integer, armor As Integer

        ' Can the npc attack the player?
        If CanNpcAttackPlayer(MapNpcNum, index) Then
            mapNum = GetPlayerMap(index)
            npcnum = MapNPC(MapNum).NPC(MapNPCNum).Num

            ' check if PLAYER can avoid the attack
            If CanPlayerDodge(index) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, 1, (Type.Player(index).X * 32), (Type.Player(index).Y * 32))
                Exit Sub
            End If

            If CanPlayerParry(index) Then
                SendActionMsg(MapNum, "Parry!", ColorType.Pink, 1, (Type.Player(index).X * 32), (Type.Player(index).Y * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            Damage = GetNpcDamage(npcnum)

            If CanPlayerBlockHit(index) Then
                SendActionMsg(MapNum, "Block!", ColorType.Pink, 1, (Type.Player(index).X * 32), (Type.Player(index).Y * 32))
                Exit Sub
            Else

                For i = 2 To EquipmentType.Count - 1 ' start at 2, so we skip weapon
                    If GetPlayerEquipment(index, i) > 0 Then
                        armor = armor + Type.Item(GetPlayerEquipment(index, i)).Data2
                    End If
                Next
                ' take away armour
                Damage = Damage - ((GetPlayerStat(index, StatType.Spirit) * 2) + (GetPlayerLevel(index) * 2) + armor)

                ' * 1.5 if crit hit
                If CanNpcCrit(npcnum) Then
                    Damage = Damage * 1.5
                    SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
                End If

            End If

            If Damage > 0 Then
                NpcAttackPlayer(MapNpcNum, index, Damage)
            End If

        End If

    End Sub

    Function CanNpcAttackPlayer(MapNpcNum As Integer, index As Integer) As Boolean
        Dim mapNum As Integer
        Dim NpcNum As Integer

        ' Check for subscript out of range
       If MapNpcNum <= 0 Or MapNPCNum > MAX_MAP_NPCS Or Not IsPlaying(index) Then
            Exit Function
        End If

        ' Check for subscript out of range
       If MapNPC(GetPlayerMap(index)).NPC(MapNPCNum).Num <= 0 Then
            Exit Function
        End If

        mapNum = GetPlayerMap(index)
        NpcNum = MapNPC(MapNum).NPC(MapNPCNum).Num

        ' Make sure the npc isn't already dead
       If MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) <= 0 Then
            Exit Function
        End If

        ' Make sure npcs dont attack more then once a second
        If GetTimeMs() < MapNPC(MapNum).NPC(MapNPCNum).AttackTimer + 1000 Then
            Exit Function
        End If

        ' Make sure we dont attack the player if they are switching maps
        If TempPlayer(index).GettingMap = 1 Then
            Exit Function
        End If

        MapNPC(MapNum).NPC(MapNPCNum).AttackTimer = GetTimeMs()

        ' Make sure they are on the same map
        If IsPlaying(index) Then
            If NpcNum > 0 Then

                ' Check if at same coordinates
                If (GetPlayerY(index) + 1 = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPlayerX(index) = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                    CanNpcAttackPlayer = 1
                Else

                    If (GetPlayerY(index) - 1 = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPlayerX(index) = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                        CanNpcAttackPlayer = 1
                    Else

                        If (GetPlayerY(index) = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPlayerX(index) + 1 = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                            CanNpcAttackPlayer = 1
                        Else

                            If (GetPlayerY(index) = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPlayerX(index) - 1 = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                                CanNpcAttackPlayer = 1
                            End If
                        End If
                    End If
                End If
            End If
        End If

    End Function

    Function CanNpcAttackNpc(MapNum As Integer, Attacker As Integer, Victim As Integer) As Boolean
        Dim aNpcNum As Integer, vNpcNum As Integer, VictimX As Integer
        Dim VictimY As Integer, AttackerX As Integer, AttackerY As Integer

        CanNpcAttackNpc = 0

        ' Check for subscript out of range
        If Attacker <= 0 Or Attacker > MAX_MAP_NPCS Then
            Exit Function
        End If

        If Victim <= 0 Or Victim > MAX_MAP_NPCS Then
            Exit Function
        End If

        ' Check for subscript out of range
       If MapNPC(MapNum).NPC(Attacker).Num <= 0 Then
            Exit Function
        End If

        ' Check for subscript out of range
       If MapNPC(MapNum).NPC(Victim).Num <= 0 Then
            Exit Function
        End If

        aNpcNum = MapNPC(MapNum).NPC(Attacker).Num
        vNpcNum = MapNPC(MapNum).NPC(Victim).Num

        If aNpcNum <= 0 Then Exit Function
        If vNpcNum <= 0 Then Exit Function

        ' Make sure the npcs arent already dead
       If MapNPC(MapNum).NPC(Attacker).Vital(VitalType.HP) < 0 Then
            Exit Function
        End If

        ' Make sure the npc isn't already dead
       If MapNPC(MapNum).NPC(Victim).Vital(VitalType.HP) < 0 Then
            Exit Function
        End If

        ' Make sure npcs dont attack more then once a second
        If GetTimeMs() < MapNPC(MapNum).NPC(Attacker).AttackTimer + 1000 Then
            Exit Function
        End If

        MapNPC(MapNum).NPC(Attacker).AttackTimer = GetTimeMs()

        AttackerX = MapNPC(MapNum).NPC(Attacker).X
        AttackerY = MapNPC(MapNum).NPC(Attacker).Y
        VictimX = MapNPC(MapNum).NPC(Victim).X
        VictimY = MapNPC(MapNum).NPC(Victim).Y

        ' Check if at same coordinates
        If (VictimY + 1 = AttackerY) And (VictimX = AttackerX) Then
            CanNpcAttackNpc = 1
        Else

            If (VictimY - 1 = AttackerY) And (VictimX = AttackerX) Then
                CanNpcAttackNpc = 1
            Else

                If (VictimY = AttackerY) And (VictimX + 1 = AttackerX) Then
                    CanNpcAttackNpc = 1
                Else

                    If (VictimY = AttackerY) And (VictimX - 1 = AttackerX) Then
                        CanNpcAttackNpc = 1
                    End If
                End If
            End If
        End If

    End Function

    Friend Sub NpcAttackPlayer(MapNpcNum As Integer, Victim As Integer, Damage As Integer)
        Dim Name As String, mapNum As Integer
        Dim z As Integer, InvCount As Integer, EqCount As Integer, j As Integer, x As Integer
        Dim buffer As New ByteStream(4)

        ' Check for subscript out of range
       If MapNpcNum <= 0 Or MapNPCNum > MAX_MAP_NPCS Or IsPlaying(Victim) = 0 Then Exit Sub

        ' Check for subscript out of range
       If MapNPC(GetPlayerMap(Victim)).NPC(MapNPCNum).Num <= 0 Then Exit Sub

        mapNum = GetPlayerMap(Victim)
        Name = Type.NPC(MapNPC(MapNum).NPC(MapNPCNum).Num).Name

        ' Send this packet so they can see the npc attacking
        buffer.WriteInt32(ServerPackets.SNpcAttack)
        buffer.WriteInt32(MapNPCNum)
        SendDataToMap(MapNum, buffer.Data, buffer.Head)
        buffer.Dispose()

        If Damage <= 0 Then Exit Sub

        ' set the regen timer
        MapNPC(MapNum).NPC(MapNPCNum).StopRegen = 1
        MapNPC(MapNum).NPC(MapNPCNum).StopRegenTimer = GetTimeMs()

        If Damage >= GetPlayerVital(Victim, VitalType.HP) Then
            ' Say damage
            SendActionMsg(GetPlayerMap(Victim), "-" & GetPlayerVital(Victim, VitalType.HP), ColorType.BrightRed, 1, (GetPlayerX(Victim) * 32), (GetPlayerY(Victim) * 32))

            ' Set NPC target to 0
            MapNPC(MapNum).NPC(MapNPCNum).Target = 0
            MapNPC(MapNum).NPC(MapNPCNum).TargetType = 0

            ' kill player
            KillPlayer(Victim)

            ' Player is dead
            GlobalMsg(GetPlayerName(Victim) & " has been killed by " & Name)
        Else
            ' Player not dead, just do the damage
            SetPlayerVital(Victim, VitalType.HP, GetPlayerVital(Victim, VitalType.HP) - Damage)
            SendVital(Victim, VitalType.HP)
            SendAnimation(MapNum, Type.NPC(MapNPC(GetPlayerMap(Victim)).NPC(MapNPCNum).Num).Animation, 0, 0, TargetType.Player, Victim)

            ' send the sound
            'SendMapSound Victim, GetPlayerX(Victim), GetPlayerY(Victim), SoundEntity.seNpc, MapNPC(MapNum).NPC(MapNPCNum).Num

            ' Say damage
            SendActionMsg(GetPlayerMap(Victim), "-" & Damage, ColorType.BrightRed, 1, (GetPlayerX(Victim) * 32), (GetPlayerY(Victim) * 32))
            SendBlood(GetPlayerMap(Victim), GetPlayerX(Victim), GetPlayerY(Victim))

            ' set the regen timer
            TempPlayer(Victim).StopRegen = 1
            TempPlayer(Victim).StopRegenTimer = GetTimeMs()

        End If

    End Sub

    Sub NpcAttackNpc(MapNum As Integer, Attacker As Integer, Victim As Integer, Damage As Integer)
        Dim buffer As New ByteStream(4)
        Dim aNpcNum As Integer
        Dim vNpcNum As Integer
        Dim n As Integer

        If Attacker <= 0 Or Attacker > MAX_MAP_NPCS Then Exit Sub
        If Victim <= 0 Or Victim > MAX_MAP_NPCS Then Exit Sub

        If Damage <= 0 Then Exit Sub

        aNpcNum = MapNPC(MapNum).NPC(Attacker).Num
        vNpcNum = MapNPC(MapNum).NPC(Victim).Num

        If aNpcNum <= 0 Then Exit Sub
        If vNpcNum <= 0 Then Exit Sub

        ' Send this packet so they can see the person attacking
        buffer.WriteInt32(ServerPackets.SNpcAttack)
        buffer.WriteInt32(Attacker)
        SendDataToMap(MapNum, buffer.Data, buffer.Head)
        buffer.Dispose()

        If Damage >= MapNPC(MapNum).NPC(Victim).Vital(VitalType.HP) Then
            SendActionMsg(MapNum, "-" & Damage, ColorType.BrightRed, 1, (MapNPC(MapNum).NPC(Victim).X * 32), (MapNPC(MapNum).NPC(Victim).Y * 32))
            SendBlood(MapNum, MapNPC(MapNum).NPC(Victim).X, MapNPC(MapNum).NPC(Victim).Y)

            ' npc is dead.

            ' Set NPC target to 0
            MapNPC(MapNum).NPC(Attacker).Target = 0
            MapNPC(MapNum).NPC(Attacker).TargetType = 0

            ' Drop the goods if they get it
            Dim tmpitem = Random.NextDouble(1, 5)
            n = Int(Rnd() * Type.NPC(vNpcNum).DropChance(tmpitem)) + 1
            If n = 1 Then
                SpawnItem(Type.NPC(vNpcNum).DropItem(tmpitem), Type.NPC(vNpcNum).DropItemValue(tmpitem), mapNum, MapNPC(MapNum).NPC(Victim).X, MapNPC(MapNum).NPC(Victim).Y)
            End If

            ' Reset victim's stuff so it dies in loop
            MapNPC(MapNum).NPC(Victim).Num = 0
            MapNPC(MapNum).NPC(Victim).SpawnWait = GetTimeMs()
            MapNPC(MapNum).NPC(Victim).Vital(VitalType.HP) = 0

            ' send npc death packet to map
            buffer = New ByteStream(4)
            buffer.WriteInt32(ServerPackets.SNpcDead)
            buffer.WriteInt32(Victim)
            SendDataToMap(MapNum, buffer.Data, buffer.Head)
            buffer.Dispose()
        Else
            ' npc not dead, just do the damage
            MapNPC(MapNum).NPC(Victim).Vital(VitalType.HP) = MapNPC(MapNum).NPC(Victim).Vital(VitalType.HP) - Damage
            ' Say damage
            SendActionMsg(MapNum, "-" & Damage, ColorType.BrightRed, 1, (MapNPC(MapNum).NPC(Victim).X * 32), (MapNPC(MapNum).NPC(Victim).Y * 32))
            SendBlood(MapNum, MapNPC(MapNum).NPC(Victim).X, MapNPC(MapNum).NPC(Victim).Y)
        End If

    End Sub

    Friend Sub KnockBackNpc(index As Integer, NpcNum As Integer, Optional IsSkill As Integer = 0)
        If IsSkill > 0 Then
            For i = 0 To Type.Skill(IsSkill).KnockBackTiles
                If CanNpcMove(GetPlayerMap(index), NpcNum, GetPlayerDir(index)) Then
                    NpcMove(GetPlayerMap(index), NpcNum, GetPlayerDir(index), MovementType.Walking)
                End If
            Next
            MapNPC(GetPlayerMap(index)).NPC(NPCNum).StunDuration = 1
            MapNPC(GetPlayerMap(index)).NPC(NPCNum).StunTimer = GetTimeMs()
        Else
            If Type.Item(Global.Core.Commands.GetPlayerEquipment(index, Global.Core.Enum.EquipmentType.Weapon)).KnockBack = 1 Then
                For i = 0 To Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).KnockBackTiles
                    If CanNpcMove(GetPlayerMap(index), NpcNum, GetPlayerDir(index)) Then
                        NpcMove(GetPlayerMap(index), NpcNum, GetPlayerDir(index), MovementType.Walking)
                    End If
                Next
                MapNPC(GetPlayerMap(index)).NPC(NPCNum).StunDuration = 1
                MapNPC(GetPlayerMap(index)).NPC(NPCNum).StunTimer = GetTimeMs()
            End If
        End If
    End Sub

    Friend Function RandomNpcAttack(MapNum As Integer, MapNPCNum As Integer) As Integer
        Dim i As Integer, SkillList As New List(Of Byte)

        RandomNpcAttack = 0

       If MapNPC(MapNum).NPC(MapNPCNum).SkillBuffer > 0 Then Exit Function

        For i = 1 To MAX_NPC_SKILLS
            If Type.NPC(MapNPC(MapNum).NPC(MapNPCNum).Num).Skill(i) > 0 Then
                SkillList.Add(Type.NPC(MapNPC(MapNum).NPC(MapNPCNum).Num).Skill(i))
            End If
        Next

        If SkillList.Count > 1 Then
            RandomNpcAttack = SkillList(Random.NextDouble(0, SkillList.Count - 1))
        Else
            RandomNpcAttack = 0
        End If

    End Function

    Friend Function GetNpcSkill(NpcNum As Integer, skillslot As Integer) As Integer
        GetNpcSkill = Type.NPC(NPCNum).Skill(skillslot)
    End Function

    Friend Sub BufferNpcSkill(MapNum As Integer, MapNPCNum As Integer, skillslot As Integer)
        Dim skillnum As Integer
        Dim MPCost As Integer
        Dim SkillCastType As Integer
        Dim range As Integer
        Dim HasBuffered As Boolean

        Dim TargetType As Byte
        Dim Target As Integer

        ' Prevent subscript out of range
        If skillslot < 0 Or skillslot > MAX_NPC_SKILLS Then Exit Sub

        skillnum = GetNpcSkill(MapNPC(MapNum).NPC(MapNPCNum).Num, skillslot)

        If skillnum <= 0 Or skillnum > MAX_SKILLS Then Exit Sub

        ' see if cooldown has finished
       If MapNPC(MapNum).NPC(MapNPCNum).SkillCd(skillslot) > GetTimeMs() Then
            TryNpcAttackPlayer(MapNpcNum, MapNPC(MapNum).NPC(MapNPCNum).Target)
            Exit Sub
        End If

        MPCost = Type.Skill(skillNum).MpCost

        ' Check if they have enough MP
       If MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.SP) < MPCost Then Exit Sub

        ' find out what kind of skill it is! self cast, target or AOE
        If Type.Skill(skillNum).Range > 0 Then
            ' ranged attack, single target or aoe?
            If Not Type.Skill(skillNum).IsAoE Then
                SkillCastType = 2 ' targetted
            Else
                SkillCastType = 3 ' targetted aoe
            End If
        Else
            If Not Type.Skill(skillNum).IsAoE Then
                SkillCastType = 0 ' self-cast
            Else
                SkillCastType = 1 ' self-cast AoE
            End If
        End If

        TargetType = MapNPC(MapNum).NPC(MapNPCNum).TargetType
        Target = MapNPC(MapNum).NPC(MapNPCNum).Target
        range = Type.Skill(skillNum).Range
        HasBuffered = 0

        Select Case SkillCastType
            Case 0, 1 ' self-cast & self-cast AOE
                HasBuffered = 1
            Case 2, 3 ' targeted & targeted AOE
                ' check if have target
                If Not Target > 0 Then
                    Exit Sub
                End If
                If TargetType = Core.TargetType.Player Then
                    ' if have target, check in range
                    If Not IsInRange(range, MapNPC(MapNum).NPC(MapNPCNum).X, MapNPC(MapNum).NPC(MapNPCNum).Y, GetPlayerX(Target), GetPlayerY(Target)) Then
                        Exit Sub
                    Else
                        HasBuffered = 1
                    End If
                ElseIf TargetType = Core.TargetType.NPC Then
                    '' if have target, check in range
                    'If Not isInRange(range, GetPlayerX(Index), GetPlayerY(Index), MapNPC(MapNum).NPC(Target).x, MapNPC(MapNum).NPC(Target).y) Then
                    '    PlayerMsg(Index, "Target not in range.")
                    '    HasBuffered = 0
                    'Else
                    '    ' go through skill Type
                    '    If Type.Skill(skillNum).Type <> SkillType.DAMAGEHP And Type.Skill(skillNum).Type <> SkillType.DAMAGEMP Then
                    '        HasBuffered = 1
                    '    Else
                    '        If CanAttackNpc(Index, Target, True) Then
                    '            HasBuffered = 1
                    '        End If
                    '    End If
                    'End If
                End If
        End Select

        If HasBuffered Then
            SendAnimation(MapNum, Type.Skill(skillNum).CastAnim, 0, 0, Core.TargetType.Player, Target)
            MapNPC(MapNum).NPC(MapNPCNum).SkillBuffer = skillslot
            MapNPC(MapNum).NPC(MapNPCNum).SkillBufferTimer = GetTimeMs()
            Exit Sub
        End If
    End Sub

    Friend Function CanNpcBlock(npcnum As Integer) As Boolean
        Dim rate As Integer
        Dim stat As Integer
        Dim rndNum As Integer

        CanNpcBlock = 0

        stat = Type.NPC(NPCNum).Stat(StatType.Luck) / 5  'guessed shield agility
        rate = stat / 12.08
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then CanNpcBlock = 1

    End Function

    Friend Function CanNpcCrit(npcnum As Integer) As Boolean
        Dim rate As Integer
        Dim rndNum As Integer

        CanNpcCrit = 0

        rate = Type.NPC(NPCNum).Stat(StatType.Luck) / 3
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then CanNpcCrit = 1

    End Function

    Friend Function CanNpcDodge(npcnum As Integer) As Boolean
        Dim rate As Integer
        Dim rndNum As Integer

        CanNpcDodge = 0

        rate = Type.NPC(NPCNum).Stat(StatType.Luck) / 4
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then CanNpcDodge = 1

    End Function

    Friend Function CanNpcParry(npcnum As Integer) As Boolean
        Dim rate As Integer
        Dim rndNum As Integer

        CanNpcParry = 0

        rate = Type.NPC(NPCNum).Stat(StatType.Luck) / 6
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then CanNpcParry = 1

    End Function

    Function GetNpcDamage(npcnum As Integer) As Integer

        GetNpcDamage = (Type.NPC(NPCNum).Stat(StatType.Strength) * 2) + (Type.NPC(NPCNum).Damage * 2) + (Type.NPC(NPCNum).Level * 3) + Random.NextDouble(1, 20)

    End Function

    Friend Function IsNpcDead(MapNum As Integer, MapNPCNum As Integer)
        IsNpcDead = 0
       If MapNum <= 0 Or mapNum > MAX_MAPS Or MapNPCNum <= 0 Or MapNPCNum > MAX_MAP_NPCS Then Exit Function
       If MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) <= 0 Then IsNpcDead = 1
    End Function

    Friend Sub DropNpcItems(MapNum As Integer, MapNPCNum As Integer)
        Dim NpcNum = MapNPC(MapNum).NPC(MapNPCNum).Num
        Dim tmpitem = Random.NextDouble(1, 5)
        Dim n = Int(Rnd() * Type.NPC(NPCNum).DropChance(tmpitem)) + 1

        If n = 1 Then
            SpawnItem(Type.NPC(NPCNum).DropItem(tmpitem), Type.NPC(NPCNum).DropItemValue(tmpitem), mapNum, MapNPC(MapNum).NPC(MapNPCNum).X, MapNPC(MapNum).NPC(MapNPCNum).Y)
        End If
    End Sub

#End Region

#Region "Outgoing Packets"

    Sub SendMapNPCsToMap(MapNum As Integer)
        Dim i As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapNPCData)

        For i = 1 To MAX_MAP_NPCS
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Num)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).X)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Y)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Dir)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Vital(VitalType.HP))
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Vital(VitalType.SP))
        Next

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

#End Region

#Region "Incoming Packets"

    Sub Packet_EditNpc(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.NPC)

        If user <> "" Then
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        TempPlayer(index).Editor = EditorType.NPC

        SendItems(index)
        SendAnimations(index)
        SendSkills(index)
        SendNpcs(index)

        Dim Buffer = New ByteStream(4)
        Buffer.WriteInt32(ServerPackets.SNpcEditor)
        Socket.SendDataTo(index, Buffer.Data, Buffer.Head)

        Buffer.Dispose()
    End Sub

    Sub Packet_SaveNPC(index As Integer, ByRef data() As Byte)
        Dim NpcNum As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        NpcNum = buffer.ReadInt32

        ' Update the Npc
        Type.NPC(NPCNum).Animation = buffer.ReadInt32()
        Type.NPC(NPCNum).AttackSay = buffer.ReadString()
        Type.NPC(NPCNum).Behaviour = buffer.ReadByte()

        For i = 1 To MAX_DROP_ITEMS
            Type.NPC(NPCNum).DropChance(i) = buffer.ReadInt32()
            Type.NPC(NPCNum).DropItem(i) = buffer.ReadInt32()
            Type.NPC(NPCNum).DropItemValue(i) = buffer.ReadInt32()
        Next

        Type.NPC(NPCNum).Exp = buffer.ReadInt32()
        Type.NPC(NPCNum).Faction = buffer.ReadByte()
        Type.NPC(NPCNum).HP = buffer.ReadInt32()
        Type.NPC(NPCNum).Name = buffer.ReadString()
        Type.NPC(NPCNum).Range = buffer.ReadByte()
        Type.NPC(NPCNum).SpawnTime = buffer.ReadByte()
        Type.NPC(NPCNum).SpawnSecs = buffer.ReadInt32()
        Type.NPC(NPCNum).Sprite = buffer.ReadInt32()

        For i = 1 To StatType.Count - 1
            Type.NPC(NPCNum).Stat(i) = buffer.ReadByte()
        Next

        For i = 1 To MAX_NPC_SKILLS
            Type.NPC(NPCNum).Skill(i) = buffer.ReadByte()
        Next

        Type.NPC(NPCNum).Level = buffer.ReadInt32()
        Type.NPC(NPCNum).Damage = buffer.ReadInt32()

        ' Save it
        SendUpdateNpcToAll(NpcNum)
        SaveNPC(NPCNum)
        Addlog(GetPlayerLogin(index) & " saved Npc #" & NpcNum & ".", ADMIN_LOG)

        buffer.Dispose()
    End Sub

    Sub SendNpcs(index As Integer)
        Dim i As Integer

        For i = 1 To MAX_NPCS
            If Len(Type.NPC(i).Name) > 0 Then
                SendUpdateNpcTo(index, i)
            End If
        Next

    End Sub

    Sub SendUpdateNpcTo(index As Integer, NpcNum As Integer)
        Dim buffer As ByteStream, i As Integer

        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdateNpc)

        buffer.WriteInt32(NpcNum)
        buffer.WriteInt32(Type.NPC(NPCNum).Animation)
        buffer.WriteString(Type.NPC(NPCNum).AttackSay)
        buffer.WriteByte(Type.NPC(NPCNum).Behaviour)

        For i = 1 To MAX_DROP_ITEMS
            buffer.WriteInt32(Type.NPC(NPCNum).DropChance(i))
            buffer.WriteInt32(Type.NPC(NPCNum).DropItem(i))
            buffer.WriteInt32(Type.NPC(NPCNum).DropItemValue(i))
        Next

        buffer.WriteInt32(Type.NPC(NPCNum).Exp)
        buffer.WriteByte(Type.NPC(NPCNum).Faction)
        buffer.WriteInt32(Type.NPC(NPCNum).HP)
        buffer.WriteString(Type.NPC(NPCNum).Name)
        buffer.WriteByte(Type.NPC(NPCNum).Range)
        buffer.WriteByte(Type.NPC(NPCNum).SpawnTime)
        buffer.WriteInt32(Type.NPC(NPCNum).SpawnSecs)
        buffer.WriteInt32(Type.NPC(NPCNum).Sprite)

        For i = 1 To StatType.Count - 1
            buffer.WriteByte(Type.NPC(NPCNum).Stat(i))
        Next

        For i = 1 To MAX_NPC_SKILLS
            buffer.WriteByte(Type.NPC(NPCNum).Skill(i))
        Next

        buffer.WriteInt32(Type.NPC(NPCNum).Level)
        buffer.WriteInt32(Type.NPC(NPCNum).Damage)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateNpcToAll(NpcNum As Integer)
        Dim buffer As ByteStream, i As Integer

        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdateNpc)

        buffer.WriteInt32(NpcNum)
        buffer.WriteInt32(Type.NPC(NPCNum).Animation)
        buffer.WriteString(Type.NPC(NPCNum).AttackSay)
        buffer.WriteByte(Type.NPC(NPCNum).Behaviour)

        For i = 1 To MAX_DROP_ITEMS
            buffer.WriteInt32(Type.NPC(NPCNum).DropChance(i))
            buffer.WriteInt32(Type.NPC(NPCNum).DropItem(i))
            buffer.WriteInt32(Type.NPC(NPCNum).DropItemValue(i))
        Next

        buffer.WriteInt32(Type.NPC(NPCNum).Exp)
        buffer.WriteByte(Type.NPC(NPCNum).Faction)
        buffer.WriteInt32(Type.NPC(NPCNum).HP)
        buffer.WriteString(Type.NPC(NPCNum).Name)
        buffer.WriteByte(Type.NPC(NPCNum).Range)
        buffer.WriteByte(Type.NPC(NPCNum).SpawnTime)
        buffer.WriteInt32(Type.NPC(NPCNum).SpawnSecs)
        buffer.WriteInt32(Type.NPC(NPCNum).Sprite)

        For i = 1 To StatType.Count - 1
            buffer.WriteByte(Type.NPC(NPCNum).Stat(i))
        Next

        For i = 1 To MAX_NPC_SKILLS
            buffer.WriteByte(Type.NPC(NPCNum).Skill(i))
        Next

        buffer.WriteInt32(Type.NPC(NPCNum).Level)
        buffer.WriteInt32(Type.NPC(NPCNum).Damage)

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendMapNPCsTo(index As Integer, mapNum As Integer)
        Dim i As Integer
        Dim buffer As ByteStream
        buffer = New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapNPCData)

        For i = 1 To MAX_MAP_NPCS
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Num)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).X)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Y)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Dir)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Vital(VitalType.HP))
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Vital(VitalType.SP))
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendMapNPCTo(MapNum As Integer, MapNPCNum As Integer)
        Dim buffer As ByteStream
        buffer = New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapNPCUpdate)

        buffer.WriteInt32(MapNPCNum)

        With MapNPC(MapNum).NPC(MapNPCNum)
            buffer.WriteInt32(.Num)
            buffer.WriteInt32(.X)
            buffer.WriteInt32(.Y)
            buffer.WriteInt32(.Dir)
            buffer.WriteInt32(.Vital(VitalType.HP))
            buffer.WriteInt32(.Vital(VitalType.SP))
        End With

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendMapNPCVitals(MapNum As Integer, MapNPCNum As Byte)
        Dim i As Integer
        Dim buffer As ByteStream
        buffer = New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapNPCVitals)
        buffer.WriteInt32(MapNPCNum)
        
        For i = 1 To VitalType.Count - 1
            buffer.WriteInt32(MapNPC(MapNum).NPC(MapNPCNum).Vital(i))
        Next

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendNpcAttack(index As Integer, NpcNum As Integer)
        Dim Buffer = New ByteStream(4)
        Buffer.WriteInt32(ServerPackets.SAttack)

        Buffer.WriteInt32(NpcNum)
        SendDataToMap(GetPlayerMap(index), Buffer.Data, Buffer.Head)
        Buffer.Dispose()
    End Sub

    Sub SendNpcDead(MapNum As Integer, index As Integer)
        Dim Buffer = New ByteStream(4)
        Buffer.WriteInt32(ServerPackets.SNpcDead)

        Buffer.WriteInt32(index)
        SendDataToMap(MapNum, Buffer.Data, Buffer.Head)
        Buffer.Dispose()
    End Sub

#End Region

End Module