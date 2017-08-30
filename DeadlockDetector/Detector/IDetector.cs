namespace DeadlockDetector.Detector
{
    public interface IDetector
    {
        string Name { get; }

        bool Detect();
    }
}