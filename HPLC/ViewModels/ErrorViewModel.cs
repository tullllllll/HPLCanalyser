using ReactiveUI;

namespace HPLC.ViewModels;

public class ErrorViewModel : ReactiveObject
{
    private string _errorMessage;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public ErrorViewModel(string message)
    {
        ErrorMessage = message;
    }
}