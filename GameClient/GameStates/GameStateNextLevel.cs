using ElementEngine;
using ElementEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public class GameStateNextLevel : GameState, IUIEventHandler
    {
        public Game Game;
        public UIMenu Menu;
        public SpriteBatch2D SpriteBatch;

        public GameStateNextLevel(Game game)
        {
            Game = game;
        }

        public override void Initialize()
        {
            SpriteBatch = new SpriteBatch2D();

            Menu = new UIMenu();
            Menu.Load("NextLevelMenu.xml", "Templates.xml");
            Menu.AddUIEventHandler(this);
        }

        public override void Load()
        {
            var lblTitle = Menu.GetWidget<UIWLabel>("lblTitle");
            lblTitle.Text = "Level " + Globals.CurrentLevel.ToString();

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
                case "btnStartLevel":
                    if (type == UIEventType.OnMouseClicked)
                        Game.SetGameState(GameStateType.Play);
                    break;

                case "btnExit":
                    if (type == UIEventType.OnMouseClicked)
                        Game.Quit();
                    break;
            }

        } // HandleUIEvent

    } // GameStateNextLevel
}
