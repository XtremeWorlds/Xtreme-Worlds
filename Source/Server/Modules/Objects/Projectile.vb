
Imports Mirage.Sharp.Asfw
Imports Core
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Friend Module Projectile
#Region "Type"

    Public Structure ProjectileRec
        Dim Name As String
        Dim Sprite As Integer
        Dim Range As Byte
        Dim Speed As Integer
        Dim Damage As Integer
    End Structure

    Public Structure MapProjectileRec
        Dim ProjectileNum As Integer
        Dim Owner As Integer
        Dim OwnerType As Byte
        Dim X As Integer
        Dim Y As Integer
        Dim Dir As Byte
        Dim Timer As Integer
    End Structure

#End Region

#Region "Database"
    Sub SaveProjectile(projectileNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Projectile(projectileNum)).ToString()

        If RowExists(projectileNum, "projectile")
            UpdateRow(projectileNum, json, "projectile", "data")
        Else
            InsertRow(projectileNum, json, "projectile")
        End If
    End Sub

    Sub LoadProjectiles()
        Dim i As Integer

        For i = 1 To MAX_PROJECTILES
            LoadProjectile(i)
        Next
    End Sub

    Sub LoadProjectile(projectileNum As Integer)
        Dim data As JObject

        data = SelectRow(projectileNum, "projectile", "data")

        If data Is Nothing Then
            ClearProjectile(projectileNum)
            Exit Sub
        End If

        Dim projectileData = JObject.FromObject(data).toObject(Of ProjectileStruct)()
        Type.Projectile(projectileNum) = projectileData
    End Sub

    Sub ClearMapProjectile()
        Dim x As Integer
        Dim y As Integer

        ReDim MapProjectile(MAX_MAPS, MAX_PROJECTILES)

        For x = 1 To MAX_MAPS
            For y = 1 To MAX_PROJECTILES
                ClearMapProjectile(x, y)
            Next
        Next

    End Sub

    Sub ClearMapProjectile(MapNum As Integer, index As Integer)

        MapProjectile(MapNum, index).ProjectileNum = 0
        MapProjectile(MapNum, index).Owner = 0
        MapProjectile(MapNum, index).OwnerType = 0
        MapProjectile(MapNum, index).X = 0
        MapProjectile(MapNum, index).Y = 0
        MapProjectile(MapNum, index).Dir = 0
        MapProjectile(MapNum, index).Timer = 0

    End Sub

    Sub ClearProjectile(index As Integer)

        Type.Projectile(index).Name = ""
        Type.Projectile(index).Sprite = 0
        Type.Projectile(index).Range = 0
        Type.Projectile(index).Speed = 0
        Type.Projectile(index).Damage = 0

    End Sub

    Sub ClearProjectile()
        Dim i As Integer

        ReDim Type.Projectile(MAX_PROJECTILES)

        For i = 1 To MAX_PROJECTILES
            ClearProjectile(i)
        Next

    End Sub

#End Region

