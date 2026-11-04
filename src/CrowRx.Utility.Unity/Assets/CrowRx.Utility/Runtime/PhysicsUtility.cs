using System.Collections.Generic;
using UnityEngine;
using Unity.Scripting.LifecycleManagement;

namespace CrowRx.Utility
{
    public static class PhysicsUtility
    {
        [NoAutoStaticsCleanup] private static Dictionary<int, int> _masksByLayer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitMasksByLayer()
        {
            _masksByLayer = new Dictionary<int, int>();

            for (int i = 0; i < 32; i++)
            {
                int mask = 0;
                for (int j = 0; j < 32; j++)
                {
                    if (!Physics.GetIgnoreLayerCollision(i, j))
                    {
                        mask |= 1 << j;
                    }
                }

                _masksByLayer.Add(i, mask);
            }
        }

        public static int MaskForLayer(int layer) => _masksByLayer.GetValueOrDefault(layer, 0);
    }
}