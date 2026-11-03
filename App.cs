using System.Diagnostics;
using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;

namespace HelloBrutal;

internal record struct Vertex(float3 Position, float3 Color);

public class App : IDisposable
{
    private readonly ImGuiContextPtr _context;
    private readonly GlfwWindow _window;

    private const int Width = 800;
    private const int Height = 600;

    private Camera _camera;
    private float dt;

    public App()
    {
        if (!Glfw.Init())
        {
            var errorCode = Glfw.GetError(out var errorMessage);
            throw new InvalidOperationException($"Failed to initialize GLFW: {errorCode}, {errorMessage}");
        }

        _context = ImGui.CreateContext();

        Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4);
        Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 1);
        Glfw.WindowHint(GlfwWindowHint.OpenGlProfile, Glfw.Constants.GLFW_OPENGL_CORE_PROFILE);

        _window = Glfw.CreateWindow(
            new GlfwWindow.CreateInfo { Title = "Hello ImGui", Size = new int2(Width, Height) }
        );

        _window.MakeContextCurrent();
        Glfw.SwapInterval(1); // vsync

        unsafe
        {
            if (!Glad.Initialize(Glfw.PInvoke.GetProcAddress))
                throw new InvalidOperationException("Failed to load OpenGL functions with GLAD");
        }
        
        OpenGl.Enable(GlCapability.CullFace);

        _camera = new Camera();
        
        _window.OnKey += (handle, key, _, _, _) =>
        {
            if (key == GlfwKey.Escape)
                handle.ShouldClose = true;

            var speed = 20.0f;
            
            if (key == GlfwKey.A)
                _camera.MoveRelative(-float3.UnitX * dt * speed);
            if (key == GlfwKey.D)
                _camera.MoveRelative(float3.UnitX * dt * speed);
            if (key == GlfwKey.W)
                _camera.MoveRelative(float3.UnitZ * dt * speed);
            if (key == GlfwKey.S)
                _camera.MoveRelative(-float3.UnitZ * dt * speed);
            if (key == GlfwKey.Space)
                _camera.MoveRelative(float3.UnitY * dt * speed);
            if (key == GlfwKey.LeftControl)
                _camera.MoveRelative(-float3.UnitY * dt * speed);

            var sensitivity = 200.0f;
            
            if (key == GlfwKey.Up) 
                _camera.Rotate(0.0f, float.DegreesToRadians(1.0f * dt * sensitivity));
            if (key == GlfwKey.Down) 
                _camera.Rotate(0.0f, float.DegreesToRadians(-1.0f * dt * sensitivity));
            if (key == GlfwKey.Left) 
                _camera.Rotate(float.DegreesToRadians(-1.0f * dt * sensitivity), 0.0f);
            if (key == GlfwKey.Right) 
                _camera.Rotate(float.DegreesToRadians(1.0f * dt * sensitivity), 0.0f);
        }; 

        _window.OnFramebufferSize += (_, size) =>
        {
            OpenGl.Viewport(0, 0, size.X, size.Y);
            _camera.AspectRatio = (float)size.X / size.Y;
        };
    }

    public void Run()
    {
        using var program = LinkProgram();

        var vertexArray = OpenGl.GenVertexArray().Bind();
        var vertexBuffer = OpenGl.GenBuffer();
        vertexBuffer.Bind(GlBindTarget.ArrayBuffer);
        OpenGl.BufferData(
            GlBindTarget.ArrayBuffer,
            new Vertex[]
            {
                new(new float3(-0.5f, -0.5f, 0.0f), new float3(0.0f, 0.0f, 0.0f)),
                new(new float3(0.5f, -0.5f, 0.0f), new float3(1.0f, 0.0f, 0.0f)),
                new(new float3(0.5f, 0.5f, 0.0f), new float3(1.0f, 1.0f, 0.0f)),
                new(new float3(0.5f, 0.5f, 0.0f), new float3(1.0f, 1.0f, 0.0f)),
                new(new float3(-0.5f, 0.5f, 0.0f), new float3(0.0f, 1.0f, 0.0f)),
                new(new float3(-0.5f, -0.5f, 0.0f), new float3(0.0f, 0.0f, 0.0f))
            },
            GlBufferUsage.StaticDraw
        );

        OpenGl.VertexAttribute(new GlAttribute(0, 3, OpenGl.GL_FLOAT, GlScaling.Float, 6 * sizeof(float), 0));
        OpenGl.VertexAttribute(
            new GlAttribute(1, 3, OpenGl.GL_FLOAT, GlScaling.Float, 6 * sizeof(float), 3 * sizeof(float)));
        OpenGl.EnableVertexAttribArray(0);
        OpenGl.EnableVertexAttribArray(1);

        ImGui.GetIO().IniFilename = null;

        using var glfwBackend = new GlfwBackend(_window, GlfwBackend.ClientApi.OpenGL);
        using var openGlBackend = new OpenGlBackend();

        OpenGl.ClearColor(float4.Rgb(0.2f, 0.3f, 0.3f));
        
        var model = float4x4.Identity;

        var last = Stopwatch.GetTimestamp();
        
        while (!_window.ShouldClose)
        {
            var now = Stopwatch.GetTimestamp();
            dt = float.Min((float)Stopwatch.GetElapsedTime(last, now).TotalSeconds, 0.1f);
            last = now;
            
            Glfw.PollEvents();

            glfwBackend.NewFrame();
            openGlBackend.NewFrame();
            ImGui.NewFrame();
            ImString.SharedStorage.Reset();

            ImGui.SetNextWindowSize(new float2(400, 200), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Hello ImGui"u8))
            {
                ImGui.Text("Hello from Dear ImGui!"u8);

                ImGui.DragFloat3("Camera Position"u8, ref _camera.Position);
            }

            ImGui.End();

            ImGui.Render();

            OpenGl.Clear(GlClearBit.Color);
            program.Use();

            program.Uniform("mvp"u8, model * _camera.GetView() * _camera.GetProjection());

            vertexArray.Bind();
            OpenGl.Draw(GlTopology.Triangles, 6, 1, 0, 0);
            openGlBackend.RenderDrawData(ImGui.GetDrawData());
            _window.SwapBuffers();
        }

        OpenGl.DeleteVertexArrays([vertexArray]);
        OpenGl.DeleteBuffers([vertexBuffer]);
    }

    private static GlProgram LinkProgram()
    {
        using var vertexShader = OpenGl
            .CreateShader(GlShaderType.Vertex)
            .SetSource(
                """
                #version 410 core
                layout (location = 0) in vec3 aPos;
                layout (location = 1) in vec3 aColor;

                uniform mat4 mvp;

                out vec3 color;

                void main()
                {
                    color = aColor;
                    gl_Position = mvp * vec4(aPos, 1.0);
                }
                """u8
            )
            .Compile();

        using var fragmentShader = OpenGl
            .CreateShader(GlShaderType.Fragment)
            .SetSource(
                """
                #version 410 core
                in vec3 color;
                out vec4 FragColor;
                void main()
                {
                    FragColor = vec4(color, 1.0);
                }
                """u8
            )
            .Compile();

        return OpenGl.CreateProgram().Attach(vertexShader, fragmentShader).Link();
    }


    public void Dispose()
    {
        ImGui.DestroyContext(_context);
        Glfw.Terminate();

        GC.SuppressFinalize(this);
    }
}