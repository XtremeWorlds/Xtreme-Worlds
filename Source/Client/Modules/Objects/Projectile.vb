Imports System.IO
Imports Mirage.Sharp.Asfw
Imports Core
Imports SFML.Graphics
Imports SFML.System

Friend Module Projectile

#Region "Defines"
    Friend NumProjectiles As Integer
    Friend InitProjectileEditor As Boolean
    Friend Const EditorProjectile As Byte = 10
    Friend ProjectileChanged(MAX_PROJECTILES) As Boolean

#End Region

#Region "Sending"
    Sub SendRequestEditProjectile()
        Dim buffer As ByteStream

        buffer = New ByteStream(4)
        buffer.WriteInt32(Packets.ClientPackets.CRequestEditProjectiles)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendSaveProjectile(ProjectileNum As Integer)
        Dim buffer As ByteStream

        buffer = New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSaveProjectile)
        buffer.WriteInt32(ProjectileNum)

        buffer.WriteString(Type.Projectile(ProjectileNum).Name)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Sprite)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Range)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Speed)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Damage)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendRequestProjectile()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestProjectiles)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendClearProjectile(projectileNum As Integer, collisionindex As Integer, collisionType As Byte, collisionZone As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CClearProjectile)
        buffer.WriteInt32(projectileNum)
        buffer.WriteInt32(collisionindex)
        buffer.WriteInt32(collisionType)
        buffer.WriteInt32(collisionZone)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

#End Region

#Region "Recieving"

    Friend Sub HandleUpdateProjectile(ByRef data() As Byte)
        Dim projectileNum As Integer
        Dim buffer As New ByteStream(data)
        projectileNum = buffer.ReadInt32

        Type.Projectile(projectileNum).Name = buffer.ReadString
        Type.Projectile(projectileNum).Sprite = buffer.ReadInt32
        Type.Projectile(projectileNum).Range = buffer.ReadInt32
        Type.Projectile(projectileNum).Speed = buffer.ReadInt32
        Type.Projectile(projectileNum).Damage = buffer.ReadInt32

        buffer.Dispose()

    End Sub

    Friend Sub HandleMapProjectile(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)
        i = buffer.ReadInt32

        With MapProjectile(Type.Player(MyIndex).Map, i)
            .ProjectileNum = buffer.ReadInt32
            .Owner = buffer.ReadInt32
            .OwnerType = buffer.ReadInt32
            .Dir = buffer.ReadInt32
            .X = buffer.ReadInt32
            .Y = buffer.ReadInt32
            .Range = 0
            .Timer = GetTickCount() + 60000
        End With

        buffer.Dispose()

    End Sub

#End Region

#Region "Database"

    Sub ClearProjectile()
        Dim i As Integer

       For i = 1 To MAX_PROJECTILES
            ClearProjectile(i)
        Next

    End Sub

    Sub ClearProjectile(index As Integer)

        Type.Projectile(index).Name = ""
        Type.Projectile(index).Sprite = 0
        Type.Projectile(index).Range = 0
        Type.Projectile(index).Speed = 0
        Type.Projectile(index).Damage = 0

    End Sub

    Sub ClearMapProjectile(projectileNum As Integer)

        MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum = 0
        MapProjectile(Type.Player(MyIndex).Map, projectileNum).Owner = 0
        MapProjectile(Type.Player(MyIndex).Map, projectileNum).OwnerType = 0
        MapProjectile(Type.Player(MyIndex).Map, projectileNum).X = 0
        MapProjectile(Type.Player(MyIndex).Map, projectileNum).Y = 0
        MapProjectile(Type.Player(MyIndex).Map, projectileNum).Dir = 0
        MapProjectile(Type.Player(MyIndex).Map, projectileNum).Timer = 0

    End Sub

#End Region

