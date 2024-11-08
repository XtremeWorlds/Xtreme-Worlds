Imports System.Drawing
Imports Core
Imports Mirage.Sharp.Asfw

Friend Module S_EventLogic

    Friend Sub RemoveDeadEvents()
        Dim i As Integer, mapNum As Integer, x As Integer, id As Integer, page As Integer, compare As Integer

        For i = 1 To Socket.HighIndex()
            If TempPlayer(i).EventMap.CurrentEvents > 0 And TempPlayer(i).GettingMap = 0 Then
                mapNum = GetPlayerMap(i)
                For x = 0 To TempPlayer(i).EventMap.CurrentEvents
                    id = TempPlayer(i).EventMap.EventPages(x).EventId
                    If id > TempPlayer(i).EventMap.CurrentEvents Then Exit For
                    page = TempPlayer(i).EventMap.EventPages(x).PageId

                    If x < id Then Continue For
                    If Type.Map(MapNum).Event(id).PageCount >= page Then
                        'See if there is any reason to delete this event....
                        'In other words, go back through conditions and make sure they all check up.
                        If TempPlayer(i).EventMap.EventPages(x).Visible = True Then
                            If Type.Map(MapNum).Event(id).Pages(page).ChkHasItem = 1 Then
                                If HasItem(i, Type.Map(MapNum).Event(id).Pages(page).HasItemIndex) = 0 Then
                                    TempPlayer(i).EventMap.EventPages(x).Visible = False
                                End If
                            End If

                            If Type.Map(MapNum).Event(id).Pages(page).ChkSelfSwitch = 1 Then
                                If Type.Map(MapNum).Event(id).Pages(page).SelfSwitchCompare = 0 Then
                                    compare = 1
                                Else
                                    compare = 0
                                End If
                                If Type.Map(MapNum).Event(id).Globals = 1 Then
                                    If Type.Map(MapNum).Event(id).SelfSwitches(Type.Map(MapNum).Event(id).Pages(page).SelfSwitchIndex) <> compare Then
                                        TempPlayer(i).EventMap.EventPages(x).Visible = False
                                    End If
                                Else
                                    If TempPlayer(i).EventMap.EventPages(id).SelfSwitches(Type.Map(MapNum).Event(id).Pages(page).SelfSwitchIndex) <> compare Then
                                        TempPlayer(i).EventMap.EventPages(x).Visible = False
                                    End If
                                End If
                            End If

                            If Type.Map(MapNum).Event(id).Pages(page).ChkVariable = 1 Then
                                Select Case Type.Map(MapNum).Event(id).Pages(page).VariableCompare
                                    Case 0
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(page).VariableIndex) <> Type.Map(MapNum).Event(id).Pages(page).VariableCondition Then
                                            TempPlayer(i).EventMap.EventPages(x).Visible = False
                                        End If
                                    Case 1
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(page).VariableIndex) < Type.Map(MapNum).Event(id).Pages(page).VariableCondition Then
                                            TempPlayer(i).EventMap.EventPages(x).Visible = False
                                        End If
                                    Case 2
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(page).VariableIndex) > Type.Map(MapNum).Event(id).Pages(page).VariableCondition Then
                                            TempPlayer(i).EventMap.EventPages(x).Visible = False
                                        End If
                                    Case 3
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(page).VariableIndex) <= Type.Map(MapNum).Event(id).Pages(page).VariableCondition Then
                                            TempPlayer(i).EventMap.EventPages(x).Visible = False
                                        End If
                                    Case 4
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(page).VariableIndex) >= Type.Map(MapNum).Event(id).Pages(page).VariableCondition Then
                                            TempPlayer(i).EventMap.EventPages(x).Visible = False
                                        End If
                                    Case 5
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(page).VariableIndex) = Type.Map(MapNum).Event(id).Pages(page).VariableCondition Then
                                            TempPlayer(i).EventMap.EventPages(x).Visible = False
                                        End If
                                End Select
                            End If

                            If Type.Map(MapNum).Event(id).Pages(page).ChkSwitch = 1 Then
                                If Type.Map(MapNum).Event(id).Pages(page).SwitchCompare = 1 Then 'we expect true
                                    If Type.Player(i).Switches(Type.Map(MapNum).Event(id).Pages(page).SwitchIndex) = 0 Then ' we see false so we despawn the event
                                        TempPlayer(i).EventMap.EventPages(x).Visible = False
                                    End If
                                Else
                                    If Type.Player(i).Switches(Type.Map(MapNum).Event(id).Pages(page).SwitchIndex) = 1 Then ' we expect false and we see true so we despawn the event
                                        TempPlayer(i).EventMap.EventPages(x).Visible = False
                                    End If
                                End If
                            End If

                            If Type.Map(MapNum).Event(id).Globals = 1 And TempPlayer(i).EventMap.EventPages(x).Visible = False Then TempEventMap(MapNum).Event(id).Active = 0

                            If TempPlayer(i).EventMap.EventPages(x).Visible = False And id > 0 Then
                                Dim Buffer As New ByteStream(4)
                                Buffer.WriteInt32(Packets.ServerPackets.SSpawnEvent)
                                Buffer.WriteInt32(id)
                                With TempPlayer(i).EventMap.EventPages(x)
                                    Buffer.WriteString(Type.Map(GetPlayerMap(i)).Event(.EventId).Name)
                                    Buffer.WriteInt32(.Dir)
                                    Buffer.WriteByte(.GraphicType)
                                    Buffer.WriteInt32(.Graphic)
                                    Buffer.WriteInt32(.GraphicX)
                                    Buffer.WriteInt32(.GraphicX2)
                                    Buffer.WriteInt32(.GraphicY)
                                    Buffer.WriteInt32(.GraphicY2)
                                    Buffer.WriteInt32(.MovementSpeed)
                                    Buffer.WriteInt32(.X)
                                    Buffer.WriteInt32(.Y)
                                    Buffer.WriteByte(.Position)
                                    Buffer.WriteInt32(.Visible)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(page).WalkAnim)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(page).DirFix)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(page).WalkThrough)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(page).ShowName)
                                    Buffer.WriteInt32(.QuestNum)
                                End With
                                Socket.SendDataTo(i, Buffer.Data, Buffer.Head)
                                Buffer.Dispose()
                            End If
                        End If
                    End If
                Next
            End If
        Next

    End Sub

    Friend Sub SpawnNewEvents()
        Dim pageID As Integer, id As Integer, compare As Integer, i As Integer, mapNum As Integer
        Dim n As Integer, x As Integer, z As Integer, spawnevent As Boolean, p As Integer

        For i = 1 To Socket.HighIndex()
            If TempPlayer(i).EventMap.CurrentEvents > 0 Then
                mapNum = GetPlayerMap(i)
                For x = 0 To TempPlayer(i).EventMap.CurrentEvents
                    id = TempPlayer(i).EventMap.EventPages(x).EventId
                    If id > 0 And id <= TempPlayer(i).EventMap.CurrentEvents Then
                        pageID = TempPlayer(i).EventMap.EventPages(x).PageId

                        If TempPlayer(i).EventMap.EventPages(x).Visible = False Then pageID = 1

                        If x < id Then Continue For
                        For z = Type.Map(MapNum).Event(id).PageCount To 1 Step -1
                            spawnevent = 1

                            If Type.Map(MapNum).Event(id).Pages(z).ChkHasItem = 1 Then
                                If HasItem(i, Type.Map(MapNum).Event(id).Pages(z).HasItemIndex) = 0 Then
                                    spawnevent = 0
                                End If
                            End If

                            If Type.Map(MapNum).Event(id).Pages(z).ChkSelfSwitch = 1 Then
                                If Type.Map(MapNum).Event(id).Pages(z).SelfSwitchCompare = 0 Then
                                    compare = 1
                                Else
                                    compare = 0
                                End If
                                If Type.Map(MapNum).Event(id).Globals = 1 Then
                                    If Type.Map(MapNum).Event(id).SelfSwitches(Type.Map(MapNum).Event(id).Pages(z).SelfSwitchIndex) <> compare Then
                                        spawnevent = 0
                                    End If
                                Else
                                    If TempPlayer(i).EventMap.EventPages(id).SelfSwitches(Type.Map(MapNum).Event(id).Pages(z).SelfSwitchIndex) <> compare Then
                                        spawnevent = 0
                                    End If
                                End If
                            End If

                            If Type.Map(MapNum).Event(id).Pages(z).ChkVariable = 1 Then
                                Select Case Type.Map(MapNum).Event(id).Pages(z).VariableCompare
                                    Case 0
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(z).VariableIndex) <> Type.Map(MapNum).Event(id).Pages(z).VariableCondition Then
                                            spawnevent = 0
                                        End If
                                    Case 1
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(z).VariableIndex) < Type.Map(MapNum).Event(id).Pages(z).VariableCondition Then
                                            spawnevent = 0
                                        End If
                                    Case 2
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(z).VariableIndex) > Type.Map(MapNum).Event(id).Pages(z).VariableCondition Then
                                            spawnevent = 0
                                        End If
                                    Case 3
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(z).VariableIndex) <= Type.Map(MapNum).Event(id).Pages(z).VariableCondition Then
                                            spawnevent = 0
                                        End If
                                    Case 4
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(z).VariableIndex) >= Type.Map(MapNum).Event(id).Pages(z).VariableCondition Then
                                            spawnevent = 0
                                        End If
                                    Case 5
                                        If Type.Player(i).Variables(Type.Map(MapNum).Event(id).Pages(z).VariableIndex) = Type.Map(MapNum).Event(id).Pages(z).VariableCondition Then
                                            spawnevent = 0
                                        End If
                                End Select
                            End If

                            If Type.Map(MapNum).Event(id).Pages(z).ChkSwitch = 1 Then
                                If Type.Map(MapNum).Event(id).Pages(z).SwitchCompare = 0 Then 'we want false
                                    If Type.Player(i).Switches(Type.Map(MapNum).Event(id).Pages(z).SwitchIndex) = 1 Then 'and switch is true
                                        spawnevent = 0 'do not spawn
                                    End If
                                Else
                                    If Type.Player(i).Switches(Type.Map(MapNum).Event(id).Pages(z).SwitchIndex) = 0 Then ' else we want true and the switch is false
                                        spawnevent = 0
                                    End If
                                End If
                            End If

                            If spawnevent = 1 Then
                                If TempPlayer(i).EventMap.EventPages(x).Visible = True Then
                                    If z <= pageID Then
                                        spawnevent = 0
                                    End If
                                End If
                            End If

                            If spawnevent = 1 Then

                                If TempPlayer(i).EventProcessingCount > 0 Then
                                    For n = 0 To UBound(TempPlayer(i).EventProcessing)
                                        If TempPlayer(i).EventProcessing(n).EventId = id Then
                                            TempPlayer(i).EventProcessing(n).Active = 0
                                        End If
                                    Next
                                End If

                                With TempPlayer(i).EventMap.EventPages(id)
                                    If Type.Map(MapNum).Event(id).Pages(z).GraphicType = 1 Then
                                        Select Case Type.Map(MapNum).Event(id).Pages(z).GraphicY
                                            Case 0
                                                .Dir = DirectionType.Down
                                            Case 1
                                                .Dir = DirectionType.Left
                                            Case 2
                                                .Dir = DirectionType.Right
                                            Case 3
                                                .Dir = DirectionType.Up
                                        End Select
                                    Else
                                        .Dir = 0
                                    End If
                                    .Graphic = Type.Map(MapNum).Event(id).Pages(z).Graphic
                                    .GraphicType = Type.Map(MapNum).Event(id).Pages(z).GraphicType
                                    .GraphicX = Type.Map(MapNum).Event(id).Pages(z).GraphicX
                                    .GraphicY = Type.Map(MapNum).Event(id).Pages(z).GraphicY
                                    .GraphicX2 = Type.Map(MapNum).Event(id).Pages(z).GraphicX2
                                    .GraphicY2 = Type.Map(MapNum).Event(id).Pages(z).GraphicY2
                                    .QuestNum = Type.Map(MapNum).Event(id).Pages(z).QuestNum
                                    Select Case Type.Map(MapNum).Event(id).Pages(z).MoveSpeed
                                        Case 0
                                            .MovementSpeed = 2
                                        Case 1
                                            .MovementSpeed = 3
                                        Case 2
                                            .MovementSpeed = 4
                                        Case 3
                                            .MovementSpeed = 6
                                        Case 4
                                            .MovementSpeed = 12
                                        Case 5
                                            .MovementSpeed = 24
                                    End Select
                                    .Position = Type.Map(MapNum).Event(id).Pages(z).Position
                                    .EventId = id
                                    .PageId = z
                                    .Visible = True

                                    .MoveType = Type.Map(MapNum).Event(id).Pages(z).MoveType
                                    If .MoveType = 2 Then
                                        .MoveRouteCount = Type.Map(MapNum).Event(id).Pages(z).MoveRouteCount
                                        If .MoveRouteCount > 0 Then
                                            ReDim .MoveRoute(Type.Map(MapNum).Event(id).Pages(z).MoveRouteCount)
                                            For p = 0 To Type.Map(MapNum).Event(id).Pages(z).MoveRouteCount
                                                .MoveRoute(p) = Type.Map(MapNum).Event(id).Pages(z).MoveRoute(p)
                                            Next
                                            .MoveRouteComplete = 0
                                        Else
                                            .MoveRouteComplete = 1
                                        End If
                                    Else
                                        .MoveRouteComplete = 1
                                    End If

                                    .RepeatMoveRoute = Type.Map(MapNum).Event(id).Pages(z).RepeatMoveRoute
                                    .IgnoreIfCannotMove = Type.Map(MapNum).Event(id).Pages(z).IgnoreMoveRoute

                                    .MoveFreq = Type.Map(MapNum).Event(id).Pages(z).MoveFreq
                                    .MoveSpeed = Type.Map(MapNum).Event(id).Pages(z).MoveSpeed

                                    .WalkThrough = Type.Map(MapNum).Event(id).Pages(z).WalkThrough
                                    .ShowName = Type.Map(MapNum).Event(id).Pages(z).ShowName
                                    .WalkingAnim = Type.Map(MapNum).Event(id).Pages(z).WalkAnim
                                    .FixedDir = Type.Map(MapNum).Event(id).Pages(z).DirFix

                                End With

                                If Type.Map(MapNum).Event(id).Globals = 1 Then
                                    If spawnevent Then TempEventMap(MapNum).Event(id).Active = z : TempEventMap(MapNum).Event(id).Position = Type.Map(MapNum).Event(id).Pages(z).Position
                                End If

                                Dim Buffer = New ByteStream(4)
                                Buffer.WriteInt32(ServerPackets.SSpawnEvent)
                                Buffer.WriteInt32(id)
                                With TempPlayer(i).EventMap.EventPages(x)
                                    Buffer.WriteString(Type.Map(GetPlayerMap(i)).Event(.EventId).Name)
                                    Buffer.WriteInt32(.Dir)
                                    Buffer.WriteByte(.GraphicType)
                                    Buffer.WriteInt32(.Graphic)
                                    Buffer.WriteInt32(.GraphicX)
                                    Buffer.WriteInt32(.GraphicX2)
                                    Buffer.WriteInt32(.GraphicY)
                                    Buffer.WriteInt32(.GraphicY2)
                                    Buffer.WriteInt32(.MovementSpeed)
                                    Buffer.WriteInt32(.X)
                                    Buffer.WriteInt32(.Y)
                                    Buffer.WriteByte(.Position)
                                    Buffer.WriteInt32(.Visible)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(z).WalkAnim)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(z).DirFix)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(z).WalkThrough)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(z).ShowName)
                                    Buffer.WriteInt32(Type.Map(MapNum).Event(id).Pages(z).QuestNum)
                                    Buffer.WriteInt32(.QuestNum)
                                End With
                                Socket.SendDataTo(i, Buffer.Data, Buffer.Head)

                                Buffer.Dispose()
                                z = 1
                            End If
                        Next
                    End If
                Next
            End If
        Next

    End Sub

    Friend Sub ProcessEventMovement()
        Dim rand As Integer, x As Integer, i As Integer, playerID As Integer, eventID As Integer, WalkThrough As Integer, isglobal As Boolean, mapNum As Integer
        Dim actualmovespeed As Integer, Buffer As ByteStream, z As Integer, sendupdate As Boolean
        Dim donotprocessmoveroute As Boolean, pageNum As Integer

        'Process Movement if needed for each player/each map/each event....

        For i = 1 To MAX_MAPS
            If PlayersOnMap(i) Then
                'Manage Global Events First, then all the others.....
                If TempEventMap(i).EventCount > 0 Then
                    For x = 0 To TempEventMap(i).EventCount
                        If TempEventMap(i).Event(x).Active > 0 Then
                            pageNum = 1
                            If TempEventMap(i).Event(x).MoveTimer <= GetTimeMs() Then
                                'Real event! Lets process it!
                                Select Case TempEventMap(i).Event(x).MoveType
                                    Case 0
                                        'Nothing, fixed position
                                    Case 1 'Random, move randomly if possible...
                                        rand = Random.NextDouble(0,3)
                                        If CanEventMove(0, i, TempEventMap(i).Event(x).X, TempEventMap(i).Event(x).Y, x, TempEventMap(i).Event(x).WalkThrough, rand, True) Then
                                            Select Case TempEventMap(i).Event(x).MoveSpeed
                                                Case 0
                                                    EventMove(0, i, x, rand, 2, True)
                                                Case 1
                                                    EventMove(0, i, x, rand, 3, True)
                                                Case 2
                                                    EventMove(0, i, x, rand, 4, True)
                                                Case 3
                                                    EventMove(0, i, x, rand, 6, True)
                                                Case 4
                                                    EventMove(0, i, x, rand, 12, True)
                                                Case 5
                                                    EventMove(0, i, x, rand, 24, True)
                                            End Select
                                        Else
                                            EventDir(0, i, x, rand, True)
                                        End If
                                    Case 2 'Move Route
                                        With TempEventMap(i).Event(x)
                                            isglobal = 1
                                            mapNum = i
                                            playerID = 0
                                            eventID = x
                                            WalkThrough = TempEventMap(i).Event(x).WalkThrough
                                            If .MoveRouteCount > 0 Then
                                                If .MoveRouteStep >= .MoveRouteCount And .RepeatMoveRoute = 1 Then
                                                    .MoveRouteStep = 0
                                                    .MoveRouteComplete = 1
                                                ElseIf .MoveRouteStep >= .MoveRouteCount And .RepeatMoveRoute = 0 Then
                                                    donotprocessmoveroute = 1
                                                    .MoveRouteComplete = 1
                                                Else
                                                    .MoveRouteComplete = 0
                                                End If
                                                If donotprocessmoveroute = 0 Then
                                                    .MoveRouteStep = .MoveRouteStep + 1
                                                    Select Case .MoveSpeed
                                                        Case 0
                                                            actualmovespeed = 2
                                                        Case 1
                                                            actualmovespeed = 3
                                                        Case 2
                                                            actualmovespeed = 4
                                                        Case 3
                                                            actualmovespeed = 6
                                                        Case 4
                                                            actualmovespeed = 12
                                                        Case 5
                                                            actualmovespeed = 24
                                                    End Select
                                                    Select Case .MoveRoute(.MoveRouteStep).Index
                                                        Case 1
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Up, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Up, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 2
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Down, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Down, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 3
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Left, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Left, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 4
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Right, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Right, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 5
                                                            z = Random.NextDouble(0,3)
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 6
                                                            If isglobal = 0 Then
                                                                If IsOneBlockAway(.X, .Y, GetPlayerX(playerID), GetPlayerY(playerID)) = 1 Then
                                                                    EventDir(playerID, GetPlayerMap(playerID), eventID, GetDirToPlayer(playerID, GetPlayerMap(playerID), eventID), False)
                                                                    If .IgnoreIfCannotMove = 0 Then
                                                                        .MoveRouteStep = .MoveRouteStep - 1
                                                                    End If
                                                                Else
                                                                    z = CanEventMoveTowardsPlayer(playerID, mapNum, eventID)
                                                                    If z >= 4 Then
                                                                        'No
                                                                        If .IgnoreIfCannotMove = 0 Then
                                                                            .MoveRouteStep = .MoveRouteStep - 1
                                                                        End If
                                                                    Else
                                                                        'i is the direct, lets go...
                                                                        If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                            EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                                        Else
                                                                            If .IgnoreIfCannotMove = 0 Then
                                                                                .MoveRouteStep = .MoveRouteStep - 1
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                        Case 7
                                                            If isglobal = 0 Then
                                                                z = CanEventMoveAwayFromPlayer(playerID, mapNum, eventID)
                                                                If z >= 5 Then
                                                                    'No
                                                                Else
                                                                    'i is the direct, lets go...
                                                                    If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                        EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                                    Else
                                                                        If .IgnoreIfCannotMove = 0 Then
                                                                            .MoveRouteStep = .MoveRouteStep - 1
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                        Case 8
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, .Dir, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, .Dir, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 9
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Up
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Right
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Left
                                                            End Select
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 10
                                                            .MoveTimer = GetTimeMs() + 100
                                                        Case 11
                                                            .MoveTimer = GetTimeMs() + 500
                                                        Case 12
                                                            .MoveTimer = GetTimeMs() + 1000
                                                        Case 13
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Up, isglobal)
                                                        Case 14
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Down, isglobal)
                                                        Case 15
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Left, isglobal)
                                                        Case 16
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Right, isglobal)
                                                        Case 17
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Right
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Up
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Left
                                                            End Select
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 18
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Left
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Up
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Right
                                                            End Select
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 19
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Left
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Right
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Up
                                                            End Select
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 20
                                                            z = Random.NextDouble(0,3)
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 21
                                                            If isglobal = 0 Then
                                                                z = GetDirToPlayer(playerID, mapNum, eventID)
                                                                EventDir(playerID, mapNum, eventID, z, isglobal)
                                                            End If
                                                        Case 22
                                                            If isglobal = 0 Then
                                                                z = GetDirAwayFromPlayer(playerID, mapNum, eventID)
                                                                EventDir(playerID, mapNum, eventID, z, isglobal)
                                                            End If
                                                        Case 23
                                                            .MoveSpeed = 0
                                                        Case 24
                                                            .MoveSpeed = 1
                                                        Case 25
                                                            .MoveSpeed = 2
                                                        Case 26
                                                            .MoveSpeed = 3
                                                        Case 27
                                                            .MoveSpeed = 4
                                                        Case 28
                                                            .MoveSpeed = 5
                                                        Case 29
                                                            .MoveFreq = 0
                                                        Case 30
                                                            .MoveFreq = 1
                                                        Case 31
                                                            .MoveFreq = 2
                                                        Case 32
                                                            .MoveFreq = 3
                                                        Case 33
                                                            .MoveFreq = 4
                                                        Case 34
                                                            .WalkingAnim = 1
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 35
                                                            .WalkingAnim = 0
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 36
                                                            .FixedDir = 1
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 37
                                                            .FixedDir = 0
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 38
                                                            .WalkThrough = 1
                                                        Case 39
                                                            .WalkThrough = 0
                                                        Case 40
                                                            .Position = 0
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 41
                                                            .Position = 1
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 42
                                                            .Position = 2
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 43
                                                            .GraphicType = .MoveRoute(.MoveRouteStep).Data1
                                                            .Graphic = .MoveRoute(.MoveRouteStep).Data2
                                                            .GraphicX = .MoveRoute(.MoveRouteStep).Data3
                                                            .GraphicX2 = .MoveRoute(.MoveRouteStep).Data4
                                                            .GraphicY = .MoveRoute(.MoveRouteStep).Data5
                                                            .GraphicY2 = .MoveRoute(.MoveRouteStep).Data6
                                                            If .GraphicType = 1 Then
                                                                Select Case .GraphicY
                                                                    Case 0
                                                                        .Dir = DirectionType.Down
                                                                    Case 1
                                                                        .Dir = DirectionType.Left
                                                                    Case 2
                                                                        .Dir = DirectionType.Right
                                                                    Case 3
                                                                        .Dir = DirectionType.Up
                                                                End Select
                                                            End If
                                                            'Need to Send Update to client
                                                            sendupdate = 1
                                                    End Select

                                                    If sendupdate Then
                                                        Buffer = New ByteStream(4)
                                                        Buffer.WriteInt32(ServerPackets.SSpawnEvent)
                                                        Buffer.WriteInt32(eventID)
                                                        With TempEventMap(i).Event(x)
                                                            Buffer.WriteString(Type.Map(i).Event(x).Name)
                                                            Buffer.WriteInt32(.Dir)
                                                            Buffer.WriteByte(.GraphicType)
                                                            Buffer.WriteInt32(.Graphic)
                                                            Buffer.WriteInt32(.GraphicX)
                                                            Buffer.WriteInt32(.GraphicX2)
                                                            Buffer.WriteInt32(.GraphicY)
                                                            Buffer.WriteInt32(.GraphicY2)
                                                            Buffer.WriteByte(.MoveSpeed)
                                                            Buffer.WriteInt32(.X)
                                                            Buffer.WriteInt32(.Y)
                                                            Buffer.WriteByte(.Position)
                                                            Buffer.WriteInt32(.Active)
                                                            Buffer.WriteInt32(.WalkingAnim)
                                                            Buffer.WriteInt32(.FixedDir)
                                                            Buffer.WriteInt32(.WalkThrough)
                                                            Buffer.WriteInt32(.ShowName)
                                                            Buffer.WriteInt32(.QuestNum)
                                                        End With
                                                        SendDataToMap(i, Buffer.Data, Buffer.Head)
                                                        Buffer.Dispose()
                                                    End If
                                                End If
                                                donotprocessmoveroute = 0
                                            End If
                                        End With
                                End Select

                                Select Case TempEventMap(i).Event(x).MoveFreq
                                    Case 0
                                        TempEventMap(i).Event(x).MoveTimer = GetTimeMs() + 4000
                                    Case 1
                                        TempEventMap(i).Event(x).MoveTimer = GetTimeMs() + 2000
                                    Case 2
                                        TempEventMap(i).Event(x).MoveTimer = GetTimeMs() + 1000
                                    Case 3
                                        TempEventMap(i).Event(x).MoveTimer = GetTimeMs() + 500
                                    Case 4
                                        TempEventMap(i).Event(x).MoveTimer = GetTimeMs() + 250
                                End Select
                            End If
                        End If
                    Next
                End If
            End If
        Next

    End Sub

    Friend Sub ProcessLocalEventMovement()
        Dim rand As Integer, x As Integer, i As Integer, playerID As Integer, eventID As Integer, WalkThrough As Integer
        Dim isglobal As Boolean, mapNum As Integer, actualmovespeed As Integer, Buffer As ByteStream, z As Integer, sendupdate As Boolean
        Dim donotprocessmoveroute As Boolean

        For i = 1 To Socket.HighIndex()
            playerID = i
            If TempPlayer(i).EventMap.CurrentEvents > 0 Then
                For x = 0 To TempPlayer(i).EventMap.CurrentEvents
                    If TempPlayer(i).EventMap.EventPages(x).EventId > UBound(Type.Map(GetPlayerMap(i)).Event) Then Exit For
                    If Type.Map(GetPlayerMap(i)).Event(TempPlayer(i).EventMap.EventPages(x).EventId).Globals = 0 Then
                        If TempPlayer(i).EventMap.EventPages(x).Visible = True Then
                            If TempPlayer(i).EventMap.EventPages(x).MoveTimer <= GetTimeMs() Then
                                'Real event! Lets process it!
                                Select Case TempPlayer(i).EventMap.EventPages(x).MoveType
                                    Case 0
                                        'Nothing, fixed position
                                    Case 1 'Random, move randomly if possible...
                                        rand = Random.NextDouble(0,3)
                                        playerID = i
                                        If CanEventMove(i, GetPlayerMap(i), TempPlayer(i).EventMap.EventPages(x).X, TempPlayer(i).EventMap.EventPages(x).Y, x, TempPlayer(i).EventMap.EventPages(x).WalkThrough, rand, False) Then
                                            Select Case TempPlayer(i).EventMap.EventPages(x).MoveSpeed
                                                Case 0
                                                    EventMove(i, GetPlayerMap(i), x, rand, 2, False)
                                                Case 1
                                                    EventMove(i, GetPlayerMap(i), x, rand, 3, False)
                                                Case 2
                                                    EventMove(i, GetPlayerMap(i), x, rand, 4, False)
                                                Case 3
                                                    EventMove(i, GetPlayerMap(i), x, rand, 6, False)
                                                Case 4
                                                    EventMove(i, GetPlayerMap(i), x, rand, 12, False)
                                                Case 5
                                                    EventMove(i, GetPlayerMap(i), x, rand, 24, False)
                                            End Select
                                        Else
                                            EventDir(0, GetPlayerMap(i), x, rand, True)
                                        End If
                                    Case 2 'Move Route
                                        With TempPlayer(i).EventMap.EventPages(x)
                                            isglobal = 0
                                            sendupdate = 0
                                            mapNum = GetPlayerMap(i)
                                            playerID = i
                                            eventID = x
                                            WalkThrough = .WalkThrough
                                            If TempPlayer(i).EventMap.EventPages(x).MoveRouteCount > 0 Then
                                                If TempPlayer(i).EventMap.EventPages(x).MoveRouteStep >= TempPlayer(i).EventMap.EventPages(x).MoveRouteCount And TempPlayer(i).EventMap.EventPages(x).RepeatMoveRoute = 1 Then
                                                    .MoveRouteStep = 0
                                                    .MoveRouteComplete = 1
                                                ElseIf .MoveRouteStep >= .MoveRouteCount And .RepeatMoveRoute = 0 Then
                                                    donotprocessmoveroute = 1
                                                    .MoveRouteComplete = 1
                                                Else
                                                    .MoveRouteComplete = 0
                                                End If
                                                If donotprocessmoveroute = 0 Then

                                                    Select Case TempPlayer(i).EventMap.EventPages(x).MoveSpeed
                                                        Case 0
                                                            actualmovespeed = 2
                                                        Case 1
                                                            actualmovespeed = 3
                                                        Case 2
                                                            actualmovespeed = 4
                                                        Case 3
                                                            actualmovespeed = 6
                                                        Case 4
                                                            actualmovespeed = 12
                                                        Case 5
                                                            actualmovespeed = 24
                                                    End Select
                                                    TempPlayer(i).EventMap.EventPages(x).MoveRouteStep = TempPlayer(i).EventMap.EventPages(x).MoveRouteStep + 1
                                                    Select Case TempPlayer(i).EventMap.EventPages(x).MoveRoute(TempPlayer(i).EventMap.EventPages(x).MoveRouteStep).Index
                                                        Case 1
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Up, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Up, actualmovespeed, isglobal)
                                                            Else
                                                                If TempPlayer(i).EventMap.EventPages(x).IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 2
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Down, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Down, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 3
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Left, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Left, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 4
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, DirectionType.Right, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, DirectionType.Right, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 5
                                                            z = Random.NextDouble(0,3)
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 6
                                                            If isglobal = 0 Then
                                                                If IsOneBlockAway(.X, .Y, GetPlayerX(playerID), GetPlayerY(playerID)) = 1 Then
                                                                    EventDir(playerID, GetPlayerMap(playerID), eventID, GetDirToPlayer(playerID, GetPlayerMap(playerID), eventID), False)
                                                                    'Lets do cool stuff!
                                                                   If Type.Map(GetPlayerMap(playerID)).Event(eventID).Pages(TempPlayer(playerID).EventMap.EventPages(eventID).PageId).Trigger = 1 Then
                                                                        If Type.Map(MapNum).Event(eventID).Pages(TempPlayer(playerID).EventMap.EventPages(eventID).PageId).CommandListCount > 0 Then
                                                                            TempPlayer(playerID).EventProcessing(eventID).Active = 1
                                                                            TempPlayer(playerID).EventProcessing(eventID).ActionTimer = GetTimeMs()
                                                                            TempPlayer(playerID).EventProcessing(eventID).CurList = 1
                                                                            TempPlayer(playerID).EventProcessing(eventID).CurSlot = 1
                                                                            TempPlayer(playerID).EventProcessing(eventID).EventId = eventID
                                                                            TempPlayer(playerID).EventProcessing(eventID).PageId = TempPlayer(playerID).EventMap.EventPages(eventID).PageId
                                                                            TempPlayer(playerID).EventProcessing(eventID).WaitingForResponse = 0
                                                                            ReDim TempPlayer(playerID).EventProcessing(eventID).ListLeftOff(Type.Map(GetPlayerMap(playerID)).Event(TempPlayer(playerID).EventMap.EventPages(eventID).EventId).Pages(TempPlayer(playerID).EventMap.EventPages(eventID).PageId).CommandListCount)
                                                                        End If
                                                                    End If
                                                                    If .IgnoreIfCannotMove = 0 Then
                                                                        .MoveRouteStep = .MoveRouteStep - 1
                                                                    End If
                                                                Else
                                                                    z = CanEventMoveTowardsPlayer(playerID, mapNum, eventID)
                                                                    If z >= 4 Then
                                                                        'No
                                                                        If .IgnoreIfCannotMove = 0 Then
                                                                            .MoveRouteStep = .MoveRouteStep - 1
                                                                        End If
                                                                    Else
                                                                        'i is the direct, lets go...
                                                                        If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                            EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                                        Else
                                                                            If .IgnoreIfCannotMove = 0 Then
                                                                                .MoveRouteStep = .MoveRouteStep - 1
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                        Case 7
                                                            If isglobal = 0 Then
                                                                z = CanEventMoveAwayFromPlayer(playerID, mapNum, eventID)
                                                                If z >= 5 Then
                                                                    'No
                                                                Else
                                                                    'i is the direct, lets go...
                                                                    If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                        EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                                    Else
                                                                        If .IgnoreIfCannotMove = 0 Then
                                                                            .MoveRouteStep = .MoveRouteStep - 1
                                                                        End If
                                                                    End If
                                                                End If
                                                            End If
                                                        Case 8
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, .Dir, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, .Dir, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 9
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Up
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Right
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Left
                                                            End Select
                                                            If CanEventMove(playerID, mapNum, .X, .Y, eventID, WalkThrough, z, isglobal) Then
                                                                EventMove(playerID, mapNum, eventID, z, actualmovespeed, isglobal)
                                                            Else
                                                                If .IgnoreIfCannotMove = 0 Then
                                                                    .MoveRouteStep = .MoveRouteStep - 1
                                                                End If
                                                            End If
                                                        Case 10
                                                            .MoveTimer = GetTimeMs() + 100
                                                        Case 11
                                                            .MoveTimer = GetTimeMs() + 500
                                                        Case 12
                                                            .MoveTimer = GetTimeMs() + 1000
                                                        Case 13
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Up, isglobal)
                                                        Case 14
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Down, isglobal)
                                                        Case 15
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Left, isglobal)
                                                        Case 16
                                                            EventDir(playerID, mapNum, eventID, DirectionType.Right, isglobal)
                                                        Case 17
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Right
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Up
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Left
                                                            End Select
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 18
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Left
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Up
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Right
                                                            End Select
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 19
                                                            Select Case .Dir
                                                                Case DirectionType.Up
                                                                    z = DirectionType.Down
                                                                Case DirectionType.Right
                                                                    z = DirectionType.Left
                                                                Case DirectionType.Left
                                                                    z = DirectionType.Right
                                                                Case DirectionType.Down
                                                                    z = DirectionType.Up
                                                            End Select
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 20
                                                            z = Random.NextDouble(0,3)
                                                            EventDir(playerID, mapNum, eventID, z, isglobal)
                                                        Case 21
                                                            If isglobal = 0 Then
                                                                z = GetDirToPlayer(playerID, mapNum, eventID)
                                                                EventDir(playerID, mapNum, eventID, z, isglobal)
                                                            End If
                                                        Case 22
                                                            If isglobal = 0 Then
                                                                z = GetDirAwayFromPlayer(playerID, mapNum, eventID)
                                                                EventDir(playerID, mapNum, eventID, z, isglobal)
                                                            End If
                                                        Case 23
                                                            .MoveSpeed = 0
                                                        Case 24
                                                            .MoveSpeed = 1
                                                        Case 25
                                                            .MoveSpeed = 2
                                                        Case 26
                                                            .MoveSpeed = 3
                                                        Case 27
                                                            .MoveSpeed = 4
                                                        Case 28
                                                            .MoveSpeed = 5
                                                        Case 29
                                                            .MoveFreq = 0
                                                        Case 30
                                                            .MoveFreq = 1
                                                        Case 31
                                                            .MoveFreq = 2
                                                        Case 32
                                                            .MoveFreq = 3
                                                        Case 33
                                                            .MoveFreq = 4
                                                        Case 34
                                                            .WalkingAnim = 1
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 35
                                                            .WalkingAnim = 0
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 36
                                                            .FixedDir = 1
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 37
                                                            .FixedDir = 0
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 38
                                                            .WalkThrough = 1
                                                        Case 39
                                                            .WalkThrough = 0
                                                        Case 40
                                                            .Position = 0
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 41
                                                            .Position = 1
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 42
                                                            .Position = 2
                                                            'Need to send update to client
                                                            sendupdate = 1
                                                        Case 43
                                                            .GraphicType = .MoveRoute(.MoveRouteStep).Data1
                                                            .Graphic = .MoveRoute(.MoveRouteStep).Data2
                                                            .GraphicX = .MoveRoute(.MoveRouteStep).Data3
                                                            .GraphicX2 = .MoveRoute(.MoveRouteStep).Data4
                                                            .GraphicY = .MoveRoute(.MoveRouteStep).Data5
                                                            .GraphicY2 = .MoveRoute(.MoveRouteStep).Data6
                                                            If .GraphicType = 1 Then
                                                                Select Case .GraphicY
                                                                    Case 0
                                                                        .Dir = DirectionType.Down
                                                                    Case 1
                                                                        .Dir = DirectionType.Left
                                                                    Case 2
                                                                        .Dir = DirectionType.Right
                                                                    Case 3
                                                                        .Dir = DirectionType.Up
                                                                End Select
                                                            End If
                                                            'Need to Send Update to client
                                                            sendupdate = 1
                                                    End Select

                                                    If sendupdate And TempPlayer(playerID).EventMap.EventPages(eventID).EventId > 0 Then
                                                        Buffer = New ByteStream(4)
                                                        Buffer.WriteInt32(ServerPackets.SSpawnEvent)
                                                        Buffer.WriteInt32(TempPlayer(playerID).EventMap.EventPages(eventID).EventId)
                                                        With TempPlayer(playerID).EventMap.EventPages(eventID)
                                                            Buffer.WriteString((Type.Map(GetPlayerMap(playerID)).Event(TempPlayer(playerID).EventMap.EventPages(eventID).EventId).Name))
                                                            Buffer.WriteInt32(.Dir)
                                                            Buffer.WriteByte(.GraphicType)
                                                            Buffer.WriteInt32(.Graphic)
                                                            Buffer.WriteInt32(.GraphicX)
                                                            Buffer.WriteInt32(.GraphicX2)
                                                            Buffer.WriteInt32(.GraphicY)
                                                            Buffer.WriteInt32(.GraphicY2)
                                                            Buffer.WriteInt32(.MoveSpeed)
                                                            Buffer.WriteInt32(.X)
                                                            Buffer.WriteInt32(.Y)
                                                            Buffer.WriteByte(.Position)
                                                            Buffer.WriteInt32(.Visible)
                                                            Buffer.WriteInt32(.WalkingAnim)
                                                            Buffer.WriteInt32(.FixedDir)
                                                            Buffer.WriteInt32(.WalkThrough)
                                                            Buffer.WriteInt32(.ShowName)
                                                            Buffer.WriteInt32(.QuestNum)
                                                        End With
                                                        Socket.SendDataTo(playerID, Buffer.Data, Buffer.Head)
                                                        Buffer.Dispose()
                                                    End If
                                                End If
                                                donotprocessmoveroute = 0
                                            End If
                                        End With
                                End Select
                                Select Case TempPlayer(playerID).EventMap.EventPages(x).MoveFreq
                                    Case 0
                                        TempPlayer(playerID).EventMap.EventPages(x).MoveTimer = GetTimeMs() + 4000
                                    Case 1
                                        TempPlayer(playerID).EventMap.EventPages(x).MoveTimer = GetTimeMs() + 2000
                                    Case 2
                                        TempPlayer(playerID).EventMap.EventPages(x).MoveTimer = GetTimeMs() + 1000
                                    Case 3
                                        TempPlayer(playerID).EventMap.EventPages(x).MoveTimer = GetTimeMs() + 500
                                    Case 4
                                        TempPlayer(playerID).EventMap.EventPages(x).MoveTimer = GetTimeMs() + 250
                                End Select
                            End If
                        End If
                    End If
                Next
            End If
        Next

    End Sub

    Friend Sub ProcessEventCommands()
        Dim buffer As New ByteStream(4), i As Integer, x As Integer, removeEventProcess As Boolean, w As Integer, v As Integer, p As Integer
        Dim restartlist As Boolean, restartloop As Boolean, endprocess As Boolean

        'Now, we process the damn things for commands :P
        For i = 1 To Socket.HighIndex()
            If IsPlaying(i) Then
                If TempPlayer(i).GettingMap = 0 Then
                    If TempPlayer(i).EventMap.CurrentEvents > 0 Then
                        For x = 1 To TempPlayer(i).EventMap.CurrentEvents
                            If TempPlayer(i).EventProcessingCount > 0 Then
                                If TempPlayer(i).EventMap.EventPages(x).Visible = True Then
                                    If x < TempPlayer(i).EventMap.EventPages(x).EventId Then Continue For
                                    If Type.Map(Type.Player(i).Map).Event(Type.TempPlayer(i).EventMap.EventPages(x).EventId).Pages(Type.TempPlayer(i).EventMap.EventPages(x).PageId).Trigger = 2 Then 'Parallel Process baby!

                                        If TempPlayer(i).EventProcessing(x).Active = 0 Then
                                            If Type.Map(GetPlayerMap(i)).Event(TempPlayer(i).EventMap.EventPages(x).EventId).Pages(TempPlayer(i).EventMap.EventPages(x).PageId).CommandListCount > 0 Then
                                                'start new event processing
                                                TempPlayer(i).EventProcessing(TempPlayer(i).EventMap.EventPages(x).EventId).Active = 1
                                                With TempPlayer(i).EventProcessing(TempPlayer(i).EventMap.EventPages(x).EventId)
                                                    .ActionTimer = GetTimeMs()
                                                    .CurList = 1
                                                    .CurSlot = 1
                                                    .EventId = TempPlayer(i).EventMap.EventPages(x).EventId
                                                    .PageId = TempPlayer(i).EventMap.EventPages(x).PageId
                                                    .WaitingForResponse = 0
                                                    ReDim .ListLeftOff(Type.Map(GetPlayerMap(i)).Event(TempPlayer(i).EventMap.EventPages(x).EventId).Pages(TempPlayer(i).EventMap.EventPages(x).PageId).CommandListCount)
                                                End With
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
            End If
        Next

        'That is it for starting parallel processes :D now we just have to make the code that actually processes the events to their fullest
        For i = 1 To Socket.HighIndex()
            If IsPlaying(i) Then
                If TempPlayer(i).EventProcessingCount > 0 Then
                    If TempPlayer(i).GettingMap = 0 Then
                        restartloop = 1
                        Do While restartloop = 1
                            restartloop = 0
                            For x = 1 To TempPlayer(i).EventProcessingCount
                                If TempPlayer(i).EventProcessing(x).Active = 1 Then
                                    With TempPlayer(i).EventProcessing(x)
                                        If TempPlayer(i).EventProcessingCount = 0 Then Exit Sub
                                        removeEventProcess = 0
                                        If .WaitingForResponse = 2 Then
                                            If TempPlayer(i).InShop = 0 Then
                                                .WaitingForResponse = 0
                                            End If
                                        End If
                                        If .WaitingForResponse = 3 Then
                                            If TempPlayer(i).InBank = 0 Then
                                                .WaitingForResponse = 0
                                            End If
                                        End If
                                        If .WaitingForResponse = 4 Then
                                            'waiting for eventmovement to complete
                                            If .EventMovingType = 0 Then
                                                If TempPlayer(i).EventMap.EventPages(.EventMovingId).MoveRouteComplete = 1 Then
                                                    .WaitingForResponse = 0
                                                End If
                                            Else
                                                If TempEventMap(GetPlayerMap(i)).Event(.EventMovingId).MoveRouteComplete = 1 Then
                                                    .WaitingForResponse = 0
                                                End If
                                            End If
                                        End If

                                        If .WaitingForResponse = 0 Then
                                            If .ActionTimer <= GetTimeMs() Then
                                                restartlist = 1
                                                endprocess = 0
                                                Do While restartlist = 1 And endprocess = 0 And .WaitingForResponse = 0
                                                    restartlist = 0
                                                    If .ListLeftOff(.CurList) > 0 Then
                                                        .CurSlot = .ListLeftOff(.CurList) + 1
                                                        .ListLeftOff(.CurList) = 0
                                                    End If

                                                    If .CurList > Type.Map(Type.Player(i).Map).Event(.EventId).Pages(.PageId).CommandListCount Then
                                                        'Get rid of this event, it is bad
                                                        removeEventProcess = 1
                                                        endprocess = 1
                                                    End If

                                                    If .CurSlot > Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).CommandCount Then
                                                        If .CurList = 1 Then
                                                            'Get rid of this event, it is bad
                                                            removeEventProcess = 1
                                                            endprocess = 1
                                                        Else
                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).ParentList
                                                            .CurSlot = 1
                                                            restartlist = 1
                                                        End If
                                                    End If

                                                    If restartlist = 0 And endprocess = 0 Then
                                                        'If we are still here, then we are good to process shit :D
                                                        Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Index
                                                            Case EventType.AddText
                                                                Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2
                                                                    Case 0
                                                                        PlayerMsg(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                    Case 1
                                                                        MapMsg(GetPlayerMap(i), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                    Case 2
                                                                        GlobalMsg(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1) ' Type.Map(GetPlayerMap(i)).Events(.EventID).Pages(.PageID).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                End Select
                                                            Case EventType.ShowText
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SEventChat)
                                                                buffer.WriteInt32(.EventId)
                                                                buffer.WriteInt32(.PageId)
                                                                buffer.WriteInt32(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                buffer.WriteString((ParseEventText(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1)))
                                                                buffer.WriteInt32(0)

                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).CommandCount > .CurSlot Then
                                                                    If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot + 1).Index = EventType.ShowText Or Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot + 1).Index = EventType.ShowChoices Then
                                                                        buffer.WriteInt32(1)
                                                                    ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot + 1).Index = EventType.Condition Then
                                                                        buffer.WriteInt32(2)
                                                                    Else
                                                                        buffer.WriteInt32(0)
                                                                    End If
                                                                Else
                                                                    buffer.WriteInt32(2)
                                                                End If
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)
                                                                buffer.Dispose()
                                                                .WaitingForResponse = 1
                                                            Case EventType.ShowChoices
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SEventChat)
                                                                buffer.WriteInt32(.EventId)
                                                                buffer.WriteInt32(.PageId)
                                                                buffer.WriteInt32(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data5)
                                                                buffer.WriteString((ParseEventText(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1)))

                                                                If Len(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text2) > 0 Then
                                                                    w = 1
                                                                    If Len(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text3) > 0 Then
                                                                        w = 2
                                                                        If Len(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text4) > 0 Then
                                                                            w = 3
                                                                            If Len(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text5) > 0 Then
                                                                                w = 4
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                                buffer.WriteInt32(w)
                                                                For v = 0 To w
                                                                    Select Case v
                                                                        Case 1
                                                                            buffer.WriteString((ParseEventText(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text2)))
                                                                        Case 2
                                                                            buffer.WriteString((ParseEventText(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text3)))
                                                                        Case 3
                                                                            buffer.WriteString((ParseEventText(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text4)))
                                                                        Case 4
                                                                            buffer.WriteString((ParseEventText(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text5)))
                                                                    End Select
                                                                Next
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).CommandCount > .CurSlot Then
                                                                    If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot + 1).Index = EventType.ShowText Or Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot + 1).Index = EventType.ShowChoices Then
                                                                        buffer.WriteInt32(1)
                                                                    ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot + 1).Index = EventType.Condition Then
                                                                        buffer.WriteInt32(2)
                                                                    Else
                                                                        buffer.WriteInt32(0)
                                                                    End If
                                                                Else
                                                                    buffer.WriteInt32(2)
                                                                End If
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)
                                                                buffer.Dispose()
                                                                .WaitingForResponse = 1
                                                            Case EventType.PlayerVar
                                                                Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2
                                                                    Case 0
                                                                        Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3
                                                                    Case 1
                                                                        Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) + Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3
                                                                    Case 2
                                                                        Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) - Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3
                                                                    Case 3
                                                                        Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = Random.NextDouble(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data4)
                                                                End Select
                                                            Case EventType.PlayerSwitch
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 0 Then
                                                                    Type.Player(i).Switches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = 1
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 1 Then
                                                                    Type.Player(i).Switches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = 0
                                                                End If
                                                            Case EventType.SelfSwitch
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Globals = 1 Then
                                                                    If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 0 Then
                                                                        Type.Map(GetPlayerMap(i)).Event(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 + 1) = 1
                                                                    ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 1 Then
                                                                        Type.Map(GetPlayerMap(i)).Event(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 + 1) = 0
                                                                    End If
                                                                Else
                                                                    If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 0 Then
                                                                        TempPlayer(i).EventMap.EventPages(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 + 1) = 1
                                                                    ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 1 Then
                                                                        TempPlayer(i).EventMap.EventPages(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 + 1) = 0
                                                                    End If
                                                                End If
                                                            Case EventType.Condition
                                                                Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Condition
                                                                    Case 0
                                                                        Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data2
                                                                            Case 0
                                                                                If Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data3 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 1
                                                                                If Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) >= Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data3 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 2
                                                                                If Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) <= Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data3 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 3
                                                                                If Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) > Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data3 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 4
                                                                                If Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) < Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data3 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 5
                                                                                If Type.Player(i).Variables(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) <> Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data3 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                        End Select
                                                                    Case 1
                                                                        Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data2
                                                                            Case 0
                                                                                If Type.Player(i).Switches(Type.Map(Global.Core.Commands.GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) = 1 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 1
                                                                                If Type.Player(i).Switches(Type.Map(Global.Core.Commands.GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) = 0 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                        End Select
                                                                    Case 2
                                                                        If HasItem(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) >= Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data2 Then
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                            .CurSlot = 1
                                                                        Else
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                            .CurSlot = 1
                                                                        End If
                                                                    Case 3
                                                                        If Type.Player(i).Job = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                            .CurSlot = 1
                                                                        Else
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                            .CurSlot = 1
                                                                        End If
                                                                    Case 4
                                                                        If HasSkill(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1) = 1 Then
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                            .CurSlot = 1
                                                                        Else
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                            .CurSlot = 1
                                                                        End If
                                                                    Case 5
                                                                        Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data2
                                                                            Case 0
                                                                                If GetPlayerLevel(i) = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 1
                                                                                If GetPlayerLevel(i) >= Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 2
                                                                                If GetPlayerLevel(i) <= Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 3
                                                                                If GetPlayerLevel(i) > Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 4
                                                                                If GetPlayerLevel(i) < Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                            Case 5
                                                                                If GetPlayerLevel(i) <> Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                    .CurSlot = 1
                                                                                Else
                                                                                    .ListLeftOff(.CurList) = .CurSlot
                                                                                    .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                    .CurSlot = 1
                                                                                End If
                                                                        End Select
                                                                    Case 6
                                                                        If Type.Map(GetPlayerMap(i)).Event(.EventId).Globals = 1 Then
                                                                            Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data2
                                                                                Case 0 'Self Switch is true
                                                                                    If Type.Map(GetPlayerMap(i)).Event(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 + 1) = 1 Then
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                        .CurSlot = 1
                                                                                    Else
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                        .CurSlot = 1
                                                                                    End If
                                                                                Case 1  'self switch is false
                                                                                    If Type.Map(GetPlayerMap(i)).Event(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 + 1) = 0 Then
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                        .CurSlot = 1
                                                                                    Else
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                        .CurSlot = 1
                                                                                    End If
                                                                            End Select
                                                                        Else
                                                                            Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data2
                                                                                Case 0 'Self Switch is true
                                                                                    If TempPlayer(i).EventMap.EventPages(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 + 1) = 1 Then
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                        .CurSlot = 1
                                                                                    Else
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                        .CurSlot = 1
                                                                                    End If
                                                                                Case 1  'self switch is false
                                                                                    If TempPlayer(i).EventMap.EventPages(.EventId).SelfSwitches(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 + 1) = 0 Then
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                                        .CurSlot = 1
                                                                                    Else
                                                                                        .ListLeftOff(.CurList) = .CurSlot
                                                                                        .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                                        .CurSlot = 1
                                                                                    End If
                                                                            End Select
                                                                        End If
                                                                    Case 7

                                                                    Case 8
                                                                        If Type.Player(i).Sex = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                            .CurSlot = 1
                                                                        Else
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                            .CurSlot = 1
                                                                        End If
                                                                    Case 9
                                                                        If Core.Time.Instance.TimeOfDay = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.Data1 Then
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.CommandList
                                                                            .CurSlot = 1
                                                                        Else
                                                                            .ListLeftOff(.CurList) = .CurSlot
                                                                            .CurList = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).ConditionalBranch.ElseCommandList
                                                                            .CurSlot = 1
                                                                        End If
                                                                End Select
                                                                endprocess = 1
                                                            Case EventType.ExitProcess
                                                                removeEventProcess = 1
                                                                endprocess = 1
                                                            Case EventType.ChangeItems
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 0 Then
                                                                    If HasItem(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) > 0 Then
                                                                        SetPlayerInvValue(i, FindItemSlot(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3)
                                                                    End If
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 1 Then
                                                                    GiveInv(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3, True)
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 2 Then
                                                                    Dim itemAmount As Integer
                                                                    itemAmount = HasItem(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                    ' Check Amount
                                                                    If itemAmount >= Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3 Then
                                                                        TakeInv(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3)
                                                                    End If
                                                                End If
                                                                SendInventory(i)
                                                            Case EventType.RestoreHP
                                                                SetPlayerVital(i, VitalType.HP, GetPlayerMaxVital(i, VitalType.HP))
                                                                SendVital(i, VitalType.HP)
                                                            Case EventType.RestoreSP
                                                                SetPlayerVital(i, VitalType.SP, GetPlayerMaxVital(i, VitalType.SP))
                                                                SendVital(i, VitalType.SP)
                                                            Case EventType.LevelUp
                                                                SetPlayerExp(i, GetPlayerNextLevel(i))
                                                                CheckPlayerLevelUp(i)
                                                                SendExp(i)
                                                                SendPlayerData(i)
                                                            Case EventType.ChangeLevel
                                                                SetPlayerLevel(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                SetPlayerExp(i, 0)
                                                                SendExp(i)
                                                                SendPlayerData(i)
                                                            Case EventType.ChangeSkills
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 0 Then
                                                                    If FindOpenSkill(i) > 0 Then
                                                                        If HasSkill(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = 0 Then
                                                                            SetPlayerSkill(i, FindOpenSkill(i), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                        Else
                                                                            'Error, already knows skill
                                                                        End If
                                                                    Else
                                                                        'Error, no room for skills
                                                                    End If
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 1 Then
                                                                    If HasSkill(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) = 1 Then
                                                                        For p = 1 To MAX_PLAYER_SKILLS
                                                                            If Type.Player(i).Skill(p).Num = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 Then
                                                                                SetPlayerSkill(i, p, 0)
                                                                            End If
                                                                        Next
                                                                    End If
                                                                End If
                                                                SendPlayerSkills(i)
                                                            Case EventType.ChangeJob
                                                                Type.Player(i).Job = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1
                                                                SendPlayerData(i)
                                                            Case EventType.ChangeSprite
                                                                SetPlayerSprite(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                SendPlayerData(i)
                                                            Case EventType.ChangeSex
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 = 0 Then
                                                                    Type.Player(i).Sex = SexType.Male
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 = 1 Then
                                                                    Type.Player(i).Sex = SexType.Female
                                                                End If
                                                                SendPlayerData(i)
                                                            Case EventType.ChangePk
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 = 0 Then
                                                                    Type.Player(i).Pk = 0
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 = 1 Then
                                                                    Type.Player(i).Pk = 1
                                                                End If
                                                                SendPlayerData(i)
                                                            Case EventType.WarpPlayer
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data4 = 0 Then
                                                                    PlayerWarp(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3)
                                                                Else
                                                                    Type.Player(i).Dir = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data4 - 1
                                                                    PlayerWarp(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3)
                                                                End If
                                                            Case EventType.SetMoveRoute
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 <= Type.Map(GetPlayerMap(i)).EventCount Then
                                                                    If Type.Map(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).Globals = 1 Then
                                                                        TempEventMap(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveType = 2
                                                                        TempEventMap(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).IgnoreIfCannotMove = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2
                                                                        TempEventMap(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).RepeatMoveRoute = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3
                                                                        TempEventMap(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRouteCount = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).MoveRouteCount
                                                                        TempEventMap(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRoute = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).MoveRoute
                                                                        TempEventMap(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRouteStep = 0
                                                                        TempEventMap(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRouteComplete = 0
                                                                    Else
                                                                        TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveType = 2
                                                                        TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).IgnoreIfCannotMove = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2
                                                                        TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).RepeatMoveRoute = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3
                                                                        TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRouteCount = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).MoveRouteCount
                                                                        TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRoute = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).MoveRoute
                                                                        TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRouteStep = 0
                                                                        TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).MoveRouteComplete = 0
                                                                    End If
                                                                End If
                                                            Case EventType.PlayAnimation
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 0 Then
                                                                    SendAnimation(GetPlayerMap(i), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, GetPlayerX(i), GetPlayerY(i), TargetType.Player, i)
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 1 Then
                                                                    If Type.Map(GetPlayerMap(i)).Event(.EventId).Globals = 1 Then
                                                                        SendAnimation(GetPlayerMap(i), Type.Map(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).X, Type.Map(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3).Y)
                                                                    Else
                                                                        SendAnimation(GetPlayerMap(i), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3).X, TempPlayer(i).EventMap.EventPages(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3).Y, 0, 0)
                                                                    End If
                                                                ElseIf Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2 = 2 Then
                                                                    SendAnimation(GetPlayerMap(i), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data4, 0, 0)
                                                                End If
                                                            Case EventType.CustomScript
                                                                'Runs Through Cases for a script
                                                                CustomScript(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, GetPlayerMap(i), .EventId)
                                                            Case EventType.PlayBgm
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SPlayBGM)
                                                                buffer.WriteString((Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1))
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)
                                                                buffer.Dispose()
                                                            Case EventType.FadeoutBgm
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SFadeoutBGM)
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)
                                                                buffer.Dispose()
                                                            Case EventType.PlaySound
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SPlaySound)
                                                                buffer.WriteString((Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1))
                                                                buffer.WriteInt32(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).X)
                                                                buffer.WriteInt32(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).Y)
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)
                                                                buffer.Dispose()
                                                            Case EventType.StopSound
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SStopSound)
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)
                                                                buffer.Dispose()
                                                            Case EventType.SetAccess
                                                                SetPlayerAccess(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                SendPlayerData(i)
                                                            Case EventType.OpenShop
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 > 0 Then ' shop exists?
                                                                    If Len(Type.Shop(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).Name) > 0 Then ' name exists?
                                                                        SendOpenShop(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                        TempPlayer(i).InShop = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 ' stops movement and the like
                                                                        .WaitingForResponse = 2
                                                                    End If
                                                                End If
                                                            Case EventType.OpenBank
                                                                SendBank(i)
                                                                TempPlayer(i).InBank = 1
                                                                .WaitingForResponse = 3
                                                            Case EventType.GiveExp
                                                                GivePlayerExp(i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                            Case EventType.ShowChatBubble
                                                                Select Case Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1
                                                                    Case TargetType.Player
                                                                        SendChatBubble(GetPlayerMap(i), i, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1, ColorType.Blue)
                                                                    Case TargetType.NPC
                                                                        SendChatBubble(GetPlayerMap(i), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1, ColorType.Blue)
                                                                    Case TargetType.Event
                                                                        SendChatBubble(GetPlayerMap(i), Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1, ColorType.Blue)
                                                                End Select
                                                            Case EventType.Label
                                                                'Do nothing, just a label
                                                            Case EventType.GotoLabel
                                                                'Find the label's list of commands and slot
                                                                FindEventLabel(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Text1, GetPlayerMap(i), .EventId, .PageId, .CurSlot, .CurList, .ListLeftOff)
                                                            Case EventType.SpawnNpc
                                                                If Type.Map(GetPlayerMap(i)).NPC(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1) > 0 Then
                                                                    SpawnNpc(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, GetPlayerMap(i))
                                                                End If
                                                            Case EventType.FadeIn
                                                                SendSpecialEffect(i, EffectTypeFadein)
                                                            Case EventType.FadeOut
                                                                SendSpecialEffect(i, EffectTypeFadeout)
                                                            Case EventType.FlashWhite
                                                                SendSpecialEffect(i, EffectTypeFlash)
                                                            Case EventType.SetFog
                                                                SendSpecialEffect(i, EffectTypeFog, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3)
                                                            Case EventType.SetWeather
                                                                SendSpecialEffect(i, EffectTypeWeather, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2)
                                                            Case EventType.SetTint
                                                                SendSpecialEffect(i, EffectTypeTint, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3, Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data4)
                                                            Case EventType.Wait
                                                                .ActionTimer = GetTimeMs() + Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1
                                                            Case EventType.ShowPicture
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SPic)
                                                                buffer.WriteInt32(.EventId)
                                                                buffer.WriteByte(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1)
                                                                buffer.WriteByte(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data2)
                                                                buffer.WriteByte(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data3)
                                                                buffer.WriteByte(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data4)
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)

                                                                buffer.Dispose()
                                                            Case EventType.HidePicture
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SPic)
                                                                buffer.WriteByte(0)
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)

                                                                buffer.Dispose()
                                                            Case EventType.WaitMovement
                                                                If Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1 <= Type.Map(GetPlayerMap(i)).EventCount Then
                                                                    If Type.Map(GetPlayerMap(i)).Event(Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1).Globals = 1 Then
                                                                        .WaitingForResponse = 4
                                                                        .EventMovingId = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1
                                                                        .EventMovingType = 1
                                                                    Else
                                                                        .WaitingForResponse = 4
                                                                        .EventMovingId = Type.Map(GetPlayerMap(i)).Event(.EventId).Pages(.PageId).CommandList(.CurList).Commands(.CurSlot).Data1
                                                                        .EventMovingType = 0
                                                                    End If
                                                                End If
                                                            Case EventType.HoldPlayer
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SHoldPlayer)
                                                                buffer.WriteInt32(0)
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)

                                                                buffer.Dispose()
                                                            Case EventType.ReleasePlayer
                                                                buffer = New ByteStream(4)
                                                                buffer.WriteInt32(ServerPackets.SHoldPlayer)
                                                                buffer.WriteInt32(1)
                                                                Socket.SendDataTo(i, buffer.Data, buffer.Head)

                                                                buffer.Dispose()
                                                        End Select
                                                    End If
                                                Loop
                                                If endprocess = 0 Then
                                                    .CurSlot = .CurSlot + 1
                                                End If
                                            End If
                                        End If
                                    End With
                                End If
                                If removeEventProcess = 1 Then
                                    TempPlayer(i).EventProcessing(x).Active = 0
                                    restartloop = 1
                                    removeEventProcess = 0
                                End If
                            Next
                        Loop
                    End If
                End If
            End If
        Next

    End Sub

    Friend Sub UpdateEventLogic()
        RemoveDeadEvents()
        SpawnNewEvents()
        ProcessEventMovement()
        ProcessLocalEventMovement()
        ProcessEventCommands()
    End Sub

    Function ParseEventText(index As Integer, txt As String) As String
        Dim i As Integer, x As Integer, newtxt As String, parsestring As String, z As Integer

        txt = Replace(txt, "/name", Type.Player(index).Name)
        txt = Replace(txt, "/p", Type.Player(index).Name)
        txt = Replace(txt, "$playername$", Type.Player(index).Name)
        txt = Replace(txt, "$playerclass$", Type.Job(Type.Player(index).Job).Name)
        Do While InStr(1, txt, "/v") > 0
            x = InStr(1, txt, "/v")
            If x > 0 Then
                i = 0
                Do Until IsNumeric(Mid(txt, x + 2 + i, 1)) = 0
                    i = i + 1
                Loop
                newtxt = Mid(txt, 1, x - 1)
                parsestring = Mid(txt, x + 2, i)
                z = Type.Player(index).Variables(Val(parsestring))
                newtxt = newtxt & CStr(z)
                newtxt = newtxt & Mid(txt, x + 2 + i, Len(txt) - (x + i))
                txt = newtxt
            End If
        Loop
        ParseEventText = txt

    End Function

    Sub FindEventLabel(Label As String, mapNum As Integer, eventID As Integer, pageID As Integer, ByRef CurSlot As Integer, ByRef CurList As Integer, ByRef ListLeftOff() As Integer)
        Dim tmpCurSlot As Integer, tmpCurList As Integer, CurrentListOption() As Integer
        Dim removeEventProcess As Boolean, tmpListLeftOff() As Integer, restartlist As Boolean, w As Integer

        'Store the Old data, just in case

        tmpCurSlot = CurSlot
        tmpCurList = CurList
        tmpListLeftOff = ListLeftOff

        ReDim ListLeftOff(Type.Map(MapNum).Event(eventID).Pages(pageID).CommandListCount)
        ReDim CurrentListOption(Type.Map(MapNum).Event(eventID).Pages(pageID).CommandListCount)
        CurList = 1
        CurSlot = 1

        Do Until removeEventProcess = 1
            If ListLeftOff(CurList) > 0 Then
                CurSlot = ListLeftOff(CurList)
                ListLeftOff(CurList) = 0
            End If
            If CurList > Type.Map(MapNum).Event(eventID).Pages(pageID).CommandListCount Then
                'Get rid of this event, it is bad
                removeEventProcess = 1
            End If

            If CurSlot > Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).CommandCount Then
                If CurList = 1 Then
                    removeEventProcess = 1
                Else
                    CurList = Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).ParentList
                    CurSlot = 1
                    restartlist = 1
                End If
            End If

            If restartlist = 0 Then
                If removeEventProcess = 0 Then
                    'If we are still here, then we are good to process shit :D
                    Select Case Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Index
                        Case EventType.ShowChoices
                            If Len(Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Text2) > 0 Then
                                w = 1
                                If Len(Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Text3) > 0 Then
                                    w = 2
                                    If Len(Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Text4) > 0 Then
                                        w = 3
                                        If Len(Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Text5) > 0 Then
                                            w = 4
                                        End If
                                    End If
                                End If
                            End If
                            If w > 0 Then
                                If CurrentListOption(CurList) < w Then
                                    CurrentListOption(CurList) = CurrentListOption(CurList) + 1
                                    'Process
                                    ListLeftOff(CurList) = CurSlot
                                    Select Case CurrentListOption(CurList)
                                        Case 1
                                            CurList = Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Data1
                                        Case 2
                                            CurList = Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Data2
                                        Case 3
                                            CurList = Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Data3
                                        Case 4
                                            CurList = Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Data4
                                    End Select
                                    CurSlot = 0
                                Else
                                    CurrentListOption(CurList) = 0
                                    'continue on
                                End If
                            End If
                            w = 0
                        Case EventType.Condition
                            If CurrentListOption(CurList) = 0 Then
                                CurrentListOption(CurList) = 1
                                ListLeftOff(CurList) = CurSlot
                                CurList = Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).ConditionalBranch.CommandList
                                CurSlot = 0
                            ElseIf CurrentListOption(CurList) = 1 Then
                                CurrentListOption(CurList) = 2
                                ListLeftOff(CurList) = CurSlot
                                CurList = Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).ConditionalBranch.ElseCommandList
                                CurSlot = 0
                            ElseIf CurrentListOption(CurList) = 2 Then
                                CurrentListOption(CurList) = 0
                            End If
                        Case EventType.Label
                            'Do nothing, just a label
                            If Type.Map(MapNum).Event(eventID).Pages(pageID).CommandList(CurList).Commands(CurSlot).Text1 = Label Then
                                Exit Sub
                            End If
                    End Select
                    CurSlot = CurSlot + 1
                End If
            End If
            restartlist = 0
        Loop

        ListLeftOff = tmpListLeftOff
        CurList = tmpCurList
        CurSlot = tmpCurSlot

    End Sub

    Function FindNpcPath(MapNum As Integer, mapnpcnum As Integer, targetx As Integer, targety As Integer) As Integer
        Dim tim As Integer, sX As Integer, sY As Integer, pos(,) As Integer, reachable As Boolean, j As Integer, LastSum As Integer, Sum As Integer, FX As Integer, FY As Integer, i As Integer
        Dim path() As Point, LastX As Integer, LastY As Integer, did As Boolean

        'Initialization phase

        tim = 0

        sX = MapNPC(MapNum).NPC(MapNPCNum).X
        sY = MapNPC(MapNum).NPC(MapNPCNum).Y

        FX = targetx
        FY = targety

        If FX = -1 Then FX = 0
        If FY = -1 Then FY = 0

        ReDim pos(Type.Map(MapNum).MaxX, Type.Map(MapNum).MaxY)
        'pos = MapBlocks(MapNum).Blocks

        pos(sX, sY) = 100 + tim
        pos(FX, FY) = 2

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
                Sum = 0
                For j = 0 To Type.Map(MapNum).MaxY
                    For i = 0 To Type.Map(MapNum).MaxX
                        'we add up ALL the squares
                        Sum = Sum + pos(i, j)
                    Next i
                Next j

                'Now if the sum is euqal to the last sum, its not reachable, if it isnt, then we store
                'sum to lastsum
                If Sum = LastSum Then
                    FindNpcPath = 4
                    Exit Function
                Else
                    LastSum = Sum
                End If
            End If

            'we increase the pointer to point to the next squares to be expanded
            tim = tim + 1
        Loop

        'We work backwards to find the way...
        LastX = FX
        LastY = FY

        ReDim path(tim + 1)

        'The following code may be a little bit confusing but ill try my best to explain it.
        'We are working backwards to find ONE of the shortest ways back to Start.
        'So we repeat the loop until the LastX and LastY arent in start. Look in the code to see
        'how LastX and LasY change
        Do While LastX <> sX Or LastY <> sY
            'We decrease tim by one, and then we are finding any adjacent square to the final one, that
            'has that value. So lets say the tim would be 5, because it takes 5 steps to get to the target.
            'Now everytime we decrease that, so we make it 4, and we look for any adjacent square that has
            'that value. When we find it, we just color it yellow as for the solution
            tim = tim - 1
            'reset did to false
            did = 0

            'If we arent on edge
            If LastX < Type.Map(MapNum).MaxX Then
                'check the square on the right of the solution. Is it a tim-1 one? or just a blank one
                If pos(LastX + 1, LastY) = 100 + tim Then
                    'if it, then make it yellow, and change did to true
                    LastX = LastX + 1
                    did = 1
                End If
            End If

            'This will then only work if the previous part didnt execute, and did is still false. THen
            'we want to check another square, the on left. Is it a tim-1 one ?
            If did = 0 Then
                If LastX > 0 Then
                    If pos(LastX - 1, LastY) = 100 + tim Then
                        LastX = LastX - 1
                        did = 1
                    End If
                End If
            End If

            'We check the one below it
            If did = 0 Then
                If LastY < Type.Map(MapNum).MaxY Then
                    If pos(LastX, LastY + 1) = 100 + tim Then
                        LastY = LastY + 1
                        did = 1
                    End If
                End If
            End If

            'And above it. One of these have to be it, since we have found the solution, we know that already
            'there is a way back.
            If did = 0 Then
                If LastY > 0 Then
                    If pos(LastX, LastY - 1) = 100 + tim Then
                        LastY = LastY - 1
                    End If
                End If
            End If

            path(tim).X = LastX
            path(tim).Y = LastY
        Loop

        'Ok we got a Core.Path. Now, lets look at the first step and see what direction we should take.
        If path(1).X > LastX Then
            FindNpcPath = DirectionType.Right
        ElseIf path(1).Y > LastY Then
            FindNpcPath = DirectionType.Down
        ElseIf path(1).Y < LastY Then
            FindNpcPath = DirectionType.Up
        ElseIf path(1).X < LastX Then
            FindNpcPath = DirectionType.Left
        End If

    End Function

    Sub SpawnAllMapGlobalEvents()
        Dim i As Integer

        For i = 1 To MAX_MAPS
            SpawnGlobalEvents(i)
        Next

    End Sub

    Sub SpawnGlobalEvents(MapNum As Integer)
        Dim i As Integer, z As Integer

        If Type.Map(MapNum).EventCount > 0 Then
            TempEventMap(MapNum).EventCount = 0
            ReDim TempEventMap(MapNum).Event(0)
            For i = 0 To Type.Map(MapNum).EventCount
                TempEventMap(MapNum).EventCount = TempEventMap(MapNum).EventCount + 1
                ReDim Preserve TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount)
                If Type.Map(MapNum).Event(i).PageCount > 0 Then
                    If Type.Map(MapNum).Event(i).Globals = 1 Then
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).X = Type.Map(MapNum).Event(i).X
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).Y = Type.Map(MapNum).Event(i).Y
                        If Type.Map(MapNum).Event(i).Pages(1).GraphicType = 1 Then
                            Select Case Type.Map(MapNum).Event(i).Pages(1).GraphicY
                                Case 0
                                    TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).Dir = DirectionType.Down
                                Case 1
                                    TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).Dir = DirectionType.Left
                                Case 2
                                    TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).Dir = DirectionType.Right
                                Case 3
                                    TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).Dir = DirectionType.Up
                            End Select
                        Else
                            TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).Dir = DirectionType.Down
                        End If
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).Active = 1

                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveType = Type.Map(MapNum).Event(i).Pages(1).MoveType

                        If TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveType = 2 Then
                            TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveRouteCount = Type.Map(MapNum).Event(i).Pages(1).MoveRouteCount
                            ReDim TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveRoute(Type.Map(MapNum).Event(i).Pages(1).MoveRouteCount)
                            For z = 0 To Type.Map(MapNum).Event(i).Pages(1).MoveRouteCount
                                TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveRoute(z) = Type.Map(MapNum).Event(i).Pages(1).MoveRoute(z)
                            Next
                            TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveRouteComplete = 0
                        Else
                            TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveRouteComplete = 1
                        End If

                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).RepeatMoveRoute = Type.Map(MapNum).Event(i).Pages(1).RepeatMoveRoute
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).IgnoreIfCannotMove = Type.Map(MapNum).Event(i).Pages(1).IgnoreMoveRoute

                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveFreq = Type.Map(MapNum).Event(i).Pages(1).MoveFreq
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).MoveSpeed = Type.Map(MapNum).Event(i).Pages(1).MoveSpeed

                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).WalkThrough = Type.Map(MapNum).Event(i).Pages(1).WalkThrough
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).FixedDir = Type.Map(MapNum).Event(i).Pages(1).DirFix
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).WalkingAnim = Type.Map(MapNum).Event(i).Pages(1).WalkAnim
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).ShowName = Type.Map(MapNum).Event(i).Pages(1).ShowName
                        TempEventMap(MapNum).Event(TempEventMap(MapNum).EventCount).QuestNum = Type.Map(MapNum).Event(i).Pages(1).QuestNum
                    End If
                End If
            Next
        End If

    End Sub

    Friend Sub SpawnMapEventsFor(index As Integer, mapNum As Integer)
        Dim i As Integer, z As Integer, spawncurrentevent As Boolean, p As Integer, compare As Integer

        TempPlayer(index).EventMap.CurrentEvents = 0
        ReDim TempPlayer(index).EventMap.EventPages(0)

        If Type.Map(MapNum).EventCount > 0 Then
            ReDim TempPlayer(index).EventProcessing(Type.Map(MapNum).EventCount)
            TempPlayer(index).EventProcessingCount = Type.Map(MapNum).EventCount
        Else
            ReDim TempPlayer(index).EventProcessing(0)
            TempPlayer(index).EventProcessingCount = 0
        End If

        If Type.Map(MapNum).EventCount <= 0 Then Exit Sub
        For i = 0 To Type.Map(MapNum).EventCount
            If Type.Map(MapNum).Event(i).PageCount > 0 Then
                For z = Type.Map(MapNum).Event(i).PageCount To 0 Step -1
                    With Type.Map(MapNum).Event(i).Pages(z)
                        spawncurrentevent = 1

                        If .ChkVariable = 1 Then
                            Select Case .VariableCompare
                                Case 0
                                    If Type.Player(index).Variables(.VariableIndex) <> .VariableCondition Then
                                        spawncurrentevent = 0
                                    End If
                                Case 1
                                    If Type.Player(index).Variables(.VariableIndex) < .VariableCondition Then
                                        spawncurrentevent = 0
                                    End If
                                Case 2
                                    If Type.Player(index).Variables(.VariableIndex) > .VariableCondition Then
                                        spawncurrentevent = 0
                                    End If
                                Case 3
                                    If Type.Player(index).Variables(.VariableIndex) <= .VariableCondition Then
                                        spawncurrentevent = 0
                                    End If
                                Case 4
                                    If Type.Player(index).Variables(.VariableIndex) >= .VariableCondition Then
                                        spawncurrentevent = 0
                                    End If
                                Case 5
                                    If Type.Player(index).Variables(.VariableIndex) = .VariableCondition Then
                                        spawncurrentevent = 0
                                    End If
                            End Select
                        End If

                        'we are assuming the event will spawn, and are looking for ways to stop it
                        If .ChkSwitch = 1 Then
                            If .SwitchCompare = 1 Then 'we want true
                                If Type.Player(index).Switches(.SwitchIndex) = 0 Then 'it is false, so we stop the spawn
                                    spawncurrentevent = 0
                                End If
                            Else
                                If Type.Player(index).Switches(.SwitchIndex) = 1 Then 'we want false and it is true so we stop the spawn
                                    spawncurrentevent = 0
                                End If
                            End If
                        End If

                        If .ChkHasItem = 1 Then
                            If HasItem(index, .HasItemIndex) = 0 Then
                                spawncurrentevent = 0
                            End If
                        End If

                        If .ChkSelfSwitch = 1 Then
                            If .SelfSwitchCompare = 0 Then
                                compare = 1
                            Else
                                compare = 0
                            End If
                            If Type.Map(MapNum).Event(i).Globals = 1 Then
                                If Type.Map(MapNum).Event(i).SelfSwitches(.SelfSwitchIndex) <> compare Then
                                    spawncurrentevent = 0
                                End If
                            Else
                                If compare = 1 Then
                                    spawncurrentevent = 0
                                End If
                            End If
                        End If

                        If spawncurrentevent = 1 Or (spawncurrentevent = 0 And z = 1) Then
                            'spawn the event... send data to player
                            TempPlayer(index).EventMap.CurrentEvents = TempPlayer(index).EventMap.CurrentEvents + 1
                            ReDim Preserve TempPlayer(index).EventMap.EventPages(TempPlayer(index).EventMap.CurrentEvents)
                            With TempPlayer(index).EventMap.EventPages(TempPlayer(index).EventMap.CurrentEvents)
                                If Type.Map(MapNum).Event(i).Pages(z).GraphicType = 1 Then
                                    Select Case Type.Map(MapNum).Event(i).Pages(z).GraphicY
                                        Case 0
                                            .Dir = DirectionType.Down
                                        Case 1
                                            .Dir = DirectionType.Left
                                        Case 2
                                            .Dir = DirectionType.Right
                                        Case 3
                                            .Dir = DirectionType.Up
                                    End Select
                                Else
                                    .Dir = 0
                                End If
                                .Graphic = Type.Map(MapNum).Event(i).Pages(z).Graphic
                                .GraphicType = Type.Map(MapNum).Event(i).Pages(z).GraphicType
                                .GraphicX = Type.Map(MapNum).Event(i).Pages(z).GraphicX
                                .GraphicY = Type.Map(MapNum).Event(i).Pages(z).GraphicY
                                .GraphicX2 = Type.Map(MapNum).Event(i).Pages(z).GraphicX2
                                .GraphicY2 = Type.Map(MapNum).Event(i).Pages(z).GraphicY2
                                Select Case Type.Map(MapNum).Event(i).Pages(z).MoveSpeed
                                    Case 0
                                        .MovementSpeed = 2
                                    Case 1
                                        .MovementSpeed = 3
                                    Case 2
                                        .MovementSpeed = 4
                                    Case 3
                                        .MovementSpeed = 6
                                    Case 4
                                        .MovementSpeed = 12
                                    Case 5
                                        .MovementSpeed = 24
                                End Select
                                If Type.Map(MapNum).Event(i).Globals Then
                                    .X = TempEventMap(MapNum).Event(i).X
                                    .Y = TempEventMap(MapNum).Event(i).Y
                                    .Dir = TempEventMap(MapNum).Event(i).Dir
                                    .MoveRouteStep = TempEventMap(MapNum).Event(i).MoveRouteStep
                                Else
                                    .X = Type.Map(MapNum).Event(i).X
                                    .Y = Type.Map(MapNum).Event(i).Y
                                    .MoveRouteStep = 0
                                End If
                                .Position = Type.Map(MapNum).Event(i).Pages(z).Position
                                .EventId = i
                                .PageId = z
                                If spawncurrentevent = 1 Then
                                    .Visible = True
                                Else
                                    .Visible = False
                                End If

                                .MoveType = Type.Map(MapNum).Event(i).Pages(z).MoveType
                                If .MoveType = 2 Then
                                    .MoveRouteCount = Type.Map(MapNum).Event(i).Pages(z).MoveRouteCount
                                    ReDim .MoveRoute(Type.Map(MapNum).Event(i).Pages(z).MoveRouteCount)
                                    If Type.Map(MapNum).Event(i).Pages(z).MoveRouteCount > 0 Then
                                        For p = 0 To Type.Map(MapNum).Event(i).Pages(z).MoveRouteCount
                                            .MoveRoute(p) = Type.Map(MapNum).Event(i).Pages(z).MoveRoute(p)
                                        Next
                                    End If
                                    .MoveRouteComplete = 0
                                Else
                                    .MoveRouteComplete = 1
                                End If

                                .RepeatMoveRoute = Type.Map(MapNum).Event(i).Pages(z).RepeatMoveRoute
                                .IgnoreIfCannotMove = Type.Map(MapNum).Event(i).Pages(z).IgnoreMoveRoute

                                .MoveFreq = Type.Map(MapNum).Event(i).Pages(z).MoveFreq
                                .MoveSpeed = Type.Map(MapNum).Event(i).Pages(z).MoveSpeed

                                .WalkingAnim = Type.Map(MapNum).Event(i).Pages(z).WalkAnim
                                .WalkThrough = Type.Map(MapNum).Event(i).Pages(z).WalkThrough
                                .ShowName = Type.Map(MapNum).Event(i).Pages(z).ShowName
                                .FixedDir = Type.Map(MapNum).Event(i).Pages(z).DirFix
                                .QuestNum = Type.Map(MapNum).Event(i).Pages(z).QuestNum
                            End With
                            Exit For
                        End If
                    End With
                Next
            End If
        Next

        Dim buffer As ByteStream
        If TempPlayer(index).EventMap.CurrentEvents > 0 Then
            For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                If TempPlayer(index).EventMap.EventPages(i).EventId > 0 Then
                    buffer = New ByteStream(4)
                    buffer.WriteInt32(ServerPackets.SSpawnEvent)
                    buffer.WriteInt32(TempPlayer(index).EventMap.EventPages(i).EventId)
                    With TempPlayer(index).EventMap.EventPages(i)
                        buffer.WriteString(Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Name)
                        buffer.WriteInt32(.Dir)
                        buffer.WriteByte(.GraphicType)
                        buffer.WriteInt32(.Graphic)
                        buffer.WriteInt32(.GraphicX)
                        buffer.WriteInt32(.GraphicX2)
                        buffer.WriteInt32(.GraphicY)
                        buffer.WriteInt32(.GraphicY2)
                        buffer.WriteInt32(.MovementSpeed)
                        buffer.WriteInt32(.X)
                        buffer.WriteInt32(.Y)
                        buffer.WriteByte(.Position)
                        buffer.WriteInt32(.Visible)
                        buffer.WriteInt32(Type.Map(MapNum).Event(.EventId).Pages(.PageId).WalkAnim)
                        buffer.WriteInt32(Type.Map(MapNum).Event(.EventId).Pages(.PageId).DirFix)
                        buffer.WriteInt32(Type.Map(MapNum).Event(.EventId).Pages(.PageId).WalkThrough)
                        buffer.WriteInt32(Type.Map(MapNum).Event(.EventId).Pages(.PageId).ShowName)
                        buffer.WriteInt32(Type.Map(MapNum).Event(.EventId).Pages(.PageId).QuestNum)
                    End With
                    Socket.SendDataTo(index, buffer.Data, buffer.Head)

                    buffer.Dispose()
                End If
            Next
        End If

    End Sub

    Function TriggerEvent(Index As Integer, i As Integer, triggerType As Byte, x As Integer, y As Integer)
        If TempPlayer(index).EventMap.CurrentEvents > 0 Then
            For z = 1 To TempPlayer(Index).EventMap.CurrentEvents
                If TempPlayer(Index).EventMap.EventPages(z).EventId = i Then
                    i = z
                    Exit For
                End If
            Next
        End If

        If TempPlayer(index).EventMap.EventPages(i).EventId = 0 Then
            Exit Function
        End If

        If Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Pages(TempPlayer(index).EventMap.EventPages(i).PageId).Trigger <> triggerType Then
            Exit Function
        End If

        Select Case GetPlayerDir(index)
            Case DirectionType.Up

                If GetPlayerY(index) = 0 Then Exit Function
                x = GetPlayerX(index)
                y = GetPlayerY(index) - 1
            Case DirectionType.Down

                If GetPlayerY(index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Function
                x = GetPlayerX(index)
                y = GetPlayerY(index) + 1
            Case DirectionType.Left

                If GetPlayerX(index) = 0 Then Exit Function
                x = GetPlayerX(index) - 1
                y = GetPlayerY(index)
            Case DirectionType.Right

                If GetPlayerX(Index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Function
                x = GetPlayerX(Index) + 1
                y = GetPlayerY(Index)

            Case DirectionType.UpRight

                If GetPlayerX(Index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Function
                If GetPlayerY(Index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Function
                x = GetPlayerX(Index) + 1
                y = GetPlayerY(Index) - 1

            Case DirectionType.UpLeft

                If GetPlayerX(Index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Function
                If GetPlayerY(Index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Function
                x = GetPlayerX(Index) - 1
                y = GetPlayerY(Index) - 1

            Case DirectionType.DownLeft

                If GetPlayerX(Index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Function
                If GetPlayerY(Index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Function
                x = GetPlayerX(Index) - 1
                y = GetPlayerY(Index) + 1

            Case DirectionType.DownRight

                If GetPlayerX(Index) = Type.Map(GetPlayerMap(index)).MaxX Then Exit Function
                If GetPlayerY(Index) = Type.Map(GetPlayerMap(index)).MaxY Then Exit Function
                x = GetPlayerX(Index) + 1
                y = GetPlayerY(Index) + 1
        End Select

        If x <> TempPlayer(index).EventMap.EventPages(i).X Or y <> TempPlayer(index).EventMap.EventPages(i).Y Then Exit Function

        If Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Pages(TempPlayer(index).EventMap.EventPages(i).PageId).CommandListCount > 0 Then
            If (TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).Active = 0) Then
                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).Active = 1
                With TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId)
                    .ActionTimer = GetTimeMs()
                    .CurList = 1
                    .CurSlot = 1
                    .EventId = TempPlayer(index).EventMap.EventPages(i).EventId
                    .PageId = TempPlayer(index).EventMap.EventPages(i).PageId
                    .WaitingForResponse = 0
                    ReDim .ListLeftOff(Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Pages(TempPlayer(index).EventMap.EventPages(i).PageId).CommandListCount)
                End With
                Exit Function
            End If
        End If

        TriggerEvent = 1
    End Function

End Module