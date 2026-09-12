using BandoWare.GameplayTags.Editor.GameplayTagAssets;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BandoWare.GameplayTags.Editor.Config
{
   internal class GameplayTagProjectSettingsProvider : SettingsProvider
   {
      /////////////////////////////// Member Fields ////////////////////////////////

      public const string SettingsPath = "Project/Custom/Gameplay Tags";
      
      private const string k_DefaultNewTagName = "A.B.C";
      private const float k_SpaceAfterPropertyBlocks = 15f;

      private GameplayTagConfigData m_CachedConfigData;

      private Vector2 m_ScrollPosition;
      private float m_MinTagNameFieldWidth = 100f;
      private float m_TagNameFieldWidth = -1f;

      private float TagNameNameFieldWidth
      {
         get
         {
            if (m_TagNameFieldWidth < 0f)
            {
               m_TagNameFieldWidth = EditorPrefs.GetFloat("GameplayTagProjectSettingsProvider.TagNameFieldWidth", 300f);
            }
            return m_TagNameFieldWidth;
         }
         set
         {
            if (!Mathf.Approximately(m_TagNameFieldWidth, value))
            {
               EditorPrefs.SetFloat("GameplayTagProjectSettingsProvider.TagNameFieldWidth", value);
               m_TagNameFieldWidth = value;
            }
         }
      }

      private readonly GUIStyle m_ListBoxStyle = new(EditorStyles.helpBox)
      {
         padding = new RectOffset(8, 8, 8, 8),
      };

      private readonly GUIStyle m_WhiteTextFieldStyle = new(EditorStyles.textField)
      {
         normal = {textColor = Color.white},
         hover = {textColor = Color.white},
         active = {textColor = Color.white},
         focused = {textColor = Color.white},
      };

      private readonly GUIStyle m_ItalicWhiteTextFieldStyle = new(EditorStyles.textField)
      {
         fontStyle = FontStyle.Italic,
         normal = {textColor = Color.white},
         hover = {textColor = Color.white},
         active = {textColor = Color.white},
         focused = {textColor = Color.white},
      };

      private readonly GUIStyle m_TagNameFieldWidthSliderStyle = new()
      {
         imagePosition = ImagePosition.ImageOnly,
      };

      private readonly GUIStyle m_TagNameFieldWidthThumbStyle = new()
      {
         imagePosition = ImagePosition.ImageOnly,
         clipping = TextClipping.Clip,
         fixedHeight = 16f,
         fixedWidth = 0.5f,
         margin = new RectOffset(0, 0, 3, 0),
         overflow = new RectOffset(1, 1, 0, 0),
         normal = new GUIStyleState(),
      };

      ///////////////////////////// Public Functions ///////////////////////////////

      [SettingsProvider]
      public static SettingsProvider CreateGameplayTagProjectSettingsProvider() => new GameplayTagProjectSettingsProvider();

      public GameplayTagProjectSettingsProvider()
         : base(SettingsPath, SettingsScope.Project)
      {
         keywords = GetSearchKeywordsFromSerializedObject(new SerializedObject(GameplayTagProjectSettings.instance));

         // Initialize background texture for the "TagFieldWidthThumbStyle" GUIStyle
         Texture2D thumbStyleTexture = new(1, 1);
         thumbStyleTexture.SetPixels(new[] {Color.gray, Color.gray});
         thumbStyleTexture.Apply();
         m_TagNameFieldWidthThumbStyle.normal.background = thumbStyleTexture;
      }

      public override void OnActivate(string searchContext, VisualElement rootElement)
      {
         base.OnActivate(searchContext, rootElement);

         m_CachedConfigData = new GameplayTagConfigData();
         m_CachedConfigData.LoadFromGeneratedClass();
      }

      public override void OnDeactivate()
      {
         base.OnDeactivate();

         if (m_CachedConfigData != null && m_CachedConfigData.IsDirty())
         {
            if (DisplayAnySettingChangedDialog())
            {
               m_CachedConfigData.ApplyChanges();
            }

            m_CachedConfigData = null;
         }
      }

      public override void OnGUI(string searchContext)
      {
         base.OnGUI(searchContext);

         GUILayout.Space(10f);
         m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, EditorStyles.inspectorDefaultMargins);

         GameplayTagConfigData configData = m_CachedConfigData;

         EditorGUIUtility.labelWidth = 200f;
         GeneratedClassSettingsGUI(configData);
         TagDefinitionEntriesGUI(configData);
         TagRenameEntriesGUI(configData);
         ApplyAndRevertButtonsGUI(configData);

         GUILayout.EndScrollView();
      }

      private static void GeneratedClassSettingsGUI(GameplayTagConfigData configData)
      {
         EditorGUI.BeginChangeCheck();

         if (GUILayout.Button("Create Gameplay Tags Asset", GUILayout.ExpandWidth(true)))
         {
            GameplayTagAssetsImporter.CreateGameplayTagsAssetFile();
         }
         
         using (new GUILayout.HorizontalScope())
         {
            configData.GeneratedClassPath = EditorGUILayout.TextField("Generated Class Path", configData.GeneratedClassPath);

            if (GUILayout.Button("…", EditorStyles.miniButton, GUILayout.MaxWidth(20)))
            {
               string filePath = EditorUtility.SaveFilePanel("Location for generated C# file",
                                                             Path.GetDirectoryName(configData.GeneratedClassPath),
                                                             Path.GetFileName(configData.GeneratedClassPath), "cs");
               if (!string.IsNullOrEmpty(filePath))
               {
                  if (filePath.StartsWith(Application.dataPath))
                  {
                     filePath = "Assets/" + filePath.Substring(Application.dataPath.Length + 1);
                  }

                  configData.GeneratedClassPath = filePath;
                  configData.SetDirty();
               }
            }
         }

         configData.GeneratedClassName = EditorGUILayout.TextField("Generated Class Name", configData.GeneratedClassName);
         configData.GeneratedClassNamespace = EditorGUILayout.TextField("Generated Class Namespace", configData.GeneratedClassNamespace);

         if (EditorGUI.EndChangeCheck())
         {
            configData.SetDirty();
         }

         GUILayout.Space(k_SpaceAfterPropertyBlocks);
      }

      private void TagDefinitionEntriesGUI(GameplayTagConfigData configData)
      {
         GUILayout.BeginVertical(m_ListBoxStyle);

         GUILayout.Label("Gameplay Tag Definitions", EditorStyles.boldLabel);

         GUIContent duplicateButtonContent = new("Duplicate");
         GUIContent editButtonContent = new("Edit");
         const float buttonExtent = 3f /*Margin*/ + 6f /*Border*/ + 6f /*Padding*/;
         float buttonsWidth = GUI.skin.button.CalcSize(duplicateButtonContent).x + GUI.skin.button.CalcSize(editButtonContent).x + buttonExtent * 2f;

         const float tagFieldsSpace = 6f;

         Rect headerRowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

         Rect sliderRect = headerRowRect;
         sliderRect.xMin = buttonsWidth + tagFieldsSpace / 2f;
         TagNameNameFieldWidth = GUI.HorizontalSlider(sliderRect, TagNameNameFieldWidth, 0f, sliderRect.width, m_TagNameFieldWidthSliderStyle, m_TagNameFieldWidthThumbStyle);
         TagNameNameFieldWidth = Math.Max(m_MinTagNameFieldWidth, TagNameNameFieldWidth);

         Rect tagNameHeaderRect = headerRowRect;
         tagNameHeaderRect.x = buttonsWidth;
         tagNameHeaderRect.width = TagNameNameFieldWidth;
         GUI.Label(tagNameHeaderRect, "Tag Name:");

         Rect descriptionHeaderRect = headerRowRect;
         descriptionHeaderRect.xMin = buttonsWidth + tagNameHeaderRect.width + tagFieldsSpace;
         GUI.Label(descriptionHeaderRect, "Description:");

         // Create temp copy via "ToArray()", so we can modify the original collection during iteration
         foreach (TagDefinitionEntry entry in configData.TagDefinitionEntries.ToArray())
         {
            using (new GUILayout.HorizontalScope())
            {
               if (GUILayout.Button(duplicateButtonContent, GUILayout.ExpandWidth(false)))
               {
                  TagDefinitionEntry newEntry = new(entry.tagName, entry.description);
                  GameplayTagDefinitionEditWindow.ShowWindow(TagDefinitionOperation.CreateNew, configData, newEntry, Repaint);
               }

               if (GUILayout.Button(editButtonContent, GUILayout.ExpandWidth(false)))
               {
                  GameplayTagDefinitionEditWindow.ShowWindow(TagDefinitionOperation.EditExisting, configData, entry, Repaint);
               }

               GUI.enabled = false;
               EditorGUILayout.TextField(entry.tagName, m_WhiteTextFieldStyle, GUILayout.Width(TagNameNameFieldWidth - tagFieldsSpace / 2f));
               GUILayout.Space(tagFieldsSpace);
               EditorGUILayout.TextField(entry.description, m_ItalicWhiteTextFieldStyle);
               GUI.enabled = true;
            }
         }

         using (new GUILayout.HorizontalScope())
         {
            if (GUILayout.Button("Add New Gameplay Tag", GUILayout.ExpandWidth(true)))
            {
               TagDefinitionEntry newEntry = new(k_DefaultNewTagName, "");
               GameplayTagDefinitionEditWindow.ShowWindow(TagDefinitionOperation.CreateNew, configData, newEntry, Repaint);
            }
         }

         GUILayout.EndVertical();
         GUILayout.Space(k_SpaceAfterPropertyBlocks);
      }

      private void TagRenameEntriesGUI(GameplayTagConfigData configData)
      {
         GUILayout.BeginVertical(m_ListBoxStyle);

         GUILayout.Label("Renamed Gameplay Tags", EditorStyles.boldLabel);
         GUILayout.Space(3f);

         if (configData.TagRenameEntries.Count == 0)
         {
            GUILayout.Label("No Tags renamed yet");
         }

         // Create temp copy via "ToArray()", so we can modify the original collection during iteration
         foreach (TagRenameEntry entry in configData.TagRenameEntries.ToArray())
         {
            using (new GUILayout.HorizontalScope())
            {
               if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
               {
                  if (DisplayDeleteTagRenameEntryDialog(entry))
                  {
                     configData.TagRenameEntries.Remove(entry);
                     configData.SetDirty();
                  }
               }

               GUILayout.Label("Old:", GUILayout.ExpandWidth(false));
               GUI.enabled = false;
               EditorGUILayout.TextField(entry.oldTagName, m_WhiteTextFieldStyle);
               GUI.enabled = true;
               GUILayout.Label("New:", GUILayout.ExpandWidth(false));
               GUI.enabled = false;
               EditorGUILayout.TextField(entry.newTagName, m_WhiteTextFieldStyle);
               GUI.enabled = true;
            }
         }

         GUILayout.EndVertical();
         GUILayout.Space(k_SpaceAfterPropertyBlocks);
      }

      private static void ApplyAndRevertButtonsGUI(GameplayTagConfigData configData)
      {
         // Apply and Revert Buttons:
         using (new EditorGUI.DisabledScope(!configData.IsDirty()))
         {
            using (new GUILayout.HorizontalScope())
            {
               GUILayout.FlexibleSpace();

               if (GUILayout.Button(new GUIContent("Apply", "Apply changes and regenerate Config Class")))
               {
                  configData.ApplyChanges();
               }

               if (GUILayout.Button(new GUIContent("Revert", "Discard all current changes")))
               {
                  // Revert back to original/saved values, by loading them again from the generated class
                  configData.LoadFromGeneratedClass();
               }
            }
         }
      }

      private static bool DisplayAnySettingChangedDialog()
      {
         return EditorUtility.DisplayDialog(
            "GameplayTag Settings have been modified",
            "Do you want to apply the changes made to the GameplayTag Settings?\n\nYour changes will be lost if you don't save them.",
            "Apply", "Revert");
      }

      private static bool DisplayDeleteTagRenameEntryDialog(TagRenameEntry entry)
      {
         return EditorUtility.DisplayDialog(
            "Delete GameplayTag Rename Entry?",
            $"Do you want to remove the name redirector from '{entry.oldTagName}' to '{entry.newTagName}' GameplayTag?" +
            "\n\nIf any GameplayTags are still serialized with the old TagName, they cant be remapped to the new TagName anymore.",
            "Delete", "Cancel");
      }
   }
}