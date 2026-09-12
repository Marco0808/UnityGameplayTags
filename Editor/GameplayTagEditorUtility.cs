using System.Reflection;
using UnityEditor;

namespace BandoWare.GameplayTags.Editor
{
   public static class GameplayTagEditorUtility
   {
      public static GameplayTagFilter GetTagFilterFromField(FieldInfo fieldInfo, SerializedProperty property)
      {
         if (fieldInfo == null)
         {
            return GameplayTagFilter.NoFilter;
         }

#if MP_INSPECTOR_ATTRIBUTES
         // If our parent property is an Array of GameplayTags (not wrapped in another struct or class)
         // or Requirements struct, retrieve our FilterTags from that parent field directly.
         SerializedProperty parentProperty = MP.InspectorAttributes.Editor.PropertyUtility.FindParentProperty(property);
         if (parentProperty != null
             && (parentProperty.isArray && parentProperty.arrayElementType == nameof(GameplayTag)
                || fieldInfo.DeclaringType == typeof(GameplayTagRequirements)))
         {
            fieldInfo = MP.InspectorAttributes.Editor.PropertyUtility.GetField(parentProperty);
         }
#endif

         return fieldInfo.GetCustomAttribute<GameplayTagFilterAttribute>(false)?.TagFilter ?? GameplayTagFilter.NoFilter;
      }
   }
}