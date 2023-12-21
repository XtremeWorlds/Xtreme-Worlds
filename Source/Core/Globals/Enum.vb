﻿Public Module [Enum]

    ''' <Summary> Text Color Contstant </Summary>
    Enum ColorType As Byte
        Black = 0
        Blue
        Green
        Cyan
        Red
        Magenta
        Brown
        Gray
        DarkGray
        BrightBlue
        BrightGreen
        BrightCyan
        BrightRed
        Pink
        Yellow
        White
    End Enum

    ''' <Summary> Sex Constant </Summary>
    Enum SexType As Byte
        None = 0
        Male = 1
        Female
    End Enum

    ''' <Summary> Map Moral Constant </Summary>
    Enum MapMoralType As Byte
        None = 0
        Safe
        Danger
    End Enum

    ''' <Summary> Tile Constant </Summary>
    Enum TileType As Byte
        None = 0
        Blocked
        Warp
        Item
        NpcAvoid
        Resource
        NpcSpawn
        Shop
        Bank
        Heal
        Trap
        Light
        Animation

        Count
    End Enum

    Enum XWTileType As Byte
        None = 0
        Block = 1
        Direction_Block = 15
        Door = 2
        Warp = 10
        Item = 3
        Npc_Avoid = 4
        Key = 5
        Key_Open = 6
        Heal = 8
        Damage = 7
        Sign = 11
        Shop = 13
        Npc = 12
        No_Xing = 19
        Walkthru = 20
        Arena = 9
        Roof = 21
    End Enum

    ''' <Summary> Item Constant </Summary>
    Enum ItemType As Byte
        None = 0
        Equipment
        Consumable
        CommonEvent
        Currency
        Skill
        Projectile
        Pet

        Count
    End Enum

    ''' <Summary> Consumable Constant </Summary>
    Enum ConsumableType As Byte
        None = 0
        HP
        MP
        Sp
        Exp
    End Enum

    ''' <Summary> Direction Constant </Summary>
    Enum DirectionType As Byte
        None = 0
        Up
        Right
        Down
        Left
    End Enum

    ''' <Summary> Movement Constant </Summary>
    Enum MovementType As Byte
        None = 0
        Standing
        Walking
        Running
    End Enum

    ''' <Summary> Admin Constant </Summary>
    Enum AdminType As Byte
        None = 0
        Player
        Moderator
        Mapper
        Developer
        Creator
    End Enum

    ''' <Summary> Npc Behavior Constant </Summary>
    Enum NpcBehavior As Byte
        None = 0
        AttackOnSight
        AttackWhenAttacked
        Friendly
        ShopKeeper
        Guard
        Quest
    End Enum

    ''' <Summary> Skill Constant </Summary>
    Enum SkillType As Byte
        None = 0
        DamageHp
        DamageMp
        HealHp
        HealMp
        Warp
        Pet
    End Enum

    ''' <Summary> Target Constant </Summary>
    Enum TargetType As Byte
        None = 0
        Player
        Npc
        [Event]
        Pet
    End Enum

    ''' <Summary> Action Message Constant </Summary>
    Enum ActionMsgType As Byte
        None = 0
        [Static]
        Scroll
        Screen
    End Enum

    ''' <Summary> Stats used by Players, Npcs and Job </Summary>
    Public Enum StatType As Byte
        None = 0
        Strength
        Endurance
        Vitality
        Luck
        Intelligence
        Spirit

        Count
    End Enum

    ''' <Summary> Vitals used by Players, Npcs, and Job </Summary>
    Public Enum VitalType As Byte
        None = 0
        HP
        MP
        SP

        Count
    End Enum

    ''' <Summary> Equipment used by Players </Summary>
    Public Enum EquipmentType As Byte
        None = 0
        Weapon
        Armor
        Helmet
        Shield
        Shoes
        Gloves

        Count
    End Enum

    ''' <Summary> Layers in a map </Summary>
    Public Enum LayerType As Byte
        None = 0
        Ground
        Mask
        Cover
        Fringe
        Roof
        Count
    End Enum

    ''' <Summary> Resource Skills </Summary>
    Public Enum ResourceType As Byte
        None = 0
        Herbing
        Woodcutting
        Mining
        Fishing
        Count
    End Enum

    Public Enum RarityType
        Broken = 1
        Common
        Uncommon
        Rare
        Epic
    End Enum

    Public Enum Weather
        None = 0
        Rain
        Snow
        Hail
        Sandstorm
        Storm
        Fog
    End Enum

    Public Enum QuestType
        Slay = 1
        Collect
        Talk
        Reach
        Give
        Kill
        Gather
        Fetch
        TalkEvent
    End Enum

    Public Enum QuestStatusType
        NotStarted = 1
        Started
        Completed
        Repeatable
    End Enum

    Public Enum MoveRouteOpts
        MoveUp = 1
        MoveDown
        MoveLeft
        MoveRight
        MoveRandom
        MoveTowardsPlayer
        MoveAwayFromPlayer
        StepForward
        StepBack
        Wait100Ms
        Wait500Ms
        Wait1000Ms
        TurnUp
        TurnDown
        TurnLeft
        TurnRight
        Turn90Right
        Turn90Left
        Turn180
        TurnRandom
        TurnTowardPlayer
        TurnAwayFromPlayer
        SetSpeed8XSlower
        SetSpeed4XSlower
        SetSpeed2XSlower
        SetSpeedNormal
        SetSpeed2XFaster
        SetSpeed4XFaster
        SetFreqLowest
        SetFreqLower
        SetFreqNormal
        SetFreqHigher
        SetFreqHighest
        WalkingAnimOn
        WalkingAnimOff
        DirFixOn
        DirFixOff
        WalkThroughOn
        WalkThroughOff
        PositionBelowPlayer
        PositionWithPlayer
        PositionAbovePlayer
        ChangeGraphic
    End Enum

    ' Event Types
    Public Enum EventType
        ' Message
        AddText = 1

        ShowText
        ShowChoices

        ' Game Progression
        PlayerVar

        PlayerSwitch
        SelfSwitch

        ' Flow Control
        Condition

        ExitProcess

        ' Player
        ChangeItems

        RestoreHP
        RestoreMP
        LevelUp
        ChangeLevel
        ChangeSkills
        ChangeJob
        ChangeSprite
        ChangeSex
        ChangePk

        ' Movement
        WarpPlayer

        SetMoveRoute

        ' Character
        PlayAnimation

        ' Music and Sounds
        PlayBgm

        FadeoutBgm
        PlaySound
        StopSound

        'Etc...
        CustomScript

        SetAccess

        'Shop/Bank
        OpenBank

        OpenShop

        'New
        GiveExp

        ShowChatBubble
        Label
        GotoLabel
        SpawnNpc
        FadeIn
        FadeOut
        FlashWhite
        SetFog
        SetWeather
        SetTint
        Wait
        ShowPicture
        HidePicture
        WaitMovement
        HoldPlayer
        ReleasePlayer
    End Enum

    Public Enum CommonEventType
        Switch = 1
        Variable
        Key
        CustomScript
    End Enum

    Public Enum EditorType
        Item = 1
        Map
        NPC
        Skill
        Shop
        Resource
        Animation
        Pet
        Quest
        Job
        Projectile
    End Enum

    Public Enum QuadrantType
        NE = 1
        SE
        SW
        NW
    End Enum

    Public Enum WrapModeType
        Characters
        Font
    End Enum

    Public Enum WrapType
        None
        BreakWord
        Whitespace
        Smart
    End Enum

    Public Enum EntityType
        Label = 1
        Window
        Button
        TextBox
        Scrollbar
        PictureBox
        Checkbox
        Combobox
        Combomenu
    End Enum

    Public Enum DesignType
        ' Boxes
        Wood = 1
        Wood_Small
        Wood_Empty
        Green
        Green_Hover
        Green_Click
        Red
        Red_Hover
        Red_Click
        Blue
        Blue_Hover
        Blue_Click
        Orange
        Orange_Hover
        Orange_Click
        Grey
        DescPic
        ' Windows
        Win_Black
        Win_Norm
        Win_NoBar
        Win_Empty
        Win_Desc
        Win_Shadow
        Win_Party
        ' Pictures
        Parchment
        BlackOval
        ' Textboxes
        TextBlack
        TextWhite
        TextBlack_Sq
        ' Checkboxes
        ChkNorm
        ChkChat
        ChkCustom_Buying
        ChkCustom_Selling
        ' Right-click Menu
        MenuHeader
        MenuOption
        ' Comboboxes
        ComboNorm
        ComboMenuNorm
        ' tile Selection
        TileBox
    End Enum

    Public Enum EntState
        Normal = 0
        Hover
        MouseDown
        MouseMove
        MouseUp
        DblClick
        Enter
        MouseScroll
        KeyDown
        KeyUp
        Count
    End Enum

    Public Enum AlignmentType
        Left = 0
        Right
        Center
    End Enum

    Public Enum PartType
        None = 0
        Item
        Spell
    End Enum

    Public Enum PartOriginType
        None = 0
        Inventory
        Hotbar
        Spells
    End Enum

    Public Enum FontType
        None = 0
        Georgia
        Arial
        Verdana
        Count
    End Enum

    Public Enum MenuType
        None = 0
        Main
        Login
        Register
        Credits
        Job
        NewChar
        Chars
    End Enum

    Public Enum DialogueMsg
        None = 0
        Connection
        Banned
        Kicked
        Outdated
        Maintenance
        NameTaken
        NameLength
        NameIllegal
        Database
        WrongPass
        Activate
        MaxChar
        DelChar
        CreateAccount
        MultiAccount
        Login
    End Enum

    Public Enum DialogueType
        Name = 0
        Trade
        Forget
        Party
        LootItem
        Alert
        DelChar
        DropItem
        DepositItem
        WithdrawItem
        TradeAmount
        UntradeAmount
        ClearLayer
        FillLayer
    End Enum

    Public Enum DialogueStyle
        Okay = 1
        YesNo
        Input
    End Enum

    Public Enum ChatChannel
        Game = 1
        Map
        Broadcast
        Party
        Guild
        Whisper
        Count
    End Enum

    Public Enum PartOriginsType
        None = 0
        Inventory
        Hotbar
        Spell
        Bank
    End Enum

End Module