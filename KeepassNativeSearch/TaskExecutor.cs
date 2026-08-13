namespace KeepassNativeSearch;

/**
 * <summary>A utility class that takes in an action and schedules it on a task. If a new action gets scheduled,
 * the old one (if running) is canceled and disposed</summary>
 *
 */
public class TaskExecutor
{
    private CancellationTokenSource? _cts;

    /**
     * Schedules an action, and cancels any running after a given delay in milliseconds.
     *
     * <param name="action">The action to run in a task.</param>
     * <param name="millisecondsDelay">The delay before an action is executed in a Task.</param>
     */
    public void Execute(Action action, int millisecondsDelay = 300)
    {
        try
        {
            // Cancel the prior task
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var token = _cts.Token;

            Task.Delay(millisecondsDelay, token).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    action();
                }
            }, token, TaskContinuationOptions.None, TaskScheduler.Default);
        }
        catch (OperationCanceledException)
        {
            // Task was canceled because the user changed input again; ignore safely
        }
    }

    /**
     * <summary>
     * Cancels any pending actions.
     * </summary>
     */
    public void Cancel()
    {
        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // Safely do nothing
        }
    }
}