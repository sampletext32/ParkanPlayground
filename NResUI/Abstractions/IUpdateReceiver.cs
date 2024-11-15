namespace NResUI.Abstractions
{
    public interface IUpdateReceiver
    {
        /// <summary>
        /// Called before every UI render 
        /// </summary>
        void OnUpdate(float delta);
    }
}