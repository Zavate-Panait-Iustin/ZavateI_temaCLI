using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;

namespace tema_4
{
    public class Game : GameWindow
    {
        private List<Cube> cubes = new List<Cube>();
        private Random random = new Random();

        private bool isGridVisible = true;
        private Color4 backgroundColor = Color4.CornflowerBlue;
        private Vector3 cameraPosition = new Vector3(5, 5, 5);
        private float cameraSpeed = 0.5f;
        private float gravity = 9.8f;

        public Game() : base(800, 600, GraphicsMode.Default, "3D Cube Spawner")
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(backgroundColor);
            GL.Enable(EnableCap.DepthTest);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                (float)Width / Height,
                0.1f,
                100f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            DisplayMenu();
        }

        private void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("Meniu:");
            Console.WriteLine("ESC - Iesire");
            Console.WriteLine("B   - Schimba culoarea fundalului");
            Console.WriteLine("V   - Schimba vizibilitatea grid-ului");
            Console.WriteLine("G   - Schimba gravitatia");
            Console.WriteLine("Click stanga - Spawneaza cub");
            Console.WriteLine("Click dreapta - Curata lista de cuburi");
            Console.WriteLine("W, A, S, D   - Schimbare pozitie camera");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Deplasare camera
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Key.W)) cameraPosition.X -= cameraSpeed;
            if (keyboardState.IsKeyDown(Key.S)) cameraPosition.X += cameraSpeed;
            if (keyboardState.IsKeyDown(Key.A)) cameraPosition.Z += cameraSpeed;
            if (keyboardState.IsKeyDown(Key.D)) cameraPosition.Z -= cameraSpeed;

            if (keyboardState.IsKeyDown(Key.B))
            {
                ChangeBackgroundColor();
                DisplayMenu();
            }

            if (keyboardState.IsKeyDown(Key.G))
            {
                gravity = gravity == 9.8f ? 0f : 9.8f;

            }

            float deltaTime = (float)e.Time;
            foreach (var cube in cubes)
            {
                cube.Update(deltaTime, gravity);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left)
            {
                SpawnCube();
            }
            else if (e.Button == MouseButton.Right)
            {
                ClearCubes();
            }
        }

        private void SpawnCube()
        {
            var newCube = new Cube(
                position: new Vector3(
                    (float)(random.NextDouble() * 1.0),
                    (float)(random.NextDouble() * 3.0 + 3.0),
                    (float)(random.NextDouble() * 1.0)
                ),
                size: (float)(random.NextDouble() * 0.5 + 0.5),
                color: new Color4(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    1.0f
                )
            );
            cubes.Add(newCube);
        }

        private void ClearCubes()
        {
            cubes.Clear();
            Console.WriteLine("Lista de cuburi a fost curatata.");
        }

        private void ChangeBackgroundColor()
        {
            backgroundColor = new Color4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1.0f);
            GL.ClearColor(backgroundColor);
            Console.WriteLine("Fundal schimbat la culoarea: " + backgroundColor);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Isometric view
            Matrix4 modelview = Matrix4.LookAt(cameraPosition, new Vector3(0, 0, 0), Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);

            DrawAxes();

            if (isGridVisible)
            {
                DrawGrid();
            }

            foreach (var cube in cubes)
            {
                cube.Draw();
            }

            SwapBuffers();
        }

        private void DrawAxes()
        {
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(1.0f, 0.0f, 0.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(5, 0, 0);

            GL.Color3(0.0f, 1.0f, 0.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 5, 0);

            GL.Color3(0.0f, 0.0f, 1.0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 5);

            GL.End();
        }

        private void DrawGrid(int gridSize = 10, float spacing = 1.0f)
        {
            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.Begin(PrimitiveType.Lines);

            for (int i = -gridSize; i <= gridSize; i++)
            {
                GL.Vertex3(i * spacing, 0, -gridSize * spacing);
                GL.Vertex3(i * spacing, 0, gridSize * spacing);

                GL.Vertex3(-gridSize * spacing, 0, i * spacing);
                GL.Vertex3(gridSize * spacing, 0, i * spacing);
            }

            GL.End();
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
        public Vector3 Position { get; private set; }
        private float Size { get; set; }
        private Color4 Color { get; set; }

        public Cube(Vector3 position, float size, Color4 color)
        {
            Position = position;
            Size = size;
            Color = color;
        }

        public void Update(float deltaTime, float gravity)
        {
            if (Position.Y > 0)
            {
                Position -= new Vector3(0, deltaTime * gravity, 0);
                if (Position.Y < 0)
                {
                    Position = new Vector3(Position.X, 0, Position.Z);
                }
            }
        }

        public void Draw()
        {
            GL.PushMatrix();
            GL.Translate(Position);
            GL.Scale(Size, Size, Size);
            GL.Color4(Color);

            GL.Begin(PrimitiveType.Quads);
            // Front face
            GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.Vertex3(0.5f, 0.5f, 0.5f);
            GL.Vertex3(-0.5f, 0.5f, 0.5f);
            // Back face
            GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.Vertex3(0.5f, 0.5f, -0.5f);
            GL.Vertex3(0.5f, -0.5f, -0.5f);
            // Top face
            GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.Vertex3(-0.5f, 0.5f, 0.5f);
            GL.Vertex3(0.5f, 0.5f, 0.5f);
            GL.Vertex3(0.5f, 0.5f, -0.5f);
            // Bottom face
            GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.Vertex3(0.5f, -0.5f, 0.5f);
            GL.Vertex3(-0.5f, -0.5f, 0.5f);
            // Right face
            GL.Vertex3(0.5f, -0.5f, -0.5f);
            GL.Vertex3(0.5f, 0.5f, -0.5f);
            GL.Vertex3(0.5f, 0.5f, 0.5f);
            GL.Vertex3(0.5f, -0.5f, 0.5f);
            // Left face
            GL.Vertex3(-0.5f, -0.5f, 0.5f);
            GL.Vertex3(-0.5f, 0.5f, 0.5f);
            GL.Vertex3(-0.5f, 0.5f, -0.5f);
            GL.Vertex3(-0.5f, -0.5f, -0.5f);
            GL.End();

            GL.PopMatrix();
        }
    }
}
