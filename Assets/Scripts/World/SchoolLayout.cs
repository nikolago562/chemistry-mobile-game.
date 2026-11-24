using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// Every classroom and hallway position in one place. Classroom points are local to the classroom root
    /// (the scene's tilemaps share that space); hallway points are local to the hallway room.
    /// The classroom is a compact 28 x 23 floor (it was 42 x 35) so a phone thumb crosses it in a few seconds.
    /// Change the room here: tiles, walls, camera, stations, props, NPC spots and checks all read it.
    /// </summary>
    public static class SchoolLayout
    {
        // ---------- classroom room ----------

        /// <summary>Walkable floor cells, inclusive. The back wall fills rows 11..14 above the floor; trim surrounds it all.</summary>
        public const int FloorMinX = 32, FloorMaxX = 59, FloorMinY = -12, FloorMaxY = 10, WallMinY = 11, WallMaxY = 14;
        public const float CentreX = 46f;
        /// <summary>Camera limits (classroom local): the trim plus the top of the wall.</summary>
        public static readonly Vector2 CameraMin = new Vector2(FloorMinX - 1, FloorMinY - 1), CameraMax = new Vector2(FloorMaxX + 2, 16);
        public static Vector2 RoomCentre => new Vector2(CentreX, (FloorMinY + WallMaxY) * 0.5f);

        /// <summary>
        /// Maps a point from the old 42 x 35 classroom into this one: x squeezes toward the middle, the floor squeezes
        /// toward the back wall (the back-wall strip above y 8 keeps its depth). For free-standing décor, glows and paths;
        /// anything that belongs to a table or wall item is placed from that item's anchor instead.
        /// </summary>
        public static Vector2 P(float x, float y) => new Vector2(CentreX + (x - 45.5f) * (28f / 42f), y >= 8 ? y : 8 + (y - 8) * (20f / 32f));
        public static Vector2 P(Vector2 old) => P(old.x, old.y);

        // ---------- back-wall row (left to right) ----------
        public static readonly Vector2 CollectionCabinet = new Vector2(33.4f, 9.3f);
        public static readonly Vector2 TeacherBench = new Vector2(37.2f, 9.2f);
        public static readonly Vector2[] TeacherSpots = { TeacherBench + new Vector2(-0.9f, -0.6f), TeacherBench + new Vector2(0.9f, -0.6f) };
        public static readonly Vector2 LessonBench = new Vector2(41.3f, 9.2f);
        /// <summary>The lesson table's use spot, in front of its bench.</summary>
        public static readonly Vector2 LessonTable = LessonBench + new Vector2(-0.2f, -0.5f);
        /// <summary>The personal-lab door: its trigger sits on the floor just under the wall.</summary>
        public static readonly Vector2 LabDoor = new Vector2(46.5f, 10.1f);
        public static readonly Vector2 Noticeboard = new Vector2(49.6f, 10.4f);
        public static readonly Vector2 SinkBench = new Vector2(53.4f, 9.2f);
        public static readonly Vector2 PeriodicChart = new Vector2(56.6f, 10.5f);
        public static readonly Vector2 Locker = new Vector2(58.8f, 9.2f);

        // ---------- back-wall art (x only; heights stay as drawn) ----------
        public static readonly float[] Windows = { 33.7f, 53.4f };
        public const float ChalkboardX = 37.4f, ClockX = 40.75f, VialShelfX = 40.75f, FlaskPictureX = 42.7f, MoleculePictureX = 44.3f,
            VentX = 43.5f, SafetyLightX = 49.6f, WallShelfX = 59.1f;
        /// <summary>Pendant lamps hang in the gaps between wall items.</summary>
        public static readonly float[] Pendants = { 35.5f, 47.85f, 55.05f };

        // ---------- the six tables, the advanced desk and the hallway door ----------
        public const float LeftColumn = 40.7f, RightColumn = 50.3f;
        public static readonly float[] Rows = { 4.25f, -0.75f, -5.75f };
        public static readonly Vector2 AtomBench = new Vector2(LeftColumn, Rows[0]), PotionBench = new Vector2(RightColumn, Rows[0]),
            MixBench = new Vector2(LeftColumn, Rows[1]), MoleculeBench = new Vector2(RightColumn, Rows[1]),
            SolutionBench = new Vector2(LeftColumn, Rows[2]), ElementBench = new Vector2(RightColumn, Rows[2]),
            QuizBench = new Vector2(LeftColumn, -10.1f);
        /// <summary>A table's use spot is one unit in front of its bench.</summary>
        public static readonly Vector2 Use = new Vector2(0, -1);
        public static readonly Vector2 MatSize = new Vector2(5.8f, 3.8f);
        public static readonly Vector2 HallwayDoor = new Vector2(46.5f, FloorMinY + 0.8f);
        public static readonly Vector2 PlayerStart = new Vector2(46.5f, 6.8f);

        // ---------- side lanes (furniture flush with the side walls) ----------
        public const float LeftWall = 33.1f, RightWall = 58.9f;
        public static readonly Vector2 FrontShelf = new Vector2(RightWall - 4.7f, FloorMinY + 0.7f);
        public static readonly Vector2 ShopShelf = new Vector2(58.6f, -3.25f), SupplyCabinet = new Vector2(58.6f, -2.6f);

        // ---------- hallway (local to the hallway room) ----------
        public const int HallHalfWidth = 11;
        public const float HallFront = -3.2f;
        /// <summary>Doors on the hallway's back wall, left to right.</summary>
        public static readonly (string to, float x)[] HallDoors =
            { ("classroom", -9), ("storage", -4.5f), ("reading", 0), ("greenhouse", 4.5f), ("exhibition", 9) };
        /// <summary>Maps an old hallway point (30 x 11) into the compact one (22 x 8).</summary>
        public static Vector2 Hall(float x, float y) => new Vector2(x * (HallHalfWidth / 15f), y);
    }
}
