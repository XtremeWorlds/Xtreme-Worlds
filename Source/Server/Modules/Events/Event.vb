
Imports Mirage.Sharp.Asfw
Imports Core
Imports System.Drawing
Imports Core.Serialization

Friend Module [Event]

#Region "Globals"

    Friend TempEventMap() As GlobalEventsStruct
    Friend Switches() As String
    Friend Variables() As String

    Friend Const PathfindingType As Integer = 1

    'Effect Constants - Used for event options...
    Friend Const EffectTypeFadein As Integer = 2

    Friend Const EffectTypeFadeout As Integer = 1
    Friend Const EffectTypeFlash As Integer = 3
    Friend Const EffectTypeFog As Integer = 4
    Friend Const EffectTypeWeather As Integer = 5
    Friend Const EffectTypeTint As Integer = 6

#End Region

#Region "Database"

    Sub CreateSwitches()
        ReDim Switches(0 To MAX_SWITCHES)

        For i = 1 To MAX_SWITCHES
            Switches(i) = String.Empty
        Next

        SaveSwitches()
    End Sub

    Sub CreateVariables()
        ReDim Variables(0 To NAX_VARIABLES)

        For i = 1 To NAX_VARIABLES
            Variables(i) = String.Empty
        Next

        SaveVariables()
    End Sub

    Sub SaveSwitches()
        Dim json As New JsonSerializer(Of String())()

        json.Write(System.IO.Path.Combine(Core.Path.Database, "Switches.json"), Switches)
    End Sub

    Sub SaveVariables()
        Dim json As New JsonSerializer(Of String())()

        json.Write(System.IO.Path.Combine(Core.Path.Database, "Variables.json"), Variables)
    End Sub

    Sub LoadSwitches()
        Dim json As New JsonSerializer(Of String())()

        Switches = json.Read(System.IO.Path.Combine(Core.Path.Database, "Switches.json"))

        If Switches Is Nothing Then CreateSwitches()
    End Sub

    Sub LoadVariables()
        Dim json As New JsonSerializer(Of String())()

        Variables = json.Read(System.IO.Path.Combine(Core.Path.Database, "Variables.json"))

        If Variables Is Nothing Then CreateVariables()
    End Sub

#End Region

