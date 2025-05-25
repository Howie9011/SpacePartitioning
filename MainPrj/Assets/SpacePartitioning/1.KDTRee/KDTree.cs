/*
 * KDTree 适合静态点的划分
 * 确定划分轴 排序 确定左右子树
 */
using System.Collections.Generic;
using UnityEngine;


public enum SplitAxis
{
    X = 0,
    Y,
    Z,
}

public class KDTreeNode
{
    public Transform Trans;
    public KDTreeNode Root;
    public KDTreeNode LeftNode;
    public KDTreeNode RightNode;
    public int SplitAxis;
}

public class KDTree
{
    public KDTreeNode RootNode;
    private List<Transform> _trans;
    private int _k;

    public KDTree(int k, List<Transform> trans)
    {
        _k = k;
        _trans = trans;
        RootNode = BuildTree(_trans, 0);
    }

    /// <summary>
    /// 确定初始划分轴 方差计算
    /// </summary>
    /// <returns></returns>
    private int GetInitAxis()
    {
        if (_k == 1)
        {
            return (int)SplitAxis.X;
        }
        
        float maxVariance = float.MinValue;
        int bestAxis = 0;

        for (int axis = 0; axis < _k; axis++)
        {
            var points = new List<float>();
            foreach (var tran in _trans)
            {
                points.Add(tran.position[axis]);
            }

            float variance = GetVariance(points);
            if (variance > maxVariance)
            {
                maxVariance = variance;
                bestAxis = axis;
            }
        }
        return bestAxis;
    }

    /// <summary>
    /// 求方差
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    private float GetVariance(List<float> points)
    {
        //1.求平均值
        int count = points.Count;
        float sum = 0;
        for (int i = 0; i < count; i++)
        {
            sum += points[i];
        }

        var avgValue = sum / count;

        float resultSum = 0;
        for (int i = 0; i < count; i++)
        {
            var sub = points[i] - avgValue;
            resultSum += (sub * sub);
        }

        return resultSum / count;
    }

    /// <summary>
    /// 获取分割轴
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    private int GetSplitAxis(int depth)
    {
        int initAxis = GetInitAxis();
        return (initAxis + depth) % _k;
    }

    /// <summary>
    /// 创建KD Tree
    /// </summary>
    /// <param name="points"></param>
    /// <param name="depth"></param>
    /// <returns></returns>
    private KDTreeNode BuildTree(List<Transform> trans, int depth)
    {
        if (trans.Count <= 0)
            return null;
        
        
        var splitAxis = GetSplitAxis(depth);
        //排序
        trans.Sort((x, y) =>
        {
            if (x.position[splitAxis] > y.position[splitAxis])
                return 1;
            return -1;
        });
        int middleIndex = trans.Count / 2;
        KDTreeNode node = new KDTreeNode
        {
            Trans = trans[middleIndex],
            SplitAxis = splitAxis,
        };

        var leftPoints = new List<Transform>();
        for (int i = 0; i < middleIndex; i++)
        {
            leftPoints.Add(trans[i]);
        }

        var rightPoint = new List<Transform>();

        for (int i = middleIndex + 1; i < trans.Count; i++)
        {
            rightPoint.Add(trans[i]);
        }

        node.LeftNode = BuildTree(leftPoints, depth + 1);
        if (node.LeftNode != null)
        {
            node.LeftNode.Root = node;
        }

        node.RightNode = BuildTree(rightPoint, depth + 1);
        if (node.RightNode != null)
        {
            node.RightNode.Root = node;
        }
        return node;
    }

    /// <summary>
    /// 查找离目标点最近的点坐标
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public bool Search(Vector3 targetPos,out Transform trans ,out float length)
    {
        length = 0;
        KDTreeNode bestNode = null;
        trans = null;
        var bestLength = float.MaxValue;
        SearchNearestRecursive(RootNode,targetPos,ref  bestNode,ref bestLength);
        if (bestNode == null)
            return false;

        trans = bestNode.Trans;
        length = bestLength;
        return true;
    }

    private void SearchNearestRecursive(KDTreeNode node, Vector3 targetPos, ref KDTreeNode bestNode,
        ref float bestSqrDist)
    {
        if(node == null)
            return;

        float currentSqrDist = (node.Trans.position - targetPos).sqrMagnitude;
        if (currentSqrDist < bestSqrDist)
        {
            bestSqrDist = currentSqrDist;
            bestNode = node;
        }
        
        var axis = node.SplitAxis;
        var targetValue = targetPos[axis];
        var value = node.Trans.position[axis];

        KDTreeNode firstNode = null;
        KDTreeNode secondNode = null;

        if (targetValue > value)
        {
            firstNode = node.RightNode;
            secondNode = node.LeftNode;
        }
        else
        {
            firstNode = node.LeftNode;
            secondNode = node.RightNode;
        }
        //优先搜索所在node
        SearchNearestRecursive(firstNode, targetPos, ref bestNode, ref bestSqrDist);
        
        float axisDist = targetValue - value;
        float sqrAxisDist = axisDist * axisDist;

        // 如果另一侧可能存在更近的点，则继续搜索
        if (sqrAxisDist < bestSqrDist)
        {
            SearchNearestRecursive(secondNode, targetPos, ref bestNode, ref bestSqrDist);
        }
    }
}