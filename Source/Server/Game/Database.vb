Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Core
Imports Type
Imports Mirage.Sharp.Asfw
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Npgsql
Imports NpgsqlType
Imports NpgsqlTypes
Imports Path = System.IO.Path

Module Database
    Dim connectionString As String = Configuration.GetValue("Database:ConnectionString", "Host=localhost;Port=5432;Username=postgres;Password=mirage;Database=mirage")

    Public Sub ExecuteSql(connectionString As String, sql As String)
        Using connection As New NpgsqlConnection(connectionString)
            connection?.Open()

            Using command As New NpgsqlCommand(sql, connection)
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Function DatabaseExists(databaseName As String) As Boolean
        Try
            Dim sql As String = "SELECT 1 FROM pg_database WHERE datname = @databaseName;"

            Using connection As New NpgsqlConnection(connectionString)
                connection.Open()
                Using command As New NpgsqlCommand(sql, connection)
                    command.Parameters.AddWithValue("@databaseName", databaseName)

                    Using reader As NpgsqlDataReader = command.ExecuteReader()
                        If reader.Read() Then
                            Return True
                        Else
                            Return False
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Sub CreateDatabase(databaseName As String)
        Dim checkDbExistsSql As String = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'"
        Dim createDbSql As String = $"CREATE DATABASE {databaseName}"

        Using connection As New NpgsqlConnection(connectionString.Substring(0, connectionString.LastIndexOf(";"c)))
            connection.Open()

            Using checkCommand As New NpgsqlCommand(checkDbExistsSql, connection)
                Dim dbExists As Boolean = checkCommand.ExecuteScalar() IsNot Nothing

                If Not dbExists Then
                    Using createCommand As New NpgsqlCommand(createDbSql, connection)
                        Dim discard = createCommand.ExecuteNonQuery()

                        Using dbConnection As New NpgsqlConnection(connectionString)
                            dbConnection.Close()
                        End Using
                    End Using
                End If
            End Using
        End Using
    End Sub

    Public Function RowExistsByColumn(columnName As String, value As Int64, tableName As String) As Boolean
        Dim sql As String = $"SELECT EXISTS (SELECT 1 FROM {tableName} WHERE {columnName} = @value);"

        Using connection As New NpgsqlConnection(connectionString)
            connection.Open()

            Using command As New NpgsqlCommand(sql, connection)
                command.Parameters.AddWithValue("@value", value)

                Dim exists As Boolean = command.ExecuteScalar()
                Return exists
            End Using
        End Using
    End Function

    Sub UpdateRow(id As Int64, data As String, table As String, columnName As String)
        Dim sqlCheck As String = $"SELECT column_name FROM information_schema.columns WHERE table_name='{table}' AND column_name='{columnName}';"

        Using connection As New NpgsqlConnection(connectionString)
            connection.Open()

            ' Check if column exists
            Using commandCheck As New NpgsqlCommand(sqlCheck, connection)
                Dim result As Object = commandCheck.ExecuteScalar()

                ' If column exists, then proceed with update
                If result IsNot Nothing Then
                    Dim sqlUpdate As String = $"UPDATE {table} SET {columnName} = @data WHERE id = @id;"

                    Using commandUpdate As New NpgsqlCommand(sqlUpdate, connection)
                        Dim jsonString As String = data.ToString()
                        commandUpdate.Parameters.AddWithValue("@data", NpgsqlDbType.Jsonb, jsonString)
                        commandUpdate.Parameters.AddWithValue("@id", id)

                        commandUpdate.ExecuteNonQuery()
                    End Using
                Else
                    Call Global.System.Console.WriteLine($"Column '{columnName}' does not exist in table {table}.")
                End If
            End Using
        End Using
    End Sub

    Public Function StringToHex(input As String) As String
        Dim byteArray() As Byte = Encoding.UTF8.GetBytes(input)
        Dim hex As New StringBuilder(byteArray.Length * 2)

        For Each b As Byte In byteArray
            hex.AppendFormat("{0:x2}", b)
        Next

        Return hex.ToString()
    End Function

    Public Function GetStringHash(input As String) As Long
        Using sha256Hash As SHA256 = SHA256.Create()
            ' ComputeHash - returns byte array
            Dim bytes As Byte() = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input))

            ' Convert byte array to a long
            If BitConverter.IsLittleEndian Then
                Array.Reverse(bytes)
            End If

            ' Use only the first 8 bytes (64 bits) to fit a Long
            Return BitConverter.ToInt64(bytes, 0)
        End Using
    End Function

    Public Sub UpdateRowByColumn(columnName As String, value As Int64, targetColumn As String, newValue As String, tableName As String)
        Dim sql As String = $"UPDATE {tableName} SET {targetColumn} = @newValue::jsonb WHERE {columnName} = @value;"

        Dim jsonData As JObject = JObject.Parse(newValue) ' Parse the string into a JObject

        Using connection As New NpgsqlConnection(connectionString)
            connection.Open()

            Using command As New NpgsqlCommand(sql, connection)
                command.Parameters.AddWithValue("@value", value)
                command.Parameters.AddWithValue("@newValue", jsonData.ToString()) ' Convert JObject back to string

                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub CreateTables()
        Dim dataTable As String = "id SERIAL PRIMARY KEY, data jsonb"
        Dim playerTable As String = "id BIGINT PRIMARY KEY, data jsonb, bank jsonb"

        For i = 1 To MAX_CHARS
            playerTable += $", character{i} jsonb"
        Next

        Dim tableNames As String() = {"job", "item", "map", "npc", "shop", "skill", "resource", "animation", "pet", "projectile", "moral"}

        For Each tableName In tableNames
            CreateTable(tableName, dataTable)
        Next

        CreateTable("account", playerTable)
    End Sub

    Public Sub CreateTable(tableName As String, layout As String)
        Using conn As New NpgsqlConnection(connectionString)
            conn.Open()

            Using cmd As New NpgsqlCommand()
                cmd.Connection = conn
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({layout});"
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Async Function GetData(tableName As String) As Task(Of List(Of Int64))
        Dim ids As New List(Of Int64)

        Using conn As New NpgsqlConnection(connectionString)
            Await conn.OpenAsync()

            ' Define a query
            Dim cmd As New NpgsqlCommand($"SELECT id FROM {tableName}", conn)

            ' Execute a query
            Using reader As NpgsqlDataReader = Await cmd.ExecuteReaderAsync()
                ' Read all rows and output the first column in each row
                While Await reader.ReadAsync()
                    Dim id = Await reader.GetFieldValueAsync(Of Int64)(0)
                    ids.Add(id)
                End While
            End Using
        End Using

        Return ids
    End Function

    Public Function RowExists(id As Int64, table As String) As Boolean
        Dim sql As String = $"SELECT EXISTS (SELECT 1 FROM {table} WHERE id = @id);"

        Using connection As New NpgsqlConnection(connectionString)
            connection.Open()

            Using command As New NpgsqlCommand(sql, connection)
                command.Parameters.AddWithValue("@id", id)

                Using reader As NpgsqlDataReader = command.ExecuteReader()
                    If reader.Read() Then
                        Return reader.GetBoolean(0)
                    Else
                        Return False
                    End If
                End Using
            End Using
        End Using
    End Function

    Public Sub InsertRow(id As Int64, data As String, tableName As String)
        Dim jsonData As JObject = JObject.Parse(data) ' Parse the string into a JObject

        Using conn As New NpgsqlConnection(connectionString)
            conn.Open()

            Using cmd As New NpgsqlCommand()
                cmd.Connection = conn
                cmd.CommandText = $"INSERT INTO {tableName} (id, data) VALUES (@id, @data::jsonb);"
                cmd.Parameters.AddWithValue("@id", id)
                cmd.Parameters.AddWithValue("@data", jsonData.ToString()) ' Convert JObject back to string

                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub InsertRow(id As Int64, data As String, tableName As String, columnName As String)
        Dim jsonData As JObject = JObject.Parse(data) ' Parse the string into a JObject

        Using conn As New NpgsqlConnection(connectionString)
            conn.Open()

            Using cmd As New NpgsqlCommand()
                cmd.Connection = conn
                cmd.CommandText = $"INSERT INTO {tableName} (id, data) VALUES (@id, @data::jsonb);"
                cmd.Parameters.AddWithValue("@id", id)
                cmd.Parameters.AddWithValue("@" + columnName, jsonData.ToString()) ' Convert JObject back to string

                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub InsertRowByColumn(id As Int64, data As String, tableName As String, dataColumn As String, idColumn As String)
        Dim jsonData As JObject = JObject.Parse(data) ' Parse the string into a JObject

        Dim sql As String = $"INSERT INTO {tableName} ({idColumn}, {dataColumn}) VALUES (@id, @data::jsonb);"

        Using connection As New NpgsqlConnection(connectionString)
            connection.Open()

            Using command As New NpgsqlCommand(sql, connection)
                command.Parameters.AddWithValue("@id", id)
                command.Parameters.AddWithValue("@data", jsonData.ToString()) ' Convert JObject back to string

                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Function SelectRow(id As Int64, tableName As String, columnName As String) As JObject
        Dim sql As String = $"SELECT {columnName} FROM {tableName} WHERE id = @id;"

        Using connection As New NpgsqlConnection(connectionString)
            connection.Open()

            Using command As New NpgsqlCommand(sql, connection)
                command.Parameters.AddWithValue("@id", id)

                Using reader As NpgsqlDataReader = command.ExecuteReader()
                    If reader.Read() Then
                        Dim jsonbData As String = reader.GetString(0)
                        Dim jsonObject As JObject = JObject.Parse(jsonbData)
                        Return jsonObject
                    Else
                        Return Nothing
                    End If
                End Using
            End Using
        End Using
    End Function

    Public Function SelectRowByColumn(columnName As String, value As Int64, tableName As String, dataColumn As String) As JObject
        Dim sql As String = $"SELECT {dataColumn} FROM {tableName} WHERE {columnName} = @value;"

        Using connection As New NpgsqlConnection(connectionString)
            connection.Open()

            Using command As New NpgsqlCommand(sql, connection)
                command.Parameters.AddWithValue("@value", value)

                Using reader As NpgsqlDataReader = command.ExecuteReader()
                    If reader.Read() Then
                        ' Check if the first column is not null
                        If Not reader.IsDBNull(0) Then
                            Dim jsonbData As String = reader.GetString(0)
                            Dim jsonObject As JObject = JObject.Parse(jsonbData)
                            Return jsonObject
                        Else
                            ' Handle null value or return null JObject...
                            Return Nothing
                        End If
                    Else
                        Return Nothing
                    End If
                End Using
            End Using
        End Using
    End Function

