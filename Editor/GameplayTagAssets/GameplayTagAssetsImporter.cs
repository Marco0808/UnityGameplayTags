using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace BandoWare.GameplayTags.Editor.GameplayTagAssets
{
   [ScriptedImporter(1, k_AssetExtension)]
   internal class GameplayTagAssetsImporter : ScriptedImporter
   {
      private const string k_AssetExtension = "gameplaytags";

      internal static void CreateGameplayTagsAssetFile()
      {
         string filePath = EditorUtility.SaveFilePanelInProject("Create Gameplay Tags Asset", "GameplayTags", k_AssetExtension, "");
         if (!string.IsNullOrEmpty(filePath))
         {
            System.IO.File.WriteAllText(filePath, "");
         }
      }

      [InitializeOnLoadMethod]
      private static void ReimportAllGameplayTagAssets()
      {
         // Don't reimport when the AssetDatabase is read-only (is the case for multiplayer-play-mode virtual editor instances)
         if (!AssetDatabase.IsDirectoryMonitoringEnabled())
         {
            return;
         }

         // TODO: Store all tags in one or more .gameplaytag text files, and generate code from there. This will then also auto-import these files and generate assets 
         GUID[] guids = AssetDatabase.FindAssetGUIDs($"t:{nameof(GameplayTagRootAsset)}");
         foreach (GUID guid in guids)
         {
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
         }
      }

      public override void OnImportAsset(AssetImportContext ctx)
      {
         GameplayTagRootAsset rootAsset = ScriptableObject.CreateInstance<GameplayTagRootAsset>();
         ctx.AddObjectToAsset("RootAsset", rootAsset);
         ctx.SetMainObject(rootAsset);

         foreach (GameplayTag tag in GameplayTagManager.GetAllTags())
         {
            if (!GameplayTagManager.IsTagForTestingOnly(tag.Name))
            {
               ctx.AddObjectToAsset(tag.Name, CreateGameplayTagAsset(tag));
            }
         }

         foreach (KeyValuePair<string, GameplayTagDefinition> pair in GameplayTagManager.GetRenamedTagDefinitionsByOldName())
         {
            ctx.AddObjectToAsset(pair.Key, CreateGameplayTagAsset(pair.Value.Tag, "[Outdated] "));
         }
      }

      private static GameplayTagAsset CreateGameplayTagAsset(GameplayTag tag, string assetNamePrefix = null)
      {
         GameplayTagAsset asset = ScriptableObject.CreateInstance<GameplayTagAsset>();
         asset.tag = tag;
         asset.name = assetNamePrefix + tag.Name;
         asset.hideFlags = HideFlags.NotEditable;
         return asset;
      }
   }
}