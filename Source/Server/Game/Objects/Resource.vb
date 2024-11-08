
Imports Core
Imports Mirage.Sharp.Asfw
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Friend Module Resource

#Region "Database"

    Sub SaveResource(resourceNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Resource(resourceNum)).ToString()

        If RowExists(resourceNum, "resource")
            UpdateRow(resourceNum, json, "resource", "data")
        Else
            InsertRow(resourceNum, json, "resource")
        End If
    End Sub

    Sub LoadResources()
        Dim i As Integer

        For i = 1 To MAX_RESOURCES
            LoadResource(i)
        Next

    End Sub

    Sub LoadResource(resourceNum As Integer)
        Dim data As JObject

        data = SelectRow(resourceNum, "resource", "data")

        If data Is Nothing Then
            ClearResource(resourceNum)
            Exit Sub
        End If

        Dim resourceData = JObject.FromObject(data).toObject(Of Type.ResourceStruct)()
        Type.Resource(resourceNum) = resourceData
    End Sub

    Sub ClearResource(index As Integer)
        Type.Resource(index).Name = ""
        Type.Resource(index).EmptyMessage = ""
        Type.Resource(index).SuccessMessage = ""
    End Sub

    Sub ClearResources()
        ReDim MapResource(MAX_MAPS)
        ReDim MapResource(MAX_MAPS).ResourceData(MAX_RESOURCES)

        For i = 1 To MAX_RESOURCES
            ClearResource(i)
        Next
    End Sub

    Friend Sub CacheResources(MapNum As Integer)
        Dim x As Integer, y As Integer, Resource_Count As Integer

        For X = 0 To Type.Map(MapNum).MaxX
            For Y = 0 To Type.Map(MapNum).MaxY

                If Type.Map(MapNum).Tile(x, y).Type = TileType.Resource Or Type.Map(MapNum).Tile(x, y).Type2 = TileType.Resource Then
                    Resource_Count += 1
                    ReDim Preserve MapResource(MapNum).ResourceData(Resource_Count)
                    MapResource(MapNum).ResourceData(Resource_Count).X = x
                    MapResource(MapNum).ResourceData(Resource_Count).Y = y
                    MapResource(MapNum).ResourceData(Resource_Count).Health = Type.Resource(Type.Map(MapNum).Tile(x, y).Data1).Health
                End If

            Next
        Next

        MapResource(MapNum).ResourceCount = Resource_Count
    End Sub

    Function ResourcesData() As Byte()
        Dim buffer As New ByteStream(4)
        For i = 1 To MAX_RESOURCES
            If Not Len(Type.Resource(i).Name) > 0 Then Continue For
            buffer.WriteBlock(ResourceData(i))
        Next
        Return buffer.ToArray
    End Function

    Function ResourceData(ResourceNum As Integer) As Byte()
        Dim buffer As New ByteStream(4)
        buffer.WriteInt32(ResourceNum)
        buffer.WriteInt32(Type.Resource(ResourceNum).Animation)
        buffer.WriteString((Type.Resource(ResourceNum).EmptyMessage))
        buffer.WriteInt32(Type.Resource(ResourceNum).ExhaustedImage)
        buffer.WriteInt32(Type.Resource(ResourceNum).Health)
        buffer.WriteInt32(Type.Resource(ResourceNum).ExpReward)
        buffer.WriteInt32(Type.Resource(ResourceNum).ItemReward)
        buffer.WriteString((Type.Resource(ResourceNum).Name))
        buffer.WriteInt32(Type.Resource(ResourceNum).ResourceImage)
        buffer.WriteInt32(Type.Resource(ResourceNum).ResourceType)
        buffer.WriteInt32(Type.Resource(ResourceNum).RespawnTime)
        buffer.WriteString((Type.Resource(ResourceNum).SuccessMessage))
        buffer.WriteInt32(Type.Resource(ResourceNum).LvlRequired)
        buffer.WriteInt32(Type.Resource(ResourceNum).ToolRequired)
        buffer.WriteInt32(Type.Resource(ResourceNum).Walkthrough)
        Return buffer.ToArray
    End Function

#End Region

#Region "Gather Skills"
    Sub CheckResourceLevelUp(index As Integer, SkillSlot As Integer)
        Dim expRollover As Integer, level_count As Integer

        level_count = 0

        If GetPlayerGatherSkillLvl(index, SkillSlot) = MAX_LEVEL Then Exit Sub

        Do While GetPlayerGatherSkillExp(index, SkillSlot) >= GetPlayerGatherSkillMaxExp(index, SkillSlot)
            expRollover = GetPlayerGatherSkillExp(index, SkillSlot) - GetPlayerGatherSkillMaxExp(index, SkillSlot)
            SetPlayerGatherSkillLvl(index, SkillSlot, GetPlayerGatherSkillLvl(index, SkillSlot) + 1)
            SetPlayerGatherSkillExp(index, SkillSlot, expRollover)
            SetPlayerGatherSkillMaxExp(index, SkillSlot, GetSkillNextLevel(index, SkillSlot))
            level_count = level_count + 1
        Loop

        If level_count > 0 Then
            If level_count = 1 Then
                'singular
                PlayerMsg(index, String.Format("Your {0} has gone up a level!", GetResourceSkillName(SkillSlot)), ColorType.BrightGreen)
            Else
                'plural
                PlayerMsg(index, String.Format("Your {0} has gone up by {1} levels!", GetResourceSkillName(SkillSlot), level_count), ColorType.BrightGreen)
            End If

            SendPlayerData(index)
        End If
    End Sub

