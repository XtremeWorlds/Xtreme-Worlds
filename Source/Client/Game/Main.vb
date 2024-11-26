Imports System.Windows.Forms
Imports Core.Enum
Imports Microsoft.Xna.Framework

Module Program
    Sub Main()
        Client.Run()
    End Sub

    Sub UpdateForms()
        If InitEventEditorForm Then
            With frmEditor_Event.Instance
                .Show()
            End With
            InitEventEditorForm = False
        End If

        If GameState.InitAdminForm Then
            With FrmAdmin.Instance
                .Show()
            End With
            GameState.InitAdminForm = False
        End If

        If GameState.InitMapReport Then
            For i = 1 To GameState.MapNames.Length - 1
                Dim item1 As New ListViewItem(i.ToString)
                item1.SubItems.Add(GameState.MapNames(i))
                FrmAdmin.Instance.lstMaps.Items.Add(item1)
            Next
            GameState.InitMapReport = False
        End If

        If GameState.InitMapEditor Then
            With frmEditor_Map.Instance
                GameState.MyEditorType = EditorType.Map
                GameState.EditorIndex = 1
                .Show()
                .MapEditorInit()
            End With
            GameState.InitMapEditor = False
        End If

        If GameState.InitPetEditor Then
            With frmEditor_Pet.Instance
                GameState.MyEditorType = EditorType.Pet
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                PetEditorInit()
            End With
            GameState.InitPetEditor = False
        End If

        If GameState.InitAnimationEditor Then
            With frmEditor_Animation.Instance
                GameState.MyEditorType = EditorType.Animation
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                AnimationEditorInit()
            End With
            GameState.InitAnimationEditor = False
        End If

        If GameState.InitItemEditor Then
            With frmEditor_Item.Instance
                GameState.MyEditorType = EditorType.Item
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                ItemEditorInit()
            End With
            GameState.InitItemEditor = False
        End If

        If GameState.InitJobEditor Then
            With frmEditor_Job.Instance
                GameState.MyEditorType = EditorType.Job
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0

                JobEditorInit()
            End With
            GameState.InitJobEditor = False
        End If

        If GameState.InitMoralEditor Then
            With frmEditor_Moral.Instance
                GameState.MyEditorType = EditorType.Moral
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                MoralEditorInit()
            End With
            GameState.InitMoralEditor = False
        End If

        If GameState.InitResourceEditor Then
            With frmEditor_Resource.Instance
                GameState.MyEditorType = EditorType.Resource
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                ResourceEditorInit()
            End With
            GameState.InitResourceEditor = False
        End If

        If GameState.InitNPCEditor Then
            With frmEditor_NPC.Instance
                GameState.MyEditorType = EditorType.NPC
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                NpcEditorInit()
            End With
            GameState.InitNPCEditor = False
        End If

        If GameState.InitSkillEditor Then
            With frmEditor_Skill.Instance
                GameState.MyEditorType = EditorType.Skill
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                SkillEditorInit()
            End With
            GameState.InitSkillEditor = False
        End If

        If GameState.InitShopEditor Then
            With frmEditor_Shop.Instance
                GameState.MyEditorType = EditorType.Shop
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                ShopEditorInit()
            End With
            GameState.InitShopEditor = False
        End If

        If GameState.InitProjectileEditor Then
            With frmEditor_Projectile.Instance
                GameState.MyEditorType = EditorType.Projectile
                GameState.EditorIndex = 1
                .Show()
                .lstIndex.SelectedIndex = 0
                ProjectileEditorInit()
            End With

            GameState.InitProjectileEditor = False
        End If

        Application.DoEvents()
    End Sub
End Module
