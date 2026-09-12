using UnityEngine;

namespace BandoWare.GameplayTags
{
   /// <summary>
   /// <para>Wrapper class which makes it possible to define and reference a single GameplayTag as a unity-object (ScriptableObject asset).</para>
   /// <para>Note: If possible, always reference GameplayTags by value directly, but the Unity Behavior Graph for example, can only reference custom object-types.</para>
   /// </summary>
   public class GameplayTagAsset : ScriptableObject
   {
      [SerializeField]
      public GameplayTag tag;

      public static implicit operator GameplayTag(GameplayTagAsset tagAsset) => tagAsset != null ? tagAsset.tag : GameplayTag.None;
   }
}