Imports Core
Imports Microsoft.Xna.Framework
Imports Mirage.Sharp.Asfw.IO.Encryption

Public Class GameState
    Private Shared ReadOnly stateLock As New Object
    
    Public Sub New()
    End Sub
    
    Public Shared ResolutionHeight As Integer
    Public Shared ResolutionWidth As Integer

    Public Shared MapAnim As Byte

    ' Global dialogue index
    Public Shared diaHeader As String
    Public Shared diaBody As String
    Public Shared diaBody2 As String
    Public Shared diaIndex As Long
    Public Shared diaData1 As Long
    Public Shared diaData2 As Long
    Public Shared diaData3 As Long
    Public Shared diaData4 As Long
    Public Shared diaData5 As Long
    Public Shared diaDataString As String
    Public Shared diaStyle As Byte

    ' shop
    Public Shared shopSelectedSlot As Long
    Public Shared shopSelectedItem As Long
    Public Shared shopIsSelling As Boolean

    ' right click menu
    Public Shared PlayerMenuIndex As Long

    ' description
    Public Shared descType As Byte
    Public Shared descItem As Long
    Public Shared descLastType As Byte
    Public Shared descLastItem As Long
    Public Shared descText() As Type.TextStruct

    ' New char
    Public Shared newCharSprite As Long
    Public Shared newCharJob As Long
    Public Shared newCharGender As Long

    ' chars
    Public Shared CharName(MAX_CHARS) As String
    Public Shared CharSprite(MAX_CHARS) As Long
    Public Shared CharAccess(MAX_CHARS) As Long
    Public Shared CharJob(MAX_CHARS) As Long
    Public Shared CharNum As Byte

    ' elastic bars
    Public Shared BarWidth_NpcHP(MAX_MAP_NPCS) As Long
    Public Shared BarWidth_PlayerHP(MAX_PLAYERS) As Long
    Public Shared BarWidth_PlayerSP(MAX_PLAYERS) As Long
    Public Shared BarWidth_NpcHP_Max(MAX_MAP_NPCS) As Long
    Public Shared BarWidth_PlayerHP_Max(MAX_PLAYERS) As Long
    Public Shared BarWidth_PlayerSP_Max(MAX_PLAYERS) As Long
    Public Shared BarWidth_GuiHP As Long
    Public Shared BarWidth_GuiSP As Long
    Public Shared BarWidth_GuiEXP As Long
    Public Shared BarWidth_GuiHP_Max As Long
    Public Shared BarWidth_GuiSP_Max As Long
    Public Shared BarWidth_GuiEXP_Max As Long

    Public Shared CurrentEvents As Integer

    ' Directional blocking
    Public Shared DirArrowX(4) As Byte
    Public Shared DirArrowY(4) As Byte

    Public Shared UseFade As Boolean
    Public Shared FadeType As Integer
    Public Shared FadeAmount As Integer
    Public Shared FlashTimer As Integer

    ' Targetting
    Public Shared MyTarget As Integer
    Public Shared MyTargetType As Integer

    ' Chat bubble
    Public Shared ChatBubbleindex As Integer
    
    Public Shared chatShowLine As String

    ' chat
    Public Shared inSmallChat As Boolean
    Public Shared actChatHeight As Long
    Public Shared actChatWidth As Long
    Public Shared ChatButtonUp As Boolean
    Public Shared ChatButtonDown As Boolean
    Public Shared ChatScroll As Long
    Public Shared Chat_HighIndex As Long

    Public Shared EditorTileX As Integer
    Public Shared EditorTileY As Integer
    Public Shared EditorTileWidth As Integer
    Public Shared EditorTileHeight As Integer
    Public Shared EditorWarpMap As Integer
    Public Shared EditorWarpX As Integer
    Public Shared EditorWarpY As Integer
    Public Shared EditorShop As Integer
    Public Shared EditorAnimation As Integer
    Public Shared EditorLight As Integer
    Public Shared EditorShadow As Byte
    Public Shared EditorFlicker As Byte
    Public Shared EditorAttribute As Byte
    Public Shared EditorTileSelStart As Microsoft.Xna.Framework.Point
    Public Shared EditorTileSelEnd As Microsoft.Xna.Framework.Point
    Public Shared CopyMap As Boolean
    Public Shared TmpMaxX As Byte
    Public Shared TmpMaxY As Byte
    Public Shared TileHistoryHighIndex As Integer

    ' Player variables
    Public Shared MyIndex As Integer ' Index of actual player

    Public Shared InBank As Boolean
    
    Public Shared SkillBuffer As Integer
    Public Shared SkillBufferTimer As Integer
    Public Shared StunDuration As Integer
    Public Shared NextlevelExp As Integer

    ' Stops movement when updating a map
    Public Shared CanMoveNow As Boolean

    ' Controls main gameloop
    Public Shared InGame As Boolean
    Public Shared InMenu As Boolean
    
    Public Shared MapData As Boolean
    Public Shared PlayerData As Boolean

    ' Draw map name location
    Public Shared DrawLocX As Single = 10
    Public Shared DrawLocY As Single = 0
    Public Shared DrawMapNameColor As Color

    ' Game direction vars
    Public Shared DirUp As Boolean
    Public Shared DirDown As Boolean
    Public Shared DirLeft As Boolean
    Public Shared DirRight As Boolean

    ' Used to freeze controls when getting a new map
    Public Shared GettingMap As Boolean

    ' Used to check if FPS needs to be drawn
    Public Shared Bfps As Boolean
    Public Shared Blps As Boolean
    Public Shared BLoc As Boolean

    ' FPS and Time-based movement vars
    Public Shared ElapsedTime As Integer

    ' Mouse cursor tile location
    Public Shared CurX As Integer
    Public Shared CurY As Integer
    Public Shared CurMouseX As Integer
    Public Shared CurMouseY As Integer

    ' Game editors
    Public Shared MyEditorType As Integer
    Public Shared EditorIndex As Integer

    ' Spawn
    Public Shared SpawnNpcNum As Integer
    Public Shared SpawnNpcDir As Integer

    ' Items
    Public Shared ItemEditorNum As Integer
    Public Shared ItemEditorValue As Integer

    ' Resources
    Public Shared ResourceEditorNum As Integer

    ' Used for map editor heal & trap & slide tiles
    Public Shared MapEditorHealType As Integer
    Public Shared MapEditorHealAmount As Integer
    Public Shared MapEditorSlideDir As Integer

    Public Shared Camera As Rectangle
    Public Shared TileView As RectStruct

    ' Pinging
    Public Shared PingStart As Integer
    Public Shared PingEnd As Integer
    Public Shared Ping As Integer

    ' Indexing
    Public Shared ActionMsgIndex As Byte
    Public Shared BloodIndex As Byte

    Public Shared TempMapData() As Byte

    Public Shared ShakeTimerEnabled As Boolean
    Public Shared ShakeTimer As Integer
    Public Shared ShakeCount As Byte
    Public Shared LastDir As Byte

    Public Shared ShowAnimLayers As Boolean
    Public Shared ShowAnimTimer As Integer

    Public Shared EKeyPair As New KeyPair()

    ' Stream Content
    Public Shared Item_Loaded(MAX_ITEMS)
    Public Shared NPC_Loaded(MAX_NPCS)
    Public Shared Resource_Loaded(MAX_RESOURCES)
    Public Shared Animation_Loaded(MAX_ANIMATIONS)
    Public Shared Skill_Loaded(MAX_SKILLS)
    Public Shared Shop_Loaded(MAX_SHOPS)
    Public Shared Pet_Loaded(MAX_PETS)
    Public Shared Moral_Loaded(MAX_MORALS)

    Public Shared AnimEditorFrame(1) As Integer
    Public Shared AnimEditorTimer(1) As Integer

    Public Shared MovementSpeed As Double
    Public Shared CurrentCameraX As Double
    Public Shared CurrentCameraY As Double

    ' Number of graphic files
    Public Shared NumTileSets As Integer
    Public Shared NumCharacters As Integer
    Public Shared NumPaperdolls As Integer
    Public Shared NumItems As Integer
    Public Shared NumResources As Integer
    Public Shared NumAnimations As Integer
    Public Shared NumSkills As Integer
    Public Shared NumFogs As Integer
    Public Shared NumEmotes As Integer
    Public Shared NumPanoramas As Integer
    Public Shared NumParallax As Integer
    Public Shared NumPictures As Integer
    Public Shared NumInterface As Integer
    Public Shared NumGradients As Integer
    Public Shared NumDesigns As Integer
    
    Public Shared GameDestroyed As Boolean

    Public Shared VbKeyRight As Boolean
    Public Shared VbKeyLeft As Boolean
    Public Shared VbKeyUp As Boolean
    Public Shared VbKeyDown As Boolean
    Public Shared VbKeyShift As Boolean
    Public Shared VbKeyControl As Boolean
    Public Shared VbKeyAlt As Boolean
    Public Shared VbKeyEnter As Boolean
    
    Public Shared LastLeftClickTime As Integer
    Public Const DoubleClickTImer As Integer = 500 ' Time in milliseconds for double-click detection

    Public Const ChatBubbleWidth As Integer = 300

    Public Const Chat_Timer As Long = 20000

    Public Const EffectTypeFadein As Integer = 1
    Public Const EffectTypeFadeout As Integer = 2
    Public Const EffectTypeFlash As Integer = 3
    Public Const EffectTypeFog As Integer = 4
    Public Const EffectTypeWeather As Integer = 5
    Public Const EffectTypeTint As Integer = 6

    ' Chat variables
    Public Const ChatWidth As Long = 316

    ' Bank constants
    Public Const BankTop As Long = 28
    Public Const BankLeft As Long = 9
    Public Const BankOffsetY As Long = 6
    Public Const BankOffsetX As Long = 6
    Public Const BankColumns As Long = 10

    ' Inventory constants
    Public Const InvTop As Long = 28
    Public Const InvLeft As Long = 9
    Public Const InvOffsetY As Long = 6
    Public Const InvOffsetX As Long = 6
    Public Const InvColumns As Long = 5

    ' Character consts
    Public Const EqTop As Long = 315
    Public Const EqLeft As Long = 11
    Public Const EqOffsetX As Long = 8
    Public Const EqColumns As Long = 4

    ' Skill constants
    Public Const SkillTop As Long = 28
    Public Const SkillLeft As Long = 9
    Public Const SkillOffsetY As Long = 6
    Public Const SkillOffsetX As Long = 6
    Public Const SkillColumns As Long = 5

    ' Hotbar constants
    Public Const HotbarTop As Long = 0
    Public Const HotbarLeft As Long = 8
    Public Const HotbarOffsetX As Long = 41

    ' Shop constants
    Public Const ShopTop As Long = 28
    Public Const ShopLeft As Long = 9
    Public Const ShopOffsetY As Long = 6
    Public Const ShopOffsetX As Long = 6
    Public Const ShopColumns As Long = 7

    ' Trade
    Public Const TradeTop As Long = 0
    Public Const TradeLeft As Long = 0
    Public Const TradeOffsetY As Long = 6
    Public Const TradeOffsetX As Long = 6
    Public Const TradeColumns As Long = 5

    ' Gfx Path and variables
    Public Const GfxExt As String = ".png"

    Public Shared MapGrid As Boolean
    Public Shared EyeDropper As Boolean
    Public Shared HistoryIndex As Integer
    Public Shared HideLayers As Boolean
    Public Shared Night As Boolean

    ' Speed moving vars
    Public Const WalkSpeed As Byte = 8
    Public Const RunSpeed As Byte = 16

    ' Tile size constants
    Public Const PicX As Integer = 32
    Public Const PicY As Integer = 32

    ' Sprite, item, skill size constants
    Public Const SizeX As Integer = 32
    Public Const SizeY As Integer = 32

    ' Map
    Public Const MaxTileHistory As Byte = 50
    Public Const TileSize As Byte = 32 ' Tile size is 32x32 pixels
    
    ' Autotiles
    Public Const AutoInner As Byte = 1

    Public Const AutoOuter As Byte = 2
    Public Const AutoHorizontal As Byte = 3
    Public Const AutoVertical As Byte = 4
    Public Const AutoFill As Byte = 5

    ' Autotile Type
    Public Const AutotileNone As Byte = 0

    Public Const AutotileNormal As Byte = 1
    Public Const AutotileFake As Byte = 2
    Public Const AutotileAnim As Byte = 3
    Public Const AutotileCliff As Byte = 4
    Public Const AutotileWaterfall As Byte = 5

    ' Rendering
    Public Const RenderStateNone As Integer = 0

    Public Const RenderStateNormal As Integer = 1
    Public Const RenderStateAutotile As Integer = 2

    ' Map animations
    Public Shared WaterfallFrame As Integer

    Public Shared AutoTileFrame As Integer
    
    Public Shared NumProjectiles As Integer
    Public Shared InitProjectileEditor As Boolean
    Public Const EditorProjectile As Byte = 10
    Public Shared ProjectileChanged(MAX_PROJECTILES) As Boolean
    
    Public Shared ResourceIndex As Integer
    Public Shared ResourcesInit As Boolean
    
    Public Const MaxWeatherParticles As Integer = 100
    Public Shared WeatherParticle(MaxWeatherParticles) As WeatherParticleStruct

    Public Shared  FogOffsetX As Integer
    Public Shared FogOffsetY As Integer

    Public Shared CurrentWeather As Integer
    Public Shared CurrentWeatherIntensity As Integer
    Public Shared CurrentFog As Integer
    Public Shared CurrentFogSpeed As Integer
    Public Shared CurrentFogOpacity As Integer
    Public Shared CurrentTintR As Integer
    Public Shared CurrentTintG As Integer
    Public Shared CurrentTintB As Integer
    Public Shared CurrentTintA As Integer
    Public Shared DrawThunder As Integer
    
    Public Shared InShop As Integer ' is the player in a shop?
    Public Shared ShopAction As Byte ' stores the current shop action

End Class