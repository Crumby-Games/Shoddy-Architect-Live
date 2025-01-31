using Godot;
using System.Collections.Generic;
using System.Linq;

/// Helper geometry-related static functions. Under a namespace to reduce global clutter.
namespace Geometry
{
    // Mainly consists of polygon generators and extension methods for arrays of points.
    public static class Polygon
    {
        // Arbitrary margin added to the end of slice polygons to ensure it cuts through the full shape
        const int SPLIT_LENGTH_MARGIN = 10;

        // Returns points for a rectangle polygon based on a width and height (the centre of the rectangle is Vector2.Zero)
        public static Vector2[] GeneratePointsForRectangle(Vector2 dimensions)
        {
            Vector2[] points = new Vector2[4];
            points[0] = dimensions;
            dimensions.X *= -1;
            points[1] = dimensions;
            dimensions.Y *= -1;
            points[2] = dimensions;
            dimensions.X *= -1;
            points[3] = dimensions;

            return points;
        }

        // Returns points for a circle polygon based on only a radius (the centre of the circle is Vector2.Zero)
        public static Vector2[] GeneratePointsForCircle(float radius)
        {
             // Determines number of edges logarithmically so that scales nicely with both small and large circles
            int numberOfEdges = (int)Mathf.Log(radius) * 8;

            // Calculates the angle difference between each edge of circle
            float internalAngle = 2 * Mathf.Pi / numberOfEdges;

            // Calculates points by rotating an angle continually and using trig to generate position of vertex
            float currentAngle = 0f;
            Vector2[] points = new Vector2[numberOfEdges];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)) * radius;
                currentAngle += internalAngle;
            }

            return points;
        }

        // Returns points for thin rectangular polygon based on only a direction (the start of the line is Vector2.Zero)
        public static Vector2[] GeneratePointsFromLine(Vector2 direction, float width, float length) 
        {
            Vector2 normal = direction.Rotated(Mathf.Pi);
            Vector2[] points = new Vector2[4];
            Vector2 end = direction * length;
            Vector2 widthOffset = width / 2 * normal;

            points[0] = widthOffset;
            points[1] = -widthOffset;
            points[2] = end - widthOffset;
            points[3] = end + widthOffset;

            return points;
        }

        // Returns the centroid of a polygon by considering cross product results for each point on an edge as weight for a point average
        public static Vector2 GetRelativeCenterOfMass(this Vector2[] polygon)
        {
            // Area is calculated at the same time as the vector sum (as opposed to using CalculateArea()) to save iterations
            float area = 0;

            // Sum of points weighted by cross result of edges
            Vector2 pointSum = Vector2.Zero; 

            // Iterate through each edge
            for (int i = 0; i < polygon.Length; i++)
            {
                (Vector2 v1, Vector2 v2) edge = (polygon[i], polygon[(i + 1) % polygon.Length]);

                float crossResult = edge.v1.Cross(edge.v2);
                area += crossResult;

                pointSum += (edge.v1 + edge.v2) * crossResult;
            }

            area = Mathf.Abs(area)*0.5f;

            // Avoid division by zero error
            if (area == 0) area = 1;

            // Divide by area to correct scale
            return pointSum / (6 * area);
        }

        // Divide one polygon into two, based on a direction of split and a point on the edge of the polygon
        public static List<Vector2[]> Split(this Vector2[] polygon, (Vector2 start, Vector2 direction) line, float sliceWidth)
        {
            // The theoretical maximum width/height the shape could be, even if there were points in exact opposite corners of the bounding box
            float furthestPossibleDistance = polygon.GetBoundingBox().Length();

            // Adjust start of line for margin
            line.start += -line.direction * SPLIT_LENGTH_MARGIN;
            
            // Generate thin polygon from line that is used to clip the starting polygon. Add some margin in length
            Vector2[] splitterPolygon = GeneratePointsFromLine(line.direction, sliceWidth, furthestPossibleDistance + SPLIT_LENGTH_MARGIN); 

            // Offset line to match the starting location of the slice
            splitterPolygon.MovePoints(line.start);

            // Clip and return polygons. Convert from built-in Godot collection to List from system.linq for consistency with other functions
            return Geometry2D.ClipPolygons(polygon, splitterPolygon).ToList();;
        }


        // Calculates area as half the sum of cross product of points of edges
        public static float CalculateArea(this Vector2[] polygon)
        {
            float result = 0f;

            for (int i = 0; i < polygon.Length; i++)
            {
                (Vector2 v1, Vector2 v2) edge = (polygon[i], polygon[(i + 1) % polygon.Length]);
                result += edge.v1.Cross(edge.v2);
            }

            return Mathf.Abs(result) * 0.5f;
        }

        // Offsets all points in polygon by reference
        public static void MovePoints(this Vector2[] points, Vector2 offset)
        {
            for (int i = 0; i < points.Length; i++) points[i] += offset;
        }

        // Returns the rectangle in which all points can be found (on global axes)
        public static Vector2 GetBoundingBox(this Vector2[] polygon)
        {
            return new Vector2(getRightBound(polygon) - getLeftBound(polygon), getBottomBound(polygon) - getTopBound(polygon));
        }

        static float getLeftBound(this Vector2[] polygon)
        {
            float? min = null;
            foreach (Vector2 point in polygon) if (min == null || point.X < min) min = point.X;
            return (float)min;
        }

        static float getRightBound(this Vector2[] polygon)
        {
            float? max = null;
            foreach (Vector2 point in polygon) if (max == null || point.X > max) max = point.X;
            return (float)max;
        }

        static float getTopBound(this Vector2[] polygon)
        {
            float? min = null;
            foreach (Vector2 point in polygon) if (min == null || point.Y < min) min = point.Y;
            return (float)min;
        }

        static float getBottomBound(this Vector2[] polygon)
        {
            float? max = null;
            foreach (Vector2 point in polygon) if (max == null || point.Y > max) max = point.Y;
            return (float)max;
        }
    }
}