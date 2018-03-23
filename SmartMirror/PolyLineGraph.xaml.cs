using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace SmartMirror
{
	public sealed partial class PolyLineGraph : UserControl
	{
		// Graph bounds
		Point start = new Point(0.0, 0.0);
		Point end = new Point(1.0, 1.0);

		// Polyline
		Polyline polyLine = new Polyline();
		Color lineColor = Color.FromArgb(255, 255, 255, 255);
		double lineThickness = 8.0;

		// Points
		List<Point> points = new List<Point>();
		Color pointColor = Color.FromArgb(255, 255, 255, 255);
		bool showPoints = false;
		int subdivision = 0;

		// Labels
		List<string> labels = new List<string>();
		Color labelColor = Color.FromArgb(255, 255, 255, 255);
		string labelSuffix = "";
		double fontSize = 20.0;

		// Dividers
		Color dividerColor = Color.FromArgb(255, 255, 255, 255);
		double dividerThickness = 1.0;
		int numDividersX = 0;
		int numDividersY = 0;

		// Divider labels
		//Color labelDividerColor = Color.FromArgb(255, 255, 255, 255);
		//List<string> horzLabels = new List<string>();
		//List<string> vertLabels = new List<string>();

		public PolyLineGraph() { InitializeComponent(); }

		// Bounding box region of the points to view
		public void SetBounds(Point start, Point end)
		{
			this.start = new Point(start.X, start.Y);
			this.end = new Point(end.X, end.Y);
		}
		public void SetBounds(double startX, double startY, double endX, double endY) { SetBounds(new Point(startX, startY), new Point(endX, endY)); }

		public void SetLineThickness(double thickness) { lineThickness = thickness; }
		public void SetLineColor(Color color) { lineColor = color; }
		public void SetPointColor(Color color) { pointColor = color; }
		public void SetLabelColor(Color color) { labelColor = color; }
		public void SetNumberOfSubdivisions(int subdiv) { subdivision = subdiv; }
		public void SetShowPoints(bool val) { showPoints = val; }
		public void SetLabelSuffix(string str) { labelSuffix = str; }
		public void SetFontSize(double fontSize) { this.fontSize = fontSize; }
		public void SetNumberOfDividers(int x, int y)
		{
			numDividersX = x;
			numDividersY = y;
		}
		public void SetDividerColor(Color color) { dividerColor = color; }
		public void SetDividerThickness(double thickness) { dividerThickness = thickness; }
		//public void AddHorzLabel(string str) { horzLabels.Add(str); }
		//public void AddVertLabel(string str) { vertLabels.Add(str); }
		//public void SetLabelDivderColor(Color color) { labelDividerColor = color; }

		public void SetPoints(List<Point> points) { this.points = new List<Point>(points); }
		public void AddPoint(Point pt) { points.Add(new Point(pt.X, pt.Y)); }
		public void SetLabels(List<string> labels) { this.labels = new List<string>(labels); }
		public void AddLabel(string label) { labels.Add(label); }
		public void Clear()
		{
			points.Clear();
			labels.Clear();
			//horzLabels.Clear();
			//vertLabels.Clear();
		}

		// Updates the polyline representation to the specified points
		public void Update()
		{
			double width = end.X - start.X;
			double height = end.Y - start.Y;

			// Calculate the proper coordinates
			for (int i = 0; i < points.Count; i++)
			{
				points[i] = new Point((points[i].X - start.X) * Width / width,
					Height - (points[i].Y - start.Y) * Height / height);
			}

			// Remove everything from the canvas
			MainCanvas.Children.Clear();

			UpdateDividers();
			UpdatePolyLine();
			UpdatePoints();
			UpdateLabels();
		}

		private void UpdateDividers()
		{
			// Add the background dividers
			double divXSize = Width / (numDividersX + 1);
			for (int i = 1; i < numDividersX + 1; i++)
			{
				Rectangle rect = new Rectangle();
				rect.Width = dividerThickness;
				rect.Height = Height;
				rect.Fill = new SolidColorBrush(dividerColor);
				Canvas.SetLeft(rect, i * divXSize);
				Canvas.SetTop(rect, 0);
				MainCanvas.Children.Add(rect);
			}
			double divYSize = Height / (numDividersY + 1);
			for (int i = 1; i < numDividersY + 1; i++)
			{
				Rectangle rect = new Rectangle();
				rect.Width = Width;
				rect.Height = dividerThickness;
				rect.Fill = new SolidColorBrush(dividerColor);
				Canvas.SetLeft(rect, 0);
				Canvas.SetTop(rect, Height - i * divYSize);
				MainCanvas.Children.Add(rect);
			}
		}

		private void UpdatePolyLine()
		{
			// Create a new polyline
			polyLine = new Polyline();
			polyLine.Stroke = new SolidColorBrush(lineColor);
			polyLine.StrokeThickness = lineThickness;
			MainCanvas.Children.Add(polyLine);
			if (subdivision == 0)
				AddRange(polyLine.Points, points);
			else
			{
				AddRange(polyLine.Points, CatmullRomSubdiv(points[0], points[0], points[1], points[2], subdivision));
				for (int i = 0; i < points.Count - 4; i++)
				{
					AddRange(polyLine.Points, CatmullRomSubdiv(points[i], points[i + 1], points[i + 2], points[i + 3], subdivision));
				}
				AddRange(polyLine.Points, CatmullRomSubdiv(points[points.Count - 3], points[points.Count - 2], points[points.Count - 1], points[points.Count - 1], subdivision));
			}
		}

		private void UpdatePoints()
		{
			// Add the points
			if (showPoints)
			{
				const double r = 8;
				const double d = r * 2;
				for (int i = 0; i < points.Count; i++)
				{
					Ellipse circle = new Ellipse();
					circle.Fill = new SolidColorBrush(pointColor);
					circle.Width = d;
					circle.Height = d;
					Canvas.SetLeft(circle, points[i].X - r);
					Canvas.SetTop(circle, points[i].Y - r);
					MainCanvas.Children.Add(circle);
				}
			}
		}

		private void UpdateLabels()
		{
			// Add the labels
			if (labels.Count == points.Count)
			{
				for (int i = 0; i < points.Count; i++)
				{
					TextBlock label = new TextBlock();
					label.Foreground = new SolidColorBrush(labelColor);
					label.FontSize = fontSize;
					label.Text = labels[i] + labelSuffix;
					label.FontFamily = new FontFamily("Assets/AGENCYB.TTF#Agency FB");
					Canvas.SetLeft(label, points[i].X);
					Canvas.SetTop(label, points[i].Y);
					MainCanvas.Children.Add(label);
				}
			}
		}


		private static void AddRange<T>(ICollection<T> collection, IEnumerable<T> enumerable)
		{
			foreach (var cur in enumerable)
			{
				collection.Add(cur);
			}
		}
		// Creates a set of n (divisions) between p1 and p2. Using p0 and p3 as well for the curve.
		public static Point[] CatmullRomSubdiv(Point p0, Point p1, Point p2, Point p3, int divisions)
		{
			Point[] pts = new Point[divisions];
			for (int i = 0; i < divisions; i++)
			{
				pts[i] = CatmullRom(p0, p1, p2, p3, i / (float)divisions);
			}
			return pts;
		}
		// Catmull rom for weather curves (could possibly go in a separate "mathhelper" 
		// class but why make a mathehelper class with one function in it)
		public static Point CatmullRom(Point p0, Point p1, Point p2, Point p3, float t)
		{
			float t2 = t * t;
			float t3 = t2 * t;
			return new Point(0.5f * ((2.0f * p1.X) + (-p0.X + p2.X) * t + (2.0f * p0.X - 5.0f * p1.X + 4.0f * p2.X - p3.X) * t2 + (-p0.X + 3.0f * p1.X - 3.0f * p2.X + p3.X) * t3),
				0.5f * ((2.0f * p1.Y) + (-p0.Y + p2.Y) * t + (2.0f * p0.Y - 5.0f * p1.Y + 4.0f * p2.Y - p3.Y) * t2 + (-p0.Y + 3.0f * p1.Y - 3.0f * p2.Y + p3.Y) * t3));
		}
	}
}