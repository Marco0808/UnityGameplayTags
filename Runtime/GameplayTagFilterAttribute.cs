using System;

namespace BandoWare.GameplayTags
{
   /// <summary>
   /// Can be used on <see cref="GameplayTag"/> or <see cref="GameplayTagContainer"/> fields and properties or on <see cref="TypedGameplayTagBase"/> derived classes.
   /// </summary>
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
   public class GameplayTagFilterAttribute : Attribute
   {
      public GameplayTagFilter TagFilter { get; }

      public GameplayTagFilterAttribute(params Type[] filterTagTypes)
      {
         TagFilter = new GameplayTagFilter(filterTagTypes);
      }

      public GameplayTagFilterAttribute(bool childTagsOnly, params Type[] filterTagTypes)
      {
         TagFilter = new GameplayTagFilter(filterTagTypes, childTagsOnly);
      }

      public GameplayTagFilterAttribute(params string[] filterTagNames)
      {
         TagFilter = new GameplayTagFilter(filterTagNames);
      }

      public GameplayTagFilterAttribute(bool childTagsOnly, params string[] filterTagNames)
      {
         TagFilter = new GameplayTagFilter(filterTagNames, childTagsOnly);
      }
   }
}