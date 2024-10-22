Imports Core
Imports Mirage.Sharp.Asfw

Module Events

#Region "Globals"

    ' Temp event storage
    Friend TmpEvent As EventStruct

    Friend IsEdit As Boolean

    Friend CurPageNum As Integer
    Friend CurCommand As Integer
    Friend GraphicSelX As Integer
    Friend GraphicSelY As Integer
    Friend GraphicSelX2 As Integer
    Friend GraphicSelY2 As Integer

    Friend EventTileX As Integer
    Friend EventTileY As Integer

    Friend EditorEvent As Integer

    Friend GraphicSelType As Integer
    Friend TempMoveRouteCount As Integer
    Friend TempMoveRoute() As MoveRouteStruct
    Friend IsMoveRouteCommand As Boolean
    Friend ListOfEvents() As Integer

    Friend EventReplyId As Integer
    Friend EventReplyPage As Integer
    Friend EventChatFace As Integer

    Friend RenameType As Integer
    Friend RenameIndex As Integer
    Friend EventChatTimer As Integer

    Friend EventChat As Boolean
    Friend EventText As String
    Friend ShowEventLbl As Boolean
    Friend EventChoices(4) As String
    Friend EventChoiceVisible(4) As Boolean
    Friend EventChatType As Integer
    Friend AnotherChat As Integer

    'constants
    Friend Switches(MAX_SWITCHES) As String
    Friend Variables(NAX_VARIABLES) As String

    Friend EventCopy As Boolean
    Friend EventPaste As Boolean
    Friend EventList() As EventListStruct
    Friend CopyEvent As EventStruct
    Friend CopyEventPage As EventPageStruct

    Friend InEvent As Boolean
    Friend HoldPlayer As Boolean
    Friend InitEventEditorForm As Boolean

    Friend Picture As PictureStruct

#End Region

