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


        public static void ReadFromFilePatientData(String dirName, Dictionary<String, int> dict, Dictionary<Doctor, HashSet<Patient>> doctorToPatients)
        {
            int defaultId = dict.Values.Max();
            string[] files = Directory.GetFiles(dirName);
            // бегаем по пациентам
            int countLines = 0;
            foreach (string filename in files)
            {
                if (filename.EndsWith(".png"))
                    continue;
                //Получаем ID/имена пациентов в pacientid
                string pacientid = getNicePatientId(filename);
                Patient patient = new Patient(pacientid);
                var lines = File.ReadAllLines(filename);
                countLines += lines.Length;
                if (lines.Length > 0)
                {
                    // бегаем по докторам
                    foreach (var line in lines)
                    {

                        var parts = line.Split(';');
                        //обрезаем лишние в дате
                        DateTime datedate = getNiceDate(parts[0]);
                        //чистим поле с категорией и фио
                        String fio = getNiceFIO(parts[1]);

                        //присваиваем каждой записи ID посещенного врача
                        int docId;
                        if (!dict.TryGetValue(fio.Replace(" ", ""), out docId))
                        {
                            Console.WriteLine("FIO:"+fio+" NOT FOUND!");
                            docId = defaultId;
                            defaultId++;
                        }
                        var surname = getSurameFromFio(fio); //вырезаем фамилию из fio

                        //заполняем структуру
                        var visit = new Visit
                        {
                            id = docId,
                            date = datedate,
                            category = fio.Split(':')[0].TrimEnd(),
                            fio = fio.Split(':')[1].TrimStart()
                        };
                        //добавляем структуру в лист
                        patient.addNewVisit(visit);

                        var doctor = new Doctor
                        {
                            id = docId,
                            category = fio.Split(':')[0].TrimEnd(),
                            fio = fio.Split(':')[1].TrimStart()
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
            Console.WriteLine(countLines);
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
    }
}