#Region "Var"

    Public Function GetVar(filePath As String, section As String, key As String) As String
        Dim isInSection As Boolean = False

        For Each line As String In File.ReadAllLines(filePath)
            If line.Equals("[" & section & "]", StringComparison.OrdinalIgnoreCase) Then
                isInSection = 1
            ElseIf line.StartsWith("[") And line.EndsWith("]") Then
                isInSection = 0
            ElseIf isInSection And line.Contains("=") Then
                Dim parts() As String = line.Split(New Char() {"="c}, 2)
                If parts(0).Equals(key, StringComparison.OrdinalIgnoreCase) Then
                    Return parts(1)
                End If

            End If
        Next

        Return String.Empty ' Key not found
    End Function

    Public Sub PutVar(filePath As String, section As String, key As String, value As String)
        Dim lines As New List(Of String)(File.ReadAllLines(filePath))
        Dim updated As Boolean = False
        Dim isInSection As Boolean = False
        Dim i As Integer = 0

        While i < lines.Count
            If lines(i).Equals("[" & section & "]", StringComparison.OrdinalIgnoreCase) Then
                isInSection = 1
                i += 1
                While i < lines.Count And Not lines(i).StartsWith("[")
                    If lines(i).Contains("=") Then
                        Dim parts() As String = lines(i).Split(New Char() {"="c}, 2)
                        If parts(0).Equals(key, StringComparison.OrdinalIgnoreCase) Then
                            lines(i) = key & "=" & value
                            updated = 1
                            Exit While
                        End If
                    End If
                    i += 1
                End While
                Exit While
            End If
            i += 1
        End While

        If Not updated Then
            ' Key not found, add it to the section
            lines.Add("[" & section & "]")
            lines.Add(key & "=" & value)
        End If

        File.WriteAllLines(filePath, lines)
    End Sub


#End Region

#Region "Job"

    Sub ClearJobs()
        Dim i As Integer

        ReDim Type.Job(MAX_JOBS)

        For i = 1 To MAX_JOBS
            ClearJob(i)
        Next
    End Sub

    Sub ClearJob(jobNum As Integer)
        ReDim Type.Job(jobNum).Stat(StatType.Count - 1)
        ReDim Type.Job(jobNum).StartItem(5)
        ReDim Type.Job(jobNum).StartValue(5)

        Type.Job(jobNum).Name = ""
        Type.Job(jobNum).Desc = ""
        Type.Job(jobNum).StartMap = 1
        Type.Job(jobNum).MaleSprite = 1
        Type.Job(jobNum).FemaleSprite = 1
    End Sub

    Sub LoadJob(jobNum As Integer)
        Dim data As JObject

        data = SelectRow(jobNum, "job", "data")

        If data Is Nothing Then
            ClearJob(jobNum)
            Exit Sub
        End If

        Dim jobData = JObject.FromObject(data).ToObject(Of JobStruct)()
        Type.Job(jobNum) = jobData
    End Sub

    Sub LoadJobs()
        Dim i As Integer

        For i = 1 To MAX_JOBS
            LoadJob(i)
        Next
    End Sub

    Sub SaveJob(jobNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Job(jobNum)).ToString()

        If RowExists(jobNum, "job") Then
            UpdateRow(jobNum, json, "job", "data")
        Else
            InsertRow(jobNum, "data", "job")
        End If
    End Sub

    Sub SaveJobs()
        Dim i As Integer

        For i = 1 To MAX_JOBS
            SaveJob(i)
        Next
    End Sub
#End Region

