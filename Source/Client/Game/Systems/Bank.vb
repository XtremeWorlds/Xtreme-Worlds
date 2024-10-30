Imports Core
Imports Mirage.Sharp.Asfw

Module Bank

#Region "Database"

    Sub ClearBanks()
        Dim i As Integer, x As Integer

        For x = 1 To MAX_PLAYERS
            ReDim Type.Bank(x).Item(MAX_BANK)

            For i = 1 To MAX_BANK
                Type.Bank(x).Item(i).Num = 0
                Type.Bank(x).Item(i).Value = 0
            Next
        Next
    End Sub

#End Region

#Region "Incoming Packets"

    Friend Sub Packet_OpenBank(ByRef data() As Byte)
        Dim i As Integer, x As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_BANK
            SetBank(GameState.MyIndex, i, buffer.ReadInt32)
            SetBankValue(GameState.MyIndex, i, buffer.ReadInt32)
        Next

        GameState.InBank = 1

        If Not Gui.Windows(Gui.GetWindowIndex("winBank")).Window.Visible = True
            Gui.ShowWindow(Gui.GetWindowIndex("winBank"), , False)
        End If

        buffer.Dispose()
    End Sub

#End Region

#Region "Outgoing Packets"

    Friend Sub DepositItem(invslot As Integer, amount As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CDepositItem)
        buffer.WriteInt32(invslot)
        buffer.WriteInt32(amount)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub WithdrawItem(bankSlot As Integer, amount As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CWithdrawItem)
        buffer.WriteInt32(bankSlot)
        buffer.WriteInt32(amount)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub ChangeBankSlots(oldSlot As Integer, newSlot As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CChangeBankSlots)
        buffer.WriteInt32(oldSlot)
        buffer.WriteInt32(newSlot)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub CloseBank()
        If Gui.Windows(Gui.GetWindowIndex("winBank")).Window.Visible = True
            Gui.HideWindow(Gui.GetWindowIndex("winBank"))
        End If

        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CCloseBank)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

        GameState.InBank = 0
    End Sub

#End Region

End Module