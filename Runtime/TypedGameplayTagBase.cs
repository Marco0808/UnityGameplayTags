using System;
using System.Reflection;
using UnityEngine;

namespace BandoWare.GameplayTags
{
   /// <summary>
   /// Used to create wrapper types for GameplayTags, which allow pre-defined tag filter options, type-safety and GraphToolkit node support.
   /// </summary>
   [Serializable]
   public abstract class TypedGameplayTagBase
   {
      /////////////////////////////// Public Fields ////////////////////////////////

      [SerializeField] private GameplayTag tag;

      ///////////////////////////// Public Properties //////////////////////////////

      public GameplayTag Tag
      {
         get => tag;
         set
         {
#if DEBUG
            if (value.IsValid() && !GetTagFilter().IncludesTag(value))
            {
               throw new InvalidOperationException($"GameplayTag '{value}' does not match tags allowed by '{GetType().Name}' type filter.");
            }
#endif

            tag = value;
         }
      }

      ///////////////////////////// Public Functions ///////////////////////////////

      protected TypedGameplayTagBase(GameplayTag tag)
      {
         Tag = tag;
      }

      //--------------------------------------------------------------------------------------------------------------

      public static implicit operator GameplayTag(TypedGameplayTagBase typedTag) => typedTag.Tag;

      //--------------------------------------------------------------------------------------------------------------

      public GameplayTagFilter GetTagFilter()
      {
         return GetType().GetCustomAttribute<GameplayTagFilterAttribute>(true)?.TagFilter ?? GameplayTagFilter.NoFilter;
      }

      //--------------------------------------------------------------------------------------------------------------
   }
}