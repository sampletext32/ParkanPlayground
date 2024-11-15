using Silk.NET.Input;

namespace NResUI.Abstractions
{
    public interface IKeyPressReceiver
    {
        void OnKeyPressed(Key key);
    }
    public interface IKeyReleaseReceiver
    {
        void OnKeyReleased(Key key);
    }
    public interface IKeyDownReceiver
    {
        void OnKeyDown(Key key);
    }
}