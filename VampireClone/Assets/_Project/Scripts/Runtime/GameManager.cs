using Etienne;
using System;
using System.Collections.Generic;
using UnityEngine;
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

        [Header("Levels")]
        [SerializeField] private AnimationCurve experienceCurve;
        [SerializeField, MinMaxRange(1, 1000)] private Range levelExperienceRange = new Range(1, 100);
        [SerializeField, MinMaxRange(1, 1000)] private Range levelRange = new Range(1, 100);
        [SerializeField, ReadOnly] private int currentLevel;
        [SerializeField, ReadOnly] private int experience;
        [SerializeField, ReadOnly] private int currentLevelGoal;
        [SerializeField] private GameObject upgrade;

        private Player player;
        private ExperienceBar experienceBar;
        private List<Enemy> enemies = new List<Enemy>();
        private float currentTime;

        private Vector3 forward, right;

        private void Start()
        {
            CalculateDirectionVectors();
            LevelUp(false);
        }

        private void Update()
        {
            currentTime += Time.deltaTime;
            TimeSpan currentTimeSpan = TimeSpan.FromSeconds(currentTime);
            currentTimeFormatted = $"{currentTimeSpan.Minutes:00}:{currentTimeSpan.Seconds:00}";
            timerTextMesh.text = currentTimeFormatted;
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
        public void AddEnemy(Enemy enemy) => enemies.Add(enemy);
        public void RemoveEnemy(Enemy enemy) => enemies.Remove(enemy);

        public void HarvestRubis(int value)
        {
            experience += value;
            experienceBar.SetValue(experience);
            if (experience >= currentLevelGoal) LevelUp();
        }

        private void LevelUp(bool notify = true)
        {
            currentLevel++;
            currentLevelGoal = Mathf.RoundToInt(levelExperienceRange.Lerp(experienceCurve.Evaluate(levelRange.Normalize(currentLevel))));
            experienceBar.SetMax(currentLevelGoal);
            experienceBar.SetValue(experience);
            experience = 0;
            if (notify) upgrade.SetActive(true);
        }
    }
}
