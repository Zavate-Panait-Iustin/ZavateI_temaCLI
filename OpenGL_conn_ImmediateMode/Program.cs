using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

class InteractiveWindow : GameWindow
{
    private Vector3[] vertices = new Vector3[3];
    private float[] color = { 1.0f, 0.0f, 0.0f, 1.0f };
    private float cameraAngleX = 0.0f;
    private float cameraAngleY = 0.0f;

    public InteractiveWindow()
        : base(800, 600, GraphicsMode.Default, "Triangle Color and Camera Control")
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        GL.ClearColor(0.39f, 0.58f, 0.93f, 1.0f);

        LoadTriangleVertices("triangle_vertices.txt");

        GL.Enable(EnableCap.DepthTest);
    }

    private void LoadTriangleVertices(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < 3; i++)
        {
            string[] parts = lines[i].Split(' ');
            vertices[i] = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);


        if (Keyboard.GetState().IsKeyDown(Key.R)) color[0] = Math.Min(1.0f, color[0] + 0.01f);
        if (Keyboard.GetState().IsKeyDown(Key.G)) color[1] = Math.Min(1.0f, color[1] + 0.01f);
        if (Keyboard.GetState().IsKeyDown(Key.B)) color[2] = Math.Min(1.0f, color[2] + 0.01f);
        if (Keyboard.GetState().IsKeyDown(Key.A)) color[3] = Math.Max(0.0f, color[3] - 0.01f);


        if (Keyboard.GetState().IsKeyDown(Key.C))
        {
            color[0] = 1.0f; color[1] = 0.0f; color[2] = 0.0f; color[3] = 1.0f;
        }
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);


        cameraAngleX += e.XDelta * 0.1f;
        cameraAngleY += e.YDelta * 0.1f;
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
        GL.Rotate(cameraAngleX, 0.0f, 1.0f, 0.0f);  // Rotirea pe axa Y
        GL.Rotate(cameraAngleY, 1.0f, 0.0f, 0.0f);  // Rotirea pe axa X

        GL.Begin(PrimitiveType.Triangles);

        // Primul vertex roșu
        GL.Color3(1.0f, 0.0f, 0.0f);
        GL.Vertex3(vertices[0]);
        Console.WriteLine($"Vertex 1 RGB: 1.0, 0.0, 0.0");

        // Al doilea vertex verde
        GL.Color3(0.0f, 1.0f, 0.0f);
        GL.Vertex3(vertices[1]);
        Console.WriteLine($"Vertex 2 RGB: 0.0, 1.0, 0.0");

        // Al treilea vertex albastru
        GL.Color3(0.0f, 0.0f, 1.0f);
        GL.Vertex3(vertices[2]);
        Console.WriteLine($"Vertex 3 RGB: 0.0, 0.0, 1.0");

        GL.End();

        SwapBuffers();
    }


    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Width, Height);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(-1.0, 1.0, -1.0, 1.0, -10.0, 10.0);
        GL.MatrixMode(MatrixMode.Modelview);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        // Închidem aplicația la apăsarea tastei Escape
        if (e.Key == Key.Escape)
            this.Close();
    }

    static void Main(string[] args)
    {
        using (InteractiveWindow window = new InteractiveWindow())
        {
            window.Run(60.0);
        }
    }
}
