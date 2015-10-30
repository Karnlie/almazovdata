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
using GraphVis.Helpers;
using GraphVis.Models;
using GraphVis.Models.Frames;
using GraphVis.Models.Medicine;

namespace GraphVis
{
	public class GraphVis : Game
	{
		Random rnd = new Random();
		int selectedNodeIndex;

        Dictionary<int, int> nodeId_NodeNumber = new Dictionary<int, int>();

		SpriteFont font1;
	    private SpriteFont labelFontNormal;

		Vector3 selectedNodePos;
		bool isSelected;
		Tuple<Point, Point> dragFrame;
		StanfordNetwork stNet;
		MainFrame mainPanel;

		Dictionary<Doctor, HashSet<Patient>> doctorToPatients;
        HashSet<Patient> patients;
	    private Doctor selectedDoctor;
	    private Patient selectedPatient;

		/// <summary>
		/// GraphVis constructor
		/// </summary>
		public GraphVis()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;
			AddService(new SpriteBatch(this), false, false, 0, 0);
			AddService(new DebugStrings(this), true, true, 9999, 9999);
			AddService(new DebugRender(this), true, true, 9998, 9998);

			AddService(new Camera(this), true, false, 9997, 9997);
			AddService(new GreatCircleCamera(this), true, false, 9995, 9995);
			AddService(new GraphSystem(this), true, true, 9994, 9994);
			AddService( new UserInterface( this, @"alsBold" ), true, true, 10000, 10000 );

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
            labelFontNormal = Content.Load<SpriteFont>("alsNormal");
			var cam = GetService<Camera>();
			cam.Config.FreeCamEnabled = false;
			selectedNodeIndex = 0;
			selectedNodePos = new Vector3();
			isSelected = false;
			//add gui interface
			var ui = GetService<UserInterface>();
            mainPanel = new MainFrame(this, 0, 0, this.GraphicsDevice.DisplayBounds.Width, this.GraphicsDevice.DisplayBounds.Height, "", new Color(20, 20, 20, 0f));
            ui.RootFrame = mainPanel;
			ui.SettleControls();
			GraphicsDevice.DisplayBoundsChanged += (s, e) =>
			{
                mainPanel.resizePanel();
			};

			InputDevice.KeyDown += InputDevice_KeyDown;
			InputDevice.MouseScroll += inputDevice_MouseScroll;
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
			Rectangle patientButtonArea = new Rectangle( GraphicsDevice.DisplayBounds.Width - 200, 0, 200, GraphicsDevice.DisplayBounds.Height );
			if ( patientButtonArea.Contains( InputDevice.MousePosition )  && e.WheelDelta != 0 )
			{
			    mainPanel.GetRightFrame().updatePanelScroll(verticalOffset, e);
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
					doctorToPatients.TryGetValue( doctor, out patients);
                    mainPanel.GetRightFrame().fillPatientList(patients, drawPatientsPath);
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
			    doctorToPatients = new Dictionary<Doctor, HashSet<Patient>>();
                Dictionary<string, int> dict = new Dictionary<string, int>();
                ReaderFiles.ReadFromFilePatientData("../../../../almazovdata", dict, doctorToPatients);
                stNet.BuildGraphFromDictinary(doctorToPatients);
				graphSys.AddGraph(stNet);
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

			//ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
			//ds.Add(Color.Orange, "F1   - show developer console");
			//ds.Add(Color.Orange, "F5   - build content and reload textures");
			//ds.Add(Color.Orange, "F12  - make screenshot");
			//ds.Add(Color.Orange, "ESC  - exit");
			//ds.Add(Color.Orange, "Press Z or X to load graph");
			//ds.Add(Color.Orange, "Press M to load painted graph (SLOW!)");
			//ds.Add(Color.Orange, "Press P to pause/unpause");

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
	        

			dr.View = cam.GetViewMatrix(stereoEye);
			dr.Projection = cam.GetProjectionMatrix(stereoEye);
			var ds = GetService<DebugStrings>();
			if (isSelected)
			{
                selectedDoctor = doctorToPatients.Keys.First(x => x.id == selectedNodeIndex );
				pSys.Select(selectedNodeIndex);
				//вывод в панель пациентов и врача
				mainPanel.Children.ElementAt( 0 ).Text = "Id # " + selectedNodeIndex;
			}
			else
			{

				//ds.Add(Color.Orange, "No selection");
				//pSys.Deselect();
			}
            if(selectedDoctor!=null)
                printInfoDoctor(selectedDoctor.fio + ": " + selectedDoctor.category);
            var offsetY = this.GraphicsDevice.DisplayBounds.Height*95/100;
            // TODO:
//            foreach (var level in mainPanel.listLevel)
//		    {
//                printInfo(level.ToString(), (200 - level.ToString().Count() * 5) / 2, offsetY);
//		        offsetY -= 90;
//		    }
           
		}

	    public void drawPatientsPath(Patient patient, bool reDraw = true)
	    {
	        selectedPatient = patient;
            drawVisitsPath(patient.visitList.ToArray());
	        if (reDraw)
	        {
                mainPanel.clearBottomFrames();
	            var bottomFrame = mainPanel.createBottomFrame();

	            var groupType = HelperFrameNew.GroupType.DAY;
                Dictionary<string, List<Visit>> visitByDate = new Dictionary<string, List<Visit>>();
                groupType = bottomFrame.getFrameVisitFromLevel(patient.visitList, groupType, out visitByDate);
                if(HelperFrameNew.isUpdatedLevel(groupType))
                {
                    bottomFrame.setLevel((int) groupType);
                    bottomFrame.setPosition(1);
                    mainPanel.AddBottomFrame(bottomFrame);
                    bottomFrame.fillVisitData(patient, visitByDate, drawVisitsPath, drawPatientsPath);
	            }
           }
	    }

        public void drawVisitsPath(Visit[] listVisit)
        {
            var graphSys = GetService<GraphSystem>();
            graphSys.SelectPath(listVisit.Select(visit => visit.id).ToList());
        }

	    public void printInfoDoctor(String doctorInfo)
	    {
            String info = doctorInfo;
            int x = GraphicsDevice.DisplayBounds.Width * 50 / 100 - info.Length * 5;
            int y = GraphicsDevice.DisplayBounds.Height * 5 / 100;
	        printInfo(info, x, y);
	    }

        public void printInfo(String info, int offsetX = 0, int offsetY = 0)
        {
            var sb = GetService<SpriteBatch>();
            sb.Begin();
            labelFontNormal.DrawString(sb, info, offsetX, offsetY, Color.White);
            sb.End();
        }

	}
}
