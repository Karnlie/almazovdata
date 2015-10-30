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

//        public static void drawBottomPanel(Frame panel, Patient patient, SpriteFont font, List<Frame> listVisitButton, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient)
//        {
//            deleteButton(panel, listVisitButton, 0);
//            MainFrame.GroupType typeGroup = MainFrame.GroupType.DAY;
//            var game = panel.Game;
//
//            Dictionary<string, List<Visit>> visitByDate = HelperDate.getVisitsByDate(patient, "dd MMM");
//            if (!isBlend(visitByDate.Count()))
//            {
//                typeGroup = MainFrame.GroupType.WEEK;
//                visitByDate = HelperDate.getVisitsByWeek(patient);
//                if (!isBlend(visitByDate.Count()))
//                {
//                    typeGroup = MainFrame.GroupType.MONTH;
//                    visitByDate = HelperDate.getVisitsByDate(patient, "MMM yyyy");
//                }
//            }
//            updateLevel(typeGroup);
//            // сборка нижней панели
//            addButtonToFrame(panel, font, patient, visitByDate, listVisitButton, actionForVisit, actionForPatient, false);
//        }

//        private static void addButtonToFrame(Frame panel, SpriteFont font, Patient patient, Dictionary<string, List<Visit>> dateToVisits, List<Frame> listVisitButton, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient, bool addActionOnClick = true)
//        {
//            int maxVisits = dateToVisits.Max(x => x.Value.Count);
//            int elementNumber = 0;
//            foreach (var dateVisits in dateToVisits)
//            {
//                var date = dateVisits.Key;
//                var visits = dateVisits.Value;
//                var radius = ((float)visits.Count / maxVisits) * (radiusMax - radiusMin) + radiusMin;
//                int positionX = PositionXForBottonPanel + radiusMax*elementNumber; // depends of elementNumber 
//                // TODO: depends of level
//                int positionY = PositionYForBottonPanel;
//                listVisitButton.Add(AddButton(panel, font, positionX, positionY, radiusMax, buttonHeightRightPanel, date, FrameAnchor.Top | FrameAnchor.Left, () => { }, Color.Zero));
//                // add image
//                var buttonImage = AddMapButton(panel, font, positionX, positionY + (radiusMax - (int)radius) / 2 - radiusMax, (int)radius, (int)radius, "node",
//                    visits.Count.ToString(), () => { });
//                buttonImage.MouseIn += (s, e) => actionForVisit(visits.ToArray());
//                buttonImage.MouseOut += (s, e) => actionForPatient(patient, false);
//                buttonImage.Click += (s, e) =>
//                {
//                    //if (addActionOnClick) drawTopBottomPanel(panel, patient, visits.ToList(), font, listVisitButton, actionForVisit, actionForPatient, offsetVertical + radiusMax + height, visitsButtonCount);
//                };
//                listVisitButton.Add(buttonImage);
//                elementNumber++;
//            }
//        }
//
//
//        private static void updateLevel(MainFrame.GroupType typeGroup)
//        {
//            if (MainFrame.listLevel.Count > 0)
//            {
//                if ((int) MainFrame.listLevel.Last() < (int)typeGroup)
//                {
//                    if (MainFrame.listLevel.Count > 2)
//                    {
//                        MainFrame.listLevel.RemoveAt(MainFrame.listLevel.Count - 1);
//                    }
//                    MainFrame.listLevel.Add(typeGroup);
//                }
//
//            }
//            else
//            {
//                MainFrame.listLevel.Add(typeGroup);
//            }
//        }
//
//        private static bool isBlend(int countVisitByDate)
//        {
//            return widthDisplay > countVisitByDate * radiusMax;
//        }


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
//                    if (listLevel.Count > 2)
//                    {
//                        listLevel.RemoveAt(listLevel.Count - 1);
//                    }
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


//        public void fun()
//        {
//            if ((int)listLevel.Last() < (int)typeGroup)
//                listLevel.Add(typeGroup);
//            else
//            {
//                if (((int)listLevel.Last() - (int)typeGroup == 1) && listLevel.Count > 2)
//                    listLevel.RemoveAt(listLevel.Count - 1);
//                else
//                {
//                    if (((int)listLevel.Last() - (int)typeGroup == 0) && listLevel.Count == 2)
//                    {
//                        listLevel.RemoveAt(listLevel.Count - 1);
//                        listLevel.Add(typeGroup);
//                    }
//
//                }
//            }
//        }
    }
}
