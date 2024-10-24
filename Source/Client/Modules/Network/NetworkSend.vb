
Imports Mirage.Sharp.Asfw
Imports Core
Imports System.Buffers
Imports System.Reflection

Module NetworkSend
    Friend Sub SendAddChar(name As String, sexNum As Integer, jobNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(Packets.ClientPackets.CAddChar)
        buffer.WriteInt32(CharNum)
        buffer.WriteString((name))
        buffer.WriteInt32(sexNum)
        buffer.WriteInt32(jobNum)
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Friend Sub SendUseChar(slot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(Packets.ClientPackets.CUseChar)
        buffer.WriteInt32(slot)
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Friend Sub SendDelChar(slot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(Packets.ClientPackets.CDelChar)
        buffer.WriteInt32(slot)
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Friend Sub SendLogin(name As String, pass As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CLogin)
        buffer.WriteString((EKeyPair.EncryptString(name)))
        buffer.WriteString((EKeyPair.EncryptString(pass)))
        
        ' Get the current executing assembly
        Dim assembly As Assembly = Assembly.GetExecutingAssembly()

        ' Retrieve the version information
        Dim version As Version = assembly.GetName().Version
        buffer.WriteString(EKeyPair.EncryptString(version.ToString()))
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Friend Sub SendRegister(name As String, pass As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRegister)
        buffer.WriteString((EKeyPair.EncryptString(name)))
        buffer.WriteString((EKeyPair.EncryptString(pass)))
        
        ' Get the current executing assembly
        Dim assembly As Assembly = Assembly.GetExecutingAssembly()

        ' Retrieve the version information
        Dim version As Version = assembly.GetName().Version
        buffer.WriteString(EKeyPair.EncryptString(version.ToString()))
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Public Sub btnLogin_Click()
        Dim user As String, pass As String

        With Windows(GetWindowIndex("winLogin"))
            user = .Controls(GetControlIndex("winLogin", "txtUsername")).Text
            pass = .Controls(GetControlIndex("winLogin", "txtPassword")).Text

            If Socket.IsConnected() Then
                SendLogin(user, pass)
            Else
                InitNetwork()
                Dialogue("Invalid Connection", "Cannot connect to game server.", "Please try again.", DialogueType.Alert)
            End If
        End With
    End Sub

    Sub GetPing()
        Dim buffer As New ByteStream(4)
        PingStart = GetTickCount()

        buffer.WriteInt32(ClientPackets.CCheckPing)
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Friend Sub SendPlayerMove()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPlayerMove)
        buffer.WriteInt32(GetPlayerDir(MyIndex))
        buffer.WriteInt32(Type.Player(MyIndex).Moving)
        buffer.WriteInt32(Type.Player(MyIndex).X)
        buffer.WriteInt32(Type.Player(MyIndex).Y)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SayMsg(text As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSayMsg)
        buffer.WriteString((text))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendKick(name As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CKickPlayer)
        buffer.WriteString((name))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendBan(name As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CBanPlayer)
        buffer.WriteString((name))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub WarpMeTo(name As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CWarpMeTo)
        buffer.WriteString((name))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub WarpToMe(name As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CWarpToMe)
        buffer.WriteString((name))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub WarpTo(MapNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CWarpTo)
        buffer.WriteInt32(MapNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestLevelUp()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestLevelUp)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendSpawnItem(tmpItem As Integer, tmpAmount As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSpawnItem)
        buffer.WriteInt32(tmpItem)
        buffer.WriteInt32(tmpAmount)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSetSprite(spriteNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSetSprite)
        buffer.WriteInt32(spriteNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSetAccess(name As String, access As Byte)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSetAccess)
        buffer.WriteString((name))
        buffer.WriteInt32(access)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendAttack()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CAttack)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendPlayerDir()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPlayerDir)
        buffer.WriteInt32(GetPlayerDir(MyIndex))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendRequestNPC(npcNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestNPC)
        buffer.WriteInt32(npcNum)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendRequestSkill(skillNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestSkill)
        buffer.WriteInt32(skillNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendTrainStat(statNum As Byte)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CTrainStat)
        buffer.WriteInt32(statNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendRequestPlayerData()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestPlayerData)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub BroadcastMsg(text As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CBroadcastMsg)
        buffer.WriteString(text)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub PlayerMsg(text As String, msgTo As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPlayerMsg)
        buffer.WriteString((msgTo))
        buffer.WriteString((text))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub AdminMsg(text As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CAdminMsg)
        buffer.WriteString(text)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendWhosOnline()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CWhosOnline)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendPlayerInfo(name As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPlayerInfoRequest)
        buffer.WriteString(name)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendMotdChange(welcome As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSetMotd)
        buffer.WriteString(welcome)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendBanList()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CBanList)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendBanDestroy()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CBanDestroy)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendChangeInvSlots(oldSlot As Integer, newSlot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSwapInvSlots)
        buffer.WriteInt32(oldSlot)
        buffer.WriteInt32(newSlot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendChangeSkillSlots(oldSlot As Integer, newSlot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSwapSkillSlots)
        buffer.WriteInt32(oldSlot)
        buffer.WriteInt32(newSlot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendUseItem(invNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CUseItem)
        buffer.WriteInt32(invNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendDropItem(invNum As Integer, amount As Integer)
        Dim buffer As New ByteStream(4)

        If InBank Or InShop > 0 Then Exit Sub

        ' do basic checks
        If invNum <= 0 Or invNum > MAX_INV Then Exit Sub
        If Type.Player(MyIndex).Inv(invNum).Num <= 0 Or Type.Player(MyIndex).Inv(invNum).Num > MAX_ITEMS Then Exit Sub
        If Type.Item(GetPlayerInv(MyIndex, invNum)).Type = ItemType.Currency Or Type.Item(GetPlayerInv(MyIndex, invNum)).Stackable = 1 Then
            If amount <= 0 Or amount > Type.Player(MyIndex).Inv(invNum).Value Then Exit Sub
        End If

        buffer.WriteInt32(ClientPackets.CMapDropItem)
        buffer.WriteInt32(invNum)
        buffer.WriteInt32(amount)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub PlayerSearch(curX As Integer, curY As Integer, rClick As Byte)
        Dim buffer As New ByteStream(4)

        If IsInBounds() Then
            buffer.WriteInt32(ClientPackets.CSearch)
            buffer.WriteInt32(curX)
            buffer.WriteInt32(curY)
            buffer.WriteInt32(rClick)
            Socket.SendData(buffer.Data, buffer.Head)
        End If

        buffer.Dispose()
    End Sub

    Friend Sub AdminWarp(x As Integer, y As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CAdminWarp)
        buffer.WriteInt32(x)
        buffer.WriteInt32(y)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendLeaveGame()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CQuit)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUnequip(eqNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CUnequip)
        buffer.WriteInt32(eqNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub ForgetSkill(skillslot As Integer)
        Dim buffer As New ByteStream(4)

        ' Check for subscript out of range
        If skillslot < 0 Or skillslot > MAX_PLAYER_SKILLS Then Exit Sub

        ' dont let them forget a skill which is in CD
        If Type.Player(MyIndex).Skill(skillslot).CD > 0 Then
            AddText("Cannot forget a skill which is cooling down!", ColorType.Red)
            Exit Sub
        End If

        ' dont let them forget a skill which is buffered
        If SkillBuffer = skillslot Then
            AddText("Cannot forget a skill which you are casting!", ColorType.Red)
            Exit Sub
        End If

        If Type.Player(MyIndex).Skill(skillslot).Num > 0 Then
            buffer.WriteInt32(ClientPackets.CForgetSkill)
            buffer.WriteInt32(skillslot)
            Socket.SendData(buffer.Data, buffer.Head)
        Else
            AddText("No skill found.", ColorType.Red)
        End If

        buffer.Dispose()
    End Sub

    Friend Sub SendRequestMapReport()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CMapReport)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestAdmin()
        If GetPlayerAccess(MyIndex) < AccessType.Moderator Then Exit Sub

        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CAdmin)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendUseEmote(emote As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CEmote)
        buffer.WriteInt32(emote)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestEditResource()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditResource)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSaveResource(ResourceNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveResource)

        buffer.WriteInt32(ResourceNum)
        buffer.WriteInt32(Type.Resource(ResourceNum).Animation)
        buffer.WriteString((Type.Resource(ResourceNum).EmptyMessage))
        buffer.WriteInt32(Type.Resource(ResourceNum).ExhaustedImage)
        buffer.WriteInt32(Type.Resource(ResourceNum).Health)
        buffer.WriteInt32(Type.Resource(ResourceNum).ExpReward)
        buffer.WriteInt32(Type.Resource(ResourceNum).ItemReward)
        buffer.WriteString((Type.Resource(ResourceNum).Name))
        buffer.WriteInt32(Type.Resource(ResourceNum).ResourceType)
        buffer.WriteInt32(Type.Resource(ResourceNum).RespawnTime)
        buffer.WriteString((Type.Resource(ResourceNum).SuccessMessage))
        buffer.WriteInt32(Type.Resource(ResourceNum).LvlRequired)
        buffer.WriteInt32(Type.Resource(ResourceNum).ToolRequired)
        buffer.WriteInt32(Type.Resource(ResourceNum).Walkthrough)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestEditNPC()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditNpc)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSaveNPC(NPCNum As Integer)
        Dim buffer As New ByteStream(4), i As Integer

        buffer.WriteInt32(ClientPackets.CSaveNpc)
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
        buffer.WriteString((Type.NPC(NPCNum).Name))
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

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestEditSkill()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditSkill)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSaveSkill(skillnum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveSkill)
        buffer.WriteInt32(skillnum)

        buffer.WriteInt32(Type.Skill(skillNum).AccessReq)
        buffer.WriteInt32(Type.Skill(skillNum).AoE)
        buffer.WriteInt32(Type.Skill(skillNum).CastAnim)
        buffer.WriteInt32(Type.Skill(skillNum).CastTime)
        buffer.WriteInt32(Type.Skill(skillNum).CdTime)
        buffer.WriteInt32(Type.Skill(skillNum).JobReq)
        buffer.WriteInt32(Type.Skill(skillNum).Dir)
        buffer.WriteInt32(Type.Skill(skillNum).Duration)
        buffer.WriteInt32(Type.Skill(skillNum).Icon)
        buffer.WriteInt32(Type.Skill(skillNum).Interval)
        buffer.WriteInt32(Type.Skill(skillNum).IsAoE)
        buffer.WriteInt32(Type.Skill(skillNum).LevelReq)
        buffer.WriteInt32(Type.Skill(skillNum).Map)
        buffer.WriteInt32(Type.Skill(skillNum).MpCost)
        buffer.WriteString((Type.Skill(skillNum).Name))
        buffer.WriteInt32(Type.Skill(skillNum).Range)
        buffer.WriteInt32(Type.Skill(skillNum).SkillAnim)
        buffer.WriteInt32(Type.Skill(skillNum).StunDuration)
        buffer.WriteInt32(Type.Skill(skillNum).Type)
        buffer.WriteInt32(Type.Skill(skillNum).Vital)
        buffer.WriteInt32(Type.Skill(skillNum).X)
        buffer.WriteInt32(Type.Skill(skillNum).Y)

        buffer.WriteInt32(Type.Skill(skillNum).IsProjectile)
        buffer.WriteInt32(Type.Skill(skillNum).Projectile)

        buffer.WriteInt32(Type.Skill(skillNum).KnockBack)
        buffer.WriteInt32(Type.Skill(skillNum).KnockBackTiles)

        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Friend Sub SendSaveShop(shopNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveShop)
        buffer.WriteInt32(shopnum)

        buffer.WriteInt32(Type.Shop(shopNum).BuyRate)
        buffer.WriteString(Type.Shop(shopNum).Name)

        For i = 1 To MAX_TRADES
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostItem)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostValue)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).Item)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).ItemValue)
        Next

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Friend Sub SendRequestEditShop()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditShop)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSaveAnimation(animationNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveAnimation)
        buffer.WriteInt32(animationNum)

        For i = 0 To UBound(Type.Animation(animationNum).Frames)
            buffer.WriteInt32(Type.Animation(animationNum).Frames(i))
        Next

        For i = 0 To UBound(Type.Animation(animationNum).LoopCount)
            buffer.WriteInt32(Type.Animation(animationNum).LoopCount(i))
        Next

        For i = 0 To UBound(Type.Animation(animationNum).LoopTime)
            buffer.WriteInt32(Type.Animation(animationNum).LoopTime(i))
        Next

        buffer.WriteString((Type.Animation(animationNum).Name))
        buffer.WriteString((Type.Animation(animationNum).Sound))

        For i = 0 To UBound(Type.Animation(animationNum).Sprite)
            buffer.WriteInt32(Type.Animation(animationNum).Sprite(i))
        Next

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestEditAnimation()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditAnimation)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestEditJob()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditJob)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSaveJob(jobNum As Integer)
        Dim i As Integer, q As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveJob)

        buffer.WriteInt32(jobNum)

        buffer.WriteString((Type.Job(jobNum).Name))
        buffer.WriteString((Type.Job(jobNum).Desc))

        buffer.WriteInt32(Type.Job(jobNum).MaleSprite)
        buffer.WriteInt32(Type.Job(jobNum).FemaleSprite)

        For i = 1 To StatType.Count - 1
            buffer.WriteInt32(Type.Job(jobNum).Stat(i))
        Next

        For q = 1 To 5
            buffer.WriteInt32(Type.Job(jobNum).StartItem(q))
            buffer.WriteInt32(Type.Job(jobNum).StartValue(q))
        Next

        buffer.WriteInt32(Type.Job(jobNum).StartMap)
        buffer.WriteByte(Type.Job(jobNum).StartX)
        buffer.WriteByte(Type.Job(jobNum).StartY)

        buffer.WriteInt32(Type.Job(jobNum).BaseExp)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendSaveItem(itemNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveItem)
        buffer.WriteInt32(itemNum)
        buffer.WriteInt32(Type.Item(itemNum).AccessReq)

        For i = 1 To StatType.Count - 1
            buffer.WriteInt32(Type.Item(itemNum).Add_Stat(i))
        Next

        buffer.WriteInt32(Type.Item(itemNum).Animation)
        buffer.WriteInt32(Type.Item(itemNum).BindType)
        buffer.WriteInt32(Type.Item(itemNum).JobReq)
        buffer.WriteInt32(Type.Item(itemNum).Data1)
        buffer.WriteInt32(Type.Item(itemNum).Data2)
        buffer.WriteInt32(Type.Item(itemNum).Data3)
        buffer.WriteInt32(Type.Item(itemNum).TwoHanded)
        buffer.WriteInt32(Type.Item(itemNum).LevelReq)
        buffer.WriteInt32(Type.Item(itemNum).Mastery)
        buffer.WriteString((Type.Item(itemNum).Name))
        buffer.WriteInt32(Type.Item(itemNum).Paperdoll)
        buffer.WriteInt32(Type.Item(itemNum).Icon)
        buffer.WriteInt32(Type.Item(itemNum).Price)
        buffer.WriteInt32(Type.Item(itemNum).Rarity)
        buffer.WriteInt32(Type.Item(itemNum).Speed)

        buffer.WriteInt32(Type.Item(itemNum).Stackable)
        buffer.WriteString((Type.Item(itemNum).Description))

        For i = 1 To StatType.Count - 1
            buffer.WriteInt32(Type.Item(itemNum).Stat_Req(i))
        Next

        buffer.WriteInt32(Type.Item(itemNum).Type)
        buffer.WriteInt32(Type.Item(itemNum).SubType)

        buffer.WriteInt32(Type.Item(itemNum).ItemLevel)

        buffer.WriteInt32(Type.Item(itemNum).KnockBack)
        buffer.WriteInt32(Type.Item(itemNum).KnockBackTiles)

        buffer.WriteInt32(Type.Item(itemNum).Projectile)
        buffer.WriteInt32(Type.Item(itemNum).Ammo)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestEditItem()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditItem)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendCloseEditor()
        If InGame = False Then Exit Sub
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CCloseEditor)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendSetHotbarSlot(type As Integer, newSlot As Integer, oldSlot As Integer, num As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSetHotbarSlot)

        buffer.WriteInt32(type)
        buffer.WriteInt32(newSlot)
        buffer.WriteInt32(oldSlot)
        buffer.WriteInt32(num)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendDeleteHotbar(slot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CDeleteHotbarSlot)

        buffer.WriteInt32(slot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendUseHotbarSlot(slot As Integer)
        Select Case Type.Player(MyIndex).Hotbar(slot).SlotType
            Case PartType.Skill
                PlayerCastSkill(FindSkill(Type.Player(MyIndex).Hotbar(slot).Slot))
                Exit Sub
        End Select

        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CUseHotbarSlot)

        buffer.WriteInt32(slot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendLearnSkill(tmpSkill As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSkillLearn)
        buffer.WriteInt32(tmpSkill)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendCast(skillSlot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CCast)
        buffer.WriteInt32(skillslot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

        SkillBuffer = skillslot
        SkillBufferTimer = GetTickCount()
    End Sub

    Sub SendRequestMoral(moralNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestMoral)
        buffer.WriteInt32(moralNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendRequestEditMoral()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditMoral)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendSaveMoral(moralNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveMoral)
        buffer.WriteInt32(moralNum)

        With Type.Moral(moralNum)
            buffer.WriteString(.Name)
            buffer.WriteByte(.Color)
            buffer.WriteBoolean(.CanCast)
            buffer.WriteBoolean(.CanPK)
            buffer.WriteBoolean(.CanDropItem)
            buffer.WriteBoolean(.CanPickupItem)
            buffer.WriteBoolean(.CanUseItem)
            buffer.WriteBoolean(.DropItems)
            buffer.WriteBoolean(.LoseExp)
            buffer.WriteBoolean(.PlayerBlock)
            buffer.WriteBoolean(.NPCBlock)
        End With

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendCloseShop()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CCloseShop)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

End Module