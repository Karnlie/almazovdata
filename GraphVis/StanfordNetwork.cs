using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphVis
{
	public class StanfordNetwork : GraphFromFile
	{//отсюда
		const int maxNodes = 50000; //кол-во макс. допустимых точек

		public override void ReadFromFile(string path)
		{
			var lines = File.ReadAllLines(path); //открывается файл, считывает все строки и закр. его
			Dictionary<int, int> nodeId_NodeNumber = new Dictionary<int, int>(); //создается словарь айдишников	
			List<int> nodeDegrees = new List<int>(); //создается коллекция

			int numOfNodesAdded = 0; 
			if (lines.Length > 0)    //если файл не пустой            
			{
				// создается словарь с конвертацией айдишников:
				foreach (var line in lines) //для каждого элемента с файла
				{
					if (line.Length < 4)   //если длина строки меньше 4
						continue;
                    if (line.ElementAt(0) != '#') //если строка не символ #
					{
						string[] parts; //создаем массив
						parts = line.Split(new Char[] { '\t', ' ' }); // возвращение массива строк
						int index1 = int.Parse(parts[0]); //первый столбец
						int index2 = int.Parse(parts[1]); //второй

						if (!nodeId_NodeNumber.ContainsKey(index1)) //если первой столбец не содержится в 
						{
							nodeId_NodeNumber.Add(index1, numOfNodesAdded); // добавляем значение
							++numOfNodesAdded;
						}
						if (!nodeId_NodeNumber.ContainsKey(index2)) // тоже самое
						{
							nodeId_NodeNumber.Add(index2, numOfNodesAdded);
							++numOfNodesAdded;
						}
					}
				}
                //List<int> t = new List<int>(); add //раскраска точек
                //t.Contains(2);

				int numNodes = maxNodes < nodeId_NodeNumber.Count ? maxNodes : nodeId_NodeNumber.Count;
				// создаем точки:
				for (int i = 0; i < numNodes; ++i)
				{
                    {
                        AddNode(new NodeWithText());
                    }
                    nodeDegrees.Add(0);
				}

				// создаем ребра:
				Console.WriteLine("checking...");
				foreach ( var line in lines )
				{
					if (line.Length < 4)
						continue;
					if (line.ElementAt(0) != '#') //если элемент не символ #, то создаем  массив, состоящий из 2 столбцов
					{
						string[] parts;
						parts = line.Split(new Char[] {'\t', ' '}); //разбиение на подстроки
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
		} //до сюда

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

       
        public void ReadFromFilePatientData(String dirName, Dictionary<String, int> dict, Dictionary<int, List<Visit>> patData, Dictionary<int,int> graphEdges)
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
                        string date = parts[0].Replace("\"", "").Trim();
                        if (date.Length > 14)
                            date = date.Remove(14);
					    string doctor = parts[1];
                        doctor = doctor.Trim().Replace("(", "").Replace(")", "");

                        var category = doctor.Split(':'); 
                        string cat = category[0].Trim();
                        string fio = category[1].Trim().Replace("\"", "");
                        int iddoc = 9999;
                        if (dict.Count != 0) {
                            if (dict.ContainsKey(fio)) { dict.TryGetValue(fio, out iddoc); }
                            else {
                                iddoc = dict.Count + 1;
                                dict.Add(fio, iddoc);
                            };

                        }
                        else {
                            iddoc = 1;
                            dict.Add(fio, iddoc);
                        }
                        

                        string dateFormat = "dd'.'MM'.'yy' 'HH':'mm";
                        
                        var dateNew = DateTime.ParseExact(date, dateFormat, null);

                        list.Add(new Visit {Date = dateNew, Fio = fio, Categ = cat, Id = iddoc});
                        
                        
				    }
                    list.Sort((one, two) => one.Date.CompareTo(two.Date));

                    for (int i = 0; i < list.Count - 1; i++)
                    {
                        if (i < list.Count - 2)
                        {
                            if (list[i].Id != list[i + 1].Id) graphEdges.Add(list[i].Id, list[i + 1].Id);
                        }
                    }    

                    patData.Add(pid, list);
                    
                    //foreach (List[i].Id in patData)
                   
                    {       
                     //   patData[pid][i].Id
                        //int array = new int
                    }
                
                    
			    }
            }
		}

	}
}