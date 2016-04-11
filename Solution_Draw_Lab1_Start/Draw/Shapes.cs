using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Data;

namespace Draw
{
    public enum ShapeType { Line, Rectangle, FreeLine, Text, None };

    public abstract class Shape
	{
		public static bool MouseIsDown;	// Client program must set to true while drawing any shape using the mouse.
										// (Set in MouseIsDown handler when drawing; clear in MouseUp handler)
										// When initially displaying a file read from disk, MouseIsDown should be false.
		public static Shape CurrentShape;	// The shape objext that is currently being drawn with the mouse.
										// Client program must set in MouseIsDown handler when new shape is created
		public static Font CurrentFont = new Font("Courier New", 10);
		public static Encoding TextEncodingType;

		//public Pen Pen {get; set;}
		public Color PenColor { get; set; }
		public int PenWidth { get; set; }
		public Point Pt1 { get; set; }
		public Point Pt2 { get; set; }	// Not a member of the final shape for some figures but needed
										// for drawing temporary shapes (e.g., Text shape)- allows MouseIsDown
										//  handler in client program to save new point.
		public abstract void Draw(Graphics g);
		public abstract void writeBinary(BinaryWriter bw);
		public abstract void readBinary(BinaryReader br);
		public abstract void writeText(StreamWriter sw);
		public abstract void readText(StreamReader sr);
        public abstract void getData(SqlDataReader reader);
        public abstract void putData(SqlConnection conn,int drawID);

        public static Shape CreateShape(ShapeType type)
        {
            switch (type)
            {
                case ShapeType.Line:
                    return new Line();
                case ShapeType.Rectangle:
                    return new Rect();
                case ShapeType.FreeLine:
                    return new FreeLine();
				case ShapeType.Text:
					return new Text();
                default:
                    return null;
            }
        }

        

        /////////////////// ADD UTILITY FUNCTIONS HERE IF DESIRED //////////////////////

    }  // End Shape class

    public class Line : Shape
	{ 
		public override void Draw(Graphics g)
		{
			Pen pen = new Pen(PenColor, PenWidth);
			g.DrawLine(pen, Pt1, Pt2);
		}

		public override string ToString()
		{
			string s = string.Format("Line: ({0},{1}), ({2},{3}), {4}, {5})",
				Pt1.X, Pt1.Y, Pt2.X, Pt2.Y, PenWidth, PenColor);
			return s;
		}
        public override void getData(SqlDataReader reader)
        {
            int x1 = Convert.ToInt32(reader["X1"].ToString());
            int y1 = Convert.ToInt32(reader["Y1"].ToString());
            int x2 = Convert.ToInt32(reader["X2"].ToString());
            int y2 = Convert.ToInt32(reader["Y2"].ToString());
            PenColor = ColorTranslator.FromHtml((string)reader["Color"]);
            PenWidth = Convert.ToInt32(reader["penwidth"]);
            Console.WriteLine(" " + x1 + " " + y1 + " " + x2 + " " + y2);
            Pt1 = new Point(x1, y1);
            Pt2 = new Point(x2, y2);
            //PenColor = Color.Black;

           
        }
		public override void writeBinary(BinaryWriter bw)
		{
            // YOUR CODE HERE
            bw.Write("Line");
            bw.Write(Pt1.X);
            bw.Write(Pt1.Y);
            bw.Write(Pt2.X);
            bw.Write(Pt2.Y);
            bw.Write((int)PenWidth);
            bw.Write(PenColor.ToArgb());

        }

		public override void readBinary(BinaryReader br)
		{
            // YOUR CODE HERE
            //get line details
            int p1x=br.ReadInt32(); //reading x and y coordinate of the points
            int p1y = br.ReadInt32();
            int p2x = br.ReadInt32();
            int p2y = br.ReadInt32();
            Pt1 = new Point(p1x, p1y);
            Pt2 = new Point(p2x, p2y);
            PenWidth = br.ReadInt32();
            PenColor = Color.FromArgb(br.ReadInt32());
            Console.WriteLine("The shape details: "+ToString());

        }
        /*
        write line text format: 'Line: (144,38), (144,38), 4, FF00FF00)'  which is same as that of rectangle
            */

