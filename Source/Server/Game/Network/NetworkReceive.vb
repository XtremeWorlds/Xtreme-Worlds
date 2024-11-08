Imports System.IO
Imports System.Reflection
Imports Core
Imports Mirage.Sharp.Asfw
Imports Newtonsoft.Json.Linq

Module NetworkReceive
    Friend Sub PacketRouter()
        Socket.PacketId(ClientPackets.CCheckPing) = AddressOf Packet_Ping
        Socket.PacketId(ClientPackets.CLogin) = AddressOf Packet_Login
        Socket.PacketId(ClientPackets.CRegister) = AddressOf Packet_Register
        Socket.PacketId(ClientPackets.CAddChar) = AddressOf Packet_AddChar
        Socket.PacketId(ClientPackets.CUseChar) = AddressOf Packet_UseChar
        Socket.PacketId(ClientPackets.CDelChar) = AddressOf Packet_DelChar
        Socket.PacketId(ClientPackets.CSayMsg) = AddressOf Packet_SayMessage
        Socket.PacketId(ClientPackets.CBroadcastMsg) = AddressOf Packet_BroadCastMsg
        Socket.PacketId(ClientPackets.CPlayerMsg) = AddressOf Packet_PlayerMsg
        Socket.PacketId(ClientPackets.CAdminMsg) = AddressOf Packet_AdminMsg
        Socket.PacketId(ClientPackets.CPlayerMove) = AddressOf Packet_PlayerMove
        Socket.PacketId(ClientPackets.CPlayerDir) = AddressOf Packet_PlayerDirection
        Socket.PacketId(ClientPackets.CUseItem) = AddressOf Packet_UseItem
        Socket.PacketId(ClientPackets.CAttack) = AddressOf Packet_Attack
        Socket.PacketId(ClientPackets.CPlayerInfoRequest) = AddressOf Packet_PlayerInfo
        Socket.PacketId(ClientPackets.CWarpMeTo) = AddressOf Packet_WarpMeTo
        Socket.PacketId(ClientPackets.CWarpToMe) = AddressOf Packet_WarpToMe
        Socket.PacketId(ClientPackets.CWarpTo) = AddressOf Packet_WarpTo
        Socket.PacketId(ClientPackets.CSetSprite) = AddressOf Packet_SetSprite
        Socket.PacketId(ClientPackets.CGetStats) = AddressOf Packet_GetStats
        Socket.PacketId(ClientPackets.CRequestNewMap) = AddressOf Packet_RequestNewMap
        Socket.PacketId(ClientPackets.CSaveMap) = AddressOf Packet_MapData
        Socket.PacketId(ClientPackets.CNeedMap) = AddressOf Packet_NeedMap
        Socket.PacketId(ClientPackets.CMapGetItem) = AddressOf Packet_GetItem
        Socket.PacketId(ClientPackets.CMapDropItem) = AddressOf Packet_DropItem
        Socket.PacketId(ClientPackets.CMapRespawn) = AddressOf Packet_RespawnMap
        Socket.PacketId(ClientPackets.CMapReport) = AddressOf Packet_MapReport
        Socket.PacketId(ClientPackets.CKickPlayer) = AddressOf Packet_KickPlayer
        Socket.PacketId(ClientPackets.CBanList) = AddressOf Packet_Banlist
        Socket.PacketId(ClientPackets.CBanDestroy) = AddressOf Packet_DestroyBans
        Socket.PacketId(ClientPackets.CBanPlayer) = AddressOf Packet_BanPlayer

        Socket.PacketId(ClientPackets.CRequestEditMap) = AddressOf Packet_EditMapRequest

        Socket.PacketId(ClientPackets.CSetAccess) = AddressOf Packet_SetAccess
        Socket.PacketId(ClientPackets.CWhosOnline) = AddressOf Packet_WhosOnline
        Socket.PacketId(ClientPackets.CSetMotd) = AddressOf Packet_SetMotd
        Socket.PacketId(ClientPackets.CSearch) = AddressOf Packet_PlayerSearch
        Socket.PacketId(ClientPackets.CSkills) = AddressOf Packet_Skills
        Socket.PacketId(ClientPackets.CCast) = AddressOf Packet_Cast
        Socket.PacketId(ClientPackets.CQuit) = AddressOf Packet_QuitGame
        Socket.PacketId(ClientPackets.CSwapInvSlots) = AddressOf Packet_SwapInvSlots
        Socket.PacketId(ClientPackets.CSwapSkillSlots) = AddressOf Packet_SwapSkillSlots

        Socket.PacketId(ClientPackets.CCheckPing) = AddressOf Packet_CheckPing
        Socket.PacketId(ClientPackets.CUnequip) = AddressOf Packet_Unequip
        Socket.PacketId(ClientPackets.CRequestPlayerData) = AddressOf Packet_RequestPlayerData
        Socket.PacketId(ClientPackets.CRequestItem) = AddressOf Packet_RequestItem
        Socket.PacketId(ClientPackets.CRequestNPC) = AddressOf Packet_RequestNPC
        Socket.PacketId(ClientPackets.CRequestResource) = AddressOf Packet_RequestResource
        Socket.PacketId(ClientPackets.CSpawnItem) = AddressOf Packet_SpawnItem
        Socket.PacketId(ClientPackets.CTrainStat) = AddressOf Packet_TrainStat

        Socket.PacketId(ClientPackets.CRequestAnimation) = AddressOf Packet_RequestAnimation
        Socket.PacketId(ClientPackets.CRequestSkill) = AddressOf Packet_RequestSkill
        Socket.PacketId(ClientPackets.CRequestShop) = AddressOf Packet_RequestShop
        Socket.PacketId(ClientPackets.CRequestLevelUp) = AddressOf Packet_RequestLevelUp
        Socket.PacketId(ClientPackets.CForgetSkill) = AddressOf Packet_ForgetSkill
        Socket.PacketId(ClientPackets.CCloseShop) = AddressOf Packet_CloseShop
        Socket.PacketId(ClientPackets.CBuyItem) = AddressOf Packet_BuyItem
        Socket.PacketId(ClientPackets.CSellItem) = AddressOf Packet_SellItem
        Socket.PacketId(ClientPackets.CChangeBankSlots) = AddressOf Packet_ChangeBankSlots
        Socket.PacketId(ClientPackets.CDepositItem) = AddressOf Packet_DepositItem
        Socket.PacketId(ClientPackets.CWithdrawItem) = AddressOf Packet_WithdrawItem
        Socket.PacketId(ClientPackets.CCloseBank) = AddressOf Packet_CloseBank
        Socket.PacketId(ClientPackets.CAdminWarp) = AddressOf Packet_AdminWarp

        Socket.PacketId(ClientPackets.CTradeInvite) = AddressOf Packet_TradeInvite
        Socket.PacketId(ClientPackets.CHandleTradeInvite) = AddressOf Packet_HandleTradeInvite
        Socket.PacketId(ClientPackets.CAcceptTrade) = AddressOf Packet_AcceptTrade
        Socket.PacketId(ClientPackets.CDeclineTrade) = AddressOf Packet_DeclineTrade
        Socket.PacketId(ClientPackets.CTradeItem) = AddressOf Packet_TradeItem
        Socket.PacketId(ClientPackets.CUntradeItem) = AddressOf Packet_UntradeItem

        Socket.PacketId(ClientPackets.CAdmin) = AddressOf Packet_Admin

        Socket.PacketId(ClientPackets.CSetHotbarSlot) = AddressOf Packet_SetHotbarSlot
        Socket.PacketId(ClientPackets.CDeleteHotbarSlot) = AddressOf Packet_DeleteHotbarSlot
        Socket.PacketId(ClientPackets.CUseHotbarSlot) = AddressOf Packet_UseHotbarSlot

        Socket.PacketId(ClientPackets.CSkillLearn) = AddressOf Packet_SkillLearn

        Socket.PacketId(ClientPackets.CEventChatReply) = AddressOf Packet_EventChatReply
        Socket.PacketId(ClientPackets.CEvent) = AddressOf Packet_Event
        Socket.PacketId(ClientPackets.CRequestSwitchesAndVariables) = AddressOf Packet_RequestSwitchesAndVariables
        Socket.PacketId(ClientPackets.CSwitchesAndVariables) = AddressOf Packet_SwitchesAndVariables

        Socket.PacketId(ClientPackets.CRequestProjectiles) = AddressOf HandleRequestProjectile
        Socket.PacketId(ClientPackets.CClearProjectile) = AddressOf HandleClearProjectile

        Socket.PacketId(ClientPackets.CEmote) = AddressOf Packet_Emote

        Socket.PacketId(ClientPackets.CRequestParty) = AddressOf Packet_PartyRquest
        Socket.PacketId(ClientPackets.CAcceptParty) = AddressOf Packet_AcceptParty
        Socket.PacketId(ClientPackets.CDeclineParty) = AddressOf Packet_DeclineParty
        Socket.PacketId(ClientPackets.CLeaveParty) = AddressOf Packet_LeaveParty
        Socket.PacketId(ClientPackets.CPartyChatMsg) = AddressOf Packet_PartyChatMsg

        Socket.PacketId(ClientPackets.CRequestPets) = AddressOf Packet_RequestPets
        Socket.PacketId(ClientPackets.CSummonPet) = AddressOf Packet_SummonPet
        Socket.PacketId(ClientPackets.CPetMove) = AddressOf Packet_PetMove
        Socket.PacketId(ClientPackets.CSetBehaviour) = AddressOf Packet_SetPetBehaviour
        Socket.PacketId(ClientPackets.CReleasePet) = AddressOf Packet_ReleasePet
        Socket.PacketId(ClientPackets.CPetSkill) = AddressOf Packet_PetSkill
        Socket.PacketId(ClientPackets.CPetUseStatPoint) = AddressOf Packet_UsePetStatPoint
        Socket.PacketId(ClientPackets.CRequestEditPet) = AddressOf Packet_RequestPet

        Socket.PacketId(ClientPackets.CRequestEditItem) = AddressOf Packet_EditItem
        Socket.PacketId(ClientPackets.CSaveItem) = AddressOf Packet_SaveItem
        Socket.PacketId(ClientPackets.CRequestEditNpc) = AddressOf Packet_EditNpc
        Socket.PacketId(ClientPackets.CSaveNpc) = AddressOf Packet_SaveNPC
        Socket.PacketId(ClientPackets.CRequestEditShop) = AddressOf Packet_EditShop
        Socket.PacketId(ClientPackets.CSaveShop) = AddressOf Packet_SaveShop
        Socket.PacketId(ClientPackets.CRequestEditSkill) = AddressOf Packet_EditSkill
        Socket.PacketId(ClientPackets.CSaveSkill) = AddressOf Packet_SaveSkill
        Socket.PacketId(ClientPackets.CRequestEditResource) = AddressOf Packet_EditResource
        Socket.PacketId(ClientPackets.CSaveResource) = AddressOf Packet_SaveResource
        Socket.PacketId(ClientPackets.CRequestEditAnimation) = AddressOf Packet_EditAnimation
        Socket.PacketId(ClientPackets.CSaveAnimation) = AddressOf Packet_SaveAnimation
        Socket.PacketId(ClientPackets.CRequestEditProjectiles) = AddressOf HandleRequestEditProjectile
        Socket.PacketId(ClientPackets.CSaveProjectile) = AddressOf HandleSaveProjectile
        Socket.PacketId(ClientPackets.CRequestEditJob) = AddressOf Packet_RequestEditJob
        Socket.PacketId(ClientPackets.CSaveJob) = AddressOf Packet_SaveJob

        Socket.PacketId(ClientPackets.CRequestEditPet) = AddressOf Packet_RequestEditPet
        Socket.PacketId(ClientPackets.CSavePet) = AddressOf Packet_SavePet

        Socket.PacketId(ClientPackets.CRequestMoral) = AddressOf Packet_RequestMoral
        Socket.PacketId(ClientPackets.CRequestEditMoral) = AddressOf Packet_RequestEditMoral
        Socket.PacketId(ClientPackets.CSaveMoral) = AddressOf Packet_SaveMoral

        Socket.PacketId(ClientPackets.CCloseEditor) = AddressOf Packet_CloseEditor

    End Sub

    Private Sub Packet_Ping(index As Integer, ByRef data() As Byte)
        TempPlayer(index).DataPackets = TempPlayer(index).DataPackets + 1
    End Sub

    Private Sub Packet_Login(index As Integer, ByRef data() As Byte)
        Dim username As String, IP As String
        Dim password As String, i As Integer
        Dim buffer As New ByteStream(data)

        If Not IsPlaying(index) Then
            If Not IsLoggedIn(index) Then
                'check if its banned
                ' Cut off last portion of ip
                IP = Socket.ClientIp(index)

                For i = Len(IP) To 1 Step -1

                    If Mid$(IP, i, 1) = "." Then
                        Exit For
                    End If

                Next

                IP = Mid$(IP, 1, i)

                If shutDownDuration > 0 Then
                    Call AlertMsg(index, DialogueMsg.Maintenance, MenuType.Login)
                    Exit Sub
                End If

                ' Get the data
                username = EKeyPair.DecryptString(buffer.ReadString()).ToLower
                password = EKeyPair.DecryptString(buffer.ReadString())

                ' Get the current executing assembly
                Dim assembly As Assembly = Assembly.GetExecutingAssembly()

                ' Retrieve the version information
                Dim version As Version = assembly.GetName().Version

                ' Check versions
                If EKeyPair.DecryptString(buffer.ReadString) <> version.ToString() Then
                    AlertMsg(index, DialogueMsg.Outdated, MenuType.Login)
                    Exit Sub
                End If

                If username.Length > NAME_LENGTH Or username.Length < 3 Then
                    AlertMsg(index, DialogueMsg.NameLength)
                    Exit Sub
                End If

                If IsMultiAccounts(index, username) Then
                    AlertMsg(index, DialogueMsg.MultiAccount, MenuType.Login)
                    Exit Sub
                End If

                If LoadAccount(index, username) = 0 Then
                    AlertMsg(index, DialogueMsg.Login, MenuType.Login)
                    Exit Sub
                End If

                If GetPlayerPassword(index) <> password Then
                    AlertMsg(index, DialogueMsg.WrongPass, MenuType.Login)
                    Exit Sub
                End If

                If IsBanned(index, IP) Then
                    AlertMsg(index, DialogueMsg.Banned, MenuType.Login)
                    Exit Sub
                End If

                If GetPlayerLogin(index) = "" Then
                    AlertMsg(index, DialogueMsg.Database, MenuType.Login)
                    Exit Sub
                End If

                ' Show the player up on the socket status
                Addlog(GetPlayerLogin(index) & " has logged in from " & Socket.ClientIp(index) & ".", PLAYER_LOG)
                Console.WriteLine(GetPlayerLogin(index) & " has logged in from " & Socket.ClientIp(index) & ".")

                ' send them to the character portal
                SendPlayerChars(index)
                SendJobs(index)
            End If
        End If
    End Sub

    Private Sub Packet_Register(index As Integer, ByRef data() As Byte)
        Dim username As String, IP As String
        Dim password As String, i As Integer, n As Integer
        Dim buffer As New ByteStream(data)
        Dim userData As JObject

        If Not IsPlaying(index) Then
            If Not IsLoggedIn(index) Then
                'check if its banned
                ' Cut off last portion of ip
                IP = Socket.ClientIp(index)

                For i = Len(IP) To 1 Step -1

                    If Mid$(IP, i, 1) = "." Then
                        Exit For
                    End If

                Next

                IP = Mid$(IP, 1, i)

                If IsBanned(index, IP) Then
                    AlertMsg(index, DialogueMsg.Banned, MenuType.Register)
                    Exit Sub
                End If

                If shutDownTimer.IsRunning Then
                    Call AlertMsg(index, DialogueMsg.Maintenance, MenuType.Register)
                    Exit Sub
                End If

                ' Get the data
                username = EKeyPair.DecryptString(buffer.ReadString()).ToLower
                password = EKeyPair.DecryptString(buffer.ReadString())

                For i = 1 To Len(username)
                    n = AscW(Mid$(username, i, 1))

                    If Not IsNameLegal(n) Then
                        AlertMsg(index, DialogueMsg.NameIllegal, MenuType.Register)
                        Exit Sub
                    End If

                Next

                ' Get the current executing assembly
                Dim assembly As Assembly = Assembly.GetExecutingAssembly()

                ' Retrieve the version information
                Dim version As Version = assembly.GetName().Version

                ' Check versions
                If EKeyPair.DecryptString(buffer.ReadString) <> version.ToString() Then
                    AlertMsg(index, DialogueMsg.Outdated, MenuType.Register)
                    Exit Sub
                End If

                If username.Length > NAME_LENGTH Or username.Length < 3 Then
                    AlertMsg(index, DialogueMsg.NameLength, MenuType.Register)
                    Exit Sub
                End If

                If IsMultiAccounts(index, username) Then
                    AlertMsg(index, DialogueMsg.MultiAccount, MenuType.Register)
                    Exit Sub
                End If

                userData = SelectRowByColumn("id", GetStringHash(username), "account", "data")

                If Not userData Is Nothing Then
                    AlertMsg(index, DialogueMsg.NameTaken, MenuType.Register)
                    Exit Sub
                End If

                RegisterAccount(index, username, password)

                ' send them to the character portal
                SendPlayerChars(index)
                SendJobs(index)
            End If
        End If
    End Sub

    Private Sub Packet_UseChar(index As Integer, ByRef data() As Byte)
        Dim slot As Byte
        Dim buffer As New ByteStream(data)

        If Not IsPlaying(index) Then
            If IsLoggedIn(index) Then
                slot = buffer.ReadInt32

                If slot < 1 Or slot > MAX_CHARS Then
                    AlertMsg(index, DialogueMsg.MaxChar, MenuType.Chars)
                    Exit Sub
                End If

                LoadCharacter(index, slot)
                LoadBank(index)

                ' Check if character data has been created
                If Len(Type.Player(index).Name) > 0 Then
                    ' we have a char!
                    TempPlayer(index).Slot = slot
                    HandleUseChar(index)
                Else
                    AlertMsg(index, DialogueMsg.Database, MenuType.Chars)
                    Exit Sub
                End If
            End If
        End If

    End Sub

    Private Sub Packet_AddChar(index As Integer, ByRef data() As Byte)
        Dim name As String, slot As Byte
        Dim sexNum As Integer
        Dim jobNum As Integer
        Dim sprite As Integer
        Dim i As Integer
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        If Not IsPlaying(index) Then
            slot = buffer.ReadInt32
            name = buffer.ReadString
            sexNum = buffer.ReadInt32
            jobNum = buffer.ReadInt32 + 1

            If slot < 1 Or slot > MAX_CHARS Then
                AlertMsg(index, DialogueMsg.MaxChar, MenuType.Chars)
                Exit Sub
            End If

            If LoadCharacter(index, slot) Then
                SendPlayerChars(index)
                Exit Sub
            End If

            If name.Length > NAME_LENGTH Or name.Length < 3 Then
                AlertMsg(index, DialogueMsg.NameLength, MenuType.NewChar)
                Exit Sub
            End If

            For i = 1 To Len(name)
                n = AscW(Mid$(name, i, 1))

                If Not IsNameLegal(n) Then
                    AlertMsg(index, DialogueMsg.NameIllegal, MenuType.NewChar)
                    Exit Sub
                End If

            Next

            ' Check if name is already in use
            If Core.Char.Find(name) Then
                AlertMsg(index, DialogueMsg.NameTaken, MenuType.NewChar)
                Exit Sub
            End If

            If (sexNum < SexType.Male) Or (sexNum > SexType.Female) Then Exit Sub

            If jobNum <= 0 Or jobNum > MAX_JOBS Then Exit Sub

            If sexNum = SexType.Male Then
                sprite = Type.Job(jobNum).MaleSprite
            Else
                sprite = Type.Job(jobNum).FemaleSprite
            End If

            ' Everything went ok, add the character
            Core.Char.Add(name)
            AddChar(index, slot, name, sexNum, jobNum, sprite)
            Addlog("Character " & name & " added to " & GetPlayerLogin(index) & "'s account.", PLAYER_LOG)
            HandleUseChar(index)

            buffer.Dispose()
        End If

    End Sub

    Private Sub Packet_DelChar(index As Integer, ByRef data() As Byte)
        Dim slot As Byte
        Dim buffer As New ByteStream(data)

        If Not IsPlaying(index) Then
            slot = buffer.ReadInt32

            If slot < 1 Or slot > MAX_CHARS Then
                AlertMsg(index, DialogueMsg.MaxChar, MenuType.Chars)
                Exit Sub
            End If

            LoadCharacter(index, slot)
            Core.Char.Remove(GetPlayerName(index))
            ClearCharacter(index)
            SaveCharacter(index, slot)

            ' send them to the character portal
            Call SendPlayerChars(index)

            For i = 1 To MAX_JOBS
                Call SendUpdateJob(index, i)
            Next

            buffer.Dispose()
        End If
    End Sub

    Private Sub Packet_SayMessage(index As Integer, ByRef data() As Byte)
        Dim msg As String
        Dim buffer As New ByteStream(data)

        msg = buffer.ReadString

        Addlog("Map #" & GetPlayerMap(index) & ": " & GetPlayerName(index) & " says, '" & msg & "'", PLAYER_LOG)

        SayMsg_Map(GetPlayerMap(index), index, msg, ColorType.White)
        SendChatBubble(GetPlayerMap(index), index, TargetType.Player, msg, ColorType.White)

        buffer.Dispose()
    End Sub

    Private Sub Packet_BroadCastMsg(index As Integer, ByRef data() As Byte)
        Dim msg As String
        Dim s As String
        Dim buffer As New ByteStream(data)

        msg = buffer.ReadString

        s = "[Global] " & GetPlayerName(index) & ": " & msg
        SayMsg_Global(index, msg, ColorType.White)
        Addlog(s, PLAYER_LOG)
        Console.WriteLine(s)

        buffer.Dispose()
    End Sub

    Friend Sub Packet_PlayerMsg(index As Integer, ByRef data() As Byte)
        Dim OtherPlayer As String, Msg As String, OtherPlayerindex As Integer
        Dim buffer As New ByteStream(data)

        OtherPlayer = buffer.ReadString
        Msg = buffer.ReadString
        buffer.Dispose()

        OtherPlayerindex = FindPlayer(OtherPlayer)
        If OtherPlayerindex <> index Then
            If OtherPlayerindex > 0 Then
                Addlog(GetPlayerName(index) & " tells " & GetPlayerName(index) & ", '" & Msg & "'", PLAYER_LOG)
                PlayerMsg(OtherPlayerindex, GetPlayerName(index) & " tells you, '" & Msg & "'", ColorType.Pink)
                PlayerMsg(index, "You tell " & GetPlayerName(OtherPlayerindex) & ", '" & Msg & "'", ColorType.Pink)
            Else
                PlayerMsg(index, "Player is not online.", ColorType.BrightRed)
            End If
        Else
            PlayerMsg(index, "Cannot message your self!", ColorType.BrightRed)
        End If
    End Sub

    Private Sub Packet_AdminMsg(index As Integer, ByRef data() As Byte)
        Dim msg As String
        Dim s As String
        Dim buffer As New ByteStream(data)

        msg = buffer.ReadString

        AdminMsg(GetPlayerMap(index), msg, ColorType.BrightCyan)
        Addlog(s, PLAYER_LOG)
        Console.WriteLine(s)

        buffer.Dispose()
    End Sub

    Private Sub Packet_PlayerMove(index As Integer, ByRef data() As Byte)
        Dim Dir As Integer
        Dim movement As Integer
        Dim tmpX As Integer, tmpY As Integer
        Dim buffer As New ByteStream(data)

        If TempPlayer(index).GettingMap Then Exit Sub

        Dir = buffer.ReadInt32
        movement = buffer.ReadInt32
        tmpX = buffer.ReadInt32
        tmpY = buffer.ReadInt32
        buffer.Dispose()

        ' Prevent player from moving if they have casted a skill
        If TempPlayer(index).SkillBuffer > 0 Then
            SendPlayerXY(index)
            Exit Sub
        End If

        'Cant move if in the bank!
        If TempPlayer(index).InBank Then
            SendPlayerXY(index)
            Exit Sub
        End If

        ' if stunned, stop them moving
        If TempPlayer(index).StunDuration > 0 Then
            SendPlayerXY(index)
            Exit Sub
        End If

        ' Prevent player from moving if in shop
        If TempPlayer(index).InShop > 0 Then
            SendPlayerXY(index)
            Exit Sub
        End If

        ' Desynced
        If GetPlayerX(index) <> tmpX Then
            SendPlayerXY(index)
            Exit Sub
        End If

        If GetPlayerY(index) <> tmpY Then
            SendPlayerXY(index)
            Exit Sub
        End If

        PlayerMove(index, Dir, movement, False)

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerDirection(index As Integer, ByRef data() As Byte)
        Dim dir As Integer
        Dim buffer As New ByteStream(data)

        If TempPlayer(index).GettingMap = 1 Then Exit Sub

        dir = buffer.ReadInt32
        buffer.Dispose()

        ' Prevent hacking
        If dir <= DirectionType.None Or dir > DirectionType.Left Then Exit Sub

        SetPlayerDir(index, dir)

        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SPlayerDir)
        buffer.WriteInt32(index)
        buffer.WriteInt32(GetPlayerDir(index))
        SendDataToMapBut(index, GetPlayerMap(index), buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub Packet_UseItem(index As Integer, ByRef data() As Byte)
        Dim invNum As Integer
        Dim buffer As New ByteStream(data)

        invNum = buffer.ReadInt32
        buffer.Dispose()

        UseItem(index, invNum)
    End Sub

    Sub Packet_Attack(index As Integer, ByRef data() As Byte)
        Dim i As Integer
        Dim Tempindex As Integer
        Dim x As Integer, y As Integer
        Dim buffer As New ByteStream(data)

        ' can't attack whilst casting
        If TempPlayer(index).SkillBuffer > 0 Then Exit Sub

        ' can't attack whilst stunned
        If TempPlayer(index).StunDuration > 0 Then Exit Sub

        ' Send this packet so they can see the person attacking
        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SAttack)
        buffer.WriteInt32(index)
        SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)
        buffer.Dispose()

        ' Projectile check
        If GetPlayerEquipment(index, EquipmentType.Weapon) > 0 Then
            If Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Projectile > 0 Then 'Item has a projectile
                If Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Ammo > 0 Then
                    If HasItem(index, Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Ammo) Then
                        TakeInv(index, Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Ammo, 1)
                        PlayerFireProjectile(index)
                        Exit Sub
                    Else
                        PlayerMsg(index, "No More " & Type.Item(Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Ammo).Name & " !", ColorType.BrightRed)
                        Exit Sub
                    End If
                Else
                    PlayerFireProjectile(index)
                    Exit Sub
                End If
            End If
        End If

        ' Try to attack a player
        For i = 1 To Socket.HighIndex()
            Tempindex = i

            ' Make sure we dont try to attack ourselves
            If Tempindex <> index Then
                If IsPlaying(Tempindex) Then
                    TryPlayerAttackPlayer(index, i)
                End If
            End If
        Next

        ' Try to attack a npc
        For i = 1 To MAX_MAP_NPCS
            TryPlayerAttackNpc(index, i)
        Next

        ' Check tradeskills
        Select Case GetPlayerDir(index)
            Case DirectionType.Up

                If GetPlayerY(index) = 0 Then Exit Sub
                x = GetPlayerX(index)
                y = GetPlayerY(index) - 1
            Case DirectionType.Down

                If GetPlayerY(index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Sub
                x = GetPlayerX(index)
                y = GetPlayerY(index) + 1
            Case DirectionType.Left

                If GetPlayerX(index) = 0 Then Exit Sub
                x = GetPlayerX(index) - 1
                y = GetPlayerY(index)
            Case DirectionType.Right

                If GetPlayerX(index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Sub
                x = GetPlayerX(index) + 1
                y = GetPlayerY(index)

            Case DirectionType.UpRight

                If GetPlayerX(index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Sub
                If GetPlayerY(index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Sub
                x = GetPlayerX(index) + 1
                y = GetPlayerY(index) - 1

            Case DirectionType.UpLeft

                If GetPlayerX(index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Sub
                If GetPlayerY(index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Sub
                x = GetPlayerX(index) - 1
                y = GetPlayerY(index) - 1

            Case DirectionType.DownRight

                If GetPlayerX(index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Sub
                If GetPlayerY(index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Sub
                x = GetPlayerX(index) + 1
                y = GetPlayerY(index) + 1

            Case DirectionType.DownLeft

                If GetPlayerX(index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Sub
                If GetPlayerY(index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Sub
                x = GetPlayerX(index) - 1
                y = GetPlayerY(index) + 1
        End Select

        CheckResource(index, x, y)

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerInfo(index As Integer, ByRef data() As Byte)
        Dim i As Integer, n As Integer
        Dim name As String
        Dim buffer As New ByteStream(data)

        name = buffer.ReadString
        i = FindPlayer(name)

        If i > 0 Then
            PlayerMsg(index, "Account:  " & GetPlayerLogin(i) & ", Name: " & GetPlayerName(i), ColorType.Yellow)

            If GetPlayerAccess(index) > AccessType.Moderator Then
                PlayerMsg(index, " Stats for " & GetPlayerName(i) & " ", ColorType.Yellow)
                PlayerMsg(index, "Level: " & GetPlayerLevel(i) & "  Exp: " & GetPlayerExp(i) & "/" & GetPlayerNextLevel(i), ColorType.Yellow)
                PlayerMsg(index, "HP: " & GetPlayerVital(i, VitalType.HP) & "/" & GetPlayerMaxVital(i, VitalType.HP) & "  MP: " & GetPlayerVital(i, VitalType.SP) & "/" & GetPlayerMaxVital(i, VitalType.SP) & "  SP: " & GetPlayerVital(i, VitalType.SP) & "/" & GetPlayerMaxVital(i, VitalType.SP), ColorType.Yellow)
                PlayerMsg(index, "Strength: " & GetPlayerStat(i, StatType.Strength) & "  Defense: " & GetPlayerStat(i, StatType.Luck) & "  Magic: " & GetPlayerStat(i, StatType.Intelligence) & "  Speed: " & GetPlayerStat(i, StatType.Spirit), ColorType.Yellow)
                n = (GetPlayerStat(i, StatType.Strength) \ 2) + (GetPlayerLevel(i) \ 2)
                i = (GetPlayerStat(i, StatType.Luck) \ 2) + (GetPlayerLevel(i) \ 2)

                If n > 100 Then n = 100
                If i > 100 Then i = 100
                PlayerMsg(index, "Critical Hit Chance: " & n & "%, Block Chance: " & i & "%", ColorType.Yellow)
            End If
        Else
            PlayerMsg(index, "Player is not online.", ColorType.BrightRed)
        End If

        buffer.Dispose()
    End Sub

    Sub Packet_WarpMeTo(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        ' The player
        n = FindPlayer(buffer.ReadString)
        buffer.Dispose()

        If n <> index Then
            If n > 0 Then
                PlayerWarp(index, GetPlayerMap(n), GetPlayerX(n), GetPlayerY(n))
                PlayerMsg(n, GetPlayerName(index) & " has warped to you.", ColorType.Yellow)
                PlayerMsg(index, "You have been warped to " & GetPlayerName(n) & ".", ColorType.Yellow)
                Addlog(GetPlayerName(index) & " has warped to " & GetPlayerName(n) & ", map #" & GetPlayerMap(n) & ".", ADMIN_LOG)
            Else
                PlayerMsg(index, "Player is not online.", ColorType.BrightRed)
            End If
        Else
            PlayerMsg(index, "You cannot warp to yourself, dumbass!", ColorType.BrightRed)
        End If

    End Sub

    Sub Packet_WarpToMe(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        ' The player
        n = FindPlayer(buffer.ReadString)
        buffer.Dispose()

        If n <> index Then
            If n > 0 Then
                PlayerWarp(n, GetPlayerMap(index), GetPlayerX(index), GetPlayerY(index))
                PlayerMsg(n, "You have been summoned by " & GetPlayerName(index) & ".", ColorType.Yellow)
                PlayerMsg(index, GetPlayerName(n) & " has been summoned.", ColorType.Yellow)
                Addlog(GetPlayerName(index) & " has warped " & GetPlayerName(n) & " to self, map #" & GetPlayerMap(index) & ".", ADMIN_LOG)
            Else
                PlayerMsg(index, "Player is not online.", ColorType.BrightRed)
            End If
        Else
            PlayerMsg(index, "You cannot warp yourself to yourself, dumbass!", ColorType.BrightRed)
        End If

    End Sub

    Sub Packet_WarpTo(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        ' The map
        n = buffer.ReadInt32
        buffer.Dispose()

        ' Prevent hacking
        If n <= 0 Or n > MAX_MAPS Then Exit Sub

        PlayerWarp(index, n, GetPlayerX(index), GetPlayerY(index))
        PlayerMsg(index, "You have been warped to map #" & n, ColorType.Yellow)
        Addlog(GetPlayerName(index) & " warped to map #" & n & ".", ADMIN_LOG)
    End Sub

    Sub Packet_SetSprite(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        ' The sprite
        n = buffer.ReadInt32
        buffer.Dispose()

        SetPlayerSprite(index, n)
        SendPlayerData(index)

    End Sub

    Sub Packet_GetStats(index As Integer, ByRef data() As Byte)
        Dim i As Integer
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        PlayerMsg(index, "Stats: " & GetPlayerName(index), ColorType.Yellow)
        PlayerMsg(index, "Level: " & GetPlayerLevel(index) & "  Exp: " & GetPlayerExp(index) & "/" & GetPlayerNextLevel(index), ColorType.Yellow)
        PlayerMsg(index, "HP: " & GetPlayerVital(index, VitalType.HP) & "/" & GetPlayerMaxVital(index, VitalType.HP) & "  MP: " & GetPlayerVital(index, VitalType.SP) & "/" & GetPlayerMaxVital(index, VitalType.SP) & "  SP: " & GetPlayerVital(index, VitalType.SP) & "/" & GetPlayerMaxVital(index, VitalType.SP), ColorType.Yellow)
        PlayerMsg(index, "STR: " & GetPlayerStat(index, StatType.Strength) & "  DEF: " & GetPlayerStat(index, StatType.Luck) & "  MAGI: " & GetPlayerStat(index, StatType.Intelligence) & "  Speed: " & GetPlayerStat(index, StatType.Spirit), ColorType.Yellow)
        n = (GetPlayerStat(index, StatType.Strength) \ 2) + (GetPlayerLevel(index) \ 2)
        i = (GetPlayerStat(index, StatType.Luck) \ 2) + (GetPlayerLevel(index) \ 2)

        If n > 100 Then n = 100
        If i > 100 Then i = 100
        PlayerMsg(index, "Critical Hit Chance: " & n & "%, Block Chance: " & i & "%", ColorType.Yellow)
        buffer.Dispose()
    End Sub

    Sub Packet_RequestNewMap(index As Integer, ByRef data() As Byte)
        Dim dir As Integer
        Dim buffer As New ByteStream(data)

        dir = buffer.ReadInt32
        buffer.Dispose()

        PlayerMove(index, dir, 1, True)
    End Sub

    Sub Packet_MapData(index As Integer, ByRef data() As Byte)
        Dim i As Integer
        Dim mapNum As Integer
        Dim x As Integer
        Dim y As Integer

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        Dim buffer As New ByteStream(Mirage.Sharp.Asfw.IO.Compression.DecompressBytes(data))

        mapNum = GetPlayerMap(index)

        i = Type.Map(mapNum).Revision + 1
        ClearMap(mapNum)

        Type.Map(mapNum).Name = buffer.ReadString
        Type.Map(mapNum).Music = buffer.ReadString
        Type.Map(mapNum).Revision = i
        Type.Map(mapNum).Moral = buffer.ReadInt32
        Type.Map(mapNum).Tileset = buffer.ReadInt32
        Type.Map(mapNum).Up = buffer.ReadInt32
        Type.Map(mapNum).Down = buffer.ReadInt32
        Type.Map(mapNum).Left = buffer.ReadInt32
        Type.Map(mapNum).Right = buffer.ReadInt32
        Type.Map(mapNum).BootMap = buffer.ReadInt32
        Type.Map(mapNum).BootX = buffer.ReadInt32
        Type.Map(mapNum).BootY = buffer.ReadInt32
        Type.Map(mapNum).MaxX = buffer.ReadInt32
        Type.Map(mapNum).MaxY = buffer.ReadInt32
        Type.Map(mapNum).Weather = buffer.ReadInt32
        Type.Map(mapNum).Fog = buffer.ReadInt32
        Type.Map(mapNum).WeatherIntensity = buffer.ReadInt32
        Type.Map(mapNum).FogOpacity = buffer.ReadInt32
        Type.Map(mapNum).FogSpeed = buffer.ReadInt32
        Type.Map(mapNum).MapTint = buffer.ReadInt32
        Type.Map(mapNum).MapTintR = buffer.ReadInt32
        Type.Map(mapNum).MapTintG = buffer.ReadInt32
        Type.Map(mapNum).MapTintB = buffer.ReadInt32
        Type.Map(mapNum).MapTintA = buffer.ReadInt32
        Type.Map(mapNum).Panorama = buffer.ReadByte
        Type.Map(mapNum).Parallax = buffer.ReadByte
        Type.Map(mapNum).Brightness = buffer.ReadByte
        Type.Map(mapNum).NoRespawn = buffer.ReadInt32
        Type.Map(mapNum).Indoors = buffer.ReadInt32
        Type.Map(mapNum).Shop = buffer.ReadInt32

        ReDim Type.Map(mapNum).Tile(Type.Map(mapNum).MaxX, Type.Map(mapNum).MaxY)

        For x = 1 To MAX_MAP_NPCS
            ClearMapNPC(x, mapNum)
            Type.Map(mapNum).NPC(x) = buffer.ReadInt32
        Next

        With Type.Map(mapNum)
            For x = 0 To .MaxX
                For y = 0 To .MaxY
                    .Tile(x, y).Data1 = buffer.ReadInt32
                    .Tile(x, y).Data2 = buffer.ReadInt32
                    .Tile(x, y).Data3 = buffer.ReadInt32
                    .Tile(x, y).Data1_2 = buffer.ReadInt32
                    .Tile(x, y).Data2_2 = buffer.ReadInt32
                    .Tile(x, y).Data3_2 = buffer.ReadInt32
                    .Tile(x, y).DirBlock = buffer.ReadInt32
                    ReDim .Tile(x, y).Layer(LayerType.Count - 1)
                    For i = 1 To LayerType.Count - 1
                        .Tile(x, y).Layer(i).Tileset = buffer.ReadInt32
                        .Tile(x, y).Layer(i).X = buffer.ReadInt32
                        .Tile(x, y).Layer(i).Y = buffer.ReadInt32
                        .Tile(x, y).Layer(i).AutoTile = buffer.ReadInt32
                    Next
                    .Tile(x, y).Type = buffer.ReadInt32
                    .Tile(x, y).Type2 = buffer.ReadInt32
                Next
            Next

        End With

        Type.Map(mapNum).EventCount = buffer.ReadInt32

        If Type.Map(mapNum).EventCount > 0 Then
            ReDim Type.Map(mapNum).Event(Type.Map(mapNum).EventCount)
            For i = 0 To Type.Map(mapNum).EventCount
                With Type.Map(mapNum).Event(i)
                    .Name = buffer.ReadString
                    .Globals = buffer.ReadByte
                    .X = buffer.ReadInt32
                    .Y = buffer.ReadInt32
                    .PageCount = buffer.ReadInt32
                End With

                If Type.Map(mapNum).Event(i).PageCount > 0 Then
                    ReDim Type.Map(mapNum).Event(i).Pages(Type.Map(mapNum).Event(i).PageCount)
                    ReDim TempPlayer(i).EventMap.EventPages(Type.Map(mapNum).Event(i).PageCount)
                    For x = 0 To Type.Map(mapNum).Event(i).PageCount
                        With Type.Map(mapNum).Event(i).Pages(x)
                            .ChkVariable = buffer.ReadInt32
                            .VariableIndex = buffer.ReadInt32
                            .VariableCondition = buffer.ReadInt32
                            .VariableCompare = buffer.ReadInt32

                            .ChkSwitch = buffer.ReadInt32
                            .SwitchIndex = buffer.ReadInt32
                            .SwitchCompare = buffer.ReadInt32

                            .ChkHasItem = buffer.ReadInt32
                            .HasItemIndex = buffer.ReadInt32
                            .HasItemAmount = buffer.ReadInt32

                            .ChkSelfSwitch = buffer.ReadInt32
                            .SelfSwitchIndex = buffer.ReadInt32
                            .SelfSwitchCompare = buffer.ReadInt32

                            .GraphicType = buffer.ReadByte
                            .Graphic = buffer.ReadInt32
                            .GraphicX = buffer.ReadInt32
                            .GraphicY = buffer.ReadInt32
                            .GraphicX2 = buffer.ReadInt32
                            .GraphicY2 = buffer.ReadInt32

                            .MoveType = buffer.ReadByte
                            .MoveSpeed = buffer.ReadByte
                            .MoveFreq = buffer.ReadByte
                            .MoveRouteCount = buffer.ReadInt32
                            .IgnoreMoveRoute = buffer.ReadInt32
                            .RepeatMoveRoute = buffer.ReadInt32

                            If .MoveRouteCount > 0 Then
                                ReDim Type.Map(mapNum).Event(i).Pages(x).MoveRoute(.MoveRouteCount)
                                For y = 0 To .MoveRouteCount
                                    .MoveRoute(y).Index = buffer.ReadInt32
                                    .MoveRoute(y).Data1 = buffer.ReadInt32
                                    .MoveRoute(y).Data2 = buffer.ReadInt32
                                    .MoveRoute(y).Data3 = buffer.ReadInt32
                                    .MoveRoute(y).Data4 = buffer.ReadInt32
                                    .MoveRoute(y).Data5 = buffer.ReadInt32
                                    .MoveRoute(y).Data6 = buffer.ReadInt32
                                Next
                            End If

                            .WalkAnim = buffer.ReadInt32
                            .DirFix = buffer.ReadInt32
                            .WalkThrough = buffer.ReadInt32
                            .ShowName = buffer.ReadInt32
                            .Trigger = buffer.ReadByte
                            .CommandListCount = buffer.ReadInt32
                            .Position = buffer.ReadByte
                            .QuestNum = buffer.ReadInt32
                        End With

                        If Type.Map(mapNum).Event(i).Pages(x).CommandListCount > 0 Then
                            ReDim Type.Map(mapNum).Event(i).Pages(x).CommandList(Type.Map(mapNum).Event(i).Pages(x).CommandListCount)
                            For y = 0 To Type.Map(mapNum).Event(i).Pages(x).CommandListCount
                                Type.Map(mapNum).Event(i).Pages(x).CommandList(y).CommandCount = buffer.ReadInt32
                                Type.Map(mapNum).Event(i).Pages(x).CommandList(y).ParentList = buffer.ReadInt32
                                If Type.Map(mapNum).Event(i).Pages(x).CommandList(y).CommandCount > 0 Then
                                    ReDim Type.Map(mapNum).Event(i).Pages(x).CommandList(y).Commands(Type.Map(mapNum).Event(i).Pages(x).CommandList(y).CommandCount)
                                    For z = 0 To Type.Map(mapNum).Event(i).Pages(x).CommandList(y).CommandCount
                                        With Type.Map(mapNum).Event(i).Pages(x).CommandList(y).Commands(z)
                                            .Index = buffer.ReadByte
                                            .Text1 = buffer.ReadString
                                            .Text2 = buffer.ReadString
                                            .Text3 = buffer.ReadString
                                            .Text4 = buffer.ReadString
                                            .Text5 = buffer.ReadString
                                            .Data1 = buffer.ReadInt32
                                            .Data2 = buffer.ReadInt32
                                            .Data3 = buffer.ReadInt32
                                            .Data4 = buffer.ReadInt32
                                            .Data5 = buffer.ReadInt32
                                            .Data6 = buffer.ReadInt32
                                            .ConditionalBranch.CommandList = buffer.ReadInt32
                                            .ConditionalBranch.Condition = buffer.ReadInt32
                                            .ConditionalBranch.Data1 = buffer.ReadInt32
                                            .ConditionalBranch.Data2 = buffer.ReadInt32
                                            .ConditionalBranch.Data3 = buffer.ReadInt32
                                            .ConditionalBranch.ElseCommandList = buffer.ReadInt32
                                            .MoveRouteCount = buffer.ReadInt32
                                            Dim tmpcount As Integer = .MoveRouteCount
                                            If tmpcount > 0 Then
                                                ReDim Preserve .MoveRoute(tmpcount)
                                                For w = 0 To tmpcount
                                                    .MoveRoute(w).Index = buffer.ReadInt32
                                                    .MoveRoute(w).Data1 = buffer.ReadInt32
                                                    .MoveRoute(w).Data2 = buffer.ReadInt32
                                                    .MoveRoute(w).Data3 = buffer.ReadInt32
                                                    .MoveRoute(w).Data4 = buffer.ReadInt32
                                                    .MoveRoute(w).Data5 = buffer.ReadInt32
                                                    .MoveRoute(w).Data6 = buffer.ReadInt32
                                                Next
                                            End If
                                        End With
                                    Next
                                End If
                            Next
                        End If
                    Next
                End If
            Next
        End If

        ' Save the map
        SaveMap(mapNum)
        SendMapNPCsToMap(mapNum)
        SpawnMapNPCs(mapNum)
        SpawnGlobalEvents(mapNum)

        For i = 1 To Socket.HighIndex()
            If IsPlaying(i) Then
                If Type.Player(i).Map = mapNum Then
                    SpawnMapEventsFor(i, mapNum)
                End If
            End If
        Next

        ' Clear it all out
        For i = 1 To MAX_MAP_ITEMS
            SpawnItemSlot(i, 0, 0, GetPlayerMap(index), MapItem(GetPlayerMap(index), i).X, MapItem(GetPlayerMap(index), i).Y)
            ClearMapItem(i, GetPlayerMap(index))
        Next

        ' Respawn
        SpawnMapItems(GetPlayerMap(index))
        CacheResources(mapNum)

        ' Refresh map for everyone online
        For i = 1 To Socket.HighIndex()
            If IsPlaying(i) And GetPlayerMap(i) = mapNum Then
                PlayerWarp(i, mapNum, GetPlayerX(i), GetPlayerY(i))
                ' Send map
                SendMapData(i, mapNum, True)
            End If
        Next

        buffer.Dispose()
    End Sub

    Private Sub Packet_NeedMap(index As Integer, ByRef data() As Byte)
        Dim s As String
        Dim buffer As New ByteStream(data)

        ' Get yes/no value
        s = buffer.ReadInt32
        buffer.Dispose()

        ' CheckIf Type.Map data is needed to be sent
        If s = 1 Then
            SendMapData(index, GetPlayerMap(index), True)
        Else
            SendMapData(index, GetPlayerMap(index), False)
        End If

        If Type.Map(GetPlayerMap(index)).Shop > 0 Then
            If Type.Shop(Type.Map(GetPlayerMap(index)).Shop).Name <> "" Then
                TempPlayer(index).InShop = Type.Map(GetPlayerMap(index)).Shop
                SendOpenShop(index, Type.Map(GetPlayerMap(index)).Shop)
            End If
        End If

        SpawnMapEventsFor(index, GetPlayerMap(index))
        SendJoinMap(index)
        TempPlayer(index).GettingMap = 0
    End Sub

    Sub Packet_RespawnMap(index As Integer, ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        ' Clear out it all
        For i = 1 To MAX_MAP_ITEMS
            SpawnItemSlot(i, 0, 0, GetPlayerMap(index), MapItem(GetPlayerMap(index), i).X, MapItem(GetPlayerMap(index), i).Y)
            ClearMapItem(i, GetPlayerMap(index))
        Next

        ' Respawn
        SpawnMapItems(GetPlayerMap(index))

        ' Respawn NPCS
        For i = 1 To MAX_MAP_NPCS
            SpawnNpc(i, GetPlayerMap(index))
        Next

        SpawnMapEventsFor(index, GetPlayerMap(index))
        SpawnGlobalEvents(GetPlayerMap(index))

        CacheResources(GetPlayerMap(index))
        PlayerMsg(index, "Map respawned.", ColorType.BrightGreen)
        Addlog(GetPlayerName(index) & " has respawned map #" & GetPlayerMap(index), ADMIN_LOG)

        buffer.Dispose()
    End Sub

    Sub Packet_KickPlayer(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Moderator Then
            Exit Sub
        End If

        ' The player index
        n = FindPlayer(buffer.ReadString)
        buffer.Dispose()

        If n <> index Then
            If n > 0 Then
                If GetPlayerAccess(n) < GetPlayerAccess(index) Then
                    GlobalMsg(GetPlayerName(n) & " has been kicked from " & Settings.GameName & " by " & GetPlayerName(index) & "!")
                    Addlog(GetPlayerName(index) & " has kicked " & GetPlayerName(n) & ".", ADMIN_LOG)
                    AlertMsg(n, DialogueMsg.Kicked, MenuType.Login)
                Else
                    PlayerMsg(index, "That is a higher or same access admin then you!", ColorType.BrightRed)
                End If
            Else
                PlayerMsg(index, "Player is not online.", ColorType.BrightRed)
            End If
        Else
            PlayerMsg(index, "You cannot kick yourself!", ColorType.BrightRed)
        End If
    End Sub

    Sub Packet_Banlist(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Moderator Then
            Exit Sub
        End If

        PlayerMsg(index, "Command /banlist is not available.", ColorType.Yellow)
    End Sub

    Sub Packet_DestroyBans(index As Integer, ByRef data() As Byte)
        Dim filename As String

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Owner Then Exit Sub

        filename = System.IO.Path.Combine(Core.Path.Database, "banlist.txt")

        If File.Exists(filename) Then Kill(filename)

        PlayerMsg(index, "Ban list destroyed.", ColorType.BrightGreen)
    End Sub

    Sub Packet_BanPlayer(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Moderator Then Exit Sub

        ' The player index
        n = FindPlayer(buffer.ReadString)
        buffer.Dispose()

        If n <> index Then
            If n > 0 Then
                If GetPlayerAccess(n) < GetPlayerAccess(index) Then
                    BanIndex(n, index)
                Else
                    PlayerMsg(index, "That is a higher or same access admin then you!", ColorType.BrightRed)
                End If
            Else
                PlayerMsg(index, "Player is not online.", ColorType.BrightRed)
            End If
        Else
            PlayerMsg(index, "You cannot ban yourself!", ColorType.BrightRed)
        End If

    End Sub

    Private Sub Packet_EditMapRequest(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        If GetPlayerMap(index) > MAX_MAPS Then
            PlayerMsg(index, "Cant edit instanced maps!", ColorType.BrightRed)
            Exit Sub
        End If

        Dim user As String

        user = IsEditorLocked(index, EditorType.Map)

        If user <> "" Then
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        SendNpcs(index)
        SendItems(index)
        SendAnimations(index)
        SendShops(index)
        SendResources(index)
        SendMapEventData(index)
        SendMorals(index)

        TempPlayer(index).Editor = EditorType.Map

        Dim Buffer As New ByteStream(4)
        Buffer.WriteInt32(ServerPackets.SEditMap)

        Socket.SendDataTo(index, Buffer.Data, Buffer.Head)
        Buffer.Dispose()
    End Sub

    Sub Packet_EditShop(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Shop)

        If user <> "" Then
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        TempPlayer(index).Editor = EditorType.Shop

        SendItems(index)
        SendShops(index)

        Dim Buffer = New ByteStream(4)
        Buffer.WriteInt32(ServerPackets.SShopEditor)
        Socket.SendDataTo(index, Buffer.Data, Buffer.Head)

        Buffer.Dispose()
    End Sub

    Sub Packet_SaveShop(index As Integer, ByRef data() As Byte)
        Dim ShopNum As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        ShopNum = buffer.ReadInt32()

        ' Prevent hacking
        If ShopNum <= 0 Or ShopNum > MAX_SHOPS Then Exit Sub

        Type.Shop(ShopNum).BuyRate = buffer.ReadInt32()
        Type.Shop(ShopNum).Name = buffer.ReadString()

        For i = 1 To MAX_TRADES
            Type.Shop(ShopNum).TradeItem(i).CostItem = buffer.ReadInt32()
            Type.Shop(ShopNum).TradeItem(i).CostValue = buffer.ReadInt32()
            Type.Shop(ShopNum).TradeItem(i).Item = buffer.ReadInt32()
            Type.Shop(ShopNum).TradeItem(i).ItemValue = buffer.ReadInt32()
        Next

        buffer.Dispose()

        ' Save it
        SendUpdateShopToAll(ShopNum)
        SaveShop(ShopNum)
        Addlog(GetPlayerLogin(index) & " saving shop #" & ShopNum & ".", ADMIN_LOG)
    End Sub

    Sub Packet_EditSkill(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Skill)

        If user <> "" Then
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        TempPlayer(index).Editor = EditorType.Skill

        SendJobs(index)
        SendProjectiles(index)
        SendAnimations(index)
        SendSkills(index)

        Dim Buffer = New ByteStream(4)
        Buffer.WriteInt32(ServerPackets.SSkillEditor)
        Socket.SendDataTo(index, Buffer.Data, Buffer.Head)

        Buffer.Dispose()
    End Sub

    Sub Packet_SaveSkill(index As Integer, ByRef data() As Byte)
        Dim skillnum As Integer
        Dim buffer As New ByteStream(data)

        skillnum = buffer.ReadInt32

        ' Prevent hacking
        If skillnum <= 0 Or skillnum > MAX_SKILLS Then Exit Sub

        Type.Skill(skillnum).AccessReq = buffer.ReadInt32()
        Type.Skill(skillnum).AoE = buffer.ReadInt32()
        Type.Skill(skillnum).CastAnim = buffer.ReadInt32()
        Type.Skill(skillnum).CastTime = buffer.ReadInt32()
        Type.Skill(skillnum).CdTime = buffer.ReadInt32()
        Type.Skill(skillnum).JobReq = buffer.ReadInt32()
        Type.Skill(skillnum).Dir = buffer.ReadInt32()
        Type.Skill(skillnum).Duration = buffer.ReadInt32()
        Type.Skill(skillnum).Icon = buffer.ReadInt32()
        Type.Skill(skillnum).Interval = buffer.ReadInt32()
        Type.Skill(skillnum).IsAoE = buffer.ReadInt32()
        Type.Skill(skillnum).LevelReq = buffer.ReadInt32()
        Type.Skill(skillnum).Map = buffer.ReadInt32()
        Type.Skill(skillnum).MpCost = buffer.ReadInt32()
        Type.Skill(skillnum).Name = buffer.ReadString()
        Type.Skill(skillnum).Range = buffer.ReadInt32()
        Type.Skill(skillnum).SkillAnim = buffer.ReadInt32()
        Type.Skill(skillnum).StunDuration = buffer.ReadInt32()
        Type.Skill(skillnum).Type = buffer.ReadInt32()
        Type.Skill(skillnum).Vital = buffer.ReadInt32()
        Type.Skill(skillnum).X = buffer.ReadInt32()
        Type.Skill(skillnum).Y = buffer.ReadInt32()

        'projectiles
        Type.Skill(skillnum).IsProjectile = buffer.ReadInt32()
        Type.Skill(skillnum).Projectile = buffer.ReadInt32()

        Type.Skill(skillnum).KnockBack = buffer.ReadInt32()
        Type.Skill(skillnum).KnockBackTiles = buffer.ReadInt32()

        ' Save it
        SendUpdateSkillToAll(skillnum)
        SaveSkill(skillnum)
        Addlog(GetPlayerLogin(index) & " saved Skill #" & skillnum & ".", ADMIN_LOG)

        buffer.Dispose()
    End Sub

    Sub Packet_SetAccess(index As Integer, ByRef data() As Byte)
        Dim buffer As New ByteStream(data)
        Dim n As Integer
        Dim i As Integer

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Owner Then Exit Sub

        ' The index
        n = FindPlayer(buffer.ReadString)

        ' The access
        i = buffer.ReadInt32

        ' Check for invalid access level
        If i >= 1 Or i <= 5 Then

            ' Check if player is on
            If n > 0 Then

                'check to see if same level access is trying to change another access of the very same level and boot them if they are.
                If GetPlayerAccess(n) = GetPlayerAccess(index) Then
                    PlayerMsg(index, "Invalid access level.", ColorType.BrightRed)
                    Exit Sub
                End If

                If GetPlayerAccess(n) <= 0 Then
                    GlobalMsg(GetPlayerName(n) & " has been blessed with administrative access.")
                End If

                SetPlayerAccess(n, i)
                SendPlayerData(n)
                Addlog(GetPlayerName(index) & " has modified " & GetPlayerName(n) & "'s access.", ADMIN_LOG)
            Else
                PlayerMsg(index, "Player is not online.", ColorType.BrightRed)
            End If
        Else
            PlayerMsg(index, "Invalid access level.", ColorType.BrightRed)
        End If

        buffer.Dispose()
    End Sub

    Sub Packet_WhosOnline(index As Integer, ByRef data() As Byte)
        SendWhosOnline(index)
    End Sub

    Sub Packet_SetMotd(index As Integer, ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        Settings.Welcome = buffer.ReadString
        Settings.Save()

        GlobalMsg("Welcome changed to: " & Settings.Welcome)
        Addlog(GetPlayerName(index) & " changed welcome to: " & Settings.Welcome, ADMIN_LOG)

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerSearch(index As Integer, ByRef data() As Byte)
        Dim rclick As Byte
        Dim x As Integer, y As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        x = buffer.ReadInt32
        y = buffer.ReadInt32
        rclick = buffer.ReadInt32

        ' Prevent subscript out of range
        If x < 0 Or x > Type.Map(GetPlayerMap(index)).MaxX Or y < 0 Or y > Type.Map(GetPlayerMap(index)).MaxY Then Exit Sub

        ' Check for a player
        For i = 1 To Socket.HighIndex()

            If GetPlayerMap(index) = GetPlayerMap(i) Then
                If GetPlayerX(i) = x Then
                    If GetPlayerY(i) = y Then

                        ' Consider the player
                        If i <> index Then
                            If GetPlayerLevel(i) >= GetPlayerLevel(index) + 5 Then
                                PlayerMsg(index, "You wouldn't stand a chance.", ColorType.BrightRed)
                            Else

                                If GetPlayerLevel(i) > GetPlayerLevel(index) Then
                                    PlayerMsg(index, "This one seems to have an advantage over you.", ColorType.Yellow)
                                Else

                                    If GetPlayerLevel(i) = GetPlayerLevel(index) Then
                                        PlayerMsg(index, "This would be an even fight.", ColorType.White)
                                    Else

                                        If GetPlayerLevel(index) >= GetPlayerLevel(i) + 5 Then
                                            PlayerMsg(index, "You could slaughter that player.", ColorType.BrightBlue)
                                        Else

                                            If GetPlayerLevel(index) > GetPlayerLevel(i) Then
                                                PlayerMsg(index, "You would have an advantage over that player.", ColorType.BrightCyan)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If

                        ' Change target
                        If TempPlayer(index).Target = 0 Or i <> TempPlayer(index).Target Then
                            TempPlayer(index).Target = i
                            TempPlayer(index).TargetType = TargetType.Player
                        Else
                            TempPlayer(index).Target = 0
                            TempPlayer(index).TargetType = 0
                        End If

                        PlayerMsg(index, "Your target is now " & GetPlayerName(i) & ".", ColorType.Yellow)
                        SendTarget(index, TempPlayer(index).Target, TempPlayer(index).TargetType)
                        If rclick = 1 Then SendRightClick(index)
                        Exit Sub
                    End If
                End If
            End If

        Next

        ' Check for an item
        For i = 1 To MAX_MAP_ITEMS

            If Type.MapItem(GetPlayerMap(index), i).Num > 0 Then
                If Type.Item(Type.MapItem(GetPlayerMap(index), i).Num).Name <> "" Then
                    If Type.MapItem(GetPlayerMap(index), i).X = x Then
                        If Type.MapItem(GetPlayerMap(index), i).Y = y Then
                            PlayerMsg(index, "You see " & CheckGrammar(Type.Item(Type.MapItem(GetPlayerMap(index), i).Num).Name) & ".", ColorType.BrightGreen)
                            Exit Sub
                        End If
                    End If
                End If
            End If
        Next

        ' Check for an npc
        For i = 1 To MAX_MAP_NPCS
            If MapNPC(GetPlayerMap(index)).NPC(i).Num > 0 Then
                If MapNPC(GetPlayerMap(index)).NPC(i).X = x Then
                    If MapNPC(GetPlayerMap(index)).NPC(i).Y = y Then
                        ' Change target
                        If TempPlayer(index).Target = 0 Then
                            TempPlayer(index).Target = i
                            TempPlayer(index).TargetType = TargetType.NPC
                        Else
                            TempPlayer(index).Target = 0
                            TempPlayer(index).TargetType = 0
                        End If
                        PlayerMsg(index, "Your target is now " & CheckGrammar(Type.NPC(MapNPC(GetPlayerMap(index)).NPC(i).Num).Name) & ".", ColorType.Yellow)
                        SendTarget(index, TempPlayer(index).Target, TempPlayer(index).TargetType)
                        Exit Sub
                    End If
                End If
            End If

        Next

        buffer.Dispose()
    End Sub

    Sub Packet_Skills(index As Integer, ByRef data() As Byte)
        SendPlayerSkills(index)
    End Sub

    Sub Packet_Cast(index As Integer, ByRef data() As Byte)
        Dim n As Integer
        Dim buffer As New ByteStream(data)

        ' Skill slot
        n = buffer.ReadInt32
        buffer.Dispose()

        If Type.Map(GetPlayerMap(index)).Moral > 0 Then
            If Type.Moral(Type.Map(GetPlayerMap(index)).Moral).CanCast Then
                ' set the skill buffer before casting
                BufferSkill(index, n)
            End If
        End If
    End Sub

    Sub Packet_QuitGame(index As Integer, ByRef data() As Byte)
        SendLeftGame(index)
        LeftGame(index)
    End Sub

    Sub Packet_SwapInvSlots(index As Integer, ByRef data() As Byte)
        Dim oldSlot As Integer, newSlot As Integer
        Dim buffer As New ByteStream(data)

        If TempPlayer(index).InTrade > 0 Or TempPlayer(index).InBank Or TempPlayer(index).InShop Then Exit Sub

        ' Old Slot
        oldSlot = buffer.ReadInt32
        newSlot = buffer.ReadInt32
        buffer.Dispose()

        PlayerSwitchInvSlots(index, oldSlot, newSlot)

        buffer.Dispose()
    End Sub

    Sub Packet_SwapSkillSlots(index As Integer, ByRef data() As Byte)
        Dim oldSlot As Integer, newSlot As Integer
        Dim buffer As New ByteStream(data)

        If TempPlayer(index).InTrade > 0 Or TempPlayer(index).InBank Or TempPlayer(index).InShop Then Exit Sub

        ' Old Slot
        oldSlot = buffer.ReadInt32
        newSlot = buffer.ReadInt32
        buffer.Dispose()

        PlayerSwitchSkillSlots(index, oldSlot, newSlot)

        buffer.Dispose()
    End Sub

    Sub Packet_CheckPing(index As Integer, ByRef data() As Byte)
        Dim buffer As ByteStream
        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SSendPing)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub Packet_Unequip(index As Integer, ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        PlayerUnequipItem(index, buffer.ReadInt32)

        buffer.Dispose()
    End Sub

    Sub Packet_RequestPlayerData(index As Integer, ByRef data() As Byte)
        SendPlayerData(index)
    End Sub

    Sub Packet_RequestNPC(index As Integer, ByRef data() As Byte)
        Dim Buffer As New ByteStream(data), n As Integer

        n = Buffer.ReadInt32

        If n <= 0 Or n > MAX_NPCS Then Exit Sub

        SendUpdateNpcTo(index, n)
    End Sub

    Sub Packet_SpawnItem(index As Integer, ByRef data() As Byte)
        Dim tmpItem As Integer
        Dim tmpAmount As Integer
        Dim buffer As New ByteStream(data)

        ' item
        tmpItem = buffer.ReadInt32
        tmpAmount = buffer.ReadInt32

        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        SpawnItem(tmpItem, tmpAmount, GetPlayerMap(index), GetPlayerX(index), GetPlayerY(index))
        buffer.Dispose()
    End Sub

    Sub Packet_TrainStat(index As Integer, ByRef data() As Byte)
        Dim tmpstat As Integer
        Dim buffer As New ByteStream(data)

        ' check points
        If GetPlayerPoints(index) = 0 Then Exit Sub

        ' stat
        tmpstat = buffer.ReadInt32

        ' increment stat
        SetPlayerStat(index, tmpstat, GetPlayerRawStat(index, tmpstat) + 1)

        ' decrement points
        SetPlayerPoints(index, GetPlayerPoints(index) - 1)

        ' send player new data
        SendPlayerData(index)
        buffer.Dispose()
    End Sub

    Sub Packet_RequestSkill(index As Integer, ByRef data() As Byte)
        Dim Buffer = New ByteStream(data), n As Integer

        n = Buffer.ReadInt32

        If n <= 0 Or n > MAX_SKILLS Then Exit Sub

        SendUpdateSkillTo(index, n)
    End Sub

    Sub Packet_RequestShop(index As Integer, ByRef data() As Byte)
        Dim Buffer As New ByteStream(data), n As Integer

        n = Buffer.ReadInt32

        If n <= 0 Or n > MAX_SHOPS Then Exit Sub

        SendUpdateShopTo(index, n)
    End Sub

    Sub Packet_RequestLevelUp(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        SetPlayerExp(index, GetPlayerNextLevel(index))
        CheckPlayerLevelUp(index)
    End Sub

    Sub Packet_ForgetSkill(index As Integer, ByRef data() As Byte)
        Dim skillslot As Integer
        Dim buffer As New ByteStream(data)

        skillslot = buffer.ReadInt32

        ' Check for subscript out of range
        If skillslot < 0 Or skillslot > MAX_PLAYER_SKILLS Then Exit Sub

        ' dont let them forget a skill which is in CD
        If TempPlayer(index).SkillCd(skillslot) > 0 Then
            PlayerMsg(index, "Cannot forget a skill which is cooling down!", ColorType.BrightRed)
            Exit Sub
        End If

        ' dont let them forget a skill which is buffered
        If TempPlayer(index).SkillBuffer = skillslot Then
            PlayerMsg(index, "Cannot forget a skill which you are casting!", ColorType.BrightRed)
            Exit Sub
        End If

        Type.Player(index).Skill(skillslot).Num = 0
        SendPlayerSkills(index)

        buffer.Dispose()
    End Sub

    Sub Packet_CloseShop(index As Integer, ByRef data() As Byte)
        TempPlayer(index).InShop = 0
    End Sub

    Sub Packet_BuyItem(index As Integer, ByRef data() As Byte)
        Dim shopslot As Integer, shopnum As Integer, itemamount As Integer
        Dim buffer As New ByteStream(data)

        shopslot = buffer.ReadInt32

        ' not in shop, exit out
        shopnum = TempPlayer(index).InShop
        If shopnum <= 0 Or shopnum > MAX_SHOPS Then Exit Sub

        With Type.Shop(shopnum).TradeItem(shopslot)
            ' check trade exists
            If .Item < 0 Then Exit Sub

            ' check has the cost item
            itemamount = HasItem(index, .CostItem)
            If itemamount = 0 Or itemamount < .CostValue Then
                PlayerMsg(index, "You do not have enough to buy this item.", ColorType.BrightRed)
                ResetShopAction(index)
                Exit Sub
            End If

            ' it's fine, let's go ahead
            For i = 1 To .CostValue
                TakeInv(index, .CostItem, .CostValue)
            Next
            GiveInv(index, .Item, .ItemValue)
        End With

        ' send confirmation message & reset their shop action
        PlayerMsg(index, "Trade successful.", ColorType.BrightGreen)
        ResetShopAction(index)

        buffer.Dispose()
    End Sub

    Sub Packet_SellItem(index As Integer, ByRef data() As Byte)
        Dim invSlot As Integer
        Dim itemNum As Integer
        Dim price As Integer
        Dim multiplier As Double
        Dim buffer As New ByteStream(data)

        invSlot = buffer.ReadInt32

        ' if invalid, exit out
        If invSlot <= 0 Or invSlot > MAX_INV Then Exit Sub

        ' has item?
        If GetPlayerInv(index, invSlot) <= 0 Or GetPlayerInv(index, invSlot) > MAX_ITEMS Then Exit Sub

        ' seems to be valid
        itemNum = GetPlayerInv(index, invSlot)

        ' work out price
        multiplier = Type.Shop(TempPlayer(index).InShop).BuyRate / 100
        price = Type.Item(itemNum).Price * multiplier

        ' item has cost?
        If price <= 0 Then
            PlayerMsg(index, "The shop doesn't want that item.", ColorType.Yellow)
            ResetShopAction(index)
            Exit Sub
        End If

        ' take item and give gold
        TakeInv(index, itemNum, 1)
        GiveInv(index, 1, price)

        ' send confirmation message & reset their shop action
        PlayerMsg(index, "Sold the " & Type.Item(GetPlayerInv(index, invSlot)).Name & " for " & price & " " & Type.Item(itemNum).Name & "!", ColorType.BrightGreen)
        ResetShopAction(index)

        buffer.Dispose()
    End Sub

    Sub Packet_ChangeBankSlots(index As Integer, ByRef data() As Byte)
        Dim oldslot As Integer, newslot As Integer
        Dim buffer As New ByteStream(data)

        oldslot = buffer.ReadInt32
        newslot = buffer.ReadInt32

        PlayerSwitchBankSlots(index, oldslot, newslot)

        buffer.Dispose()
    End Sub

    Sub Packet_DepositItem(index As Integer, ByRef data() As Byte)
        Dim invslot As Integer, amount As Integer
        Dim buffer As New ByteStream(data)

        invslot = buffer.ReadInt32
        amount = buffer.ReadInt32

        GiveBank(index, invslot, amount)

        buffer.Dispose()
    End Sub

    Sub Packet_WithdrawItem(index As Integer, ByRef data() As Byte)
        Dim bankSlot As Integer, amount As Integer
        Dim buffer As New ByteStream(data)

        bankSlot = buffer.ReadInt32
        amount = buffer.ReadInt32

        TakeBank(index, bankSlot, amount)

        buffer.Dispose()
    End Sub

    Sub Packet_CloseBank(index As Integer, ByRef data() As Byte)
        TempPlayer(index).InBank = 0
    End Sub

    Sub Packet_AdminWarp(index As Integer, ByRef data() As Byte)
        Dim x As Integer, y As Integer
        Dim buffer As New ByteStream(data)

        x = buffer.ReadInt32
        y = buffer.ReadInt32

        If GetPlayerAccess(index) >= AccessType.Mapper Then
            ' Set the information
            SetPlayerX(index, x)
            SetPlayerY(index, y)

            ' send the stuff
            SendPlayerXY(index)
        End If

        buffer.Dispose()
    End Sub

    Sub Packet_TradeInvite(index As Integer, ByRef data() As Byte)
        Dim Name As String, tradetarget As Integer
        Dim buffer As New ByteStream(data)

        Name = buffer.ReadString

        buffer.Dispose()

        ' Check for a player
        tradetarget = FindPlayer(Name)

        If tradetarget <= 0 Or tradetarget > MAX_PLAYERS Then Exit Sub

        ' can't trade with yourself..
        If tradetarget = index Then
            PlayerMsg(index, "You can't trade with yourself!", ColorType.BrightRed)
            Exit Sub
        End If

        ' send the trade request
        TempPlayer(index).TradeRequest = tradetarget
        TempPlayer(tradetarget).TradeRequest = index

        PlayerMsg(tradetarget, GetPlayerName(index) & " has invited you to trade.", ColorType.Yellow)
        PlayerMsg(index, "You have invited " & GetPlayerName(tradetarget) & " to trade.", ColorType.BrightGreen)

        SendTradeInvite(tradetarget, index)
    End Sub

    Sub Packet_HandleTradeInvite(index As Integer, ByRef data() As Byte)
        Dim tradetarget As Integer, status As Byte
        Dim buffer As New ByteStream(data)

        status = buffer.ReadInt32

        buffer.Dispose()

        tradetarget = TempPlayer(index).TradeRequest

        If status = 0 Then
            PlayerMsg(tradetarget, GetPlayerName(index) & " has declined your trade request.", ColorType.BrightRed)
            PlayerMsg(index, "You have declined the trade with " & GetPlayerName(tradetarget) & ".", ColorType.BrightRed)
            TempPlayer(index).TradeRequest = 0
            Exit Sub
        End If

        ' Let them trade!
        If TempPlayer(tradetarget).TradeRequest = index Then
            ' let them know they're trading
            PlayerMsg(index, "You have accepted " & GetPlayerName(tradetarget) & "'s trade request.", ColorType.Yellow)
            PlayerMsg(tradetarget, GetPlayerName(index) & " has accepted your trade request.", ColorType.BrightGreen)

            ' clear the tradeRequest server-side
            TempPlayer(index).TradeRequest = 0
            TempPlayer(tradetarget).TradeRequest = 0

            ' set that they're trading with each other
            TempPlayer(index).InTrade = tradetarget
            TempPlayer(tradetarget).InTrade = index

            ' clear out their trade offers
            ReDim TempPlayer(index).TradeOffer(MAX_INV)
            ReDim TempPlayer(tradetarget).TradeOffer(MAX_INV)
            For i = 1 To MAX_INV
                TempPlayer(index).TradeOffer(i).Num = 0
                TempPlayer(index).TradeOffer(i).Value = 0
                TempPlayer(tradetarget).TradeOffer(i).Num = 0
                TempPlayer(tradetarget).TradeOffer(i).Value = 0
            Next

            ' Used to init the trade window clientside
            SendTrade(index, tradetarget)
            SendTrade(tradetarget, index)

            ' Send the offer data - Used to clear their client
            SendTradeUpdate(index, 0)
            SendTradeUpdate(index, 1)
            SendTradeUpdate(tradetarget, 0)
            SendTradeUpdate(tradetarget, 1)
        End If
    End Sub

    Sub Packet_TradeInviteDecline(index As Integer, ByRef data() As Byte)
        TempPlayer(index).TradeRequest = 0
    End Sub

    Sub Packet_AcceptTrade(index As Integer, ByRef data() As Byte)
        Dim itemNum As Integer
        Dim tradeTarget As Integer, i As Integer
        Dim tmpTradeItem(MAX_INV) As PlayerInvStruct
        Dim tmpTradeItem2(MAX_INV) As PlayerInvStruct

        TempPlayer(index).AcceptTrade = 1

        tradeTarget = TempPlayer(index).InTrade

        ' if not both of them accept, then exit
        If Not TempPlayer(tradeTarget).AcceptTrade Then
            SendTradeStatus(index, 2)
            SendTradeStatus(tradeTarget, 1)
            Exit Sub
        End If

        ' take their items
        For i = 1 To MAX_INV
            ' player
            If TempPlayer(index).TradeOffer(i).Num > 0 Then
                itemNum = Type.Player(index).Inv(TempPlayer(index).TradeOffer(i).Num).Num
                If itemNum > 0 Then
                    ' store temp
                    tmpTradeItem(i).Num = itemNum
                    tmpTradeItem(i).Value = TempPlayer(index).TradeOffer(i).Value
                    ' take item
                    TakeInvSlot(index, TempPlayer(index).TradeOffer(i).Num, tmpTradeItem(i).Value)
                End If
            End If
            ' target
            If TempPlayer(tradeTarget).TradeOffer(i).Num > 0 Then
                itemNum = GetPlayerInv(tradeTarget, TempPlayer(tradeTarget).TradeOffer(i).Num)
                If itemNum > 0 Then
                    ' store temp
                    tmpTradeItem2(i).Num = itemNum
                    tmpTradeItem2(i).Value = TempPlayer(tradeTarget).TradeOffer(i).Value
                    ' take item
                    TakeInvSlot(tradeTarget, TempPlayer(tradeTarget).TradeOffer(i).Num, tmpTradeItem2(i).Value)
                End If
            End If
        Next

        ' taken all items. now they can't not get items because of no inventory space.
        For i = 1 To MAX_INV
            ' player
            If tmpTradeItem2(i).Num > 0 Then
                ' give away!
                GiveInv(index, tmpTradeItem2(i).Num, tmpTradeItem2(i).Value, False)
            End If
            ' target
            If tmpTradeItem(i).Num > 0 Then
                ' give away!
                GiveInv(tradeTarget, tmpTradeItem(i).Num, tmpTradeItem(i).Value, False)
            End If
        Next

        SendInventory(index)
        SendInventory(tradeTarget)

        ' they now have all the items. Clear out values + let them out of the trade.
        For i = 1 To MAX_INV
            TempPlayer(index).TradeOffer(i).Num = 0
            TempPlayer(index).TradeOffer(i).Value = 0
            TempPlayer(tradeTarget).TradeOffer(i).Num = 0
            TempPlayer(tradeTarget).TradeOffer(i).Value = 0
        Next

        TempPlayer(index).InTrade = 0
        TempPlayer(tradeTarget).InTrade = 0

        PlayerMsg(index, "Trade completed.", ColorType.BrightGreen)
        PlayerMsg(tradeTarget, "Trade completed.", ColorType.BrightGreen)

        SendCloseTrade(index)
        SendCloseTrade(tradeTarget)
    End Sub

    Sub Packet_DeclineTrade(index As Integer, ByRef data() As Byte)
        Dim tradeTarget As Integer

        tradeTarget = TempPlayer(index).InTrade

        For i = 1 To MAX_INV
            TempPlayer(index).TradeOffer(i).Num = 0
            TempPlayer(index).TradeOffer(i).Value = 0
            TempPlayer(tradeTarget).TradeOffer(i).Num = 0
            TempPlayer(tradeTarget).TradeOffer(i).Value = 0
        Next

        TempPlayer(index).InTrade = 0
        TempPlayer(tradeTarget).InTrade = 0

        PlayerMsg(index, "You declined the trade.", ColorType.BrightRed)
        PlayerMsg(tradeTarget, GetPlayerName(index) & " has declined the trade.", ColorType.BrightRed)

        SendCloseTrade(index)
        SendCloseTrade(tradeTarget)
    End Sub

    Sub Packet_TradeItem(index As Integer, ByRef data() As Byte)
        Dim itemnum As Integer
        Dim invslot As Integer, amount As Integer, emptyslot As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        invslot = buffer.ReadInt32
        amount = buffer.ReadInt32

        buffer.Dispose()

        If invslot <= 0 Or invslot > MAX_INV Then Exit Sub

        itemnum = GetPlayerInv(index, invslot)

        If itemnum <= 0 Or itemnum > MAX_ITEMS Then Exit Sub

        ' make sure they have the amount they offer
        If amount < 0 Or amount > GetPlayerInvValue(index, invslot) Then Exit Sub

        If Type.Item(itemnum).Type = ItemType.Currency Or Type.Item(itemnum).Stackable = 1 Then

            ' check if already offering same currency item
            For i = 1 To MAX_INV

                If TempPlayer(index).TradeOffer(i).Num = invslot Then
                    ' add amount
                    TempPlayer(index).TradeOffer(i).Value = TempPlayer(index).TradeOffer(i).Value + amount

                    ' clamp to limits
                    If TempPlayer(index).TradeOffer(i).Value > GetPlayerInvValue(index, invslot) Then
                        TempPlayer(index).TradeOffer(i).Value = GetPlayerInvValue(index, invslot)
                    End If

                    ' cancel any trade agreement
                    TempPlayer(index).AcceptTrade = 0
                    TempPlayer(TempPlayer(index).InTrade).AcceptTrade = 0

                    SendTradeStatus(index, 0)
                    SendTradeStatus(TempPlayer(index).InTrade, 0)

                    SendTradeUpdate(index, 0)
                    SendTradeUpdate(TempPlayer(index).InTrade, 1)
                    Exit Sub
                End If
            Next
        Else
            ' make sure they're not already offering it
            For i = 1 To MAX_INV
                If TempPlayer(index).TradeOffer(i).Num = invslot Then
                    PlayerMsg(index, "You've already offered this item.", ColorType.BrightRed)
                    Exit Sub
                End If
            Next
        End If

        ' not already offering - find earliest empty slot
        For i = 1 To MAX_INV
            If TempPlayer(index).TradeOffer(i).Num = 0 Then
                emptyslot = i
                Exit For
            End If
        Next
        TempPlayer(index).TradeOffer(emptyslot).Num = invslot
        TempPlayer(index).TradeOffer(emptyslot).Value = amount

        ' cancel any trade agreement and send new data
        TempPlayer(index).AcceptTrade = 0
        TempPlayer(TempPlayer(index).InTrade).AcceptTrade = 0

        SendTradeStatus(index, 0)
        SendTradeStatus(TempPlayer(index).InTrade, 0)

        SendTradeUpdate(index, 0)
        SendTradeUpdate(TempPlayer(index).InTrade, 1)
    End Sub

    Sub Packet_UntradeItem(index As Integer, ByRef data() As Byte)
        Dim tradeslot As Integer
        Dim buffer As New ByteStream(data)

        tradeslot = buffer.ReadInt32

        buffer.Dispose()

        If tradeslot <= 0 Or tradeslot > MAX_INV Then Exit Sub
        If TempPlayer(index).TradeOffer(tradeslot).Num <= 0 Then Exit Sub

        TempPlayer(index).TradeOffer(tradeslot).Num = 0
        TempPlayer(index).TradeOffer(tradeslot).Value = 0

        If TempPlayer(index).AcceptTrade Then TempPlayer(index).AcceptTrade = 0
        If TempPlayer(TempPlayer(index).InTrade).AcceptTrade Then TempPlayer(TempPlayer(index).InTrade).AcceptTrade = 0

        SendTradeStatus(index, 0)
        SendTradeStatus(TempPlayer(index).InTrade, 0)

        SendTradeUpdate(index, 0)
        SendTradeUpdate(TempPlayer(index).InTrade, 1)
    End Sub

    Sub HackingAttempt(index As Integer, Reason As String)

        If index > 0 And IsPlaying(index) Then
            GlobalMsg(GetPlayerLogin(index) & "/" & GetPlayerName(index) & " has been booted for (" & Reason & ")")

            AlertMsg(index, DialogueMsg.Connection, MenuType.Login)
        End If

    End Sub

    Sub Packet_MapReport(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        SendMapReport(index)
    End Sub

    Sub Packet_Admin(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        SendAdminPanel(index)
    End Sub

    Sub Packet_SetHotbarSlot(index As Integer, ByRef data() As Byte)
        Dim newSlot As Integer, skill As Integer, type As Byte, oldSlot As Integer
        Dim buffer As New ByteStream(data)

        type = buffer.ReadInt32
        newSlot = buffer.ReadInt32
        oldSlot = buffer.ReadInt32
        skill = buffer.ReadInt32

        If newSlot < 1 Or newSlot > MAX_HOTBAR Then Exit Sub
        If type = PartOriginType.Hotbar Then
            If oldSlot < 1 Or oldSlot > MAX_HOTBAR Then Exit Sub

            Core.Player(index).Hotbar(newSlot).Slot = skill
            Core.Player(index).Hotbar(newSlot).SlotType = Core.Player(index).Hotbar(oldSlot).SlotType

            Core.Player(index).Hotbar(oldSlot).Slot = 0
            Core.Player(index).Hotbar(oldSlot).SlotType = 0
        Else
            Core.Player(index).Hotbar(newSlot).Slot = skill
            Core.Player(index).Hotbar(newSlot).SlotType = type
        End If

        SendHotbar(index)

        buffer.Dispose()
    End Sub

    Sub Packet_DeleteHotbarSlot(index As Integer, ByRef data() As Byte)
        Dim slot As Integer
        Dim buffer As New ByteStream(data)

        slot = buffer.ReadInt32

        If slot < 1 Or slot > MAX_HOTBAR Then Exit Sub

        Core.Player(index).Hotbar(slot).Slot = 0
        Core.Player(index).Hotbar(slot).SlotType = 0

        SendHotbar(index)

        buffer.Dispose()
    End Sub

    Sub Packet_UseHotbarSlot(index As Integer, ByRef data() As Byte)
        Dim slot As Integer
        Dim buffer As New ByteStream(data)

        slot = buffer.ReadInt32
        buffer.Dispose()

        If slot < 1 Or slot > MAX_HOTBAR Then Exit Sub

        If Core.Player(index).Hotbar(slot).Slot > 0 Then
            If Core.Player(index).Hotbar(slot).SlotType = PartType.Item Then
                UseItem(index, FindItemSlot(index, Core.Player(index).Hotbar(slot).Slot))
            End If
        End If

        SendHotbar(index)
    End Sub

    Sub Packet_SkillLearn(index As Integer, ByRef data() As Byte)
        Dim skillNum As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        skillNum = buffer.ReadInt32()

        PlayerLearnSkill(index, 0, skillNum)
    End Sub

    Sub Packet_RequestEditJob(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Job)

        If user <> "" Then
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        SendItems(index)
        SendJobs(index)

        TempPlayer(index).Editor = EditorType.Job

        SendJobs(index)

        SendClassEditor(index)
    End Sub

    Sub Packet_SaveJob(index As Integer, ByRef data() As Byte)
        Dim i As Integer, z As Integer, x As Integer, jobNum As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        jobNum = buffer.ReadInt32

        With Type.Job(jobNum)
            .Name = buffer.ReadString
            .Desc = buffer.ReadString

            .MaleSprite = buffer.ReadInt32
            .FemaleSprite = buffer.ReadInt32

            For x = 1 To StatType.Count - 1
                .Stat(x) = buffer.ReadInt32()
            Next

            For q = 1 To 5
                .StartItem(q) = buffer.ReadInt32
                .StartValue(q) = buffer.ReadInt32
            Next

            .StartMap = buffer.ReadInt32
            .StartX = buffer.ReadByte
            .StartY = buffer.ReadInt32
            .BaseExp = buffer.ReadInt32
        End With

        buffer.Dispose()

        SaveJobs()
        SendJobToAll(index)
    End Sub

    Private Sub Packet_Emote(index As Integer, ByRef data() As Byte)
        Dim Emote As Integer
        Dim buffer As New ByteStream(data)

        Emote = buffer.ReadInt32

        SendEmote(index, Emote)

        buffer.Dispose()
    End Sub

    Private Sub Packet_CloseEditor(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Mapper Then Exit Sub

        If TempPlayer(index).Editor = -1 Then Exit Sub

        TempPlayer(index).Editor = -1
    End Sub

End Module