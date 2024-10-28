Imports Core

Friend Module Autotile
    Sub ClearAutotiles()
        Dim x As Integer, y As Integer, i As Integer

        ReDim Type.Autotile(MyMap.MaxX, MyMap.MaxY)

        For X = 0 To MyMap.MaxX
            For Y = 0 To MyMap.MaxY
                ReDim Type.Autotile(x, y).Layer(LayerType.Count - 1)
                For i = 1 To LayerType.Count - 1
                    ReDim Type.Autotile(x, y).Layer(i).SrcX(4)
                    ReDim Type.Autotile(x, y).Layer(i).SrcY(4)
                    ReDim Type.Autotile(x, y).Layer(i).QuarterTile(4)
                Next
            Next
        Next
    End Sub

    '\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    '   All of this code is for auto tiles and the math behind generating them.
    '\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
    Friend Sub PlaceAutotile(layerNum As Integer, x As Integer, y As Integer, tileQuarter As Byte, autoTileLetter As String)

        If layerNum > LayerType.Count - 1 Then
            layerNum = layerNum - (LayerType.Count - 1)
            With Type.Autotile(x, y).ExLayer(layerNum).QuarterTile(tileQuarter)
                Select Case autoTileLetter
                    Case "a"
                        .X = AutoIn(1).X
                        .Y = AutoIn(1).Y
                    Case "b"
                        .X = AutoIn(2).X
                        .Y = AutoIn(2).Y
                    Case "c"
                        .X = AutoIn(3).X
                        .Y = AutoIn(3).Y
                    Case "d"
                        .X = AutoIn(4).X
                        .Y = AutoIn(4).Y
                    Case "e"
                        .X = AutoNw(1).X
                        .Y = AutoNw(1).Y
                    Case "f"
                        .X = AutoNw(2).X
                        .Y = AutoNw(2).Y
                    Case "g"
                        .X = AutoNw(3).X
                        .Y = AutoNw(3).Y
                    Case "h"
                        .X = AutoNw(4).X
                        .Y = AutoNw(4).Y
                    Case "i"
                        .X = AutoNe(1).X
                        .Y = AutoNe(1).Y
                    Case "j"
                        .X = AutoNe(2).X
                        .Y = AutoNe(2).Y
                    Case "k"
                        .X = AutoNe(3).X
                        .Y = AutoNe(3).Y
                    Case "l"
                        .X = AutoNe(4).X
                        .Y = AutoNe(4).Y
                    Case "m"
                        .X = AutoSw(1).X
                        .Y = AutoSw(1).Y
                    Case "n"
                        .X = AutoSw(2).X
                        .Y = AutoSw(2).Y
                    Case "o"
                        .X = AutoSw(3).X
                        .Y = AutoSw(3).Y
                    Case "p"
                        .X = AutoSw(4).X
                        .Y = AutoSw(4).Y
                    Case "q"
                        .X = AutoSe(1).X
                        .Y = AutoSe(1).Y
                    Case "r"
                        .X = AutoSe(2).X
                        .Y = AutoSe(2).Y
                    Case "s"
                        .X = AutoSe(3).X
                        .Y = AutoSe(3).Y
                    Case "t"
                        .X = AutoSe(4).X
                        .Y = AutoSe(4).Y
                End Select
            End With
        Else
            With Type.Autotile(x, y).Layer(layerNum).QuarterTile(tileQuarter)
                Select Case autoTileLetter
                    Case "a"
                        .X = AutoIn(1).X
                        .Y = AutoIn(1).Y
                    Case "b"
                        .X = AutoIn(2).X
                        .Y = AutoIn(2).Y
                    Case "c"
                        .X = AutoIn(3).X
                        .Y = AutoIn(3).Y
                    Case "d"
                        .X = AutoIn(4).X
                        .Y = AutoIn(4).Y
                    Case "e"
                        .X = AutoNw(1).X
                        .Y = AutoNw(1).Y
                    Case "f"
                        .X = AutoNw(2).X
                        .Y = AutoNw(2).Y
                    Case "g"
                        .X = AutoNw(3).X
                        .Y = AutoNw(3).Y
                    Case "h"
                        .X = AutoNw(4).X
                        .Y = AutoNw(4).Y
                    Case "i"
                        .X = AutoNe(1).X
                        .Y = AutoNe(1).Y
                    Case "j"
                        .X = AutoNe(2).X
                        .Y = AutoNe(2).Y
                    Case "k"
                        .X = AutoNe(3).X
                        .Y = AutoNe(3).Y
                    Case "l"
                        .X = AutoNe(4).X
                        .Y = AutoNe(4).Y
                    Case "m"
                        .X = AutoSw(1).X
                        .Y = AutoSw(1).Y
                    Case "n"
                        .X = AutoSw(2).X
                        .Y = AutoSw(2).Y
                    Case "o"
                        .X = AutoSw(3).X
                        .Y = AutoSw(3).Y
                    Case "p"
                        .X = AutoSw(4).X
                        .Y = AutoSw(4).Y
                    Case "q"
                        .X = AutoSe(1).X
                        .Y = AutoSe(1).Y
                    Case "r"
                        .X = AutoSe(2).X
                        .Y = AutoSe(2).Y
                    Case "s"
                        .X = AutoSe(3).X
                        .Y = AutoSe(3).Y
                    Case "t"
                        .X = AutoSe(4).X
                        .Y = AutoSe(4).Y
                End Select
            End With
        End If

    End Sub

    Friend Sub InitAutotiles()
        Dim x As Integer, y As Integer, layerNum As Integer
        ' Procedure used to cache autotile positions. All positioning is
        ' independant from the tileset. Calculations are convoluted and annoying.
        ' Maths is not my strong point. Luckily we're caching them so it's a one-off
        ' thing when the map is originally loaded. As such optimisation isn't an issue.
        ' For simplicity's sake we cache all subtile SOURCE positions in to an array.
        ' We also give letters to each subtile for easy rendering tweaks. ;]
        ' First, we need to re-size the array

        ReDim Type.Autotile(MyMap.MaxX, MyMap.MaxY)
        For x = 0 To MyMap.MaxX
            For y = 0 To MyMap.MaxY
                ReDim Type.Autotile(x, y).Layer(LayerType.Count - 1)
                For i = 1 To LayerType.Count - 1
                    ReDim Type.Autotile(x, y).Layer(i).SrcX(4)
                    ReDim Type.Autotile(x, y).Layer(i).SrcY(4)
                    ReDim Type.Autotile(x, y).Layer(i).QuarterTile(4)
                Next
            Next
        Next

        ' Inner tiles (Top right subtile region)
        ' NW - a
        AutoIn(1).X = 32
        AutoIn(1).Y = 0
        ' NE - b
        AutoIn(2).X = 48
        AutoIn(2).Y = 0
        ' SW - c
        AutoIn(3).X = 32
        AutoIn(3).Y = 16
        ' SE - d
        AutoIn(4).X = 48
        AutoIn(4).Y = 16
        ' Outer Tiles - NW (bottom subtile region)
        ' NW - e
        AutoNw(1).X = 0
        AutoNw(1).Y = 32
        ' NE - f
        AutoNw(2).X = 16
        AutoNw(2).Y = 32
        ' SW - g
        AutoNw(3).X = 0
        AutoNw(3).Y = 48
        ' SE - h
        AutoNw(4).X = 16
        AutoNw(4).Y = 48
        ' Outer Tiles - NE (bottom subtile region)
        ' NW - i
        AutoNe(1).X = 32
        AutoNe(1).Y = 32
        ' NE - g
        AutoNe(2).X = 48
        AutoNe(2).Y = 32
        ' SW - k
        AutoNe(3).X = 32
        AutoNe(3).Y = 48
        ' SE - l
        AutoNe(4).X = 48
        AutoNe(4).Y = 48
        ' Outer Tiles - SW (bottom subtile region)
        ' NW - m
        AutoSw(1).X = 0
        AutoSw(1).Y = 64
        ' NE - n
        AutoSw(2).X = 16
        AutoSw(2).Y = 64
        ' SW - o
        AutoSw(3).X = 0
        AutoSw(3).Y = 80
        ' SE - p
        AutoSw(4).X = 16
        AutoSw(4).Y = 80
        ' Outer Tiles - SE (bottom subtile region)
        ' NW - q
        AutoSe(1).X = 32
        AutoSe(1).Y = 64
        ' NE - r
        AutoSe(2).X = 48
        AutoSe(2).Y = 64
        ' SW - s
        AutoSe(3).X = 32
        AutoSe(3).Y = 80
        ' SE - t
        AutoSe(4).X = 48
        AutoSe(4).Y = 80

        For X = 0 To MyMap.MaxX
            For Y = 0 To MyMap.MaxY
                For layerNum = 0 To LayerType.Count - 1
                    ' calculate the subtile positions and place them
                    CalculateAutotile(x, y, layerNum)
                    ' cache the rendering state of the tiles and set them
                    CacheRenderState(x, y, layerNum)
                Next
            Next
        Next

    End Sub

    Friend Sub CacheRenderState(x As Integer, y As Integer, layerNum As Integer)
        Dim quarterNum As Integer

        If x < 0 Or x > MyMap.MaxX Or y < 0 Or y > MyMap.MaxY Then Exit Sub

        With MyMap.Tile(x, y)
            ' check if the tile can be rendered
            If .Layer(layerNum).Tileset <= 0 Or .Layer(layerNum).Tileset > GameState.NumTileSets Then
                Type.Autotile(x, y).Layer(layerNum).RenderState = GameState.RenderStateNone
                Exit Sub
            End If

            ' check if it needs to be rendered as an autotile
            If .Layer(layerNum).AutoTile = GameState.AutotileNone Or .Layer(layerNum).AutoTile = GameState.AutotileFake Then
                ' default to... default
                Type.Autotile(x, y).Layer(layerNum).RenderState = GameState.RenderStateNormal
            Else
                Type.Autotile(x, y).Layer(layerNum).RenderState = GameState.RenderStateAutotile
                ' cache tileset positioning
                For quarterNum = 0 To 4
                    Type.Autotile(x, y).Layer(layerNum).SrcX(quarterNum) = (MyMap.Tile(x, y).Layer(layerNum).X * 32) + Type.Autotile(x, y).Layer(layerNum).QuarterTile(quarterNum).X
                    Type.Autotile(x, y).Layer(layerNum).SrcY(quarterNum) = (MyMap.Tile(x, y).Layer(layerNum).Y * 32) + Type.Autotile(x, y).Layer(layerNum).QuarterTile(quarterNum).Y
                Next
            End If
        End With

    End Sub

    Friend Sub CalculateAutotile(x As Integer, y As Integer, layerNum As Integer)
        ' Right, so we've split the tile block in to an easy to remember
        ' collection of letters. We now need to do the calculations to find
        ' out which little lettered block needs to be rendered. We do this
        ' by reading the surrounding tiles to check for matches.
        ' First we check to make sure an autotile situation is actually there.
        ' Then we calculate exactly which situation has arisen.
        ' The situations are "inner", "outer", "horizontal", "vertical" and "fill".
        ' Exit out if we don't have an autotile

        If MyMap.Tile(x, y).Layer(layerNum).AutoTile = 0 Then Exit Sub
        ' Okay, we have autotiling but which one?
        Select Case MyMap.Tile(x, y).Layer(layerNum).AutoTile
            ' Normal or animated - same difference
            Case GameState.AutotileNormal, GameState.AutotileAnim
                ' North West Quarter
                CalculateNW_Normal(layerNum, x, y)
                ' North East Quarter
                CalculateNE_Normal(layerNum, x, y)
                ' South West Quarter
                CalculateSW_Normal(layerNum, x, y)
                ' South East Quarter
                CalculateSE_Normal(layerNum, x, y)
            ' Cliff
            Case GameState.AutotileCliff
                ' North West Quarter
                CalculateNW_Cliff(layerNum, x, y)
                ' North East Quarter
                CalculateNE_Cliff(layerNum, x, y)
                ' South West Quarter
                CalculateSW_Cliff(layerNum, x, y)
                ' South East Quarter
                CalculateSE_Cliff(layerNum, x, y)
            ' Waterfalls
            Case GameState.AutotileWaterfall
                ' North West Quarter
                CalculateNW_Waterfall(layerNum, x, y)
                ' North East Quarter
                CalculateNE_Waterfall(layerNum, x, y)
                ' South West Quarter
                CalculateSW_Waterfall(layerNum, x, y)
                ' South East Quarter
                CalculateSE_Waterfall(layerNum, x, y)
                ' Anything else
        End Select

    End Sub

    ' Normal autotiling
    Friend Sub CalculateNW_Normal(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' North West
        If CheckTileMatch(layerNum, x, y, x - 1, y - 1) Then tmpTile(1) = True

        ' North
        If CheckTileMatch(layerNum, x, y, x, y - 1) Then tmpTile(2) = True

        ' West
        If CheckTileMatch(layerNum, x, y, x - 1, y) Then tmpTile(3) = True

        ' Calculate Situation - Inner
        If Not tmpTile(2) And Not tmpTile(3) Then situation = GameState.AutoInner

        ' Horizontal
        If Not tmpTile(2) And tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If tmpTile(2) And Not tmpTile(3) Then situation = GameState.AutoVertical

        ' Outer
        If Not tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoOuter

        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill

        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 1, "e")
            Case GameState.AutoOuter
                PlaceAutotile(layerNum, x, y, 1, "a")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 1, "i")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 1, "m")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 1, "q")
        End Select

    End Sub

    Friend Sub CalculateNE_Normal(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' North
        If CheckTileMatch(layerNum, x, y, x, y - 1) Then tmpTile(1) = True

        ' North East
        If CheckTileMatch(layerNum, x, y, x + 1, y - 1) Then tmpTile(2) = True

        ' East
        If CheckTileMatch(layerNum, x, y, x + 1, y) Then tmpTile(3) = True

        ' Calculate Situation - Inner
        If Not tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoInner

        ' Horizontal
        If Not tmpTile(1) And tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoVertical
        ' Outer
        If tmpTile(1) And Not tmpTile(2) And tmpTile(3) Then situation = GameState.AutoOuter
        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill
        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 2, "j")
            Case GameState.AutoOuter
                PlaceAutotile(layerNum, x, y, 2, "b")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 2, "f")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 2, "r")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 2, "n")
        End Select

    End Sub

    Friend Sub CalculateSW_Normal(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' West
        If CheckTileMatch(layerNum, x, y, x - 1, y) Then tmpTile(1) = True

        ' South West
        If CheckTileMatch(layerNum, x, y, x - 1, y + 1) Then tmpTile(2) = True

        ' South
        If CheckTileMatch(layerNum, x, y, x, y + 1) Then tmpTile(3) = True

        ' Calculate Situation - Inner
        If Not tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoInner

        ' Horizontal
        If tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If Not tmpTile(1) And tmpTile(3) Then situation = GameState.AutoVertical

        ' Outer
        If tmpTile(1) And Not tmpTile(2) And tmpTile(3) Then situation = GameState.AutoOuter

        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill

        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 3, "o")
            Case GameState.AutoOuter
                PlaceAutotile(layerNum, x, y, 3, "c")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 3, "s")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 3, "g")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 3, "k")
        End Select

    End Sub

    Friend Sub CalculateSE_Normal(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' South
        If CheckTileMatch(layerNum, x, y, x, y + 1) Then tmpTile(1) = True

        ' South East
        If CheckTileMatch(layerNum, x, y, x + 1, y + 1) Then tmpTile(2) = True

        ' East
        If CheckTileMatch(layerNum, x, y, x + 1, y) Then tmpTile(3) = True

        ' Calculate Situation - Inner
        If Not tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoInner

        ' Horizontal
        If Not tmpTile(1) And tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoVertical

        ' Outer
        If tmpTile(1) And Not tmpTile(2) And tmpTile(3) Then situation = GameState.AutoOuter

        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill

        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 4, "t")
            Case GameState.AutoOuter
                PlaceAutotile(layerNum, x, y, 4, "d")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 4, "p")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 4, "l")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 4, "h")
        End Select

    End Sub

    ' Waterfall autotiling
    Friend Sub CalculateNW_Waterfall(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile As Boolean

        ' West
        If CheckTileMatch(layerNum, x, y, x - 1, y) Then tmpTile = True
        ' Actually place the subtile
        If tmpTile Then
            ' Extended
            PlaceAutotile(layerNum, x, y, 1, "i")
        Else
            ' Edge
            PlaceAutotile(layerNum, x, y, 1, "e")
        End If

    End Sub

    Friend Sub CalculateNE_Waterfall(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile As Boolean

        ' East
        If CheckTileMatch(layerNum, x, y, x + 1, y) Then tmpTile = True
        ' Actually place the subtile
        If tmpTile Then
            ' Extended
            PlaceAutotile(layerNum, x, y, 2, "f")
        Else
            ' Edge
            PlaceAutotile(layerNum, x, y, 2, "j")
        End If

    End Sub

    Friend Sub CalculateSW_Waterfall(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile As Boolean

        ' West
        If CheckTileMatch(layerNum, x, y, x - 1, y) Then tmpTile = True
        ' Actually place the subtile
        If tmpTile Then
            ' Extended
            PlaceAutotile(layerNum, x, y, 3, "k")
        Else
            ' Edge
            PlaceAutotile(layerNum, x, y, 3, "g")
        End If

    End Sub

    Friend Sub CalculateSE_Waterfall(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile As Boolean

        ' East
        If CheckTileMatch(layerNum, x, y, x + 1, y) Then tmpTile = True
        ' Actually place the subtile
        If tmpTile Then
            ' Extended
            PlaceAutotile(layerNum, x, y, 4, "h")
        Else
            ' Edge
            PlaceAutotile(layerNum, x, y, 4, "l")
        End If

    End Sub

    ' Cliff autotiling
    Friend Sub CalculateNW_Cliff(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' North West
        If CheckTileMatch(layerNum, x, y, x - 1, y - 1) Then tmpTile(1) = True

        ' North
        If CheckTileMatch(layerNum, x, y, x, y - 1) Then tmpTile(2) = True

        ' West
        If CheckTileMatch(layerNum, x, y, x - 1, y) Then tmpTile(3) = True
        situation = GameState.AutoFill

        ' Calculate Situation - Horizontal
        If Not tmpTile(2) And tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If tmpTile(2) And Not tmpTile(3) Then situation = GameState.AutoVertical

        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill

        ' Inner
        If Not tmpTile(2) And Not tmpTile(3) Then situation = GameState.AutoInner

        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 1, "e")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 1, "i")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 1, "m")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 1, "q")
        End Select

    End Sub

    Friend Sub CalculateNE_Cliff(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' North
        If CheckTileMatch(layerNum, x, y, x, y - 1) Then tmpTile(1) = True

        ' North East
        If CheckTileMatch(layerNum, x, y, x + 1, y - 1) Then tmpTile(2) = True

        ' East
        If CheckTileMatch(layerNum, x, y, x + 1, y) Then tmpTile(3) = True
        situation = GameState.AutoFill

        ' Calculate Situation - Horizontal
        If Not tmpTile(1) And tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoVertical

        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill

        ' Inner
        If Not tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoInner

        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 2, "j")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 2, "f")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 2, "r")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 2, "n")
        End Select

    End Sub

    Friend Sub CalculateSW_Cliff(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' West
        If CheckTileMatch(layerNum, x, y, x - 1, y) Then tmpTile(1) = True

        ' South West
        If CheckTileMatch(layerNum, x, y, x - 1, y + 1) Then tmpTile(2) = True

        ' South
        If CheckTileMatch(layerNum, x, y, x, y + 1) Then tmpTile(3) = True
        situation = GameState.AutoFill

        ' Calculate Situation - Horizontal
        If tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If Not tmpTile(1) And tmpTile(3) Then situation = GameState.AutoVertical

        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill

        ' Inner
        If Not tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoInner
        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 3, "o")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 3, "s")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 3, "g")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 3, "k")
        End Select

    End Sub

    Friend Sub CalculateSE_Cliff(layerNum As Integer, x As Integer, y As Integer)
        Dim tmpTile(3) As Boolean
        Dim situation As Byte

        ' South
        If CheckTileMatch(layerNum, x, y, x, y + 1) Then tmpTile(1) = True

        ' South East
        If CheckTileMatch(layerNum, x, y, x + 1, y + 1) Then tmpTile(2) = True

        ' East
        If CheckTileMatch(layerNum, x, y, x + 1, y) Then tmpTile(3) = True

        situation = GameState.AutoFill
        ' Calculate Situation -  Horizontal
        If Not tmpTile(1) And tmpTile(3) Then situation = GameState.AutoHorizontal

        ' Vertical
        If tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoVertical

        ' Fill
        If tmpTile(1) And tmpTile(2) And tmpTile(3) Then situation = GameState.AutoFill

        ' Inner
        If Not tmpTile(1) And Not tmpTile(3) Then situation = GameState.AutoInner

        ' Actually place the subtile
        Select Case situation
            Case GameState.AutoInner
                PlaceAutotile(layerNum, x, y, 4, "t")
            Case GameState.AutoHorizontal
                PlaceAutotile(layerNum, x, y, 4, "p")
            Case GameState.AutoVertical
                PlaceAutotile(layerNum, x, y, 4, "l")
            Case GameState.AutoFill
                PlaceAutotile(layerNum, x, y, 4, "h")
        End Select

    End Sub

    Friend Function CheckTileMatch(layerNum As Integer, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer) As Boolean
        CheckTileMatch = True

        ' if it's off the map then set it as autotile and exit out early
        If x2 < 0 Or x2 > MyMap.MaxX Or y2 < 0 Or y2 > MyMap.MaxY Then
            CheckTileMatch = True
            Exit Function
        End If

        ' fakes ALWAYS return true
        If MyMap.Tile(x2, y2).Layer(layerNum).AutoTile = GameState.AutotileFake Then
            CheckTileMatch = True
            Exit Function
        End If

        ' check neighbour is an autotile
        If MyMap.Tile(x2, y2).Layer(layerNum).AutoTile = 0 Then
            CheckTileMatch = False
            Exit Function
        End If

        ' check we're a matching
        If MyMap.Tile(x1, y1).Layer(layerNum).Tileset <> MyMap.Tile(x2, y2).Layer(layerNum).Tileset Then
            CheckTileMatch = False
            Exit Function
        End If

        ' check tiles match
        If MyMap.Tile(x1, y1).Layer(layerNum).X <> MyMap.Tile(x2, y2).Layer(layerNum).X Then
            CheckTileMatch = False
            Exit Function
        Else
            If MyMap.Tile(x1, y1).Layer(layerNum).Y <> MyMap.Tile(x2, y2).Layer(layerNum).Y Then
                CheckTileMatch = False
                Exit Function
            End If
        End If
    End Function

End Module