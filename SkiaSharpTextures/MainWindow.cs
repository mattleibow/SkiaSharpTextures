using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SkiaSharp;

namespace SkiaSharpTextures
{
	public sealed class MainWindow : GameWindow
	{
		private GRContext context;

		private int textureId;
		private GCHandle textureHandle;
		private GRBackendTextureDesc textureDesc;
		private SKSurface textureSurface;

		private GRBackendRenderTargetDesc renderDesc;
		private SKSurface renderSurface;

		private bool previousState;

		public MainWindow()
			: base(1280, 720)
		{
			Title = $"SkiaSharp Textures - OpenGL {GL.GetString(StringName.Version)}";

			Load += OnLoad;
			Resize += OnResize;
			Unload += OnUnload;
			UpdateFrame += OnUpdateFrame;
			RenderFrame += OnRenderFrame;
		}

		private void OnResize(object sender, EventArgs e)
		{
			GL.Viewport(0, 0, Width, Height);
		}

		private void OnLoad(object sender, EventArgs e)
		{
			CursorVisible = true;

			// CONTEXT

			// create the SkiaSharp context
			var glInterface = GRGlInterface.CreateNativeGlInterface();
			context = GRContext.Create(GRBackend.OpenGL, glInterface);

			// TEXTURE

			// the texture size
			var textureSize = new SKSizeI(256, 256);

			// create the OpenGL texture
			textureId = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, textureId);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textureSize.Width, textureSize.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.BindTexture(TextureTarget.Texture2D, 0);

			// create the SkiaSharp texture description
			var textureInfo = new GRGlTextureInfo
			{
				Id = (uint)textureId,
				Target = (uint)TextureTarget.Texture2D
			};
			textureHandle = GCHandle.Alloc(textureInfo, GCHandleType.Pinned);
			textureDesc = new GRBackendTextureDesc
			{
				Width = textureSize.Width,
				Height = textureSize.Height,
				Config = GRPixelConfig.Rgba8888,
				Flags = GRBackendTextureDescFlags.RenderTarget,
				Origin = GRSurfaceOrigin.TopLeft,
				SampleCount = 0,
				TextureHandle = textureHandle.AddrOfPinnedObject(),
			};

			// create the SkiaSharp texture surface
			textureSurface = SKSurface.CreateAsRenderTarget(context, textureDesc);

			// initialize the texture content
			UpdateTexture(false);

			// RENDER TARGET

			// create the SkiaSharp render target description
			renderDesc = new GRBackendRenderTargetDesc
			{
				RenderTargetHandle = (IntPtr)0,
				Width = Width,
				Height = Height,
				Config = GRPixelConfig.Rgba8888,
				Origin = GRSurfaceOrigin.TopLeft,
				SampleCount = 0,
				StencilBits = 0,
			};

			// create the SkiaSharp render target surface
			renderSurface = SKSurface.Create(context, renderDesc);
		}

		private void OnUnload(object sender, EventArgs e)
		{
			textureSurface.Dispose();
			textureSurface = null;

			renderSurface.Dispose();
			renderSurface = null;

			context.Dispose();
			context = null;

			textureHandle.Free();
		}

		private void OnUpdateFrame(object sender, FrameEventArgs e)
		{
			var currentState = Mouse[MouseButton.Left];
			if (previousState != currentState)
			{
				UpdateTexture(currentState);
			}
			previousState = currentState;
		}

		private void OnRenderFrame(object sender, FrameEventArgs e)
		{
			var renderCanvas = renderSurface.Canvas;

			renderCanvas.Clear(SKColors.CornflowerBlue);

			using (var paint = new SKPaint { IsAntialias = true, TextSize = 100, TextAlign = SKTextAlign.Center })
			{
				renderCanvas.DrawText("Hello World!", renderDesc.Width / 2, 150, paint);
			}
			using (var paint = new SKPaint { IsAntialias = true, TextSize = 24, Typeface = SKTypeface.FromFamilyName(null, SKTypefaceStyle.Italic) })
			{
				renderCanvas.DrawText($"V-Sync: {VSync}", 16, 16 + paint.TextSize, paint);
				renderCanvas.DrawText($"FPS: {1f / e.Time:0}", 16, 16 + paint.TextSize + 8 + paint.TextSize, paint);
			}

			renderCanvas.DrawSurface(textureSurface, (renderDesc.Width - textureDesc.Width) / 2, 200);

			context.Flush();

			SwapBuffers();
		}

		private void UpdateTexture(bool isDown)
		{
			var textureCanvas = textureSurface.Canvas;

			textureCanvas.Clear(SKColors.SeaGreen);

			using (var paint = new SKPaint { IsAntialias = true, TextSize = 32, TextAlign = SKTextAlign.Center })
			{
				var y = (textureDesc.Height + paint.TextSize) / 2;
				textureCanvas.DrawText("Texture!", textureDesc.Width / 2, y, paint);

				paint.Typeface = SKTypeface.FromFamilyName(null, SKTypefaceStyle.Italic);
				textureCanvas.DrawText(isDown ? "(mouse down)" : "(try clicking)", textureDesc.Width / 2, y + paint.TextSize + 8, paint);
			}

			context.Flush();
		}

		public struct GRGlTextureInfo
		{
			public uint Target;
			public uint Id;
		}
	}
}
