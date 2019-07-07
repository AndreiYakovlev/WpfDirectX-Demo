using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfDirectX.Primitives
{
    public class Transform : DependencyObject
    {
        public static readonly DependencyProperty PositionProperty;
        public static readonly DependencyProperty RotationProperty;
        public static readonly DependencyProperty ScaleProperty;

        static Transform()
        {
            PositionProperty = DependencyProperty.Register("Position", typeof(Vector3), typeof(Transform));
            RotationProperty = DependencyProperty.Register("Rotation", typeof(Quaternion), typeof(Transform));
            ScaleProperty = DependencyProperty.Register("Scale", typeof(Vector3), typeof(Transform));
        }

        public Transform()
        {
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
        }

        public Vector3 Position
        {
            get { return (Vector3)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public Quaternion Rotation
        {
            get { return (Quaternion)GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }

        public Vector3 Scale
        {
            get { return (Vector3)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
    }

    public abstract class Primitive
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexPositionColor
        {
            public Vector3 Position;
            public uint Color;

            public static readonly int SizeInBytes = Vector3.SizeInBytes + sizeof(uint);

            public VertexPositionColor(Vector3 position, uint color)
            {
                Position = position;
                Color = color;
            }
        }

        public VertexBuffer VertexBuffer { get; protected set; }
        public static readonly VertexElement[] VertexElements;
        public static readonly int Stride;

        public Transform Transform { get; set; }

        public static implicit operator VertexBuffer(Primitive primitive) => primitive.VertexBuffer;

        static Primitive()
        {
            List<VertexElement> vertexElements = new List<VertexElement>();
            vertexElements.Add(new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0));
            vertexElements.Add(new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0));
            vertexElements.Add(VertexElement.VertexDeclarationEnd);
            VertexElements = vertexElements.ToArray();
            Stride = VertexPositionColor.SizeInBytes;
        }

        public Primitive()
        {
            Transform = new Transform();
        }

        public virtual void DrawPrimitive(Device device)
        {
            Matrix translate = Matrix.Translation(Transform.Position);
            Matrix scaling = Matrix.Scaling(Transform.Scale);
            Matrix rotating = Matrix.RotationQuaternion(Transform.Rotation);
            Matrix world = scaling * rotating * translate;
            device.SetTransform(TransformState.World, world);
        }
    }

    public class Triangle : Primitive
    {
        public Triangle(Device device)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            vertices.Add(new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0), 0xFFFF0000));
            vertices.Add(new VertexPositionColor(new Vector3(0.5f, -0.5f, 0), 0xFF00FF00));
            vertices.Add(new VertexPositionColor(new Vector3(0f, 0.5f, 0), 0xFF0000FF));

            VertexBuffer = new VertexBuffer(device, Stride * vertices.Count, Usage.WriteOnly, VertexFormat.None, Pool.Default);
            VertexBuffer.Lock(0, 0, LockFlags.None).WriteRange(vertices.ToArray(), 0, vertices.Count);
            VertexBuffer.Unlock();
        }

        public override void DrawPrimitive(Device device)
        {
            base.DrawPrimitive(device);
            device.SetStreamSource(0, this, 0, Stride);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        }
    }

    public abstract class IndexedPrimitive : Primitive
    {
        public IndexBuffer IndexBuffer { get; protected set; }

        public static implicit operator IndexBuffer(IndexedPrimitive primitive) => primitive.IndexBuffer;
    }

    public class Quad : IndexedPrimitive
    {
        public Quad(Device device)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            vertices.Add(new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0), 0xFFFF0000));
            vertices.Add(new VertexPositionColor(new Vector3(0.5f, 0.5f, 0), 0xFF00FF00));
            vertices.Add(new VertexPositionColor(new Vector3(0.5f, -0.5f, 0), 0xFF0000FF));
            vertices.Add(new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0), 0xFFFFFFFF));

            VertexBuffer = new VertexBuffer(device, Stride * vertices.Count, Usage.WriteOnly, VertexFormat.None, Pool.Default);
            VertexBuffer.Lock(0, 0, LockFlags.None).WriteRange(vertices.ToArray());
            VertexBuffer.Unlock();

            short[] indices = new short[]
            {
                0, 1, 2,
                2, 3, 0,
            };

            IndexBuffer = new IndexBuffer(device, sizeof(short) * indices.Length, Usage.WriteOnly, Pool.Default, true);
            IndexBuffer.Lock(0, 0, LockFlags.None).WriteRange(indices);
            IndexBuffer.Unlock();
        }

        public override void DrawPrimitive(Device device)
        {
            base.DrawPrimitive(device);

            device.SetStreamSource(0, this, 0, Stride);
            device.Indices = this;
            device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }
    }

    public class Box : IndexedPrimitive
    {
        public Box(Device device)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            vertices.Add(new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), 0xFFFF0000));
            vertices.Add(new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), 0xFF00FF00));
            vertices.Add(new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), 0xFF0000FF));
            vertices.Add(new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), 0xFFFFFFFF));
            vertices.Add(new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), 0xFFFF0000));
            vertices.Add(new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), 0xFF00FF00));
            vertices.Add(new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), 0xFF0000FF));
            vertices.Add(new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), 0xFFFFFFFF));

            VertexBuffer = new VertexBuffer(device, Stride * vertices.Count, Usage.WriteOnly, VertexFormat.None, Pool.Default);
            VertexBuffer.Lock(0, 0, LockFlags.None).WriteRange(vertices.ToArray());
            VertexBuffer.Unlock();

            short[] indices = new short[]
            {
                0, 1, 2,
                2, 3, 0,

                4, 7, 6,
                6, 5, 4,

                0, 4, 5,
                5, 1, 0,

                1, 5, 6,
                6, 2, 1,

                2, 6, 7,
                7, 3, 2,

                3, 7, 4,
                4, 0, 3,
            };

            IndexBuffer = new IndexBuffer(device, sizeof(short) * indices.Length, Usage.WriteOnly, Pool.Default, true);
            IndexBuffer.Lock(0, 0, LockFlags.None).WriteRange(indices);
            IndexBuffer.Unlock();
        }

        public override void DrawPrimitive(Device device)
        {
            base.DrawPrimitive(device);

            device.SetStreamSource(0, this, 0, Stride);
            device.Indices = this;
            device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, 8, 0, 6 * 2);
        }
    }

    public class Sphere : Primitive
    {
        public override void DrawPrimitive(Device device)
        {
            base.DrawPrimitive(device);
        }
    }
}