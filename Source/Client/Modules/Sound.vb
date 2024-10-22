Imports System.IO
Imports System.Xml
Imports Core
Imports Core.Path
Imports ManagedBass
Imports ManagedBass.Midi

Module Sound

    ' Sound and Music handles for ManagedBass
    Friend MusicStream As Integer
    Friend SoundStream As Integer
    Friend WeatherStream As Integer
    Friend ExtraSoundStream As Integer

    Friend MusicCache() As String
    Friend SoundCache() As String

    Friend FadeInSwitch As Boolean
    Friend FadeOutSwitch As Boolean
    Friend CurrentMusic As String
    Friend CurrentWeatherMusic As String

    Friend SoundFontHandle As Integer

    Sub PlayMusic(fileName As String)
        Dim path = IO.Path.Combine(Core.Path.Music, fileName)

        If fileName = "None" Then
            Exit Sub
        End If

        If Core.Type.Setting.Music = False Or Not File.Exists(path) Then
            StopMusic()
            Exit Sub
        End If

        If fileName = CurrentMusic Then
            Exit Sub
        End If

        If IO.Path.GetExtension(fileName).ToLower() = ".mid" Then
            StopMusic()
            PlayMidi(path)
            CurrentMusic = fileName
            Exit Sub
        End If

        Try
            StopMusic() ' Stop any currently playing music before starting a new one

            MusicStream = Bass.CreateStream(path, 0, 0, BassFlags.Loop)
            If MusicStream <> 0 Then
                Bass.ChannelPlay(MusicStream)
                Bass.ChannelSetAttribute(MusicStream, ChannelAttribute.Volume, Core.Type.Setting.MusicVolume / 100.0F)
                CurrentMusic = fileName
                FadeInSwitch = True
            End If
        Catch ex As Exception
            Console.WriteLine($"Error playing music: {ex.Message}")
        End Try
    End Sub

    Sub InitializeBASS()
        ' Initialize BASS with the default output device
        If Not Bass.Init(-1, 44100, DeviceInitFlags.Default) Then
            Console.WriteLine($"Failed to initialize BASS. Error: {Bass.LastError}")
            Exit Sub
        End If

        ' Load the SoundFont (.sf2) for MIDI playback
        Dim soundFontPath As String = "GeneralUser.sf2"
        If Not File.Exists(soundFontPath) Then
            Console.WriteLine($"SoundFont not found: {soundFontPath}")
            Exit Sub
        End If

        ' Initialize the SoundFont
        SoundFontHandle = BassMidi.FontInit(soundFontPath, BassFlags.Default)
        If SoundFontHandle = 0 Then
            Console.WriteLine($"Failed to load SoundFont. Error: {Bass.LastError}")
            Exit Sub
        End If

        ' Set the volume for the SoundFont
        BassMidi.FontSetVolume(SoundFontHandle, 1.0F) ' 100% volume

        CacheMusic()
        CacheSound()
    End Sub

    Sub PlayMidi(filePath As String)
        StopMusic() ' Ensure previous music is stopped

        ' Load and play the MIDI file using ManagedBass.Midi
        MusicStream = BassMidi.CreateStream(filePath, 0, 0, BassFlags.Loop, 44100)

        ' Correctly set the SoundFont for the MIDI stream
        Dim font As New MidiFont With {
            .Handle = SoundFontHandle, ' Use the SoundFont handle
            .Preset = -1, ' -1 means all presets from the SoundFont
            .Bank = 0 ' Bank 0 (General MIDI bank)
        }

        ' Create an array with the MidiFont structure
        Dim fonts() As MidiFont = {font}

        ' Set the fonts for the MIDI stream and check if it succeeded
        Dim fontCount As Integer = BassMidi.StreamSetFonts(MusicStream, fonts, fonts.Length)

        If fontCount = 0 Then
            Console.WriteLine($"Failed to assign SoundFont. Error: {Bass.LastError}")
        End If

        ' Ensure the file exists before attempting to load it
        If Not File.Exists(filePath) Then
            Console.WriteLine($"MIDI file not found: {filePath}")
            Exit Sub
        End If

        If MusicStream <> 0 Then
            Bass.ChannelPlay(MusicStream)
            Bass.ChannelSetAttribute(MusicStream, ChannelAttribute.Volume, Core.Type.Setting.MusicVolume / 100.0F)
        Else
            ' Log the last error if stream creation fails
            Dim errorCode As Errors = Bass.LastError
            Console.WriteLine($"Failed to load MIDI file. Error: {errorCode}")
        End If
    End Sub

    Sub StopMusic()
        If MusicStream <> 0 Then
            Bass.ChannelStop(MusicStream)
            Bass.StreamFree(MusicStream)
            MusicStream = 0
            CurrentMusic = ""
        End If
    End Sub

    Sub PlaySound(fileName As String, x As Integer, y As Integer, Optional looped As Boolean = False)
        If Core.Type.Setting.Sound = 0 Or Not File.Exists(Core.Path.Sounds & fileName) Then Exit Sub

        Try
            StopSound() ' Stop previous sound if any

            SoundStream = Bass.CreateStream(Core.Path.Sounds & fileName, 0, 0, If(looped, BassFlags.Loop, BassFlags.Default))
            If SoundStream <> 0 Then
                Dim calculatedVolume As Double = CalculateSoundVolume(x, y)
                Bass.ChannelSetAttribute(SoundStream, ChannelAttribute.Volume, calculatedVolume / 100.0F)
                Bass.ChannelPlay(SoundStream)
            End If
        Catch ex As Exception
            Console.WriteLine($"Failed to load sound: {ex.Message}")
        End Try
    End Sub

    Sub StopSound()
        If SoundStream <> 0 Then
            Bass.ChannelStop(SoundStream)
            Bass.StreamFree(SoundStream)
            SoundStream = 0
        End If
    End Sub

    Sub PlayExtraSound(fileName As String, Optional looped As Boolean = False)
        If Core.Type.Setting.Sound = 0 Or Not File.Exists(Core.Path.Sounds & fileName) Then Exit Sub

        Try
            StopExtraSound()

            ExtraSoundStream = Bass.CreateStream(Core.Path.Sounds & fileName, 0, 0, If(looped, BassFlags.Loop, BassFlags.Default))
            If ExtraSoundStream <> 0 Then
                Bass.ChannelSetAttribute(ExtraSoundStream, ChannelAttribute.Volume, Core.Type.Setting.SoundVolume / 100.0F)
                Bass.ChannelPlay(ExtraSoundStream)
            End If
        Catch ex As Exception
            Console.WriteLine($"Failed to load extra sound: {ex.Message}")
        End Try
    End Sub

    Sub StopExtraSound()
        If ExtraSoundStream <> 0 Then
            Bass.ChannelStop(ExtraSoundStream)
            Bass.StreamFree(ExtraSoundStream)
            ExtraSoundStream = 0
        End If
    End Sub

    ' The fade methods should directly adjust the volume of the ManagedBass streams
    Sub FadeIn()
        If MusicStream <> 0 Then
            Dim currentVolume As Single
            Bass.ChannelGetAttribute(MusicStream, ChannelAttribute.Volume, currentVolume)
            If currentVolume < Core.Type.Setting.MusicVolume / 100.0F Then
                Bass.ChannelSetAttribute(MusicStream, ChannelAttribute.Volume, currentVolume + 0.03F)
            Else
                FadeInSwitch = False
            End If
        End If
    End Sub

    Sub FadeOut()
        If MusicStream <> 0 Then
            Dim currentVolume As Single
            Bass.ChannelGetAttribute(MusicStream, ChannelAttribute.Volume, currentVolume)
            If currentVolume > 0 Then
                Bass.ChannelSetAttribute(MusicStream, ChannelAttribute.Volume, currentVolume - 0.03F)
            Else
                FadeOutSwitch = False
                StopMusic()
            End If
        End If
    End Sub

    Public Function CalculateSoundVolume(ByRef x As Integer, ByRef y As Integer) As Double
        Dim X1, X2, Y1, Y2 As Integer
        Dim Distance As Double

        If Not InGame Then
            CalculateSoundVolume = 1
            Return CalculateSoundVolume
        End If

        If InGame AndAlso x = GetPlayerX(MyIndex) AndAlso y = GetPlayerY(MyIndex) Then
            CalculateSoundVolume = 1
            CalculateSoundVolume *= Core.Type.Setting.SoundVolume
            Return CalculateSoundVolume
        End If

        If x > -1 OrElse y > -1 Then
            If x = -1 Then x = 0
            If y = -1 Then y = 0
            X1 = CInt((Core.Type.Player(MyIndex).X * 32) + Core.Type.Player(MyIndex).XOffset)
            Y1 = CInt((Core.Type.Player(MyIndex).Y * 32) + Core.Type.Player(MyIndex).YOffset)
            X2 = x * 32
            Y2 = y * 32

            If CInt(Math.Pow((X2 - X1), 2)) + CInt(Math.Pow((Y2 - Y1), 2)) < 0 Then
                Distance = Math.Sqrt(CInt(Math.Pow((X2 - X1), 2)) + CInt(Math.Pow((Y2 - Y1), 2)) * -1)
            Else
                Distance = Math.Sqrt(CInt(Math.Pow((X2 - X1), 2)) + CInt(Math.Pow((Y2 - Y1), 2)))
            End If

            ' If the range is greater than 32, do not send a sound
            If (Distance / 32) > 32 Then
                CalculateSoundVolume = 0
            Else
                CalculateSoundVolume = 1 / (Distance / 32)

                If CalculateSoundVolume > 1 Then
                    CalculateSoundVolume = 1
                ElseIf CalculateSoundVolume < 0 Then
                    CalculateSoundVolume *= -1
                End If
            End If
        Else
            CalculateSoundVolume = 1
        End If

        CalculateSoundVolume *= Core.Type.Setting.SoundVolume

        Return CalculateSoundVolume
    End Function

    Sub FreeBASS()
        If SoundFontHandle <> 0 Then
            BassMidi.FontFree(SoundFontHandle)
        End If
        Bass.Free()
    End Sub

    Sub PlayWeatherSound(fileName As String, Optional looped As Boolean = False)
        ' Check if sound is enabled and the file exists
        If Not Core.Type.Setting.Sound = 1 Or Not File.Exists(Core.Path.Sounds & fileName) Then Exit Sub

        ' Avoid reloading the same sound if it's already playing
        If CurrentWeatherMusic = fileName Then Exit Sub

        ' Stop any previously playing weather sound
        StopWeatherSound()

        ' Load the new sound file
        Dim soundPath As String = Core.Path.Sounds & fileName
        WeatherStream = Bass.CreateStream(soundPath, 0, 0, If(looped, BassFlags.Loop, BassFlags.Default))

        ' Check if the stream was created successfully
        If WeatherStream <> 0 Then
            ' Play the sound
            Bass.ChannelSetAttribute(WeatherStream, ChannelAttribute.Volume, Core.Type.Setting.SoundVolume / 100.0F)
            Bass.ChannelPlay(WeatherStream, False)
            CurrentWeatherMusic = fileName
        Else
            Console.WriteLine("Failed to play sound: " & Bass.LastError.ToString())
        End If
    End Sub

    Sub StopWeatherSound()
        ' Stop and free the current weather sound channel
        If WeatherStream <> 0 Then
            Bass.ChannelStop(WeatherStream)
            Bass.StreamFree(WeatherStream)
            WeatherStream = 0
        End If

        CurrentWeatherMusic = ""
    End Sub

End Module