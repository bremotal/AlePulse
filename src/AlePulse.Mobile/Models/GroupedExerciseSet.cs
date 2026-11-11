using System.Collections.ObjectModel;

namespace AlePulse.Mobile.Models;

public class GroupedExerciseSet : ObservableCollection<ExerciseSetDto>
{
    public string DateDisplay { get; set; } = string.Empty;

    public GroupedExerciseSet(string dateDisplay, IEnumerable<ExerciseSetDto> sets)
    {
        DateDisplay = dateDisplay;
        foreach (var set in sets)
        {
            Add(set);
        }
    }
}