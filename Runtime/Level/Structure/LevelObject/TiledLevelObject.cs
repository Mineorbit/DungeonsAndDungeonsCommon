using System.Collections.Generic;
using NetLevel;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    
    public class TiledLevelObject : LevelObject
    {
        public struct TileLevelObject
        {
            public int x;
            public int y;
            public int z;
        }
        public CubeTileGrid cubeTileGrid;
        public TileLevelObject[] GetTiles()
        {
            List<TileLevelObject> tiles = new List<TileLevelObject>();

            for (int x = 0; x < cubeTileGrid.gridSize;x++)
            {
             
                for (int y = 0; y < cubeTileGrid.gridSize;y++)
                {
                
                    for (int z = 0; z < cubeTileGrid.gridSize;z++)
                    {
                        if (cubeTileGrid.Exists(x, y, z))
                        {
                            TileLevelObject tl = new TileLevelObject();
                            tl.x = x;
                            tl.y = y;
                            tl.z = z;
                            tiles.Add(tl);
                        }
                    }
                }   
            }
            
            return tiles.ToArray();
        }
    }
}