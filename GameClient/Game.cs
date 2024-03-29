﻿using ElementEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public enum GameStateType
    {
        NextLevel,
        Play,
        GameOver,
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

            ClearColor = Veldrid.RgbaFloat.Black;
            Window.Resizable = false;

            GameStates.Add(GameStateType.NextLevel, new GameStateNextLevel(this));
            GameStates.Add(GameStateType.Play, new GameStatePlay(this));
            GameStates.Add(GameStateType.GameOver, new GameStateGameOver(this));

            SetGameState(GameStateType.NextLevel);

            SoundManager.Play(AssetManager.LoadAudioSourceByExtension("SerfsUp.ogg"), 0);
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
