namespace SmartProd_Mobile_Front_end.Services
{
    public interface INfcService
    {
        bool IsSupported { get; }
        bool IsEnabled { get; }
        event EventHandler<string>? TagDetected;
        void StartListening();
        void StopListening();
        void SimulateTagRead(string tagId);
    }
}
