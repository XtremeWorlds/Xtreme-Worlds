Imports System.Drawing
Imports Core
Imports Mirage.Sharp.Asfw

Module Pet

#Region "Globals etc"

    Friend Const PetbarTop As Byte = 2
    Friend Const PetbarLeft As Byte = 2
    Friend Const PetbarOffsetX As Byte = 4
    Friend Const MaxPetbar As Byte = 7
    Friend Const PetHpBarWidth As Integer = 129
    Friend Const PetMpBarWidth As Integer = 129

    Friend PetSkillBuffer As Integer
    Friend PetSkillBufferTimer As Integer
    Friend PetSkillCd() As Integer

    Friend Const PetBehaviourFollow As Byte = 0 'The pet will attack all npcs around

    Friend Const PetBehaviourGoto As Byte = 1 'If attacked, the pet will fight back
    Friend Const PetAttackBehaviourAttackonsight As Byte = 2 'The pet will attack all npcs around
    Friend Const PetAttackBehaviourGuard As Byte = 3 'If attacked, the pet will fight back
    Friend Const PetAttackBehaviourDonothing As Byte = 4 'The pet will not attack even if attacked

#End Region

#Region "Database"

    Sub ClearPet(index As Integer)
        Type.Pet(index) = Nothing
        Type.Pet(index).Name = ""

        ReDim Type.Pet(index).Stat(StatType.Count - 1)
        ReDim Type.Pet(index).Skill(4)
        Pet_Loaded(index) = False
    End Sub

    Sub ClearPets()
        Dim i As Integer

        ReDim Type.Pet(MAX_PETS)
        ReDim PetSkillCd(4)

        For i = 1 To MAX_PETS
            ClearPet(i)
        Next

    End Sub

    Sub StreamPet(petNum As Integer)
        If petNum > 0 And Type.Pet(petNum).Name = "" Or Pet_Loaded(petNum) = False Then
            Pet_Loaded(petNum) = True
            SendRequestPet(petNum)
        End If
    End Sub

#End Region

