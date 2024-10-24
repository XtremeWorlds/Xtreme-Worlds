Imports System.Configuration
Imports System.Drawing
Imports System.IO
Imports Core
Imports Microsoft.Xna.Framework.Graphics
Imports Mirage.Sharp.Asfw
Imports Mirage.Sharp.Asfw.IO

Module Map
#Region "Drawing"
    Friend Sub DrawThunderEffect()
        If DrawThunder > 0 Then
            ' Create a temporary texture matching the camera size
            Using thunderTexture As New Texture2D(Client.GraphicsDevice, ResolutionWidth, ResolutionHeight)
                ' Create an array to store pixel data
                Dim whitePixels(ResolutionWidth * ResolutionHeight - 1) As Microsoft.Xna.Framework.Color

                ' Fill the pixel array with semi-transparent white pixels
                For i = 0 To whitePixels.Length - 1
                    whitePixels(i) = New Microsoft.Xna.Framework.Color(255, 255, 255, 150) ' White with 150 alpha
                Next

                ' Set the pixel data for the texture
                thunderTexture.SetData(whitePixels)

                ' Begin SpriteBatch to render the thunder effect
                Client.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive)
                Client.SpriteBatch.Draw(thunderTexture, New Microsoft.Xna.Framework.Rectangle(0, 0, ResolutionWidth, ResolutionHeight), Microsoft.Xna.Framework.Color.White)
                Client.SpriteBatch.End()
            End Using

            ' Decrease the thunder counter
            DrawThunder -= 1
        End If
    End Sub

    Friend Sub DrawWeather()
        Dim i As Integer, spriteLeft As Integer

        For i = 1 To MaxWeatherParticles
            If WeatherParticle(i).InUse Then
                If WeatherParticle(i).Type = [Enum].Weather.Storm Then
                    spriteLeft = 0
                Else
                    spriteLeft = WeatherParticle(i).Type - 1
                End If

                Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Misc, "Weather"), ConvertMapX(WeatherParticle(i).X), ConvertMapY(WeatherParticle(i).Y), spriteLeft * 32, 0, 32, 32, 32, 32)
            End If
        Next
    End Sub

    Friend Sub DrawFog()
        Dim fogNum As Integer = CurrentFog

        If fogNum <= 0 Or fogNum > NumFogs Then Exit Sub

        Dim sX As Integer = 0
        Dim sY As Integer = 0
        Dim sW As Integer = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Fogs, fogNum)).Width  ' Using the full width of the fog texture
        Dim sH As Integer = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Fogs, fogNum)).Height ' Using the full height of the fog texture

        ' These should match the scale calculations for full coverage plus extra area
        Dim dX As Integer = (FogOffsetX * 2.5) - 50
        Dim dY As Integer = (FogOffsetY * 3.5) - 50
        Dim dW As Integer = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Fogs, fogNum)).Width + 200
        Dim dH As Integer = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Fogs, fogNum)).Height + 200

        Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Fogs, fogNum), dX, dY, sX, sY, dW, dH, sW, sH, CurrentFogOpacity)
    End Sub

    Friend Sub DrawMapLowerTile(x As Integer, y As Integer)
        Dim i As Integer, alpha As Byte
        Dim rect As New Rectangle(0, 0, 0, 0)

        ' Check if the map or its tile data is not ready
        If GettingMap OrElse MyMap.Tile Is Nothing OrElse MapData = False Then Exit Sub

        ' Ensure x and y are within the bounds of the map
        If x < 0 OrElse y < 0 OrElse x > MyMap.MaxX OrElse y > MyMap.MaxY Then Exit Sub

        On Error GoTo mapsync

        For i = LayerType.Ground To LayerType.CoverAnim
            ' Check if the tile's layer array is initialized
            If MyMap.Tile(x, y).Layer Is Nothing Then Exit Sub

            ' Check if this layer has a valid tileset
            If MyMap.Tile(x, y).Layer(i).Tileset > 0 AndAlso MyMap.Tile(x, y).Layer(i).Tileset <= NumTileSets Then
                ' Normal rendering state
                If Type.Autotile(x, y).Layer(i).RenderState = RenderStateNormal Then
                    With rect
                        .X = MyMap.Tile(x, y).Layer(i).X * PicX
                        .Y = MyMap.Tile(x, y).Layer(i).Y * PicY
                        .Width = PicX
                        .Height = PicY
                    End With
                    
                    alpha = 255
                    
                    ' Render the tile
                    Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Tilesets, MyMap.Tile(x, y).Layer(i).Tileset), ConvertMapX(x * PicX), ConvertMapY(y * PicY), rect.X, rect.Y, rect.Width, rect.Height, rect.Width, rect.Height, alpha)

                    ' Autotile rendering state
                ElseIf Type.Autotile(x, y).Layer(i).RenderState = RenderStateAutotile Then
                    If Type.Setting.Autotile Then
                        DrawAutoTile(i, ConvertMapX(x * PicX), ConvertMapY(y * PicY), 1, x, y, 0, False)
                        DrawAutoTile(i, ConvertMapX(x * PicX) + 16, ConvertMapY(y * PicY), 2, x, y, 0, False)
                        DrawAutoTile(i, ConvertMapX(x * PicX), ConvertMapY(y * PicY) + 16, 3, x, y, 0, False)
                        DrawAutoTile(i, ConvertMapX(x * PicX) + 16, ConvertMapY(y * PicY) + 16, 4, x, y, 0, False)
                    End If
                End If
            End If
        Next

