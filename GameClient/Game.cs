using ElementEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public enum GameStateType
    {
        Menu,
        Play,
        Settings,
    }

    public class Game : BaseGame
    {
        public Dictionary<GameStateType, GameState> GameStates = new Dictionary<GameStateType, GameState>();

        public override void Load()
        {
            SettingsManager.LoadFromPath("Settings.xml");

            var windowRect = new ElementEngine.Rectangle()
            {
                X = 100,
                Y = 100,
                Width = SettingsManager.GetSetting<int>("Window", "Width"),
                Height = SettingsManager.GetSetting<int>("Window", "Height")
            };

            var vsync = true;
#if DEBUG
            vsync = false;
#endif

            SetupWindow(windowRect, "VagabondRL", null, vsync);
            SetupAssets("Mods");
            InputManager.LoadGameControls();

            ClearColor = Veldrid.RgbaFloat.CornflowerBlue;
            Window.Resizable = false;

            GameStates.Add(GameStateType.Play, new GameStatePlay(this));
            SetGameState(GameStateType.Play);
        }

        public override void Update(GameTimer gameTimer)
        {
        }

        public override void Draw(GameTimer gameTimer)
        {
        }

        public void SetGameState(GameStateType type)
        {
            SetGameState(GameStates[type]);
        }

    } // Game
}
