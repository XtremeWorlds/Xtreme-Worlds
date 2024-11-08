Imports System.Linq
Imports Mirage.Sharp.Asfw
Imports Core

Module Player

#Region "PlayerCombat"

    Function CanPlayerAttackPlayer(Attacker As Integer, Victim As Integer, Optional IsSkill As Boolean = False) As Boolean

        If Not IsSkill Then
            ' Check attack timer
            If GetPlayerEquipment(Attacker, Core.EquipmentType.Weapon) > 0 Then
                If GetTimeMs() < TempPlayer(Attacker).AttackTimer + Type.Item(GetPlayerEquipment(Attacker, EquipmentType.Weapon)).Speed Then Exit Function
            Else
                If GetTimeMs() < TempPlayer(Attacker).AttackTimer + 1000 Then Exit Function
            End If
        End If

        ' Check for subscript out of range
        If Not IsPlaying(Victim) Then Exit Function

        ' Make sure they are on the same map
        If Not GetPlayerMap(Attacker) = GetPlayerMap(Victim) Then Exit Function

        ' Make sure we dont attack the player if they are switching maps
        If TempPlayer(Victim).GettingMap = 1 Then Exit Function

        If Not IsSkill Then
            ' Check if at same coordinates
            Select Case GetPlayerDir(Attacker)
                Case DirectionType.Up

                    If Not ((GetPlayerY(Victim) + 1 = GetPlayerY(Attacker)) And (GetPlayerX(Victim) = GetPlayerX(Attacker))) Then Exit Function
                Case DirectionType.Down

                    If Not ((GetPlayerY(Victim) - 1 = GetPlayerY(Attacker)) And (GetPlayerX(Victim) = GetPlayerX(Attacker))) Then Exit Function
                Case DirectionType.Left

                    If Not ((GetPlayerY(Victim) = GetPlayerY(Attacker)) And (GetPlayerX(Victim) + 1 = GetPlayerX(Attacker))) Then Exit Function
                Case DirectionType.Right

                    If Not ((GetPlayerY(Victim) = GetPlayerY(Attacker)) And (GetPlayerX(Victim) - 1 = GetPlayerX(Attacker))) Then Exit Function
                Case Else
                    Exit Function
            End Select
        End If

        ' CheckIf Type.Map is attackable
       If Type.Map(GetPlayerMap(attacker)).Moral > 0 Then
            If Not Type.Moral(Type.Map(GetPlayerMap(Attacker)).Moral).CanPK Then
                If GetPlayerPK(Victim) = 0 Then
                    PlayerMsg(Attacker, "This is a safe zone!", ColorType.BrightRed)
                    Exit Function
                End If
            End If
        End If

        ' Make sure they have more then 0 hp
        If GetPlayerVital(Victim, VitalType.HP) <= 0 Then Exit Function

        ' Check to make sure that they dont have access
        If GetPlayerAccess(Attacker) > AccessType.Moderator Then
            PlayerMsg(Attacker, "You cannot attack any player for thou art an admin!", ColorType.BrightRed)
            Exit Function
        End If

        ' Check to make sure the victim isn't an admin
        If GetPlayerAccess(Victim) > AccessType.Moderator Then
            PlayerMsg(Attacker, "You cannot attack " & GetPlayerName(Victim) & "!", ColorType.BrightRed)
            Exit Function
        End If

        ' Make sure attacker is high enough level
        If GetPlayerLevel(Attacker) < 10 Then
            PlayerMsg(Attacker, "You are below level 10, you cannot attack another player yet!", ColorType.BrightRed)
            Exit Function
        End If

        ' Make sure victim is high enough level
        If GetPlayerLevel(Victim) < 10 Then
            PlayerMsg(Attacker, GetPlayerName(Victim) & " is below level 10, you cannot attack this player yet!", ColorType.BrightRed)
            Exit Function
        End If

        CanPlayerAttackPlayer = 1
    End Function

    Function CanPlayerBlockHit(index As Integer) As Boolean
        Dim i As Integer
        Dim n As Integer
        Dim ShieldSlot As Integer
        ShieldSlot = GetPlayerEquipment(index, EquipmentType.Shield)

        CanPlayerBlockHit = 0

        If ShieldSlot > 0 Then
            n = Int(Rnd() * 2)

            If n = 1 Then
                i = (GetPlayerStat(index, StatType.Luck) \ 2) + (GetPlayerLevel(index) \ 2)
                n = Int(Rnd() * 100) + 1

                If n <= i Then
                    CanPlayerBlockHit = 1
                End If
            End If
        End If

    End Function

    Function CanPlayerCriticalHit(index As Integer) As Boolean
        On Error Resume Next
        Dim i As Integer
        Dim n As Integer

        If GetPlayerEquipment(index, EquipmentType.Weapon) > 0 Then
            n = (Rnd()) * 2

            If n = 1 Then
                i = (GetPlayerStat(index, StatType.Strength) \ 2) + (GetPlayerLevel(index) \ 2)
                n = Int(Rnd() * 100) + 1

                If n <= i Then
                    CanPlayerCriticalHit = 1
                End If
            End If
        End If

    End Function

    Function GetPlayerDamage(index As Integer) As Integer
        Dim weaponNum As Integer

        GetPlayerDamage = 0

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or index < 0 Or index > MAX_PLAYERS Then
            Exit Function
        End If

        If GetPlayerEquipment(index, EquipmentType.Weapon) > 0 Then
            weaponNum = GetPlayerEquipment(index, EquipmentType.Weapon)
            GetPlayerDamage = (GetPlayerStat(index, StatType.Strength) * 2) + (Type.Item(weaponNum).Data2 * 2) + (GetPlayerLevel(index) * 3) + Random.NextDouble(0, 20)
        Else
            GetPlayerDamage = (GetPlayerStat(index, StatType.Strength) * 2) + (GetPlayerLevel(index) * 3) + Random.NextDouble(0, 20)
        End If

    End Function

    Function GetPlayerProtection(index As Integer) As Integer
        Dim Armor As Integer, Helm As Integer, Shield As Integer
        GetPlayerProtection = 0

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or index < 0 Or index > MAX_PLAYERS Then
            Exit Function
        End If

        Armor = GetPlayerEquipment(index, EquipmentType.Armor)
        Helm = GetPlayerEquipment(index, EquipmentType.Helmet)
        Shield = GetPlayerEquipment(index, EquipmentType.Shield)  

        If Armor > 0 Then
            GetPlayerProtection += Type.Item(Armor).Data2
        End If

        If Helm > 0 Then
            GetPlayerProtection += Type.Item(Helm).Data2
        End If

        If Shield > 0 Then
            GetPlayerProtection += Type.Item(Shield).Data2
        End If

        GetPlayerProtection /= 6
        GetPlayerProtection += (GetPlayerStat(index, StatType.Luck) \ 5)
    End Function

    Sub AttackPlayer(attacker As Integer, victim As Integer, damage As Integer, Optional skillnum As Integer = 0, Optional npcnum As Integer = 0)
        Dim exp As Integer, mapNum As Integer
        Dim n As Integer
        Dim buffer As ByteStream

        If npcnum = 0 Then
            ' Check for subscript out of range
            If IsPlaying(attacker) = 0 Or IsPlaying(victim) = 0 Or damage < 0 Then
                Exit Sub
            End If

            ' Check for weapon
            If GetPlayerEquipment(attacker, EquipmentType.Weapon) > 0 Then
                n = GetPlayerEquipment(attacker, EquipmentType.Weapon)
            End If

            ' Send this packet so they can see the person attacking
            buffer = New ByteStream(4)
            buffer.WriteInt32(ServerPackets.SAttack)
            buffer.WriteInt32(attacker)
            SendDataToMapBut(attacker, GetPlayerMap(attacker), buffer.Data, buffer.Head)
            buffer.Dispose()

            If damage >= GetPlayerVital(victim, VitalType.HP) Then
                SendActionMsg(GetPlayerMap(victim), "-" & damage, ColorType.BrightRed, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))

                ' Player is dead
                GlobalMsg(GetPlayerName(victim) & " has been killed by " & GetPlayerName(attacker))
                
               If Type.Map(GetPlayerMap(victim)).Moral > 0 Then
                    If Type.Moral(Type.Map(GetPlayerMap(victim)).Moral).LoseExp Then
                        ' Calculate exp to give attacker
                        exp = Math.Round(GetPlayerExp(victim) \ 3)

                        ' Make sure we dont get less then 0
                        If exp < 0 Then
                            exp = 0
                        End If

                        If exp = 0 Then
                            PlayerMsg(victim, "You lost no exp.", ColorType.BrightGreen)
                            PlayerMsg(attacker, "You received no exp.", ColorType.BrightRed)
                        Else
                            SetPlayerExp(victim, GetPlayerExp(victim) - exp)
                            SendExp(victim)
                            PlayerMsg(victim, "You lost " & exp & " exp.", ColorType.BrightRed)
                            SetPlayerExp(attacker, GetPlayerExp(attacker) + exp)
                            SendExp(attacker)
                            PlayerMsg(attacker, "You received " & exp & " exp.", ColorType.BrightGreen)
                        End If

                        ' Check for a level up
                        CheckPlayerLevelUp(attacker)
                    End If
                End If

                ' Check if target is player who died and if so set target to 0
                If TempPlayer(attacker).TargetType = TargetType.Player Then
                    If TempPlayer(attacker).Target = victim Then
                        TempPlayer(attacker).Target = 0
                        TempPlayer(attacker).TargetType = 0
                    End If
                End If

                If GetPlayerPK(victim) = 0 Then
                    If GetPlayerPK(attacker) = 0 Then
                        SetPlayerPK(attacker, True)
                        SendPlayerData(attacker)
                        GlobalMsg(GetPlayerName(attacker) & " has been deemed a Player Killer!!!")
                    End If
                Else
                    GlobalMsg(GetPlayerName(victim) & " has paid the price for being a Player Killer!!!")
                End If

                OnDeath(victim)
            Else
                ' Player not dead, just do the damage
                SetPlayerVital(victim, VitalType.HP, GetPlayerVital(victim, VitalType.HP) - damage)
                SendVital(victim, VitalType.HP)
                SendActionMsg(GetPlayerMap(victim), "-" & damage, ColorType.BrightRed, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))

                'if a stunning skill, stun the player
                If skillnum > 0 Then
                    If Type.Skill(skillNum).StunDuration > 0 Then StunPlayer(victim, skillnum)
                End If
            End If

            ' Reset attack timer
            TempPlayer(attacker).AttackTimer = GetTimeMs()
        Else ' npc to player
            ' Check for subscript out of range
            If IsPlaying(victim) = 0 Or damage < 0 Then Exit Sub

            mapNum = GetPlayerMap(victim)

            ' Send this packet so they can see the person attacking
            buffer = New ByteStream(4)
            buffer.WriteInt32(ServerPackets.SNpcAttack)
            buffer.WriteInt32(attacker)
            SendDataToMap(MapNum, buffer.Data, buffer.Head)
            buffer.Dispose()

            If damage >= GetPlayerVital(victim, VitalType.HP) Then

                SendActionMsg(MapNum, "-" & damage, ColorType.BrightRed, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))

                ' Player is dead
                GlobalMsg(GetPlayerName(victim) & " has been killed by " & Type.NPC(MapNPC(MapNum).NPC(attacker).Num).Name)

                ' Check if target is player who died and if so set target to 0
                If TempPlayer(attacker).TargetType = TargetType.Player Then
                    If TempPlayer(attacker).Target = victim Then
                        TempPlayer(attacker).Target = 0
                        TempPlayer(attacker).TargetType = 0
                    End If
                End If

                OnDeath(victim)
            Else
                ' Player not dead, just do the damage
                SetPlayerVital(victim, VitalType.HP, GetPlayerVital(victim, VitalType.HP) - damage)
                SendVital(victim, VitalType.HP)
                SendActionMsg(MapNum, "-" & damage, ColorType.BrightRed, 1, (GetPlayerX(victim) * 32), (GetPlayerY(victim) * 32))

                'if a stunning skill, stun the player
                If skillnum > 0 Then
                    If Type.Skill(skillNum).StunDuration > 0 Then StunPlayer(victim, skillnum)
                End If
            End If

            ' Reset attack timer
            MapNPC(MapNum).NPC(attacker).AttackTimer = GetTimeMs()
        End If

    End Sub

    Friend Sub StunPlayer(index As Integer, skillnum As Integer)
        ' check if it's a stunning skill
        If Type.Skill(skillNum).StunDuration > 0 Then
            ' set the values on index
            TempPlayer(index).StunDuration = Type.Skill(skillNum).StunDuration
            TempPlayer(index).StunTimer = GetTimeMs()
            ' send it to the index
            SendStunned(index)
            ' tell him he's stunned
            PlayerMsg(index, "You have been stunned!", ColorType.Yellow)
        End If
    End Sub

    Function CanPlayerAttackNpc(Attacker As Integer, MapNPCNum As Integer, Optional IsSkill As Boolean = False) As Boolean
        Dim mapNum As Integer
        Dim NpcNum As Integer
        Dim atkX As Integer
        Dim atkY As Integer
        Dim attackspeed As Integer

        ' Check for subscript out of range
        If IsPlaying(Attacker) = 0 Or MapNPCnum <= 0 Or MapNPCNum > MAX_MAP_NPCS Then
            Exit Function
        End If

        ' Check for subscript out of range
       If MapNPC(GetPlayerMap(Attacker)).NPC(MapNPCNum).Num <= 0 Then
            Exit Function
        End If

        mapNum = GetPlayerMap(Attacker)
        NpcNum = MapNPC(MapNum).NPC(MapNPCNum).Num

        ' Make sure the npc isn't already dead
       If MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) <= 0 Then
            Exit Function
        End If

        ' Make sure they are on the same map

        ' attack speed from weapon
        If GetPlayerEquipment(Attacker, EquipmentType.Weapon) > 0 Then
            attackspeed = Type.Item(GetPlayerEquipment(Attacker, EquipmentType.Weapon)).Speed
        Else
            attackspeed = 1000
        End If

        If NpcNum > 0 And GetTimeMs() > TempPlayer(Attacker).AttackTimer + attackspeed Then
            ' exit out early
            If IsSkill Then
                If Type.NPC(NPCNum).Behaviour <> NpcBehavior.Friendly And Type.NPC(NPCNum).Behaviour <> NpcBehavior.ShopKeeper Then
                    CanPlayerAttackNpc = 1
                    Exit Function
                End If
            End If

            ' Check if at same coordinates
            Select Case GetPlayerDir(Attacker)
                Case DirectionType.Up
                    atkX = GetPlayerX(Attacker)
                    atkY = GetPlayerY(Attacker) - 1
                Case DirectionType.Down
                    atkX = GetPlayerX(Attacker)
                    atkY = GetPlayerY(Attacker) + 1
                Case DirectionType.Left
                    atkX = GetPlayerX(Attacker) - 1
                    atkY = GetPlayerY(Attacker)
                Case DirectionType.Right
                    atkX = GetPlayerX(Attacker) + 1
                    atkY = GetPlayerY(Attacker)
            End Select

            If atkX = MapNPC(MapNum).NPC(MapNPCNum).X Then
                If atkY = MapNPC(MapNum).NPC(MapNPCNum).Y Then
                    If Type.NPC(NPCNum).Behaviour <> NpcBehavior.Friendly And Type.NPC(NPCNum).Behaviour <> NpcBehavior.ShopKeeper And Type.NPC(NPCNum).Behaviour <> NpcBehavior.Quest Then
                        CanPlayerAttackNpc = 1
                    Else
                        If Len(Type.NPC(NPCNum).AttackSay) > 0 Then
                            PlayerMsg(Attacker, Type.NPC(NPCNum).Name & ": " & Type.NPC(NPCNum).AttackSay, ColorType.Yellow)
                        End If
                    End If
                End If
            End If
        End If

    End Function

    Friend Sub StunNPC(index As Integer, mapNum As Integer, skillnum As Integer)
        ' check if it's a stunning skill
        If Type.Skill(skillNum).StunDuration > 0 Then
            ' set the values on index
            MapNPC(MapNum).NPC(index).StunDuration = Type.Skill(skillNum).StunDuration
            MapNPC(MapNum).NPC(index).StunTimer = GetTimeMs()
        End If
    End Sub

    Sub PlayerAttackNpc(Attacker As Integer, MapNPCNum As Integer, Damage As Integer)
        ' Check for subscript out of range
        If IsPlaying(Attacker) = 0 Or MapNPCNum <= 0 Or MapNPCNum > MAX_MAP_NPCS Or Damage <= 0 Then Exit Sub

        Dim MapNum = GetPlayerMap(Attacker)
        Dim NpcNum = MapNPC(MapNum).NPC(MapNPCNum).Num
        Dim Name = Type.NPC(NPCNum).Name

        ' Check for weapon
        Dim Weapon = 0
        If GetPlayerEquipment(Attacker, EquipmentType.Weapon) > 0 Then
            Weapon = GetPlayerEquipment(Attacker, EquipmentType.Weapon)
        End If

        ' Deal damage to our NPC.
        MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) = MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) - Damage

        ' Set the NPC target to the player so they can come after them.
        MapNPC(MapNum).NPC(MapNPCNum).TargetType = TargetType.Player
        MapNPC(MapNum).NPC(MapNPCNum).Target = Attacker

        ' Check for any mobs on the map with the Guard behaviour so they can come after our player.
        If Type.NPC(MapNPC(MapNum).NPC(MapNPCNum).Num).Behaviour = NpcBehavior.Guard Then
            ' Find all NPCs with the same ID as the current NPC in the group
            Dim guards = MapNPC(MapNum).NPC.
                            Where(Function(npc) npc.Num = MapNPC(MapNum).NPC(MapNPCNum).Num).
                            Select(Function(npc, index) index)

            ' Set the target for each guard NPC
            For Each guardIndex In guards
                MapNPC(MapNum).NPC(guardIndex).Target = Attacker
                MapNPC(MapNum).NPC(guardIndex).TargetType = TargetType.Player
            Next
        End If

        ' Send our general visual stuff.
        SendActionMsg(MapNum, "-" & Damage, ColorType.BrightRed, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
        SendBlood(GetPlayerMap(Attacker), MapNPC(MapNum).NPC(MapNPCNum).X, MapNPC(MapNum).NPC(MapNPCNum).Y)
        SendPlayerAttack(Attacker)
        If Weapon > 0 Then
            SendAnimation(MapNum, Type.Item(GetPlayerEquipment(Attacker, EquipmentType.Weapon)).Animation, 0, 0, TargetType.NPC, MapNPCNum)
        End If

        ' Reset our attack timer.
        TempPlayer(Attacker).AttackTimer = GetTimeMs()

        If Not IsNpcDead(MapNum, MapNPCNum) Then
            ' Check if our NPC has something to share with our player.
           If MapNPC(MapNum).NPC(MapNPCNum).Target = 0 Then
                If Len(Type.NPC(NPCNum).AttackSay) > 0 Then
                    PlayerMsg(Attacker, String.Format("{0} says: '{1}'", Type.NPC(NPCNum).Name, Type.NPC(NPCNum).AttackSay), ColorType.Yellow)
                End If
            End If

            SendMapNPCTo(MapNum, MapNPCNum)
        Else
            HandlePlayerKillNpc(MapNum, Attacker, MapNPCNum)
        End If
    End Sub

    Function IsInRange(range As Integer, x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer) As Boolean
        Dim nVal As Integer
        IsInRange = 0
        nVal = Math.Sqrt((x1 - x2) ^ 2 + (y1 - y2) ^ 2)
        If nVal <= range Then IsInRange = 1
    End Function

    Friend Function CanPlayerDodge(index As Integer) As Boolean
        Dim rate As Integer, rndNum As Integer

        CanPlayerDodge = 0

        rate = GetPlayerStat(index, StatType.Luck) / 4
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then
            CanPlayerDodge = 1
        End If

    End Function

    Friend Function CanPlayerParry(index As Integer) As Boolean
        Dim rate As Integer, rndNum As Integer

        CanPlayerParry = 0

        rate = GetPlayerStat(index, StatType.Luck) / 6
        rndNum = Random.NextDouble(1, 100)

        If rndNum <= rate Then
            CanPlayerParry = 1
        End If

    End Function

    Friend Sub TryPlayerAttackPlayer(Attacker As Integer, Victim As Integer)
        Dim mapNum As Integer
        Dim Damage As Integer, i As Integer, armor As Integer

        Damage = 0

        ' Can we attack the player?
        If CanPlayerAttackPlayer(Attacker, Victim) Then

            mapNum = GetPlayerMap(Attacker)

            ' check if NPC can avoid the attack
            If CanPlayerDodge(Victim) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, 1, (GetPlayerX(Victim) * 32), (GetPlayerY(Victim) * 32))
                Exit Sub
            End If

            If CanPlayerParry(Victim) Then
                SendActionMsg(MapNum, "Parry!", ColorType.Pink, 1, (GetPlayerX(Victim) * 32), (GetPlayerY(Victim) * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            Damage = GetPlayerDamage(Attacker)

            If CanPlayerBlockHit(Victim) Then
                SendActionMsg(MapNum, "Block!", ColorType.BrightCyan, 1, (GetPlayerX(Victim) * 32), (GetPlayerY(Victim) * 32))
                Damage = 0
                Exit Sub
            Else

                For i = 1 To EquipmentType.Count - 1
                    If GetPlayerEquipment(Victim, i) > 0 Then
                        armor += Type.Item(GetPlayerEquipment(Victim, i)).Data2
                    End If
                Next

                ' take away armour
                Damage -= (GetPlayerStat(Victim, StatType.Spirit) * 2) + (GetPlayerLevel(Victim) * 3) + armor

                ' * 1.5 if it's a crit!
                If CanPlayerCriticalHit(Attacker) Then
                    Damage *= 1.5
                    SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, 1, GetPlayerX(Attacker) * 32, GetPlayerY(Attacker) * 32)
                End If
            End If

            If Damage > 0 Then
                PlayerAttackPlayer(Attacker, Victim, Damage)
            Else
                PlayerMsg(Attacker, "Your attack does nothing.", ColorType.BrightRed)
            End If

        End If

    End Sub

    Sub PlayerAttackPlayer(Attacker As Integer, Victim As Integer, Damage As Integer)
        ' Check for subscript out of range
        If IsPlaying(Attacker) = 0 Or IsPlaying(Victim) = 0 Or Damage < 0 Then
            Exit Sub
        End If

        ' Check if our assailant has a weapon.
        Dim Weapon = 0
        If GetPlayerEquipment(Attacker, EquipmentType.Weapon) > 0 Then
            Weapon = GetPlayerEquipment(Attacker, EquipmentType.Weapon)
        End If

        ' Stop our player's regeneration abilities.
        TempPlayer(Attacker).StopRegen = 1
        TempPlayer(Attacker).StopRegenTimer = GetTimeMs()

        ' Deal damage to our player.
        SetPlayerVital(Victim, VitalType.HP, GetPlayerVital(Victim, VitalType.HP) - Damage)

        ' Send all the visuals to our player.
        If Weapon > 0 Then
            SendAnimation(GetPlayerMap(Victim), Type.Item(Weapon).Animation, 0, 0, TargetType.Player, Victim)
        End If
        SendActionMsg(GetPlayerMap(Victim), "-" & Damage, ColorType.BrightRed, 1, (GetPlayerX(Victim) * 32), (GetPlayerY(Victim) * 32))
        SendBlood(GetPlayerMap(Victim), GetPlayerX(Victim), GetPlayerY(Victim))

        ' set the regen timer
        TempPlayer(Victim).StopRegen = 1
        TempPlayer(Victim).StopRegenTimer = GetTimeMs()

        ' Reset attack timer
        TempPlayer(Attacker).AttackTimer = GetTimeMs()

        If Not IsPlayerDead(Victim) Then
            ' Send our player's new vitals to everyone that needs them.
            SendVital(Victim, VitalType.HP)
        Else
            ' Handle our dead player.
            HandlePlayerKillPlayer(Attacker, Victim)
        End If
    End Sub

    Friend Sub TryPlayerAttackNpc(index As Integer, mapnpcnum As Integer)

        Dim npcnum As Integer

        Dim mapNum As Integer

        Dim Damage As Integer

        Damage = 0

        ' Can we attack the npc?
        If CanPlayerAttackNpc(index, mapnpcnum) Then
            mapNum = GetPlayerMap(index)
            npcnum = MapNPC(MapNum).NPC(MapNPCNum).Num

            ' check if NPC can avoid the attack
            If CanNpcDodge(npcnum) Then
                SendActionMsg(MapNum, "Dodge!", ColorType.Pink, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
                Exit Sub
            End If

            If CanNpcParry(npcnum) Then
                SendActionMsg(MapNum, "Parry!", ColorType.Pink, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
                Exit Sub
            End If

            ' Get the damage we can do
            Damage = GetPlayerDamage(index)

            If CanNpcBlock(npcnum) Then
                SendActionMsg(MapNum, "Block!", ColorType.BrightCyan, 1, (MapNPC(MapNum).NPC(MapNPCNum).X * 32), (MapNPC(MapNum).NPC(MapNPCNum).Y * 32))
                Damage = 0
                Exit Sub
            Else

                Damage -= ((Type.NPC(NPCNum).Stat(StatType.Spirit) * 2) + (Type.NPC(NPCNum).Level * 3))

                ' * 1.5 if it's a crit!
                If CanPlayerCriticalHit(index) Then
                    Damage *= 1.5
                    SendActionMsg(MapNum, "Critical!", ColorType.BrightCyan, 1, (GetPlayerX(index) * 32), (GetPlayerY(index) * 32))
                End If

            End If

            TempPlayer(index).Target = mapnpcnum
            TempPlayer(index).TargetType = TargetType.NPC
            SendTarget(index, mapnpcnum, TargetType.NPC)

            If Damage > 0 Then
                PlayerAttackNpc(index, mapnpcnum, Damage)
            Else
                PlayerMsg(index, "Your attack does nothing.", ColorType.BrightRed)
            End If

        End If

    End Sub

    Friend Function IsPlayerDead(index As Integer)
        IsPlayerDead = 0
        If index <= 0 Or index > MAX_PLAYERS Or Not TempPlayer(index).InGame Then Exit Function
        If GetPlayerVital(index, VitalType.HP) < 0 Then IsPlayerDead = 1
    End Function

    Friend Sub HandlePlayerKillPlayer(Attacker As Integer, Victim As Integer)
        ' Notify everyone that our player has bit the dust.
        GlobalMsg(String.Format("{0} has been killed by {1}!", GetPlayerName(Victim), GetPlayerName(Attacker)))

        ' Hand out player experience
        HandlePlayerKillExperience(Attacker, Victim)

        ' Handle our PK outcomes.
        HandlePlayerKilledPK(Attacker, Victim)

        ' Remove our player from everyone's target list.
        For Each p In TempPlayer.Where(Function(x, i) x.InGame And GetPlayerMap(i + 1) = GetPlayerMap(Victim) And x.TargetType = TargetType.Player And x.Target = Victim).Select(Function(x, i) i + 1).ToArray()
            TempPlayer(p).Target = 0
            TempPlayer(p).TargetType = 0
            SendTarget(p, 0, 0)
        Next

        ' Actually kill the player.
        OnDeath(Victim)
    End Sub

    Friend Sub HandlePlayerKillNpc(MapNum As Integer, index As Integer, MapNPCNum As Integer)
        ' Set our attacker's target to nothing.
        SendTarget(index, 0, 0)

        ' Hand out player experience
        HandleNpcKillExperience(index, MapNPC(MapNum).NPC(MapNPCNum).Num)

        ' Drop items if we can.
        DropNpcItems(MapNum, MapNPCNum)

        ' Set our NPC's data to default so we know it's dead.
        MapNPC(MapNum).NPC(MapNPCNum).Num = 0
        MapNPC(MapNum).NPC(MapNPCNum).SpawnWait = GetTimeMs()
        MapNPC(MapNum).NPC(MapNPCNum).Vital(VitalType.HP) = 0

        ' Notify all our clients that the NPC has died.
        SendNpcDead(MapNum, MapNPCNum)

        ' Check if our dead NPC is targetted by another player and remove their targets.
        For Each p In TempPlayer.Where(Function(x, i) x.InGame And GetPlayerMap(i + 1) = mapNum And x.TargetType = TargetType.NPC And x.Target = MapNPCNum).Select(Function(x, i) i + 1).ToArray()
            TempPlayer(p).Target = 0
            TempPlayer(p).TargetType = 0
            SendTarget(p, 0, 0)
        Next
    End Sub

    Friend Sub HandlePlayerKilledPK(attacker As Integer, victim As Integer)
        ' TODO: Redo this method, it is horrendous.
        Dim z As Integer, eqcount As Integer, invcount, j As Integer
        If GetPlayerPK(victim) = 0 Then
            If GetPlayerPK(attacker) = 0 Then
                SetPlayerPK(attacker, 1)
                SendPlayerData(attacker)
                GlobalMsg(GetPlayerName(attacker) & " has been deemed a Player Killer!!!")
            End If
        Else
            GlobalMsg(GetPlayerName(victim) & " has paid the price for being a Player Killer!!!")
        End If

       If Type.Map(GetPlayerMap(victim)).Moral > 0 Then
            If Type.Moral(Type.Map(GetPlayerMap(victim)).Moral).DropItems Then
                If GetPlayerLevel(victim) >= 10 Then

                    For z = 1 To MAX_INV
                        If GetPlayerInv(victim, z) > 0 Then
                            invcount += 1
                        End If
                    Next

                    For z = 0 To EquipmentType.Count - 1
                        If GetPlayerEquipment(victim, z) > 0 Then
                            eqcount += 1
                        End If
                    Next
                    z = Random.NextDouble(1, invcount + eqcount)

                    If z = 0 Then z = 1
                    If z > invcount + eqcount Then z = invcount + eqcount
                    If z > invcount Then
                        z -= invcount

                        For x = 0 To EquipmentType.Count - 1
                            If GetPlayerEquipment(victim, x) > 0 Then
                                j += 1

                                If j = z Then
                                    'Here it is, drop this piece of equipment!
                                    PlayerMsg(victim, "In death you lost grip on your " & Type.Item(GetPlayerEquipment(victim, x)).Name, ColorType.BrightRed)
                                    SpawnItem(GetPlayerEquipment(victim, x), 1, GetPlayerMap(victim), GetPlayerX(victim), GetPlayerY(victim))
                                    SetPlayerEquipment(victim, 0, x)
                                    SendWornEquipment(victim)
                                    SendMapEquipment(victim)
                                End If
                            End If
                        Next
                    Else

                        For x = 1 To MAX_INV
                            If GetPlayerInv(victim, x) > 0 Then
                                j += 1

                                If j = z Then
                                    'Here it is, drop this item!
                                    PlayerMsg(victim, "In death you lost grip on your " & Type.Item(GetPlayerInv(victim, x)).Name, ColorType.BrightRed)
                                    SpawnItem(GetPlayerInv(victim, x), GetPlayerInvValue(victim, x), GetPlayerMap(victim), GetPlayerX(victim), GetPlayerY(victim))
                                    SetPlayerInv(victim, x, 0)
                                    SetPlayerInvValue(victim, x, 0)
                                    SendInventory(victim)
                                End If
                            End If
                        Next
                    End If
                End If
            End If
        End If
    End Sub

#End Region

#Region "Data"

    Sub CheckPlayerLevelUp(index As Integer)
        Dim expRollover As Integer
        Dim level_count As Integer

        level_count = 0

        Do While GetPlayerExp(index) >= GetPlayerNextLevel(index)
            expRollover = GetPlayerExp(index) - GetPlayerNextLevel(index)
            SetPlayerLevel(index, GetPlayerLevel(index) + 1)
            SetPlayerPoints(index, GetPlayerPoints(index) + STAT_PER_LEVEL)
            SetPlayerExp(index, expRollover)
            level_count += 1
        Loop

        If level_count > 0 Then
            If level_count = 1 Then
                'singular
                GlobalMsg(GetPlayerName(index) & " has gained " & level_count & " level!")
            Else
                'plural
                GlobalMsg(GetPlayerName(index) & " has gained " & level_count & " levels!")
            End If
            SendActionMsg(GetPlayerMap(index), "Level Up", ColorType.Yellow, 1, (GetPlayerX(index) * 32), (GetPlayerY(index) * 32))
            SendExp(index)
            SendPlayerData(index)
        End If
    End Sub

    Function GetPlayerJob(index As Integer) As Integer
        If Type.Player(index).Job = 0 Then Type.Player(index).Job = 1
        GetPlayerJob = Type.Player(index).Job
    End Function

    Sub SetPlayerPK(index As Integer, PK As Integer)
        Type.Player(index).Pk = PK
    End Sub

#End Region

#Region "Incoming Packets"

    Friend Sub HandleUseChar(index As Integer)
        If Not IsPlaying(index) Then
            ' Send an ok to client to start receiving in game data
            SendLoginOK(index)
            JoinGame(index)
            Dim text = String.Format("{0} | {1} has began playing {2}.", GetPlayerLogin(index), GetPlayerName(index), Settings.GameName)
            Addlog(text, PLAYER_LOG)
            Call Global.System.Console.WriteLine(text)
        End If
    End Sub

#End Region

#Region "Outgoing Packets"

    Sub SendLeaveMap(index As Integer, mapNum As Integer)
        Dim buffer As New ByteStream(4)

        buffer.WriteInt32(ServerPackets.SLeftMap)
        buffer.WriteInt32(index)
        SendDataToMapBut(index, mapNum, buffer.Data, buffer.Head)

        buffer.Dispose()
    End Sub

#End Region

#Region "Movement"
    Sub PlayerWarp(index As Integer, MapNum As Integer, X As Integer, Y As Integer, Optional NoInstance As Boolean = False)
        Dim OldMap As Integer
        Dim i As Integer
        Dim buffer As ByteStream

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or MapNum <= 0 Or MapNum > MAX_MAPS Then Exit Sub

        ' Check if you are out of bounds
        If X > Type.Map(MapNum).MaxX Then X = Type.Map(MapNum).MaxX
        If Y > Type.Map(MapNum).MaxY Then Y = Type.Map(MapNum).MaxY

        TempPlayer(index).EventProcessingCount = 0
        TempPlayer(index).EventMap.CurrentEvents = 0

        'clear target
        TempPlayer(index).Target = 0
        TempPlayer(index).TargetType = 0
        SendTarget(index, 0, 0)

        ' clear events
        TempPlayer(index).EventMap.CurrentEvents = 0

        ' Save old map to send erase player data to
        OldMap = GetPlayerMap(index)

        If OldMap <> MapNum Then
            SendLeaveMap(index, OldMap)
        End If

        SetPlayerMap(index, MapNum)
        SetPlayerX(index, X)
        SetPlayerY(index, Y)
        If PetAlive(index) Then
            SetPetX(index, X)
            SetPetY(index, Y)
            TempPlayer(index).PetTarget = 0
            TempPlayer(index).PetTargetType = 0
            SendPetXy(index, X, Y)
        End If

        SendPlayerXY(index)

        ' send equipment of all people on new map
        If GetTotalMapPlayers(MapNum) > 0 Then
            For i = 1 To Socket.HighIndex()
                If IsPlaying(i) Then
                    If GetPlayerMap(i) = MapNum Then
                        SendMapEquipmentTo(i, index)
                    End If
                End If
            Next
        End If

        ' Now we check if there were any players left on the map the player just left, and if not stop processing npcs
        If GetTotalMapPlayers(OldMap) = 0 Then
            PlayersOnMap(OldMap) = 0

            ' Regenerate all NPCs' health
            For i = 1 To MAX_MAP_NPCS

               If MapNPC(OldMap).NPC(i).Num > 0 Then
                    MapNPC(OldMap).NPC(i).Vital(VitalType.HP) = GetNpcMaxVital(MapNPC(OldMap).NPC(i).Num, VitalType.HP)
                End If

            Next

        End If

        ' Sets it so we know to process npcs on the map
        PlayersOnMap(MapNum) = 1
        TempPlayer(index).GettingMap = 1

        SendUpdateMoralTo(index, Type.Map(MapNum).Moral)

        buffer = New ByteStream(4)
        buffer.WriteInt32(ServerPackets.SCheckForMap)
        buffer.WriteInt32(MapNum)
        buffer.WriteInt32(Type.Map(MapNum).Revision)
        Socket.SendDataTo(index, buffer.Data, buffer.Head)

        buffer.Dispose()

    End Sub

    Sub PlayerMove(index As Integer, Dir As Integer, Movement As Integer, ExpectingWarp As Boolean)
        Dim mapNum As Integer
        Dim x As Integer, y As Integer, begineventprocessing As Boolean
        Dim Moved As Boolean, DidWarp As Boolean
        Dim NewMapX As Byte, NewMapY As Byte
        Dim VitalType As Integer, Color As Integer, amount As Integer

        ' Check for subscript out of range
        If Dir < DirectionType.Up Or Dir > DirectionType.DownRight Or Movement < MovementType.Standing Or Movement > MovementType.Running Then
            Exit Sub
        End If

        If TempPlayer(index).InShop > 0 Or TempPlayer(index).InBank Then
            Exit Sub
        End If

        SetPlayerDir(index, Dir)
        Moved = 0
        mapNum = GetPlayerMap(index)

        Select Case Dir
            Case DirectionType.Up
                ' Check to make sure not outside of boundaries
                If GetPlayerY(index) > 0 Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.Up) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) - 1).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) - 1).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) - 1).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) - 1).Type2 <> TileType.Resource Then
                                SetPlayerY(index, GetPlayerY(index) - 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                Else
                    If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type <> TileType.NoXing And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type2 <> TileType.NoXing Then
                        ' Check to see if we can move them to another map
                        If Type.Map(GetPlayerMap(index)).Up > 0 Then
                            NewMapY = Type.Map(Type.Map(GetPlayerMap(index)).Up).MaxY
                            PlayerWarp(index, Type.Map(GetPlayerMap(index)).Up, GetPlayerX(index), NewMapY)
                            DidWarp = 1
                            Moved = 1
                        End If
                    End If
                End If

            Case DirectionType.Down
                ' Check to make sure not outside of boundaries
                If GetPlayerY(index) < Type.Map(MapNum).MaxY Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.Down) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) + 1).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) + 1).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) + 1).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index) + 1).Type2 <> TileType.Resource Then
                                SetPlayerY(index, GetPlayerY(index) + 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                Else
                    If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type <> TileType.NoXing And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type2 <> TileType.NoXing Then
                        ' Check to see if we can move them to another map
                        If Type.Map(GetPlayerMap(index)).Down > 0 Then
                            PlayerWarp(index, Type.Map(GetPlayerMap(index)).Down, GetPlayerX(index), 0)
                            DidWarp = 1
                            Moved = 1
                        End If
                    End If
                End If

            Case DirectionType.Left
                ' Check to make sure not outside of boundaries
                If GetPlayerX(index) > 0 Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.Left) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index)).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index)).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index)).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index)).Type2 <> TileType.Resource Then
                                SetPlayerX(index, GetPlayerX(index) - 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                Else
                    If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type <> TileType.NoXing And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type2 <> TileType.NoXing Then
                        ' Check to see if we can move them to another map
                        If Type.Map(GetPlayerMap(index)).Left > 0 Then
                            NewMapX = Type.Map(Type.Map(GetPlayerMap(index)).Left).MaxX
                            PlayerWarp(index, Type.Map(GetPlayerMap(index)).Left, NewMapX, GetPlayerY(index))
                            DidWarp = 1
                            Moved = 1
                        End If
                    End If
                End If

            Case DirectionType.Right
                ' Check to make sure not outside of boundaries
                If GetPlayerX(index) < Type.Map(MapNum).MaxX Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.Right) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index)).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index)).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index)).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index)).Type2 <> TileType.Resource Then
                                SetPlayerX(index, GetPlayerX(index) + 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                Else
                    If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type <> TileType.NoXing And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).Type2 <> TileType.NoXing Then
                        ' Check to see if we can move them to another map
                        If Type.Map(GetPlayerMap(index)).Right > 0 Then
                            PlayerWarp(index, Type.Map(GetPlayerMap(index)).Right, 0, GetPlayerY(index))
                            DidWarp = 1
                            Moved = 1
                        End If
                    End If
                End If

            Case DirectionType.UpRight
                ' Check to make sure not outside of boundaries
                If GetPlayerY(index) > 0 AndAlso GetPlayerX(index) < Type.Map(MapNum).MaxX Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.UpRight) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) - 1).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) - 1).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) - 1).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) - 1).Type2 <> TileType.Resource Then
                                SetPlayerX(index, GetPlayerX(index) + 1)
                                SetPlayerY(index, GetPlayerY(index) - 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                End If

            Case DirectionType.UpLeft
                ' Check to make sure not outside of boundaries
                If GetPlayerY(index) > 0 AndAlso GetPlayerX(index) > 0 Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.UpLeft) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) - 1).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) - 1).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) - 1).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) - 1).Type2 <> TileType.Resource Then
                                SetPlayerX(index, GetPlayerX(index) - 1)
                                SetPlayerY(index, GetPlayerY(index) - 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                End If

            Case DirectionType.DownRight
                ' Check to make sure not outside of boundaries
                If GetPlayerY(index) < Type.Map(MapNum).MaxY AndAlso GetPlayerX(index) < Type.Map(MapNum).MaxX Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.DownRight) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) + 1).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) + 1).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) + 1).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) + 1, GetPlayerY(index) + 1).Type2 <> TileType.Resource Then
                                SetPlayerX(index, GetPlayerX(index) + 1)
                                SetPlayerY(index, GetPlayerY(index) + 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                End If

            Case DirectionType.DownLeft
                ' Check to make sure not outside of boundaries
                If GetPlayerY(index) < Type.Map(MapNum).MaxY AndAlso GetPlayerX(index) > 0 Then
                    ' Check to make sure that the tile is walkable
                    If Not IsDirBlocked(Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index)).DirBlock, DirectionType.DownLeft) Then
                        If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) + 1).Type <> TileType.Blocked And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) + 1).Type2 <> TileType.Blocked Then
                            If Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) + 1).Type <> TileType.Resource And Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index) - 1, GetPlayerY(index) + 1).Type2 <> TileType.Resource Then
                                SetPlayerX(index, GetPlayerX(index) - 1)
                                SetPlayerY(index, GetPlayerY(index) + 1)
                                SendPlayerMove(index, Movement)
                                Moved = 1

                                ' Check for event
                                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                                    TriggerEvent(index, i, 1, GetPlayerX(index), GetPlayerY(index))
                                Next
                            End If
                        End If
                    End If
                End If
        End Select

        With Type.Map(GetPlayerMap(index)).Tile(GetPlayerX(index), GetPlayerY(index))
            mapNum = 0
            x = 0
            y = 0

            ' Check to see if the tile is a warp tile, and if so warp them
            If .Type = TileType.Warp Then
                mapNum = .Data1
                x = .Data2
                y = .Data3
            End If

            If .Type2 = TileType.Warp Then
                mapNum = .Data1_2
                x = .Data2_2
                y = .Data3_2
            End if

           If MapNum > 0 Then
                PlayerWarp(index, mapNum, x, y)

                DidWarp = 1
                Moved = 1
            End If

            mapNum = 0
            x = 0
            y = 0

            ' Check for a shop, and if so open it
            If .Type = TileType.Shop Then
                x = .Data1
            End If

            If .Type2 = TileType.Shop Then
                x = .Data1_2
            End If

            If x > 0 Then ' shop exists?
                If Len(Type.Shop(x).Name) > 0 Then ' name exists?
                    SendOpenShop(index, x)
                    TempPlayer(index).InShop = x ' stops movement and the like
                End If
            End If

            ' Check to see if the tile is a bank, and if so send bank
            If .Type = TileType.Bank Or .Type2 = TileType.Bank Then
                SendBank(index)
                TempPlayer(index).InBank = 1
                Moved = 1
            End If

            ' Check if it's a heal tile
            If .Type = TileType.Heal Then
                VitalType = .Data1
                amount = .Data2
            End if

            If .Type2 = TileType.Heal
                VitalType = .Data1_2
                amount += .Data2_2
            End If

            If VitalType > 0 Then
                If Not GetPlayerVital(index, VitalType) = GetPlayerMaxVital(index, VitalType) Then
                    If VitalType = Core.VitalType.HP Then
                        Color = ColorType.BrightGreen
                    Else
                        Color = ColorType.BrightBlue
                    End If

                    SendActionMsg(GetPlayerMap(index), "+" & amount, Color, ActionMsgType.Scroll, GetPlayerX(index) * 32, GetPlayerY(index) * 32, 1)
                    SetPlayerVital(index, VitalType, GetPlayerVital(index, VitalType) + amount)
                    PlayerMsg(index, "You feel rejuvenating forces coursing through your body.", ColorType.BrightGreen)
                    SendVital(index, VitalType)
                End If
                Moved = 1
            End If

            ' Check if it's a trap tile
            If .Type = TileType.Trap Then
                amount = .Data1
            End if

            If .Type2 = TileType.Trap Then
                amount += .Data1_2
            End If

            If amount > 0 Then
                SendActionMsg(GetPlayerMap(index), "-" & amount, ColorType.BrightRed, ActionMsgType.Scroll, GetPlayerX(index) * 32, GetPlayerY(index) * 32, 1)
                If GetPlayerVital(index, Core.VitalType.HP) - amount < 0 Then
                    KillPlayer(index)
                    PlayerMsg(index, "You've been killed by a trap.", ColorType.BrightRed)
                Else
                    SetPlayerVital(index, Core.VitalType.HP, GetPlayerVital(index, Core.VitalType.HP) - amount)
                    PlayerMsg(index, "You've been injured by a trap.", ColorType.BrightRed)
                    SendVital(index, Core.VitalType.HP)
                End If
                Moved = 1
            End If

        End With

        ' They tried to hack
        If Moved = 0 Or (ExpectingWarp And Not DidWarp) Then
            PlayerWarp(index, GetPlayerMap(index), GetPlayerX(index), GetPlayerY(index))
        End If

        x = GetPlayerX(index)
        y = GetPlayerY(index)

        If Moved = 1 Then
            If TempPlayer(index).EventMap.CurrentEvents > 0 Then
                For i = 1 To TempPlayer(index).EventMap.CurrentEvents
                    If TempPlayer(index).EventMap.EventPages(i).EventId > 0 Then
                        If Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Globals = 1 Then
                            If Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).X = x And Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Y = y And Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Pages(TempPlayer(index).EventMap.EventPages(i).PageId).Trigger = 1 And TempPlayer(index).EventMap.EventPages(i).Visible = True Then begineventprocessing = 1
                        Else
                            If TempPlayer(index).EventMap.EventPages(i).X = x And TempPlayer(index).EventMap.EventPages(i).Y = y And Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Pages(TempPlayer(index).EventMap.EventPages(i).PageId).Trigger = 1 And TempPlayer(index).EventMap.EventPages(i).Visible = True Then begineventprocessing = 1
                        End If
                        begineventprocessing = 0
                        If begineventprocessing = 1 Then
                            'Process this event, it is on-touch and everything checks out.
                            If Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Pages(TempPlayer(index).EventMap.EventPages(i).PageId).CommandListCount > 0 Then
                                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).Active = 1
                                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).ActionTimer = GetTimeMs()
                                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).CurList = 1
                                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).CurSlot = 1
                                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).EventId = TempPlayer(index).EventMap.EventPages(i).EventId
                                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).PageId = TempPlayer(index).EventMap.EventPages(i).PageId
                                TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).WaitingForResponse = 0
                                ReDim TempPlayer(index).EventProcessing(TempPlayer(index).EventMap.EventPages(i).EventId).ListLeftOff(Type.Map(GetPlayerMap(index)).Event(TempPlayer(index).EventMap.EventPages(i).EventId).Pages(TempPlayer(index).EventMap.EventPages(i).PageId).CommandListCount)
                            End If
                            begineventprocessing = 0
                        End If
                    End If
                Next
            End If
        End If

    End Sub

