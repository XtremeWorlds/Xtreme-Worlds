Imports System.Drawing
Imports Mirage.Sharp.Asfw
Imports Core

Module Party

#Region "Database"

    Sub ClearParty()
        Type.Party = New PartyStruct With {
            .Leader = 0,
            .MemberCount = 0
        }
        ReDim Type.Party.Member(MAX_PARTY_MEMBERS)
    End Sub

#End Region

#Region "Incoming Packets"

    Sub Packet_PartyInvite(ByRef data() As Byte)
        Dim name As String
        Dim buffer As New ByteStream(data)

        name = buffer.ReadString
        Dialogue("Party Invite", name & " has invited you to a party.", "Would you like to join?", DialogueType.Party, DialogueStyle.YesNo)

        buffer.Dispose()
    End Sub

    Sub Packet_PartyUpdate(ByRef data() As Byte)
        Dim i As Integer, inParty As Integer
        Dim buffer As New ByteStream(data)

        inParty = buffer.ReadInt32

        ' exit out if we're not in a party
        If inParty = 0 Then
            ClearParty()
            Gui.UpdatePartyInterface
            ' exit out early
            buffer.Dispose()
            Exit Sub
        End If

        ' carry on otherwise
        Type.Party.Leader = buffer.ReadInt32
        For i = 1 To MAX_PARTY_MEMBERS
            Type.Party.Member(I) = buffer.ReadInt32
        Next
        Type.Party.MemberCount = buffer.ReadInt32

        Gui.UpdatePartyInterface

        buffer.Dispose()
    End Sub

    Sub Packet_PartyVitals(ByRef data() As Byte)
        Dim playerNum As Integer, partyindex As Integer
        Dim buffer As New ByteStream(data)

        ' which player?
        playerNum = buffer.ReadInt32

        ' find the party number
        For i = 1 To MAX_PARTY_MEMBERS
            If Type.Party.Member(I) = playerNum Then
                partyindex = I
            End If
        Next

        ' exit out if wrong data
        If partyindex <= 0 Or partyindex > MAX_PARTY_MEMBERS Then Exit Sub

        ' set vitals
        For i = 1 To VitalType.Count - 1
            Type.Player(playerNum).Vital(i) = buffer.ReadInt32
        Next

        UpdatePartyBars

        buffer.Dispose()
    End Sub

#End Region

#Region "Outgoing Packets"

    Friend Sub SendPartyRequest(name As String)
        Dim buffer As New ByteStream(4)
        buffer.WriteInt32(ClientPackets.CRequestParty)
        buffer.WriteString((name))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendAcceptParty()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CAcceptParty)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendDeclineParty()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CDeclineParty)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendLeaveParty()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CLeaveParty)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendPartyChatMsg(text As String)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPartyChatMsg)
        buffer.WriteString((text))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

#End Region

End Module