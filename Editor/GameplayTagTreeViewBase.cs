using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor
{
   public class GameplayTagTreeViewItem : TreeViewItem<int>
   {
      public GameplayTag Tag => m_Tag;

      public string DisplayName => Tag.Label;

      public bool IsIncluded { get; set; }

      public bool IsExplicitIncluded { get; set; }

      private GameplayTag m_Tag;

      public GameplayTagTreeViewItem(int id, GameplayTag tag)
         : base(id, tag.HierarchyLevel, tag.Label)
      {
         m_Tag = tag;
      }
   }

   public abstract class GameplayTagTreeViewBase : TreeViewPopupContent.TreeView
   {
      public bool IsEmpty => m_IsEmpty;

      private static Styles s_Styles;
      private GameplayTagFilter m_TagFilter;
      private SearchField m_SearchField;
      private bool m_IsEmpty;

      public GameplayTagTreeViewBase(TreeViewState<int> treeViewState, GameplayTagFilter tagFilter)
         : base(treeViewState)
      {
         m_TagFilter = tagFilter;
         m_SearchField = new SearchField();
         showAlternatingRowBackgrounds = true;

         Reload();
      }

      public override float GetTotalHeight()
      {
         return base.GetTotalHeight() + EditorStyles.toolbar.fixedHeight * 2f;
      }

      public override void OnGUI(Rect rect)
      {
         s_Styles ??= new Styles();

         Rect toolbarRect = rect;
         toolbarRect.height = EditorStyles.toolbar.fixedHeight * 2f;
         ToolbarGUI(toolbarRect);

         rect.yMin += toolbarRect.height;
         base.OnGUI(rect);
      }

      private void ToolbarGUI(Rect rect)
      {
         GUILayout.BeginArea(rect);
         GUILayout.BeginVertical(EditorStyles.toolbar);

         if (GUILayout.Button("Manage Tags", s_Styles.ToolbarButton, GUILayout.ExpandWidth(true)))
         {
            SettingsService.OpenProjectSettings(Config.GameplayTagProjectSettingsProvider.SettingsPath);
         }

         GUILayout.BeginHorizontal(EditorStyles.toolbar);

         if (ToolbarButton("Expand All"))
            ExpandAll();

         if (ToolbarButton("Collapse All"))
            CollapseAll();

         OnToolbarGUI();

         searchString = m_SearchField.OnToolbarGUI(searchString);

         GUILayout.EndHorizontal();
         GUILayout.EndVertical();
         GUILayout.EndArea();
      }

      protected virtual void OnToolbarGUI()
      { }

      protected bool ToolbarButton(string text)
      {
         return GUILayout.Button(text, s_Styles.ToolbarButton, GUILayout.ExpandWidth(false));
      }

      protected override bool DoesItemMatchSearch(TreeViewItem<int> item, string search)
      {
         GameplayTagTreeViewItem tagItem = item as GameplayTagTreeViewItem;
         return tagItem?.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
      }

      protected override TreeViewItem<int> BuildRoot()
      {
         TreeViewItem<int> root = new(-2, -1, "<Root>");
         m_IsEmpty = true;

         List<TreeViewItem<int>> items = new();

         // Create a new tag filter which forces parent tags to be included,
         // so they can be displayed in the tree but are not selectable (see "IsDisabledFilterTag()").  
         GameplayTagFilter parentIncludingTagFilter = new(m_TagFilter, false);

         foreach (GameplayTag tag in GameplayTagManager.GetAllTagsFiltered(parentIncludingTagFilter))
         {
            if (GameplayTagManager.IsTagForTestingOnly(tag.Name))
               continue;

            items.Add(new GameplayTagTreeViewItem(tag.RuntimeIndex, tag));
            m_IsEmpty = false;
         }

         SetupParentsAndChildrenFromDepths(root, items);

         // Filter tags should start expanded
         if (m_TagFilter.HasFilterTags)
         {
            foreach (GameplayTag filterTag in m_TagFilter.FilterTags)
            {
               SetExpanded(filterTag.RuntimeIndex, true);
            }
         }

         return root;
      }

      protected GameplayTagTreeViewItem FindItem(int runtimeTagIndex)
      {
         return FindItem(runtimeTagIndex, rootItem) as GameplayTagTreeViewItem;
      }

      /// <summary>
      /// Whether a tag should be disabled (not selectable), because it is just displayed parent tag which itself is filtered out.
      /// </summary>
      protected bool IsDisabledFilterTag(GameplayTag tag)
      {
         return m_TagFilter.HasFilterTags
            && m_TagFilter.ChildTagsOnly
            && m_TagFilter.FilterTags.Contains(tag);
      }

      protected class Styles
      {
         public readonly GUIStyle SearchField;
         public readonly GUIStyle ToolbarButton;

         public Styles()
         {
            SearchField = new GUIStyle("SearchTextField");

            ToolbarButton = new GUIStyle(EditorStyles.toolbarButton);
            ToolbarButton.fontSize = 11;
         }
      }
   }
}