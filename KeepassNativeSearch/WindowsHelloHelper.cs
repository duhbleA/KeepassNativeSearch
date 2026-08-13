using Windows.Security.Credentials.UI;

namespace KeepassNativeSearch;

/**
 * <summary>Simple utility class to present an authentication to the user if Windows Hello is enabled.</summary>
 */
public static class WindowsHelloHelper
{
    /**
     * <summary>Attempt to authenticate with Windows Hello, if available, with a reason.</summary>
     *
     * <param name="reasonMessage">The reason for authenticating with the user.</param>
     */
    public static async Task<bool> Authenticate(string reasonMessage)
    {
        var availability = await UserConsentVerifier.CheckAvailabilityAsync();
        if (availability != UserConsentVerifierAvailability.Available &&
            availability != UserConsentVerifierAvailability.DeviceBusy)
        {
            return true;
        }

        var consentResult = await UserConsentVerifier.RequestVerificationAsync(reasonMessage);

        return consentResult == UserConsentVerificationResult.Verified;
    }
}