using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;
using BasePopupField = UnityEngine.UIElements.BasePopupField<string, string>;

namespace BandoWare.GameplayTags.Editor
{
   public class GameplayTagField : BaseField<GameplayTag>
   {
      /////////////////////////////// Member Fields ////////////////////////////////

      private readonly VisualElement m_TagEditInput;
      private readonly TextElement m_TagTextElement;
      private readonly GameplayTagFilter m_TagFilter;

      // ReSharper disable InconsistentNaming
      public static new readonly string ussClassName = "gameplay-tag-field";
      public static new readonly string labelUssClassName = ussClassName + "__label";
      public static new readonly string inputUssClassName = ussClassName + "__input";
      // ReSharper restore InconsistentNaming

      ///////////////////////////// Public Functions ///////////////////////////////

      public GameplayTagField()
         : this(null, GameplayTagFilter.NoFilter)
      {
      }

      //--------------------------------------------------------------------------------------------------------------

      public GameplayTagField(string label, GameplayTagFilter tagFilter)
         : base(label, null)
      {
         m_TagFilter = tagFilter;

         VisualElement visualInput = this.Q(className: BaseField<GameplayTag>.inputUssClassName);

         visualInput.focusable = false;
         labelElement.focusable = false;

         AddToClassList(ussClassName);
         AddToClassList(BasePopupField.ussClassName);
         visualInput.AddToClassList(inputUssClassName);
         labelElement.AddToClassList(labelUssClassName);

         m_TagEditInput = new VisualElement();
         m_TagEditInput.AddToClassList(BasePopupField.inputUssClassName);
         m_TagEditInput.RegisterCallback<PointerDownEvent>(_ => ShowPopupWindow());
         visualInput.Add(m_TagEditInput);

         m_TagTextElement = new TextElement();
         m_TagTextElement.AddToClassList(BasePopupField.textUssClassName);
         m_TagTextElement.pickingMode = PickingMode.Ignore;
         m_TagEditInput.Add(m_TagTextElement);

         VisualElement arrowElement = new();
         arrowElement.AddToClassList(BasePopupField.arrowUssClassName);
         arrowElement.pickingMode = PickingMode.Ignore;
         m_TagEditInput.Add(arrowElement);

         UpdateDisplay();
      }

      //--------------------------------------------------------------------------------------------------------------

      public override void SetValueWithoutNotify(GameplayTag newValue)
      {
         bool valueChanged = !value.Equals(newValue);
         base.SetValueWithoutNotify(newValue);
         if (valueChanged)
         {
            UpdateDisplay();
         }
      }

      ///////////////////////////// Private Functions //////////////////////////////

      private void UpdateDisplay() => UpdateMixedValueContent();

      //--------------------------------------------------------------------------------------------------------------

      protected override void UpdateMixedValueContent()
      {
         if (showMixedValue)
         {
            m_TagTextElement.text = mixedValueString;
            m_TagEditInput.tooltip = "Mixed Values";
         }
         else if (value.IsValid())
         {
            m_TagTextElement.text = value.Name;
            m_TagEditInput.tooltip = value.Description;
         }
         else
         {
            m_TagTextElement.text = "None";
            m_TagEditInput.tooltip = null;
         }
      }

      //--------------------------------------------------------------------------------------------------------------

      private void ShowPopupWindow()
      {
         GameplayTagTreeView tagTreeView = new(new TreeViewState<int>(), m_TagFilter, this, static () =>
         {
            EditorWindow.GetWindow<UnityEditor.PopupWindow>().Close();
         });
         tagTreeView.ShowPopupWindow(m_TagEditInput.worldBound);
      }

      //--------------------------------------------------------------------------------------------------------------
   }
}