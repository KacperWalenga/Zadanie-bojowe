using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using ZB.Maze;

public class Movement : MonoBehaviour
{
    private const int DirectionCount = 4;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform gridOrigin;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip turnSound;
    
    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float cellSize = 1f;
    [SerializeField, Min(0.01f)] private float moveDuration = 0.5f;
    [SerializeField, Min(0.01f)] private float turnDuration = 0.25f;

    private MazeData maze;
    private Vector2Int gridPosition;
    private Direction facing;
    private MotionHandle motion;

    public Vector2Int GridPosition => gridPosition;

    public void Initialize(MazeData mazeData, Vector2Int startPosition)
    {
        motion.TryCancel();

        maze = mazeData;
        gridPosition = startPosition;
        facing = Direction.West;

        player.SetPositionAndRotation(GetCellCenter(gridPosition), GetWorldRotation());
    }

    public void MoveForward() => Move(GetDirectionVector(facing));
    public void MoveBackward() => Move(-GetDirectionVector(facing));
    public void MoveLeft() => Move(GetDirectionVector(GetDirectionAfterTurn(-1)));
    public void MoveRight() => Move(GetDirectionVector(GetDirectionAfterTurn(1)));
    public void TurnLeft() => Turn(-1);
    public void TurnRight() => Turn(1);

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        var localPosition = gridOrigin.InverseTransformPoint(worldPosition);

        return new Vector2Int(
            Mathf.FloorToInt(localPosition.x / cellSize),
            Mathf.FloorToInt(localPosition.z / cellSize)
        );
    }

    private void Move(Vector2Int direction)
    {
        if (!CanAct())
            return;

        var targetPosition = gridPosition + direction;
        var cellType = maze.GetCellType(targetPosition);

        if (cellType != WallType.ROOM && cellType != WallType.CORRIDOR)
            return;

        gridPosition = targetPosition;
        motion = LMotion.Create(player.position, GetCellCenter(targetPosition), moveDuration)
            .WithEase(Ease.InOutSine)
            .BindToPosition(player)
            .AddTo(this);
        
        audioSource.PlayOneShot(moveSound);
    }

    private void Turn(int quarterTurns)
    {
        if (!CanAct())
            return;

        facing = GetDirectionAfterTurn(quarterTurns);

        var start = player.rotation;
        var target = GetWorldRotation();

        motion = LMotion.Create(0f, 1f, turnDuration)
            .WithEase(Ease.InOutSine)
            .Bind(t => player.rotation = Quaternion.Slerp(start, target, t))
            .AddTo(this);
        
        //audioSource.PlayOneShot(turnSound);
    }

    private bool CanAct() => maze != null && !motion.IsActive();

    private Vector3 GetCellCenter(Vector2Int position)
    {
        var localCenter = new Vector3((position.x + 0.5f) * cellSize, 0f, (position.y + 0.5f) * cellSize);

        return gridOrigin.TransformPoint(localCenter);
    }

    private Quaternion GetWorldRotation()
    {
        var direction = GetDirectionVector(facing);
        var forward = gridOrigin.TransformDirection(new Vector3(direction.x, 0f, direction.y));

        return Quaternion.LookRotation(forward, gridOrigin.up);
    }

    private Direction GetDirectionAfterTurn(int quarterTurns)
    {
        return (Direction)(((int)facing + quarterTurns + DirectionCount) % DirectionCount);
    }

    private static Vector2Int GetDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.East => Vector2Int.right,
            Direction.South => Vector2Int.down,
            Direction.West => Vector2Int.left,
            _ => Vector2Int.up
        };
    }

    private enum Direction
    {
        North,
        East,
        South,
        West
    }
}
