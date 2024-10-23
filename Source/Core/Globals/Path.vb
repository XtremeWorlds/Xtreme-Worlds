Imports System.IO
Imports System.Reflection

Public Module [Path]
    ''' <summary> Returns the application directory </summary>
    Public ReadOnly Property Local As String
        Get
            Dim assemblyPath = Assembly.GetEntryAssembly().Location
            Return IO.Directory.GetParent(assemblyPath).FullName
        End Get
    End Property

    ''' <summary> Returns content directory </summary>
    Public ReadOnly Property Content As String
        Get
            Return IO.Path.Combine(Local, "Content")
        End Get
    End Property

    ''' <summary> Returns config directory </summary>
    Public ReadOnly Property Config As String
        Get
            Return IO.Path.Combine(Local, "Configuration")
        End Get
    End Property

    ''' <summary> Returns scripts directory </summary>
    Public ReadOnly Property Scripts As String
        Get
            Return IO.Path.Combine(Local, "Scripting", "Scripts")
        End Get
    End Property

    ''' <summary> Returns graphics directory </summary>
    Public ReadOnly Property Graphics As String
        Get
            Return IO.Path.Combine(Content, "Graphics")
        End Get
    End Property

    ''' <summary> Returns Fonts directory </summary>
    Public ReadOnly Property Fonts As String
        Get
            Return IO.Path.Combine(Content, "Fonts")
        End Get
    End Property

    ''' <summary> Returns GUI directory </summary>
    Public ReadOnly Property Gui As String
        Get
            Return IO.Path.Combine(Graphics, "Gui")
        End Get
    End Property

    ''' <summary> Returns gradients directory </summary>
    Public ReadOnly Property Gradients As String
        Get
            Return IO.Path.Combine(Gui, "Gradients")
        End Get
    End Property

    ''' <summary> Returns designs directory </summary>
    Public ReadOnly Property Designs As String
        Get
            Return IO.Path.Combine(Gui, "Designs")
        End Get
    End Property

    ''' <summary> Returns tilesets directory </summary>
    Public ReadOnly Property Tilesets As String
        Get
            Return IO.Path.Combine(Graphics, "Tilesets")
        End Get
    End Property

    ''' <summary> Returns characters directory </summary>
    Public ReadOnly Property Characters As String
        Get
            Return IO.Path.Combine(Graphics, "Characters")
        End Get
    End Property

    ''' <summary> Returns emotes directory </summary>
    Public ReadOnly Property Emotes As String
        Get
            Return IO.Path.Combine(Graphics, "Emotes")
        End Get
    End Property

    ''' <summary> Returns paperdolls directory </summary>
    Public ReadOnly Property Paperdolls As String
        Get
            Return IO.Path.Combine(Graphics, "Paperdolls")
        End Get
    End Property

    ''' <summary> Returns fogs directory </summary>
    Public ReadOnly Property Fogs As String
        Get
            Return IO.Path.Combine(Graphics, "Fogs")
        End Get
    End Property

    ''' <summary> Returns parallax directory </summary>
    Public ReadOnly Property Parallax As String
        Get
            Return IO.Path.Combine(Graphics, "Parallax")
        End Get
    End Property

    ''' <summary> Returns panoramas directory </summary>
    Public ReadOnly Property Panoramas As String
        Get
            Return IO.Path.Combine(Graphics, "Panoramas")
        End Get
    End Property

    ''' <summary> Returns pictures directory </summary>
    Public ReadOnly Property Pictures As String
        Get
            Return IO.Path.Combine(Graphics, "Pictures")
        End Get
    End Property

    ''' <summary> Returns logs directory </summary>
    Public ReadOnly Property Logs As String
        Get
            Return IO.Path.Combine(Local, "Logs")
        End Get
    End Property

    ''' <summary> Returns database directory </summary>
    Public ReadOnly Property Database As String
        Get
            Return IO.Path.Combine(Local, "Database")
        End Get
    End Property

    ''' <summary> Returns music directory </summary>
    Public ReadOnly Property Music As String
        Get
            Return IO.Path.Combine(Content, "Music")
        End Get
    End Property

    ''' <summary> Returns sounds directory </summary>
    Public ReadOnly Property Sounds As String
        Get
            Return IO.Path.Combine(Content, "Sounds")
        End Get
    End Property

    ''' <summary> Returns items directory </summary>
    Public ReadOnly Property Items As String
        Get
            Return IO.Path.Combine(Graphics, "Items")
        End Get
    End Property

    ''' <summary> Returns maps directory </summary>
    Public ReadOnly Property Maps As String
        Get
            Return IO.Path.Combine(Graphics, "Maps")
        End Get
    End Property

    ''' <summary> Returns animations directory </summary>
    Public ReadOnly Property Animations As String
        Get
            Return IO.Path.Combine(Graphics, "Animations")
        End Get
    End Property

    ''' <summary> Returns skills directory </summary>
    Public ReadOnly Property Skills As String
        Get
            Return IO.Path.Combine(Graphics, "Skills")
        End Get
    End Property

    ''' <summary> Returns projectiles directory </summary>
    Public ReadOnly Property Projectiles As String
        Get
            Return IO.Path.Combine(Graphics, "Projectiles")
        End Get
    End Property

    ''' <summary> Returns resources directory </summary>
    Public ReadOnly Property Resources As String
        Get
            Return IO.Path.Combine(Graphics, "Resources")
        End Get
    End Property

    ''' <summary> Returns misc directory </summary>
    Public ReadOnly Property Misc As String
        Get
            Return IO.Path.Combine(Graphics, "Misc")
        End Get
    End Property

    ' Helper function to check if a file path has an extension
    Public Function EnsureFileExtension(path As String, Optional defaultExtension As String = ".png") As String
        ' Check if the path has an extension
        If String.IsNullOrWhiteSpace(IO.Path.GetExtension(path)) Then
            ' If not, add the default extension
            Return path & defaultExtension
        End If

        ' Return the original path if it already has an extension
        Return path
    End Function

End Module
