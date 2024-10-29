Imports System.Drawing
Imports Core
Imports Mirage.Sharp.Asfw
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq


Module S_Pet

#Region "Declarations"

    Friend Pet() As Type.PetStruct

    ' PET constants
    Friend Const PetBehaviourFollow As Byte = 0 'The pet will attack all npcs around
    Friend Const PetBehaviourGoto As Byte = 1 'If attacked, the pet will fight back
    Friend Const PetAttackBehaviourAttackonsight As Byte = 2 'The pet will attack all npcs around
    Friend Const PetAttackBehaviourGuard As Byte = 3 'If attacked, the pet will fight back
    Friend Const PetAttackBehaviourDonothing As Byte = 4 'The pet will not attack even if attacked

    Friend givePetHpTimer As Integer

#End Region

#Region "Database"

    Sub SavePets()
        Dim i As Integer

        For i = 1 To MAX_PETS
            SavePet(i)
        Next

    End Sub

    Sub SavePet(petNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Pet(petNum)).ToString()

        If RowExists(petNum, "pet")
            UpdateRow(petNum, json, "pet", "data")
        Else
            InsertRow(petNum, json, "pet")
        End If
    End Sub

    Sub LoadPets()
        Dim i As Integer

        For i = 1 To MAX_PETS
            LoadPet(i)
        Next
    End Sub

    Sub LoadPet(petNum As Integer)
        Dim data As JObject

        data = SelectRow(petNum, "pet", "data")

        If data Is Nothing Then
            ClearPet(petNum)
            Exit Sub
        End If

        Dim petData = JObject.FromObject(data).toObject(Of PetStruct)()
        Pet(petNum) = petData
    End Sub

    Sub ClearPet(petNum As Integer)
        Pet(petNum).Name = ""

        ReDim Pet(petNum).Stat(StatType.Count - 1)
        ReDim Pet(petNum).Skill(4)
    End Sub

    Sub ClearPets()
        Dim i As Integer

        ReDim Pet(MAX_PETS)

        For i = 1 To MAX_PETS
            ClearPet(i)
        Next

    End Sub

#End Region

#Region "Outgoing Packets"

    Sub SendPets(index As Integer)
        Dim i As Integer

        For i = 1 To MAX_PETS
            If Pet(i).Name.Length > 0 Then
                SendUpdatePetTo(index, i)
            End If
        Next

    End Sub

    Sub SendPet(index As Integer, petnum As Integer)
        SendUpdatePetTo(index, petnum)
    End Sub

    Sub SendUpdatePetToAll(petNum As Integer)
        Dim buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdatePet)

        buffer.WriteInt32(petNum)

        With Pet(petNum)
            buffer.WriteInt32(.Num)
            buffer.WriteString(.Name)
            buffer.WriteInt32(.Sprite)
            buffer.WriteInt32(.Range)
            buffer.WriteInt32(.Level)
            buffer.WriteInt32(.MaxLevel)
            buffer.WriteInt32(.ExpGain)
            buffer.WriteInt32(.LevelPnts)
            buffer.WriteInt32(.StatType)
            buffer.WriteInt32(.LevelingType)

            For i = 1 To StatType.Count - 1
                buffer.WriteInt32(.Stat(i))
            Next

            For i = 1 To 4
                buffer.WriteInt32(.Skill(i))
            Next

            buffer.WriteInt32(.Evolvable)
            buffer.WriteInt32(.EvolveLevel)
            buffer.WriteInt32(.EvolveNum)
        End With

        SendDataToAll(buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub SendUpdatePetTo(index As Integer, petNum As Integer)
        Dim buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdatePet)

        buffer.WriteInt32(petNum)

        With Pet(petNum)
            buffer.WriteInt32(.Num)
            buffer.WriteString(.Name)
            buffer.WriteInt32(.Sprite)
            buffer.WriteInt32(.Range)
            buffer.WriteInt32(.Level)
            buffer.WriteInt32(.MaxLevel)
            buffer.WriteInt32(.ExpGain)
            buffer.WriteInt32(.LevelPnts)
            buffer.WriteInt32(.StatType)
            buffer.WriteInt32(.LevelingType)

            For i = 1 To StatType.Count - 1
                buffer.WriteInt32(.Stat(i))
            Next

            For i = 1 To 4
                buffer.WriteInt32(.Skill(i))
            Next

            buffer.WriteInt32(.Evolvable)
            buffer.WriteInt32(.EvolveLevel)
            buffer.WriteInt32(.EvolveNum)
        End With

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Friend Sub SendUpdatePlayerPet(index As Integer, ownerOnly As Boolean)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdatePlayerPet)

        buffer.WriteInt32(index)

        buffer.WriteInt32(GetPetNum(index))
        buffer.WriteInt32(GetPetVital(index, VitalType.HP))
        buffer.WriteInt32(GetPetVital(index, VitalType.SP))
        buffer.WriteInt32(GetPetLevel(index))

        For i = 1 To StatType.Count - 1
            buffer.WriteInt32(GetPetStat(index, i))
        Next

        For i = 1 To 4
            buffer.WriteInt32(Type.Player(index).Pet.Skill(i))
        Next

        buffer.WriteInt32(GetPetX(index))
        buffer.WriteInt32(GetPetY(index))
        buffer.WriteInt32(GetPetDir(index))

        buffer.WriteInt32(GetPetMaxVital(index, VitalType.HP))
        buffer.WriteInt32(GetPetMaxVital(index, VitalType.SP))

        buffer.WriteInt32(Type.Player(index).Pet.Alive)

        buffer.WriteInt32(GetPetBehaviour(index))
        buffer.WriteInt32(GetPetPoints(index))
        buffer.WriteInt32(GetPetExp(index))
        buffer.WriteInt32(GetPetNextLevel(index))

        If ownerOnly Then
            Socket.SendDataTo(index, buffer.Data, buffer.Head)
        Else
            SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)
        End If

        buffer.Dispose()
    End Sub

    Sub SendPetAttack(index As Integer, mapNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPetAttack)
        buffer.WriteInt32(index)
        SendDataToMap(MapNum, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendPetXy(index As Integer, x As Integer, y As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPetXY)
        buffer.WriteInt32(index)
        buffer.WriteInt32(x)
        buffer.WriteInt32(y)
        SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendPetExp(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPetExp)
        buffer.WriteInt32(GetPetExp(index))
        buffer.WriteInt32(GetPetNextLevel(index))
        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendPetVital(index As Integer, vital As VitalType)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPetVital)

        buffer.WriteInt32(index)

        If vital = VitalType.HP Then
            buffer.WriteInt32(1)
        ElseIf vital = VitalType.SP Then
            buffer.WriteInt32(2)
        End If

        Select Case vital
            Case VitalType.HP
                buffer.WriteInt32(GetPetMaxVital(index, VitalType.HP))
                buffer.WriteInt32(GetPetVital(index, VitalType.HP))

            Case VitalType.SP
                buffer.WriteInt32(GetPetMaxVital(index, VitalType.SP))
                buffer.WriteInt32(GetPetVital(index, VitalType.SP))
        End Select

        SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub SendClearPetSkillBuffer(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SClearPetSkillBuffer)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

#End Region

#Region "Incoming Packets"

    Sub Packet_RequestEditPet(index As Integer, ByRef data() As Byte)
        Dim buffer = New ByteStream(4)

        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Pet)

        If user <> "" Then
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        TempPlayer(index).Editor = EditorType.Pet

        buffer.WriteInt32(ServerPackets.SPetEditor)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub Packet_SavePet(index As Integer, ByRef data() As Byte)
        Dim petNum As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        petNum = buffer.ReadInt32

        ' Prevent hacking
        If petNum <= 0 Or petNum > MAX_PETS Then Exit Sub

        With Pet(petNum)
            .Num = buffer.ReadInt32
            .Name = buffer.ReadString
            .Sprite = buffer.ReadInt32
            .Range = buffer.ReadInt32
            .Level = buffer.ReadInt32
            .MaxLevel = buffer.ReadInt32
            .ExpGain = buffer.ReadInt32
            .LevelPnts = buffer.ReadInt32
            .StatType = buffer.ReadInt32
            .LevelingType = buffer.ReadInt32

            For i = 1 To StatType.Count - 1
                .Stat(i) = buffer.ReadInt32
            Next

            For i = 1 To 4
                .Skill(i) = buffer.ReadInt32
            Next

            .Evolvable = buffer.ReadInt32
            .EvolveLevel = buffer.ReadInt32
            .EvolveNum = buffer.ReadInt32
        End With

        ' Save it
        SendUpdatePetToAll(petNum)
        SavePet(petNum)
        Addlog(GetPlayerLogin(index) & " saved Pet #" & petNum & ".", ADMIN_LOG)
        SendPets(index)
    End Sub

    Sub Packet_RequestPets(index As Integer, ByRef data() As Byte)
        SendPets(index)
    End Sub

    Sub Packet_SummonPet(index As Integer, ByRef data() As Byte)
        If PetAlive(index) Then
            RecallPet(index)
        Else
            SummonPet(index)
        End If
    End Sub

    Sub Packet_PetMove(index As Integer, ByRef data() As Byte)
        Dim x As Integer, y As Integer, i As Integer
        Dim buffer As New ByteStream(data)
        x = buffer.ReadInt32
        y = buffer.ReadInt32

        ' Prevent subscript out of range
        If x < 0 Or x > Type.Map(GetPlayerMap(index)).MaxX Or y < 0 Or y > Type.Map(GetPlayerMap(index)).MaxY Then Exit Sub

        ' Check for a player
        For i = 1 To Socket.HighIndex()

            If IsPlaying(i) Then
                If GetPlayerMap(index) = GetPlayerMap(i) And GetPlayerX(i) = x And GetPlayerY(i) = y Then
                    If i = index Then
                        ' Change target
                        If TempPlayer(index).PetTargetType = TargetType.Player And TempPlayer(index).PetTarget = i Then
                            TempPlayer(index).PetTarget = 0
                            TempPlayer(index).PetTargetType = 0
                            TempPlayer(index).PetBehavior = PetBehaviourGoto
                            TempPlayer(index).GoToX = x
                            TempPlayer(index).GoToY = y
                            ' send target to player
                            PlayerMsg(index, "Your pet is no longer following you.", ColorType.BrightGreen)
                        Else
                            TempPlayer(index).PetTarget = i
                            TempPlayer(index).PetTargetType = TargetType.Player
                            ' send target to player
                            TempPlayer(index).PetBehavior = PetBehaviourFollow
                            PlayerMsg(index, "Your " & GetPetName(index) & " is now following you.", ColorType.BrightGreen)
                        End If
                    Else
                        ' Change target
                        If TempPlayer(index).PetTargetType = TargetType.Player And TempPlayer(index).PetTarget = i Then
                            TempPlayer(index).PetTarget = 0
                            TempPlayer(index).PetTargetType = 0
                            ' send target to player
                            PlayerMsg(index, "Your pet is no longer targetting " & GetPlayerName(i) & ".", ColorType.BrightGreen)
                        Else
                            TempPlayer(index).PetTarget = i
                            TempPlayer(index).PetTargetType = TargetType.Player
                            ' send target to player
                            PlayerMsg(index, "Your pet is now targetting " & GetPlayerName(i) & ".", ColorType.BrightGreen)
                        End If
                    End If
                    Exit Sub
                End If
            End If

            If PetAlive(i) And i <> index Then
                If GetPetX(i) = x And GetPetY(i) = y Then
                    ' Change target
                    If TempPlayer(index).PetTargetType = TargetType.Pet And TempPlayer(index).PetTarget = i Then
                        TempPlayer(index).PetTarget = 0
                        TempPlayer(index).PetTargetType = 0
                        ' send target to player
                        PlayerMsg(index, "Your pet is no longer targetting " & GetPlayerName(i) & "'s " & GetPetName(i) & ".", ColorType.BrightGreen)
                    Else
                        TempPlayer(index).PetTarget = i
                        TempPlayer(index).PetTargetType = TargetType.Pet
                        ' send target to player
                        PlayerMsg(index, "Your pet is now targetting " & GetPlayerName(i) & "'s " & GetPetName(i) & ".", ColorType.BrightGreen)
                    End If
                    Exit Sub
                End If
            End If
        Next

        'Search For Target First
        ' Check for an npc
        For i = 1 To MAX_MAP_NPCS
           If MapNPC(GetPlayerMap(index)).NPC(i).Num > 0 And MapNPC(GetPlayerMap(index)).NPC(i).X = x And MapNPC(GetPlayerMap(index)).NPC(i).Y = y Then
                If TempPlayer(index).PetTarget = i And TempPlayer(index).PetTargetType = TargetType.NPC Then
                    ' Change target
                    TempPlayer(index).PetTarget = 0
                    TempPlayer(index).PetTargetType = 0
                    ' send target to player
                    PlayerMsg(index, "Your " & GetPetName(index) & "'s target is no longer a " & Type.NPC(MapNPC(GetPlayerMap(index)).NPC(i).Num).Name & "!", ColorType.BrightGreen)
                    Exit Sub
                Else
                    ' Change target
                    TempPlayer(index).PetTarget = i
                    TempPlayer(index).PetTargetType = TargetType.NPC
                    ' send target to player
                    PlayerMsg(index, "Your " & GetPetName(index) & "'s target is now a " & Type.NPC(MapNPC(GetPlayerMap(index)).NPC(i).Num).Name & "!", ColorType.BrightGreen)
                    Exit Sub
                End If
            End If
        Next

        TempPlayer(index).PetBehavior = PetBehaviourGoto
        TempPlayer(index).PetTargetType = 0
        TempPlayer(index).PetTarget = 0
        TempPlayer(index).GoToX = x
        TempPlayer(index).GoToY = y

        buffer.Dispose()

    End Sub

    Sub Packet_SetPetBehaviour(index As Integer, ByRef data() As Byte)
        Dim behaviour As Integer
        Dim buffer As New ByteStream(data)
        behaviour = buffer.ReadInt32

        If PetAlive(index) Then
            Select Case behaviour
                Case PetAttackBehaviourAttackonsight
                    SetPetBehaviour(index, PetAttackBehaviourAttackonsight)
                    SendActionMsg(GetPlayerMap(index), "Agressive Mode!", ColorType.White, 0, GetPetX(index) * 32, GetPetY(index) * 32, index)
                Case PetAttackBehaviourGuard
                    SetPetBehaviour(index, PetAttackBehaviourGuard)
                    SendActionMsg(GetPlayerMap(index), "Defensive Mode!", ColorType.White, 0, GetPetX(index) * 32, GetPetY(index) * 32, index)
            End Select
        End If

        buffer.Dispose()

    End Sub

    Sub Packet_ReleasePet(index As Integer, ByRef data() As Byte)
        If GetPetNum(index) > 0 Then ReleasePet(index)
    End Sub

    Sub Packet_PetSkill(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)
        ' Skill slot
        n = buffer.ReadInt32

        buffer.Dispose()

        ' set the skill buffer before castin
        BufferPetSkill(index, n)

    End Sub

    Sub Packet_UsePetStatPoint(index As Integer, ByRef data() As Byte)
        Dim pointType As Byte
        Dim sMes As String = ""
        Dim buffer As New ByteStream(data)
        pointType = buffer.ReadInt32
        buffer.Dispose()

        ' Prevent hacking
        If (pointType < 0) Or (pointType > StatType.Count) Then Exit Sub

        If Not PetAlive(index) Then Exit Sub

        ' Make sure they have points
        If GetPetPoints(index) > 0 Then

            ' make sure they're not maxed#
            If GetPetStat(index, pointType) >= 255 Then
                PlayerMsg(index, "You cannot spend any more points on that stat for your pet.", ColorType.BrightRed)
                Exit Sub
            End If

            SetPetPoints(index, GetPetPoints(index) - 1)

            ' Everything is ok
            Select Case pointType
                Case StatType.Strength
                    SetPetStat(index, pointType, GetPetStat(index, pointType) + 1)
                    sMes = "Strength"
                Case StatType.Intelligence
                    SetPetStat(index, pointType, GetPetStat(index, pointType) + 1)
                    sMes = "Intelligence"
                Case StatType.Luck
                    SetPetStat(index, pointType, GetPetStat(index, pointType) + 1)
                    sMes = "Luck"
                Case StatType.Spirit
                    SetPetStat(index, pointType, GetPetStat(index, pointType) + 1)
                    sMes = "Spirit"
                Case StatType.Vitality
                    SetPetStat(index, pointType, GetPetStat(index, pointType) + 1)
                    sMes = "Vitality"
            End Select

            SendActionMsg(GetPlayerMap(index), "+1 " & sMes, ColorType.White, 1, (GetPetX(index) * 32), (GetPetY(index) * 32))
        Else
            Exit Sub
        End If

        ' Send the update
        SendUpdatePlayerPet(index, True)

    End Sub

    Sub Packet_RequestPet(index As Integer, ByRef data() As Byte)
        Dim Buffer = New ByteStream(data), n As Integer

        n = Buffer.ReadInt32

        If n <= 0 Or n > MAX_RESOURCES Then Exit Sub

        SendUpdatePetTo(index, n)
    End Sub

