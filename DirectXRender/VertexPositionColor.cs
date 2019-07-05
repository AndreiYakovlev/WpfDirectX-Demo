using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor
    {
        public Vector4 Position;
        public uint Color;

        public VertexPositionColor(Vector4 position, uint color)
        {
            Position = position;
            Color = color;
        }
    }
}