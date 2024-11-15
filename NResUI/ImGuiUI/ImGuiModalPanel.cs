using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;

namespace NResUI.ImGuiUI;

public abstract class ImGuiModalPanel : IImGuiPanel
{
    protected abstract string ImGuiId { get; }

    private bool _shouldOpen = false;

    public virtual void Open()
    {
        _shouldOpen = true;
    }

    protected abstract void OnImGuiRenderContent();

    public void OnImGuiRender()
    {
        // this is a ImGui stack fix. Because menubars and some other controls use their separate stack context,
        // The panel gets rendered on it's own, at the root of the stack, and with _shouldOpen we control, if the panel should open this frame.
        if (_shouldOpen)
        {
            ImGui.OpenPopup(ImGuiId, ImGuiPopupFlags.AnyPopupLevel);
            _shouldOpen = false;
        }

        ImGui.SetNextWindowSize(new Vector2(600, 400));

        if (ImGui.BeginPopup(ImGuiId, ImGuiWindowFlags.NoResize))
        {
            OnImGuiRenderContent();

            ImGui.EndPopup();
        }
    }
}