#Region "Maps"

    Sub ClearMaps()
        ReDim Type.Map(MAX_MAPS)
        ReDim MapNPC(MAX_MAPS)

        For i = 1 To MAX_MAPS
            ReDim MapNPC(i).NPC(MAX_MAP_NPCS)
            ReDim Type.Map(i).NPC(MAX_MAP_NPCS)
        Next

        ReDim Switches(MAX_SWITCHES)
        ReDim Variables(NAX_VARIABLES)
        ReDim TempEventMap(MAX_MAPS)

        For i = 1 To MAX_MAPS
            ClearMap(i)
        Next
    End Sub

    Sub ClearMap(MapNum As Integer)
        Dim x As Integer
        Dim y As Integer

        Type.Map(MapNum).Tileset = 1
        Type.Map(MapNum).Name = ""
        Type.Map(MapNum).MaxX = MAX_MAPX
        Type.Map(MapNum).MaxY = MAX_MAPY
        ReDim Type.Map(MapNum).NPC(MAX_MAP_NPCS)
        ReDim Type.Map(MapNum).Tile(Type.Map(MapNum).MaxX, Type.Map(MapNum).MaxY)

        For x = 0 To MAX_MAPX
            For y = 0 To MAX_MAPY
                ReDim Type.Map(MapNum).Tile(x, y).Layer(LayerType.Count - 1)
            Next
        Next

        Type.Map(MapNum).EventCount = 0
        ReDim Type.Map(MapNum).Event(0)

        ' Reset the values for if a player is on the map or not
        PlayersOnMap(MapNum) = 0
        Type.Map(MapNum).Tileset = 1
        Type.Map(MapNum).Name = ""
        Type.Map(MapNum).Music = ""
        Type.Map(MapNum).MaxX = MAX_MAPX
        Type.Map(MapNum).MaxY = MAX_MAPY
    End Sub

    Public Sub SaveMap(MapNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Map(MapNum)).ToString()

        If RowExists(MapNum, "map") Then
            UpdateRow(MapNum, json, "map", "data")
        Else
            InsertRow(MapNum, json, "map")
        End If
    End Sub

    Sub LoadMaps()
        Dim i As Integer

        For i = 1 To MAX_MAPS
            LoadMap(i)
        Next
    End Sub

    Sub LoadMap(MapNum As Integer)
        ' Get the base directory of the application
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory

        ' Construct the path to the "maps" directory
        Dim mapsDir As String = Path.Combine(baseDir, "maps")
        Directory.CreateDirectory(mapsDir)

        CacheResources(MapNum)

        If File.Exists(mapsDir & "\cs\map" & MapNum & ".ini") Then
            Dim csMap As CSMapStruct = LoadCSMap(MapNum)
            Type.Map(MapNum) = MapFromCSMap(csMap)
            Exit Sub
        End If

        If File.Exists(mapsDir & "\xw\map" & MapNum & ".dat") Then
            Dim xwMap As XWMapStruct = LoadXWMap(mapsDir & "\xw\map" & MapNum.ToString() & ".dat")
            Type.Map(MapNum) = MapFromXWMap(xwMap)
            Exit Sub
        End If

        If File.Exists(mapsDir & "\sd\map" & MapNum & ".dat") Then
            'Dim sdMap As SDMapStruct = loadsdmap(Type.MapsDir & "\sd\map" & mapNum.ToString() & ".dat")
            'Type.Map(MapNum) = MapFromSDMap(sdMap)
            Exit Sub
        End If

        Dim data As JObject

        data = SelectRow(MapNum, "map", "data")

        If data Is Nothing Then
            ClearMap(MapNum)
            Exit Sub
        End If

        Dim mapData = JObject.FromObject(data).ToObject(Of MapStruct)()
        Type.Map(MapNum) = mapData
    End Sub

    Public Function LoadCSMap(MapNum As Long) As CSMapStruct
        Dim filename As String
        Dim i As Long
        Dim x As Long, y As Long
        Dim csMap As CSMapStruct

        ' Load map data
        filename = AppDomain.CurrentDomain.BaseDirectory & "\maps\cs\map" & MapNum & ".ini"

        ReDim csMap.MapData.Npc(MAX_MAP_NPCS)

        ' General
        With csMap.MapData
            .Name = GetVar(filename, "General", "Name")
            .Music = GetVar(filename, "General", "Music")
            .Moral = Val(GetVar(filename, "General", "Moral"))
            .Up = Val(GetVar(filename, "General", "Up"))
            .Down = Val(GetVar(filename, "General", "Down"))
            .Left = Val(GetVar(filename, "General", "Left"))
            .Right = Val(GetVar(filename, "General", "Right"))
            .BootMap = Val(GetVar(filename, "General", "BootMap"))
            .BootX = Val(GetVar(filename, "General", "BootX"))
            .BootY = Val(GetVar(filename, "General", "BootY"))
            .MaxX = Val(GetVar(filename, "General", "MaxX"))
            .MaxY = Val(GetVar(filename, "General", "MaxY"))

            .Weather = Val(GetVar(filename, "General", "Weather"))
            .WeatherIntensity = Val(GetVar(filename, "General", "WeatherIntensity"))

            .Fog = Val(GetVar(filename, "General", "Fog"))
            .FogSpeed = Val(GetVar(filename, "General", "FogSpeed"))
            .FogOpacity = Val(GetVar(filename, "General", "FogOpacity"))

            .Red = Val(GetVar(filename, "General", "Red"))
            .Green = Val(GetVar(filename, "General", "Green"))
            .Blue = Val(GetVar(filename, "General", "Blue"))
            .Alpha = Val(GetVar(filename, "General", "Alpha"))

            .BossNpc = Val(GetVar(filename, "General", "BossNpc"))
            For i = 1 To 30
                .Npc(i) = Val(GetVar(filename, "General", "Npc" & i))
            Next
        End With

        ' Redim the map
        ReDim csMap.Tile(csMap.MapData.MaxX, csMap.MapData.MaxY)

        filename = AppDomain.CurrentDomain.BaseDirectory & "\maps\cs\map" & MapNum & ".dat"

        Using fileStream As New FileStream(filename, FileMode.Open, FileAccess.Read),
            binaryReader As New BinaryReader(fileStream)

            With csMap
                ' Assuming MAX_X and MAX_Y are the dimensions of your map
                Dim MAX_X As Integer = csMap.MapData.MaxX
                Dim MAX_Y As Integer = csMap.MapData.MaxY

                For x = 0 To MAX_X
                    For y = 0 To MAX_Y
                        ' Resize arrays only once if possible
                        ReDim csMap.Tile(x, y).Autotile(LayerType.Count - 1)
                        ReDim csMap.Tile(x, y).Layer(LayerType.Count - 1)

                        With csMap.Tile(x, y)
                            .Type = binaryReader.ReadByte()
                            .Data1 = binaryReader.ReadInt32()
                            .Data2 = binaryReader.ReadInt32()
                            .Data3 = binaryReader.ReadInt32()
                            .Data4 = binaryReader.ReadInt32()
                            .Data5 = binaryReader.ReadInt32()

                            For i = 1 To LayerType.Count - 1
                                .Autotile(i) = binaryReader.ReadByte()
                            Next
                            .DirBlock = binaryReader.ReadByte()

                            For i = 1 To LayerType.Count - 1
                                .Layer(i).TileSet = binaryReader.ReadInt32()
                                .Layer(i).x = binaryReader.ReadInt32()
                                .Layer(i).y = binaryReader.ReadInt32()
                            Next
                        End With
                    Next
                Next
            End With
        End Using

        Return csMap
    End Function

    Sub ClearMapItem(index As Integer, mapNum As Integer)
        MapItem(mapNum, index).PlayerName = ""
    End Sub

    Sub ClearMapItems()
        Dim x As Integer
        Dim y As Integer

        For y = 1 To MAX_MAPS
            For x = 1 To MAX_MAP_ITEMS
                ClearMapItem(x, y)
            Next
        Next

    End Sub

    Public Function LoadXWMap(ByVal fileName As String) As XWMapStruct
        Dim encoding As New ASCIIEncoding()
        Dim xwMap As New XWMapStruct

        ReDim xwMap.Tile(15, 11)
        ReDim xwMap.NPC(14)

        Using fs As New FileStream(fileName, FileMode.Open)
            Using reader As New BinaryReader(fs)
                'OFFSET 0: The first 20 bytes are the map name.
                xwMap.Name = encoding.GetString(reader.ReadBytes(20))

                'OFFSET 20: The revision is stored here @ 4 bytes.
                xwMap.Revision = reader.ReadInt32()

                'OFFSET 24: Contains the map moral as a byte.
                xwMap.Moral = reader.ReadByte()

                'OFFSET 25: Stored as 2 bytes, the map UP.
                xwMap.Up = reader.ReadInt16()

                'OFFSET 27: Stored as 2 bytes, the map DOWN.
                xwMap.Down = reader.ReadInt16()

                'OFFSET 29: Stored as 2 bytes, the map LEFT.
                xwMap.Left = reader.ReadInt16()

                'OFFSET 31: Stored as 2 bytes, the map RIGHT.
                xwMap.Right = reader.ReadInt16()

                'OFFSET 33: Stored as 2 bytes, the map music.
                xwMap.Music = reader.ReadInt16()

                'OFFSET 35: Stored as 2 bytes, the Boot map.
                xwMap.BootMap = reader.ReadInt16()

                'OFFSET 37: Stored as a single byte, the boot X
                xwMap.BootX = reader.ReadByte()

                'OFFSET 38: Stored as a single byte, the boot Y
                xwMap.BootY = reader.ReadByte()

                'OFFSET 39: Stored as two bytes, the Shop ID.
                xwMap.Shop = reader.ReadInt16()

                'OFFSET 41: Stored as a single byte, is the map indoors?
                xwMap.Indoors = (reader.ReadByte() = 1)

                ' Now, we decode the Tiles
                For y As Integer = 0 To 11
                    For x As Integer = 0 To 15
                        xwMap.Tile(x, y).Ground = reader.ReadInt16() ' 42
                        xwMap.Tile(x, y).Mask = reader.ReadInt16() ' 44
                        xwMap.Tile(x, y).MaskAnim = reader.ReadInt16() ' 46
                        xwMap.Tile(x, y).Fringe = reader.ReadInt16() ' 48
                        xwMap.Tile(x, y).Type = CType(reader.ReadByte(), TileType) ' 50
                        xwMap.Tile(x, y).Data1 = reader.ReadInt16() ' 51
                        xwMap.Tile(x, y).Data2 = reader.ReadInt16() ' 53
                        xwMap.Tile(x, y).Data3 = reader.ReadInt16() ' 55
                        xwMap.Tile(x, y).Type2 = CType(reader.ReadByte(), TileType) ' 57
                        xwMap.Tile(x, y).Data1_2 = reader.ReadInt16() ' 59
                        xwMap.Tile(x, y).Data2_2 = reader.ReadInt16() ' 61
                        xwMap.Tile(x, y).Data3_2 = reader.ReadInt16() ' 63
                        xwMap.Tile(x, y).Mask2 = reader.ReadInt16() ' 64
                        xwMap.Tile(x, y).Mask2Anim = reader.ReadInt16() ' 66
                        xwMap.Tile(x, y).FringeAnim = reader.ReadInt16() ' 68
                        xwMap.Tile(x, y).Roof = reader.ReadInt16() ' 70
                        xwMap.Tile(x, y).Fringe2Anim = reader.ReadInt16() ' 72
                    Next
                Next

                For i As Integer = 1 To 14
                    xwMap.NPC(i) = reader.ReadInt32
                Next
            End Using
        End Using

        Return xwMap
    End Function

    Private Function ConvertXWTileToTile(ByVal xwTile As XWTileStruct) As TileStruct
        Dim tile As New TileStruct

        ' Constants for the new tileset
        Const TilesPerRow As Integer = 8
        Const RowsPerTileset As Integer = 16

        ' Initialize the layers
        ReDim tile.Layer(LayerType.Count - 1)

        ' Process each layer
        For i As Integer = LayerType.Ground To LayerType.Count - 1
            Dim tileNumber As Integer = 0

            ' Select the appropriate tile number for each layer
            Select Case i
                Case LayerType.Ground
                    tileNumber = xwTile.Ground
                Case LayerType.Mask
                    tileNumber = xwTile.Mask
                Case LayerType.MaskAnim
                    tileNumber = xwTile.MaskAnim
                Case LayerType.Cover
                    tileNumber = xwTile.Mask2
                Case LayerType.CoverAnim
                    tileNumber = xwTile.Mask2Anim
                Case LayerType.Fringe
                    tileNumber = xwTile.Fringe
                Case LayerType.FringeAnim
                    tileNumber = xwTile.FringeAnim
                Case LayerType.Roof
                    tileNumber = xwTile.Roof
                Case LayerType.RoofAnim
                    tileNumber = xwTile.Fringe2Anim
            End Select

            ' Ensure tileNumber is non-negative
            If tileNumber > 0 Then
                tile.Layer(i).Tileset = Math.Floor(tileNumber / TilesPerRow / RowsPerTileset) + 1
                tile.Layer(i).Y = Math.Floor(tileNumber / TilesPerRow) Mod RowsPerTileset
                tile.Layer(i).X = tileNumber Mod TilesPerRow
            End If
        Next

        ' Copy over additional data fields
        tile.Data1 = xwTile.Data1
        tile.Data2 = xwTile.Data2
        tile.Data3 = xwTile.Data3
        tile.Data1_2 = xwTile.Data1_2
        tile.Data2_2 = xwTile.Data2_2
        tile.Data3_2 = xwTile.Data3_2
        tile.Type = xwTile.Type
        tile.Type2 = xwTile.Type2

        SetXWTileType(tile.Type)
        SetXWTileType(tile.Type2)

        Return tile
    End Function

    Public Sub SetXWTileType(ByRef tileType As TileType)
        Select Case tileType
            Case XWTileType.Warp
                tileType = TileType.Warp
            Case XWTileType.Damage
                tileType = TileType.Trap
            Case XWTileType.Heal
                tileType = TileType.Heal
            Case XWTileType.Shop
                tileType = TileType.Shop
            Case XWTileType.No_Xing
                tileType = TileType.NoXing
            Case XWTileType.Key
                tileType = TileType.Key
            Case XWTileType.Key_Open
                tileType = TileType.KeyOpen
            Case XWTileType.Door
                tileType = TileType.Door
            Case XWTileType.Walkthru
                tileType = TileType.WalkThrough
            Case XWTileType.Arena
                tileType = TileType.Arena
            Case XWTileType.Roof
                tileType = TileType.Roof
            Case XWTileType.Direction_Block
        End Select
    End Sub

    Public Function MapFromXWMap(xwMap As XWMapStruct) As MapStruct
        Dim mwMap As New MapStruct

        ReDim mwMap.Tile(15, 11)
        ReDim mwMap.NPC(MAX_MAP_NPCS)

        mwMap.Name = xwMap.Name
        mwMap.Music = "Music" & xwMap.Music.ToString() & ".mid"
        mwMap.Revision = CInt(xwMap.Revision)
        mwMap.Moral = xwMap.Moral
        mwMap.Up = xwMap.Up
        mwMap.Down = xwMap.Down
        mwMap.Left = xwMap.Left
        mwMap.Right = xwMap.Right
        mwMap.BootMap = xwMap.BootMap
        mwMap.BootX = xwMap.BootX
        mwMap.BootY = xwMap.BootY
        mwMap.Shop = xwMap.Shop

        ' Convert Byte to Boolean (False if 0, True otherwise)
        mwMap.Indoors = xwMap.Indoors <> 0

        ' Loop through each tile in xwMap and copy the data to map
        For y As Integer = 0 To 11
            For x As Integer = 0 To 15
                mwMap.Tile(x, y) = ConvertXWTileToTile(xwMap.Tile(x, y))
            Next
        Next

        ' NPC array conversion (Long to Integer), if necessary
        If xwMap.NPC IsNot Nothing Then
            mwMap.NPC = Array.ConvertAll(xwMap.NPC, Function(i) CInt(i))
        End If

        mwMap.Weather = xwMap.Weather
        mwMap.NoRespawn = xwMap.Respawn = 0
        mwMap.MaxX = 15
        mwMap.MaxY = 11

        Return mwMap
    End Function

    Public Function MapFromCSMap(csMap As CSMapStruct) As MapStruct
        Dim mwMap As MapStruct

        ReDim mwMap.NPC(MAX_MAP_NPCS)

        mwMap.Name = csMap.MapData.Name
        mwMap.MaxX = csMap.MapData.MaxX
        mwMap.MaxY = csMap.MapData.MaxY
        mwMap.BootMap = csMap.MapData.BootMap
        mwMap.BootX = csMap.MapData.BootX
        mwMap.BootY = csMap.MapData.BootY
        mwMap.Moral = csMap.MapData.Moral
        mwMap.Music = csMap.MapData.Music
        mwMap.Fog = csMap.MapData.Fog
        mwMap.Weather = csMap.MapData.Weather
        mwMap.WeatherIntensity = csMap.MapData.WeatherIntensity
        mwMap.Up = csMap.MapData.Up
        mwMap.Down = csMap.MapData.Down
        mwMap.Left = csMap.MapData.Left
        mwMap.Right = csMap.MapData.Right
        mwMap.MapTintA = csMap.MapData.Alpha
        mwMap.MapTintR = csMap.MapData.Red
        mwMap.MapTintG = csMap.MapData.Green
        mwMap.MapTintB = csMap.MapData.Blue
        mwMap.FogOpacity = csMap.MapData.FogOpacity
        mwMap.FogSpeed = csMap.MapData.FogSpeed

        ReDim mwMap.Tile(mwMap.MaxX, mwMap.MaxY)

        ' Loop through each tile in xwMap and copy the data to map
        For y As Integer = 0 To mwMap.MaxX
            For x As Integer = 0 To mwMap.MaxY
                ' Resize arrays only once if possible
                ReDim mwMap.Tile(x, y).Layer(LayerType.Count - 1)

                mwMap.Tile(x, y).Data1 = csMap.Tile(x, y).Data1
                mwMap.Tile(x, y).Data2 = csMap.Tile(x, y).Data2
                mwMap.Tile(x, y).Data3 = csMap.Tile(x, y).Data3
                mwMap.Tile(x, y).DirBlock = csMap.Tile(x, y).DirBlock

                For i As Integer = LayerType.Ground To LayerType.Count - 1
                    mwMap.Tile(x, y).Layer(i).X = csMap.Tile(x, y).Layer(i).x
                    mwMap.Tile(x, y).Layer(i).Y = csMap.Tile(x, y).Layer(i).y
                    mwMap.Tile(x, y).Layer(i).Tileset = csMap.Tile(x, y).Layer(i).TileSet
                    mwMap.Tile(x, y).Layer(i).AutoTile = csMap.Tile(x, y).Autotile(i)
                Next
            Next
        Next

        For i As Integer = 1 To 30
            mwMap.NPC(i) = csMap.MapData.Npc(i)
        Next

        Return mwMap
    End Function

    Private Function MapFromSDMap(sdMap As SDMapStruct) As MapStruct
        Dim mwMap As MapStruct

        mwMap.Name = sdMap.Name
        mwMap.Music = sdMap.Music
        mwMap.Revision = sdMap.Revision

        mwMap.Up = sdMap.Up
        mwMap.Down = sdMap.Down
        mwMap.Left = sdMap.Left
        mwMap.Right = sdMap.Right

        mwMap.Tileset = sdMap.Tileset
        mwMap.MaxX = sdMap.MaxX
        mwMap.MaxY = sdMap.MaxY

        Return mwMap
    End Function

