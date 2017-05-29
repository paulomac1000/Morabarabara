﻿using Morabara.Logic;
using Morabara.Models;
using Morabara.Views.Base;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Morabara.Views
{
    public class Game : BaseWindow
    {
        private readonly Sprite backgroundSprite;
        private readonly Sprite boardSprite;
        private readonly GameLogic gameLogic;

        private readonly Text PlayerName;
        private readonly Text PlayerPoints;
        private readonly Text MoveNumber;
        private readonly Stopwatch stopwatch;
        private readonly Text PlayTime;
        private readonly Text WchichMove;
        private readonly Text ActionName;

        public Game()
        {
            var backgroundTexture = new Texture("Data/Texture/bg.jpg");
            if (backgroundTexture.CPointer == IntPtr.Zero)
            {
                MessageBox.Show("Could't load Data/Texture/bg.jpg");
                throw new LoadingFailedException("texture");
            }
            backgroundSprite = new Sprite(backgroundTexture);

            var boardTexture = new Texture("Data/Texture/plansza.png");
            if (boardTexture.CPointer == IntPtr.Zero)
            {
                MessageBox.Show("Could't load Data/Texture/plansza.png");
                throw new LoadingFailedException("texture");
            }

            boardSprite = new Sprite(boardTexture) { Position = new Vector2f(Setting.BoardMarginX, Setting.BoardMarginY) };
            gameLogic = new GameLogic();

            PlayerName = new Text($"Nick: {Setting.PlayerName}", Font)
            {
                CharacterSize = 30,
                Style = Text.Styles.Regular,
                Position = new Vector2f(550, 100),
                Color = Color.Blue
            };

            PlayerPoints = new Text($"Points: {0}", Font)
            {
                CharacterSize = 30,
                Style = Text.Styles.Regular,
                Position = new Vector2f(550, 150),
                Color = Color.Blue
            };

            MoveNumber = new Text($"Move: {8}", Font)
            {
                CharacterSize = 30,
                Style = Text.Styles.Regular,
                Position = new Vector2f(550, 200),
                Color = Color.Blue
            };

            stopwatch = new Stopwatch();
            stopwatch.Start();
            
            PlayTime = new Text($"0 m. 0 s.", Font)
            {
                CharacterSize = 30,
                Style = Text.Styles.Regular,
                Position = new Vector2f(550, 250),
                Color = Color.Blue
            };

            WchichMove = new Text("Move: Your", Font)
            {
                CharacterSize = 30,
                Style = Text.Styles.Regular,
                Position = new Vector2f(550, 350),
                Color = Color.Blue
            };

            ActionName = new Text("Action name", Font)
            {
                CharacterSize = 30,
                Style = Text.Styles.Regular,
                Position = new Vector2f(550, 400),
                Color = Color.Blue
            };

            bindEvents();
            start();
        }

        private void start()
        {
            while (Window.IsOpen)
            {
                Window.DispatchEvents(); //init event
                Window.Clear(); //clear window
                updateText();

                Window.Draw(backgroundSprite);
                Window.Draw(boardSprite);
                foreach (var circle in gameLogic.GetAllBalls())
                {
                    Window.Draw(circle);
                }

                Window.Draw(PlayerName);
                Window.Draw(PlayerPoints);
                Window.Draw(MoveNumber);
                Window.Draw(PlayTime);
                Window.Draw(WchichMove);
                Window.Draw(ActionName);

                Window.Display(); //display render up view
            }
        }

        private void bindEvents()
        {
            Window.MouseMoved += (sender, e) =>
            {
            };

            Window.MouseButtonReleased += (sender, args) =>
            {
                if (!gameLogic.IsPlayerMove) return;

                foreach (var field in gameLogic.GetAllFields())
                {
                    if (!field.Circle.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y)) continue;

                    if (field.TakenBy != TakenBy.Nobody) continue;

                    gameLogic.AssignBallTo(field.Id, TakenBy.Player);
                    gameLogic.SwitchMoveOrder();
                }
            };
        }

        private void updateText()
        {
            PlayerPoints.DisplayedString = "Points";
            MoveNumber.DisplayedString = "8";

            var minutes = stopwatch.ElapsedMilliseconds / 1000 / 60;
            var seconds = stopwatch.ElapsedMilliseconds / 1000 - (minutes * 60);
            PlayTime.DisplayedString = $"{minutes} m. {seconds} s.";

            WchichMove.DisplayedString = (gameLogic.IsPlayerMove) ? "Move: Your" : "move: Comp.";

            ActionName.DisplayedString = "Action name";
        }
    }
}