using System.Text;

namespace UnifiedAccount.Reporting.Extensions;

internal static class ReportingEncodingInitializer
{
    private static readonly object SyncRoot = new();
    private static bool _codePagesProviderRegistered;

    public static void EnsureRegistered()
    {
        if (_codePagesProviderRegistered)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (_codePagesProviderRegistered)
            {
                return;
            }

            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch (ArgumentException)
            {
                // 既に登録済みの場合はそのまま続行する。
            }

            _codePagesProviderRegistered = true;
        }
    }
}
