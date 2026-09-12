using System;
using System.Collections.Generic;
using UnityEngine;

namespace BandoWare.GameplayTags
{
   [Serializable]
   public struct GameplayTagFilter
   {
      /////////////////////////////// Public Fields ////////////////////////////////

      [SerializeField] private bool childTagsOnly;
      [SerializeField] private GameplayTag[] filterTags;

      ///////////////////////////// Public Properties //////////////////////////////

      /// <summary>
      /// Global default value, for whether only child tags of the given filter-tags should included by a <see cref="GameplayTagFilter"/>.  
      /// </summary>
      public const bool ChildTagsOnlyDefault = true;

      public bool ChildTagsOnly => childTagsOnly;
      public bool HasFilterTags => filterTags != null && filterTags.Length > 0;
      public IReadOnlyCollection<GameplayTag> FilterTags => filterTags;

      public static GameplayTagFilter NoFilter = new();

      ///////////////////////////// Public Functions ///////////////////////////////

      public GameplayTagFilter(GameplayTag[] filterTags, bool childTagsOnly = ChildTagsOnlyDefault)
      {
         this.filterTags = filterTags;
         this.childTagsOnly = childTagsOnly;
      }

      //--------------------------------------------------------------------------------------------------------------

      public GameplayTagFilter(Type[] filterTagTypes, bool childTagsOnly = ChildTagsOnlyDefault)
      {
         this.filterTags = GameplayTagManager.RequestTagsByType(filterTagTypes).ToArray();
         this.childTagsOnly = childTagsOnly;
      }

      //--------------------------------------------------------------------------------------------------------------

      public GameplayTagFilter(string[] filterTagNames, bool childTagsOnly = ChildTagsOnlyDefault)
      {
         this.filterTags = GameplayTagManager.RequestTagsByName(filterTagNames).ToArray();
         this.childTagsOnly = childTagsOnly;
      }

      //--------------------------------------------------------------------------------------------------------------

      /// <summary>
      /// Copies the filter tags from source, but parent tag inclusion can be changed.
      /// </summary>
      public GameplayTagFilter(GameplayTagFilter sourceFilter, bool childTagsOnly)
      {
         this.filterTags = sourceFilter.filterTags;
         this.childTagsOnly = childTagsOnly;
      }

      //--------------------------------------------------------------------------------------------------------------

      public bool IncludesTag(GameplayTag tag)
      {
         foreach (GameplayTag filterTag in filterTags)
         {
            if (tag.IsValid() && (!childTagsOnly && tag == filterTag || tag.IsChildOf(filterTag)))
            {
               return true;
            }
         }

         return false;
      }

      //--------------------------------------------------------------------------------------------------------------
   }
}