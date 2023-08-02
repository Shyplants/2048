using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPool : MonoBehaviour
{
    private class PoolItem
    {
        public bool isActive;
        public GameObject gameObject;
    }
    private int maxCount;
    private int activeCount;

    private GameObject poolObject;
    private Transform poolObjectParent;
    private List<PoolItem> poolItemList;

    public MemoryPool(GameObject poolObject, Transform poolObjectParent, int n)
    {
        maxCount = n*n;
        activeCount = 0;
        this.poolObject = poolObject;
        this.poolObjectParent = poolObjectParent;

        poolItemList = new List<PoolItem>();

        InstantiateObjects();
    }

    public void InstantiateObjects()
    {
        for(int i=0; i<maxCount; ++i)
        {
            PoolItem poolItem = new PoolItem();

            poolItem.isActive = false;
            poolItem.gameObject = GameObject.Instantiate(poolObject, poolObjectParent);
            poolItem.gameObject.SetActive(false);

            poolItemList.Add(poolItem);
        }
    }

    public void DestoryObjects()
    {
        if(poolItemList == null) return;

        int count = poolItemList.Count;
        for(int i=0; i<count; ++i)
        {
            GameObject.Destroy(poolItemList[i].gameObject);
        }

        poolItemList.Clear();
    }

    public GameObject ActivePoolItem()
    {
        if(poolItemList == null) return null;

        int count = poolItemList.Count;
        for(int i=0; i<count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(!poolItem.isActive)
            {
                activeCount++;

                poolItem.isActive = true;
                poolItem.gameObject.SetActive(true);

                return poolItem.gameObject;
            }
        }

        return null;
    }
    public GameObject ActivePoolItem(GameObject poolObject)
    {
        if(poolItemList == null || poolObject == null) return null;

        int count = poolItemList.Count;
        for(int i=0; i<count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.gameObject == poolObject)
            {
                activeCount++;

                poolItem.isActive = true;
                poolItem.gameObject.SetActive(true);

                return poolItem.gameObject;
            }
        }

        return null;
    }

    public void DeactivatePoolItem(GameObject removeObject)
    {
        if(poolItemList == null || removeObject == null) return;

        int count = poolItemList.Count;
        for(int i=0; i<count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.gameObject == removeObject)
            {
                activeCount--;

                poolItem.isActive = false;
                poolItem.gameObject.SetActive(false);

                return;
            }
        }
    }

    public void DeactivatePoolItems()
    {
        if(poolItemList == null) return;

        int count = poolItemList.Count;
        for(int i=0; i<count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            activeCount--;
            poolItem.isActive = false;
            poolItem.gameObject.SetActive(false);
        }
    }

    public bool IsActivePoolItem(GameObject poolObject)
    {
        if(poolItemList == null || poolObject == null) return false;

        int count = poolItemList.Count;
        for(int i=0; i<count; ++i)
        {
            PoolItem poolItem = poolItemList[i];

            if(poolItem.gameObject == poolObject)
            {
                return poolItem.isActive;
            }
        }

        return false;
    }
}