mapsync:
    End Sub

    Friend Sub DrawMapUpperTile(x As Integer, y As Integer)
        Dim i As Integer, alpha As Integer
        Dim rect As Rectangle

        ' Exit earlyIf Type.Map is still loading or tile data is not available
        If GettingMap OrElse MyMap.Tile Is Nothing OrElse MapData = False Then Exit Sub

        ' Ensure x and y are within valid map bounds
        If x < 0 OrElse y < 0 OrElse x > MyMap.MaxX OrElse y > MyMap.MaxY Then Exit Sub

        On Error GoTo mapsync

        ' Loop through the layers from Fringe to RoofAnim
        For i = LayerType.Fringe To LayerType.RoofAnim
            ' Ensure that the layer array exists for the current tile
            If MyMap.Tile(x, y).Layer Is Nothing Then Exit Sub

            ' Handle animated layers
            If MapAnim = 1 Then
                Select Case i
                    Case LayerType.Fringe
                        i = LayerType.Fringe
                    Case LayerType.Roof
                        i = LayerType.Roof
                End Select
            End If

            ' Ensure the tileset is valid before proceeding
            If MyMap.Tile(x, y).Layer(i).Tileset > 0 AndAlso MyMap.Tile(x, y).Layer(i).Tileset <= NumTileSets Then
                ' Check if the render state is normal and render the tile
                If Type.Autotile(x, y).Layer(i).RenderState = RenderStateNormal Then
                    With rect
                        .X = MyMap.Tile(x, y).Layer(i).X * PicX
                        .Y = MyMap.Tile(x, y).Layer(i).Y * PicY
                        .Width = PicX
                        .Height = PicY
                    End With

                    alpha = 255

                    ' Render the tile with the calculated rectangle and transparency
                    Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Tilesets, MyMap.Tile(x, y).Layer(i).Tileset), ConvertMapX(x * PicX), ConvertMapY(y * PicY), rect.X, rect.Y, rect.Width, rect.Height, rect.Width, rect.Height, alpha)

                    ' Handle autotile rendering
                ElseIf Type.Autotile(x, y).Layer(i).RenderState = RenderStateAutotile Then
                    If Type.Setting.Autotile Then
                        ' Render autotiles
                        DrawAutoTile(i, ConvertMapX(x * PicX), ConvertMapY(y * PicY), 1, x, y, 0, False)
                        DrawAutoTile(i, ConvertMapX(x * PicX) + 16, ConvertMapY(y * PicY), 2, x, y, 0, False)
                        DrawAutoTile(i, ConvertMapX(x * PicX), ConvertMapY(y * PicY) + 16, 3, x, y, 0, False)
                        DrawAutoTile(i, ConvertMapX(x * PicX) + 16, ConvertMapY(y * PicY) + 16, 4, x, y, 0, False)
                    End If
                End If
            End If
        Next

