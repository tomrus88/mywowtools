using System.Collections.Generic;
using WowTools.Core;

namespace WoWObjects
{
	public class SplineInfo
	{
		public SplineFlags Flags;
		public Coords3 Point;
		public ulong Guid;
		public float Rotation;
		public uint CurrentTime;
		public uint FullTime;
		public uint Unknown1;
		public float UnknownFloat1;
		public float UnknownFloat2;
		public float UnknownFloat3;
		public uint Unknown2;
		public uint Count;
		public List<Coords3> Splines = new List<Coords3>();
		public byte Unknown3;
		public Coords3 EndPoint;
	};
}