using System;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace BandoWare.GameplayTags
{
   [Serializable]
   [DebuggerDisplay("{m_Name,nq}")]
   public struct GameplayTag : IEquatable<GameplayTag>, ISerializationCallbackReceiver
   {
#if UNITY_EDITOR
      /// <summary>
      /// Makes it possible to use <see cref="GameplayTag"/> type with UI Builder.
      /// </summary>
      internal class GameplayTagConverter : UxmlAttributeConverter<GameplayTag>
      {
         public override GameplayTag FromString(string value) => DeserializeFromString(value);
         public override string ToString(GameplayTag value) => SerializeToString(value);
      }
#endif

      private const int k_RuntimeIndexUninitialized = -1;
      private const int k_RuntimeIndexNone = 0;

      /// <summary>
      /// Represents an invalid tag.
      /// </summary>
      public static readonly GameplayTag None = new() {m_RuntimeIndex = k_RuntimeIndexNone};

      internal int RuntimeIndex
      {
         get
         {
            InitializeIfNeeded();
            return m_RuntimeIndex;
         }
      }

      internal GameplayTagDefinition Definition
      {
         get
         {
            ValidateIsNotNone();
            return GameplayTagManager.GetDefinitionFromRuntimeIndex(RuntimeIndex);
         }
      }

      /// <inheritdoc cref="GameplayTagDefinition.ParentTags" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ReadOnlySpan<GameplayTag> ParentTags => Definition.ParentTags;

      /// <inheritdoc cref="GameplayTagDefinition.ChildTags" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ReadOnlySpan<GameplayTag> ChildTags => Definition.ChildTags;

      /// <inheritdoc cref="GameplayTagDefinition.HierarchyTags" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public ReadOnlySpan<GameplayTag> HierarchyTags => Definition.HierarchyTags;

      /// <inheritdoc cref="GameplayTagDefinition.Label" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public string Label => Definition.Label;

      /// <inheritdoc cref="GameplayTagDefinition.HierarchyLevel" />
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      public int HierarchyLevel => Definition.HierarchyLevel;

      /// <inheritdoc cref="GameplayTagDefinition.Description" />
      public string Description => Definition.Description;

      /// <summary>
      /// The parent tag of this tag. If this tag is "A.B.C", the parent tag will be "A.B".
      /// </summary>
      public GameplayTag ParentTag
      {
         get
         {
            GameplayTagDefinition parentDefinition = Definition.ParentTagDefinition;

            if (parentDefinition == null)
               return None;

            return parentDefinition.Tag;
         }
      }

      /// <inheritdoc cref="GameplayTagDefinition.Flags" />
      public GameplayTagFlags Flags => Definition.Flags;

      public string Name
      {
         get
         {
            ValidateIsNotNone();
            return m_Name;
         }
      }

      [SerializeField]
      private string m_Name;

      private int m_RuntimeIndex;

      internal GameplayTag(string name, int runtimeTagIndex)
      {
         m_Name = name;
         m_RuntimeIndex = runtimeTagIndex;
      }

      public bool IsValid()
      {
         return RuntimeIndex != 0;
      }

      /// <summary>
      /// Tags match if they are equal or this tag is a child of the given tag.<br/>
      /// "A.B".MatchesTag("A") = true, "A".MatchesTag("A.B") = false.
      /// </summary>
      public bool MatchesTag(in GameplayTag tag)
      {
         ValidateIsNotNone();
         return RuntimeIndex == tag.RuntimeIndex || IsChildOf(tag);
      }

      /// <inheritdoc cref="GameplayTagDefinition.IsParentOf(GameplayTag)"/>/>
      public bool IsParentOf(in GameplayTag tag)
      {
         ValidateIsNotNone();
         return Definition.IsParentOf(tag);
      }


      /// <inheritdoc cref="GameplayTagDefinition.IsChildOf(GameplayTag)"/>/>
      public bool IsChildOf(in GameplayTag parentTag)
      {
         ValidateIsNotNone();
         return Definition.IsChildOf(parentTag);
      }

      public bool Equals(GameplayTag other)
      {
         return RuntimeIndex == other.RuntimeIndex;
      }

      public override bool Equals(object obj)
      {
         if (obj is GameplayTag other)
            return other.RuntimeIndex == RuntimeIndex;

         if (obj is string otherStr)
            return m_Name == otherStr;

         return false;
      }

      public override int GetHashCode()
      {
         return RuntimeIndex;
      }

      public override string ToString()
      {
         InitializeIfNeeded();
         return m_Name ?? "<None>";
      }

      void ISerializationCallbackReceiver.OnBeforeSerialize()
      {
         // This is used to apply any renaming when serializing the tag
         m_Name = SerializeToString(this);
      }

      void ISerializationCallbackReceiver.OnAfterDeserialize()
      {
         m_RuntimeIndex = k_RuntimeIndexUninitialized;
         // Do not call "InitializeIfNeeded()" here, as it caused crashes when a SerializedProperty was being read
      }

      internal static string SerializeToString(GameplayTag tag)
      {
         if (tag.RuntimeIndex == 0)
         {
            return null;
         }

         GameplayTagDefinition definition = GameplayTagManager.GetDefinitionFromRuntimeIndex(tag.RuntimeIndex);
         return definition?.TagName;
      }

      internal static GameplayTag DeserializeFromString(string tagName)
      {
         return GameplayTagManager.RequestTag(tagName);
      }

      private void InitializeIfNeeded()
      {
         // If the RuntimeIndex is uninitialized, try to find its tag by name and set it.
         // (should only happen when a tag was deserialized from disk)
         if (m_RuntimeIndex == k_RuntimeIndexUninitialized)
         {
            GameplayTag tag = GameplayTagManager.RequestTag(m_Name);
            m_RuntimeIndex = tag.m_RuntimeIndex;
            m_Name = tag.m_Name;
         }
      }

      [Conditional("DEBUG")]
      private void ValidateIsNotNone()
      {
         if (RuntimeIndex == k_RuntimeIndexNone)
            throw new InvalidOperationException("Cannot perform operation on GameplayTag.None.");
      }

      public static implicit operator GameplayTag(string tagName)
      {
         return GameplayTagManager.RequestTag(tagName);
      }

      public static bool operator ==(in GameplayTag lhs, in GameplayTag rhs)
      {
         return lhs.RuntimeIndex == rhs.RuntimeIndex;
      }

      public static bool operator !=(in GameplayTag lhs, in GameplayTag rhs)
      {
         return lhs.RuntimeIndex != rhs.RuntimeIndex;
      }
   }
}