mapsync:
    End Sub

    Friend Sub DrawAutoTile(layerNum As Integer, dX As Integer, dY As Integer, quarterNum As Integer, x As Integer, y As Integer, Optional forceFrame As Integer = 0, Optional strict As Boolean = True)
        Dim yOffset As Integer, xOffset As Integer

        ' calculate the offset
        If forceFrame > 0 Then
            Select Case forceFrame - 1
                Case 0
                    WaterfallFrame = 1
                Case 1
                    WaterfallFrame = 2
                Case 2
                    WaterfallFrame = 0
            End Select

            ' animate autotiles
            Select Case forceFrame - 1
                Case 0
                    AutoTileFrame = 1
                Case 1
                    AutoTileFrame = 2
                Case 2
                    AutoTileFrame = 0
            End Select
        End If

        Select Case MyMap.Tile(x, y).Layer(layerNum).AutoTile
            Case AutotileWaterfall
                yOffset = (WaterfallFrame - 1) * 32
            Case AutotileAnim
                xOffset = AutoTileFrame * 64
            Case AutotileCliff
                yOffset = -32
        End Select

        If MyMap.Tile(x, y).Layer Is Nothing Then Exit Sub
        Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Tilesets, MyMap.Tile(x, y).Layer(layerNum).Tileset), dX, dY, Type.Autotile(x, y).Layer(layerNum).SrcX(quarterNum) + xOffset, Type.Autotile(x, y).Layer(layerNum).SrcY(quarterNum) + yOffset, 16, 16, 16, 16)
    End Sub

    Friend Sub DrawMapTint()
        If MyMap.MapTint = 0 Then Exit Sub ' Skip if no tint is applied

        ' Create a new texture matching the camera size
        Dim tintTexture As New Texture2D(Client.GraphicsDevice, ResolutionWidth, ResolutionHeight)
        Dim tintPixels(ResolutionWidth * ResolutionHeight - 1) As Microsoft.Xna.Framework.Color

        ' Define the tint color with the given RGBA values
        Dim tintColor As New Microsoft.Xna.Framework.Color(CurrentTintR, CurrentTintG, CurrentTintB, CurrentTintA)

        ' Fill the texture's pixel array with the tint color
        For i = 0 To tintPixels.Length - 1
            tintPixels(i) = tintColor
        Next

        ' Set the pixel data on the texture
        tintTexture.SetData(tintPixels)

        ' Start the sprite batch
        Client.SpriteBatch.Begin()

        ' Draw the tinted texture over the entire camera view
        Client.SpriteBatch.Draw(tintTexture,
                         New Microsoft.Xna.Framework.Rectangle(0, 0, ResolutionWidth, ResolutionHeight),
                         Microsoft.Xna.Framework.Color.White)

        Client.SpriteBatch.End()

        ' Dispose of the temporary texture to free resources
        tintTexture.Dispose()
    End Sub

    Friend Sub DrawMapFade()
        If Not UseFade Then Exit Sub ' Exit if fading is disabled

        ' Create a new texture matching the camera view size
        Dim fadeTexture As New Texture2D(Client.GraphicsDevice, ResolutionWidth, ResolutionHeight)
        Dim blackPixels(ResolutionWidth * ResolutionHeight - 1) As Microsoft.Xna.Framework.Color

        ' Fill the pixel array with black color and specified alpha for the fade effect
        For i = 0 To blackPixels.Length - 1
            blackPixels(i) = New Microsoft.Xna.Framework.Color(0, 0, 0, FadeAmount)
        Next

        ' Set the texture's pixel data
        fadeTexture.SetData(blackPixels)

        ' Start the sprite batch
        Client.SpriteBatch.Begin()

        ' Draw the fade texture over the entire camera view
        Client.SpriteBatch.Draw(fadeTexture,
                         New Microsoft.Xna.Framework.Rectangle(0, 0, ResolutionWidth, ResolutionHeight),
                         Microsoft.Xna.Framework.Color.White)

        Client.SpriteBatch.End()

        ' Dispose of the texture to free resources
        fadeTexture.Dispose()
    End Sub

    Friend Sub DrawPanorama(index As Integer)
        If MyMap.Indoors Then Exit Sub

        If index < 1 Or index > NumPanoramas Then Exit Sub

        Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Panoramas & index),
                      0, 0, 0, 0,
                       Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Panoramas, index)).Width, Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Panoramas, index)).Height,
                       Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Panoramas, index)).Width, Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Panoramas, index)).Height)
    End Sub

    Friend Sub DrawParallax(index As Integer)
        Dim horz As Single = 0
        Dim vert As Single = 0

        If MyMap.Moral = MyMap.Indoors Then Exit Sub
        If index < 1 Or index > NumParallax Then Exit Sub

        ' Calculate horizontal and vertical offsets based on player position
        horz = ConvertMapX(GetPlayerX(MyIndex)) * 2.5F - 50
        vert = ConvertMapY(GetPlayerY(MyIndex)) * 2.5F - 50

        Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Parallax, index),
                      CInt(horz), CInt(vert), 0, 0,
                       Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Parallax, index)).Width, Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Parallax, index)).Height,
                       Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Parallax, index)).Width, Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Parallax, index)).Height)
    End Sub

    Friend Sub DrawPicture(Optional index As Integer = 0, Optional type As Integer = 0)
        If index = 0 Then
            index = Picture.Index
        End If

        If type = 0 Then
            type = Picture.SpriteType
        End If

        If index < 1 Or index > NumPictures Then Exit Sub
        If type < 0 Or type >= PictureType.Count Then Exit Sub

        Dim posX As Integer = 0
        Dim posY As Integer = 0

        ' Determine position based on type
        Select Case type
            Case PictureType.TopLeft
                posX = 0 - Picture.xOffset
                posY = 0 - Picture.yOffset

            Case PictureType.CenterScreen
                posX = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Pictures, index)).Width / 2 - Client.GetGfxInfo((Core.Path.Pictures & index)).Width / 2 - Picture.xOffset
                posY = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Pictures, index)).Height / 2 - Client.GetGfxInfo((Core.Path.Pictures & index)).Height / 2 - Picture.yOffset

            Case PictureType.CenterEvent
                If CurrentEvents < Picture.EventId Then
                    ' Reset picture details and exit if event is invalid
                    Picture.EventId = 0
                    Picture.Index = 0
                    Picture.SpriteType = 0
                    Picture.xOffset = 0
                    Picture.yOffset = 0
                    Exit Sub
                End If
                posX = ConvertMapX(MapEvents(Picture.EventId).X * 32) / 2 - Picture.xOffset
                posY = ConvertMapY(MapEvents(Picture.EventId).Y * 32) / 2 - Picture.yOffset

            Case PictureType.CenterPlayer
                posX = ConvertMapX(Core.Type.Player(MyIndex).X * 32) / 2 - Picture.xOffset
                posY = ConvertMapY(Core.Type.Player(MyIndex).Y * 32) / 2 - Picture.yOffset
        End Select

        Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Pictures, index), posX, posY, 0, 0,
                       Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Pictures, index)).Width, Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Pictures, index)).Height, Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Pictures, index)).Width, Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Pictures, index)).Height)
    End Sub

