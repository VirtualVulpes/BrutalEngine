using Brutal.Enums;
using Brutal.GlfwApi;
using Brutal.Numerics;

namespace HelloBrutal;

public class InputManager
{
    private readonly GlfwKeyAction[] _actions = new GlfwKeyAction[(int)GlfwKey.Last];

    public double2 MousePos { get; private set; } = double2.Zero;
    public double2 MouseDelta { get; private set; } = double2.Zero;
    
    public InputManager(GlfwWindow window)
    {
        window.OnKey += OnKey;
        window.OnCursorPos += OnCursor;
    }

    private void OnKey(GlfwWindow window, GlfwKey key, int scancode, GlfwKeyAction action, GlfwModifier mod)
    {
        _actions[(int)key] = action;
    }

    private void OnCursor(GlfwWindow window, double2 pos)
    {
        MouseDelta += pos - MousePos;
        MousePos = pos;
    }

    public bool IsKeyDown(GlfwKey key)
    {
        return _actions[(int)key] is GlfwKeyAction.Press or GlfwKeyAction.Repeat;
    }
    
    public void Update()
    {
        MouseDelta = double2.Zero;
    }
}