#End Region

#Region "Incoming Packets"

    Sub Packet_EditResource(index As Integer, ByRef data() As Byte)
        Dim Buffer As New ByteStream(4)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Resource)

        If user <> "" Then 
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        TempPlayer(index).Editor = EditorType.Resource

        SendItems(index)
        SendAnimations(index)
        SendResources(index)

        Buffer.WriteInt32(ServerPackets.SResourceEditor)
        Socket.SendDataTo(index, Buffer.Data, Buffer.Head)

        Buffer.Dispose()
    End Sub

    Sub Packet_SaveResource(index As Integer, ByRef data() As Byte)
        Dim resourcenum As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        resourcenum = buffer.ReadInt32

        ' Prevent hacking
        If resourcenum <= 0 Or resourcenum > MAX_RESOURCES Then Exit Sub

        Type.Resource(resourcenum).Animation = buffer.ReadInt32()
        Type.Resource(resourcenum).EmptyMessage = buffer.ReadString()
        Type.Resource(resourcenum).ExhaustedImage = buffer.ReadInt32()
        Type.Resource(resourcenum).Health = buffer.ReadInt32()
        Type.Resource(resourcenum).ExpReward = buffer.ReadInt32()
        Type.Resource(resourcenum).ItemReward = buffer.ReadInt32()
        Type.Resource(resourcenum).Name = buffer.ReadString()
        Type.Resource(resourcenum).ResourceImage = buffer.ReadInt32()
        Type.Resource(resourcenum).ResourceType = buffer.ReadInt32()
        Type.Resource(resourcenum).RespawnTime = buffer.ReadInt32()
        Type.Resource(resourcenum).SuccessMessage = buffer.ReadString()
        Type.Resource(resourcenum).LvlRequired = buffer.ReadInt32()
        Type.Resource(resourcenum).ToolRequired = buffer.ReadInt32()
        Type.Resource(resourcenum).Walkthrough = buffer.ReadInt32()

        ' Save it
        SendUpdateResourceToAll(resourcenum)
        SaveResource(resourcenum)

        Addlog(GetPlayerLogin(index) & " saved Resource #" & resourcenum & ".", ADMIN_LOG)

        buffer.Dispose()
    End Sub

    Sub Packet_RequestResource(index As Integer, ByRef data() As Byte)
        Dim Buffer = New ByteStream(data), n As Integer

        n = Buffer.ReadInt32

        If n <= 0 Or n > MAX_RESOURCES Then Exit Sub

        SendUpdateResourceTo(index, n)
    End Sub

#End Region

