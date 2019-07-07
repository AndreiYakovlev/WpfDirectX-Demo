using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WpfDirectX.Models;
using WpfDirectX.Primitives;
using Box = WpfDirectX.Primitives.Box;
using Matrix = SharpDX.Matrix;

namespace WpfDirectX.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        [DllImport("DirectXNative.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateHwnd();

        private MainWindow _window;
        private Device device;
        private Surface renderTarget;

        private D3DImage _di;
        private ImageSource imageSource;

        private Primitive activePrimitive;

        public Primitive ActivePrimitive
        {
            get => activePrimitive;
            set
            {
                activePrimitive = value;
                OnPropertyChanged();
            }
        }

        public SharpDX.Color BackgroundColor { get; set; }

        public ImageSource DirectSurface
        {
            get => imageSource;
            private set
            {
                imageSource = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(MainWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            _di = new D3DImage();
            _di.IsFrontBufferAvailableChanged
                += new DependencyPropertyChangedEventHandler(OnIsFrontBufferAvailableChanged);
        }

        private void BeginRenderingScene()
        {
            if (_di.IsFrontBufferAvailable)
            {
                // set the back buffer using the new scene pointer
                try
                {
                    _di.Lock();
                    _di.SetBackBuffer(D3DResourceType.IDirect3DSurface9, renderTarget.NativePointer, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                    return;
                }
                finally
                {
                    _di.Unlock();
                }

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

        [DllImport("Winmm.dll")]
        private static extern int timeGetTime();

        private void OnRendering(object sender, EventArgs e)
        {
            // when WPF's composition target is about to render, we update our
            // custom render target so that it can be blended with the WPF target
            if (_di.IsFrontBufferAvailable && device != null)
            {
                // lock the D3DImage
                _di.Lock();

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, BackgroundColor, 1.0f, 0);
                device.BeginScene();

                Matrix View = Matrix.LookAtLH(new Vector3(2, 2, -2), Vector3.Zero, Vector3.Up);
                Matrix Proj = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(60), 1, 0.1f, 100);

                device.SetTransform(TransformState.View, View);
                device.SetTransform(TransformState.Projection, Proj);

                ActivePrimitive.Transform.Rotation = Quaternion.RotationAxis(Vector3.Up, MathUtil.DegreesToRadians(timeGetTime() / 10));
                ActivePrimitive.DrawPrimitive(device);

                device.EndScene();

                // invalidate the updated region of the D3DImage (in this case, the whole image)
                _di.AddDirtyRect(new Int32Rect(0, 0, renderTarget.Description.Width, renderTarget.Description.Height));

                // unlock the D3DImage
                _di.Unlock();
            }
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // if the front buffer is available, then WPF has just created a new
            // D3D device, so we need to start rendering our custom scene
            if (_di.IsFrontBufferAvailable && device != null)
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

        private PresentParameters CreatePresentParameters(IntPtr hWnd)
        {
            DirectXComponent component = _window.TryFindResource("presentParameters") as DirectXComponent;

            return new PresentParameters()
            {
                BackBufferFormat = component?.BackBufferFormat ?? Format.A8R8G8B8,
                BackBufferWidth = component?.BackBufferWidth ?? 1,
                BackBufferHeight = component?.BackBufferHeight ?? 1,
                MultiSampleType = component?.MultiSampleType ?? MultisampleType.None,
                Windowed = true,
                BackBufferCount = 1,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = hWnd,
                AutoDepthStencilFormat = Format.D24X8,
                EnableAutoDepthStencil = true,
            };
        }

        private Surface CreateRenderTarget(Device device, int width, int height, Format surfaceFormat)
        {
            var texture = new Texture(device, width, height, 0, Usage.RenderTarget, surfaceFormat, Pool.Default);
            return texture.GetSurfaceLevel(0);
        }

        private Device CreateDevice(IntPtr hWnd, DeviceType deviceType)
        {
            var direct = new Direct3DEx();

            PresentParameters presentParameters = CreatePresentParameters(hWnd);

            try
            {
                foreach (var adapter in direct.Adapters)
                {
                    string adapterDeviceName = adapter.Details.Description;
                    if (!direct.CheckDeviceType(adapter.Adapter, deviceType, adapter.CurrentDisplayMode.Format,
                        presentParameters.BackBufferFormat, true))
                    {
                        throw new Exception($"Устройство {adapterDeviceName} c форматом {presentParameters.BackBufferFormat} не поддерживается");
                    }
                    if (!direct.CheckDeviceFormat(adapter.Adapter, deviceType, adapter.CurrentDisplayMode.Format,
                        Usage.RenderTarget | Usage.Dynamic, ResourceType.Surface,
                        presentParameters.BackBufferFormat))
                    {
                        throw new Exception($"Формат {presentParameters.BackBufferFormat} не поддерживается для этого адаптера {adapterDeviceName}");
                    }

                    if (!direct.CheckDeviceMultisampleType(adapter.Adapter, deviceType, presentParameters.BackBufferFormat,
                        true, presentParameters.MultiSampleType))
                    {
                        throw new Exception($"Сглаживание типа {presentParameters.MultiSampleType} не поддерживается устройством {adapterDeviceName}");
                    }
                }
            }
            catch (Exception ex)
            {
                direct.Dispose();
                throw ex;
            }

            var device = new DeviceEx(direct, 0, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                presentParameters);

            device.SetRenderState(RenderState.CullMode, Cull.None);
            device.SetRenderState(RenderState.Lighting, false);

            renderTarget = CreateRenderTarget(device, presentParameters.BackBufferWidth, presentParameters.BackBufferHeight, presentParameters.BackBufferFormat);
            device.SetRenderTarget(0, renderTarget);

            return device;
        }

        #region COMMANDS

        public static Command CloseWindowCommand
        {
            get => new Command((obj) =>
            {
                if (obj is Window window)
                {
                    window.Close();
                }
            }, (obj) =>
            {
                return obj != null;
            });
        }

        public Command CreateDeviceCommand
        {
            get => new Command(() =>
            {
                try
                {
                    IntPtr hWnd = CreateHwnd();
                    device = CreateDevice(hWnd, DeviceType.Hardware);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Не удалось создать устройство DirectX9");
                    return;
                }

                ActivePrimitive = new Box(device);
                ActivePrimitive.Transform.Position = new Vector3(0.4f, 0.2f, 0.3f);

                device.VertexDeclaration = new VertexDeclaration(device, Primitive.VertexElements);
                device.SetStreamSource(0, ActivePrimitive, 0, Primitive.Stride);

                DirectSurface = _di;

                BeginRenderingScene();
            }, (e) => device == null);
        }

        public Command RemoveDeviceCommand
        {
            get => new Command(() =>
            {
                renderTarget.Dispose();
                renderTarget = null;

                device.Dispose();
                device = null;

                DirectSurface = null;
            }, (e) => device != null);
        }

        #endregion COMMANDS
    }
}