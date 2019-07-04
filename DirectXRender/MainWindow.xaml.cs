using SharpDX.Direct3D;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //PInvoke declarations
        [DllImport("DirectXNative.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateHwnd();

        private DeviceEx device;
        private Surface renderTarget;

        public MainWindow()
        {
            d3dimg = new D3DImage();
            d3dimg.IsFrontBufferAvailableChanged
                += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);

            // parse the XAML
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);

            InitializeScene();
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private PresentParameters CreatePresentParameters(IntPtr hWnd, Format surfaceFormat)
        {
            return new PresentParameters()
            {
                Windowed = true,
                BackBufferFormat = surfaceFormat,
                BackBufferWidth = 1,
                BackBufferHeight = 1,
                BackBufferCount = 1,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = hWnd,
            };
        }

        private void InitializeScene()
        {
            try
            {
                IntPtr hWnd = CreateHwnd();

                InitializeDevice(hWnd, DeviceType.Hardware, Format.A8R8G8B8, MultisampleType.None);
                InitializeRenderTarget(device, 300, 300, Format.A8R8G8B8);
                InitializeVertexDeclaration(device);
                InitializeTriangle(device);
                BeginRenderingScene();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Не удалось создать устройство вывода");
            }
        }

        private void InitializeRenderTarget(Device device, int width, int height, Format surfaceFormat)
        {
            var texture = new Texture(device, width, height, 0, Usage.RenderTarget, surfaceFormat, Pool.Default);
            renderTarget = texture.GetSurfaceLevel(0);
        }

        private void InitializeVertexDeclaration(Device device)
        {
            List<VertexElement> vertexElements = new List<VertexElement>();
            vertexElements.Add(new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.Position, 0));
            vertexElements.Add(new VertexElement(0, 16, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0));
            vertexElements.Add(VertexElement.VertexDeclarationEnd);

            device.VertexDeclaration = new VertexDeclaration(device, vertexElements.ToArray());
        }

        private void InitializeTriangle(Device device)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();
            vertices.Add(new VertexPositionColor(new Vector4(-1f, 0f, 0), 0xFFFF0000));
            vertices.Add(new VertexPositionColor(new Vector4(1f, 0f, 0), 0xFF00FF00));
            vertices.Add(new VertexPositionColor(new Vector4(0f, 1f, 0), 0xFF0000FF));

            int stride = 0;
            unsafe
            {
                stride = sizeof(VertexPositionColor);
            }

            VertexBuffer vertexBuffer = new VertexBuffer(device, stride * vertices.Count, Usage.Dynamic, VertexFormat.None, Pool.Default);
            vertexBuffer.Lock(0, 0, LockFlags.None).WriteRange(vertices.ToArray(), 0, vertices.Count);
            vertexBuffer.Unlock();
            device.SetStreamSource(0, vertexBuffer, 0, stride);
        }

        private void InitializeDevice(IntPtr hWnd, DeviceType deviceType, Format surfaceFormat, MultisampleType multisampleType = MultisampleType.None)
        {
            var direct = new Direct3DEx();

            PresentParameters presentParameters = CreatePresentParameters(hWnd, surfaceFormat);

            try
            {
                foreach (var adapter in direct.Adapters)
                {
                    string adapterDeviceName = adapter.Details.DeviceName;
                    if (!direct.CheckDeviceType(adapter.Adapter, deviceType, Format.X8R8G8B8, surfaceFormat, true))
                    {
                        throw new Exception($"Устройство {adapterDeviceName} не поддерживается");
                    }
                    if (!direct.CheckDeviceFormat(adapter.Adapter, deviceType, Format.X8R8G8B8, Usage.RenderTarget | Usage.Dynamic, ResourceType.Surface, surfaceFormat))
                    {
                        throw new Exception($"Формат {surfaceFormat} не поддерживается для этого адаптера {adapterDeviceName}");
                    }

                    if (!direct.CheckDeviceMultisampleType(adapter.Adapter, deviceType, surfaceFormat, true, multisampleType))
                    {
                        System.Diagnostics.Debug.WriteLine($"Сглаживание типа {multisampleType} не поддреживается устройством {adapterDeviceName}");
                        multisampleType = MultisampleType.None;
                    }
                }
            }
            catch (Exception ex)
            {
                direct.Dispose();
                throw ex;
            }

            device = new DeviceEx(direct, 0, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                presentParameters);

            device.SetRenderState(RenderState.CullMode, Cull.None);
            device.SetRenderState(RenderState.Lighting, false);
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // if the front buffer is available, then WPF has just created a new
            // D3D device, so we need to start rendering our custom scene
            if (d3dimg.IsFrontBufferAvailable && device != null)
            {
                BeginRenderingScene();
            }
            else
            {
                // If the front buffer is no longer available, then WPF has lost its
                // D3D device so there is no reason to waste cycles rendering our
                // custom scene until a new device is created.
                StopRenderingScene();
            }
        }

        private void BeginRenderingScene()
        {
            if (d3dimg.IsFrontBufferAvailable)
            {
                // set the back buffer using the new scene pointer
                d3dimg.Lock();
                device.SetRenderTarget(0, renderTarget);
                d3dimg.SetBackBuffer(D3DResourceType.IDirect3DSurface9, renderTarget.NativePointer, true);
                d3dimg.Unlock();

                // leverage the Rendering event of WPF's composition target to
                // update the custom D3D scene
                CompositionTarget.Rendering += OnRendering;
            }
        }

        private void StopRenderingScene()
        {
            // This method is called when WPF loses its D3D device.
            // In such a circumstance, it is very likely that we have lost
            // our custom D3D device also, so we should just release the scene.
            // We will create a new scene when a D3D device becomes
            // available again.
            CompositionTarget.Rendering -= OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            // when WPF's composition target is about to render, we update our
            // custom render target so that it can be blended with the WPF target
            if (d3dimg.IsFrontBufferAvailable)
            {
                // lock the D3DImage
                d3dimg.Lock();

                device.Clear(ClearFlags.Target, new SharpDX.Mathematics.Interop.RawColorBGRA(51, 51, 51, 255), 1.0f, 0);
                device.BeginScene();
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                device.EndScene();

                // invalidate the updated region of the D3DImage (in this case, the whole image)
                d3dimg.AddDirtyRect(new Int32Rect(0, 0, 300, 300));

                // unlock the D3DImage
                d3dimg.Unlock();
            }
        }
    }
}