using Windows.Security.Credentials.UI;

namespace KeepassNativeSearch;

public static class WindowsHelloHelper
{
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