#End Region

#Region "NPCs"

    Sub SaveNPC(npcNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.NPC(NPCNum)).ToString()

        If RowExists(npcNum, "npc") Then
            UpdateRow(npcNum, json, "npc", "data")
        Else
            InsertRow(npcNum, json, "npc")
        End If
    End Sub

    Sub LoadNPCs()
        Dim i As Integer

        For i = 1 To MAX_NPCS
            LoadNPC(i)
        Next
    End Sub

    Sub LoadNPC(npcNum As Integer)
        Dim data As JObject

        data = SelectRow(npcNum, "npc", "data")

        If data Is Nothing Then
            ClearNPC(NPCNum)
            Exit Sub
        End If

        Dim npcData = JObject.FromObject(data).ToObject(Of NpcStruct)()
        Type.NPC(NPCNum) = npcData
    End Sub

    Sub ClearMapNPC(index As Integer, mapNum As Integer)
        MapNPC(MapNum).NPC(index).Num = 0
        ReDim MapNPC(MapNum).NPC(index).Vital(VitalType.Count - 1)
        ReDim MapNPC(MapNum).NPC(index).SkillCd(MAX_NPC_SKILLS)
    End Sub

    Sub ClearAllMapNPCs()
        Dim i As Integer

        For i = 1 To MAX_MAPS
            ClearMapNPCs(i)
        Next

    End Sub

    Sub ClearMapNPCs(MapNum As Integer)
        Dim x As Integer

        For x = 1 To MAX_MAP_NPCS
            ClearMapNPC(x, mapNum)
        Next

    End Sub

    Sub ClearNPC(index As Integer)
       Type.NPC(index).Name = ""
       Type.NPC(index).AttackSay = ""

        ReDim Type.NPC(index).Stat(StatType.Count - 1)

        For i = 1 To MAX_DROP_ITEMS
            ReDim Type.NPC(index).DropChance(5)
            ReDim Type.NPC(index).DropItem(5)
            ReDim Type.NPC(index).DropItemValue(5)
            ReDim Type.NPC(index).Skill(MAX_NPC_SKILLS)
        Next
    End Sub

    Sub ClearNPCs()
        For i = 1 To MAX_NPCS
            ClearNPC(i)
        Next

    End Sub