#Region "Movement"

    Function CanEventMove(index As Integer, mapNum As Integer, x As Integer, y As Integer, eventId As Integer, walkThrough As Integer, dir As Byte, Optional globalevent As Boolean = False) As Boolean
        Dim i As Integer
        Dim n As Integer, n2 As Integer, z As Integer, begineventprocessing As Boolean

        ' Check for subscript out of range
       If MapNum <= 0 Or mapNum > MAX_MAPS Or dir <= DirectionType.None Or Dir > DirectionType.Left Then Exit Function

        CanEventMove = 1

        Select Case dir
            Case DirectionType.Up
                ' Check to make sure not outside of boundries
                If y > 0 Then
                    n = Type.Map(MapNum).Tile(x, y - 1).Type
                    n2 = Type.Map(MapNum).Tile(x, y - 1).Type2

                    If walkThrough = 1 Then
                        CanEventMove = 1
                        Exit Function
                    End If

                    ' Check to make sure that the tile is walkable
                    If n = TileType.Blocked Or n2 = TileType.Blocked Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    If n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.Item And n2 <> TileType.NPCSpawn Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = x) And (GetPlayerY(i) = y - 1) Then
                                CanEventMove = 0
                                'There IS a player in the way. But now maybe we can call the event touch thingy!
                                If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).Trigger = 1 Then
                                    begineventprocessing = 1
                                    If begineventprocessing = 1 Then
                                        'Process this event, it is on-touch and everything checks out.
                                        If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount > 0 Then
                                            TempPlayer(index).EventProcessing(eventId).Active = 1
                                            TempPlayer(index).EventProcessing(eventId).ActionTimer = GetTimeMs()
                                            TempPlayer(index).EventProcessing(eventId).CurList = 1
                                            TempPlayer(index).EventProcessing(eventId).CurSlot = 1
                                            TempPlayer(index).EventProcessing(eventId).EventId = eventId
                                            TempPlayer(index).EventProcessing(eventId).PageId = TempPlayer(index).EventMap.EventPages(eventId).PageId
                                            TempPlayer(index).EventProcessing(eventId).WaitingForResponse = 0
                                            ReDim TempPlayer(index).EventProcessing(eventId).ListLeftOff(Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(eventId).EventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount)
                                        End If
                                        begineventprocessing = 0
                                    End If
                                End If
                            End If
                        End If
                    Next

                    If CanEventMove = 0 Then Exit Function
                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).X = x) And (MapNPC(MapNum).NPC(i).Y = y - 1) Then
                            CanEventMove = 0
                            Exit Function
                        End If
                    Next

                    If globalevent = 1 And TempEventMap(MapNum).EventCount > 0 Then
                        For z = 0 To TempEventMap(MapNum).EventCount
                            If (z <> eventId) And (z > 0) And (TempEventMap(MapNum).Event(z).X = x) And (TempEventMap(MapNum).Event(z).Y = y - 1) And (TempEventMap(MapNum).Event(z).WalkThrough = 0) Then
                                CanEventMove = 0
                                Exit Function
                            End If
                        Next
                    Else
                        If TempPlayer(index).EventMap.CurrentEvents > 0 Then
                            For z = 0 To TempPlayer(index).EventMap.CurrentEvents
                                If (TempPlayer(index).EventMap.EventPages(z).EventId <> eventId) And (eventId > 0) And (TempPlayer(index).EventMap.EventPages(z).X = TempPlayer(index).EventMap.EventPages(eventId).X) And (TempPlayer(index).EventMap.EventPages(z).Y = TempPlayer(index).EventMap.EventPages(eventId).Y - 1) And (TempPlayer(index).EventMap.EventPages(z).WalkThrough = 0) Then
                                    CanEventMove = 0
                                    Exit Function
                                End If
                            Next
                        End If
                    End If

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(x, y).DirBlock, DirectionType.Up) Then
                        CanEventMove = 0
                        Exit Function
                    End If
                Else
                    CanEventMove = 0
                End If

            Case DirectionType.Down
                ' Check to make sure not outside of boundries
                If y < Type.Map(MapNum).MaxY Then
                    n = Type.Map(MapNum).Tile(x, y + 1).Type
                    n2 = Type.Map(MapNum).Tile(x, y + 1).Type

                    If walkThrough = 1 Then
                        CanEventMove = 1
                        Exit Function
                    End If

                    ' Check to make sure that the tile is walkable
                    If n = TileType.Blocked Or n2 = TileType.Blocked Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    If n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.Item And n2 <> TileType.NPCSpawn  Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = x) And (GetPlayerY(i) = y + 1) Then
                                CanEventMove = 0
                                'There IS a player in the way. But now maybe we can call the event touch thingy!
                                If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).Trigger = 1 Then
                                    begineventprocessing = 1
                                    If begineventprocessing = 1 Then
                                        'Process this event, it is on-touch and everything checks out.
                                        If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount > 0 Then
                                            TempPlayer(index).EventProcessing(eventId).Active = 1
                                            TempPlayer(index).EventProcessing(eventId).ActionTimer = GetTimeMs()
                                            TempPlayer(index).EventProcessing(eventId).CurList = 1
                                            TempPlayer(index).EventProcessing(eventId).CurSlot = 1
                                            TempPlayer(index).EventProcessing(eventId).EventId = eventId
                                            TempPlayer(index).EventProcessing(eventId).PageId = TempPlayer(index).EventMap.EventPages(eventId).PageId
                                            TempPlayer(index).EventProcessing(eventId).WaitingForResponse = 0
                                            ReDim TempPlayer(index).EventProcessing(eventId).ListLeftOff(Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(eventId).EventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount)
                                        End If
                                        begineventprocessing = 0
                                    End If
                                End If
                            End If
                        End If
                    Next

                    If CanEventMove = 0 Then Exit Function

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).X = x) And (MapNPC(MapNum).NPC(i).Y = y + 1) Then
                            CanEventMove = 0
                            Exit Function
                        End If
                    Next

                    If globalevent = 1 And TempEventMap(MapNum).EventCount > 0 Then
                        For z = 0 To TempEventMap(MapNum).EventCount
                            If (z <> eventId) And (z > 0) And (TempEventMap(MapNum).Event(z).X = x) And (TempEventMap(MapNum).Event(z).Y = y + 1) And (TempEventMap(MapNum).Event(z).WalkThrough = 0) Then
                                CanEventMove = 0
                                Exit Function
                            End If
                        Next
                    Else
                        If TempPlayer(index).EventMap.CurrentEvents > 0 Then
                            For z = 0 To TempPlayer(index).EventMap.CurrentEvents
                                If (TempPlayer(index).EventMap.EventPages(z).EventId <> eventId) And (eventId > 0) And (TempPlayer(index).EventMap.EventPages(z).X = TempPlayer(index).EventMap.EventPages(eventId).X) And (TempPlayer(index).EventMap.EventPages(z).Y = TempPlayer(index).EventMap.EventPages(eventId).Y + 1) And (TempPlayer(index).EventMap.EventPages(z).WalkThrough = 0) Then
                                    CanEventMove = 0
                                    Exit Function
                                End If
                            Next
                        End If
                    End If

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(x, y).DirBlock, DirectionType.Down) Then
                        CanEventMove = 0
                        Exit Function
                    End If
                Else
                    CanEventMove = 0
                End If

            Case DirectionType.Left
                ' Check to make sure not outside of boundries
                If x > 0 Then
                    n = Type.Map(MapNum).Tile(x - 1, y).Type
                    n2 = Type.Map(MapNum).Tile(x - 1, y).Type2

                    If walkThrough = 1 Then
                        CanEventMove = 1
                        Exit Function
                    End If

                    ' Check to make sure that the tile is walkable
                    If n = TileType.Blocked Or n2 = TileType.Blocked Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    If n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.Item And n2 <> TileType.NPCSpawn Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = x - 1) And (GetPlayerY(i) = y) Then
                                CanEventMove = 0
                                'There IS a player in the way. But now maybe we can call the event touch thingy!
                                If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).Trigger = 1 Then
                                    begineventprocessing = 1
                                    If begineventprocessing = 1 Then
                                        'Process this event, it is on-touch and everything checks out.
                                        If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount > 0 Then
                                            TempPlayer(index).EventProcessing(eventId).Active = 1
                                            TempPlayer(index).EventProcessing(eventId).ActionTimer = GetTimeMs()
                                            TempPlayer(index).EventProcessing(eventId).CurList = 1
                                            TempPlayer(index).EventProcessing(eventId).CurSlot = 1
                                            TempPlayer(index).EventProcessing(eventId).EventId = eventId
                                            TempPlayer(index).EventProcessing(eventId).PageId = TempPlayer(index).EventMap.EventPages(eventId).PageId
                                            TempPlayer(index).EventProcessing(eventId).WaitingForResponse = 0
                                            ReDim TempPlayer(index).EventProcessing(eventId).ListLeftOff(Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(eventId).EventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount)
                                        End If
                                        begineventprocessing = 0
                                    End If
                                End If
                            End If
                        End If
                    Next

                    If CanEventMove = 0 Then Exit Function

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).X = x - 1) And (MapNPC(MapNum).NPC(i).Y = y) Then
                            CanEventMove = 0
                            Exit Function
                        End If
                    Next

                    If globalevent = 1 And TempEventMap(MapNum).EventCount > 0 Then
                        For z = 0 To TempEventMap(MapNum).EventCount
                            If (z <> eventId) And (z > 0) And (TempEventMap(MapNum).Event(z).X = x - 1) And (TempEventMap(MapNum).Event(z).Y = y) And (TempEventMap(MapNum).Event(z).WalkThrough = 0) Then
                                CanEventMove = 0
                                Exit Function
                            End If
                        Next
                    Else
                        If TempPlayer(index).EventMap.CurrentEvents > 0 Then
                            For z = 0 To TempPlayer(index).EventMap.CurrentEvents
                                If (TempPlayer(index).EventMap.EventPages(z).EventId <> eventId) And (eventId > 0) And (TempPlayer(index).EventMap.EventPages(z).X = TempPlayer(index).EventMap.EventPages(eventId).X - 1) And (TempPlayer(index).EventMap.EventPages(z).Y = TempPlayer(index).EventMap.EventPages(eventId).Y) And (TempPlayer(index).EventMap.EventPages(z).WalkThrough = 0) Then
                                    CanEventMove = 0
                                    Exit Function
                                End If
                            Next
                        End If
                    End If

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(x, y).DirBlock, DirectionType.Left) Then
                        CanEventMove = 0
                        Exit Function
                    End If
                Else
                    CanEventMove = 0
                End If

            Case DirectionType.Right
                ' Check to make sure not outside of boundries
                If x < Type.Map(MapNum).MaxX Then
                    n = Type.Map(MapNum).Tile(x + 1, y).Type
                    n2 = Type.Map(MapNum).Tile(x + 1, y).Type2

                    If walkThrough = 1 Then
                        CanEventMove = 1
                        Exit Function
                    End If

                    ' Check to make sure that the tile is walkable
                    If n = TileType.Blocked Or n2 = TileType.Blocked Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    If n <> TileType.Item And n <> TileType.NPCSpawn And n2 <> TileType.Item And n2 <> TileType.NPCSpawn Then
                        CanEventMove = 0
                        Exit Function
                    End If

                    ' Check to make sure that there is not a player in the way
                    For i = 1 To Socket.HighIndex
                        If IsPlaying(i) Then
                            If (GetPlayerMap(i) = mapNum) And (GetPlayerX(i) = x + 1) And (GetPlayerY(i) = y) Then
                                CanEventMove = 0
                                'There IS a player in the way. But now maybe we can call the event touch thingy!
                                If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).Trigger = 1 Then
                                    begineventprocessing = 1
                                    If begineventprocessing = 1 Then
                                        'Process this event, it is on-touch and everything checks out.
                                        If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount > 0 Then
                                            TempPlayer(index).EventProcessing(eventId).Active = 1
                                            TempPlayer(index).EventProcessing(eventId).ActionTimer = GetTimeMs()
                                            TempPlayer(index).EventProcessing(eventId).CurList = 1
                                            TempPlayer(index).EventProcessing(eventId).CurSlot = 1
                                            TempPlayer(index).EventProcessing(eventId).EventId = eventId
                                            TempPlayer(index).EventProcessing(eventId).PageId = TempPlayer(index).EventMap.EventPages(eventId).PageId
                                            TempPlayer(index).EventProcessing(eventId).WaitingForResponse = 0
                                            ReDim TempPlayer(index).EventProcessing(eventId).ListLeftOff(Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(eventId).EventId).Pages(TempPlayer(index).EventMap.EventPages(eventId).PageId).CommandListCount)
                                        End If
                                        begineventprocessing = 0
                                    End If
                                End If
                            End If
                        End If
                    Next

                    If CanEventMove = 0 Then Exit Function

                    ' Check to make sure that there is not another npc in the way
                    For i = 1 To MAX_MAP_NPCS
                        If (MapNPC(MapNum).NPC(i).X = x + 1) And (MapNPC(MapNum).NPC(i).Y = y) Then
                            CanEventMove = 0
                            Exit Function
                        End If
                    Next

                    If globalevent = 1 And TempEventMap(MapNum).EventCount > 0 Then
                        For z = 0 To TempEventMap(MapNum).EventCount
                            If (z <> eventId) And (z > 0) And (TempEventMap(MapNum).Event(z).X = x + 1) And (TempEventMap(MapNum).Event(z).Y = y) And (TempEventMap(MapNum).Event(z).WalkThrough = 0) Then
                                CanEventMove = 0
                                Exit Function
                            End If
                        Next
                    Else
                        If TempPlayer(index).EventMap.CurrentEvents > 0 Then
                            For z = 0 To TempPlayer(index).EventMap.CurrentEvents
                                If (TempPlayer(index).EventMap.EventPages(z).EventId <> eventId) And (eventId > 0) And (TempPlayer(index).EventMap.EventPages(z).X = TempPlayer(index).EventMap.EventPages(eventId).X + 1) And (TempPlayer(index).EventMap.EventPages(z).Y = TempPlayer(index).EventMap.EventPages(eventId).Y) And (TempPlayer(index).EventMap.EventPages(z).WalkThrough = 0) Then
                                    CanEventMove = 0
                                    Exit Function
                                End If
                            Next
                        End If
                    End If

                    ' Directional blocking
                    If IsDirBlocked(Type.Map(MapNum).Tile(x, y).DirBlock, DirectionType.Right) Then
                        CanEventMove = 0
                        Exit Function
                    End If
                Else
                    CanEventMove = 0
                End If

        End Select

    End Function

    Sub EventDir(playerindex As Integer, mapNum As Integer, eventId As Integer, dir As Integer, Optional globalevent As Boolean = False)
        Dim buffer As New ByteStream(4)
        Dim eventindex As Integer, i As Integer

        ' Check for subscript out of range
       If MapNum <= 0 Or mapNum > MAX_MAPS Or dir <= DirectionType.None Or Dir > DirectionType.Left Then
            Exit Sub
        End If

        If globalevent = 0 Then
            If TempPlayer(playerindex).EventMap.CurrentEvents > 0 Then
                For i = 0 To TempPlayer(playerindex).EventMap.CurrentEvents
                    If eventId = i Then
                        eventindex = eventId
                        eventId = TempPlayer(playerindex).EventMap.EventPages(i).EventId
                        Exit For
                    End If
                Next
            End If

            If eventindex = 0 Or eventId = 0 Then Exit Sub
        End If

        If globalevent Then
            If Type.Map(MapNum).Event(eventId).Pages(1).DirFix = 0 Then TempEventMap(MapNum).Event(eventId).Dir = dir
        Else
            If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(playerindex).EventMap.EventPages(eventindex).PageId).DirFix = 0 Then TempPlayer(playerindex).EventMap.EventPages(eventindex).Dir = dir
        End If

        buffer.WriteInt32(ServerPackets.SEventDir)
        buffer.WriteInt32(eventId)

        If globalevent Then
            buffer.WriteInt32(TempEventMap(MapNum).Event(eventId).Dir)
        Else
            buffer.WriteInt32(TempPlayer(playerindex).EventMap.EventPages(eventindex).Dir)
        End If

        SendDataToMap(MapNum, buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub EventMove(index As Integer, mapNum As Integer, eventId As Integer, dir As Integer, movementspeed As Integer, Optional globalevent As Boolean = False)
        Dim buffer As New ByteStream(4)
        Dim eventindex As Integer, i As Integer

        ' Check for subscript out of range
       If MapNum <= 0 Or mapNum > MAX_MAPS Or dir <= DirectionType.None Or Dir > DirectionType.Left Then Exit Sub

        If globalevent = 0 Then
            If TempPlayer(index).EventMap.CurrentEvents > 0 Then
                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                    If TempPlayer(index).EventMap.EventPages(i).EventId > 0 Then
                        If eventId = i Then
                            eventindex = eventId
                            eventId = TempPlayer(index).EventMap.EventPages(i).EventId
                            Exit For
                        End If
                    End If
                Next
            End If

            If eventindex = 0 Or eventId = 0 Then Exit Sub
        Else
            eventindex = eventId
            If eventindex = 0 Then Exit Sub
        End If

        If globalevent Then
            If Type.Map(MapNum).Event(eventId).Pages(1).DirFix = 0 Then TempEventMap(MapNum).Event(eventId).Dir = dir
        Else
            If Type.Map(MapNum).Event(eventId).Pages(TempPlayer(index).EventMap.EventPages(eventindex).PageId).DirFix = 0 Then TempPlayer(index).EventMap.EventPages(eventindex).Dir = dir
        End If

        Select Case dir
            Case DirectionType.Up
                If globalevent Then
                    TempEventMap(MapNum).Event(eventindex).Y = TempEventMap(MapNum).Event(eventindex).Y - 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).X)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                Else
                    TempPlayer(index).EventMap.EventPages(eventindex).Y = TempPlayer(index).EventMap.EventPages(eventindex).Y - 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).X)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                End If

            Case DirectionType.Down
                If globalevent Then
                    TempEventMap(MapNum).Event(eventindex).Y = TempEventMap(MapNum).Event(eventindex).Y + 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).X)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                Else
                    TempPlayer(index).EventMap.EventPages(eventindex).Y = TempPlayer(index).EventMap.EventPages(eventindex).Y + 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).X)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                End If
            Case DirectionType.Left
                If globalevent Then
                    TempEventMap(MapNum).Event(eventindex).X = TempEventMap(MapNum).Event(eventindex).X - 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).X)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                Else
                    TempPlayer(index).EventMap.EventPages(eventindex).X = TempPlayer(index).EventMap.EventPages(eventindex).X - 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).X)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                End If
            Case DirectionType.Right
                If globalevent Then
                    TempEventMap(MapNum).Event(eventindex).X = TempEventMap(MapNum).Event(eventindex).X + 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).X)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempEventMap(MapNum).Event(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                Else
                    TempPlayer(index).EventMap.EventPages(eventindex).X = TempPlayer(index).EventMap.EventPages(eventindex).X + 1
                    buffer.WriteInt32(ServerPackets.SEventMove)
                    buffer.WriteInt32(eventId)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).X)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Y)
                    buffer.WriteInt32(dir)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(eventindex).Dir)
                    buffer.WriteInt32(movementspeed)

                    If globalevent Then
                        SendDataToMap(MapNum, buffer.Data, buffer.Head)
                    Else
                        Socket.SendDataTo(index, buffer.Data, buffer.Head)
                    End If
                    buffer.Dispose()
                End If
        End Select

    End Sub

    Function IsOneBlockAway(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer) As Boolean

        If x1 = x2 Then
            If y1 = y2 - 1 Or y1 = y2 + 1 Then
                IsOneBlockAway = 1
            Else
                IsOneBlockAway = 0
            End If
        ElseIf y1 = y2 Then
            If x1 = x2 - 1 Or x1 = x2 + 1 Then
                IsOneBlockAway = 1
            Else
                IsOneBlockAway = 0
            End If
        Else
            IsOneBlockAway = 0
        End If

    End Function

    Function GetNpcDir(x As Integer, y As Integer, x1 As Integer, y1 As Integer) As Integer
        Dim i As Integer, distance As Integer

        i = DirectionType.Right

        If x - x1 > 0 Then
            If x - x1 > distance Then
                i = DirectionType.Right
                distance = x - x1
            End If
        ElseIf x - x1 < 0 Then
            If ((x - x1) * -1) > distance Then
                i = DirectionType.Left
                distance = ((x - x1) * -1)
            End If
        End If

        If y - y1 > 0 Then
            If y - y1 > distance Then
                i = DirectionType.Down
                distance = y - y1
            End If
        ElseIf y - y1 < 0 Then
            If ((y - y1) * -1) > distance Then
                i = DirectionType.Up
                distance = ((y - y1) * -1)
            End If
        End If

        GetNpcDir = i

    End Function

    Function CanEventMoveTowardsPlayer(playerId As Integer, mapNum As Integer, eventId As Integer) As Integer
        Dim i As Integer, x As Integer, y As Integer, x1 As Integer, y1 As Integer, didwalk As Boolean, walkThrough As Integer
        Dim tim As Integer, sX As Integer, sY As Integer, pos(,) As Integer, reachable As Boolean, j As Integer, lastSum As Integer, sum As Integer, fx As Integer, fy As Integer
        Dim path() As Point, lastX As Integer, lastY As Integer, did As Boolean
        'This does not work for global events so this MUST be a player one....

        'This Event returns a direction, 4 is not a valid direction so we assume fail unless otherwise told.
        CanEventMoveTowardsPlayer = 4

        If playerId < 0 Or playerId > MAX_PLAYERS Then Exit Function
       If MapNum <= 0 Or mapNum > MAX_MAPS Then Exit Function
        If eventId < 0 Or eventId > TempPlayer(playerId).EventMap.CurrentEvents Then Exit Function

        x = GetPlayerX(playerId)
        y = GetPlayerY(playerId)
        x1 = TempPlayer(playerId).EventMap.EventPages(eventId).X
        y1 = TempPlayer(playerId).EventMap.EventPages(eventId).Y
        walkThrough = Type.Map(MapNum).Event(TempPlayer(playerId).EventMap.EventPages(eventId).EventId).Pages(TempPlayer(playerId).EventMap.EventPages(eventId).PageId).WalkThrough
        'Add option for pathfinding to random guessing option.

        If PathfindingType = 1 Then
            i = Int(Rnd() * 5)
            didwalk = 0

            ' Lets move the event
            Select Case i
                Case 0
                    ' Up
                    If y1 > y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Up
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Down
                    If y1 < y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Down
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Left
                    If x1 > x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Left
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Right
                    If x1 < x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Right
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                Case 1
                    ' Right
                    If x1 < x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Right
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Left
                    If x1 > x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Left
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Down
                    If y1 < y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Down
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Up
                    If y1 > y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Up
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                Case 2
                    ' Down
                    If y1 < y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Down
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Up
                    If y1 > y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Up
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Right
                    If x1 < x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Right
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Left
                    If x1 > x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Left
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                Case 3
                    ' Left
                    If x1 > x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Left
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Right
                    If x1 < x And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Right
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Up
                    If y1 > y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Up
                            Exit Function
                            didwalk = 1
                        End If
                    End If

                    ' Down
                    If y1 < y And Not didwalk Then
                        If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                            CanEventMoveTowardsPlayer = DirectionType.Down
                            Exit Function
                            didwalk = 1
                        End If
                    End If
            End Select
            CanEventMoveTowardsPlayer = Random.NextDouble(0,3)
        ElseIf PathfindingType = 2 Then
            'Initialization phase
            tim = 0
            sX = x1
            sY = y1
            fx = x
            fy = y

            ReDim pos(Type.Map(MapNum).MaxX, Type.Map(MapNum).MaxY)

            For i = 0 To TempPlayer(playerId).EventMap.CurrentEvents
                If TempPlayer(playerId).EventMap.EventPages(i).Visible = True
                    If TempPlayer(playerId).EventMap.EventPages(i).WalkThrough = 1 Then
                        pos(TempPlayer(playerId).EventMap.EventPages(i).X, TempPlayer(playerId).EventMap.EventPages(i).Y) = 9
                    End If
                End If
            Next

            pos(sX, sY) = 100 + tim
            pos(fx, fy) = 2

            'reset reachable
            reachable = 0

            'Do while reachable is false... if its set true in progress, we jump out
            'If the path is decided unreachable in process, we will use exit sub. Not proper,
            'but faster ;-)
            Do While reachable = 0
                'we loop through all squares
                For j = 0 To Type.Map(MapNum).MaxY
                    For i = 0 To Type.Map(MapNum).MaxX
                        'If j = 10 And i = 0 Then MsgBox "hi!"
                        'If they are to be extended, the pointer TIM is on them
                        If pos(i, j) = 100 + tim Then
                            'The part is to be extended, so do it
                            'We have to make sure that there is a pos(i+1,j) BEFORE we actually use it,
                            'because then we get error... If the square is on side, we dont test for this one!
                            If i < Type.Map(MapNum).MaxX Then
                                'If there isnt a wall, or any other... thing
                                If pos(i + 1, j) = 0 Then
                                    'Expand it, and make its pos equal to tim+1, so the next time we make this loop,
                                    'It will exapand that square too! This is crucial part of the program
                                    pos(i + 1, j) = 100 + tim + 1
                                ElseIf pos(i + 1, j) = 2 Then
                                    'If the position is no 0 but its 2 (FINISH) then Reachable = 1!!! We found end
                                    reachable = 1
                                End If
                            End If

                            'This is the same as the last one, as i said a lot of copy paste work and editing that
                            'This is simply another side that we have to test for... so instead of i+1 we have i-1
                            'Its actually pretty same then... I wont comment it therefore, because its only repeating
                            'same thing with minor changes to check sides
                            If i > 0 Then
                                If pos((i - 1), j) = 0 Then
                                    pos(i - 1, j) = 100 + tim + 1
                                ElseIf pos(i - 1, j) = 2 Then
                                    reachable = 1
                                End If
                            End If

                            If j < Type.Map(MapNum).MaxY Then
                                If pos(i, j + 1) = 0 Then
                                    pos(i, j + 1) = 100 + tim + 1
                                ElseIf pos(i, j + 1) = 2 Then
                                    reachable = 1
                                End If
                            End If

                            If j > 0 Then
                                If pos(i, j - 1) = 0 Then
                                    pos(i, j - 1) = 100 + tim + 1
                                ElseIf pos(i, j - 1) = 2 Then
                                    reachable = 1
                                End If
                            End If
                        End If
                    Next i
                Next j

                'If the reachable is STILL false, then
                If reachable = 0 Then
                    'reset sum
                    sum = 0
                    For j = 0 To Type.Map(MapNum).MaxY
                        For i = 0 To Type.Map(MapNum).MaxX
                            'we add up ALL the squares
                            sum = sum + pos(i, j)
                        Next i
                    Next j

                    'Now if the sum is euqal to the last sum, its not reachable, if it isnt, then we store
                    'sum to lastsum
                    If sum = lastSum Then
                        CanEventMoveTowardsPlayer = 4
                        Exit Function
                    Else
                        lastSum = sum
                    End If
                End If

                'we increase the pointer to point to the next squares to be expanded
                tim = tim + 1
            Loop

            'We work backwards to find the way...
            lastX = fx
            lastY = fy

            ReDim path(tim + 1)

            'The following code may be a little bit confusing but ill try my best to explain it.
            'We are working backwards to find ONE of the shortest ways back to Start.
            'So we repeat the loop until the LastX and LastY arent in start. Look in the code to see
            'how LastX and LasY change
            Do While lastX <> sX Or lastY <> sY
                'We decrease tim by one, and then we are finding any adjacent square to the final one, that
                'has that value. So lets say the tim would be 5, because it takes 5 steps to get to the target.
                'Now everytime we decrease that, so we make it 4, and we look for any adjacent square that has
                'that value. When we find it, we just color it yellow as for the solution
                tim = tim - 1
                'reset did to false
                did = 0

                'If we arent on edge
                If lastX < Type.Map(MapNum).MaxX Then
                    'check the square on the right of the solution. Is it a tim-1 one? or just a blank one
                    If pos(lastX + 1, lastY) = 100 + tim Then
                        'if it, then make it yellow, and change did to true
                        lastX = lastX + 1
                        did = 1
                    End If
                End If

                'This will then only work if the previous part didnt execute, and did is still false. THen
                'we want to check another square, the on left. Is it a tim-1 one ?
                If did = 0 Then
                    If lastX > 0 Then
                        If pos(lastX - 1, lastY) = 100 + tim Then
                            lastX = lastX - 1
                            did = 1
                        End If
                    End If
                End If

                'We check the one below it
                If did = 0 Then
                    If lastY < Type.Map(MapNum).MaxY Then
                        If pos(lastX, lastY + 1) = 100 + tim Then
                            lastY = lastY + 1
                            did = 1
                        End If
                    End If
                End If

                'And above it. One of these have to be it, since we have found the solution, we know that already
                'there is a way back.
                If did = 0 Then
                    If lastY > 0 Then
                        If pos(lastX, lastY - 1) = 100 + tim Then
                            lastY = lastY - 1
                        End If
                    End If
                End If

                path(tim).X = lastX
                path(tim).Y = lastY
            Loop

            'Ok we got a Core.Path. Now, lets look at the first step and see what direction we should take.
            If path(1).X > lastX Then
                CanEventMoveTowardsPlayer = DirectionType.Right
            ElseIf path(1).Y > lastY Then
                CanEventMoveTowardsPlayer = DirectionType.Down
            ElseIf path(1).Y < lastY Then
                CanEventMoveTowardsPlayer = DirectionType.Up
            ElseIf path(1).X < lastX Then
                CanEventMoveTowardsPlayer = DirectionType.Left
            End If

        End If

    End Function

    Function CanEventMoveAwayFromPlayer(playerId As Integer, mapNum As Integer, eventId As Integer) As Integer
        Dim i As Integer, x As Integer, y As Integer, x1 As Integer, y1 As Integer, didwalk As Boolean, walkThrough As Integer
        'This does not work for global events so this MUST be a player one....

        'This Event returns a direction, 5 is not a valid direction so we assume fail unless otherwise told.
        CanEventMoveAwayFromPlayer = 5

        If playerId < 0 Or playerId > MAX_PLAYERS Then Exit Function
       If MapNum <= 0 Or mapNum > MAX_MAPS Then Exit Function
        If eventId < 0 Or eventId > TempPlayer(playerId).EventMap.CurrentEvents Then Exit Function

        x = GetPlayerX(playerId)
        y = GetPlayerY(playerId)
        x1 = TempPlayer(playerId).EventMap.EventPages(eventId).X
        y1 = TempPlayer(playerId).EventMap.EventPages(eventId).Y
        walkThrough = Type.Map(MapNum).Event(TempPlayer(playerId).EventMap.EventPages(eventId).EventId).Pages(TempPlayer(playerId).EventMap.EventPages(eventId).PageId).WalkThrough

        i = Int(Rnd() * 5)
        didwalk = 0

        ' Lets move the event
        Select Case i
            Case 0
                ' Up
                If y1 > y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Down
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Down
                If y1 < y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Up
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Left
                If x1 > x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Right
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Right
                If x1 < x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Left
                        Exit Function
                        didwalk = 1
                    End If
                End If

            Case 1
                ' Right
                If x1 < x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Left
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Left
                If x1 > x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Right
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Down
                If y1 < y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Up
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Up
                If y1 > y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Down
                        Exit Function
                        didwalk = 1
                    End If
                End If

            Case 2
                ' Down
                If y1 < y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Up
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Up
                If y1 > y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Down
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Right
                If x1 < x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Left
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Left
                If x1 > x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Right
                        Exit Function
                        didwalk = 1
                    End If
                End If

            Case 3
                ' Left
                If x1 > x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Right, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Right
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Right
                If x1 < x And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Left, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Left
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Up
                If y1 > y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Down, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Down
                        Exit Function
                        didwalk = 1
                    End If
                End If

                ' Down
                If y1 < y And Not didwalk Then
                    If CanEventMove(playerId, mapNum, x1, y1, eventId, walkThrough, DirectionType.Up, False) Then
                        CanEventMoveAwayFromPlayer = DirectionType.Up
                        Exit Function
                        didwalk = 1
                    End If
                End If

        End Select

        CanEventMoveAwayFromPlayer = Random.NextDouble(0,3)

    End Function

    Function GetDirToPlayer(playerId As Integer, mapNum As Integer, eventId As Integer) As Integer
        Dim i As Integer, x As Integer, y As Integer, x1 As Integer, y1 As Integer, distance As Integer
        'This does not work for global events so this MUST be a player one....

        If playerId < 0 Or playerId > MAX_PLAYERS Then Exit Function
       If MapNum <= 0 Or mapNum > MAX_MAPS Then Exit Function
        If eventId < 0 Or eventId > TempPlayer(playerId).EventMap.CurrentEvents Then Exit Function

        x = GetPlayerX(playerId)
        y = GetPlayerY(playerId)
        x1 = TempPlayer(playerId).EventMap.EventPages(eventId).X
        y1 = TempPlayer(playerId).EventMap.EventPages(eventId).Y

        i = DirectionType.Right

        If x - x1 > 0 Then
            If x - x1 > distance Then
                i = DirectionType.Right
                distance = x - x1
            End If
        ElseIf x - x1 < 0 Then
            If ((x - x1) * -1) > distance Then
                i = DirectionType.Left
                distance = ((x - x1) * -1)
            End If
        End If

        If y - y1 > 0 Then
            If y - y1 > distance Then
                i = DirectionType.Down
                distance = y - y1
            End If
        ElseIf y - y1 < 0 Then
            If ((y - y1) * -1) > distance Then
                i = DirectionType.Up
                distance = ((y - y1) * -1)
            End If
        End If

        GetDirToPlayer = i

    End Function

    Function GetDirAwayFromPlayer(playerId As Integer, mapNum As Integer, eventId As Integer) As Integer
        Dim i As Integer, x As Integer, y As Integer, x1 As Integer, y1 As Integer, distance As Integer
        'This does not work for global events so this MUST be a player one....

        If playerId < 0 Or playerId > MAX_PLAYERS Then Exit Function
       If MapNum <= 0 Or mapNum > MAX_MAPS Then Exit Function
        If eventId < 0 Or eventId > TempPlayer(playerId).EventMap.CurrentEvents Then Exit Function

        x = GetPlayerX(playerId)
        y = GetPlayerY(playerId)
        x1 = TempPlayer(playerId).EventMap.EventPages(eventId).X
        y1 = TempPlayer(playerId).EventMap.EventPages(eventId).Y

        i = DirectionType.Right

        If x - x1 > 0 Then
            If x - x1 > distance Then
                i = DirectionType.Left
                distance = x - x1
            End If
        ElseIf x - x1 < 0 Then
            If ((x - x1) * -1) > distance Then
                i = DirectionType.Right
                distance = ((x - x1) * -1)
            End If
        End If

        If y - y1 > 0 Then
            If y - y1 > distance Then
                i = DirectionType.Up
                distance = y - y1
            End If
        ElseIf y - y1 < 0 Then
            If ((y - y1) * -1) > distance Then
                i = DirectionType.Down
                distance = ((y - y1) * -1)
            End If
        End If

        GetDirAwayFromPlayer = i

    End Function

