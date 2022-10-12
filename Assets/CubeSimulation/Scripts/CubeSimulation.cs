
using UnityEngine;

public class CubeSimulation : Simulation
{
    [SerializeField] private GameObject cubePrefab;
    private Transform cube;

    public bool cubeIsSpinning;

    private void Start()
    {
        // Create a cube
        if (cubePrefab)
        {
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Transform parent = transform;
            cube = Instantiate(cubePrefab, position, rotation, parent).GetComponent<Transform>();
        }
    }

    private void Update()
    {
        if (!cube || !cubeIsSpinning) { return; }

        // Rotate the cube
        Vector3 rotationAxis = (Vector3.right + Vector3.up).normalized;
        cube.Rotate(rotationAxis, 10 * Time.deltaTime);
    }
}
