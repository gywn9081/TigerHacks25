// using UnityEngine;

// public class DayNightCycle : MonoBehaviour
// {
//     public float dayLengthMinutes = 10f;
//     public float nightLengthMinutes = 5f;
//     private float rotationSpeed;
//     public int cycleState = 1;
//     public GameObject playerCamera;   // Assign the player's camera in the Inspector
//     public GameObject sunSphere;      // Assign the sun sphere in the Inspector
//     public Light directionalLight;    // Assign the directional light representing the sun
//     public float sunDistance = 10f;   // Distance from player to sun
//     public float maxSunScale = 10f;    // Max scale factor for sun size
//     public float minSunScale = 1f;    // Min scale factor for sun size
//     private float currentSunScale;    // Current sun scale
//     private int strob = 1;            // Scaling direction toggle

//     void Start()
//     {
//         SetRotationSpeed();
//         currentSunScale = minSunScale; // Start with the minimum scale
//     }

//     // void Update()
//     // {
//     //     if (cycleState == 1)
//     //     {
//     //         // Update rotation speed if day/night length changes
//     //         SetRotationSpeed();

//     //         // Rotate the directional light to simulate day-night cycle
//     //         directionalLight.transform.Rotate(new Vector3(1, 0, 0) * rotationSpeed * Time.deltaTime);

//     //         // Update sun sphere position and scale to match the directional light
//     //         UpdateSunSpherePosition();
//     //     }
//     //     else if (cycleState == 2)
//     //     {
//     //         InflateSun();
//     //         // Position sun sphere in front of the player to simulate close proximity
//     //         sunSphere.transform.position = playerCamera.transform.position + playerCamera.transform.forward * sunDistance;
//     //     } else if (cycleState == 3){
//     //         dayLengthMinutes = 0.000001f;
//     //         nightLengthMinutes = 0.000005f;
//     //         cycleState = 1;
//     //     }
//     // }

// void Update()
// {
//     if (cycleState == 1)
//     {
//         // Lerp between day and night lengths over time
//         elapsedTime += Time.deltaTime; // Increment elapsed time
//         float t = Mathf.Clamp01(elapsedTime / lerpDuration); // Calculate t between 0 and 1

//         // Interpolating both values
//         currentDayLength = Mathf.Lerp(0.000001f, 0.000005f, t);
//         currentNightLength = Mathf.Lerp(0.000005f, 0.000001f, t);

//         // Update rotation speed based on the interpolated lengths
//         rotationSpeed = IsDay() ? 360 / currentDayLength / 60 : 360 / currentNightLength / 60;

//         // Rotate the directional light to simulate day-night cycle
//         directionalLight.transform.Rotate(new Vector3(1, 0, 0) * rotationSpeed * Time.deltaTime);

//         // Update sun sphere position and scale to match the directional light
//         UpdateSunSpherePosition();

//         // Reset elapsed time after a complete cycle
//         if (t >= 1f)
//         {
//             elapsedTime = 0f; // Reset for the next cycle
//         }
//     }
//     else if (cycleState == 2)
//     {
//         InflateSun();
//         // Position sun sphere in front of the player to simulate close proximity
//         sunSphere.transform.position = playerCamera.transform.position + playerCamera.transform.forward * sunDistance;
//     }
//     else if (cycleState == 3)
//     {
//         // Initialize day and night lengths and reset cycle state
//         elapsedTime = 0f; // Reset elapsed time for next transition
//         cycleState = 1; // Switch back to cycle state 1 to start Lerp
//     }
// }


//     void SetRotationSpeed()
//     {
//         if (IsDay())
//         {
//             rotationSpeed = 360 / dayLengthMinutes / 60;
//         }
//         else
//         {
//             rotationSpeed = 360 / nightLengthMinutes / 60;
//         }
//     }

//     void UpdateSunSpherePosition()
//     {
//         // Position the sun sphere at a fixed distance in the direction of the directional light
//         Vector3 sunDirection = -directionalLight.transform.forward; // Use forward vector for placement
//         sunSphere.transform.position = playerCamera.transform.position + sunDirection * sunDistance;
//     }

//     void InflateSun()
//     {
//         // Adjust scale based on strob direction and scalingSpeed
//         currentSunScale += strob * scalingSpeed * Time.deltaTime;


//         if (currentSunScale >= maxSunScale)
//         {
//             cycleState = 0;
//             dayLengthMinutes = 1000000f;
//             nightLengthMinutes = 5000000f;
//             sunSphere.transform.position = new Vector3(0, -1000, 0);
//         }

//         // Apply the updated scale to the sun sphere
//         sunSphere.transform.localScale = Vector3.one * currentSunScale;
//     }

//     bool IsDay()
//     {
//         float sunAngleX = directionalLight.transform.rotation.eulerAngles.x;
//         return sunAngleX > 0 && sunAngleX < 180;
//     }
// }


