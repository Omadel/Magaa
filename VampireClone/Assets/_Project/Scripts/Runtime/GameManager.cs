using DG.Tweening;
using Etienne;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Range = Etienne.Range;

namespace Magaa
{
    [DefaultExecutionOrder(-1)]
    public class GameManager : Singleton<GameManager>
    {
        public int AngleSteps => angleSteps;
        public Vector3 Forward => forward;
        public Vector3 Right => right;
        public Player Player => player;
        public bool HasAnyEnemies => enemies.Count > 0;

        [SerializeField] private int angleSteps = 45;
        [Header("Time")]
        [SerializeField, Tooltip("Time in minute")] private float totalGameTime = 15f;
        [SerializeField, ReadOnly] private string currentTimeFormatted;
        [SerializeField] private TMPro.TextMeshProUGUI timerTextMesh;
        [SerializeField] private GameObject helicopter;

        [Header("Levels")]
        [SerializeField] private AnimationCurve experienceCurve;
        [SerializeField, MinMaxRange(1, 1000)] private Range levelExperienceRange = new Range(1, 100);
        [SerializeField, MinMaxRange(1, 1000)] private Range levelRange = new Range(1, 100);
        [SerializeField, ReadOnly] private int currentLevel;
        [SerializeField, ReadOnly] private int experience;
        [SerializeField, ReadOnly] private int currentLevelGoal;
        [SerializeField] private GameObject upgrade;

        [Header("ZombieCap")]
        [SerializeField] private int maxZombies;
        [SerializeField, ReadOnly] private int currentZombies;
        [SerializeField] private TMPro.TextMeshProUGUI maxZombiesUGUI, currentZombiesUGUI;
        [SerializeField] private Slider zombiesSlider;
        [SerializeField] private GameObject nuclearExplosionPrefab;

        [Header("EnemyWaves")]
        [SerializeField] private Enemy enemyPrefab;
        [SerializeField] private AnimationCurve enemySpawnRate;
        [SerializeField] private AnimationCurve enemySpawnCount;
        [SerializeField] private AnimationCurve enemyHealth;
        [SerializeField] private AnimationCurve enemyDamage;
        [SerializeField] private AnimationCurve enemySpeed;
        [SerializeField, MinMaxRange(0f, 50f)] private Range enemySpawnRange = new Range(10f, 15f);
        [Header("Bosses")]
        [SerializeField] private BossSpawn[] bossSpawns;

        [System.Serializable]
        private class BossSpawn
        {
            public float WarningTime => (SpawnTime * 60f) - WarningSound.Clip.length;
            public float SpawnSoundTime => (SpawnTime * 60f) - SpawnSound.Clip.length;
            [Tooltip("In Minutes")] public float SpawnTime;
            public BruteBoss Prefab;
            [Header("Stats")]
            public int Health = 1000;
            public Reward Reward;
            public Sound WarningSound;
            public Sound SpawnSound;
            [ReadOnly] public bool HasBeenAnounced = false;
            [ReadOnly] public bool HasSpawnSounded = false;
        }

        private int bossIndex = 0;
        private Player player;
        private ExperienceBar experienceBar;
        private List<Enemy> enemies = new List<Enemy>();
        private float currentTime;
        private float ennemyTime;
        private Vector3 forward, right;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Player player = Application.isPlaying ? this.player : FindObjectOfType<Player>();
            Color color = Color.white;
            using (new UnityEditor.Handles.DrawingScope(color, player.transform.localToWorldMatrix))
            {
                UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 360f, enemySpawnRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 360f, enemySpawnRange.Max);
            }
            color.a = .1f;
            using (new UnityEditor.Handles.DrawingScope(color, player.transform.localToWorldMatrix))
            {
                UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 360f, enemySpawnRange.Min);
                UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 360f, enemySpawnRange.Max);
            }
        }
