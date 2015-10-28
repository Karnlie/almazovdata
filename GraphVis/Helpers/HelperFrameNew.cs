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
    class HelperFrameNew
    {
        public enum GroupType
        {
            DAY = 3,
            WEEK = 2,
            MONTH = 1
        }
        public static List<GroupType> listLevel = new List<GroupType>(); 

        // common setting field
        public static int widthDisplay = 0;
        public static int heightDisplay = 0;
        //setting for right panel
        public static int buttonWidthRightPanel = 200;
        public static int buttonHeightRightPanel = 0;
        // setting for bottom panel
        private const int radiusMax = 75;
        private const int radiusMin = 15;
        public static int PositionXForBottonPanel = 0;
        public static int PositionYForBottonPanel = 0;


        public static void initHelper(Game game)
        {
            widthDisplay = game.GraphicsDevice.DisplayBounds.Width;
            heightDisplay = game.GraphicsDevice.DisplayBounds.Height;
            PositionXForBottonPanel = widthDisplay* 5 / 100;
            PositionYForBottonPanel = widthDisplay * 10 / 100;
        }

        public static void resizePanel(Frame panel)
        {
            initHelper(panel.Game);
            panel.Width = widthDisplay;
            panel.Height = heightDisplay;
            for(int i=0; i<panel.Children.Count(); i++)
            {
                panel.Children.ElementAt(i).X = widthDisplay - buttonWidthRightPanel;
                panel.Children.ElementAt(i).Y = heightDisplay*1/100 + buttonHeightRightPanel*i;
            }
        }

        public static Frame CreatePanel(Game game, List<Frame> listPatientsButton, SpriteFont font)
        {
            Frame frame = new Frame(game, 0, 0, widthDisplay, heightDisplay, "", new Color(20, 20, 20, 0f)){};
            buttonHeightRightPanel = frame.Font.LineHeight;
            
            int positionX = widthDisplay - buttonWidthRightPanel;
            int positionY = heightDisplay*1/100;

            listPatientsButton.Add(
                AddButton(
                    frame,
                    font,
                    positionX,
                    positionY,
                    buttonWidthRightPanel,
                    buttonHeightRightPanel,
                    "",
                    FrameAnchor.Top | FrameAnchor.Left,
                    () => { }, Color.Zero)
            );
            listPatientsButton.Add(
                AddButton(
                    frame,
                    font,
                    frame.Width - buttonWidthRightPanel,
                    heightDisplay*1/100 + buttonHeightRightPanel,
                    buttonWidthRightPanel,
                    buttonHeightRightPanel,
                    "List of Patients",
                    FrameAnchor.Top | FrameAnchor.Left,
                    () => { }, Color.Zero)
                );
            return frame;
        }

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

        public static void CreatePatientList(Frame panel, SpriteFont font, HashSet<Patient> patients, List<Frame> listPatientsButton, Action<Patient, bool> action)
        {
            deleteButton(panel, listPatientsButton, 2);
            foreach (var patient in patients)
            {
                String s = patient.id;
                listPatientsButton.Add(
                    HelperFrame.AddButton(
                        panel,
                        font,
                        widthDisplay - buttonWidthRightPanel,
                        heightDisplay * 1 / 100 + buttonHeightRightPanel * listPatientsButton.Count,
                        buttonWidthRightPanel,
                        buttonHeightRightPanel, s,
                        FrameAnchor.Top | FrameAnchor.Left,
                        () => { action(patient, true); },
                        Color.Zero)
                    );
            }
        }

        public static void deleteButton(Frame frame, List<Frame> listPatientsButton, int stayElements)
        {
            while (listPatientsButton.Count > stayElements)
            {
                frame.Remove(listPatientsButton[stayElements]);
                listPatientsButton.RemoveAt(stayElements);
            }
        }


        public static void drawBottomPanel(Frame panel, Patient patient, SpriteFont font, List<Frame> listVisitButton, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient)
        {
            deleteButton(panel, listVisitButton, 0);
            GroupType typeGroup = GroupType.DAY;
            var game = panel.Game;

            Dictionary<string, List<Visit>> visitByMonth = null;
            Dictionary<string, List<Visit>> visitByWeek = null;
            Dictionary<string, List<Visit>> visitByDay = HelperDate.getVisitsByDate(patient, "dd MMM");
            if (!isBlend(visitByDay.Count()))
            {
                typeGroup = GroupType.WEEK;
                visitByWeek = HelperDate.getVisitsByWeek(patient);
                if (!isBlend(visitByWeek.Count()))
                {
                    typeGroup = GroupType.MONTH;
                    visitByMonth = HelperDate.getVisitsByDate(patient, "MMM yyyy");
                }
            }
            updateLevel(typeGroup);
            // сборка нижней панели
            switch (typeGroup)
            {
                case GroupType.DAY:
                {
                    addButtonToFrame(panel, font, patient, visitByDay, listVisitButton, actionForVisit, actionForPatient, false);
                }
                break;
                case GroupType.WEEK:
                {		
                    addButtonToFrame(panel, font, patient, visitByWeek, listVisitButton, actionForVisit, actionForPatient);
                }
                break;
                case GroupType.MONTH:
                {
                    addButtonToFrame(panel, font, patient, visitByMonth, listVisitButton, actionForVisit, actionForPatient);
                }
                break;
            }
        }

        private static void addButtonToFrame(Frame panel, SpriteFont font, Patient patient, Dictionary<string, List<Visit>> dateToVisits, List<Frame> listVisitButton, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient, bool addActionOnClick = true)
        {
            int maxVisits = dateToVisits.Max(x => x.Value.Count);
            int elementNumber = 0;
            foreach (var dateVisits in dateToVisits)
            {
                var date = dateVisits.Key;
                var visits = dateVisits.Value;
                var radius = ((float)visits.Count / maxVisits) * (radiusMax - radiusMin) + radiusMin;
                int positionX = PositionXForBottonPanel + radiusMax*elementNumber; // depends of elementNumber 
                // TODO: depends of level
                int positionY = PositionYForBottonPanel;
                listVisitButton.Add(AddButton(panel, font, positionX, positionY, radiusMax, buttonHeightRightPanel, date, FrameAnchor.Top | FrameAnchor.Left, () => { }, Color.Zero));
                // add image
                var buttonImage = AddMapButton(panel, font, positionX, positionY + (radiusMax - (int)radius) / 2, (int)radius, (int)radius, "node",
                    visits.Count.ToString(), () => { });
                buttonImage.MouseIn += (s, e) => actionForVisit(visits.ToArray());
                buttonImage.MouseOut += (s, e) => actionForPatient(patient, false);
                buttonImage.Click += (s, e) =>
                {
                    //if (addActionOnClick) drawTopBottomPanel(panel, patient, visits.ToList(), font, listVisitButton, actionForVisit, actionForPatient, offsetVertical + radiusMax + height, visitsButtonCount);
                };
                listVisitButton.Add(buttonImage);
                elementNumber++;
            }
        }


        private static void updateLevel(GroupType typeGroup)
        {
            if (listLevel.Count > 0)
            {
                if ((int)listLevel.Last() < (int)typeGroup)
                {
                    if (listLevel.Count > 2)
                    {
                        listLevel.RemoveAt(listLevel.Count - 1);
                    }
                    listLevel.Add(typeGroup);
                }

            }
            else
            {
                listLevel.Add(typeGroup);
            }
        }

        private static bool isBlend(int countVisitByDate)
        {
            return widthDisplay > countVisitByDate * radiusMax;
        }
    }
}
