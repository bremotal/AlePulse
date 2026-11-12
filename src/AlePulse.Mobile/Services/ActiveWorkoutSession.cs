using AlePulse.Mobile.Models;

namespace AlePulse.Mobile.Services;

public static class ActiveWorkoutSession
{
    public static Guid WorkoutId { get; private set; }
    public static List<WorkoutExerciseDto> Exercises { get; private set; } = new();
    public static int CurrentIndex { get; private set; }
    public static DateTime StartTime { get; private set; }
    public static bool IsActive { get; private set; }

    public static TimeSpan Elapsed => DateTime.Now - StartTime;

    public static void Start(Guid workoutId, List<WorkoutExerciseDto> exercises)
    {
        WorkoutId = workoutId;
        Exercises = exercises;
        CurrentIndex = 0;
        StartTime = DateTime.Now;
        IsActive = true;
        CrashReportService.AddBreadcrumb($"Treino iniciado ({exercises.Count} exercícios)");
    }

    public static void UpdateExercises(List<WorkoutExerciseDto> exercises, int keepIndex)
    {
        Exercises = exercises;
        CurrentIndex = exercises.Count == 0 ? 0 : Math.Clamp(keepIndex, 0, exercises.Count - 1);
    }

    public static void SetCurrentIndex(int index)
    {
        if (Exercises.Count == 0) return;
        CurrentIndex = Math.Clamp(index, 0, Exercises.Count - 1);
    }

    public static void Finish()
    {
        IsActive = false;
        Exercises = new List<WorkoutExerciseDto>();
        CurrentIndex = 0;
        CrashReportService.AddBreadcrumb("Sessão de treino encerrada");
    }
}
