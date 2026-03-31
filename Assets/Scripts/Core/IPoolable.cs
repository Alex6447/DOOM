namespace DOOM.Core
{
    /// <summary>
    /// DOOM-1.5 — Интерфейс для объектов, которые возвращаются в Object Pool.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}
