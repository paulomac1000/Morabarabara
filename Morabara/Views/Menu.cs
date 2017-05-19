using Morabara.Logic;
using Morabara.Views.Base;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Windows.Forms;

namespace Morabara.Views
{
    public class Menu : BaseWindow
    {
        private readonly RectangleShape newGameButton;
        private readonly RectangleShape topButton;
        private readonly RectangleShape settingsButton;
        private readonly RectangleShape exitButton;

        Text newGameText;
        Text topText;
        Text settingsText;
        Text exitText;

        public Menu()
        {
            newGameButton = new RectangleShape(new Vector2f(700, 100))
            {
                FillColor = new Color(150, 200, 150),
                Position = new Vector2f(50, 25)
            };

            newGameText = new Text("NEW GAME", Font)
            {
                CharacterSize = 60,
                Style = Text.Styles.Regular,
                Position = new Vector2f(235, 35),
                Color = Color.Blue
            };

            topButton = new RectangleShape(new Vector2f(700, 100))
            {
                FillColor = new Color(150, 200, 150),
                Position = new Vector2f(50, 175)
            };

            topText = new Text("TOP 10", Font)
            {
                CharacterSize = 60,
                Style = Text.Styles.Regular,
                Position = new Vector2f(300, 185),
                Color = Color.Blue
            };

            settingsButton = new RectangleShape(new Vector2f(700, 100))
            {
                FillColor = new Color(150, 200, 150),
                Position = new Vector2f(50, 325)
            };

            settingsText = new Text("Settings", Font)
            {
                CharacterSize = 60,
                Style = Text.Styles.Regular,
                Position = new Vector2f(260, 345),
                Color = Color.Blue
            };

            exitButton = new RectangleShape(new Vector2f(700, 100))
            {
                FillColor = new Color(150, 200, 150),
                Position = new Vector2f(50, 475)
            };

            exitText = new Text("EXIT", Font)
            {
                CharacterSize = 60,
                Style = Text.Styles.Regular,
                Position = new Vector2f(340, 485),
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
                Window.Clear(new Color(0, 192, 255)); //clear window

                Window.Draw(newGameButton);
                Window.Draw(newGameText);

                Window.Draw(topButton);
                Window.Draw(topText);

                Window.Draw(settingsButton);
                Window.Draw(settingsText);

                Window.Draw(exitButton);
                Window.Draw(exitText);

                Window.Display(); //display render up view
            }
        }

        private void bindEvents()
        {
            Window.MouseMoved += (sender, e) =>
            {
                newGameButton.FillColor = newGameButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y)
                    ? new Color(100, 100, 100)
                    : new Color(150, 200, 150);
                topButton.FillColor = topButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y)
                    ? new Color(100, 100, 100)
                    : new Color(150, 200, 150);
                settingsButton.FillColor = settingsButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y)
                    ? new Color(100, 100, 100)
                    : new Color(150, 200, 150);
                exitButton.FillColor = exitButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y)
                    ? new Color(100, 100, 100)
                    : new Color(150, 200, 150);
            };

            Window.MouseButtonReleased += (sender, args) =>
            {
                if (newGameButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y))
                {
                    MessageBox.Show("Not implemented yet");
                }
                else if (topButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y))
                {
                    new Top();
                }
                else if (settingsButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y))
                {
                    MessageBox.Show("Not implemented yet");
                }
                else if (exitButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y))
                {
                    WindowsStack.CloseLastWindow();
                }
            };
        }
    }
}