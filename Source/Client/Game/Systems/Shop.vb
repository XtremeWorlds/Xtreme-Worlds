Imports System.Drawing
Imports Mirage.Sharp.Asfw
Imports Core

Module Shop

#Region "Database"

    Sub ClearShop(index As Integer)
        Type.Shop(index) = Nothing
        Type.Shop(index).Name = ""
        For x = 1 To MAX_TRADES
            ReDim Type.Shop(index).TradeItem(x)
        Next
        GameState.Shop_Loaded(index) = 0
    End Sub

    Sub ClearShops()
        Dim i As Integer

        ReDim Type.Shop(MAX_SHOPS)

       For i = 1 To MAX_SHOPS
            ClearShop(i)
        Next

    End Sub

    Sub StreamShop(shopNum As Integer)
        If shopNum > 0 And Type.Shop(shopNum).Name = "" Or GameState.Shop_Loaded(shopNum) = 0 Then
            GameState.Shop_Loaded(shopNum) = 1
            SendRequestShop(shopNum)
        End If
    End Sub

#End Region

#Region "Incoming Packets"

    Friend Sub Packet_OpenShop(ByRef data() As Byte)
        Dim shopnum As Integer
        Dim buffer As New ByteStream(data)

        shopNum = buffer.ReadInt32

        OpenShop(shopNum)

        buffer.Dispose()
    End Sub

    Friend Sub Packet_ResetShopAction(ByRef data() As Byte)
        GameState.ShopAction = 0
    End Sub

    Friend Sub Packet_UpdateShop(ByRef data() As Byte)
        Dim shopnum As Integer
        Dim buffer As New ByteStream(data)
        shopnum = buffer.ReadInt32

        Type.Shop(shopNum).BuyRate = buffer.ReadInt32()
        Type.Shop(shopNum).Name = buffer.ReadString()

        For i = 1 To MAX_TRADES
            Type.Shop(shopNum).TradeItem(i).CostItem = buffer.ReadInt32()
            Type.Shop(shopNum).TradeItem(i).CostValue = buffer.ReadInt32()
            Type.Shop(shopNum).TradeItem(i).Item = buffer.ReadInt32()
            Type.Shop(shopNum).TradeItem(i).ItemValue = buffer.ReadInt32()
        Next

        If Type.Shop(shopNum).Name Is Nothing Then Type.Shop(shopNum).Name = ""

        buffer.Dispose()
    End Sub

#End Region

#Region "Outgoing Packets"

    Friend Sub SendRequestShop(shopNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestShop)
        buffer.WriteInt32(shopNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub BuyItem(shopSlot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CBuyItem)
        buffer.WriteInt32(shopSlot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SellItem(invslot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSellItem)
        buffer.WriteInt32(invslot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

#End Region

End Module