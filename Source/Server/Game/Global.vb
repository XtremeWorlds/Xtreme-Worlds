Imports Mirage.Sharp.Asfw.IO.Encryption

Module [Global]
    Public DebugTxt As Boolean
    Public ErrorCount As Integer

    ' Used for closing key doors again
    Public KeyTimer As Integer

    ' Used for gradually giving back npcs hp
    Public GiveNPCHPTimer As Integer

    Public GiveNPCMPTimer As Integer

    Public EKeyPair As New KeyPair()
End Module