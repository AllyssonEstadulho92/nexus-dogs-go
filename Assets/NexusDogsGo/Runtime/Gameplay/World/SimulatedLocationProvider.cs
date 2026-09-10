using System.Threading;
using System.Threading.Tasks;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class SimulatedLocationProvider : ILocationProvider
    {
        private GeoCoordinate _coordinate;

        public bool IsRunning { get; private set; }

        public SimulatedLocationProvider()
            : this(new GeoCoordinate(38.7369d, -9.1427d))
        {
        }

        public SimulatedLocationProvider(GeoCoordinate initialCoordinate)
        {
            _coordinate = initialCoordinate;
        }

        public Task<bool> StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsRunning = true;
            return Task.FromResult(true);
        }

        public bool TryGetLocation(out GeoCoordinate coordinate)
        {
            coordinate = _coordinate;
            return IsRunning;
        }

        public void SetLocation(GeoCoordinate coordinate)
        {
            _coordinate = coordinate;
        }

        public void MoveMeters(double northMeters, double eastMeters)
        {
            _coordinate = GeoMath.OffsetMeters(_coordinate, northMeters, eastMeters);
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
