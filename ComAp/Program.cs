using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace ComAp
{
    class Program
    {
        static void Main(string[] args)
        {
            string loadFile = "examination.txt";

            List<Student> students = File.LoadData(loadFile);
            Console.WriteLine("Loaded files from file {0}", loadFile);

            //setřídí studenty podle skupin, aby nedošlo k chybě pokud by byl do listu ještě nějaký student přidán
            students = students.OrderBy(o => o.gr).ToList();

            string saveFile = "output.xml";
            File.SaveXml(students, saveFile);
            Console.WriteLine("Saved XML file as {0}", saveFile);

            File.FindErrStudents(students);

            Console.ReadLine();
        }

        public class Student
        {
            public string name;
            public int gr;
            private float avgW;
            private int oma, oph, oen, ma, ph, en;

            public Student(string name, int ma, int ph, int en, int gr)
            {
                this.name = name;

                //originální údaje
                oma = ma;
                oph = ph;
                oen = en;

                //opravené údaje 
                this.ma = CorrVal(ma);
                this.ph = CorrVal(ph);
                this.en = CorrVal(en);

                this.gr = gr;
                
                CalcAvgW();
            }
            //oprava údaje
            static int CorrVal(int value)
            {
                int minimum = 0;
                int maximum = 100;
                if (value < minimum)
                {
                    return 0;
                }
                if (value > maximum)
                {
                    return 100;
                }
                else
                    return value;
            }
            //vážený průměr
            void CalcAvgW()
            {
                avgW = (ma * 0.4f + ph * 0.35f + en * 0.25f);
            }

            //gettery a settery v případě změny údajů
            public int Ma
            {
                get { return ma; }
                set
                {
                    oma = Ma;
                    ma = CorrVal(Ma);
                    CalcAvgW();
                }
            }
            public int Ph
            {
                get { return ph; }
                set
                {
                    oph = Ph;
                    ph = CorrVal(Ph);
                    CalcAvgW();
                }
            }
            public int En
            {
                get { return en; }
                set
                {
                    oen = En;
                    en = CorrVal(En);
                    CalcAvgW();
                }
            }
            public float AvgW
            {
                get { return avgW; }
            }
            public int Oma
            {
                get { return oma; }
            }
            public int Oph
            {
                get { return oph; }
            }
            public int Oen
            {
                get { return oen; }
            }
        }
        class Math
        { 
        public static decimal GetMedian(List<int> list)
        {
            int[] temp = list.ToArray();
            Array.Sort(temp);
            int count = temp.Length;
            if (count == 0)
            {
                throw new Exception("List is empty");
            }
            else if (count % 2 == 0)
            {
                int a = temp[count / 2 - 1];
                int b = temp[count / 2];
                return (a + b) / 2m;
            }
            else
            {
                return temp[count / 2];
            }
        }
        public static int GetModus(List<int> list)
        {
            int mode = default(int);
            if (list != null && list.Count() > 0)
            {
                Dictionary<int, int> counts = new Dictionary<int, int>();
                foreach (int element in list)
                {
                    if (counts.ContainsKey(element))
                        counts[element]++;
                    else
                        counts.Add(element, 1);
                }
                int max = 0;
                foreach (KeyValuePair<int, int> count in counts)
                {
                    if (count.Value > max)
                    {
                        mode = count.Key;
                        max = count.Value;
                    }
                }
            }
            else
            {
                throw new Exception("List is empty");
            }
            return mode;
        }
    }

        class File {

            //vyhledává chybné studenty
            public static void FindErrStudents(List<Student> students)
            {
                List<Student> errStudents = new List<Student>();
                foreach (Student student in students)
                {
                    //kontrola jestli sedí originální data s opravenými, pokud ne, přidá studenta do listu chybných studentů
                    if (student.Oma != student.Ma || student.Oph != student.Ph || student.Oen != student.En)
                    {
                        errStudents.Add(student);
                    }
                }

                //pokud není list prázdný, vypíše error log
                if (errStudents.Any())
                {
                    string saveLogFile = "error log.xml";
                    SaveLog(errStudents, saveLogFile);
                    Console.WriteLine("Errors found in student data, error log saved as: {0}", saveLogFile);
                }
                else
                {
                    Console.WriteLine("No errors found, log has not been saved");
                }

            }
            //načítá soubor
            public static List<Student> LoadData(string fileName)
            {
            List<Student> studentList = new List<Student>();
            using (StreamReader reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        if (line.Contains("Group"))
                        {
                            string[] group = line.Split('p');
                            int gr = Int32.Parse(group[1]);

                            while ((line = reader.ReadLine()) != "" && line != null)
                            {
                                string[] nodes = line.Split(';');
                                string name = nodes[0];

                                string[] math = nodes[1].Split('=');
                                int ma = Int32.Parse(math[1]);

                                string[] phys = nodes[2].Split('=');
                                int ph = Int32.Parse(phys[1]);

                                string[] eng = nodes[3].Split('=');
                                int en = Int32.Parse(eng[1]);



                                Student st = new Student(name, ma, ph, en, gr);
                                studentList.Add(st);
                            }
                        }
                    }
                    catch
                    {
                        throw new Exception("File is in an unknown format");
                    }

                }
            }
                if (!studentList.Any())
                {
                    throw new Exception("No students found in file");
                }
                return studentList;
        }
        //ukládá XML
        public static void SaveXml(List<Student> students, string fileName)
        {

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(fileName, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Students");

            //listy známek v předmětech pro výpočet průměrů, modusů a medianů
            List<int> maG = new List<int>();
            List<int> phG = new List<int>();
            List<int> enG = new List<int>();

            List<int> ma = new List<int>();
            List<int> ph = new List<int>();
            List<int> en = new List<int>();

            int group = 0;
            foreach (Student student in students)
            {
                if (group == 0)
                {
                    xmlWriter.WriteStartElement("Group", student.gr.ToString());
                    group = student.gr;
                }
                //pokud se skupina tohoto studenta liší od skupiny předchožího studenta, vypočítej průměr, modusy a mediany předchoží skupiny
                if (student.gr != group)
                {
                    xmlWriter.WriteElementString("MathGroupAverage", maG.Average().ToString());
                    xmlWriter.WriteElementString("MathGroupModus", Math.GetModus(maG).ToString());
                    xmlWriter.WriteElementString("MathGroupMedian", Math.GetMedian(maG).ToString());

                    xmlWriter.WriteElementString("PhysicsGroupAverage", phG.Average().ToString());
                    xmlWriter.WriteElementString("PhysicsGroupModus", Math.GetModus(phG).ToString());
                    xmlWriter.WriteElementString("PhysicsGroupMedian", Math.GetMedian(phG).ToString());

                    xmlWriter.WriteElementString("EnglishGroupAverage", enG.Average().ToString());
                    xmlWriter.WriteElementString("EnglishGroupModus", Math.GetModus(enG).ToString());
                    xmlWriter.WriteElementString("EnglishGroupMedian", Math.GetMedian(enG).ToString());

                    maG.Clear();
                    phG.Clear();
                    enG.Clear();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("Group", student.gr.ToString());
                    group = student.gr;
                }

                xmlWriter.WriteStartElement("Student", student.name);
                xmlWriter.WriteElementString("Math", student.Ma.ToString());
                xmlWriter.WriteElementString("Physics", student.Ph.ToString());
                xmlWriter.WriteElementString("English", student.En.ToString());
                xmlWriter.WriteElementString("WeightedAverage", student.AvgW.ToString());

                maG.Add(student.Ma);
                phG.Add(student.Ph);
                enG.Add(student.En);

                ma.Add(student.Ma);
                ph.Add(student.Ph);
                en.Add(student.En);

                xmlWriter.WriteEndElement();
            }

            //průměry, modusy a mediany poslední skupiny
            xmlWriter.WriteElementString("MathGroupAverage", maG.Average().ToString());
            xmlWriter.WriteElementString("MathGroupModus", Math.GetModus(maG).ToString());
            xmlWriter.WriteElementString("MathGroupMedian", Math.GetMedian(maG).ToString());

            xmlWriter.WriteElementString("PhysicsGroupAverage", phG.Average().ToString());
            xmlWriter.WriteElementString("PhysicsGroupModus", Math.GetModus(phG).ToString());
            xmlWriter.WriteElementString("PhysicsGroupMedian", Math.GetMedian(phG).ToString());

            xmlWriter.WriteElementString("EnglishGroupAverage", enG.Average().ToString());
            xmlWriter.WriteElementString("EnglishGroupModus", Math.GetModus(enG).ToString());
            xmlWriter.WriteElementString("EnglishGroupMedian", Math.GetMedian(enG).ToString());

            xmlWriter.WriteEndElement();

            //celkové průměry, modusy a mediany
            xmlWriter.WriteElementString("MathAverage", ma.Average().ToString());
            xmlWriter.WriteElementString("MathModus", Math.GetModus(ma).ToString());
            xmlWriter.WriteElementString("MathMedian", Math.GetMedian(ma).ToString());

            xmlWriter.WriteElementString("PhysicsAverage", ph.Average().ToString());
            xmlWriter.WriteElementString("PhysicsModus", Math.GetModus(ph).ToString());
            xmlWriter.WriteElementString("PhysicsMedian", Math.GetMedian(ph).ToString());

            xmlWriter.WriteElementString("EnglishAverage", en.Average().ToString());
            xmlWriter.WriteElementString("EnglishModus", Math.GetModus(en).ToString());
            xmlWriter.WriteElementString("EnglishMedian", Math.GetMedian(en).ToString());

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        //ukládá error log
        static void SaveLog(List<Student> students, string fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(fileName, settings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Students");
            foreach (Student student in students)
            {
                //pokud nesedí originální zaznamenaná hodnota s opravenou hodnotou v daném předmětu, vypíše studenta s danou chybnou hodnotou
                    xmlWriter.WriteStartElement("Student", student.name);

                    if (student.Oma != student.Ma)
                    {
                        xmlWriter.WriteElementString("Math", student.Oma.ToString());
                    }
                    if (student.Oph != student.Ph)
                    {
                        xmlWriter.WriteElementString("Physics", student.Oph.ToString());
                    }
                    if (student.Oen != student.En)
                    {
                        xmlWriter.WriteElementString("English", student.Oen.ToString());
                    }
                    xmlWriter.WriteEndElement();
                

            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }
    }
    }
}
