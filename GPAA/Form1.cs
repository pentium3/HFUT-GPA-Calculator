using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace GPAA
{
    public partial class Form1 : Form
    {
        //String[] Semester = new String[1010];
        //String[] Subject = new String[1010];
        //String[] Code = new String[1010];
        //int[] Grade = new int[1010];
        //double[] Credit = new double[1010];
        //int[] IDnum = new int[1010];
        //sbj[] subjects = new sbj[1010];
        List<sbj> subject = new List<sbj>();
        double[] gpaw = new double[1010];
        double[] gpah = new double[1010];
        double[] gpac = new double[1010];
        int num_sbj;
        bool hfcampus = false;

        private double CalcGPAfromjson(String str)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            List<custom_GPA> gpalist = new List<custom_GPA>();
            gpalist = ser.Deserialize<List<custom_GPA>>(str);
            //Console.WriteLine(gpalist.Count);
            //for (int i = 0; i < gpalist.Count; i++)
            //    Console.WriteLine(gpalist[i].grade.ToString() + gpalist[i].level.ToString());

            double tot_Credit = 0;
            double tot_GPAc = 0;
            for (int i = 0; i < num_sbj; i++)
            {
                tot_Credit += subject[i].Credit;
                double lv = 0.0;
                for(int j=0;j<gpalist.Count;j++)
                {
                    if ((subject[i].Grade >= gpalist[j].grade) && (gpalist[j].level > lv))
                        lv = gpalist[j].level;
                }
                gpac[i] = lv * subject[i].Credit;
                //Console.WriteLine(gpac[i].ToString() + " " + lv.ToString() + " " + subject[i].Credit.ToString());
                tot_GPAc += gpac[i];
            }
            tot_GPAc = tot_GPAc / tot_Credit;
            return tot_GPAc;
        }


        public Form1()
        {
            InitializeComponent();
        }


        private int sbjsort(sbj A, sbj B)
        {
            return (String.Compare(A.Code, B.Code));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String usr = textBox1.Text;
            String pwd = textBox2.Text;
            //UserStyle=student&user=2013217204&password=00a9aa11
            //String form = "UserStyle=student&user=" + usr + "&password=" + pwd;
            HtmlElement Submit = webBrowser1.Document.All["button"];
            HtmlElement usrText = webBrowser1.Document.All["user"];
            HtmlElement pwdText = webBrowser1.Document.All["password"];

            usrText.SetAttribute("value", usr);
            pwdText.SetAttribute("value", pwd);
            Submit.InvokeMember("click");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(hfcampus)
                webBrowser1.Navigate("http://121.251.19.29/");
            else
                webBrowser1.Navigate("http://222.195.8.201/");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(hfcampus)
                webBrowser1.Navigate("http://121.251.19.29/student/asp/Select_Success.asp");
            else
                webBrowser1.Navigate("http://222.195.8.201/student/asp/Select_Success.asp");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            subject.Clear();

            //String page = webBrowser1.DocumentText;     //HTML Code
            StreamReader sr = new StreamReader(webBrowser1.DocumentStream, Encoding.GetEncoding("gb2312"));
            String page = sr.ReadToEnd();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);
            
            num_sbj = -1;
            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                //Console.WriteLine("Found: ", table.Id);
                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    ListViewItem line = new ListViewItem();
                    line.Text = "";
                    //Console.WriteLine("row");
                    int counter = 0;
                    num_sbj++;
                    double CurrentG = 0.0;
                    double CurrentC = 0.0;
                    String CurrentCd = "";
                    String CurrentSem = "";
                    String CurrentSbj = "";
                    foreach (HtmlNode cell in row.SelectNodes("th|td"))
                    {
                        counter++;
                        //Console.WriteLine("cell: " + cell.InnerText);
                        if (counter == 5)          //Grade
                        {
                            String gg = cell.InnerText;
                            if (gg == "\r\n\t\t\t 优") gg = "90";
                            else if (gg == "\r\n\t\t\t 良") gg = "80";
                            else if (gg == "\r\n\t\t\t 中") gg = "70";
                            else if (gg == "\r\n\t\t\t 及格") gg = "60";
                            else if (gg == "\r\n\t\t\t 不及格") gg = "0";
                            else if (gg == "\r\n\t\t\t 未考") gg = "0";
                            else if (gg == "\r\n\t\t\t 免修") gg = "0";        //BUGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG!!!
                            CurrentG = double.Parse(gg);
                        }
                        else if (counter == 7)     //Credit
                        {
                            CurrentC = double.Parse(cell.InnerText);
                            if (CurrentG == 0) CurrentC = 0;            //Do not statictic if failed the subject
                        }
                        else if (counter == 2)     //Code
                        {
                            CurrentCd = cell.InnerText;
                            line.SubItems.Add(cell.InnerText);
                        }
                        else if(counter==1)
                        {
                            CurrentSem = cell.InnerText;
                        }
                        else if (counter==3)
                        {
                            CurrentSbj = cell.InnerText;
                        }
                        else
                        {
                            line.SubItems.Add(cell.InnerText);
                        }
                    }
                    if (CurrentCd != "")
                    {
                        //subjects[num_sbj] = new sbj(CurrentCd, CurrentSem, CurrentSbj, CurrentG, CurrentC, num_sbj);
                        subject.Add(new sbj(CurrentCd, CurrentSem, CurrentSbj, CurrentG, CurrentC, num_sbj));
                        //Code[num_sbj] = CurrentCd;
                        //Semester[num_sbj] = CurrentSem;
                        //Subject[num_sbj] = CurrentSbj;
                        //Grade[num_sbj] = CurrentG;
                        //Credit[num_sbj] = CurrentC;
                        //IDnum[num_sbj] = num_sbj;
                    }
                }
            }

            subject.Sort(sbjsort);
            dataGridView1.DataSource = subject;

            label6.Visible = true;
            label5.Visible = false;
        }


        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            double tot_Credit = 0;
            double tot_Grade = 0;
            double tot_GPAh = 0;
            double tot_GPAw = 0;
            for (int i=0;i<num_sbj;i++)
            {
                tot_Grade += subject[i].Credit * subject[i].Grade;
                tot_Credit += subject[i].Credit;
                if (subject[i].Grade >= 95)
                    gpah[i] = 4.3;
                else if (subject[i].Grade >= 90)
                    gpah[i] = 4;
                else if (subject[i].Grade >= 85)
                    gpah[i] = 3.7;
                else if (subject[i].Grade >= 82)
                    gpah[i] = 3.3;
                else if (subject[i].Grade >= 78)
                    gpah[i] = 3;
                else if (subject[i].Grade >= 75)
                    gpah[i] = 2.7;
                else if (subject[i].Grade >= 72)
                    gpah[i] = 2.3;
                else if (subject[i].Grade >= 68)
                    gpah[i] = 2;
                else if (subject[i].Grade >= 66)
                    gpah[i] = 1.7;
                else if (subject[i].Grade >= 64)
                    gpah[i] = 1.3;
                else if (subject[i].Grade >= 60)
                    gpah[i] = 1;
                else
                    gpah[i] = 0;
                if (subject[i].Grade >= 85)
                    gpaw[i] = 4;
                else if (subject[i].Grade >= 75)
                    gpaw[i] = 3;
                else if (subject[i].Grade >= 65)
                    gpaw[i] = 2;
                else if (subject[i].Grade >= 60)
                    gpaw[i] = 1;
                else
                    gpaw[i] = 0;
                gpah[i] = gpah[i] * subject[i].Credit;
                tot_GPAh += gpah[i];
                gpaw[i] = gpaw[i] * subject[i].Credit;
                tot_GPAw += gpaw[i];
            }
            double avr_Grade = tot_Grade / tot_Credit;
            double avr_GPAh = tot_GPAh / tot_Credit;
            double avr_GPAw = tot_GPAw / tot_Credit;
            label1.Text = "总平均分："+avr_Grade.ToString();
            label2.Text = "总学分：" + tot_Credit.ToString();
            label3.Text = "工大GPA算法：" + avr_GPAh.ToString();
            label4.Text = "WES GPA算法：" + avr_GPAw.ToString();

            label6.Visible = false;
            label5.Visible = true;
            label15.Visible = true;
            button7.Visible = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                hfcampus = true;
                webBrowser1.Url = new Uri("http://121.251.19.29/");
            }
            else if (radioButton2.Checked)
            {
                hfcampus = false;
                webBrowser1.Url = new Uri("http://222.195.8.201/");
            }
            groupBox1.Visible = false;
            groupBox2.Visible = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String fName;
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.InitialDirectory = "C:\\";
            openfile.Filter = "ConfigFile|*.txt";
            openfile.RestoreDirectory = true;
            openfile.FilterIndex = 1;
            if(openfile.ShowDialog()==DialogResult.OK)
            {
                fName = openfile.FileName;
                String jsonstr = File.ReadAllText(fName);
                //label15.Text = jsonstr;
                double ans = CalcGPAfromjson(jsonstr);
                label15.Text = "自定义GPA算法：" + ans.ToString();
            }
        }
    }

    class custom_GPA
    {
        public double level { get; set; }
        public double grade { get; set; }
    }

    class sbj
    {
        public int ID { get; set; }
        public String Semester { get; set; }
        public String Subject { get; set; }
        public String Code { get; set; }
        public double Grade { get; set; }
        public double Credit { get; set; }
        public sbj(String cd, String sem, String sbj, double g, double c, int id)
        {
            Semester = sem;
            Subject = sbj;
            Code = cd;
            Grade = g;
            Credit = c;
            ID = id;
        }
    }

}
