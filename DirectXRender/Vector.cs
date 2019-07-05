using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4
    {
        private float X;
        private float Y;
        private float Z;
        private float W;

        public Vector4(float x, float y, float z = 0, float w = 0)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}