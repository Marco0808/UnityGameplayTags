using System;
using System.Collections.Generic;
using System.Linq;

namespace BandoWare.GameplayTags
{
   internal class GameplayTagRegistrationContext
   {
      private readonly List<GameplayTagDefinition> m_TagDefinitions = new();
      private readonly Dictionary<string, GameplayTagDefinition> m_TagDefinitionsByName = new(StringComparer.OrdinalIgnoreCase);

      public void RegisterTag(string name, string description = null, GameplayTagFlags flags = GameplayTagFlags.None)
      {
         GameplayTagUtility.ValidateName(name);

         if (m_TagDefinitionsByName.ContainsKey(name))
            return;

         GameplayTagDefinition definition = new(name, description, flags);

         m_TagDefinitionsByName.Add(name, definition);
         m_TagDefinitions.Add(definition);
      }

      public GameplayTagDefinition[] GenerateDefinitions()
      {
         RegisterMissingParents();
         SortDefinitionsAlphabetically();
         RegisterNoneTag();
         SetTagRuntimeIndices();
         FillParentsAndChildren();
         SetHierarchyTags();

         return m_TagDefinitions.ToArray();
      }

      private void RegisterNoneTag()
      {
         m_TagDefinitions.Insert(0, GameplayTagDefinition.CreateNoneTagDefinition());
      }

      private void RegisterMissingParents()
      {
         List<GameplayTagDefinition> definitions = new(m_TagDefinitions);
         foreach (GameplayTagDefinition definition in definitions)
         {
            string[] parentTagNames = GameplayTagUtility.GetHeirarchyNames(definition.TagName);

            GameplayTagFlags flags = definition.Flags;
            foreach (string parentTagName in Enumerable.Reverse(parentTagNames))
            {
               if (m_TagDefinitionsByName.TryGetValue(parentTagName, out GameplayTagDefinition parentTag))
               {
                  flags |= parentTag.Flags;
                  continue;
               }

               RegisterTag(parentTagName, string.Empty, flags);
            }
         }
      }

      private void SortDefinitionsAlphabetically()
      {
         m_TagDefinitions.Sort((a, b) => string.Compare(a.TagName, b.TagName, StringComparison.OrdinalIgnoreCase));
      }

      private void FillParentsAndChildren()
      {
         Dictionary<GameplayTagDefinition, List<GameplayTagDefinition>> childrenLists = new();

         // Skip the first tag definition which is the "None" tag
         for (int i = 1; i < m_TagDefinitions.Count; i++)
         {
            GameplayTagDefinition definition = m_TagDefinitions[i];
            string[] parentTagNames = GameplayTagUtility.GetHeirarchyNames(definition.TagName);
            for (int j = 0; j < parentTagNames.Length - 1; j++)
            {
               string parentTagName = parentTagNames[j];
               GameplayTagDefinition parentDefinition = m_TagDefinitionsByName[parentTagName];
               if (!childrenLists.TryGetValue(parentDefinition, out List<GameplayTagDefinition> children))
               {
                  children = new();
                  childrenLists.Add(parentDefinition, children);
               }

               children.Add(definition);
            }
         }

         foreach ((GameplayTagDefinition definition, List<GameplayTagDefinition> children) in childrenLists)
         {
            definition.SetChildren(children);
            foreach (GameplayTagDefinition child in children)
               child.SetParent(definition);
         }
      }

      private void SetHierarchyTags()
      {
         for (int i = 1; i < m_TagDefinitions.Count; i++)
         {
            GameplayTagDefinition definition = m_TagDefinitions[i];

            List<GameplayTag> hierarcyTags = new();

            if (definition.ParentTagDefinition != null)
               hierarcyTags.AddRange(definition.ParentTagDefinition.HierarchyTags.ToArray());

            hierarcyTags.Add(definition.Tag);
            definition.SetHierarchyTags(hierarcyTags.ToArray());
         }
      }

      private void SetTagRuntimeIndices()
      {
         for (int i = 0; i < m_TagDefinitions.Count; i++)
            m_TagDefinitions[i].SetRuntimeIndex(i);
      }
   }
}