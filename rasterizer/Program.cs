using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Texture2D = rasterizer.Buffer2D<uint>;
using Color = System.UInt32;

namespace rasterizer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Glfw.Init())
            {
                Console.Error.WriteLine("Failed to initialize GLFW");
                Environment.Exit(0);
            }
            MainGameState main = new MainGameState();
            main.init();
            main.run();

        }
    }

    public class MainGameState
    {
        public const int WindowWidth = 800, WindowHeight = 600;

        public const int scale = 1;
        public const int ScreenWidth = WindowWidth / scale, ScreenHeight = WindowHeight / scale;

        public static Texture2D Screen;
        private int oglTexture;

        public static Color GetColor(float a, float r, float g, float b)
        {
            byte rb = (byte)(a * 255);
            byte gb = (byte)(r * 255);
            byte bb = (byte)(g * 255);
            byte ab = (byte)(b * 255);
            return (Color)((rb << 24) | (gb << 16) | (bb << 8) | ab);

        }

        public void init()
        {
            Glfw.OpenWindowHint(OpenWindowHint.NoResize, 1);
            Glfw.OpenWindow(WindowWidth, WindowHeight, 8, 8, 8, 0, 24, 0, WindowMode.Window);
            Glfw.SetWindowPos(100, 100);
            Glfw.SetWindowTitle("Rasterizer");
            Glfw.SwapInterval(false);
            GL.Enable(EnableCap.TextureRectangle);
            GL.GenTextures(1, out oglTexture);
            GL.BindTexture(TextureTarget.TextureRectangle, oglTexture);
            GL.TexParameter(TextureTarget.TextureRectangle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.Viewport(0, 0, WindowWidth, WindowHeight);
            GL.MatrixMode(MatrixMode.Projection);
            GL.Ortho(0.0, WindowWidth, WindowHeight, 0.0, 0.0, 1.0);
            GL.MatrixMode(MatrixMode.Modelview);

            Screen = new Texture2D(ScreenWidth, ScreenHeight);
        }

        public void run()
        {
            while (Glfw.GetWindowParam(WindowParam.Opened) == 1)
            {
                Draw();

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();


                GL.Enable(EnableCap.Texture2D);

                GL.BindTexture(TextureTarget.TextureRectangle, oglTexture);
                GL.TexImage2D(TextureTarget.TextureRectangle, 0, PixelInternalFormat.Rgba, ScreenWidth, ScreenHeight, 0, PixelFormat.Bgra, PixelType.UnsignedByte, Screen.Data);


                GL.Begin(BeginMode.Quads);

                GL.TexCoord2(0, 0);
                GL.Vertex2(0, 0);
                GL.TexCoord2(ScreenWidth, 0);
                GL.Vertex2(WindowWidth, 0);
                GL.TexCoord2(ScreenWidth, ScreenHeight);
                GL.Vertex2(WindowWidth, WindowHeight);
                GL.TexCoord2(0, ScreenHeight);
                GL.Vertex2(0, WindowHeight);

                GL.End();

                Glfw.SwapBuffers();
                Glfw.PollEvents();
            }
        }

        public void Draw()
        {
            Screen.Fill((x, y) =>
            {
                float yDist = Math.Abs(ScreenHeight / 2 - y);
                yDist /= ScreenHeight / 2;

                // yDist: 0; z: inf
                // yDist: 1, z: 1
                //
                // z(yDist) = 1 / yDist
                //
                // yDist = 1 / z
                // yDist = | ScreenHeight / 2 - y |;
                // | ScreenHeight / 2 - y | = 1 / z
                //

                float zMS = 1f / yDist;

                float xDist = ScreenWidth / 2 - x;
                xDist /= ScreenWidth / 2;


                float xMS = xDist * zMS / 2;



                zMS += (float)Glfw.GetTime();

                float zCol = tcmod(zMS, 1);
                float xCol = tcmod(xMS, 1);
                if (zMS > 3 && zMS < 5 && Math.Abs(xMS) > 1f)
                {
                    return GetColor(1, 0.5f, 0.5f, 0.5f);
                }
                else
                {
                    return GetColor(1, zCol * yDist, xCol * yDist, 0);
                }
            });

        }

        private static float tcmod(float x, float mod)
        {
            return x < 0 ? Math.Abs(mod - (Math.Abs(x) % mod)) : x % mod;
        }
    }
}
