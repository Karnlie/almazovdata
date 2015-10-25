using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphVis
{
	public class StanfordNetwork : GraphFromFile
	{
		const int maxNodes = 50000;

		public override void ReadFromFile(string path)
		{
			var lines = File.ReadAllLines(path);
			Dictionary<int, int> nodeId_NodeNumber = new Dictionary<int, int>();	
			List<int> nodeDegrees = new List<int>();

			int numOfNodesAdded = 0;
			if (lines.Length > 0)
			{
				// construct dictionary to convert id to number:
				foreach (var line in lines)
				{
					if (line.Length < 4)
						continue;
					if (line.ElementAt(0) != '#')
					{
						string[] parts;
						parts = line.Split(new Char[] { '\t', ' ' });
						int index1 = int.Parse(parts[0]);
						int index2 = int.Parse(parts[1]);

						if (!nodeId_NodeNumber.ContainsKey(index1))
						{
							nodeId_NodeNumber.Add(index1, numOfNodesAdded);
							++numOfNodesAdded;
						}
						if (!nodeId_NodeNumber.ContainsKey(index2))
						{
							nodeId_NodeNumber.Add(index2, numOfNodesAdded);
							++numOfNodesAdded;
						}
					}
				}
                //List<int> t = new List<int>(); add //раскраска точек
                //t.Contains(2);

				int numNodes = maxNodes < nodeId_NodeNumber.Count ? maxNodes : nodeId_NodeNumber.Count;
				// add nodes:
				for (int i = 0; i < numNodes; ++i)
				{
                    //if (i =) {
                    //    AddNode(new NodeWithText("", 3.0f, Fusion.Mathematics.Color.Red));
                    //}
                    //else
                    {
                        AddNode(new NodeWithText());
                    }
                    nodeDegrees.Add(0);
				}

				// add edges:
				Console.WriteLine("checking...");
				foreach ( var line in lines )
				{
					if (line.Length < 4)
						continue;
					if (line.ElementAt(0) != '#')
					{
						string[] parts;
						parts = line.Split(new Char[] {'\t', ' '});
						int index1 = nodeId_NodeNumber[int.Parse(parts[0])];
						int index2 = nodeId_NodeNumber[int.Parse(parts[1])];		
						
						if (index1 != index2)
						{
							if ( index1 < numNodes && index2 < numNodes ) {
								AddEdge(index1, index2);
								nodeDegrees[index1] += 1;
								nodeDegrees[index2] += 1;
								((NodeWithText)Nodes[index1]).Text = "id=" + parts[0];
								((NodeWithText)Nodes[index2]).Text = "id=" + parts[1];
							}
						}
						else
						{
							Console.WriteLine("bad edge " + line);
						}
						
					}
				}
				Console.WriteLine("checked");

				int maxDegree = 0;
				foreach (var d2 in nodeDegrees)
				{
					int degree = d2/2;
					maxDegree = degree > maxDegree ? degree : maxDegree;	
				}
				Console.WriteLine("max degree = " + maxDegree);
			}
		}

		public void ReadFromFileInforNode(String filename, Dictionary<int, String> dict)
		{
			// TODO: dict
			var lines = File.ReadAllLines(filename);
			if (lines.Length > 0)
			{
				// construct dictionary to convert id to number:
				foreach (var line in lines)
				{
					string[] parts;
					parts = line.Split(new Char[] { '\t', '^' }); //^
					int index1 = int.Parse(parts[0]);
					string index2 = parts[1];

                    dict.Add(index1, index2);
                    index2 = index2.Trim();
				}
            }
            

        }
        

        public struct Visit
        {
            public DateTime Date;
            public string Fio;
            public string Categ;
            public int Id;
 
        }

       
        public void ReadFromFilePatientData(String dirName, Dictionary<int, String> dict, Dictionary<int, List<Visit>> patData)
		{
			string[] files = Directory.GetFiles(dirName);
            // run to pac
            foreach(string filename in files){
                
                
                var lines = File.ReadAllLines(filename);
                if (lines.Length > 0)
			    {
                    var fnicename = filename.Remove(0, filename.LastIndexOf('\\') + 1); //получаем id пациента
                    var splitfilename = fnicename.Split('.');
                    var pacientid = splitfilename[0];
                    int pid = Convert.ToInt32(pacientid);
				 
                    // run to doc
                    var list = new List<Visit>();
				    foreach (var line in lines)
				    {					    
                        var parts = line.Split(';');
                        string date = parts[0];
					    string doctor = parts[1];
                        doctor = doctor.Trim().Replace("(", "").Replace(")", "");

                        var category = doctor.Split(':'); 
                        string cat = category[0].Trim();
                        string fio = category[1].Trim().Replace("\"", "");
                        //cat = cat;
                        //fio = fio;
                        int iddoc = 9999;
                        for (int i = 0; i < dict.Keys.Count - 1; i++)
                        {
                            if (dict[i] == fio)
                                iddoc = i;

                            else
                            {
                                dict[i+1] = doctor;

                            }
                        }

                        //int iddoc = Dictionary.Key.Value;

                        // привести переменную date к типу DateTime (метод ParseExact)

                        string dateFormat = "dd'.'MM'.'yy' 'HH':'mm";
                       
                        var dateNew = DateTime.ParseExact(date, dateFormat, null);

                        list.Add(new Visit {Date = dateNew, Fio = fio, Categ = cat, Id = iddoc});
                        
                        
				    }
                    patData.Add(pid, list);
			    }
            }
		}

	}
}