using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FVoxel {
    public class VoxelBrushLibrary : MonoBehaviour {
        private static VoxelBrushLibrary _instance;
        public static VoxelBrushLibrary Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            brushLookup = new Dictionary<string, VoxelPaintBrush>();
            foreach (var entry in brushes)
            {
                brushLookup[entry.key] = entry.brush;
            }
        }

        [System.Serializable]
        public struct BrushSet
        {
            public string key;
            public VoxelPaintBrush brush;
        }

        public List<BrushSet> brushes;
        private Dictionary<string, VoxelPaintBrush> brushLookup;

        /// <summary>
        /// Get voxel brush by a given key.
        /// </summary>
        public VoxelPaintBrush this[string key]
        {
            get
            {
                VoxelPaintBrush brush;
                if (brushLookup.TryGetValue(key, out brush))
                {
                    return brush;
                }
                else
                {
                    throw new KeyNotFoundException("Voxel brush not found in library:" + key);
                }
            }
        }

        public static VoxelPaintBrush GetBrush(string key)
        {
            return Instance[key];
        }
    }
}