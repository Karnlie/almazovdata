using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;
using Fusion.UserInterface;
using GraphVis.HelperFiles;
using GraphVis.Models.Medicine;

namespace GraphVis
{
	public class GraphVis : Game
	{
		Random rnd = new Random();
		int selectedNodeIndex;

        List<Frame> listPatientsButton = new List<Frame>();
        List<Frame> listVisitButton = new List<Frame>();
        Dictionary<int, int> nodeId_NodeNumber = new Dictionary<int, int>();

		SpriteFont font1;
	    private SpriteFont labelFontNormal;

		Vector3 selectedNodePos;
		bool isSelected;
		Tuple<Point, Point> dragFrame;
		int time;
		StanfordNetwork stNet;
		Frame rightPanel;
		Dictionary<Doctor, HashSet<Patient>> doctorToPatients;


		/// <summary>
		/// GraphVis constructor
		/// </summary>
		public GraphVis()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService(new SpriteBatch(this), false, false, 0, 0);
			AddService(new DebugStrings(this), true, true, 9999, 9999);
			AddService(new DebugRender(this), true, true, 9998, 9998);

			//	add here additional services :
			AddService(new Camera(this), true, false, 9997, 9997);
			//		AddService(new OrbitCamera(this), true, false, 9996, 9996 );
			AddService(new GreatCircleCamera(this), true, false, 9995, 9995);
			AddService(new GraphSystem(this), true, true, 9994, 9994);
			AddService( new UserInterface( this, @"headerFont" ), true, true, 10000, 10000 );

			

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += Game_Exiting;
		}


		/// <summary>
		/// Initializes game :
		/// </summary>
		protected override void Initialize()
		{
			//	initialize services :
			base.Initialize();
			font1 = Content.Load<SpriteFont>("headerFont");
            labelFontNormal = Content.Load<SpriteFont>("labelFontNormal");
			var cam = GetService<Camera>();
			cam.Config.FreeCamEnabled = false;
			selectedNodeIndex = 0;
			selectedNodePos = new Vector3();
			isSelected = false;
			time = 0;

			//add gui interface
			var ui = GetService<UserInterface>();
            CreatePanel();
            ui.RootFrame = rightPanel;
			ui.SettleControls();
			GraphicsDevice.DisplayBoundsChanged += (s, e) =>
			{
				rightPanel.Height = GraphicsDevice.DisplayBounds.Height;
                rightPanel.Width = GraphicsDevice.DisplayBounds.Width;
				foreach (var child in listPatientsButton){
					child.X = rightPanel.Width - 200;
				}
            };

			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;
			InputDevice.MouseScroll += inputDevice_MouseScroll;
			//	load content & create graphics and audio resources here:
		}