#Region "Incoming Packets"

    Sub Packet_SpawnEvent(ByRef data() As Byte)
        Dim id As Integer
        Dim buffer As New ByteStream(data)

        id = buffer.ReadInt32

        If id > CurrentEvents Then
            CurrentEvents = id
            ReDim Preserve MapEvents(CurrentEvents)
        End If

        With MapEvents(id)
            .Name = buffer.ReadString
            .Dir = buffer.ReadInt32
            .ShowDir = .Dir
            .GraphicType = buffer.ReadByte
            .Graphic = buffer.ReadInt32
            .GraphicX = buffer.ReadInt32
            .GraphicX2 = buffer.ReadInt32
            .GraphicY = buffer.ReadInt32
            .GraphicY2 = buffer.ReadInt32
            .MovementSpeed = buffer.ReadInt32
            .Moving = 0
            .X = buffer.ReadInt32
            .Y = buffer.ReadInt32
            .XOffset = 0
            .YOffset = 0
            .Position = buffer.ReadByte
            .Visible = buffer.ReadInt32
            .WalkAnim = buffer.ReadInt32
            .DirFix = buffer.ReadInt32
            .WalkThrough = buffer.ReadInt32
            .ShowName = buffer.ReadInt32
            .QuestNum = buffer.ReadInt32
        End With

        buffer.Dispose()

    End Sub

    Sub Packet_EventMove(ByRef data() As Byte)
        Dim id As Integer
        Dim x As Integer
        Dim y As Integer
        Dim dir As Integer, showDir As Integer
        Dim movementSpeed As Integer
        Dim buffer As New ByteStream(data)

        id = buffer.ReadInt32
        x = buffer.ReadInt32
        y = buffer.ReadInt32
        dir = buffer.ReadInt32
        showDir = buffer.ReadInt32
        movementSpeed = buffer.ReadInt32

        If id > CurrentEvents Then Exit Sub

        With MapEvents(id)
            .X = x
            .Y = y
            .Dir = dir
            .XOffset = 0
            .YOffset = 0
            .Moving = 1
            .ShowDir = showDir
            .MovementSpeed = movementSpeed

            Select Case dir
                Case DirectionType.Up
                    .YOffset = PicY
                Case DirectionType.Down
                    .YOffset = PicY * -1
                Case DirectionType.Left
                    .XOffset = PicX
                Case DirectionType.Right
                    .XOffset = PicX * -1
            End Select

        End With

    End Sub

    Sub Packet_EventDir(ByRef data() As Byte)
        Dim i As Integer
        Dim dir As Byte
        Dim buffer As New ByteStream(data)
        i = buffer.ReadInt32
        dir = buffer.ReadInt32

        If i > CurrentEvents Then Exit Sub

        With MapEvents(i)
            .Dir = dir
            .ShowDir = dir
            .XOffset = 0
            .YOffset = 0
            .Moving = 0
        End With

    End Sub

    Sub Packet_SwitchesAndVariables(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_SWITCHES
            Switches(i) = buffer.ReadString
        Next
        For i = 1 To NAX_VARIABLES
            Variables(i) = buffer.ReadString
        Next

        buffer.Dispose()

    End Sub

    Sub Packet_MapEventData(ByRef data() As Byte)
        Dim i As Integer, x As Integer, y As Integer, z As Integer, w As Integer
        Dim buffer As New ByteStream(data)

        MyMap.EventCount = buffer.ReadInt32

        If MyMap.EventCount > 0 Then
            ReDim MyMap.Event(MyMap.EventCount)
            For i = 0 To MyMap.EventCount
                With MyMap.Event(i)
                    .Name = buffer.ReadString
                    .Globals = buffer.ReadByte
                    .X = buffer.ReadInt32
                    .Y = buffer.ReadInt32
                    .PageCount = buffer.ReadInt32
                End With

                If MyMap.Event(i).PageCount > 0 Then
                    ReDim MyMap.Event(i).Pages(MyMap.Event(i).PageCount)
                    For x = 0 To MyMap.Event(i).PageCount
                        With MyMap.Event(i).Pages(x)
                            .ChkVariable = buffer.ReadInt32
                            .VariableIndex = buffer.ReadInt32
                            .VariableCondition = buffer.ReadInt32
                            .VariableCompare = buffer.ReadInt32
                            .ChkSwitch = buffer.ReadInt32
                            .SwitchIndex = buffer.ReadInt32
                            .SwitchCompare = buffer.ReadInt32
                            .ChkHasItem = buffer.ReadInt32
                            .HasItemIndex = buffer.ReadInt32
                            .HasItemAmount = buffer.ReadInt32
                            .ChkSelfSwitch = buffer.ReadInt32
                            .SelfSwitchIndex = buffer.ReadInt32
                            .SelfSwitchCompare = buffer.ReadInt32
                            .GraphicType = buffer.ReadByte
                            .Graphic = buffer.ReadInt32
                            .GraphicX = buffer.ReadInt32
                            .GraphicY = buffer.ReadInt32
                            .GraphicX2 = buffer.ReadInt32
                            .GraphicY2 = buffer.ReadInt32

                            .MoveType = buffer.ReadByte
                            .MoveSpeed = buffer.ReadByte
                            .MoveFreq = buffer.ReadByte
                            .MoveRouteCount = buffer.ReadInt32
                            .IgnoreMoveRoute = buffer.ReadInt32
                            .RepeatMoveRoute = buffer.ReadInt32

                            If .MoveRouteCount > 0 Then
                                ReDim MyMap.Event(i).Pages(x).MoveRoute(.MoveRouteCount)
                                For y = 0 To .MoveRouteCount
                                    .MoveRoute(y).Index = buffer.ReadInt32
                                    .MoveRoute(y).Data1 = buffer.ReadInt32
                                    .MoveRoute(y).Data2 = buffer.ReadInt32
                                    .MoveRoute(y).Data3 = buffer.ReadInt32
                                    .MoveRoute(y).Data4 = buffer.ReadInt32
                                    .MoveRoute(y).Data5 = buffer.ReadInt32
                                    .MoveRoute(y).Data6 = buffer.ReadInt32
                                Next
                            End If

                            .WalkAnim = buffer.ReadInt32
                            .DirFix = buffer.ReadInt32
                            .WalkThrough = buffer.ReadInt32
                            .ShowName = buffer.ReadInt32
                            .Trigger = buffer.ReadByte
                            .CommandListCount = buffer.ReadInt32
                            .Position = buffer.ReadByte
                            .QuestNum = buffer.ReadInt32
                        End With

                        If MyMap.Event(i).Pages(x).CommandListCount > 0 Then
                            ReDim MyMap.Event(i).Pages(x).CommandList(MyMap.Event(i).Pages(x).CommandListCount)
                            For y = 0 To MyMap.Event(i).Pages(x).CommandListCount
                                MyMap.Event(i).Pages(x).CommandList(y).CommandCount = buffer.ReadInt32
                                MyMap.Event(i).Pages(x).CommandList(y).ParentList = buffer.ReadInt32
                                If MyMap.Event(i).Pages(x).CommandList(y).CommandCount > 0 Then
                                    ReDim MyMap.Event(i).Pages(x).CommandList(y).Commands(MyMap.Event(i).Pages(x).CommandList(y).CommandCount)
                                    For z = 0 To MyMap.Event(i).Pages(x).CommandList(y).CommandCount
                                        With MyMap.Event(i).Pages(x).CommandList(y).Commands(z)
                                            .Index = buffer.ReadByte
                                            .Text1 = buffer.ReadString
                                            .Text2 = buffer.ReadString
                                            .Text3 = buffer.ReadString
                                            .Text4 = buffer.ReadString
                                            .Text5 = buffer.ReadString
                                            .Data1 = buffer.ReadInt32
                                            .Data2 = buffer.ReadInt32
                                            .Data3 = buffer.ReadInt32
                                            .Data4 = buffer.ReadInt32
                                            .Data5 = buffer.ReadInt32
                                            .Data6 = buffer.ReadInt32
                                            .ConditionalBranch.CommandList = buffer.ReadInt32
                                            .ConditionalBranch.Condition = buffer.ReadInt32
                                            .ConditionalBranch.Data1 = buffer.ReadInt32
                                            .ConditionalBranch.Data2 = buffer.ReadInt32
                                            .ConditionalBranch.Data3 = buffer.ReadInt32
                                            .ConditionalBranch.ElseCommandList = buffer.ReadInt32
                                            .MoveRouteCount = buffer.ReadInt32

                                            If .MoveRouteCount > 0 Then
                                                ReDim .MoveRoute(.MoveRouteCount)
                                                For w = 0 To .MoveRouteCount
                                                    .MoveRoute(w).Index = buffer.ReadInt32
                                                    .MoveRoute(w).Data1 = buffer.ReadInt32
                                                    .MoveRoute(w).Data2 = buffer.ReadInt32
                                                    .MoveRoute(w).Data3 = buffer.ReadInt32
                                                    .MoveRoute(w).Data4 = buffer.ReadInt32
                                                    .MoveRoute(w).Data5 = buffer.ReadInt32
                                                    .MoveRoute(w).Data6 = buffer.ReadInt32
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

        buffer.Dispose()

    End Sub

    Sub Packet_EventChat(ByRef data() As Byte)
        Dim i As Integer
        Dim choices As Integer
        Dim buffer As New ByteStream(data)
        EventReplyId = buffer.ReadInt32
        EventReplyPage = buffer.ReadInt32
        EventChatFace = buffer.ReadInt32
        EventText = buffer.ReadString
        If EventText = "" Then EventText = " "
        EventChat = True
        ShowEventLbl = True
        choices = buffer.ReadInt32
        InEvent = True
        For i = 1 To 4
            EventChoices(i) = ""
            EventChoiceVisible(i) = False
        Next
        EventChatType = 0
        If choices = 0 Then
        Else
            EventChatType = 1
            For i = 0 To choices
                EventChoices(i) = buffer.ReadString
                EventChoiceVisible(i) = True
            Next
        End If
        AnotherChat = buffer.ReadInt32

        buffer.Dispose()

    End Sub

    Sub Packet_EventStart(ByRef data() As Byte)
        InEvent = True
    End Sub

    Sub Packet_EventEnd(ByRef data() As Byte)
        InEvent = False
    End Sub

    Sub Packet_Picture(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)
        Dim picIndex As Integer, spriteType As Integer, xOffset As Integer, yOffset As Integer, eventid As Integer

        eventid = buffer.ReadInt32
        picIndex = buffer.ReadByte

        If picIndex = 0 Then
            Picture.Index = 0
            Picture.EventId = 0
            Picture.SpriteType = 0
            Picture.xOffset = 0
            Picture.yOffset = 0
            Exit Sub
        End If

        spriteType = buffer.ReadByte
        xOffset = buffer.ReadByte
        yOffset = buffer.ReadByte

        Picture.Index = picIndex
        Picture.EventId = eventid
        Picture.SpriteType = spriteType
        Picture.xOffset = xOffset
        Picture.yOffset = yOffset

        buffer.Dispose()

    End Sub

    Sub Packet_HidePicture(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        Picture = Nothing
    End Sub

    Sub Packet_HoldPlayer(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)
        If buffer.ReadInt32 = 0 Then
            HoldPlayer = True
        Else
            HoldPlayer = False
        End If

        buffer.Dispose()

    End Sub

    Sub Packet_PlayBGM(ByRef data() As Byte)
        Dim music As String
        Dim buffer As New ByteStream(data)

        music = buffer.ReadString
        MyMap.Music = music

        buffer.Dispose()
    End Sub

    Sub Packet_FadeOutBGM(ByRef data() As Byte)
        CurrentMusic = ""
        FadeOutSwitch = True
    End Sub

    Sub Packet_PlaySound(ByRef data() As Byte)
        Dim sound As String
        Dim buffer As New ByteStream(data)
        Dim x As Integer, y As Integer

        sound = buffer.ReadString
        x = buffer.ReadInt32
        y = buffer.ReadInt32

        PlaySound(sound, x, y)

        buffer.Dispose()
    End Sub

    Sub Packet_StopSound(ByRef data() As Byte)
        StopSound()
    End Sub

    Sub Packet_SpecialEffect(ByRef data() As Byte)
        Dim effectType As Integer
        Dim buffer As New ByteStream(data)
        effectType = buffer.ReadInt32

        Select Case effectType
            Case EffectTypeFadein
                UseFade = True
                FadeType = 1
                FadeAmount = 0
            Case EffectTypeFadeout
                UseFade = True
                FadeType = 0
                FadeAmount = 255
            Case EffectTypeFlash
                FlashTimer = GetTickCount() + 150
            Case EffectTypeFog
                CurrentFog = buffer.ReadInt32
                CurrentFogSpeed = buffer.ReadInt32
                CurrentFogOpacity = buffer.ReadInt32
            Case EffectTypeWeather
                CurrentWeather = buffer.ReadInt32
                CurrentWeatherIntensity = buffer.ReadInt32
            Case EffectTypeTint
                MyMap.MapTint = 1
                CurrentTintR = buffer.ReadInt32
                CurrentTintG = buffer.ReadInt32
                CurrentTintB = buffer.ReadInt32
                CurrentTintA = buffer.ReadInt32
        End Select

        buffer.Dispose()
    End Sub

#End Region

#Region "Outgoing Packets"

    Sub RequestSwitchesAndVariables()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestSwitchesAndVariables)
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub SendSwitchesAndVariables()
        Dim i As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSwitchesAndVariables)

        For i = 1 To MAX_SWITCHES
            buffer.WriteString((Switches(i)))
        Next
        For i = 1 To NAX_VARIABLES
            buffer.WriteString((Variables(i)))
        Next

        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

