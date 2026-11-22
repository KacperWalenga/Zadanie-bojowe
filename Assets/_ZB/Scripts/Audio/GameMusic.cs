using UnityEngine;
using ZB.Mobs;

namespace ZB
{
    public class GameMusic : MonoBehaviour
    {
        [SerializeField] private AudioSource music;
        [SerializeField] private AudioClip bossTrack;
        [SerializeField] private AudioClip gameOverTrack;

        private bool gameEnded;

        private void OnEnable()
        {
            EventManager.AddListener<Mob>(Consts.Events.Combat.COMBAT_STARTED, CombatStarted);
            EventManager.AddListener(Consts.Events.Game.LOST, PlayGameOver);
            EventManager.AddListener(Consts.Events.Game.WON, StopOnVictory);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Mob>(Consts.Events.Combat.COMBAT_STARTED, CombatStarted);
            EventManager.RemoveListener(Consts.Events.Game.LOST, PlayGameOver);
            EventManager.RemoveListener(Consts.Events.Game.WON, StopOnVictory);
        }

        private void CombatStarted(Mob mob)
        {
            if (gameEnded || mob.GetComponentInParent<Boss>() == null || music.clip == bossTrack)
                return;

            Play(bossTrack, true);
        }

        private void PlayGameOver()
        {
            if (gameEnded)
                return;

            gameEnded = true;
            Play(gameOverTrack, false);
        }

        private void StopOnVictory()
        {
            if (gameEnded)
                return;

            gameEnded = true;
            music.Stop();
        }

        private void Play(AudioClip clip, bool loop)
        {
            music.Stop();
            music.clip = clip;
            music.loop = loop;
            music.Play();
        }
    }
}
