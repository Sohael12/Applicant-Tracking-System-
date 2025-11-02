public interface IStateNotifier
{
    event Action? StateChanged;
    void NotifyStateChanged();
}

public class StateNotifier : IStateNotifier
{
    public event Action? StateChanged;

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();  // Dit roept de geregistreerde componenten aan om te updaten
    }
}
