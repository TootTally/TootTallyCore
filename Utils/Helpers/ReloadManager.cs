using System;
using BaboonAPI.Hooks;
using BaboonAPI.Hooks.Tracks;
using JetBrains.Annotations;

namespace TootTallyCore.Utils.Helpers;

public class ReloadManager
{
    private readonly Plugin Plugin;
    public bool IsCurrentlyReloading { get; private set; }

    public ReloadManager(Plugin plugin)
    {
        Plugin = plugin;
    }

    public void ReloadAll([CanBeNull] IProgressCallbacks callbacks)
    {
        if (IsCurrentlyReloading) return;

        IsCurrentlyReloading = true;
        Plugin.StartCoroutine(TrackReloader.ReloadAll(progress => callbacks?.OnProgressUpdate(progress)).ForEach(result =>
        {
            IsCurrentlyReloading = false;
            if (result.IsError)
            {
                callbacks?.OnError(result.ErrorValue);
            }
            else
            {
                callbacks?.OnComplete();
            }
        }));
    }
}

public interface IProgressCallbacks
{
    void OnProgressUpdate(Progress progress);
    void OnComplete();
    void OnError(Exception err);
}
