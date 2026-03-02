// EnemySpawnManager.cs
// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)

using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Responsible for spawning all enemies for a round.
    /// Handles loading enemy scenes, finding spawn areas,
    /// scheduling waves, and initializing each spawned enemy.
    /// </summary>
    /// <remarks>
    /// EnemySpawnManager does not track collectibles. Instead, after spawning an enemy,
    /// it connects the enemy's death signal to <see cref="CollectibleSpawnManager"/>.
    ///
    /// Spawn areas are collected from the assigned root node and used to determine
    /// valid positions for spawning wave batches.
    ///
    /// This manager is driven by <see cref="RoundManager"/> which provides round and wave data.
    /// </remarks>
    public partial class EnemySpawnManager : Node3D
    {
        #region Nested types
        private sealed class SpawnAreaInfo
        {
            public Node3D SpawnAreaNode { get; }
            public List<Marker3D> SpawnAreaMarkers { get; }
            public float DistanceToPlayer { get; set; }

            public SpawnAreaInfo(Node3D spawnAreaNode)
            {
                SpawnAreaNode = spawnAreaNode;
                SpawnAreaMarkers = new List<Marker3D>();
                DistanceToPlayer = 0.0f;
            }
        }

        private sealed class EnemySpawnEntry
        {
            public EnemyType EnemyType { get; }
            public PackedScene Scene { get; }

            public EnemySpawnEntry(EnemyType enemyType, PackedScene scene)
            {
                EnemyType = enemyType;
                Scene = scene;
            }
        }
        #endregion Nested types

        #region Constants
        private const float WAVE_SPAWN_OVERFLOW_DELAY = 0.1f;
        #endregion Constants

        #region Exported fields
        [Export]
        private PackedScene _meleeScene = null;

        [Export]
        private PackedScene _rangedScene = null;

        [Export]
        private PackedScene _shieldedScene = null;

        [ExportGroup("SpawnAreaSize")]
        [Export]
        private Node3D _spawnAreasRoot = null;

        [Export(PropertyHint.Range, "1,1,1,or_greater")]
        private int _maxAreasToUse = 6;
        #endregion Exported fields

        #region Runtime state
        private readonly List<SpawnAreaInfo> _spawnAreasList = new List<SpawnAreaInfo>();
        private Dictionary<EnemyType, PackedScene> _enemyScenes = new Dictionary<EnemyType, PackedScene>();

        private RoundData _currentRound = null;
        private double _roundLength = 0.0;
        private bool _roundActive = false;

        private CollectibleSpawnManager _collectibleSpawnManager = null;
        #endregion Runtime state

        #region Properties
        private CharacterBody3D Player
        {
            get
            {
                if (TargetProvider.Instance == null)
                {
                    GD.PushWarning("TargetProvider.Instance not found. Can't spawn enemies based on player position");
                    return null;
                }

                return TargetProvider.Instance.Player;
            }
        }
        #endregion Properties

        #region Godot lifecycle
        public override void _Ready()
        {
            SetEnemyTypeSceneReference();
            CollectSpawnAreas();

            _collectibleSpawnManager = GetParent().GetNodeOrNull<CollectibleSpawnManager>("CollectibleSpawnManager");

            if (_collectibleSpawnManager == null)
            {
                GD.PushError("CollectibleSpawnManager not found as sibling to EnemySpawnManager.");
            }

            if (!ValidateSetup())
            {
                GD.PushError("One or more exported references are not assigned OR resource not found.");
            }
        }
        #endregion Godot lifecycle

        #region Public Round API
        /// <summary>
        /// Begins a round using the provided round data and starts scheduling waves.
        /// </summary>
        /// <param name="round">The round that defines wave timings and enemy counts.</param>
        public void StartRound(RoundData round)
        {
            if (round == null)
            {
                GD.PushError("StartRound called with null RoundData.");
                return;
            }

            _currentRound = round;
            _roundActive = true;

            StartWaveTimers();

            GD.Print(
                string.Format("[{0}] Round started. Length={1}s, Waves={2}", Name, round.RoundLength, round.Waves.Count)
            );
        }

        /// <summary>
        /// Stops the current round and cancels further wave spawning.
        /// </summary>
        public void StopRound()
        {
            _roundActive = false;
            _currentRound = null;

            GD.Print(string.Format("[{0}] Round stopped by RoundManager.", Name));
        }
        #endregion Public Round API

        #region Validation & Setup
        /// <summary>
        /// Assigns the PackedScene associated with each enemy type.
        /// </summary>
        private void SetEnemyTypeSceneReference()
        {
            _enemyScenes = new Dictionary<EnemyType, PackedScene>
            {
                { EnemyType.Melee, _meleeScene },
                { EnemyType.Ranged, _rangedScene },
                { EnemyType.Shielded, _shieldedScene },
            };
        }

        /// <summary>
        /// Scans the spawn areas root for valid spawn areas and their marker positions.
        /// </summary>
        private void CollectSpawnAreas()
        {
            _spawnAreasList.Clear();

            if (_spawnAreasRoot == null)
            {
                return;
            }

            foreach (Node child in _spawnAreasRoot.GetChildren())
            {
                if (child is not Node3D areaNode)
                {
                    continue;
                }

                SpawnAreaInfo info = new SpawnAreaInfo(areaNode);

                foreach (Node markerChild in areaNode.GetChildren())
                {
                    if (markerChild is Marker3D marker)
                    {
                        info.SpawnAreaMarkers.Add(marker);
                    }
                }

                if (info.SpawnAreaMarkers.Count > 0)
                {
                    _spawnAreasList.Add(info);
                }
            }
        }

        private bool ValidateSetup()
        {
            bool isValid = true;

            if (_meleeScene == null)
            {
                GD.PushError("Enemy Melee Scene is not assigned.");
                isValid = false;
            }

            if (_rangedScene == null)
            {
                GD.PushError("Enemy Ranged Scene is not assigned.");
                isValid = false;
            }

            if (_shieldedScene == null)
            {
                GD.PushError("Enemy Shielded Scene is not assigned.");
                isValid = false;
            }

            if (_spawnAreasRoot == null)
            {
                GD.PushError("SpawnAreasRoot is not assigned.");
                isValid = false;
            }

            if (_spawnAreasList.Count == 0)
            {
                GD.PushError("No spawn areas with markers found.");
                isValid = false;
            }

            return isValid;
        }
        #endregion Validation & Setup

        #region Wave Scheduling
        /// <summary>
        /// Creates timers for each wave so they spawn at the correct times.
        /// </summary>
        private void StartWaveTimers()
        {
            if (_currentRound.Waves == null || _currentRound.Waves.Count == 0)
            {
                GD.PushError("_currentRound.Waves is null OR _currentRound.Waves.Count is 0.");
                return;
            }

            _roundLength = Mathf.Max(0.0, _currentRound.RoundLength);

            for (int i = 0; i < _currentRound.Waves.Count; i++)
            {
                WaveData wave = _currentRound.Waves[i];
                if (wave == null)
                {
                    continue;
                }

                double time = Mathf.Max(0.0, wave.SpawnTimeAfterStart);

                if (time > _roundLength)
                {
                    GD.PushWarning(
                        string.Format(
                            "Wave {0} spawn time {1:F2}s exceeds round length {2:F2}s. Clamping to round length.",
                            i,
                            time,
                            _roundLength
                        )
                    );
                    time = _roundLength;
                }

                SceneTreeTimer timer = GetTree().CreateTimer((float)time);
                timer.Timeout += () => OnWaveTimerTimeout(wave);
            }
        }

        private void OnWaveTimerTimeout(WaveData wave)
        {
            if (!_roundActive)
            {
                GD.PushWarning("Wave timer ran out while round was set to stop.");
                return;
            }

            if (_currentRound == null)
            {
                GD.PushError("Wave timer ran out, but _currentRound is null.");
                return;
            }

            if (_currentRound.Waves == null || !_currentRound.Waves.Contains(wave))
            {
                GD.PushError("Wave timer ran out, but Waves is null OR doesn't contain waves.");
                return;
            }

            SpawnWave(wave);
        }
        #endregion Wave Scheduling

        #region Spawning
        /// <summary>
        /// Creates a full roster for the wave and begins spawning it.
        /// </summary>
        private void SpawnWave(WaveData wave)
        {
            if (wave == null)
            {
                GD.PushWarning("SpawnWave called with null WaveData.");
                return;
            }

            List<EnemySpawnEntry> fullRoster = BuildEnemyRoster(wave);
            if (fullRoster.Count == 0)
            {
                GD.PushWarning("Wave roster is empty; nothing to spawn.");
                return;
            }

            ShuffleRoster(fullRoster);

            SpawnWaveBatch(wave, fullRoster, 0);
        }

        /// <summary>
        /// Spawns a portion of the wave's enemies using selected spawn areas.
        /// Marker count = 16 -> max Batch size 16
        /// </summary>
        private void SpawnWaveBatch(WaveData wave, List<EnemySpawnEntry> roster, int startIndex)
        {
            if (!CanSpawnBatch(wave, roster, startIndex))
            {
                return;
            }

            CharacterBody3D player = Player;
            if (player == null)
            {
                GD.PushWarning("Player is null. Spawning wave without distance-based area selection.");
            }

            List<SpawnAreaInfo> chosenAreas = GetSpawnAreasForBatch(player);
            if (chosenAreas.Count == 0)
            {
                GD.PushWarning("No spawn areas available.");
                return;
            }

            if (
                !PrepareBatchAreas(
                    chosenAreas,
                    out List<List<Marker3D>> markersPerArea,
                    out List<int> activeAreaIndices,
                    out int totalMarkers
                )
            )
            {
                GD.PushWarning("Chosen areas have no markers.");
                return;
            }

            int remaining = roster.Count - startIndex;
            int batchCount = Mathf.Min(remaining, totalMarkers);

            List<Marker3D> chosenMarkers = AssignMarkersForBatch(markersPerArea, activeAreaIndices, batchCount);

            int spawnedCount = SpawnEnemiesAtMarkers(roster, startIndex, chosenMarkers);

            GD.Print(
                string.Format("[{0}] Spawned batch: {1} enemies (wave total: {2}).", Name, spawnedCount, roster.Count)
            );

            ScheduleNextBatchIfNeeded(wave, roster, startIndex, spawnedCount);
        }

        private bool CanSpawnBatch(WaveData wave, List<EnemySpawnEntry> roster, int startIndex)
        {
            if (!_roundActive || _currentRound == null)
            {
                GD.PushWarning("Cannot spawn batch: round is not active or _currentRound is null.");
                return false;
            }

            if (wave == null)
            {
                GD.PushWarning("Cannot spawn batch: WaveData is null.");
                return false;
            }

            if (roster == null)
            {
                GD.PushWarning("Cannot spawn batch: roster is null.");
                return false;
            }

            if (startIndex >= roster.Count)
            {
                GD.PushWarning(
                    string.Format(
                        "Cannot spawn batch: startIndex ({0}) is >= roster.Count ({1}).",
                        startIndex,
                        roster.Count
                    )
                );
                return false;
            }

            return true;
        }

        private bool PrepareBatchAreas(
            List<SpawnAreaInfo> chosenAreas,
            out List<List<Marker3D>> markersPerArea,
            out List<int> activeAreaIndices,
            out int totalMarkers
        )
        {
            markersPerArea = new List<List<Marker3D>>(chosenAreas.Count);
            activeAreaIndices = new List<int>();
            totalMarkers = 0;

            for (int i = 0; i < chosenAreas.Count; i++)
            {
                List<Marker3D> areaMarkers = new List<Marker3D>(chosenAreas[i].SpawnAreaMarkers);
                if (areaMarkers.Count == 0)
                {
                    continue;
                }

                markersPerArea.Add(areaMarkers);
                activeAreaIndices.Add(markersPerArea.Count - 1);
                totalMarkers += areaMarkers.Count;
            }

            return totalMarkers > 0;
        }

        /// <summary>
        /// Picks spawn markers for a wave batch.
        /// </summary>
        private List<Marker3D> AssignMarkersForBatch(
            List<List<Marker3D>> markersPerArea,
            List<int> activeAreaIndices,
            int batchCount
        )
        {
            List<Marker3D> chosenMarkers = new List<Marker3D>(batchCount);

            while (chosenMarkers.Count < batchCount && activeAreaIndices.Count > 0)
            {
                int activeIndex = GetRandomIntInclusive(0, activeAreaIndices.Count - 1);
                int areaIndex = activeAreaIndices[activeIndex];

                List<Marker3D> areaMarkers = markersPerArea[areaIndex];
                int lastMarkerIndex = areaMarkers.Count - 1;
                Marker3D marker = areaMarkers[lastMarkerIndex];
                areaMarkers.RemoveAt(lastMarkerIndex);

                chosenMarkers.Add(marker);

                if (areaMarkers.Count == 0)
                {
                    activeAreaIndices.RemoveAt(activeIndex);
                }
            }

            return chosenMarkers;
        }

        /// <summary>
        /// Instantiates enemies at the provided markers, initializes them,
        /// and connects their death signals.
        /// </summary>
        private int SpawnEnemiesAtMarkers(List<EnemySpawnEntry> roster, int startIndex, List<Marker3D> chosenMarkers)
        {
            int spawnedCount = 0;

            for (int i = 0; i < chosenMarkers.Count; i++)
            {
                int rosterIndex = startIndex + i;
                if (rosterIndex >= roster.Count)
                {
                    break;
                }

                EnemySpawnEntry entry = roster[rosterIndex];
                if (entry.Scene == null)
                {
                    GD.PushWarning($"{Name}: Null PackedScene in roster at index {rosterIndex}.");
                    continue;
                }

                Marker3D marker = chosenMarkers[i];

                Enemy enemy = entry.Scene.Instantiate<Enemy>();

                // TODO:
                // Once LevelManager implements AddLevelObject(...) for enemies,
                // replace AddChild(enemy) with:
                // LevelManager.active.AddLevelObject(enemy)

                AddChild(enemy);
                enemy.GlobalPosition = marker.GlobalPosition;

                enemy.Initialize(entry.EnemyType);

                if (_collectibleSpawnManager != null)
                {
                    enemy.EnemyDied += _collectibleSpawnManager.OnEnemyDiedSignal;
                }

                ISpawnable spawnable = enemy as ISpawnable;
                if (spawnable != null)
                {
                    spawnable.OnSpawn();
                }

                spawnedCount++;
            }

            return spawnedCount;
        }

        private void ScheduleNextBatchIfNeeded(
            WaveData wave,
            List<EnemySpawnEntry> roster,
            int startIndex,
            int spawnedCount
        )
        {
            int nextStartIndex = startIndex + spawnedCount;
            if (!_roundActive)
            {
                return;
            }

            if (nextStartIndex >= roster.Count)
            {
                return;
            }

            SceneTreeTimer timer = GetTree().CreateTimer(WAVE_SPAWN_OVERFLOW_DELAY);
            int capturedNextStartIndex = nextStartIndex;
            timer.Timeout += () => SpawnWaveBatch(wave, roster, capturedNextStartIndex);
        }

        /// <summary>
        /// Generates a randomized enemy list for a wave.
        /// </summary>
        private List<EnemySpawnEntry> BuildEnemyRoster(WaveData wave)
        {
            List<EnemySpawnEntry> roster = new List<EnemySpawnEntry>();

            foreach (KeyValuePair<EnemyType, int> pair in wave.Enemies)
            {
                EnemyType type = pair.Key;
                int count = pair.Value;

                if (count <= 0)
                {
                    continue;
                }

                if (!_enemyScenes.TryGetValue(type, out PackedScene scene) || scene == null)
                {
                    GD.PushWarning(string.Format("Missing scene mapping for {0}.", type));
                    continue;
                }

                for (int i = 0; i < count; i++)
                {
                    roster.Add(new EnemySpawnEntry(type, scene));
                }
            }

            return roster;
        }

        private void ShuffleRoster(List<EnemySpawnEntry> roster)
        {
            for (int i = roster.Count - 1; i > 0; i--)
            {
                int j = GetRandomIntInclusive(0, i);
                (roster[i], roster[j]) = (roster[j], roster[i]);
            }
        }
        #endregion Spawning

        #region Area Selection
        /// <summary>
        /// Selects spawn areas based on distance to the player.
        /// </summary>
        private List<SpawnAreaInfo> GetSpawnAreasForBatch(CharacterBody3D player)
        {
            List<SpawnAreaInfo> result = new List<SpawnAreaInfo>();

            if (_spawnAreasList.Count == 0)
            {
                return result;
            }

            int availableAreas = _spawnAreasList.Count;
            int areasToUse = Mathf.Min(_maxAreasToUse, availableAreas);
            if (areasToUse <= 0)
            {
                return result;
            }

            if (player != null)
            {
                Vector3 playerPosition = player.GlobalPosition;

                foreach (SpawnAreaInfo spawnAreaInfo in _spawnAreasList)
                {
                    float distance = spawnAreaInfo.SpawnAreaNode.GlobalPosition.DistanceTo(playerPosition);
                    spawnAreaInfo.DistanceToPlayer = distance;
                }

                ShuffleSpawnAreaList(_spawnAreasList);
                _spawnAreasList.Sort((a, b) => b.DistanceToPlayer.CompareTo(a.DistanceToPlayer));
            }
            else
            {
                ShuffleSpawnAreaList(_spawnAreasList);
            }

            for (int i = 0; i < areasToUse; i++)
            {
                result.Add(_spawnAreasList[i]);
            }

            ShuffleSpawnAreaList(result);

            return result;
        }

        private void ShuffleSpawnAreaList(List<SpawnAreaInfo> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = GetRandomIntInclusive(0, i);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        #endregion Area Selection

        #region Helpers
        /// <summary>
        /// Utility random inclusive integer.
        /// </summary>
        private static int GetRandomIntInclusive(int minInclusive, int maxInclusive)
        {
            if (maxInclusive < minInclusive)
            {
                (minInclusive, maxInclusive) = (maxInclusive, minInclusive);
            }

            int rangeSize = maxInclusive - minInclusive + 1;
            uint randomValue = GD.Randi();
            int offset = (int)(randomValue % (uint)rangeSize);
            return minInclusive + offset;
        }
        #endregion Helpers
    }
}
