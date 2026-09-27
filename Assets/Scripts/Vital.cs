using System;
using UnityEngine;

public class Vital : MonoBehaviour
{
    public int Team;
    public float Max = 100f;
    public float Current = 100f;
    public float SafeUntil;
    public float LastHit;
    public bool Alive => Current > 0f;

    public event Action Died;

    public void Hurt(float amount)
    {
        if (!Alive || Time.time < SafeUntil)
            return;

        Current = Mathf.Max(0f, Current - amount);
        LastHit = Time.time;
        if (Current <= 0f)
            Died?.Invoke();
    }

    public void Fill()
    {
        Current = Max;
    }
}
