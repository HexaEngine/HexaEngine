namespace HexaEngine.Mathematics.HosekWilkie
{
    using System;
    using System.Numerics;

    public struct SkyParameters
    {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 D;
        public Vector3 E;
        public Vector3 F;
        public Vector3 G;
        public Vector3 I;
        public Vector3 H;
        public Vector3 Z;
        public const int Count = 10;

        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return A;

                    case 1:
                        return B;

                    case 2:
                        return C;

                    case 3:
                        return D;

                    case 4:
                        return E;

                    case 5:
                        return F;

                    case 6:
                        return G;

                    case 7:
                        return I;

                    case 8:
                        return H;

                    case 9:
                        return Z;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            set
            {
                switch (index)
                {
                    case 0:
                        A = value; break;

                    case 1:
                        B = value; break;

                    case 2:
                        C = value; break;

                    case 3:
                        D = value; break;

                    case 4:
                        E = value; break;

                    case 5:
                        F = value; break;

                    case 6:
                        G = value; break;

                    case 7:
                        I = value; break;

                    case 8:
                        H = value; break;

                    case 9:
                        Z = value; break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}