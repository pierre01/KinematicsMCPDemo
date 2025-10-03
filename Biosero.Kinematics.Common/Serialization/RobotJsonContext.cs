using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Biosero.Kinematics.Common.Serialization;

[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(List<RobotCoordinate>))]
[JsonSerializable(typeof(RobotCoordinate[]))]
[JsonSerializable(typeof(RobotCoordinate))]
public partial class RobotJsonContext : JsonSerializerContext 
{
}
