using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;


public static class ImportAutotile
{

    static public void ImportSubtile(Object _image, AutoTileLookup _lookup)
    {
        Texture2D texture = _image as Texture2D;
        ProcessSubtileTexture(texture, _lookup);
    }



    public static RuleTile.TilingRule GetRule(int neighbourMask)
    {

        RuleTile.TilingRule m_rule = new RuleTile.TilingRule();
        m_rule.m_Neighbors = new int[8];

        int mask = 128;
        for (int i = 7; i >= 0; i--)
        {
            if (neighbourMask >= mask)
            {
                m_rule.m_Neighbors[i] = 1;
                neighbourMask -= mask;
            }
            else
            {
                m_rule.m_Neighbors[i] = 0;
            }
            mask /= 2;
        }

        //discard corner tiles if their cardinal neighbours are not needed.
        if (m_rule.m_Neighbors[0] == 1 &&
        (m_rule.m_Neighbors[1] == 0 ||
            m_rule.m_Neighbors[3] == 0))
        {
            return null;
        }

        if (m_rule.m_Neighbors[2] == 1 &&
        (m_rule.m_Neighbors[1] == 0 ||
            m_rule.m_Neighbors[4] == 0))
        {
            return null;
        }

        if (m_rule.m_Neighbors[5] == 1 &&
        (m_rule.m_Neighbors[3] == 0 ||
            m_rule.m_Neighbors[6] == 0))
        {
            return null;
        }

        if (m_rule.m_Neighbors[7] == 1 &&
        (m_rule.m_Neighbors[4] == 0 ||
            m_rule.m_Neighbors[6] == 0))
        {
            return null;
        }
        return m_rule;

    }

    #region SUBTILES
    static void ProcessSubtileTexture(Texture2D _texture, AutoTileLookup _lookup)
    {
        //Spprite importer stuff to prep the asset for slicing.
        string path = AssetDatabase.GetAssetPath(_texture);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;


        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.spritePivot = Vector2.down;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        textureSettings.spriteExtrude = 0;

        importer.SetTextureSettings(textureSettings);

        //for A2s we know the subtil size is width/4
        int minSpriteSize = _texture.width / 4;

        //Slice the asset
        Rect[] rects = InternalSpriteUtility.GenerateGridSpriteRectangles(
            _texture,
            Vector2.zero,
            new Vector2(minSpriteSize, minSpriteSize),
            Vector2.zero);

        List<Rect> rectList = new List<Rect>(rects);

        string filenameNoExtension = Path.GetFileNameWithoutExtension(path);
        List<SpriteMetaData> metas = new List<SpriteMetaData>();
        int count = 0;

        foreach (Rect rect in rectList)
        {
            var meta = new SpriteMetaData();
            meta.pivot = Vector2.one * 0.5f;//center
            meta.alignment = (int)SpriteAlignment.Center;
            meta.rect = rect;
            int xpos = count % 4;
            int ypos = Mathf.CeilToInt(count / 4);

            int rightside = xpos % 2;
            int topside = (ypos + 1) % 2;
            int lookup = (topside * 2) + rightside;


            meta.name = RuleTileEditor.m_ModDirections[lookup] + count.ToString("00") + "_" + filenameNoExtension;
            metas.Add(meta);
            count++;
        }

        importer.spritesheet = metas.ToArray();

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < objects.Length; i++)
        {
            //filter out non-sprites.  I was getting some extra entry here.
            if ((objects[i] as Sprite) != null)
            {
                sprites.Add(objects[i] as Sprite);
            }
        }
        sprites.Sort((Sprite x, Sprite y) => x.name.CompareTo(y.name));

        RuleTile m_tile = ScriptableObject.CreateInstance<RuleTile>();

        m_tile.m_TilingRules = new List<RuleTile.TilingRule>();

        int rulecount = 0;
        for (int i = 255; i >= 0; i--)
        {
            RuleTile.TilingRule rule = ImportAutotile.GetRule(i);
            if (rule != null)
            {
                AddSpritesToRule(ref rule, sprites, rulecount, _lookup);

                rulecount++;
                m_tile.m_TilingRules.Add(rule);
            }
        }

        AssetDatabase.CreateAsset(m_tile, "Assets/" + filenameNoExtension + "tile.asset");

    }

    static void AddSpritesToRule(ref RuleTile.TilingRule _rule, List<Sprite> _sprites, int _ruleID, AutoTileLookup _lookup)
    {

        _rule.m_Sprites = new Sprite[4];
        _rule.m_Output = RuleTile.TilingRule.OutputSprite.Modulo;


        // 0 sw, 1 se, 2 nw, 3 ne
        int[] lookups =
        {
            (_ruleID*4) + 2,
            (_ruleID*4) + 3,
            (_ruleID*4) + 0,
            (_ruleID*4) + 1
        };

        int[] tileIDs =
        {
            _lookup.m_TileQuads[lookups[0]],
            _lookup.m_TileQuads[lookups[1]],
            _lookup.m_TileQuads[lookups[2]],
            _lookup.m_TileQuads[lookups[3]]
        };

        //how do we go from tileid to sprite?
        //sprites are in alphabetical order based on importer settings
        //its a fixed layout on the A2, so 6 tiles total, 4 subtiles each.
        //NE->NW->SE->SW

        _rule.m_Sprites[0] = _sprites[18 + tileIDs[0]];
        _rule.m_Sprites[1] = _sprites[12 + tileIDs[1]];
        _rule.m_Sprites[2] = _sprites[6 + tileIDs[2]];
        _rule.m_Sprites[3] = _sprites[tileIDs[3]];



    }

    #endregion
}
