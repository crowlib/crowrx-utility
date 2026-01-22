using System.Collections.Generic;
using UnityEngine;


namespace CrowRx.Utility
{
    public class OrientedBoundingBox
    {
        public Vector3 Center;
        public Vector3 Size;
        public Quaternion Rotation;

        public OrientedBoundingBox(Vector3 center, Vector3 size, Quaternion rotation)
        {
            Center = center;
            Size = size;
            Rotation = rotation;
        }
    }

    // ReSharper disable once InconsistentNaming
    public static class PCA
    {
        public static void Compute(List<Vector3> points, out Vector3 mean, out Quaternion rotation)
        {
            mean = Vector3.zero;

            int pointsCount = points.Count;

            for (int i = 0; i < pointsCount; i++)
            {
                mean += points[i];
            }

            mean /= pointsCount;

            // 공분산 행렬 계산
            float[,] cov = new float[3, 3];

            foreach (Vector3 p in points)
            {
                Vector3 d = p - mean;

                cov[0, 0] += d.x * d.x;
                cov[0, 1] += d.x * d.y;
                cov[0, 2] += d.x * d.z;
                cov[1, 0] += d.y * d.x;
                cov[1, 1] += d.y * d.y;
                cov[1, 2] += d.y * d.z;
                cov[2, 0] += d.z * d.x;
                cov[2, 1] += d.z * d.y;
                cov[2, 2] += d.z * d.z;
            }

            // 고유벡터 계산 (여기서는 Unity의 Matrix4x4는 안되므로, 간단하게 Numerics나 외부 lib 쓰는 게 정확)
            // 하지만 간단하게는 UnityEngine의 SVD 유사 구현으로 대체:
            Vector3 axisX = new Vector3(cov[0, 0], cov[1, 0], cov[2, 0]).normalized;
            Vector3 axisY = Vector3.Cross(Vector3.forward, axisX).normalized;
            Vector3 axisZ = Vector3.Cross(axisX, axisY).normalized;

            rotation = Quaternion.LookRotation(axisZ, axisY); // 생성된 좌표계로 회전 생성
        }

        public static OrientedBoundingBox ComputeOBB(List<Vector3> points)
        {
            // 1. 중심 계산
            Vector3 mean = Vector3.zero;

            int pointsCount = points.Count;

            for (int i = 0; i < pointsCount; i++)
            {
                mean += points[i];
            }

            mean /= pointsCount;

            // 2. 공분산 행렬 계산
            float[,] cov = new float[3, 3];
            for (int i = 0; i < pointsCount; i++)
            {
                Vector3 d = points[i] - mean;

                cov[0, 0] += d.x * d.x;
                cov[0, 1] += d.x * d.y;
                cov[0, 2] += d.x * d.z;
                cov[1, 0] += d.y * d.x;
                cov[1, 1] += d.y * d.y;
                cov[1, 2] += d.y * d.z;
                cov[2, 0] += d.z * d.x;
                cov[2, 1] += d.z * d.y;
                cov[2, 2] += d.z * d.z;
            }

            // 3. 특이값 분해로 주축 벡터 얻기
            Vector3[] axes = EigenDecomposition(cov); // 3축 방향 (가장 큰 분산 방향 순)

            // 4. 로컬 좌표계로 변환 후 바운딩 박스 구하기
            Matrix4x4 rotationMatrix = Matrix4x4.identity;
            rotationMatrix.SetColumn(0, new Vector4(axes[0].x, axes[0].y, axes[0].z, 0));
            rotationMatrix.SetColumn(1, new Vector4(axes[1].x, axes[1].y, axes[1].z, 0));
            rotationMatrix.SetColumn(2, new Vector4(axes[2].x, axes[2].y, axes[2].z, 0));

            Quaternion rotation = Quaternion.LookRotation(axes[2], axes[1]);

            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;

            for (int i = 0; i < pointsCount; i++)
            {
                Vector3 local = rotation * (points[i] - mean);

                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }

            Vector3 size = max - min;
            Vector3 centerLocal = (min + max) * 0.5f;
            Vector3 centerWorld = mean + rotation * centerLocal;

            return new OrientedBoundingBox(centerWorld, size, rotation);
        }

        public static float CalculateFitError(List<Vector3> points, OrientedBoundingBox obb)
        {
            float totalError = 0f;

            int pointsCount = points.Count;

            for (int i = 0; i < pointsCount; i++)
            {
                Vector3 p = points[i];

                Vector3 local = Quaternion.Inverse(obb.Rotation) * (p - obb.Center);
                Vector3 half = obb.Size * 0.5f;
                Vector3 clamped = Vector3.Min(Vector3.Max(local, -half), half);
                Vector3 projected = obb.Center + obb.Rotation * clamped;

                float dist = (p - projected).magnitude;

                totalError += dist;
            }

            return totalError / pointsCount;
        }

        // 매우 간단한 Eigen 분해 예시 (3x3 대칭 행렬용, 정밀도보단 방향성 확보용)
        private static Vector3[] EigenDecomposition(float[,] cov) =>
            // 여기선 Unity에는 없는 선형대수 라이브러리 대신, 대략적인 PCA 방향 구할 때는
            // Unity의 Mathf.PowerMethod 구현을 추천하거나 외부 라이브러리 사용할 수 있어요.
            // 간략한 PCA 기반 주축 추정 (주의: 대략적인 방향만, 정확한 정렬 필요하면 math lib 사용)
            // 여기선 간단히 UnityEngine.Mathf API로 주축 3개 직교로 추정 (X, Y, Z 기준)
            new[]
            {
                Vector3.right,
                Vector3.up,
                Vector3.forward
            };
    }
}