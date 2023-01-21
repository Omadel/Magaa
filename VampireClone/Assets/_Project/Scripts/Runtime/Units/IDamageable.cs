namespace Magaa
{
    interface IDamageable
    {
        int Health { get; }
        void Hit(int damage);
    }
}