#End Region

#Region "Pet Functions"

    Friend Sub UpdatePetAi()
        Dim didWalk As Boolean, playerindex As Integer
        Dim mapNum As Integer, tickCount As Integer, i As Integer, n As Integer
        Dim distanceX As Integer, distanceY As Integer, tmpdir As Integer
        Dim target As Integer, targetType As Byte, targetX As Integer, targetY As Integer, targetVerify As Boolean

        For mapNum = 1 To MAX_MAPS
            For playerindex = 1 To Socket.HighIndex()
                tickCount = GetTimeMs()

                If GetPlayerMap(playerindex) = mapNum And PetAlive(playerindex) Then
                    ' // This is used for ATTACKING ON SIGHT //

                    ' If the npc is a attack on sight, search for a player on the map
                    If GetPetBehaviour(playerindex) <> PetAttackBehaviourDonothing Then

                        ' make sure it's not stunned
                        If Not TempPlayer(playerindex).PetStunDuration > 0 Then

                            For i = 1 To Socket.HighIndex
                                If TempPlayer(playerindex).PetTargetType > 0 Then
                                    If TempPlayer(playerindex).PetTargetType = 1 And TempPlayer(playerindex).PetTarget = playerindex Then
                                    Else
                                        Exit For
                                    End If
                                End If

                                If IsPlaying(i) And i <> playerindex Then
                                    If GetPlayerMap(i) = mapNum And GetPlayerAccess(i) <= AccessType.Moderator Then
                                        If PetAlive(i) Then
                                            n = GetPetRange(playerindex)
                                            distanceX = GetPetX(playerindex) - GetPetX(i)
                                            distanceY = GetPetY(playerindex) - GetPetY(i)

                                            ' Make sure we get a positive value
                                            If distanceX < 0 Then distanceX *= -1
                                            If distanceY < 0 Then distanceY *= -1

                                            ' Are they in range?  if so GET'M!
                                            If distanceX <= n And distanceY <= n Then
                                                If GetPetBehaviour(playerindex) = PetAttackBehaviourAttackonsight Then
                                                    TempPlayer(playerindex).PetTargetType = Core.TargetType.Pet ' pet
                                                    TempPlayer(playerindex).PetTarget = i
                                                End If
                                            End If
                                        Else
                                            n = GetPetRange(playerindex)
                                            distanceX = GetPetX(playerindex) - GetPlayerX(i)
                                            distanceY = GetPetY(playerindex) - GetPlayerY(i)

                                            ' Make sure we get a positive value
                                            If distanceX < 0 Then distanceX *= -1
                                            If distanceY < 0 Then distanceY *= -1

                                            ' Are they in range?  if so GET'M!
                                            If distanceX <= n And distanceY <= n Then
                                                If GetPetBehaviour(playerindex) = PetAttackBehaviourAttackonsight Then
                                                    TempPlayer(playerindex).PetTargetType = Core.TargetType.Player ' player
                                                    TempPlayer(playerindex).PetTarget = i
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            Next

                            If TempPlayer(playerindex).PetTargetType = 0 Then
                                For i = 1 To MAX_MAP_NPCS

                                    If TempPlayer(playerindex).PetTargetType > 0 Then Exit For
                                    If PetAlive(playerindex) Then
                                        n = GetPetRange(playerindex)
                                        distanceX = GetPetX(playerindex) - MapNPC(GetPlayerMap(playerindex)).NPC(i).X
                                        distanceY = GetPetY(playerindex) - MapNPC(GetPlayerMap(playerindex)).NPC(i).Y

                                        ' Make sure we get a positive value
                                        If distanceX < 0 Then distanceX *= -1
                                        If distanceY < 0 Then distanceY *= -1

                                        ' Are they in range?  if so GET'M!
                                        If distanceX <= n And distanceY <= n Then
                                            If GetPetBehaviour(playerindex) = PetAttackBehaviourAttackonsight Then
                                                TempPlayer(playerindex).PetTargetType = Core.TargetType.NPC ' npc
                                                TempPlayer(playerindex).PetTarget = i
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                        End If

                        targetVerify = 0

                        ' // This is used for Pet walking/targetting //

                        ' Make sure theres a npc with the map
                        If TempPlayer(playerindex).PetStunDuration > 0 Then
                            ' check if we can unstun them
                            If GetTimeMs() > TempPlayer(playerindex).PetStunTimer + (TempPlayer(playerindex).PetStunDuration * 1000) Then
                                TempPlayer(playerindex).PetStunDuration = 0
                                TempPlayer(playerindex).PetStunTimer = 0
                            End If
                        Else
                            target = TempPlayer(playerindex).PetTarget
                            targetType = TempPlayer(playerindex).PetTargetType

                            ' Check to see if its time for the npc to walk
                            If GetPetBehaviour(playerindex) <> PetAttackBehaviourDonothing Then

                                If targetType = Core.TargetType.Player Then
                                    ' Check to see if we are following a player or not
                                    If target > 0 Then

                                        ' Check if the player is even playing, if so follow'm
                                        If IsPlaying(target) And GetPlayerMap(target) = mapNum Then
                                            If target <> playerindex Then
                                                didWalk = 0
                                                targetVerify = 1
                                                targetY = GetPlayerY(target)
                                                targetX = GetPlayerX(target)
                                            End If
                                        Else
                                            TempPlayer(playerindex).PetTargetType = 0 ' clear
                                            TempPlayer(playerindex).PetTarget = 0
                                        End If
                                    End If
                                ElseIf targetType = Core.TargetType.NPC Then
                                    If target > 0 Then
                                       If MapNPC(MapNum).NPC(target).Num > 0 Then
                                            didWalk = 0
                                            targetVerify = 1
                                            targetY = MapNPC(MapNum).NPC(target).Y
                                            targetX = MapNPC(MapNum).NPC(target).X
                                        Else
                                            TempPlayer(playerindex).PetTargetType = 0 ' clear
                                            TempPlayer(playerindex).PetTarget = 0
                                        End If
                                    End If
                                ElseIf targetType = Core.TargetType.Pet Then
                                    If target > 0 Then
                                        If IsPlaying(target) And GetPlayerMap(target) = mapNum And PetAlive(target) Then
                                            didWalk = 0
                                            targetVerify = 1
                                            targetY = GetPetY(target)
                                            targetX = GetPetX(target)
                                        Else
                                            TempPlayer(playerindex).PetTargetType = 0 ' clear
                                            TempPlayer(playerindex).PetTarget = 0
                                        End If
                                    End If
                                End If
                            End If

                            If targetVerify Then
                                didWalk = 0

                                If IsOneBlockAway(GetPetX(playerindex), GetPetY(playerindex), targetX, targetY) Then
                                    If GetPetX(playerindex) < targetX Then
                                        PetDir(playerindex, DirectionType.Right)
                                        didWalk = 1
                                    ElseIf GetPetX(playerindex) > targetX Then
                                        PetDir(playerindex, DirectionType.Left)
                                        didWalk = 1
                                    ElseIf GetPetY(playerindex) < targetY Then
                                        PetDir(playerindex, DirectionType.Up)
                                        didWalk = 1
                                    ElseIf GetPetY(playerindex) > targetY Then
                                        PetDir(playerindex, DirectionType.Down)
                                        didWalk = 1
                                    End If
                                Else
                                    didWalk = PetTryWalk(playerindex, targetX, targetY)
                                End If

                            ElseIf TempPlayer(playerindex).PetBehavior = PetBehaviourGoto And targetVerify = 0 Then

                                If GetPetX(playerindex) = TempPlayer(playerindex).GoToX And GetPetY(playerindex) = TempPlayer(playerindex).GoToY Then
                                    'Unblock these for the random turning
                                    'i = Int(Rnd() * 4)
                                    'PetDir(playerindex, i)
                                Else
                                    didWalk = 0
                                    targetX = TempPlayer(playerindex).GoToX
                                    targetY = TempPlayer(playerindex).GoToY
                                    didWalk = PetTryWalk(playerindex, targetX, targetY)

                                    If didWalk = 0 Then
                                        tmpdir = Int(Rnd() * 4)

                                        If tmpdir = 1 Then
                                            tmpdir = Int(Rnd() * 4)
                                            If CanPetMove(playerindex, mapNum, tmpdir) Then
                                                PetMove(playerindex, mapNum, tmpdir, MovementType.Walking)
                                            End If
                                        End If
                                    End If
                                End If

                            ElseIf TempPlayer(playerindex).PetBehavior = PetBehaviourFollow Then

                                If IsPetByPlayer(playerindex) Then
                                    'Unblock these to enable random turning
                                    'i = Int(Rnd() * 4)
                                    'PetDir(playerindex, i)
                                Else
                                    didWalk = 0
                                    targetX = GetPlayerX(playerindex)
                                    targetY = GetPlayerY(playerindex)
                                    didWalk = PetTryWalk(playerindex, targetX, targetY)

                                    If didWalk = 0 Then
                                        tmpdir = Int(Rnd() * 4)
                                        If tmpdir = 1 Then
                                            tmpdir = Int(Rnd() * 4)
                                            If CanPetMove(playerindex, mapNum, tmpdir) Then
                                                PetMove(playerindex, mapNum, tmpdir, MovementType.Walking)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If

                        ' // This is used for pets to attack targets //

                        ' Make sure theres a npc with the map
                        target = TempPlayer(playerindex).PetTarget
                        targetType = TempPlayer(playerindex).PetTargetType

                        ' Check if the pet can attack the targeted player
                        If target > 0 Then
                            If targetType = Core.TargetType.Player Then
                                ' Is the target playing and on the same map?
                                If IsPlaying(target) And GetPlayerMap(target) = mapNum Then
                                    If playerindex <> target Then TryPetAttackPlayer(playerindex, target)
                                Else
                                    ' Player left map or game, set target to 0
                                    TempPlayer(playerindex).PetTarget = 0
                                    TempPlayer(playerindex).PetTargetType = 0 ' clear

                                End If
                            ElseIf targetType = Core.TargetType.NPC Then 
                               If MapNPC(GetPlayerMap(playerindex)).NPC(TempPlayer(playerindex).PetTarget).Num > 0 Then
                                    TryPetAttackNpc(playerindex, TempPlayer(playerindex).PetTarget)
                                Else
                                    ' Player left map or game, set target to 0
                                    TempPlayer(playerindex).PetTarget = 0
                                    TempPlayer(playerindex).PetTargetType = 0 ' clear
                                End If
                            ElseIf targetType = Core.TargetType.Pet Then
                                ' Is the target playing and on the same map? And is pet alive??
                                If IsPlaying(target) And GetPlayerMap(target) = mapNum And PetAlive(target) Then
                                    TryPetAttackPet(playerindex, target)
                                Else
                                    ' Player left map or game, set target to 0
                                    TempPlayer(playerindex).PetTarget = 0
                                    TempPlayer(playerindex).PetTargetType = 0 ' clear
                                End If
                            End If
                        End If

                        ' ////////////////////////////////////////////
                        ' // This is used for regenerating Pet's HP //
                        ' ////////////////////////////////////////////
                        ' Check to see if we want to regen some of the npc's hp
                        If Not TempPlayer(playerindex).PetstopRegen Then
                            If PetAlive(playerindex) And tickCount > givePetHpTimer + 10000 Then
                                If GetPetVital(playerindex, VitalType.HP) > 0 Then
                                    SetPetVital(playerindex, VitalType.HP, GetPetVital(playerindex, VitalType.HP) + GetPetVitalRegen(playerindex, VitalType.HP))
                                    SetPetVital(playerindex, VitalType.SP, GetPetVital(playerindex, VitalType.SP) + GetPetVitalRegen(playerindex, VitalType.SP))

                                    ' Check if they have more then they should and if so just set it to max
                                    If GetPetVital(playerindex, VitalType.HP) > GetPetMaxVital(playerindex, VitalType.HP) Then
                                        SetPetVital(playerindex, VitalType.HP, GetPetMaxVital(playerindex, VitalType.HP))
                                    End If

                                    If GetPetVital(playerindex, VitalType.SP) > GetPetMaxVital(playerindex, VitalType.SP) Then
                                        SetPetVital(playerindex, VitalType.SP, GetPetMaxVital(playerindex, VitalType.SP))
                                    End If

                                    If Not GetPetVital(playerindex, VitalType.HP) = GetPetMaxVital(playerindex, VitalType.HP) Then
                                        SendPetVital(playerindex, VitalType.HP)
                                        SendPetVital(playerindex, VitalType.SP)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If

            Next

        Next

        ' Make sure we reset the timer for npc hp regeneration
        If GetTimeMs() > givePetHpTimer + 10000 Then
            givePetHpTimer = GetTimeMs()
        End If
    End Sub

    Sub SummonPet(index As Integer)
        Type.Player(index).Pet.Alive = 1
        PlayerMsg(index, "You summoned your " & GetPetName(index) & "!", ColorType.BrightGreen)
        SendUpdatePlayerPet(index, False)
    End Sub

    Sub RecallPet(index As Integer)
        PlayerMsg(index, "You recalled your " & GetPetName(index) & "!", ColorType.BrightGreen)
        Type.Player(index).Pet.Alive = 0
        SendUpdatePlayerPet(index, False)
    End Sub

    Sub ReleasePet(index As Integer)
        Dim i As Integer

        Type.Player(index).Pet.Alive = 0
        Type.Player(index).Pet.Num = 0
        Type.Player(index).Pet.AttackBehaviour = 0
        Type.Player(index).Pet.Dir = 0
        Type.Player(index).Pet.Health = 0
        Type.Player(index).Pet.Level = 0
        Type.Player(index).Pet.Mana = 0
        Type.Player(index).Pet.X = 0
        Type.Player(index).Pet.Y = 0

        TempPlayer(index).PetTarget = 0
        TempPlayer(index).PetTargetType = 0
        TempPlayer(index).GoToX = -1
        TempPlayer(index).GoToY = -1

        For i = 1 To 4
            Type.Player(index).Pet.Skill(i) = 0
        Next

        For i = 1 To StatType.Count - 1
            Type.Player(index).Pet.Stat(i) = 0
        Next

        SendUpdatePlayerPet(index, False)

        PlayerMsg(index, "You released your pet!", ColorType.BrightGreen)

        For i = 1 To MAX_MAP_NPCS
           If MapNPC(GetPlayerMap(index)).NPC(i).Vital(VitalType.HP) > 0 Then
               If MapNPC(GetPlayerMap(index)).NPC(i).TargetType = TargetType.Pet Then
                   If MapNPC(GetPlayerMap(index)).NPC(i).Target = index Then
                        MapNPC(GetPlayerMap(index)).NPC(i).TargetType = TargetType.Player
                        MapNPC(GetPlayerMap(index)).NPC(i).Target = index
                    End If
                End If
            End If
        Next

    End Sub

    Sub AdoptPet(index As Integer, petNum As Integer)

        If GetPetNum(index) = 0 Then
            PlayerMsg(index, "You have adopted a " & Pet(petNum).Name, ColorType.BrightGreen)
        Else
            PlayerMsg(index, "You allready have a " & Pet(petNum).Name & ", release your old pet first!", ColorType.BrightGreen)
            Exit Sub
        End If

        Type.Player(index).Pet.Num = petNum

        For i = 1 To 4
            Type.Player(index).Pet.Skill(i) = Pet(petNum).Skill(i)
        Next

        If Pet(petNum).StatType = 0 Then
            Type.Player(index).Pet.Health = GetPlayerMaxVital(index, VitalType.HP)
            Type.Player(index).Pet.Mana = GetPlayerMaxVital(index, VitalType.SP)
            Type.Player(index).Pet.Level = GetPlayerLevel(index)

            For i = 1 To StatType.Count - 1
                Type.Player(index).Pet.Stat(i) = Type.Player(index).Stat(i)
            Next

            Type.Player(index).Pet.AdoptiveStats = 1
        Else
            For i = 1 To StatType.Count - 1
                Type.Player(index).Pet.Stat(i) = Pet(petNum).Stat(i)
            Next

            Type.Player(index).Pet.Level = Pet(petNum).Level
            Type.Player(index).Pet.AdoptiveStats = 0
            Type.Player(index).Pet.Health = GetPetMaxVital(index, VitalType.HP)
            Type.Player(index).Pet.Mana = GetPetMaxVital(index, VitalType.SP)
        End If

        Type.Player(index).Pet.X = GetPlayerX(index)
        Type.Player(index).Pet.Y = GetPlayerY(index)

        Type.Player(index).Pet.Alive = 1
        Type.Player(index).Pet.Points = 0
        Type.Player(index).Pet.Exp = 0

        Type.Player(index).Pet.AttackBehaviour = PetAttackBehaviourGuard 'By default it will guard but this can be changed

        SendUpdatePlayerPet(index, False)

    End Sub

    Sub PetMove(index As Integer, mapNum As Integer, dir As Integer, movement As Integer)
        Dim buffer As New ByteStream(4)

       If MapNum <= 0 Or mapNum > MAX_MAPS Or index < 0 Or index > MAX_PLAYERS Or dir <= DirectionType.None Or Dir > DirectionType.Left Or movement < 0 Or movement > 2 Then
            Exit Sub
        End If

        Type.Player(index).Pet.Dir = dir

        Select Case dir
            Case DirectionType.Up
                SetPetY(index, GetPetY(index) - 1)

            Case DirectionType.Down
                SetPetY(index, GetPetY(index) + 1)

            Case DirectionType.Left
                SetPetX(index, GetPetX(index) - 1)

            Case DirectionType.Right
                SetPetX(index, GetPetX(index) + 1)
        End Select

        buffer.WriteInt32(ServerPackets.SPetMove)
        buffer.WriteInt32(index)
        buffer.WriteInt32(GetPetX(index))
        buffer.WriteInt32(GetPetY(index))
        buffer.WriteInt32(GetPetDir(index))
        buffer.WriteInt32(movement)
        SendDataToMap(MapNum, buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Function CanPetMove(index As Integer, mapNum As Integer, dir As Byte) As Boolean
        Dim i As Integer, n As Integer, n2 As Integer
        Dim x As Integer, y As Integer

       If MapNum <= 0 Or mapNum > MAX_MAPS Or index < 0 Or index > MAX_PLAYERS Or dir <= DirectionType.None Or Dir > DirectionType.Left Then
            Exit Function
        End If

        If index <= 0 Or index > MAX_PLAYERS Then Exit Function

        x = GetPetX(index)
        y = GetPetY(index)

        If x < 0 Or x > Type.Map(MapNum).MaxX Then Exit Function
        If y < 0 Or y > Type.Map(MapNum).MaxY Then Exit Function

        CanPetMove = 1

        If TempPlayer(index).PetskillBuffer.Skill > 0 Then
            CanPetMove = 0
            Exit Function
        End If

        Select Case dir

            Case DirectionType.Up
                ' Check to make sure not outside of boundries
                If y > 0 Then
                    n = Type.Map(MapNum).Tile(x, y - 1).Type
                    n2 = Type.Map(MapNum).Tile(x, y - 1).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.NPCSpawn And n2 <> TileType.NPCSpawn Then
                        CanPetMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = GetPetX(index) + 1) And (GetPlayerY(i) = GetPetY(index) - 1) Then
                                CanPetMove = 0
                                Exit Function
                            ElseIf PetAlive(i) And (GetPlayerMap(i) = mapNum) And (GetPetX(i) = GetPetX(index)) And (GetPetY(i) = GetPetY(index) - 1) Then
                                CanPetMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = GetPetX(index)) And (MapNPC(MapNum).NPC(i).Y = GetPetY(index) - 1) Then
                            CanPetMove = 0
                            Exit Function
                        End If
                    Next

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(GetPetX(index), GetPetY(index)).DirBlock, DirectionType.Up) Then
                        CanPetMove = 0
                        Exit Function
                    End If
                Else
                    CanPetMove = 0
                End If

            Case DirectionType.Down
                ' Check to make sure not outside of boundries
                If y < Type.Map(MapNum).MaxY Then
                    n = Type.Map(MapNum).Tile(x, y + 1).Type
                    n2 = Type.Map(MapNum).Tile(x, y + 1).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.NPCSpawn And n2 <> TileType.NPCSpawn Then
                        CanPetMove = 0
                        Exit Function
                    End If

                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = GetPetX(index)) And (GetPlayerY(i) = GetPetY(index) + 1) Then
                                CanPetMove = 0
                                Exit Function
                            ElseIf PetAlive(i) And (GetPlayerMap(i) = mapNum) And (GetPetX(i) = GetPetX(index)) And (GetPetY(i) = GetPetY(index) + 1) Then
                                CanPetMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = GetPetX(index)) And (MapNPC(MapNum).NPC(i).Y = GetPetY(index) + 1) Then
                            CanPetMove = 0
                            Exit Function
                        End If
                    Next

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(GetPetX(index), GetPetY(index)).DirBlock, DirectionType.Down) Then
                        CanPetMove = 0
                        Exit Function
                    End If
                Else
                    CanPetMove = 0
                End If

            Case DirectionType.Left

                ' Check to make sure not outside of boundries
                If x > 0 Then
                    n = Type.Map(MapNum).Tile(x - 1, y).Type
                    n2 = Type.Map(MapNum).Tile(x - 1, y).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.NPCSpawn And n2 <> TileType.NPCSpawn Then
                        CanPetMove = 0
                        Exit Function
                    End If

                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = GetPetX(index) - 1) And (GetPlayerY(i) = GetPetY(index)) Then
                                CanPetMove = 0
                                Exit Function
                            ElseIf PetAlive(i) And (GetPlayerMap(i) = mapNum) And (GetPetX(i) = GetPetX(index) - 1) And (GetPetY(i) = GetPetY(index)) Then
                                CanPetMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = GetPetX(index) - 1) And (MapNPC(MapNum).NPC(i).Y = GetPetY(index)) Then
                            CanPetMove = 0
                            Exit Function
                        End If
                    Next

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(GetPetX(index), GetPetY(index)).DirBlock, DirectionType.Left) Then
                        CanPetMove = 0
                        Exit Function
                    End If
                Else
                    CanPetMove = 0
                End If

            Case DirectionType.Right
                ' Check to make sure not outside of boundries
                If x < Type.Map(MapNum).MaxX Then
                    n = Type.Map(MapNum).Tile(x + 1, y).Type
                    n2 = Type.Map(MapNum).Tile(x + 1, y).Type2

                    ' Check to make sure that the tile is walkable
                    If n <> TileType.NPCSpawn And n2 <> TileType.NPCSpawn Then
                        CanPetMove = 0
                        Exit Function
                    End If

                    For i = 1 To Socket.HighIndex()
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = GetPetX(index) + 1) And (GetPlayerY(i) = GetPetY(index)) Then
                                CanPetMove = 0
                                Exit Function
                            ElseIf PetAlive(i) And (GetPlayerMap(i) = mapNum) And (GetPetX(i) = GetPetX(index) + 1) And (GetPetY(i) = GetPetY(index)) Then
                                CanPetMove = 0
                                Exit Function
                            End If
                        End If
                    Next

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).Num > 0) And (MapNPC(MapNum).NPC(i).X = GetPetX(index) + 1) And (MapNPC(MapNum).NPC(i).Y = GetPetY(index)) Then
                            CanPetMove = 0
                            Exit Function
                        End If
                    Next

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(GetPetX(index), GetPetY(index)).DirBlock, DirectionType.Right) Then
                        CanPetMove = 0
                        Exit Function
                    End If
                Else
                    CanPetMove = 0
                End If

        End Select

    End Function

    Sub PetDir(index As Integer, dir As Integer)
        Dim buffer As New ByteStream(4)

        If index <= 0 Or index > MAX_PLAYERS Or dir <= DirectionType.None Or Dir > DirectionType.Left Then Exit Sub

        If TempPlayer(index).PetskillBuffer.Skill > 0 Then Exit Sub

        Type.Player(index).Pet.Dir = dir

        buffer.WriteInt32(ServerPackets.SPetDir)
        buffer.WriteInt32(index)
        buffer.WriteInt32(dir)
        SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Function PetTryWalk(index As Integer, targetX As Integer, targetY As Integer) As Boolean
        Dim i As Integer, x As Integer, didwalk As Boolean
        Dim mapNum As Integer

        mapNum = GetPlayerMap(index)
        x = index

        If IsOneBlockAway(targetX, targetY, GetPetX(index), GetPetY(index)) = 0 Then

            If PathfindingType = 1 Then
                i = Int(Rnd() * 5)

                ' Lets move the pet
                Select Case i
                    Case 0
                        ' Up
                        If Type.Player(x).Pet.Y > targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Up) Then
                                PetMove(x, mapNum, DirectionType.Up, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Down
                        If Type.Player(x).Pet.Y < targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Down) Then
                                PetMove(x, mapNum, DirectionType.Down, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Left
                        If Type.Player(x).Pet.X > targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Left) Then
                                PetMove(x, mapNum, DirectionType.Left, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Right
                        If Type.Player(x).Pet.X < targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Right) Then
                                PetMove(x, mapNum, DirectionType.Right, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If
                    Case 1

                        ' Right
                        If Type.Player(x).Pet.X < targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Right) Then
                                PetMove(x, mapNum, DirectionType.Right, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Left
                        If Type.Player(x).Pet.X > targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Left) Then
                                PetMove(x, mapNum, DirectionType.Left, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Down
                        If Type.Player(x).Pet.Y < targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Down) Then
                                PetMove(x, mapNum, DirectionType.Down, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Up
                        If Type.Player(x).Pet.Y > targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Up) Then
                                PetMove(x, mapNum, DirectionType.Up, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                    Case 2

                        ' Down
                        If Type.Player(x).Pet.Y < targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Down) Then
                                PetMove(x, mapNum, DirectionType.Down, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Up
                        If Type.Player(x).Pet.Y > targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Up) Then
                                PetMove(x, mapNum, DirectionType.Up, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Right
                        If Type.Player(x).Pet.X < targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Right) Then
                                PetMove(x, mapNum, DirectionType.Right, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Left
                        If Type.Player(x).Pet.X > targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Left) Then
                                PetMove(x, mapNum, DirectionType.Left, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                    Case 3

                        ' Left
                        If Type.Player(x).Pet.X > targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Left) Then
                                Call PetMove(x, mapNum, DirectionType.Left, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Right
                        If Type.Player(x).Pet.X < targetX And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Right) Then
                                PetMove(x, mapNum, DirectionType.Right, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Up
                        If Type.Player(x).Pet.Y > targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Up) Then
                                PetMove(x, mapNum, DirectionType.Up, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                        ' Down
                        If Type.Player(x).Pet.Y < targetY And Not didwalk Then
                            If CanPetMove(x, mapNum, DirectionType.Down) Then
                                PetMove(x, mapNum, DirectionType.Down, MovementType.Walking)
                                didwalk = 1
                            End If
                        End If

                End Select

                ' Check if we can't move and if Target is behind something and if we can just switch dirs
                If Not didwalk Then
                    If GetPetX(x) - 1 = targetX And GetPetY(x) = targetY Then

                        If GetPetDir(x) <> DirectionType.Left Then
                            PetDir(x, DirectionType.Left)
                        End If

                        didwalk = 1
                    End If

                    If GetPetX(x) + 1 = targetX And GetPetY(x) = targetY Then

                        If GetPetDir(x) <> DirectionType.Right Then
                            PetDir(x, DirectionType.Right)
                        End If

                        didwalk = 1
                    End If

                    If GetPetX(x) = targetX And GetPetY(x) - 1 = targetY Then

                        If GetPetDir(x) <> DirectionType.Up Then
                            PetDir(x, DirectionType.Up)
                        End If

                        didwalk = 1
                    End If

                    If GetPetX(x) = targetX And GetPetY(x) + 1 = targetY Then

                        If GetPetDir(x) <> DirectionType.Down Then
                            PetDir(x, DirectionType.Down)
                        End If

                        didwalk = 1
                    End If
                End If
            Else
                'Pathfind
                i = FindPetPath(MapNum, x, targetX, targetY)

                If i < 4 Then 'Returned an answer. Move the pet
                    If CanPetMove(x, mapNum, i) Then
                        PetMove(x, mapNum, i, MovementType.Walking)
                        didwalk = 1
                    End If
                End If
            End If
        Else

            'Look to target
            If GetPetX(index) > TempPlayer(index).GoToX Then
                If CanPetMove(x, mapNum, DirectionType.Left) Then
                    PetMove(x, mapNum, DirectionType.Left, MovementType.Walking)
                    didwalk = 1
                Else
                    PetDir(x, DirectionType.Left)
                    didwalk = 1
                End If

            ElseIf GetPetX(index) < TempPlayer(index).GoToX Then

                If CanPetMove(x, mapNum, DirectionType.Right) Then
                    PetMove(x, mapNum, DirectionType.Right, MovementType.Walking)
                    didwalk = 1
                Else
                    PetDir(x, DirectionType.Right)
                    didwalk = 1
                End If

            ElseIf GetPetY(index) > TempPlayer(index).GoToY Then

                If CanPetMove(x, mapNum, DirectionType.Up) Then
                    PetMove(x, mapNum, DirectionType.Up, MovementType.Walking)
                    didwalk = 1
                Else
                    PetDir(x, DirectionType.Up)
                    didwalk = 1
                End If

            ElseIf GetPetY(index) < TempPlayer(index).GoToY Then

                If CanPetMove(x, mapNum, DirectionType.Down) Then
                    PetMove(x, mapNum, DirectionType.Down, MovementType.Walking)
                    didwalk = 1
                Else
                    PetDir(x, DirectionType.Down)
                    didwalk = 1
                End If
            End If
        End If

        ' We could not move so Target must be behind something, walk randomly.
        If Not didwalk Then
            i = Int(Rnd() * 2)

            If i = 1 Then
                i = Int(Rnd() * 4)

                If CanPetMove(x, mapNum, i) Then
                    PetMove(x, mapNum, i, MovementType.Walking)
                End If
            End If
        End If

        PetTryWalk = didwalk

    End Function

    Function FindPetPath(MapNum As Integer, index As Integer, targetX As Integer, targetY As Integer) As Integer

        Dim tim As Integer, sX As Integer, sY As Integer, pos(,) As Integer, reachable As Boolean, j As Integer, lastSum As Integer, sum As Integer, fx As Integer, fy As Integer, i As Integer

        Dim path() As Point, lastX As Integer, lastY As Integer, did As Boolean

        'Initialization phase

        tim = 0
        sX = GetPetX(index)
        sY = GetPetY(index)

        fx = targetX
        fy = targetY

        If fx = -1 Then Exit Function
        If fy = -1 Then Exit Function

        ReDim pos(Type.Map(MapNum).MaxX, Type.Map(MapNum).MaxY)
        'pos = MapBlocks(MapNum).Blocks

        pos(sX, sY) = 100 + tim
        pos(fx, fy) = 2

        'reset reachable
        reachable = 0

        'Do while reachable is false... if its set true in progress, we jump out
        'If the path is decided unreachable in process, we will use exit sub. Not proper,
        'but faster ;-)
        Do While reachable = 0

            'we loop through all squares
            For j = 0 To Type.Map(MapNum).MaxY
                For i = 0 To Type.Map(MapNum).MaxX

                    'If j = 10 And i = 0 Then MsgBox "hi!"
                    'If they are to be extended, the pointer TIM is on them
                    If pos(i, j) = 100 + tim Then

                        'The part is to be extended, so do it
                        'We have to make sure that there is a pos(i+1,j) BEFORE we actually use it,
                        'because then we get error... If the square is on side, we dont test for this one!
                        If i < Type.Map(MapNum).MaxX Then

                            'If there isnt a wall, or any other... thing
                            If pos(i + 1, j) = 0 Then
                                'Expand it, and make its pos equal to tim+1, so the next time we make this loop,
                                'It will exapand that square too! This is crucial part of the program
                                pos(i + 1, j) = 100 + tim + 1
                            ElseIf pos(i + 1, j) = 2 Then
                                'If the position is no 0 but its 2 (FINISH) then Reachable = 1!!! We found end
                                reachable = 1
                            End If
                        End If

                        'This is the same as the last one, as i said a lot of copy paste work and editing that
                        'This is simply another side that we have to test for... so instead of i+1 we have i-1
                        'Its actually pretty same then... I wont comment it therefore, because its only repeating
                        'same thing with minor changes to check sides
                        If i > 0 Then
                            If pos((i - 1), j) = 0 Then
                                pos(i - 1, j) = 100 + tim + 1
                            ElseIf pos(i - 1, j) = 2 Then
                                reachable = 1
                            End If
                        End If

                        If j < Type.Map(MapNum).MaxY Then
                            If pos(i, j + 1) = 0 Then
                                pos(i, j + 1) = 100 + tim + 1
                            ElseIf pos(i, j + 1) = 2 Then
                                reachable = 1
                            End If
                        End If

                        If j > 0 Then
                            If pos(i, j - 1) = 0 Then
                                pos(i, j - 1) = 100 + tim + 1
                            ElseIf pos(i, j - 1) = 2 Then
                                reachable = 1
                            End If
                        End If
                    End If
                Next
            Next

            'If the reachable is STILL false, then
            If reachable = 0 Then
                'reset sum
                sum = 0

                For j = 0 To Type.Map(MapNum).MaxY
                    For i = 0 To Type.Map(MapNum).MaxX
                        'we add up ALL the squares
                        sum += pos(i, j)
                    Next i
                Next j

                'Now if the sum is euqal to the last sum, its not reachable, if it isnt, then we store
                'sum to lastsum
                If sum = lastSum Then
                    FindPetPath = 4
                    Exit Function
                Else
                    lastSum = sum
                End If
            End If

            'we increase the pointer to point to the next squares to be expanded
            tim += 1
        Loop

        'We work backwards to find the way...
        lastX = fx
        lastY = fy

        ReDim path(tim + 1)

        'The following code may be a little bit confusing but ill try my best to explain it.
        'We are working backwards to find ONE of the shortest ways back to Start.
        'So we repeat the loop until the LastX and LastY arent in start. Look in the code to see
        'how LastX and LasY change
        Do While lastX <> sX Or lastY <> sY
            'We decrease tim by one, and then we are finding any adjacent square to the final one, that
            'has that value. So lets say the tim would be 5, because it takes 5 steps to get to the target.
            'Now everytime we decrease that, so we make it 4, and we look for any adjacent square that has
            'that value. When we find it, we just color it yellow as for the solution
            tim -= 1
            'reset did to false
            did = 0

            'If we arent on edge
            If lastX < Type.Map(MapNum).MaxX Then

                'check the square on the right of the solution. Is it a tim-1 one? or just a blank one
                If pos(lastX + 1, lastY) = 100 + tim Then
                    'if it, then make it yellow, and change did to true
                    lastX += 1
                    did = 1
                End If
            End If

            'This will then only work if the previous part didnt execute, and did is still false. THen
            'we want to check another square, the on left. Is it a tim-1 one ?
            If did = 0 Then
                If lastX > 0 Then
                    If pos(lastX - 1, lastY) = 100 + tim Then
                        lastX -= 1
                        did = 1
                    End If
                End If
            End If

            'We check the one below it
            If did = 0 Then
                If lastY < Type.Map(MapNum).MaxY Then
                    If pos(lastX, lastY + 1) = 100 + tim Then
                        lastY += 1
                        did = 1
                    End If
                End If
            End If

            'And above it. One of these have to be it, since we have found the solution, we know that already
            'there is a way back.
            If did = 0 Then
                If lastY > 0 Then
                    If pos(lastX, lastY - 1) = 100 + tim Then
                        lastY -= 1
                    End If
                End If
            End If

            path(tim).X = lastX
            path(tim).Y = lastY
        Loop

        'Ok lets look at the first step and see what direction we should take.
        If path(1).X > lastX Then
            FindPetPath = DirectionType.Right
        ElseIf path(1).Y > lastY Then
            FindPetPath = DirectionType.Down
        ElseIf path(1).Y < lastY Then
            FindPetPath = DirectionType.Up
        ElseIf path(1).X < lastX Then
            FindPetPath = DirectionType.Left
        End If

    End Function

    Function GetPetDamage(index As Integer) As Integer
        GetPetDamage = 0

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or index < 0 Or index > MAX_PLAYERS Or Not PetAlive(index) Then
            Exit Function
        End If

        GetPetDamage = (Type.Player(index).Pet.Stat(StatType.Strength) * 2) + (Type.Player(index).Pet.Level * 3) + Random.NextDouble(0, 20)

    End Function

    Friend Function CanPetCrit(index As Integer) As Boolean
        Dim rate As Integer
        Dim rndNum As Integer

        If Not PetAlive(index) Then Exit Function

        CanPetCrit = 0

        rate = Type.Player(index).Pet.Stat(StatType.Luck) / 3
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then CanPetCrit = 1

    End Function

    Function IsPetByPlayer(index As Integer) As Boolean
        Dim x As Integer, y As Integer, x1 As Integer, y1 As Integer

        If index <= 0 Or index > MAX_PLAYERS Or Not PetAlive(index) Then Exit Function

        IsPetByPlayer = 0

        x = GetPlayerX(index)
        y = GetPlayerY(index)
        x1 = GetPetX(index)
        y1 = GetPetY(index)

        If x = x1 Then
            If y = y1 + 1 Or y = y1 - 1 Then
                IsPetByPlayer = 1
            End If
        ElseIf y = y1 Then
            If x = x1 - 1 Or x = x1 + 1 Then
                IsPetByPlayer = 1
            End If
        End If

    End Function

    Function GetPetVitalRegen(index As Integer, vital As VitalType) As Integer
        Dim i As Integer

        If index <= 0 Or index > MAX_PLAYERS Or Not PetAlive(index) Then
            GetPetVitalRegen = 0
            Exit Function
        End If

        Select Case vital
            Case VitalType.HP
                i = (GetPlayerStat(index, StatType.Spirit) * 0.8) + 6

            Case VitalType.SP
                i = (GetPlayerStat(index, StatType.Spirit) / 4) + 12.5
        End Select

        GetPetVitalRegen = i

    End Function

    Sub CheckPetLevelUp(index As Integer)
        Dim expRollover As Integer, levelCount As Integer

        levelCount = 0

        Do While GetPetExp(index) >= GetPetNextLevel(index)
            expRollover = GetPetExp(index) - GetPetNextLevel(index)

            ' can level up?
            If GetPetLevel(index) < 99 And GetPetLevel(index) < Pet(Type.Player(index).Pet.Num).MaxLevel Then
                SetPetLevel(index, GetPetLevel(index) + 1)
            End If

            SetPetPoints(index, GetPetPoints(index) + Pet(Type.Player(index).Pet.Num).LevelPnts)
            SetPetExp(index, expRollover)
            levelCount += 1
        Loop

        If levelCount > 0 Then
            If levelCount = 1 Then
                'singular
                PlayerMsg(index, "Your " & GetPetName(index) & " has gained " & levelCount & " level!", ColorType.BrightGreen)
            Else
                'plural
                PlayerMsg(index, "Your " & GetPetName(index) & " has gained " & levelCount & " levels!", ColorType.BrightGreen)
            End If

            SendPlayerData(index)

        End If

    End Sub

    Friend Sub PetFireProjectile(index As Integer, Skillnum As Integer)
        Dim projectileSlot As Integer, projectileNum As Integer
        Dim mapNum As Integer, i As Integer

        ' Prevent subscript out of range

        mapNum = GetPlayerMap(index)

        'Find a free projectile
        For i = 1 To MAX_PROJECTILES
           If Type.MapProjectile(MapNum, i).ProjectileNum = 0 Then ' Free Projectile
                projectileSlot = i
                Exit For
            End If
        Next

        'Check for no projectile, if so just overwrite the first slot
        If projectileSlot = 0 Then projectileSlot = 1

        If Skillnum <= 0 Or Skillnum > MAX_SKILLS Then Exit Sub

        projectileNum = Type.Skill(skillNum).Projectile

        With MapProjectile(MapNum, projectileSlot)
            .ProjectileNum = projectileNum
            .Owner = index
            .OwnerType = TargetType.Pet
            .Dir = Type.Player(i).Pet.Dir
            .X = Type.Player(i).Pet.X
            .Y = Type.Player(i).Pet.Y
            .Timer = GetTimeMs() + 60000
        End With

        SendProjectileToMap(MapNum, projectileSlot)

    End Sub

#End Region

#Region "Pet > Npc"

    Friend Sub TryPetAttackNpc(index As Integer, mapNpcNum As Integer)
        Dim blockAmount As Integer
        Dim npcnum As Integer
        Dim mapNum As Integer
        Dim damage As Integer


        ' Can we attack the npc?
        If CanPetAttackNpc(index, mapNpcNum) Then

            mapNum = GetPlayerMap(index)
            npcnum = MapNPC(MapNum).NPC(MapNPCNum).Num

            ' check if NPC can avoid the attack
            If CanNpcDodge(npcnum) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
                Exit Sub
            End If

            If CanNpcParry(npcnum) Then
                SendActionMsg(MapNum, "Parry!", ColorType.Pink, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            damage = GetPetDamage(index)

            ' if the npc blocks, take away the block amount
            blockAmount = CanNpcBlock(MapNPCNum)
            damage -= blockAmount

            ' take away armour
            damage -= Random.NextDouble(1, (Type.NPC(NPCNum).Stat(StatType.Luck) * 2))
            ' randomise from 1 to max hit
            damage = Random.NextDouble(1, damage)

            ' * 1.5 if it's a crit!
            If CanPetCrit(index) Then
                damage *= 1.5
                SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, 1, (GetPlayerX(index) * 32), (GetPlayerY(index) * 32))
            End If

            If damage > 0 Then
                PetAttackNpc(index, mapNpcNum, damage)
            Else
                PlayerMsg(index, "Your pet's attack does nothing.", ColorType.BrightRed)
            End If

        End If

    End Sub

    Friend Function CanPetAttackNpc(attacker As Integer, mapnpcnum As Integer, Optional isSkill As Boolean = 0) As Boolean
        Dim mapNum As Integer
        Dim npcnum As Integer
        Dim npcX As Integer
        Dim npcY As Integer
        Dim attackspeed As Integer

        If IsPlaying(attacker) = 0 Or mapnpcnum <= 0 Or mapnpcnum > MAX_MAP_NPCS Or Not PetAlive(attacker) Then
            Exit Function
        End If

        ' Check for subscript out of range
       If MapNPC(GetPlayerMap(attacker)).NPC(MapNPCNum).Num <= 0 Then Exit Function

        mapNum = GetPlayerMap(attacker)
        npcnum = MapNPC(MapNum).NPC(MapNPCNum).Num

        ' Make sure the npc isn't already dead
       If MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) <= 0 Then Exit Function

        ' Make sure they are on the same map
        If IsPlaying(attacker) Then

            If TempPlayer(attacker).PetskillBuffer.Skill > 0 And isSkill = 0 Then Exit Function

            ' exit out early
            If isSkill And npcnum > 0 Then
                If Type.NPC(NPCNum).Behaviour <> NpcBehavior.Friendly And Type.NPC(NPCNum).Behaviour <> NpcBehavior.ShopKeeper Then
                    CanPetAttackNpc = 1
                    Exit Function
                End If
            End If

            attackspeed = 1000 'Pet cannot wield a weapon

            If npcnum > 0 And GetTimeMs() > TempPlayer(attacker).PetAttackTimer + attackspeed Then

                ' Check if at same coordinates
                Select Case GetPetDir(attacker)

                    Case DirectionType.Up
                        npcX = MapNPC(MapNum).NPC(MapNPCNum).X
                        npcY = MapNPC(MapNum).NPC(MapNPCNum).Y + 1

                    Case DirectionType.Down
                        npcX = MapNPC(MapNum).NPC(MapNPCNum).X
                        npcY = MapNPC(MapNum).NPC(MapNPCNum).Y - 1

                    Case DirectionType.Left
                        npcX = MapNPC(MapNum).NPC(MapNPCNum).X + 1
                        npcY = MapNPC(MapNum).NPC(MapNPCNum).Y

                    Case DirectionType.Right
                        npcX = MapNPC(MapNum).NPC(MapNPCNum).X - 1
                        npcY = MapNPC(MapNum).NPC(MapNPCNum).Y

                End Select

                If npcX = GetPetX(attacker) And npcY = GetPetY(attacker) Then
                    If Type.NPC(NPCNum).Behaviour <> NpcBehavior.Friendly And Type.NPC(NPCNum).Behaviour <> NpcBehavior.ShopKeeper Then
                        CanPetAttackNpc = 1
                    Else
                        CanPetAttackNpc = 0
                    End If
                End If
            End If
        End If

    End Function

    Friend Sub PetAttackNpc(attacker As Integer, mapnpcnum As Integer, damage As Integer, Optional skillnum As Integer = 0) ', Optional overTime As Boolean = 0)
        Dim name As String, exp As Integer
        Dim i As Integer, mapNum As Integer, npcnum As Integer

        ' Check for subscript out of range
        If IsPlaying(attacker) = 0 Or mapnpcnum <= 0 Or mapnpcnum > MAX_MAP_NPCS Or damage < 0 Or Not PetAlive(attacker) Then
            Exit Sub
        End If

        mapNum = GetPlayerMap(attacker)
        npcnum = MapNPC(MapNum).NPC(MapNPCNum).Num
        name = Type.NPC(NPCNum).Name

        If skillnum = 0 Then
            ' Send this packet so they can see the pet attacking
            SendPetAttack(attacker, mapNum)
        End If

        ' set the regen timer
        TempPlayer(attacker).PetstopRegen = 1
        TempPlayer(attacker).PetstopRegenTimer = GetTimeMs()

        If damage >= MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) Then

            SendActionMsg(GetPlayerMap(attacker), "-" & MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP), ColorType.BrightRed, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
            SendBlood(GetPlayerMap(attacker), MapNPC(MapNum).NPC(MapNPCNum).X, MapNPC(MapNum).NPC(MapNPCNum).Y)

            ' Calculate exp to give attacker
            exp = Type.NPC(NPCNum).Exp

            ' Make sure we dont get less then 0
            If exp < 0 Then
                exp = 1
            End If

            ' in party?
            If TempPlayer(attacker).InParty > 0 Then
                ' pass through party sharing function
                Party_ShareExp(TempPlayer(attacker).InParty, exp, attacker, mapNum)
            Else
                ' no party - keep exp for self
                GivePlayerExp(attacker, exp)
            End If

            'For n = 0 To 20
            '   If MapNPC(MapNum).NPC(MapNPCNum).Num > 0 Then
            '        'SpawnItem(MapNPC(MapNum).NPC(MapNPCNum).Inventory(n).Num, MapNPC(MapNum).NPC(MapNPCNum).Inventory(n).Value, MapNum, MapNPC(MapNum).NPC(MapNPCNum).x, MapNPC(MapNum).NPC(MapNPCNum).y)
            '        'MapNPC(MapNum).NPC(MapNPCNum).Inventory(n).Value = 0
            '        'MapNPC(MapNum).NPC(MapNPCNum).Inventory(n).Num = 0
            '    End If
            'Next

            ' Now set HP to 0 so we know to actually kill them in the server loop (this prevents subscript out of range)
            MapNPC(MapNum).NPC(MapNPCNum).Num = 0
            MapNPC(MapNum).NPC(MapNPCNum).SpawnWait = GetTimeMs()
            MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) = 0
            MapNPC(MapNum).NPC(MapNPCNum).TargetType = 0
            MapNPC(MapNum).NPC(MapNPCNum).Target = 0

            ' clear DoTs and HoTs
            'For i = 1 To MAX_COTS
            '    With MapNPC(MapNum).NPC(MapNPCNum).DoT(i)
            '        .Skill = 0
            '        .Timer = 0
            '        .Caster = 0
            '        .StartTime = 0
            '        .Used = 0
            '    End With
            '    With MapNPC(MapNum).NPC(MapNPCNum).HoT(i)
            '        .Skill = 0
            '        .Timer = 0
            '        .Caster = 0
            '        .StartTime = 0
            '        .Used = 0
            '    End With
            'Next

            ' send death to the map
            SendNpcDead(MapNum, mapnpcnum)

            'Loop through entire map and purge NPC from targets
            For i = 1 To Socket.HighIndex

                If IsPlaying(i) Then
                    If GetPlayerMap(i) = mapNum Then
                        If TempPlayer(i).TargetType = TargetType.NPC Then
                            If TempPlayer(i).Target = mapnpcnum Then
                                TempPlayer(i).Target = 0
                                TempPlayer(i).TargetType = 0
                                SendTarget(i, 0, 0)
                            End If
                        End If

                        If TempPlayer(i).PetTargetType = TargetType.NPC Then
                            If TempPlayer(i).PetTarget = mapnpcnum Then
                                TempPlayer(i).PetTarget = 0
                                TempPlayer(i).PetTargetType = 0
                            End If
                        End If
                    End If
                End If
            Next
        Else
            ' NPC not dead, just do the damage
            MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) = MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) - damage

            ' Check for a weapon and say damage
            SendActionMsg(MapNum, "-" & damage, ColorType.BrightRed, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
            SendBlood(GetPlayerMap(attacker), MapNPC(MapNum).NPC(MapNPCNum).X, MapNPC(MapNum).NPC(MapNPCNum).Y)

            ' send the sound
            'If Skillnum > 0 Then SendMapSound Attacker, MapNPC(MapNum).NPC(MapNPCNum).x, MapNPC(MapNum).NPC(MapNPCNum).y, SoundEntity.seSkill, Skillnum

            ' Set the NPC target to the player
            MapNPC(MapNum).NPC(MapNPCNum).TargetType = TargetType.Pet ' player's pet
            MapNPC(MapNum).NPC(MapNPCNum).Target = attacker

            ' Now check for guard ai and if so have all onmap guards come after'm
            If Type.NPC(MapNPC(MapNum).NPC(MapNPCNum).Num).Behaviour = NpcBehavior.Guard Then
                For i = 1 To MAX_MAP_NPCS
                   If MapNPC(MapNum).NPC(i).Num = MapNPC(MapNum).NPC(MapNPCNum).Num Then
                        MapNPC(MapNum).NPC(i).Target = attacker
                        MapNPC(MapNum).NPC(i).TargetType = TargetType.Pet ' pet
                    End If
                Next
            End If

            ' set the regen timer
            MapNPC(MapNum).NPC(MapNPCNum).StopRegen = 1
            MapNPC(MapNum).NPC(MapNPCNum).StopRegenTimer = GetTimeMs()

            ' if stunning Skill, stun the npc
            If skillnum > 0 Then
                If Type.Skill(skillNum).StunDuration > 0 Then StunNPC(MapNpcNum, mapNum, skillnum)
                ' DoT
                If Type.Skill(skillNum).Duration > 0 Then
                    'AddDoT_Npc(MapNum, mapnpcnum, Skillnum, Attacker, 3)
                End If
            End If

            SendMapNPCVitals(MapNum, mapnpcnum)
        End If

        If skillnum = 0 Then
            ' Reset attack timer
            TempPlayer(attacker).PetAttackTimer = GetTimeMs()
        End If

    End Sub

#End Region

#Region "Npc > Pet"

    Friend Sub TryNpcAttackPet(MapNpcNum As Integer, index As Integer)

        Dim mapNum As Integer, npcnum As Integer, damage As Integer

        ' Can the npc attack the pet?

        If CanNpcAttackPet(MapNpcNum, index) Then
            mapNum = GetPlayerMap(index)
            npcnum = MapNPC(MapNum).NPC(MapNPCNum).Num

            ' check if Pet can avoid the attack
            If CanPetDodge(index) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, ActionMsgType.Scroll, (GetPetX(index) * 32), (GetPetY(index) * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            damage = GetNpcDamage(npcnum)

            ' take away armour
            damage -= ((GetPetStat(index, StatType.Luck) * 2) + (GetPetLevel(index) * 2))

            ' * 1.5 if crit hit
            If CanNpcCrit(npcnum) Then
                damage *= 1.5
                SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, ActionMsgType.Scroll, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
            End If
        End If

        If damage > 0 Then
            NpcAttackPet(MapNpcNum, index, damage)
        End If

    End Sub

    Function CanNpcAttackPet(MapNpcNum As Integer, index As Integer) As Boolean
        Dim mapNum As Integer
        Dim npcnum As Integer

        CanNpcAttackPet = 0

       If MapNpcNum <= 0 Or mapNpcNum > MAX_MAP_NPCS Or Not IsPlaying(index) Or Not PetAlive(index) Then
            Exit Function
        End If

        ' Check for subscript out of range
       If MapNPC(GetPlayerMap(index)).NPC(MapNPCNum).Num <= 0 Then Exit Function

        mapNum = GetPlayerMap(index)
        npcnum = MapNPC(MapNum).NPC(MapNPCNum).Num

        ' Make sure the npc isn't already dead
       If MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) <= 0 Then Exit Function

        ' Make sure npcs dont attack more then once a second
        If GetTimeMs() < MapNPC(MapNum).NPC(MapNPCNum).AttackTimer + 1000 Then Exit Function

        ' Make sure we dont attack the player if they are switching maps
        If TempPlayer(index).GettingMap = 1 Then Exit Function

        MapNPC(MapNum).NPC(MapNPCNum).AttackTimer = GetTimeMs()

        ' Make sure they are on the same map
        If IsPlaying(index) And PetAlive(index) Then
            If npcnum > 0 Then

                ' Check if at same coordinates
                If (GetPetY(index) + 1 = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPetX(index) = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                    CanNpcAttackPet = 1
                Else

                    If (GetPetY(index) - 1 = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPetX(index) = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                        CanNpcAttackPet = 1
                    Else

                        If (GetPetY(index) = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPetX(index) + 1 = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                            CanNpcAttackPet = 1
                        Else

                            If (GetPetY(index) = MapNPC(MapNum).NPC(MapNPCNum).Y) And (GetPetX(index) - 1 = MapNPC(MapNum).NPC(MapNPCNum).X) Then
                                CanNpcAttackPet = 1
                            End If
                        End If
                    End If
                End If
            End If
        End If

    End Function

    Sub NpcAttackPet(MapNpcNum As Integer, victim As Integer, damage As Integer)
        Dim name As String, mapNum As Integer

        ' Check for subscript out of range
       If MapNpcNum <= 0 Or mapnpcnum > MAX_MAP_NPCS Or IsPlaying(victim) = 0 Or Not PetAlive(victim) Then
            Exit Sub
        End If

        ' Check for subscript out of range
       If MapNPC(GetPlayerMap(victim)).NPC(MapNPCNum).Num <= 0 Then Exit Sub

        mapNum = GetPlayerMap(victim)
        name = Type.NPC(MapNPC(MapNum).NPC(MapNPCNum).Num).Name

        ' Send this packet so they can see the npc attacking
        SendNpcAttack(victim, mapnpcnum)

        If damage <= 0 Then Exit Sub

        ' set the regen timer
        MapNPC(MapNum).NPC(MapNPCNum).StopRegen = 1
        MapNPC(MapNum).NPC(MapNPCNum).StopRegenTimer = GetTimeMs()

        If damage >= GetPetVital(victim, VitalType.HP) Then
            ' Say damage
            SendActionMsg(GetPlayerMap(victim), "-" & GetPetVital(victim, VitalType.HP), ColorType.BrightRed, ActionMsgType.Scroll, (GetPetX(victim) * 32), (GetPetY(victim) * 32))

            ' kill pet
            PlayerMsg(victim, "Your " & GetPetName(victim) & " was killed by a " & Type.NPC(MapNPC(MapNum).NPC(MapNPCNum).Num).Name & ".", ColorType.BrightRed)
            RecallPet(victim)

            ' Now that pet is dead, go for owner
            MapNPC(MapNum).NPC(MapNPCNum).Target = victim
            MapNPC(MapNum).NPC(MapNPCNum).TargetType = TargetType.Player
        Else
            ' Pet not dead, just do the damage
            SetPetVital(victim, VitalType.HP, GetPetVital(victim, VitalType.HP) - damage)
            SendPetVital(victim, VitalType.HP)
            SendAnimation(MapNum, Type.NPC(MapNPC(GetPlayerMap(victim)).NPC(MapNPCNum).Num).Animation, 0, 0, TargetType.Pet, victim)

            ' Say damage
            SendActionMsg(GetPlayerMap(victim), "-" & damage, ColorType.BrightRed, ActionMsgType.Scroll, (GetPetX(victim) * 32), (GetPetY(victim) * 32))
            SendBlood(GetPlayerMap(victim), GetPetX(victim), GetPetY(victim))

            ' set the regen timer
            TempPlayer(victim).PetstopRegen = 1
            TempPlayer(victim).PetstopRegenTimer = GetTimeMs()

            'pet gets attacked, lets set this target
            TempPlayer(victim).PetTarget = mapnpcnum
            TempPlayer(victim).PetTargetType = TargetType.NPC
        End If

    End Sub

#End Region

#Region "Pet > Player"

    Function CanPetAttackPlayer(attacker As Integer, victim As Integer, Optional isSkill As Boolean = 0) As Boolean

        If Not isSkill Then
            If GetTimeMs() < TempPlayer(attacker).PetAttackTimer + 1000 Then Exit Function
        End If

        ' Check for subscript out of range
        If Not IsPlaying(victim) Then Exit Function

        ' Make sure they are on the same map
        If Not GetPlayerMap(attacker) = GetPlayerMap(victim) Then Exit Function

        ' Make sure we dont attack the player if they are switching maps
        If TempPlayer(victim).GettingMap = 1 Then Exit Function

        If TempPlayer(attacker).PetskillBuffer.Skill > 0 And isSkill = 0 Then Exit Function

        If Not isSkill Then
            ' Check if at same coordinates
            Select Case GetPetDir(attacker)
                Case DirectionType.Up
                    If Not (GetPlayerY(victim) + 1 = GetPetY(attacker)) And (GetPlayerX(victim) = GetPetX(attacker)) Then Exit Function

                Case DirectionType.Down
                    If Not (GetPlayerY(victim) - 1 = GetPetY(attacker)) And (GetPlayerX(victim) = GetPetX(attacker)) Then Exit Function

                Case DirectionType.Left
                    If Not (GetPlayerY(victim) = GetPetY(attacker)) And (GetPlayerX(victim) + 1 = GetPetX(attacker)) Then Exit Function

                Case DirectionType.Right
                    If Not (GetPlayerY(victim) = GetPetY(attacker)) And (GetPlayerX(victim) - 1 = GetPetX(attacker)) Then Exit Function

                Case Else
                    Exit Function
            End Select
        End If

        ' CheckIf Type.Map is attackable
       If Type.Map(GetPlayerMap(attacker)).Moral > 0 Then
            If Not Type.Moral(Type.Map(GetPlayerMap(attacker)).Moral).CanPK Then
                If GetPlayerPK(victim) = 0 Then
                    Exit Function
                End If
            End If
        End If

        ' Make sure they have more then 0 hp
        If GetPlayerVital(victim, VitalType.HP) <= 0 Then Exit Function

        ' Check to make sure that they dont have access
        If GetPlayerAccess(attacker) > AccessType.Moderator Then
            PlayerMsg(attacker, "Admins cannot attack other players.", ColorType.Yellow)
            Exit Function
        End If

        ' Check to make sure the victim isn't an admin
        If GetPlayerAccess(victim) > AccessType.Moderator Then
            PlayerMsg(attacker, "You cannot attack " & GetPlayerName(victim) & "!", ColorType.Yellow)
            Exit Function
        End If

        ' Don't attack a party member
        If TempPlayer(attacker).InParty > 0 And TempPlayer(victim).InParty > 0 Then
            If TempPlayer(attacker).InParty = TempPlayer(victim).InParty Then
                PlayerMsg(attacker, "You can't attack another party member!", ColorType.Yellow)
                Exit Function
            End If
        End If

        CanPetAttackPlayer = 1

    End Function

    Sub PetAttackPlayer(attacker As Integer, victim As Integer, damage As Integer, Optional skillNum As Integer = 0)
        Dim exp As Integer, i As Integer

        ' Check for subscript out of range

        If IsPlaying(attacker) = 0 Or IsPlaying(victim) = 0 Or damage < 0 Or PetAlive(attacker) = 0 Then
            Exit Sub
        End If

        If skillNum = 0 Then
            ' Send this packet so they can see the pet attacking
            SendPetAttack(attacker, victim)
        End If

        ' set the regen timer
        TempPlayer(attacker).PetstopRegen = 1
        TempPlayer(attacker).PetstopRegenTimer = GetTimeMs()

        If damage >= GetPlayerVital(victim, VitalType.HP) Then
            SendActionMsg(GetPlayerMap(victim), "-" & GetPlayerVital(victim, VitalType.HP), ColorType.BrightRed, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))

            ' send the sound
            'If skillNum > 0 Then SendMapSound(Victim, GetPlayerX(Victim), GetPlayerY(Victim), SoundEntity.seSkill, skillNum)

            ' Player is dead
            GlobalMsg(GetPlayerName(victim) & " has been killed by " & GetPlayerName(attacker) & "'s " & GetPetName(attacker) & ".")

            ' Calculate exp to give attacker
            exp = (GetPlayerExp(victim) \ 10)

            ' Make sure we dont get less then 0
            If exp < 0 Then
                exp = 0
            End If

            If exp = 0 Then
                PlayerMsg(victim, "You lost no exp.", ColorType.BrightGreen)
                PlayerMsg(attacker, "You received no exp.", ColorType.BrightRed)
            Else
                SetPlayerExp(victim, GetPlayerExp(victim) - exp)
                SendExp(victim)
                PlayerMsg(victim, "You lost " & exp & " exp.", ColorType.BrightRed)

                ' check if we're in a party
                If TempPlayer(attacker).InParty > 0 Then
                    ' pass through party exp share function
                    Party_ShareExp(TempPlayer(attacker).InParty, exp, attacker, GetPlayerMap(attacker))
                Else
                    ' not in party, get exp for self
                    GivePlayerExp(attacker, exp)
                End If
            End If

            ' purge target info of anyone who targetted dead guy
            For i = 1 To Socket.HighIndex

                If IsPlaying(i) And Socket.IsConnected(i) Then
                    If GetPlayerMap(i) = GetPlayerMap(attacker) Then
                        If TempPlayer(i).TargetType = TargetType.Player Then
                            If TempPlayer(i).Target = victim Then
                                TempPlayer(i).Target = 0
                                TempPlayer(i).TargetType = 0
                                SendTarget(i, 0, 0)
                            End If
                        End If

                        If Type.Player(i).Pet.Alive = 1 Then
                            If TempPlayer(i).PetTargetType = TargetType.Player Then
                                If TempPlayer(i).PetTarget = victim Then
                                    TempPlayer(i).PetTarget = 0
                                    TempPlayer(i).PetTargetType = 0
                                End If
                            End If
                        End If
                    End If
                End If
            Next

            If GetPlayerPK(victim) = 0 Then
                If GetPlayerPK(attacker) = 0 Then
                    SetPlayerPK(attacker, 1)
                    SendPlayerData(attacker)
                    GlobalMsg(GetPlayerName(attacker) & " has been deemed a Player Killer")
                End If
            Else
                GlobalMsg(GetPlayerName(victim) & " has paid the price for being a Player Killer!")
            End If

            OnDeath(victim)
        Else
            ' Player not dead, just do the damage
            SetPlayerVital(victim, VitalType.HP, GetPlayerVital(victim, VitalType.HP) - damage)
            SendVital(victim, VitalType.HP)

            ' send the sound
            'If skillNum > 0 Then SendMapSound(Victim, GetPlayerX(Victim), GetPlayerY(Victim), SoundEntity.seSkill, skillNum)

            SendActionMsg(GetPlayerMap(victim), "-" & damage, ColorType.BrightRed, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))
            SendBlood(GetPlayerMap(victim), GetPlayerX(victim), GetPlayerY(victim))

            ' set the regen timer
            TempPlayer(victim).StopRegen = 1
            TempPlayer(victim).StopRegenTimer = GetTimeMs()

            'if a stunning Skill, stun the player
            If skillNum > 0 Then
                If Type.Skill(skillNum).StunDuration > 0 Then StunPlayer(victim, skillNum)

                ' DoT
                If Type.Skill(skillNum).Duration > 0 Then
                    'AddDoT_Player(Victim, skillNum, Attacker)
                End If
            End If
        End If

        ' Reset attack timer
        TempPlayer(attacker).PetAttackTimer = GetTimeMs()

    End Sub

    Friend Sub TryPetAttackPlayer(index As Integer, victim As Integer)
        Dim mapNum As Integer, blockAmount As Integer, damage As Integer

        If GetPlayerMap(index) <> GetPlayerMap(victim) Then Exit Sub

        If Not PetAlive(index) Then Exit Sub

        ' Can the npc attack the player?
        If CanPetAttackPlayer(index, victim) Then
            mapNum = GetPlayerMap(index)

            ' check if PLAYER can avoid the attack
            If CanPlayerDodge(victim) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))
                Exit Sub
            End If

            If CanPlayerParry(victim) Then
                SendActionMsg(MapNum, "Parry!", ColorType.Pink, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            damage = GetPetDamage(index)

            ' if the player blocks, take away the block amount
            blockAmount = CanPlayerBlockHit(victim)
            damage -= blockAmount

            ' take away armour
            damage -= Random.NextDouble(1, (GetPetStat(index, StatType.Luck)) * 2)

            ' randomise for up to 10% lower than max hit
            damage = Random.NextDouble(1, damage)

            ' * 1.5 if crit hit
            If CanPetCrit(index) Then
                damage *= 1.5
                SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, 1, (GetPetX(index) * 32), (GetPetY(index) * 32))
            End If

            If damage > 0 Then
                PetAttackPlayer(index, victim, damage)
            End If

        End If

    End Sub

