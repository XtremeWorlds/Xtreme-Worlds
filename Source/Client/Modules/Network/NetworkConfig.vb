Imports Mirage.Sharp.Asfw.Network
Imports Core

Friend Module NetworkConfig
    Friend WithEvents Socket As NetworkClient

    Friend Sub InitNetwork()
        Socket = New NetworkClient(ServerPackets.COUNT, 8192)
        Connect()
        PacketRouter()
    End Sub

    Friend Sub Connect()
        Socket?.Connect(Type.Setting.Ip, Type.Setting.Port)
    End Sub

    Friend Sub DestroyNetwork()
        ' Calling a disconnect is not necessary when using destroy network as
        ' Dispose already calls it and cleans up the memory internally.
        Socket?.Dispose()
    End Sub

#Region " Events "

    Private Sub Socket_ConnectionSuccess() Handles Socket.ConnectionSuccess

    End Sub

    Private Sub Socket_ConnectionFailed() Handles Socket.ConnectionFailed
        DestroyNetwork()
        InitNetwork()
    End Sub

    Private Sub Socket_ConnectionLost() Handles Socket.ConnectionLost
        
    End Sub

    Private Sub Socket_CrashReport(err As String) Handles Socket.CrashReport
        LogoutGame()
        DialogueAlert(DialogueMsg.Crash)

        Dim currentDateTime As DateTime = DateTime.Now
        Dim timestampForFileName As String = currentDateTime.ToString("yyyyMMdd_HHmmss")
        Dim logFileName As String = $"{timestampForFileName}.txt"

        Addlog(err, logFileName)
    End Sub

#If DEBUG Then

    Private Sub Socket_TrafficReceived(size As Integer, ByRef data() As Byte) Handles Socket.TrafficReceived
        Console.WriteLine("Traffic Received : [Size: " & size & "]")
        Dim tmpData = data
#Disable Warning BC42024 ' Unused local variable
        Dim breakPointDummy As Integer
#Enable Warning BC42024 ' Unused local variable
        'Put breakline on BreakPointDummy to look at what is contained in data at runtime in the VS logger.
    End Sub

    Private Sub Socket_PacketReceived(size As Integer, header As Integer, ByRef data() As Byte) Handles Socket.PacketReceived
        Console.WriteLine("Packet Received : [Size: " & size & "| Packet: " & CType(header, ServerPackets).ToString() & "]")
        Dim tmpData = data
#Disable Warning BC42024 ' Unused local variable
        Dim breakPointDummy As Integer
#Enable Warning BC42024 ' Unused local variable
        'Put breakline on BreakPointDummy to look at what is contained in data at runtime in the VS logger.
    End Sub

#End If

#End Region

End Module