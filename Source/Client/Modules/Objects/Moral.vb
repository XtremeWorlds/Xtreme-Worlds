Imports Core
Imports Mirage.Sharp.Asfw

Friend Module Moral
#Region "Database"

    Friend Sub ClearMoral(index As Integer)
        Type.Moral(index) = Nothing

        Type.Moral(index).Name = ""
        Moral_Loaded(index) = False
    End Sub

    Sub ClearMorals()
        Dim i As Integer

        ReDim Type.Moral(MAX_MORALS)

        For i = 1 To MAX_MORALS
            ClearMoral(i)
        Next
    End Sub

    Friend Sub StreamMoral(moralNum As Integer)
        If moralnum > 0 and Type.Moral(moralNum).Name = "" Or Moral_Loaded(moralNum) = False Then
            Moral_Loaded(moralNum) = True
            SendRequestMoral(moralNum)
        End If
    End Sub

#End Region

#Region "Incoming Packets"

#End Region
End Module
