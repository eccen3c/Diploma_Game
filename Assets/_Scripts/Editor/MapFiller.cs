using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class MapFiller : Editor
{
    [MenuItem("Tools/Fill Map with Grass")]
    static void FillMap()
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Sprites/map/tiles/Tilemap_color2_9.asset");
        if (tile == null) { Debug.LogError("Tile not found!"); return; }

        Tilemap tilemap = FindFirstObjectByType<Tilemap>();
        if (tilemap == null) { Debug.LogError("No Tilemap in scene!"); return; }

        Undo.RegisterCompleteObjectUndo(tilemap, "Fill Map");

        tilemap.ClearAllTiles();

        for (int x = -75; x <= 75; x++)
            for (int y = -14; y <= 13; y++)
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);

        tilemap.RefreshAllTiles();
        EditorUtility.SetDirty(tilemap);
        Debug.Log("Map filled!");
    }
}