#End Region

#Region "Database"

    Sub ClearMap()
        Dim i As Integer, x As Integer, y As Integer

        MyMap.Name = ""
        MyMap.Tileset = 1
        MyMap.MaxX = MAX_MAPX
        MyMap.MaxY = MAX_MAPY
        MyMap.BootMap = 0
        MyMap.BootX = 0
        MyMap.BootY = 0
        MyMap.Down = 0
        MyMap.Left = 0
        MyMap.Moral = 0
        MyMap.Music = ""
        MyMap.Revision = 0
        MyMap.Right = 0
        MyMap.Up = 0

        ReDim MyMap.NPC(MAX_MAP_NPCS)
        ReDim MyMap.Tile(MyMap.MaxX, MyMap.MaxY)
        ReDim TileHistory(MaxTileHistory)
        For i = 0 To MaxTileHistory
            ReDim TileHistory(i).Tile(MAX_MAPX,MAX_MAPY)
        Next
HistoryIndex = 0
        TileHistoryHighIndex = 0

        For x = 0 To MAX_MAPX
            For y = 0 To MAX_MAPY
                ReDim MyMap.Tile(x, y).Layer(LayerType.Count - 1)

                For i = 0 To MaxTileHistory
                    ReDim TileHistory(i).Tile(x, y).Layer(LayerType.Count - 1)
                Next

                For l = 1 To LayerType.Count - 1
                    MyMap.Tile(x, y).Layer(l).Tileset = 0
                    MyMap.Tile(x, y).Layer(l).X = 0
                    MyMap.Tile(x, y).Layer(l).Y = 0
                    MyMap.Tile(x, y).Layer(l).AutoTile = 0
                    MyMap.Tile(x, y).Data1 = 0
                    MyMap.Tile(x, y).Data2 = 0
                    MyMap.Tile(x, y).Data3 = 0
                    MyMap.Tile(x, y).Data1_2 = 0
                    MyMap.Tile(x, y).Data2_2 = 0
                    MyMap.Tile(x, y).Data3_2 = 0
                    MyMap.Tile(x, y).Type = 0
                    MyMap.Tile(x, y).Type2 = 0
                    MyMap.Tile(x, y).DirBlock = 0

                    For i = 1 To MaxTileHistory
                        TileHistory(i).Tile(x, y).Layer(l).Tileset = 0
                        TileHistory(i).Tile(x, y).Layer(l).X = 0
                        TileHistory(i).Tile(x, y).Layer(l).Y = 0
                        TileHistory(i).Tile(x, y).Layer(l).AutoTile = 0
                        TileHistory(i).Tile(x, y).Data1 = 0
                        TileHistory(i).Tile(x, y).Data2 = 0
                        TileHistory(i).Tile(x, y).Data3 = 0
                        TileHistory(i).Tile(x, y).Type = 0
                        TileHistory(i).Tile(x, y).DirBlock = 0
                    Next
                Next

            Next
        Next

        ClearMapEvents()
        TileLights = Nothing

    End Sub

    Sub ClearMapItems()
        Dim i As Integer

        For i = 1 To MAX_MAP_ITEMS
            ClearMapItem(i)
        Next

    End Sub

    Sub ClearMapItem(index As Integer)
        Dim batchList = Client.Batches.ToList() ' Snapshot of the queue

        SyncLock Client.BatchLock
            For Each batch In batchList
                ' Search for an existing command and update its properties
                Dim existingCommand = batch.Commands.FirstOrDefault(Function(cmd) cmd.Path = System.IO.Path.Combine(Core.Path.Characters, Type.Item(MyMapItem(index).Num).Icon))
                
                if existingCommand.dRect.X = MyMapItem(index).X And existingCommand.dRect.Y = MyMapItem(index).Y Then
                    batch.Commands.Remove(existingCommand)
                    Exit For
                End If
            Next
        End SyncLock
        
        MyMapItem(index).Num = 0
        MyMapItem(index).Value = 0
        MyMapItem(index).X = 0
        MyMapItem(index).Y = 0
    End Sub

    Sub ClearMapNPC(index As Integer)
        With MyMapNPC(index)
            Dim batchList = Client.Batches.ToList() ' Snapshot of the queue

            SyncLock Client.BatchLock
                For Each batch In batchList
                    ' Search for an existing command and update its properties
                    Dim existingCommand = batch.Commands.FirstOrDefault(Function(cmd) cmd.Path = System.IO.Path.Combine(Core.Path.Characters, Type.NPC(.Num).Sprite))
                
                    if existingCommand.dRect.X = .X And existingCommand.dRect.Y = .Y Then
                        batch.Commands.Remove(existingCommand)
                        Exit For
                    End If
                Next
            End SyncLock
            
            .Attacking = 0
            .AttackTimer = 0
            .Dir = 0
            .Moving = 0
            .Num = 0
            .Steps = 0
            .Target = 0
            .TargetType = 0
            ReDim .Vital(VitalType.Count - 1)
            .Vital(VitalType.HP) = 0
            .Vital(VitalType.SP) = 0
            .Vital(VitalType.SP) = 0
            .X = 0
            .XOffset = 0
            .Y = 0
            .YOffset = 0
        End With
    End Sub

    Sub ClearMapNPCs()
        Dim i As Integer

        For i = 1 To MAX_MAP_NPCS
            ClearMapNPC(i)
        Next

    End Sub

