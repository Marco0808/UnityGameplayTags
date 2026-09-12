using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor
{
   public static class TreeViewMethodExtensions
   {
      public static void ShowPopupWindow(this TreeViewPopupContent.TreeView treeView, Rect activatorRect, float width = 320f, float maxHeight = 285f)
      {
         TreeViewPopupContent treeViewPopupContent = new(width, maxHeight, treeView);
         PopupWindow.Show(activatorRect, treeViewPopupContent);
      }
   }

   public class TreeViewPopupContent : PopupWindowContent
   {
      public abstract class TreeView : TreeView<int>
      {
         public TreeView(TreeViewState<int> state) : base(state)
         {
         }

         public virtual float GetTotalHeight()
         {
            return totalHeight;
         }
      }

      private TreeView m_TreeView;
      private float m_Width;
      private float m_MaxHeight;

      public TreeViewPopupContent(float width, float maxHeight, TreeView tagTreeView)
      {
         m_Width = width;
         m_MaxHeight = maxHeight;
         m_TreeView = tagTreeView;
      }

      public override void OnGUI(Rect rect)
      {
         m_TreeView.OnGUI(rect);
      }

      public override Vector2 GetWindowSize()
      {
         return new Vector2(m_Width, m_MaxHeight);
      }
   }
}