#End Region

#Region "Pet > Pet"

    Function CanPetAttackPet(attacker As Integer, victim As Integer, Optional isSkill As Integer = 0) As Boolean

        If Not isSkill Then
            If GetTimeMs() < TempPlayer(attacker).PetAttackTimer + 1000 Then Exit Function
        End If

        ' Check for subscript out of range
        If Not IsPlaying(victim) Or Not IsPlaying(attacker) Then Exit Function

        ' Make sure they are on the same map
        If Not GetPlayerMap(attacker) = GetPlayerMap(victim) Then Exit Function

        ' Make sure we dont attack the player if they are switching maps
        If TempPlayer(victim).GettingMap = 1 Then Exit Function

        If TempPlayer(attacker).PetskillBuffer.Skill > 0 And isSkill = 0 Then Exit Function

        If Not isSkill Then

            ' Check if at same coordinates
            Select Case GetPetDir(attacker)
                Case DirectionType.Up
                    If Not ((GetPetY(victim) - 1 = GetPetY(attacker)) And (GetPetX(victim) = GetPetX(attacker))) Then Exit Function

                Case DirectionType.Down
                    If Not ((GetPetY(victim) + 1 = GetPetY(attacker)) And (GetPetX(victim) = GetPetX(attacker))) Then Exit Function

                Case DirectionType.Left
                    If Not ((GetPetY(victim) = GetPetY(attacker)) And (GetPetX(victim) + 1 = GetPetX(attacker))) Then Exit Function

                Case DirectionType.Right
                    If Not ((GetPetY(victim) = GetPetY(attacker)) And (GetPetX(victim) - 1 = GetPetX(attacker))) Then Exit Function

                Case Else
                    Exit Function
            End Select
        End If

        ' CheckIf Type.Map is attackable
       If Type.Map(GetPlayerMap(attacker)).Moral > 0 Then
            If Not Type.Moral(Type.Map(GetPlayerMap(attacker)).Moral).CanPK Then
                If GetPlayerPK(victim) = 0 Then
                    Exit Function
                End If
            End If
        End If

        ' Make sure they have more then 0 hp
        If Type.Player(victim).Pet.Health <= 0 Then Exit Function

        ' Check to make sure that they dont have access
        If GetPlayerAccess(attacker) > AccessType.Moderator Then
            PlayerMsg(attacker, "Admins cannot attack other players.", ColorType.BrightRed)
            Exit Function
        End If

        ' Check to make sure the victim isn't an admin
        If GetPlayerAccess(victim) > AccessType.Moderator Then
            PlayerMsg(attacker, "You cannot attack " & GetPlayerName(victim) & "!", ColorType.BrightRed)
            Exit Function
        End If

        ' Don't attack a party member
        If TempPlayer(attacker).InParty > 0 And TempPlayer(victim).InParty > 0 Then
            If TempPlayer(attacker).InParty = TempPlayer(victim).InParty Then
                PlayerMsg(attacker, "You can't attack another party member!", ColorType.BrightRed)
                Exit Function
            End If
        End If

        If TempPlayer(attacker).InParty > 0 And TempPlayer(victim).InParty > 0 And TempPlayer(attacker).InParty = TempPlayer(victim).InParty Then
            If isSkill > 0 Then
                If Type.Skill(isSkill).Type = SkillType.HealMp Or Type.Skill(isSkill).Type = SkillType.HealHp Then
                    'Carry On :D
                Else
                    Exit Function
                End If
            Else
                Exit Function
            End If
        End If

        CanPetAttackPet = 1

    End Function

    Sub PetAttackPet(attacker As Integer, victim As Integer, damage As Integer, Optional skillnum As Integer = 0)
        Dim exp As Integer, i As Integer

        ' Check for subscript out of range

        If IsPlaying(attacker) = 0 Or IsPlaying(victim) = 0 Or damage < 0 Or PetAlive(attacker) = 0 Or PetAlive(victim) = 0 Then
            Exit Sub
        End If

        If skillnum = 0 Then
            ' Send this packet so they can see the pet attacking
            SendPetAttack(attacker, victim)
        End If

        ' set the regen timer
        TempPlayer(attacker).PetstopRegen = 1
        TempPlayer(attacker).PetstopRegenTimer = GetTimeMs()

        If damage >= GetPetVital(victim, VitalType.HP) Then
            SendActionMsg(GetPlayerMap(victim), "-" & GetPetVital(victim, VitalType.HP), ColorType.BrightRed, ActionMsgType.Scroll, (GetPetX(victim) * 32), (GetPetY(victim) * 32))

            ' send the sound
            'If Skillnum > 0 Then SendMapSound Victim, Player(Victim).characters(TempPlayer(Victim).CurChar).Pet.x, Player(Victim).characters(TempPlayer(Victim).CurChar).Pet.y, SoundEntity.seSkill, Skillnum

            ' purge target info of anyone who targetted dead guy
            For i = 1 To Socket.HighIndex

                If IsPlaying(i) And Socket.IsConnected(i) Then
                    If GetPlayerMap(i) = GetPlayerMap(attacker) Then
                        If TempPlayer(i).TargetType = TargetType.Player Then
                            If TempPlayer(i).Target = victim Then
                                TempPlayer(i).Target = 0
                                TempPlayer(i).TargetType = 0
                                SendTarget(i, 0, 0)
                            End If
                        End If

                        If PetAlive(i) Then
                            If TempPlayer(i).PetTargetType = TargetType.Player Then
                                If TempPlayer(i).PetTarget = victim Then
                                    TempPlayer(i).PetTarget = 0
                                    TempPlayer(i).PetTargetType = 0
                                End If
                            End If
                        End If
                    End If
                End If
            Next

            If GetPlayerPK(victim) = 0 Then
                If GetPlayerPK(attacker) = 0 Then
                    SetPlayerPK(attacker, 1)
                    SendPlayerData(attacker)
                    GlobalMsg(GetPlayerName(attacker) & " has been deemed a Player Killer!!!")
                End If
            Else
                GlobalMsg(GetPlayerName(victim) & " has paid the price for being a Player Killer!!!")
            End If

            ' kill pet
            PlayerMsg(victim, "Your " & GetPetName(victim) & " was killed by " & GetPlayerName(attacker) & "'s " & GetPetName(attacker) & "!", ColorType.BrightRed)
            ReleasePet(victim)
        Else
            ' Player not dead, just do the damage
            SetPetVital(victim, VitalType.HP, GetPetVital(victim, VitalType.HP) - damage)
            SendPetVital(victim, VitalType.HP)

            'Set pet to begin attacking the other pet if it isn't dead or dosent have another target
            If TempPlayer(victim).PetTarget < 0 And TempPlayer(victim).PetBehavior <> PetBehaviourGoto Then
                TempPlayer(victim).PetTarget = attacker
                TempPlayer(victim).PetTargetType = TargetType.Pet
            End If

            ' send the sound
            'If Skillnum > 0 Then SendMapSound Victim, Player(Victim).characters(TempPlayer(Victim).CurChar).Pet.x, Player(Victim).characters(TempPlayer(Victim).CurChar).Pet.y, SoundEntity.seSkill, Skillnum

            SendActionMsg(GetPlayerMap(victim), "-" & damage, ColorType.BrightRed, 1, (GetPetX(victim) * 32), (GetPetY(victim) * 32))
            SendBlood(GetPlayerMap(victim), GetPetX(victim), GetPetY(victim))

            ' set the regen timer
            TempPlayer(victim).PetstopRegen = 1
            TempPlayer(victim).PetstopRegenTimer = GetTimeMs()

            'if a stunning Skill, stun the player
            If skillnum > 0 Then
                If Type.Skill(skillNum).StunDuration > 0 Then StunPet(victim, skillnum)
                ' DoT
                If Type.Skill(skillNum).Duration > 0 Then
                    'AddDoT_Pet(Victim, Skillnum, Attacker, TargetType.Pet)
                End If
            End If
        End If

        ' Reset attack timer
        TempPlayer(attacker).PetAttackTimer = GetTimeMs()

    End Sub

    Friend Sub TryPetAttackPet(index As Integer, victim As Integer)
        Dim mapNum As Integer, blockAmount As Integer, damage As Integer

        If GetPlayerMap(index) <> GetPlayerMap(victim) Then Exit Sub

        If Not PetAlive(index) Or Not PetAlive(victim) Then Exit Sub

        ' Can the npc attack the player?
        If CanPetAttackPet(index, victim) Then
            mapNum = GetPlayerMap(index)

            ' check if Pet can avoid the attack
            If CanPetDodge(victim) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, 1, (GetPetX(victim) * 32), (GetPetY(victim) * 32))
                Exit Sub
            End If

            If CanPetParry(victim) Then
                SendActionMsg(MapNum, "Parry!", ColorType.Pink, 1, (GetPetX(victim) * 32), (GetPetY(victim) * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            damage = GetPetDamage(index)

            ' if the player blocks, take away the block amount
            damage -= blockAmount

            ' take away armour
            damage -= Random.NextDouble(1, (Type.Player(index).Pet.Stat(StatType.Luck) * 2))

            ' randomise for up to 10% lower than max hit
            damage = Random.NextDouble(1, damage)

            ' * 1.5 if crit hit
            If CanPetCrit(index) Then
                damage *= 1.5
                SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, 1, (GetPetX(index) * 32), (GetPetY(index) * 32))
            End If

            If damage > 0 Then
                PetAttackPet(index, victim, damage)
            End If

        End If

    End Sub

#End Region

#Region "Skills"

    Friend Sub BufferPetSkill(index As Integer, skillSlot As Integer)
        Dim skillnum As Integer, mpCost As Integer, levelReq As Integer
        Dim mapNum As Integer, skillCastType As Integer
        Dim accessReq As Integer, range As Integer, hasBuffered As Boolean
        Dim targetType As Byte, target As Integer

        ' Prevent subscript out of range

        If skillSlot < 0 Or skillSlot > 4 Then Exit Sub

        skillnum = Type.Player(index).Pet.Skill(skillSlot)
        mapNum = GetPlayerMap(index)

        If skillnum <= 0 Or skillnum > MAX_SKILLS Then Exit Sub

        ' see if cooldown has finished
        If TempPlayer(index).PetSkillCd(skillSlot) > GetTimeMs() Then
            PlayerMsg(index, GetPetName(index) & "'s Skill hasn't cooled down yet!", ColorType.BrightRed)
            Exit Sub
        End If

        mpCost = Type.Skill(skillNum).MpCost

        ' Check if they have enough MP
        If GetPetVital(index, VitalType.SP) < mpCost Then
            PlayerMsg(index, "Your " & GetPetName(index) & " does not have enough mana!", ColorType.BrightRed)
            Exit Sub
        End If

        levelReq = Type.Skill(skillNum).LevelReq

        ' Make sure they are the right level
        If levelReq > GetPetLevel(index) Then
            PlayerMsg(index, GetPetName(index) & " must be level " & levelReq & " to cast this skill.", ColorType.BrightRed)
            Exit Sub
        End If

        accessReq = Type.Skill(skillNum).AccessReq

        ' make sure they have the right access
        If accessReq > GetPlayerAccess(index) Then
            PlayerMsg(index, "You must be an administrator to cast this Skill, even as a pet owner.", ColorType.BrightRed)
            Exit Sub
        End If

        ' find out what kind of Skill it is! self cast, target or AOE
        If Type.Skill(skillNum).Range > 0 Then

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

        targetType = TempPlayer(index).PetTargetType
        target = TempPlayer(index).PetTarget
        range = Type.Skill(skillNum).Range
        hasBuffered = 0

        Select Case skillCastType

            'PET
            Case 0, 1, SkillType.Pet ' self-cast & self-cast AOE
                hasBuffered = 1

            Case 2, 3 ' targeted & targeted AOE

                ' check if have target
                If Not target > 0 Then
                    If skillCastType = SkillType.HealHp Or skillCastType = SkillType.HealMp Then
                        target = index
                        targetType = Core.TargetType.Pet
                    Else
                        PlayerMsg(index, "Your " & GetPetName(index) & " does not have a target.", ColorType.Yellow)
                    End If
                End If

                If targetType = Core.TargetType.Player Then

                    ' if have target, check in range
                    If Not IsInRange(range, GetPetX(index), GetPetY(index), GetPlayerX(target), GetPlayerY(target)) Then
                        PlayerMsg(index, "Target not in range of " & GetPetName(index) & ".", ColorType.Yellow)
                    Else
                        ' go through Skill Type
                        If Type.Skill(skillNum).Type <> SkillType.DamageHp And Type.Skill(skillNum).Type <> SkillType.DamageMp Then
                            hasBuffered = 1
                        Else
                            If CanPetAttackPlayer(index, target, True) Then
                                hasBuffered = 1
                            End If
                        End If
                    End If

                ElseIf targetType = Core.TargetType.NPC Then

                    ' if have target, check in range
                    If Not IsInRange(range, GetPetX(index), GetPetY(index), MapNPC(MapNum).NPC(target).X, MapNPC(MapNum).NPC(target).Y) Then
                        PlayerMsg(index, "Target not in range of " & GetPetName(index) & ".", ColorType.Yellow)
                        hasBuffered = 0
                    Else
                        ' go through Skill Type
                        If Type.Skill(skillNum).Type <> SkillType.DamageHp And Type.Skill(skillNum).Type <> SkillType.DamageMp Then
                            hasBuffered = 1
                        Else
                            If CanPetAttackNpc(index, target, True) Then
                                hasBuffered = 1
                            End If
                        End If
                    End If

                    'PET
                ElseIf targetType = Core.TargetType.Pet Then

                    ' if have target, check in range
                    If Not IsInRange(range, GetPetX(index), GetPetY(index), GetPetX(target), GetPetY(target)) Then
                        PlayerMsg(index, "Target not in range of " & GetPetName(index) & ".", ColorType.Yellow)
                        hasBuffered = 0
                    Else
                        ' go through Skill Type
                        If Type.Skill(skillNum).Type <> SkillType.DamageHp And Type.Skill(skillNum).Type <> SkillType.DamageMp Then
                            hasBuffered = 1
                        Else
                            If CanPetAttackPet(index, target, skillnum) Then
                                hasBuffered = 1
                            End If
                        End If
                    End If
                End If
        End Select

        If hasBuffered Then
            SendAnimation(MapNum, Type.Skill(skillNum).CastAnim, 0, 0, Core.TargetType.Pet, index)
            SendActionMsg(MapNum, "Casting " & Type.Skill(skillNum).Name & "!", ColorType.BrightRed, ActionMsgType.Scroll, GetPetX(index) * 32, GetPetY(index) * 32)
            TempPlayer(index).PetskillBuffer.Skill = skillSlot
            TempPlayer(index).PetskillBuffer.Timer = GetTimeMs()
            TempPlayer(index).PetskillBuffer.Target = target
            TempPlayer(index).PetskillBuffer.TargetType = targetType
            Exit Sub
        Else
            SendClearPetSkillBuffer(index)
        End If

    End Sub

    Friend Sub PetCastSkill(index As Integer, skillslot As Integer, target As Integer, targetType As Byte, Optional takeMana As Boolean = 1)
        Dim skillnum As Integer, mpCost As Integer, levelReq As Integer
        Dim mapNum As Integer, vital As Integer, didCast As Boolean
        Dim accessReq As Integer, i As Integer
        Dim aoE As Integer, range As Integer, vitalType As Byte
        Dim increment As Boolean, x As Integer, y As Integer
        Dim skillCastType As Integer

        didCast = 0

        ' Prevent subscript out of range
        If skillslot < 0 Or skillslot > 4 Then Exit Sub

        skillnum = Type.Player(index).Pet.Skill(skillslot)
        mapNum = GetPlayerMap(index)

        mpCost = Type.Skill(skillNum).MpCost

        ' Check if they have enough MP
        If Type.Player(index).Pet.Mana < mpCost Then
            PlayerMsg(index, "Your " & GetPetName(index) & " does not have enough mana!", ColorType.BrightRed)
            Exit Sub
        End If

        levelReq = Type.Skill(skillNum).LevelReq

        ' Make sure they are the right level
        If levelReq > Type.Player(index).Pet.Level Then
            PlayerMsg(index, GetPetName(index) & " must be level " & levelReq & " to cast this Skill.", ColorType.BrightRed)
            Exit Sub
        End If

        accessReq = Type.Skill(skillNum).AccessReq

        ' make sure they have the right access
        If accessReq > GetPlayerAccess(index) Then
            PlayerMsg(index, "You must be an administrator for even your pet to cast this Skill.", ColorType.BrightRed)
            Exit Sub
        End If

        ' find out what kind of Skill it is! self cast, target or AOE
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

        ' set the vital
        vital = Type.Skill(skillNum).Vital
        aoE = Type.Skill(skillNum).AoE
        range = Type.Skill(skillNum).Range

        Select Case skillCastType
            Case 0 ' self-cast target
                Select Case Type.Skill(skillNum).Type
                    Case SkillType.HealHp
                        SkillPet_Effect(Core.VitalType.HP, True, index, vital, skillnum)
                        didCast = 1
                    Case SkillType.HealMp
                        SkillPet_Effect(Core.VitalType.SP, True, index, vital, skillnum)
                        didCast = 1
                End Select

            Case 1, 3 ' self-cast AOE & targetted AOE

                If skillCastType = 1 Then
                    x = GetPetX(index)
                    y = GetPetY(index)
                ElseIf skillCastType = 3 Then

                    If targetType = 0 Then Exit Sub
                    If target = 0 Then Exit Sub

                    If targetType = Core.TargetType.Player Then
                        x = GetPlayerX(target)
                        y = GetPlayerY(target)
                    ElseIf targetType = Core.TargetType.NPC Then
                        x = MapNPC(MapNum).NPC(target).X
                        y = MapNPC(MapNum).NPC(target).Y
                    ElseIf targetType = Core.TargetType.Pet Then
                        x = GetPetX(target)
                        y = GetPetY(target)
                    End If

                    If Not IsInRange(range, GetPetX(index), GetPetY(index), x, y) Then
                        PlayerMsg(index, GetPetName(index) & "'s target not in range.", ColorType.Yellow)
                        SendClearPetSkillBuffer(index)
                    End If
                End If

                Select Case Type.Skill(skillNum).Type

                    Case SkillType.DamageHp
                        didCast = 1

                        For i = 1 To Socket.HighIndex()
                            If IsPlaying(i) And i <> index Then
                                If GetPlayerMap(i) = GetPlayerMap(index) Then
                                    If IsInRange(aoE, x, y, GetPlayerX(i), GetPlayerY(i)) Then
                                        If CanPetAttackPlayer(index, i, True) And index <> target Then
                                            SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.Player, i)
                                            PetAttackPlayer(index, i, vital, skillnum)
                                        End If
                                    End If

                                    If PetAlive(i) Then
                                        If IsInRange(aoE, x, y, GetPetX(i), GetPetY(i)) Then

                                            If CanPetAttackPet(index, i, skillnum) Then
                                                SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.Pet, i)
                                                PetAttackPet(index, i, vital, skillnum)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next

                        For i = 1 To MAX_MAP_NPCS
                           If MapNPC(MapNum).NPC(i).Num > 0 And MapNPC(MapNum).NPC(i).Vital(Core.VitalType.HP) > 0 Then
                                If IsInRange(aoE, x, y, MapNPC(MapNum).NPC(i).X, MapNPC(MapNum).NPC(i).Y) Then
                                    If CanPetAttackNpc(index, i, True) Then
                                        SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.NPC, i)
                                        PetAttackNpc(index, i, vital, skillnum)
                                    End If
                                End If
                            End If
                        Next

                    Case SkillType.HealHp, SkillType.HealMp, SkillType.DamageMp

                        If Type.Skill(skillNum).Type = SkillType.HealHp Then
                            vitalType = Core.VitalType.HP
                            increment = 1
                        ElseIf Type.Skill(skillNum).Type = SkillType.HealMp Then
                            vitalType = Core.VitalType.SP
                            increment = 1
                        ElseIf Type.Skill(skillNum).Type = SkillType.DamageMp Then
                            vitalType = Core.VitalType.SP
                            increment = 0
                        End If

                        didCast = 1

                        For i = 1 To Socket.HighIndex()
                            If IsPlaying(i) And GetPlayerMap(i) = GetPlayerMap(index) Then
                                If IsInRange(aoE, x, y, GetPlayerX(i), GetPlayerY(i)) Then
                                    SkillPlayer_Effect(vitalType, increment, i, vital, skillnum)
                                End If

                                If PetAlive(i) Then
                                    If IsInRange(aoE, x, y, GetPetX(i), GetPetY(i)) Then
                                        SkillPet_Effect(vitalType, increment, i, vital, skillnum)
                                    End If
                                End If
                            End If
                        Next
                End Select

            Case 2 ' targetted

                If targetType = 0 Then Exit Sub
                If target = 0 Then Exit Sub

                If targetType = Core.TargetType.Player Then
                    x = GetPlayerX(target)
                    y = GetPlayerY(target)
                ElseIf targetType = Core.TargetType.NPC Then
                    x = MapNPC(MapNum).NPC(target).X
                    y = MapNPC(MapNum).NPC(target).Y
                ElseIf targetType = Core.TargetType.Pet Then
                    x = GetPetX(target)
                    y = GetPetY(target)
                End If

                If Not IsInRange(range, GetPetX(index), GetPetY(index), x, y) Then
                    PlayerMsg(index, "Target is not in range of your " & GetPetName(index) & "!", ColorType.Yellow)
                    SendClearPetSkillBuffer(index)
                    Exit Sub
                End If

                Select Case Type.Skill(skillNum).Type

                    Case SkillType.DamageHp

                        If targetType = Core.TargetType.Player Then
                            If CanPetAttackPlayer(index, target, True) And index <> target Then
                                If vital > 0 Then
                                    SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.Player, target)
                                    PetAttackPlayer(index, target, vital, skillnum)
                                    didCast = 1
                                End If
                            End If
                        ElseIf targetType = Core.TargetType.NPC Then
                            If CanPetAttackNpc(index, target, True) Then
                                If vital > 0 Then
                                    SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.NPC, target)
                                    PetAttackNpc(index, target, vital, skillnum)
                                    didCast = 1
                                End If
                            End If
                        ElseIf targetType = Core.TargetType.Pet Then
                            If CanPetAttackPet(index, target, skillnum) Then
                                If vital > 0 Then
                                    SendAnimation(MapNum, Type.Skill(skillNum).SkillAnim, 0, 0, Core.TargetType.Pet, target)
                                    PetAttackPet(index, target, vital, skillnum)
                                    didCast = 1
                                End If
                            End If
                        End If

                    Case SkillType.DamageMp, SkillType.HealMp, SkillType.HealHp

                        If Type.Skill(skillNum).Type = SkillType.DamageMp Then
                            vitalType = Core.VitalType.SP
                            increment = 0
                        ElseIf Type.Skill(skillNum).Type = SkillType.HealMp Then
                            vitalType = Core.VitalType.SP
                            increment = 1
                        ElseIf Type.Skill(skillNum).Type = SkillType.HealHp Then
                            vitalType = [Enum].VitalType.HP
                            increment = 1
                        End If

                        If targetType = Core.TargetType.Player Then
                            If Type.Skill(skillNum).Type = SkillType.DamageMp Then
                                If CanPetAttackPlayer(index, target, True) Then
                                    SkillPlayer_Effect(vitalType, increment, target, vital, skillnum)
                                End If
                            Else
                                SkillPlayer_Effect(vitalType, increment, target, vital, skillnum)
                            End If

                        ElseIf targetType = Core.TargetType.NPC Then

                            If Type.Skill(skillNum).Type = SkillType.DamageMp Then
                                If CanPetAttackNpc(index, target, True) Then
                                    SkillNpc_Effect(vitalType, increment, target, vital, skillnum, mapNum)
                                End If
                            Else
                                If Type.Skill(skillNum).Type = SkillType.HealHp Or Type.Skill(skillNum).Type = SkillType.HealMp Then
                                    SkillPet_Effect(vitalType, increment, index, vital, skillnum)
                                Else
                                    SkillNpc_Effect(vitalType, increment, target, vital, skillnum, mapNum)
                                End If
                            End If

                        ElseIf targetType = Core.TargetType.Pet Then

                            If Type.Skill(skillNum).Type = SkillType.DamageMp Then
                                If CanPetAttackPet(index, target, skillnum) Then
                                    SkillPet_Effect(vitalType, increment, target, vital, skillnum)
                                End If
                            Else
                                SkillPet_Effect(vitalType, increment, target, vital, skillnum)
                                SendPetVital(target, vital)
                            End If
                        End If
                End Select

            Case 4 ' Projectile
                PetFireProjectile(index, skillnum)
                didCast = 1
        End Select

        If didCast Then
            If takeMana Then SetPetVital(index, Core.VitalType.SP, GetPetVital(index, Core.VitalType.SP) - mpCost)
            SendPetVital(index, Core.VitalType.SP)
            SendPetVital(index, Core.VitalType.HP)

            TempPlayer(index).PetSkillCd(skillslot) = GetTimeMs() + (Type.Skill(skillNum).CdTime * 1000)

            SendActionMsg(MapNum, Type.Skill(skillNum).Name & "!", ColorType.BrightRed, ActionMsgType.Scroll, GetPetX(index) * 32, GetPetY(index) * 32)
        End If

    End Sub

    Friend Sub SkillPet_Effect(vital As Byte, increment As Boolean, index As Integer, damage As Integer, skillnum As Integer)
        Dim sSymbol As String
        Dim Color As Integer

        If damage > 0 Then
            If increment Then
                sSymbol = "+"
                If vital = VitalType.HP Then Color = ColorType.BrightGreen
                If vital = VitalType.SP Then Color = ColorType.BrightBlue
            Else
                sSymbol = "-"
                Color = ColorType.Blue
            End If

            SendAnimation(GetPlayerMap(index), Type.Skill(skillNum).SkillAnim, 0, 0, TargetType.Pet, index)
            SendActionMsg(GetPlayerMap(index), sSymbol & damage, Color, ActionMsgType.Scroll, GetPetX(index) * 32, GetPetY(index) * 32)

            ' send the sound
            'SendMapSound(Index, Player(index).Pet.x, Player(index).Pet.y, SoundEntity.seSkill, Skillnum)

            If increment Then
                SetPetVital(index, VitalType.HP, GetPetVital(index, VitalType.HP) + damage)

                If Type.Skill(skillNum).Duration > 0 Then
                    AddHoT_Pet(index, skillnum)
                End If

            ElseIf Not increment Then
                If vital = VitalType.HP Then
                    SetPetVital(index, VitalType.HP, GetPetVital(index, VitalType.HP) - damage)
                ElseIf vital = VitalType.SP Then
                    SetPetVital(index, VitalType.SP, GetPetVital(index, VitalType.SP) - damage)
                End If
            End If
        End If

        If GetPetVital(index, VitalType.HP) > GetPetMaxVital(index, VitalType.HP) Then SetPetVital(index, VitalType.HP, GetPetMaxVital(index, VitalType.HP))

        If GetPetVital(index, VitalType.SP) > GetPetMaxVital(index, VitalType.SP) Then SetPetVital(index, VitalType.SP, GetPetMaxVital(index, VitalType.SP))

    End Sub

    Friend Sub AddHoT_Pet(index As Integer, skillnum As Integer)
        Dim i As Integer

        For i = 1 To MAX_COTS
            With TempPlayer(index).PetHoT(i)

                If .Skill = skillnum Then
                    .Timer = GetTimeMs()
                    .StartTime = GetTimeMs()
                    Exit Sub
                End If

                If .Used = 0 Then
                    .Skill = skillnum
                    .Timer = GetTimeMs()
                    .Used = 1
                    .StartTime = GetTimeMs()
                    Exit Sub
                End If
            End With
        Next

    End Sub

    Friend Sub AddDoT_Pet(index As Integer, skillnum As Integer, caster As Integer, attackerType As Integer)
        Dim i As Integer

        If Not PetAlive(index) Then Exit Sub

        For i = 1 To MAX_COTS
            With TempPlayer(index).PetDoT(i)
                If .Skill = skillnum Then
                    .Timer = GetTimeMs()
                    .Caster = caster
                    .StartTime = GetTimeMs()
                    .AttackerType = attackerType
                    Exit Sub
                End If

                If .Used = 0 Then
                    .Skill = skillnum
                    .Timer = GetTimeMs()
                    .Caster = caster
                    .Used = 1
                    .StartTime = GetTimeMs()
                    .AttackerType = attackerType
                    Exit Sub
                End If
            End With
        Next

    End Sub

    Friend Sub StunPet(index As Integer, skillnum As Integer)
        ' check if it's a stunning Skill

        If PetAlive(index) Then
            If Type.Skill(skillNum).StunDuration > 0 Then
                ' set the values on index
                TempPlayer(index).PetStunDuration = Type.Skill(skillNum).StunDuration
                TempPlayer(index).PetStunTimer = GetTimeMs()
                ' tell him he's stunned
                PlayerMsg(index, "Your " & GetPetName(index) & " has been stunned.", ColorType.Yellow)
            End If
        End If

    End Sub

    Friend Sub HandleDoT_Pet(index As Integer, dotNum As Integer)

        With TempPlayer(index).PetDoT(dotNum)

            If .Used And .Skill > 0 Then
                ' time to tick?
                If GetTimeMs() > .Timer + (Type.Skill(.Skill).Interval * 1000) Then
                    If .AttackerType = TargetType.Pet Then
                        If CanPetAttackPet(.Caster, index, .Skill) Then
                            PetAttackPet(.Caster, index, Type.Skill(.Skill).Vital)
                            SendPetVital(index, VitalType.HP)
                            SendPetVital(index, VitalType.SP)
                        End If
                    ElseIf .AttackerType = TargetType.Player Then
                        If CanPlayerAttackPet(.Caster, index, .Skill) Then
                            PlayerAttackPet(.Caster, index, Type.Skill(.Skill).Vital)
                            SendPetVital(index, VitalType.HP)
                            SendPetVital(index, VitalType.SP)
                        End If
                    End If

                    .Timer = GetTimeMs()

                    ' check if DoT is still active - if player died it'll have been purged
                    If .Used And .Skill > 0 Then
                        ' destroy DoT if finished
                        If GetTimeMs() - .StartTime >= (Type.Skill(.Skill).Duration * 1000) Then
                            .Used = 0
                            .Skill = 0
                            .Timer = 0
                            .Caster = 0
                            .StartTime = 0
                        End If
                    End If
                End If
            End If
        End With

    End Sub

    Friend Sub HandleHoT_Pet(index As Integer, hotNum As Integer)

        With TempPlayer(index).PetHoT(hotNum)

            If .Used And .Skill > 0 Then
                ' time to tick?
                If GetTimeMs() > .Timer + (Type.Skill(.Skill).Interval * 1000) Then
                    SendActionMsg(GetPlayerMap(index), "+" & Type.Skill(.Skill).Vital, ColorType.BrightGreen, ActionMsgType.Scroll, Type.Player(index).Pet.X * 32, Type.Player(index).Pet.Y * 32,)
                    SetPetVital(index, VitalType.HP, GetPetVital(index, VitalType.HP) + Type.Skill(.Skill).Vital)

                    If GetPetVital(index, VitalType.HP) > GetPetMaxVital(index, VitalType.HP) Then SetPetVital(index, VitalType.HP, GetPetMaxVital(index, VitalType.HP))

                    If GetPetVital(index, VitalType.SP) > GetPetMaxVital(index, VitalType.SP) Then SetPetVital(index, VitalType.SP, GetPetMaxVital(index, VitalType.SP))

                    SendPetVital(index, VitalType.HP)
                    SendPetVital(index, VitalType.SP)
                    .Timer = GetTimeMs()

                    ' check if DoT is still active - if player died it'll have been purged
                    If .Used And .Skill > 0 Then
                        ' destroy hoT if finished
                        If GetTimeMs() - .StartTime >= (Type.Skill(.Skill).Duration * 1000) Then
                            .Used = 0
                            .Skill = 0
                            .Timer = 0
                            .Caster = 0
                            .StartTime = 0
                        End If
                    End If
                End If
            End If
        End With

    End Sub

    Friend Function CanPetDodge(index As Integer) As Boolean
        Dim rate As Integer, rndNum As Integer

        If Not PetAlive(index) Then Exit Function

        CanPetDodge = 0

        rate = GetPetStat(index, StatType.Luck) / 4
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then
            CanPetDodge = 1
        End If

    End Function

    Friend Function CanPetParry(index As Integer) As Boolean
        Dim rate As Integer, rndNum As Integer

        If Not PetAlive(index) Then Exit Function

        CanPetParry = 0

        rate = GetPetStat(index, StatType.Luck) / 6
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then
            CanPetParry = 1
        End If

    End Function

