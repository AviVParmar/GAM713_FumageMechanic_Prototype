// CanvasGyro.cs
// GAM713 Prototype 1
//
// Created by Avi Virendra Parmar
// On 1st February 2025
//

using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasGyro : MonoBehaviour
{
    #region Variables
    InputSystem_Actions inputSystem;
    InputAction gyroscope;
    InputAction accelerometer;
    InputAction touchTap;


    [Header("Controller Settings")]
    [SerializeField] private bool gyroFunctionality = true;
    [SerializeField] private float gyroSpeed = 1f;
    [SerializeField] private bool accelorometerFunctionality = true;
    [SerializeField] private float accelorometerSpeed = 1f;
    [SerializeField] private float filterFactor = 0.1f;
    Vector3 smoothAcceleration;
    Vector3 baselineAcceleration = Vector3.zero;
    #endregion
    
    #region Awake, Enable, Disable
    private void Awake() {
        inputSystem = new InputSystem_Actions();
    }

    private void OnEnable() {
        gyroscope = inputSystem.Player.Gyro;
        gyroscope.Enable();

        accelerometer = inputSystem.Player.Accelorometer;
        accelerometer.Enable();

        touchTap = inputSystem.Player.TouchTap;
        touchTap.Enable();
        touchTap.performed += OnTap;
    }

    private void OnDisable() {
        gyroscope.Disable();

        accelerometer.Disable();

        touchTap.performed -= OnTap;
        touchTap.Disable();
    }
    #endregion

    #region Start, Update, OnTap
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baselineAcceleration = accelerometer.ReadValue<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gyroFunctionality)
        GetGyroData();

        if (accelorometerFunctionality)
        GetAccelorometerData();
    }
    
    void OnTap(InputAction.CallbackContext context) {
        ResetController();
    }
    #endregion



    #region Functions
    bool CheckDevice() {
        if (UnityEngine.InputSystem.Gyroscope.current != null && UnityEngine.InputSystem.Accelerometer.current != null) {
            return true;
        }
        else {
            //Debug.Log("Gyro or Accelorometer is not supported on this device");
            return false;
        }
    }

    void GetGyroData() {
        //Check if gyro functionality exists
        if (CheckDevice()) {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
        }

        //Rotate object based on gyro data
        if (Input.gyro.enabled) {
            Vector3 rotation = inputSystem.Player.Gyro.ReadValue<Vector3>() * gyroSpeed;
            transform.Rotate(rotation.x, rotation.y, -rotation.z);
        }
    }

    void GetAccelorometerData() {
        if (CheckDevice()) {
            InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
        }

        Vector3 acceleration = accelerometer.ReadValue<Vector3>() - baselineAcceleration;
        
        smoothAcceleration = Vector3.Lerp(smoothAcceleration, acceleration, filterFactor);

        Debug.Log(smoothAcceleration);
            
        //move object based on accelerometer input
        transform.Translate(smoothAcceleration.x * accelorometerSpeed, 0, smoothAcceleration.z * accelorometerSpeed);
    }

    public void ResetController() {
        // Reset Gyro
        transform.rotation = Quaternion.Euler(0, 0, 180f);
        transform.position = Vector3.zero;

        // Reset Accelerometer
        baselineAcceleration = accelerometer.ReadValue<Vector3>(); 
    }
    #endregion
}