#End Region

#Region "Shops"

    Sub SaveShop(shopNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Shop(shopNum)).ToString()

        If RowExists(shopNum, "shop") Then
            UpdateRow(shopNum, json, "shop", "data")
        Else
            InsertRow(shopNum, json, "shop")
        End If
    End Sub

    Sub LoadShops()
        Dim i As Integer

        For i = 1 To MAX_SHOPS
            LoadShop(i)
        Next

    End Sub

    Sub LoadShop(shopNum As Integer)
        Dim data As JObject

        data = SelectRow(shopNum, "shop", "data")

        If data Is Nothing Then
            ClearShop(shopNum)
            Exit Sub
        End If

        Dim shopData = JObject.FromObject(data).ToObject(Of ShopStruct)()
        Type.Shop(shopNum) = shopData
    End Sub

    Sub ClearShop(index As Integer)
        Type.Shop(index) = Nothing
        Type.Shop(index).Name = ""

        For i = 1 To MAX_TRADES
            ReDim Type.Shop(index).TradeItem(i)
        Next
    End Sub

    Sub ClearShops()
        For i = 1 To MAX_SHOPS
            Call ClearShop(i)
        Next
    End Sub

#End Region

#Region "Skills"

    Sub SaveSkill(skillNum As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Skill(skillNum)).ToString()

        If RowExists(skillNum, "skill") Then
            UpdateRow(skillNum, json, "skill", "data")
        Else
            InsertRow(skillNum, json, "skill")
        End If
    End Sub

    Sub LoadSkills()
        Dim i As Integer

        For i = 1 To MAX_SKILLS
            LoadSkill(i)
        Next

    End Sub

    Sub LoadSkill(skillNum As Integer)
        Dim data As JObject

        data = SelectRow(skillNum, "skill", "data")

        If data Is Nothing Then
            ClearSkill(skillNum)
            Exit Sub
        End If

        Dim skillData = JObject.FromObject(data).ToObject(Of SkillStruct)()
        Type.Skill(skillNum) = skillData
    End Sub

    Sub ClearSkill(index As Integer)
        Type.Skill(index).Name = ""
        Type.Skill(index).LevelReq = 1
    End Sub

    Sub ClearSkills()
        Dim i As Integer

        For i = 1 To MAX_SKILLS
            ClearSkill(i)
        Next

    End Sub

