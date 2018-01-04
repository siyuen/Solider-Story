using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tiled2Unity;

[Tiled2Unity.CustomTiledImporter]
public class CustomImporter_StrategyTiles : Tiled2Unity.ICustomTiledImporter
{

    public void HandleCustomProperties(GameObject gameObject,
        IDictionary<string, string> customProperties)
    {
        if (customProperties.ContainsKey("Terrain"))
        {
            // Add the terrain tile game object
            MapNode tile = gameObject.AddComponent<MapNode>();
            tile.TileType = customProperties["Terrain"];
        }
    }

    public void CustomizePrefab(GameObject prefab)
    {
        // Do nothing
    }
}
