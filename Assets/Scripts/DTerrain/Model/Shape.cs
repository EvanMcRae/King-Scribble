using System.Collections.Generic;
using UnityEngine;

namespace DTerrain
{
    /// <summary>
    /// Shape is a simple class that holds a list of Ranges (not ranges) and then is used to destroy terrain with.
    /// To make complicated shape destructions (not squares, circles etc.) don't use it as it supports only list of ranges.
    /// </summary>
    public class Shape
    {

        public List<Range> Ranges;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Shape(int w, int h)
        {
            Width = w;
            Height = h;
            Ranges = new List<Range>();
        }

        public static Shape GenerateShapeRange(int length)
        {
            Shape s = new Shape(1, length);
            s.Ranges.Add(new Range(0, length));
            return s;
        }

        /// <summary>
        /// Generates a Shape: oval.
        /// </summary>
        /// <param name="rx">X Radius</param>
        /// <param name="ry">Y Radius</param>
        /// <returns>Shape: oval.</returns>
        public static Shape GenerateShapeOval(int rx, int ry)
        {
            int centerX = rx;
            int centerY = ry;
            Shape s = new(2 * rx, 2 * ry);
            for (int i = 0; i <= 2 * rx; i++)
            {
                bool down = false;
                int min = 0;
                int max = 0;
                for (int j = 0; j <= 2 * ry; j++)
                {
                    float dx = (i - centerX) / (float)rx;
                    float dy = (j - centerY) / (float)ry;
                    if (dx * dx + dy * dy < 1f)
                    {
                        if (down == false)
                        {
                            down = true;
                            min = j;
                        }

                    }
                    else
                    {
                        if (down)
                        {
                            max = j;
                            break;

                        }

                    }

                }
                if (down)
                {
                    Range range = new Range(min, max);
                    s.Ranges.Add(range);
                }

            }

            return s;
        }

        /// <summary>
        /// Generates a Shape: circle.
        /// </summary>
        /// <param name="r">Radius</param>
        /// <returns>Shape: circle.</returns>
        public static Shape GenerateShapeCircle(int r)
        {
            return GenerateShapeOval(r, r);
        }

        public static Shape GenerateShapeRect(int w, int h)
        {
            Shape s = new Shape(w, h);

            for(int i = 0; i<w;i++)
            {
                s.Ranges.Add(new Range(0, h-1)); //0,1,2...h-1
            }

            return s;
        }
    }
}
