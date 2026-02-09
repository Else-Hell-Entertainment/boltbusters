using System.Collections.Generic;
using Godot;

namespace EHE.BoltBusters
{
    /// <summary>
    /// Spawns enemies at Marker3D points grouped under SpawnAreas (SpawnArea1,...).
    /// Chooses the furthest spawn areas from the player, then spawns enemies
    /// into their markers in order (Spawn1, Spawn2, ...).
    /// </summary>
    public partial class EnemySpawner : Node3D
    {
        // Designed to only be used inside class EnemySpawner.
        // Designed not to be inherited.
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

        private const int MIN_ALLOWED_AREAS = 1;
        private const int MIN_ALLOWED_ENEMIES_PER_AREA = 1;

        [Export]
        private PackedScene _enemyScene = null;

        [Export]
        private Node3D _spawnAreasRoot = null;

        [Export]
        private Node3D _enemiesRoot = null;

        [Export]
        private Timer _spawnTimer = null;

        // How many spawn areas to use per wave.
        [Export]
        private int _minAreasToUse = 2;

        [Export]
        private int _maxAreasToUse = 4;

        // How many enemies to spawn per selected area.
        [Export]
        private int _minEnemiesPerArea = 1;

        [Export]
        private int _maxEnemiesPerArea = 4;

        // Time between spawn checks (seconds).
        [Export]
        private float _spawnIntervalSeconds = 5.0f;

        // If enemies alive drops below this, allow a new wave to spawn.
        [Export]
        private int _minAliveEnemies = 4;

        // Timer to track when to check alive enemy count.
        private float _aliveCheckTimer = 0.0f;
        private const float ALIVE_CHECK_INTERVAL = 1.0f;

        /// <summary>
        /// Enemy scene to instantiate when spawning.
        /// </summary>
        public PackedScene EnemyScene
        {
            get { return _enemyScene; }
        }

        /// <summary>
        /// Current player used as distance reference for spawn areas.
        /// Always read from TargetProvider.
        /// </summary>
        private CharacterBody3D Player
        {
            get
            {
                if (TargetProvider.Instance == null)
                {
                    return null;
                }

                return TargetProvider.Instance.Player;
            }
        }

        /// <summary>
        /// Number of enemies currently alive under the enemies root.
        /// </summary>
        public int AliveEnemyCount
        {
            get
            {
                if (_enemiesRoot == null)
                {
                    return 0;
                }

                return _enemiesRoot.GetChildCount();
            }
        }

        // List of all spawn areas with their markers.
        private List<SpawnAreaInfo> _spawnAreasList = new List<SpawnAreaInfo>();

        public override void _Ready()
        {
            if (!ValidateSetup())
            {
                // If setup is invalid, do not continue.
                return;
            }

            CollectSpawnAreas();

            if (_spawnAreasList.Count == 0)
            {
                GD.PushWarning($"{Name}: No spawn areas with markers found.");
            }

            SetupSpawnTimer();

            // Spawn an initial wave at start.
            CallDeferred(nameof(SpawnWaveFromPlayerPosition));
        }

        public override void _Process(double delta)
        {
            // If no player, skip entirely
            if (Player == null)
            {
                return;
            }

            // Count up
            _aliveCheckTimer += (float)delta;

            // Run the check only every ALIVE_CHECK_INTERVAL seconds
            if (_aliveCheckTimer >= ALIVE_CHECK_INTERVAL)
            {
                _aliveCheckTimer = 0.0f; // reset timer

                // If too few enemies alive -> spawn AND reset wave timer
                if (AliveEnemyCount <= _minAliveEnemies)
                {
                    SpawnWaveAndResetMainTimer();
                }
            }
        }

        /// <summary>
        /// Public helper if you want to trigger a spawn wave from another script.
        /// Uses the current Player position from TargetProvider.
        /// </summary>
        public void SpawnWaveFromPlayerPosition()
        {
            CharacterBody3D player = Player;

            if (player == null)
            {
                GD.PushError($"{Name}: Cannot spawn wave, Player is null.");
                return;
            }

            SpawnWave(player);
        }

