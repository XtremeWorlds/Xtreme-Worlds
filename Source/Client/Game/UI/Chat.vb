Imports System.Windows.Forms
Imports Microsoft.Xna.Framework.Audio
Imports Microsoft.Xna.Framework.Input

Public Structure ChatCursor
    Friend X As Integer
    Friend Y As Integer
End Structure

Public Structure ChatData
    Friend Active As Boolean
    Friend HistoryLimit As Integer
    Friend MessageLimit As Integer

    Friend History As List(Of String)
    Friend CachedMessage As String
    Friend CurrentMessage As String
    Friend Cursor As ChatCursor
    
    Friend Function ProcessKey(ByVal keyboardState As KeyboardState) As Boolean
        Dim keys = keyboardState.GetPressedKeys()

        If Not Active Then
            If keys.Contains(Microsoft.Xna.Framework.Input.Keys.Enter) Then
                Active = True
                Return True
            End If

            Return False
        End If

        If CurrentMessage Is Nothing Then CurrentMessage = ""

        For Each key In keys
            Select Case key
                Case Microsoft.Xna.Framework.Input.Keys.Enter
                    History.Add(CurrentMessage)
                    If History.Count > HistoryLimit Then
                        History.RemoveRange(0, History.Count - HistoryLimit)
                    End If
                    Cursor.Y = History.Count
                    Active = False
                    Return True

                Case Microsoft.Xna.Framework.Input.Keys.Back
                    If CurrentMessage.Length > 0 Then
                        CurrentMessage = CurrentMessage.Remove(CurrentMessage.Length - 1)
                    End If

                Case Microsoft.Xna.Framework.Input.Keys.Left
                    Cursor.X = Math.Max(0, Cursor.X - 1)

                Case Microsoft.Xna.Framework.Input.Keys.Right
                    Cursor.X = Math.Min(CurrentMessage.Length, Cursor.X + 1)

                Case Microsoft.Xna.Framework.Input.Keys.Down
                    If History.Count > 0 Then
                        Cursor.Y = Math.Min(History.Count, Cursor.Y + 1)
                        If Cursor.Y = History.Count Then
                            CurrentMessage = CachedMessage
                        Else
                            CurrentMessage = History(Cursor.Y)
                        End If
                    End If

                Case Microsoft.Xna.Framework.Input.Keys.Up
                    If History.Count > 0 Then
                        If Cursor.Y = History.Count Then
                            CachedMessage = CurrentMessage
                        End If

                        Cursor.Y = Math.Max(0, Cursor.Y - 1)
                        CurrentMessage = History(Cursor.Y)
                    End If

                Case Microsoft.Xna.Framework.Input.Keys.V
                    If keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) OrElse keyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl) Then
                        'CurrentMessage &= Clipboard.GetText()
                    End If

                Case Else
                    Dim keyName = [Enum].GetName(GetType(Microsoft.Xna.Framework.Input.Keys), key)
                    If keyName IsNot Nothing AndAlso keyName.Length = 1 Then
                        CurrentMessage &= keyName
                        Cursor.Y = History.Count
                        CachedMessage = CurrentMessage
                    End If
            End Select
        Next

        Return True
    End Function

End Structure

Module ChatModule
    Friend ChatInput As ChatData = New ChatData With {.Active = False, .HistoryLimit = 10, .MessageLimit = 100, .History = New List(Of String)(.HistoryLimit + 1), .CurrentMessage = "", .Cursor = New ChatCursor() With {.X = 0, .Y = 0}}
End Module