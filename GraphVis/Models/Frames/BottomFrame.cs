﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.UserInterface;
using GraphVis.Helpers;
using GraphVis.Models.Medicine;

namespace GraphVis.Models.Frames
{
    class BottomFrame : BasicFrame
    {

        private List<Frame> listVisitButton;
        public int LineHeight;
        // setting for bottom panel
        private const int radiusMax = 75;
        private const int radiusMin = 15;
        public int PositionXForBottonPanel = 0;
        public int PositionYForBottonPanel = 0;
        private int level = 0;
        private int position = 0;

        public BottomFrame(Game game) : base(game)
        {
            
        }

        public BottomFrame(Game game, int x, int y, int w, int h, string text, Color backColor, int position) : base(game, x, y, w, h, text, backColor)
        {
            LineHeight = this.Font.LineHeight;
            listVisitButton = new List<Frame>();
            this.position = position;
        }

        public override void resizePanel()
        {
            base.initGlobalSize();
            this.Width = widthDisplay - ConstantFrame.BOTTOM_PANEL_LEFT_BORDER - ConstantFrame.BOTTOM_PANEL_RIGHT_BORDER;
            this.Y = heightDisplay - ConstantFrame.BOTTOM_PANEL_HEIGHT * position;
        }

        public void fillVisitData(Patient patient, Dictionary<string, List<Visit>> visitByDate, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient)
        {
            HelperFrameNew.deleteButton(this, listVisitButton, 0);
            addButtonToFrame(patient, visitByDate, actionForVisit, actionForPatient);
        }



        private void addButtonToFrame(Patient patient, Dictionary<string, List<Visit>> dateToVisits, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient)
        {
            int maxVisits = dateToVisits.Max(x => x.Value.Count);
            int elementNumber = 0;
            foreach (var dateVisits in dateToVisits)
            {
                var date = dateVisits.Key;
                var visits = dateVisits.Value;
                var radius = ((float)visits.Count / maxVisits) * (radiusMax - radiusMin) + radiusMin;
                int positionX = PositionXForBottonPanel + radiusMax*elementNumber; // depends of elementNumber 
                int positionYdate = this.Height - LineHeight;
                listVisitButton.Add(HelperFrameNew.AddButton(this, this.Font, positionX, positionYdate, radiusMax, LineHeight, date, FrameAnchor.Top | FrameAnchor.Left, () => { }, Color.Zero));
                // add image
                var buttonImage = HelperFrameNew.AddMapButton(this, this.Font, positionX, positionYdate + (radiusMax - (int)radius) / 2 - radiusMax, (int)radius, (int)radius, "node",
                    visits.Count.ToString(), () => { });
                buttonImage.MouseIn += (s, e) => actionForVisit(visits.ToArray());
                buttonImage.MouseOut += (s, e) => actionForPatient(patient, false);
                buttonImage.Click += (s, e) =>
                {
                    if (level< (int)HelperFrameNew.GroupType.DAY)
                    {
                        MainFrame mainFrame = (MainFrame) this.Parent;
                        HelperFrameNew.GroupType groupType = HelperFrameNew.GroupType.DAY;
                        Dictionary<string, List<Visit>> visitByDate = null;
                        groupType = getFrameVisitFromLevel(dateVisits.Value, groupType, out visitByDate);
                        mainFrame.RemoveBottomFrame(position);
                        BottomFrame bottomFrame = mainFrame.createBottomFrame((int)groupType);
                        bottomFrame.setPosition(HelperFrameNew.listLevel.Count);
                        mainFrame.AddBottomFrame(bottomFrame);
                        bottomFrame.fillVisitData(patient, visitByDate, actionForVisit, actionForPatient);
                    }      
                };
                listVisitButton.Add(buttonImage);
                elementNumber++;
            }
        }
        
        private bool isBlend(int countVisitByDate)
        {
            return this.Width > countVisitByDate * radiusMax;
        }

        public HelperFrameNew.GroupType getFrameVisitFromLevel(List<Visit> listVisit, HelperFrameNew.GroupType groupType, out Dictionary<string, List<Visit>> visitByDate)
        {
            visitByDate = HelperDate.getVisitsByDate(listVisit, "dd MMM");
            if (!isBlend(visitByDate.Count()))
            {
                groupType = HelperFrameNew.GroupType.WEEK;
                visitByDate = HelperDate.getVisitsByWeek(listVisit);
                if (!isBlend(visitByDate.Count()))
                {
                    groupType = HelperFrameNew.GroupType.MONTH;
                    visitByDate = HelperDate.getVisitsByDate(listVisit, "MMM yyyy");
                }
            }
            return groupType;
        }

        public void setLevel(int level)
        {
            this.level = level;
        }
        public void setPosition(int position)
        {
            this.position = position;
        }
    }
}
