using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
    [CustomGridBrush(true, false, false, "Modulo Brush")]
    [CreateAssetMenu(fileName = "New Modulo Brush", menuName = "Brushes/Modulo Brush")]
    public class ModuloBrush : GridBrush
    {
        public int z = 0;
        Vector3Int[] quads = new Vector3Int[4];

        public Vector3Int[] GetQuadsForTile(BoundsInt position)
        {
            return GetQuadsForTile(new Vector3Int(position.x, position.y, z));
        }

        public Vector3Int[] GetQuadsForTile(Vector3Int position)
        {
            //01
            //23
            quads[0].z = quads[1].z = quads[2].z = quads[3].z = z;

            if (Mathf.Abs(position.x) % 2 == 1)
            {
                quads[0].x = quads[2].x = position.x - 1;
                quads[1].x = quads[3].x = position.x;
            }
            else
            {
                quads[0].x = quads[2].x = position.x;
                quads[1].x = quads[3].x = position.x + 1;
            }
            if (Mathf.Abs(position.y) % 2 == 1)
            {
                quads[0].y = quads[1].y = position.y - 1;
                quads[2].y = quads[3].y = position.y;
            }
            else
            {
                quads[0].y = quads[1].y = position.y;
                quads[2].y = quads[3].y = position.y + 1;
            }
            return quads;
        }

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            foreach (Vector3Int q in GetQuadsForTile(position))
            {
                base.Paint(gridLayout, brushTarget, q);
            }
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            foreach (Vector3Int q in GetQuadsForTile(position))
            {
                base.Erase(gridLayout, brushTarget, q);
            }
        }

        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            var zPosition = new Vector3Int(position.x, position.y, z);
            base.FloodFill(gridLayout, brushTarget, zPosition);
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            var zPosition = new Vector3Int(position.x, position.y, z);
            position.position = zPosition;
            base.BoxFill(gridLayout, brushTarget, position);
        }

    }

    [CustomEditor(typeof(ModuloBrush))]
    public class ModuloBrushEditor : GridBrushEditor
    {
        private ModuloBrush moduloBrush { get { return target as ModuloBrush; } }

        private Tilemap tilemap;

        public override void OnPaintSceneGUI(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
        {


            tilemap = brushTarget.GetComponent<Tilemap>();
            if (tilemap != null)
                tilemap.ClearAllEditorPreviewTiles();


            if (tool == GridBrushBase.Tool.Paint)
            {
                foreach (Vector3Int q in moduloBrush.GetQuadsForTile(position))
                {
                    base.PaintPreview(gridLayout, brushTarget, q);
                }
            }



        }

        public override void OnMouseLeave()
        {
            if (tilemap != null)
                tilemap.ClearAllEditorPreviewTiles();

            base.OnMouseLeave();
        }






        /*
        public override void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {

          Vector3Int[] quad = new Vector3Int[2];
          if (Mathf.Abs(position.x) % 2 == 1)
          {
              quad[0] = new Vector3Int(position.x - 1, position.y, moduloBrush.z);
              quad[1] = new Vector3Int(position.x, position.y, moduloBrush.z);
          }
          else
          {
              quad[1] = new Vector3Int(position.x + 1, position.y, moduloBrush.z);
              quad[0] = new Vector3Int(position.x, position.y, moduloBrush.z);
          }

          var zPosition = new Vector3Int(position.x, position.y, moduloBrush.z);
          brushTarget
          base.PaintPreview(gridLayout, brushTarget, zPosition);

        }
       */
        public override void ClearPreview()
        {
        }
    }
}
