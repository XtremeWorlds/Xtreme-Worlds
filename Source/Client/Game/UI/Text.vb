Imports System.Text
Imports Core
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics

Module Text
    Public Fonts As New Dictionary(Of FontType, SpriteFont)

    Friend Const MaxChatDisplayLines As Byte = 11
    Friend Const ChatLineSpacing As Byte = 10 ' Should be same height as font
    Friend Const MyChatTextLimit As Integer = 40
    Friend Const MyAmountValueLimit As Integer = 3
    Friend Const AllChatLineWidth As Integer = 40
    Friend Const ChatboxPadding As Integer = 45 + 16 + 2 ' 10 = left and right border padding +2 each (3+2+3+2), 16 = scrollbar width, +2 for padding between scrollbar and text
    Friend Const ChatEntryPadding As Integer = 10 ' 5 on left and right
    Friend FirstLineindex As Integer = 0
    Friend LastLineindex As Integer = 0
    Friend ScrollMod As Integer = 0

    Public Function CensorText(input As String) As String
        Return New String("*"c, input.Length)
    End Function

    Public Function SanitizeText(text As String, font As SpriteFont) As String
        Dim sanitizedText As New StringBuilder()
        For Each ch As Char In text
            If font.Characters.Contains(ch) Then
                sanitizedText.Append(ch)
            Else
                sanitizedText.Append("?"c) ' Replace unsupported characters with a placeholder
            End If
        Next
        Return sanitizedText.ToString()
    End Function

    ' Get the width of the text with optional scaling
    Public Function TextWidth(text As String, Optional font As FontType = FontType.Georgia, Optional textSize As Single = 1.0F) As Integer
        If Not Fonts.ContainsKey(font) Then Throw New ArgumentException("Font not found.")
        Dim sanitizedText = SanitizeText(text, Fonts(font))
        Dim textDimensions = Fonts(font).MeasureString(sanitizedText)
        Return CInt(textDimensions.X * textSize)
    End Function

    ' Get the height of the text with optional scaling
    Public Function TextHeight(text As String, Optional font As FontType = FontType.Georgia, Optional textSize As Single = 1.0F) As Integer
        If Not Fonts.ContainsKey(font) Then Throw New ArgumentException("Font not found.")
        Dim textDimensions = Fonts(font).MeasureString(text)
        Return CInt(textDimensions.Y * textSize)
    End Function

    Public Sub AddText(ByVal text As String, ByVal Color As Integer, Optional ByVal alpha As Long = 255, Optional channel As Byte = 1)
        Dim i As Long

        ' Move the rest of it up
        For i = (CHAT_LINES - 1) To 1 Step -1
            If Len(Chat(i).Text) > 0 Then
                If i > GameState.Chat_HighIndex Then GameState.Chat_HighIndex = i + 1
            End If
            Chat(i + 1) = Chat(i)
        Next

        Chat(1).Text = text
        Chat(1).Color = Color
        Chat(1).Visible = True
        Chat(1).Timer = GetTickCount()
        Chat(1).Channel = channel
    End Sub

    Public Sub WordWrap(ByVal text As String, ByVal font As FontType, ByVal MaxLineLen As Long, ByRef theArray() As String)
        Dim lineCount As Long, i As Long, size As Long, lastSpace As Long, b As Long, tmpNum As Long

        'Too small of text
        If Len(text) < 2 Then
            ReDim theArray(1)
            theArray(1) = text
            Exit Sub
        End If

        ' default values
        b = 1
        lastSpace = 1
        size = 0
        tmpNum = Len(text)

        For i = 1 To tmpNum

            ' if it's a space, store it
            Select Case Mid$(text, i, 1)
                Case " " : lastSpace = i
            End Select

            'Add up the size
            size = size + 10

            'Check for too large of a size
            If size > MaxLineLen Then
                'Check if the last space was too far back
                If i - lastSpace > 10 Then
                    'Too far away to the last space, so break at the last character
                    lineCount = lineCount + 1
                    ReDim Preserve theArray(lineCount)
                    theArray(lineCount) = Mid$(text, b, (i - 1) - b)
                    b = i - 1
                    size = 0
                Else
                    'Break at the last space to preserve the word
                    lineCount = lineCount + 1
                    ReDim Preserve theArray(lineCount)

                    ' Ensure b is within valid range
                    If b < 1 Then b = 1
                    If b > text.Length Then b = text.Length

                    ' Ensure the length parameter is not negative
                    Dim substringLength As Integer = lastSpace - b
                    If substringLength < 0 Then substringLength = 0

                    ' Extract the substring and assign it to the array
                    theArray(lineCount) = Mid$(text, b, substringLength)

                    b = lastSpace + 1
                    'Count all the words we ignored (the ones that weren't printed, but are before "i")
                    size = TextWidth(Mid$(text, lastSpace, i - lastSpace), font)
                End If
            End If

            ' Remainder
            If i = Len(text) Then
                If b <> i Then
                    lineCount = lineCount + 1
                    ReDim Preserve theArray(lineCount)
                    theArray(lineCount) = theArray(lineCount) & Mid$(text, b, i)
                End If
            End If
        Next
    End Sub

    Public Function WordWrap(ByVal text As String, ByVal font As FontType, ByVal MaxLineLen As Integer, Optional ByRef lineCount As Long = 0) As String
        Dim TempSplit() As String, TSLoop As Long, lastSpace As Long, size As Long, i As Long, b As Long, tmpNum As Long, skipCount As Long

        'Too small of text
        If Len(text) < 2 Then
            WordWrap = text
            Exit Function
        End If

        'Check if there are any line breaks - if so, we will support them
        TempSplit = Split(text, vbNewLine)
        tmpNum = UBound(TempSplit)

        For TSLoop = 0 To tmpNum
            'Clear the values for the new line
            size = 0
            b = 1
            lastSpace = 1

            'Add back in the vbNewLines
            If TSLoop < UBound(TempSplit) Then TempSplit(TSLoop) = TempSplit(TSLoop) & vbNewLine

            'Only check lines with a space
            If InStr(1, TempSplit(TSLoop), " ") Then
                'Loop through all the characters
                tmpNum = Len(TempSplit(TSLoop))

                For i = 1 To tmpNum
                    'If it is a space, store it so we can easily break at it
                    Select Case Mid$(TempSplit(TSLoop), i, 1)
                        Case " "
                            lastSpace = i
                    End Select

                    If skipCount > 0 Then
                        skipCount = skipCount - 1
                    ElseIf TSLoop > 0 Then
                        'Add up the size
                        size = size + TextWidth(TempSplit(TSLoop), font)

                        'Check for too large of a size
                        If size > MaxLineLen Then
                            'Check if the last space was too far back
                            If i - lastSpace > 12 Then
                                'Too far away to the last space, so break at the last character
                                WordWrap = WordWrap & Mid$(TempSplit(TSLoop), b, (i - 1) - b) & vbNewLine
                                lineCount = lineCount + 1
                                b = i - 1
                                size = 0
                            Else
                                'Break at the last space to preserve the word
                                WordWrap = WordWrap & Mid$(TempSplit(TSLoop), b, lastSpace - b) & vbNewLine
                                lineCount = lineCount + 1
                                b = lastSpace + 1

                                'Count all the words we ignored (the ones that weren't printed, but are before "i")
                                size = TextWidth(Mid$(TempSplit(TSLoop), lastSpace, i - lastSpace), font)
                            End If
                        End If

                        'This handles the remainder
                        If i = Len(TempSplit(TSLoop)) Then
                            If b <> i Then
                                WordWrap = WordWrap & Mid$(TempSplit(TSLoop), b, i)
                                lineCount = lineCount + 1
                            End If
                        End If
                    End If
                Next i
            Else
                WordWrap = WordWrap & TempSplit(TSLoop)
            End If
        Next TSLoop
    End Function

    Public Function Explode(str As String, splitChars As Char()) As String()
        Dim parts As New List(Of String)()
        Dim startindex As Integer = 0

        Explode = Nothing

        If str = Nothing Then Exit Function

        While True
            Dim index As Integer = str.IndexOfAny(splitChars, startindex)

            If index = -1 Then
                parts.Add(str.Substring(startindex))
                Return parts.ToArray()
            End If

            Dim word As String = str.Substring(startindex, index - startindex)
            Dim nextChar As Char = str.Substring(index, 1)(0)
            ' Dashes and the likes should stick to the word occuring before it. Whitespace doesn't have to.
            If Char.IsWhiteSpace(nextChar) Then
                parts.Add(word)
                parts.Add(nextChar.ToString())
            Else
                parts.Add(word + nextChar)
            End If

            startindex = index + 1
        End While

    End Function

    Public Sub RenderText(text As String, x As Integer, y As Integer,
                               frontColor As Color, backColor As Color, Optional font As FontType = FontType.Georgia)

        GameClient.SpriteBatch.DrawString(Fonts(font), text, New Vector2(x + 1, y + 1), backColor,
                               0.0F, Vector2.Zero, 12 / 16.0F, SpriteEffects.None, 0.0F)
        GameClient.SpriteBatch.DrawString(Fonts(font), text, New Vector2(x, y), frontColor,
                               0.0F, Vector2.Zero, 12 / 16.0F, SpriteEffects.None, 0.0F)
    End Sub

    Sub DrawNPCName(MapNpcNum As Integer)
        Dim textX As Integer
        Dim textY As Integer
        Dim color As Color, backColor As Color
        Dim npcNum As Integer

        npcNum = MyMapNPC(MapNpcNum).Num

        Select Case Type.NPC(npcNum).Behaviour
            Case 0 ' attack on sight
                color = Color.Red
                backColor = Color.Black
            Case 1, 4 ' attack when attacked + guard
                color = Color.Green
                backColor = Color.Black
            Case 2, 3, 5 ' friendly + shopkeeper + quest
                color = Color.Yellow
                backColor = Color.Black
        End Select
        textX = ConvertMapX(MyMapNPC(MapNpcNum).X * GameState.PicX) + MyMapNPC(MapNpcNum).XOffset + (GameState.PicX \ 2) - (TextWidth((Type.NPC(npcNum).Name))) / 2 - 2
        If Type.NPC(npcNum).Sprite < 1 Or Type.NPC(npcNum).Sprite > GameState.NumCharacters Then
            textY = ConvertMapY(MyMapNPC(MapNpcNum).Y * GameState.PicY) + MyMapNPC(MapNpcNum).YOffset - 16
        Else
            textY = ConvertMapY(MyMapNPC(MapNpcNum).Y * GameState.PicY) + MyMapNPC(MapNpcNum).YOffset - (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, Type.NPC(npcNum).Sprite)).Height / 4) + 16
        End If

        ' Draw name
        RenderText(Type.NPC(npcNum).Name, textX, textY, color, backColor)
    End Sub

    Sub DrawEventName(index As Integer)
        Dim textX As Integer
        Dim textY As Integer
        Dim color As Color, backcolor As Color
        Dim name As String

        color = Color.Yellow
        backcolor = Color.Black

        name = MapEvents(index).Name

        ' calc pos
        textX = ConvertMapX(MapEvents(index).X * GameState.PicX) + MapEvents(index).XOffset + (GameState.PicX \ 2) - (TextWidth(name)) \ 2 - 2
        If MapEvents(index).GraphicType = 0 Then
            textY = ConvertMapY(MapEvents(index).Y * GameState.PicY) + MapEvents(index).YOffset - 16
        ElseIf MapEvents(index).GraphicType = 1 Then
            If MapEvents(index).Graphic < 1 Or MapEvents(index).Graphic > GameState.NumCharacters Then
                textY = ConvertMapY(MapEvents(index).Y * GameState.PicY) + MapEvents(index).YOffset - 16
            Else
                ' Determine location for text
                textY = ConvertMapY(MapEvents(index).Y * GameState.PicY) + MapEvents(index).YOffset - (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, MapEvents(index).Graphic)).Height \ 4) + 16
            End If
        ElseIf MapEvents(index).GraphicType = 2 Then
            If MapEvents(index).GraphicY2 > 0 Then
                textX = textX + (MapEvents(index).GraphicY2 * GameState.PicY) \ 2 - 16
                textY = ConvertMapY(MapEvents(index).Y * GameState.PicY) + MapEvents(index).YOffset - (MapEvents(index).GraphicY2 * GameState.PicY) + 16
            Else
                textY = ConvertMapY(MapEvents(index).Y * GameState.PicY) + MapEvents(index).YOffset - 32 + 16
            End If
        End If

        ' Draw name
        RenderText(name, textX, textY, color, backcolor)
    End Sub
    
    Sub DrawActionMsg(index As Integer)
        Dim x As Integer, y As Integer, i As Integer, time As Integer

        ' how long we want each message to appear
        Select Case ActionMsg(index).Type
            Case ActionMsgType.Static
                time = 1500

                If ActionMsg(index).Y > 0 Then
                    x = ActionMsg(index).X + Int(GameState.PicX \ 2) - ((Len(ActionMsg(index).Message)) \ 2) * 8
                    y = ActionMsg(index).Y - Int(GameState.PicY \ 2) - 2
                Else
                    x = ActionMsg(index).X + Int(GameState.PicX \ 2) - ((Len(ActionMsg(index).Message)) \ 2) * 8
                    y = ActionMsg(index).Y - Int(GameState.PicY \ 2) + 18
                End If

            Case ActionMsgType.Scroll
                time = 1500

                If ActionMsg(index).Y > 0 Then
                    x = ActionMsg(index).X + Int(GameState.PicX \ 2) - ((Len(ActionMsg(index).Message)) \ 2) * 8
                    y = ActionMsg(index).Y - Int(GameState.PicY \ 2) - 2 - (ActionMsg(index).Scroll * 0.6)
                    ActionMsg(index).Scroll = ActionMsg(index).Scroll + 1
                Else
                    x = ActionMsg(index).X + Int(GameState.PicX \ 2) - ((Len(ActionMsg(index).Message)) \ 2) * 8
                    y = ActionMsg(index).Y - Int(GameState.PicY \ 2) + 18 + (ActionMsg(index).Scroll * 0.6)
                    ActionMsg(index).Scroll = ActionMsg(index).Scroll + 1
                End If

            Case ActionMsgType.Screen
                time = 3000

                ' This will kill any action screen messages that there in the system
                For i = Byte.MaxValue To 1 Step -1
                    If ActionMsg(i).Type = ActionMsgType.Screen Then
                        If i <> index Then
                            ClearActionMsg(index)
                            index = i
                        End If
                    End If
                Next
                x = (GameState.ResolutionWidth \ 2) - ((Len(ActionMsg(index).Message)) \ 2) * 8
                y = 425

        End Select

        x = ConvertMapX(x)
        y = ConvertMapY(y)

        If GetTickCount() < ActionMsg(index).Created + time Then
            RenderText(ActionMsg(index).Message, x, y, GameClient.QbColorToXnaColor(ActionMsg(index).Color), (Color.Black))
        Else
            ClearActionMsg(index)
        End If

    End Sub

    Sub DrawChat()
        Dim xO As Long, yO As Long, Color As Integer, yOffset As Long, rLines As Integer, lineCount As Integer
        Dim tmpText As String, i As Long, isVisible As Boolean, topWidth As Integer, tmpArray() As String, x As Integer
        Dim Color2 As Color

        ' set the position
        xO = 19
        yO = GameState.ResolutionHeight - 40

        ' loop through chat
        rLines = 1
        i = 1 + GameState.ChatScroll

        Do While rLines <= 8
            If i > CHAT_LINES Then Exit Do
            lineCount = 0

            ' exit out early if we come to a blank string
            If Len(Chat(i).Text) = 0 Then Exit Do

            ' get visible state
            isVisible = True
            If GameState.inSmallChat = True Then
                If Not Chat(i).Visible = True Then isVisible = False
            End If

            If Settings.ChannelState(Type.Chat(i).Channel) = 0 Then isVisible = False

            ' make sure it's visible
            If isVisible = True
                ' render line
                Color = Chat(i).Color
                Color2 = GameClient.QbColorToXnaColor(Color)

                ' check if we need to word wrap
                If TextWidth(Chat(i).Text) > GameState.ChatWidth Then
                    ' word wrap
                    tmpText = WordWrap(Chat(i).Text, FontType.Georgia, GameState.ChatWidth)

                    ' can't have it going offscreen.
                    If rLines + lineCount > 9 Then Exit Do

                    ' continue on
                    yOffset = yOffset - (14 * lineCount)
                    RenderText(tmpText, xO, yO + yOffset, Color2, Color2)
                    rLines = rLines + lineCount

                    ' set the top width
                    tmpArray = Split(tmpText, vbNewLine)
                    For x = 0 To UBound(tmpArray)
                        If TextWidth(tmpArray(x)) > topWidth Then topWidth = TextWidth(tmpArray(x))
                    Next
                Else
                    ' normal
                    yOffset = yOffset - 14

                    RenderText(Chat(i).Text, xO, yO + yOffset, Color2, Color2)
                    rLines = rLines + 1

                    ' set the top width
                    If TextWidth(Chat(i).Text) > topWidth Then topWidth = TextWidth(Chat(i).Text)
                End If
            End If
            ' increment chat pointer
            i = i + 1
        Loop

        ' get the height of the small chat box
        SetChatHeight(rLines * 14)
        SetChatWidth(topWidth)
    End Sub

    Sub DrawMapName()
        RenderText(Language.Game.MapName & MyMap.Name, GameState.ResolutionWidth / 2 - TextWidth(MyMap.Name), 10, GameState.DrawMapNameColor, Color.Black)
    End Sub

    Sub DrawPlayerName(index As Integer)
        Dim textX As Integer
        Dim textY As Integer
        Dim color As Color, backColor As Color
        Dim name As String

        ' Check access level
        If GetPlayerPK(index) = 0 Then
            Select Case GetPlayerAccess(index)
                Case AccessType.Player
                    color = Color.White
                    backColor = Color.Black
                Case AccessType.Moderator
                    color = Color.Cyan
                    backColor = Color.White
                Case AccessType.Mapper
                    color = Color.Green
                    backColor = Color.Black
                Case AccessType.Developer
                    color = Color.Blue
                    backColor = Color.Black
                Case AccessType.Owner
                    color = Color.Yellow
                    backColor = Color.Black
            End Select
        Else
            color = Color.Red
        End If

        name = Type.Player(index).Name

        ' calc pos
        textX = ConvertMapX(GetPlayerX(index) * GameState.PicX) + Type.Player(index).XOffset + (GameState.PicX \ 2) - 2
        textX = textX - (TextWidth((name)) / 2)

        If GetPlayerSprite(index) < 0 Or GetPlayerSprite(index) > GameState.NumCharacters Then
            textY = ConvertMapY(GetPlayerY(index) * GameState.PicY) + Type.Player(GameState.MyIndex).YOffset - 16
        Else
            ' Determine location for text
            textY = ConvertMapY(GetPlayerY(index) * GameState.PicY) + Type.Player(index).YOffset - (GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Characters, GetPlayerSprite(index))).Height / 4) + 16
        End If

        ' Draw name
        RenderText(name, textX, textY, color, backColor)
    End Sub

End Module