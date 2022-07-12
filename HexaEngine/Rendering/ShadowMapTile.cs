namespace HexaEngine.Rendering
{
    using HexaEngine.Cameras;
    using HexaEngine.Lights;
    using System;
    using System.Numerics;

    public class ShadowMapTile
    {
        private static Vector3 camPosition;

        public static void TileLights(Light[] light, Camera camera)
        {
            camPosition = camera.Transform.Position;
            Array.Sort(light, Comparison);
        }

        /// <summary>
        /// Represents the method that compares two lights of the same type.
        /// </summary>
        /// <param name="a">The first light to compare.</param>
        /// <param name="b">The second light to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of x and y, as shown in the following table.<br/>
        /// Value – Meaning<br/>
        /// Less than 0 –x is less than y.<br/>
        /// 0 –x equals y.<br/>
        /// Greater than 0 –x is greater than y.<br/>
        /// </returns>
        private static int Comparison(Light a, Light b)
        {
            float distA = Vector3.Distance(a.Transform.Position, camPosition);
            float distB = Vector3.Distance(b.Transform.Position, camPosition);

            if (distA > distB) return 1;
            if (distA < distB) return -1;
            return 0;
        }
    }
}