#Region "Outgoing Packets"
    Sub SendRequestPet(petNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestPet)

        buffer.WriteInt32(petNum)
        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()
    End Sub

    Friend Sub SendPetBehaviour(index As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSetBehaviour)

        buffer.WriteInt32(index)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendTrainPetStat(statNum As Byte)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPetUseStatPoint)

        buffer.WriteInt32(statNum)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendRequestPets()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestPets)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendUsePetSkill(skill As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPetSkill)
        buffer.WriteInt32(skill)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

        PetSkillBuffer = skill
        PetSkillBufferTimer = GetTickCount()
    End Sub

    Sub SendSummonPet()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CSummonPet)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Sub SendReleasePet()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CReleasePet)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

    Friend Sub SendRequestEditPet()
        Dim buffer As ByteStream
        buffer = New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CRequestEditPet)

        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Friend Sub SendSavePet(petNum As Integer)
        Dim buffer As ByteStream
        Dim i As Integer

        buffer = New ByteStream(4)
        buffer.WriteInt32(ClientPackets.CSavePet)
        buffer.WriteInt32(petNum)

        With Type.Pet(petNum)
            buffer.WriteInt32(.Num)
            buffer.WriteString((.Name))
            buffer.WriteInt32(.Sprite)
            buffer.WriteInt32(.Range)
            buffer.WriteInt32(.Level)
            buffer.WriteInt32(.MaxLevel)
            buffer.WriteInt32(.ExpGain)
            buffer.WriteInt32(.LevelPnts)
            buffer.WriteInt32(.StatType)
            buffer.WriteInt32(.LevelingType)

            For i = 1 To StatType.Count - 1
                buffer.WriteInt32(.Stat(i))
            Next

            For i = 1 To 4
                buffer.WriteInt32(.Skill(i))
            Next

            buffer.WriteInt32(.Evolvable)
            buffer.WriteInt32(.EvolveLevel)
            buffer.WriteInt32(.EvolveNum)
        End With

        Socket.SendData(buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub
#End Region

#Region "Incoming Packets"

    Friend Sub Packet_UpdatePlayerPet(ByRef data() As Byte)
        Dim n As Integer, i As Integer
        Dim buffer As New ByteStream(data)
        n = buffer.ReadInt32

        'pet
        Type.Player(n).Pet.Num = buffer.ReadInt32
        Type.Player(n).Pet.Health = buffer.ReadInt32
        Type.Player(n).Pet.Mana = buffer.ReadInt32
        Type.Player(n).Pet.Level = buffer.ReadInt32

        For i = 1 To StatType.Count - 1
            Type.Player(n).Pet.Stat(i) = buffer.ReadInt32
        Next

        For i = 1 To 4
            Type.Player(n).Pet.Skill(i) = buffer.ReadInt32
        Next

        Type.Player(n).Pet.X = buffer.ReadInt32
        Type.Player(n).Pet.Y = buffer.ReadInt32
        Type.Player(n).Pet.Dir = buffer.ReadInt32

        Type.Player(n).Pet.MaxHp = buffer.ReadInt32
        Type.Player(n).Pet.MaxMp = buffer.ReadInt32

        Type.Player(n).Pet.Alive = buffer.ReadInt32

        Type.Player(n).Pet.AttackBehaviour = buffer.ReadInt32
        Type.Player(n).Pet.Points = buffer.ReadInt32
        Type.Player(n).Pet.Exp = buffer.ReadInt32
        Type.Player(n).Pet.Tnl = buffer.ReadInt32

        buffer.Dispose()
    End Sub

    Friend Sub Packet_UpdatePet(ByRef data() As Byte)
        Dim n As Integer, i As Integer
        Dim buffer As New ByteStream(data)
        n = buffer.ReadInt32

        With Type.Pet(n)
            .Num = buffer.ReadInt32
            .Name = buffer.ReadString
            .Sprite = buffer.ReadInt32
            .Range = buffer.ReadInt32
            .Level = buffer.ReadInt32
            .MaxLevel = buffer.ReadInt32
            .ExpGain = buffer.ReadInt32
            .LevelPnts = buffer.ReadInt32
            .StatType = buffer.ReadInt32
            .LevelingType = buffer.ReadInt32

            For i = 1 To StatType.Count - 1
                .Stat(i) = buffer.ReadInt32
            Next

            For i = 1 To 4
                .Skill(i) = buffer.ReadInt32
            Next

            .Evolvable = buffer.ReadInt32
            .EvolveLevel = buffer.ReadInt32
            .EvolveNum = buffer.ReadInt32
        End With

        buffer.Dispose()

    End Sub

    Friend Sub Packet_PetMove(ByRef data() As Byte)
        Dim i As Integer, x As Integer, y As Integer
        Dim dir As Integer, movement As Integer
        Dim buffer As New ByteStream(data)
        i = buffer.ReadInt32
        x = buffer.ReadInt32
        y = buffer.ReadInt32
        dir = buffer.ReadInt32
        movement = buffer.ReadInt32

        With Type.Player(i).Pet
            .X = x
            .Y = y
            .Dir = dir
            .XOffset = 0
            .YOffset = 0
            .Moving = movement

            Select Case .Dir
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

        buffer.Dispose()
    End Sub

    Friend Sub Packet_PetDir(ByRef data() As Byte)
        Dim i As Integer
        Dim dir As Integer
        Dim buffer As New ByteStream(data)
        i = buffer.ReadInt32
        dir = buffer.ReadInt32

        Type.Player(i).Pet.Dir = dir

        buffer.Dispose()
    End Sub

    Friend Sub Packet_PetVital(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)
        i = buffer.ReadInt32

        If buffer.ReadInt32 = 1 Then
            Type.Player(i).Pet.MaxHp = buffer.ReadInt32
            Type.Player(i).Pet.Health = buffer.ReadInt32
        Else
            Type.Player(i).Pet.MaxMp = buffer.ReadInt32
            Type.Player(i).Pet.Mana = buffer.ReadInt32
        End If

        buffer.Dispose()

    End Sub

    Friend Sub Packet_ClearPetSkillBuffer(ByRef data() As Byte)
        PetSkillBuffer = 0
        PetSkillBufferTimer = 0
    End Sub

    Friend Sub Packet_PetAttack(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)
        i = buffer.ReadInt32

        ' Set pet to attacking
        Type.Player(i).Pet.Attacking = 1
        Type.Player(i).Pet.AttackTimer = GetTickCount()

        buffer.Dispose()
    End Sub

    Friend Sub Packet_PetXY(ByRef data() As Byte)
        Dim i As Integer
        Dim buffer As New ByteStream(data)

        Type.Player(i).Pet.X = buffer.ReadInt32
        Type.Player(i).Pet.Y = buffer.ReadInt32

        buffer.Dispose()
    End Sub

    Friend Sub Packet_PetExperience(ByRef data() As Byte)
        Dim buffer As New ByteStream(data)

        Type.Player(MyIndex).Pet.Exp = buffer.ReadInt32
        Type.Player(MyIndex).Pet.Tnl = buffer.ReadInt32

        buffer.Dispose()
    End Sub

#End Region

#Region "Movement"

    Sub ProcessPetMovement(index As Integer)

        ' Check if pet is walking, and if so process moving them over

        If Type.Player(index).Pet.Moving = MovementType.Walking Then

            Select Case Type.Player(index).Pet.Dir
                Case DirectionType.Up
                    Type.Player(index).Pet.YOffset = Type.Player(index).Pet.YOffset - ((ElapsedTime / 1000) * (WalkSpeed * SizeY))
                    If Type.Player(index).Pet.YOffset < 0 Then Type.Player(index).Pet.YOffset = 0

                Case DirectionType.Down
                    Type.Player(index).Pet.YOffset = Type.Player(index).Pet.YOffset + ((ElapsedTime / 1000) * (WalkSpeed * SizeY))
                    If Type.Player(index).Pet.YOffset > 0 Then Type.Player(index).Pet.YOffset = 0

                Case DirectionType.Left
                    Type.Player(index).Pet.XOffset = Type.Player(index).Pet.XOffset - ((ElapsedTime / 1000) * (WalkSpeed * SizeX))
                    If Type.Player(index).Pet.XOffset < 0 Then Type.Player(index).Pet.XOffset = 0

                Case DirectionType.Right
                    Type.Player(index).Pet.XOffset = Type.Player(index).Pet.XOffset + ((ElapsedTime / 1000) * (WalkSpeed * SizeX))
                    If Type.Player(index).Pet.XOffset > 0 Then Type.Player(index).Pet.XOffset = 0

            End Select

            ' Check if completed walking over to the next tile
            If Type.Player(index).Pet.Moving > 0 Then
                If Type.Player(index).Pet.Dir = DirectionType.Right Or Type.Player(index).Pet.Dir = DirectionType.Down Then
                    If (Type.Player(index).Pet.XOffset >= 0) And (Type.Player(index).Pet.YOffset >= 0) Then
                        Type.Player(index).Pet.Moving = 0
                        If Type.Player(index).Pet.Steps = 1 Then
                            Type.Player(index).Pet.Steps = 2
                        Else
                            Type.Player(index).Pet.Steps = 1
                        End If
                    End If
                Else
                    If (Type.Player(index).Pet.XOffset <= 0) And (Type.Player(index).Pet.YOffset <= 0) Then
                        Type.Player(index).Pet.Moving = 0
                        If Type.Player(index).Pet.Steps = 1 Then
                            Type.Player(index).Pet.Steps = 2
                        Else
                            Type.Player(index).Pet.Steps = 1
                        End If
                    End If
                End If
            End If
        End If

    End Sub

    Friend Sub PetMove(x As Integer, y As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ClientPackets.CPetMove)

        buffer.WriteInt32(x)
        buffer.WriteInt32(y)

        Socket.SendData(buffer.Data, buffer.Head)
        buffer.Dispose()

    End Sub

#End Region

#Region "Drawing"

    Friend Sub DrawPet(index As Integer)
        Dim anim As Byte, x As Integer, y As Integer
        Dim spriteNum As Integer, spriteleft As Integer
        Dim rect As Microsoft.Xna.Framework.Rectangle
        Dim attackspeed As Integer

        StreamPet(Type.Player(index).Pet.Num)

        spriteNum = Type.Pet(Type.Player(index).Pet.Num).Sprite

        If spriteNum < 1 Or spriteNum > NumCharacters Then Exit Sub

        attackspeed = 1000

        ' Reset frame
        If Type.Player(index).Pet.Steps = 3 Then
            anim = 0
        ElseIf Type.Player(index).Pet.Steps = 1 Then
            anim = 2
        ElseIf Type.Player(index).Pet.Steps = 2 Then
            anim = 3
        End If

        ' Check for attacking animation
        If Type.Player(index).Pet.AttackTimer + (attackspeed / 2) > GetTickCount() Then
            If Type.Player(index).Pet.Attacking = 1 Then
                anim = 3
            End If
        Else
            ' If not attacking, walk normally
            Select Case Type.Player(index).Pet.Dir
                Case DirectionType.Up
                    If (Type.Player(index).Pet.YOffset > 8) Then anim = Type.Player(index).Pet.Steps
                Case DirectionType.Down
                    If (Type.Player(index).Pet.YOffset < -8) Then anim = Type.Player(index).Pet.Steps
                Case DirectionType.Left
                    If (Type.Player(index).Pet.XOffset > 8) Then anim = Type.Player(index).Pet.Steps
                Case DirectionType.Right
                    If (Type.Player(index).Pet.XOffset < -8) Then anim = Type.Player(index).Pet.Steps
            End Select
        End If

        ' Check to see if we want to stop making him attack
        With Type.Player(index).Pet
            If .AttackTimer + attackspeed < GetTickCount() Then
                .Attacking = 0
                .AttackTimer = 0
            End If
        End With

        ' Set the left
        Select Case Type.Player(index).Pet.Dir
            Case DirectionType.Up
                spriteleft = 3
            Case DirectionType.Right
                spriteleft = 2
            Case DirectionType.Down
                spriteleft = 0
            Case DirectionType.Left
                spriteleft = 1
        End Select

        rect = New Microsoft.Xna.Framework.Rectangle(
            anim * (Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spriteNum)).Width \ 4),
            spriteleft * (Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spriteNum)).Height \ 4),
             Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spriteNum)).Width \ 4,
             Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spriteNum)).Height \ 4
        )

        ' Calculate the X
        x = Type.Player(index).Pet.X * PicX + Type.Player(index).Pet.XOffset - ((Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spriteNum)).Width / 4 - 32) / 2)

        ' Is the player's height more than 32..?
        If (Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spriteNum)).Height / 4) > 32 Then
            ' Create a 32 pixel offset for larger sprites
            y = Type.Player(index).Pet.Y * PicY + Type.Player(index).Pet.YOffset - ((Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, spriteNum)).Width / 4) - 32)
        Else
            ' Proceed as normal
            y = Type.Player(index).Pet.Y * PicY + Type.Player(index).Pet.YOffset
        End If

        ' render the actual sprite
        Client.DrawCharacterSprite(spriteNum, x, y, rect)

    End Sub

    Friend Sub DrawPlayerPetName(index As Integer)
        Dim textX As Integer
        Dim textY As Integer
        Dim color As Microsoft.Xna.Framework.Color, backcolor As Microsoft.Xna.Framework.Color
        Dim name As String

        ' Check access level
        If GetPlayerPK(index) = False Then

            Select Case GetPlayerAccess(index)
                Case 0
                    color = Microsoft.Xna.Framework.Color.Red
                    backcolor = Microsoft.Xna.Framework.Color.Black
                Case 1
                    color = Microsoft.Xna.Framework.Color.Black
                    backcolor = Microsoft.Xna.Framework.Color.White
                Case 2
                    color = Microsoft.Xna.Framework.Color.Cyan
                    backcolor = Microsoft.Xna.Framework.Color.Black
                Case 3
                    color = Microsoft.Xna.Framework.Color.Green
                    backcolor = Microsoft.Xna.Framework.Color.Black
                Case 4
                    color = Microsoft.Xna.Framework.Color.Yellow
                    backcolor = Microsoft.Xna.Framework.Color.Black
            End Select
        Else
            color = Microsoft.Xna.Framework.Color.Red
        End If

        name = GetPlayerName(index) & "'s " & Type.Pet(Type.Player(index).Pet.Num).Name

        ' calc pos
        textX = ConvertMapX(Type.Player(index).Pet.X * PicX) + Type.Player(index).Pet.XOffset + (PicX \ 2) - TextWidth(name) / 2
        If Type.Pet(Type.Player(index).Pet.Num).Sprite < 1 Or Type.Pet(Type.Player(index).Pet.Num).Sprite > NumCharacters Then
            textY = ConvertMapY(Type.Player(index).Pet.Y * PicY) + Type.Player(index).Pet.YOffset - 16
        Else
            ' Determine location for text
            textY = ConvertMapY(Type.Player(index).Pet.Y * PicY) + Type.Player(index).Pet.YOffset - (Client.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, Type.Pet(Type.Player(index).Pet.Num).Sprite)).Height / 4) + 16
        End If

        ' Draw name
        RenderText(name, textX, textY, color, backcolor)
    End Sub

#End Region

#Region "Misc"

    Friend Function PetAlive(index As Integer) As Boolean
        PetAlive = False

        If Type.Player(index).Pet.Alive = 1 Then
            PetAlive = True
        End If

    End Function

#End Region
    
End Module