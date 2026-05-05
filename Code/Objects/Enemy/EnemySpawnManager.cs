// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): TimeForNano <tuominen.mika-95@hotmail.com>
//            Miska Rihu <miska.rihu@tuni.fi>

using System;
using System.Collections.Generic;
using EHE.Common.Godot.Logging;
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
    /// This manager is driven by <see cref="LevelManager"/> which provides round and wave data.
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

        private const float WAVE_SPAWN_OVERFLOW_DELAY = 0.5f;

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

        private Queue<WaveData> _waveQueue = new Queue<WaveData>();
        private readonly List<SpawnAreaInfo> _spawnAreasList = new List<SpawnAreaInfo>();
        private Dictionary<EnemyType, PackedScene> _enemyScenes = new Dictionary<EnemyType, PackedScene>();
        private RoundData _currentRound = null;
        private double _timePassedSinceRoundStart = 0.0;
        private CollectibleSpawnManager _collectibleSpawnManager = null;

        #endregion Runtime state


        #region Properties

        private CharacterBody3D Player
        {
            get
            {
                if (TargetProvider.Instance == null)
                {
                    this.LogWarning("TargetProvider.Instance not found. Can't spawn enemies based on player position");
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
                this.LogError("CollectibleSpawnManager not found as sibling to EnemySpawnManager.");
            }

            if (!ValidateSetup())
            {
                this.LogError("One or more exported references are not assigned OR resource not found.");
            }
        }

        /// <summary>
        ///  Determines when enemy waves are spawned.
        /// </summary>
        /// <param name="delta">Time in seconds since the last frame.</param>
        public override void _Process(double delta)
        {
            if (LevelManager.Active == null || !LevelManager.Active.RoundInProgress)
            {
                return;
            }

            if (_waveQueue.Count == 0)
            {
                return;
            }

            _timePassedSinceRoundStart += delta;

            if (_timePassedSinceRoundStart >= _waveQueue.Peek().SpawnTimeAfterStart)
            {
                OnWaveTimerTimeout(_waveQueue.Dequeue());
            }
        }

        #endregion Godot lifecycle


        #region Public Round API

        /// <summary>
        ///  Initializes the
        /// </summary>
        /// <param name="roundData"></param>
        public void Initialize(RoundData roundData)
        {
            _timePassedSinceRoundStart = 0.0;
            _waveQueue = new Queue<WaveData>();
            this.LogDebug("Cleared wave queue.");

            if (roundData == null)
            {
                this.LogError($"Cannot initialize enemy spawner with null {typeof(RoundData)}!");
                return;
            }

            _currentRound = roundData;

            if (_currentRound.Waves == null)
            {
                this.LogError($"Round {_currentRound.ResourcePath} contains no waves!");
                return;
            }

            // Queue waves.
            this.LogInfo("Queueing waves.");
            for (var i = 0; i < _currentRound.Waves.Count; ++i)
            {
                var wave = _currentRound.Waves[i];

                if (wave == null)
                {
                    this.LogError($"Cannot queue a wave that is null!");
                    continue;
                }

                if (wave.SpawnTimeAfterStart > _currentRound.RoundLength)
                {
                    this.LogError($"Start time for wave {i} exceeds round length. Excluding this wave.");
                    continue;
                }

                _waveQueue.Enqueue(wave);
                this.LogInfo($"Queued wave {i} to start at {wave.SpawnTimeAfterStart} s after round start.");
            }
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
                this.LogError("Enemy Melee Scene is not assigned.");
                isValid = false;
            }

            if (_rangedScene == null)
            {
                this.LogError("Enemy Ranged Scene is not assigned.");
                isValid = false;
            }

            if (_shieldedScene == null)
            {
                this.LogError("Enemy Shielded Scene is not assigned.");
                isValid = false;
            }

            if (_spawnAreasRoot == null)
            {
                this.LogError("SpawnAreasRoot is not assigned.");
                isValid = false;
            }

            if (_spawnAreasList.Count == 0)
            {
                this.LogError("No spawn areas with markers found.");
                isValid = false;
            }

            return isValid;
        }

        #endregion Validation & Setup


        #region Wave Scheduling

        private void OnWaveTimerTimeout(WaveData wave)
        {
            this.LogDebug("Wave timer timed out.");

            if (LevelManager.Active != null && !LevelManager.Active.RoundInProgress)
            {
                this.LogWarning("Wave timer ran out while round was set to stop.");
                return;
            }

            if (_currentRound == null)
            {
                this.LogError("Wave timer ran out, but _currentRound is null.");
                return;
            }

            if (_currentRound.Waves == null || !_currentRound.Waves.Contains(wave))
            {
                this.LogError("Wave timer ran out, but Waves is null OR doesn't contain waves.");
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
                this.LogWarning("SpawnWave called with null WaveData.");
                return;
            }

            List<EnemySpawnEntry> fullRoster = BuildEnemyRoster(wave);
            if (fullRoster.Count == 0)
            {
                this.LogWarning("Wave roster is empty; nothing to spawn.");
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
                this.LogWarning("Player is null. Spawning wave without distance-based area selection.");
            }

            List<SpawnAreaInfo> chosenAreas = GetSpawnAreasForBatch(player);
            if (chosenAreas.Count == 0)
            {
                this.LogWarning("No spawn areas available.");
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
                this.LogWarning("Chosen areas have no markers.");
                return;
            }

            int remaining = roster.Count - startIndex;
            int batchCount = Mathf.Min(remaining, totalMarkers);

            List<Marker3D> chosenMarkers = AssignMarkersForBatch(markersPerArea, activeAreaIndices, batchCount);

            int spawnedCount = SpawnEnemiesAtMarkers(roster, startIndex, chosenMarkers);

            this.LogInfo($"Spawned batch: {spawnedCount} enemies (wave total: {roster.Count}).");

            ScheduleNextBatchIfNeeded(wave, roster, startIndex, spawnedCount);
        }

        private bool CanSpawnBatch(WaveData wave, List<EnemySpawnEntry> roster, int startIndex)
        {
            if ((LevelManager.Active != null && !LevelManager.Active.RoundInProgress) || _currentRound == null)
            {
                this.LogWarning("Cannot spawn batch: round is not active or _currentRound is null.");
                return false;
            }

            if (wave == null)
            {
                this.LogWarning("Cannot spawn batch: WaveData is null.");
                return false;
            }

            if (roster == null)
            {
                this.LogWarning("Cannot spawn batch: roster is null.");
                return false;
            }

            if (startIndex >= roster.Count)
            {
                this.LogWarning($"Cannot spawn batch: startIndex ({startIndex}) is >= roster.Count ({roster.Count}).");
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
                    this.LogWarning($"Null PackedScene in roster at index {rosterIndex}.");
                    continue;
                }

                Marker3D marker = chosenMarkers[i];

                Enemy enemy = entry.Scene.Instantiate<Enemy>();
                enemy.Initialize(entry.EnemyType);
                LevelManager.Active.AddLevelObject(enemy);
                spawnedCount++;

                enemy.GlobalPosition = marker.GlobalPosition;
                enemy.OnSpawn();

                if (_collectibleSpawnManager != null)
                {
                    enemy.EnemyDied += _collectibleSpawnManager.OnEnemyDiedSignal;
                }
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
            if (LevelManager.Active != null && !LevelManager.Active.RoundInProgress)
            {
                return;
            }

            if (nextStartIndex >= roster.Count)
            {
                return;
            }

            if (WAVE_SPAWN_OVERFLOW_DELAY > 0)
            {
                Timer timer = new Timer();
                timer.OneShot = true;
                timer.WaitTime = WAVE_SPAWN_OVERFLOW_DELAY;

                int capturedNextStartIndex = nextStartIndex;

                AddChild(timer);

                timer.Timeout += () =>
                {
                    try
                    {
                        SpawnWaveBatch(wave, roster, capturedNextStartIndex);
                        timer.QueueFree();
                    }
                    catch (ObjectDisposedException e)
                    {
                        this.LogWarning($"Spawn overflow timer expired after being disposed: {e}");
                    }
                    catch (Exception e)
                    {
                        this.LogWarning($"An unexpected exception was raised when spawn overflow timer expired: {e}");
                    }
                };

                timer.Start();
            }
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
                    this.LogWarning($"Missing scene mapping for {type}.");
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
