using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfDirectX.Models
{
    internal class DirectXComponent : DependencyObject
    {
        public static readonly DependencyProperty BackBufferWidthProperty;
        public static readonly DependencyProperty BackBufferHeightProperty;
        public static readonly DependencyProperty BackBufferFormatProperty;
        public static readonly DependencyProperty MultiSampleProperty;

        static DirectXComponent()
        {
            BackBufferWidthProperty = DependencyProperty.Register("BackBufferWidth", typeof(int), typeof(DirectXComponent),
                new FrameworkPropertyMetadata(1, OnBackBufferWidthChanged, CoerceBackBufferSize));
            BackBufferHeightProperty = DependencyProperty.Register("BackBufferHeight", typeof(int), typeof(DirectXComponent),
              new FrameworkPropertyMetadata(1, OnBackBufferHeightChanged, CoerceBackBufferSize));
            BackBufferFormatProperty = DependencyProperty.Register("BackBufferFormat", typeof(SharpDX.Direct3D9.Format), typeof(DirectXComponent),
                new FrameworkPropertyMetadata(SharpDX.Direct3D9.Format.A8R8G8B8));
            MultiSampleProperty = DependencyProperty.Register("MultisampleType", typeof(SharpDX.Direct3D9.MultisampleType), typeof(DirectXComponent),
              new FrameworkPropertyMetadata(SharpDX.Direct3D9.MultisampleType.None));
        }

        public int BackBufferWidth
        {
            get { return (int)GetValue(BackBufferWidthProperty); }
            set { SetValue(BackBufferWidthProperty, value); }
        }

        public int BackBufferHeight
        {
            get { return (int)GetValue(BackBufferHeightProperty); }
            set { SetValue(BackBufferHeightProperty, value); }
        }

        private static object CoerceBackBufferSize(DependencyObject d, object value)
        {
            int size = (int)value;
            return Math.Max(size, 1);
        }

        private static void OnBackBufferWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnBackBufferHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public SharpDX.Direct3D9.Format BackBufferFormat
        {
            get { return (SharpDX.Direct3D9.Format)GetValue(BackBufferFormatProperty); }
            set { SetValue(BackBufferFormatProperty, value); }
        }

        public SharpDX.Direct3D9.MultisampleType MultiSampleType
        {
            get { return (SharpDX.Direct3D9.MultisampleType)GetValue(MultiSampleProperty); }
            set { SetValue(MultiSampleProperty, value); }
        }

        public DirectXComponent()
        {
        }
    }
}