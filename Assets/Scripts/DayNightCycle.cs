using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayLengthMinutes;
    public float nightLengthMinutes;
    float rotationSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isDay(transform.rotation.eulerAngles))
        {
            rotationSpeed = 360 / dayLengthMinutes / 60;
        }
        else
        {
            rotationSpeed = 360 / nightLengthMinutes / 60;
        }

        transform.Rotate(new Vector3(1, 0, 0) * rotationSpeed * Time.deltaTime);

    }

    bool isDay(Vector3 pos)
    {
        if(pos.x > 0 && pos.x < 180)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
