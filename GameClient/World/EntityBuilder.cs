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

        public EntityBuilder(Registry registry)
        {
            Registry = registry;
        }

        public Entity CreatePlayer(Vector2I position, Texture2D texture, Vector2I frameSize)
        {
            var player = Registry.CreateEntity();
            player.TryAddComponent(new PlayerComponent());
            player.TryAddComponent(new TransformComponent()
            {
                Position = position,
            });
            player.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = new Rectangle(0, 0, frameSize.X, frameSize.Y),
                Texture = texture,
                Layer = (int)LayerType.Player,
            });

            return player;
        }

        public Entity CreateGuard(Vector2I position, Texture2D texture, Vector2I frameSize)
        {
            var guard = Registry.CreateEntity();
            guard.TryAddComponent(new GuardComponent());
            guard.TryAddComponent(new TransformComponent()
            {
                Position = position,
            });
            guard.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = new Rectangle(0, 0, frameSize.X, frameSize.Y),
                Texture = texture,
                Layer = (int)LayerType.Guard,
            });

            return guard;
        }
    }
}
