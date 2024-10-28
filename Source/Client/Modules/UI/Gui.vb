Imports System.Drawing
Imports System.Windows.Forms
Imports Mirage.Sharp.Asfw
Imports Core

Friend Module Gui

#Region "Support Functions"

   Public Function IsEq(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To EquipmentType.Count - 1
            If GetPlayerEquipment(GameState.MyIndex, i) Then
                With tempRec
                .Top = StartY + GameState.EqTop + (GameState.PicY * ((i - 1) \ GameState.EqColumns))
                .bottom = .Top + GameState.PicY
                .Left = StartX + GameState.EqLeft + ((GameState.EqOffsetX + GameState.PicX) * (((i - 1) Mod GameState.EqColumns)))
                .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsEq = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Public Function IsInv(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_INV
            If GetPlayerInv(GameState.MyIndex, i) > 0 Then
                With tempRec
                    .Top = StartY + GameState.InvTop + ((GameState.InvOffsetY + GameState.PicY) * ((i - 1) \ GameState.InvColumns))
                    .bottom = .Top + GameState.PicY
                    .Left = StartX + GameState.InvLeft + ((GameState.InvOffsetX + GameState.PicX) * (((i - 1) Mod GameState.InvColumns)))
                    .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsInv = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Public Function IsSkill(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_PLAYER_SKILLS
            If Type.Player(GameState.MyIndex).Skill(i).Num Then
                With tempRec
                    .Top = StartY + GameState.SkillTop + ((GameState.SkillOffsetY + GameState.PicY) * ((i - 1) \ GameState.SkillColumns))
                    .bottom = .Top + GameState.PicY
                    .Left = StartX + GameState.SkillLeft + ((GameState.SkillOffsetX + GameState.PicX) * (((i - 1) Mod GameState.SkillColumns)))
                    .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsSkill = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Public Function IsBank(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_BANK
            If GetBank(GameState.MyIndex, i) > 0 Then
                With tempRec
                    .Top = StartY + GameState.BankTop + ((GameState.BankOffsetY + GameState.PicY) * ((i - 1) \ GameState.BankColumns))
                    .bottom = .Top + GameState.PicY
                    .Left = StartX + GameState.BankLeft + ((GameState.BankOffsetX + GameState.PicX) * (((i - 1) Mod GameState.BankColumns)))
                    .Right = .Left + GameState.PicX
                End With

                If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                    If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                        IsBank = i
                        Exit Function
                    End If
                End If
            End If
        
        Next

    End Function

  Public Function IsShop(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_TRADES
            With tempRec
                .Top = StartY + GameState.ShopTop + ((GameState.ShopOffsetY + GameState.PicY) * ((i - 1) \ GameState.ShopColumns))
                .bottom = .Top + GameState.PicY
                .Left = StartX + GameState.ShopLeft + ((GameState.ShopOffsetX + GameState.PicX) * (((i - 1) Mod GameState.ShopColumns)))
                .Right = .Left + GameState.PicX
            End With

            If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                    IsShop = i
                    Exit Function
                End If
            End If
        Next
    End Function

    Public Function IsTrade(StartX As Long, StartY As Long) As Long
        Dim tempRec As RectStruct
        Dim i As Long

        For i = 1 To MAX_INV
            With tempRec
                .Top = StartY + GameState.TradeTop + ((GameState.TradeOffsetY + GameState.PicY) * ((i - 1) \ GameState.TradeColumns))
                .bottom = .Top + GameState.PicY
                .Left = StartX + GameState.TradeLeft + ((GameState.TradeOffsetX + GameState.PicX) * (((i - 1) Mod GameState.TradeColumns)))
                .Right = .Left + GameState.PicX
            End With

            If GameState.CurMouseX >= tempRec.Left And GameState.CurMouseX <= tempRec.Right Then
                If GameState.CurMouseY >= tempRec.Top And GameState.CurMouseY <= tempRec.bottom Then
                    IsTrade = i
                    Exit Function
                End If
            End If
        Next
    End Function

#End Region

End Module