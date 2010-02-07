using System.IO;

namespace WowTools.Core
{
    public class MovementInfo
    {
        private readonly float[] speeds = new float[9];
        public float FacingAdjustement { get; private set; }
        public uint FallTime { get; private set; }
        public MovementFlags Flags { get; private set; }

        public ulong FullGuid { get; private set; }

        public ulong Guid0X100 { get; private set; }
        public ulong Guid0X200 { get; private set; }
        public uint HighGuid { get; private set; }

        public float JumpCosAngle { get; private set; }
        public float JumpSinAngle { get; private set; }
        public float JumpUnk1 { get; private set; }
        public float JumpXySpeed { get; private set; }

        public uint LowGuid { get; private set; }
        public Coords3 Pos0X100 { get; private set; }
        public Coords4 Pos20X100 { get; private set; }
        public Coords4 Position { get; private set; }

        public SplineInfo Spline { get; private set; }
        public float SwimPitch { get; private set; }
        public uint TimeStamp { get; private set; }
        public TransportInfo Transport { get; private set; }
        public uint TransportTime { get; private set; }
        public float UnkFloat0X100 { get; private set; }
        public MovementFlags2 Flags2 { get; private set; }
        public float SplineElevation { get; private set; }
        public UpdateFlags UpdateFlags { get; private set; }
        public uint VehicleId { get; private set; }

        public float[] Speeds
        {
            get { return speeds; }
        }

        public static MovementInfo Read(BinaryReader gr)
        {
            var movement = new MovementInfo();

            movement.UpdateFlags = (UpdateFlags)gr.ReadUInt16();

            // 0x20
            if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                movement.Flags = (MovementFlags)gr.ReadUInt32();
                movement.Flags2 = (MovementFlags2)gr.ReadUInt16();
                movement.TimeStamp = gr.ReadUInt32();

                movement.Position = gr.ReadCoords4();

                if ((movement.Flags & MovementFlags.ONTRANSPORT) != 0)
                {
                    movement.Transport = TransportInfo.Read(gr);
                }

                if (((movement.Flags & (MovementFlags.SWIMMING | MovementFlags.FLYING)) != 0) ||
                    ((movement.Flags2 & MovementFlags2.AlwaysAllowPitching) != 0))
                {
                    movement.SwimPitch = gr.ReadSingle();
                }

                movement.FallTime = gr.ReadUInt32();

                if ((movement.Flags & MovementFlags.FALLING) != 0)
                {
                    movement.JumpUnk1 = gr.ReadSingle();
                    movement.JumpSinAngle = gr.ReadSingle();
                    movement.JumpCosAngle = gr.ReadSingle();
                    movement.JumpXySpeed = gr.ReadSingle();
                }

                if ((movement.Flags & MovementFlags.SPLINEELEVATION) != 0)
                {
                    movement.SplineElevation = gr.ReadSingle();
                }

                for (byte i = 0; i < movement.speeds.Length; ++i)
                    movement.speeds[i] = gr.ReadSingle();

                if ((movement.Flags & MovementFlags.SPLINEENABLED) != 0)
                {
                    movement.Spline = SplineInfo.Read(gr);
                }
            }
            else
            {
                if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_GO_POSITION) != 0)
                {
                    // 0x100
                    movement.Guid0X100 = gr.ReadPackedGuid();
                    movement.Pos0X100 = gr.ReadCoords3();
                    movement.Pos20X100 = gr.ReadCoords4();
                    movement.UnkFloat0X100 = gr.ReadSingle();
                }
                else if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_HAS_POSITION) != 0)
                {
                    // 0x40
                    movement.Position = gr.ReadCoords4();
                }
            }

            if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_LOWGUID) != 0)
            {
                movement.LowGuid = gr.ReadUInt32();
            }

            if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
            {
                movement.HighGuid = gr.ReadUInt32();
            }

            if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_TARGET_GUID) != 0)
            {
                movement.FullGuid = gr.ReadPackedGuid();
            }

            if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                movement.TransportTime = gr.ReadUInt32();
            }

            // WotLK
            if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_VEHICLE) != 0)
            {
                movement.VehicleId = gr.ReadUInt32();
                movement.FacingAdjustement = gr.ReadSingle();
            }

            // 3.1
            if ((movement.UpdateFlags & UpdateFlags.UPDATEFLAG_GO_ROTATION) != 0)
            {
                movement.Guid0X200 = gr.ReadUInt64();
            }
            return movement;
        }
    }
}
