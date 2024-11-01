Imports Core
Imports System.IO

Module General
    Public Client As New GameClient()
    Public State As New GameState()
    Public Random As New Random()
    Public Gui As New Gui()

    Friend Function GetTickCount() As Integer
        Return Environment.TickCount
    End Function

    Sub Startup()
        GameState.InMenu = True
        ClearGameData()
        LoadGame()
        GameClient.LoadingCompleted.WaitOne()
        GameLoop()
    End Sub

    Friend Sub LoadGame()
        Settings.Load()
        Languages.Load()
        CheckAnimations()
        CheckCharacters()
        CheckEmotes()
        CheckTilesets()
        CheckFogs()
        CheckItems()
        CheckPanoramas()
        CheckPaperdolls()
        CheckParallax()
        CheckPictures()
        CheckProjectile()
        CheckResources()
        CheckSkills()
        ChecKInterface()
        CheckGradients()
        CheckDesigns()
        InitializeBASS()
        InitNetwork()
        Gui.Init()
        GameState.Ping = -1
    End Sub

    Friend Sub CheckAnimations()
        GameState.NumAnimations = GetFileCount(Core.Path.Animations)
    End Sub

    Friend Sub CheckCharacters()
        GameState.NumCharacters = GetFileCount(Core.Path.Characters)
    End Sub

    Friend Sub CheckEmotes()
        GameState.NumEmotes = GetFileCount(Core.Path.Emotes)
    End Sub

    Friend Sub CheckTilesets()
        GameState.NumTileSets = GetFileCount(Core.Path.Tilesets)
    End Sub

    Friend Sub CheckFogs()
        GameState.NumFogs = GetFileCount(Core.Path.Fogs)
    End Sub

    Friend Sub CheckItems()
        GameState.NumItems = GetFileCount(Core.Path.Items)
    End Sub

    Friend Sub CheckPanoramas()
        GameState.NumPanoramas = GetFileCount(Core.Path.Panoramas)
    End Sub

    Friend Sub CheckPaperdolls()
        GameState.NumPaperdolls = GetFileCount(Core.Path.Paperdolls)
    End Sub

    Friend Sub CheckParallax()
        GameState.NumParallax = GetFileCount(Core.Path.Parallax)
    End Sub

    Friend Sub CheckPictures()
        GameState.NumPictures = GetFileCount(Core.Path.Pictures)
    End Sub

    Friend Sub CheckProjectile()
        GameState.NumProjectiles = GetFileCount(Core.Path.Projectiles)
    End Sub

    Friend Sub CheckResources()
        GameState.NumResources = GetFileCount(Core.Path.Resources)
    End Sub

    Friend Sub CheckSkills()
        GameState.NumSkills = GetFileCount(Core.Path.Skills)
    End Sub

    Friend Sub CheckInterface()
        GameState.NumInterface = GetFileCount(Core.Path.Gui)
    End Sub

    Friend Sub CheckGradients()
        GameState.NumGradients = GetFileCount(Core.Path.Gradients)
    End Sub

    Friend Sub CheckDesigns()
        GameState.NumDesigns = GetFileCount(Core.Path.Designs)
    End Sub

    Function GetResolutionSize(Resolution As Byte, ByRef Width As Integer, ByRef Height As Integer)
        Select Case Resolution
            Case 1
                Width = 1920
                Height = 1080
            Case 2
                Width = 1680
                Height = 1050
            Case 3
                Width = 1600
                Height = 900
            Case 4
                Width = 1440
                Height = 900
            Case 5
                Width = 1440
                Height = 1050
            Case 6
                Width = 1366
                Height = 768
            Case 7
                Width = 1360
                Height = 1024
            Case 8
                Width = 1360
                Height = 768
            Case 9
                Width = 1280
                Height = 1024
            Case 10
                Width = 1280
                Height = 800
            Case 11
                Width = 1280
                Height = 768
            Case 12
                Width = 1280
                Height = 720
            Case 13
                Width = 1120
                Height = 864
            Case 14      
                Width = 1024
                Height = 768
        End Select
    End Function

    Friend Sub ClearGameData()
        ClearMap()
        ClearMapNPCs()
        ClearMapItems()
        ClearNpcs()
        ClearResources()
        ClearItems()
        ClearShops()
        ClearSkills()
        ClearAnimations()
        ClearProjectile()
        ClearPets()
        ClearJobs()
        ClearMorals()
        ClearBanks()
        ClearParty()

        For i = 1 To MAX_PLAYERS
            ClearPlayer(i)
        Next

        ClearAnimInstances()
        ClearAutotiles()

        ' clear chat
        For i = 1 To CHAT_LINES
            Chat(i).text = ""
        Next
    End Sub

    Friend Function GetFileCount(folderName As String) As Integer
        Dim folderPath As String = IO.Path.Combine(Core.Path.Graphics, folderName)
        If Directory.Exists(folderPath) Then
            Return Directory.GetFiles(folderPath, "*.png").Length ' Adjust for other formats if needed
        Else
            Console.WriteLine($"Folder not found: {folderPath}")
            Return 0
        End If
    End Function

    Friend Sub CacheMusic()
        ReDim MusicCache(Directory.GetFiles(Core.Path.Music, "*" & Settings.MusicExt).Count)
        Dim files As String() = Directory.GetFiles(Core.Path.Music, "*" & Settings.MusicExt)
        Dim maxNum As String = Directory.GetFiles(Core.Path.Music, "*" & Settings.MusicExt).Count
        Dim counter As Integer = 0

        For Each FileName In files
            counter = counter + 1
            ReDim Preserve MusicCache(counter)

            MusicCache(counter) = IO.Path.GetFileName(FileName)
        Next
    End Sub

    Friend Sub CacheSound()
        ReDim SoundCache(Directory.GetFiles(Core.Path.Sounds, "*" & Settings.SoundExt).Count)
        Dim files As String() = Directory.GetFiles(Core.Path.Sounds, "*" & Settings.SoundExt)
        Dim maxNum As String = Directory.GetFiles(Core.Path.Sounds,  "*" & Settings.SoundExt).Count
        Dim counter As Integer = 0

        For Each FileName In files
            counter = counter + 1
            ReDim Preserve SoundCache(counter)

            SoundCache(counter) = IO.Path.GetFileName(FileName)
        Next
    End Sub

    Sub GameInit()
        ' Send a request to the server to open the admin menu if the user wants it.
        If Settings.OpenAdminPanelOnLogin = 1 Then
            If GetPlayerAccess(GameState.MyIndex) > 0 Then
                SendRequestAdmin()
            End If
        End If
    End Sub

    Friend Sub DestroyGame()
        ' break out of GameLoop
        GameState.InGame = False
        GameState.InMenu = False
        FreeBASS
        End
    End Sub

    ' Get the shifted version of a digit key (for symbols)
    Public Function GetShiftedDigit(digit As Char) As Char
        Select Case digit
            Case "1"c : Return "!"c
            Case "2"c : Return "@"c
            Case "3"c : Return "#"c
            Case "4"c : Return "$"c
            Case "5"c : Return "%"c
            Case "6"c : Return "^"c
            Case "7"c : Return "&"c
            Case "8"c : Return "*"c
            Case "9"c : Return "("c
            Case "0"c : Return ")"c
            Case Else : Return digit
        End Select
    End Function
    
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

End Module