#End Region

#Region "Inventory"

    Function HasItem(index As Integer, ItemNum As Integer) As Integer
        Dim i As Integer

        ' Check for subscript out of range
        If Itemnum <= 0 Or ItemNum > MAX_ITEMS Then
            Exit Function
        End If

        For i = 1 To MAX_INV
            ' Check to see if the player has the item
            If GetPlayerInv(index, i) = ItemNum Then
                If Type.Item(ItemNum).Type = ItemType.Currency Or Type.Item(ItemNum).Stackable = 1 Then
                    HasItem += GetPlayerInvValue(index, i)
                Else
                    HasItem += 1
                End If
            End If
        Next

    End Function

    Function FindItemSlot(index As Integer, ItemNum As Integer) As Integer
        Dim i As Integer

        FindItemSlot = 0

        ' Check for subscript out of range
        If Itemnum <= 0 Or ItemNum > MAX_ITEMS Then
            Exit Function
        End If

        For i = 1 To MAX_INV
            ' Check to see if the player has the item
            If GetPlayerInv(index, i) = ItemNum Then
                FindItemSlot = i
                Exit Function
            End If
        Next

    End Function

    Sub PlayerMapGetItem(index As Integer)
        Dim i As Integer, itemnum As Integer
        Dim n As Integer
        Dim mapNum As Integer
        Dim Msg As String

        mapNum = GetPlayerMap(index)

        For i = 1 To MAX_MAP_ITEMS

            ' See if theres even an item here
            If (Type.MapItem(MapNum, i).Num > 0) And (Type.MapItem(MapNum, i).Num <= MAX_ITEMS) Then
                ' our drop?
                If CanPlayerPickupItem(index, i) Then
                    ' Check if item is at the same location as the player
                    If (Type.MapItem(MapNum, i).X = GetPlayerX(index)) Then
                        If (Type.MapItem(MapNum, i).Y = GetPlayerY(index)) Then
                            ' Find open slot
                            n = FindOpenInvSlot(index, MapItem(MapNum, i).Num)

                            ' Open slot available?
                            If n <> -1 Then
                                ' Set item in players inventor
                                itemnum = MapItem(MapNum, i).Num

                                SetPlayerInv(index, n, MapItem(MapNum, i).Num)

                                If Type.Item(GetPlayerInv(index, n)).Type = ItemType.Currency Or Type.Item(Global.Core.Commands.GetPlayerInv(index, n)).Stackable = 1 Then
                                    SetPlayerInvValue(index, n, GetPlayerInvValue(index, n) + MapItem(MapNum, i).Value)
                                    Msg = MapItem(MapNum, i).Value & " " & Type.Item(GetPlayerInv(index, n)).Name
                                Else
                                    SetPlayerInvValue(index, n, 1)
                                    Msg = Type.Item(GetPlayerInv(index, n)).Name
                                End If

                                ' Erase item from the map
                                MapItem(MapNum, i).Num = 0
                                MapItem(MapNum, i).Value = 0
                                MapItem(MapNum, i).X = 0
                                MapItem(MapNum, i).Y = 0

                                SendInventoryUpdate(index, n)
                                SpawnItemSlot(i, 0, 0, GetPlayerMap(index), 0, 0)
                                SendActionMsg(GetPlayerMap(index), Msg, ColorType.White, ActionMsgType.Static, (GetPlayerX(index) * 32), (GetPlayerY(index) * 32))
                                Exit For
                            Else
                                PlayerMsg(index, "Your inventory is full.", ColorType.BrightRed)
                                Exit For
                            End If
                        End If
                    End If
                End If
            End If
        Next
    End Sub

    Function CanPlayerPickupItem(index As Integer, mapItemNum As Integer) As Boolean
        Dim mapNum As Integer

        mapNum = GetPlayerMap(index)

        If Type.Map(MapNum).Moral > 0 Then
            If Type.Moral(Type.Map(MapNum).Moral).CanPickupItem Then
                ' no lock or locked to player?
               If Type.MapItem(MapNum, mapItemNum).PlayerName = "" Or MapItem(MapNum, mapItemNum).PlayerName = GetPlayerName(index) Then
                    CanPlayerPickupItem = 1
                    Exit Function
                End If
            Else 
                Call PlayerMsg(index, "You can't pickup items here!", ColorType.BrightRed)
            End If
        End If

        CanPlayerPickupItem = 0
    End Function

    Function FindOpenInvSlot(index As Integer, ItemNum As Integer) As Integer
        Dim i As Integer

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or Itemnum <= 0 Or ItemNum > MAX_ITEMS Then
            Exit Function
        End If

        If Type.Item(ItemNum).Type = ItemType.Currency Or Type.Item(ItemNum).Stackable = 1 Then
            ' If currency then check to see if they already have an instance of the item and add it to that
            For i = 1 To MAX_INV
                If GetPlayerInv(index, i) = ItemNum Then
                    FindOpenInvSlot = i
                    Exit Function
                End If
            Next
        End If

        For i = 1 To MAX_INV
            ' Try to find an open free slot
            If GetPlayerInv(index, i) = 0 Then
                FindOpenInvSlot = i
                Exit Function
            End If
        Next

        FindOpenInvSlot = -1
    End Function

    Function TakeInv(index As Integer, ItemNum As Integer, ItemVal As Integer) As Boolean
        Dim i As Integer

        TakeInv = 0

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or Itemnum <= 0 Or ItemNum > MAX_ITEMS Then
            Exit Function
        End If

        For i = 1 To MAX_INV

            ' Check to see if the player has the item
            If GetPlayerInv(index, i) = ItemNum Then
                If Type.Item(ItemNum).Type = ItemType.Currency Or Type.Item(ItemNum).Stackable = 1 Then

                    ' Is what we are trying to take away more then what they have?  If so just set it to zero
                    If ItemVal >= GetPlayerInvValue(index, i) Then
                        TakeInv = 1
                    Else
                        SetPlayerInvValue(index, i, GetPlayerInvValue(index, i) - ItemVal)
                        SendInventoryUpdate(index, i)
                    End If
                Else
                    TakeInv = 1
                End If

                If TakeInv Then
                    SetPlayerInv(index, i, 0)
                    SetPlayerInvValue(index, i, 0)
                    ' Send the inventory update
                    SendInventoryUpdate(index, i)
                    Exit Function
                End If
            End If

        Next

    End Function

    Function GiveInv(index As Integer, ItemNum As Integer, ItemVal As Integer, Optional SendUpdate As Boolean = True) As Boolean
        Dim i As Integer

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or Itemnum <= 0 Or ItemNum > MAX_ITEMS Then
            GiveInv = 0
            Exit Function
        End If

        i = FindOpenInvSlot(index, ItemNum)

        ' Check to see if inventory is full
        If i <> -1 Then
            SetPlayerInv(index, i, ItemNum)
            SetPlayerInvValue(index, i, GetPlayerInvValue(index, i) + ItemVal)
            If SendUpdate Then SendInventoryUpdate(index, i)
            GiveInv = 1
        Else
            PlayerMsg(index, "Your inventory is full.", ColorType.BrightRed)
            GiveInv = 0
        End If

    End Function

    Sub PlayerMapDropItem(index As Integer, invNum As Integer, amount As Integer)
        Dim i As Integer

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or invNum <= 0 Or invNum > MAX_INV Then
            Exit Sub
        End If

        ' check the player isn't doing something
        If TempPlayer(index).InBank Or TempPlayer(index).InShop Or TempPlayer(index).InTrade > 0 Then Exit Sub

        If Type.Moral(GetPlayerMap(index)).CanDropItem = 0 Then
            Call PlayerMsg(index, "You can't drop items here!", ColorType.BrightRed)
            Exit Sub
        End If

        If (GetPlayerInv(index, invNum) > 0) Then
            If (GetPlayerInv(index, invNum) <= MAX_ITEMS) Then
                i = FindOpenMapItemSlot(GetPlayerMap(index))

                If i <> 0 Then
                    With MapItem(GetPlayerMap(index), i)
                        .Num = GetPlayerInv(index, invNum)
                        .X = GetPlayerX(index)
                        .Y = GetPlayerY(index)
                        .PlayerName = GetPlayerName(index)
                        .PlayerTimer = GetTimeMs() + ITEM_SPAWN_TIME

                        .CanDespawn = 1
                        .DespawnTimer = GetTimeMs() + ITEM_DESPAWN_TIME

                        If Type.Item(GetPlayerInv(index, invNum)).Type = ItemType.Currency Or Type.Item(Global.Core.Commands.GetPlayerInv(index, invNum)).Stackable = 1 Then
                            ' Check if its more then they have and if so drop it all
                            If amount >= GetPlayerInvValue(index, invNum) Then
                                amount = GetPlayerInvValue(index, invNum)
                                .Value = amount
                                SetPlayerInv(index, invNum, 0)
                                SetPlayerInvValue(index, invNum, 0)
                            Else
                                .Value = amount
                                SetPlayerInvValue(index, invNum, GetPlayerInvValue(index, invNum) - amount)
                            End If
                            MapMsg(GetPlayerMap(index), String.Format("{0} has dropped {1} ({2}x).", GetPlayerName(index), CheckGrammar(Type.Item(GetPlayerInv(index, invNum)).Name), amount), ColorType.Yellow)
                        Else
                            ' It's not a currency object so this is easy
                            .Value = 1

                            ' send message
                            MapMsg(GetPlayerMap(index), String.Format("{0} has dropped {1}.", GetPlayerName(index), CheckGrammar(Type.Item(GetPlayerInv(index, invNum)).Name)), ColorType.Yellow)
                            SetPlayerInv(index, invNum, 0)
                            SetPlayerInvValue(index, invNum, 0)
                        End If

                        ' Send inventory update
                        SendInventoryUpdate(index, invNum)
                        ' Spawn the item before we set the num or we'll get a different free map item slot
                        SpawnItemSlot(i, .Num, amount, GetPlayerMap(index), GetPlayerX(index), GetPlayerY(index))
                    End With
                Else
                    PlayerMsg(index, "Too many items already on the ground.", ColorType.Yellow)
                End If
            End If
        End If

    End Sub

    Function TakeInvSlot(index As Integer, InvSlot As Integer, ItemVal As Integer) As Boolean
        Dim itemNum

        TakeInvSlot = 0

        ' Check for subscript out of range
        If IsPlaying(index) = 0 Or InvSlot < 0 Or InvSlot > MAX_ITEMS Then Exit Function

        itemNum = GetPlayerInv(index, InvSlot)

        If Type.Item(itemNum).Type = ItemType.Currency Or Type.Item(itemNum).Stackable = 1 Then

            ' Is what we are trying to take away more then what they have?  If so just set it to zero
            If ItemVal >= GetPlayerInvValue(index, InvSlot) Then
                TakeInvSlot = 1
            Else
                SetPlayerInvValue(index, InvSlot, GetPlayerInvValue(index, InvSlot) - ItemVal)
            End If
        Else
            TakeInvSlot = 1
        End If

        If TakeInvSlot Then
            SetPlayerInv(index, InvSlot, 0)
            SetPlayerInvValue(index, InvSlot, 0)
            Exit Function
        End If

    End Function

    Function CanPlayerUseItem(Index As Integer, itemNum As Integer)
        Dim i As Integer

        If Type.Map(GetPlayerMap(index)).Moral > 0 Then
            If Type.Moral(Type.Map(GetPlayerMap(index)).Moral).CanUseItem = 0 Then
                PlayerMsg(Index, "You can't use items here!", ColorType.BrightRed)
                Exit Function
            End If
        End If

        For i = 1 To StatType.Count - 1
            If GetPlayerStat(Index, i) < Type.Item(itemNum).Stat_Req(i) Then
                PlayerMsg(Index, "You do not meet the stat requirements to use this item.", ColorType.BrightRed)
                Exit Function
            End If
        Next

        If Type.Item(itemNum).LevelReq > GetPlayerLevel(Index) Then
            PlayerMsg(Index, "You do not meet the level requirements to use this item.", ColorType.BrightRed)
            Exit Function
        End If

        ' Make sure they are the right job
        If Not Type.Item(itemNum).JobReq = GetPlayerJob(Index) And Not Type.Item(itemNum).JobReq = 0 Then
            PlayerMsg(Index, "You do not meet the class requirements to use this item.", ColorType.BrightRed)
            Exit Function
        End If

        ' access requirement
        If Not GetPlayerAccess(Index) >= Type.Item(itemNum).AccessReq Then
            PlayerMsg(Index, "You do not meet the access requirement to equip this item.", ColorType.BrightRed)
            Exit Function
        End If

        ' check the player isn't doing something
        If TempPlayer(index).InBank Or TempPlayer(index).InShop Or TempPlayer(index).InTrade > 0 Then
            PlayerMsg(Index, "You can't use items while in a bank, shop, or trade!", ColorType.BrightRed)
            Exit Function
        End If

        CanPlayerUseItem = 1
    End Function

    Friend Sub UseItem(index As Integer, InvNum As Integer)
        Dim itemNum As Integer, i As Integer, n As Integer, tempitem As Integer
        Dim m As Integer, tempdata(StatType.Count + 3) As Integer, tempstr(2) As String

        ' Prevent hacking
        If InvNum <= 0 Or InvNum > MAX_INV Then Exit Sub

        itemNum = GetPlayerInv(index, InvNum)

        If itemNum <= 0 Or itemNum > MAX_ITEMS Then Exit Sub

        If CanPlayerUseItem(index, itemNum) = 0 Then Exit Sub

        ' Find out what kind of item it is
        Select Case Type.Item(itemNum).Type
            Case ItemType.Equipment
                Select Case Type.Item(itemNum).SubType
                    Case EquipmentType.Weapon

                        If Type.Item(itemNum).TwoHanded > 0 Then
                            If GetPlayerEquipment(index, EquipmentType.Shield) > 0 Then
                                PlayerMsg(index, "This is a 2-Handed weapon! Please unequip your shield first.", ColorType.BrightRed)
                                Exit Sub
                            End If
                        End If

                        If GetPlayerEquipment(index, EquipmentType.Weapon) > 0 Then
                            tempitem = GetPlayerEquipment(index, EquipmentType.Weapon)
                        End If

                        SetPlayerEquipment(index, itemNum, EquipmentType.Weapon)

                        PlayerMsg(index, "You equip " & CheckGrammar(Type.Item(itemNum).Name), ColorType.BrightGreen)

                        SetPlayerInv(index, InvNum, 0)
                        SetPlayerInvValue(index, InvNum, 0)

                        If tempitem > 0 Then ' give back the stored item
                            m = FindOpenInvSlot(index, tempitem)
                            SetPlayerInv(index, m, tempitem)
                            SetPlayerInvValue(index, m, 0)
                        End If

                        SendWornEquipment(index)
                        SendMapEquipment(index)
                        SendInventory(index)
                        SendInventoryUpdate(index, InvNum)
                        SendStats(index)

                        ' send vitals
                        SendVitals(index)

                    Case EquipmentType.Armor
                        If GetPlayerEquipment(index, EquipmentType.Armor) > 0 Then
                            tempitem = GetPlayerEquipment(index, EquipmentType.Armor)
                        End If

                        SetPlayerEquipment(index, itemNum, EquipmentType.Armor)

                        PlayerMsg(index, "You equip " & CheckGrammar(Type.Item(itemNum).Name), ColorType.BrightGreen)
                        TakeInv(index, itemNum, 0)

                        If tempitem > 0 Then ' Return their old equipment to their inventory.
                            m = FindOpenInvSlot(index, tempitem)
                            SetPlayerInv(index, m, tempitem)
                            SetPlayerInvValue(index, m, 0)
                        End If

                        SendWornEquipment(index)
                        SendMapEquipment(index)

                        SendInventory(index)
                        SendStats(index)

                        ' send vitals
                        SendVitals(index)

                    Case EquipmentType.Helmet
                        If GetPlayerEquipment(index, EquipmentType.Helmet) > 0 Then
                            tempitem = GetPlayerEquipment(index, EquipmentType.Helmet)
                        End If

                        SetPlayerEquipment(index, itemNum, EquipmentType.Helmet)

                        PlayerMsg(index, "You equip " & CheckGrammar(Type.Item(itemNum).Name), ColorType.BrightGreen)
                        TakeInv(index, itemNum, 1)

                        If tempitem > 0 Then ' give back the stored item
                            m = FindOpenInvSlot(index, tempitem)
                            SetPlayerInv(index, m, tempitem)
                            SetPlayerInvValue(index, m, 0)
                        End If

                        SendWornEquipment(index)
                        SendMapEquipment(index)
                        SendInventory(index)
                        SendStats(index)

                        ' send vitals
                        SendVitals(index)

                    Case EquipmentType.Shield
                        If Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).TwoHanded > 0 Then
                            PlayerMsg(index, "Please unequip your 2-handed weapon first.", ColorType.BrightRed)
                            Exit Sub
                        End If

                        If GetPlayerEquipment(index, EquipmentType.Shield) > 0 Then
                            tempitem = GetPlayerEquipment(index, EquipmentType.Shield)
                        End If

                        SetPlayerEquipment(index, itemNum, EquipmentType.Shield)

                        PlayerMsg(index, "You equip " & CheckGrammar(Type.Item(itemNum).Name), ColorType.BrightGreen)
                        TakeInv(index, itemNum, 1)

                        If tempitem > 0 Then ' give back the stored item
                            m = FindOpenInvSlot(index, tempitem)
                            SetPlayerInv(index, m, tempitem)
                            SetPlayerInvValue(index, m, 0)
                        End If

                        SendWornEquipment(index)
                        SendMapEquipment(index)
                        SendInventory(index)
                        SendStats(index)

                        ' send vitals
                        SendVitals(index)

                End Select

            Case ItemType.Consumable
                Select Case Type.Item(itemNum).SubType
                    Case ConsumableType.HP
                        SendActionMsg(GetPlayerMap(index), "+" & Type.Item(itemNum).Data1, ColorType.BrightGreen, ActionMsgType.Scroll, GetPlayerX(index) * 32, GetPlayerY(index) * 32)
                        SendAnimation(GetPlayerMap(index), Type.Item(itemNum).Animation, 0, 0, TargetType.Player, index)
                        SetPlayerVital(index, VitalType.HP, GetPlayerVital(index, VitalType.HP) + Type.Item(itemNum).Data1)
                        If Type.Item(itemNum).Stackable = 1 Then
                            TakeInv(index, itemNum, 1)
                        Else
                            TakeInv(index, itemNum, 0)
                        End If
                        SendVital(index, VitalType.HP)

                    Case ConsumableType.MP
                        SendActionMsg(GetPlayerMap(index), "+" & Type.Item(itemNum).Data1, ColorType.BrightBlue, ActionMsgType.Scroll, GetPlayerX(index) * 32, GetPlayerY(index) * 32)
                        SendAnimation(GetPlayerMap(index), Type.Item(itemNum).Animation, 0, 0, TargetType.Player, index)
                        SetPlayerVital(index, VitalType.SP, GetPlayerVital(index, VitalType.SP) + Type.Item(itemNum).Data1)
                        If Type.Item(itemNum).Stackable = 1 Then
                            TakeInv(index, itemNum, 1)
                        Else
                            TakeInv(index, itemNum, 0)
                        End If
                        SendVital(index, VitalType.SP)

                    Case ConsumableType.SP
                        SendAnimation(GetPlayerMap(index), Type.Item(itemNum).Animation, 0, 0, TargetType.Player, index)
                        SetPlayerVital(index, VitalType.SP, GetPlayerVital(index, VitalType.SP) + Type.Item(itemNum).Data1)
                        If Type.Item(itemNum).Stackable = 1 Then
                            TakeInv(index, itemNum, 1)
                        Else
                            TakeInv(index, itemNum, 0)
                        End If
                        SendVital(index, VitalType.SP)

                    Case ConsumableType.Exp
                        SendAnimation(GetPlayerMap(index), Type.Item(itemNum).Animation, 0, 0, TargetType.Player, index)
                        SetPlayerExp(index, GetPlayerExp(index) + Type.Item(itemNum).Data1)
                        If Type.Item(itemNum).Stackable = 1 Then
                            TakeInv(index, itemNum, 1)
                        Else
                            TakeInv(index, itemNum, 0)
                        End If
                        SendExp(index)

                End Select

            Case ItemType.Projectile
                If Type.Item(itemNum).Ammo > 0 Then
                    If HasItem(index, Type.Item(itemNum).Ammo) Then
                        TakeInv(index, Type.Item(itemNum).Ammo, 1)
                        PlayerFireProjectile(index)
                    Else
                        PlayerMsg(index, "No More " & Type.Item(Type.Item(GetPlayerEquipment(index, EquipmentType.Weapon)).Ammo).Name & " !", ColorType.BrightRed)
                        Exit Sub
                    End If
                Else
                    PlayerFireProjectile(index)
                    Exit Sub
                End If

            Case ItemType.CommonEvent
                n = Type.Item(itemNum).Data1

                Select Case Type.Item(itemNum).SubType
                    Case CommonEventType.Variable
                        Type.Player(index).Variables(n) = Type.Item(itemNum).Data2
                    Case CommonEventType.Switch
                        Type.Player(index).Switches(n) = Type.Item(itemNum).Data2
                    Case CommonEventType.Key
                        TriggerEvent(index, 1, 0, GetPlayerX(index), GetPlayerY(index))
                    Case CommonEventType.CustomScript
                        CustomScript(index, Type.Item(itemNum).Data2, GetPlayerMap(index), n)
                End Select

            Case ItemType.Skill
                PlayerLearnSkill(index, itemNum)

            Case ItemType.Pet
                If Type.Item(itemNum).Stackable = 1 Then
                    TakeInv(index, itemNum, 1)
                Else
                    TakeInv(index, itemNum, 0)
                End If
                n = Type.Item(itemNum).Data1
                AdoptPet(index, n)
        End Select
    End Sub

    Sub PlayerLearnSkill(Index As Integer, itemNum As Integer, Optional skillNum As Integer = 0)
        Dim n As Integer, i As Integer

        ' Get the skill num
        If skillNum > 0 Then
            n = skillNum
        Else
            n = Type.Item(itemNum).Data1
        End If

        If n < 1 Or n > MAX_SKILLS Then Exit Sub

        If n > 0 Then
            ' Make sure they are the right class
            If Type.Skill(n).JobReq = GetPlayerJob(index) Or Type.Skill(n).JobReq = 0 Then
                ' Make sure they are the right level
                i = Type.Skill(n).LevelReq

                If i <= GetPlayerLevel(index) Then
                    i = FindOpenSkill(index)

                    ' Make sure they have an open skill slot
                    If i > 0 Then

                        ' Make sure they dont already have the skill
                        If Not HasSkill(index, n) Then
                            SetPlayerSkill(index, i, n)
                            SendAnimation(GetPlayerMap(index), Type.Item(itemNum).Animation, 0, 0, TargetType.Player, index)
                            TakeInv(index, itemNum, 0)
                            PlayerMsg(index, "You study the skill carefully.", ColorType.Yellow)
                            PlayerMsg(index, "You have learned a new skill!", ColorType.BrightGreen)
                            SendPlayerSkills(Index)
                        Else
                            PlayerMsg(index, "You have already learned this skill!", ColorType.BrightRed)
                        End If
                    Else
                        PlayerMsg(index, "You have learned all that you can learn!", ColorType.BrightRed)
                    End If
                Else
                    PlayerMsg(index, "You must be level " & i & " to learn this skill.", ColorType.Yellow)
                End If
            Else
                PlayerMsg(index, String.Format("Only {0} can use this skill.", CheckGrammar(Type.Job(Type.Skill(n).JobReq).Name, 1)), ColorType.BrightRed)
            End If
        Else
            PlayerMsg(index, "This scroll is not connected to a skill, please inform an admin!", ColorType.BrightRed)
        End If
    End Sub

    Sub PlayerSwitchInvSlots(index As Integer, OldSlot As Integer, NewSlot As Integer)
        Dim OldNum As Integer, OldValue As Integer, OldRarity As Integer, OldPrefix As String
        Dim OldSuffix As String, OldSpeed As Integer, OldDamage As Integer
        Dim NewNum As Integer, NewValue As Integer, NewRarity As Integer, NewPrefix As String
        Dim NewSuffix As String, NewSpeed As Integer, NewDamage As Integer

        If OldSlot = 0 Or NewSlot = 0 Then Exit Sub

        OldNum = GetPlayerInv(index, OldSlot)
        OldValue = GetPlayerInvValue(index, OldSlot)
        NewNum = GetPlayerInv(index, NewSlot)
        NewValue = GetPlayerInvValue(index, NewSlot)

        If OldNum = NewNum And Type.Item(NewNum).Stackable = 1 Then ' same item, if we can stack it, lets do that :P
            SetPlayerInv(index, NewSlot, NewNum)
            SetPlayerInvValue(index, NewSlot, OldValue + NewValue)
            SetPlayerInv(index, OldSlot, 0)
            SetPlayerInvValue(index, OldSlot, 0)
        Else
            SetPlayerInv(index, NewSlot, OldNum)
            SetPlayerInvValue(index, NewSlot, OldValue)
            SetPlayerInv(index, OldSlot, NewNum)
            SetPlayerInvValue(index, OldSlot, NewValue)
        End If

        SendInventory(index)
    End Sub

     Sub PlayerSwitchSkillSlots(index As Integer, OldSlot As Integer, NewSlot As Integer)
        Dim OldNum As Integer, OldValue As Integer, OldRarity As Integer, OldPrefix As String
        Dim OldSuffix As String, OldSpeed As Integer, OldDamage As Integer
        Dim NewNum As Integer, NewValue As Integer, NewRarity As Integer, NewPrefix As String
        Dim NewSuffix As String, NewSpeed As Integer, NewDamage As Integer

        If OldSlot = 0 Or NewSlot = 0 Then Exit Sub

        OldNum = GetPlayerSkill(index, OldSlot)
        OldValue = GetPlayerSkillCD(index, OldSlot)
        NewNum = GetPlayerSkill(index, NewSlot)
        NewValue = GetPlayerSkillCD(index, NewSlot)

        If OldNum = NewNum And Type.Item(NewNum).Stackable = 1 Then ' same item, if we can stack it, lets do that :P
            SetPlayerSkill(index, NewSlot, NewNum)
            SetPlayerSkillCD(index, NewSlot, NewValue)
            SetPlayerSkill(index, OldSlot, 0)
            SetPlayerSkillCD(index, OldSlot, 0)
        Else
            SetPlayerSkill(index, NewSlot, OldNum)
            SetPlayerSkillCD(index, NewSlot, OldValue)
            SetPlayerSkill(index, OldSlot, NewNum)
            SetPlayerSkillCD(index, OldSlot, NewValue)
        End If

        SendPlayerSkills(index)
    End Sub

