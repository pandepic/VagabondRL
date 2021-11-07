using ElementEngine;
using ElementEngine.ECS;
using SharpNeat.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public enum LayerType
    {
        Terrain = 0,
        Loot,
        Guard,
        Player,
    }

    public class EntityBuilder
    {
        public static readonly Vector2I SpriteFrameSize = new Vector2I(16, 32);
        public Registry Registry;

        private FastRandom _rng = new FastRandom();

        public EntityBuilder(Registry registry)
        {
            Registry = registry;
        }

        public Entity CreatePlayer(Vector2I position)
        {
            var player = Registry.CreateEntity();
            player.TryAddComponent(new PlayerComponent());
            player.TryAddComponent(new TransformComponent()
            {
                Position = position.ToVector2(),
            });
            player.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = new Rectangle(0, 0, SpriteFrameSize.X, SpriteFrameSize.Y),
                Texture = AssetManager.LoadTexture2D("Player.png"),
                Layer = (int)LayerType.Player,
                Scale = new Vector2(1f),
                IsVisible = true,
            });
            player.TryAddComponent(new FourDirectionComponent()
            {
                BaseFrameTime = 0.2f,
                CurrentFrame = 0,
                Facing = FacingType.Down
            });
            player.TryAddComponent(new PhysicsComponent()
            {
                Velocity = Vector2.Zero,
                Speed = 60f,
            });
            player.TryAddComponent(new VisionComponent()
            {
                Range = 10,
            });
            player.TryAddComponent(new ColliderComponent());

            return player;

        } // CreatePlayer

        public Entity CreateGuard(Vector2I position)
        {
            var guard = Registry.CreateEntity();
            guard.TryAddComponent(new GuardComponent()
            {
                State = GuardStateType.Patrol,
            });
            guard.TryAddComponent(new TransformComponent()
            {
                Position = position.ToVector2(),
            });
            guard.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = new Rectangle(0, 0, SpriteFrameSize.X, SpriteFrameSize.Y),
                Texture = AssetManager.LoadTexture2D("Guard.png"),
                Layer = (int)LayerType.Guard,
                Scale = new Vector2(1f),
            });
            guard.TryAddComponent(new MovementComponent()
            {
                MovementPath = new List<Vector2>(),
                Destination = Vector2.Zero
            });
            guard.TryAddComponent(new FourDirectionComponent());

            guard.TryAddComponent(new GuardSensesComponent());
            guard.TryAddComponent(new GuardMemoryComponent());
            guard.TryAddComponent(new ColliderComponent());

            guard.TryAddComponent(new PhysicsComponent()
            {
                Velocity = Vector2.Zero,
                Speed = 40f,
            });

            guard.TryAddComponent(new VisionComponent()
            {
                Range = 6,
            });

            return guard;
        } // CreateGuard

        public Entity CreateLoot(Vector2I position)
        {
            var loot = Registry.CreateEntity();
            loot.TryAddComponent(new LootComponent());
            loot.TryAddComponent(new DrawableComponent()
            {
                AtlasRect = new Rectangle(MapGenerator.TileSize.X * _rng.Next(0, 3), 0, MapGenerator.TileSize.X, MapGenerator.TileSize.Y),
                Texture = AssetManager.LoadTexture2D("EnvironmentObjects.png"),
                Layer = (int)LayerType.Loot,
                Scale = new Vector2(1f),
                IsVisible = true,
            });
            loot.TryAddComponent(new TransformComponent()
            {
                Position = position.ToVector2(),
            });
            
            return loot;
        }

    } // EntityBuilder
}
