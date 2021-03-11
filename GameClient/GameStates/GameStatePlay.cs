using ElementEngine;
using ElementEngine.ECS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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
        public AreaSoundsManager AreaSounds;

        public PrimitiveBatch PrimitiveBatch = new PrimitiveBatch();

        protected bool _dragging = false;
        protected Vector2 _dragMousePosition = Vector2.Zero;

        protected int _zoomIndex = 1;
        protected float[] _zoomLevels = new float[]
        {
            2f,
            1f,
            0.5f,
            0.25f,
            0.125f,
        };

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
        public Group GuardGroup;

        // Entities
        public EntityBuilder EntityBuilder;
        public Entity Player;

        public GameStatePlay(Game game)
        {
            Game = game;
            SpriteBatch = new SpriteBatch2D();
            Camera = new Camera2D(new Rectangle(0, 0, Game.Window.Width, Game.Window.Height));
            Camera.Zoom = 2;
            
            Registry = new Registry();
            DrawableGroup = Registry.RegisterGroup<TransformComponent, DrawableComponent>();
            MovementGroup = Registry.RegisterGroup<TransformComponent, MovementComponent>();
            PathingGroup = Registry.RegisterGroup<MovementComponent>();
            PhysicsGroup = Registry.RegisterGroup<TransformComponent, PhysicsComponent>();
            FourDirectionSpriteGroup = Registry.RegisterGroup<FourDirectionComponent, PhysicsComponent, DrawableComponent>();
            GuardGroup = Registry.RegisterGroup<GuardComponent, MovementComponent>();

            Tilemap = Registry.CreateEntity();
            Tilemap.TryAddComponent(new TilemapComponent()
            {
            });

            ref var tilemapComponent = ref Tilemap.GetComponent<TilemapComponent>();

            EntityBuilder = new EntityBuilder(Registry);
            Pathfinder = new AStarPathfinder(tilemapComponent.Graph);

            Player = EntityBuilder.CreatePlayer(new Vector2I());
            var testGuard = EntityBuilder.CreateGuard(new Vector2I());

            MapGenerator = new MapGenerator(Tilemap);
            MapGenerator.GenerateMap();

            AreaSounds = new AreaSoundsManager();
            ref var playerTransform = ref Player.GetComponent<TransformComponent>();
            playerTransform.Position = tilemapComponent.PlayerSpawn;
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
            // Player
            PlayerSystems.ControllerMovementSystem(Player);
            GeneralSystems.FourDirectionSystem(FourDirectionSpriteGroup, gameTimer);
            GeneralSystems.MovementSystem(MovementGroup);
            AISystems.GuardAISystem(GuardGroup, Player);
            AISystems.PathingSystem(PathingGroup, Pathfinder);
            AISystems.MovementSystem(MovementGroup, gameTimer);
            AISystems.AreaSoundSystem(AreaSounds, gameTimer);

            GeneralSystems.PhysicsSystem(PhysicsGroup, gameTimer);

            // process queues for removing entities and components etc.
            Registry.SystemsFinished();

            Camera.Center(Player.GetComponent<TransformComponent>().TransformedPosition.ToVector2I());

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

                    PrimitiveBatch.DrawOutlinedRect(new Rectangle(new Vector2I(x, y) * MapGenerator.TileSize, MapGenerator.TileSize), RgbaFloat.Clear, color, 2);
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
            var mousePosition = InputManager.MousePosition;

            switch (controlName)
            {
                case "PassTurn":
                    if (state == GameControlState.Released)
                    {
                        MapGenerator.GenerateMap();
                    }
                    break;

                //case "DragCamera":
                //    if (state == GameControlState.Pressed)
                //    {
                //        if (!_dragging)
                //        {
                //            _dragging = true;
                //            _dragMousePosition = mousePosition;
                //        }
                //    }
                //    else if (state == GameControlState.Released)
                //    {
                //        if (_dragging)
                //            _dragging = false;
                //    }
                //    break;

                case "ZoomIn":
                    if (state == GameControlState.Released || state == GameControlState.WheelUp)
                    {
                        _zoomIndex -= 1;
                        if (_zoomIndex < 0)
                            _zoomIndex = 0;

                        Camera.Zoom = _zoomLevels[_zoomIndex];
                    }
                    break;

                case "ZoomOut":
                    if (state == GameControlState.Released || state == GameControlState.WheelDown)
                    {
                        _zoomIndex += 1;
                        if (_zoomIndex >= _zoomLevels.Length)
                            _zoomIndex = _zoomLevels.Length - 1;

                        Camera.Zoom = _zoomLevels[_zoomIndex];
                    }
                    break;
            }
        }

        public override void HandleMouseMotion(Vector2 mousePosition, Vector2 prevMousePosition, GameTimer gameTimer)
        {
            if (_dragging)
            {
                var difference = mousePosition - _dragMousePosition;
                difference /= Camera.Zoom;
                Camera.Position -= difference;

                _dragMousePosition = mousePosition;
            }
        }

    } // GameStatePlay
}
