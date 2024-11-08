Imports System.Linq
Imports System.Threading
Imports Core

Module [Loop]

    Sub ServerLoop()
        Dim tick As Integer
        Dim tmr25 As Integer, tmr300 As Integer
        Dim tmr500 As Integer, tmr1000 As Integer
        Dim lastUpdateSavePlayers As Integer
        Dim lastUpdateMapSpawnItems As Integer
        Dim lastUpdatePlayerVitals As Integer

        Do
            ' Update our current tick value.
            tick = GetTimeMs()

            ' Don't process anything else if we're going down.
            If ServerDestroyed Then Stop

            ' Get all our online players.
            Dim onlinePlayers = TempPlayer.Where(Function(player) player.InGame).Select(Function(player, index) New With {Key .Index = index + 1, player}).ToArray()

            CheckShutDownCountDown()

            If tick > tmr25 Then
                ' Check if any of our players has completed casting and get their skill going if they have.
                Dim playerskills = (
                    From p In onlinePlayers
                    Where p.player.SkillBuffer > 0 And GetTimeMs() > (p.player.SkillBufferTimer + Type.Skill(p.player.SkillBuffer).CastTime * 1000)
                    Select New With {p.Index, Key .Success = HandleCastSkill(p.Index)}
                ).ToArray()

                ' Check if we need to clear any of our players from being stunned.
                Dim playerstuns = (
                    From p In onlinePlayers
                    Where p.player.StunDuration > 0 And p.player.StunTimer + (p.player.StunDuration * 1000)
                    Select New With {p.Index, Key .Success = HandleClearStun(p.Index)}
                ).ToArray()

                ' Check if any of our pets has completed casting and get their skill going if they have.
                Dim petskills = (
                From p In onlinePlayers
                Where Type.Player(p.Index).Pet.Alive = 1 And TempPlayer(p.Index).PetskillBuffer.Skill > 0 And GetTimeMs() > p.player.PetskillBuffer.Timer + (Type.Skill(Type.Player(p.Index).Pet.Skill(p.player.PetskillBuffer.Skill)).CastTime * 1000)
                Select New With {p.Index, Key .Success = HandlePetSkill(p.Index)}
                ).ToArray()

                ' Check if we need to clear any of our pets from being stunned.
                Dim petstuns = (
                    From p In onlinePlayers
                    Where p.player.PetStunDuration > 0 And p.player.PetStunTimer + (p.player.PetStunDuration * 1000)
                    Select New With {p.Index, Key .Success = HandleClearPetStun(p.Index)}
                ).ToArray()

                ' check pet regen timer
                Dim petregen = (
                    From p In onlinePlayers
                    Where p.player.PetstopRegen = 1 And p.player.PetstopRegenTimer + 5000 < GetTimeMs()
                    Select New With {p.Index, Key .Success = HandleStopPetRegen(p.Index)}
                ).ToArray()

                ' HoT and DoT logic
                'For x = 1 To MAX_COTS
                '    HandleDoT_Pet i, x
                '        HandleHoT_Pet i, x
                '    Next

                ' Update all our available events.
                UpdateEventLogic()

                ' Move the timer up 25ms.
                tmr25 = GetTimeMs() + 25
            End If

            If tick > tmr1000 Then
                Core.Time.Instance.Tick()

                ' Move the timer up 1000ms.
                tmr1000 = GetTimeMs() + 1000
            End If

            If tick > tmr500 Then
                ' Move the timer up 500ms.
                tmr500 = GetTimeMs() + 500
            End If

            If GetTimeMs() > tmr300 Then
                UpdateNpcAi()
                UpdatePetAi()
                tmr300 = GetTimeMs() + 300
            End If

            ' Checks to update player vitals every 5 seconds
            If tick > lastUpdatePlayerVitals Then
                UpdatePlayerVitals()
                lastUpdatePlayerVitals = GetTimeMs() + 5000
            End If

            ' Checks to spawn map items every 1 minute
            If tick > lastUpdateMapSpawnItems Then
                UpdateMapSpawnItems()
                lastUpdateMapSpawnItems = GetTimeMs() + 60000
            End If

            ' Checks to save players every 5 minutes
            If tick > lastUpdateSavePlayers Then
                UpdateSavePlayers()
                lastUpdateSavePlayers = GetTimeMs() + 300000
            End If

            Thread.Sleep(1)
        Loop
    End Sub

    Sub UpdateSavePlayers()
        Dim i As Integer

        If Socket.HighIndex() > 0 Then
            Call Global.System.Console.WriteLine("Saving all online players...")

            For i = 1 To Socket.HighIndex()
                SaveCharacter(i, TempPlayer(i).Slot)
                SaveBank(i)
            Next

        End If

    End Sub

    Private Sub UpdateMapSpawnItems()
        Dim x As Integer
        Dim y As Integer

        ' ///////////////////////////////////////////
        ' // This is used for respawning map items //
        ' ///////////////////////////////////////////
        For y = 1 To MAX_MAPS

            ' Make sure no one is on the map when it respawns
            If Not PlayersOnMap(y) Then

                ' Clear out unnecessary junk
                For x = 1 To MAX_MAP_ITEMS
                    ClearMapItem(x, y)
                Next

                ' Spawn the items
                SpawnMapItems(y)
                SendMapItemsToAll(y)
            End If

        Next

    End Sub

    Private Sub UpdatePlayerVitals()
        Dim i As Integer

        For i = 1 To Socket.HighIndex()
            If IsPlaying(i) Then
                For x = 1 To VitalType.Count - 1
                    If GetPlayerVital(i, x) <> GetPlayerMaxVital(i, x) Then
                        SetPlayerVital(i, x, GetPlayerVital(i, x) + GetPlayerVitalRegen(i, x))
                        SendVital(i, x)
                    End If
                Next
            End If
        Next

    End Sub

    Private Sub UpdateNpcAi()
        Dim i As Integer, x As Integer, n As Integer, x1 As Integer, y1 As Integer
        Dim mapNum As Integer, tickCount As Integer
        Dim damage As Integer
        Dim distanceX As Integer, distanceY As Integer
        Dim npcNum As Integer
        Dim target As Integer, targetType As Byte, targetX As Integer, targetY As Integer, targetVerify As Boolean
        Dim resourceIndex As Integer

        For mapNum = 1 To MAX_MAPS

            If ServerDestroyed Then Exit Sub

            ' items appearing to everyone
            For i = 1 To MAX_MAP_ITEMS
               If Type.MapItem(MapNum, i).Num > 0 Then
                   If Type.MapItem(MapNum, i).PlayerName <> "" Then
                        ' make item public?
                       If Type.MapItem(MapNum, i).PlayerTimer < GetTimeMs() Then
                            ' make it public
                            MapItem(MapNum, i).PlayerName = ""
                            MapItem(MapNum, i).PlayerTimer = 0
                            ' send updates to everyone
                            SendMapItemsToAll(MapNum)
                        End If
                        ' despawn item?
                       If Type.MapItem(MapNum, i).CanDespawn Then
                           If Type.MapItem(MapNum, i).DespawnTimer < GetTimeMs() Then
                                ' despawn it
                                ClearMapItem(i, mapNum)
                                ' send updates to everyone
                                SendMapItemsToAll(MapNum)
                            End If
                        End If
                    End If
                End If
            Next

            ' Respawning Resources
           If Type.MapResource(MapNum).ResourceCount > 0 Then
                For i = 1 To MapResource(MapNum).ResourceCount

                    resourceIndex = Type.Map(MapNum).Tile(Type.MapResource(MapNum).ResourceData(i).X, MapResource(MapNum).ResourceData(i).Y).Data1

                    If resourceIndex > 0 Then
                       If Type.MapResource(MapNum).ResourceData(i).State = 1 Or MapResource(MapNum).ResourceData(i).Health < 1 Then  ' dead or fucked up
                           If Type.MapResource(MapNum).ResourceData(i).Timer + (Type.Resource(resourceIndex).RespawnTime * 1000) < GetTimeMs() Then
                                MapResource(MapNum).ResourceData(i).Timer = GetTimeMs()
                                MapResource(MapNum).ResourceData(i).State = 0 ' normal
                                ' re-set health to resource root
                                MapResource(MapNum).ResourceData(i).Health = Type.Resource(resourceIndex).Health
                                SendMapResourceToMap(MapNum, i)
                            End If
                        End If
                    End If
                Next
            End If

            If ServerDestroyed Then Exit Sub

            If PlayersOnMap(MapNum) = 1 Then
                tickCount = GetTimeMs()

                For x = 1 To MAX_MAP_NPCS
                    npcNum = MapNPC(MapNum).NPC(x).Num

                    ' check if they've completed casting, and if so set the actual skill going
                   If MapNPC(MapNum).NPC(x).SkillBuffer > 0 And Type.Map(MapNum).NPC(x) > 0 And MapNPC(MapNum).NPC(x).Num > 0 Then
                        If GetTimeMs() > MapNPC(MapNum).NPC(x).SkillBufferTimer + (Type.Skill(Type.NPC(NPCNum).Skill(MapNPC(MapNum).NPC(x).SkillBuffer)).CastTime * 1000) Then
                            CastNpcSkill(x, mapNum, MapNPC(MapNum).NPC(x).SkillBuffer)
                            MapNPC(MapNum).NPC(x).SkillBuffer = 0
                            MapNPC(MapNum).NPC(x).SkillBufferTimer = 0
                        End If
                    Else
                        ' /////////////////////////////////////////
                        ' // This is used for ATTACKING ON SIGHT //
                        ' /////////////////////////////////////////
                        ' Make sure theres a npc with the map
                        If Type.Map(MapNum).NPC(x) > 0 And MapNPC(MapNum).NPC(x).Num > 0 Then

                            ' If the npc is a attack on sight, search for a player on the map
                            If Type.NPC(NPCNum).Behaviour = NpcBehavior.AttackOnSight Or Type.NPC(NPCNum).Behaviour = NpcBehavior.Guard Then

                                ' make sure it's not stunned
                                If Not MapNPC(MapNum).NPC(x).StunDuration > 0 Then

                                    For i = 1 To Socket.HighIndex()
                                        If IsPlaying(i) Then
                                            If GetPlayerMap(i) = mapNum And MapNPC(MapNum).NPC(x).Target = 0 And GetPlayerAccess(i) <= AccessType.Moderator Then
                                                If PetAlive(i) Then
                                                    n = Type.NPC(NPCNum).Range
                                                    distanceX = MapNPC(MapNum).NPC(x).X - Type.Player(i).Pet.X
                                                    distanceY = MapNPC(MapNum).NPC(x).Y - Type.Player(i).Pet.Y

                                                    ' Make sure we get a positive value
                                                    If distanceX < 0 Then distanceX *= -1
                                                    If distanceY < 0 Then distanceY *= -1

                                                    ' Are they in range?  if so GET'M!
                                                    If distanceX <= n And distanceY <= n Then
                                                        If Type.NPC(NPCNum).Behaviour = NpcBehavior.AttackOnSight Or GetPlayerPK(i) = i Then
                                                            If Len(Type.NPC(NPCNum).AttackSay) > 0 Then
                                                                PlayerMsg(i, Type.NPC(NPCNum).Name & " says: " & Type.NPC(NPCNum).AttackSay, ColorType.White)
                                                            End If
                                                            MapNPC(MapNum).NPC(x).TargetType = Core.TargetType.Pet
                                                            MapNPC(MapNum).NPC(x).Target = i
                                                        End If
                                                    End If
                                                Else
                                                    n = Type.NPC(NPCNum).Range
                                                    distanceX = MapNPC(MapNum).NPC(x).X - GetPlayerX(i)
                                                    distanceY = MapNPC(MapNum).NPC(x).Y - GetPlayerY(i)

                                                    ' Make sure we get a positive value
                                                    If distanceX < 0 Then distanceX *= -1
                                                    If distanceY < 0 Then distanceY *= -1

                                                    ' Are they in range?  if so GET'M!
                                                    If distanceX <= n And distanceY <= n Then
                                                        If Type.NPC(NPCNum).Behaviour = NpcBehavior.AttackOnSight Or GetPlayerPK(i) = 1 Then
                                                            If Type.NPC(NPCNum).AttackSay.Length > 0 Then
                                                                PlayerMsg(i, CheckGrammar(Type.NPC(NPCNum).Name, 1) & " says, '" & Type.NPC(NPCNum).AttackSay & "' to you.", ColorType.Yellow)
                                                            End If
                                                            MapNPC(MapNum).NPC(x).TargetType = Core.TargetType.Player
                                                            MapNPC(MapNum).NPC(x).Target = i
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next

                                    ' Check if target was found for NPC targetting
                                   If MapNPC(MapNum).NPC(x).Target = 0 And Type.NPC(NPCNum).Faction > 0 Then
                                        ' search for npc of another faction to target
                                        For i = 1 To MAX_MAP_NPCS
                                            ' exist?
                                           If MapNPC(MapNum).NPC(i).Num > 0 Then
                                                ' different faction?
                                                If Type.NPC(MapNPC(MapNum).NPC(i).Num).Faction > 0 And Type.NPC(MapNPC(MapNum).NPC(i).Num).Faction <> Type.NPC(NPCNum).Faction Then
                                                    n = Type.NPC(NPCNum).Range
                                                    distanceX = MapNPC(MapNum).NPC(x).X - CLng(MapNPC(MapNum).NPC(i).X)
                                                    distanceY = MapNPC(MapNum).NPC(x).Y - CLng(MapNPC(MapNum).NPC(i).Y)

                                                    ' Make sure we get a positive value
                                                    If distanceX < 0 Then distanceX *= -1
                                                    If distanceY < 0 Then distanceY *= -1

                                                    ' Are they in range?  if so GET'M!
                                                    If distanceX <= n And distanceY <= n And Type.NPC(NPCNum).Behaviour = NpcBehavior.AttackOnSight Then
                                                        MapNPC(MapNum).NPC(x).TargetType = 2 ' npc
                                                        MapNPC(MapNum).NPC(x).Target = i
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                        End If

                        targetVerify = 0

                        ' /////////////////////////////////////////////
                        ' // This is used for NPC walking/targetting //
                        ' /////////////////////////////////////////////
                        ' Make sure theres a npc with the map
                        If Type.Map(MapNum).NPC(x) > 0 And MapNPC(MapNum).NPC(x).Num > 0 Then
                           If MapNPC(MapNum).NPC(x).StunDuration > 0 Then
                                ' check if we can unstun them
                                If GetTimeMs() > MapNPC(MapNum).NPC(x).StunTimer + (MapNPC(MapNum).NPC(x).StunDuration * 1000) Then
                                    MapNPC(MapNum).NPC(x).StunDuration = 0
                                    MapNPC(MapNum).NPC(x).StunTimer = 0
                                End If
                            Else

                                target = MapNPC(MapNum).NPC(x).Target
                                targetType = MapNPC(MapNum).NPC(x).TargetType

                                ' Check to see if its time for the npc to walk
                                If Type.NPC(NPCNum).Behaviour <> NpcBehavior.ShopKeeper And Type.NPC(NPCNum).Behaviour <> NpcBehavior.Quest Then
                                    If targetType = Core.TargetType.Player Then ' player
                                        ' Check to see if we are following a player or not
                                        If target > 0 Then
                                            ' Check if the player is even playing, if so follow'm
                                            If IsPlaying(target) And GetPlayerMap(target) = mapNum Then
                                                targetVerify = 1
                                                targetY = GetPlayerY(target)
                                                targetX = GetPlayerX(target)
                                            Else
                                                MapNPC(MapNum).NPC(x).TargetType = 0 ' clear
                                                MapNPC(MapNum).NPC(x).Target = 0
                                            End If
                                        End If
                                    ElseIf targetType = Core.TargetType.NPC Then
                                        If target > 0 Then
                                           If MapNPC(MapNum).NPC(target).Num > 0 Then
                                                targetVerify = 1
                                                targetY = MapNPC(MapNum).NPC(target).Y
                                                targetX = MapNPC(MapNum).NPC(target).X
                                            Else
                                                MapNPC(MapNum).NPC(x).TargetType = 0 ' clear
                                                MapNPC(MapNum).NPC(x).Target = 0
                                            End If
                                        End If
                                    ElseIf targetType = Core.TargetType.Pet Then
                                        If target > 0 Then
                                            If IsPlaying(target) = 1 And GetPlayerMap(target) = mapNum And PetAlive(target) Then
                                                targetVerify = 1
                                                targetY = Type.Player(target).Pet.Y
                                                targetX = Type.Player(target).Pet.X
                                            Else
                                                MapNPC(MapNum).NPC(x).TargetType = 0 ' clear
                                                MapNPC(MapNum).NPC(x).Target = 0
                                            End If
                                        End If
                                    End If

                                    If targetVerify Then
                                        'Gonna make the npcs smarter.. Implementing a pathfinding algorithm.. we shall see what happens.
                                        If IsOneBlockAway(targetX, targetY, CLng(MapNPC(MapNum).NPC(x).X), CLng(MapNPC(MapNum).NPC(x).Y)) = 0 Then

                                            i = FindNpcPath(MapNum, x, targetX, targetY)
                                            If i < 4 Then 'Returned an answer. Move the NPC
                                                If CanNpcMove(MapNum, x, i) Then
                                                    NpcMove(MapNum, x, i, MovementType.Walking)
                                                End If
                                            Else 'No good path found. Move randomly
                                                i = Random.NextDouble(1,4)
                                                If i = 1 Then
                                                    i = Random.NextDouble(1,4)

                                                    If CanNpcMove(MapNum, x, i) Then
                                                        NpcMove(MapNum, x, i, MovementType.Walking)
                                                    End If
                                                End If
                                            End If
                                        Else
                                            NpcDir(MapNum, x, GetNpcDir(targetX, targetY, CLng(MapNPC(MapNum).NPC(x).X), CLng(MapNPC(MapNum).NPC(x).Y)))
                                        End If
                                    Else
                                        i = Int(Rnd() * 4)

                                        If i = 1 Then
                                            i = Int(Rnd() * 4)

                                            If CanNpcMove(MapNum, x, i) Then
                                                NpcMove(MapNum, x, i, MovementType.Walking)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If

                    End If

                    ' /////////////////////////////////////////////
                    ' // This is used for npcs to attack targets //
                    ' /////////////////////////////////////////////
                    ' Make sure theres a npc with the map
                    If Type.Map(MapNum).NPC(x) > 0 And MapNPC(MapNum).NPC(x).Num > 0 Then
                        target = MapNPC(MapNum).NPC(x).Target
                        targetType = MapNPC(MapNum).NPC(x).TargetType

                        ' Check if the npc can attack the targeted player player
                        If target > 0 Then

                            If targetType = Core.TargetType.Player Then ' player

                                ' Is the target playing and on the same map?
                                If IsPlaying(target) And GetPlayerMap(target) = mapNum Then
                                    If IsPlaying(target) And GetPlayerMap(target) = mapNum Then
                                        If Random.NextDouble(1, 3) = 1 Then
                                            Dim skillnum As Integer = RandomNpcAttack(MapNum, x)
                                            If skillnum > 0 Then
                                                BufferNpcSkill(MapNum, x, skillnum)
                                            Else
                                                TryNpcAttackPlayer(x, target) ', Damage)
                                            End If
                                        Else
                                            TryNpcAttackPlayer(x, target)
                                        End If
                                    Else
                                        ' Player left map or game, set target to 0
                                        MapNPC(MapNum).NPC(x).Target = 0
                                        MapNPC(MapNum).NPC(x).TargetType = 0 ' clear

                                    End If
                                Else
                                    ' Player left map or game, set target to 0
                                    MapNPC(MapNum).NPC(x).Target = 0
                                    MapNPC(MapNum).NPC(x).TargetType = 0 ' clear
                                End If
                            ElseIf targetType = Core.TargetType.NPC Then
                               If MapNPC(MapNum).NPC(target).Num > 0 Then ' npc exists
                                    'Can the npc attack the npc?
                                    If CanNpcAttackNpc(MapNum, x, target) Then
                                        damage = Type.NPC(NPCNum).Stat(StatType.Strength) - CLng(Type.NPC(target).Stat(StatType.Luck))
                                        NpcAttackNpc(MapNum, x, target, damage)
                                    End If
                                Else
                                    ' npc is dead or non-existant
                                    MapNPC(MapNum).NPC(x).Target = 0
                                    MapNPC(MapNum).NPC(x).TargetType = 0 ' clear
                                End If
                            ElseIf targetType = Core.TargetType.Pet Then
                                If IsPlaying(target) And GetPlayerMap(target) = mapNum And PetAlive(target) Then
                                    TryNpcAttackPet(x, target)
                                Else
                                    ' Player left map or game, set target to 0
                                    MapNPC(MapNum).NPC(x).Target = 0
                                    MapNPC(MapNum).NPC(x).TargetType = 0 ' clear
                                End If
                            End If
                        End If
                    End If

                    ' ////////////////////////////////////////////
                    ' // This is used for regenerating NPC's HP //
                    ' ////////////////////////////////////////////
                    ' Check to see if we want to regen some of the npc's hp
                   If MapNPC(MapNum).NPC(x).Num > 0 And tickCount > GiveNPCHPTimer + 10000 Then
                       If MapNPC(MapNum).NPC(x).Vital(VitalType.HP) > 0 Then
                            MapNPC(MapNum).NPC(x).Vital(VitalType.HP) = MapNPC(MapNum).NPC(x).Vital(VitalType.HP) + GetNpcVitalRegen(npcNum, VitalType.HP)

                            ' Check if they have more then they should and if so just set it to max
                           If MapNPC(MapNum).NPC(x).Vital(VitalType.HP) > GetNpcMaxVital(npcNum, VitalType.HP) Then
                                MapNPC(MapNum).NPC(x).Vital(VitalType.HP) = GetNpcMaxVital(npcNum, VitalType.HP)
                            End If
                        End If
                    End If

                   If MapNPC(MapNum).NPC(x).Num > 0 And tickCount > GiveNPCMPTimer + 10000 And MapNPC(MapNum).NPC(x).Vital(VitalType.SP) > 0 Then
                        MapNPC(MapNum).NPC(x).Vital(VitalType.SP) = MapNPC(MapNum).NPC(x).Vital(VitalType.SP) + GetNpcVitalRegen(npcNum, VitalType.SP)

                        ' Check if they have more then they should and if so just set it to max
                       If MapNPC(MapNum).NPC(x).Vital(VitalType.SP) > GetNpcMaxVital(npcNum, VitalType.SP) Then
                            MapNPC(MapNum).NPC(x).Vital(VitalType.SP) = GetNpcMaxVital(npcNum, VitalType.SP)
                        End If
                    End If

                    ' ////////////////////////////////////////////////////////
                    ' // This is used for checking if an NPC is dead or not //
                    ' ////////////////////////////////////////////////////////
                    ' Check if the npc is dead or not
                   If MapNPC(MapNum).NPC(x).Num > 0 And MapNPC(MapNum).NPC(x).Vital(VitalType.HP) < 0 And MapNPC(MapNum).NPC(x).SpawnWait > 0 Then
                        MapNPC(MapNum).NPC(x).Num = 0
                        MapNPC(MapNum).NPC(x).SpawnWait = GetTimeMs()
                        MapNPC(MapNum).NPC(x).Vital(VitalType.HP) = 0
                    End If

                    ' //////////////////////////////////////
                    ' // This is used for spawning an NPC //
                    ' //////////////////////////////////////
                    ' Check if we are supposed to spawn an npc or not
                   If MapNPC(MapNum).NPC(x).Num = 0 And Type.Map(MapNum).NPC(x) > 0 And Type.NPC(Type.Map(MapNum).NPC(x)).SpawnSecs > 0 Then
                        If tickCount > MapNPC(MapNum).NPC(x).SpawnWait + (Type.NPC(Type.Map(MapNum).NPC(x)).SpawnSecs * 1000) Then
                            SpawnNpc(x, mapNum)
                        End If
                    End If
                Next
            End If

        Next

        ' Make sure we reset the timer for npc hp regeneration
        If GetTimeMs() > GiveNPCHPTimer + 10000 Then GiveNPCHPTimer = GetTimeMs()

        If GetTimeMs() > GiveNPCMPTimer + 10000 Then GiveNPCMPTimer = GetTimeMs()

        ' Make sure we reset the timer for door closing
        If GetTimeMs() > KeyTimer + 15000 Then KeyTimer = GetTimeMs()
    End Sub

    Function GetNpcVitalRegen(npcNum As Integer, vital As VitalType) As Integer
        Dim i As Integer

        GetNpcVitalRegen = 0

        'Prevent subscript out of range
        If npcNum <= 0 Or npcNum > MAX_NPCS Then
            Exit Function
        End If

        Select Case vital
            Case VitalType.HP
                i = Type.NPC(NPCNum).Stat(StatType.Vitality) \ 3

                If i < 0 Then i = 1
                GetNpcVitalRegen = i
            Case VitalType.SP
                i = Type.NPC(NPCNum).Stat(StatType.Intelligence) \ 3

                If i < 0 Then i = 1
                GetNpcVitalRegen = i
        End Select

    End Function

    Friend Function HandlePetSkill(index As Integer) As Boolean
        PetCastSkill(index, TempPlayer(index).PetskillBuffer.Skill, TempPlayer(index).PetskillBuffer.Target, TempPlayer(index).PetskillBuffer.TargetType, True)
        TempPlayer(index).PetskillBuffer.Skill = 0
        TempPlayer(index).PetskillBuffer.Timer = 0
        TempPlayer(index).PetskillBuffer.Target = 0
        TempPlayer(index).PetskillBuffer.TargetType = 0
        HandlePetSkill = 1
    End Function

    Friend Function HandleClearStun(index As Integer) As Boolean
        TempPlayer(index).StunDuration = 0
        TempPlayer(index).StunTimer = 0
        SendStunned(index)
        HandleClearStun = 1
    End Function

    Friend Function HandleClearPetStun(index As Integer) As Boolean
        TempPlayer(index).PetStunDuration = 0
        TempPlayer(index).PetStunTimer = 0
        HandleClearPetStun = 1
    End Function

    Friend Function HandleStopPetRegen(index As Integer) As Boolean
        TempPlayer(index).PetstopRegen = 0
        TempPlayer(index).PetstopRegenTimer = 0
        HandleStopPetRegen = 1
    End Function

    Friend Function HandleCastSkill(index As Integer) As Boolean
        CastSkill(index, TempPlayer(index).SkillBuffer)
        TempPlayer(index).SkillBuffer = 0
        TempPlayer(index).SkillBufferTimer = 0
        HandleCastSkill = 1
    End Function

    Friend Sub CastSkill(index As Integer, skillSlot As Integer)
        ' Set up some basic variables we'll be using.
        Dim skillId = GetPlayerSkill(index, skillSlot)

        ' Preventative checks
        If Not IsPlaying(index) Or skillSlot < 0 Or skillSlot > MAX_PLAYER_SKILLS Or Not HasSkill(index, skillId) Then Exit Sub

        ' Check if the player is able to cast the Skill.
        If GetPlayerVital(index, VitalType.SP) < Type.Skill(skillId).MpCost Then
            PlayerMsg(index, "Not enough mana!", ColorType.BrightRed)
            Exit Sub
        ElseIf GetPlayerLevel(index) < Type.Skill(skillId).LevelReq Then
            PlayerMsg(index, String.Format("You must be level {0} to use this skill.", Type.Skill(skillId).LevelReq), ColorType.BrightRed)
            Exit Sub
        ElseIf GetPlayerAccess(index) < Type.Skill(skillId).AccessReq Then
            PlayerMsg(index, "You must be an administrator to use this skill.", ColorType.BrightRed)
            Exit Sub
        ElseIf Not Type.Skill(skillId).JobReq = 0 And GetPlayerJob(index) <> Type.Skill(skillId).JobReq Then
            PlayerMsg(index, String.Format("Only {0} can use this skill.", CheckGrammar(Type.Job(Type.Skill(skillId).JobReq).Name, 1)), ColorType.BrightRed)
            Exit Sub
        ElseIf Type.Skill(skillId).Range > 0 And Not IsTargetOnMap(index) Then
            Exit Sub
        ElseIf Type.Skill(skillId).Range > 0 And Not IsInSkillRange(index, skillId) And Type.Skill(skillId).IsProjectile = 0 Then
            PlayerMsg(index, "Target not in range.", ColorType.BrightRed)
            SendClearSkillBuffer(index)
            Exit Sub
        End If

        ' Determine what kind of Skill Type we're dealing with and move on to the appropriate methods.
        If Type.Skill(skillId).IsProjectile = 1 Then
            PlayerFireProjectile(index, skillId)
        Else
            If Type.Skill(skillId).Range = 0 And Not Type.Skill(skillId).IsAoE Then HandleSelfCastSkill(index, skillId)
            If Type.Skill(skillId).Range = 0 And Type.Skill(skillId).IsAoE Then HandleSelfCastAoESkill(index, skillId)
            If Type.Skill(skillId).Range > 0 And Type.Skill(skillId).IsAoE Then HandleTargetedAoESkill(index, skillId)
            If Type.Skill(skillId).Range > 0 And Not Type.Skill(skillId).IsAoE Then HandleTargetedSkill(index, skillId)
        End If

        ' Do everything we need to do at the end of the cast.
        FinalizeCast(index, GetPlayerSkill(index, skillId), Type.Skill(skillId).MpCost)
    End Sub

    Private Sub HandleSelfCastAoESkill(index As Integer, skillId As Integer)

        ' Set up some variables we'll definitely be using.
        Dim centerX = GetPlayerX(index)
        Dim centerY = GetPlayerY(index)

        ' Determine what kind of Skill we're dealing with and process it.
        Select Case Type.Skill(skillId).Type
            Case SkillType.DamageHp, SkillType.DamageMp, SkillType.HealHp, SkillType.HealMp
                HandleAoE(index, skillId, centerX, centerY)

            Case Else
                Throw New NotImplementedException()
        End Select

    End Sub

    Private Sub HandleTargetedAoESkill(index As Integer, skillId As Integer)

        ' Set up some variables we'll definitely be using.
        Dim centerX As Integer
        Dim centerY As Integer
        Select Case TempPlayer(index).TargetType
            Case TargetType.NPC
                centerX = MapNPC(GetPlayerMap(index)).NPC(TempPlayer(index).Target).X
                centerY = MapNPC(GetPlayerMap(index)).NPC(TempPlayer(index).Target).Y

            Case TargetType.Player
                centerX = GetPlayerX(TempPlayer(index).Target)
                centerY = GetPlayerY(TempPlayer(index).Target)

            Case Else
                Throw New NotImplementedException()

        End Select

        ' Determine what kind of Skill we're dealing with and process it.
        Select Case Type.Skill(skillId).Type
            Case SkillType.HealMp, SkillType.DamageHp, SkillType.DamageMp, SkillType.HealHp
                HandleAoE(index, skillId, centerX, centerY)

            Case Else
                Throw New NotImplementedException()
        End Select
    End Sub

    Private Sub HandleSelfCastSkill(index As Integer, skillId As Integer)
        ' Determine what kind of Skill we're dealing with and process it.
        Select Case Type.Skill(skillId).Type
            Case SkillType.HealHp
                SkillPlayer_Effect(VitalType.HP, True, index, Type.Skill(skillId).Vital, skillId)
            Case SkillType.HealMp
                SkillPlayer_Effect(VitalType.SP, True, index, Type.Skill(skillId).Vital, skillId)
            Case SkillType.Warp
                SendAnimation(GetPlayerMap(index), Type.Skill(skillId).SkillAnim, 0, 0, TargetType.Player, index)
                PlayerWarp(index, Type.Skill(skillId).Map, Type.Skill(skillId).X, Type.Skill(skillId).Y)
        End Select

        ' Play our animation.
        SendAnimation(GetPlayerMap(index), Type.Skill(skillId).SkillAnim, 0, 0, TargetType.Player, index)
    End Sub

    Private Sub HandleTargetedSkill(index As Integer, skillId As Integer)
        ' Set up some variables we'll definitely be using.
        Dim vital As VitalType
        Dim dealsDamage As Boolean
        Dim amount = Type.Skill(skillId).Vital
        Dim target = TempPlayer(index).Target

        ' Determine what vital we need to adjust and how.
        Select Case Type.Skill(skillId).Type
            Case SkillType.DamageHp
                vital = VitalType.HP
                dealsDamage = 1

            Case SkillType.DamageMp
                vital = VitalType.SP
                dealsDamage = 1

            Case SkillType.HealHp
                vital = VitalType.HP
                dealsDamage = 0

            Case SkillType.HealMp
                vital = VitalType.SP
                dealsDamage = 0

            Case Else
                Throw New NotImplementedException
        End Select

        Select Case TempPlayer(index).TargetType
            Case TargetType.NPC
                ' Deal with damaging abilities.
                If dealsDamage And CanPlayerAttackNpc(index, target, True) Then SkillNpc_Effect(vital, False, target, amount, skillId, GetPlayerMap(index))

                ' Deal with healing abilities
                If Not dealsDamage Then SkillNpc_Effect(vital, True, target, amount, skillId, GetPlayerMap(index))

                ' Handle our NPC death if it kills them
                If IsNpcDead(GetPlayerMap(index), TempPlayer(index).Target) Then
                    HandlePlayerKillNpc(GetPlayerMap(index), index, TempPlayer(index).Target)
                End If

            Case TargetType.Player

                ' Deal with damaging abilities.
                If dealsDamage And CanPlayerAttackPlayer(index, target, True) Then SkillPlayer_Effect(vital, False, target, amount, skillId)

                ' Deal with healing abilities
                If Not dealsDamage Then SkillPlayer_Effect(vital, True, target, amount, skillId)

                If IsPlayerDead(target) Then
                    ' Actually kill the player.
                    OnDeath(target)

                    ' Handle PK stuff.
                    HandlePlayerKilledPK(index, target)
                End If
            Case Else
                Throw New NotImplementedException()

        End Select

        ' Play our animation.
        SendAnimation(GetPlayerMap(index), Type.Skill(skillId).SkillAnim, 0, 0, TempPlayer(index).TargetType, target)
    End Sub

    Private Sub HandleAoE(index As Integer, skillId As Integer, x As Integer, y As Integer)
        ' Get some basic things set up.
        Dim map = GetPlayerMap(index)
        Dim range = Type.Skill(skillId).Range
        Dim amount = Type.Skill(skillId).Vital
        Dim vital As VitalType
        Dim dealsDamage As Boolean

        ' Determine what vital we need to adjust and how.
        Select Case Type.Skill(skillId).Type
            Case SkillType.DamageHp
                vital = VitalType.HP
                dealsDamage = 1

            Case SkillType.DamageMp
                vital = VitalType.SP
                dealsDamage = 1

            Case SkillType.HealHp
                vital = VitalType.HP
                dealsDamage = 0

            Case SkillType.HealMp
                vital = VitalType.SP
                dealsDamage = 0

            Case Else
                Throw New NotImplementedException
        End Select

        ' Loop through all online players on the current map.
        For Each id In TempPlayer.Where(Function(p) p.InGame).Select(Function(p, i) i + 1).Where(Function(i) GetPlayerMap(i) = map And i <> index).ToArray()
            If IsInRange(range, x, y, GetPlayerX(id), GetPlayerY(id)) Then

                ' Deal with damaging abilities.
                If dealsDamage And CanPlayerAttackPlayer(index, id, True) Then SkillPlayer_Effect(vital, False, id, amount, skillId)

                ' Deal with healing abilities
                If Not dealsDamage Then SkillPlayer_Effect(vital, True, id, amount, skillId)

                ' Send our animation to the map.
                SendAnimation(map, Type.Skill(skillId).SkillAnim, 0, 0, TargetType.Player, id)

                If IsPlayerDead(id) Then
                    ' Actually kill the player.
                    OnDeath(id)

                    ' Handle PK stuff.
                    HandlePlayerKilledPK(index, id)
                End If
            End If
        Next

        ' Loop through all the NPCs on this map
        For Each id In MapNPC(map).NPC.Where(Function(n) n.Num > 0 And n.Vital(VitalType.HP) > 0).Select(Function(n, i) i + 1).ToArray()
            If IsInRange(range, x, y, MapNPC(map).NPC(id).X, MapNPC(map).NPC(id).Y) Then

                ' Deal with damaging abilities.
                If dealsDamage And CanPlayerAttackNpc(index, id, True) Then SkillNpc_Effect(vital, False, id, amount, skillId, map)

                ' Deal with healing abilities
                If Not dealsDamage Then SkillNpc_Effect(vital, True, id, amount, skillId, map)

                ' Send our animation to the map.
                SendAnimation(map, Type.Skill(skillId).SkillAnim, 0, 0, TargetType.NPC, id)

                ' Handle our NPC death if it kills them
                If IsNpcDead(map, id) Then
                    HandlePlayerKillNpc(map, index, id)
                End If
            End If
        Next
    End Sub

    Private Sub FinalizeCast(index As Integer, skillSlot As Integer, skillCost As Integer)
        SetPlayerVital(index, VitalType.SP, GetPlayerVital(index, VitalType.SP) - skillCost)
        SendVital(index, VitalType.SP)
        TempPlayer(index).SkillCd(skillSlot) = GetTimeMs() + (Type.Skill(skillSlot).CdTime * 1000)
        SendCooldown(index, skillSlot)
    End Sub

    Private Function IsTargetOnMap(index As Integer) As Boolean
        If TempPlayer(index).TargetType = TargetType.Player Then
            If GetPlayerMap(TempPlayer(index).Target) = GetPlayerMap(index) Then IsTargetOnMap = 1
        ElseIf TempPlayer(index).TargetType = TargetType.NPC Then
            If TempPlayer(index).Target > 0 And TempPlayer(index).Target <= MAX_MAP_NPCS And MapNPC(GetPlayerMap(index)).NPC(TempPlayer(index).Target).Vital(VitalType.HP) > 0 Then IsTargetOnMap = 1
        End If
    End Function

    Private Function IsInSkillRange(index As Integer, SkillId As Integer) As Boolean
        Dim targetX As Integer, targetY As Integer

        If TempPlayer(index).TargetType = TargetType.Player Then
            targetX = GetPlayerX(TempPlayer(index).Target)
            targetY = GetPlayerY(TempPlayer(index).Target)
        ElseIf TempPlayer(index).TargetType = TargetType.NPC Then
            targetX = MapNPC(GetPlayerMap(index)).NPC(TempPlayer(index).Target).X
            targetY = MapNPC(GetPlayerMap(index)).NPC(TempPlayer(index).Target).Y
        End If

        IsInSkillRange = IsInRange(Type.Skill(skillId).Range, GetPlayerX(index), GetPlayerY(index), targetX, targetY)
    End Function

    Friend Sub CastNpcSkill(npcNum As Integer, mapNum As Integer, skillslot As Integer)
        Dim skillnum As Integer, mpCost As Integer
        Dim vital As Integer, didCast As Boolean
        Dim i As Integer
        Dim aoe As Integer, range As Integer, vitalType As Byte
        Dim increment As Boolean, x As Integer, y As Integer

        Dim targetType As Byte
        Dim target As Integer
        Dim skillCastType As Integer

        didCast = 0

        ' Prevent subscript out of range
        If skillslot < 0 Or skillslot > MAX_NPC_SKILLS Then Exit Sub

        skillnum = GetNpcSkill(MapNPC(MapNum).NPC(NPCNum).Num, skillslot)

        mpCost = Type.Skill(skillNum).MpCost

        ' Check if they have enough MP
       If MapNPC(MapNum).NPC(NPCNum).Vital(Core.VitalType.SP) < mpCost Then Exit Sub

        ' find out what kind of skill it is! self cast, target or AOE
        If Type.Skill(skillNum).IsProjectile = 1 Then
            skillCastType = 4 ' Projectile
        ElseIf Type.Skill(skillNum).Range > 0 Then
            ' ranged attack, single target or aoe?
            If Not Type.Skill(skillNum).IsAoE Then
                skillCastType = 2 ' targetted
            Else
                skillCastType = 3 ' targetted aoe
            End If
        Else
            If Not Type.Skill(skillNum).IsAoE Then
                skillCastType = 0 ' self-cast
            Else
                skillCastType = 1 ' self-cast AoE
            End If
        End If

        ' set the Core.VitalType
        vital = Type.Skill(skillNum).Vital
        aoe = Type.Skill(skillNum).AoE
        range = Type.Skill(skillNum).Range

        Select Case skillCastType
            Case 0 ' self-cast target
                'Select Case Type.Skill(skillNum).Type
                '    Case SkillType.HEALHP
                '        SkillPlayer_Effect(VitalType.HP, True, NpcNum, Vital, skillnum)
                '        DidCast = 1
                '    Case SkillType.HEALMP
                '        SkillPlayer_Effect(VitalType.SP, True, NpcNum, Vital, skillnum)
                '        DidCast = 1
                '    Case SkillType.WARP
                '        SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, TargetType.PLAYER, NpcNum)
                '        PlayerWarp(NpcNum, Type.Skill(skillNum).Map, Type.Skill(skillNum).x, Type.Skill(skillNum).y)
                '        SendAnimation(GetPlayerMap(NpcNum), Type.Skill(skillNum).SkillAnim, 0, 0, TargetType.PLAYER, NpcNum)
                '        DidCast = 1
                'End Select

            Case 1, 3 ' self-cast AOE & targetted AOE
                If skillCastType = 1 Then
                    x = MapNPC(MapNum).NPC(NPCNum).X
                    y = MapNPC(MapNum).NPC(NPCNum).Y
                ElseIf skillCastType = 3 Then
                    targetType = MapNPC(MapNum).NPC(NPCNum).TargetType
                    target = MapNPC(MapNum).NPC(NPCNum).Target

                    If targetType = 0 Then Exit Sub
                    If target = 0 Then Exit Sub

                    If targetType = Core.TargetType.Player Then
                        x = GetPlayerX(target)
                        y = GetPlayerY(target)
                    Else
                        x = MapNPC(MapNum).NPC(target).X
                        y = MapNPC(MapNum).NPC(target).Y
                    End If

                    If Not IsInRange(range, x, y, GetPlayerX(npcNum), GetPlayerY(npcNum)) Then
                        Exit Sub
                    End If
                End If
                Select Case Type.Skill(skillNum).Type
                    Case SkillType.DamageHp
                        didCast = 1
                        For i = 1 To Socket.HighIndex()
                            If IsPlaying(i) Then
                                If GetPlayerMap(i) = mapNum Then
                                    If IsInRange(aoe, x, y, GetPlayerX(i), GetPlayerY(i)) Then
                                        If CanNpcAttackPlayer(npcNum, i) Then
                                            SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.Player, i)
                                            PlayerMsg(i, Type.NPC(MapNPC(MapNum).NPC(NPCNum).Num).Name & " uses " & Type.Skill(skillNum).Name & "!", ColorType.Yellow)
                                            SkillPlayer_Effect(Core.VitalType.HP, False, i, vital, skillnum)
                                        End If
                                    End If
                                End If
                            End If
                        Next

                        For i = 1 To MAX_MAP_NPCS
                           If MapNPC(MapNum).NPC(i).Num > 0 Then
                               If MapNPC(MapNum).NPC(i).Vital(Core.VitalType.HP) > 0 Then
                                    If IsInRange(aoe, x, y, MapNPC(MapNum).NPC(i).X, MapNPC(MapNum).NPC(i).Y) Then
                                        If CanPlayerAttackNpc(npcNum, i, True) Then
                                            SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.NPC, i)
                                            SkillNpc_Effect(Core.VitalType.HP, False, i, vital, skillnum, mapNum)
                                            If Type.Skill(skillNum).KnockBack = 1 Then
                                                KnockBackNpc(npcNum, target, skillnum)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    Case SkillType.HealHp, SkillType.HealMp, SkillType.DamageMp
                        If Type.Skill(skillNum).Type = SkillType.HealHp Then
                            vital = Core.VitalType.HP
                            increment = 1
                        ElseIf Type.Skill(skillNum).Type = SkillType.HealMp Then
                            vital = Core.VitalType.SP
                            increment = 1
                        ElseIf Type.Skill(skillNum).Type = SkillType.DamageMp Then
                            vital = Core.VitalType.SP
                            increment = 0
                        End If

                        didCast = 1
                        For i = 1 To Socket.HighIndex()
                            If IsPlaying(i) And GetPlayerMap(i) = GetPlayerMap(npcNum) Then
                                If IsInRange(aoe, x, y, GetPlayerX(i), GetPlayerY(i)) Then
                                    SkillPlayer_Effect(vital, increment, i, vital, skillnum)
                                End If
                            End If
                        Next

                        For i = 1 To MAX_MAP_NPCS
                           If MapNPC(MapNum).NPC(i).Num > 0 And MapNPC(MapNum).NPC(i).Vital(Core.VitalType.HP) > 0 Then
                                If IsInRange(aoe, x, y, MapNPC(MapNum).NPC(i).X, MapNPC(MapNum).NPC(i).Y) Then
                                    SkillNpc_Effect(vital, increment, i, vital, skillnum, mapNum)
                                End If
                            End If
                        Next
                End Select

            Case 2 ' targetted

                targetType = MapNPC(MapNum).NPC(NPCNum).TargetType
                target = MapNPC(MapNum).NPC(NPCNum).Target

                If targetType = 0 Or target = 0 Then Exit Sub

               If MapNPC(MapNum).NPC(NPCNum).TargetType = Core.TargetType.Player Then
                    x = GetPlayerX(target)
                    y = GetPlayerY(target)
                Else
                    x = MapNPC(MapNum).NPC(target).X
                    y = MapNPC(MapNum).NPC(target).Y
                End If

                If Not IsInRange(range, MapNPC(MapNum).NPC(NPCNum).X, MapNPC(MapNum).NPC(NPCNum).Y, x, y) Then Exit Sub

                Select Case Type.Skill(skillNum).Type
                    Case SkillType.DamageHp
                       If MapNPC(MapNum).NPC(NPCNum).TargetType = Core.TargetType.Player Then
                            If CanNpcAttackPlayer(npcNum, target) And vital > 0 Then
                                SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.Player, target)
                                PlayerMsg(target, Type.NPC(MapNPC(MapNum).NPC(NPCNum).Num).Name & " uses " & Type.Skill(skillNum).Name & "!", ColorType.Yellow)
                                SkillPlayer_Effect(Core.VitalType.HP, False, target, vital, skillnum)
                                didCast = 1
                            End If
                        Else
                            If CanPlayerAttackNpc(npcNum, target, True) And vital > 0 Then
                                SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.NPC, target)
                                SkillNpc_Effect(Core.VitalType.HP, False, i, vital, skillnum, mapNum)

                                If Type.Skill(skillNum).KnockBack = 1 Then
                                    KnockBackNpc(npcNum, target, skillnum)
                                End If
                                didCast = 1
                            End If
                        End If

                    Case SkillType.DamageMp, SkillType.HealMp, SkillType.HealHp
                        If Type.Skill(skillNum).Type = SkillType.DamageMp Then
                            vital = Core.VitalType.SP
                            increment = 0
                        ElseIf Type.Skill(skillNum).Type = SkillType.HealMp Then
                            vital = Core.VitalType.SP
                            increment = 1
                        ElseIf Type.Skill(skillNum).Type = SkillType.HealHp Then
                            vital = Core.VitalType.HP
                            increment = 1
                        End If

                        If TempPlayer(npcNum).TargetType = Core.TargetType.Player Then
                            If Type.Skill(skillNum).Type = SkillType.DamageMp Then
                                If CanPlayerAttackPlayer(npcNum, target, True) Then
                                    SkillPlayer_Effect(vital, increment, target, vital, skillnum)
                                End If
                            Else
                                SkillPlayer_Effect(vital, increment, target, vital, skillnum)
                            End If
                        Else
                            If Type.Skill(skillNum).Type = SkillType.DamageMp Then
                                If CanPlayerAttackNpc(npcNum, target, True) Then
                                    SkillNpc_Effect(vital, increment, target, vital, skillnum, mapNum)
                                End If
                            Else
                                SkillNpc_Effect(vital, increment, target, vital, skillnum, mapNum)
                            End If
                        End If
                End Select
            Case 4 ' Projectile
                PlayerFireProjectile(npcNum, skillnum)

                didCast = 1
        End Select

        If didCast Then
            MapNPC(MapNum).NPC(NPCNum).Vital(Core.VitalType.SP) = MapNPC(MapNum).NPC(NPCNum).Vital(Core.VitalType.SP) - mpCost
            SendMapNPCVitals(MapNum, npcNum)
            MapNPC(MapNum).NPC(NPCNum).SkillCd(skillslot) = GetTimeMs() + (Type.Skill(skillNum).CdTime * 1000)
        End If
    End Sub

    Friend Sub SkillPlayer_Effect(vital As Byte, increment As Boolean, index As Integer, damage As Integer, skillnum As Integer)
        Dim sSymbol As String
        Dim Color As Integer

        If damage > 0 Then
            ' Calculate for Magic Resistance.
            damage -= ((GetPlayerStat(index, StatType.Spirit) * 2) + (GetPlayerLevel(index) * 3))

            If increment Then
                sSymbol = "+"
                If vital = VitalType.HP Then Color = ColorType.BrightGreen
                If vital = VitalType.SP Then Color = ColorType.BrightBlue
            Else
                sSymbol = "-"
                Color = ColorType.BrightRed
            End If

            ' Deal with stun effects.
            If Type.Skill(skillNum).StunDuration > 0 Then StunPlayer(index, skillnum)

            SendActionMsg(GetPlayerMap(index), sSymbol & damage, Color, ActionMsgType.Scroll, GetPlayerX(index) * 32, GetPlayerY(index) * 32)
            If increment Then SetPlayerVital(index, vital, GetPlayerVital(index, vital) + damage)
            If Not increment Then SetPlayerVital(index, vital, GetPlayerVital(index, vital) - damage)
            SendVital(index, vital)
        End If
    End Sub

    Friend Sub SkillNpc_Effect(vital As Byte, increment As Boolean, index As Integer, damage As Integer, skillnum As Integer, mapNum As Integer)
        Dim sSymbol As String
        Dim color As Integer

        If index <= 0 Or index > MAX_MAP_NPCS Or damage <= 0 Or MapNPC(MapNum).NPC(index).Vital(vital) <= 0 Then Exit Sub

        If damage > 0 Then
            If increment Then
                sSymbol = "+"
                If vital = VitalType.HP Then color = ColorType.BrightGreen
                If vital = VitalType.SP Then color = ColorType.BrightBlue
            Else
                sSymbol = "-"
                color = ColorType.BrightRed
            End If

            ' Deal with Stun and Knockback effects.
            If Type.Skill(skillNum).KnockBack = 1 Then KnockBackNpc(index, index, skillnum)
            If Type.Skill(skillNum).StunDuration > 0 Then StunNPC(index, mapNum, skillnum)

            SendActionMsg(MapNum, sSymbol & damage, color, ActionMsgType.Scroll, MapNPC(MapNum).NPC(index).X * 32, MapNPC(MapNum).NPC(index).Y * 32)
            If increment Then MapNPC(MapNum).NPC(index).Vital(vital) = MapNPC(MapNum).NPC(index).Vital(vital) + damage
            If Not increment Then MapNPC(MapNum).NPC(index).Vital(vital) = MapNPC(MapNum).NPC(index).Vital(vital) - damage
            SendMapNPCVitals(MapNum, index)
        End If
    End Sub

End Module