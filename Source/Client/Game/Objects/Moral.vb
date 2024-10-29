Imports Core
Imports Mirage.Sharp.Asfw

Friend Module Moral
#Region "Database"

    Friend Sub ClearMoral(index As Integer)
        Type.Moral(index) = Nothing

        Type.Moral(index).Name = ""
        GameState.Moral_Loaded(index) = 0
    End Sub

    Sub ClearMorals()
        Dim i As Integer

        ReDim Type.Moral(MAX_MORALS)

        For i = 1 To MAX_MORALS
            ClearMoral(i)
        Next
    End Sub

    Friend Sub StreamMoral(moralNum As Integer)
        If moralnum > 0 and Type.Moral(moralNum).Name = "" Or GameState.Moral_Loaded(moralNum) = 0 Then
            GameState.Moral_Loaded(moralNum) = 1
            SendRequestMoral(moralNum)
        End If
    End Sub

#End Region

#Region "Incoming Packets"

#End Region
End Module
