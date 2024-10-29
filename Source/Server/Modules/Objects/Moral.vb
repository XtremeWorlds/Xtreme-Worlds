Imports Core
Imports Mirage.Sharp.Asfw
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Friend Module Moral

#Region "Database"
    Sub ClearMorals()
        Dim i As Integer

        ReDim Type.Moral(MAX_MORALS)

        For i = 1 To MAX_MORALS
            ClearMoral(i)
        Next
    End Sub

    Sub ClearMoral(moralNum As Integer)
        Type.Moral(moralNum).Name = ""
        Type.Moral(moralNum).Color = 0
        Type.Moral(moralNum).CanCast = 0
        Type.Moral(moralNum).CanDropItem = 0
        Type.Moral(moralNum).CanPK = 0
        Type.Moral(moralNum).CanPickupItem = 0
        Type.Moral(moralNum).CanUseItem = 0
        Type.Moral(moralNum).DropItems = 0
        Type.Moral(moralNum).LoseExp = 0
        Type.Moral(moralNum).NPCBlock = 0
        Type.Moral(moralNum).PlayerBlock = 0
    End Sub

    Sub LoadMoral(moralNum As Integer)
        Dim data As JObject

        data = SelectRow(moralNum, "moral", "data")

        If data Is Nothing Then
            ClearMoral(moralNum)
            Exit Sub
        End If

        Dim moralData = JObject.FromObject(data).ToObject(Of MoralStruct)()
        Type.Moral(moralNum) = moralData
    End Sub

    Sub LoadMorals()
        Dim i As Integer

        For i = 1 To MAX_MORALS
            LoadMoral(i)
        Next
    End Sub

    Sub SaveMoral(moralNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Moral(moralNum)).ToString()

        If RowExists(moralNum, "moral") Then
            UpdateRow(moralNum, json, "moral", "data")
        Else
            InsertRow(moralNum, json, "moral")
        End If
    End Sub

    Sub SaveMorals()
        Dim i As Integer

        For i = 1 To MAX_MORALS
            SaveMoral(i)
        Next
    End Sub

    Function MoralData(moralNum As Integer) As Byte()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(moralNum)
        buffer.WriteString(Type.Moral(moralNum).Name)
        buffer.WriteByte(Type.Moral(moralNum).Color)
        buffer.WriteBoolean(Type.Moral(moralNum).NPCBlock)
        buffer.WriteBoolean(Type.Moral(moralNum).PlayerBlock)
        buffer.WriteBoolean(Type.Moral(moralNum).DropItems)
        buffer.WriteBoolean(Type.Moral(moralNum).CanCast)
        buffer.WriteBoolean(Type.Moral(moralNum).CanDropItem)
        buffer.WriteBoolean(Type.Moral(moralNum).CanPickupItem)
        buffer.WriteBoolean(Type.Moral(moralNum).CanPK)
        buffer.WriteBoolean(Type.Moral(moralNum).DropItems)
        buffer.WriteBoolean(Type.Moral(moralNum).LoseExp)

        Return buffer.ToArray
    End Function

#End Region

#Region "Outgoing Packets"

    Sub SendMorals(index As Integer)
        Dim i As Integer

       For i = 1 To MAX_MORALS
            If Len(Type.Moral(i).Name) > 0 Then
                SendUpdateMoralTo(index, i)
            End If
        Next

    End Sub

    Sub SendUpdateMoralTo(index As Integer, moralNum As Integer)
        Dim buffer As ByteStream
        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdateMoral)

        buffer.WriteBlock(MoralData(moralNum))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateMoralToAll(moralNum As Integer)
        Dim buffer As ByteStream
        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SUpdateMoral)

        buffer.WriteBlock(MoralData(moralNum))

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub


#End Region

#Region "Incoming Packets"
    Sub Packet_RequestEditMoral(index As Integer, ByRef data() As Byte)
        Dim buffer = New ByteStream(4)

        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Moral)

        If user <> "" Then
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        SendMorals(index)

        TempPlayer(index).Editor = EditorType.Moral

        buffer.WriteInt32(ServerPackets.SMoralEditor)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub Packet_SaveMoral(index As Integer, ByRef data() As Byte)
        Dim moralNum As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        moralNum = buffer.ReadInt32()

        ' Prevent hacking
        If moralNum <= 0 Or moralNum > MAX_MORALS Then Exit Sub

        With Type.Moral(moralNum)
            .Name = buffer.ReadString()
            .Color = buffer.ReadByte()
            .CanCast = buffer.ReadBoolean()
            .CanPK = buffer.ReadBoolean()
            .CanDropItem = buffer.ReadBoolean()
            .CanPickupItem = buffer.ReadBoolean()
            .CanUseItem = buffer.ReadBoolean()
            .DropItems = buffer.ReadBoolean()
            .LoseExp = buffer.ReadBoolean()
            .PlayerBlock = buffer.ReadBoolean()
            .NPCBlock = buffer.ReadBoolean()
        End With

        ' Save it
        SendUpdateMoralToAll(moralNum)
        SaveMoral(moralNum)
        Addlog(GetPlayerLogin(index) & " saved moral #" & moralNum & ".", ADMIN_LOG)
        SendMorals(index)
    End Sub

    Sub Packet_RequestMoral(index As Integer, ByRef data() As Byte)
        SendMorals(index)
    End Sub
#End Region
End Module
