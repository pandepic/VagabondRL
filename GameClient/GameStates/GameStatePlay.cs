using ElementEngine;
using ElementEngine.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public class GameStatePlay : GameState
    {
        public Game Game;
        public SpriteBatch2D SpriteBatch;
        public Camera2D Camera;

        // AI
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
        public Entity TestGuard;

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

            Player = Registry.CreateEntity();
            Player.TryAddComponent(new PlayerComponent());
            Player.TryAddComponent(new TransformComponent());
            Player.TryAddComponent(new DrawableComponent());
            Player.TryAddComponent(new FourDirectionComponent());
            Player.TryAddComponent(new ControllerMovementComponent());
            Player.TryAddComponent(new PhysicsComponent());
            Player.TryAddComponent(new ColliderComponent());

            TestGuard = Registry.CreateEntity();
            TestGuard.TryAddComponent(new GuardComponent());
            TestGuard.TryAddComponent(new TransformComponent());
            TestGuard.TryAddComponent(new DrawableComponent());
            TestGuard.TryAddComponent(new FourDirectionComponent());
            TestGuard.TryAddComponent(new PhysicsComponent());
            TestGuard.TryAddComponent(new ColliderComponent());
        }

        public override void Initialize()
        {

            // Initialize Player
            Player.GetComponent<TransformComponent>().Position = new Vector2I(20, 30);
            Player.GetComponent<TransformComponent>().Rotation = 0.0f;

            Player.GetComponent<PhysicsComponent>().Velocity = Vector2I.Zero;
            Player.GetComponent<PhysicsComponent>().Speed = 50.0f;

            Player.GetComponent<DrawableComponent>().AtlasRect = new Rectangle(new Vector2I(0, 32), new Vector2I(16, 32));
            Player.GetComponent<DrawableComponent>().Origin = new Vector2I(0, 0);
            Player.GetComponent<DrawableComponent>().Scale = new Vector2I(1, 1);
            Player.GetComponent<DrawableComponent>().Layer = 0;

            Player.GetComponent<FourDirectionComponent>().Facing = FacingType.Down;


            // Initialize Test Guard
            TestGuard.GetComponent<TransformComponent>().Position = new Vector2I(200, 30);
            TestGuard.GetComponent<TransformComponent>().Rotation = 0.0f;

            TestGuard.GetComponent<PhysicsComponent>().Velocity = Vector2I.Zero;
            TestGuard.GetComponent<PhysicsComponent>().Speed = 50.0f;

            TestGuard.GetComponent<DrawableComponent>().AtlasRect = new Rectangle(new Vector2I(0, 32), new Vector2I(16, 32));
            TestGuard.GetComponent<DrawableComponent>().Origin = new Vector2I(0, 0);
            TestGuard.GetComponent<DrawableComponent>().Scale = new Vector2I(1, 1);
            TestGuard.GetComponent<DrawableComponent>().Layer = 0;

            Player.GetComponent<FourDirectionComponent>().Facing = FacingType.Down;
        }

        public override void Load()
        {
            Player.GetComponent<DrawableComponent>().Texture = 
                AssetManager.LoadTexture2DFromPath("../../../Art/Player.png");

            TestGuard.GetComponent<DrawableComponent>().Texture =
                AssetManager.LoadTexture2DFromPath("../../../Art/Guard.png");

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
            // world space
            SpriteBatch.Begin(SamplerType.Point, Camera.GetViewMatrix());
            GeneralSystems.DrawableSystem(DrawableGroup, SpriteBatch, Camera);
            SpriteBatch.End();

            // screen space (UI)
            SpriteBatch.Begin(SamplerType.Point);
            SpriteBatch.End();
        } // Draw

    } // GameStatePlay
}
