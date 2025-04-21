using System;
using BaboonAPI.Hooks;
using BaboonAPI.Hooks.Tracks;
using JetBrains.Annotations;

namespace TootTallyCore.Utils.Helpers;

/// <summary>
/// Handles reloading tracks & collections, ensuring only one reload is active at a time.
/// </summary>
public class ReloadManager
{
    private readonly Plugin _plugin;
    private ReloadOperation ActiveOperation { get; set; }
    public bool IsCurrentlyReloading => ActiveOperation != null;

    public ReloadManager(Plugin plugin)
    {
        _plugin = plugin;
    }

    public void ReloadAll([CanBeNull] ProgressCallbacks callbacks)
    {
        if (IsCurrentlyReloading) return;

        ActiveOperation = new ReloadOperation(callbacks);
        _plugin.StartCoroutine(TrackReloader.ReloadAll(ActiveOperation.OnProgress).ForEach(result =>
        {
            ActiveOperation = null;
            if (result.IsError)
            {
                callbacks?.OnError?.Invoke(result.ErrorValue);
            }
            else
            {
                callbacks?.OnComplete?.Invoke();
            }
        }));
    }

    public void Update()
    {
        ActiveOperation?.Update();
    }
}

/// <summary>
/// Manages the active reload operation, synchronizing progress updates back to the main thread.
/// </summary>
public class ReloadOperation
{
    private readonly ProgressCallbacks _callbacks;
    private volatile Progress _lastProgress;

    public ReloadOperation(ProgressCallbacks callbacks)
    {
        _callbacks = callbacks;
    }

    internal void OnProgress(Progress progress)
    {
        _lastProgress = progress;
    }

    internal void Update()
    {
        if (_lastProgress != null) _callbacks?.OnProgressUpdate?.Invoke(_lastProgress);
    }
}

/// <summary>
/// Handle progress updates about the reload operation
/// </summary>
public class ProgressCallbacks
{
    public Action<Progress> OnProgressUpdate;
    public Action OnComplete;
    public Action<Exception> OnError;
}
