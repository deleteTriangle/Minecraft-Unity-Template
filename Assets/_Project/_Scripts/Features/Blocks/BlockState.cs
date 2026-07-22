public class BlockState
{
    public BlockType type;
    public byte direction;
    public byte state;
    
    public BlockState(BlockType type, byte direction = 0, byte state = 0)
    {
        this.type = type;
        this.direction = direction;
        this.state = state;
    }
}

public enum TrapdoorState
{
    Closed = 0,
    Open = 1
}

public enum DoorState
{
    Closed = 0,
    Open = 1
}