using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace tema_5
{
    public class Game : GameWindow
    {
        private List<Cube> cubes = new List<Cube>();
        private Random random = new Random();
        private Color4 backgroundColor = Color4.CornflowerBlue;
        private Vector3 cameraPosition = new Vector3(0, 0, 5);
        private float cameraSpeed = 0.05f;
        private string filename = "cube_data.txt";
        private bool colorChanged = false;
        private List<Color4> initialColors;

        public Game() : base(800, 600, GraphicsMode.Default, "3D Color Manipulation Cube")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(backgroundColor);
            GL.Enable(EnableCap.DepthTest);

            EnsureFileExists(filename);

            LoadCubeData();

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Width / (float)Height, 0.1f, 100.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        private void EnsureFileExists(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Fișierul nu există. Creăm fișierul cu date implicite...");
                string[] defaultData = {
                    "0.5,0.5,0.5,1.0,0.0,0.0",  // Vertex 1
                    "-0.5,0.5,0.5,0.0,1.0,0.0", // Vertex 2
                    "-0.5,-0.5,0.5,0.0,0.0,1.0", // Vertex 3
                    "0.5,-0.5,0.5,1.0,1.0,0.0",  // Vertex 4
                    "0.5,0.5,-0.5,0.0,1.0,1.0",  // Vertex 5
                    "-0.5,0.5,-0.5,1.0,0.0,1.0", // Vertex 6
                    "-0.5,-0.5,-0.5,0.5,0.5,0.5", // Vertex 7
                    "0.5,-0.5,-0.5,0.5,0.0,0.5"  // Vertex 8
                };
                File.WriteAllLines(fileName, defaultData);
                Console.WriteLine("Fișierul a fost creat cu date implicite.");
            }
        }

        private void LoadCubeData()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Color4> colors = new List<Color4>();

            foreach (var line in File.ReadAllLines(filename))
            {
                var parts = line.Split(',');
                if (parts.Length == 6)
                {
                    float x = float.Parse(parts[0]);
                    float y = float.Parse(parts[1]);
                    float z = float.Parse(parts[2]);
                    Color4 color = new Color4(float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5]), 1.0f);

                    vertices.Add(new Vector3(x, y, z));
                    colors.Add(color);

                    Console.WriteLine($"Vertex: ({x}, {y}, {z}) - Color (R: {color.R}, G: {color.G}, B: {color.B})");
                }
            }

            initialColors = new List<Color4>(colors);

            cubes.Add(new Cube(vertices, colors));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Key.R))
                ChangeCubeColor(0, new Color4(1f, 0f, 0f, 1f));
            if (keyboardState.IsKeyDown(Key.G))
                ChangeCubeColor(0, new Color4(0f, 1f, 0f, 1f));
            if (keyboardState.IsKeyDown(Key.B))
                ChangeCubeColor(0, new Color4(0f, 0f, 1f, 1f));
            if (keyboardState.IsKeyDown(Key.V))
                ChangeCubeColor(0, GetRandomColor());
            if (keyboardState.IsKeyDown(Key.T))
                ChangeCubeColor(0, new Color4(1f, 0f, 0f, 0.5f));
            if (keyboardState.IsKeyDown(Key.C))
                ResetCubeColor(0);

            if (keyboardState.IsKeyDown(Key.W)) cameraPosition.Z -= cameraSpeed;
            if (keyboardState.IsKeyDown(Key.S)) cameraPosition.Z += cameraSpeed;
            if (keyboardState.IsKeyDown(Key.A)) cameraPosition.X -= cameraSpeed;
            if (keyboardState.IsKeyDown(Key.D)) cameraPosition.X += cameraSpeed;

            if (keyboardState.IsKeyDown(Key.Q)) cameraPosition.Y += cameraSpeed;
            if (keyboardState.IsKeyDown(Key.E)) cameraPosition.Y -= cameraSpeed;
        }

        private Color4 GetRandomColor()
        {
            return new Color4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1.0f);
        }

        private void ChangeCubeColor(int cubeIndex, Color4 color)
        {
            if (cubeIndex < cubes.Count)
            {
                cubes[cubeIndex].ChangeColor(color);
                Console.WriteLine($"Cube color changed to: R:{color.R}, G:{color.G}, B:{color.B}");
                colorChanged = true;
            }
        }

        private void ResetCubeColor(int cubeIndex)
        {
            if (cubeIndex < cubes.Count && initialColors != null)
            {
                cubes[cubeIndex].ResetColor(initialColors);
                Console.WriteLine("Cube colors reset to initial values.");
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 modelview = Matrix4.LookAt(cameraPosition, Vector3.Zero, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            foreach (var cube in cubes)
            {
                cube.Draw();
            }

            SwapBuffers();
        }

        [STAThread]
        public static void Main()
        {
            using (var game = new Game())
            {
                game.Run(60.0);
            }
        }
    }

    public class Cube
    {
        private List<Vector3> vertices;
        private List<Color4> colors;

        public Cube(List<Vector3> vertices, List<Color4> colors)
        {
            this.vertices = vertices;
            this.colors = colors;
        }

        public void ChangeColor(Color4 color)
        {
            for (int i = 0; i < colors.Count; i++)
            {
                colors[i] = color;
            }
        }

        public void ResetColor(List<Color4> initialColors)
        {
            for (int i = 0; i < initialColors.Count; i++)
            {
                colors[i] = initialColors[i];
            }
        }

        public void Draw()
        {
            GL.Begin(PrimitiveType.Triangles);

            // Fața din față (față + culori)
            DrawFace(0, 1, 2, 3);
            DrawFace(0, 2, 3, 1);

            // Fața din spate (față + culori)
            DrawFace(4, 5, 6, 7);
            DrawFace(4, 6, 7, 5);

            // Fața stângă
            DrawFace(0, 3, 7, 4);
            DrawFace(0, 7, 4, 3);
            // Fața dreaptă
            DrawFace(1, 2, 6, 5);
            DrawFace(1, 6, 5, 2);
            // Fața de sus
            DrawFace(0, 1, 5, 4);
            DrawFace(0, 5, 4, 1);

            // Fața de jos
            DrawFace(2, 3, 7, 6);
            DrawFace(2, 7, 6, 3);

            GL.End();
        }

        private void DrawFace(int v0, int v1, int v2, int v3)
        {
            GL.Color4(colors[v0]);
            GL.Vertex3(vertices[v0]);
            GL.Color4(colors[v1]);
            GL.Vertex3(vertices[v1]);
            GL.Color4(colors[v2]);
            GL.Vertex3(vertices[v2]);

            GL.Color4(colors[v0]);
            GL.Vertex3(vertices[v0]);
            GL.Color4(colors[v2]);
            GL.Vertex3(vertices[v2]);
            GL.Color4(colors[v3]);
            GL.Vertex3(vertices[v3]);
        }
    }

}
