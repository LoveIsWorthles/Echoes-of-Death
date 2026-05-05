// This is not a class, it's an interface! 
// Any script that uses this MUST have a TakeDamage function.
public interface IDamagable
{
    void TakeDamage(float amount);
}