#End Region

#Region "Equipment"

    Sub CheckEquippedItems(index As Integer)
        Dim itemNum As Integer
        Dim i As Integer

        ' We want to check incase an admin takes away an object but they had it equipped
        For i = 1 To EquipmentType.Count - 1
            itemNum = GetPlayerEquipment(index, i)

            If itemNum > 0 Then

                Select Case i
                    Case EquipmentType.Weapon

                        If Type.Item(itemNum).SubType <> EquipmentType.Weapon Then SetPlayerEquipment(index, 0, i)
                    Case EquipmentType.Armor

                        If Type.Item(itemNum).SubType <> EquipmentType.Armor Then SetPlayerEquipment(index, 0, i)
                    Case EquipmentType.Helmet

                        If Type.Item(itemNum).SubType <> EquipmentType.Helmet Then SetPlayerEquipment(index, 0, i)
                    Case EquipmentType.Shield

                        If Type.Item(itemNum).SubType <> EquipmentType.Shield Then SetPlayerEquipment(index, 0, i)
                End Select
            Else
                SetPlayerEquipment(index, 0, i)
            End If

        Next

    End Sub

    Sub PlayerUnequipItem(index As Integer, EqSlot As Integer)
        Dim i As Integer, m As Integer, itemnum As Integer

        If EqSlot < 1 Or EqSlot > EquipmentType.Count - 1 Then Exit Sub ' exit out early if error'd

        If FindOpenInvSlot(index, GetPlayerEquipment(index, EqSlot)) > 0 Then
            itemnum = GetPlayerEquipment(index, EqSlot)

            m = FindOpenInvSlot(index, Type.Player(index).Equipment(EqSlot))
            SetPlayerInv(index, m, Type.Player(index).Equipment(EqSlot))
            SetPlayerInvValue(index, m, 0)

            PlayerMsg(index, "You unequip " & CheckGrammar(Type.Item(GetPlayerEquipment(index, EqSlot)).Name), ColorType.Yellow)

            ' remove equipment
            SetPlayerEquipment(index, 0, EqSlot)
            SendWornEquipment(index)
            SendMapEquipment(index)
            SendStats(index)
            SendInventory(index)

            ' send vitals
            SendVitals(index)
        Else
            PlayerMsg(index, "Your inventory is full.", ColorType.BrightRed)
        End If

    End Sub

