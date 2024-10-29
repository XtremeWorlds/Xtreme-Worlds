Imports Mirage.Sharp.Asfw
Imports Mirage.Sharp.Asfw.IO
Imports Core
Imports Npgsql.Replication.PgOutput
Imports System.Buffers

Module NetworkSend

    Sub AlertMsg(ByVal index As Long, ByVal menuNo As Integer, Optional ByVal menuReset As Integer = 0, Optional ByVal kick As Boolean = 1)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(Packets.ServerPackets.SAlertMsg)
        buffer.WriteInt32(menuNo)
        buffer.WriteInt32(menuReset)
        If kick Then
            buffer.WriteInt32(1)
        Else
            buffer.WriteInt32(0)
        End If
        ClearAccount(index)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub GlobalMsg(msg As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SGlobalMsg)
        buffer.WriteString((msg))
        SendDataToAll(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub PlayerMsg(index As Integer, msg As String, color As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerMsg)
        buffer.WriteString((msg))
        buffer.WriteInt32(color)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendPlayerChars(ByVal index As Long)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerChars)

        ' loop through each character. clear, load, add. repeat.
        For i = 1 To MAX_CHARS
            LoadCharacter(index, i)

            buffer.WriteString(Type.Player(index).Name)
            buffer.WriteInt32(Type.Player(index).Sprite)
            buffer.WriteInt32(Type.Player(index).Access)
            buffer.WriteInt32(Type.Player(index).Job)

            ClearCharacter(index)
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateJob(index As Integer, jobNum As Integer)
        Dim i As Integer, n As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateJob)
        buffer.WriteInt32(jobNum)

        buffer.WriteString((Type.Job(jobNum).Name))
        buffer.WriteString((Type.Job(jobNum).Desc))

        buffer.WriteInt32(Type.Job(jobNum).MaleSprite)
        buffer.WriteInt32(Type.Job(jobNum).FemaleSprite)

        For q = 1 To StatType.Count - 1
            buffer.WriteInt32(Type.Job(jobNum).Stat(q))
        Next

        For q = 1 To 5
            buffer.WriteInt32(Type.Job(jobNum).StartItem(q))
            buffer.WriteInt32(Type.Job(jobNum).StartValue(q))
        Next

        buffer.WriteInt32(Type.Job(jobNum).StartMap)
        buffer.WriteByte(Type.Job(jobNum).StartX)
        buffer.WriteByte(Type.Job(jobNum).StartY)
        buffer.WriteInt32(Type.Job(jobNum).BaseExp)
 
        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendCloseTrade(index As Integer)
        Dim buffer As New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SCloseTrade)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendExp(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerEXP)
        buffer.WriteInt32(index)
        buffer.WriteInt32(GetPlayerExp(index))
        buffer.WriteInt32(GetPlayerNextLevel(index))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendLoginOK(index As Integer)
        Dim Buffer As New ByteStream(4)

        Buffer.WriteInt32(ServerPackets.SLoginOK)
        Buffer.WriteInt32(index)
        Socket.SendDataTo(index, Buffer.Data, Buffer.Head)

        Buffer.Dispose()
    End Sub

    Sub SendInGame(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SInGame)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendJobs(index As Integer)
        Dim i As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SJobData)

        For i = 1 To MAX_JOBS
            buffer.WriteBlock(JobData(i))
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendJobToAll(jobNum As Integer)
        Dim i As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SJobData)
        buffer.WriteBlock(JobData(jobNum))
        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateJobTo(index As Integer, jobNum As Integer)
        Dim buffer As ByteStream
        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdateJob)

        buffer.WriteBlock(JobData(jobNum))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateJobToAll(jobNum As Integer)
        Dim buffer As ByteStream
        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdateJob)

        buffer.WriteBlock(JobData(jobNum))

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendInventory(index As Integer)
        Dim i As Integer, n As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerInv)

        For i = 1 To MAX_INV
            buffer.WriteInt32(GetPlayerInv(index, i))
            buffer.WriteInt32(GetPlayerInvValue(index, i))
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendLeftMap(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SLeftMap)
        buffer.WriteInt32(index)
        SendDataToAllBut(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendLeftGame(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SLeftGame)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendMapEquipment(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapWornEq)
        buffer.WriteInt32(index)
        For i = 1 To EquipmentType.Count - 1
            buffer.WriteInt32(GetPlayerEquipment(index, i))
        Next
        
        SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendMapEquipmentTo(PlayerNum As Integer, index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapWornEq)
        buffer.WriteInt32(PlayerNum)
        For i = 1 To EquipmentType.Count - 1
            buffer.WriteInt32(GetPlayerEquipment(PlayerNum, i))
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendShops(index As Integer)
        Dim i As Integer

        For i = 1 To MAX_SHOPS
            If Type.Shop(i).Name.Length > 0 Then
                SendUpdateShopTo(index, i)
            End If
        Next

    End Sub

    Sub SendUpdateShopTo(index As Integer, shopNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateShop)

        buffer.WriteInt32(shopNum)

        buffer.WriteInt32(Type.Shop(shopNum).BuyRate)
        buffer.WriteString((Type.Shop(shopNum).Name))

        For i = 1 To MAX_TRADES
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostItem)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostValue)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).Item)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).ItemValue)
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateShopToAll(shopNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateShop)

        buffer.WriteInt32(shopNum)
        buffer.WriteInt32(Type.Shop(shopNum).BuyRate)
        buffer.WriteString(Type.Shop(shopNum).Name)

        For i = 1 To MAX_TRADES
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostItem)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostValue)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).Item)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).ItemValue)
        Next

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendSkills(index As Integer)
        Dim i As Integer

        For i = 1 To MAX_SKILLS
            If Type.Skill(i).Name.Length > 0 Then
                SendUpdateSkillTo(index, i)
            End If
        Next
    End Sub

    Sub SendUpdateSkillTo(index As Integer, skillnum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateSkill)
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
        buffer.WriteString(Type.Skill(skillNum).Name)
        buffer.WriteInt32(Type.Skill(skillNum).Range)
        buffer.WriteInt32(Type.Skill(skillNum).SkillAnim)
        buffer.WriteInt32(Type.Skill(skillNum).StunDuration)
        buffer.WriteInt32(Type.Skill(skillNum).Type)
        buffer.WriteInt32(Type.Skill(skillNum).Vital)
        buffer.WriteInt32(Type.Skill(skillNum).X)
        buffer.WriteInt32(Type.Skill(skillNum).Y)

        'projectiles
        buffer.WriteInt32(Type.Skill(skillNum).IsProjectile)
        buffer.WriteInt32(Type.Skill(skillNum).Projectile)

        buffer.WriteInt32(Type.Skill(skillNum).KnockBack)
        buffer.WriteInt32(Type.Skill(skillNum).KnockBackTiles)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateSkillToAll(skillnum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateSkill)
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
        buffer.WriteString(Type.Skill(skillNum).Name)
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

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendStats(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerStats)
        buffer.WriteInt32(index)

        For i = 1 To StatType.Count - 1
            buffer.WriteInt32(GetPlayerStat(index, i))
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendVitals(index As Integer)
        For i = 1 To VitalType.Count - 1
            SendVital(index, i)
        Next
    End Sub

    Sub SendVital(index As Integer, Vital As VitalType)
        Dim buffer As New ByteStream(4)
        Dim amount As Integer

        ' Get our packet type.
        Select Case Vital
            Case VitalType.HP
                buffer.WriteInt32(ServerPackets.SPlayerHP)
            Case VitalType.SP
                buffer.WriteInt32(ServerPackets.SPlayerSP)
        End Select

        amount = GetPlayerVital(index, Vital)

        ' Set and send related data.
        buffer.WriteInt32(amount)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()

        ' send vitals to party if in one
        If TempPlayer(index).InParty > 0 Then SendPartyVitals(TempPlayer(index).InParty, index)
    End Sub

    Sub SendWelcome(index As Integer)

        ' Send them welcome
        If Type.Setting.Welcome.Length > 0 Then
            PlayerMsg(index, Type.Setting.Welcome, ColorType.BrightCyan)
        End If

        ' Send whos online
        SendWhosOnline(index)
    End Sub

    Sub SendWhosOnline(index As Integer)
        Dim s As String = ""
        Dim n As Integer
        Dim i As Integer

        If GetPlayerAccess(index) < AccessType.Moderator Then Exit Sub

        For i = 1 To Socket.HighIndex()
            If i <> index And GetPlayerName(i)  <> "" Then
                s = s & GetPlayerName(i) & ", "
                n = n + 1
            End If
        Next

        If n = 0 Then
            s = "There are no other players online."
        Else
            s = Mid$(s, 1, Len(s) - 2)
            s = "There are " & n & " other players online: " & s & "."
        End If

        PlayerMsg(index, s, ColorType.White)
    End Sub

    Sub SendWornEquipment(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerWornEq)

        For i = 1 To EquipmentType.Count - 1
            buffer.WriteInt32(GetPlayerEquipment(index, i))
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendMapData(index As Integer, mapNum As Integer, SendMap As Boolean)
        Dim buffer As New ByteStream(4)
        Dim data() As Byte

        If SendMap Then
            buffer.WriteInt32(1)
            buffer.WriteInt32(MapNum)
            buffer.WriteString((Type.Map(MapNum).Name))
            buffer.WriteString((Type.Map(MapNum).Music))
            buffer.WriteInt32(Type.Map(MapNum).Revision)
            buffer.WriteInt32(Type.Map(MapNum).Moral)
            buffer.WriteInt32(Type.Map(MapNum).Tileset)
            buffer.WriteInt32(Type.Map(MapNum).Up)
            buffer.WriteInt32(Type.Map(MapNum).Down)
            buffer.WriteInt32(Type.Map(MapNum).Left)
            buffer.WriteInt32(Type.Map(MapNum).Right)
            buffer.WriteInt32(Type.Map(MapNum).BootMap)
            buffer.WriteInt32(Type.Map(MapNum).BootX)
            buffer.WriteInt32(Type.Map(MapNum).BootY)
            buffer.WriteInt32(Type.Map(MapNum).MaxX)
            buffer.WriteInt32(Type.Map(MapNum).MaxY)
            buffer.WriteInt32(Type.Map(MapNum).Weather)
            buffer.WriteInt32(Type.Map(MapNum).Fog)
            buffer.WriteInt32(Type.Map(MapNum).WeatherIntensity)
            buffer.WriteInt32(Type.Map(MapNum).FogOpacity)
            buffer.WriteInt32(Type.Map(MapNum).FogSpeed)
            buffer.WriteInt32(Type.Map(MapNum).MapTint)
            buffer.WriteInt32(Type.Map(MapNum).MapTintR)
            buffer.WriteInt32(Type.Map(MapNum).MapTintG)
            buffer.WriteInt32(Type.Map(MapNum).MapTintB)
            buffer.WriteInt32(Type.Map(MapNum).MapTintA)
            buffer.WriteByte(Type.Map(MapNum).Panorama)
            buffer.WriteByte(Type.Map(MapNum).Parallax)
            buffer.WriteByte(Type.Map(MapNum).Brightness)
            buffer.WriteInt32(Type.Map(MapNum).NoRespawn)
            buffer.WriteInt32(Type.Map(MapNum).Indoors)
            buffer.WriteInt32(Type.Map(MapNum).Shop)

            For i = 1 To MAX_MAP_NPCS
                buffer.WriteInt32(Type.Map(MapNum).NPC(i))
            Next

            For X = 0 To Type.Map(MapNum).MaxX
                For Y = 0 To Type.Map(MapNum).MaxY
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Data1)
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Data2)
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Data3)
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Data1_2)
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Data2_2)
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Data3_2)
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).DirBlock)
                    For i = 1 To LayerType.Count - 1
                        buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Layer(i).Tileset)
                        buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Layer(i).X)
                        buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Layer(i).Y)
                        buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Layer(i).AutoTile)
                    Next
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Type)
                    buffer.WriteInt32(Type.Map(MapNum).Tile(X, Y).Type2)
                Next
            Next

            buffer.WriteInt32(Type.Map(MapNum).EventCount)

            If Type.Map(MapNum).EventCount > 0 Then
                For i = 1 To Type.Map(MapNum).EventCount
                    With Type.Map(MapNum).Event(i)
                        buffer.WriteString((.Name))
                        buffer.WriteByte(.Globals)
                        buffer.WriteInt32(.X)
                        buffer.WriteInt32(.Y)
                        buffer.WriteInt32(.PageCount)
                    End With

                    If Type.Map(MapNum).Event(i).PageCount > 0 Then
                        For X = 0 To Type.Map(MapNum).Event(i).PageCount
                            With Type.Map(MapNum).Event(i).Pages(X)
                                buffer.WriteInt32(.ChkVariable)
                                buffer.WriteInt32(.VariableIndex)
                                buffer.WriteInt32(.VariableCondition)
                                buffer.WriteInt32(.VariableCompare)
                                buffer.WriteInt32(.ChkSwitch)
                                buffer.WriteInt32(.SwitchIndex)
                                buffer.WriteInt32(.SwitchCompare)
                                buffer.WriteInt32(.ChkHasItem)
                                buffer.WriteInt32(.HasItemIndex)
                                buffer.WriteInt32(.HasItemAmount)
                                buffer.WriteInt32(.ChkSelfSwitch)
                                buffer.WriteInt32(.SelfSwitchIndex)
                                buffer.WriteInt32(.SelfSwitchCompare)
                                buffer.WriteByte(.GraphicType)
                                buffer.WriteInt32(.Graphic)
                                buffer.WriteInt32(.GraphicX)
                                buffer.WriteInt32(.GraphicY)
                                buffer.WriteInt32(.GraphicX2)
                                buffer.WriteInt32(.GraphicY2)
                                buffer.WriteByte(.MoveType)
                                buffer.WriteByte(.MoveSpeed)
                                buffer.WriteByte(.MoveFreq)
                                buffer.WriteInt32(.MoveRouteCount)
                                buffer.WriteInt32(.IgnoreMoveRoute)
                                buffer.WriteInt32(.RepeatMoveRoute)

                                If .MoveRouteCount > 0 Then
                                    For Y = 0 To .MoveRouteCount
                                        buffer.WriteInt32(.MoveRoute(Y).Index)
                                        buffer.WriteInt32(.MoveRoute(Y).Data1)
                                        buffer.WriteInt32(.MoveRoute(Y).Data2)
                                        buffer.WriteInt32(.MoveRoute(Y).Data3)
                                        buffer.WriteInt32(.MoveRoute(Y).Data4)
                                        buffer.WriteInt32(.MoveRoute(Y).Data5)
                                        buffer.WriteInt32(.MoveRoute(Y).Data6)
                                    Next
                                End If

                                buffer.WriteInt32(.WalkAnim)
                                buffer.WriteInt32(.DirFix)
                                buffer.WriteInt32(.WalkThrough)
                                buffer.WriteInt32(.ShowName)
                                buffer.WriteByte(.Trigger)
                                buffer.WriteInt32(.CommandListCount)
                                buffer.WriteByte(.Position)
                                buffer.WriteInt32(.QuestNum)
                            End With

                            If Type.Map(MapNum).Event(i).Pages(X).CommandListCount > 0 Then
                                For Y = 0 To Type.Map(MapNum).Event(i).Pages(X).CommandListCount
                                    buffer.WriteInt32(Type.Map(MapNum).Event(i).Pages(X).CommandList(Y).CommandCount)
                                    buffer.WriteInt32(Type.Map(MapNum).Event(i).Pages(X).CommandList(Y).ParentList)
                                    If Type.Map(MapNum).Event(i).Pages(X).CommandList(Y).CommandCount > 0 Then
                                        For z = 0 To Type.Map(MapNum).Event(i).Pages(X).CommandList(Y).CommandCount
                                            With Type.Map(MapNum).Event(i).Pages(X).CommandList(Y).Commands(z)
                                                buffer.WriteByte(.Index)
                                                buffer.WriteString((.Text1))
                                                buffer.WriteString((.Text2))
                                                buffer.WriteString((.Text3))
                                                buffer.WriteString((.Text4))
                                                buffer.WriteString((.Text5))
                                                buffer.WriteInt32(.Data1)
                                                buffer.WriteInt32(.Data2)
                                                buffer.WriteInt32(.Data3)
                                                buffer.WriteInt32(.Data4)
                                                buffer.WriteInt32(.Data5)
                                                buffer.WriteInt32(.Data6)
                                                buffer.WriteInt32(.ConditionalBranch.CommandList)
                                                buffer.WriteInt32(.ConditionalBranch.Condition)
                                                buffer.WriteInt32(.ConditionalBranch.Data1)
                                                buffer.WriteInt32(.ConditionalBranch.Data2)
                                                buffer.WriteInt32(.ConditionalBranch.Data3)
                                                buffer.WriteInt32(.ConditionalBranch.ElseCommandList)
                                                buffer.WriteInt32(.MoveRouteCount)
                                                If .MoveRouteCount > 0 Then
                                                    For w = 0 To .MoveRouteCount
                                                        buffer.WriteInt32(.MoveRoute(w).Index)
                                                        buffer.WriteInt32(.MoveRoute(w).Data1)
                                                        buffer.WriteInt32(.MoveRoute(w).Data2)
                                                        buffer.WriteInt32(.MoveRoute(w).Data3)
                                                        buffer.WriteInt32(.MoveRoute(w).Data4)
                                                        buffer.WriteInt32(.MoveRoute(w).Data5)
                                                        buffer.WriteInt32(.MoveRoute(w).Data6)
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
        Else
            buffer.WriteInt32(0)
        End If

        For i = 1 To MAX_MAP_ITEMS
            buffer.WriteInt32(Type.MapItem(MapNum, i).Num)
            buffer.WriteInt32(Type.MapItem(MapNum, i).Value)
            buffer.WriteInt32(Type.MapItem(MapNum, i).X)
            buffer.WriteInt32(Type.MapItem(MapNum, i).Y)
        Next

        For i = 1 To MAX_MAP_NPCS
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Num)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).X)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Y)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Dir)
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Vital(VitalType.HP))
            buffer.WriteInt32(MapNPC(MapNum).NPC(i).Vital(VitalType.SP))
        Next

       If Type.MapResource(GetPlayerMap(index)).ResourceCount > 0 Then
            buffer.WriteInt32(1)
            buffer.WriteInt32(Type.MapResource(GetPlayerMap(index)).ResourceCount)

            For i = 0 To MapResource(GetPlayerMap(index)).ResourceCount
                buffer.WriteByte(Type.MapResource(GetPlayerMap(index)).ResourceData(i).State)
                buffer.WriteInt32(Type.MapResource(GetPlayerMap(index)).ResourceData(i).X)
                buffer.WriteInt32(Type.MapResource(GetPlayerMap(index)).ResourceData(i).Y)
            Next
        Else
            buffer.WriteInt32(0)
        End If

        data = Compression.CompressBytes(buffer.ToArray)
        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SMapData)
        buffer.WriteBlock(data)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendJoinMap(index As Integer)
        Dim i As Integer
        Dim data As Byte()

        ' Send all players on current map to index
        For i = 1 To Socket.HighIndex()
            If GetPlayerMap(i) = GetPlayerMap(index) Then
                data = PlayerData(i)
                Socket.SendDataTo(index, data, data.Length)
                SendPlayerXYTo(index, i)
            End If
        Next

        ' Send index's player data to everyone on the map including himself
        data = PlayerData(index)
        SendDataToMapBut(index, GetPlayerMap(index), data, data.Length)
        SendVitals(index)
        SendPlayerXYToMap(index)
    End Sub

    Function PlayerData(index As Integer) As Byte()
        Dim buffer As New ByteStream(4), i As Integer

        buffer.WriteInt32(ServerPackets.SPlayerData)
        buffer.WriteInt32(index)
        buffer.WriteString(GetPlayerName(index))
        buffer.WriteInt32(GetPlayerJob(index))
        buffer.WriteInt32(GetPlayerLevel(index))
        buffer.WriteInt32(GetPlayerPoints(index))
        buffer.WriteInt32(GetPlayerSprite(index))
        buffer.WriteInt32(GetPlayerMap(index))
        buffer.WriteInt32(GetPlayerAccess(index))
        buffer.WriteInt32(GetPlayerPK(index))

        For i = 1 To StatType.Count - 1
            buffer.WriteInt32(GetPlayerStat(index, i))
        Next

        For i = 1 To ResourceType.Count - 1
            buffer.WriteInt32(GetPlayerGatherSkillLvl(index, i))
            buffer.WriteInt32(GetPlayerGatherSkillExp(index, i))
            buffer.WriteInt32(GetPlayerGatherSkillMaxExp(index, i))
        Next

        PlayerData = buffer.ToArray()

        buffer.Dispose()
    End Function

    Sub SendPlayerXY(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerXY)
        buffer.WriteInt32(index)
        buffer.WriteInt32(GetPlayerX(index))
        buffer.WriteInt32(GetPlayerY(index))
        buffer.WriteInt32(GetPlayerDir(index))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendPlayerXYTo(index As Integer, playerNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerXY)
        buffer.WriteInt32(playerNum)
        buffer.WriteInt32(GetPlayerX(playerNum))
        buffer.WriteInt32(GetPlayerY(playerNum))
        buffer.WriteInt32(GetPlayerDir(playerNum))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendPlayerXYToMap(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerXY)
        buffer.WriteInt32(index)
        buffer.WriteInt32(GetPlayerX(index))
        buffer.WriteInt32(GetPlayerY(index))
        buffer.WriteInt32(GetPlayerDir(index))

        SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendPlayerMove(index As Integer, Movement As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SPlayerMove)
        buffer.WriteInt32(index)
        buffer.WriteInt32(GetPlayerX(index))
        buffer.WriteInt32(GetPlayerY(index))
        buffer.WriteInt32(GetPlayerDir(index))
        buffer.WriteInt32(Movement)

        SendDataToMapBut(index, GetPlayerMap(index), buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub MapMsg(MapNum As Integer, Msg As String, Color As Byte)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapMsg)
        buffer.WriteString((Msg))

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub AdminMsg(MapNum As Integer, Msg As String, Color As Byte)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SAdminMsg)
        buffer.WriteString((Msg))

        For i = 1 To Socket.HighIndex
            If GetPlayerAccess(i) >= AccessType.Moderator Then
                SendDataTo(i, buffer.Data, buffer.Head)
            End If
        Next

        buffer.Dispose()
    End Sub

    Sub SendActionMsg(MapNum As Integer, Message As String, Color As Integer, MsgType As Integer, X As Integer, Y As Integer, Optional PlayerOnlyNum As Integer = 0)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SActionMsg)
        buffer.WriteString((Message))
        buffer.WriteInt32(Color)
        buffer.WriteInt32(MsgType)
        buffer.WriteInt32(X)
        buffer.WriteInt32(Y)

        If PlayerOnlyNum > 0 Then
            Socket.SendDataTo(PlayerOnlyNum, buffer.Data, buffer.Head)
        Else
            SendDataToMap(MapNum, buffer.Data, buffer.Head)
        End If

        buffer.Dispose()
    End Sub

    Sub SayMsg_Map(MapNum As Integer, index As Integer, Message As String, SayColor As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SSayMsg)
        buffer.WriteString(GetPlayerName(index))
        buffer.WriteInt32(GetPlayerAccess(index))
        buffer.WriteInt32(GetPlayerPK(index))
        buffer.WriteString(Message)
        buffer.WriteString(("[Map]"))
        buffer.WriteInt32(SayColor)

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SayMsg_Global(index As Integer, Message As String, SayColor As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SSayMsg)
        buffer.WriteString(GetPlayerName(index))
        buffer.WriteInt32(GetPlayerAccess(index))
        buffer.WriteInt32(GetPlayerPK(index))
        buffer.WriteString(Message)
        buffer.WriteString(("[Global]"))
        buffer.WriteInt32(SayColor)

        SendDataToAll(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendPlayerData(index As Integer)
        Dim data = PlayerData(index)
        SendDataToMap(GetPlayerMap(index), data, data.Length)
    End Sub

    Sub SendInventoryUpdate(index As Integer, InvSlot As Integer)
        Dim buffer As New ByteStream(4), n As Integer

        buffer.WriteInt32(ServerPackets.SPlayerInvUpdate)

        buffer.WriteInt32(InvSlot)

        buffer.WriteInt32(GetPlayerInv(index, InvSlot))
        buffer.WriteInt32(GetPlayerInvValue(index, InvSlot))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendOpenShop(index As Integer, ShopNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SOpenShop)
        buffer.WriteInt32(ShopNum)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub ResetShopAction(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SResetShopAction)

        SendDataToAll(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendBank(index As Integer)
        Dim buffer As New ByteStream(4)
        Dim i As Integer

        buffer.WriteInt32(ServerPackets.SBank)

        For i = 1 To MAX_BANK
            buffer.WriteInt32(Bank(index).Item(i).Num)
            buffer.WriteInt32(Bank(index).Item(i).Value)
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendClearSkillBuffer(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SClearSkillBuffer)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendTradeInvite(index As Integer, TradeIndex As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.STradeInvite)
        buffer.WriteInt32(TradeIndex)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendTrade(index As Integer, TradeTarget As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.STrade)
        buffer.WriteInt32(TradeTarget)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendTradeUpdate(index As Integer, DataType As Byte)
        Dim buffer As New ByteStream(4)
        Dim i As Integer
        Dim tradeTarget As Integer
        Dim totalWorth As Integer

        tradeTarget = TempPlayer(index).InTrade

        If tradeTarget = 0 Then Exit Sub

        buffer.WriteInt32(ServerPackets.STradeUpdate)
        buffer.WriteInt32(DataType)

        If DataType = 0 Then ' own inventory

            For i = 1 To MAX_INV
                buffer.WriteInt32(TempPlayer(index).TradeOffer(i).Num)
                buffer.WriteInt32(TempPlayer(index).TradeOffer(i).Value)

                ' add total worth
                If TempPlayer(index).TradeOffer(i).Num > 0 Then
                    ' currency?
                    If Type.Item(TempPlayer(index).TradeOffer(i).Num).Type = ItemType.Currency Or Type.Item(Type.TempPlayer(index).TradeOffer(i).Num).Stackable = 1 Then
                        If TempPlayer(index).TradeOffer(i).Value = 0 Then TempPlayer(index).TradeOffer(i).Value = 1
                        totalWorth = totalWorth + (Type.Item(GetPlayerInv(index, TempPlayer(index).TradeOffer(i).Num)).Price * TempPlayer(index).TradeOffer(i).Value)
                    Else
                        totalWorth = totalWorth + Type.Item(GetPlayerInv(index, TempPlayer(index).TradeOffer(i).Num)).Price
                    End If
                End If
            Next
        ElseIf DataType = 1 Then ' other inventory

            For i = 1 To MAX_INV
                buffer.WriteInt32(GetPlayerInv(tradeTarget, TempPlayer(tradeTarget).TradeOffer(i).Num))
                buffer.WriteInt32(TempPlayer(tradeTarget).TradeOffer(i).Value)

                ' add total worth
                If GetPlayerInv(tradeTarget, TempPlayer(tradeTarget).TradeOffer(i).Num) > 0 Then
                    ' currency?
                    If Type.Item(GetPlayerInv(tradeTarget, TempPlayer(tradeTarget).TradeOffer(i).Num)).Type = ItemType.Currency Or Type.Item(Global.Core.Commands.GetPlayerInv(tradeTarget, Type.TempPlayer(tradeTarget).TradeOffer(i).Num)).Stackable = 1 Then
                        If TempPlayer(tradeTarget).TradeOffer(i).Value = 0 Then TempPlayer(tradeTarget).TradeOffer(i).Value = 1
                        totalWorth = totalWorth + (Type.Item(GetPlayerInv(tradeTarget, TempPlayer(tradeTarget).TradeOffer(i).Num)).Price * TempPlayer(tradeTarget).TradeOffer(i).Value)
                    Else
                        totalWorth = totalWorth + Type.Item(GetPlayerInv(tradeTarget, TempPlayer(tradeTarget).TradeOffer(i).Num)).Price
                    End If
                End If
            Next
        End If

        ' send total worth of trade
        buffer.WriteInt32(totalWorth)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendTradeStatus(index As Integer, Status As Byte)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.STradeStatus)
        buffer.WriteInt32(Status)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendStunned(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SStunned)
        buffer.WriteInt32(TempPlayer(index).StunDuration)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendBlood(MapNum As Integer, X As Integer, Y As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SBlood)
        buffer.WriteInt32(X)
        buffer.WriteInt32(Y)

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendPlayerSkills(index As Integer)
        Dim i As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SSkills)

        For i = 1 To MAX_PLAYER_SKILLS
            buffer.WriteInt32(GetPlayerSkill(index, i))
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendCooldown(index As Integer, Slot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SCooldown)
        buffer.WriteInt32(Slot)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendTarget(index As Integer, Target As Integer, TargetType As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.STarget)
        buffer.WriteInt32(Target)
        buffer.WriteInt32(TargetType)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendMapReport(index As Integer)
        Dim buffer As New ByteStream(4), I As Integer

        buffer.WriteInt32(ServerPackets.SMapReport)

        For i = 1 To MAX_MAPS
            buffer.WriteString(Type.Map(i).Name)
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendAdminPanel(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SAdmin)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendMapNames(index As Integer)
        Dim buffer As New ByteStream(4), I As Integer

        buffer.WriteInt32(ServerPackets.SMapNames)

        For i = 1 To MAX_MAPS
            buffer.WriteString(Type.Map(i).Name)
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendHotbar(index As Integer)
        Dim buffer As New ByteStream(4), i As Integer

        buffer.WriteInt32(ServerPackets.SHotbar)

        For i = 1 To MAX_Hotbar
            buffer.WriteInt32(Type.Player(index).Hotbar(i).Slot)
            buffer.WriteInt32(Type.Player(index).Hotbar(i).SlotType)
        Next

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendCritical(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SCritical)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendKeyPair(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SKeyPair)
        buffer.WriteString(EKeyPair.ExportKeyString(False))
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendRightClick(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SrClick)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendClassEditor(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SJobEditor)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendEmote(index As Integer, Emote As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SEmote)

        buffer.WriteInt32(index)
        buffer.WriteInt32(Emote)

        SendDataToMap(GetPlayerMap(index), buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendChatBubble(MapNum As Integer, Target As Integer, TargetType As Integer, Message As String, Color As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SChatBubble)

        buffer.WriteInt32(Target)
        buffer.WriteInt32(TargetType)
        buffer.WriteString(Message)
        buffer.WriteInt32(Color)
        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub SendPlayerAttack(index As Integer)
        Dim Buffer As New ByteStream(4)

        Buffer.WriteInt32(ServerPackets.SAttack)

        Buffer.WriteInt32(index)
        SendDataToMapBut(index, GetPlayerMap(index), Buffer.Data, Buffer.Head)
        Buffer.Dispose()
    End Sub

End Module