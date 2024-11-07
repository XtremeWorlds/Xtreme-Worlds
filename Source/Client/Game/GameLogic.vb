Imports Core
Imports Mirage.Sharp.Asfw

Module GameLogic
    Sub ProcessNpcMovement(MapNpcNum As Integer)
        ' Check if NPC is walking, and if so process moving them over
        If MyMapNPC(MapNPCNum).Moving = MovementType.Walking Then

            Select Case MyMapNPC(MapNPCNum).Dir
                Case DirectionType.Up
                    MyMapNPC(MapNPCNum).YOffset = MyMapNPC(MapNPCNum).YOffset - ((GameState.ElapsedTime / 1000) * (GameState.WalkSpeed * GameState.SizeY))
                    If MyMapNPC(MapNPCNum).YOffset < 0 Then MyMapNPC(MapNPCNum).YOffset = 0

                Case DirectionType.Down
                    MyMapNPC(MapNPCNum).YOffset = MyMapNPC(MapNPCNum).YOffset + ((GameState.ElapsedTime / 1000) * (GameState.WalkSpeed * GameState.SizeY))
                    If MyMapNPC(MapNPCNum).YOffset > 0 Then MyMapNPC(MapNPCNum).YOffset = 0

                Case DirectionType.Left
                    MyMapNPC(MapNPCNum).XOffset = MyMapNPC(MapNPCNum).XOffset - ((GameState.ElapsedTime / 1000) * (GameState.WalkSpeed * GameState.SizeX))
                    If MyMapNPC(MapNPCNum).XOffset < 0 Then MyMapNPC(MapNPCNum).XOffset = 0

                Case DirectionType.Right
                    MyMapNPC(MapNPCNum).XOffset = MyMapNPC(MapNPCNum).XOffset + ((GameState.ElapsedTime / 1000) * (GameState.WalkSpeed * GameState.SizeX))
                    If MyMapNPC(MapNPCNum).XOffset > 0 Then MyMapNPC(MapNPCNum).XOffset = 0

            End Select

            ' Check if completed walking over to the next tile
            If MyMapNPC(MapNPCNum).Moving > 0 Then
                If MyMapNPC(MapNPCNum).Dir = DirectionType.Right Or MyMapNPC(MapNPCNum).Dir = DirectionType.Down Then
                    If (MyMapNPC(MapNPCNum).XOffset >= 0) And (MyMapNPC(MapNPCNum).YOffset >= 0) Then
                        MyMapNPC(MapNPCNum).Moving = 0
                        If MyMapNPC(MapNPCNum).Steps = 1 Then
                            MyMapNPC(MapNPCNum).Steps = 3
                        Else
                            MyMapNPC(MapNPCNum).Steps = 1
                        End If
                    End If
                Else
                    If (MyMapNPC(MapNPCNum).XOffset <= 0) And (MyMapNPC(MapNPCNum).YOffset <= 0) Then
                        MyMapNPC(MapNPCNum).Moving = 0
                        If MyMapNPC(MapNPCNum).Steps = 1 Then
                            MyMapNPC(MapNPCNum).Steps = 3
                        Else
                            MyMapNPC(MapNPCNum).Steps = 1
                        End If
                    End If
                End If
            End If
        End If
    End Sub

    Friend Function IsInBounds()
        IsInBounds = 0

        If (GameState.CurX >= 0) And (GameState.CurX <= MyMap.MaxX) Then
            If (GameState.CurY >= 0) And (GameState.CurY <= MyMap.MaxY) Then
                IsInBounds = 1
            End If
        End If

    End Function

    Function GameStarted() As Boolean
        GameStarted = 0
        If GameState.InGame = False Then Exit Function
        If GameState.MapData = False Then Exit Function
        If GameState.PlayerData = False Then Exit Function
        GameStarted = 1
    End Function

    Friend Sub CreateActionMsg(message As String, color As Integer, msgType As Byte, x As Integer, y As Integer)

        GameState.ActionMsgIndex = GameState.ActionMsgIndex + 1
        If GameState.ActionMsgIndex >= Byte.MaxValue Then GameState.ActionMsgIndex = 1

        With ActionMsg(GameState.ActionMsgIndex)
            .Message = message
            .Color = color
            .Type = msgType
            .Created = GetTickCount()
            .Scroll = 1
            .X = x
            .Y = y
        End With

        If ActionMsg(GameState.ActionMsgIndex).Type = ActionMsgType.Scroll Then
            ActionMsg(GameState.ActionMsgIndex).Y = ActionMsg(GameState.ActionMsgIndex).Y + Rand(-2, 6)
            ActionMsg(GameState.ActionMsgIndex).X = ActionMsg(GameState.ActionMsgIndex).X + Rand(-8, 8)
        End If

    End Sub

    Friend Function Rand(maxNumber As Integer, Optional minNumber As Integer = 0) As Integer
        If minNumber > maxNumber Then
            Dim t As Integer = minNumber
            minNumber = maxNumber
            maxNumber = t
        End If

        Return Random.NextDouble(minNumber, maxNumber)
    End Function

    ' BitWise Operators for directional blocking
    Friend Sub SetDirBlock(ByRef blockvar As Byte, ByRef dir As Byte, block As Boolean)
        If block Then
            blockvar = blockvar Or (2 ^ dir)
        Else
            blockvar = blockvar And Not (2 ^ dir)
        End If
    End Sub

    Friend Function IsDirBlocked(ByRef blockvar As Byte, ByRef dir As Byte) As Boolean
        Return blockvar And (2 ^ dir)
    End Function

    Friend Function ConvertCurrency(amount As Integer) As String

        If Int(amount) < 10000 Then
            ConvertCurrency = amount
        ElseIf Int(amount) < 999999 Then
            ConvertCurrency = Int(amount / 1000) & "k"
        ElseIf Int(amount) < 999999999 Then
            ConvertCurrency = Int(amount / 1000000) & "m"
        Else
            ConvertCurrency = Int(amount / 1000000000) & "b"
        End If

    End Function

    Sub HandlePressEnter()
        Dim chatText As String
        Dim name As String
        Dim i As Integer
        Dim n As Integer
        Dim command() As String
        Dim buffer As ByteStream

        If GameState.InGame Then
            chatText = Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).Text
        End If

        ' hide/show chat window
        If chatText = "" Then
            If Gui.Windows(Gui.GetWindowIndex("winChat")).Visible = True
                Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).Text = ""
                HideChat()
                Exit Sub
            End If
        End If

        ' Admin message
        If Left$(chatText, 1) = "@" Then
            chatText = Mid$(chatText, 2, Len(chatText) - 1)

            If Len(chatText) > 0 Then
                AdminMsg(chatText)
            End If

            Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).text = ""
            Exit Sub
        End If

        ' Broadcast message
        If Left$(chatText, 1) = "'" Then
            chatText = Mid$(chatText, 2, Len(chatText) - 1)

            If Len(chatText) > 0 Then
                BroadcastMsg(chatText)
            End If

            Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).text = ""
            Exit Sub
        End If

        ' party message
        If Left$(chatText, 1) = "-" Then
            chatText = Mid$(chatText, 2, Len(chatText) - 1)

            If Len(chatText) > 0 Then
                SendPartyChatMsg(chatText)
            End If

            Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).text = ""
            Exit Sub
        End If

        ' Player message
        If Left$(chatText, 1) = "!" Then
            chatText = Mid$(chatText, 2, Len(chatText) - 1)
            name = ""

            ' Get the desired player from the user text
            For i = 0 To Len(chatText)

                If Mid$(chatText, i, 1) <> Space(1) Then
                    name = name & Mid$(chatText, i, 1)
                Else
                    Exit For
                End If

            Next

            chatText = Mid$(chatText, i, Len(chatText) - 1)

            ' Make sure they are actually sending something
            If Len(chatText) > 0 Then
                ' Send the message to the player
                PlayerMsg(chatText, name)
            Else
                AddText(Language.Chat.PlayerMsg, ColorType.Yellow)
            End If

            GoTo Continue1
        End If

        If Left$(chatText, 1) = "/" Then
            command = Split(chatText, Space(1))

            Select Case command(0)
                Case "/emote"
                    ' Checks to make sure we have more than one string in the array
                    If UBound(command) < 1 OrElse Not IsNumeric(command(1)) Then
                        AddText(Language.Chat.Emote, ColorType.Yellow)
                        GoTo Continue1
                    End If

                    SendUseEmote(command(1))

                Case "/help"
                    AddText(Language.Chat.Help1, ColorType.Yellow)
                    AddText(Language.Chat.Help2, ColorType.Yellow)
                    AddText(Language.Chat.Help3, ColorType.Yellow)
                    AddText(Language.Chat.Help4, ColorType.Yellow)
                    AddText(Language.Chat.Help5, ColorType.Yellow)
                    AddText(Language.Chat.Help6, ColorType.Yellow)

                Case "/info"
                    If GameState.MyTarget > 0 Then
                        If GameState.MyTargetType = TargetType.Player Then
                            SendPlayerInfo(GetPlayerName(GameState.MyTarget))
                            GoTo Continue1
                        End If
                    End If

                    ' Checks to make sure we have more than one string in the array
                    If UBound(command) < 1 OrElse IsNumeric(command(1)) Then
                        AddText(Language.Chat.Info, ColorType.Yellow)
                        GoTo Continue1
                    End If

                    SendPlayerInfo(command(1))

                ' Whos Online
                Case "/who"
                    SendWhosOnline()

                ' Requets level up
                Case "/levelup"
                    SendRequestLevelUp()

                ' Checking fps
                Case "/fps"
                    GameState.Bfps = Not GameState.Bfps

                Case "/lps"
                    GameState.Blps = Not GameState.Blps

                ' Request stats
                Case "/stats"
                    buffer = New ByteStream(4)
                    buffer.WriteInt32(ClientPackets.CGetStats)
                    Socket.SendData(buffer.Data, buffer.Head)
                    buffer.Dispose()

                Case "/party"
                    If GameState.MyTarget > 0 Then
                        If GameState.MyTargetType = TargetType.Player Then
                            SendPartyRequest(GetPlayerName(GameState.MyTarget))
                            GoTo Continue1
                        End If
                    End If

                    ' Make sure they are actually sending something
                    If UBound(command) < 1 OrElse IsNumeric(command(1)) Then
                        AddText(Language.Chat.Party, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendPartyRequest(command(1))

                ' Join party
                Case "/join"
                    SendAcceptParty()

                ' Leave party
                Case "/leave"
                    SendLeaveParty()

                ' Trade
                Case "/trade"
                    If GameState.MyTarget > 0 Then
                        If GameState.MyTargetType = TargetType.Player Then
                            SendTradeRequest(GetPlayerName(GameState.MyTarget))
                            GoTo Continue1
                        End If
                    End If

                    ' Make sure they are actually sending something
                    If UBound(command) < 1 OrElse IsNumeric(command(1)) Then
                        AddText(Language.Chat.Trade, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendTradeRequest(command(1))

                ' // Moderator Admin Commands //
                ' Admin Help
                Case "/admin"
                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Moderator Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    AddText(Language.Chat.Admin1, ColorType.Yellow)
                    AddText(Language.Chat.Admin2, ColorType.Yellow)
                    AddText(Language.Chat.AdminGblMsg, ColorType.Yellow)
                    AddText(Language.Chat.AdminPvtMsg, ColorType.Yellow)

                ' Kicking a player
                Case "/kick"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Moderator Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 1 OrElse IsNumeric(command(1)) Then
                        AddText(Language.Chat.Kick, ColorType.Yellow)
                        GoTo Continue1
                    End If

                    SendKick(command(1))

                ' // Mapper Admin Commands //
                ' Location
                Case "/loc"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    GameState.BLoc = Not GameState.BLoc

                ' Warping to a player
                Case "/warpmeto"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 1 OrElse IsNumeric(command(1)) Then
                        AddText(Language.Chat.WarpMeTo, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    WarpMeTo(command(1))

                ' Warping a player to you
                Case "/warptome"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 1 OrElse IsNumeric(command(1)) Then
                        AddText(Language.Chat.WarpToMe, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    WarpToMe(command(1))

                ' Warping to a map
                Case "/warpto"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 1 OrElse Not IsNumeric(command(1)) Then
                        AddText(Language.Chat.WarpTo, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    n = command(1)

                    ' Check to make sure its a valid map #
                    If n > 0 And n <= MAX_MAPS Then
                        WarpTo(n)
                    Else
                        AddText(Language.Chat.InvalidMap, ColorType.BrightRed)
                    End If

                ' Setting sprite
                Case "/sprite"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 1 OrElse Not IsNumeric(command(1)) Then
                        AddText(Language.Chat.Sprite, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendSetSprite(command(1))

                ' Map report
                Case "/mapreport"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestMapReport()

                ' Respawn request
                Case "/respawn"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendMapRespawn()

                Case "/editmap"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Mapper Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditMap()

                ' // Moderator Commands //
                ' Welcome change
                Case "/welcome"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Moderator Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 1 Then
                        AddText(Language.Chat.Welcome, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendMotdChange(Right$(chatText, Len(chatText) - 5))

                ' Check the ban list
                Case "/banlist"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Moderator Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendBanList()

                ' Banning a player
                Case "/ban"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Moderator Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 1 Then
                        AddText(Language.Chat.Ban, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendBan(command(1))

                ' // Owner Admin Commands //
                ' Giving another player access
                Case "/bandestroy"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Owner Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendBanDestroy()

                Case "/access"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Owner Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    If UBound(command) < 2 OrElse
                        IsNumeric(command(1)) Or
                        Not IsNumeric(command(2)) Then
                        AddText(Language.Chat.Access, ColorType.Yellow)
                        GoTo Continue1
                    End If

                    SendSetAccess(command(1), CLng(command(2)))

                ' // Developer Admin Commands //
                Case "/editresource"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditResource()

                Case "/editanimation"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditAnimation()

                Case "/editpet"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditPet()

                Case "/edititem"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditItem()

                Case "/editprojectile"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditProjectile()

                Case "/editnpc"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditNpc()

                Case "/editjob"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditJob()

                Case "/editskill"

                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditSkill()

                Case "/editshop"
                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditShop()

                    
                Case "/editmoral"
                    If GetPlayerAccess(GameState.MyIndex) < AccessType.Developer Then
                        AddText(Language.Chat.AccessAlert, ColorType.BrightRed)
                        GoTo Continue1
                    End If

                    SendRequestEditMoral()
                Case ""

                Case Else
                    AddText(Language.Chat.InvalidCmd, ColorType.BrightRed)
            End Select

        ElseIf Len(chatText) > 0 Then ' Say message
            SayMsg(chatText)
        End If

Continue1:
        Gui.Windows(Gui.GetWindowIndex("winChat")).Controls(Gui.GetControlIndex("winChat", "txtChat")).Text = ""
    End Sub

    Sub CheckMapGetItem()
        Dim buffer As New ByteStream(4)
        buffer = New ByteStream(4)

        If GetTickCount() > Type.Player(GameState.MyIndex).MapGetTimer + 250 Then
            Type.Player(GameState.MyIndex).MapGetTimer = GetTickCount()
            buffer.WriteInt32(ClientPackets.CMapGetItem)
            Socket.SendData(buffer.Data, buffer.Head)
        End If

        buffer.Dispose()
    End Sub

    Friend Sub ClearActionMsg(index As Byte)
        ActionMsg(index).Message = ""
        ActionMsg(index).Created = 0
        ActionMsg(index).Type = 0
        ActionMsg(index).Color = 0
        ActionMsg(index).Scroll = 0
        ActionMsg(index).X = 0
        ActionMsg(index).Y = 0
    End Sub

    Friend Sub UpdateDrawMapName()
        If MyMap.Moral > 0 Then
            GameState.DrawMapNameColor = GameClient.QbColorToXnaColor(Type.Moral(MyMap.Moral).Color)
        End If
    End Sub

    Friend Sub AddChatBubble(target As Integer, targetType As Byte, msg As String, Color As Integer)
        Dim i As Integer, index As Integer

        ' Set the global index
        GameState.ChatBubbleindex = GameState.ChatBubbleindex + 1
        If GameState.ChatBubbleindex < 1 Or GameState.ChatBubbleindex > Byte.MaxValue Then GameState.ChatBubbleindex = 1

        ' Default to new bubble
        index = GameState.ChatBubbleindex

        ' Loop through and see if that player/npc already has a chat bubble
        For i = 1 To Byte.MaxValue
            If ChatBubble(i).TargetType = targetType Then
                If ChatBubble(i).Target = target Then
                    ' Reset master index
                    If GameState.ChatBubbleindex > 1 Then GameState.ChatBubbleindex = GameState.ChatBubbleindex - 1

                    ' We use this one now, yes?
                    index = i
                    Exit For
                End If
            End If
        Next

        ' Set the bubble up
        With ChatBubble(index)
            .Target = target
            .TargetType = targetType
            .Msg = msg
            .Color = Color
            .Timer = GetTickCount()
            .Active = 1
        End With

    End Sub

    Public Sub DialogueAlert(ByVal Index As Long)
        Dim header As String, body As String, body2 As String

        ' find the body/header
        Select Case Index

            Case DialogueMsg.Connection
                header = "Connection Problem"
                body = "You lost connection to the server."
                body2 = "Please try again later."

            Case DialogueMsg.Banned
                header = "Banned"
                body = "You have been banned, have a nice day!"
                body2 = "Please send all ban appeals to an administrator."

            Case DialogueMsg.Kicked
                header = "Kicked"
                body = "You have been kicked."
                body2 = "Please try and behave."

            Case DialogueMsg.Outdated
                header = "Wrong Version"
                body = "Your game client is the wrong version."
                body2 = "Please try updating."

            Case DialogueMsg.Maintenance
                header = "Connection Refused"
                body = "The server is currently going under maintenance."
                body2 = "Please try again soon."

            Case DialogueMsg.NameTaken
                header = "Invalid Name"
                body = "This name is already in use."
                body2 = "Please try another name."

            Case DialogueMsg.NameLength
                header = "Invalid Name"
                body = "This name is too short or too long."
                body2 = "Please try another name."

            Case DialogueMsg.NameIllegal
                header = "Invalid Name"
                body = "This name contains illegal characters."
                body2 = "Please try another name."

            Case DialogueMsg.Database
                header = "Invalid Connection"
                body = "Cannot connect to database."
                body2 = "Please try again later."

            Case DialogueMsg.WrongPass
                header = "Invalid Login"
                body = "Invalid username or password."
                body2 = "Please try again."
                Gui.ClearPasswordTexts()

            Case DialogueMsg.Activate
                header = "Inactive Account"
                body = "Your account is not activated."
                body2 = "Please activate your account then try again."

            Case DialogueMsg.MaxChar
                header = "Cannot Merge"
                body = "You cannot merge a full account."
                body2 = "Please clear a character slot."

            Case DialogueMsg.DelChar
                header = "Deleted Character"
                body = "Your character was successfully deleted."
                body2 = "Please log on to continue playing."

            Case DialogueMsg.CreateAccount
                header = "Account Created"
                body = "Your account was successfully created."
                body2 = "Now, you can play!"

            Case DialogueMsg.MultiAccount
                header = "Multiple Accounts"
                body = "Multiple accounts is not authorized."
                body2 = "Please logout and try again!"

            Case DialogueMsg.Login
                header = "Cannot Login"
                body = "This account does not exist."
                body2 = "Please try registering the account."

            Case DialogueMsg.Crash
                header = "Error"
                body = "There was a network error."
                body2 = "Check logs folder for details."

                Gui.HideWindows
                Gui.ShowWindow(Gui.GetWindowIndex("winLogin")) 
        End Select

        ' set the dialogue up!
        Dialogue(header, body, body2, DialogueType.Alert)
    End Sub

    Public Sub CloseDialogue()
        Gui.HideWindow(Gui.GetWindowIndex("winDialogue"))
    End Sub

    Public Sub Dialogue(ByVal header As String, ByVal body As String, ByVal body2 As String, ByVal Index As Byte, Optional ByVal style As Byte = 1, Optional ByVal Data1 As Long = 0, Optional ByVal Data2 As Long = 0, Optional ByVal Data3 As Long = 0, Optional ByVal Data4 As Long = 0, Optional ByVal Data5 As Long = 0)
        ' exit out if we've already got a dialogue open
        If Gui.GetWindowIndex("winDialogue") = 0 Then Exit Sub
        If Gui.Windows(Gui.GetWindowIndex("winDialogue")).Visible = True Then Exit Sub

        ' set buttons
        With Gui.Windows(Gui.GetWindowIndex("winDialogue"))
            If style = DialogueStyle.YesNo Then
                .Controls(Gui.GetControlIndex("winDialogue", "btnYes")).Visible = True
                .Controls(Gui.GetControlIndex("winDialogue", "btnNo")).Visible = True
                .Controls(Gui.GetControlIndex("winDialogue", "btnOkay")).Visible = False
                .Controls(Gui.GetControlIndex("winDialogue", "txtInput")).Visible = False
                .Controls(Gui.GetControlIndex("winDialogue", "lblBody_2")).Visible = True
            ElseIf style = DialogueStyle.Okay Then
                .Controls(Gui.GetControlIndex("winDialogue", "btnYes")).Visible = False
                .Controls(Gui.GetControlIndex("winDialogue", "btnNo")).Visible = False
                .Controls(Gui.GetControlIndex("winDialogue", "btnOkay")).Visible = True
                .Controls(Gui.GetControlIndex("winDialogue", "txtInput")).Visible = False
                .Controls(Gui.GetControlIndex("winDialogue", "lblBody_2")).Visible = True
            ElseIf style = DialogueStyle.Input Then
                .Controls(Gui.GetControlIndex("winDialogue", "btnYes")).Visible = False
                .Controls(Gui.GetControlIndex("winDialogue", "btnNo")).Visible = False
                .Controls(Gui.GetControlIndex("winDialogue", "btnOkay")).Visible = True
                .Controls(Gui.GetControlIndex("winDialogue", "txtInput")).Visible = True
                .Controls(Gui.GetControlIndex("winDialogue", "lblBody_2")).Visible = False
            End If

            ' set labels
            .Controls(Gui.GetControlIndex("winDialogue", "lblHeader")).Text = header
            .Controls(Gui.GetControlIndex("winDialogue", "lblBody_1")).Text = body
            .Controls(Gui.GetControlIndex("winDialogue", "lblBody_2")).Text = body2
            .Controls(Gui.GetControlIndex("winDialogue", "txtInput")).Text = ""
        End With

        ' set it all up
        GameState.diaIndex = Index
        GameState.diaData1 = Data1
        GameState.diaData2 = Data2
        GameState.diaData3 = Data3
        GameState.diaData4 = Data4
        GameState.diaData5 = Data5
        GameState.diaStyle = style

        ' make the Gui.Windows visible
        Gui.ShowWindow(Gui.GetWindowIndex("winDialogue"), True)
    End Sub

    Public Sub DialogueHandler(ByVal Index As Long)
        Dim value As Long, diaInput As String
        Dim X As Integer
        Dim Y As Integer

        diaInput = Gui.Windows(Gui.GetWindowIndex("winDialogue")).Controls(Gui.GetControlIndex("winDialogue", "txtInput")).Text

        ' Find out which button
        If Index = 1 Then ' Okay button
            ' Dialogue index
            Select Case GameState.diaIndex
                Case DialogueType.TradeAmount
                    value = Val(diaInput)
                    TradeItem(GameState.diaData1, value)

                Case DialogueType.DepositItem
                    value = Val(diaInput)
                    DepositItem(GameState.diaData1, value)

                Case DialogueType.WithdrawItem
                    value = Val(diaInput)
                    WithdrawItem(GameState.diaData1, value)

                Case DialogueType.DropItem
                    value = Val(diaInput)
                    SendDropItem(GameState.diaData1, value)
            End Select

        ElseIf Index = 2 Then ' Yes button
            ' Dialogue index
            Select Case GameState.diaIndex
                Case DialogueType.Trade
                    SendHandleTradeInvite(1)

                Case DialogueType.Forget
                    ForgetSkill(GameState.diaData1)

                Case DialogueType.Party
                    SendAcceptParty()

                Case DialogueType.LootItem
                    CheckMapGetItem()

                Case DialogueType.DelChar
                    SendDelChar(GameState.diaData1)

                Case DialogueType.FillLayer
                    If GameState.diaData2 > 1 Then
                        For X = 0 To MyMap.MaxX
                            For Y = 0 To MyMap.MaxY
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).X = GameState.diaData3
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).Y = GameState.diaData4
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).Tileset = GameState.diaData5
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).AutoTile = GameState.diaData2
                                CacheRenderState(X, Y, GameState.diaData1)
                            Next
                        Next

                        ' do a re-init so we can see our changes
                        InitAutotiles()
                    Else
                        For X = 0 To MyMap.MaxX
                            For Y = 0 To MyMap.MaxY
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).X = GameState.diaData3
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).Y = GameState.diaData4
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).Tileset = GameState.diaData5
                                MyMap.Tile(X, Y).Layer(GameState.diaData1).AutoTile = 0
                                CacheRenderState(X, Y, GameState.diaData1)
                            Next
                        Next
                    End If

                Case DialogueType.ClearLayer
                    For X = 0 To MyMap.MaxX
                        For Y = 0 To MyMap.MaxY
                            With MyMap.Tile(X, Y)
                                .Layer(GameState.diaData1).X = 0
                                .Layer(GameState.diaData1).Y = 0
                                .Layer(GameState.diaData1).Tileset = 0
                                .Layer(GameState.diaData1).AutoTile = 0
                                CacheRenderState(X, Y, GameState.diaData1)
                            End With
                        Next
                    Next

                Case DialogueType.ClearAttributes
                    For X = 0 To MyMap.MaxX
                        For Y = 0 To MyMap.MaxY
                            MyMap.Tile(X, Y).Type = 0
                            MyMap.Tile(X, Y).Type2 = 0
                        Next
                    Next
            End Select

        ElseIf Index = 3 Then ' No button
            ' Dialogue index
            Select Case GameState.diaIndex
                Case DialogueType.Trade
                    SendHandleTradeInvite(0)

                Case DialogueType.Party
                    SendDeclineParty()
            End Select
        End If

        CloseDialogue()
        GameState.diaIndex = 0
        diaInput = ""
    End Sub

    Public Sub ShowJobs()
        Gui.HideWindows()
        GameState.newCharJob = 1
        GameState.newCharSprite = 1
        GameState.newCharGender = SexType.Male
        Gui.Windows(Gui.GetWindowIndex("winJob")).Controls(Gui.GetControlIndex("winJob", "lblClassName")).Text = Type.Job(GameState.newCharJob).Name
        Gui.Windows(Gui.GetWindowIndex("winNewChar")).Controls(Gui.GetControlIndex("winNewChar", "txtName")).Text = ""
        Gui.Windows(Gui.GetWindowIndex("winNewChar")).Controls(Gui.GetControlIndex("winNewChar", "chkMale")).Value = 1
        Gui.Windows(Gui.GetWindowIndex("winNewChar")).Controls(Gui.GetControlIndex("winNewChar", "chkFemale")).Value = 0
        Gui.ShowWindow(Gui.GetWindowIndex("winJob"))
    End Sub

    Public Sub AddChar(name As String, sex As Integer, job As Integer, sprite As Integer)
        If Socket.IsConnected() Then
            Call SendAddChar(name, sex, job)
        Else
            InitNetwork()
            Dialogue("Invalid Connection", "Cannot connect to game server.", "Please try again.", DialogueType.Alert)
        End If
    End Sub

    Public Sub ShowChat()
        Gui.ShowWindow(Gui.GetWindowIndex("winChat"), , False)
        Gui.HideWindow(Gui.GetWindowIndex("winChatSmall"))
        ' Set the active control
        Gui.ActiveWIndow = Gui.GetWindowIndex("winChat")
        Gui.SetActiveControl(Gui.GetWindowIndex("winChat"), Gui.GetControlIndex("winChat", "txtChat"))
        GameState.inSmallChat = 0
        GameState.ChatScroll = 0
    End Sub

    Public Sub HideChat()
        Gui.ShowWindow(Gui.GetWindowIndex("winChatSmall"), , False)
        Gui.HideWindow(Gui.GetWindowIndex("winChat"))

        ' Set the active control
        Gui.ActiveWIndow = Gui.GetWindowIndex("winChat")
        Gui.SetActiveControl(Gui.GetWindowIndex("winChat"), Gui.GetControlIndex("winChat", "txtChat"))

        GameState.inSmallChat = 1
        GameState.ChatScroll = 0
    End Sub

    Public Sub SetChatHeight(Height As Long)
        GameState.actChatHeight = Height
    End Sub

    Public Sub SetChatWidth(Width As Long)
        GameState.actChatWidth = Width
    End Sub

    Public Sub UpdateChat()
        Settings.Save()
    End Sub

    Public Sub ScrollChatBox(ByVal direction As Byte)
        If direction = 0 Then ' up
            If Len(Chat(GameState.ChatScroll + 7).Text) > 0 Then
                If GameState.ChatScroll < CHAT_LINES Then
                    GameState.ChatScroll = GameState.ChatScroll + 1
                End If
            End If
        Else
            If GameState.ChatScroll > 0 Then
                GameState.ChatScroll = GameState.ChatScroll - 1
            End If
        End If
    End Sub

    Public Function IsHotbar(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectangleStruct
        Dim i As Long

        For i = 1 To MAX_Hotbar
            With tempRec
                .Top = StartY + GameState.HotbarTop
                .Left = StartX + ((i - 1) * GameState.HotbarOffsetX)
                .Right = .Left + GameState.PicX
                .Bottom = .Top + GameState.PicY
            End With

            If Type.Player(GameState.MyIndex).Hotbar(i).Slot > 0 Then
                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.Bottom Then
                        IsHotbar = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Public Sub ShowInvDesc(x As Long, y As Long, invNum As Long)
        Dim soulBound As Boolean

        If invNum <= 0 Or invNum > MAX_INV Then Exit Sub

        ' show
        If GetPlayerInv(GameState.MyIndex, invNum) > 0 Then
            If Type.Item(GetPlayerInv(GameState.MyIndex, invNum)).BindType > 0 And Type.Player(GameState.MyIndex).Inv(invNum).Bound > 0 Then soulBound = 1
            ShowItemDesc(x, y, GetPlayerInv(GameState.MyIndex, invNum))
        End If
    End Sub

    Public Sub ShowItemDesc(x As Long, y As Long, itemNum As Long)
        Dim Color As Microsoft.Xna.Framework.Color, theName As String, jobName As String, levelTxt As String, i As Long

        ' set globals
        GameState.descType = PartType.Item ' inventory
        GameState.descItem = itemNum

        ' set position
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Left = x
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Top = y

        ' show the window
        Gui.ShowWindow(Gui.GetWindowIndex("winDescription"), , False)

        ' exit out early if last is same
        If (GameState.descLastType = GameState.descType) And (GameState.descLastItem = GameState.descItem) Then Exit Sub

        ' set last to this
        GameState.descLastType = GameState.descType
        GameState.descLastItem = GameState.descItem

        ' show req. labels
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Controls(Gui.GetControlIndex("winDescription", "lblClass")).Visible = True
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Controls(Gui.GetControlIndex("winDescription", "lblLevel")).Visible = True
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Controls(Gui.GetControlIndex("winDescription", "picBar")).Visible = False

        ' set variables
        With Gui.Windows(Gui.GetWindowIndex("winDescription"))
            ' name
            'If Not soulBound Then
            theName = Type.Item(itemNum).Name
            'Else
            'theName = "(SB) " & Item(itemNum).Name)
            'End If
            .Controls(Gui.GetControlIndex("winDescription", "lblName")).Text = theName
            Select Case Type.Item(itemNum).Rarity
                Case 0 ' white
                    Color = Color.White
                Case 1 ' green
                    Color = Color.Green
                Case 2 ' blue
                    Color = Color.Blue
                Case 3 ' maroon
                    Color = Color.Red
                Case 4 ' purple
                    Color = Color.Magenta
                Case 5 ' cyan
                    Color = Color.Cyan
            End Select
            .Controls(Gui.GetControlIndex("winDescription", "lblName")).Color = Color

            ' class req
            If Type.Item(itemNum).JobReq > 0 Then
                jobName = Type.Job(Type.Item(itemNum).JobReq).Name
                ' do we match it?
                If GetPlayerJob(GameState.MyIndex) = Type.Item(itemNum).JobReq Then
                    Color = Color.Green
                Else
                    Color = Color.Red
                End If
            Else
                jobName = "No Job Req."
                Color = Color.Green
            End If

            .Controls(Gui.GetControlIndex("winDescription", "lblClass")).Text = jobName
            .Controls(Gui.GetControlIndex("winDescription", "lblClass")).Color = Color
            
            ' level
            If Type.Item(itemNum).LevelReq > 0 Then
                levelTxt = "Level " & Type.Item(itemNum).LevelReq
                ' do we match it?
                If GetPlayerLevel(GameState.MyIndex) >= Type.Item(itemNum).LevelReq Then
                    Color = Color.Green
                Else
                    Color = Color.Red
                End If
            Else
                levelTxt = "No Level Req."
                Color = Color.Green
            End If
            .Controls(Gui.GetControlIndex("winDescription", "lblLevel")).Text = levelTxt
            .Controls(Gui.GetControlIndex("winDescription", "lblLevel")).Color = Color
        End With

        ' clear
        ReDim GameState.descText(1)

        ' go through the rest of the text
        Select Case Type.Item(itemNum).Type
            Case ItemType.None
                AddDescInfo("No Type", Microsoft.Xna.Framework.Color.White)
            Case ItemType.Equipment
                Select Case Type.Item(itemNum).SubType
                    Case ItemSubType.Weapon
                        AddDescInfo("Weapon", Microsoft.Xna.Framework.Color.White)
                    Case ItemSubType.Armor
                        AddDescInfo("Armor", Microsoft.Xna.Framework.Color.White)
                    Case ItemSubType.Helmet
                        AddDescInfo("Helmet", Microsoft.Xna.Framework.Color.White)
                    Case ItemSubType.Shield
                        AddDescInfo("Shield", Microsoft.Xna.Framework.Color.White)
                    Case ItemSubType.Shoes
                        AddDescInfo("Shoes", Microsoft.Xna.Framework.Color.White)
                    Case ItemSubType.Gloves
                        AddDescInfo("Gloves", Microsoft.Xna.Framework.Color.White)
                End Select
            Case ItemType.Consumable
                AddDescInfo("Consumable", Microsoft.Xna.Framework.Color.White)
            Case ItemType.Currency
                AddDescInfo("Currency", Microsoft.Xna.Framework.Color.White)
            Case ItemType.Skill
                AddDescInfo("Skill", Microsoft.Xna.Framework.Color.White)
            Case ItemType.Projectile
                AddDescInfo("Projectile", Microsoft.Xna.Framework.Color.White)
            Case ItemType.Pet
                AddDescInfo("Pet", Microsoft.Xna.Framework.Color.White)
        End Select

        ' more info
        Select Case Type.Item(itemNum).Type
            Case ItemType.None, ItemType.Currency
                ' binding
                If Type.Item(itemNum).BindType = 1 Then
                    AddDescInfo("Bind on Pickup",Microsoft.Xna.Framework. Color.White)
                ElseIf Type.Item(itemNum).BindType = 2 Then
                    AddDescInfo("Bind on Equip", Microsoft.Xna.Framework.Color.White)
                End If

                AddDescInfo("Value: " & Type.Item(itemNum).Price & " g", Microsoft.Xna.Framework.Color.Yellow)
            Case ItemType.Equipment
                ' Damage/defense
                If Type.Item(itemNum).SubType = EquipmentType.Weapon Then
                    AddDescInfo("Damage: " & Type.Item(itemNum).Data2, Microsoft.Xna.Framework.Color.White)
                    AddDescInfo("Speed: " & (Type.Item(itemNum).Speed / 1000) & "s", Microsoft.Xna.Framework.Color.White)
                Else
                    If Type.Item(itemNum).Data2 > 0 Then
                        AddDescInfo("Defense: " & Type.Item(itemNum).Data2, Microsoft.Xna.Framework.Color.White)
                    End If
                End If

                ' binding
                If Type.Item(itemNum).BindType = 1 Then
                    AddDescInfo("Bind on Pickup", Microsoft.Xna.Framework.Color.White)
                ElseIf Type.Item(itemNum).BindType = 2 Then
                    AddDescInfo("Bind on Equip", Microsoft.Xna.Framework.Color.White)
                End If

                AddDescInfo("Value: " & Type.Item(itemNum).Price & " G", Microsoft.Xna.Framework.Color.Yellow)

                ' stat bonuses
                If Type.Item(itemNum).Add_Stat(StatType.Strength) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Strength) & " Str", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Luck) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Luck) & " End", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Spirit) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Spirit) & " Spi", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Luck) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Luck) & " Luc", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Intelligence) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Intelligence) & " Int", Microsoft.Xna.Framework.Color.White)
                End If
            Case ItemType.Consumable
                If Type.Item(itemNum).Add_Stat(StatType.Strength) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Strength) & " Str", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Luck) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Luck) & " End", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Spirit) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Spirit) & " Spi", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Luck) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Luck) & " Luc", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Add_Stat(StatType.Intelligence) > 0 Then
                    AddDescInfo("+" & Type.Item(itemNum).Add_Stat(StatType.Intelligence) & " Int", Microsoft.Xna.Framework.Color.White)
                End If

                If Type.Item(itemNum).Data1 > 0 Then
                    Select Case Type.Item(itemNum).SubType
                        Case ItemSubType.AddHP
                            AddDescInfo("+" & Type.Item(itemNum).Data1 & " HP", Microsoft.Xna.Framework.Color.White)
                        Case ItemSubType.AddMP
                            AddDescInfo("+" & Type.Item(itemNum).Data1 & " MP", Microsoft.Xna.Framework.Color.White)
                        Case ItemSubType.AddSP
                            AddDescInfo("+" & Type.Item(itemNum).Data1 & " SP", Microsoft.Xna.Framework.Color.White)
                        Case ItemSubType.Exp
                            AddDescInfo("+" & Type.Item(itemNum).Data1 & " EXP", Microsoft.Xna.Framework.Color.White)
                    End Select
                    
                End If

                AddDescInfo("Value: " & Type.Item(itemNum).Price & " G", Microsoft.Xna.Framework.Color.Yellow)
            Case ItemType.Skill
                AddDescInfo("Value: " & Type.Item(itemNum).Price & " G", Microsoft.Xna.Framework.Color.Yellow)
        End Select
    End Sub

    Public Sub ShowSkillDesc(x As Long, y As Long, Skillnum As Long, SkillSlot As Long)
        Dim Color As Long, theName As String, sUse As String, i As Long, barWidth As Long, tmpWidth As Long

        ' set globals
        GameState.descType = 2 ' Skill
        GameState.descItem = Skillnum
    
        ' set position
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Left = x
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Top = y
    
        ' show the window
        Gui.ShowWindow(Gui.GetWindowIndex("winDescription"), , False)
    
        ' exit out early if last is same
        If (GameState.descLastType = GameState.descType) And (GameState.descLastItem = GameState.descItem) Then Exit Sub
    
        ' clear
        ReDim GameState.descText(1)
    
        ' hide req. labels
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Controls(Gui.GetControlIndex("winDescription", "lblLevel")).Visible = False
        Gui.Windows(Gui.GetWindowIndex("winDescription")).Controls(Gui.GetControlIndex("winDescription", "picBar")).Visible = True
    
        ' set variables
        With Gui.Windows(Gui.GetWindowIndex("winDescription"))
            ' set name
            .Controls(Gui.GetControlIndex("winDescription", "lblName")).Text = Type.Skill(skillNum).Name
            .Controls(Gui.GetControlIndex("winDescription", "lblName")).Color =  Microsoft.Xna.Framework.Color.White
        
            ' find ranks
            If SkillSlot > 0 Then
                ' draw the rank bar
                barWidth = 66
                'If Type.Skill(skillNum).rank > 0 Then
                    'tmpWidth = ((PlayerSkills(SkillSlot).Uses / barWidth) / (Type.Skill(skillNum).NextUses / barWidth)) * barWidth
                'Else
                    tmpWidth = 66
                'End If
                .Controls(Gui.GetControlIndex("winDescription", "picBar")).value = tmpWidth
                ' does it rank up?
                'If Type.Skill(skillNum).NextRank > 0 Then
                    Color = ColorType.White
                    'sUse = "Uses: " & PlayerSkills(SkillSlot).Uses & "/" & Type.Skill(skillNum).NextUses
                    'If PlayerSkills(SkillSlot).Uses = Type.Skill(skillNum).NextUses Then
                        'If Not GetPlayerLevel(GameState.MyIndex) >= Skill(Type.Skill(skillNum).NextRank).LevelReq Then
                            'Color = BrightRed
                            'sUse = "Lvl " & Skill(Type.Skill(skillNum).NextRank).LevelReq & " req."
                        'End If
                    'End If
                'Else
                    Color = ColorType.Gray
                    sUse = "Max Rank"
                'End If
                ' show controls
                .Controls(Gui.GetControlIndex("winDescription", "lblClass")).Visible = True
                .Controls(Gui.GetControlIndex("winDescription", "picBar")).Visible = True
                 'set vals
                .Controls(Gui.GetControlIndex("winDescription", "lblClass")).Text = sUse
                .Controls(Gui.GetControlIndex("winDescription", "lblClass")).Color = Microsoft.Xna.Framework.Color.White
            Else
                ' hide some controls
                .Controls(Gui.GetControlIndex("winDescription", "lblClass")).Visible = False
                .Controls(Gui.GetControlIndex("winDescription", "picBar")).Visible = False
            End If
        End With
    
        Select Case Type.Skill(skillNum).Type
            Case SkillType.DamageHp
                AddDescInfo("Damage HP", Microsoft.Xna.Framework.Color.White)
            Case SkillType.DamageMp
                AddDescInfo("Damage SP", Microsoft.Xna.Framework.Color.White)
            Case SkillType.HealHp
                AddDescInfo("Heal HP", Microsoft.Xna.Framework.Color.White)
            Case SkillType.HealMp
                AddDescInfo("Heal SP", Microsoft.Xna.Framework.Color.White)
            Case SkillType.Warp
                AddDescInfo("Warp", Microsoft.Xna.Framework.Color.White)
        End Select
    
        ' more info
        Select Case Type.Skill(skillNum).Type
            Case SkillType.DamageHp, SkillType.DamageMp, SkillType.HealHp, SkillType.HealMp
                ' damage
                AddDescInfo("Vital: " & Type.Skill(skillNum).Vital, Microsoft.Xna.Framework.Color.White)
            
                ' mp cost
                AddDescInfo("Cost: " & Type.Skill(skillNum).MPCost & " SP", Microsoft.Xna.Framework.Color.White)
            
                ' cast time
                AddDescInfo("Cast Time: " & Type.Skill(skillNum).CastTime & "s", Microsoft.Xna.Framework.Color.White)
            
                ' cd time
                AddDescInfo("Cooldown: " & Type.Skill(skillNum).CDTime & "s", Microsoft.Xna.Framework.Color.White)
            
                ' aoe
                If Type.Skill(skillNum).AoE > 0 Then
                    AddDescInfo("AoE: " & Type.Skill(skillNum).AoE, Microsoft.Xna.Framework.Color.White)
                End If
            
                ' stun
                If Type.Skill(skillNum).StunDuration > 0 Then
                    AddDescInfo("Stun: " & Type.Skill(skillNum).StunDuration & "s", Microsoft.Xna.Framework.Color.White)
                End If
            
                ' dot
                If Type.Skill(skillNum).Duration > 0 And Type.Skill(skillNum).Interval > 0 Then
                    AddDescInfo("DoT: " & (Type.Skill(skillNum).Duration / Type.Skill(skillNum).Interval) & " tick", Microsoft.Xna.Framework.Color.White)
                End If
        End Select
    End Sub

    Public Sub ShowShopDesc(X As Long, Y As Long, ItemNum As Long)
        If ItemNum <= 0 Or ItemNum > MAX_ITEMS Then Exit Sub
        ' show
        ShowItemDesc(X, Y, ItemNum)
    End Sub

    Public Sub ShowEqDesc(x As Long, y As Long, eqNum As Long)
        Dim soulBound As Boolean

        ' rte9
        If eqNum <= 0 Or eqNum > EquipmentType.Count - 1 Then Exit Sub

        ' show
        If Type.Player(GameState.MyIndex).Equipment(eqNum) Then
            If Type.Item(Type.Player(GameState.MyIndex).Equipment(eqNum)).BindType > 0 Then soulBound = 1
            ShowItemDesc(x, y, Type.Player(GameState.MyIndex).Equipment(eqNum))
        End If
    End Sub

    Public Sub AddDescInfo(text As String, color As Microsoft.Xna.Framework.Color)
        Dim count As Long
        count = UBound(GameState.descText)
        ReDim Preserve GameState.descText(count + 1)
        GameState.descText(count + 1).Text = text
        GameState.descText(count + 1).Color = color
    End Sub

    Public Sub LogoutGame()
        GameState.InMenu = True
        GameState.InGame = False

        DestroyNetwork()
        InitNetwork()
    End Sub

    Sub SetOptionsScreen()
        ' Resolutions
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1920x1080")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1680x1050")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1600x900")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1440x900")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1440x1050")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1366x768")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1360x1024")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1360x768")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1280x1024")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1280x800")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1280x768")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1280x720")
        Gui.Combobox_AddItem(Gui.GetWindowIndex("winOptions"), Gui.GetControlIndex("winOptions", "cmbRes"), "1120x864")    

        ' fill the options screen
        With Gui.Windows(Gui.GetWindowIndex("winOptions"))
            .Controls(Gui.GetControlIndex("winOptions", "chkMusic")).Value = Settings.Music
            .Controls(Gui.GetControlIndex("winOptions", "chkSound")).Value = Settings.Sound
            .Controls(Gui.GetControlIndex("winOptions", "chkAutotile")).Value = Settings.Autotile
            .Controls(Gui.GetControlIndex("winOptions", "chkFullscreen")).Value = Settings.Fullscreen
            .Controls(Gui.GetControlIndex("winOptions", "cmbRes")).Value = Settings.Resolution
        End With
    End Sub

    Public Sub OpenShop(shopNum As Long)
        ' set globals
        GameState.InShop = shopNum
        GameState.shopSelectedSlot = 1
        GameState.shopSelectedItem = Type.Shop(GameState.InShop).TradeItem(1).Item
        Gui.Windows(Gui.GetWindowIndex("winShop")).Controls(Gui.GetControlIndex("winShop", "chkSelling")).Value = 0
        Gui.Windows(Gui.GetWindowIndex("winShop")).Controls(Gui.GetControlIndex("winShop", "chkBuying")).Value = 1
        Gui.Windows(Gui.GetWindowIndex("winShop")).Controls(Gui.GetControlIndex("winShop", "btnSell")).Visible = False
        Gui.Windows(Gui.GetWindowIndex("winShop")).Controls(Gui.GetControlIndex("winShop", "btnBuy")).Visible = True
        GameState.shopIsSelling = 0
    
        ' set the current item
        Gui.UpdateShop
    
        ' show the window
        Gui.ShowWindow(Gui.GetWindowIndex("winShop"))
    End Sub

    Public Sub CloseShop()
        SendCloseShop
        Gui.HideWindow(Gui.GetWindowIndex("winShop"))
        GameState.shopSelectedSlot = 0
        GameState.shopSelectedItem = 0
        GameState.shopIsSelling = 0
        GameState.InShop = 0
    End Sub

    Sub UpdatePartyBars()
        Dim i As Long, pIndex As Long, barWidth As Long, Width As Long

        ' unload it if we're not in a party
        If Type.Party.Leader = 0 Then
            Exit Sub
        End If
    
        ' max bar width
        barWidth = 173
    
        ' make sure we're in a party
        With Gui.Windows(Gui.GetWindowIndex("winParty"))
            For i = 1 To 3
                ' get the pIndex from the control
                If .Controls(Gui.GetControlIndex("winParty", "picChar" & i)).Visible = True Then
                    pIndex = .Controls(Gui.GetControlIndex("winParty", "picChar" & i)).value
                    ' make sure they exist
                    If pIndex > 0 Then
                        If IsPlaying(pIndex) Then
                            ' get their health
                            If GetPlayerVital(pIndex, VitalType.HP) > 0 And GetPlayerMaxVital(pIndex, VitalType.HP) > 0 Then
                                Width = ((GetPlayerVital(pIndex, VitalType.HP) / barWidth) / (GetPlayerMaxVital(pIndex, VitalType.HP) / barWidth)) * barWidth
                                .Controls(Gui.GetControlIndex("winParty", "picBar_HP" & i)).Width = Width
                            Else
                                .Controls(Gui.GetControlIndex("winParty", "picBar_HP" & i)).Width = 0
                            End If
                            ' get their spirit
                            If GetPlayerVital(pIndex, VitalType.SP) > 0 And GetPlayerMaxVital(pIndex, VitalType.SP) > 0 Then
                                Width = ((GetPlayerVital(pIndex, VitalType.SP) / barWidth) / (GetPlayerMaxVital(pIndex, VitalType.SP) / barWidth)) * barWidth
                                .Controls(Gui.GetControlIndex("winParty", "picBar_SP" & i)).Width = Width
                            Else
                                .Controls(Gui.GetControlIndex("winParty", "picBar_SP" & i)).Width = 0
                            End If
                        End If
                    End If
                End If
            Next
        End With
    End Sub

    Sub ShowTrade()
        ' show the window
        Gui.ShowWindow(Gui.GetWindowIndex("winTrade"))

        ' set the controls up
        With Gui.Windows(Gui.GetWindowIndex("winTrade"))
            .Text = "Trading with " & GetPlayerName(InTrade)
            .Controls(Gui.GetControlIndex("winTrade", "lblYourTrade")).text = GetPlayerName(GameState.MyIndex) & "'s Offer"
            .Controls(Gui.GetControlIndex("winTrade", "lblTheirTrade")).text = GetPlayerName(InTrade) & "'s Offer"
            .Controls(Gui.GetControlIndex("winTrade", "lblYourValue")).text = "0g"
            .Controls(Gui.GetControlIndex("winTrade", "lblTheirValue")).text = "0g"
            .Controls(Gui.GetControlIndex("winTrade", "lblStatus")).text = "Choose items to offer."
        End With
    End Sub

    Sub ShowPlayerMenu(Index As Long, X As Long, Y As Long)
        GameState.PlayerMenuIndex = Index
        If GameState.PlayerMenuIndex = 0 Or GameState.PlayerMenuIndex = GameState.MyIndex Then Exit Sub
        Gui.Windows(Gui.GetWindowIndex("winPlayerMenu")).Left = X - 5
        Gui.Windows(Gui.GetWindowIndex("winPlayerMenu")).Top = Y - 5
        Gui.Windows(Gui.GetWindowIndex("winPlayerMenu")).Controls(Gui.GetControlIndex("winPlayerMenu", "btnName")).text = GetPlayerName(GameState.PlayerMenuIndex)
        Gui.ShowWindow(Gui.GetWindowIndex("winRightClickBG"))
        Gui.ShowWindow(Gui.GetWindowIndex("winPlayerMenu"))
    End Sub

    Public Sub SetBarWidth(ByRef MaxWidth As Long, ByRef Width As Long)
        Dim barDifference As Long

        If MaxWidth < Width Then
            ' find out the amount to increase per loop
            barDifference = ((Width - MaxWidth) / 100) * 10

            ' if it's less than 1 then default to 1
            If barDifference < 1 Then barDifference = 1
            ' set the width
            Width = Width - barDifference
        ElseIf MaxWidth > Width Then
            ' find out the amount to increase per loop
            barDifference = ((MaxWidth - Width) / 100) * 10

            ' if it's less than 1 then default to 1
            If barDifference < 1 Then barDifference = 1
            ' set the width
            Width = Width + barDifference
        End If
    End Sub

    Public Sub SetGoldLabel()
        Dim i As Long, Amount As Long

        For i = 1 To MAX_INV
            If GetPlayerInv(GameState.MyIndex, i) = 1 Then
                Amount = GetPlayerInvValue(GameState.MyIndex, i)
            End If
        Next
        Gui.Windows(Gui.GetWindowIndex("winShop")).Controls(Gui.GetControlIndex("winShop", "lblGold")).text = Format$(Amount, "#,###,###,###") & "g"
        Gui.Windows(Gui.GetWindowIndex("winInventory")).Controls(Gui.GetControlIndex("winInventory", "lblGold")).text = Format$(Amount, "#,###,###,###") & "g"
    End Sub

    Public Function Clamp(value As Integer, min As Integer, max As Integer) As Integer
        Return If((value < min), min, If((value > max), max, value))
    End Function

    Public Function ConvertMapX(x As Integer) As Integer
        ConvertMapX = x - (GameState.TileView.Left * GameState.PicX) - GameState.Camera.Left
    End Function

    Public Function ConvertMapY(y As Integer) As Integer
        ConvertMapY = y - (GameState.TileView.Top * GameState.PicY) - GameState.Camera.Top
    End Function

     Public Function IsValidMapPoint(x As Integer, y As Integer) As Boolean
        If x < 0 Then Exit Function
        If y < 0 Then Exit Function
        If x > Type.Map(GetPlayerMap(GameState.MyIndex)).MaxX Then Exit Function
        If y > Type.Map(GetPlayerMap(GameState.MyIndex)).MaxY Then Exit Function

        Return True
    End Function

    Public Function GetCellsInSquare(xCenter As Integer, yCenter As Integer, distance As Integer) As List(Of Microsoft.Xna.Framework.Vector2)
        Dim xMin As Integer = Math.Max(0, xCenter - distance)
        Dim xMax As Integer = Math.Min(MyMap.MaxX, xCenter + distance)
        Dim yMin As Integer = Math.Max(0, yCenter - distance)
        Dim yMax As Integer = Math.Min(MyMap.MaxY, yCenter + distance)

        Dim cells = New List(Of Microsoft.Xna.Framework.Vector2)()
        For y As Integer = yMin To yMax
            For x As Integer = xMin To xMax
                cells.Add(New Microsoft.Xna.Framework.Vector2(x, y))
            Next
        Next
        Return cells
    End Function

    Public Function GetBorderCellsInSquare(xCenter As Integer, yCenter As Integer, distance As Integer) _
        As List(Of Microsoft.Xna.Framework.Vector2)
        Dim xMin As Integer = Math.Max(0, xCenter - distance)
        Dim xMax As Integer = Math.Min(MyMap.MaxX, xCenter + distance)
        Dim yMin As Integer = Math.Max(0, yCenter - distance)
        Dim yMax As Integer = Math.Min(MyMap.MaxY, yCenter + distance)

        Dim borderCells = New List(Of  Microsoft.Xna.Framework.Vector2)()

        ' Top and bottom border
        For x As Integer = xMin To xMax
            borderCells.Add(New Microsoft.Xna.Framework.Vector2(x, yMin))
            borderCells.Add(New Microsoft.Xna.Framework.Vector2(x, yMax))
        Next

        ' Left and right border
        For y As Integer = yMin + 1 To yMax - 1
            borderCells.Add(New Microsoft.Xna.Framework.Vector2(xMin, y))
            borderCells.Add(New Microsoft.Xna.Framework.Vector2(xMax, y))
        Next

        borderCells.Remove(New Microsoft.Xna.Framework.Vector2(xCenter, yCenter))
        Return borderCells
    End Function

    Private Function Line(x As Integer, y As Integer, xDestination As Integer, yDestination As Integer) _
        As List(Of Microsoft.Xna.Framework.Vector2)
        Dim discovered = New HashSet(Of Microsoft.Xna.Framework.Vector2)()
        Dim litTiles = New List(Of Microsoft.Xna.Framework.Vector2)()

        Dim dx As Integer = Math.Abs(xDestination - x)
        Dim dy As Integer = Math.Abs(yDestination - y)
        Dim sx As Integer = If(x < xDestination, 1, -1)
        Dim sy As Integer = If(y < yDestination, 1, -1)
        Dim err As Integer = dx - dy

        While True
            Dim pos = New Microsoft.Xna.Framework.Vector2(x, y)
            If discovered.Add(pos) Then litTiles.Add(pos)

            If x = xDestination AndAlso y = yDestination Then Exit While

            Dim e2 As Integer = 2 * err
            If e2 > -dy Then
                err -= dy
                x += sx
            End If
            If e2 < dx Then
                err += dx
                y += sy
            End If
        End While

        Return litTiles
    End Function

      Private Sub PostProcessFovQuadrant(ByRef _inFov As List(Of Microsoft.Xna.Framework.Vector2), x As Integer, y As Integer,
                                       quadrant As QuadrantType)
        Dim x1 As Integer = x
        Dim y1 As Integer = y
        Dim x2 As Integer = x
        Dim y2 As Integer = y
        Dim pos As New Microsoft.Xna.Framework.Vector2(x, y) ' Use Vector2i for integer-based coordinates

        ' Adjust coordinates based on the quadrant
        Select Case quadrant
            Case QuadrantType.NE
                y1 = y + 1
                x2 = x - 1
            Case QuadrantType.SE
                y1 = y - 1
                x2 = x - 1
            Case QuadrantType.SW
                y1 = y - 1
                x2 = x + 1
            Case QuadrantType.NW
                y1 = y + 1
                x2 = x + 1
        End Select

        ' Check if the position is already in the field of view and is not transparent
        If Not _inFov.Contains(pos) AndAlso Not IsTransparent(x, y) Then
            ' Check neighboring cells to determine visibility
            If (IsTransparent(x1, y1) AndAlso _inFov.Contains(New Microsoft.Xna.Framework.Vector2(x1, y1))) OrElse
               (IsTransparent(x2, y2) AndAlso _inFov.Contains(New Microsoft.Xna.Framework.Vector2(x2, y2))) OrElse
               (IsTransparent(x2, y1) AndAlso _inFov.Contains(New Microsoft.Xna.Framework.Vector2(x2, y1))) Then
                _inFov.Add(pos)
            End If
        End If
    End Sub

     Public Function AppendFov(xOrigin As Integer, yOrigin As Integer, radius As Integer, lightWalls As Boolean) _
        As List(Of Microsoft.Xna.Framework.Vector2)
        Dim inFov = New List(Of Microsoft.Xna.Framework.Vector2)()

        ' Get all the border cells in a square around the origin within the given radius
        For Each borderCell As Microsoft.Xna.Framework.Vector2 In GetBorderCellsInSquare(xOrigin, yOrigin, radius)
            ' Trace a line from the origin to the border cell
            For Each cell As Microsoft.Xna.Framework.Vector2 In Line(xOrigin, yOrigin, borderCell.X, borderCell.Y)
                ' Stop if the cell is outside the radius
                If Math.Abs(cell.X - xOrigin) + Math.Abs(cell.Y - yOrigin) > radius Then Exit For

                ' Add the cell to the FOV list if it's transparent or light walls is true
                If IsTransparent(cell.X, cell.Y) Then
                    inFov.Add(cell)
                Else
                    If lightWalls Then inFov.Add(cell)
                    Exit For ' Stop the line if a non-transparent wall is encountered
                End If
            Next
        Next

        ' Optional: Post-process the FOV for specific quadrants
        If lightWalls Then
            For Each cell As Microsoft.Xna.Framework.Vector2 In GetCellsInSquare(xOrigin, yOrigin, radius)
                ' Check the relative position to the origin and post-process based on quadrant
                If cell.X > xOrigin Then
                    If cell.Y > yOrigin Then
                        PostProcessFovQuadrant(inFov, cell.X, cell.Y, QuadrantType.SE)
                    ElseIf cell.Y < yOrigin Then
                        PostProcessFovQuadrant(inFov, cell.X, cell.Y, QuadrantType.NE)
                    End If
                ElseIf cell.X < xOrigin Then
                    If cell.Y > yOrigin Then
                        PostProcessFovQuadrant(inFov, cell.X, cell.Y, QuadrantType.SW)
                    ElseIf cell.Y < yOrigin Then
                        PostProcessFovQuadrant(inFov, cell.X, cell.Y, QuadrantType.NW)
                    End If
                End If
            Next
        End If

        Return inFov
    End Function

    Private Function IsTransparent(x As Integer, y As Integer) As Boolean
      If MyMap.Tile(x, y).Type = TileType.Blocked Or MyMap.Tile(x, y).Type2 = TileType.Blocked Then
          Return False
      End If

      Return True
    End Function

    Friend Sub UpdateCamera()
        Dim lerpSpeed As Double = 0.05 ' Lerp speed for smooth camera movement
        Dim mapMaxWidth As Double = MyMap.MaxX * GameState.TileSize
        Dim mapMaxHeight As Double = MyMap.MaxY * GameState.TileSize

        ' Get player's position in pixels
        Dim playerPosX As Double = GetPlayerX(GameState.MyIndex)
        Dim playerPosY As Double = GetPlayerY(GameState.MyIndex)

        ' Calculate the target camera position to center on the player
        Dim targetX As Double = playerPosX - (GameState.ResolutionWidth / 2)
        Dim targetY As Double = playerPosY - (GameState.ResolutionHeight / 2)

        ' Smoothly interpolate the camera position using Lerp
        GameState.CurrentCameraX = Lerp(GameState.CurrentCameraX, targetX, lerpSpeed)
        GameState.CurrentCameraY = Lerp(GameState.CurrentCameraY, targetY, lerpSpeed)

        ' Clamp the camera position within the map bounds after interpolation
        If GameState.CurrentCameraX < 0 Then
            GameState.CurrentCameraX = 0
        ElseIf GameState.CurrentCameraX + Settings.CameraWidth > mapMaxWidth Then
            GameState.CurrentCameraX = mapMaxWidth - Settings.CameraWidth
        End If

        If GameState.CurrentCameraY < 0 Then
            GameState.CurrentCameraY = 0
        ElseIf GameState.CurrentCameraY + Settings.CameraHeight > mapMaxHeight Then
            GameState.CurrentCameraY = mapMaxHeight - Settings.CameraHeight
        End If

        ' Set the TileView properties based on the clamped camera position
        With GameState.TileView
            .Top = GameState.CurrentCameraY
            .Bottom = GameState.CurrentCameraY + Settings.CameraHeight + 3
            .Left = GameState.CurrentCameraX
            .Right = GameState.CurrentCameraX + Settings.CameraWidth + 3
        End With

        ' Update the Camera properties
        With GameState.Camera
            .Y = GameState.CurrentCameraY
            .X = GameState.CurrentCameraX
            .Height = GameState.ResolutionHeight
            .Width = GameState.ResolutionWidth
        End With

        ' Optional: Update the map name display
        UpdateDrawMapName()
    End Sub

    ' Linear interpolation function to smooth camera movements
    Function Lerp(start As Double, [end] As Double, t As Double) As Double
        Return start + (t * ([end] - start))
    End Function

End Module