#endif
        private void Start()
        {
            CalculateDirectionVectors();
            LevelUp(false);
            upgrade.SetActive(false);
            helicopter.SetActive(false);
            maxZombiesUGUI.text = maxZombies.ToString();
        }

        private void Update()
        {
            currentTime += Time.deltaTime;
            TimeSpan currentTimeSpan = TimeSpan.FromSeconds(currentTime);
            currentTimeFormatted = $"{currentTimeSpan.Minutes:00}:{currentTimeSpan.Seconds:00}";
            timerTextMesh.text = currentTimeFormatted;
            HandleEnemySpawns();
            if (bossIndex >= bossSpawns.Length) return;
            BossSpawn currentBoss = bossSpawns[bossIndex];
            if (currentTime >= currentBoss.WarningTime && !currentBoss.HasBeenAnounced)
            {
                currentBoss.WarningSound.Play(transform.position);
                currentBoss.HasBeenAnounced = true;
            }
            if (currentTime >= currentBoss.SpawnSoundTime && !currentBoss.HasSpawnSounded)
            {
                currentBoss.SpawnSound.Play(transform.position);
                currentBoss.HasSpawnSounded = true;
            }
            if (currentTime > currentBoss.SpawnTime * 60f)
            {
                bossIndex++;

                Vector3 position = player.transform.position + GetRandomPositionInsindeRange();
                Vector3 direction = player.transform.position - position;
                var boss = GameObject.Instantiate(currentBoss.Prefab, position, Quaternion.LookRotation(RoundWorldDirection(direction)));
                boss.SetReward(currentBoss.Health, currentBoss.Reward);
            }
            if (currentTime >= totalGameTime * 60f)
            {
                helicopter.SetActive(true);
                helicopter.transform.position = player.transform.position;
                enabled = false;
            }
        }

        private void HandleEnemySpawns()
        {
            ennemyTime += Time.deltaTime;
            float curveValue = currentTime / (totalGameTime * 60);
            float spawnDelay = enemySpawnRate.Evaluate(curveValue) / enemySpawnCount.Evaluate(curveValue);
            if (ennemyTime < spawnDelay) return;
            ennemyTime -= spawnDelay;

            Vector3 position = player.transform.position + GetRandomPositionInsindeRange();
            Vector3 direction = player.transform.position - position;
            Enemy enemy = GameObject.Instantiate(enemyPrefab, position, Quaternion.LookRotation(RoundWorldDirection(direction)));

            float value = Random.value;
            float speed = enemySpeed.Evaluate(curveValue);
            bool isRunning = value < speed;
            enemy.SetStats(Mathf.RoundToInt(enemyHealth.Evaluate(curveValue)), Mathf.RoundToInt(enemyDamage.Evaluate(curveValue)), isRunning);
        }

        private Vector3 GetRandomPositionInsindeRange()
        {
            Vector3 position;
            do
            {
                position = UnityEngine.Random.insideUnitCircle * enemySpawnRange.Max;
                position.z = position.y;
                position.y = 0f;
                NavMesh.SamplePosition(position, out NavMeshHit hit, 10f, NavMesh.AllAreas);
                position = hit.position;
            } while (position.magnitude < enemySpawnRange.Min);
            return position;
        }

        private void CalculateDirectionVectors()
        {
            Camera camera = Camera.main;
            Vector3 camRotation = camera.transform.localEulerAngles;
            Quaternion camRotationFlatted = Quaternion.Euler(0, camRotation.y, camRotation.z);
            forward = camRotationFlatted * Vector3.forward;
            right = Vector3.Cross(Vector3.up, forward);
            Debug.DrawRay(transform.position, forward * 5, Color.blue, 10);
            Debug.DrawRay(transform.position, right * 5, Color.red, 10);
        }

        public Vector3 RoundWorldDirection(Vector3 worldDirection)
        {
            float angle = Vector3.SignedAngle(forward, worldDirection, Vector3.up);
            // Round the angle to the nearest multiple of angleSteps
            int roundedAngle = Mathf.RoundToInt(angle / Instance.angleSteps) * Instance.angleSteps;
            // Use Quaternion to rotate the object by the rounded angle
            return Quaternion.Euler(0, roundedAngle, 0) * Instance.forward;
        }
        public Vector3 RoundDirectionFromUIDirection(Vector3 uiDirection)
        {
            Vector3 targetDirection = (forward * uiDirection.y) + (right * uiDirection.x);
            targetDirection.Normalize();
            return RoundWorldDirection(targetDirection);
        }
        public Vector3 RoundUIDirection(Vector3 direction)
        {
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            // Round the angle to the nearest multiple of angleSteps
            int roundedAngle = Mathf.RoundToInt(angle / GameManager.Instance.AngleSteps) * GameManager.Instance.AngleSteps;
            // Use Quaternion to rotate the object by the rounded angle
            return Quaternion.Euler(0, 0, roundedAngle) * Vector3.up;
        }

        public void SetExperienceBar(ExperienceBar experienceBar) => this.experienceBar = experienceBar;
        public void SetPlayer(Player player) => this.player = player;
        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
            currentZombies++;
            UpdateZombiesUI();
            CheckNuclear();
        }

        public void RemoveEnemy(Enemy enemy)
        {
            enemies.Remove(enemy);
            currentZombies--;
            UpdateZombiesUI();
        }

        private void CheckNuclear()
        {
            if (currentZombies != maxZombies) return;
            player.Die();
            GameObject.Instantiate(nuclearExplosionPrefab, player.transform.position, Quaternion.identity);
            enabled = false;
            BruteBoss[] bosses = GameObject.FindObjectsOfType<BruteBoss>();
            lock (bosses)
            {
                int length = bosses.Length;
                for (int i = length - 1; i >= 0; i--)
                {
                    bosses[i].Die();
                }
            }
            lock (enemies)
            {
                int count = enemies.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    enemies[i].Die();
                }
            }
        }

        private void UpdateZombiesUI()
        {
            currentZombiesUGUI.text = ((float)currentZombies).ToString();
            zombiesSlider.value = currentZombies / (float)maxZombies;
        }

        public void HarvestRubis(int value)
        {
            experience += value;
            experienceBar.SetValue(experience);
            if (experience >= currentLevelGoal) LevelUp();
        }

        private void LevelUp(bool notify = true)
        {
            if (!enabled) return;
            currentLevel++;
            currentLevelGoal = Mathf.RoundToInt(levelExperienceRange.Lerp(experienceCurve.Evaluate(levelRange.Normalize(currentLevel))));
            experienceBar.SetMax(currentLevelGoal);
            experienceBar.SetValue(experience);
            experience = 0;
            if (notify)
            {
                upgrade.SetActive(true);
                PauseGame();
            }
        }

        private Tween pauseTween;
        public void PauseGame()
        {
            pauseTween?.Kill();
            pauseTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, .01f, .4f).SetUpdate(true);
            //Time.timeScale = 0f;
        }
        public void UnPauseGame()
        {
            pauseTween?.Kill();
            pauseTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, .4f).SetUpdate(true);
            //Time.timeScale = 1f;
        }
    }
}
