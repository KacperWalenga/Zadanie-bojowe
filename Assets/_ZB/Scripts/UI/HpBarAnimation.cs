using UnityEngine;

namespace ZB
{
    class HpBarAnimation
    {
        private const float FullFill = 0.97f;
        private const float PeakAgitation = 0.224f;
        private const float MaxMovementSway = 3f;
        private const float DamageFillDuration = 0.5f;

        private float healthFraction;
        private float damageFillStart;
        private double? damageStartedAt;
        private float refillFillStart;
        private double? refillStartedAt;
        private double movementStartedAt;
        private float movementDuration;
        private float movementPeakSway;
        private float movementStartSway;
        private float bubbleTimeOffset;

        public float IdleAgitation { private get; set; }
        public bool IsPlaying => movementDuration > 0f || damageStartedAt.HasValue || refillStartedAt.HasValue;

        public void Step(float duration, double now)
        {
            const float StepSway = 1.2f;

            AddMovementSway(duration, StepSway, now);
        }

        public void Turn(float duration, double now) => AddMovementSway(duration, 1f, now);

        public void StopMovement() => movementDuration = 0f;

        public bool SetHealth(int health, int maxHealth, double now)
        {
            const float InstantDamageFraction = 0.35f;

            var nextHealth = Mathf.Clamp01((float)health / Mathf.Max(1, maxHealth));

            if (nextHealth == healthFraction)
                return false;

            var currentFill = GetAnimatedHealth(now);
            var adjustingRefill = refillStartedAt.HasValue && currentFill < nextHealth;

            if (nextHealth > healthFraction || adjustingRefill)
            {
                refillFillStart = currentFill;
                refillStartedAt = now;
                healthFraction = nextHealth;
                damageStartedAt = null;
                
                return true;
            }

            refillStartedAt = null;
            
            var instantDrop = (healthFraction - nextHealth) * InstantDamageFraction;

            damageFillStart = Mathf.Max(nextHealth, currentFill - instantDrop);
            healthFraction = nextHealth;
            damageStartedAt = now;
            
            return true;
        }

        public (float Fill, float FillOffset, float Agitation, float BubbleOffset) Evaluate(double now, float deltaTime)
        {
            StartDamageSway(now);

            var movementStrength = GetMovementSway(now) / MaxMovementSway;
            var fill = GetAnimatedHealth(now) * FullFill;
            var fillOffset = GetMovementFillOffset(now, movementStrength);
            var agitation = Mathf.Lerp(IdleAgitation, PeakAgitation, movementStrength);

            UpdateBubbleFlow(agitation, deltaTime);
            
            return (fill, fillOffset, agitation, bubbleTimeOffset);
        }

        private void AddMovementSway(float duration, float amount, double now)
        {
            var currentSway = GetMovementSway(now);
            var timeLeft = movementDuration - (float)(now - movementStartedAt);
            var accumulatedSway = timeLeft > 0f ? movementPeakSway : currentSway;

            movementStartSway = currentSway;
            movementPeakSway = Mathf.Min(MaxMovementSway, accumulatedSway + amount);
            movementStartedAt = now;
            movementDuration = Mathf.Max(duration, timeLeft);
        }

        private float GetMovementSway(double now)
        {
            const float MovementSwayTailDuration = 1.6f;

            if (movementDuration <= 0f)
                return 0f;

            var elapsed = (float)(now - movementStartedAt);
            var settleProgress = (elapsed - movementDuration) / MovementSwayTailDuration;

            if (settleProgress >= 1f)
            {
                movementDuration = 0f;
                return 0f;
            }

            var sway = Mathf.SmoothStep(movementStartSway, movementPeakSway, elapsed / movementDuration);
            
            return Mathf.SmoothStep(sway, 0f, settleProgress);
        }

        private float GetAnimatedHealth(double now)
        {
            const float RefillDuration = 1.2f;

            if (refillStartedAt is double refillStart)
            {
                var progress = (float)((now - refillStart) / RefillDuration);

                if (progress < 1f)
                    return Mathf.SmoothStep(refillFillStart, healthFraction, progress);

                refillStartedAt = null;
            }

            if (damageStartedAt is not double start)
                return healthFraction;

            var remaining = 1f - Mathf.Clamp01((float)(now - start) / DamageFillDuration);
            
            return Mathf.Lerp(healthFraction, damageFillStart, remaining * remaining);
        }

        private void StartDamageSway(double now)
        {
            const float DamageSwayHoldDuration = 1.2f;

            if (damageStartedAt is not double start || now < start + DamageFillDuration)
                return;

            damageStartedAt = null;
            AddMovementSway(DamageSwayHoldDuration, MaxMovementSway, start + DamageFillDuration);
            movementStartSway = MaxMovementSway;
        }

        private float GetMovementFillOffset(double now, float movementStrength)
        {
            const float MaxMovementFillOffset = 0.00225f;
            const float MovementFillPeriod = 0.9f;

            var phase = (float)(now % MovementFillPeriod) * (2f * Mathf.PI / MovementFillPeriod);
            var amplitude = MaxMovementFillOffset * movementStrength;

            return Mathf.Sin(phase) * Mathf.Min(amplitude, healthFraction * FullFill);
        }

        private void UpdateBubbleFlow(float agitation, float deltaTime)
        {
            const float MaxBubbleSpeedBoost = 1.5f;

            var strength = Mathf.InverseLerp(IdleAgitation, PeakAgitation, agitation);

            bubbleTimeOffset += deltaTime * strength * MaxBubbleSpeedBoost;
        }
    }
}