#Region "Incoming"

    Sub HandleRequestEditProjectile(index As Integer, ByRef data() As Byte)
        Dim buffer As New ByteStream(4)

        ' Prevent hacking
        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub
        If TempPlayer(index).Editor > -1 Then  Exit Sub

        Dim user As String

        user = IsEditorLocked(index, EditorType.Projectile)

        If user <> "" Then 
            PlayerMsg(index, "The game editor is locked and being used by " + user + ".", ColorType.BrightRed)
            Exit Sub
        End If

        TempPlayer(index).Editor = EditorType.Projectile

        buffer.WriteInt32(ServerPackets.SProjectileEditor)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub HandleSaveProjectile(index As Integer, ByRef data() As Byte)
        Dim ProjectileNum As Integer
        Dim buffer As New ByteStream(data)

        If GetPlayerAccess(index) < AccessType.Developer Then Exit Sub

        ProjectileNum = buffer.ReadInt32

        ' Prevent hacking
        If Projectilenum <= 0 Or ProjectileNum > MAX_PROJECTILES Then
            Exit Sub
        End If

        Type.Projectile(ProjectileNum).Name = buffer.ReadString
        Type.Projectile(ProjectileNum).Sprite = buffer.ReadInt32
        Type.Projectile(ProjectileNum).Range = buffer.ReadInt32
        Type.Projectile(ProjectileNum).Speed = buffer.ReadInt32
        Type.Projectile(ProjectileNum).Damage = buffer.ReadInt32

        ' Save it
        SendUpdateProjectileToAll(ProjectileNum)
        SaveProjectile(ProjectileNum)
        Addlog(GetPlayerLogin(index) & " saved Projectile #" & ProjectileNum & ".", ADMIN_LOG)
        buffer.Dispose()

    End Sub

    Sub HandleRequestProjectile(index As Integer, ByRef data() As Byte)
        SendProjectiles(index)
    End Sub

    Sub HandleClearProjectile(index As Integer, ByRef data() As Byte)
        Dim ProjectileNum As Integer
        Dim Targetindex As Integer
        Dim TargetType As TargetType
        Dim TargetZone As Integer
        Dim mapNum As Integer
        Dim Damage As Integer
        Dim armor As Integer
        Dim npcnum As Integer
        Dim buffer As New ByteStream(data)
        ProjectileNum = buffer.ReadInt32
        Targetindex = buffer.ReadInt32
        TargetType = buffer.ReadInt32
        TargetZone = buffer.ReadInt32
        buffer.Dispose()

        mapNum = GetPlayerMap(index)

        Select Case MapProjectile(MapNum, ProjectileNum).OwnerType
            Case TargetType.Player
               If Type.MapProjectile(MapNum, ProjectileNum).Owner = index Then
                    Select Case TargetType
                        Case TargetType.Player

                            If IsPlaying(Targetindex) Then
                                If Targetindex <> index Then
                                    If CanPlayerAttackPlayer(index, Targetindex, True) = 1 Then

                                        ' Get the damage we can do
                                        Damage = GetPlayerDamage(index) + Type.Projectile(Type.MapProjectile(MapNum, ProjectileNum).ProjectileNum).Damage

                                        ' if the npc blocks, take away the block amount
                                        armor = CanPlayerBlockHit(Targetindex)
                                        Damage = Damage - armor

                                        ' randomise for up to 10% lower than max hit
                                        Damage = Random.NextDouble(1, Damage)

                                        AttackPlayer(index, Targetindex, Damage)
                                    End If
                                End If
                            End If

                        Case TargetType.NPC
                            npcnum = MapNPC(MapNum).NPC(Targetindex).Num
                            If CanPlayerAttackNpc(index, Targetindex, True) = 1 Then
                                ' Get the damage we can do
                                Damage = GetPlayerDamage(index) + Type.Projectile(Type.MapProjectile(MapNum, ProjectileNum).ProjectileNum).Damage

                                ' if the npc blocks, take away the block amount
                                armor = 0
                                Damage = Damage - armor

                                ' randomise from 1 to max hit
                                Damage = Random.NextDouble(1, Damage)

                                PlayerAttackNpc(index, Targetindex, Damage)
                            End If
                    End Select
                End If

        End Select

        ClearMapProjectile(MapNum, ProjectileNum)

    End Sub

#End Region

