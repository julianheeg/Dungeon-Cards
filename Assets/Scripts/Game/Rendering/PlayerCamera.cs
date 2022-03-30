using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a script that controls the camera perspective and also casts rays onto objects at queried positions
/// </summary>
public class PlayerCamera : MonoBehaviour {

    public float minXBound, minZBound, maxXBound, maxZBound;    //a rectangle that limits the range of this camera
    public float scrollSpeed;
    public int mapScrollBoundary;   //how many pixels away from the screen boundary should the mouse trigger the camera movement

    private int screenWidth, screenHeight;

	// Use this for initialization
	void Start () {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
	}

    /// <summary>
    /// translate the camera according to the inputs
    /// </summary>
    void LateUpdate () {
        //get inputs
        float xAxisValue = Input.GetAxis("Horizontal");
        float zAxisValue = Input.GetAxis("Vertical"); //negative down, positive up
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        int mouseX = (int)Input.mousePosition.x;
        int mouseY = (int)Input.mousePosition.y;

        if (Camera.current != null)
        {
            //current camera position
            float currentX = Camera.current.transform.position.x;
            float currentZ = Camera.current.transform.position.z;

            //arrow keys and wasd, override cursor currently
            float xTranslation = ((currentX > minXBound || xAxisValue > 0f) && (currentX < maxXBound || xAxisValue < 0f)) ? xAxisValue * Time.deltaTime : 0f;
            float zTranslation = ((currentZ > minZBound || zAxisValue > 0f) && (currentZ < maxZBound || zAxisValue < 0f)) ? zAxisValue * Time.deltaTime: 0f;

            //cursor at the edges
            if (mouseX < mapScrollBoundary && currentX > minXBound && xTranslation == 0f)
                xTranslation = -1 * Time.deltaTime;
            else if (mouseX >= screenWidth - mapScrollBoundary && currentX < maxXBound && xTranslation == 0f)
                xTranslation = 1 * Time.deltaTime;
            if (mouseY < mapScrollBoundary && currentZ > minZBound && zTranslation == 0f)
                zTranslation = -1 * Time.deltaTime;
            else if (mouseY > screenHeight - mapScrollBoundary && currentZ < maxZBound && zTranslation == 0f)
                zTranslation = 1 * Time.deltaTime;

            //translate in x and z direction
            Camera.current.transform.Translate(new Vector3(xTranslation, 0.0f, zTranslation), Space.World);

            //zoom in/out
            Camera.current.transform.Translate(new Vector3(0, 0, scrollValue * scrollSpeed * Time.deltaTime), Space.Self);
        }
    }

    /// <summary>
    /// casts a ray from the screenpoint onto the plane with y==Config.raycastIntersectionPlaneY and returns the intersection point
    /// </summary>
    /// <param name="screenPoint">the screenpoint from where to raycast</param>
    /// <returns>the intersection with the plane</returns>
    static public Vector3 GetRaycastIntersection(Vector2 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        Vector3 basePoint = ray.origin;
        Vector3 direction = ray.direction;
        
        float lambda = (Config.raycastIntersectionPlaneY - ray.origin.y) / ray.direction.y;

        return basePoint + lambda * direction; 
    }

    /// <summary>
    /// casts a ray from the mouse position onto the map
    /// </summary>
    /// <param name="screenPoint">the mouse position on screen</param>
    /// <param name="hit">the hit object</param>
    /// <param name="layermask">a layermask. Pass -1 if not applicable</param>
    /// <returns>true if the raycast hit anything</returns>
    static public bool RaycastOntoMap(Vector2 screenPoint, out RaycastHit hit, int layermask)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        return Physics.Raycast(ray, out hit, Mathf.Infinity, layermask);
    }

    /// <summary>
    /// casts a ray from the mouse position onto the map, but returns everything that has been hit (subject to the layermask)
    /// </summary>
    /// <param name="screenPoint">the mouse position on screen</param>
    /// <param name="layermask">a layermask. Pass -1 if not applicable</param>
    /// <returns>an array of RaycastHits</returns>
    static public RaycastHit[] RaycastOntoMap(Vector2 screenPoint, int layermask)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        if (layermask == -1)
        {
            return Physics.RaycastAll(ray);
        }
        else
        {
            return Physics.RaycastAll(ray, layermask);
        }
    }
}