using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Graphics;
using Fusion.Mathematics;
using Fusion.UserInterface;
using GraphVis.Models.Frames;
using GraphVis.Models.Medicine;

namespace GraphVis.Helpers
{
    class HelperFrame
    {
        public static Frame AddButton(Frame parent, SpriteFont font, int x, int y, int w, int h, string text, FrameAnchor anchor, Action action, Color bcol, bool visibility = true)
        {
            var button = new Frame(parent.Game, x, y, w, h, text, Color.White)
            {
                Anchor = anchor,
                TextAlignment = Alignment.MiddleCenter,
                PaddingLeft = 0,
                Font = font,
                Visible = visibility,
            };

            if (action != null)
            {
                button.Click += (s, e) =>
                {
                    action();				
                };
            }
            button.StatusChanged += (s, e) =>
            {
                if (e.Status == FrameStatus.None) { button.BackColor = bcol; }
                if (e.Status == FrameStatus.Hovered) { button.BackColor = new Color(25, 71, 138, 255); }
                if (e.Status == FrameStatus.Pushed) { button.BackColor = new Color(99, 132, 181, 255); }
            };
            parent.Add(button);
            return button;
        }

        public static Frame AddMapButton(Frame parent, SpriteFont font, int x, int y, int w, int h, string img, string text, Action action)
        {
            var button = new Frame(parent.Game, x, y, w, h, text, Color.Zero)
            {
                Image = parent.Game.Content.Load<Texture2D>(img),
                ImageColor = Color.Orange,
                ImageMode = FrameImageMode.Stretched,
                Font = font,
                ImageOffsetX = 0,
                TextAlignment = Alignment.MiddleCenter,
                TextOffsetY = 0,
                TextOffsetX = 0,
                Anchor = FrameAnchor.Top | FrameAnchor.Left,
                ForeColor = Color.Red,
                TextEffect = TextEffect.Shadow
            };
            button.StatusChanged += (s, e) =>
            {
                if (e.Status == FrameStatus.None) { button.ImageColor = new Color(255, 255, 255, 128); button.ForeColor = Color.Black; button.BackColor = Color.Zero; }
                if (e.Status == FrameStatus.Hovered) { button.ImageColor = Color.Orange; button.ForeColor = Color.White; button.BackColor = Color.Zero; }
                if (e.Status == FrameStatus.Pushed) { button.ImageColor = Color.LightGray; button.ForeColor = Color.White; button.BackColor = Color.Zero; }
            };

            if (action != null)
            {
                button.Click += (s, e) => action();
            }
            parent.Add(button);
            return button;
        }


        public static void deleteButton(Frame frame, List<Frame> listButton, int stayElements)
        {
            while (listButton.Count > stayElements)
            {
                frame.Remove(listButton[stayElements]);
                listButton.RemoveAt(stayElements);
            }
        }

        public enum GroupType
        {
            DAY = 3,
            WEEK = 2,
            MONTH = 1
        }
        
        public static List<GroupType> listLevel = new List<GroupType>();

        public static bool isUpdatedLevel(GroupType typeGroup)
        {
            if (listLevel.Count > 0)
            {
                if ((int) listLevel.Last() < (int) typeGroup)
                {
                    listLevel.Add(typeGroup);
                }
                else
                {
                    if((int) listLevel.Last() == (int) typeGroup)
                        return false;
                }

            }
            else
            {
                listLevel.Add(typeGroup);
            }
            return true;
        }
        
        public static bool isTypeDay()
        {
            return listLevel.Last() == GroupType.DAY;
        }
    }
}
