using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morabara.Views.Base;
using SFML.Graphics;

namespace Morabara.Views
{
    public class Top: BaseWindow
    {
        public Top()
        {

            bindEvents();
            start();
        }

        private void start()
        {
            while (Window.IsOpen)
            {
                Window.DispatchEvents(); //init event
                Window.Clear(new Color(0, 192, 255)); //clear window



                Window.Display(); //display render up view
            }
        }

        private void bindEvents()
        {

        }
    }
}
