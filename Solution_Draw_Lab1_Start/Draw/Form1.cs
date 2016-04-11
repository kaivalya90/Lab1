using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.SqlClient;

using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;


namespace Draw
{
	public partial class Form1 : Form
	{
		Point pt1;
        List<Point> pointlist=new List<Point>();
        Color penColor = Color.FromArgb(255, 0, 0);
        int penWidth = 2;
        List<Shape> shapeList = new List<Shape>();
        ShapeType currentShapeType = ShapeType.Line;
        Shape shape;
        bool dataModified = false;
		string currentFile; // = "drawing1.bin
      //  private string saveFileName;
        String filename;
        SqlConnection myConnection;
        SqlCommand myCommand; //database connection variables
        public Form1()
		{
			InitializeComponent();
			this.DoubleBuffered = true;
		}

	 	private void Form1_MouseDown(object sender, MouseEventArgs e)
		{
			if (currentShapeType == ShapeType.None)
			{
				MessageBox.Show("Must select shape type");
				return;
			}

            //for clustering
        //    pointlist.Add(e.Location);

			pt1 = e.Location;
			Console.WriteLine("currentShapeType = {0}", currentShapeType);
            shape = Shape.CreateShape(currentShapeType);
			shape.Pt1 = pt1;
			shape.PenColor = Color.Black;
			//shape.Pen = penTemp;
			Shape.MouseIsDown = true;
			Shape.CurrentShape = shape;
            if (currentFile == null) //see if the current filename is null
                this.Text = "Untitled"; //label the form untitiled in case filename is null
            else
                this.Text= currentFile; //else filename will be label of the form
         
		}

		private void Form1_MouseMove(object sender, MouseEventArgs e)
		{
			if (Shape.MouseIsDown && e.Button == MouseButtons.Left)
			{
				shape.Pt2 = e.Location;
                //      pointlist.Add(e.Location);

                Invalidate();
			}
		}

		private void Form1_MouseUp(object sender, MouseEventArgs e)
		{
            if (e != null)
            {
                if (e.Button == MouseButtons.Right)
                    return; // Don't respond to right mouse button up
                //Graphics g = this.CreateGraphics();
                if (e.Location != null && shape!=null)
                {
                    shape.Pt2 = e.Location;
            //        pointlist.Add(e.Location);
                    shape.PenColor = penColor;
                    shape.PenWidth = penWidth;
                    //penFinal = new Pen(penColor, penWidth);
                    shapeList.Add(shape);
                    Shape.MouseIsDown = false;
                    Invalidate();
                    dataModified = true;
                }
            }
        }

		private void Form1_Paint(object sender, PaintEventArgs e)
		{
			// Draw shapes already in shapeList
            foreach (Shape s in shapeList)
                s.Draw(e.Graphics);
			// Draw current shape, if any
			if (Shape.MouseIsDown && shape != null)
				shape.Draw(e.Graphics);
        }

		private void penWidthMenuItem_Click(object sender, EventArgs e)
		{
			PenDialog dlg = new PenDialog();
            dlg.PenWidth = penWidth;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                penWidth = dlg.PenWidth;
            }
		}

        private void lineMenuItem_Click(object sender, EventArgs e)
        {
            currentShapeType = ShapeType.Line;
			clearShapeChecks();
			lineToolStripMenuItem.Checked = true;
        }

        private void rectangleMenuItem_Click(object sender, EventArgs e)
        {
            currentShapeType = ShapeType.Rectangle;
			clearShapeChecks();
			rectangleToolStripMenuItem.Checked = true;
		}

        private void freeLineMenuItem_Click(object sender, EventArgs e)
        {
            currentShapeType = ShapeType.FreeLine;
			clearShapeChecks();
			freeLineToolStripMenuItem.Checked = true;
		}

		private void textMenuItem_Click(object sender, EventArgs e)
		{
			currentShapeType = ShapeType.Text;
			FontDialog dlg = new FontDialog();
			DialogResult dr = dlg.ShowDialog();
			if (dr == DialogResult.OK)
				Shape.CurrentFont = dlg.Font;
			clearShapeChecks();
			textToolStripMenuItem.Checked = true;
		}

