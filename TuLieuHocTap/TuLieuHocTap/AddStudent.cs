﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Data.SqlClient;


namespace TuLieuHocTap
{
	public partial class AddStudent : Form
	{
        Capture grabber; //to open the camera 
        Image<Bgr, byte> currentFrame; //to capture image 
        Image<Gray, byte> gray, result, result2, result3, result4, TrainedFace = null, TrainedEyes = null, TrainedMouth = null, TrainedNose = null; //initializing as an empty object  

        //initializing hharcascade for face detection (detects in order)
        HaarCascade face; //detection by face
        HaarCascade eyes; //detectiom by eyes
        HaarCascade mouth; 
        HaarCascade nose;

        //initializing faces and name storage array 
        List<Image<Gray, byte>> detectedImages = new List<Image<Gray, byte>>();
        List<Image<Gray, byte>> detectedImages2 = new List<Image<Gray, byte>>();
        List<Image<Gray, byte>> detectedImages3 = new List<Image<Gray, byte>>();
        List<Image<Gray, byte>> detectedImages4 = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();

        int t = 0; //making t into 0 so that algorithm can increment it into 1 = True when face is found 
        int a = 0;
        int b = 0;
        int c = 0;

        //connection string to the database
        SqlConnection con;
        

        int NumLabels,ContTrain=0;



        byte[] imgface = null;
        byte[] imgeyes = null;
        byte[] imgmouth = null;
        byte[] imgnose = null;




        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MAINGUI main = new MAINGUI();
            main.Show();
            this.Close();
        }

        private void closeProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void button1_Click_1(object sender, EventArgs e) //start camera button
        {
            grabber = new Capture(); //when click camera wil be opened
            // 1.initializing the grabber event 
            grabber.QueryFrame();
            // 2.Now to capture the video 
            Application.Idle += new EventHandler(FrameGrabber); //if the application is idel and the camera is on then call the frame grabber event 
            // 3.initializing frame grabber 
        }

        private void button2_Click_1(object sender, EventArgs e) //extract features button
        {
            //a. Resizing detected faces to grey scale images
            try
            {
                TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //TrainedEyes = result2.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //TrainedMouth = result3.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //TrainedNose = result4.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //b. Image box to show the detected faces
                imageBox2.Image = TrainedFace;
                //imageBox3.Image = TrainedEyes;
                //imageBox4.Image = TrainedMouth;
                //imageBox5.Image = TrainedNose;
            }
            catch 
            {

                
            }
        }

        private void button3_Click_1(object sender, EventArgs e) //save face button
        {
            extractfeatures();
            addtodatabase(imgface);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
        

        public AddStudent()
		{
            //loading haarcascade file by file name and assining to haarcascade variable
            //Load haarcascades for face detection
            face = new HaarCascade("haarcascade-frontalface-default.xml");
            //Load haarcascades for eye detection
            eyes = new HaarCascade("haarcascade_mcs_eyepair_big.xml");
            //Load haarcascades for mouth detection
            mouth = new HaarCascade("mouth.xml");
            //Load haarcascades for nose detection
            nose = new HaarCascade("nose.xml");

			InitializeComponent();

            try
            {
                //Previous trained faces and labels for each image
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]); //total number of faces detected 
                ContTrain = NumLabels; //new images will be added to the previous set  
                string LoadFaces;
                string LoadEyes;
                string LoadMouth;
                string LoadNose;

                for (int tf = 1; tf <NumLabels+1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    //LoadEyes = "eyes" + tf + ".bmp";
                    //LoadMouth = "mouth" + tf + ".bmp";
                    //LoadNose = "nose" + tf + ".bmp";
                    TrainedFace.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    //TrainedEyes.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadEyes));
                    //TrainedMouth.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadMouth));
                    //TrainedNose.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadNose));
                    labels.Add(Labels[tf]);

     
                }
            }

