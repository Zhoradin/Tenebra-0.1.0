using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class RoomConnection
{
    public Room RoomA { get; private set; }
    public Room RoomB { get; private set; }

    public string connectionName;

    public RoomConnection(Room roomA, Room roomB, string name)
    {
        RoomA = roomA;
        RoomB = roomB;
        this.connectionName = name;
    }
}

[Serializable]
public class Room
{
    public int X { get; set; }
    public int Y { get; set; }
    public RoomType RoomType { get; set; }
    public int Id { get; private set; }
    public string Name;

    public Room(int x, int y, int id, string name)
    {
        X = x;
        Y = y;
        this.Name = name;
        RoomType = RoomType.None;
        Id = id;
    }
}


public class MapGenerator : MonoBehaviour
{
    public GameObject roomButtonPrefab;
    public RectTransform contentTransform;

    public GameObject lineSegmentPrefab;
    [SerializeField] private int width = 7;
    [SerializeField] private int height = 16;
    [SerializeField] private int minPaths = 3;
    [SerializeField] private int maxPaths = 4;

    [SerializeField] private float verticalOffset = 100f;
    [SerializeField] private float horizontalOffset = 100f;

    private RoomManager roomManager;
    private Room[,] grid;
    private System.Random random = new System.Random();
    private int extendedHeight;

    public List<Room> remainingRooms = new List<Room>();
    public List<RoomConnection> remainingConnections = new List<RoomConnection>();

