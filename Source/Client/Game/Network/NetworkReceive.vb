Imports System.Threading.Channels
Imports Core
Imports Mirage.Sharp.Asfw
Imports Mirage.Sharp.Asfw.IO

Module NetworkReceive

    Sub PacketRouter()
        Socket.PacketId(ServerPackets.SAlertMsg) = AddressOf Packet_AlertMsg
        Socket.PacketId(ServerPackets.SKeyPair) = AddressOf Packet_KeyPair
        Socket.PacketId(ServerPackets.SLoginOK) = AddressOf Packet_LoginOk
        Socket.PacketId(ServerPackets.SPlayerChars) = AddressOf Packet_PlayerChars
        Socket.PacketId(ServerPackets.SUpdateJob) = AddressOf Packet_UpdateJob
        Socket.PacketId(ServerPackets.SJobData) = AddressOf Packet_JobData
        Socket.PacketId(ServerPackets.SInGame) = AddressOf Packet_InGame
        Socket.PacketId(ServerPackets.SPlayerInv) = AddressOf Packet_PlayerInv
        Socket.PacketId(ServerPackets.SPlayerInvUpdate) = AddressOf Packet_PlayerInvUpdate
        Socket.PacketId(ServerPackets.SPlayerWornEq) = AddressOf Packet_PlayerWornEquipment
        Socket.PacketId(ServerPackets.SPlayerHP) = AddressOf Packet_PlayerHP
        Socket.PacketId(ServerPackets.SPlayerSP) = AddressOf Packet_PlayerSP
        Socket.PacketId(ServerPackets.SPlayerStats) = AddressOf Packet_PlayerStats
        Socket.PacketId(ServerPackets.SPlayerData) = AddressOf Packet_PlayerData
        Socket.PacketId(ServerPackets.SPlayerMove) = AddressOf Packet_PlayerMove
        Socket.PacketId(ServerPackets.SNpcMove) = AddressOf Packet_NpcMove
        Socket.PacketId(ServerPackets.SPlayerDir) = AddressOf Packet_PlayerDir
        Socket.PacketId(ServerPackets.SNpcDir) = AddressOf Packet_NpcDir
        Socket.PacketId(ServerPackets.SPlayerXY) = AddressOf Packet_PlayerXY
        Socket.PacketId(ServerPackets.SAttack) = AddressOf Packet_Attack
        Socket.PacketId(ServerPackets.SNpcAttack) = AddressOf Packet_NpcAttack
        Socket.PacketId(ServerPackets.SCheckForMap) = AddressOf Packet_CheckMap
        Socket.PacketId(ServerPackets.SMapData) = AddressOf Packet_MapData
        Socket.PacketId(ServerPackets.SMapNPCData) = AddressOf Packet_MapNPCData
        Socket.PacketId(ServerPackets.SMapNPCUpdate) = AddressOf Packet_MapNPCUpdate
        Socket.PacketId(ServerPackets.SMapDone) = AddressOf Packet_MapDone
        Socket.PacketId(ServerPackets.SGlobalMsg) = AddressOf Packet_GlobalMsg
        Socket.PacketId(ServerPackets.SAdminMsg) = AddressOf Packet_AdminMsg
        Socket.PacketId(ServerPackets.SPlayerMsg) = AddressOf Packet_PlayerMsg
        Socket.PacketId(ServerPackets.SMapMsg) = AddressOf Packet_MapMsg
        Socket.PacketId(ServerPackets.SSpawnItem) = AddressOf Packet_SpawnItem
        Socket.PacketId(ServerPackets.SUpdateItem) = AddressOf Packet_UpdateItem
        Socket.PacketId(ServerPackets.SSpawnNpc) = AddressOf Packet_SpawnNPC
        Socket.PacketId(ServerPackets.SNpcDead) = AddressOf Packet_NpcDead
        Socket.PacketId(ServerPackets.SUpdateNpc) = AddressOf Packet_UpdateNPC
        Socket.PacketId(ServerPackets.SEditMap) = AddressOf Packet_EditMap
        Socket.PacketId(ServerPackets.SUpdateShop) = AddressOf Packet_UpdateShop
        Socket.PacketId(ServerPackets.SUpdateSkill) = AddressOf Packet_UpdateSkill
        Socket.PacketId(ServerPackets.SSkills) = AddressOf Packet_Skills
        Socket.PacketId(ServerPackets.SLeftMap) = AddressOf Packet_LeftMap
        Socket.PacketId(ServerPackets.SMapResource) = AddressOf Packet_MapResource
        Socket.PacketId(ServerPackets.SUpdateResource) = AddressOf Packet_UpdateResource
        Socket.PacketId(ServerPackets.SSendPing) = AddressOf Packet_Ping
        Socket.PacketId(ServerPackets.SActionMsg) = AddressOf Packet_ActionMessage
        Socket.PacketId(ServerPackets.SPlayerEXP) = AddressOf Packet_PlayerExp
        Socket.PacketId(ServerPackets.SBlood) = AddressOf Packet_Blood
        Socket.PacketId(ServerPackets.SUpdateAnimation) = AddressOf Packet_UpdateAnimation
        Socket.PacketId(ServerPackets.SAnimation) = AddressOf Packet_Animation
        Socket.PacketId(ServerPackets.SMapNPCVitals) = AddressOf Packet_NPCVitals
        Socket.PacketId(ServerPackets.SCooldown) = AddressOf Packet_Cooldown
        Socket.PacketId(ServerPackets.SClearSkillBuffer) = AddressOf Packet_ClearSkillBuffer
        Socket.PacketId(ServerPackets.SSayMsg) = AddressOf Packet_SayMessage
        Socket.PacketId(ServerPackets.SOpenShop) = AddressOf Packet_OpenShop
        Socket.PacketId(ServerPackets.SResetShopAction) = AddressOf Packet_ResetShopAction
        Socket.PacketId(ServerPackets.SStunned) = AddressOf Packet_Stunned
        Socket.PacketId(ServerPackets.SMapWornEq) = AddressOf Packet_MapWornEquipment
        Socket.PacketId(ServerPackets.SBank) = AddressOf Packet_OpenBank
        Socket.PacketId(ServerPackets.SLeftGame) = AddressOf Packet_LeftGame

        Socket.PacketId(ServerPackets.STradeInvite) = AddressOf Packet_TradeInvite
        Socket.PacketId(ServerPackets.STrade) = AddressOf Packet_Trade
        Socket.PacketId(ServerPackets.SCloseTrade) = AddressOf Packet_CloseTrade
        Socket.PacketId(ServerPackets.STradeUpdate) = AddressOf Packet_TradeUpdate
        Socket.PacketId(ServerPackets.STradeStatus) = AddressOf Packet_TradeStatus

        Socket.PacketId(ServerPackets.SGameData) = AddressOf Packet_GameData
        Socket.PacketId(ServerPackets.SMapReport) = AddressOf Packet_MapReport
        Socket.PacketId(ServerPackets.STarget) = AddressOf Packet_Target

        Socket.PacketId(ServerPackets.SAdmin) = AddressOf Packet_Admin
        Socket.PacketId(ServerPackets.SMapNames) = AddressOf Packet_MapNames

        Socket.PacketId(ServerPackets.SCritical) = AddressOf Packet_Critical
        Socket.PacketId(ServerPackets.SrClick) = AddressOf Packet_RClick

        Socket.PacketId(ServerPackets.SHotbar) = AddressOf Packet_Hotbar

        Socket.PacketId(ServerPackets.SSpawnEvent) = AddressOf Packet_SpawnEvent
        Socket.PacketId(ServerPackets.SEventMove) = AddressOf Packet_EventMove
        Socket.PacketId(ServerPackets.SEventDir) = AddressOf Packet_EventDir
        Socket.PacketId(ServerPackets.SEventChat) = AddressOf Packet_EventChat
        Socket.PacketId(ServerPackets.SEventStart) = AddressOf Packet_EventStart
        Socket.PacketId(ServerPackets.SEventEnd) = AddressOf Packet_EventEnd
        Socket.PacketId(ServerPackets.SPlayBGM) = AddressOf Packet_PlayBGM
        Socket.PacketId(ServerPackets.SPlaySound) = AddressOf Packet_PlaySound
        Socket.PacketId(ServerPackets.SFadeoutBGM) = AddressOf Packet_FadeOutBGM
        Socket.PacketId(ServerPackets.SStopSound) = AddressOf Packet_StopSound
        Socket.PacketId(ServerPackets.SSwitchesAndVariables) = AddressOf Packet_SwitchesAndVariables
        Socket.PacketId(ServerPackets.SMapEventData) = AddressOf Packet_MapEventData
        Socket.PacketId(ServerPackets.SChatBubble) = AddressOf Packet_ChatBubble
        Socket.PacketId(ServerPackets.SSpecialEffect) = AddressOf Packet_SpecialEffect

        Socket.PacketId(ServerPackets.SPic) = AddressOf Packet_Picture
        Socket.PacketId(ServerPackets.SHoldPlayer) = AddressOf Packet_HoldPlayer

        Socket.PacketId(ServerPackets.SUpdateProjectile) = AddressOf HandleUpdateProjectile
        Socket.PacketId(ServerPackets.SMapProjectile) = AddressOf HandleMapProjectile

        Socket.PacketId(ServerPackets.SEmote) = AddressOf Packet_Emote

        Socket.PacketId(ServerPackets.SPartyInvite) = AddressOf Packet_PartyInvite
        Socket.PacketId(ServerPackets.SPartyUpdate) = AddressOf Packet_PartyUpdate
        Socket.PacketId(ServerPackets.SPartyVitals) = AddressOf Packet_PartyVitals

        Socket.PacketId(ServerPackets.SUpdatePet) = AddressOf Packet_UpdatePet
        Socket.PacketId(ServerPackets.SUpdatePlayerPet) = AddressOf Packet_UpdatePlayerPet
        Socket.PacketId(ServerPackets.SPetMove) = AddressOf Packet_PetMove
        Socket.PacketId(ServerPackets.SPetDir) = AddressOf Packet_PetDir
        Socket.PacketId(ServerPackets.SPetVital) = AddressOf Packet_PetVital
        Socket.PacketId(ServerPackets.SClearPetSkillBuffer) = AddressOf Packet_ClearPetSkillBuffer
        Socket.PacketId(ServerPackets.SPetAttack) = AddressOf Packet_PetAttack
        Socket.PacketId(ServerPackets.SPetXY) = AddressOf Packet_PetXY
        Socket.PacketId(ServerPackets.SPetExp) = AddressOf Packet_PetExperience

        Socket.PacketId(ServerPackets.SClock) = AddressOf Packet_Clock
        Socket.PacketId(ServerPackets.STime) = AddressOf Packet_Time

        Socket.PacketId(ServerPackets.SItemEditor) = AddressOf Packet_EditItem
        Socket.PacketId(ServerPackets.SNpcEditor) = AddressOf Packet_NPCEditor
        Socket.PacketId(ServerPackets.SShopEditor) = AddressOf Packet_EditShop
        Socket.PacketId(ServerPackets.SSkillEditor) = AddressOf Packet_EditSkill
        Socket.PacketId(ServerPackets.SResourceEditor) = AddressOf Packet_ResourceEditor
        Socket.PacketId(ServerPackets.SAnimationEditor) = AddressOf Packet_EditAnimation
        Socket.PacketId(ServerPackets.SProjectileEditor) = AddressOf HandleProjectileEditor
        Socket.PacketId(ServerPackets.SJobEditor) = AddressOf Packet_ClassEditor
        Socket.PacketId(ServerPackets.SPetEditor) = AddressOf Packet_PetEditor
        Socket.PacketId(ServerPackets.SUpdateMoral) = AddressOf Packet_UpdateMoral
        Socket.PacketId(ServerPackets.SMoralEditor) = AddressOf Packet_EditMoral

    End Sub

    Private Sub Packet_AlertMsg(ByRef data() As Byte)
        Dim dialogueIndex As Integer, menuReset As Integer, kick As Integer
        Dim buffer As New ByteStream(data)

        dialogueIndex = buffer.ReadInt32
        menuReset = buffer.ReadInt32()
        kick = buffer.ReadInt32()

        If menuReset > 0 Then
            Gui.HideWindows()

            Select Case menuReset
                Case MenuType.Login
                    Gui.ShowWindow(Gui.GetWindowIndex("winLogin"))
                Case MenuType.Chars
                    Gui.ShowWindow(Gui.GetWindowIndex("winChars"))
                Case MenuType.Job
                    Gui.ShowWindow(Gui.GetWindowIndex("winJob"))
                Case MenuType.NewChar
                    Gui.ShowWindow(Gui.GetWindowIndex("winNewChar"))
                Case MenuType.Main
                    Gui.ShowWindow(Gui.GetWindowIndex("winLogin"))
                Case MenuType.Register
                    Gui.ShowWindow(Gui.GetWindowIndex("winRegister"))
            End Select
        Else
            If kick > 0 Or GameState.InGame = 0 Then
                Gui.ShowWindow(Gui.GetWindowIndex("winLogin"))
                InitNetwork()
                DialogueAlert(dialogueIndex)
                Exit Sub
            End If
        End If

        DialogueAlert(dialogueIndex)
        buffer.Dispose()
    End Sub

    Private Sub Packet_KeyPair(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        GameState.EKeyPair.ImportKeyString(buffer.ReadString())
        buffer.Dispose()
    End Sub

    Private Sub Packet_LoginOk(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        ' Now we can receive game data
        GameState.MyIndex = buffer.ReadInt32

        buffer.Dispose()
    End Sub

    Sub Packet_PlayerChars(ByRef data() As Byte)
        Dim buffer As New ByteStream(data), I As Long, winNum As Long, conNum As Long, isSlotEmpty(MAX_CHARS) As Boolean, x As Long

        Type.Setting.Username = Gui.Windows(Gui.GetWindowIndex("winLogin")).Controls(Gui.GetControlIndex("winLogin", "txtUsername")).Text
        Type.Setting.Save()

        For I = 1 To MAX_CHARS
            GameState.CharName(I) = buffer.ReadString
            GameState.CharSprite(I) = buffer.ReadInt32
            GameState.CharAccess(I) = buffer.ReadInt32
            GameState.CharJob(I) = buffer.ReadInt32

            ' set as empty or not
            If Not Len(GameState.CharName(I)) > 0 Then isSlotEmpty(I) = 1
        Next

        buffer.Dispose()

        Gui.HideWindows()
        Gui.ShowWindow(Gui.GetWindowIndex("winChars"))

        ' set GUI window up
        winNum = Gui.GetWindowIndex("winChars")
        For I = 1 To MAX_CHARS
            conNum = Gui.GetControlIndex("winChars", "lblCharName_" & I)
            With Gui.Windows(winNum).Controls(conNum)
                If Not isSlotEmpty(I) Then
                    .Text = GameState.CharName(I)
                Else
                    .Text = "Blank Slot"
                End If
            End With

            ' hide/show buttons
            If isSlotEmpty(I) Then
                ' create button
                conNum = Gui.GetControlIndex("winChars", "btnCreateChar_" & I)
                Gui.Windows(winNum).Controls(conNum).Visible = 1
                ' select button
                conNum = Gui.GetControlIndex("winChars", "btnSelectChar_" & I)
                Gui.Windows(winNum).Controls(conNum).Visible = 0
                ' delete button
                conNum = Gui.GetControlIndex("winChars", "btnDelChar_" & I)
                Gui.Windows(winNum).Controls(conNum).Visible = 0
            Else
                ' create button
                conNum = Gui.GetControlIndex("winChars", "btnCreateChar_" & I)
                Gui.Windows(winNum).Controls(conNum).Visible = 0
                ' select button
                conNum = Gui.GetControlIndex("winChars", "btnSelectChar_" & I)
                Gui.Windows(winNum).Controls(conNum).Visible = 1
                ' delete button
                conNum = Gui.GetControlIndex("winChars", "btnDelChar_" & I)
                Gui.Windows(winNum).Controls(conNum).Visible = 1
            End If
        Next
    End Sub

    Sub Packet_UpdateJob(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32()
        buffer.WriteInt32(i)

        With Type.Job(i)
            .Name = buffer.ReadString
            .Desc = buffer.ReadString

            .MaleSprite = buffer.ReadInt32
            .FemaleSprite = buffer.ReadInt32

            For q = 1 To StatType.Count - 1
                .Stat(q) = buffer.ReadInt32
            Next

            For q = 1 To 5
                .StartItem(q) = buffer.ReadInt32
                .StartValue(q) = buffer.ReadInt32
            Next

            .StartMap = buffer.ReadInt32
            .StartX = buffer.ReadByte
            .StartY = buffer.ReadByte
            .BaseExp = buffer.ReadInt32
        End With

        buffer.Dispose()
    End Sub

    Sub Packet_JobData(ByRef data() As Byte)
        Dim i As Integer, x As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_JOBS
            With Type.Job(i)
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
                .StartY = buffer.ReadByte
                .BaseExp = buffer.ReadInt32
            End With

        Next

        buffer.Dispose()
    End Sub

    Private Sub Packet_InGame(ByRef data() As Byte)
        GameState.InMenu = 0
        GameState.InGame = 1
        Gui.HideWindows()
        GameState.CanMoveNow = 1
        GameState.MyEditorType = -1

        ' show gui
        Gui.ShowWindow(Gui.GetWindowIndex("winHotbar"), , False)
        Gui.ShowWindow(Gui.GetWindowIndex("winMenu"), , False)
        Gui.ShowWindow(Gui.GetWindowIndex("winBars"), , False)
        HideChat()

        GameInit()
    End Sub

    Private Sub Packet_PlayerInv(ByRef data() As Byte)
        Dim i As Integer, invNum As Integer, amount As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_INV
            invNum = buffer.ReadInt32
            amount = buffer.ReadInt32
            SetPlayerInv(GameState.MyIndex, i, invNum)
            SetPlayerInvValue(GameState.MyIndex, i, amount)
        Next

        SetGoldLabel

        buffer.Dispose()
    End Sub

    Private Sub Packet_PlayerInvUpdate(ByRef data() As Byte)
        Dim n As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        n = buffer.ReadInt32()

        SetPlayerInv(GameState.MyIndex, n, buffer.ReadInt32)
        SetPlayerInvValue(GameState.MyIndex, n, buffer.ReadInt32)

        SetGoldLabel

        buffer.Dispose()
    End Sub

    Private Sub Packet_PlayerWornEquipment(ByRef data() As Byte)
        Dim i As Integer, n As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To EquipmentType.Count - 1
            n = buffer.ReadInt32
            SetPlayerEquipment(GameState.MyIndex, n, i)
        Next

        buffer.Dispose()
    End Sub

    Private Sub Packet_NpcMove(ByRef data() As Byte)
        Dim mapNpcNum As Integer, movement As Integer
        Dim x As Integer, y As Integer, dir As Integer
        Dim buffer As New ByteStream(data)

        mapNpcNum = buffer.ReadInt32
        x = buffer.ReadInt32
        y = buffer.ReadInt32
        dir = buffer.ReadInt32
        movement = buffer.ReadInt32

        With MyMapNPC(MapNPCNum)
            .X = x
            .Y = y
            .Dir = dir
            .XOffset = 0
            .YOffset = 0
            .Moving = movement

            Select Case .Dir
                Case DirectionType.Up
                    .YOffset = GameState.PicY
                Case DirectionType.Down
                    .YOffset = GameState.PicY * -1
                Case DirectionType.Left
                    .XOffset = GameState.PicX
                Case DirectionType.Right
                    .XOffset = GameState.PicX * -1
            End Select
        End With

        buffer.Dispose()
    End Sub

    Private Sub Packet_NpcDir(ByRef data() As Byte)
        Dim dir As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32
        dir = buffer.ReadInt32

        With Type.MyMapNPC(i)
            .Dir = dir
            .XOffset = 0
            .YOffset = 0
            .Moving = 0
        End With

        buffer.Dispose()
    End Sub

    Private Sub Packet_Attack(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32

        ' Set player to attacking
        Type.Player(i).Attacking = 1
        Type.Player(i).AttackTimer = GetTickCount()

        buffer.Dispose()
    End Sub

    Private Sub Packet_NpcAttack(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32

        ' Set npc to attacking
        Type.MyMapNPC(i).Attacking = 1
        Type.MyMapNPC(i).AttackTimer = GetTickCount()

        buffer.Dispose()
    End Sub

    Private Sub Packet_GlobalMsg(ByRef data() As Byte)
        Dim msg As String
        Dim buffer As New ByteStream(data)

        msg = buffer.ReadString

        buffer.Dispose()

        AddText(msg, ColorType.Yellow, , ChatChannel.Broadcast)
    End Sub

    Private Sub Packet_MapMsg(ByRef data() As Byte)
        Dim msg As String
        Dim buffer As New ByteStream(data)

        msg = buffer.ReadString

        buffer.Dispose()

        AddText(msg, ColorType.White, , ChatChannel.Map)

    End Sub

    Private Sub Packet_AdminMsg(ByRef data() As Byte)
        Dim msg As String
        Dim buffer As New ByteStream(data)

        msg = buffer.ReadString

        buffer.Dispose()

        AddText(msg, ColorType.BrightCyan, , ChatChannel.Broadcast)
    End Sub

    Private Sub Packet_PlayerMsg(ByRef data() As Byte)
        Dim msg As String, color As Integer
        Dim buffer As New ByteStream(data)

        msg = buffer.ReadString
        color = buffer.ReadInt32

        buffer.Dispose()

        AddText(msg, color, , ChatChannel.Player)
    End Sub

    Private Sub Packet_SpawnItem(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32

        With MyMapItem(i)
            .Num = buffer.ReadInt32
            .Value = buffer.ReadInt32
            .X = buffer.ReadInt32
            .Y = buffer.ReadInt32
        End With

        buffer.Dispose()
    End Sub

    Private Sub Packet_SpawnNPC(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32

        With Type.MyMapNPC(i)
            .Num = buffer.ReadInt32
            .X = buffer.ReadInt32
            .Y = buffer.ReadInt32
            .Dir = buffer.ReadInt32

            For i = 1 To VitalType.Count - 1
                .Vital(i) = buffer.ReadInt32
            Next
            ' Client use only
            .XOffset = 0
            .YOffset = 0
            .Moving = 0
        End With

        buffer.Dispose()
    End Sub

    Private Sub Packet_NpcDead(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32
        ClearMapNPC(i)

        buffer.Dispose()
    End Sub

    Private Sub Packet_UpdateNPC(ByRef data() As Byte)
        Dim i As Integer, x As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32

        ' Update the Npc
        Type.NPC(i).Animation = buffer.ReadInt32()
        Type.NPC(i).AttackSay = buffer.ReadString()
        Type.NPC(i).Behaviour = buffer.ReadByte()

        For x = 1 To MAX_DROP_ITEMS
            Type.NPC(i).DropChance(x) = buffer.ReadInt32()
            Type.NPC(i).DropItem(x) = buffer.ReadInt32()
            Type.NPC(i).DropItemValue(x) = buffer.ReadInt32()
        Next

        Type.NPC(i).Exp = buffer.ReadInt32()
        Type.NPC(i).Faction = buffer.ReadByte()
        Type.NPC(i).HP = buffer.ReadInt32()
        Type.NPC(i).Name = buffer.ReadString()
        Type.NPC(i).Range = buffer.ReadByte()
        Type.NPC(i).SpawnTime = buffer.ReadByte()
        Type.NPC(i).SpawnSecs = buffer.ReadInt32()
        Type.NPC(i).Sprite = buffer.ReadInt32()

        For x = 1 To StatType.Count - 1
            Type.NPC(i).Stat(x) = buffer.ReadByte()
        Next

        For x = 1 To MAX_NPC_SKILLS
            Type.NPC(i).Skill(x) = buffer.ReadByte()
        Next

        Type.NPC(i).Level = buffer.ReadInt32()
        Type.NPC(i).Damage = buffer.ReadInt32()

        buffer.Dispose()
    End Sub

    Private Sub Packet_UpdateSkill(ByRef data() As Byte)
        Dim skillnum As Integer
        Dim buffer As New ByteStream(data)
        skillnum = buffer.ReadInt32

        Type.Skill(skillNum).AccessReq = buffer.ReadInt32()
        Type.Skill(skillNum).AoE = buffer.ReadInt32()
        Type.Skill(skillNum).CastAnim = buffer.ReadInt32()
        Type.Skill(skillNum).CastTime = buffer.ReadInt32()
        Type.Skill(skillNum).CdTime = buffer.ReadInt32()
        Type.Skill(skillNum).JobReq = buffer.ReadInt32()
        Type.Skill(skillNum).Dir = buffer.ReadInt32()
        Type.Skill(skillNum).Duration = buffer.ReadInt32()
        Type.Skill(skillNum).Icon = buffer.ReadInt32()
        Type.Skill(skillNum).Interval = buffer.ReadInt32()
        Type.Skill(skillNum).IsAoE = buffer.ReadInt32()
        Type.Skill(skillNum).LevelReq = buffer.ReadInt32()
        Type.Skill(skillNum).Map = buffer.ReadInt32()
        Type.Skill(skillNum).MpCost = buffer.ReadInt32()
        Type.Skill(skillNum).Name = buffer.ReadString()
        Type.Skill(skillNum).Range = buffer.ReadInt32()
        Type.Skill(skillNum).SkillAnim = buffer.ReadInt32()
        Type.Skill(skillNum).StunDuration = buffer.ReadInt32()
        Type.Skill(skillNum).Type = buffer.ReadInt32()
        Type.Skill(skillNum).Vital = buffer.ReadInt32()
        Type.Skill(skillNum).X = buffer.ReadInt32()
        Type.Skill(skillNum).Y = buffer.ReadInt32()

        Type.Skill(skillNum).IsProjectile = buffer.ReadInt32()
        Type.Skill(skillNum).Projectile = buffer.ReadInt32()

        Type.Skill(skillNum).KnockBack = buffer.ReadInt32()
        Type.Skill(skillNum).KnockBackTiles = buffer.ReadInt32()

        buffer.Dispose()

    End Sub

    Private Sub Packet_Skills(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_PLAYER_SKILLS
            Type.Player(GameState.MyIndex).Skill(i).Num = buffer.ReadInt32
        Next

        buffer.Dispose()
    End Sub

    Private Sub Packet_LeftMap(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        ClearPlayer(buffer.ReadInt32)

        buffer.Dispose()
    End Sub

    Private Sub Packet_Ping(ByRef data() As Byte)
        GameState.PingEnd = GetTickCount()
        GameState.Ping = GameState.PingEnd - GameState.PingStart
    End Sub

    Private Sub Packet_ActionMessage(ByRef data() As Byte)
        Dim x As Integer, y As Integer, message As String, color As Integer, tmpType As Integer
        Dim buffer As New ByteStream(data)

        message = buffer.ReadString
        color = buffer.ReadInt32
        tmpType = buffer.ReadInt32
        x = buffer.ReadInt32
        y = buffer.ReadInt32

        buffer.Dispose()

        CreateActionMsg(message, color, tmpType, x, y)
    End Sub

    Private Sub Packet_Blood(ByRef data() As Byte)
        Dim x As Integer, y As Integer, sprite As Integer
        Dim buffer As New ByteStream(data)

        x = buffer.ReadInt32
        y = buffer.ReadInt32

        ' randomise sprite
        sprite = Rand(1, 3)

        GameState.BloodIndex = GameState.BloodIndex + 1
        If GameState.BloodIndex >= Byte.MaxValue Then GameState.BloodIndex = 1

        With Blood(GameState.BloodIndex)
            .X = x
            .Y = y
            .Sprite = sprite
            .Timer = GetTickCount()
        End With

        buffer.Dispose()
    End Sub
    Private Sub Packet_NPCVitals(ByRef data() As Byte)
        Dim mapNpcNum As Integer
        Dim buffer As New ByteStream(data)

        mapNpcNum = buffer.ReadInt32
        For i = 1 To VitalType.Count - 1
            MyMapNPC(MapNPCNum).Vital(i) = buffer.ReadInt32
        Next

        buffer.Dispose()
    End Sub

    Private Sub Packet_Cooldown(ByRef data() As Byte)
        Dim slot As Integer
        Dim buffer As New ByteStream(data)

        slot = buffer.ReadInt32
        Type.Player(GameState.MyIndex).Skill(slot).CD = GetTickCount()

        buffer.Dispose()
    End Sub

    Private Sub Packet_ClearSkillBuffer(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        GameState.SkillBuffer = 0
        GameState.SkillBufferTimer = 0

        buffer.Dispose()
    End Sub

    Private Sub Packet_SayMessage(ByRef data() As Byte)
        Dim access As Integer, name As String, message As String
        Dim header As String, pk As Integer, channelType As Byte, color As Byte
        Dim buffer As New ByteStream(data)

        name = buffer.ReadString
        access = buffer.ReadInt32
        pk = buffer.ReadInt32
        message = buffer.ReadString
        header = buffer.ReadString

        ' Check access level
        Select Case access
            Case AccessType.Player
                color = ColorType.White
            Case AccessType.Moderator
                color = ColorType.Cyan
            Case AccessType.Mapper
                color = ColorType.Green
            Case AccessType.Developer
                color = ColorType.BrightBlue
            Case AccessType.Owner
                color = ColorType.Yellow
            Case Else
                color = ColorType.White
        End Select

        If pk > 0 Then color = ColorType.BrightRed

        ' find channel
        channelType = 0
        Select Case header
            Case "[Map]"
                channelType = ChatChannel.Map
            Case "[Global]"
                channelType = ChatChannel.Broadcast
        End Select

        ' add to the chat box
        AddText(header & name & ": " & message, color, , channelType)

        buffer.Dispose()
    End Sub

    Private Sub Packet_Stunned(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        GameState.StunDuration = buffer.ReadInt32

        buffer.Dispose()
    End Sub

    Private Sub Packet_MapWornEquipment(ByRef data() As Byte)
        Dim playernum As Integer, n As Integer
        Dim buffer As New ByteStream(data)

        playernum = buffer.ReadInt32
        For i = 1 To EquipmentType.Count - 1
            n = buffer.ReadInt32
            SetPlayerEquipment(playernum, n, i)
        Next

        buffer.Dispose()
    End Sub

    Private Sub Packet_GameData(ByRef data() As Byte)
        Dim n As Integer, i As Integer, z As Integer, x As Integer
        Dim buffer As New ByteStream(Compression.DecompressBytes(data))

        For i = 1 To MAX_JOBS
            With Type.Job(i)
                ReDim .Stat(StatType.Count - 1)

                .Name = buffer.ReadString
                .Desc = buffer.ReadString

                .MaleSprite = buffer.ReadInt32
                .FemaleSprite = buffer.ReadInt32

                For q = 1 To StatType.Count - 1
                    .Stat(q) = buffer.ReadInt32
                Next

                For q = 1 To 5
                    .StartItem(q) = buffer.ReadInt32
                    .StartValue(q) = buffer.ReadInt32
                Next

                .StartMap = buffer.ReadInt32
                .StartX = buffer.ReadByte
                .StartY = buffer.ReadByte

                .BaseExp = buffer.ReadInt32
            End With

        Next

        i = 0
        x = 0
        n = 0
        z = 0

        x = buffer.ReadInt32

        For i = 0 To x
            n = buffer.ReadInt32

            ' Update the item
            Type.Item(n).AccessReq = buffer.ReadInt32()

            For z = 1 To StatType.Count - 1
                Type.Item(n).Add_Stat(z) = buffer.ReadInt32()
            Next

            Type.Item(n).Animation = buffer.ReadInt32()
            Type.Item(n).BindType = buffer.ReadInt32()
            Type.Item(n).JobReq = buffer.ReadInt32()
            Type.Item(n).Data1 = buffer.ReadInt32()
            Type.Item(n).Data2 = buffer.ReadInt32()
            Type.Item(n).Data3 = buffer.ReadInt32()
            Type.Item(n).TwoHanded = buffer.ReadInt32()
            Type.Item(n).LevelReq = buffer.ReadInt32()
            Type.Item(n).Mastery = buffer.ReadInt32()
            Type.Item(n).Name = buffer.ReadString()
            Type.Item(n).Paperdoll = buffer.ReadInt32()
            Type.Item(n).Icon = buffer.ReadInt32()
            Type.Item(n).Price = buffer.ReadInt32()
            Type.Item(n).Rarity = buffer.ReadInt32()
            Type.Item(n).Speed = buffer.ReadInt32()

            Type.Item(n).Stackable = buffer.ReadInt32()
            Type.Item(n).Description = buffer.ReadString()

            For z = 1 To StatType.Count - 1
                Type.Item(n).Stat_Req(z) = buffer.ReadInt32()
            Next

            Type.Item(n).Type = buffer.ReadInt32()
            Type.Item(n).SubType = buffer.ReadInt32

            Type.Item(n).ItemLevel = buffer.ReadInt32

            Type.Item(n).KnockBack = buffer.ReadInt32()
            Type.Item(n).KnockBackTiles = buffer.ReadInt32()

            Type.Item(n).Projectile = buffer.ReadInt32()
            Type.Item(n).Ammo = buffer.ReadInt32()
        Next

        i = 0
        n = 0
        x = 0
        z = 0
        x = buffer.ReadInt32

        For i = 0 To x
            n = buffer.ReadInt32
            ' Update the Animation
            For z = 0 To UBound(Type.Animation(n).Frames) - 1
                Type.Animation(n).Frames(z) = buffer.ReadInt32()
            Next

            For z = 0 To UBound(Type.Animation(n).LoopCount) - 1
                Type.Animation(n).LoopCount(z) = buffer.ReadInt32()
            Next

            For z = 0 To UBound(Type.Animation(n).LoopTime) - 1
                Type.Animation(n).LoopTime(z) = buffer.ReadInt32()
            Next

            Type.Animation(n).Name = buffer.ReadString
            Type.Animation(n).Sound = buffer.ReadString

            For z = 0 To UBound(Type.Animation(n).Sprite) - 1
                Type.Animation(n).Sprite(z) = buffer.ReadInt32()
            Next
        Next

        i = 0
        n = 0
        x = 0
        z = 0

        x = buffer.ReadInt32
        For i = 0 To x
            n = buffer.ReadInt32
            ' Update the Npc
            Type.NPC(n).Animation = buffer.ReadInt32()
            Type.NPC(n).AttackSay = buffer.ReadString()
            Type.NPC(n).Behaviour = buffer.ReadByte()

            For z = 0 To 5
                Type.NPC(n).DropChance(z) = buffer.ReadInt32()
                Type.NPC(n).DropItem(z) = buffer.ReadInt32()
                Type.NPC(n).DropItemValue(z) = buffer.ReadInt32()
            Next

            Type.NPC(n).Exp = buffer.ReadInt32()
            Type.NPC(n).Faction = buffer.ReadByte()
            Type.NPC(n).HP = buffer.ReadInt32()
            Type.NPC(n).Name = buffer.ReadString()
            Type.NPC(n).Range = buffer.ReadByte()
            Type.NPC(n).SpawnTime = buffer.ReadByte()
            Type.NPC(n).SpawnSecs = buffer.ReadInt32()
            Type.NPC(n).Sprite = buffer.ReadInt32()

            For z = 1 To StatType.Count - 1
                Type.NPC(n).Stat(z) = buffer.ReadByte()
            Next

            ReDim Type.NPC(n).Skill(MAX_NPC_SKILLS)
            For z = 1 To MAX_NPC_SKILLS
                Type.NPC(n).Skill(z) = buffer.ReadByte()
            Next

            Type.NPC(i).Level = buffer.ReadInt32()
            Type.NPC(i).Damage = buffer.ReadInt32()
        Next

        i = 0
        n = 0
        x = 0
        z = 0

        x = buffer.ReadInt32

        For i = 0 To x
            n = buffer.ReadInt32

            Type.Shop(n).BuyRate = buffer.ReadInt32()
            Type.Shop(n).Name = buffer.ReadString()

            For z = 1 To MAX_TRADES
                Type.Shop(n).TradeItem(z).CostItem = buffer.ReadInt32()
                Type.Shop(n).TradeItem(z).CostValue = buffer.ReadInt32()
                Type.Shop(n).TradeItem(z).Item = buffer.ReadInt32()
                Type.Shop(n).TradeItem(z).ItemValue = buffer.ReadInt32()
            Next

        Next

        i = 0
        n = 0
        x = 0
        z = 0

        x = buffer.ReadInt32

        For i = 0 To x
            n = buffer.ReadInt32

            Type.Skill(n).AccessReq = buffer.ReadInt32()
            Type.Skill(n).AoE = buffer.ReadInt32()
            Type.Skill(n).CastAnim = buffer.ReadInt32()
            Type.Skill(n).CastTime = buffer.ReadInt32()
            Type.Skill(n).CdTime = buffer.ReadInt32()
            Type.Skill(n).JobReq = buffer.ReadInt32()
            Type.Skill(n).Dir = buffer.ReadInt32()
            Type.Skill(n).Duration = buffer.ReadInt32()
            Type.Skill(n).Icon = buffer.ReadInt32()
            Type.Skill(n).Interval = buffer.ReadInt32()
            Type.Skill(n).IsAoE = buffer.ReadInt32()
            Type.Skill(n).LevelReq = buffer.ReadInt32()
            Type.Skill(n).Map = buffer.ReadInt32()
            Type.Skill(n).MpCost = buffer.ReadInt32()
            Type.Skill(n).Name = buffer.ReadString()
            Type.Skill(n).Range = buffer.ReadInt32()
            Type.Skill(n).SkillAnim = buffer.ReadInt32()
            Type.Skill(n).StunDuration = buffer.ReadInt32()
            Type.Skill(n).Type = buffer.ReadInt32()
            Type.Skill(n).Vital = buffer.ReadInt32()
            Type.Skill(n).X = buffer.ReadInt32()
            Type.Skill(n).Y = buffer.ReadInt32()

            Type.Skill(n).IsProjectile = buffer.ReadInt32()
            Type.Skill(n).Projectile = buffer.ReadInt32()

            Type.Skill(n).KnockBack = buffer.ReadInt32()
            Type.Skill(n).KnockBackTiles = buffer.ReadInt32()

        Next

        i = 0
        x = 0
        n = 0
        z = 0

        x = buffer.ReadInt32

        For i = 0 To x
            n = buffer.ReadInt32

            Type.Resource(n).Animation = buffer.ReadInt32()
            Type.Resource(n).EmptyMessage = buffer.ReadString()
            Type.Resource(n).ExhaustedImage = buffer.ReadInt32()
            Type.Resource(n).Health = buffer.ReadInt32()
            Type.Resource(n).ExpReward = buffer.ReadInt32()
            Type.Resource(n).ItemReward = buffer.ReadInt32()
            Type.Resource(n).Name = buffer.ReadString()
            Type.Resource(n).ResourceImage = buffer.ReadInt32()
            Type.Resource(n).ResourceType = buffer.ReadInt32()
            Type.Resource(n).RespawnTime = buffer.ReadInt32()
            Type.Resource(n).SuccessMessage = buffer.ReadString()
            Type.Resource(n).LvlRequired = buffer.ReadInt32()
            Type.Resource(n).ToolRequired = buffer.ReadInt32()
            Type.Resource(n).Walkthrough = buffer.ReadInt32()
        Next

        i = 0
        n = 0
        x = 0
        z = 0

        buffer.Dispose()
    End Sub

    Private Sub Packet_Target(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        GameState.MyTarget = buffer.ReadInt32
        GameState.MyTargetType = buffer.ReadInt32

        buffer.Dispose()
    End Sub

    Private Sub Packet_MapReport(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_MAPS
            MapNames(i) = buffer.ReadString()
        Next
        
        buffer.Dispose()
    End Sub

    Private Sub Packet_Admin(ByRef data() As Byte)

    End Sub

    Private Sub Packet_MapNames(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)
        For i = 1 To MAX_MAPS
            MapNames(i) = buffer.ReadString()
        Next

        buffer.Dispose()
    End Sub
    
    Private Sub Packet_Critical(ByRef data() As Byte)
        GameState.ShakeTimerEnabled = 1
        GameState.ShakeTimer = GetTickCount()
    End Sub

    Private Sub Packet_RClick(ByRef data() As Byte)
 
    End Sub

    Private Sub Packet_Emote(ByRef data() As Byte)
        Dim index As Integer, emote As Integer
        Dim buffer As New ByteStream(data)
        index = buffer.ReadInt32
        emote = buffer.ReadInt32

        With Type.Player(index)
            .Emote = emote
            .EmoteTimer = GetTickCount() + 5000
        End With

        buffer.Dispose()

    End Sub

    Private Sub Packet_ChatBubble(ByRef data() As Byte)
        Dim targetType As Integer, target As Integer, message As String, Color As Integer
        Dim buffer As New ByteStream(data)

        target = buffer.ReadInt32
        targetType = buffer.ReadInt32
        message = buffer.ReadString
        Color = buffer.ReadInt32
        AddChatBubble(target, targetType, message, Color)

        buffer.Dispose()

    End Sub

    Private Sub Packet_LeftGame(ByRef data() As Byte)
        LogoutGame()
    End Sub

    '*****************
    '***  EDITORS  ***
    '*****************
    Private Sub Packet_EditAnimation(ByRef data() As Byte)

    End Sub

    Private Sub Packet_ClassEditor(ByRef data() As Byte)

    End Sub

    Sub Packet_EditItem(ByRef data() As Byte)
 
    End Sub

    Private Sub Packet_NPCEditor(ByRef data() As Byte)

    End Sub

    Private Sub Packet_ResourceEditor(ByRef data() As Byte)

    End Sub

    Friend Sub Packet_PetEditor(ByRef data() As Byte)

    End Sub

    Friend Sub HandleProjectileEditor(ByRef data() As Byte)
        GameState.InitProjectileEditor = 1
    End Sub

    Private Sub Packet_EditShop(ByRef data() As Byte)

    End Sub

    Private Sub Packet_EditSkill(ByRef data() As Byte)

    End Sub

    Private Sub Packet_Clock(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)
        Time.Instance.GameSpeed = buffer.ReadInt32()
        Time.Instance.Time = New Date(BitConverter.ToInt64(buffer.ReadBytes(), 0))

        buffer.Dispose()
    End Sub

    Private Sub Packet_Time(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        Time.Instance.TimeOfDay = buffer.ReadByte

        Select Case Time.Instance.TimeOfDay
            Case TimeOfDay.Dawn
                AddText("A chilling, refreshing, breeze has come with the morning.", ColorType.DarkGray)
                Exit Select

            Case TimeOfDay.Day
                AddText("Day has dawned in this region.", ColorType.DarkGray)
                Exit Select

            Case TimeOfDay.Dusk
                AddText("Dusk has begun darkening the skies...", ColorType.DarkGray)
                Exit Select

            Case Else
                AddText("Night has fallen upon the weary travelers.", ColorType.DarkGray)
                Exit Select
        End Select

        buffer.Dispose()
    End Sub

    Friend Sub Packet_Hotbar(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_Hotbar
            Type.Player(GameState.MyIndex).Hotbar(i).Slot = buffer.ReadInt32
            Type.Player(GameState.MyIndex).Hotbar(i).SlotType = buffer.ReadInt32
        Next

        buffer.Dispose()
    End Sub

    Sub Packet_EditMoral(ByRef data() As Byte)

    End Sub

     Sub Packet_UpdateMoral(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32()

        With Type.Moral(i)
            .Name = buffer.ReadString()
            .Color = buffer.ReadByte()
            .NPCBlock = buffer.ReadBoolean()
            .PlayerBlock = buffer.ReadBoolean()
            .DropItems = buffer.ReadBoolean()
            .CanCast = buffer.ReadBoolean()
            .CanDropItem = buffer.ReadBoolean()
            .CanPickupItem = buffer.ReadBoolean()
            .CanPK = buffer.ReadBoolean()
            .DropItems = buffer.ReadBoolean()
            .LoseExp = buffer.ReadBoolean()
        End With

        buffer.Dispose()
    End Sub

End Module