        private void printShapesMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("\nAll Shapes");
            foreach (Shape shape in shapeList)
                Console.WriteLine(shape);
        }

        private void penColorMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = penColor;
            if (dlg.ShowDialog() == DialogResult.OK)
                penColor = dlg.Color;

        }

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (currentShapeType == ShapeType.Text)
			{
				Text t = (Text)shape;
				if (e.KeyCode == Keys.Enter)
					t.Open = false;
				if (e.KeyCode == Keys.Back && t.Open)
				{
					t.TextLine = t.TextLine.Substring(0, t.TextLine.Length - 1);
					Invalidate();
					Update();
				}
			}
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (currentShapeType == ShapeType.Text)
			{
				if (((Text)shape).Open == false)
					return;
				if (e.KeyChar == '\b')
					return;

				((Text)shape).TextLine += e.KeyChar;
				Graphics g = this.CreateGraphics();
				shape.Draw(g);
			}
		}
	

		
		
		public void clearShapeChecks()
		{
			lineToolStripMenuItem.Checked = false;
			rectangleToolStripMenuItem.Checked = false;
			freeLineToolStripMenuItem.Checked = false;
			textToolStripMenuItem.Checked = false;
		}

		private void saveAsMenuItem_Click(object sender, EventArgs e)
		{
            // YOUR CODE HERE
            StreamWriter str = null;
            BinaryWriter bw = null;
            SaveFileDialog svd = new SaveFileDialog();
            svd.Filter = " txt files (*.txt) | *.txt | binary files(*.bin) | *.bin";
           // svd.DefaultExt = Path.GetExtension(filename);
            svd.RestoreDirectory = true;  //to get back to the same directory where the current file is saved
            svd.FileName = currentFile; //to retain the filename            
            DialogResult = svd.ShowDialog();
            String shapeData = null;
            Console.WriteLine("Clicked on save as menu");
            if (DialogResult == DialogResult.OK)
            {
                if ((svd.FileName != null) && (svd.FilterIndex == 1))//if text file 
                {
                    str = new StreamWriter(svd.FileName); //storing the file name.
                    currentFile = Path.GetFileNameWithoutExtension(svd.FileName); //get only file name 
                    foreach (Shape s in shapeList)//for each shape 
                    {
                        s.writeText(str);      //writing each shape details in text to the file         
                    }
                    Console.WriteLine("The shape details : " + shapeData);
                    str.Close();//close the writeStream
                }
                if (svd.FilterIndex == 2)//if binary file
                {
                    bw = new BinaryWriter(File.Open(svd.FileName, FileMode.Create)); //storing the file name.

                    currentFile = Path.GetFileNameWithoutExtension(svd.FileName); //get only file name 
                    foreach (Shape s in shapeList)//for each shape 
                    {                       
                        s.writeBinary(bw);   //write in binary file                                     
                    }                                      
                    bw.Close(); //closing the filestream after writing the data to the file
                }
            }
            filename = svd.FileName;
            this.Text = currentFile;
        }

        /*
        newMenu: check if the current changes are saved to the file, if not ask user if he wants to and then open the file
         */
        private void newMenuItem_Click(object sender, EventArgs e)
		{
            // YOUR CODE HERE
            SaveFileDialog saveFileDialogObject = new SaveFileDialog();
            Graphics g = this.CreateGraphics();
            Console.WriteLine(dataModified);
           
            g.Clear(Form1.DefaultBackColor); //clear the form
            shapeList.Clear(); //clear the shape list 
            dataModified = false;//This is to just check that even if you click an empty form with a new button click event it will not ask you to save file without drawing something
            Text = "Untitled";//change the form label to indicate it is not any file 
            currentFile = null;
        }

        /*
        save file: ask user where to save the file and save it according to the extension
        */

       
        private void exitMenuItem_Click(object sender, EventArgs e)
		{
            /* SaveFileDialog saveFileDialogObject = new SaveFileDialog();
             if (dataModified)//save the current changes before exit
             {
                 // YOUR CODE HERE

                 DialogResult result = new DialogResult();
                 result = MessageBox.Show("Do you want to save the file?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                 if (result == DialogResult.No)
                 {
                     Application.Exit();
                 }
                 else if (result == DialogResult.Yes)
                 {
                     saveFileDialogObject.Filter = "Text files (*.txt)|*.txt|Binary Files(*.bin)|*.bin|All files (*.*)|*.*"; // Filter files by extension
                     if (saveFileDialogObject.ShowDialog() == DialogResult.OK)
                     {
                         saveFile(saveFileDialogObject);
                     }
                 }
                 else
                 {
                     return;                     //dont respond to cancel, return to the form page
                 }

             }*/


            if (myConnection!=null && myConnection.State == ConnectionState.Open)
                myConnection.Close();
            Console.WriteLine("closed the database connections");
            Application.Exit();
		}

        private void Form1_Load(object sender, EventArgs e)
        {
            //    Exception e;
            string name="";
            string username = "";
            string db = "";
            string servern = "";
            string password="";
            string[] words;
            string line;
            try
            {
                Console.WriteLine("Loaded the from");
             
                Console.WriteLine(Directory.GetCurrentDirectory());
               
               // var path = Path.Combine(Directory.GetCurrentDirectory(), "\\ConnectionData.txt");
                string p = Directory.GetCurrentDirectory() + "\\ConnectionData.txt";
             //   Console.WriteLine(p);
                System.IO.StreamReader file =   new System.IO.StreamReader(p);
                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    line.Trim();
                    words = line.Split('=');
                    if (words[0] == "username")
                        username = words[1];
                    else if (words[0] == "password")
                        password = words[1];
                    else if (words[0] == "server")
                        servern = words[1];
                    else if (words[0] == "database")
                        db = words[1];

                 
                }

          
                Console.WriteLine(username + "  " + password);
                myConnection = new SqlConnection("server="+ servern + "\\SQLEXPRESS;" +
                                     " user id ="+username +";"+
                                    " password="+password +";"+
                                       " Trusted_Connection=yes;" +
                                       " connection timeout=30");
                Console.WriteLine(myConnection.State);
                if (myConnection.State != ConnectionState.Open)
                    myConnection.Open();

                SqlCommand s = new SqlCommand();
                s.CommandText = " SELECT name FROM sys.databases WHERE name = '"+db+"'";
                s.Connection = myConnection;
                SqlDataReader sd = s.ExecuteReader();
                if(sd.Read())
                {
                    name = (string)sd["name"];
                }
                sd.Close();
                if (string.IsNullOrEmpty(name))
                {
                    string s1 = Directory.GetCurrentDirectory() + "\\script1.sql";
                    string s2 = Directory.GetCurrentDirectory() + "\\script2.sql";
                    string script1 = File.ReadAllText(s1);
                    string script2 = File.ReadAllText(s2);

                    Server server = new Server(new ServerConnection(myConnection));
                    server.ConnectionContext.ExecuteNonQuery(script1);
                    server.ConnectionContext.ExecuteNonQuery(script2);
                    myConnection.Close();
                    
                }
                myConnection = new SqlConnection("server = "+ servern + "\\SQLEXPRESS; " +
                                                               " user id =" + username + ";" +
                                    " password=" + password + ";" +
                                                          "database="+db+";" +
                                                             "Trusted_Connection=yes;" +
                                                             "connection timeout=30");
                if (myConnection.State != ConnectionState.Open)
                    myConnection.Open();                

            }

            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            } 
        }

        private void populateDropDown(ToolStripMenuItem toolStripMenuItem2, SqlConnection conn)
        {
            SqlDataReader reader = null;
            string Drw_name=null;
            SqlCommand command = new SqlCommand("Select * from Drawing",conn);
            //  command.Connection = myConnection;
            reader = command.ExecuteReader();
            while(reader.Read())
            {
                //  String name = reader["DrawingName"].ToString();
                ToolStripItem ts = new ToolStripMenuItem();
                ts.Text = reader["Drawing_Name"].ToString();
                ts.Click += new EventHandler(ts_click);
               // Console.WriteLine(ts.Text);
              
                toolStripMenuItem2.DropDownItems.Add(ts);               

            }

            reader.Close();
        }

        private void ts_click(object sender, EventArgs e)
        {
            Console.WriteLine("  " + sender.ToString());
            toolStripTextBox1.Text = sender.ToString();
            SqlDataReader reader = null;
            SqlCommand com = new SqlCommand();
            com.Connection = myConnection;
            Graphics g = this.CreateGraphics();
            g.Clear(Form1.DefaultBackColor); //clear the form
            shapeList.Clear();
            com.CommandText="select l.* from Line l "+
                "join Drawing d on l.pictureid=d.drawing_id where drawing_name='" + sender.ToString()+"'";
            Console.WriteLine(com.CommandText);
           

            try {
                reader = com.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("readine line ");
                    Shape s1 = new Line();
                    s1.getData(reader);
                    s1.Draw(g);
                    Console.WriteLine(s1.ToString());
                    shapeList.Add(s1);

                }
                reader.Close();
                SqlCommand com1=new SqlCommand();
                com1.Connection = myConnection;
                com1.CommandText = "select r.* from Rectangle r " +
               "join Drawing d on r.pictureid=d.drawing_id where drawing_name='" + sender.ToString() + "'";
                reader = com1.ExecuteReader();
                Console.WriteLine(com1.CommandText);
                while (reader.Read())
                {
                    Shape s2 = new Rect();
                    s2.getData(reader);
                    s2.Draw(g);
                    Console.WriteLine("Rectangle data "+s2.ToString());
                    shapeList.Add(s2);
                }
                reader.Close();

                SqlCommand comm2 = new SqlCommand();

                comm2.CommandText = "select * from Text where textid in (select textid from draw_text where drawid= (select drawing_id from drawing where drawing_name='" + sender.ToString() + "'))";
                Console.WriteLine("query to get the text data" + comm2.CommandText);
                comm2.Connection = myConnection;
                reader = comm2.ExecuteReader();
                while(reader.Read())
                {
                    Shape t = new Text();
                    t.getData(reader);
                    t.Draw(g);
                    shapeList.Add(t);
                    Console.WriteLine("Text to be added to the paint screen "+t.ToString());
                }
                reader.Close();                
                
                String q = "select * from freeline where freelineid in (select freelineid from draw_freeline where drawid= (select drawing_id from drawing where drawing_name='" + sender.ToString() + "'))";
                SqlDataAdapter adapter= new SqlDataAdapter(q, myConnection);
                IDbCommand selectCommand = myConnection.CreateCommand();
                DataTable dt = new DataTable();            
                DataSet selectResults = new DataSet();
                adapter.Fill(selectResults); // get dataset
                selectCommand.Dispose();               
                foreach (DataRow row in selectResults.Tables[0].Rows)
                {
                    FreeLine f = new FreeLine();
                    f.getData(row, myConnection);
                    f.Draw(g);
                    shapeList.Add(f);
                    Console.WriteLine("freeline to be added to the paint screen " + f.ToString());
                }
                selectResults.Clear();
                dataModified = false;

                /* while (reader1.Read())
                 {
                     FreeLine f = new FreeLine();
                     f.getData(reader1, myConnection);
                     f.Draw(g);
                     shapeList.Add(f);
                     Console.WriteLine("freeline to be added to the paint screen " + f.ToString());
                 }*/


            }



            catch (Exception ec)
            {
                if (myConnection.State == ConnectionState.Open)
                    myConnection.Close();
                reader.Close();
                Console.WriteLine("Error " + ec.Message);
            }

          



        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            int Drawid=0;

            Console.WriteLine(toolStripTextBox1.Text);
            if (toolStripTextBox1.Text=="")
            {
                MessageBox.Show("Please enter drawing Name", "Empty Name",
              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            else
            {
                try {
                    if (dataModified == true)
                    {
                        SqlCommand com = new SqlCommand();
                        com.CommandText = "delete from freeline_point where freelineid in(select freelineid from draw_freeline where drawid =(select drawing_id from drawing where drawing_name='" + toolStripTextBox1.Text + "'))";
                        com.Connection = myConnection;
                        Console.WriteLine(com.CommandText);
                        com.ExecuteNonQuery();
                        com.CommandText = "delete from point  where pointid not in (select pointid  from freeline_point)";
                        Console.WriteLine(com.CommandText);
                        com.ExecuteNonQuery();

                        com.CommandText = "delete from draw_freeline where drawid=(select drawing_id from drawing where drawing_name='" + toolStripTextBox1.Text + "')";
                        Console.WriteLine(com.CommandText);
                        com.ExecuteNonQuery();
                        com.CommandText = "delete from text where textid in  (select textid from draw_text where drawid=(select drawing_id from drawing where drawing_name='" + toolStripTextBox1.Text + "'))";
                        Console.WriteLine(com.CommandText);
                        com.ExecuteNonQuery();
                        com.CommandText = "delete from drawing where drawing_name='" + toolStripTextBox1.Text + "'";

                        Console.WriteLine(com.CommandText);
                        com.ExecuteNonQuery();

                        com.CommandText = "Insert into Drawing values('" + toolStripTextBox1.Text + "')";
                        com.Connection = myConnection;
                        com.ExecuteNonQuery();
                        com.CommandText = "select Drawing_id from drawing where Drawing_name='" + toolStripTextBox1.Text + "'";
                        SqlDataReader reader = com.ExecuteReader();
                        if (reader.Read())
                            Drawid = Convert.ToInt32(reader["Drawing_id"].ToString());
                        reader.Close();
                        foreach (Shape s in shapeList)//for each shape 
                        {
                            Console.WriteLine("Adding shape to DB is " + s.GetType());
                            
                            if (Drawid != 0)
                                s.putData(myConnection, Drawid);      //writing each shape details in text to the file         
                            else
                                Console.WriteLine("Cant Add the shape to the DB");
                        }
                    }
                }
                catch(Exception exc)
                {
                    Console.WriteLine(" Error: " + exc.Message);
                    if (myConnection!=null && myConnection.State == ConnectionState.Open)
                        myConnection.Close();
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
          
            try
            {

                myCommand = new SqlCommand();
                myCommand.Connection = myConnection;
                //      myCommand.CommandText="INSErt INTO  dbo.Drawings (Drawing_name,Drawing_ID) values('Auro',1 )";
                //     myCommand.ExecuteNonQuery();
                //    toolStripMenuItem2.DropDownItems.Add("Auro");
                toolStripMenuItem2.DropDownItems.Clear();
               populateDropDown(toolStripMenuItem2, myConnection);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                if (myConnection.State == ConnectionState.Open)
                    myConnection.Close();

            }


        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            dataModified = true;
            
        }

        private void KMeans_Click(object sender, EventArgs e)
        {
               Graphics g = this.CreateGraphics();
          
            Console.WriteLine("" + pointlist.Count);
            foreach (Point p in pointlist)
            {
                Console.WriteLine(p.X + " " + p.Y);
            }

            int numclusters = 3;
            int count = pointlist.Count;
            Point[] clusters = new Point[numclusters];
            int[] clusterIndex = new int[count];
            clusters[0].X = 959 / 2;

            clusters[0].Y = 353/2;
            clusters[1].X = 10;
            clusters[1].Y = 290;
            clusters[2].X = 10;
            clusters[2].Y = 20;
            Console.WriteLine(pointlist.Count);

            for (int i = 0; i < numclusters; i++)
                Console.WriteLine(i + "th cluster" + clusters[i].X + " " + clusters[i].Y);


            //  double mindist = Double.MaxValue;
            for (int i = 0; i < 1000; i++)
            {

                for (int k = 0; k < pointlist.Count; k++)
                {
                    double mindist = Double.MaxValue;
                    for (int j = 0; j < numclusters; j++)
                    {
                        if (mindist > distance(pointlist[k], clusters[j]))
                        {
                            clusterIndex[k] = j;
                            mindist = distance(pointlist[k], clusters[j]);
                        }
                    }

                }
                for (int j = 0; j < numclusters; j++)
                {
                    double x = 0; double y = 0;
                    double c = 0;
                    for (int k = 0; k < pointlist.Count; k++)
                    {
                        if (clusterIndex[k] == j)
                        {
                            c++;
                            x = x + pointlist[k].X;
                            y = y + pointlist[k].Y;
                        }


                    }
                    if (c != 0)
                    {
                        x = x / c;
                        y = y / c;
                        clusters[j].X = (int)x;

                        clusters[j].Y = (int)y;
                    }
                }



            }

            for (int i = 0; i < numclusters; i++)
            {
                Console.WriteLine(i + "th cluster" + clusters[i].X + " " + clusters[i].Y);

                Shape rect = new Rect();
                rect.Pt1 = new Point(clusters[i].X + 1, clusters[i].Y + 1);
                rect.Pt2 = new Point(clusters[i].X - 1, clusters[i].Y - 1);
                rect.PenColor = Color.Green;
                rect.PenWidth = 5;
                Console.WriteLine(rect.ToString());
                rect.Draw(g);
                shapeList.Add(rect);
            }


        }

     

        private double distance(Point point1, Point point2)
        {
            double sumSquaredDiffs = 0;
            sumSquaredDiffs += Math.Sqrt(Math.Pow((point1.X - point2.X), 2)+Math.Pow((point1.Y-point2.Y),2));
         //   Console.WriteLine(sumSquaredDiffs);
            return sumSquaredDiffs;
        }
    }  // end class Form1   
            
}