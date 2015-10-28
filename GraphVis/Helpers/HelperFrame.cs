using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
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
        
		static Func<DateTime, int> weekProjector =
            d => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    d,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Sunday);

        //public static int levelDeep = 0;
        public static List<groupType> listLevel = new List<groupType>(); 

		public static Frame CreatePanel(Game game, List<Frame> listPatientsButton, SpriteFont font)
        {
            Frame frame = new Frame(game, 0, 0, game.GraphicsDevice.DisplayBounds.Width, game.GraphicsDevice.DisplayBounds.Height, "", new Color(20, 20, 20, 0f))
            {
            };

            int buttonHeight = frame.Font.LineHeight;
            int buttonWidth = 200;

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

            deleteButton(panel, listPatientsButton, 2);

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

        private const int LIMIT_ELEMENT_ROW = 30;
        private static int width;
        private const int height = 10;
        private static float horizStep;
        // радиусы шаров
        private const int radiusMin = 15;
        private const int radiusMax = 75;

        private static int x = 200;
        private static int y;
        private static int yNext;
        private static int maxVisits;

                public enum groupType
        {
            DAY=3,
            WEEK=2,
            MONTH=1
        }

        public static void drawBottomPanel(Frame panel, Patient patient, SpriteFont font, List<Frame> listVisitButton, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient)
        {
            deleteButton(panel, listVisitButton, 0);
            //x = 200;
            newX = 200;
			float trueWidth = 0;
			

            groupType typeGroup = groupType.DAY;
            var game = panel.Game;

            Dictionary<string, List<Visit>> visitByMonth = null;
            Dictionary<string, List<Visit>> visitByWeek = null;
            Dictionary<string, List<Visit>> visitByDay = getVisitsByDate(patient, "dd MMM");
            int countVisitByDate = visitByDay.Count();
            maxVisits = visitByDay.Max((k) => k.Value.Count());
            if (!isBlend(countVisitByDate, game.GraphicsDevice.DisplayBounds.Width - 400))
            {
                typeGroup = groupType.WEEK;
                visitByWeek = getVisitsByWeek(patient);
                countVisitByDate = visitByWeek.Count();
                maxVisits = visitByWeek.Max(visits => visits.Value.Count());
                if (!isBlend(countVisitByDate, game.GraphicsDevice.DisplayBounds.Width - 400))
                {
                    typeGroup = groupType.MONTH;
                    visitByMonth = getVisitsByDate(patient, "MMM yyyy");
                    countVisitByDate = visitByMonth.Count();
                    maxVisits = visitByMonth.Max(visits => visits.Value.Count());
                }
            }
            if (listLevel.Count > 0)
            {
                if ((int) listLevel.Last() < (int) typeGroup)
                {
                    if (listLevel.Count > 2)
                    {
                        listLevel.RemoveAt(listLevel.Count-1);
                    }
                    listLevel.Add(typeGroup);
                }
                    
            }
            else
            {
                listLevel.Add(typeGroup);
            }
			//int step = 5;
			
			
            x = 200; //(int)  (game.GraphicsDevice.DisplayBounds.Width  - trueWidth ) / 2;

            width = (game.GraphicsDevice.DisplayBounds.Width - 400) / countVisitByDate;
            horizStep = (game.GraphicsDevice.DisplayBounds.Width - 400) / (patient.visitList.Count() + (float)countVisitByDate - 1);
            y = game.GraphicsDevice.DisplayBounds.Height * 95 / 100;
            yNext = y - radiusMax - 2;
			int addedButtonsCount = 0;
			
            // сборка нижней панели
            switch (typeGroup)
            {
				case groupType.DAY: {
                       // printInfoLevel(panel, font, typeGroup.ToString(), y);
						addedButtonsCount = visitByDay.Count * 2; // +1 - info buttom

						foreach ( var visit in visitByDay ) {
							addButtonToFrame( panel, font, patient, visit.Value.ToArray(), listVisitButton, visit.Key.ToString(), actionForVisit, actionForPatient, addedButtonsCount, 0, false );
						}
					}
					break;
				case groupType.WEEK: {
                    //printInfoLevel(panel, font, typeGroup.ToString(), y);
                    addedButtonsCount = visitByWeek.Count * 2;	// +1 - info buttom			

						foreach ( var visit in visitByWeek ) {
							addButtonToFrame( panel, font, patient, visit.Value.ToArray(), listVisitButton, visit.Key.ToString(), actionForVisit, actionForPatient, addedButtonsCount );
						}
					}
					break;
				case groupType.MONTH: {
                    //printInfoLevel(panel, font, typeGroup.ToString(), y);
                    addedButtonsCount = visitByMonth.Count * 2 ;	// +1 - info buttom					
						foreach ( var visit in visitByMonth ) {
							addButtonToFrame( panel, font, patient, visit.Value.ToArray(), listVisitButton, visit.Key.ToString(), actionForVisit, actionForPatient, addedButtonsCount );
						}
					}
					break;
            }
        }

		 public static void drawTopBottomPanel(Frame panel, Patient patient, List<Visit> allVisits, SpriteFont font, List<Frame> listVisitButton, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient, int offset, int buttonsCountBeforeThisFunctionCall)
        {
            deleteButton(panel, listVisitButton, buttonsCountBeforeThisFunctionCall);
            //x = 200;
		   
			float trueWidth = 0;

            groupType typeGroup = groupType.DAY;
            var game = panel.Game;

            Dictionary<string, List<Visit>> visitByMonth = null;
            Dictionary<string, List<Visit>> visitByWeek = null;
            Dictionary<string, List<Visit>> visitByDay = allVisits.GroupBy(visit => visit.date.ToString("dd MMM")).ToDictionary(g => g.Key, g => g.ToList());

            int countVisitByDate = visitByDay.Count();
            maxVisits = visitByDay.Max((k) => k.Value.Count());
		     if (!isBlend(countVisitByDate, game.GraphicsDevice.DisplayBounds.Width - 400))
		     {
		         typeGroup = groupType.WEEK;
		         visitByWeek = allVisits.GroupBy(p => weekProjector(p.date).ToString())
		             .ToDictionary(g => g.Key, g => g.ToList());
		         ;
		         countVisitByDate = visitByWeek.Count();
		         maxVisits = visitByWeek.Max(visits => visits.Value.Count());

		         // if (!isBlend(countVisitByDate, game.GraphicsDevice.DisplayBounds.Width - 400))
		         //{
		         //	typeGroup = groupType.MONTH;
		         //	visitByMonth = allVisits.GroupBy(visit => visit.date.ToString("MMM yyyy")).ToDictionary(g => g.Key, g => g.ToList());
		         //	countVisitByDate = visitByMonth.Count();
		         //	maxVisits = visitByMonth.Max(visits => visits.Value.Count());
		         //}
		     }
		     if ((int) listLevel.Last() < (int) typeGroup)
		         listLevel.Add(typeGroup);
		     else
		     {
		         if (((int) listLevel.Last() - (int) typeGroup == 1) && listLevel.Count > 2)
		             listLevel.RemoveAt(listLevel.Count - 1);
		         else
		         {
		             if (((int) listLevel.Last() - (int) typeGroup == 0) && listLevel.Count == 2)
		             {
                         listLevel.RemoveAt(listLevel.Count - 1);
                         listLevel.Add(typeGroup);
		             }
                         
		         }
		     }
             

             
			
            x = 200; //(int)  (game.GraphicsDevice.DisplayBounds.Width  - trueWidth ) / 2;

            width = (game.GraphicsDevice.DisplayBounds.Width - 400) / countVisitByDate;
            
			y		= game.GraphicsDevice.DisplayBounds.Height * 95 / 100 - 9*offset/10;
            yNext	= game.GraphicsDevice.DisplayBounds.Height * 85 / 100 - 9*offset/10;
			int addedButtonsCount = buttonsCountBeforeThisFunctionCall;
			
            // сборка нижней панели
            var offsetForButton = offset + 15;
            switch (typeGroup)
            {
				case groupType.DAY:
                {
                    //printInfoLevel(panel, font, typeGroup.ToString(), offsetForButton + radiusMax + height + 5);
                    addedButtonsCount += visitByDay.Count * 2 + 1;	// +1 - info buttom					
					foreach ( var visit in visitByDay ) {
                        addButtonToFrame(panel, font, patient, visit.Value.ToArray(), listVisitButton, visit.Key.ToString(), actionForVisit, actionForPatient, addedButtonsCount, offsetForButton, false);
					}
				}
				break;
				case groupType.WEEK: 
                {
                    //printInfoLevel(panel, font, typeGroup.ToString(), offsetForButton + radiusMax + height + 5);
                    addedButtonsCount += visitByWeek.Count * 2 + 1;	// +1 - info buttom		
					foreach ( var visit in visitByWeek ) {
                        addButtonToFrame(panel, font, patient, visit.Value.ToArray(), listVisitButton, visit.Key.ToString(), actionForVisit, actionForPatient, addedButtonsCount, offsetForButton);
					}
				}
				break;
				//case groupType.MONTH:
				//	foreach (var visit in visitByMonth)
				//	{
				//		addButtonToFrame(panel, font, patient, visit.Value.ToArray(), listVisitButton, visit.Key.ToString(), actionForVisit, actionForPatient);
				//	}
				//	break;
            }
        }

        private static int newX = 200;
        

        private static void addButtonToFrame(Frame panel, SpriteFont font, Patient patient, Visit[] visits, List<Frame> listVisitButton, String text, Action<Visit[]> actionForVisit, Action<Patient, bool> actionForPatient, int  visitsButtonCount, int offsetVertical = 0, bool addActionOnClick = true)
        {
            var radius = ( (float) visits.Length / maxVisits) * (radiusMax - radiusMin) + radiusMin;
			var offset = (width - (int)radius) / 2;
            listVisitButton.Add(HelperFrame.AddButton(panel, font, x, y, width, height, text, FrameAnchor.Top | FrameAnchor.Left, () => { }, Color.Zero));
            var button = HelperFrame.AddMapButton(panel, font, x + offset, yNext + (radiusMax - (int)radius) / 2, (int)radius, (int)radius, "node",
                visits.Length.ToString(), () => { });

			//var

            button.MouseIn += (s, e) => actionForVisit(visits);
            button.MouseOut += (s, e) => actionForPatient(patient, false);
            button.Click += (s, e) => {
				if(addActionOnClick) drawTopBottomPanel(panel, patient, visits.ToList(), font, listVisitButton, actionForVisit, actionForPatient, offsetVertical + radiusMax + height, visitsButtonCount); 
			};
				Console.WriteLine(x);

            listVisitButton.Add(button);
            x += radiusMax;
//            int newY = panel.Game.GraphicsDevice.DisplayBounds.Height * 95 / 100;
//            var radius = ((float)visits.Length / maxVisits) * (radiusMax - radiusMin) + radiusMin;
//            var offset = (width - (int)radius) / 2;
//            listVisitButton.Add(HelperFrame.AddButton(panel, font, newX, newY, width, height, text, FrameAnchor.Top | FrameAnchor.Left, () => { }, Color.Zero));
//            var button = HelperFrame.AddMapButton(panel, font, newX + offset, newY - (radiusMax + (int)radius) / 2, (int)radius, (int)radius, "node",
//              visits.Length.ToString(), () => { });
//
//			//var
//
//            button.MouseIn += (s, e) => actionForVisit(visits);
//            button.MouseOut += (s, e) => actionForPatient(patient, false);
//            button.Click += (s, e) => {
//				if(addActionOnClick) drawTopBottomPanel(panel, patient, visits.ToList(), font, listVisitButton, actionForVisit, actionForPatient, offsetVertical + radiusMax + height, visitsButtonCount); 
//			};
//				//Console.WriteLine(x);
//
//            listVisitButton.Add(button);
//            newX += radiusMax;

        }

        private static Dictionary<string, List<Visit>> getVisitsByDate(Patient patient, String datePattern)
        {
            var visitByDate = patient.visitList.GroupBy(visit => visit.date.ToString(datePattern)).ToDictionary(g => g.Key, g => g.ToList());
            return visitByDate;
        }

        private static Dictionary <string, List<Visit>> getVisitsByWeek(Patient patient)
        {
            Func<DateTime, int> weekProjector =
            d => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    d,
                    CalendarWeekRule.FirstFourDayWeek,
                    DayOfWeek.Sunday);
            var visits = patient.visitList.GroupBy(p => weekProjector(p.date).ToString()).ToDictionary(g => g.Key, g => g.ToList());
            return visits;
        }

        private static bool isBlend(int countVisitByDate, int width)
        {
            return  width > countVisitByDate * radiusMax;
        }

        public static void deleteButton(Frame frame, List<Frame> listPatientsButton, int stayElements)
        {
            while (listPatientsButton.Count > stayElements)
            {
                frame.Remove(listPatientsButton[stayElements]);
                listPatientsButton.RemoveAt(stayElements);
            }
        }
//        public static void printInfoLevel(Frame panel, SpriteFont font, String text, int offset)
//        {
//            int x = 100 - text.Length * 5;
//            int y = offset;
//            AddButton(panel, font, x, y, 5, 2, text, FrameAnchor.All, () => { }, Color.Zero);
////            var sb = game.GetService<SpriteBatch>();
////            sb.Begin();
////            font.DrawString(sb, text, x, y, Color.White);
////            sb.End();
//        }
    }
}
