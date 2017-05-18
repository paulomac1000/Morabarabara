using SFML;
using SFML.Graphics;
using SFML.Window;
using System;

namespace Morabara.Views.Base
{
    public class BaseWindow : BaseFunctionalities
    {
        protected readonly RenderWindow Window;
        protected readonly Event Event;
        protected readonly Font Font;

        public BaseWindow()
        {
            Window = new RenderWindow(new VideoMode(800, 600), "Morabara game", Styles.Titlebar);
            Window.SetFramerateLimit(60);
            Window.Closed += OnClose;

            Window.KeyPressed += (sender, e) =>
            {
                var window = (Window) sender;
                if (e.Code == Keyboard.Key.Escape)
                    window.Close();
            };

            Event = new Event();

            Font = new Font("Data/Font/zorque.ttf");
            if (Font.CPointer == IntPtr.Zero)
            {
                throw new LoadingFailedException("font");
            }
        }

        protected static void OnClose(object sender, EventArgs e)
        {
            // Close the window when OnClose event is received
            var window = (RenderWindow)sender;
            window.Close();
        }
    }
}