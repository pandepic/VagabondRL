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

        public Texture2D TileAtlas;
        public bool ShowDebug;
        public List<AStarPathResult> TestPathingList = new List<AStarPathResult>();
        public static int GuardCount;

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
        public Group GuardVisibleGroup;

        // Entities
        public EntityBuilder EntityBuilder;
        public Entity Player;

        public static readonly Vector2I SpawnOffset = new Vector2I(0, -MapGenerator.TileSize.Y);

        public GameStatePlay(Game game)
        {
            Game = game;
            SpriteBatch = new SpriteBatch2D();
            Camera = new Camera2D(new Rectangle(0, 0, Game.Window.Width, Game.Window.Height));
            Camera.Zoom = 2;

            Registry = new Registry();
            DrawableGroup = Registry.RegisterGroup<TransformComponent, DrawableComponent>();
            MovementGroup = Registry.RegisterGroup<TransformComponent, MovementComponent, GuardComponent>();
            PathingGroup = Registry.RegisterGroup<TransformComponent, MovementComponent, GuardComponent>();
            PhysicsGroup = Registry.RegisterGroup<TransformComponent, PhysicsComponent>();
            FourDirectionSpriteGroup = Registry.RegisterGroup<FourDirectionComponent, PhysicsComponent, DrawableComponent>();
            GuardVisibleGroup = Registry.RegisterGroup<GuardComponent, TransformComponent, DrawableComponent>();
            GuardGroup = Registry.RegisterGroup<GuardComponent, MovementComponent>();

            Tilemap = Registry.CreateEntity();
            Tilemap.TryAddComponent(new TilemapComponent()
            {
            });

            GuardCount = 1;
            TileAtlas = AssetManager.LoadTexture2D("Environment.png");
            EntityBuilder = new EntityBuilder(Registry);
        }

        public override void Initialize()
        {
        }

        public override void Load()
        {
            GuardCount += 1;

            MapGenerator = new MapGenerator(Tilemap);
            MapGenerator.GenerateMap(GuardCount);

            ref var tilemapComponent = ref Tilemap.GetComponent<TilemapComponent>();

            Pathfinder = new AStarPathfinder(tilemapComponent.Graph);

            Player = EntityBuilder.CreatePlayer(new Vector2I());

            ref var playerTransform = ref Player.GetComponent<TransformComponent>();
            playerTransform.Position = tilemapComponent.PlayerSpawn + SpawnOffset;

            foreach (var guardSpawn in tilemapComponent.GuardSpawns)
            {
                EntityBuilder.CreateGuard(guardSpawn + SpawnOffset);
            }
        }

        public override void Unload()
        {
            Registry.Clear();
        }

        public override void Update(GameTimer gameTimer)
        {
            // Player
            PlayerSystems.ControllerMovementSystem(Player);

            AISystems.PathingSystem(PathingGroup, Pathfinder, Tilemap);
            AISystems.MovementSystem(MovementGroup, gameTimer);

            GeneralSystems.PhysicsSystem(PhysicsGroup, gameTimer, Tilemap);
            GeneralSystems.VisionSystem(Player, Tilemap, GuardVisibleGroup);
            GeneralSystems.GuardVisionSystem(GuardGroup, Tilemap);
            GeneralSystems.FourDirectionSystem(FourDirectionSpriteGroup, gameTimer);

            // process queues for removing entities and components etc.
            Registry.SystemsFinished();

            Camera.Center(Player.GetComponent<TransformComponent>().TransformedPosition.ToVector2I());

        } // Update

        public override void Draw(GameTimer gameTimer)
        {
            ref var tilemapComponent = ref Tilemap.GetComponent<TilemapComponent>();

            var floorSourceRect = new Rectangle(16, 48, 16, 16);
            var wallSourceRect = new Rectangle(16, 112, 16, 16);

            // world space
            SpriteBatch.Begin(SamplerType.Point, Camera.GetViewMatrix());

            for (var y = 0; y < tilemapComponent.Height; y++)
            {
                for (var x = 0; x < tilemapComponent.Width; x++)
                {
                    var index = x + tilemapComponent.Width * y;

                    if (!tilemapComponent.Expored[index])
                        continue;

                    var sourceRect = wallSourceRect;

                    if (tilemapComponent.Layers[0].Tiles[index] > 0)
                        sourceRect = floorSourceRect;

                    var tintColor = new RgbaFloat(0.3f, 0.3f, 0.3f, 0.8f);

                    if (tilemapComponent.Visible[index])
                    {
                        if (tilemapComponent.GuardsVisible[index] > -1)
                            tintColor = new RgbaFloat(0f, 1f, 0f, 0.8f);
                        else
                            tintColor = RgbaFloat.White;
                    }

                    SpriteBatch.DrawTexture2D(TileAtlas, new Rectangle(new Vector2I(x, y) * MapGenerator.TileSize, MapGenerator.TileSize), sourceRect, color: tintColor);
                }
            }

            GeneralSystems.DrawableSystem(DrawableGroup, SpriteBatch, Camera);
            SpriteBatch.End();

            if (ShowDebug)
            {
                PrimitiveBatch.Begin(SamplerType.Point, Camera.GetViewMatrix());

                for (var y = 0; y < tilemapComponent.Height; y++)
                {
                    for (var x = 0; x < tilemapComponent.Width; x++)
                    {
                        var index = x + tilemapComponent.Width * y;

                        if (tilemapComponent.Layers[0].Tiles[index] <= 0)
                            continue;

                        PrimitiveBatch.DrawFilledRect(new Rectangle(new Vector2I(x, y) * MapGenerator.TileSize, MapGenerator.TileSize), RgbaFloat.Red);
                    }
                }

                foreach (var entity in GuardVisibleGroup.Entities)
                {
                    ref var transform = ref entity.GetComponent<TransformComponent>();
                    ref var movement = ref entity.GetComponent<MovementComponent>();

                    if (movement.MovementPath.Count > 0)
                    {
                        foreach (var point in movement.MovementPath)
                        {
                            var pointRect = new Rectangle(point.ToVector2I(), MapGenerator.TileSize);
                            PrimitiveBatch.DrawFilledRect(pointRect, RgbaFloat.Yellow);
                        }
                    }

                    var guardRect = new Rectangle(transform.Position.ToVector2I(), new Vector2I(MapGenerator.TileSize.X, MapGenerator.TileSize.Y * 2));
                    PrimitiveBatch.DrawFilledRect(guardRect, RgbaFloat.Blue);
                }

                //PrimitiveBatch.DrawEmptyCircle(
                //    Player.GetComponent<TransformComponent>().TransformedPosition.ToVector2I(),
                //    (float)(MapGenerator.TileSize.X * Player.GetComponent<VisionComponent>().Range), RgbaFloat.Red);

                foreach (var listResult in TestPathingList)
                {
                    var rect = new Rectangle(listResult.Position * MapGenerator.TileSize, MapGenerator.TileSize);
                    PrimitiveBatch.DrawFilledRect(rect, RgbaFloat.Green);
                }

                PrimitiveBatch.End();
            }

            // screen space (UI)
            SpriteBatch.Begin(SamplerType.Point);
            SpriteBatch.End();
        } // Draw

        public override void HandleGameControl(string controlName, GameControlState state, GameTimer gameTimer)
        {
            var mousePosition = InputManager.MousePosition;

            switch (controlName)
            {
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

#if DEBUG
                case "ToggleDebug":
                    if (state == GameControlState.Released)
                        ShowDebug = !ShowDebug;
                    break;
#endif
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
