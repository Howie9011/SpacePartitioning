using System.Collections.Generic;
using UnityEngine;

public class KDTreeTest : MonoBehaviour
{
    public List<Transform> TransPoints;
    private KDTree _kdTree;
    public Transform _target;
    private GameObject _nearestGo;
    void Start()
    {
        if (TransPoints.Count > 0)
        {
            _kdTree = new KDTree(2, TransPoints);
        }
        PrintTree(_kdTree.RootNode);
    }

    private void PrintTree(KDTreeNode node)
    {
        if (node == null)
            return;
        PrintTree(node.LeftNode);
        PrintTree(node.RightNode);
    }
    void Update()
    {
        if (_target != null && _kdTree != null)
        {
            if (_kdTree.Search(_target.position, out var trans, out var length))
            {
                if (trans != null)
                {
                    if (_nearestGo != null)
                    {
                        SetGameObjectColor(_nearestGo,Color.white);
                    }
                    SetGameObjectColor(trans.gameObject,Color.red);
                    _nearestGo = trans.gameObject;
                }
            }
        }
    }

    private void SetGameObjectColor(GameObject go,Color color)
    {
        var meshRender = go.GetComponent<MeshRenderer>();
        if (meshRender != null)
        {
            meshRender.material.color = color;
        }
    }
}