		int verticalOffset = 0;
		/// <summary>
		/// Disposes game
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				//	dispose disposable stuff here
				//	Do NOT dispose objects loaded using ContentManager.
			}
			base.Dispose(disposing);
		}
				
		void inputDevice_MouseScroll(object sender, Fusion.Input.InputDevice.MouseScrollEventArgs e)
		{
			if (rightPanel.GetBorderedRectangle().Contains(InputDevice.MousePosition) && e.WheelDelta != 0){
				//int maxwheel = rightPanel.Children.ElementAt(rightPanel.Children.Count() - 1).Y + rightPanel.Font.LineHeight;
				//float bottom_boundary = maxwheel - rightPanel.Height;
				//float top_boundary    = rightPanel.Y - rightPanel.Children.ElementAt(0).Y;
				//if ( bottom_boundary >= 0 && e.WheelDelta < 0) rightPanel.ForEachChildren( x => x.Y += e.WheelDelta);
				//if ( top_boundary >= 0 && e.WheelDelta > 0) 
				
				verticalOffset += e.WheelDelta;

				int y = verticalOffset + 5;

				foreach (var child in listPatientsButton){
					child.Y = y;

					y += child.Height + 10;
				}
				//rightPanel.ForEachChildren( x => x.Y =);
				//Console.WriteLine(e.WheelDelta);
			} else {
				var cam = GetService<GreatCircleCamera>();
				cam.DollyZoom(e.WheelDelta / 60.0f);
			}
		}


		/// <summary>
		/// Handle keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
		{
			if (e.Key == Keys.F1)
			{
				DevCon.Show(this);
			}

			if (e.Key == Keys.F5)
			{
				Reload();
			}

			if (e.Key == Keys.F12)
			{
				GraphicsDevice.Screenshot();
			}

			if (e.Key == Keys.Escape)
			{
				Exit();
			}
			if (e.Key == Keys.P)
			{
				GetService<GraphSystem>().Pause();
			}
			//if (e.Key == Keys.I)
			//{
			//	GetService<GraphSystem>().SwitchStepMode();
			//}
			//if (e.Key == Keys.M)
			//{
			//	Graph graph = Graph.MakeTree( 256, 2 );				
			//	float[] centralities = new float[graph.NodeCount];
			//	float maxC = graph.GetCentrality(0);
			//	float minC = maxC;
			//	for (int i = 0; i < graph.NodeCount; ++i)
			//	{
			//		centralities[i] = graph.GetCentrality(i);
			//		maxC = maxC < centralities[i] ? centralities[i] : maxC;
			//		minC = minC > centralities[i] ? centralities[i] : minC;
			//		Log.Message( ":{0}", i );
			//	}

			//	float range = maxC - minC;
			//	for (int i = 0; i < graph.NodeCount; ++i)
			//	{
			//		centralities[i] -= minC;
			//		centralities[i] /= range;
			//		centralities[i] *= 0.9f;
			//		centralities[i] += 0.1f;
			//		var color = new Color(0.6f, 0.3f, centralities[i], 1.0f);
			////		var color = new Color(centralities[i]); // B/W
			//		graph.Nodes[i] = new BaseNode(5.0f, color);
			//	}
			//	GetService<GraphSystem>().AddGraph(graph);
			//}
			if (e.Key == Keys.Q)
			{
				Graph graph = GetService<GraphSystem>().GetGraph();
				graph.WriteToFile("graph.gr");
				Log.Message("Graph saved to file");
			}
			if (e.Key == Keys.F) // focus on a node
			{
				var cam = GetService<GreatCircleCamera>();
				var pSys = GetService<GraphSystem>();
				if (!isSelected)
				{
					//				cam.CenterOfOrbit = new Vector3(0, 0, 0);
				}
				else
				{
					//				cam.CenterOfOrbit = pSys.GetGraph().Nodes[selectedNodeIndex].Position;
					pSys.Focus(selectedNodeIndex);
				}
			}
			if (e.Key == Keys.G) // collapse random edge
			{
				var pSys = GetService<GraphSystem>();
				Graph graph = pSys.GetGraph();
				int edge = rnd.Next(graph.EdgeCount);
				graph.CollapseEdge(edge);
				pSys.UpdateGraph(graph);
			}
			if (e.Key == Keys.LeftButton)
			{
				var pSys = GetService<GraphSystem>();
				Point cursor = InputDevice.MousePosition;
				Vector3 nodePosition = new Vector3();
				int selNode = 0;
				if (pSys.ClickNode(cursor, StereoEye.Mono, 0.025f, out selNode))
				{
					selectedNodeIndex = selNode;
					isSelected = true;
					selectedNodePos = nodePosition;
					if (stNet != null && selectedNodeIndex < stNet.NodeCount)
					{
						Console.WriteLine(((NodeWithText)stNet.Nodes[selectedNodeIndex]).Text);
					}
					
					Doctor doctor = doctorToPatients.Keys.First(x => x.id == selectedNodeIndex );
					HashSet<Patient> patients;
					doctorToPatients.TryGetValue( doctor, out patients);
					CreatePatientList(patients);
				}
				else
				{
					isSelected = false;
				}
			}

			

		}



		/// <summary>
		/// Saves configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Game_Exiting(object sender, EventArgs e)
		{
			SaveConfiguration();
		}



		/// <summary>
		/// Updates game
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update(GameTime gameTime)
		{
			var ds = GetService<DebugStrings>();
			var debRen = GetService<DebugRender>();

			var graphSys = GetService<GraphSystem>();

			if (InputDevice.IsKeyDown(Keys.X))
			{
				Graph graph = Graph.MakeTree(4096, 40);
				//		Graph<BaseNode> graph = Graph<BaseNode>.MakeRing( 512 );
				graphSys.AddGraph(graph);
			}

			if (InputDevice.IsKeyDown(Keys.Z))
			{
				//				StanfordNetwork graph = new StanfordNetwork();
				stNet = new StanfordNetwork();
                // TODO: add path file
                //stNet.ReadFromFile("../../../../Graf.txt");
                Dictionary<String, int> dict = new Dictionary<String, int>();
			    doctorToPatients = new Dictionary<Doctor, HashSet<Patient>>();

               // ReaderFiles.ReadFromFileDoctorList("../../../../Doctordata.txt", dict);
                ReaderFiles.ReadFromFilePatientData("../../../../almazovdata", dict, doctorToPatients);
                stNet.BuildGraphFromDictinary(doctorToPatients);
				graphSys.AddGraph(stNet);
				// graph file names:
				// CA-GrQc small
				// CA-HepTh middle
				// CA-CondMat large

				//CitationGraph graph = new CitationGraph();
				//graph.ReadFromFile("../../../../articles_data/idx_edges.txt");
				//graphSys.AddGraph(graph);
			}


			if (InputDevice.IsKeyDown(Keys.V))
			{
				var protGraph = new ProteinGraph();
				protGraph.ReadFromFile("../../../../signalling_table.csv");

				// add categories of nodes with different localization:
				// category 1 (membrane):
				graphSys.AddCategory(new List<int> { 0, 1, 2, 20 }, new Vector3(0, 0, 0), 700);

				// category 2 (cytoplasma):
				graphSys.AddCategory(new List<int> { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new Vector3(0, 0, 0), 300);

				// category 3 (nucleus):
				graphSys.AddCategory(new List<int> { 8, 12, 13, 14, 15, 16, 17, 18, 19 }, new Vector3(0, 0, 0), 100);

				graphSys.AddGraph(protGraph);

			}

			ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
			ds.Add(Color.Orange, "F1   - show developer console");
			ds.Add(Color.Orange, "F5   - build content and reload textures");
			ds.Add(Color.Orange, "F12  - make screenshot");
			ds.Add(Color.Orange, "ESC  - exit");
			ds.Add(Color.Orange, "Press Z or X to load graph");
			ds.Add(Color.Orange, "Press M to load painted graph (SLOW!)");
			ds.Add(Color.Orange, "Press P to pause/unpause");

			//			ds.Add(Color.Orange, "Press I to switch to manual mode");

			base.Update(gameTime);

		}





		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw(GameTime gameTime, StereoEye stereoEye)
		{
			base.Draw(gameTime, stereoEye);

			//		time += gameTime.Elapsed.Milliseconds;

			var cam = GetService<GreatCircleCamera>();
			var dr = GetService<DebugRender>();
			var pSys = GetService<GraphSystem>();
			var sb = GetService<SpriteBatch>();
			dr.View = cam.GetViewMatrix(stereoEye);
			dr.Projection = cam.GetProjectionMatrix(stereoEye);

            int width = GraphicsDevice.DisplayBounds.Width;
            int height = GraphicsDevice.DisplayBounds.Height;
			//		dr.DrawGrid(20);
			var ds = GetService<DebugStrings>();
			if (isSelected)
			{
                Doctor doctor = doctorToPatients.Keys.First(x => x.id == selectedNodeIndex );
				//HashSet<Patient> patients;
				//doctorToPatients.TryGetValue( doctor, out patients);
				//CreatePatientList(patients);
				ds.Add(Color.Orange, "Selected node # " + selectedNodeIndex);
				String info = "";
                dict = new Dictionary<int, string>();

			   
				sb.Begin();
				font1.DrawString( sb, "Id # " + selectedNodeIndex + ": " + doctor.fio + ": " + doctor.category, 44, height - 50 , Color.White );
				sb.End();
                Console.WriteLine(); //вывод имен файлов
				
				pSys.Select(selectedNodeIndex);

				//вывод в панель пациентов и врача
				rightPanel.Children.ElementAt( 0 ).Text = "Id # " + selectedNodeIndex;

			}
			else
			{
				ds.Add(Color.Orange, "No selection");
				//pSys.Deselect();
			}



		}

		public Dictionary<int, string> dict { get; set; }

		
		void CreatePanel() {
            rightPanel = new Frame(this, 0, 0, GraphicsDevice.DisplayBounds.Width, GraphicsDevice.DisplayBounds.Height, "", new Color(20, 20, 20, 0f))
            {
				//ClippingMode = ClippingMode.ClipByFrame,
			};

			int buttonHeight    = rightPanel.Font.LineHeight;
			int buttonWidth     = 200;

			Frame doctor = new Frame( this, rightPanel.Width - buttonWidth, 0, rightPanel.Width, buttonHeight, "", Color.Zero ) {
				//Anchor = FrameAnchor.Top | FrameAnchor.Right,
				//PaddingLeft = 25,
			};
		    listPatientsButton.Add(doctor);
            
			rightPanel.Add( doctor );

		    listPatientsButton.Add(
                AddButton(
                    rightPanel, 
                    rightPanel.Width - buttonWidth,
                    doctor.Height + 10,
                    buttonWidth,
                    buttonHeight, 
                    "List of Patients",
                    FrameAnchor.Top | FrameAnchor.Left,
		        //() => { for ( int i = 2; i < rightPanel.Children.Count(); i++ ) { var c = rightPanel.Children.ElementAt(i); c.Visible = !c.Visible; } }, Color.Zero );
		            () => { }, Color.Zero)
                );

		}

		void CreatePatientList(HashSet<Patient> list) {

			int buttonHeight    = rightPanel.Font.LineHeight;	
			int buttonWidth		= 200;	
			//int size = rightPanel.Children.Count();
//		    foreach (var patientButton in listPatientsButton)
//		    {
//                rightPanel.Remove(patientButton);
//		    }
            while (listPatientsButton.Count>2)
		    {
                rightPanel.Remove(listPatientsButton[2]);
                listPatientsButton.RemoveAt(2);
		    }
//            listPatientsButton.Clear();
		
			int width = GraphicsDevice.DisplayBounds.Width;
			//int id = 1;
			foreach ( var l in list ) {
				String s = l.id;
			    listPatientsButton.Add(
                    AddButton(
			        rightPanel,
			        width - buttonWidth,
			        0,
			        buttonWidth,
			        buttonHeight, s,
			        FrameAnchor.Top | FrameAnchor.Left,
			        () => { drawPatientsPath(l); },
			        Color.Zero)
			    );
			}
			verticalOffset = 0;
			int y = verticalOffset + 5;
			foreach (var child in listPatientsButton){
				child.Y = y;
				y += child.Height + 10;
			}

		}

		Frame AddButton(Frame parent, int x, int y, int w, int h, string text, FrameAnchor anchor, Action action, Color bcol, bool visibility = true) {
			var button = new Frame( this, x, y, w, h, text, Color.White ) {
				Anchor = anchor,
				TextAlignment = Alignment.MiddleCenter,
				PaddingLeft = 25,
                Font = labelFontNormal,
				Visible = visibility,
			};
			Color testcol = new Color( 51, 51, 51, 255 );

			if ( action != null ) {
				button.Click += (s, e) =>
				{
					action();
					if (bcol != testcol) {bcol = (bcol == Color.Zero) ? (new Color(62, 106, 181, 255)) : (Color.Zero);}
				};
			}
			
			button.StatusChanged += (s, e) =>
			{
				if ( e.Status == FrameStatus.None ) { button.BackColor = bcol; }
				if ( e.Status == FrameStatus.Hovered ) { button.BackColor = new Color( 25, 71, 138, 255 ); }
				if ( e.Status == FrameStatus.Pushed ) { button.BackColor = new Color( 99, 132, 181, 255 ); }
			};

			parent.Add( button );
		    return button;
		}

        Frame AddMapButton(Frame parent, int x, int y, int w, int h, string img, string text, Action action)
        {
            var button = new Frame(this, x, y, w, h, text, Color.Zero)
            {
                Image = Content.Load<Texture2D>(img),
                ImageColor = Color.Orange,
                ImageMode = FrameImageMode.Stretched,
                Font = labelFontNormal,
              //  Font = Game.GetService<Dashboard>().plotLightFont,
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


	    public void drawPatientsPath(Patient patient, bool reDraw = true)
	    {


	        if (!reDraw)
	        {
                var graphSys = GetService<GraphSystem>();
                graphSys.SelectPath(patient.visitList.Select(visit => visit.id).ToList());
	        }
	        else
	        {
                while (listVisitButton.Count > 0)
                {
                    rightPanel.Remove(listVisitButton[0]);
                    listVisitButton.RemoveAt(0);
                }
                var graphSys = GetService<GraphSystem>();
                graphSys.SelectPath(patient.visitList.Select(visit => visit.id).ToList());

                var visitByDate = patient.visitList.GroupBy(visit => visit.date.ToString("dd MMM yyyy"));
                int countVisitByDate = visitByDate.Count();
                if (countVisitByDate > 25)
                {
                    visitByDate = patient.visitList.GroupBy(visit => visit.date.ToString("dd MMM"));
                    countVisitByDate = visitByDate.Count();
                }
                var width = (GraphicsDevice.DisplayBounds.Width - 200) / countVisitByDate;
                var height = 10;
                var maxVisits = visitByDate.Max(visits => visits.Count());
                var radiusMin = 15;
                var radiusMax = 100;
                var x = 0;
                int y = GraphicsDevice.DisplayBounds.Height * 95 / 100;
                int yNext = GraphicsDevice.DisplayBounds.Height * 85 / 100;

                foreach (var visit in visitByDate)
                {
                    var radius = (((float)visit.Count()) / maxVisits) * (radiusMax - radiusMin) + radiusMin;
                    listVisitButton.Add(AddButton(rightPanel, x, y, width, height, visit.Key.ToString(), FrameAnchor.Top | FrameAnchor.Left, () => { }, Color.Zero));
                    var button = AddMapButton(rightPanel, x, yNext, (int)radius, (int)radius, "node",
                        visit.Count().ToString(), () => { });
                    button.MouseIn += (s, e) => drawPatientsPathDay(visit.ToArray());
                    button.MouseOut += (s, e) => drawPatientsPath(patient, false);
                    listVisitButton.Add(button);
                    x += width;
                }
	        }
            
	    }


        public void drawPatientsPathDay(Visit[] listVisit)
        {
            var graphSys = GetService<GraphSystem>();
            graphSys.SelectPath(listVisit.Select(visit => visit.id).ToList());
        }


	}
}
