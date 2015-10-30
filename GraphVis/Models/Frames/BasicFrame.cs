using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.UserInterface;

namespace GraphVis.Models.Frames
{
    class BasicFrame : Frame
    {
        // common setting field
        public int widthDisplay = 0;
        public int heightDisplay = 0;

        public BasicFrame(Game game) : base(game)
        {
        }

        public BasicFrame(Game game, int x, int y, int w, int h, string text, Color backColor) : base(game, x, y, w, h, text, backColor)
        {
            initGlobalSize();
        }

        protected void initGlobalSize()
        {
            widthDisplay = this.Game.GraphicsDevice.DisplayBounds.Width;
            heightDisplay = this.Game.GraphicsDevice.DisplayBounds.Height;
        }


        public virtual void resizePanel()
        {
            initGlobalSize();
            this.Width = widthDisplay;
            this.Height = heightDisplay;
        }

    }
}
