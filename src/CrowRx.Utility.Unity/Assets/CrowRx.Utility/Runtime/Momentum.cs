using UnityEngine;


namespace CrowRx.Utility
{
    public class MomentumVector3
    {
        public Vector3 velocity;
        public Vector3 total;

        float momentum_amount = 0.35f;
        float threshold = 18f;
        float dampen_strength_on = 12f;
        float dampen_strength_off = 5f;


        public MomentumVector3(float _momentum_amount = 0.35f, float _threshold = 18f, float _dampen_strength_on = 12f, float _dampen_strength_off = 5f)
        {
            velocity = total = Vector3.zero;

            Init(_momentum_amount, _threshold, _dampen_strength_on, _dampen_strength_off);
        }

        public void Init(float _momentum_amount = 0.35f, float _threshold = 18f, float _dampen_strength_on = 12f, float _dampen_strength_off = 5f)
        {
            if (_momentum_amount > 0.0f)
                momentum_amount = _momentum_amount;

            if (_threshold > 0.0f)
                threshold = _threshold;

            if (_dampen_strength_on > 0.0f)
                dampen_strength_on = _dampen_strength_on;

            if (_dampen_strength_off > 0.0f)
                dampen_strength_off = _dampen_strength_off;
        }

        public void Reset()
        {
            velocity = Vector3.zero;
        }

        public void Tick_On(float delta_time, Vector3 offset)
        {
            velocity = Vector3.Lerp(velocity, velocity + (offset * momentum_amount), 0.67f);

            Mathm.SpringDampen(ref velocity, dampen_strength_on, delta_time);
        }

        public bool Tick_Off(float delta_time)
        {
            if (velocity.sqrMagnitude < threshold)
                return false;

            total = Mathm.SpringDampen(ref velocity, dampen_strength_off, delta_time);

            return true;
        }
    }
}