#End Region

#Region "Misc"

    Sub JoinGame(index As Integer)
        Dim i As Integer

        ' Set the flag so we know the person is in the game
        TempPlayer(index).InGame = 1

        ' Notify everyone that a player has joined the game.
        GlobalMsg(String.Format("{0} has joined {1}!", GetPlayerName(index), Settings.GameName))

        ' Warp the player to his saved location
        PlayerWarp(index, GetPlayerMap(index), GetPlayerX(index), GetPlayerY(index))

        ' Send all the required game data to the user.
        CheckEquippedItems(index)
        SendInventory(index)
        SendWornEquipment(index)
        SendMapEquipment(index)
        SendExp(index)
        SendHotbar(index)
        SendPlayerSkills(index)
        SendStats(index)
        SendJoinMap(index)
        SendUpdatePlayerPet(index, True)
        SendTimeTo(index)
        SendGameClockTo(index)
        SendPets(index)
        SendProjectiles(index)

        ' Send welcome messages
        SendWelcome(index)

        ' Send the flag so they know they can start doing stuff
        SendInGame(index)

        UpdateCaption()
    End Sub

    Sub LeftGame(index As Integer)
        Dim i As Integer
        Dim tradeTarget As Integer

        If TempPlayer(index).InGame Then
            SendLeftMap(index)
            TempPlayer(index).InGame = 0

            ' Check if the player was in a party, and if so cancel it out so the other player doesn't continue to get half exp
            ' leave party.
            Party_PlayerLeave(index)

            ' cancel any trade they're in
            If TempPlayer(index).InTrade > 0 Then
                tradeTarget = TempPlayer(index).InTrade
                PlayerMsg(tradeTarget, String.Format("{0} has declined the trade.", GetPlayerName(index)), ColorType.BrightRed)
                ' clear out trade
                For i = 1 To MAX_INV
                    TempPlayer(tradeTarget).TradeOffer(i).Num = 0
                    TempPlayer(tradeTarget).TradeOffer(i).Value = 0
                Next
                TempPlayer(tradeTarget).InTrade = 0
                SendCloseTrade(tradeTarget)
            End If

            ' Send a global message that he/she left
            GlobalMsg(String.Format("{0} has left {1}!", GetPlayerName(index), Settings.GameName))

            Call Global.System.Console.WriteLine(String.Format("{0} has left {1}!", GetPlayerName(index), Settings.GameName))

            ReCallPet(index)
            SaveCharacter(index, TempPlayer(index).Slot)
            SaveBank(index)
        End If

        ClearAccount(index)
        UpdateCaption()
    End Sub

    Friend Sub KillPlayer(index As Integer)
        Dim exp As Integer

        ' Calculate exp to give attacker
        exp = GetPlayerExp(index) \ 3

        ' Make sure we dont get less then 0
        If exp < 0 Then exp = 0
        If exp = 0 Then
            PlayerMsg(index, "You've lost no experience.", ColorType.BrightGreen)
        Else
            SetPlayerExp(index, GetPlayerExp(index) - exp)
            SendExp(index)
            PlayerMsg(index, String.Format("You've lost {0} experience.", exp), ColorType.BrightRed)
        End If

        OnDeath(index)
    End Sub

    Sub OnDeath(index As Integer)
        ' Set HP to nothing
        SetPlayerVital(index, VitalType.HP, 0)

        ' Warp player away
        SetPlayerDir(index, DirectionType.Down)

        With Type.Map(GetPlayerMap(index))
            ' to the bootmap if it is set
            If .BootMap > 0 Then
                PlayerWarp(index, .BootMap, .BootX, .BootY)
            Else
                PlayerWarp(index, Type.Job(GetPlayerJob(index)).StartMap, Type.Job(GetPlayerJob(index)).StartX, Type.Job(GetPlayerJob(index)).StartY)
            End If
        End With

        ' Clear skill casting
        TempPlayer(index).SkillBuffer = 0
        TempPlayer(index).SkillBufferTimer = 0
        SendClearSkillBuffer(index)

        ' Restore vitals
        For i = 1 To VitalType.Count - 1
            SetPlayerVital(index, i, GetPlayerMaxVital(index, i))
        Next
        SendVitals(index)

        ' If the player the attacker killed was a pk then take it away
        If GetPlayerPK(index) Then
            SetPlayerPK(index, False)
            SendPlayerData(index)
        End If

    End Sub

    Function GetPlayerVitalRegen(index As Integer, Vital As VitalType) As Integer
        Dim i As Integer

        ' Prevent subscript out of range
        If IsPlaying(index) = 0 Or index < 0 Or index > MAX_PLAYERS Then
            GetPlayerVitalRegen = 0
            Exit Function
        End If

        Select Case Vital
            Case VitalType.HP
                i = (GetPlayerStat(index, StatType.Vitality) \ 2)
            Case VitalType.SP
                i = (GetPlayerStat(index, StatType.Spirit) \ 2)
            Case VitalType.SP
                i = (GetPlayerStat(index, StatType.Spirit) \ 2)
        End Select

        If i < 2 Then i = 2
        GetPlayerVitalRegen = i
    End Function

    Friend Sub HandleNpcKillExperience(index As Integer, NpcNum As Integer)
        ' Get the experience we'll have to hand out. If it's negative, just ignore this method.
        Dim Experience = Type.NPC(NPCNum).Exp
        If Experience < 0 Then Exit Sub

        ' Is our player in a party? If so, hand out exp to everyone.
        If IsPlayerInParty(index) Then
            Party_ShareExp(GetPlayerParty(index), Experience, index, GetPlayerMap(index))
        Else
            GivePlayerExp(index, Experience)
        End If
    End Sub

    Friend Sub HandlePlayerKillExperience(Attacker As Integer, Victim As Integer)
        ' Calculate exp to give attacker
        Dim exp = (GetPlayerExp(Victim) \ 10)

        ' Make sure we dont get less then 0
        If exp < 0 Then
            exp = 0
        End If

        If exp = 0 Then
            PlayerMsg(Victim, "You've lost no exp.", ColorType.BrightRed)
            PlayerMsg(Attacker, "You've received no exp.", ColorType.BrightBlue)
        Else
            SetPlayerExp(Victim, GetPlayerExp(Victim) - exp)
            SendExp(Victim)
            PlayerMsg(Victim, String.Format("You've lost {0} exp.", exp), ColorType.BrightRed)

            ' check if we're in a party
            If IsPlayerInParty(Attacker) > 0 Then
                ' pass through party exp share function
                Party_ShareExp(GetPlayerParty(Attacker), exp, Attacker, GetPlayerMap(Attacker))
            Else
                ' not in party, get exp for self
                GivePlayerExp(Attacker, exp)
            End If
        End If
    End Sub

