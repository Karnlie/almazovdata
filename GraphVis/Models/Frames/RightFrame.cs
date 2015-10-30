using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.UserInterface;
using GraphVis.Helpers;
using GraphVis.Models.Frames;
using GraphVis.Models.Medicine;


namespace GraphVis.Models
{
    class RightFrame : BasicFrame
    {
        //setting for right panel
        public int buttonWidthRightPanel = ConstantFrame.RIGHT_PANEL_WIDTH;
        public int buttonHeightRightPanel;
        private List<Frame> listPatientsButton;

        public RightFrame(Game game) : base(game)
        {
        }

        public RightFrame(Game game, int x, int y, int w, int h, string text, Color backColor)
            : base(game, x, y, w, h, text, backColor)
        {
            listPatientsButton = new List<Frame>();
            buttonHeightRightPanel = this.Font.LineHeight;
            initRightPanel();
        }

        public void initRightPanel()
        {
            listPatientsButton = new List<Fusion.UserInterface.Frame>();
            int positionX = 0;
            int positionY = this.Height * 1 / 100;

            listPatientsButton.Add(
                HelperFrame.AddButton(
                    this,
                    this.Font,
                    positionX,
                    positionY,
                    buttonWidthRightPanel,
                    buttonHeightRightPanel,
                    "",
                    FrameAnchor.Top | FrameAnchor.Left,
                    () => { }, Color.Zero)
            );
            listPatientsButton.Add(
                HelperFrame.AddButton(
                    this,
                    this.Font,
                //TODO
                    positionX,
                    positionY + buttonHeightRightPanel,
                    buttonWidthRightPanel,
                    buttonHeightRightPanel,
                    "List of Patients",
                    FrameAnchor.Top | FrameAnchor.Left,
                    () => { }, Color.Zero)
                );
        }

        public override void resizePanel()
        {
            base.initGlobalSize();
            this.X = widthDisplay - ConstantFrame.RIGHT_PANEL_WIDTH;
            this.Height = heightDisplay;
        }

        public void fillPatientList(HashSet<Patient> patients, Action<Patient, bool> action)
        {
            HelperFrame.deleteButton(this, listPatientsButton, 2);
            
            foreach (var patient in patients)
            {
                String s = patient.id;
                listPatientsButton.Add(
                    HelperFrame.AddButton(
                        this,
                        this.Font,
                        0,
                        heightDisplay * 1 / 100 + buttonHeightRightPanel * listPatientsButton.Count,
                        buttonWidthRightPanel,
                        buttonHeightRightPanel, s,
                        FrameAnchor.Top | FrameAnchor.Left,
                        () => { action(patient, true); },
                        Color.Zero)
                    );
            }
        }

        public void updatePanelScroll(int verticalOffset, Fusion.Input.InputDevice.MouseScrollEventArgs e)
        {
            int maxwheel = listPatientsButton.ElementAt(listPatientsButton.Count() - 1).Y + this.Font.LineHeight;
            float bottom_boundary = maxwheel - this.Height;

            if ((listPatientsButton.ElementAt(0).Y < 0 && e.WheelDelta > 0) || (bottom_boundary >= 0 && e.WheelDelta < 0))
            {

                verticalOffset += e.WheelDelta;

                int y = verticalOffset + 5;

                foreach (var child in listPatientsButton)
                {
                    child.Y = y;

                    y += child.Height + 10;
                }
            }
        }
    }
}