#End Region

#Region "Misc"

    Sub ProcessEventMovement(id As Integer)
        If MyEditorType = EditorType.Map Then Exit Sub
        If id > MyMap.EventCount Then Exit Sub
        If id > MapEvents.Length Then Exit Sub

       If MapEvents(id).Moving = 1 Then
            Select Case MapEvents(id).Dir
                Case DirectionType.Up
                    MapEvents(id).YOffset = MapEvents(id).YOffset - ((ElapsedTime / 1000) * (MapEvents(id).MovementSpeed * SizeY))
                   If MapEvents(id).YOffset < 0 Then MapEvents(id).YOffset = 0
                Case DirectionType.Down
                    MapEvents(id).YOffset = MapEvents(id).YOffset + ((ElapsedTime / 1000) * (MapEvents(id).MovementSpeed * SizeY))
                   If MapEvents(id).YOffset > 0 Then MapEvents(id).YOffset = 0
                Case DirectionType.Left
                    MapEvents(id).XOffset = MapEvents(id).XOffset - ((ElapsedTime / 1000) * (MapEvents(id).MovementSpeed * SizeX))
                   If MapEvents(id).XOffset < 0 Then MapEvents(id).XOffset = 0
                Case DirectionType.Right
                    MapEvents(id).XOffset = MapEvents(id).XOffset + ((ElapsedTime / 1000) * (MapEvents(id).MovementSpeed * SizeX))
                   If MapEvents(id).XOffset > 0 Then MapEvents(id).XOffset = 0
            End Select

            ' Check if completed walking over to the next tile
           If MapEvents(id).Moving > 0 Then
               If MapEvents(id).Dir = DirectionType.Right Or MapEvents(id).Dir = DirectionType.Down Then
                    If (MapEvents(id).XOffset >= 0) And (MapEvents(id).YOffset >= 0) Then
                        MapEvents(id).Moving = 0
                       If MapEvents(id).Steps = 1 Then
                            MapEvents(id).Steps = 3
                        Else
                            MapEvents(id).Steps = 1
                        End If
                    End If
                Else
                    If (MapEvents(id).XOffset <= 0) And (MapEvents(id).YOffset <= 0) Then
                        MapEvents(id).Moving = 0
                       If MapEvents(id).Steps = 1 Then
                            MapEvents(id).Steps = 3
                        Else
                            MapEvents(id).Steps = 1
                        End If
                    End If
                End If
            End If
        End If

    End Sub

    Friend Function GetColorString(color As Integer)

        Select Case color
            Case 0
                GetColorString = "Black"
            Case 1
                GetColorString = "Blue"
            Case 2
                GetColorString = "Green"
            Case 3
                GetColorString = "Cyan"
            Case 4
                GetColorString = "Red"
            Case 5
                GetColorString = "Magenta"
            Case 6
                GetColorString = "Brown"
            Case 7
                GetColorString = "Grey"
            Case 8
                GetColorString = "Dark Grey"
            Case 9
                GetColorString = "Bright Blue"
            Case 10
                GetColorString = "Bright Green"
            Case 11
                GetColorString = "Bright Cyan"
            Case 12
                GetColorString = "Bright Red"
            Case 13
                GetColorString = "Pink"
            Case 14
                GetColorString = "Yellow"
            Case 15
                GetColorString = "White"
            Case Else
                GetColorString = "Black"
        End Select

    End Function

    Sub ClearEventChat()
        Dim i As Integer

        If AnotherChat = 1 Then
            For i = 1 To 4
                EventChoiceVisible(i) = False
            Next
            EventText = ""
            EventChatType = 1
            EventChatTimer = GetTickCount() + 100
        ElseIf AnotherChat = 2 Then
            For i = 1 To 4
                EventChoiceVisible(i) = False
            Next
            EventText = ""
            EventChatType = 1
            EventChatTimer = GetTickCount() + 100
        Else
            EventChat = False
        End If
    End Sub

#End Region

End Module