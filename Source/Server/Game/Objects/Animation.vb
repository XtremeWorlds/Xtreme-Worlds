
Imports Mirage.Sharp.Asfw
Imports Core
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Friend Module Animation

#Region "Database"
    Sub SaveAnimation(animationNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Animation(animationNum)).ToString()

        If RowExists(animationNum, "animation")
            UpdateRow(animationNum, json, "animation", "data")
        Else
            InsertRow(animationNum, json, "animation")
        End If
    End Sub

    Sub LoadAnimations()
        Dim i As Integer

        For i = 1 To MAX_ANIMATIONS
            LoadAnimation(i)
        Next

    End Sub

    Sub LoadAnimation(animationNum As Integer)
        Dim data As JObject

        data = SelectRow(animationNum, "animation", "data")

        If data Is Nothing Then
            ClearAnimation(animationNum)
            Exit Sub
        End If

        Dim animationData = JObject.FromObject(data).toObject(Of AnimationStruct)()
        Type.Animation(animationNum) = animationData
    End Sub

    Sub ClearAnimation(index As Integer)
        Type.Animation(index).Name = ""
        Type.Animation(index).Sound = ""
        ReDim Type.Animation(index).Sprite(1)
        ReDim Type.Animation(index).Frames(1)
        ReDim Type.Animation(index).LoopCount(1)
        ReDim Type.Animation(index).LoopTime(1)
        Type.Animation(index).LoopCount(0) = 1
        Type.Animation(index).LoopCount(1) = 1
        Type.Animation(index).LoopTime(0) = 1
        Type.Animation(index).LoopTime(1) = 1
    End Sub

    Sub ClearAnimations()
         For i = 1 To MAX_ANIMATIONS
            ClearAnimation(i)
        Next
    End Sub

    Function AnimationData(AnimationNum As Integer) As Byte()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(AnimationNum)
        For i = 0 To Type.Animation(AnimationNum).Frames.Length - 1
            buffer.WriteInt32(Type.Animation(AnimationNum).Frames(i))
        Next

        For i = 0 To Type.Animation(AnimationNum).LoopCount.Length - 1
            buffer.WriteInt32(Type.Animation(AnimationNum).LoopCount(i))
        Next

        For i = 0 To Type.Animation(AnimationNum).LoopTime.Length - 1
            buffer.WriteInt32(Type.Animation(AnimationNum).LoopTime(i))
        Next

        buffer.WriteString((Type.Animation(AnimationNum).Name))
        buffer.WriteString((Type.Animation(AnimationNum).Sound))

        For i = 0 To UBound(Type.Animation(AnimationNum).Sprite)
            buffer.WriteInt32(Type.Animation(AnimationNum).Sprite(i))
        Next

        Return buffer.ToArray
    End Function

#End Region

#Region "Incoming Packets"

    Sub Packet_EditAnimation(index As Integer, ByRef data() As Byte)
        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > 0 Then Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Animation)

        If user <> "" Then 
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        TempPlayer(index).Editor = EditorType.Animation

        SendAnimations(index)

        Dim Buffer = New ByteStream(4)
        Buffer.WriteInt32(ServerPackets.SAnimationEditor)
        Socket.SendDataTo(index, Buffer.Data, Buffer.Head)
        Buffer.Dispose()
    End Sub

    Sub Packet_SaveAnimation(index As Integer, ByRef data() As Byte)
        Dim AnimNum As Integer
        Dim buffer As New ByteStream(data)

        AnimNum = buffer.ReadInt32

        ' Update the Animation
        For i = 0 To UBound(Type.Animation(AnimNum).Frames)
            Type.Animation(AnimNum).Frames(i) = buffer.ReadInt32()
        Next

        For i = 0 To UBound(Type.Animation(AnimNum).LoopCount)
            Type.Animation(AnimNum).LoopCount(i) = buffer.ReadInt32()
        Next

        For i = 0 To UBound(Type.Animation(AnimNum).LoopTime)
            Type.Animation(AnimNum).LoopTime(i) = buffer.ReadInt32()
        Next

        Type.Animation(AnimNum).Name = buffer.ReadString()
        Type.Animation(AnimNum).Sound = buffer.ReadString()

        For i = 0 To UBound(Type.Animation(AnimNum).Sprite)
            Type.Animation(AnimNum).Sprite(i) = buffer.ReadInt32()
        Next

        buffer.Dispose()

        ' Save it
        SaveAnimation(AnimNum)
        SendUpdateAnimationToAll(AnimNum)
        Addlog(GetPlayerLogin(index) & " saved Animation #" & AnimNum & ".", ADMIN_LOG)

    End Sub

    Sub Packet_RequestAnimation(index As Integer, ByRef data() As Byte)
        Dim Buffer = New ByteStream(data), n As Integer

        n = Buffer.ReadInt32

        If n <= 0 Or n > MAX_ANIMATIONS Then Exit Sub

        SendUpdateAnimationTo(index, n)
    End Sub

#End Region

#Region "Outgoing Packets"

    Sub SendAnimation(MapNum As Integer, Anim As Integer, X As Integer, Y As Integer, Optional LockType As Byte = 0, Optional Lockindex As Integer = 0)
        Dim buffer As New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SAnimation)
        buffer.WriteInt32(Anim)
        buffer.WriteInt32(X)
        buffer.WriteInt32(Y)
        buffer.WriteInt32(LockType)
        buffer.WriteInt32(Lockindex)

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendAnimations(index As Integer)
        Dim i As Integer

       For i = 1 To MAX_ANIMATIONS

            If Len(Type.Animation(i).Name) > 0 Then
                SendUpdateAnimationTo(index, i)
            End If

        Next

    End Sub

    Sub SendUpdateAnimationTo(index As Integer, AnimationNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateAnimation)

        buffer.WriteBlock(AnimationData(AnimationNum))

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Sub SendUpdateAnimationToAll(AnimationNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateAnimation)

        buffer.WriteBlock(AnimationData(AnimationNum))

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

#End Region

End Module