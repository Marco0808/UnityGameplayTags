using System;
using UnityEditor;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor.Config
{
   internal enum TagDefinitionOperation
   {
      CreateNew,
      EditExisting
   }

   internal class GameplayTagDefinitionEditWindow : EditorWindow
   {
      /////////////////////////////// Member Fields ////////////////////////////////

      private TagDefinitionEntry m_OriginalEntry;
      private string m_EditedTagName;
      private string m_EditedDescription;

      ///////////////////////////// Public Properties //////////////////////////////

      public TagDefinitionOperation Operation { get; private set; }
      public GameplayTagConfigData ConfigData { get; private set; }
      public Action CloseAction { get; private set; }

      public TagDefinitionEntry OriginalEntry
      {
         get => m_OriginalEntry;
         private set
         {
            m_OriginalEntry = value;
            m_EditedTagName = value.tagName;
            m_EditedDescription = value.description;
         }
      }

      ///////////////////////////// Public Functions ///////////////////////////////

      public static void ShowWindow(TagDefinitionOperation operation, GameplayTagConfigData configData, TagDefinitionEntry originalEntry, Action closeCallback = null)
      {
         string title = operation switch
         {
            TagDefinitionOperation.EditExisting => "Edit Gameplay Tag Definition",
            TagDefinitionOperation.CreateNew => "Create Gameplay Tag Definition",
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
         };

         GameplayTagDefinitionEditWindow win = GetWindow<GameplayTagDefinitionEditWindow>(true, title, true);
         win.minSize = new Vector2(500, 130);
         win.maxSize = new Vector2(900, 130);
         win.Operation = operation;
         win.ConfigData = configData;
         win.OriginalEntry = originalEntry;
         win.CloseAction = closeCallback;
         win.ShowModalUtility();
      }

      ///////////////////////////// Private Functions //////////////////////////////

      private void OnGUI()
      {
         GUIStyle outerMarginStyle = new()
         {
            margin = new RectOffset(10, 10, 10, 5)
         };

         using (new GUILayout.VerticalScope(outerMarginStyle))
         {
            GUILayoutOption labelWidthOption = GUILayout.Width(130f);

            if (Operation == TagDefinitionOperation.EditExisting)
            {
               using (new GUILayout.HorizontalScope())
               {
                  GUILayout.Label("Current Tag Name:", EditorStyles.boldLabel, labelWidthOption);
                  GUILayout.Label(OriginalEntry.tagName);
               }
            }

            bool canApplyChanges;
            bool isTagNameValid;

            // TagName field with validation
            using (new GUILayout.HorizontalScope())
            {
               GUILayout.Label("New Tag Name:", EditorStyles.boldLabel, labelWidthOption);
               m_EditedTagName = EditorGUILayout.TextField(m_EditedTagName);

               canApplyChanges = isTagNameValid = IsTagNameValid(m_EditedTagName, out string reason);

               // Cant create new if a TagDefinition with the same name already exists (description is irrelevant)
               if (Operation == TagDefinitionOperation.CreateNew
                   && ConfigData.TagDefinitionEntries.Contains(new TagDefinitionEntry(m_EditedTagName, null)))
               {
                  canApplyChanges = false;
                  reason = "Tag already exists";
               }

               GUI.contentColor = Color.red;
               GUILayout.Label(reason, EditorStyles.whiteLabel, GUILayout.Width(110f));
               GUI.contentColor = Color.white;
            }

            GUILayout.Space(10f);

            // Description field
            using (new GUILayout.HorizontalScope())
            {
               GUILayout.Label("Description:", EditorStyles.boldLabel, labelWidthOption);
               m_EditedDescription = EditorGUILayout.TextField(m_EditedDescription);
            }

            GUILayout.FlexibleSpace();

            // Footer buttons
            using (new GUILayout.HorizontalScope())
            {
               if (Operation == TagDefinitionOperation.EditExisting)
               {
                  if (GUILayout.Button("Delete Tag", GUILayout.ExpandWidth(false)))
                  {
                     // Delete this TagDefinition from the config
                     if (ConfigData.TagDefinitionEntries.Remove(OriginalEntry))
                     {
                        ConfigData.SetDirty();
                     }
                     Close();
                  }
               }

               GUILayout.FlexibleSpace();
               using (new EditorGUI.DisabledScope(!canApplyChanges))
               {
                  // Apply by Button or Return key pressed
                  if (GUILayout.Button("Apply", GUILayout.ExpandWidth(false))
                      || canApplyChanges && Event.current.keyCode == KeyCode.Return)
                  {
                     bool tagNameChanged = m_EditedTagName != OriginalEntry.tagName || Operation == TagDefinitionOperation.CreateNew;
                     bool descriptionChanged = m_EditedDescription != OriginalEntry.description;

                     // If there were any changes, save them to the config
                     if (isTagNameValid && (tagNameChanged || descriptionChanged))
                     {
                        switch (Operation)
                        {
                           case TagDefinitionOperation.CreateNew:
                              ConfigData.AddGameplayTagDefinition(new TagDefinitionEntry(m_EditedTagName, m_EditedDescription));
                              break;
                           case TagDefinitionOperation.EditExisting:
                              if (tagNameChanged)
                              {
                                 if (EditorUtility.DisplayDialog(
                                        "Rename GameplayTag",
                                        $"Which GameplayTags should be affected by this rename?\n\n'{OriginalEntry.tagName}' -> '{m_EditedTagName}'",
                                        "Rename this only", "Rename this and all related"))
                                 {
                                    ConfigData.RenameThisGameplayTag(OriginalEntry.tagName, m_EditedTagName);
                                 }
                                 else
                                 {
                                    ConfigData.RenameThisAndParentGameplayTags(OriginalEntry.tagName, m_EditedTagName);
                                 }
                              }
                              break;
                           default:
                              throw new ArgumentOutOfRangeException();
                        }
                     }
                     Close();
                  }
               }

               if (GUILayout.Button("Cancel", GUILayout.ExpandWidth(false)))
               {
                  // Close without saving
                  Close();
               }
            }
         }
      }

      private void OnLostFocus()
      {
         Close();
      }

      private void OnDestroy()
      {
         CloseAction?.Invoke();
      }

      private bool IsTagNameValid(string tagName, out string reason)
      {
         static bool IsValidLabelCharacter(char c)
         {
            return char.IsLetterOrDigit(c) || c == '_';
         }

         static bool AcceptLabel(string tagName, ref int position)
         {
            if (position >= tagName.Length || !IsValidLabelCharacter(tagName[position]))
               return false;

            position++;
            while (position < tagName.Length && IsValidLabelCharacter(tagName[position]))
            {
               position++;
            }

            return true;
         }

         if (string.IsNullOrEmpty(tagName))
         {
            reason = "Cannot be empty";
            return false;
         }

         int position = 0;
         if (AcceptLabel(tagName, ref position))
         {
            while (position < tagName.Length && tagName[position] == '.')
            {
               position++;
               if (!AcceptLabel(tagName, ref position))
               {
                  reason = "Invalid Name";
                  return false;
               }
            }
         }

         if (position == tagName.Length)
         {
            reason = null;
            return true;
         }

         reason = "Invalid Name";
         return false;
      }
   }
}