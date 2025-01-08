using Cinemachine;
using UnityEngine;

namespace Burk
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] CinemachineVirtualCamera cam;
        CinemachineOrbitalTransposer _orbitalTransposer;
        [SerializeField] CinemachineFollowZoom followZoom;
        [SerializeField] float rotateSpeed = 1f;

        [SerializeField] Vector2 zoomMinMax;


        public void Start()
        {
            _orbitalTransposer = cam.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        }
        public void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _orbitalTransposer.m_XAxis.m_MaxSpeed = rotateSpeed;
            }

            if (Input.GetMouseButtonUp(1))
            {
                _orbitalTransposer.m_XAxis.m_MaxSpeed = 0;
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                followZoom.m_Width -= Input.mouseScrollDelta.y;
                followZoom.m_Width = Mathf.Clamp(followZoom.m_Width, zoomMinMax.x, zoomMinMax.y);
            }
        }
    }
}