#End Region

#Region "Incoming Packets"

    Friend Sub Packet_EditMap(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)
        
        buffer.Dispose()
    End Sub

    Sub Packet_CheckMap(ByRef data() As Byte)
        Dim x As Integer, y As Integer, i As Integer
        Dim needMap As Byte
        Dim buffer As New ByteStream(data)

        GettingMap = True

        ' Erase all players except self
        For i = 1 To MAX_PLAYERS
            If i <> MyIndex Then
                SetPlayerMap(i, 0)
            End If
        Next

        ' Erase all temporary tile values
        ClearMapNPCs()
        ClearMapItems()
        ClearBlood()
        ClearMap()
        ClearMapEvents()

        ' Get map num
        x = buffer.ReadInt32

        ' Get revision
        y = buffer.ReadInt32

        needMap = 1

        ' Either the revisions didn't match or we dont have the map, so we need it
        buffer = New ByteStream(4)
        buffer.WriteInt32(ClientPackets.CNeedMap)
        buffer.WriteInt32(needMap)
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Sub Packet_MapData(ByRef data() As Byte)
        Dim x As Integer, y As Integer, i As Integer, j As Integer, mapNum As Integer
        Dim buffer As New ByteStream(Compression.DecompressBytes(data))

        MapData = False

        ClearMap()

        If buffer.ReadInt32 = 1 Then
            mapNum = buffer.ReadInt32
            MyMap.Name = buffer.ReadString
            MyMap.Music = buffer.ReadString
            MyMap.Revision = buffer.ReadInt32
            MyMap.Moral = buffer.ReadInt32
            MyMap.Tileset = buffer.ReadInt32
            MyMap.Up = buffer.ReadInt32
            MyMap.Down = buffer.ReadInt32
            MyMap.Left = buffer.ReadInt32
            MyMap.Right = buffer.ReadInt32
            MyMap.BootMap = buffer.ReadInt32
            MyMap.BootX = buffer.ReadInt32
            MyMap.BootY = buffer.ReadInt32
            MyMap.MaxX = buffer.ReadInt32
            MyMap.MaxY = buffer.ReadInt32
            MyMap.Weather = buffer.ReadInt32
            MyMap.Fog = buffer.ReadInt32
            MyMap.WeatherIntensity = buffer.ReadInt32
            MyMap.FogOpacity = buffer.ReadInt32
            MyMap.FogSpeed = buffer.ReadInt32
            MyMap.MapTint = buffer.ReadInt32
            MyMap.MapTintR = buffer.ReadInt32
            MyMap.MapTintG = buffer.ReadInt32
            MyMap.MapTintB = buffer.ReadInt32
            MyMap.MapTintA = buffer.ReadInt32
            MyMap.Panorama = buffer.ReadByte
            MyMap.Parallax = buffer.ReadByte
            MyMap.Brightness = buffer.ReadByte
            MyMap.NoRespawn = buffer.ReadInt32
            MyMap.Indoors = buffer.ReadInt32
            MyMap.Shop = buffer.ReadInt32

            ReDim MyMap.Tile(MyMap.MaxX, MyMap.MaxY)
            For i = 0 To MaxTileHistory
                ReDim TileHistory(i).Tile(MyMap.MaxX, MyMap.MaxY)
            Next

            For x = 1 To MAX_MAP_NPCS
                MyMap.NPC(x) = buffer.ReadInt32
            Next

            For x = 0 To MyMap.MaxX
                For y = 0 To MyMap.MaxY
                    MyMap.Tile(x, y).Data1 = buffer.ReadInt32
                    MyMap.Tile(x, y).Data2 = buffer.ReadInt32
                    MyMap.Tile(x, y).Data3 = buffer.ReadInt32
                    MyMap.Tile(x, y).Data1_2 = buffer.ReadInt32
                    MyMap.Tile(x, y).Data2_2 = buffer.ReadInt32
                    MyMap.Tile(x, y).Data3_2 = buffer.ReadInt32
                    MyMap.Tile(x, y).DirBlock = buffer.ReadInt32

                    For j = 1 To MaxTileHistory
                        TileHistory(j).Tile(x, y).Data1 = MyMap.Tile(x, y).Data1
                        TileHistory(j).Tile(x, y).Data2 = MyMap.Tile(x, y).Data2
                        TileHistory(j).Tile(x, y).Data3 = MyMap.Tile(x, y).Data3
                        TileHistory(j).Tile(x, y).Data1_2 = MyMap.Tile(x, y).Data1_2
                        TileHistory(j).Tile(x, y).Data2_2 = MyMap.Tile(x, y).Data2_2
                        TileHistory(j).Tile(x, y).Data3_2 = MyMap.Tile(x, y).Data3_2
                        TileHistory(j).Tile(x, y).DirBlock = MyMap.Tile(x, y).DirBlock
                        TileHistory(j).Tile(x, y).Type = MyMap.Tile(x, y).Type
                        TileHistory(j).Tile(x, y).Type2 = MyMap.Tile(x, y).Type2
                    Next

                    ReDim MyMap.Tile(x, y).Layer(LayerType.Count - 1)
                    For i = 0 To MaxTileHistory
                        ReDim TileHistory(i).Tile(x, y).Layer(LayerType.Count - 1)
                    Next

                    For i = 1 To LayerType.Count - 1
                        MyMap.Tile(x, y).Layer(i).Tileset = buffer.ReadInt32
                        MyMap.Tile(x, y).Layer(i).X = buffer.ReadInt32
                        MyMap.Tile(x, y).Layer(i).Y = buffer.ReadInt32
                        MyMap.Tile(x, y).Layer(i).AutoTile = buffer.ReadInt32

                        For j = 1 To MaxTileHistory
                            TileHistory(j).Tile(x, y).Layer(i).Tileset = MyMap.Tile(x, y).Layer(i).Tileset
                            TileHistory(j).Tile(x, y).Layer(i).X = MyMap.Tile(x, y).Layer(i).X
                            TileHistory(j).Tile(x, y).Layer(i).Y = MyMap.Tile(x, y).Layer(i).Y
                            TileHistory(j).Tile(x, y).Layer(i).AutoTile = MyMap.Tile(x, y).Layer(i).AutoTile
                        Next
                    Next

                    MyMap.Tile(x, y).Type = buffer.ReadInt32
                    MyMap.Tile(x, y).Type2 = buffer.ReadInt32
                Next
            Next

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
                                                    ReDim Preserve .MoveRoute(.MoveRouteCount)
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
        End If

        For i = 1 To MAX_MAP_ITEMS
            MyMapItem(i).Num = buffer.ReadInt32
            MyMapItem(i).Value = buffer.ReadInt32()
            MyMapItem(i).X = buffer.ReadInt32()
            MyMapItem(i).Y = buffer.ReadInt32()
        Next

        For i = 1 To MAX_MAP_NPCS
            Type.MyMapNPC(i).Num = buffer.ReadInt32()
            Type.MyMapNPC(i).X = buffer.ReadInt32()
            Type.MyMapNPC(i).Y = buffer.ReadInt32()
            Type.MyMapNPC(i).Dir = buffer.ReadInt32()
            For n = 1 To VitalType.Count - 1
                Type.MyMapNPC(i).Vital(n) = buffer.ReadInt32()
            Next
        Next

        If buffer.ReadInt32 = 1 Then
            ResourceIndex = buffer.ReadInt32
            ResourcesInit = False
            ReDim MapResource(ResourceIndex)

            If ResourceIndex > 0 Then
                For i = 0 To ResourceIndex
                    MyMapResource(i).State = buffer.ReadByte
                    MyMapResource(i).X = buffer.ReadInt32
                    MyMapResource(i).Y = buffer.ReadInt32
                Next

                ResourcesInit = True
            End If
        End If

        Type.Map(GetPlayerMap(MyIndex)) = MyMap

        buffer.Dispose()

        InitAutotiles()

        MapData = True

        For i = 0 To Byte.MaxValue
            ClearActionMsg(i)
        Next

        CurrentWeather = MyMap.Weather
        CurrentWeatherIntensity = MyMap.WeatherIntensity
        CurrentFog = MyMap.Fog
        CurrentFogSpeed = MyMap.FogSpeed
        CurrentFogOpacity = MyMap.FogOpacity
        CurrentTintR = MyMap.MapTintR
        CurrentTintG = MyMap.MapTintG
        CurrentTintB = MyMap.MapTintB
        CurrentTintA = MyMap.MapTintA

        UpdateDrawMapName()

        GettingMap = False
        CanMoveNow = True

    End Sub

    Sub Packet_MapNPCData(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        For i = 1 To MAX_MAP_NPCS

            With Type.MyMapNPC(i)
                .Num = buffer.ReadInt32
                .X = buffer.ReadInt32
                .Y = buffer.ReadInt32
                .Dir = buffer.ReadInt32
                .Vital(VitalType.HP) = buffer.ReadInt32
            End With

        Next

        buffer.Dispose()
    End Sub

    Sub Packet_MapNPCUpdate(ByRef data() As Byte)
        Dim npcNum As Integer
        Dim buffer As New ByteStream(data)

        npcNum = buffer.ReadInt32

        With MyMapNPC(NPCNum)
            .Num = buffer.ReadInt32
.X = buffer.ReadInt32
            .Y = buffer.ReadInt32
            .Dir = buffer.ReadInt32
            .Vital(VitalType.HP) = buffer.ReadInt32
            .Vital(VitalType.SP) = buffer.ReadInt32
        End With

        buffer.Dispose()
    End Sub

    Sub Packet_MapDone(ByRef data() As Byte)
        Dim i As Integer

        For i = 0 To Byte.MaxValue
            ClearActionMsg(i)
        Next

        CurrentWeather = MyMap.Weather
        CurrentWeatherIntensity = MyMap.WeatherIntensity
        CurrentFog = MyMap.Fog
        CurrentFogSpeed = MyMap.FogSpeed
        CurrentFogOpacity = MyMap.FogOpacity
        CurrentTintR = MyMap.MapTintR
        CurrentTintG = MyMap.MapTintG
        CurrentTintB = MyMap.MapTintB
        CurrentTintA = MyMap.MapTintA

        UpdateDrawMapName()

        GettingMap = False
        CanMoveNow = True

    End Sub

#End Region

#Region "Outgoing Packets"

    Friend Sub SendPlayerRequestNewMap()
        If GettingMap Then Exit Sub

        If MyMap.Tile(GetPlayerX(MyIndex), GetPlayerY(MyIndex)).Type = TileType.NoXing Or MyMap.Tile(GetPlayerX(MyIndex), GetPlayerY(MyIndex)).Type2 = TileType.NoXing Then
            AddText("The pathway is blocked.", ColorType.BrightRed)
            Exit Sub
        End If
 
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestNewMap)
        buffer.WriteInt32(GetPlayerDir(MyIndex))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

        GettingMap = True
        CanMoveNow = False

    End Sub

    Friend Sub SendRequestEditMap()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditMap)
        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

    Friend Sub SendMap()
        Dim x As Integer, y As Integer, i As Integer
        Dim data() As Byte
        Dim buffer As New ByteStream(4)

        CanMoveNow = False

        buffer.WriteString(MyMap.Name)
        buffer.WriteString(MyMap.Music)
        buffer.WriteInt32(MyMap.Moral)
        buffer.WriteInt32(MyMap.Tileset)
        buffer.WriteInt32(MyMap.Up)
        buffer.WriteInt32(MyMap.Down)
        buffer.WriteInt32(MyMap.Left)
        buffer.WriteInt32(MyMap.Right)
        buffer.WriteInt32(MyMap.BootMap)
        buffer.WriteInt32(MyMap.BootX)
        buffer.WriteInt32(MyMap.BootY)
        buffer.WriteInt32(MyMap.MaxX)
        buffer.WriteInt32(MyMap.MaxY)
        buffer.WriteInt32(MyMap.Weather)
        buffer.WriteInt32(MyMap.Fog)
        buffer.WriteInt32(MyMap.WeatherIntensity)
        buffer.WriteInt32(MyMap.FogOpacity)
        buffer.WriteInt32(MyMap.FogSpeed)
        buffer.WriteInt32(MyMap.MapTint)
        buffer.WriteInt32(MyMap.MapTintR)
        buffer.WriteInt32(MyMap.MapTintG)
        buffer.WriteInt32(MyMap.MapTintB)
        buffer.WriteInt32(MyMap.MapTintA)
        buffer.WriteByte(MyMap.Panorama)
        buffer.WriteByte(MyMap.Parallax)
        buffer.WriteByte(MyMap.Brightness)
        buffer.WriteInt32(MyMap.NoRespawn)
        buffer.WriteInt32(MyMap.Indoors)
        buffer.WriteInt32(MyMap.Shop)

        For i = 1 To MAX_MAP_NPCS
            buffer.WriteInt32(MyMap.NPC(i))
        Next

        For x = 0 To MyMap.MaxX
            For y = 0 To MyMap.MaxY
                buffer.WriteInt32(MyMap.Tile(x, y).Data1)
                buffer.WriteInt32(MyMap.Tile(x, y).Data2)
                buffer.WriteInt32(MyMap.Tile(x, y).Data3)
                buffer.WriteInt32(MyMap.Tile(x, y).Data1_2)
                buffer.WriteInt32(MyMap.Tile(x, y).Data2_2)
                buffer.WriteInt32(MyMap.Tile(x, y).Data3_2)
                buffer.WriteInt32(MyMap.Tile(x, y).DirBlock)
                For i = 1 To LayerType.Count - 1
                    buffer.WriteInt32(MyMap.Tile(x, y).Layer(i).Tileset)
                    buffer.WriteInt32(MyMap.Tile(x, y).Layer(i).X)
                    buffer.WriteInt32(MyMap.Tile(x, y).Layer(i).Y)
                    buffer.WriteInt32(MyMap.Tile(x, y).Layer(i).AutoTile)
                Next
                buffer.WriteInt32(MyMap.Tile(x, y).Type)
                buffer.WriteInt32(MyMap.Tile(x, y).Type2)
            Next
        Next

        buffer.WriteInt32(MyMap.EventCount)

        If MyMap.EventCount > 0 Then
            For i = 0 To MyMap.EventCount
                With MyMap.Event(i)
                    If .Name Is Nothing Then .Name = ""
                    buffer.WriteString((.Name))
                    buffer.WriteByte(.Globals)
                    buffer.WriteInt32(.X)
                    buffer.WriteInt32(.Y)
                    buffer.WriteInt32(.PageCount)
                End With
                If MyMap.Event(i).PageCount > 0 Then
                    For x = 0 To MyMap.Event(i).PageCount
                        With MyMap.Event(i).Pages(x)
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
                            buffer.WriteInt32(MyMap.Event(i).Pages(x).MoveRouteCount)
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

                        If MyMap.Event(i).Pages(x).CommandListCount > 0 Then
                            For y = 0 To MyMap.Event(i).Pages(x).CommandListCount
                                buffer.WriteInt32(MyMap.Event(i).Pages(x).CommandList(y).CommandCount)
                                buffer.WriteInt32(MyMap.Event(i).Pages(x).CommandList(y).ParentList)
                                If MyMap.Event(i).Pages(x).CommandList(y).CommandCount > 0 Then
                                    For z = 0 To MyMap.Event(i).Pages(x).CommandList(y).CommandCount
                                        With MyMap.Event(i).Pages(x).CommandList(y).Commands(z)
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

        data = buffer.ToArray

        buffer = New ByteStream(4)
        buffer.WriteInt32(ClientPackets.CSaveMap)
        buffer.WriteBlock(Compression.CompressBytes(data))

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendMapRespawn()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CMapRespawn)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub ClearMapEvents()
        For i = 0 To MyMap.EventCount
            Dim batchList = Client.Batches.ToList() ' Snapshot of the queue

            SyncLock Client.BatchLock
                For Each batch In batchList
                    Dim existingCommand As GameClient.RenderCommand
                    ' Search for an existing command and update its properties
                    Select Case MapEvents(i).GraphicType
                        Case 0
                            Exit Sub
                        Case 1
                            existingCommand = batch.Commands.FirstOrDefault(Function(cmd) cmd.Path = System.IO.Path.Combine(Core.Path.Characters, MapEvents(i).Graphic))
                        Case 2
                            existingCommand = batch.Commands.FirstOrDefault(Function(cmd) cmd.Path = System.IO.Path.Combine(Core.Path.Tilesets, MapEvents(i).Graphic))
                    End Select
                    
                    if existingCommand.dRect.X = MapEvents(i).X And existingCommand.dRect.Y = Type.MapEvents(i).Y Then
                        batch.Commands.Remove(existingCommand)
                        Exit For
                    End If
                Next
            End SyncLock
        Next

        ReDim MapEvents(MyMap.EventCount)
        CurrentEvents = 0

    End Sub

#End Region

End Module