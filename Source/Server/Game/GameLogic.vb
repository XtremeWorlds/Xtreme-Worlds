Imports Core

Module GameLogic
    Function GetTotalMapPlayers(MapNum As Integer) As Integer
        Dim i As Integer, n As Integer
        n = 0

        For i = 1 To Socket.HighIndex()
            If GetPlayerMap(i) = mapNum Then
                n = n + 1
            End If
        Next

        GetTotalMapPlayers = n
    End Function

    Function GetNpcMaxVital(NpcNum As Integer, Vital As [Enum].VitalType) As Integer
        ' Prevent subscript out of range
        If NpcNum <= 0 Or NpcNum > MAX_NPCS Then Exit Function

        Select Case Vital
            Case VitalType.HP
                GetNpcMaxVital = Type.NPC(NPCNum).HP
            Case VitalType.SP
                GetNpcMaxVital = Type.NPC(NPCNum).Stat(StatType.Intelligence) * 2
            Case VitalType.SP
                GetNpcMaxVital = Type.NPC(NPCNum).Stat(StatType.Spirit) * 2
        End Select

    End Function

    Function FindPlayer(Name As String) As Integer
        Dim i As Integer

        For i = 1 To Socket.HighIndex()
            ' Trim and convert both names to uppercase for case-insensitive comparison
            If UCase$(GetPlayerName(i)) = UCase$(Name) Then
                FindPlayer = i
                Exit Function
            End If
        Next

        FindPlayer = 0
    End Function

    Friend Function CheckGrammar(Word As String, Optional Caps As Byte = 0) As String
        Dim FirstLetter As String

        FirstLetter = LCase$(Left$(Word, 1))

        If FirstLetter = "$" Then
            CheckGrammar = (Mid$(Word, 2, Len(Word) - 1))
            Exit Function
        End If

        If FirstLetter Like "*[aeiou]*" Then
            If Caps Then CheckGrammar = "An " & Word Else CheckGrammar = "an " & Word
        Else
            If Caps Then CheckGrammar = "A " & Word Else CheckGrammar = "a " & Word
        End If
    End Function

End Module