using Morabara.Logic;
using SFML;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Windows.Forms;

namespace Morabara.Views.Base
{
    public class BaseWindow : BaseFunctionalities
    {
        protected readonly RenderWindow Window;
        protected readonly Event Event;
        protected readonly Font Font;
        protected bool IsDisposed;

        public BaseWindow()
        {
            Window = new RenderWindow(new VideoMode(800, 600), "Morabara game", Styles.Titlebar);
            WindowsStack.AddNewWindow(Window);

            Window.SetFramerateLimit(60);
            Window.Closed += (sender, e) =>
            {
                WindowsStack.CloseLastWindow();
            };

            Window.KeyPressed += (sender, e) =>
            {
                if (e.Code == Keyboard.Key.Escape)
                    WindowsStack.CloseLastWindow();
            };

            Event = new Event();

            Font = new Font("Data/Font/zorque.ttf");
            if (Font.CPointer != IntPtr.Zero) return;

            MessageBox.Show("Could't load Data/Font/zorque.ttf");
            throw new LoadingFailedException("font");
        }

        protected void Dispose()
        {
            if (!IsDisposed)
            {
                Window.Close();
            }
        }
    }
}