#End Region

#Region "Player > Pet"

    Function CanPlayerAttackPet(attacker As Integer, victim As Integer, Optional isSkill As Boolean = 0) As Boolean

        If isSkill = 0 Then
            ' Check attack timer
            If GetPlayerEquipment(attacker, EquipmentType.Weapon) > 0 Then
                If GetTimeMs() < TempPlayer(attacker).AttackTimer + Type.Item(GetPlayerEquipment(attacker, EquipmentType.Weapon)).Speed Then Exit Function
            Else
                If GetTimeMs() < TempPlayer(attacker).AttackTimer + 1000 Then Exit Function
            End If
        End If

        ' Check for subscript out of range
        If Not IsPlaying(victim) Then Exit Function

        If Not PetAlive(victim) Then Exit Function

        ' Make sure they are on the same map
        If Not GetPlayerMap(attacker) = GetPlayerMap(victim) Then Exit Function

        ' Make sure we dont attack the player if they are switching maps
        If TempPlayer(victim).GettingMap = 1 Then Exit Function

        If isSkill = 0 Then

            ' Check if at same coordinates
            Select Case GetPlayerDir(attacker)

                Case DirectionType.Up
                    If Not ((GetPetY(victim) + 1 = GetPlayerY(attacker)) And (GetPetX(victim) = GetPlayerX(attacker))) Then Exit Function

                Case DirectionType.Down
                    If Not ((GetPetY(victim) - 1 = GetPlayerY(attacker)) And (GetPetX(victim) = GetPlayerX(attacker))) Then Exit Function

                Case DirectionType.Left
                    If Not ((GetPetY(victim) = GetPlayerY(attacker)) And (GetPetX(victim) + 1 = GetPlayerX(attacker))) Then Exit Function

                Case DirectionType.Right
                    If Not ((GetPetY(victim) = GetPlayerY(attacker)) And (GetPetX(victim) - 1 = GetPlayerX(attacker))) Then Exit Function

                Case Else
                    Exit Function
            End Select
        End If

        ' CheckIf Type.Map is attackable
       If Type.Map(GetPlayerMap(attacker)).Moral > 0 Then
            If Not Type.Moral(Type.Map(GetPlayerMap(attacker)).Moral).CanPK Then
                If GetPlayerPK(victim) = 0 Then
                    PlayerMsg(attacker, "This is a safe zone!", ColorType.Yellow)
                    Exit Function
                End If
            End If
        End If

        ' Make sure they have more then 0 hp
        If GetPetVital(victim, VitalType.HP) <= 0 Then Exit Function

        ' Check to make sure that they dont have access
        If GetPlayerAccess(attacker) > AccessType.Moderator Then
            PlayerMsg(attacker, "Admins cannot attack other players.", ColorType.BrightRed)
            Exit Function
        End If

        ' Check to make sure the victim isn't an admin
        If GetPlayerAccess(victim) > AccessType.Moderator Then
            PlayerMsg(attacker, "You cannot attack " & GetPlayerName(victim) & "s " & GetPetName(victim) & "!", ColorType.BrightRed)
            Exit Function
        End If

        ' Don't attack a party member
        If TempPlayer(attacker).InParty > 0 And TempPlayer(victim).InParty > 0 Then
            If TempPlayer(attacker).InParty = TempPlayer(victim).InParty Then
                PlayerMsg(attacker, "You can't attack another party member!", ColorType.BrightRed)
                Exit Function
            End If
        End If

        If TempPlayer(attacker).InParty > 0 And TempPlayer(victim).InParty > 0 And TempPlayer(attacker).InParty = TempPlayer(victim).InParty Then
            If isSkill > 0 Then
                If Type.Skill(isSkill).Type = SkillType.HealMp Or Type.Skill(isSkill).Type = SkillType.HealHp Then
                    'Carry On :D
                Else
                    Exit Function
                End If
            Else
                Exit Function
            End If
        End If

        CanPlayerAttackPet = 1

    End Function

    Sub PlayerAttackPet(attacker As Integer, victim As Integer, damage As Integer, Optional skillnum As Integer = 0)
        Dim exp As Integer, n As Integer, i As Integer

        ' Check for subscript out of range

        If IsPlaying(attacker) = 0 Or IsPlaying(victim) = 0 Or damage < 0 Or Not PetAlive(victim) Then Exit Sub

        If GetPlayerEquipment(attacker, EquipmentType.Weapon) > 0 Then
            n = GetPlayerEquipment(attacker, EquipmentType.Weapon)
        End If

        ' set the regen timer
        TempPlayer(attacker).StopRegen = 1
        TempPlayer(attacker).StopRegenTimer = GetTimeMs()

        If damage >= GetPetVital(victim, VitalType.HP) Then
            SendActionMsg(GetPlayerMap(victim), "-" & GetPetVital(victim, VitalType.HP), ColorType.BrightRed, 1, (GetPetX(victim) * 32), (GetPetY(victim) * 32))

            ' send the sound
            'If Skillnum > 0 Then SendMapSound Victim, Player(Victim).characters(TempPlayer(Victim).CurChar).Pet.x, Player(Victim).characters(TempPlayer(Victim).CurChar).Pet.y, SoundEntity.seSkill, Skillnum

            ' Calculate exp to give attacker
            exp = (GetPlayerExp(victim) \ 10)

            ' Make sure we dont get less then 0
            If exp < 0 Then exp = 0

            If exp = 0 Then
                PlayerMsg(victim, "You lost no exp.", ColorType.BrightGreen)
                PlayerMsg(attacker, "You received no exp.", ColorType.Yellow)
            Else
                SetPlayerExp(victim, GetPlayerExp(victim) - exp)
                SendExp(victim)
                PlayerMsg(victim, "You lost " & exp & " exp.", ColorType.BrightRed)

                ' check if we're in a party
                If TempPlayer(attacker).InParty > 0 Then
                    ' pass through party exp share function
                    Party_ShareExp(TempPlayer(attacker).InParty, exp, attacker, GetPlayerMap(attacker))
                Else
                    ' not in party, get exp for self
                    GivePlayerExp(attacker, exp)
                End If
            End If

            ' purge target info of anyone who targetted dead guy
            For i = 1 To Socket.HighIndex()
                If IsPlaying(i) And Socket.IsConnected(i) And GetPlayerMap(i) = GetPlayerMap(attacker) Then
                    If TempPlayer(i).Target = TargetType.Pet And TempPlayer(i).Target = victim Then
                        TempPlayer(i).Target = 0
                        TempPlayer(i).TargetType = 0
                        SendTarget(i, 0, 0)
                    End If
                End If
            Next

            PlayerMsg(victim, "Your " & GetPetName(victim) & " was killed by " & GetPlayerName(attacker) & ".", ColorType.BrightRed)
            RecallPet(victim)
        Else
            ' Pet not dead, just do the damage
            SetPetVital(victim, VitalType.HP, GetPetVital(victim, VitalType.HP) - damage)
            SendPetVital(victim, VitalType.HP)

            'Set pet to begin attacking the other pet if it isn't dead or dosent have another target
            If TempPlayer(victim).PetTarget < 0 And TempPlayer(victim).PetBehavior <> PetBehaviourGoto Then
                TempPlayer(victim).PetTarget = attacker
                TempPlayer(victim).PetTargetType = TargetType.Player
            End If

            ' send the sound
            'If Skillnum > 0 Then SendMapSound Victim, GetPetX(Victim), GetPety(Victim), SoundEntity.seSkill, Skillnum

            SendActionMsg(GetPlayerMap(victim), "-" & damage, ColorType.BrightRed, 1, (GetPetX(victim) * 32), (GetPetY(victim) * 32))
            SendBlood(GetPlayerMap(victim), GetPetX(victim), GetPetY(victim))

            ' set the regen timer
            TempPlayer(victim).PetstopRegen = 1
            TempPlayer(victim).PetstopRegenTimer = GetTimeMs()

            'if a stunning Skill, stun the player
            If skillnum > 0 Then
                If Type.Skill(skillNum).StunDuration > 0 Then StunPet(victim, skillnum)

                ' DoT
                If Type.Skill(skillNum).Duration > 0 Then
                    AddDoT_Pet(victim, skillnum, attacker, TargetType.Player)
                End If
            End If
        End If

        ' Reset attack timer
        TempPlayer(attacker).AttackTimer = GetTimeMs()

    End Sub

    Friend Sub TryPlayerAttackPet(attacker As Integer, victim As Integer)
        Dim blockAmount As Integer, mapNum As Integer
        If Not PetAlive(victim) Then Exit Sub

        ' Can we attack the npc?
        If CanPlayerAttackPet(attacker, victim) Then

            mapNum = GetPlayerMap(attacker)

            TempPlayer(attacker).Target = victim
            TempPlayer(attacker).TargetType = TargetType.Pet

            ' check if NPC can avoid the attack
            If CanPetDodge(victim) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))
                Exit Sub
            End If

            If CanPetParry(victim) Then
                SendActionMsg(MapNum, "Parry!", ColorType.Pink, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            Dim damage As Integer = GetPlayerDamage(attacker)

            ' if the npc blocks, take away the block amount
            blockAmount = 0
            damage -= blockAmount

            ' take away armour
            damage -= Random.NextDouble(1, (GetPlayerStat(victim, StatType.Luck) * 2))

            ' randomise for up to 10% lower than max hit
            damage = Random.NextDouble(1, damage)

            ' * 1.5 if can crit
            If CanPlayerCriticalHit(attacker) Then
                damage *= 1.5
                SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, 1, (GetPlayerX(attacker) * 32), (GetPlayerY(attacker) * 32))
            End If

            If damage > 0 Then
                PlayerAttackPet(attacker, victim, damage)
            Else
                PlayerMsg(attacker, "Your attack does nothing.", ColorType.BrightRed)
            End If
        End If

    End Sub

