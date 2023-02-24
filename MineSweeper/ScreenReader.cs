using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace HumanBenchmark
{
    internal class ScreenReader
    {
        // VARIABLES

        public Bitmap screenshot;

        public int x;
        public int y;
        public int width;
        public int height;



        // CONSTRUCOTRS

        public ScreenReader() { }

        public ScreenReader(Point point1, Point point2)
        {
            this.x = Math.Min(point1.X, point2.X);
            this.y = Math.Min(point1.Y, point2.Y);
            this.width = Math.Abs(point1.X - point2.X);
            this.height = Math.Abs(point1.Y - point2.Y);
        }

        public ScreenReader(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }



        // PUBLIC METHODS

        public void getRectangleViaCursor()
        {
            Point firstCorner = new Point();
            Point secondCorner = new Point();

            Console.Write("Move cursor to one corner of the rectangle and press enter.");
            Console.ReadLine();
            CursorManager.getPosition(ref firstCorner);

            Console.Write("Move cursor to the opposite corner of the rectangle and press enter.");
            Console.ReadLine();
            CursorManager.getPosition(ref secondCorner);

            x = Math.Min(firstCorner.X, secondCorner.X);
            y = Math.Min(firstCorner.Y, secondCorner.Y);

            width = Math.Abs(firstCorner.X - secondCorner.X);
            height = Math.Abs(firstCorner.Y - secondCorner.Y);

            Console.WriteLine("Rectangle coordinates saved");
        }



        public void captureScreenshot()
        {
            screenshot = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(screenshot))
                g.CopyFromScreen(x, y, 0, 0, screenshot.Size);
        }



        public void saveScreenShot(String filename)
        {
            screenshot.Save(filename);
        }



        public Point searchScreenShot(Bitmap picture)
        {
            for (int x = 0; x < screenshot.Width - picture.Width; x++)
            {
                for (int y = 0; y < screenshot.Height - picture.Height; y++)
                {
                    bool found = true;
                    for (int i = 0; i < picture.Width; i++)
                    {
                        for (int j = 0; j < picture.Height; j++)
                        {

                            if (screenshot.GetPixel(x + i, y + j) != picture.GetPixel(i, j))
                            {
                                found = false;
                                break;
                            }
                        }
                    }

                    if (found) return new Point(x, y);
                }
            }

            return Point.Empty;
        }

        public bool searchScreenShot(Bitmap picture, int x, int y)
        {
            bool found = true;
            for (int i = 0; i < picture.Width; i++)
            {
                for (int j = 0; j < picture.Height; j++)
                {

                    if (screenshot.GetPixel(x + i, y + j) != picture.GetPixel(i, j))
                    {
                        found = false;
                        break;
                    }
                }
            }

            if (found) return true;

            return false;
        }



        public Point findColor(Color color)
        {
            for (int x = 0; x < screenshot.Width; x++)
                for (int y = 0; y < screenshot.Height; y++)
                    if (screenshot.GetPixel(x, y).Equals(color))
                        return new Point(x, y);

            Console.WriteLine("No color found");
            return Point.Empty;
        }

    }
}
