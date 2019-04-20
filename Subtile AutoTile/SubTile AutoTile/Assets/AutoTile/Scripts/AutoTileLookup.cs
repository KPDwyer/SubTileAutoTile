using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "AutoTileLookup", menuName = "AutoTile Import Lookup")]
public class AutoTileLookup : ScriptableObject
{
    public int[] m_TileQuads;
    public List<RuleTile.TilingRule> m_maskValues;
}

