using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FD.Generation;

/// <summary>
/// Draw the tile shape on the prefab
/// </summary>
public class TileShapeDebug : MonoBehaviour
{
    public TileSet tileSet;
    public Vector2Int index;
    public Vector2Int tileSize = new Vector2Int(20, 20);

    private void OnDrawGizmos()
    {
        if(tileSet == null) return;

        Tile t = tileSet.groups[index.x].tiles[index.y];
        // Draw shape
        foreach(Vector2 item in t.shape)
        {
            Gizmos.color = Color.red;//Gizmos.color == Color.blue ? Color.red : Color.blue;
            Gizmos.DrawWireCube(transform.position + new Vector3(item.x * tileSize.x, 0, item.y * tileSize.y), Vector3.one);
        }

        // Draw endPos
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + new Vector3(t.endPosition.x * tileSize.x, 0, t.endPosition.y * tileSize.y), Vector3.one);
    }
}
