Imports System.IO
Imports Mirage.Sharp.Asfw
Imports Core

Module Item

#Region "Database"
    Friend Sub ClearItem(index As Integer)
        Type.Item(index) = Nothing
        For X = 0 To StatType.Count - 1
            ReDim Type.Item(index).Add_Stat(x)
        Next

        For X = 0 To StatType.Count - 1
            ReDim Type.Item(index).Stat_Req(x)
        Next

        Type.Item(index).Name = ""
        Type.Item(index).Description = ""
        GameState.Item_Loaded(index) = 0
    End Sub

    Sub ClearItems()
        Dim i As Integer

        ReDim Type.Item(MAX_ITEMS)

        For i = 1 To MAX_ITEMS
            ClearItem(i)
        Next

    End Sub
    
    Sub StreamItem(itemNum As Integer)
        If itemNum > 0 and Type.Item(itemNum).Name = "" Or GameState.Item_Loaded(itemNum) = 0 Then
            GameState.Item_Loaded(itemNum) = 1
            SendRequestItem(itemNum)
        End If
    End Sub

#End Region

#Region "Incoming Packets"

    Friend Sub Packet_UpdateItem(ByRef data() As Byte)
        Dim n As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        n = buffer.ReadInt32

        ' Update the item
        Type.Item(n).AccessReq = buffer.ReadInt32()

        For i = 1 To StatType.Count - 1
            Type.Item(n).Add_Stat(i) = buffer.ReadInt32()
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

        For i = 1 To StatType.Count - 1
            Type.Item(n).Stat_Req(i) = buffer.ReadInt32()
        Next

        Type.Item(n).Type = buffer.ReadInt32()
        Type.Item(n).SubType = buffer.ReadInt32

        Type.Item(n).KnockBack = buffer.ReadInt32()
        Type.Item(n).KnockBackTiles = buffer.ReadInt32()

        Type.Item(n).Projectile = buffer.ReadInt32()
        Type.Item(n).Ammo = buffer.ReadInt32()

        If n = GameState.descLastItem Then
            GameState.descLastType = 0
            GameState.descLastItem = 0
        End If

        buffer.Dispose()

    End Sub

#End Region

#Region "Outgoing Packets"

    Sub SendRequestItem(itemNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestItem)
        buffer.WriteInt32(itemNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

#End Region

End Module