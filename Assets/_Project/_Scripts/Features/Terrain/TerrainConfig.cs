using UnityEngine;

namespace _Project._Scripts.Features.Terrain
{
    [CreateAssetMenu(fileName = "TerrainConfig", menuName = "Terrain/TerrainConfig")]
    public class TerrainConfig : ScriptableObject
    {
        public int chunkWidth = 16;
        public int chunkHeight = 128;
    }
}