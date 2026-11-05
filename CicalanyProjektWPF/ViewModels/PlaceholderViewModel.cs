namespace Cicalany.Wpf.ViewModels;

/// <summary>Átmeneti oldal azokhoz a részekhez, amelyek még nincsenek megépítve.</summary>
public sealed class PlaceholderViewModel(string title, string message) : ViewModelBase
{
    public string Title { get; } = title;
    public string Message { get; } = message;
}