#End Region

#Region "Data Functions"

    Friend Function PetAlive(index As Integer) As Boolean
        PetAlive = 0

        If Type.Player(index).Pet.Alive = 1 Then
            PetAlive = 1
        End If

    End Function

    Friend Function GetPetName(index As Integer) As String
        GetPetName = ""

        If PetAlive(index) Then
            GetPetName = Pet(Type.Player(index).Pet.Num).Name
        End If

    End Function

    Friend Function GetPetNum(index As Integer) As Integer
        GetPetNum = Type.Player(index).Pet.Num

    End Function

    Friend Function GetPetRange(index As Integer) As Integer
        GetPetRange = 0

        If PetAlive(index) Then
            GetPetRange = Pet(Type.Player(index).Pet.Num).Range
        End If

    End Function

    Friend Function GetPetLevel(index As Integer) As Integer
        GetPetLevel = 0

        If PetAlive(index) Then
            GetPetLevel = Type.Player(index).Pet.Level
        End If

    End Function

    Friend Sub SetPetLevel(index As Integer, newlvl As Integer)
        If PetAlive(index) Then
            Type.Player(index).Pet.Level = newlvl
        End If
    End Sub

    Friend Function GetPetX(index As Integer) As Integer
        GetPetX = 0

        If PetAlive(index) Then
            GetPetX = Type.Player(index).Pet.X
        End If

    End Function

    Friend Sub SetPetX(index As Integer, x As Integer)
        If PetAlive(index) Then
            Type.Player(index).Pet.X = x
        End If
    End Sub

    Friend Function GetPetY(index As Integer) As Integer
        GetPetY = 0

        If PetAlive(index) Then
            GetPetY = Type.Player(index).Pet.Y
        End If

    End Function

    Friend Sub SetPetY(index As Integer, y As Integer)
        If PetAlive(index) Then
            Type.Player(index).Pet.Y = y
        End If
    End Sub

    Friend Function GetPetDir(index As Integer) As Integer
        GetPetDir = 0

        If PetAlive(index) Then
            GetPetDir = Type.Player(index).Pet.Dir
        End If

    End Function

    Friend Function GetPetBehaviour(index As Integer) As Integer
        GetPetBehaviour = 0

        If PetAlive(index) Then
            GetPetBehaviour = Type.Player(index).Pet.AttackBehaviour
        End If

    End Function

    Friend Sub SetPetBehaviour(index As Integer, behaviour As Byte)
        If PetAlive(index) Then
            Type.Player(index).Pet.AttackBehaviour = behaviour
        End If
    End Sub

    Friend Function GetPetStat(index As Integer, stat As StatType) As Integer
        GetPetStat = 0

        If PetAlive(index) Then
            GetPetStat = Type.Player(index).Pet.Stat(stat)
        End If

    End Function

    Friend Sub SetPetStat(index As Integer, stat As StatType, amount As Integer)

        If PetAlive(index) Then
            Type.Player(index).Pet.Stat(stat) = amount
        End If

    End Sub

    Friend Function GetPetPoints(index As Integer) As Integer
        GetPetPoints = 0

        If PetAlive(index) Then
            GetPetPoints = Type.Player(index).Pet.Points
        End If

    End Function

    Friend Sub SetPetPoints(index As Integer, amount As Integer)

        If PetAlive(index) Then
            Type.Player(index).Pet.Points = amount
        End If

    End Sub

    Friend Function GetPetExp(index As Integer) As Integer
        GetPetExp = 0

        If PetAlive(index) Then
            GetPetExp = Type.Player(index).Pet.Exp
        End If

    End Function

    Friend Sub SetPetExp(index As Integer, amount As Integer)
        If PetAlive(index) Then
            Type.Player(index).Pet.Exp = amount
        End If
    End Sub

    Function GetPetVital(index As Integer, vital As VitalType) As Integer

        If index > MAX_PLAYERS Then Exit Function

        Select Case vital
            Case VitalType.HP
                GetPetVital = Type.Player(index).Pet.Health

            Case VitalType.SP
                GetPetVital = Type.Player(index).Pet.Mana
        End Select

    End Function

    Sub SetPetVital(index As Integer, vital As VitalType, amount As Integer)

        If index > MAX_PLAYERS Then Exit Sub

        Select Case vital
            Case VitalType.HP
                Type.Player(index).Pet.Health = amount

            Case VitalType.SP
                Type.Player(index).Pet.Mana = amount
        End Select

    End Sub

    Function GetPetMaxVital(index As Integer, vital As VitalType) As Integer
        Select Case vital
            Case VitalType.HP
                GetPetMaxVital = ((Type.Player(index).Pet.Level * 4) + (Type.Player(index).Pet.Stat(StatType.Luck) * 10)) + 150

            Case VitalType.SP
                GetPetMaxVital = ((Type.Player(index).Pet.Level * 4) + (Type.Player(index).Pet.Stat(StatType.Spirit) / 2)) * 5 + 50
        End Select

    End Function

    Function GetPetNextLevel(index As Integer) As Integer

        If PetAlive(index) Then
            If Type.Player(index).Pet.Level = Pet(Type.Player(index).Pet.Num).MaxLevel Then GetPetNextLevel = 0 : Exit Function
            GetPetNextLevel = (50 / 3) * ((Type.Player(index).Pet.Level + 1) ^ 3 - (6 * (Type.Player(index).Pet.Level + 1) ^ 2) + 17 * (Type.Player(index).Pet.Level + 1) - 12)
        End If

    End Function

#End Region

End Module