using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor
{
   public class GameplayTagTreeView : GameplayTagTreeViewBase
   {
      private static GUIContent s_TempContent = new();
      private Action m_OnSelectionChange;
      private GameplayTagField m_TagField;

      public GameplayTagTreeView(TreeViewState<int> treeViewState, GameplayTagFilter tagFilter, GameplayTagField tagField, Action onSelectionChange)
         : base(treeViewState, tagFilter)
      {
         m_OnSelectionChange = onSelectionChange;
         m_TagField = tagField;

         GameplayTag tag = m_TagField.value;
         if (tag != GameplayTag.None)
         {
            GameplayTagTreeViewItem item = FindItem(tag.RuntimeIndex);
            if (item != null)
               SetSelection(new int[] {item.id});

            while (item != null)
            {
               SetExpanded(item.id, true);
               item = item.parent as GameplayTagTreeViewItem;
            }
         }
      }

      protected override void OnToolbarGUI()
      {
         if (ToolbarButton("Clear"))
         {
            m_TagField.value = GameplayTag.None;
         }
      }

      protected override bool CanMultiSelect(TreeViewItem<int> item)
      {
         return false;
      }

      protected override void RowGUI(RowGUIArgs args)
      {
         bool isNone = args.item is not GameplayTagTreeViewItem;
         float indent = GetContentIndent(args.item);
         Rect rect = args.rowRect;
         rect.xMin += indent - (hasSearch ? 14 : 0);

         if (IsItemOrAnyChildSelected(args.item))
         {
            EditorGUI.DrawRect(args.rowRect, new Color32(44, 93, 135, 255));
         }

         if (isNone)
         {
            if (GUI.Button(rect, args.label, EditorStyles.label))
            {
               m_TagField.value = GameplayTag.None;
               m_OnSelectionChange?.Invoke();
            }

            return;
         }

         GameplayTagTreeViewItem item = args.item as GameplayTagTreeViewItem;

         // Disable the row button if this is just a parent tag which itself is filtered out
         EditorGUI.BeginDisabledGroup(IsDisabledFilterTag(item.Tag));

         s_TempContent.text = hasSearch ? item.DisplayName : args.label;
         s_TempContent.tooltip = item.Tag.Description;
         if (GUI.Button(rect, s_TempContent, EditorStyles.label))
         {
            m_TagField.value = item.Tag;
            m_OnSelectionChange?.Invoke();
         }

         EditorGUI.EndDisabledGroup();
      }

      private bool IsItemOrAnyChildSelected(TreeViewItem<int> item)
      {
         if (item != null)
         {
            if (IsSelected(item.id))
               return true;

            if (item.children != null)
            {
               foreach (TreeViewItem<int> child in item.children)
               {
                  if (IsItemOrAnyChildSelected(child))
                  {
                     return true;
                  }
               }
            }
         }

         return false;
      }
   }
}