using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
                    //index2 = index2.Trim();
				}
            }
            

        }
        //class Patient(){
        //    int id;
        //    string fio;


        //}
       // List<Patient> listPatient;
	   public struct Visit
	   {
		    public DateTime	Date;
			public string	DocCategorie;
			public string	DocFio;
			public int		DocId;
	   }


        public void ReadFromFilePatientData(String dirName, Dictionary<int, String> dict, Dictionary<String, List<Visit>> patData)
		{
            
			string[] files = Directory.GetFiles(dirName);
            // бегаем по пациентам
            
			
			foreach(string filename in files){
                
                //Получаем ID/имена пациентов в pacientid
				string pacientid = filename.Remove(0, filename.LastIndexOf('\\') + 1).Split('.')[0];
				

				var lines = File.ReadAllLines(filename);
                if (lines.Length > 0)
			    {
					var entries = new List<Visit>();
				
                    // бегаем по докторам
				    foreach (var line in lines)
				    {
					    var parts = line.Split(';'); 
                        
						//обрезаем лишние в дате
						var datefull = parts[0].Replace("\"", "");
					    if (parts[0].Replace("\"", "").Length > 14 ) {datefull = datefull.Remove(datefull.IndexOf(':') + 3, datefull.Length-14);}
						
						//парсим дату
						var dateFormat = "dd'.'MM'.'yy' 'HH':'mm";
						var datedate = DateTime.ParseExact(datefull, dateFormat, null);
						
						//чистим поле с категорией и фио
						var fio = parts[1].Trim().Replace("(", "").Replace(")", "");
                        
						//присваиваем каждой записи ID посещенного врача
						var docId = 9999;
					    var surname = fio.Remove(0, fio.LastIndexOf(':') + 2).Split(' ').First(); //вырезаем фамилию из fio
					    foreach (KeyValuePair<int, string> keyValuePair in dict) {
						    if (keyValuePair.Value.Contains(surname)) docId = keyValuePair.Key;
					    }
						
						//заполняем структуру
						var visit = new Visit { 
							Date = datedate,
							DocCategorie = fio.Split(':')[0].TrimEnd(),
							DocFio = fio.Split(':')[1].TrimStart(),
							DocId = docId
						};
						//добавляем структуру в лист
						entries.Add(visit);

					}
					//сортируем лист структур по дате
					entries.Sort((one, two) => one.Date.CompareTo(two.Date));
					
					//записываем отсортированный лист в словарь patData
					patData.Add(pacientid, entries);
					Console.WriteLine("Patient ID: " + pacientid + "\t Visits count: "+ patData[pacientid].Count);
			    }
            }
		}
        class doctor
        {
            public int id;
            public string fio;
            public string date;
			
        }
        class pacient
        {
            public int id;
            public  List <doctor> doctors; 
            
        }
	}
}