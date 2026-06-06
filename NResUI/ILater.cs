namespace NResUI;

public interface ILater
{
    void Execute(Action action);
}

public class Later : ILater
{
    private Queue<Action> _queue = new();
    
    public void Execute(Action action)
    {
        _queue.Enqueue(action);
    }

    public IEnumerable<Action> Enumerate()
    {
        while (_queue.Count > 0)
        {
            var action = _queue.Dequeue();
            yield return action;
        }
    }
}