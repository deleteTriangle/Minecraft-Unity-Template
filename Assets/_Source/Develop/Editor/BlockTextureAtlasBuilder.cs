using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BlockTextureAtlasBuilder
{
    private const int AtlasSize = 256;
    private const int TileSize = 16;
    private const string OutputPath = "Assets/_Source/Art/Textures/BlockTextureAtlas.png";
    private const string WorldMaterialPath = "Assets/_Source/Art/Materials/World.mat";

    private static readonly AtlasTile[] Tiles =
    {
        new("grass_block_side", 1),
        new("dirt", 2),
        new("grass_block_top", 3, new Color32(176, 215, 77, 255)),
        new("furnace_top", 4),
        new("furnace_front", 5),
        new("furnace_side", 6),
        new("water_still", 7),
        new("oak_trapdoor", 8),
        new("oak_door_bottom", 9),
        new("oak_door_top", 10)
    };

    [MenuItem("Tools/Blocks/Rebuild Texture Atlas")]
    public static void Rebuild()
    {
        Texture2D atlas = new Texture2D(AtlasSize, AtlasSize, TextureFormat.RGBA32, true);
        atlas.SetPixels(new Color[AtlasSize * AtlasSize]);

        foreach (AtlasTile tile in Tiles)
        {
            Texture2D sourceTexture = LoadSourceTexture(tile.textureName);
            int x = tile.index % (AtlasSize / TileSize) * TileSize;
            int y = ((AtlasSize / TileSize) - 1 - tile.index / (AtlasSize / TileSize)) * TileSize;
            Color[] pixels = sourceTexture.GetPixels(0, sourceTexture.height - TileSize, TileSize, TileSize);
            if (tile.isTinted)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] *= tile.tint;
                }
            }

            atlas.SetPixels(x, y, TileSize, TileSize, pixels);
            UnityEngine.Object.DestroyImmediate(sourceTexture);
        }

        atlas.Apply(true, false);
        string absoluteOutputPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, OutputPath);
        File.WriteAllBytes(absoluteOutputPath, atlas.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(atlas);

        AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(OutputPath) as TextureImporter;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();

        Material worldMaterial = AssetDatabase.LoadAssetAtPath<Material>(WorldMaterialPath);
        worldMaterial.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
        EditorUtility.SetDirty(worldMaterial);

        UpdateBlockConfigs();
        AssetDatabase.SaveAssets();
    }

    private static Texture2D LoadSourceTexture(string textureName)
    {
        string sourcePath = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName,
            "Assets/Bare Bones 1.21.11/assets/minecraft/textures/block",
            textureName + ".png");

        Texture2D texture = new Texture2D(TileSize, TileSize, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(File.ReadAllBytes(sourcePath)) || texture.width < TileSize || texture.height < TileSize)
        {
            throw new InvalidOperationException($"Invalid block texture: {sourcePath}");
        }

        return texture;
    }

    private static void UpdateBlockConfigs()
    {
        foreach (BlockConfig blockConfig in Resources.LoadAll<BlockConfig>("Blocks"))
        {
            switch (blockConfig.type)
            {
                case BlockType.Grass:
                    SetTextureLayout(blockConfig, 3, 0, -1, -2, -2, -2, -2);
                    break;
                case BlockType.Furnace:
                    SetTextureLayout(blockConfig, 4, 0, 0, 1, 2, 2, 2);
                    break;
                case BlockType.Water:
                    SetTextureLayout(blockConfig, 7, 0, 0, 0, 0, 0, 0);
                    break;
                case BlockType.Trapdoor:
                    SetTextureLayout(blockConfig, 8, 0, 0, 0, 0, 0, 0);
                    break;
                case BlockType.Door:
                    SetTextureLayout(blockConfig, 9, 0, 0, 0, 0, 0, 0);
                    break;
                default:
                    continue;
            }

            EditorUtility.SetDirty(blockConfig);
        }
    }

    private static void SetTextureLayout(BlockConfig blockConfig, int baseId, params int[] faceOffsets)
    {
        blockConfig.baseId = baseId;
        blockConfig.isComplex = true;
        blockConfig.faceOffsets = faceOffsets;
    }

    private readonly struct AtlasTile
    {
        public readonly string textureName;
        public readonly int index;
        public readonly Color tint;
        public readonly bool isTinted;

        public AtlasTile(string textureName, int index)
        {
            this.textureName = textureName;
            this.index = index;
            tint = Color.white;
            isTinted = false;
        }

        public AtlasTile(string textureName, int index, Color tint)
        {
            this.textureName = textureName;
            this.index = index;
            this.tint = tint;
            isTinted = true;
        }
    }
}