#End Region

#Region "Incoming Packets"

    Sub Packet_EventChatReply(index As Integer, ByRef data() As Byte)
        Dim eventId As Integer, pageId As Integer, reply As Integer, i As Integer
        Dim buffer As New ByteStream(data)

        eventId = buffer.ReadInt32
        pageId = buffer.ReadInt32
        reply = buffer.ReadInt32

        If TempPlayer(index).EventProcessingCount > 0 Then
            For i = 0 To TempPlayer(index).EventProcessingCount
                If TempPlayer(index).EventProcessing(i).EventId = eventId And TempPlayer(index).EventProcessing(i).PageId = pageId Then
                    If TempPlayer(index).EventProcessing(i).WaitingForResponse = 1 Then
                        If reply = 0 Then
                            If Type.Map(GetPlayerMap(index)).Event(eventId).Pages(pageId).CommandList(TempPlayer(index).EventProcessing(i).CurList).Commands(TempPlayer(index).EventProcessing(i).CurSlot - 1).Index = EventType.ShowText Then
                                TempPlayer(index).EventProcessing(i).WaitingForResponse = 0
                            End If
                        ElseIf reply > 0 Then
                            If Type.Map(GetPlayerMap(index)).Event(eventId).Pages(pageId).CommandList(TempPlayer(index).EventProcessing(i).CurList).Commands(TempPlayer(index).EventProcessing(i).CurSlot - 1).Index = EventType.ShowChoices Then
                                Select Case reply
                                    Case 1
                                        TempPlayer(index).EventProcessing(i).ListLeftOff(TempPlayer(index).EventProcessing(i).CurList) = TempPlayer(index).EventProcessing(i).CurSlot - 1
                                        TempPlayer(index).EventProcessing(i).CurList = Type.Map(GetPlayerMap(index)).Event(eventId).Pages(pageId).CommandList(TempPlayer(index).EventProcessing(i).CurList).Commands(TempPlayer(index).EventProcessing(i).CurSlot - 1).Data1
                                        TempPlayer(index).EventProcessing(i).CurSlot = 1
                                    Case 2
                                        TempPlayer(index).EventProcessing(i).ListLeftOff(TempPlayer(index).EventProcessing(i).CurList) = TempPlayer(index).EventProcessing(i).CurSlot - 1
                                        TempPlayer(index).EventProcessing(i).CurList = Type.Map(GetPlayerMap(index)).Event(eventId).Pages(pageId).CommandList(TempPlayer(index).EventProcessing(i).CurList).Commands(TempPlayer(index).EventProcessing(i).CurSlot - 1).Data2
                                        TempPlayer(index).EventProcessing(i).CurSlot = 1
                                    Case 3
                                        TempPlayer(index).EventProcessing(i).ListLeftOff(TempPlayer(index).EventProcessing(i).CurList) = TempPlayer(index).EventProcessing(i).CurSlot - 1
                                        TempPlayer(index).EventProcessing(i).CurList = Type.Map(GetPlayerMap(index)).Event(eventId).Pages(pageId).CommandList(TempPlayer(index).EventProcessing(i).CurList).Commands(TempPlayer(index).EventProcessing(i).CurSlot - 1).Data3
                                        TempPlayer(index).EventProcessing(i).CurSlot = 1
                                    Case 4
                                        TempPlayer(index).EventProcessing(i).ListLeftOff(TempPlayer(index).EventProcessing(i).CurList) = TempPlayer(index).EventProcessing(i).CurSlot - 1
                                        TempPlayer(index).EventProcessing(i).CurList = Type.Map(GetPlayerMap(index)).Event(eventId).Pages(pageId).CommandList(TempPlayer(index).EventProcessing(i).CurList).Commands(TempPlayer(index).EventProcessing(i).CurSlot - 1).Data4
                                        TempPlayer(index).EventProcessing(i).CurSlot = 1
                                End Select
                            End If
                            TempPlayer(index).EventProcessing(i).WaitingForResponse = 0
                        End If
                    End If
                End If
            Next
        End If

        buffer.Dispose()

    End Sub

    Sub Packet_Event(index As Integer, ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        i = buffer.ReadInt32
        buffer.Dispose()

        TriggerEvent(index, i, 0, GetPlayerX(index), GetPlayerY(index))
    End Sub

    Sub Packet_RequestSwitchesAndVariables(index As Integer, ByRef data() As Byte)
        SendSwitchesAndVariables(index)
    End Sub

    Sub Packet_SwitchesAndVariables(index As Integer, ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_SWITCHES
            Switches(i) = buffer.ReadString
        Next

        For i = 1 To NAX_VARIABLES
            Variables(i) = buffer.ReadString
        Next

        SaveSwitches()
        SaveVariables()

        buffer.Dispose()

        SendSwitchesAndVariables(0, True)

    End Sub

#End Region

#Region "Outgoing Packets"

    Sub SendSpecialEffect(index As Integer, effectType As Integer, Optional data1 As Integer = 0, Optional data2 As Integer = 0, Optional data3 As Integer = 0, Optional data4 As Integer = 0)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SSpecialEffect)
        
        Select Case effectType
            Case EffectTypeFadein
                buffer.WriteInt32(effectType)
            Case EffectTypeFadeout
                buffer.WriteInt32(effectType)
            Case EffectTypeFlash
                buffer.WriteInt32(effectType)
            Case EffectTypeFog
                buffer.WriteInt32(effectType)
                buffer.WriteInt32(data1) 'fognum
                buffer.WriteInt32(data2) 'fog movement speed
                buffer.WriteInt32(data3) 'opacity
            Case EffectTypeWeather
                buffer.WriteInt32(effectType)
                buffer.WriteInt32(data1) 'weather type
                buffer.WriteInt32(data2) 'weather intensity
            Case EffectTypeTint
                buffer.WriteInt32(effectType)
                buffer.WriteInt32(data1) 'red
                buffer.WriteInt32(data2) 'green
                buffer.WriteInt32(data3) 'blue
                buffer.WriteInt32(data4) 'alpha
        End Select

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendSwitchesAndVariables(index As Integer, Optional everyone As Boolean = False)
        Dim buffer As New ByteStream(4), i As Integer

        buffer.WriteInt32(ServerPackets.SSwitchesAndVariables)

        For i = 1 To MAX_SWITCHES
            buffer.WriteString((Switches(i)))
        Next

        For i = 1 To NAX_VARIABLES
            buffer.WriteString((Variables(i)))
        Next

        If everyone Then
            SendDataToAll(buffer.Data, buffer.Head)
        Else
            Socket.SendDataTo(index, buffer.Data, buffer.Head)
        End If

        buffer.Dispose()

    End Sub

    Sub SendMapEventData(index As Integer)
        Dim buffer As New ByteStream(4), i As Integer, x As Integer, y As Integer
        Dim z As Integer, mapNum As Integer, w As Integer

        buffer.WriteInt32(ServerPackets.SMapEventData)
        mapNum = GetPlayerMap(index)
        buffer.WriteInt32(Type.Map(MapNum).EventCount)

        If Type.Map(MapNum).EventCount > 0 Then
            For i = 0 To Type.Map(MapNum).EventCount
                With Type.Map(MapNum).Event(i)
                    buffer.WriteString((.Name))
                    buffer.WriteByte(.Globals)
                    buffer.WriteInt32(.X)
                    buffer.WriteInt32(.Y)
                    buffer.WriteInt32(.PageCount)
                End With
                If Type.Map(MapNum).Event(i).PageCount > 0 Then
                    For x = 0 To Type.Map(MapNum).Event(i).PageCount
                        With Type.Map(MapNum).Event(i).Pages(x)
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
                                For y = 0 To .MoveRouteCount
                                    buffer.WriteInt32(.MoveRoute(y).Index)
                                    buffer.WriteInt32(.MoveRoute(y).Data1)
                                    buffer.WriteInt32(.MoveRoute(y).Data2)
                                    buffer.WriteInt32(.MoveRoute(y).Data3)
                                    buffer.WriteInt32(.MoveRoute(y).Data4)
                                    buffer.WriteInt32(.MoveRoute(y).Data5)
                                    buffer.WriteInt32(.MoveRoute(y).Data6)
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

                        If Type.Map(MapNum).Event(i).Pages(x).CommandListCount > 0 Then
                            For y = 0 To Type.Map(MapNum).Event(i).Pages(x).CommandListCount
                                buffer.WriteInt32(Type.Map(MapNum).Event(i).Pages(x).CommandList(y).CommandCount)
                                buffer.WriteInt32(Type.Map(MapNum).Event(i).Pages(x).CommandList(y).ParentList)
                                If Type.Map(MapNum).Event(i).Pages(x).CommandList(y).CommandCount > 0 Then
                                    For z = 0 To Type.Map(MapNum).Event(i).Pages(x).CommandList(y).CommandCount
                                        With Type.Map(MapNum).Event(i).Pages(x).CommandList(y).Commands(z)
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

        'End Event Data
        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()
        SendSwitchesAndVariables(index)

    End Sub

#End Region

#Region "Misc"

    Friend Sub GivePlayerExp(index As Integer, exp As Integer)
        Dim petnum As Integer

        ' give the exp
        SetPlayerExp(index, GetPlayerExp(index) + exp)
        SendActionMsg(GetPlayerMap(index), "+" & exp & " Exp", ColorType.BrightGreen, 1, (GetPlayerX(index) * 32), (GetPlayerY(index) * 32))
        
        ' check if we've leveled
        CheckPlayerLevelUp(index)

        If PetAlive(index) Then
            petnum = GetPetNum(index)

            If Pet(petnum).LevelingType = 1 Then
                SetPetExp(index, GetPetExp(index) + (exp * (Pet(petnum).ExpGain / 100)))
                SendActionMsg(GetPlayerMap(index), "+" & (exp * (Pet(petnum).ExpGain / 100)) & " Exp", ColorType.BrightGreen, 1, (GetPetX(index) * 32), (GetPetY(index) * 32))
                CheckPetLevelUp(index)
                SendPetExp(index)
            End If
        End If

        SendExp(index)
        SendPlayerData(index)

    End Sub

    Friend Sub CustomScript(index As Integer, caseId As Integer, mapNum As Integer, eventId As Integer)

        Select Case caseId

            Case Else
                PlayerMsg(index, "You just activated custom script " & caseId & ". This script is not yet programmed.", ColorType.BrightRed)
        End Select

    End Sub

#End Region

End Module