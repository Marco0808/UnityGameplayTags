using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BandoWare.GameplayTags.Editor
{
   [CustomPropertyDrawer(typeof(GameplayTag))]
   [CustomPropertyDrawer(typeof(TypedGameplayTagBase), true)]
   public class GameplayTagPropertyDrawer : PropertyDrawer
   {
      public override VisualElement CreatePropertyGUI(SerializedProperty property)
      {
         SerializedProperty nameProperty;
         GameplayTagFilter tagFilter;

         // Wrapper TypedGameplayTag types specify tag filters directly via the type itself instead of via attribute
         if (property.boxedValue is TypedGameplayTagBase typedGameplayTag)
         {
            nameProperty = property.FindPropertyRelative("tag.m_Name");
            tagFilter = typedGameplayTag.GetTagFilter();
         }
         else
         {
            nameProperty = property.FindPropertyRelative("m_Name");
            tagFilter = GameplayTagEditorUtility.GetTagFilterFromField(fieldInfo, property);
         }

         GameplayTagField field = new(preferredLabel, tagFilter)
         {
            value = GameplayTagManager.RequestTag(nameProperty.stringValue)
         };

#if MP_INSPECTOR_ATTRIBUTES
         if (MP.InspectorAttributes.Editor.GraphToolkitGUIUtils.IsGraphProperty(property))
         {
            MP.InspectorAttributes.Editor.GraphToolkitGUIUtils.AddGraphFieldUssClasses(field);
         }
         else
#endif
         {
            field.AddToClassList(GameplayTagField.alignedFieldUssClassName);
         }

         // Update serialized property, if the field changes
         field.RegisterValueChangedCallback(evt =>
         {
            nameProperty.stringValue = evt.newValue.IsValid() ? evt.newValue.Name : null;
            nameProperty.serializedObject.ApplyModifiedProperties();
         });

         // Update field if the serialized property changes
         field.TrackPropertyValue(nameProperty, changedNameProperty =>
         {
            field.value = GameplayTagManager.RequestTag(changedNameProperty.stringValue);
         });

         return field;
      }
   }
}