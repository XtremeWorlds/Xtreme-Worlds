﻿Imports Mirage.Core.Database.DbContexts
Imports Mirage.Sharp.Asfw.IO.Encryption

Public Module S_Globals
    ' DbContext reference, do not use this on the client it is for the server.
    Public DatabaseContext As MirageDbContext

    Public DebugTxt As Boolean
    Public ErrorCount As Integer

    ' Used for closing key doors again
    Public KeyTimer As Integer

    ' Used for gradually giving back npcs hp
    Public GiveNPCHPTimer As Integer

    Public GiveNPCMPTimer As Integer

    Public EKeyPair As New KeyPair()
End Module