using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Graphics;
using Fusion.Mathematics;
using Fusion.UserInterface;
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
            Color testcol = new Color(51, 51, 51, 255);

            if (action != null)
            {
                button.Click += (s, e) =>
                {
                    action();
                    //if (bcol != testcol) {bcol = (bcol == Color.Zero) ? (new Color(62, 106, 181, 255)) : (Color.Zero);}					
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
                ImageOffsetX = 45,
                //ImageOffsetY = -20,
                TextAlignment = Alignment.MiddleCenter,
                TextOffsetY = 0,
                TextOffsetX = 0,
                Anchor = FrameAnchor.Top | FrameAnchor.Right,
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

        public static void CreatePatientList(Frame panel, SpriteFont font, HashSet<Patient> patients, List<Frame>listPatientsButton, Action<Patient, bool> action)
        {

            int buttonHeight = panel.Font.LineHeight;
            int buttonWidth = 150;
            
            while (listPatientsButton.Count > 2)
            {
                panel.Remove(listPatientsButton[2]);
                listPatientsButton.RemoveAt(2);
            }

            int width = panel.Game.GraphicsDevice.DisplayBounds.Width;
            foreach (var patient in patients)
            {
                String s = patient.id;
                listPatientsButton.Add(
                    HelperFrame.AddButton(
                        panel,
                        font,
                        width - buttonWidth,
                        0,
                        buttonWidth,
                        buttonHeight, s,
                        FrameAnchor.Top | FrameAnchor.Right,
                        () => { action(patient, true); },
                        Color.Zero)
                    );
            }
            int verticalOffset = 0;
            int y = verticalOffset + 2;
            foreach (var child in listPatientsButton)
            {
                child.Y = y;
                y += child.Height + 4;
            }

        }

        public static Frame CreatePanel(Game game, List<Frame> listPatientsButton, SpriteFont font )
        {
            Frame frame = new Frame(game, 0, 0, game.GraphicsDevice.DisplayBounds.Width, game.GraphicsDevice.DisplayBounds.Height, "", new Color(20, 20, 20, 0f))
            {
            };

            int buttonHeight = frame.Font.LineHeight;
            int buttonWidth = 150;

            Frame doctor = new Frame(game, frame.Width - buttonWidth, 0, buttonWidth, buttonHeight, "", Color.Zero)
            {
                TextAlignment = Alignment.MiddleCenter,
            };
            listPatientsButton.Add(doctor);

            frame.Add(doctor);

            listPatientsButton.Add(
                HelperFrame.AddButton(
                    frame,
                    font,
                    frame.Width - buttonWidth,
                    doctor.Height + 10,
                    buttonWidth,
                    buttonHeight,
                    "List of Patients",
                    FrameAnchor.Top | FrameAnchor.Left,
                    null, Color.Zero)
                );
            return frame;
        }

        public static void drawBottomPanel(Frame panel, Patient patient, SpriteFont font, List<Frame> listVisitButton, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient)
        {
            var game = panel.Game;
            var visitByDate = patient.visitList.GroupBy(visit => visit.date.ToString("dd MMM yyyy"));
            int countVisitByDate = visitByDate.Count();
            if (countVisitByDate > 25)
            {
                visitByDate = patient.visitList.GroupBy(visit => visit.date.ToString("dd MMM"));
                countVisitByDate = visitByDate.Count();
            }
            var width = (game.GraphicsDevice.DisplayBounds.Width - 400) / countVisitByDate;
            var height = 10;
            var maxVisits = visitByDate.Max(visits => visits.Count());
            float horizStep = (game.GraphicsDevice.DisplayBounds.Width - 400) / (patient.visitList.Count() + visitByDate.Count() - 1);
			int step = 5;

            // радиусы шаров
            var radiusMin = 15;
            var radiusMax = 75;

			float trueWidth = 0;
			foreach (var visit in visitByDate)
            {
                trueWidth +=  (((float)visit.Count()) / maxVisits) * (radiusMax - radiusMin) + radiusMin + step + 1;
			}
            int x = (int)  (game.GraphicsDevice.DisplayBounds.Width  - trueWidth ) / 2;
            int y = game.GraphicsDevice.DisplayBounds.Height * 95 / 100;

            int yNext = game.GraphicsDevice.DisplayBounds.Height * 85 / 100;

            // сборка нижней панели
            foreach (var visit in visitByDate)
            {
                var radius = (((float)visit.Count()) / maxVisits) * (radiusMax - radiusMin) + radiusMin;
				Console.WriteLine(x);

                listVisitButton.Add(HelperFrame.AddButton(panel, font, x, y, (int)radius, height, visit.Key.ToString(), FrameAnchor.Top | FrameAnchor.Left, () => { }, Color.Zero));
                var button = HelperFrame.AddMapButton(panel, font, x, yNext + (radiusMax - (int)radius) / 2, (int)radius, (int)radius, "node",
                    visit.Count().ToString(), () => { });

                button.MouseIn += (s, e) => actionForVisit(visit.ToArray());
                button.MouseOut += (s, e) => actionForPatient(patient, false);

                listVisitButton.Add(button);
                x += (int)radius + step;
            }
        }
    }
}
