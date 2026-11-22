internal interface IKartDriveSource
{
    /// <summary>이번 물리 틱에서 실행할 커맨드를 가져옵니다.</summary>
    bool TryGetCommand(int tick, float fixedDeltaTime, out KartDriveCommand command);
}
