#if UNITY_EDITOR
using EditorTools;
using UnityEditor;
using UnityEngine;
using WFC;

namespace EditorTools
{
    public static class MinimapSpriteBootstrap
    {
        private const string OutputFolder = "Assets/UI/Minimap/Sprites";

        [MenuItem("DungeonCrawler/Generate Minimap Sprites")]
        public static void GenerateAndAssignSprites()
        {
            EnsureOutputFolder();

            string[] guids = AssetDatabase.FindAssets("t:WFCData", new[] { "Assets/Scripts/WFC/Data" });
            int assigned = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<WFCData>(path);
                if (data == null || data.tileType == TileType.Empty)
                    continue;

                string texturePath = $"{OutputFolder}/{data.name}.png";
                var texture = MinimapSpriteUtility.GenerateTexture(data, MinimapSpriteUtility.DefaultCellPixelSize);
                SaveTexture(texturePath, texture);

                var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = MinimapSpriteUtility.DefaultCellPixelSize;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }

                var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath);
                Sprite sprite = null;
                foreach (var asset in sprites)
                {
                    if (asset is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }

                if (sprite == null)
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                data.minimapSprite = sprite;
                EditorUtility.SetDirty(data);
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[MinimapSpriteBootstrap] Assigned minimap sprites to {assigned} WFCData assets.");
        }

        private static void EnsureOutputFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/UI"))
                AssetDatabase.CreateFolder("Assets", "UI");
            if (!AssetDatabase.IsValidFolder("Assets/UI/Minimap"))
                AssetDatabase.CreateFolder("Assets/UI", "Minimap");
            if (!AssetDatabase.IsValidFolder(OutputFolder))
                AssetDatabase.CreateFolder("Assets/UI/Minimap", "Sprites");
        }

        private static void SaveTexture(string path, Texture2D texture)
        {
            byte[] png = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, png);
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
        }
    }
}
#endif
