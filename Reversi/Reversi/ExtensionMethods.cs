using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Reversi
{
    public static class ExtensionMethods
    {
        public static Point Add(this Point p1, Point p2) 
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static Point Subtract(this Point p1, Point p2) 
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
    }
}
