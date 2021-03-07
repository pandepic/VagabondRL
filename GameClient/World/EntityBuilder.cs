using ElementEngine;
using ElementEngine.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public enum TileType
    {
        Player = 25,
        Guard = 28,

        // terrain
        Grass = 7,
        Tree = 68,
        LongGrass = 64,
        Flowers = 208,
        RoungBush = 180,
        TriangleBush = 179,

        // building
        Wall = 554,
        Window = 555,
        DoorClosed = 363,
        DoorOpen = 364,
    }

    public enum LayerType
    {
        Terrain = 0,
        Guard = 1,
        Player = 2,
    }

    public class EntityBuilder
    {
        public Registry Registry;
        public Texture2D Tilesheet;
        public Vector2I TilesheetTiles;
        public Vector2I TileSize = new Vector2I(16);

        public EntityBuilder(Registry registry, Texture2D tilesheet)
        {
            Registry = registry;
            Tilesheet = tilesheet;

            TilesheetTiles = tilesheet.Size / TileSize;
        }

        public Rectangle GetSourceRect(TileType tile)
        {
            var tileIndex = (int)tile;

            var tileCoords = new Vector2I(
                tileIndex % TilesheetTiles.X,
                tileIndex / TilesheetTiles.X);

            return new Rectangle(tileCoords * TileSize, TileSize);
        }

        public Entity CreatePlayer(Vector2I position)
        {
            var player = Registry.CreateEntity();
            player.TryAddComponent(new PlayerComponent());
            player.TryAddComponent(new TransformComponent()
            {
                Position = position,
            });
            player.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = GetSourceRect(TileType.Player),
                Texture = Tilesheet,
                Layer = (int)LayerType.Player,
            });

            return player;
        }

        public Entity CreateGuard(Vector2I position)
        {
            var guard = Registry.CreateEntity();
            guard.TryAddComponent(new GuardComponent());
            guard.TryAddComponent(new TransformComponent()
            {
                Position = position,
            });
            guard.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = GetSourceRect(TileType.Guard),
                Texture = Tilesheet,
                Layer = (int)LayerType.Guard,
            });

            return guard;
        }
    }
}
