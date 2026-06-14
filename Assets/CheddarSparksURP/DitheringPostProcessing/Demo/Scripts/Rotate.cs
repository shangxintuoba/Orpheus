using UnityEngine;

namespace CheddarSparks.CustomDitheringPostProcessing.Demo
{
    public class Rotate : MonoBehaviour
    {
        [SerializeField] 
        private Vector3 rotationSpeed = new Vector3(0f, 30f, 0f);

        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
