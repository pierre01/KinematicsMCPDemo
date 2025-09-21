namespace Biosero.TeachPendant.Common.Communicators
{
    public class KinematicsDemoCommunicator(string url)
    {
        // TODO: Update when web service endpoints are finalized. These are just placeholders.
        private const string GoHomeCommandEndpoint = "/GoHome";
        private const string StopPlayCommandEndpoint = "/StopPlay";

        private readonly WebApiCommunicator _webApiHelper = new(url);

        /// <summary>
        /// Get the position of the robot on the rail
        /// </summary>
        /// <returns></returns>
        public double GetRailPosition()
            => _webApiHelper.GetDoubleFromJson(TeachPendantWebApiResources.RailPosition);

        /// <summary>
        /// Get the coordinates of the robot
        /// </summary>
        /// <returns></returns>
        public RobotCoordinate GetCoordinates()
            => _webApiHelper.GetJsonAsObject<RobotCoordinate>(TeachPendantWebApiResources.Coordinates);

        /// <summary>
        /// Get the step precision of the robot
        /// </summary>
        /// <returns>The current step precision returned by the web service.</returns>
        public double GetStepPrecision()
            => _webApiHelper.GetDoubleFromJson(TeachPendantWebApiResources.StepPrecision);

        /// <summary>
        /// Send record the current point
        /// </summary>
        public void RecordPoint()
        {
            var body = "";
            PostRequest<string>(TeachPendantWebApiResources.RecordPoint, body);
        }

        public void Move(RobotCoordinate robotCoordinate)
            => PostWithObjectBody(TeachPendantWebApiResources.Move, robotCoordinate);

        /// <summary>
        /// Move effector to the home position
        /// </summary>
        public void GoHome()
        {
            var body = "";
            PostRequest<string>(GoHomeCommandEndpoint, body);
        }

        /// <summary>
        /// Ask the Robot to play the recorded points
        /// </summary>
        public void Play()
        {
            var body = "";
            PostRequest<string>(TeachPendantWebApiResources.Play, body);
        }

        /// <summary>
        /// Stop the robot from playing the recorded points
        /// </summary>
        public void StopPlay()
        {
            var body = "";
            PostRequest<string>(StopPlayCommandEndpoint, body);
        }

        private string PostWithObjectBody(string endpoint, object body)
            => _webApiHelper.PostWithObjectBody(endpoint, body);

        private oT PostRequest<oT>(string endpoint, object body)
            => _webApiHelper.PostAndParseObjectBody<oT>(endpoint, body);
    }
}
