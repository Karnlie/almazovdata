using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using GraphVis.Models.Medicine;

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
                foreach (var line in lines)
                {
                    if (line.Length < 4)
                        continue;
                    if (line.ElementAt(0) != '#')
                    {
                        string[] parts;
                        parts = line.Split(new Char[] { '\t', ' ' });
                        int index1 = nodeId_NodeNumber[int.Parse(parts[0])];
                        int index2 = nodeId_NodeNumber[int.Parse(parts[1])];

                        if (index1 != index2)
                        {
                            if (index1 < numNodes && index2 < numNodes)
                            {
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
                    int degree = d2 / 2;
                    maxDegree = degree > maxDegree ? degree : maxDegree;
                }
                Console.WriteLine("max degree = " + maxDegree);
            }
        }

        public void BuildGraphFromDictinary(Dictionary<Doctor, HashSet<Patient>> doctorToPatients)
        {
            Dictionary<int, int> nodeId_NodeNumber = new Dictionary<int, int>();
            List<int> nodeDegrees = new List<int>();

            int numOfNodesAdded = 0;
            HashSet<Patient> uniquePatients = new HashSet<Patient>();
            foreach (HashSet<Patient> patients  in doctorToPatients.Values)
            {
                foreach (var patient in patients)
                {
                    if(uniquePatients.Contains(patient))
                        continue;
                    uniquePatients.Add(patient);
                    for (int i = 0; i < patient.visitList.Count-1; i++)
                    {
                        int index1 = patient.visitList[i].id;
                        int index2 = patient.visitList[i+1].id;
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
            }
           // enges = enges.DistinctBy();
            int numNodes = maxNodes < nodeId_NodeNumber.Count ? maxNodes : nodeId_NodeNumber.Count;
            // add nodes:
            for (int i = 0; i < numNodes; ++i)
            {
                {
                    AddNode(new NodeWithText());
                }
                nodeDegrees.Add(0);
            }

            // add edges:
            Console.WriteLine("checking...");

            int countEnges=0;

            foreach (var patient in uniquePatients)
            {
                for (int i = 0; i < patient.visitList.Count - 1; i++)
                {  
                    int index1 = patient.visitList[i].id;
                    int index2 = patient.visitList[i + 1].id;
                    if (index1 != index2)
                    {
                        if (index1 < numNodes && index2 < numNodes)
                        {
                            AddEdge(index1, index2);
                            nodeDegrees[index1] += 1;
                            nodeDegrees[index2] += 1;
                            ((NodeWithText)Nodes[index1]).Text = "id=" + index1;
                            ((NodeWithText)Nodes[index2]).Text = "id=" + index2;
                        }
                    }
                    else
                    {
                        Console.WriteLine("bad edge: " + index1+"-"+index2);
                    }
                }
            }
           
            Console.WriteLine("checked");

            int maxDegree = 0;
            foreach (var d2 in nodeDegrees)
            {
                int degree = d2 / 2;
                maxDegree = degree > maxDegree ? degree : maxDegree;
            }
            Console.WriteLine("max degree = " + maxDegree);
            
        }

	    public void paintPathPatient(Patient patient)
	    {
            for (int i = 0; i<patient.visitList.Count - 1; i++)
            {  
                int index1 = patient.visitList[i].id;
                int index2 = patient.visitList[i + 1].id;
	            
	        }
	        
	    }
	}
}