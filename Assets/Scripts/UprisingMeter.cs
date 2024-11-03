using UnityEngine;

public class UprisingMeter : MonoBehaviour
{
    private static UprisingMeter _instance;
    private static int meterLevel = 10;

    public static UprisingMeter Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<UprisingMeter>();

                if(_instance == null)
                {
                    GameObject uprisingMeter = new GameObject(typeof(UprisingMeter).Name);
                    _instance = uprisingMeter.AddComponent<UprisingMeter>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int GetMeterLevel()
    {
        return meterLevel;
    }

    public void AddMeterLevel(int amount)
    {
        meterLevel += amount;
    }

    public void SubMeterLevel(int amount)
    {
        meterLevel -= amount;
    }
}