#Region "Outgoing"

    Sub SendUpdateProjectileToAll(ProjectileNum As Integer)
        Dim buffer As ByteStream

        buffer = New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateProjectile)
        buffer.WriteInt32(ProjectileNum)
        buffer.WriteString((Type.Projectile(ProjectileNum).Name))
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Sprite)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Range)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Speed)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Damage)

        SendDataToAll(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendUpdateProjectileTo(index As Integer, ProjectileNum As Integer)
        Dim buffer As ByteStream

        buffer = New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SUpdateProjectile)
        buffer.WriteInt32(ProjectileNum)
        buffer.WriteString((Type.Projectile(ProjectileNum).Name))
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Sprite)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Range)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Speed)
        buffer.WriteInt32(Type.Projectile(ProjectileNum).Damage)

        Socket.SendDataTo(index, buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendProjectiles(index As Integer)
        Dim i As Integer

        For i = 1 To MAX_PROJECTILES
            If Len(Type.Projectile(i).Name) > 0 Then
                Call SendUpdateProjectileTo(index, i)
            End If
        Next

    End Sub

    Sub SendProjectileToMap(MapNum As Integer, ProjectileNum As Integer)
        Dim buffer As ByteStream

        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SMapProjectile)

        With MapProjectile(MapNum, ProjectileNum)
            buffer.WriteInt32(ProjectileNum)
            buffer.WriteInt32(.ProjectileNum)
            buffer.WriteInt32(.Owner)
            buffer.WriteInt32(.OwnerType)
            buffer.WriteInt32(.Dir)
            buffer.WriteInt32(.X)
            buffer.WriteInt32(.Y)
        End With

        SendDataToMap(MapNum, buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

#End Region

#Region "Functions"

    Friend Sub PlayerFireProjectile(index As Integer, Optional IsSkill As Integer = 0)
        Dim ProjectileSlot As Integer
        Dim ProjectileNum As Integer
        Dim mapNum As Integer
        Dim i As Integer

        mapNum = GetPlayerMap(index)

        'Find a free projectile
        For i = 1 To MAX_PROJECTILES
           If Type.MapProjectile(MapNum, i).ProjectileNum = 0 Then ' Free Projectile
                ProjectileSlot = i
                Exit For
            End If
        Next

        'Check for no projectile, if so just overwrite the first slot
        If ProjectileSlot = 0 Then ProjectileSlot = 1

        'Check for skill, if so then load data acordingly
        If IsSkill > 0 Then
            ProjectileNum = Type.Skill(IsSkill).Projectile
        Else
            ProjectileNum = Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Projectile
        End If

        If ProjectileNum = 0 Then Exit Sub

        With MapProjectile(MapNum, ProjectileSlot)
            .ProjectileNum = ProjectileNum
            .Owner = index
            .OwnerType = TargetType.Player
            .Dir = GetPlayerDir(index)
            .X = GetPlayerX(index)
            .Y = GetPlayerY(index)
            .Timer = GetTimeMs() + 60000
        End With

        SendProjectileToMap(MapNum, ProjectileSlot)

    End Sub

    Friend Function Engine_GetAngle(CenterX As Integer, CenterY As Integer, targetX As Integer, targetY As Integer) As Single
        '************************************************************
        'Gets the angle between two points in a 2d plane
        '************************************************************
        Dim SideA As Single
        Dim SideC As Single

        On Error GoTo ErrOut

        'Check for horizontal lines (90 or 270 degrees)
        If CenterY = targetY Then
            'Check for going right (90 degrees)
            If CenterX < targetX Then
                Engine_GetAngle = 90
                'Check for going left (270 degrees)
            Else
                Engine_GetAngle = 270
            End If

            'Exit the function
            Exit Function
        End If

        'Check for horizontal lines (360 or 180 degrees)
        If CenterX = targetX Then
            'Check for going up (360 degrees)
            If CenterY > targetY Then
                Engine_GetAngle = 360

                'Check for going down (180 degrees)
            Else
                Engine_GetAngle = 180
            End If

            'Exit the function
            Exit Function
        End If

        'Calculate Side C
        SideC = Math.Sqrt(Math.Abs(targetX - CenterX) ^ 2 + Math.Abs(targetY - CenterY) ^ 2)

        'Side B = CenterY

        'Calculate Side A
        SideA = Math.Sqrt(Math.Abs(targetX - CenterX) ^ 2 + targetY ^ 2)

        'Calculate the angle
        Engine_GetAngle = (SideA ^ 2 - CenterY ^ 2 - SideC ^ 2) / (CenterY * SideC * -2)
        Engine_GetAngle = (Math.Atan(-Engine_GetAngle / Math.Sqrt(-Engine_GetAngle * Engine_GetAngle + 1)) + 1.5708) * 57.29583

        'If the angle is >180, subtract from 360
        If targetX < CenterX Then Engine_GetAngle = 360 - Engine_GetAngle

        'Exit function
        Exit Function

        'Check for error
ErrOut:

        'Return a 0 saying there was an error
        Engine_GetAngle = 0

        Exit Function
    End Function

#End Region

End Module