using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    // Public variables for configuring the day/night cycle
    public float dayLengthMinutes = 0.000001f; // Initial day length
    public float nightLengthMinutes = 0.000005f; // Initial night length
    public float lerpDuration = 5f; // Duration to interpolate between day and night lengths

    // Private variables
    private float rotationSpeed;            // Speed at which the light rotates
    private float elapsedTime = 0f;         // Tracks elapsed time for Lerp
    private float currentDayLength;         // Current interpolated day length
    private float currentNightLength;       // Current interpolated night length

    public int cycleState = 1;              // Current state of the cycle (1 for normal, 2 for close sun)
    public GameObject playerCamera;         // Assign the player's camera in the Inspector
    public GameObject sunSphere;            // Assign the sun sphere in the Inspector
    public Light directionalLight;          // Assign the directional light representing the sun
    public float sunDistance = 10f;         // Distance from player to sun
    public float maxSunScale = 5f;          // Max scale factor for sun size
    public float minSunScale = 1f;          // Min scale factor for sun size
    public float scalingSpeed = 3f;   // Speed of scale change

    private float speed = 1.5f;                  // Scaling direction toggle
    int strob = -1;
    private float currentSunScale;           // Current scale of the sun sphere

    void Start()
    {
        // Initialize current lengths with the start values
        currentDayLength = dayLengthMinutes; 
        currentNightLength = nightLengthMinutes; 
        currentSunScale = minSunScale; // Start with the minimum scale
    }

void Update()
{
    if (cycleState == 1) {
        // Lerp between day and night lengths over time
            elapsedTime += Time.deltaTime; // Increment elapsed time
            float t = Mathf.Clamp01(elapsedTime / lerpDuration); // Calculate t between 0 and 1

            // Apply ease-out function to t (e.g., sqrt(t) for square root ease-out)
            float easeOutT = Mathf.Pow(t, 1.1f); // Adjust for more/less easing as needed

            // Interpolating both values with eased t
            currentDayLength = Mathf.Lerp(dayLengthMinutes, nightLengthMinutes, easeOutT);
            currentNightLength = Mathf.Lerp(nightLengthMinutes, dayLengthMinutes, easeOutT);

            // Clamp to avoid extreme values
            currentDayLength = Mathf.Max(currentDayLength, 0.1f); // Clamp to minimum
            currentNightLength = Mathf.Max(currentNightLength, 0.1f); // Clamp to minimum

            // Update rotation speed based on the interpolated lengths
            rotationSpeed = IsDay() ? 360 / currentDayLength / 60 : 360 / currentNightLength / 60;

            // Rotate the directional light to simulate day-night cycle
            directionalLight.transform.Rotate(new Vector3(1, 0, 0) * rotationSpeed * Time.deltaTime);

            // Update sun sphere position and scale to match the directional light
            UpdateSunSpherePosition();

            // Reset elapsed time after a complete cycle
            if (t >= 1f)
            {
                elapsedTime = 0f; // Reset for the next cycle
            }
    }
    if (cycleState == 3)
    {
        speed *= 1.5f / Time.deltaTime;

        Debug.Log(currentNightLength);

        // Update rotation speed based on the interpolated lengths
        rotationSpeed = IsDay() ? (360 / currentDayLength / 60) * speed : (360 / currentNightLength / 60) * speed;
        rotationSpeed = Mathf.Clamp(rotationSpeed, 1.1f, 10000);
        // rotationSpeed = 360 / currentDayLength / 60;

        // Rotate the directional light to simulate day-night cycle
        directionalLight.transform.Rotate(new Vector3(1, 0, 0) * rotationSpeed * Time.deltaTime);

        // Update sun sphere position and scale to match the directional light
        UpdateSunSpherePosition();
    }
    else if (cycleState == 2)
    {
        InflateSun();
        // Position sun sphere in front of the player to simulate close proximity
        sunSphere.transform.position = playerCamera.transform.position + playerCamera.transform.forward * sunDistance;
    }
}


    void UpdateSunSpherePosition()
    {
        // Position the sun sphere at a fixed distance in the direction of the directional light
        Vector3 sunDirection = -directionalLight.transform.forward; // Use forward vector for placement
        sunSphere.transform.position = playerCamera.transform.position + sunDirection * sunDistance;
        sunSphere.transform.localScale = Vector3.one * currentSunScale; // Update scale of sun sphere
    }

    // void InflateSun()
    // {
    //     // Smoothly scale the sun using Lerp (you can modify speed factor as needed)
    //     sun.transform.localScale = Vector3.Lerp(sun.transform.localScale, Vector3.one * sunScale, Time.deltaTime * 2f);
    // }

    void InflateSun()
    {
        // Adjust scale based on strob direction and scalingSpeed
        currentSunScale += strob * scalingSpeed * Time.deltaTime;


        if (currentSunScale >= maxSunScale)
        {
            cycleState = 0;
            dayLengthMinutes = 1000000f;
            nightLengthMinutes = 5000000f;
            sunSphere.transform.position = new Vector3(0, -1000, 0);
        }

        // Apply the updated scale to the sun sphere
        sunSphere.transform.localScale = Vector3.one * currentSunScale;
    }

    bool IsDay()
    {
        float sunAngleX = directionalLight.transform.rotation.eulerAngles.x;
        return sunAngleX > 0 && sunAngleX < 180; // Returns true if the sun is in the sky
    }
}