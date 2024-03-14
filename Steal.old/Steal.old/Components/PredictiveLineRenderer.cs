
namespace Steal.Components
{
    using UnityEngine;

    public class PredictFuturePosition : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public float predictionTime = 80f;
        public int lineSegments = 2;

        public Vector3 Velocity;
        public Vector3 Position;

        void Start()
        {
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.GetComponent<LineRenderer>();
                lineRenderer.positionCount = lineSegments;
            }
        }

        void Update()
        {
            DrawPredictiveLine();
        }

        void DrawPredictiveLine()
        {
            Vector3[] linePositions = new Vector3[lineSegments];

            Vector3 currentPosition = Position;
            Vector3 currentVelocity = Velocity;

            for (int i = 0; i < lineSegments; i++)
            {
                float time = i * (predictionTime / lineSegments);
                linePositions[i] = currentPosition + currentVelocity * time + 0.5f * Physics.gravity * time * time;
            }

            lineRenderer.SetPositions(linePositions);
        }
    }
}