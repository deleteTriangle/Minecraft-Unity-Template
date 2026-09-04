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

public static class BlockStateFlags
{
    public const byte Open = 1;
    public const byte Upper = 2;

    public static bool IsOpen(BlockState blockState) => (blockState.state & Open) != 0;

    public static bool IsUpper(BlockState blockState) => (blockState.state & Upper) != 0;

    public static byte WithOpen(BlockState blockState, bool isOpen)
    {
        return isOpen
            ? (byte)(blockState.state | Open)
            : (byte)(blockState.state & ~Open);
    }
}