#End Region

#Region "Players"

    Sub SaveAllPlayersOnline()
        For i = 1 To Socket.HighIndex()
            If Not IsPlaying(i) Then Continue For
            SaveCharacter(i, TempPlayer(i).Slot)
            SaveBank(i)
        Next
    End Sub

    Sub SaveAccount(index As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Account(index)).ToString()
        Dim username As String = GetPlayerLogin(index)
        Dim id As Int64 = GetStringHash(username)

        If RowExists(id, "account") Then
            UpdateRowByColumn("id", id, "data", json, "account")
        Else
            InsertRowByColumn(id, json, "account", "data", "id")
        End If
    End Sub

    Sub RegisterAccount(index As Integer, username As String, password As String)
        SetPlayerLogin(index, username)
        SetPlayerPassword(index, password)

        Dim json As String = JsonConvert.SerializeObject(Type.Account(index)).ToString()

        Dim id As Int64 = GetStringHash(username)

        InsertRowByColumn(id, json, "account", "data", "id")
    End Sub

    Function LoadAccount(index As Integer, username As String)
        Dim data As JObject

        data = SelectRowByColumn("id", GetStringHash(username), "account", "data")

        If data Is Nothing Then
            Return False
        End If

        Dim accountData = JObject.FromObject(data).ToObject(Of AccountStruct)()
        Type.Account(index) = accountData
        Return True
    End Function

    Sub ClearAccount(index As Integer)
        SetPlayerLogin(index, "")
        SetPlayerPassword(index, "")
        ClearPlayer(index)
    End Sub

    Sub ClearPlayer(index As Integer)
        Type.Player(index).Access = AccessType.Player
        ReDim TempPlayer(MAX_PLAYERS)

        For i = 1 To MAX_PLAYERS
            ReDim TempPlayer(i).SkillCd(MAX_PLAYER_SKILLS)
            ReDim TempPlayer(i).PetSkillCd(4)
            ReDim TempPlayer(i).TradeOffer(MAX_INV)
        Next

        ReDim TempPlayer(index).SkillCd(MAX_PLAYER_SKILLS)
        ReDim TempPlayer(index).PetSkillCd(4)
        TempPlayer(index).Editor = -1

        ClearBank(index)
        ClearCharacter(index)
    End Sub

#End Region

#Region "Bank"
    Friend Sub LoadBank(index As Integer)
        Dim data As JObject
        data = SelectRowByColumn("id", GetStringHash(GetPlayerLogin(index)), "account", "bank")

        If data Is Nothing Then
            ClearBank(index)
            Exit Sub
        End If

        Dim bankData = JObject.FromObject(data).ToObject(Of BankStruct)()
        Bank(index) = bankData
    End Sub

    Sub SaveBank(index As Integer)
        Dim json As String = JsonConvert.SerializeObject(Bank(index)).ToString()
        Dim username As String = GetPlayerLogin(index)
        Dim id As Int64 = GetStringHash(username)

        If RowExistsByColumn("id", id, "account") Then
            UpdateRowByColumn("id", id, "bank", json, "account")
        Else
            InsertRowByColumn(id, json, "account", "bank", "id")
        End If
    End Sub

    Sub ClearBank(index As Integer)
        ReDim Bank(index).Item(MAX_BANK)

        For i = 1 To MAX_BANK
            Bank(index).Item(i).Num = 0
            Bank(index).Item(i).Value = 0
        Next
    End Sub

#End Region

