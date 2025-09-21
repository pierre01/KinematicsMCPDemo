namespace Biosero.TeachPendant.Common
{
    public class MoveBuffer
    {
        private RobotCoordinate _bufferedCoordinate;

        private MoveDirection _bufferMoveDirection;

        public void InitializeNewBufferCoordinate()
            => _bufferedCoordinate = new RobotCoordinate();

        public RobotCoordinate GetCoordinate()
            => _bufferedCoordinate;

        public void SetBufferedMoveDirection(MoveDirection direction)
            => _bufferMoveDirection = direction;

        public void MoveInBufferedDirection(double stepScale)
            => MoveInBufferedDirection(_bufferMoveDirection, stepScale);

        public void MoveInBufferedDirection(MoveDirection direction, double stepScale)
            => _bufferedCoordinate += MoveIncrements.GetIncrement(direction) * stepScale;
    }
}