#Region "Outgoing Packets"

    Sub SendMapResourceTo(index As Integer, Resource_num As long)
        Dim i As Integer, mapnum As Integer
        Dim buffer As New ByteStream(4)

        mapnum = GetPlayerMap(index)

        buffer.WriteInt32(ServerPackets.SMapResource)
        buffer.WriteInt32(Type.MapResource(MapNum).ResourceCount)

       If Type.MapResource(MapNum).ResourceCount > 0 Then

            For i = 1 To MapResource(MapNum).ResourceCount
                buffer.WriteByte(Type.MapResource(MapNum).ResourceData(i).State)
                buffer.WriteInt32(Type.MapResource(MapNum).ResourceData(i).X)
                buffer.WriteInt32(Type.MapResource(MapNum).ResourceData(i).Y)
            Next

        End If

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendMapResourceToMap(MapNum As Integer, Resource_num As Integer)
        Dim i As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SMapResource)
        buffer.WriteInt32(Type.MapResource(MapNum).ResourceCount)

       If Type.MapResource(MapNum).ResourceCount > 0 Then

            For i = 1 To MapResource(MapNum).ResourceCount
                buffer.WriteByte(Type.MapResource(MapNum).ResourceData(i).State)
                buffer.WriteInt32(Type.MapResource(MapNum).ResourceData(i).X)
                buffer.WriteInt32(Type.MapResource(MapNum).ResourceData(i).Y)
            Next

        End If

        SendDataToMap(MapNum, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendResources(index As Integer)
        Dim i As Integer

        For i = 1 To MAX_RESOURCES

            If Len(Type.Resource(i).Name) > 0 Then
                SendUpdateResourceTo(index, i)
            End If

        Next

    End Sub

    Sub SendUpdateResourceTo(index As Integer, ResourceNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateResource)

        buffer.WriteBlock(ResourceData(ResourceNum))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateResourceToAll(ResourceNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateResource)

        buffer.WriteBlock(ResourceData(ResourceNum))

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

#End Region

#Region "Functions"

    Sub CheckResource(index As Integer, x As Integer, y As Integer)
        Dim Resource_num As Integer, ResourceType As Byte
        Dim Resource_index As Integer
        Dim rX As Integer, rY As Integer
        Dim Damage As Integer
        Dim MapNum As Integer

        MapNum = GetPlayerMap(index)

        If x < 0 Or y < 0 Then Exit Sub

        If Type.Map(MapNum).Tile(x, y).Type = TileType.Resource Or Type.Map(MapNum).Tile(x, y).Type2 = TileType.Resource Then
            Resource_num = 0
            Resource_index = Type.Map(MapNum).Tile(x, y).Data1
            ResourceType = Type.Resource(Resource_index).ResourceType

            ' Get the cache number
            For i = 1 To MapResource(MapNum).ResourceCount
               If Type.MapResource(MapNum).ResourceData(i).X = x Then
                   If Type.MapResource(MapNum).ResourceData(i).Y = y Then
                        Resource_num = i
                    End If
                End If
            Next

            If Resource_num > 0 Then
                If GetPlayerEquipment(index, EquipmentType.Weapon) > 0 Or Type.Resource(Resource_index).ToolRequired = 0 Then
                    If Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Data3 = Type.Resource(Resource_index).ToolRequired Then

                        ' inv space?
                        If Type.Resource(Resource_index).ItemReward > 0 Then
                            If FindOpenInvSlot(index, Type.Resource(Resource_index).ItemReward) = 0 Then
                                PlayerMsg(index, "You have no inventory space.", ColorType.Yellow)
                                Exit Sub
                            End If
                        End If

                        'required lvl?
                        If Type.Resource(Resource_index).LvlRequired > GetPlayerGatherSkillLvl(index, ResourceType) Then
                            PlayerMsg(index, "Your level is too low!", ColorType.Yellow)
                            Exit Sub
                        End If

                        ' check if already cut down
                        If Type.MapResource(MapNum).ResourceData(Resource_num).State = 0 Then

                            rX = MapResource(MapNum).ResourceData(Resource_num).X
                            rY = MapResource(MapNum).ResourceData(Resource_num).Y

                            If Type.Resource(Resource_index).ToolRequired = 0 Then
                                Damage = 1 * GetPlayerGatherSkillLvl(index, ResourceType)
                            Else
                                Damage = Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Data2
                            End If

                            ' check if damage is more than health
                            If Damage > 0 Then
                                ' cut it down!
                               If Type.MapResource(MapNum).ResourceData(Resource_num).Health - Damage <= 0 Then
                                    MapResource(MapNum).ResourceData(Resource_num).State = 1 ' Cut
                                    MapResource(MapNum).ResourceData(Resource_num).Timer = GetTimeMs()
                                    SendMapResourceToMap(MapNum, Resource_num)
                                    SendActionMsg(MapNum, Type.Resource(Resource_index).SuccessMessage, ColorType.BrightGreen, 1, (GetPlayerX(index) * 32), (GetPlayerY(index) * 32))
                                    GiveInv(index, Type.Resource(Resource_index).ItemReward, 1)
                                    SendAnimation(MapNum, Type.Resource(Resource_index).Animation, rX, rY)
                                    SetPlayerGatherSkillExp(index, ResourceType, GetPlayerGatherSkillExp(index, ResourceType) + Type.Resource(Resource_index).ExpReward)
                                    'send msg
                                    PlayerMsg(index, String.Format("Your {0} has earned {1} experience. ({2}/{3})", GetResourceSkillName(ResourceType), Type.Resource(Resource_index).ExpReward, GetPlayerGatherSkillExp(index, ResourceType), GetPlayerGatherSkillMaxExp(index, ResourceType)), ColorType.BrightGreen)
                                    SendPlayerData(index)

                                    CheckResourceLevelUp(index, ResourceType)
                                Else
                                    ' just do the damage
                                    MapResource(MapNum).ResourceData(Resource_num).Health = MapResource(MapNum).ResourceData(Resource_num).Health - Damage
                                    SendActionMsg(MapNum, "-" & Damage, ColorType.BrightRed, 1, (rX * 32), (rY * 32))
                                    SendAnimation(MapNum, Type.Resource(Resource_index).Animation, rX, rY)
                                End If
                            Else
                                ' too weak
                                SendActionMsg(MapNum, "Miss!", ColorType.BrightRed, 1, (rX * 32), (rY * 32))
                            End If
                        Else
                            SendActionMsg(MapNum, Type.Resource(Resource_index).EmptyMessage, ColorType.BrightRed, 1, (GetPlayerX(index) * 32), (GetPlayerY(index) * 32))
                        End If
                    Else
                        PlayerMsg(index, "You have the wrong type of tool equiped.", ColorType.Yellow)
                    End If
                Else
                    PlayerMsg(index, "You need a tool to gather this resource.", ColorType.Yellow)
                End If
            End If
        End If
    End Sub

#End Region

End Module