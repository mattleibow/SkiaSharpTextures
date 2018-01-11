using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Input;
using SkiaSharp;

namespace SkiaSharpTextures
{
	public sealed class MainWindow : GameWindow
	{
		private int texture;

		private GRGlInterface glInterface;
		private GRContext context;

		public MainWindow()
			: base(1280, 720)
		{
			Title = "SkiaSharpTextures - OpenGL Version: " + GL.GetString(StringName.Version);
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
		}

		protected override void OnLoad(EventArgs e)
		{
			CursorVisible = true;

			glInterface = GRGlInterface.CreateNativeGlInterface();
			context = GRContext.Create(GRBackend.OpenGL, glInterface);

			texture = GL.GenTexture();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			Title = $"(Vsync: {VSync}) FPS: {1f / e.Time:0}";


			{
				var desc = new GRBackendTextureDesc
				{
					Config = GRPixelConfig.Rgba8888,
					Flags = GRBackendTextureDescFlags.RenderTarget,
					Height = 480,
					Origin = GRSurfaceOrigin.TopLeft,
					SampleCount = 0,
					TextureHandle = (IntPtr)texture,
					Width = 640
				};

				using (var surface = SKSurface.CreateAsRenderTarget(context, desc))
				{
					surface.Canvas.Clear(SKColors.Green);

					using (var paint = new SKPaint { IsAntialias = true, TextSize = 50, TextAlign = SKTextAlign.Center })
					{
						surface.Canvas.DrawText("Texture!", 320, 240 + 25, paint);
					}

					context.Flush();
				}
			}


			{
				var desc = new GRBackendRenderTargetDesc
				{
					RenderTargetHandle = (IntPtr)0,
					Width = 1280,
					Height = 720,
					Config = GRPixelConfig.Rgba8888,
					Origin = GRSurfaceOrigin.TopLeft,
					SampleCount = 0,
					StencilBits = 0,
				};

				using (var surface = SKSurface.Create(context, desc))
				{
					surface.Canvas.Clear(SKColors.Red);

					using (var paint = new SKPaint { IsAntialias = true, TextSize = 100, TextAlign = SKTextAlign.Center })
					{
						surface.Canvas.DrawText("Hello World!", 640, 360 + 50, paint);
					}

					context.Flush();
				}
			}


			SwapBuffers();
		}
	}
}
