using Morabara.Logic;
using Morabara.Views.Base;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Linq;

namespace Morabara.Views
{
    public class Top : BaseWindow
    {
        private readonly RectangleShape backButton;
        private readonly Text backText;
        private readonly Text topText;

        public Top()
        {
            backButton = new RectangleShape(new Vector2f(700, 100))
            {
                FillColor = new Color(150, 200, 150),
                Position = new Vector2f(50, 475)
            };

            backText = new Text("BACK TO MENU", Font)
            {
                CharacterSize = 60,
                Style = Text.Styles.Regular,
                Position = new Vector2f(190, 485),
                Color = Color.Blue
            };

            string topTextString = string.Empty;
            var topPlayers = TopLogic.ReadTop();
            if (topPlayers != null && !topPlayers.Any())
            {
                var topScores = TopLogic.ReadTop().Select(t => $"{t.Name} - {t.Points}");
                topTextString = string.Join("\n", topScores);
            }
            else
            {
                topTextString = "No entities.";
            }

            topText = new Text(topTextString, Font)
            {
                CharacterSize = 20,
                Style = Text.Styles.Regular,
                Position = new Vector2f(50, 50),
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

                Window.Draw(backButton);
                Window.Draw(backText);

                Window.Draw(topText);

                Window.Display(); //display render up view
            }
        }

        private void bindEvents()
        {
            Window.MouseMoved += (sender, e) =>
            {
                backButton.FillColor = backButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y)
                    ? new Color(100, 100, 100)
                    : new Color(150, 200, 150);
            };

            Window.MouseButtonReleased += (sender, args) =>
            {
                if (backButton.GetGlobalBounds().Contains(Mouse.GetPosition(Window).X, Mouse.GetPosition(Window).Y))
                {
                    WindowsStack.CloseLastWindow();
                }
            };
        }
    }
}