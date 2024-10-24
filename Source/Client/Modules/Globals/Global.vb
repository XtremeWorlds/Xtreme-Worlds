Imports Core
Imports Microsoft.Xna.Framework
Imports Mirage.Sharp.Asfw.IO.Encryption

Module [Global]
    Public Started As Boolean
    Public ResolutionHeight As Integer
    Public ResolutionWidth As Integer

    Public MapAnim As Byte

    ' Global dialogue index
    Public diaHeader As String
    Public diaBody As String
    Public diaBody2 As String
    Public diaIndex As Long
    Public diaData1 As Long
    Public diaData2 As Long
    Public diaData3 As Long
    Public diaData4 As Long
    Public diaData5 As Long
    Public diaDataString As String
    Public diaStyle As Byte

    ' shop
    Public shopSelectedSlot As Long
    Public shopSelectedItem As Long
    Public shopIsSelling As Boolean

    ' right click menu
    Public PlayerMenuIndex As Long

    ' description
    Public descType As Byte
    Public descItem As Long
    Public descLastType As Byte
    Public descLastItem As Long
    Public descText() As TextStruct

    ' New char
    Public newCharSprite As Long
    Public newCharJob As Long
    Public newCharGender As Long

    ' chars
    Public CharName(MAX_CHARS) As String
    Public CharSprite(MAX_CHARS) As Long
    Public CharAccess(MAX_CHARS) As Long
    Public CharJob(MAX_CHARS) As Long
    Public CharNum As Byte

    ' elastic bars
    Public BarWidth_NpcHP(MAX_MAP_NPCS) As Long
    Public BarWidth_PlayerHP(MAX_PLAYERS) As Long
    Public BarWidth_PlayerSP(MAX_PLAYERS) As Long
    Public BarWidth_NpcHP_Max(MAX_MAP_NPCS) As Long
    Public BarWidth_PlayerHP_Max(MAX_PLAYERS) As Long
    Public BarWidth_PlayerSP_Max(MAX_PLAYERS) As Long
    Public BarWidth_GuiHP As Long
    Public BarWidth_GuiSP As Long
    Public BarWidth_GuiEXP As Long
    Public BarWidth_GuiHP_Max As Long
    Public BarWidth_GuiSP_Max As Long
    Public BarWidth_GuiEXP_Max As Long

    Public CurrentEvents As Integer

    ' Directional blocking
    Public DirArrowX(4) As Byte
    Public DirArrowY(4) As Byte

    Public UseFade As Boolean
    Public FadeType As Integer
    Public FadeAmount As Integer
    Public FlashTimer As Integer

    ' Targetting
    Public MyTarget As Integer
    Public MyTargetType As Integer

    ' Chat bubble
    Public ChatBubble(Byte.MaxValue) As ChatBubbleStruct
    Public ChatBubbleindex As Integer

    ' Gui
    Public Lps As Integer
    Public PingToDraw As String
    Public ShowRClick As Boolean

    Public chatShowLine As String

    ' chat
    Public inSmallChat As Boolean
    Public actChatHeight As Long
    Public actChatWidth As Long
    Public ChatButtonUp As Boolean
    Public ChatButtonDown As Boolean
    Public ChatScroll As Long
    Public Chat_HighIndex As Long

    Public EditorTileX As Integer
    Public EditorTileY As Integer
    Public EditorTileWidth As Integer
    Public EditorTileHeight As Integer
    Public EditorWarpMap As Integer
    Public EditorWarpX As Integer
    Public EditorWarpY As Integer
    Public EditorShop As Integer
    Public EditorAnimation As Integer
    Public EditorLight As Integer
    Public EditorShadow As Byte
    Public EditorFlicker As Byte
    Public EditorAttribute As Byte
    Public EditorTileSelStart As Microsoft.Xna.Framework.Point
    Public EditorTileSelEnd As Microsoft.Xna.Framework.Point
    Public CopyMap As Boolean
    Public TmpMaxX As Byte
    Public TmpMaxY As Byte
    Public TileHistoryHighIndex As Integer

    ' Player variables
    Public MyIndex As Integer ' Index of actual player

    Public InBank As Boolean

    Public InventoryItemSelected As Integer
    Public SkillBuffer As Integer
    Public SkillBufferTimer As Integer
    Public StunDuration As Integer
    Public NextlevelExp As Integer

    ' Stops movement when updating a map
    Public CanMoveNow As Boolean

    ' Controls main gameloop
    Public InGame As Boolean
    Public InMenu As Boolean

    Public IsLogging As Boolean
    Public MapData As Boolean
    Public PlayerData As Boolean

    ' Draw map name location
    Public DrawMapNameX As Single = 10
    Public DrawMapNameY As Single = 90
    Public DrawLocX As Single = 10
    Public DrawLocY As Single = 0
    Public DrawMapNameColor As Microsoft.Xna.Framework.Color

    ' Game direction vars
    Public DirUp As Boolean
    Public DirDown As Boolean
    Public DirLeft As Boolean
    Public DirRight As Boolean

    ' Used for dragging Picture Boxes
    Public SOffsetX As Integer
    Public SOffsetY As Integer

    ' Used to freeze controls when getting a new map
    Public GettingMap As Boolean

    ' Used to check if FPS needs to be drawn
    Public Bfps As Boolean
    Public Blps As Boolean
    Public BLoc As Boolean

    ' FPS and Time-based movement vars
    Public ElapsedTime As Integer
    Public GameLps As Integer

    ' Text vars
    Public VbQuote As String

    ' Mouse cursor tile location
    Public CurX As Integer
    Public CurY As Integer
    Public CurMouseX As Integer
    Public CurMouseY As Integer
    Public L

    ' Game editors
    Public MyEditorType As Integer
    Public EditorIndex As Integer

    ' Spawn
    Public SpawnNpcNum As Integer
    Public SpawnNpcDir As Integer

    ' Items
    Public ItemEditorNum As Integer
    Public ItemEditorValue As Integer

    ' Resources
    Public ResourceEditorNum As Integer

    ' Used for map editor heal & trap & slide tiles
    Public MapEditorHealType As Integer
    Public MapEditorHealAmount As Integer
    Public MapEditorSlideDir As Integer

    Public Camera As Rectangle
    Public TileView As RectStruct

    ' Pinging
    Public PingStart As Integer
    Public PingEnd As Integer
    Public Ping As Integer

    ' Indexing
    Public ActionMsgIndex As Byte
    Public BloodIndex As Byte

    Public TempMapData() As Byte

    Public ShakeTimerEnabled As Boolean
    Public ShakeTimer As Integer
    Public ShakeCount As Byte
    Public LastDir As Byte

    Public ShowAnimLayers As Boolean
    Public ShowAnimTimer As Integer

    Public EKeyPair As New KeyPair()

    ' Stream Content
    Public Item_Loaded(MAX_ITEMS)
    Public NPC_Loaded(MAX_NPCS)
    Public Resource_Loaded(MAX_RESOURCES)
    Public Animation_Loaded(MAX_ANIMATIONS)
    Public Skill_Loaded(MAX_SKILLS)
    Public Shop_Loaded(MAX_SHOPS)
    Public Pet_Loaded(MAX_PETS)
    Public Moral_Loaded(MAX_MORALS)

    Public AnimEditorFrame(1) As Integer
    Public AnimEditorTimer(1) As Integer

    Public MovementSpeed As Double
    Public CurrentCameraX As Double
    Public CurrentCameraY As Double

    ' Number of graphic files
    Friend NumTileSets As Integer
    Friend NumCharacters As Integer
    Friend NumPaperdolls As Integer
    Friend NumItems As Integer
    Friend NumResources As Integer
    Friend NumAnimations As Integer
    Friend NumSkills As Integer
    Friend NumFogs As Integer
    Friend NumEmotes As Integer
    Friend NumPanoramas As Integer
    Friend NumParallax As Integer
    Friend NumPictures As Integer
    Friend NumInterface As Integer
    Friend NumGradients As Integer
    Friend NumDesigns As Integer
    
    Friend GameDestroyed As Boolean

    Friend VbKeyRight As Boolean
    Friend VbKeyLeft As Boolean
    Friend VbKeyUp As Boolean
    Friend VbKeyDown As Boolean
    Friend VbKeyShift As Boolean
    Friend VbKeyControl As Boolean
    Friend VbKeyAlt As Boolean
    Friend VbKeyEnter As Boolean
End Module