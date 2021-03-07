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

        // ECS
        public Registry Registry;
        public Group DrawableGroup;
        public Group MovementGroup;

        // Entities
        public EntityBuilder EntityBuilder;

        public GameStatePlay(Game game)
        {
            Game = game;
            SpriteBatch = new SpriteBatch2D();
            Camera = new Camera2D(new Rectangle(0, 0, Game.Window.Width, Game.Window.Height));

            Registry = new Registry();
            DrawableGroup = Registry.RegisterGroup<TransformComponent, DrawableComponent>();
            MovementGroup = Registry.RegisterGroup<TransformComponent, MovementComponent>();

            EntityBuilder = new EntityBuilder(Registry, AssetManager.LoadTexture2D("tileset_legacy.png"));
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

        // call this after the player does their turn/action
        public void Tick()
        {
            GeneralSystems.MovementSystem(MovementGroup);
        }

    } // GameStatePlay
}
