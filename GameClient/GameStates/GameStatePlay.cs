using ElementEngine;
using ElementEngine.ECS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

using Rectangle = ElementEngine.Rectangle;

namespace VagabondRL
{
    public class GameStatePlay : GameState
    {
        public Game Game;
        public SpriteBatch2D SpriteBatch;
        public Camera2D Camera;

        public PrimitiveBatch PrimitiveBatch = new PrimitiveBatch();

        // AI
        public MapGenerator MapGenerator;
        public AStarPathfinder Pathfinder;
        public Entity Tilemap;

        // ECS
        public Registry Registry;
        public Group DrawableGroup;
        public Group MovementGroup;
        public Group PathingGroup;
        public Group PhysicsGroup;
        public Group FourDirectionSpriteGroup;  // entities which are drawn with a 4-direction-type sprite

        // Entities
        public EntityBuilder EntityBuilder;
        public Entity Player;

        public GameStatePlay(Game game)
        {
            Game = game;
            SpriteBatch = new SpriteBatch2D();
            Camera = new Camera2D(new Rectangle(0, 0, Game.Window.Width, Game.Window.Height));
            
            Registry = new Registry();
            DrawableGroup = Registry.RegisterGroup<TransformComponent, DrawableComponent>();
            MovementGroup = Registry.RegisterGroup<TransformComponent, MovementComponent>();
            PathingGroup = Registry.RegisterGroup<MovementComponent>();
            PhysicsGroup = Registry.RegisterGroup<TransformComponent, PhysicsComponent>();
            FourDirectionSpriteGroup = Registry.RegisterGroup<FourDirectionComponent>();

            Tilemap = Registry.CreateEntity();
            Tilemap.TryAddComponent(new TilemapComponent()
            {
            });

            EntityBuilder = new EntityBuilder(Registry);
            Pathfinder = new AStarPathfinder(Tilemap.GetComponent<TilemapComponent>().Graph);

            Player = EntityBuilder.CreatePlayer(new Vector2I());
            var testGuard = EntityBuilder.CreateGuard(new Vector2I());

            MapGenerator = new MapGenerator(Tilemap);
            MapGenerator.GenerateMap();
        }

        public override void Initialize()
        {
        }

        public override void Load()
        {
        }

        public override void Unload()
        {
        }

        public override void Update(GameTimer gameTimer)
        {
            PlayerSystems.ControllerMovementSystem(Player);
            GeneralSystems.FourDirectionSystem(FourDirectionSpriteGroup);
            GeneralSystems.MovementSystem(MovementGroup);
            AISystems.PathingSystem(PathingGroup, Pathfinder);
            AISystems.MovementSystem(MovementGroup);
            GeneralSystems.PhysicsSystem(PhysicsGroup, gameTimer);

            // process queues for removing entities and components etc.
            Registry.SystemsFinished();

        } // Update

        public override void Draw(GameTimer gameTimer)
        {
            // map gen testing
            PrimitiveBatch.Begin(SamplerType.Point, Camera.GetViewMatrix());

            ref var tilemapComponent = ref Tilemap.GetComponent<TilemapComponent>();

            for (var y = 0; y < tilemapComponent.Height; y++)
            {
                for (var x = 0; x < tilemapComponent.Width; x++)
                {
                    var index = x + tilemapComponent.Width * y;
                    var color = RgbaFloat.Clear;

                    if (tilemapComponent.Layers[0].Tiles[index] > 0)
                        color = RgbaFloat.Red;

                    PrimitiveBatch.DrawFilledRect(new Rectangle(new Vector2I(x, y) * MapGenerator.TileSize, MapGenerator.TileSize), color);
                }
            }

            PrimitiveBatch.End();

            // world space
            SpriteBatch.Begin(SamplerType.Point, Camera.GetViewMatrix());
            GeneralSystems.DrawableSystem(DrawableGroup, SpriteBatch, Camera);
            SpriteBatch.End();

            // screen space (UI)
            SpriteBatch.Begin(SamplerType.Point);
            SpriteBatch.End();
        } // Draw

        public override void HandleGameControl(string controlName, GameControlState state, GameTimer gameTimer)
        {
            if (controlName == "PassTurn" && state == GameControlState.Released)
                MapGenerator.GenerateMap();
        }

    } // GameStatePlay
}