        private bool ValidateSetup()
        {
            bool isValid = true;

            if (_enemyScene == null)
            {
                GD.PushError($"{Name}: EnemyScene is not assigned.");
                isValid = false;
            }

            if (_spawnAreasRoot == null)
            {
                GD.PushError($"{Name}: SpawnAreasRoot is not assigned.");
                isValid = false;
            }

            if (_enemiesRoot == null)
            {
                GD.PushError($"{Name}: EnemiesRoot is not assigned.");
                isValid = false;
            }

            if (_spawnTimer == null)
            {
                GD.PushWarning($"{Name}: SpawnTimer is not assigned. Automatic spawning will be disabled.");
            }

            // Clamp configuration to sane values.
            if (_minAreasToUse < MIN_ALLOWED_AREAS)
            {
                _minAreasToUse = MIN_ALLOWED_AREAS;
            }

            if (_maxAreasToUse < _minAreasToUse)
            {
                _maxAreasToUse = _minAreasToUse;
            }

            if (_minEnemiesPerArea < MIN_ALLOWED_ENEMIES_PER_AREA)
            {
                _minEnemiesPerArea = MIN_ALLOWED_ENEMIES_PER_AREA;
            }

            if (_maxEnemiesPerArea < _minEnemiesPerArea)
            {
                _maxEnemiesPerArea = _minEnemiesPerArea;
            }

            if (_spawnIntervalSeconds <= 0.0f)
            {
                _spawnIntervalSeconds = 1.0f;
            }

            if (_minAliveEnemies < 0)
            {
                _minAliveEnemies = 0;
            }

            return isValid;
        }

        private void SetupSpawnTimer()
        {
            if (_spawnTimer == null)
            {
                return;
            }

            _spawnTimer.WaitTime = _spawnIntervalSeconds;
            _spawnTimer.OneShot = true;
            _spawnTimer.Timeout += OnSpawnTimerTimeout;
            _spawnTimer.Start();
        }

        private void CollectSpawnAreas()
        {
            _spawnAreasList.Clear();

            foreach (Node child in _spawnAreasRoot.GetChildren())
            {
                Node3D areaNode = child as Node3D;

                if (areaNode == null)
                {
                    continue;
                }

                SpawnAreaInfo spawnAreaInfo = new SpawnAreaInfo(areaNode);

                foreach (Node markerChild in areaNode.GetChildren())
                {
                    Marker3D marker = markerChild as Marker3D;

                    if (marker == null)
                    {
                        continue;
                    }

                    spawnAreaInfo.SpawnAreaMarkers.Add(marker);
                }

                if (spawnAreaInfo.SpawnAreaMarkers.Count > 0)
                {
                    _spawnAreasList.Add(spawnAreaInfo);
                }
            }
        }

        private void SpawnWaveAndResetMainTimer()
        {
            CharacterBody3D player = Player;

            if (player == null)
            {
                GD.PushWarning($"{Name}: Tried to spawn wave but Player is null.");
                return;
            }

            SpawnWave(player);

            // Restart the main wave timer
            if (_spawnTimer != null)
            {
                _spawnTimer.Stop();
                _spawnTimer.Start();
            }

            // Also reset the alive-check timer so we do not fire twice in the same window
            _aliveCheckTimer = 0.0f;
        }

        private void OnSpawnTimerTimeout()
        {
            SpawnWaveAndResetMainTimer();
        }

        private void SpawnWave(CharacterBody3D player)
        {
            if (_enemyScene == null)
            {
                return;
            }

            if (_spawnAreasList.Count == 0)
            {
                GD.PushWarning($"{Name}: No spawn areas available for spawning.");
                return;
            }

            List<SpawnAreaInfo> chosenAreas = GetFurthestSpawnAreas(player);

            foreach (SpawnAreaInfo spawnArea in chosenAreas)
            {
                int enemiesToSpawn = GetRandomIntInclusive(_minEnemiesPerArea, _maxEnemiesPerArea);
                SpawnInArea(spawnArea, enemiesToSpawn);
            }
        }

