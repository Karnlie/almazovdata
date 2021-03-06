﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Mathematics;

namespace GraphVis.Models.Frames
{
    public class ConstantFrame
    {
        // right panel
        public const int RIGHT_PANEL_WIDTH = 200;
        // bottom panel
        public const int BOTTOM_PANEL_HEIGHT = 100;
        public const int BOTTOM_PANEL_RIGHT_BORDER = 200;
        public const int BOTTOM_PANEL_LEFT_BORDER = 200;

        public const int RADIUS_MAX = 75;
        public const int RADIUS_MIN = 15;

        public static readonly Color COLOR_RIGHT_FRAME = Color.Zero;
        public static readonly Color COLOR_BOTTOM_FRAME = Color.Zero;
    }
}
