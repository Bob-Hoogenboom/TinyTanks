public abstract class TankBaseState
{
    abstract public void EnterState(TankStateManager tank);
    abstract public void UpdateState(TankStateManager tank);
    abstract public void ExitState(TankStateManager tank);
    abstract public void OnCollisionEnter(TankStateManager tank);
}
