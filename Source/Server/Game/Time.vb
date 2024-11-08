Imports Mirage.Sharp.Asfw
Imports Core

Module Time

    Sub InitTime()
        ' Add handlers to time events
        AddHandler Core.Time.Instance.OnTimeChanged, AddressOf HandleTimeChanged
        AddHandler Core.Time.Instance.OnTimeOfDayChanged, AddressOf HandleTimeOfDayChanged
        AddHandler Core.Time.Instance.OnTimeSync, AddressOf HandleTimeSync

        ' Prepare the time instance
        Core.Time.Instance.Time = Date.Now
        Core.Time.Instance.GameSpeed = 1
    End Sub

    Sub HandleTimeChanged(ByRef source As Core.Time)
        UpdateCaption()
    End Sub

    Sub HandleTimeOfDayChanged(ByRef source As Core.Time)
        SendTimeToAll()
    End Sub

    Sub HandleTimeSync(ByRef source As Core.Time)
        SendGameClockToAll()
    End Sub

    Sub SendGameClockTo(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SClock)
        buffer.WriteInt32(Core.Time.Instance.GameSpeed)
        buffer.WriteBytes(BitConverter.GetBytes(Core.Time.Instance.Time.Ticks))
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendGameClockToAll()
        Dim i As Integer

        For i = 1 To Socket.HighIndex()
            If IsPlaying(i) Then
                SendGameClockTo(i)
            End If
        Next
    End Sub

    Sub SendTimeTo(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.STime)
        buffer.WriteByte(Core.Time.Instance.TimeOfDay)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendTimeToAll()
        Dim i As Integer

        For i = 1 To Socket.HighIndex()
            If IsPlaying(i) Then
                SendTimeTo(i)
            End If
        Next

    End Sub

End Module