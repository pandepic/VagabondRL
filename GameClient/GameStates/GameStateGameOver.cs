using ElementEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public class GameStateGameOver : GameState, IUIEventHandler
    {
        public Game Game;
        public SpriteBatch2D SpriteBatch;
        public UIMenu Menu;

        public GameStateGameOver(Game game)
        {
            Game = game;
        }

        public override void Initialize()
        {
            SpriteBatch = new SpriteBatch2D();

            Menu = new UIMenu();
            Menu.Load("GameOverMenu.xml", "Templates.xml");
            Menu.AddUIEventHandler(this);
        }

        public override void Load()
        {
            Menu.EnableInput();
        }

        public override void Unload()
        {
            Menu.DisableInput();
        }

        public override void Update(GameTimer gameTimer)
        {
            Menu.Update(gameTimer);
        }

        public override void Draw(GameTimer gameTimer)
        {
            SpriteBatch.Begin(SamplerType.Point);
            Menu.Draw(SpriteBatch);
            SpriteBatch.End();
        }

        public void HandleUIEvent(UIMenu source, UIEventType type, UIWidget widget)
        {
            switch (widget.Name)
            {
                case "btnRestart":
                    if (type == UIEventType.ButtonClick)
                    {
                        Globals.CurrentLevel = 1;
                        Game.SetGameState(GameStateType.NextLevel);
                    }
                    break;

                case "btnExit":
                    if (type == UIEventType.ButtonClick)
                        Game.Quit();
                    break;
            }

        } // HandleUIEvent

    } // GameStateGameOver
}