        public override void writeText(StreamWriter sw)
		{
            // YOUR CODE HERE

            string s = string.Format("Line: ({0},{1}), ({2},{3}), {4}, {5})",
                Pt1.X, Pt1.Y, Pt2.X, Pt2.Y, PenWidth, PenColor.A.ToString("X2")+ PenColor.R.ToString("X2") + PenColor.G.ToString("X2") + PenColor.B.ToString("X2"));
            sw.WriteLine(s);
        }

		public override void readText(StreamReader sr)
		{
            // YOUR CODE HERE
            String line;
            int a, r, g, b;
            if ((line = sr.ReadLine()) != null)
            {

                line = line.Replace(" ", String.Empty);
                Console.WriteLine(line);
                char[] delimiterChars = {',',')','(','\t',':',' '};
                String[] attributes = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);            
                Pt1 = new Point(int.Parse(attributes[0]), int.Parse(attributes[1]));
                Pt2 = new Point(int.Parse(attributes[2]), int.Parse(attributes[3]));
                PenWidth = int.Parse(attributes[4]);
                var argb = Convert.ToInt32(int.Parse(attributes[5], System.Globalization.NumberStyles.HexNumber));
                a = int.Parse(attributes[5].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                r = int.Parse(attributes[5].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                g = int.Parse(attributes[5].Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                b = int.Parse(attributes[5].Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                PenColor = Color.FromArgb(a, r, g, b);

            }
            Console.WriteLine("Details of Line " + ToString());
        }

        public override void putData(SqlConnection conn,int drawID)
        {
            SqlCommand com = new SqlCommand();
            try
            {
                String clr = "#" + PenColor.Name;
                com.Connection = conn;
                com.CommandText = "insert into Line (PictureID, X1,Y1,X2,Y2,color,penwidth) values (" + drawID + "," + Pt1.X + "," + Pt1.Y + "," + Pt2.X + "," + Pt2.Y + ",'"+ ColorTranslator.ToHtml(PenColor).ToString() + "',"+PenWidth+")";
                Console.WriteLine("While insertion into line" + com.CommandText);
                com.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
            }
        }
    } // End line class

    public class Rect : Line
    {

        public override void Draw(Graphics g)
        {
			int x = Math.Min(Pt1.X, Pt2.X);
			int y = Math.Min(Pt1.Y, Pt2.Y);
			int width = Math.Abs(Pt2.X - Pt1.X);
			int height = Math.Abs(Pt2.Y - Pt1.Y);
            Rectangle rect = new Rectangle(x, y, width, height);
			Pen pen = new Pen(PenColor, PenWidth);
            g.DrawRectangle(pen, rect);
        }
        public override void getData(SqlDataReader reader)
        {
            Console.WriteLine("reading rectangle data");
            int x1 = Convert.ToInt32(reader["X1"]);
            int y1 = Convert.ToInt32(reader["Y1"]);
            int x2 = Convert.ToInt32(reader["X2"]);
            int y2 = Convert.ToInt32(reader["Y2"]);
            PenColor = System.Drawing.ColorTranslator.FromHtml((string)reader["Color"]);
         //   Console.WriteLine("color got is " + PenColor.ToKnownColor());
            PenWidth = Convert.ToInt32(reader["penwidth"]);
            Console.WriteLine(" " + x1 + " " + y1 + " " + x2 + " " + y2);
            Pt1 = new Point(x1, y1);
            Pt2 = new Point(x2, y2);
         //    PenColor = Color.Black;
        }
        public override string ToString()
		{
			string s = string.Format("Rectangle: ({0},{1}), ({2},{3}), {4}, {5})",
						Pt1.X, Pt1.Y, Pt2.X, Pt2.Y, (int)PenWidth, PenColor);
			return s;

		}

		public override void writeBinary(BinaryWriter bw)
		{
            // YOUR CODE HERE
            bw.Write("Rectangle");
            bw.Write(Pt1.X);
            bw.Write(Pt1.Y);
            bw.Write(Pt2.X);
            bw.Write(Pt2.Y);
            bw.Write((int)PenWidth);
            bw.Write(PenColor.ToArgb());
		}

		public override void readBinary(BinaryReader br)
		{
            // YOUR CODE HERE
//get rect details
            int p1x = br.ReadInt32(); //reading x and y coordinate of the points
            int p1y = br.ReadInt32();
            int p2x = br.ReadInt32();
            int p2y = br.ReadInt32();
            Pt1 = new Point(p1x, p1y);
            Pt2 = new Point(p2x, p2y);
            PenWidth = br.ReadInt32();
            PenColor = Color.FromArgb(br.ReadInt32());
            Console.WriteLine("The shape details: " + ToString());
        }
        /*
        write rectangle text format: 'Rectangle: (64,41), (186,127), 4, FF00FF00)'
        */
        public override void writeText(StreamWriter sw)
		{
            // YOUR CODE HERE
         //   Console.WriteLine("Alpha of the color : " + PenColor.A.ToString("X"));

            string s = string.Format("Rectangle: ({0},{1}), ({2},{3}), {4}, {5})",
                        Pt1.X, Pt1.Y, Pt2.X, Pt2.Y, (int)PenWidth, PenColor.A.ToString("X2") + PenColor.R.ToString("X2") + PenColor.G.ToString("X2") + PenColor.B.ToString("X2"));
            sw.WriteLine(s);
        }
        public override void putData(SqlConnection conn,int drawID)
        {
            SqlCommand com = new SqlCommand();
            
            try
            {
                String clr = "#" + PenColor.Name;
                com.Connection = conn;
                com.CommandText = "insert into rectangle (PictureID, X1,Y1,X2,Y2,color,penwidth) values (" + drawID + "," + Pt1.X + "," + Pt1.Y + "," + Pt2.X + "," + Pt2.Y +",'"+ ColorTranslator.ToHtml(PenColor).ToString() + "',"+PenWidth+")";
                Console.WriteLine("While insertion into rectangle" + com.CommandText);
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error " + ex.Message);
                if (conn.State ==ConnectionState.Open)
                    conn.Close();
            }

        }

		public override void readText(StreamReader sr)
		{
            // YOUR CODE HERE
            String line;
            int a, r, g, b;
         
           if((line = sr.ReadLine()) != null)
            {
                char[] delimiters = { ',', ')', '(', ':' ,' '};
                String[] attributes = line.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);

                Console.WriteLine("Inside the line:  " + attributes[0]);
                Pt1 = new Point(int.Parse(attributes[0]), int.Parse(attributes[1]));                
                Pt2= new Point(int.Parse(attributes[2]), int.Parse(attributes[3]));
                PenWidth = int.Parse(attributes[4]);
                a = int.Parse(attributes[5].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                r = int.Parse(attributes[5].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                g = int.Parse(attributes[5].Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                b = int.Parse(attributes[5].Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                PenColor = Color.FromArgb(a, r, g, b);

            }

            Console.WriteLine("Details of rectangle " +ToString());
		}

    } // End Rect class

    public class FreeLine : Shape
    {
        public List<Point> FreeList {get; set;}
      
		public FreeLine()
			: base()
		{
			FreeList = new List<Point>();
			//FreeList.Add(Point.Empty);
			//FreeList.Add(Point.Empty);
		}

        public override void Draw(Graphics g)
        {
			// If this call to Draw is drawing the shape that is currently being drawn, then add
			// Pt2 to the list.  If the shape being drawn is a FreeLine already in the list,
			// do not add Pt2.
			if (Shape.MouseIsDown && this == Shape.CurrentShape)
				FreeList.Add(Pt2);  // Client program must set Pt2 to the new point on MouseMove
                                    // while drawing a FreeLine (like it does for all shapes)
            Console.WriteLine("Drawing Freeline");
            Console.WriteLine(FreeList.Count);
			if (FreeList.Count > 1)
			{
				Point[] ptArray = FreeList.ToArray();
				Pen pen = new Pen(PenColor, PenWidth);
				g.DrawLines(pen, ptArray);
			}
        }

		public override string ToString()
		{
			string s = string.Format("FreeLine: ({0},{1})", (int)PenWidth, PenColor);
			foreach(Point p in FreeList)
				s += string.Format("({0},{1}) ", p.X, p.Y);
			return s;
		}


        public override void writeBinary(BinaryWriter bw)
        {
            // YOUR CODE HERE

            bw.Write("FreeLine");
            bw.Write((int)PenWidth);
            bw.Write(PenColor.ToArgb());
            Point[] pts = FreeList.ToArray();
            bw.Write(pts.Length);
            foreach (Point p in FreeList)
            { 
                    bw.Write(p.X);
                    bw.Write(p.Y);

            }
           bw.Write("\n");
		}

		public override void readBinary(BinaryReader br)
		{
            // YOUR CODE HERE
            //reading freeline data 
            PenWidth = br.ReadInt32();
            PenColor = Color.FromArgb(br.ReadInt32());
            int i;
            int num_points = br.ReadInt32(); //read the number of points in the freeline
            for (i=0;i<num_points;i++)
            {
                Point currentPt = new Point(br.ReadInt32(), br.ReadInt32()); //read the coordinates of each point
                FreeList.Add(currentPt);//add the point to the freelist
            }           
          
        }
        /*
        WriteText for freeline format : 'FreeLine: (2,FFFF0000)(43,57) (42,65) (42,78) (42,86) (42,100)'
        
        */
        public override void writeText(StreamWriter sw)
		{
            // YOUR CODE HERE
            string s = string.Format("FreeLine: ({0},{1})", (int)PenWidth, PenColor.A.ToString("X2") + PenColor.R.ToString("X2") + PenColor.G.ToString("X2") + PenColor.B.ToString("X2"));
            foreach (Point p in FreeList)
                s += string.Format("({0},{1}) ", p.X, p.Y);
            sw.WriteLine(s);
        }


		public override void readText(StreamReader sr)
		{
            // YOUR CODE HERE

            //reading the free line 
            string line;
            int a, r, g, b;
            if ((line = sr.ReadLine()) != null)
            {
                char[] delimiters = { ',', ')', '(', ':',' ' }; //split the line using delimiters
                String[] attributes = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);//remove the blank entries
                PenWidth = int.Parse(attributes[0]);
                a = int.Parse(attributes[1].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);//convert hex string to color
                r = int.Parse(attributes[1].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                g = int.Parse(attributes[1].Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                b = int.Parse(attributes[1].Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                PenColor = Color.FromArgb(a, r, g, b);

                Console.WriteLine("The number of points of free line " + (attributes.Length - 2)/2); //number of points = (no of coordinates)/2
                for (int i = 2; i < ((attributes.Length - 2) / 2); i=i+2)//get the coordinates of all free points
                {
                    Point point = new Point(int.Parse(attributes[i]), int.Parse(attributes[i+1]));
                    FreeList.Add(point);
                }

                Console.WriteLine("Details of free line " + ToString());
            }

        }

        public  void getData(DataRow reader,SqlConnection conn)
        {
            int id; int x; int y;
            Point p;
            SqlDataReader read;
            PenColor = System.Drawing.ColorTranslator.FromHtml((string)reader["Color"]);
            id = Convert.ToInt32(reader["freelineid"]);
            PenWidth = Convert.ToInt32(reader["PenWidth"]);            
            SqlCommand sqlc = new SqlCommand();
            sqlc.Connection = conn;
            sqlc.CommandText = "select x,y from point where pointid in (select pointid from freeline_point where freelineid="+id+") order by pointid";
            read=sqlc.ExecuteReader();
           // PenColor = Color.Black;
            while(read.Read())
            {
                x = Convert.ToInt32(read["X"]);
                y = Convert.ToInt32(read["Y"]);
                p = new Point(x, y);
                FreeList.Add(p);
            }
            read.Close();            
        }

        public override void putData(SqlConnection conn,int drawID)
        {
            int id=0;
            try {
                SqlCommand sqlc = new SqlCommand();
                sqlc.Connection = conn;
                String clr = "#" + PenColor.Name;
                Console.WriteLine(ColorTranslator.ToHtml(PenColor).ToString());
                sqlc.CommandText = "insert into freeline (color,penwidth) values('" + ColorTranslator.ToHtml(PenColor).ToString() + "'," + PenWidth + ")"
                     + " SELECT SCOPE_IDENTITY() AS[SCOPE_IDENTITY]"; //scope_identity returns the next index generated 
                SqlDataReader reader = sqlc.ExecuteReader();
                if(reader.Read())
                id = Convert.ToInt32(reader["SCOPE_IDENTITY"]);
                reader.Close();

                sqlc.CommandText = "insert into draw_freeline (drawid,freelineid) values('" + drawID + "'," + id + ")";
                sqlc.ExecuteNonQuery();
                   
                if (id != 0)
                {

                    foreach (Point p in FreeList)
                    {
                        sqlc.CommandText = "insert into point (X,Y) values (" + p.X + "," + p.Y + ")"
                            + " SELECT SCOPE_IDENTITY() AS[SCOPE_IDENTITY]"; //scope_identity returns the next index generated 
                        reader = sqlc.ExecuteReader();
                        int x = 0 ;
                        if(reader.Read())
                        x = Convert.ToInt32(reader["SCOPE_IDENTITY"]);
                        reader.Close();
                        if(x!=0)
                        sqlc.CommandText = "insert into freeline_point (freelineid,pointid) values(" + id + "," + x + ")";
                        sqlc.ExecuteNonQuery();
                        
                    }

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error while adding the freeline" + ex.Message);
            }

        }

        public override void getData(SqlDataReader reader)
        {
            throw new NotImplementedException();
        }
    } // End FreeLine class

    /*
    text format for text file
    Text:(124,159),cjscjkkscjksc,Microsoft Sans Serif,15.75,FFFF0000)
Text:(94,61),cjskckkjscksjc,Papyrus,18,FFFF0000)
        */
    public class Text : Shape
	{
		/* Intended use for this shape type:
		 * This Text type is intended for entering one line of text.
		 * Text is entered at the point Pt1 (usually, where the mouse is clicked)
		 * When Enter is pressed, no more text is accepted.
		 * Backspace can be used to delete one character at a time before Enter is pressed.
		 */
		public string TextLine { get; set; }
		public Font TextFont { get; set; }
		//public Brush TextBrush { get; set; }
		public bool Open { get; set; }  // keyboard input accepted iff Open = true
        public Color Black { get; private set; }

        public Text()
			: base()
		{
			//TextBrush = new SolidBrush(Color.Black);
			TextLine = "";
			TextFont = CurrentFont;
			Open = true;
		}

		public override void Draw(Graphics g)
		{
			Brush brush = new SolidBrush(PenColor);
			g.DrawString(TextLine, TextFont, brush, Pt1);
		}


		public override string ToString()
		{
			string s = string.Format("Text: ({0},{1}), {2}, {3}, {4})",
				Pt1.X, Pt1.Y, TextLine, TextFont.FontFamily, PenColor);
			return s;
		}

		public override void writeBinary(BinaryWriter bw)
		{
            // YOUR CODE HERE
            bw.Write("Text");
            bw.Write(Pt1.X);
            bw.Write(Pt1.Y);
            bw.Write(TextLine);
            //  bw.Write((String)TextFont.FontFamily);
            var cvt = new FontConverter();
            bw.Write(TextFont.FontFamily.Name);
            bw.Write(TextFont.Size);
            bw.Write(PenColor.ToArgb());
            
            Console.WriteLine("Text data :" + ToString());
		}

		public override void readBinary(BinaryReader br)
		{
            // YOUR CODE HERE
            var cvt = new FontConverter();
            Pt1 =new Point(br.ReadInt32(), br.ReadInt32());//read the point 
            TextLine = br.ReadString();
            String font = br.ReadString();//read the font string
            float size = br.ReadSingle();//read float in binary
            TextFont = new Font(font, size);     
            PenColor = Color.FromArgb(br.ReadInt32()); //read the color
            Console.WriteLine("Text data :" + ToString());
        }
        /*
        text write format: 'Text:(25,171),cjdscjksckjcs,[FontFamily: Name=Microsoft Sans Serif],12,FF00FF00)'
            */
        public override void writeText(StreamWriter sw)
		{
            // YOUR CODE HERE
            string s = string.Format("Text:({0},{1}),{2},{3},{4},{5})",
                Pt1.X, Pt1.Y, TextLine, TextFont.FontFamily.Name, TextFont.Size,PenColor.A.ToString("X2") + PenColor.R.ToString("X2") + PenColor.G.ToString("X2") + PenColor.B.ToString("X2"));
            sw.WriteLine(s);
            Console.WriteLine("Writing the text data :" +s);
        }

		public override void readText(StreamReader sr)
		{
            // YOUR CODE HERE
            String line;
            int a, r, g, b;
            var cvt = new FontConverter();
            if ((line = sr.ReadLine()) != null)
            {
                Console.WriteLine(line);
                char[] delimiterChars = { ',', ')', '('};
                String[] attributes = line.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                attributes[0].Trim();
                Console.WriteLine(attributes[0]);
                attributes[1].Trim();
                Console.WriteLine(attributes[1]);//get rid of trailing spaces
                attributes[2].Trim();
                Console.WriteLine(attributes[2]);
                attributes[3].Trim();
                Console.WriteLine(attributes[3]);
                attributes[4].Trim();
                Console.WriteLine(attributes[4]);
                attributes[5].Trim();
                Console.WriteLine(attributes[5]);
                Pt1 = new Point(int.Parse(attributes[0]), int.Parse(attributes[1]));
                TextLine = attributes[2];
                TextFont = new Font(attributes[3], float.Parse(attributes[4])); //font size can be float 
                a = int.Parse(attributes[5].Substring(0, 2), System.Globalization.NumberStyles.HexNumber);//get the argb components
                r = int.Parse(attributes[5].Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                g = int.Parse(attributes[5].Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                b = int.Parse(attributes[5].Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                PenColor = Color.FromArgb(a, r, g, b);
            }
        }

        public override void getData(SqlDataReader reader)
        {
            try {
                PenWidth = Convert.ToInt32(reader["PenWidth"]);
                PenColor = System.Drawing.ColorTranslator.FromHtml((string)reader["Color"]);
                TextLine = (String)reader["TextData"];
                int X = Convert.ToInt32(reader["X"]);
                int Y = Convert.ToInt32(reader["Y"]);
                Pt1 = new Point(X, Y);
                TextFont = new Font((String)reader["FontFamily"], Convert.ToInt32(reader["FontSize"]));
         //       PenColor = Color.Black;
               
            }
            catch(Exception e1)
            {
                Console.WriteLine("Error while reading Text: "+e1.Message);
            }  
        }

        public override void putData(SqlConnection conn,int drawID)
        {
            int id;
            SqlCommand sqlc = new SqlCommand();
            String clr = "#" + PenColor.Name;
            sqlc.CommandText = "insert into Text (TextData,PenWidth,Color,FontFamily,FontSize,X,Y) values ('" + TextLine + "',"
                + PenWidth + ",'" + ColorTranslator.ToHtml(PenColor).ToString() + "','" + CurrentFont.FontFamily.Name + "'," 
                + CurrentFont.Size +","+Pt1.X+","+Pt1.Y+ ") "
                + " SELECT SCOPE_IDENTITY() AS[SCOPE_IDENTITY]"; //scope_identity returns the next index generated 
            Console.WriteLine("Text insert command "+sqlc.CommandText);
            sqlc.Connection = conn;
            try
            {
                SqlDataReader sqlread = sqlc.ExecuteReader();
                if (sqlread.Read())
                {
                    id = Convert.ToInt32(sqlread["SCOPE_IDENTITY"]);
                    Console.WriteLine(" Id of the next text inserted in the table");
                    sqlread.Close();//closing the reader as the read has been completed
                    sqlc.CommandText = "insert into draw_text (drawid,textid)values (" + drawID + "," + id + ")";
                    sqlc.ExecuteNonQuery();
                  
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("error while inserting the text data " + ex.Message);
            }
            //Console.WriteLine("returned id " + SCOPE_IDENTITY);


        }

        internal void readD(SqlDataReader reader, SqlConnection myConnection)
        {
            throw new NotImplementedException();
        }
    } // End Text class

}
