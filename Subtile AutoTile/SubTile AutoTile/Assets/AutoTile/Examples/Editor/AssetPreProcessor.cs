using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetPreProcessor : AssetPostprocessor
{

    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;


        if (File.Exists(AssetDatabase.GetTextMetaFilePathFromAssetPath(textureImporter.assetPath.ToLower())))
            return;


        TextureImporterSettings texSettings = new TextureImporterSettings();
        textureImporter.ReadTextureSettings(texSettings);
        texSettings.spriteAlignment = (int)SpriteAlignment.Center;
        textureImporter.SetTextureSettings(texSettings);

        textureImporter.filterMode = FilterMode.Point;
        textureImporter.spritePixelsPerUnit = 32;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
    }


}
