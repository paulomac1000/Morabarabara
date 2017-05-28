using Morabara.Logic;
using Morabara.Models;
using Morabara.Views.Base;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Windows.Forms;

namespace Morabara.Views
{
    public class Game : BaseWindow
    {
        private readonly Sprite backgroundSprite;
        private readonly Sprite boardSprite;
        private readonly GameLogic gameLogic;
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

            bindEvents();
            start();
        }

        private void start()
        {
            while (Window.IsOpen)
            {
                Window.DispatchEvents(); //init event
                Window.Clear(); //clear window

                Window.Draw(backgroundSprite);
                Window.Draw(boardSprite);
                foreach (var circle in gameLogic.GetAllBalls())
                {
                    Window.Draw(circle);
                }

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
    }
}