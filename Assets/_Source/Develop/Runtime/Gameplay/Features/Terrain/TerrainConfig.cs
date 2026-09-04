using UnityEngine;

namespace _Source.Develop.Runtime.Gameplay.Features.Terrain
{
    [CreateAssetMenu(fileName = "TerrainConfig", menuName = "Terrain/TerrainConfig")]
    public class TerrainConfig : ScriptableObject
    {
        public int chunkWidth = 16;
        public int chunkHeight = 128;
    }
}