#End Region

#Region "Skills"
    Friend Sub BufferSkill(index As Integer, Skillslot As Integer)
        Dim skillnum As Integer
        Dim MPCost As Integer
        Dim LevelReq As Integer
        Dim mapNum As Integer
        Dim SkillCastType As Integer
        Dim JobReq As Integer
        Dim AccessReq As Integer
        Dim range As Integer
        Dim HasBuffered As Boolean

        Dim TargetType As TargetType
        Dim Target As Integer

        ' Prevent subscript out of range
        If Skillslot < 0 Or Skillslot > MAX_PLAYER_SKILLS Then Exit Sub

        skillnum = GetPlayerSkill(index, Skillslot)
        mapNum = GetPlayerMap(index)

        If skillnum <= 0 Or skillnum > MAX_SKILLS Then Exit Sub

        ' Make sure player has the skill
        If Not HasSkill(index, skillnum) Then Exit Sub

        ' see if cooldown has finished
        If TempPlayer(index).SkillCd(Skillslot) > GetTimeMs() Then
            PlayerMsg(index, "Skill hasn't cooled down yet!", ColorType.Yellow)
            Exit Sub
        End If

        MPCost = Type.Skill(skillNum).MpCost

        ' Check if they have enough MP
        If GetPlayerVital(index, VitalType.SP) < MPCost Then
            PlayerMsg(index, "Not enough mana!", ColorType.Yellow)
            Exit Sub
        End If

        LevelReq = Type.Skill(skillNum).LevelReq

        ' Make sure they are the right level
        If LevelReq > GetPlayerLevel(index) Then
            PlayerMsg(index, "You must be level " & LevelReq & " to use this skill.", ColorType.BrightRed)
            Exit Sub
        End If

        AccessReq = Type.Skill(skillNum).AccessReq

        ' make sure they have the right access
        If AccessReq > GetPlayerAccess(index) Then
            PlayerMsg(index, "You must be an administrator to use this skill.", ColorType.BrightRed)
            Exit Sub
        End If

        JobReq = Type.Skill(skillNum).JobReq

        ' make sure the JobReq > 0
        If JobReq > 0 Then ' 0 = no req
            If JobReq <> GetPlayerJob(index) Then
                PlayerMsg(index, "Only " & CheckGrammar(Type.Job(JobReq).Name) & " can use this skill.", ColorType.Yellow)
                Exit Sub
            End If
        End If

        ' find out what kind of skill it is! self cast, target or AOE
        If Type.Skill(skillNum).Range > 0 Then
            ' ranged attack, single target or aoe?
            If Not Type.Skill(skillNum).IsAoE Then
                SkillCastType = 2 ' targetted
            Else
                SkillCastType = 3 ' targetted aoe
            End If
        Else
            If Not Type.Skill(skillNum).IsAoE Then
                SkillCastType = 0 ' self-cast
            Else
                SkillCastType = 1 ' self-cast AoE
            End If
        End If

        TargetType = TempPlayer(index).TargetType
        Target = TempPlayer(index).Target
        range = Type.Skill(skillNum).Range
        HasBuffered = 0

        Select Case SkillCastType
            Case 0, 1 ' self-cast & self-cast AOE
                HasBuffered = 1
            Case 2, 3 ' targeted & targeted AOE
                ' check if have target
                If Not Target > 0 Then
                    PlayerMsg(index, "You do not have a target.", ColorType.BrightRed)
                End If
                If TargetType = TargetType.Player Then
                    ' if have target, check in range
                    If Not IsInRange(range, GetPlayerX(index), GetPlayerY(index), GetPlayerX(Target), GetPlayerY(Target)) Then
                        PlayerMsg(index, "Target not in range.", ColorType.BrightRed)
                    Else
                        ' go through skill Type
                        If Type.Skill(skillNum).Type <> SkillType.DamageHp And Type.Skill(skillNum).Type <> SkillType.DamageMp Then
                            HasBuffered = 1
                        Else
                            If CanPlayerAttackPlayer(index, Target, True) Then
                                HasBuffered = 1
                            End If
                        End If
                    End If
                ElseIf TargetType = TargetType.NPC Then
                    ' if have target, check in range
                    If Not IsInRange(range, GetPlayerX(index), GetPlayerY(index), MapNPC(MapNum).NPC(Target).X, MapNPC(MapNum).NPC(Target).Y) Then
                        PlayerMsg(index, "Target not in range.", ColorType.BrightRed)
                        HasBuffered = 0
                    Else
                        ' go through skill Type
                        If Type.Skill(skillNum).Type <> SkillType.DamageHp And Type.Skill(skillNum).Type <> SkillType.DamageMp Then
                            HasBuffered = 1
                        Else
                            If CanPlayerAttackNpc(index, Target, True) Then
                                HasBuffered = 1
                            End If
                        End If
                    End If
                End If
        End Select

        If HasBuffered Then
            SendAnimation(MapNum, Type.Skill(skillNum).CastAnim, 0, 0, TargetType.Player, index)
            TempPlayer(index).SkillBuffer = Skillslot
            TempPlayer(index).SkillBufferTimer = GetTimeMs()
            Exit Sub
        Else
            SendClearSkillBuffer(index)
        End If
    End Sub



