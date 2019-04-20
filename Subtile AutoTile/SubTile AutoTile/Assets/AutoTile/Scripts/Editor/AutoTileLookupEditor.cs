using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutoTileLookup))]
public class AutoTileLookupEditor : Editor
{
    bool showBaseInspector = false;
    bool showSubTileMapping = false;
    bool overrideRulesFromTile = false;


    const int TILECOUNT = 47;//feel like I'm missing one here...
    const int SUBTILES = 4;



    public override void OnInspectorGUI()
    {
        AutoTileLookup lookup = target as AutoTileLookup;



        showBaseInspector = EditorGUILayout.Foldout(showBaseInspector, "Show Base Inspector");
        if (showBaseInspector)
        {
            base.OnInspectorGUI();
        }

        overrideRulesFromTile = EditorGUILayout.Foldout(overrideRulesFromTile, "Override Rules List");
        if (overrideRulesFromTile)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField(
            "This will set the target rules for this lookup to match.  " +
            "You don't need to use this unless you are creating a new lookup.", GUI.skin.box);

                RuleTile load = (RuleTile)EditorGUILayout.ObjectField("Load Ruleset", null, typeof(RuleTile), false);
                if (load)
                {
                    lookup.m_maskValues = load.m_TilingRules;
                }
            }
            EditorGUILayout.EndVertical();
        }

        showSubTileMapping = EditorGUILayout.Foldout(showSubTileMapping, "Show Subtile Mapping");
        if (showSubTileMapping)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField(
            "This shows each Rule (set with the override tile rules parameter.)  " +
            "Alongside each rule is the lookup data for each quadrant of the tile.", GUI.skin.box);
            }
            EditorGUILayout.EndVertical();

            SubTileMappingGUI(lookup);
        }



        EditorGUILayout.BeginVertical(GUI.skin.box);
        {

            EditorGUILayout.LabelField(
               "This will take the texture and generate " +
               "the appropriate assets + RuleTile data.", GUI.skin.box);

            Texture2D texture = (Texture2D)EditorGUILayout.ObjectField(null, typeof(Texture2D), false);
            if (texture != null)
            {
                ImportAutotile.ImportSubtile(texture, lookup);
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void SubTileMappingGUI(AutoTileLookup lookup)
    {
        int totalInts = TILECOUNT * SUBTILES;

        if (lookup.m_TileQuads == null)
        {
            lookup.m_TileQuads = new int[totalInts];
            EditorUtility.SetDirty(lookup);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
        }

        if (lookup.m_TileQuads.Length != totalInts)
        {
            EditorGUILayout.LabelField("error with TileQuads param");
            if (GUILayout.Button("Fix/Reset"))
            {
                lookup.m_TileQuads = new int[totalInts];
                EditorUtility.SetDirty(lookup);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return;
            }
            return;
        }

        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < TILECOUNT; i++)
        {
            EditorGUILayout.BeginHorizontal("box");
            if (lookup.m_maskValues != null && lookup.m_maskValues.Count > i)
            {

                TileMaskGUI(lookup.m_maskValues[i]);
            }
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            lookup.m_TileQuads[i * 4] = EditorGUILayout.IntField(lookup.m_TileQuads[i * 4]);
            lookup.m_TileQuads[(i * 4) + 1] = EditorGUILayout.IntField(lookup.m_TileQuads[(i * 4) + 1]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            lookup.m_TileQuads[(i * 4) + 2] = EditorGUILayout.IntField(lookup.m_TileQuads[(i * 4) + 2]);
            lookup.m_TileQuads[(i * 4) + 3] = EditorGUILayout.IntField(lookup.m_TileQuads[(i * 4) + 3]);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(lookup);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    private void TileMaskGUI(RuleTile.TilingRule _rule)
    {
        int[] neighbours = _rule.m_Neighbors;

        int count = 0;
        EditorGUILayout.BeginVertical("box");

        for (int y = 0; y < 3; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < 3; x++)
            {
                if (x == 1 && y == 1)
                {
                    GUI.color = Color.white;
                    EditorGUILayout.Toggle(false);
                }
                else
                {
                    GUI.color = neighbours[count] == 1 ? Color.green : Color.red;
                    EditorGUILayout.Toggle(neighbours[count] == 1);
                    count++;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        GUI.color = Color.white;

        EditorGUILayout.EndVertical();
    }
}