#Region "Characters"
    Sub ClearCharacter(index As Integer)
        Type.Player(index).Name = ""
        Type.Player(index).Job = 0
        Type.Player(index).Dir = 0

        ReDim Type.Player(index).Equipment(EquipmentType.Count - 1)
        For i = 1 To EquipmentType.Count - 1
            Type.Player(index).Equipment(i) = 0
        Next

        ReDim Type.Player(index).Inv(MAX_INV)
        For i = 1 To MAX_INV
            Type.Player(index).Inv(i).Num = 0
            Type.Player(index).Inv(i).Value = 0
        Next

        Type.Player(index).Exp = 0
        Type.Player(index).Level = 0
        Type.Player(index).Map = 1
        Type.Player(index).Name = ""
        Type.Player(index).Pk = 0
        Type.Player(index).Points = 0
        Type.Player(index).Sex = 0

        ReDim Type.Player(index).Skill(MAX_PLAYER_SKILLS)
        For i = 1 To MAX_PLAYER_SKILLS
            Type.Player(index).Skill(i).Num = 0
            Type.Player(index).Skill(i).CD = 0
        Next

        Type.Player(index).Sprite = 0

        ReDim Type.Player(index).Stat(StatType.Count - 1)
        For i = 1 To StatType.Count - 1
            Type.Player(index).Stat(i) = 0
        Next

        ReDim Type.Player(index).Vital(VitalType.Count - 1)
        For i = 1 To VitalType.Count - 1
            Type.Player(index).Vital(i) = 0
        Next

        Type.Player(index).X = 0
        Type.Player(index).Y = 0

        ReDim Type.Player(index).Hotbar(MAX_HOTBAR)
        For i = 1 To MAX_HOTBAR
            Type.Player(index).Hotbar(i).Slot = 0
            Type.Player(index).Hotbar(i).SlotType = 0
        Next

        ReDim Type.Player(index).Switches(MAX_SWITCHES)
        For i = 1 To MAX_SWITCHES
            Type.Player(index).Switches(i) = 0
        Next
        ReDim Type.Player(index).Variables(NAX_VARIABLES)
        For i = 1 To NAX_VARIABLES
            Type.Player(index).Variables(i) = 0
        Next

        ReDim Type.Player(index).GatherSkills(ResourceType.Count - 1)
        For i = 1 To ResourceType.Count - 1
            Type.Player(index).GatherSkills(i).SkillLevel = 1
            Type.Player(index).GatherSkills(i).SkillCurExp = 0
            SetPlayerGatherSkillMaxExp(index, i, GetSkillNextLevel(index, i))
        Next

        For i = 1 To EquipmentType.Count - 1
            Type.Player(index).Equipment(i) = 0
        Next

        Type.Player(index).Pet.Num = 0
        Type.Player(index).Pet.Health = 0
        Type.Player(index).Pet.Mana = 0
        Type.Player(index).Pet.Level = 0

        ReDim Type.Player(index).Pet.Stat(StatType.Count - 1)

        For i = 1 To StatType.Count - 1
            Type.Player(index).Pet.Stat(i) = 0
        Next

        ReDim Type.Player(index).Pet.Skill(4)
        For i = 1 To 4
            Type.Player(index).Pet.Skill(i) = 0
        Next

        Type.Player(index).Pet.X = 0
        Type.Player(index).Pet.Y = 0
        Type.Player(index).Pet.Dir = 0
        Type.Player(index).Pet.Alive = 0
        Type.Player(index).Pet.AttackBehaviour = 0
        Type.Player(index).Pet.AdoptiveStats = 0
        Type.Player(index).Pet.Points = 0
        Type.Player(index).Pet.Exp = 0
    End Sub

    Function LoadCharacter(index As Integer, charNum As Integer) As Boolean
        Dim data As JObject

        data = SelectRowByColumn("id", GetStringHash(GetPlayerLogin(index)), "account", "character" & charNum.ToString)

        If data Is Nothing Then
            Return False
        End If

        Dim characterData = JObject.FromObject(data).ToObject(Of PlayerStruct)()

        If characterData.Name = "" Then
            Return False
        End If

        Type.Player(index) = characterData
        Return True
    End Function

    Sub SaveCharacter(index As Integer, slot As Integer)
        Dim json As String = JsonConvert.SerializeObject(Type.Player(index)).ToString()
        Dim id As Int64 = GetStringHash(GetPlayerLogin(index))

        If slot < 1 Or slot > MAX_CHARS Then Exit Sub

        If RowExistsByColumn("id", id, "account") Then
            UpdateRowByColumn("id", id, "character" & slot.ToString(), json, "account")
        Else
            InsertRowByColumn(id, json, "account", "character" & slot.ToString(), "id")
        End If
    End Sub

    Sub AddChar(index As Integer, slot As Integer, name As String, Sex As Byte, jobNum As Byte, sprite As Integer)
        Dim n As Integer, i As Integer

        If Len(Type.Player(index).Name) = 0 Then
            Type.Player(index).Name = name
            Type.Player(index).Sex = Sex
            Type.Player(index).Job = jobNum
            Type.Player(index).Sprite = sprite

            Type.Player(index).Level = 1

            For n = 1 To StatType.Count - 1
                Type.Player(index).Stat(n) = Type.Job(jobNum).Stat(n)
            Next

            Type.Player(index).Dir = DirectionType.Down
            Type.Player(index).Map = Type.Job(jobNum).StartMap
            Type.Player(index).X = Type.Job(jobNum).StartX
            Type.Player(index).Y = Type.Job(jobNum).StartY
            Type.Player(index).Dir = DirectionType.Down

            For i = 1 To VitalType.Count - 1
                Call SetPlayerVital(index, i, GetPlayerMaxVital(index, i))
            Next

            ' set starter equipment
            For n = 1 To 5
                If Type.Job(jobNum).StartItem(n) > 0 Then
                    Type.Player(index).Inv(n).Num = Type.Job(jobNum).StartItem(n)
                    Type.Player(index).Inv(n).Value = Type.Job(jobNum).StartValue(n)
                End If
            Next

            ' set skills
            For i = 1 To ResourceType.Count - 1
                Type.Player(index).GatherSkills(i).SkillLevel = 1
                Type.Player(index).GatherSkills(i).SkillCurExp = 0
                SetPlayerGatherSkillMaxExp(index, i, GetSkillNextLevel(index, i))
            Next

            SaveCharacter(index, slot)
        End If

    End Sub

#End Region

#Region "Ban"

    Sub ServerBanIndex(BanPlayerindex As Integer)
        Dim filename As String
        Dim IP As String
        Dim F As Integer
        Dim i As Integer

        filename = System.IO.Path.Combine(Core.Path.Database, "banlist.txt")

        ' Make sure the file exists
        If Not File.Exists(filename) Then
            F = FreeFile()
        End If

        ' Cut off last portion of ip
        IP = Socket.ClientIp(BanPlayerindex)

        For i = Len(IP) To 1 Step -1

            If Mid$(IP, i, 1) = "." Then
                Exit For
            End If

        Next

        Type.Account(BanPlayerindex).Banned = 1

        IP = Mid$(IP, 1, i)
        AddTextToFile(IP, "banlist.txt")
        GlobalMsg(GetPlayerName(BanPlayerindex) & " has been banned from " & Settings.GameName & " by " & "the Server" & "!")
        Addlog("The Server" & " has banned " & GetPlayerName(BanPlayerindex) & ".", ADMIN_LOG)
        AlertMsg(BanPlayerindex, DialogueMsg.Banned)
    End Sub

    Function IsBanned(index As Integer, IP As String) As Boolean
        Dim filename As String, line As String

        filename = System.IO.Path.Combine(Core.Path.Database, "banlist.txt")

        ' Check if file exists
        If Not File.Exists(filename) Then
            Return False
        End If

        Dim sr As StreamReader = New StreamReader(filename)

        Do While sr.Peek() >= 0
            ' Is banned?
            line = sr.ReadLine()
            If LCase$(line) = LCase$(Mid$(IP, 1, Len(line))) Then
                IsBanned = 1
            End If
        Loop

        sr.Close()

        If Type.Account(index).Banned Then
            IsBanned = 1
        End If

    End Function

    Sub BanIndex(BanPlayerindex As Integer, BannedByindex As Integer)
        Dim filename As String = System.IO.Path.Combine(Core.Path.Database, "banlist.txt")
        Dim IP As String, i As Integer

        ' Make sure the file exists
        If Not File.Exists(filename) Then File.Create(filename).Dispose()

        ' Cut off last portion of ip
        IP = Socket.ClientIp(BanPlayerindex)

        For i = Len(IP) To 1 Step -1

            If Mid$(IP, i, 1) = "." Then
                Exit For
            End If

        Next

        Type.Account(BanPlayerindex).Banned = 1

        IP = Mid$(IP, 1, i)
        AddTextToFile(IP, "banlist.txt")
        GlobalMsg(GetPlayerName(BanPlayerindex) & " has been banned from " & Settings.GameName & " by " & GetPlayerName(BannedByindex) & "!")
        Addlog(GetPlayerName(BannedByindex) & " has banned " & GetPlayerName(BanPlayerindex) & ".", ADMIN_LOG)
        AlertMsg(BanPlayerindex, DialogueMsg.Banned)
    End Sub

