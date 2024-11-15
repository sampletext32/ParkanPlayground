using System.Numerics;
using ImGuiNET;
using NResUI.Abstractions;

namespace NResUI.ImGuiUI;

public class MessageBoxModalPanel : ImGuiModalPanel
{
    private string? _text;
    protected override string ImGuiId { get; } = "#message-box";

    public MessageBoxModalPanel()
    {
        WindowSize = new Vector2(300, 150);
    }

    public void Show(string? text)
    {
        _text = text;
        base.Open();
    }

    protected override void OnImGuiRenderContent()
    {
        ImGui.Text(_text);
        ImGui.Spacing();

        if (ImGui.Button("Ок"))
        {
            ImGui.CloseCurrentPopup();
        }
    }
}