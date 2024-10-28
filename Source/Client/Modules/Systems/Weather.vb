Imports System.IO
Imports Core

Module Weather
    
#Region "Functions"

    Sub ProcessWeather()
        Dim i As Integer, x As Integer

        If GameState.CurrentWeather > 0 And GameState.CurrentWeather < [Enum].Weather.Fog Then
            If GameState.CurrentWeather = [Enum].Weather.Rain Or GameState.CurrentWeather = [Enum].Weather.Storm Then
                PlayWeatherSound("Rain.ogg", True)
            End If

            x = Rand(1, 101 - GameState.CurrentWeatherIntensity)
            If x = 1 Then
                'Add a new particle
                For i = 0 To GameState.MaxWeatherParticles
                    If GameState.WeatherParticle(i).InUse = 0 Then
                        If Rand(1, 3) = 1 Then
                            GameState.WeatherParticle(i).InUse = 1
                            GameState.WeatherParticle(i).Type = GameState.CurrentWeather
                            GameState.WeatherParticle(i).Velocity = Rand(8, 14)
                            GameState.WeatherParticle(i).X = (GameState.TileView.Left * 32) - 32
                            GameState.WeatherParticle(i).Y = ((GameState.TileView.Top * 32) + Rand(-32, ))
                        Else
                            GameState.WeatherParticle(i).InUse = 1
                            GameState.WeatherParticle(i).Type = GameState.CurrentWeather
                            GameState.WeatherParticle(i).Velocity = Rand(10, 15)
                            GameState.WeatherParticle(i).X = ((GameState.TileView.Left * 32) + Rand(-32, GameState.ResolutionWidth))
                            GameState.WeatherParticle(i).Y = (GameState.TileView.Top * 32) - 32
                        End If
                    End If
                Next
            End If
        Else
            StopWeatherSound()
        End If
        
        If GameState.CurrentWeather = [Enum].Weather.Storm Then
            x = Rand(1, 400 - GameState.CurrentWeatherIntensity)
            If x = 1 Then
                GameState.DrawThunder = Rand(15, 22)
                PlayExtraSound("Thunder.ogg")
            End If
        End If
        
        For i = 0 To GameState.MaxWeatherParticles
            If GameState.WeatherParticle(i).InUse = 1 Then
                If GameState.WeatherParticle(i).X > GameState.TileView.Right * 32 Or GameState.WeatherParticle(i).Y > GameState.TileView.Bottom * 32 Then
                    GameState.WeatherParticle(i).InUse = 0
                Else
                    GameState.WeatherParticle(i).X = GameState.WeatherParticle(i).X + GameState.WeatherParticle(i).Velocity
                    GameState.WeatherParticle(i).Y = GameState.WeatherParticle(i).Y + GameState.WeatherParticle(i).Velocity
                End If
            End If
        Next

    End Sub

#End Region

End Module