#Region "Drawing"
    Friend Sub DrawProjectile(projectileNum As Integer)
        Dim rec As RectStruct
        Dim canClearProjectile As Boolean
        Dim collisionindex As Integer
        Dim collisionType As Byte
        Dim collisionZone As Integer
        Dim xOffset As Integer, yOffset As Integer
        Dim x As Integer, y As Integer
        Dim i As Integer
        Dim sprite As Integer

        ' check to see if it's time to move the Projectile
        If GetTickCount() > MapProjectile(Type.Player(MyIndex).Map, projectileNum).TravelTime Then
            Select Case MapProjectile(Type.Player(MyIndex).Map, projectileNum).Dir
                Case DirectionType.Up
                    MapProjectile(Type.Player(MyIndex).Map, projectileNum).Y = MapProjectile(Type.Player(MyIndex).Map, projectileNum).Y - 1
                Case DirectionType.Down
                    MapProjectile(Type.Player(MyIndex).Map, projectileNum).Y = MapProjectile(Type.Player(MyIndex).Map, projectileNum).Y + 1
                Case DirectionType.Left
                    MapProjectile(Type.Player(MyIndex).Map, projectileNum).X = MapProjectile(Type.Player(MyIndex).Map, projectileNum).X - 1
                Case DirectionType.Right
                    MapProjectile(Type.Player(MyIndex).Map, projectileNum).X = MapProjectile(Type.Player(MyIndex).Map, projectileNum).X + 1
            End Select
            MapProjectile(Type.Player(MyIndex).Map, projectileNum).TravelTime = GetTickCount() + Type.Projectile(Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum).Speed
            MapProjectile(Type.Player(MyIndex).Map, projectileNum).Range = MapProjectile(Type.Player(MyIndex).Map, projectileNum).Range + 1
        End If

        x = MapProjectile(Type.Player(MyIndex).Map, projectileNum).X
        y = MapProjectile(Type.Player(MyIndex).Map, projectileNum).Y

        'Check if its been going for over 1 minute, if so clear.
       If Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).Timer < GetTickCount() Then canClearProjectile = True

        If x > MyMap.MaxX Or x < 0 Then canClearProjectile = True
        If y > MyMap.MaxY Or y < 0 Then canClearProjectile = True

        'Check for blocked wall collision
        If canClearProjectile = False Then 'Add a check to prevent crashing
            If MyMap.Tile(x, y).Type = TileType.Blocked Or MyMap.Tile(x, y).Type2 = TileType.Blocked Then
                canClearProjectile = True
            End If
        End If

        'Check for npc collision
       For i = 1 To MAX_MAP_NPCS
            If MyMapNPC(i).X = x And MyMapNPC(i).Y = y Then
                canClearProjectile = True
                collisionindex = i
                collisionType = TargetType.NPC
                collisionZone = -1
                Exit For
            End If
        Next

        'Check for player collision
       For i = 1 To MAX_PLAYERS
            If IsPlaying(i) And GetPlayerMap(i) = GetPlayerMap(MyIndex) Then
                If GetPlayerX(i) = x And GetPlayerY(i) = y Then
                    canClearProjectile = True
                    collisionindex = i
                    collisionType = TargetType.Player
                    collisionZone = -1
                   If Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).OwnerType = TargetType.Player Then
                       If Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).Owner = i Then canClearProjectile = False ' Reset if its the owner of projectile
                    End If
                    Exit For
                End If

            End If
        Next

        'Check if it has hit its maximum range
       If Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).Range >= Type.Projectile(Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum).Range + 1 Then canClearProjectile = True

        'Clear the projectile if possible
        If canClearProjectile = True Then
            'Only send the clear to the server if you're the projectile caster or the one hit (only if owner is not a player)
            If (Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).OwnerType = TargetType.Player And MapProjectile(Type.Player(MyIndex).Map, projectileNum).Owner = MyIndex) Then
                SendClearProjectile(projectileNum, collisionindex, collisionType, collisionZone)
            End If

            ClearMapProjectile(projectileNum)
            Exit Sub
        End If

        sprite = Type.Projectile(Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum).Sprite
        If sprite < 1 Or sprite > NumProjectiles Then Exit Sub

        ' src rect
        With rec
            .Top = 0
            .Bottom = Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Projectiles, sprite)).Height
            .Left = MapProjectile(Type.Player(MyIndex).Map, projectileNum).Dir * PicX
            .Right = .Left + PicX
        End With

        'Find the offset
        Select Case MapProjectile(Type.Player(MyIndex).Map, projectileNum).Dir
            Case DirectionType.Up
                yOffset = ((Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).TravelTime - GetTickCount()) / Type.Projectile(Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum).Speed) * PicY
            Case DirectionType.Down
                yOffset = -((Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).TravelTime - GetTickCount()) / Type.Projectile(Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum).Speed) * PicY
            Case DirectionType.Left
                xOffset = ((Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).TravelTime - GetTickCount()) / Type.Projectile(Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum).Speed) * PicX
            Case DirectionType.Right
                xOffset = -((Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).TravelTime - GetTickCount()) / Type.Projectile(Type.MapProjectile(Type.Player(MyIndex).Map, projectileNum).ProjectileNum).Speed) * PicX
        End Select

        ' Convert coordinates
        x = ConvertMapX(x * PicX)
        y = ConvertMapY(y * PicY)

        ' Render texture
        Client.EnqueueTexture(System.IO.Path.Combine(Core.Path.Projectiles & sprite), x, y, rec.Left, rec.Top, 32, 32)

    End Sub
    
#End Region

End Module