#End Region

#Region "Data Functions"
    Function JobData(jobNum As Integer) As Byte()
        Dim n As Integer, q As Integer
        Dim buffer As New ByteStream(4)

        buffer.WriteString((Type.Job(jobNum).Name))
        buffer.WriteString((Type.Job(jobNum).Desc))

        buffer.WriteInt32(Type.Job(jobNum).MaleSprite)
        buffer.WriteInt32(Type.Job(jobNum).FemaleSprite)

        For i = 1 To StatType.Count - 1
            buffer.WriteInt32(Type.Job(jobNum).Stat(i))
        Next

        For q = 1 To 5
            buffer.WriteInt32(Type.Job(jobNum).StartItem(q))
            buffer.WriteInt32(Type.Job(jobNum).StartValue(q))
        Next

        buffer.WriteInt32(Type.Job(jobNum).StartMap)
        buffer.WriteByte(Type.Job(jobNum).StartX)
        buffer.WriteByte(Type.Job(jobNum).StartY)
        buffer.WriteInt32(Type.Job(jobNum).BaseExp)

        Return buffer.ToArray()
    End Function

    Function NpcsData() As Byte()
        Dim buffer As New ByteStream(4)

        For i = 1 To MAX_NPCS
            If Not Len(Type.NPC(i).Name) > 0 Then Continue For
            buffer.WriteBlock(NpcData(i))
        Next
        Return buffer.ToArray
    End Function

    Function NpcData(NpcNum As Integer) As Byte()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(NpcNum)
        buffer.WriteInt32(Type.NPC(NPCNum).Animation)
        buffer.WriteString(Type.NPC(NPCNum).AttackSay)
        buffer.WriteByte(Type.NPC(NPCNum).Behaviour)

        For i = 1 To MAX_DROP_ITEMS
            buffer.WriteInt32(Type.NPC(NPCNum).DropChance(i))
            buffer.WriteInt32(Type.NPC(NPCNum).DropItem(i))
            buffer.WriteInt32(Type.NPC(NPCNum).DropItemValue(i))
        Next

        buffer.WriteInt32(Type.NPC(NPCNum).Exp)
        buffer.WriteByte(Type.NPC(NPCNum).Faction)
        buffer.WriteInt32(Type.NPC(NPCNum).HP)
        buffer.WriteString((Type.NPC(NPCNum).Name))
        buffer.WriteByte(Type.NPC(NPCNum).Range)
        buffer.WriteByte(Type.NPC(NPCNum).SpawnTime)
        buffer.WriteInt32(Type.NPC(NPCNum).SpawnSecs)
        buffer.WriteInt32(Type.NPC(NPCNum).Sprite)

        For i = 1 To StatType.Count - 1
            buffer.WriteByte(Type.NPC(NPCNum).Stat(i))
        Next

        For i = 1 To MAX_NPC_SKILLS
            buffer.WriteByte(Type.NPC(NPCNum).Skill(i))
        Next

        buffer.WriteInt32(Type.NPC(NPCNum).Level)
        buffer.WriteInt32(Type.NPC(NPCNum).Damage)
        Return buffer.ToArray
    End Function

    Function ShopsData() As Byte()
        Dim buffer As New ByteStream(4)

        For i = 1 To MAX_SHOPS
            If Not Len(Type.Shop(i).Name) > 0 Then Continue For
            buffer.WriteBlock(ShopData(i))
        Next
        Return buffer.ToArray
    End Function

    Function ShopData(shopNum As Integer) As Byte()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(shopNum)
        buffer.WriteInt32(Type.Shop(shopNum).BuyRate)
        buffer.WriteString((Type.Shop(shopNum).Name))

        For i = 1 To MAX_TRADES
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostItem)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).CostValue)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).Item)
            buffer.WriteInt32(Type.Shop(shopNum).TradeItem(i).ItemValue)
        Next
        Return buffer.ToArray
    End Function

    Function SkillsData() As Byte()
        Dim buffer As New ByteStream(4)

        For i = 1 To MAX_SKILLS
            If Not Len(Type.Skill(i).Name) > 0 Then Continue For
            buffer.WriteBlock(SkillData(i))
        Next
        Return buffer.ToArray
    End Function

    Function SkillData(skillnum As Integer) As Byte()
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(skillnum)
        buffer.WriteInt32(Type.Skill(skillNum).AccessReq)
        buffer.WriteInt32(Type.Skill(skillNum).AoE)
        buffer.WriteInt32(Type.Skill(skillNum).CastAnim)
        buffer.WriteInt32(Type.Skill(skillNum).CastTime)
        buffer.WriteInt32(Type.Skill(skillNum).CdTime)
        buffer.WriteInt32(Type.Skill(skillNum).JobReq)
        buffer.WriteInt32(Type.Skill(skillNum).Dir)
        buffer.WriteInt32(Type.Skill(skillNum).Duration)
        buffer.WriteInt32(Type.Skill(skillNum).Icon)
        buffer.WriteInt32(Type.Skill(skillNum).Interval)
        buffer.WriteInt32(Type.Skill(skillNum).IsAoE)
        buffer.WriteInt32(Type.Skill(skillNum).LevelReq)
        buffer.WriteInt32(Type.Skill(skillNum).Map)
        buffer.WriteInt32(Type.Skill(skillNum).MpCost)
        buffer.WriteString((Type.Skill(skillNum).Name))
        buffer.WriteInt32(Type.Skill(skillNum).Range)
        buffer.WriteInt32(Type.Skill(skillNum).SkillAnim)
        buffer.WriteInt32(Type.Skill(skillNum).StunDuration)
        buffer.WriteInt32(Type.Skill(skillNum).Type)
        buffer.WriteInt32(Type.Skill(skillNum).Vital)
        buffer.WriteInt32(Type.Skill(skillNum).X)
        buffer.WriteInt32(Type.Skill(skillNum).Y)
        buffer.WriteInt32(Type.Skill(skillNum).IsProjectile)
        buffer.WriteInt32(Type.Skill(skillNum).Projectile)
        buffer.WriteInt32(Type.Skill(skillNum).KnockBack)
        buffer.WriteInt32(Type.Skill(skillNum).KnockBackTiles)
        Return buffer.ToArray
    End Function


#End Region

End Module