            catch (Exception e)
            {
                MessageBox.Show("Press okay to continue");
            }

		}

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e) 
        {
            
        }

        private void addtodatabase(byte[] imgface) //adds to database
        {

            var txtpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionString.txt");
            StreamReader sr = new StreamReader(txtpath);
            String line = sr.ReadToEnd();


            using (SqlConnection Connection = new SqlConnection(@""+ line + ""))
            {
                if (textBox1.Text == String.Empty || textBox2.Text == String.Empty)
                {
                    MessageBox.Show("Error, Student details must be entered", "Value Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                else
                {
                    try
                    {
                        Connection.Open();
                        SqlCommand cmd = new SqlCommand(@"INSERT INTO student ([name], [studentid],[imgface]) VALUES (@studentname, @idstudent,@imgface);", Connection);
                        cmd.Parameters.AddWithValue("@studentname", textBox1.Text);
                        cmd.Parameters.AddWithValue("@idstudent", textBox2.Text);
                        cmd.Parameters.AddWithValue("@imgface", imgface);
                        int i = cmd.ExecuteNonQuery();
                        Connection.Close();

                        if (i == 1)
                        {
                            MessageBox.Show("Student has been added to the database");
                            getPrimaryKey();
                            textBox1.Clear();
                            textBox2.Clear();
                            
                        }
                        else
                        {
                            MessageBox.Show("Student couldn't be added, Please Try again");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unexpected Error has occured:" + ex.Message);

                    }
                }
               
            }

        }

        private void getPrimaryKey() //Give new id to the new item
        {

            var txtpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConnectionString.txt");
            StreamReader sr = new StreamReader(txtpath);
            String line = sr.ReadToEnd();

            using (SqlConnection Connection = new SqlConnection(@""+ line + ""))
            {
                try
                {
                    Connection.Open();
                    SqlCommand cmd = new SqlCommand(@"SELECT MAX(Id)+1 FROM student;", Connection);
                    Connection.Close();

                }

                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected Error has occured: " + ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) 
        {
            

        }

        public void extractfeatures()
        {
            ContTrain = ContTrain + 1;

            //detected faces will be saved into a folder with the name of the person 
            //setting commands
            detectedImages.Add(TrainedFace);
            //detectedImages2.Add(TrainedEyes);
            //detectedImages3.Add(TrainedMouth);
            //detectedImages4.Add(TrainedNose);

            labels.Add(textBox2.Text);
            //write name of the detected person into list 
            File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", detectedImages.ToArray().Length.ToString() + "%");
            //File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", detectedImages2.ToArray().Length.ToString() + "%");
            //File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", detectedImages3.ToArray().Length.ToString() + "%");
            //File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", detectedImages4.ToArray().Length.ToString() + "%");

            //write to files 
            for (int i = 1; i < detectedImages.ToArray().Length + 1; i++)
            {
                //save faces to folder with name face(i) i being the name/number of the face detected
                detectedImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                //Saves name to text file
                File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", labels.ToArray()[i - 1] + "%");

                FileStream fsstream = new FileStream(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp", FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fsstream);
                imgface = br.ReadBytes((int)fsstream.Length);

            }

            //for (int i = 1; i < detectedImages2.ToArray().Length + 1; i++)
            //{
            //    //save faces to folder with name face(i) i being the name/number of the face detected
            //    detectedImages2.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/eyes" + i + ".bmp");
            //    //Saves name to text file
            //    //File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", labels.ToArray()[i - 1] + "/");
            //    FileStream fsstream = new FileStream(Application.StartupPath + "/TrainedFaces/eyes" + i + ".bmp", FileMode.Open, FileAccess.Read);
            //    BinaryReader br = new BinaryReader(fsstream);
            //    imgeyes = br.ReadBytes((int)fsstream.Length);
            //}

            //for (int i = 1; i < detectedImages3.ToArray().Length + 1; i++)
            //{
            //    //save faces to folder with name face(i) i being the name/number of the face detected
            //    detectedImages3.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/mouth" + i + ".bmp");
            //    //Saves name to text file
            //    //File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", labels.ToArray()[i - 1] + "/");
            //    FileStream fsstream = new FileStream(Application.StartupPath + "/TrainedFaces/mouth" + i + ".bmp", FileMode.Open, FileAccess.Read);
            //    BinaryReader br = new BinaryReader(fsstream);
            //    imgmouth = br.ReadBytes((int)fsstream.Length);
            //}

            //for (int i = 1; i < detectedImages4.ToArray().Length + 1; i++)
            //{
            //    //save faces to folder with name face(i) i being the name/number of the face detected
            //    detectedImages4.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/nose" + i + ".bmp");
            //    //Saves name to text file
            //    //File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedNames.txt", labels.ToArray()[i - 1] + "/");
            //    FileStream fsstream = new FileStream(Application.StartupPath + "/TrainedFaces/nose" + i + ".bmp", FileMode.Open, FileAccess.Read);
            //    BinaryReader br = new BinaryReader(fsstream);
            //    imgnose = br.ReadBytes((int)fsstream.Length);
            //}


            //MessageBox.Show("Features saved to the database!");
        }

        private void button2_Click(object sender, EventArgs e) 
        {
           
            
        }

        void FrameGrabber(object sender, EventArgs e) //Frame grabber event 
        {
            //initialize current frame with query grabber which is catching the frame
            currentFrame = grabber.QueryFrame().Resize(501, 407, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC); //resizing the frame with cubic frame
            //currentFrame2 = grabber.QueryFrame().Resize(400, 300, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            //1. Converting image frame to gray scale (image processing) 
            gray = currentFrame.Convert<Gray, Byte>();
            //gray2 = currentFrame.Convert<Gray, Byte>();

            //2. Detecting face by using Haar Classifier 
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 1, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            //MCvAvgComp[][] eyesDetected = gray.DetectHaarCascade(eyes, 1.2, 5, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            //MCvAvgComp[][] mouthDetected = gray.DetectHaarCascade(mouth, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(50, 50));
            //MCvAvgComp[][] noseDetected = gray.DetectHaarCascade(nose, 1.2, 5, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            //face is name of the haar cascade, giving sizes to the cascade, applying canny pruning on haar classifier 

            //3. Checking each frame of image processed by the classifer through ImageBox (video is processed as image frames for face detection), then detect face
            foreach (MCvAvgComp f in facesDetected[0])  
            {
                //a. If face is detected then increment t into 1 = True 
                t = t + 1;
                //b. Copy detected face in a frame name as result (gray.result)
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //resize copied frame and make it as cubic
                //view the result (detected image, face), convert current frame to grey scale 

                //c. Drawing traingle around on detected image (face) 
                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);

            }

            //foreach (MCvAvgComp f in eyesDetected[0])
            //{
            //    //a. If face is detected then increment t into 1 = True 
            //    a = a + 1;
            //    //b. Copy detected face in a frame name as result (gray.result)
            //    result2 = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            //    //resize copied frame and make it as cubic
            //    //view the result (detected image, face), convert current frame to grey scale 

            //    //c. Drawing traingle around on detected image (face) 
            //    currentFrame.Draw(f.rect, new Bgr(Color.White), 2);

            //}

            //foreach (MCvAvgComp f in mouthDetected[0])
            //{
            //    //a. If face is detected then increment t into 1 = True 
            //    b = b + 1;
            //    //b. Copy detected face in a frame name as result (gray.result)
            //    result3 = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            //    //resize copied frame and make it as cubic
            //    //view the result (detected image, face), convert current frame to grey scale 

            //    //c. Drawing traingle around on detected image (face) 
            //    currentFrame.Draw(f.rect, new Bgr(Color.Green), 2);

            //}

            //foreach (MCvAvgComp f in noseDetected[0])
            //{
            //    //a. If face is detected then increment t into 1 = True 
            //    c = c + 1;
            //    //b. Copy detected face in a frame name as result (gray.result)
            //    result4 = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            //    //resize copied frame and make it as cubic
            //    //view the result (detected image, face), convert current frame to grey scale 

            //    //c. Drawing traingle around on detected image (face) 
            //    currentFrame.Draw(f.rect, new Bgr(Color.Blue), 2);

            //}
            
            //View current frame in the imported ImageBox
            imageBox1.Image = currentFrame; //current frame = captured from the camera into the imagebox
            //initialize the currentframe
        }
    }
}
