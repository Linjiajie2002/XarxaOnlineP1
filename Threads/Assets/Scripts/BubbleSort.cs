using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class BubbleSort : MonoBehaviour
{
    float[] array;
    List<GameObject> mainObjects;
    public GameObject prefab;
    private Thread sortingThread;
    private bool arrayChanged = false;

    void Start()
    {
        mainObjects = new List<GameObject>();
        array = new float[300];

        for (int i = 0; i < array.Length; i++)
        {
            array[i] = Random.Range(0, 1000) / 100f;
        }

        logArray();
        spawnObjs();

        //TO DO 4 
        //Call the three previous functions in order to set up the exercise 


        //TO DO 5 
        //Create a new thread using the function "bubbleSort" and start it.
        sortingThread = new Thread(bubbleSort);
        sortingThread.Start();
    }

    void Update()
    {
        //TO DO 6
        //Call ChangeHeights() in order to update our object list.
        //Since we'll be calling UnityEngine functions to retrieve and change some data,
        //we can't call this function inside a Thread

        if (updateHeights() == false && sortingThread != null && !sortingThread.IsAlive)
        {
            sortingThread = null;
        }


    }

    //TO DO 5
    //Create a new thread using the function "bubbleSort" and start it.
    void bubbleSort()
    {
        int n = array.Length;
        bool swapped;

        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    swapped = true;
                    arrayChanged = true; 
                }
            }
            if (!swapped)
                break;
        }
    }

    void logArray()
    {
        string text = "";

        //TO DO 1
        //Simply show in the console what's inside our array.
        for (int i = 0; i < array.Length; i++)
        {
            Debug.Log(array[i]);
        }


        //Debug.Log(text);
    }

    void spawnObjs()
    {
        //TO DO 2
        //We should be storing our objects in a list so we can access them later on.

        for (int i = 0; i < array.Length; i++)
        {
            //We have to separate the objs accordingly to their width, in which case we divide their position by 1000.
            //If you decide to make your objs wider, don't forget to up this value

            mainObjects.Add(
            Instantiate(prefab, new Vector3((float)i / 10,
                this.gameObject.GetComponent<Transform>().position.y, 0), Quaternion.identity));
        }

    }

    //TO DO 3
    //We'll just change the height of every obj in our list to match the values of the array.
    //To avoid calling this function once everything is sorted, keep track of new changes to the list.
    //If there weren't, you might as well stop calling this function

    bool updateHeights()
    {

        bool changed = arrayChanged;
        arrayChanged = false;
        for (int i = 0; i < mainObjects.Count; i++)
        {
            if (mainObjects[i] != null)
            {
                Vector3 scale = mainObjects[i].transform.localScale;
                scale.y = array[i];
                mainObjects[i].transform.localScale = scale;
            }
        }
        return changed;
    }

    private void OnApplicationQuit()
    {
        if (sortingThread != null && sortingThread.IsAlive)
        {
            sortingThread.Abort(); 
        }
    }
}