#End Region

#Region "Bank"

    Sub GiveBank(index As Integer, InvSlot As Integer, Amount As Integer)
        Dim BankSlot As Integer, itemnum As Integer

        If InvSlot <= 0 Or InvSlot > MAX_INV Then Exit Sub

        If Amount <= 0 Then Amount = 1

        If GetPlayerInvValue(index, InvSlot) < 0 Then Exit Sub
        If GetPlayerInvValue(index, InvSlot) < Amount And GetPlayerInv(index, InvSlot) = 0 Then Exit Sub

        BankSlot = FindOpenBankSlot(index, GetPlayerInv(index, InvSlot))
        itemnum = GetPlayerInv(index, InvSlot)

        If BankSlot > 0 Then
            If Type.Item(GetPlayerInv(index, InvSlot)).Type = ItemType.Currency Or Type.Item(Global.Core.Commands.GetPlayerInv(index, InvSlot)).Stackable = 1 Then
                If GetPlayerBank(index, BankSlot) = GetPlayerInv(index, InvSlot) Then
                    SetPlayerBankValue(index, BankSlot, GetPlayerBankValue(index, BankSlot) + Amount)
                    TakeInv(index, GetPlayerInv(index, InvSlot), Amount)
                Else
                    SetPlayerBank(index, BankSlot, GetPlayerInv(index, InvSlot))
                    SetPlayerBankValue(index, BankSlot, Amount)
                    TakeInv(index, GetPlayerInv(index, InvSlot), Amount)
                End If
            Else
                If GetPlayerBank(index, BankSlot) = GetPlayerInv(index, InvSlot) Then
                    SetPlayerBankValue(index, BankSlot, GetPlayerBankValue(index, BankSlot) + 1)
                    TakeInv(index, GetPlayerInv(index, InvSlot), 0)
                Else
                    SetPlayerBank(index, BankSlot, itemnum)
                    SetPlayerBankValue(index, BankSlot, 1)
                    TakeInv(index, GetPlayerInv(index, InvSlot), 0)
                End If
            End If

            SendBank(index)
        End If

    End Sub

    Function GetPlayerBank(index As Integer, BankSlot As Byte) As Integer
        GetPlayerBank = Bank(index).Item(BankSlot).Num
    End Function

    Sub SetPlayerBank(index As Integer, BankSlot As Byte, ItemNum As Integer)
        Bank(index).Item(BankSlot).Num = ItemNum
    End Sub

    Function GetPlayerBankValue(index As Integer, BankSlot As Byte) As Integer
        GetPlayerBankValue = Bank(index).Item(BankSlot).Value
    End Function

    Sub SetPlayerBankValue(index As Integer, BankSlot As Byte, Value As Integer)
        Bank(index).Item(BankSlot).Value = Value
    End Sub

    Function FindOpenBankSlot(index As Integer, ItemNum As Integer) As Byte
        Dim i As Integer

        If Not IsPlaying(index) Then Exit Function
        If Itemnum <= 0 Or ItemNum > MAX_ITEMS Then Exit Function

        If Type.Item(ItemNum).Type = ItemType.Currency Or Type.Item(ItemNum).Stackable = 1 Then
            For i = 1 To MAX_BANK
                If GetPlayerBank(index, i) = ItemNum Then
                    FindOpenBankSlot = i
                    Exit Function
                End If
            Next
        End If

        For i = 1 To MAX_BANK
            If GetPlayerBank(index, i) = 0 Then
                FindOpenBankSlot = i
                Exit Function
            End If
        Next

    End Function

    Sub TakeBank(index As Integer, BankSlot As Integer, Amount As Integer)
        Dim invSlot

        If BankSlot <= 0 Or BankSlot > MAX_BANK Then Exit Sub

        If Amount <= 0 Then Amount = 1

        If GetPlayerBankValue(index, BankSlot) < Amount Then Exit Sub

        invSlot = FindOpenInvSlot(index, GetPlayerBank(index, BankSlot))

        If invSlot > 0 Then
            If Type.Item(GetPlayerBank(index, BankSlot)).Type = ItemType.Currency Or Type.Item(Global.Server.Player.GetPlayerBank(index, BankSlot)).Stackable = 1 Then
                GiveInv(index, GetPlayerBank(index, BankSlot), Amount)
                SetPlayerBankValue(index, BankSlot, GetPlayerBankValue(index, BankSlot) - Amount)
                If GetPlayerBankValue(index, BankSlot) <= 0 Then
                    SetPlayerBank(index, BankSlot, 0)
                    SetPlayerBankValue(index, BankSlot, 0)
                End If
            Else
                If GetPlayerBank(index, BankSlot) = GetPlayerInv(index, invSlot) Then
                    If GetPlayerBankValue(index, BankSlot) > 1 Then
                        GiveInv(index, GetPlayerBank(index, BankSlot), 0)
                        SetPlayerBankValue(index, BankSlot, GetPlayerBankValue(index, BankSlot) - 1)
                    End If
                Else
                    GiveInv(index, GetPlayerBank(index, BankSlot), 0)
                    SetPlayerBank(index, BankSlot, 0)
                    SetPlayerBankValue(index, BankSlot, 0)
                End If
            End If

        End If

        SendBank(index)
    End Sub

    Sub PlayerSwitchBankSlots(index As Integer, OldSlot As Integer, NewSlot As Integer)
        Dim OldNum As Integer, OldValue As Integer, NewNum As Integer, NewValue As Integer
        Dim i As Integer

        If OldSlot = 0 Or NewSlot = 0 Then Exit Sub

        OldNum = GetPlayerBank(index, OldSlot)
        OldValue = GetPlayerBankValue(index, OldSlot)
        NewNum = GetPlayerBank(index, NewSlot)
        NewValue = GetPlayerBankValue(index, NewSlot)

        SetPlayerBank(index, NewSlot, OldNum)
        SetPlayerBankValue(index, NewSlot, OldValue)

        SetPlayerBank(index, OldSlot, NewNum)
        SetPlayerBankValue(index, OldSlot, NewValue)

        SendBank(index)
    End Sub

#End Region

End Module