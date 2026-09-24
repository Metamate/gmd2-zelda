namespace GMDCore.Tweening;

internal interface ITweenTask
{
    void Update(float dt);
    bool IsComplete { get; }
}
