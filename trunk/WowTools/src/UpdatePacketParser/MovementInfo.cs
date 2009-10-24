using System.IO;
using UpdateFields;
using WowTools.Core;

namespace WoWObjects
{
	public class MovementInfo
	{
		// Fall time
		public float FacingAdjustement;
		public uint FallTime;
		public MovementFlags Flags;
		public ulong FullGuid;
		public ulong Guid0X100;
		public ulong Guid0X200;
		public uint HighGuid;

		// Jumping
		public float JumpCosAngle;
		public float JumpSinAngle;
		public float JumpUnk1;
		public float JumpXySpeed;

		// Spline

		// Other flags stuff
		public uint LowGuid;
		public Coords3 Pos0X100;
		public Coords4 Pos20X100;
		public Coords4 Position;
		public readonly float[] Speeds = new float[9];

		// Splines
		public readonly SplineInfo Spline = new SplineInfo();
		public float SwimPitch;
		public uint TimeStamp;
		public TransportInfo Transport;
		public uint TransportTime;
		public float UnkFloat0X100;
		public ushort Unknown1;
		public float Unknown2;
		public UpdateFlags UpdateFlags;
		public uint VehicleId;

		public void Read(BinaryReader gr)
		{
			UpdateFlags = (UpdateFlags) gr.ReadUInt16();

			// 0x20
			if ((UpdateFlags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
			{
				Flags = (MovementFlags) gr.ReadUInt32();
				Unknown1 = gr.ReadUInt16();
				TimeStamp = gr.ReadUInt32();

				Position = gr.ReadCoords4();

				if ((Flags & MovementFlags.MOVEMENTFLAG_ONTRANSPORT) != 0)
				{
					Transport.Guid = gr.ReadPackedGuid();
					Transport.Position = gr.ReadCoords4();
					Transport.Time = gr.ReadUInt32();
					Transport.Seat = gr.ReadByte();
				}

				if (((Flags & (MovementFlags.MOVEMENTFLAG_SWIMMING | MovementFlags.MOVEMENTFLAG_UNK5)) != 0) ||
				    ((Unknown1 & 0x20) != 0))
				{
					SwimPitch = gr.ReadSingle();
				}

				FallTime = gr.ReadUInt32();

				if ((Flags & MovementFlags.MOVEMENTFLAG_JUMPING) != 0)
				{
					JumpUnk1 = gr.ReadSingle();
					JumpSinAngle = gr.ReadSingle();
					JumpCosAngle = gr.ReadSingle();
					JumpXySpeed = gr.ReadSingle();
				}

				if ((Flags & MovementFlags.MOVEMENTFLAG_SPLINE) != 0)
				{
					Unknown2 = gr.ReadSingle();
				}

				for (byte i = 0; i < Speeds.Length; ++i)
					Speeds[i] = gr.ReadSingle();
				//float walk_speed = gr.ReadSingle();
				//float run_speed = gr.ReadSingle();
				//float swim_back = gr.ReadSingle();
				//float swin_speed = gr.ReadSingle();
				//float walk_back = gr.ReadSingle();
				//float fly_speed = gr.ReadSingle();
				//float fly_back = gr.ReadSingle();
				//float turn_speed = gr.ReadSingle();
				//float unk_speed = gr.ReadSingle();

				if ((Flags & MovementFlags.MOVEMENTFLAG_SPLINE2) != 0)
				{
					Spline.Flags = (SplineFlags) gr.ReadUInt32();

					if ((Spline.Flags & SplineFlags.POINT) != 0)
					{
						Spline.Point = gr.ReadCoords3();
					}

					if ((Spline.Flags & SplineFlags.TARGET) != 0)
					{
						Spline.Guid = gr.ReadUInt64();
					}

					if ((Spline.Flags & SplineFlags.ORIENT) != 0)
					{
						Spline.Rotation = gr.ReadSingle();
					}

					Spline.CurrentTime = gr.ReadUInt32();
					Spline.FullTime = gr.ReadUInt32();
					Spline.Unknown1 = gr.ReadUInt32();

					Spline.UnknownFloat1 = gr.ReadSingle();
					Spline.UnknownFloat2 = gr.ReadSingle();
					Spline.UnknownFloat3 = gr.ReadSingle();

					Spline.Unknown2 = gr.ReadUInt32();

					Spline.Count = gr.ReadUInt32();

					for (uint i = 0; i < Spline.Count; ++i)
					{
						Spline.Splines.Add(gr.ReadCoords3());
					}

					Spline.Unknown3 = gr.ReadByte(); // added in 3.0.8

					Spline.EndPoint = gr.ReadCoords3();
				}
			}
			else
			{
				if ((UpdateFlags & UpdateFlags.UPDATEFLAG_GO_POSITION) != 0)
				{
					// 0x100
					Guid0X100 = gr.ReadPackedGuid();
					Pos0X100 = gr.ReadCoords3();
					Pos20X100 = gr.ReadCoords4();
					UnkFloat0X100 = gr.ReadSingle();
				}
				else if ((UpdateFlags & UpdateFlags.UPDATEFLAG_HAS_POSITION) != 0)
				{
					// 0x40
					Position = gr.ReadCoords4();
				}
			}

			if ((UpdateFlags & UpdateFlags.UPDATEFLAG_LOWGUID) != 0)
			{
				LowGuid = gr.ReadUInt32();
			}

			if ((UpdateFlags & UpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
			{
				HighGuid = gr.ReadUInt32();
			}

			if ((UpdateFlags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != 0)
			{
				FullGuid = gr.ReadPackedGuid();
			}

			if ((UpdateFlags & UpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
			{
				TransportTime = gr.ReadUInt32();
			}

			// WotLK
			if ((UpdateFlags & UpdateFlags.UPDATEFLAG_VEHICLE) != 0)
			{
				VehicleId = gr.ReadUInt32();
				FacingAdjustement = gr.ReadSingle();
			}

			// 3.1
			if ((UpdateFlags & UpdateFlags.UPDATEFLAG_GO_ROTATION) != 0)
			{
				Guid0X200 = gr.ReadUInt64();
			}
		}
	} 
}