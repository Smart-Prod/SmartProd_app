using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using SmartProd_Mobile_Front_end.Services;

namespace SmartProd_Mobile_Front_end
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter(new[] { NfcAdapter.ActionNdefDiscovered, NfcAdapter.ActionTagDiscovered, NfcAdapter.ActionTechDiscovered }, Categories = new[] { Intent.CategoryDefault })]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleNfcIntent(Intent);
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            HandleNfcIntent(intent);
        }

        private void HandleNfcIntent(Intent? intent)
        {
            if (intent == null) return;

            var action = intent.Action;
            if (action == NfcAdapter.ActionNdefDiscovered ||
                action == NfcAdapter.ActionTagDiscovered ||
                action == NfcAdapter.ActionTechDiscovered)
            {
#pragma warning disable CA1422 // Validate platform compatibility
                var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
#pragma warning restore CA1422
                if (tag != null)
                {
                    var idBytes = tag.GetId();
                    var tagId = idBytes != null ? BitConverter.ToString(idBytes).Replace("-", "") : "NFC-" + DateTime.Now.Ticks;
                    var nfcService = IPlatformApplication.Current?.Services.GetService<INfcService>() as NfcService;
                    nfcService?.OnTagScanned(tagId);
                }
            }
        }
    }
}
