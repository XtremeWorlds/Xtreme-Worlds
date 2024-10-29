Imports Core
Imports Mirage.Sharp.Asfw.Network

Friend Module S_NetworkConfig
    Friend WithEvents Socket As NetworkServer

    Friend Sub InitNetwork()
        If Not Socket Is Nothing Then Return
        ' Establish some Rulez
        Socket = New NetworkServer(Packets.ClientPackets.Count, 8192, MAX_PLAYERS) With {
            .BufferLimit = 2048000, ' <- this is 2mb max data storage
            .MinimumIndex = 1, ' <- this prevents the network from giving us 0 as an index
            .PacketAcceptLimit = 500, ' Dunno what is a reasonable cap right now so why not? :P
            .PacketDisconnectCount = 100 ' If the other thing was even remotely reasonable, this is DEFINITELY spam count!
            }
        ' END THE ESTABLISHMENT! WOOH ANARCHY! ~SpiceyWolf

        PacketRouter() ' Need them packet ids boah!
    End Sub

    Friend Sub DestroyNetwork()
        Socket.Dispose()
    End Sub

    Function IsLoggedIn(index As Integer) As Boolean
        Return Type.Account(index).Login.Length > 0
    End Function

    Function IsPlaying(index As Integer) As Boolean
        Return TempPlayer(index).InGame
    End Function

    Function IsMultiAccounts(Index As Integer, Login As String) As Boolean
        For i = 1 To Socket.HighIndex()
            If i <> Index then
                If Type.Account(i).Login.ToLower() Then
                    Return True
                End If
            End If
        Next
        Return False
    End Function

    Friend Sub SendDataToAll(ByRef data() As Byte, head As Integer)
        For i = 1 To Socket.HighIndex()
            Socket.SendDataTo(i, data, head)
        Next
    End Sub

    Sub SendDataToAllBut(index As Integer, ByRef data() As Byte, head As Integer)
        For i = 1 To Socket.HighIndex()
            If i <> index Then
                Socket.SendDataTo(i, data, head)
            End If
        Next
    End Sub

    Sub SendDataToMapBut(index As Integer, mapNum As Integer, ByRef data() As Byte, head As Integer)
        For i = 1 To Socket.HighIndex()
            If GetPlayerMap(i) = mapNum And i <> index Then
                Socket.SendDataTo(i, data, head)
            End If
        Next
    End Sub

    Sub SendDataToMap(MapNum As Integer, ByRef data() As Byte, head As Integer)
        Dim i As Integer

       For i = 1 To Socket.HighIndex()

            If GetPlayerMap(i) = mapNum Then
                Socket.SendDataTo(i, data, head)
            End If

        Next

    End Sub

    Sub SendDataTo(index As Integer, ByRef data() As Byte, head As Integer)
        Socket.SendDataTo(index, data, head)
    End Sub

#Region "Events"

    Friend Sub Socket_ConnectionReceived(index As Integer) Handles Socket.ConnectionReceived
        Call Global.System.Console.WriteLine("Connection received on index[" & index & "] - IP[" & Socket.ClientIp(index) & "]")
        SendKeyPair(index)
    End Sub

    Friend Sub Socket_ConnectionLost(index As Integer) Handles Socket.ConnectionLost
        Call Global.System.Console.WriteLine("Connection lost on index [" & index & "] - IP[" & Socket.ClientIp(index) & "]")
        LeftGame(index)
    End Sub

    Friend Sub Socket_CrashReport(index As Integer, err As String) Handles Socket.CrashReport
        Call Global.System.Console.WriteLine("There was a network error index [" & index & "]")
        Call Global.System.Console.WriteLine("Report: " & err)
    End Sub

    Private Sub Socket_TrafficReceived(size As Integer, ByRef data() As Byte) Handles Socket.TrafficReceived
        If DebugTxt = 1 Then
            Call Global.System.Console.WriteLine("Traffic Received: [Size: " & size & "]")
        End If
    End Sub

    Private Sub Socket_PacketReceived(size As Integer, header As Integer, ByRef data() As Byte) Handles Socket.PacketReceived
        If DebugTxt = 1 Then
            Call Global.System.Console.WriteLine("Packet Received: [Size: " & size & "| Packet: " & CType(header, ClientPackets).ToString() & "]")
        End If
    End Sub

#End Region

End Module