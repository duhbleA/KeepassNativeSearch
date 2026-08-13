using Microsoft.Win32;

namespace KeepassNativeSearch;

/**
 * <summary>
 * Monitors session and power mode changes in Windows
 * </summary>
 *
 * <param name="settings">System settings to determine if actions should be taken by this monitor.</param>
 * <param name="action">The action taken based on the monitored system changes.</param>
 */
public class Win32StateMonitor(Settings settings, Action action)
{
    /**
     * <summary>Subscribe to system events.</summary>
     */
    public void Start()
    {
        SystemEvents.SessionSwitch += OnSessionChange;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    /**
     * <summary>Closes subscriptions to system events.</summary>
     */
    public void Close()
    {
        SystemEvents.SessionSwitch -= OnSessionChange;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }
    
    private void OnSessionChange(object sender, SessionSwitchEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLogoff:
                if (!settings.CloseDbUserSession) return;
                action();
                break;
            case SessionSwitchReason.SessionLock:
                if (!settings.CloseDbLockScreen) return;
                action();
                break;
            case SessionSwitchReason.ConsoleConnect:
            case SessionSwitchReason.ConsoleDisconnect:
            case SessionSwitchReason.RemoteConnect:
            case SessionSwitchReason.RemoteDisconnect:
            case SessionSwitchReason.SessionLogon:
            case SessionSwitchReason.SessionUnlock:
            case SessionSwitchReason.SessionRemoteControl:
            default:
                break;
        }
    }
    
    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                if (!settings.CloseDbComputerSleep) return;
                action();
                break;
            case PowerModes.Resume:
            case PowerModes.StatusChange:
            default:
                break;
        }
    }
}