        private void SpawnInArea(SpawnAreaInfo spawnAreaInfo, int enemiesToSpawn)
        {
            if (_enemyScene == null)
            {
                return;
            }

            int maxSpawns = Mathf.Min(enemiesToSpawn, spawnAreaInfo.SpawnAreaMarkers.Count);

            for (int i = 0; i < maxSpawns; i++)
            {
                Marker3D spawnMarker = spawnAreaInfo.SpawnAreaMarkers[i];

                CharacterBody3D enemy = _enemyScene.Instantiate<CharacterBody3D>();

                // Parent under the enemies root for easier tracking.
                _enemiesRoot.AddChild(enemy);

                // Set position AFTER adding to scene tree.
                enemy.GlobalPosition = spawnMarker.GlobalPosition;

                // TODO: Connect enemy death signal here to track alive count more precisely if needed.
            }
        }

        private List<SpawnAreaInfo> GetFurthestSpawnAreas(CharacterBody3D player)
        {
            List<SpawnAreaInfo> result = new List<SpawnAreaInfo>();

            if (player == null)
            {
                // No player: just avoid crashing, return empty list.
                return result;
            }

            Vector3 playerPosition = player.GlobalPosition;

            // 1) Compute distance for each area.
            foreach (SpawnAreaInfo spawnAreaInfo in _spawnAreasList)
            {
                float distance = spawnAreaInfo.SpawnAreaNode.GlobalPosition.DistanceTo(playerPosition);
                spawnAreaInfo.DistanceToPlayer = distance;
            }

            // 2) Shuffle to randomize tie-breaking of equal distances.
            ShuffleSpawnAreas();

            // 3) Sort by distance descending (furthest first).
            _spawnAreasList.Sort((a, b) => b.DistanceToPlayer.CompareTo(a.DistanceToPlayer));

            // 4) Decide how many areas to use this wave.
            int availableAreas = _spawnAreasList.Count;
            int maxUsable = Mathf.Clamp(_maxAreasToUse, MIN_ALLOWED_AREAS, availableAreas);
            int minUsable = Mathf.Clamp(_minAreasToUse, MIN_ALLOWED_AREAS, maxUsable);

            int areasToUse = GetRandomIntInclusive(minUsable, maxUsable);

            for (int i = 0; i < areasToUse; i++)
            {
                result.Add(_spawnAreasList[i]);
            }

            return result;
        }

        private void ShuffleSpawnAreas()
        {
            // Shuffle the list in-place using the Fisher–Yates algorithm.
            // This gives each spawn area an equal chance of ending up in any position.
            for (int i = _spawnAreasList.Count - 1; i > 0; i--)
            {
                int j = GetRandomIntInclusive(0, i);
                (_spawnAreasList[i], _spawnAreasList[j]) = (_spawnAreasList[j], _spawnAreasList[i]);
            }
        }

        private static int GetRandomIntInclusive(int minInclusive, int maxInclusive)
        {
            if (maxInclusive < minInclusive)
            {
                (minInclusive, maxInclusive) = (maxInclusive, minInclusive);
            }

            // Size of the range, e.g. [2, 5] -> rangeSize = 4 (2,3,4,5).
            int rangeSize = maxInclusive - minInclusive + 1;

            // GD.Randi() returns an unsigned 32-bit integer (uint).
            // We map that huge range down into [0, rangeSize - 1] using modulo,
            // then shift it into [minInclusive, maxInclusive] by adding minInclusive.
            uint randomValue = GD.Randi();
            int offset = (int)(randomValue % (uint)rangeSize);

            return minInclusive + offset;
        }
    }
}
