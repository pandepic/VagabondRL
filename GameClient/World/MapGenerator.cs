using ElementEngine;
using ElementEngine.ECS;
using SharpNeat.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public enum RoomType
    {
        Bedroom,
        Kitchen,
        Entry,
    }

    public class RoomTypeSettings
    {
        public RoomType RoomType;
        public Vector2I MinSize, MaxSize;
        public int MinCount, MaxCount;
    }

    public class Room
    {
        public RoomType RoomType;
        public Vector2I Position;
        public Vector2I Size;

        public Rectangle Rect => new Rectangle(Position, Size);
        public Rectangle PaddedRect => new Rectangle(Position - MapGenerator.RoomPadding / 2, Size + MapGenerator.RoomPadding);
    }

    public class MapGenerator
    {
        public Dictionary<RoomType, RoomTypeSettings> RoomTypes = new Dictionary<RoomType, RoomTypeSettings>()
        {
            {
                RoomType.Entry, new RoomTypeSettings()
                {
                    RoomType = RoomType.Entry,
                    MinCount = 1,
                    MaxCount = 1,
                    MinSize = new Vector2I(10),
                    MaxSize = new Vector2I(15),
                }
            },

            {
                RoomType.Bedroom, new RoomTypeSettings()
                {
                    RoomType = RoomType.Bedroom,
                    MinCount = 5,
                    MaxCount = 20,
                    MinSize = new Vector2I(5),
                    MaxSize = new Vector2I(10),
                }
            },

            {
                RoomType.Kitchen, new RoomTypeSettings()
                {
                    RoomType = RoomType.Kitchen,
                    MinCount = 2,
                    MaxCount = 4,
                    MinSize = new Vector2I(5),
                    MaxSize = new Vector2I(10),
                }
            },
        };

        public static readonly Vector2I TileSize = new Vector2I(16);
        public static readonly Vector2I RoomPadding = new Vector2I(2);

        private FastRandom _rng = new FastRandom();
        public Entity Tilemap;

        public MapGenerator(Entity tilemap)
        {
            Tilemap = tilemap;
        }

        public void GenerateMap()
        {
            ref var tilemapComponent = ref Tilemap.GetComponent<TilemapComponent>();

            var biggestRoomWidth = 0;
            var biggestRoomHeight = 0;
            var mapSize = new Vector2I();

            var rooms = new List<Room>();
            Room entry = null;

            // generate a list of rooms from the rooms config settings
            foreach (var (type, settings) in RoomTypes)
            {
                var count = _rng.Next(settings.MinCount, settings.MaxCount + 1);

                for (var i = 0; i < count; i++)
                {
                    var newRoom = new Room()
                    {
                        RoomType = type,
                        Size = new Vector2I(
                            _rng.Next(settings.MinSize.X, settings.MaxSize.X + 1),
                            _rng.Next(settings.MinSize.Y, settings.MaxSize.Y + 1)),
                    };

                    if (type == RoomType.Entry)
                        entry = newRoom;
                    else
                        rooms.Add(newRoom);

                    // find biggest padded width and height from all rooms to use for grid cells later for room placement
                    if (newRoom.PaddedRect.Width > biggestRoomWidth)
                        biggestRoomWidth = newRoom.PaddedRect.Width;
                    if (newRoom.PaddedRect.Height > biggestRoomHeight)
                        biggestRoomHeight = newRoom.PaddedRect.Height;
                }
            }

            // shuffle rooms to have them placed randomly
            rooms.Shuffle(_rng);
            // always place entry last so it'll be accessible from outside
            rooms.Add(entry);
            
            // calculate width of room cells grid (grid is biased to being wider than tall)
            var widthRooms = (int)Math.Sqrt(rooms.Count) + 2;
            var heightRooms = 0;

            // calculate height of room cells grid
            var counter = rooms.Count;
            while (counter > 0)
            {
                counter -= widthRooms;
                heightRooms += 1;
            }

            // sort rooms into a grid of cells
            for (var y = 0; y < heightRooms; y++)
            {
                for (var x = 0; x < widthRooms; x++)
                {
                    var index = x + widthRooms * y;

                    if (index >= rooms.Count)
                        break;

                    var room = rooms[index];
                    room.Position = new Vector2I(
                        x * biggestRoomWidth + ((biggestRoomWidth - room.Size.X) / 2),
                        y * biggestRoomHeight + ((biggestRoomHeight - room.Size.Y) / 2));
                }
            }

            // compress rooms to within the hallway (padded) space from each other
            foreach (var room in rooms)
            {
                var collidedX = false;

                while (!collidedX)
                {
                    var prevPosition = room.Position;
                    room.Position.X -= 1;

                    foreach (var checkRoom in rooms)
                    {
                        if (checkRoom == room)
                            continue;

                        if (checkRoom.PaddedRect.Intersects(room.PaddedRect))
                            collidedX = true;
                    }

                    if (room.Position.X < 0)
                        collidedX = true;

                    if (collidedX)
                        room.Position = prevPosition;
                }

                var collidedY = false;

                while (!collidedY)
                {
                    var prevPosition = room.Position;
                    room.Position.Y -= 1;

                    foreach (var checkRoom in rooms)
                    {
                        if (checkRoom == room)
                            continue;

                        if (checkRoom.PaddedRect.Intersects(room.PaddedRect))
                            collidedY = true;
                    }

                    if (room.Position.Y < 0)
                        collidedY = true;

                    if (collidedY)
                        room.Position = prevPosition;
                }
            }

            // find size of map
            foreach (var room in rooms)
            {
                if (room.Rect.Right > mapSize.X)
                    mapSize.X = room.Rect.Right;
                if (room.Rect.Bottom > mapSize.Y)
                    mapSize.Y = room.Rect.Bottom;
            }

            tilemapComponent.Width = mapSize.X;
            tilemapComponent.Height = mapSize.Y;
            tilemapComponent.Collisions = new CollisionType[mapSize.X * mapSize.Y];
            tilemapComponent.Layers = new TimemapLayer[1];
            tilemapComponent.Layers[0].Tiles = new int[mapSize.X * mapSize.Y];

            // fill in tiles in tilemap
            foreach (var room in rooms)
            {
                for (var y = room.Position.Y; y < room.Rect.Bottom; y++)
                {
                    for (var x = room.Position.X; x < room.Rect.Right; x++)
                    {
                        var index = x + mapSize.X * y;
                        tilemapComponent.Layers[0].Tiles[index] = 1;
                    }
                }
            }

        } // GenerateMap
    }
}