    private void Start()
    {
        roomManager = new RoomManager();
        extendedHeight = height + 1;
        GenerateMap();
        if(remainingRooms.Count == 0){
            AssignRoomLocations();
        }
        else{
            AssignRoomLocations();
        }
    }
    private void GenerateMap()
    {
        grid = new Room[width, extendedHeight];

        if (remainingRooms.Count == 0)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < extendedHeight; y++)
                {
                    string name = $"Room_{x}_{y}";
                    var room = new Room(x, y, x + y * width, name);
                    grid[x, y] = room;
                    roomManager.AddRoom(room);
                }
            }

            int pathCount = random.Next(minPaths, maxPaths + 1);
            List<Room> startingRooms = new List<Room>();
            for (int i = 0; i < pathCount; i++)
            {
                Room startRoom;
                do
                {
                    startRoom = grid[random.Next(width), 0];
                } while (startingRooms.Contains(startRoom));
                startingRooms.Add(startRoom);
            }

            foreach (Room startRoom in startingRooms)
            {
                ConnectToNextFloor(startRoom, 0);
            }

            AssignRoomLocations();
        }
        else
        {
            AssignRemainingRoomLocations();
        }

        RemoveUnconnectedRooms();

        AllocateBossRoom();

        GenerateRoomButtons();
    }

    private void ConnectToNextFloor(Room room, int currentFloor)
    {
        if (currentFloor >= extendedHeight - 1)
            return;

        int nextFloor = currentFloor + 1;

        if (nextFloor == extendedHeight - 1)
        {
            Room bossRoom = grid[width / 2, nextFloor];
            roomManager.AddConnection(room, bossRoom);
            return;
        }

        List<Room> possibleConnections = new List<Room>();
        for (int dx = -1; dx <= 1; dx++)
        {
            int nx = room.X + dx;
            if (nx >= 0 && nx < width)
            {
                possibleConnections.Add(grid[nx, nextFloor]);
            }
        }

        Room nextRoom = possibleConnections[random.Next(possibleConnections.Count)];
        roomManager.AddConnection(room, nextRoom);
        ConnectToNextFloor(nextRoom, nextFloor);
    }

    private void AssignRemainingRoomLocations()
    {
        foreach (Room room in remainingRooms)
        {
            string[] nameParts = room.Name.Split('_');
            if (nameParts.Length == 3 && int.TryParse(nameParts[1], out int x) && int.TryParse(nameParts[2], out int y))
            {
                room.X = x;
                room.Y = y;

                if (room.X >= 0 && room.X < width && room.Y >= 0 && room.Y < extendedHeight)
                {
                    grid[room.X, room.Y] = room;
                    roomManager.AddRoom(room);

                    if (room.RoomType == RoomType.None)
                    {
                        room.RoomType = GetRandomRoomType(room.Y);
                    }
                }
            }
            else
            {
                Debug.LogError($"Invalid room name format: {room.Name}");
            }
        }

        foreach (Room room in remainingRooms)
        {
            foreach (Room otherRoom in remainingRooms)
            {
                if (room != otherRoom)
                {
                    roomManager.AddConnection(room, otherRoom);
                }
            }
        }
    }

    private void AssignRoomLocations()
    {
        foreach (Room room in GetRoomsOnFloor(0)) 
        {
            room.RoomType = RoomType.Monster; 
        }
        
        foreach (Room room in GetRoomsOnFloor(8)) 
        { 
            room.RoomType = RoomType.Treasure; 
        }
        
        foreach (Room room in GetRoomsOnFloor(15)) 
        { 
            room.RoomType = RoomType.RestSite; 
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Room room = grid[x, y];

                if (room != null)
                {
                    if (room.RoomType == RoomType.None)
                    {
                        room.RoomType = GetRandomRoomType(y);
                    }
                    Debug.Log($"Room assigned: {room.Name}, Position: ({room.X}, {room.Y}), Type: {room.RoomType}");
                }
            }
        }
    }


    private RoomType GetRandomRoomType(int floor)
    {
        int rand = random.Next(100);
        if (rand < 45) return RoomType.Monster;
        if (rand < 67) return RoomType.Event;
        if (rand < 77 && floor >= 5) return RoomType.EliteMonster;
        if (rand < 89 && floor >= 5) return RoomType.RestSite;
        if (rand < 94) return RoomType.Merchant;
        return RoomType.Treasure;
    }

    private void AllocateBossRoom()
    {
        int bossRoomX = width / 2;
        int bossRoomY = extendedHeight - 1;

        string name = $"Room_{bossRoomX}_{bossRoomY}";
        Room bossRoom = new Room(bossRoomX, bossRoomY, bossRoomX + bossRoomY * width, name)
        {
            RoomType = RoomType.Boss
        };
        grid[bossRoomX, bossRoomY] = bossRoom;

        roomManager.AddRoom(bossRoom); // Boss odasını yöneticisine ekle

        foreach (Room room in GetRoomsOnFloor(extendedHeight - 2))
        {
            if (room != null)
            {
                roomManager.AddConnection(room, bossRoom);
            }
        }
    }

    private List<Room> GetRoomsOnFloor(int floor)
    {
        List<Room> rooms = new List<Room>();
        for (int x = 0; x < width; x++)
        {
            if (grid[x, floor] != null)
            {
                rooms.Add(grid[x, floor]);
            }
        }
        return rooms;
    }

    private void RemoveUnconnectedRooms()
    {
        for (int y = 0; y < extendedHeight; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Room room = grid[x, y];
                if (room != null)
                {
                    if (y == extendedHeight - 2)
                    {
                        bool hasConnectionToLowerFloor = false;
                        bool hasConnectionToBossRoom = false;

                        foreach (RoomConnection connection in roomManager.GetConnections())
                        {
                            if (connection.RoomA == room && connection.RoomB.Y == extendedHeight - 3)
                            {
                                hasConnectionToLowerFloor = true;
                            }
                            if (connection.RoomB == room && connection.RoomA.Y == extendedHeight - 3)
                            {
                                hasConnectionToLowerFloor = true;
                            }
                            if (connection.RoomA == room && connection.RoomB.Y == extendedHeight - 1)
                            {
                                hasConnectionToBossRoom = true;
                            }
                            if (connection.RoomB == room && connection.RoomA.Y == extendedHeight - 1)
                            {
                                hasConnectionToBossRoom = true;
                            }
                        }

                        if (!hasConnectionToLowerFloor || !hasConnectionToBossRoom)
                        {
                            grid[x, y] = null;
                        }
                        else
                        {
                            remainingRooms.Add(room);
                        }
                    }
                    else if (roomManager.GetConnections().FindAll(c => c.RoomA == room || c.RoomB == room).Count == 0)
                    {
                        grid[x, y] = null;
                    }
                    else
                    {
                        remainingRooms.Add(room);
                    }
                }
            }
        }

        Debug.Log($"Kalan odalar: {remainingRooms.Count}");

        foreach (var room in remainingRooms)
        {
            Debug.Log($"Oda Konum: ({room.X}, {room.Y}), Tür: {room.RoomType}");
        }
    }

    private void GenerateRoomButtons()
    {
        float mapWidth = width * horizontalOffset;
        float mapHeight = height * verticalOffset;

        float contentWidth = contentTransform.rect.width;
        float contentHeight = contentTransform.rect.height;

        float horizontalCenterOffset = (contentWidth - mapWidth) / 2;
        float verticalCenterOffset = (contentHeight - mapHeight) / 2;

        foreach (Room room in grid)
        {
            if (room != null && room.RoomType != RoomType.None)
            {
                GameObject roomButton = Instantiate(roomButtonPrefab, contentTransform);

                RectTransform buttonRect = roomButton.GetComponent<RectTransform>();
                buttonRect.anchoredPosition = new Vector2(
                    room.X * horizontalOffset + horizontalCenterOffset,
                    room.Y * verticalOffset + verticalCenterOffset
                );

                Image buttonImage = roomButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = GetColorForRoomType(room.RoomType);
                }

                roomButton.name = $"Room {room.X}, Floor {room.Y}, Type {room.RoomType}";

                RoomInteraction roomInteraction = roomButton.GetComponent<RoomInteraction>();
                if (roomInteraction != null)
                {
                    roomInteraction.InitializeRoom(room);
                    roomInteraction.button.interactable = room.Y == 0;

                    Button button = roomButton.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => OnRoomClicked(roomInteraction));
                    }
                }

                CreateConnections(room, buttonRect);
            }
        }
    }

    private void CreateConnections(Room room, RectTransform roomButtonRect)
    {
        foreach (RoomConnection connection in roomManager.GetConnections())
        {
            if (connection.RoomA == room || connection.RoomB == room)
            {
                Room connectedRoom = connection.RoomA == room ? connection.RoomB : connection.RoomA;
                if (connectedRoom != null)
                {
                    RoomInteraction connectedRoomInteraction = GetRoomInteraction(connectedRoom);

                    if (connectedRoomInteraction != null)
                    {
                        RectTransform connectedRoomRect = connectedRoomInteraction.GetComponent<RectTransform>();

                        GameObject lineSegment = Instantiate(lineSegmentPrefab, contentTransform);
                        RectTransform lineRect = lineSegment.GetComponent<RectTransform>();

                        lineSegment.transform.SetSiblingIndex(0);

                        Vector2 direction = connectedRoomRect.anchoredPosition - roomButtonRect.anchoredPosition;
                        float distance = direction.magnitude;

                        lineRect.anchoredPosition = roomButtonRect.anchoredPosition + direction / 2;

                        lineRect.sizeDelta = new Vector2(distance, 2f);

                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        lineRect.rotation = Quaternion.Euler(0, 0, angle);

                        Image lineImage = lineSegment.GetComponent<Image>();
                        if (lineImage != null)
                        {
                            lineImage.color = Color.gray;
                        }

                        // RoomConnection nesnesini remainingConnections listesine ekle
                        remainingConnections.Add(connection);
                    }
                }
            }
        }
    }

    private Color GetColorForRoomType(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Monster: return Color.red;
            case RoomType.Event: return Color.green;
            case RoomType.EliteMonster: return Color.magenta;
            case RoomType.RestSite: return Color.blue;
            case RoomType.Merchant: return Color.yellow;
            case RoomType.Treasure: return Color.cyan;
            case RoomType.Boss: return Color.black;
            default: return Color.white;
        }
    }

    private Button currentRoomButton;

    public static MapGenerator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void OnRoomClicked(RoomInteraction clickedRoom)
    {
        Debug.Log("OnRoomClicked method called.");

        SetCurrentRoom(clickedRoom);
        SetAllRoomsUnclickable();

        List<Room> connectedRooms = roomManager.GetConnectedRooms(clickedRoom.Room);

        foreach (var connectedRoom in connectedRooms)
        {
            if (connectedRoom.Y > clickedRoom.Room.Y)  // Ensure movement is only upwards
            {
                RoomInteraction nextRoom = GetRoomInteraction(connectedRoom);
                if (nextRoom != null)
                {
                    nextRoom.SetClickable(true);
                    nextRoom.UpdateClickableVisuals(); // Update visuals for clickable rooms
                }
            }
        }

        clickedRoom.UpdateClickableVisuals();
    }

    private RoomInteraction GetRoomInteraction(Room room)
    {
        foreach (var interaction in FindObjectsOfType<RoomInteraction>())
        {
            if (interaction.Room == room)
            {
                return interaction;
            }
        }
        return null;
    }

    private void SetCurrentRoom(RoomInteraction clickedRoom)
    {
        if (currentRoomButton != null)
        {
            currentRoomButton.interactable = false;  // Disable interaction for the previous room button
        }

        currentRoomButton = clickedRoom.GetComponent<Button>();
        currentRoomButton.interactable = true;  // Enable interaction for the current room button
        currentRoomButton.GetComponent<RoomInteraction>().BlinkSprite(); // Trigger any visual effects if needed

        Debug.Log("Current room set to: " + clickedRoom.Room.X + ", " + clickedRoom.Room.Y);
    }

    private void SetAllRoomsUnclickable()
    {
        Button[] allRooms = FindObjectsOfType<Button>();
        foreach (var roomButton in allRooms)
        {
            roomButton.interactable = false;  // Disable interaction for all room buttons
        }
        Debug.Log("All rooms set to unclickable.");
    }
}

public enum RoomType
{
    None,
    Monster,
    Event,
    EliteMonster,
    RestSite,
    Merchant,
    Treasure,
    Boss
}
