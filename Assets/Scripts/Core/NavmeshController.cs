using Unity.AI.Navigation;
using UnityEngine;
using Global;
namespace Core
{
    public class NavmeshController : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface _navMeshSurface;

        private void Awake() 
        {
            GlobalEvents.OnDungeonGeneratedSuccess += BuildNavmesh;
        }

        private void BuildNavmesh(int seed)
        {
            _navMeshSurface.BuildNavMesh();
        }

        private void OnDestroy()
        {
            GlobalEvents.OnDungeonGeneratedSuccess -= BuildNavmesh;
        }
    }
}