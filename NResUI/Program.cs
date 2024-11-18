// See https://aka.ms/new-console-template for more information

using System.Drawing;
using ImGuiNET;
using NResUI;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

var window = Window.Create(WindowOptions.Default);

// Declare some variables
ImGuiController controller = null!;
GL gl = null!;
IInputContext inputContext = null!;

window.IsEventDriven = true;

var app = new App();

// Our loading function
window.Load += () =>
{
    var openGl = window.CreateOpenGL();

    ImFontPtr mainFont = null;

    controller = new ImGuiController(
        gl = openGl, // load OpenGL
        window, // pass in our window
        inputContext = window.CreateInput(), // create an input context
        () =>
        {
            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            mainFont = io.Fonts.AddFontFromFileTTF(
                filename: "assets/Font/OpenSans-Regular.ttf",
                size_pixels: 18,
                font_cfg: null,
                glyph_ranges: io.Fonts.GetGlyphRangesCyrillic()
            );
        }
    );
    app.Init(window, openGl, mainFont);

    inputContext.Keyboards[0]
        .KeyDown += (keyboard, key, scancode) => { app.OnKeyDown(key); };
    inputContext.Keyboards[0]
        .KeyUp += (keyboard, key, scancode) => { app.OnKeyPressed(key); };
    inputContext.Keyboards[0]
        .KeyUp += (keyboard, key, scancode) => { app.OnKeyReleased(key); };
};

// Handle resizes
window.FramebufferResize += s =>
{
    // Adjust the viewport to the new window size
    gl.Viewport(s);
};

// Handles the dile drop and receives the array of paths to the files.
window.FileDrop += paths => { };

window.Update += delta =>
{
    // Make sure ImGui is up-to-date
    controller.Update((float) delta);

    app.Update(delta);
};

// The render function
window.Render += delta =>
{
    // This is where you'll do any rendering beneath the ImGui context
    // Here, we just have a blank screen.
    gl.ClearColor(
        Color.FromArgb(
            255,
            (int) (.45f * 255),
            (int) (.55f * 255),
            (int) (.60f * 255)
        )
    );
    gl.Clear((uint) ClearBufferMask.ColorBufferBit);

    app.OnImGuiRender();

    // Make sure ImGui renders too!
    controller.Render();
};

// The closing function
window.Closing += () =>
{
    app.Exit();

    ImGui.SaveIniSettingsToDisk("imgui.ini");
    // Dispose our controller first
    controller?.Dispose();

    // Dispose the input context
    inputContext?.Dispose();

    // Unload OpenGL
    gl?.Dispose();
};

// Now that everything's defined, let's run this bad boy!
window.Run();