using System;

#if ANDROID
using Android.App;
using Android.Nfc;
#endif

namespace SmartProd_Mobile_Front_end.Services
{
    public class NfcService : INfcService
    {
        public event EventHandler<string>? TagDetected;

        public bool IsSupported
        {
            get
            {
#if ANDROID
                try
                {
                    var adapter = NfcAdapter.GetDefaultAdapter(Android.App.Application.Context);
                    return adapter != null;
                }
                catch
                {
                    return false;
                }
#else
                return false;
#endif
            }
        }

        public bool IsEnabled
        {
            get
            {
#if ANDROID
                try
                {
                    var adapter = NfcAdapter.GetDefaultAdapter(Android.App.Application.Context);
                    return adapter != null && adapter.IsEnabled;
                }
                catch
                {
                    return false;
                }
#else
                return false;
#endif
            }
        }

        public void StartListening()
        {
            // O escaneamento contínuo em Android é gerenciado via Foreground Dispatch / IntentFilter no MainActivity
        }

        public void StopListening()
        {
        }

        public void OnTagScanned(string tagId)
        {
            if (!string.IsNullOrWhiteSpace(tagId))
            {
                TagDetected?.Invoke(this, tagId);
            }
        }

        public void SimulateTagRead(string tagId)
        {
            TagDetected?.Invoke(this, tagId);
        }
    }
}
