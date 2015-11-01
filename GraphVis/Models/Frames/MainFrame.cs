using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using GraphVis.Helpers;


namespace GraphVis.Models.Frames
{
    class MainFrame : BasicFrame
    {

        private RightFrame rightFrame;
        private List<BottomFrame> bottomFrameList;

        
        public MainFrame(Game game)
            : base(game)
        {
        }

        public MainFrame(Game game, int x, int y, int w, int h, string text, Color backColor)
            : base(game, x, y, w, h, text, backColor)
        {
            rightFrame = new RightFrame(game,
                                widthDisplay - ConstantFrame.RIGHT_PANEL_WIDTH,
                                y,
                                ConstantFrame.RIGHT_PANEL_WIDTH,
                                h,
                                "",
                                ConstantFrame.COLOR_RIGHT_FRAME
                                );
            this.Add(rightFrame);
            bottomFrameList = new List<BottomFrame>();
        }

        public RightFrame GetRightFrame()
        {
            return rightFrame;
        }

        public void resizePanel()
        {
            foreach (BasicFrame child in this.Children)
            {
                child.resizePanel();
            }
            base.resizePanel();
        }

        public void clearBottomFrames()
        {
            foreach (var bottomFrame in bottomFrameList)
            {
                this.Remove(bottomFrame);
            }
            bottomFrameList.Clear();
            HelperFrame.listLevel.Clear();
        }

        public void RemoveBottomFrame(int position)
        {
            while (bottomFrameList.Count > position)
            {
                var bottomFrame = bottomFrameList.Last();
                this.Remove(bottomFrame);
                bottomFrameList.Remove(bottomFrame);

                var level = HelperFrame.listLevel.Last();
                HelperFrame.listLevel.Remove(level);
            }
        }

        public void AddBottomFrame(BottomFrame bottomFrame)
        {
            bottomFrameList.Add(bottomFrame);
            this.Add(bottomFrame);
        }

        public BottomFrame createBottomFrame()
        {
            BottomFrame bottomFrame = new BottomFrame(this.Game,
                ConstantFrame.BOTTOM_PANEL_LEFT_BORDER, //x
                heightDisplay - ConstantFrame.BOTTOM_PANEL_HEIGHT * (bottomFrameList.Count + 1),
                widthDisplay - ConstantFrame.BOTTOM_PANEL_LEFT_BORDER - ConstantFrame.BOTTOM_PANEL_RIGHT_BORDER,// width
                ConstantFrame.BOTTOM_PANEL_HEIGHT, //height
                "",
                ConstantFrame.COLOR_BOTTOM_FRAME,
                bottomFrameList.Count
                );
            return bottomFrame;
        }
    }
}
