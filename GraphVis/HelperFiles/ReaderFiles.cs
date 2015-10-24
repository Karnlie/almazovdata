using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphVis.Models.Medicine;

namespace GraphVis.HelperFiles
{
    class ReaderFiles
    {
        public static void ReadFromFileInforNode(String filename, Dictionary<int, String> dict)
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
                }
            }


        }


        public static void ReadFromFileDoctorList(String filename, Dictionary<String, int> dict)
        {
            var lines = File.ReadAllLines(filename);
            if (lines.Length > 0)
            {
                // construct dictionary to convert id to number:
                foreach (var line in lines)
                {
                    string[] parts;
                    parts = line.Split(new Char[] { '\t', '^' }); //^
                    int doctorId = int.Parse(parts[0]);
                    string fio=getNiceFIO(parts[1]);

                    dict.Add(fio.Replace(" ", ""), doctorId);
                }
            }


        }

        static int defaultId;
        public static void ReadFromFilePatientData(String dirName, Dictionary<String, int> dict, Dictionary<Doctor, HashSet<Patient>> doctorToPatients)
        {
            defaultId = dict.Values.Max();
            string[] files = Directory.GetFiles(dirName);
            // бегаем по пациентам
            foreach (string filename in files)
            {
                if (filename.EndsWith(".png"))
                    continue;
                //Получаем ID/имена пациентов в pacientid
                string pacientid = getNicePatientId(filename);
                Patient patient = new Patient(pacientid);
                var lines = File.ReadAllLines(filename);
                if (lines.Length > 0)
                {
                    // бегаем по докторам
                    foreach (var line in lines)
                    {
                        var parts = line.Split(';');
                        DateTime datedate = getNiceDate(parts[0]);
                        String fio = getNiceFIO(parts[1]);
                        int docId = findIdDoctor(dict, fio);
                        Visit visit = new Visit
                        {
                            id = docId,
                            date = datedate,
                            category = fio.Split(':')[0].Trim(),
                            fio = fio.Split(':')[1].Trim()
                        };
                        patient.addNewVisit(visit);

                        Doctor doctor = new Doctor
                        {
                            id = docId,
                            category = fio.Split(':')[0].Trim(),
                            fio = fio.Split(':')[1].Trim()
                        };
                   
                        HashSet<Patient> listPatients;
                        if (doctorToPatients.TryGetValue(doctor, out listPatients))
                        {
                            listPatients.Add(patient);
                        }
                        else
                        {
                            listPatients = new HashSet<Patient>() { patient };
                            doctorToPatients.Add(doctor, listPatients);
                        }
                    }
                    //сортируем лист структур по дате
                    patient.visitList.Sort((one, two) => one.date.CompareTo(two.date));
                }
            }
//            int limit = 100;
            using (StreamWriter sw = new StreamWriter("myfile.txt"))
            {
                foreach (HashSet<Patient> patients in doctorToPatients.Values)
                {
                    foreach (var patient in patients)
                    {
                        sw.Write(patient);
                    }
                    break;
                }
            }
            //File.WriteAllLines("myfile.txt",
               // doctorToPatients.Select(x => x.Key.id + ";" + x.Key.fio + ";" + x.Key.category).ToArray());
        }

        private static DateTime getNiceDate(String dateString)
        {
            var datefull = dateString.Replace("\"", "");
            if (dateString.Replace("\"", "").Length > 14)
            {
                datefull = datefull.Remove(datefull.IndexOf(':') + 3, datefull.Length - 14);
            }
            //парсим дату
            var dateFormat = "dd'.'MM'.'yy' 'HH':'mm";
            var datedate = DateTime.ParseExact(datefull, dateFormat, null);
            return datedate;
        }


        private static String getNiceFIO(String fio)
        {
            return fio.Replace("(", "").Replace(")", "").TrimStart().TrimEnd();
        }

        private static String getNicePatientId(String notNiceId)
        {
            return notNiceId.Remove(0, notNiceId.LastIndexOf('\\') + 1).Split('.')[0];
        }

        private static String getSurameFromFio(String fio)
        {
            return fio.Split(':').Last().TrimStart();
        }

        private static int findIdDoctor(Dictionary<String, int> dict, String fio)
        {
            int docId;
            if (!dict.TryGetValue(fio.Replace(" ", ""), out docId))
            {
                Console.WriteLine("FIO:" + fio + " NOT FOUND!");
                docId = defaultId;
                defaultId++;
            }
            return docId;
        }
    }
}
