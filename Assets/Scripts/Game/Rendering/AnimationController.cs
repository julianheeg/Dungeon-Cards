using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a script that queues and executes animations, for example card draw or monster movement
/// </summary>
public class AnimationController : MonoBehaviour {

    /// <summary>
    /// a class that represents a step in an animation. An instance specifies the game object, the next position and the next rotation in a series of these.
    /// </summary>
    public class EventAnimation
    {
        public GameObject animatedObject;
        public Vector3? nextPosition;
        public Quaternion? nextRotation;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="animatedObject">the object to animate</param>
        /// <param name="nextPosition">the next position</param>
        /// <param name="nextRotation">the next rotation</param>
        public EventAnimation(GameObject animatedObject, Vector3? nextPosition, Quaternion? nextRotation)
        {
            this.animatedObject = animatedObject;
            this.nextPosition = nextPosition;
            this.nextRotation = nextRotation;
        }
    }

    static Queue<EventAnimation[]> AnimationQueue; //a queue of all events that have been issued but not yet executed. Entries are arrays, so that animations can be executed in parallel

    /// <summary>
    /// initialization
    /// </summary>
    private void Awake()
    {
        AnimationQueue = new Queue<EventAnimation[]>();
        animationRunning = new bool[1];
    }

    /// <summary>
    /// enqueues a series of simultaneous animations for later execution
    /// </summary>
    /// <param name="animation">the animation to enqueue</param>
    static public void QueueAnimation(EventAnimation[] animation)
    {
        AnimationQueue.Enqueue(animation);
    }

    /// <summary>
    /// enqueues a single animation for later execution
    /// </summary>
    /// <param name="animation">the animation to enqueue</param>
    static public void QueueAnimation(EventAnimation animation)
    {
        AnimationQueue.Enqueue(new EventAnimation[] { animation });
    }


    #region Animation

    static bool[] animationRunning; //a boolean for every animation that has been issued in the last animation call. Until all of them are false (meaning that the animation has finished)
                                    //the update function blocks the execution of the next set of animations

    /// <summary>
    /// if no animation is running, the update function issues the next set of animations if there are any
    /// </summary>
    private void Update()
    {
        //check if the last animation has finished, if true get another one and start the animation
        if (!AnimationRunning() && AnimationQueue.Count > 0)
        {
            EventAnimation[] nextAnimations = AnimationQueue.Dequeue();

            animationRunning = new bool[nextAnimations.Length];

            for (int i = 0; i < nextAnimations.Length; i++)
            {
                {
                    StartCoroutine(Animate(nextAnimations[i], i));
                }
            }
        }
    }

    /// <summary>
    /// is an animation currently running?
    /// </summary>
    /// <returns>true iff an animation is currently being executed</returns>
    public static bool AnimationRunning()
    {
        bool running = false;
        foreach(bool b in animationRunning)
        {
            running |= b;
        }
        return running;
    }

    /// <summary>
    /// executes an animation
    /// </summary>
    /// <param name="animation">the animation to execute</param>
    /// <param name="index">the index of this animation which corresponds to the animationRunning array</param>
    /// <returns></returns>
    private IEnumerator Animate(EventAnimation animation, int index)
    {
        GameObject animatedObject = animation.animatedObject;
        Vector3 nextPosition = animation.nextPosition == null ? animatedObject.transform.position : (Vector3)animation.nextPosition;
        Quaternion nextRotation = animation.nextRotation == null ? animatedObject.transform.rotation : (Quaternion)animation.nextRotation;

        //set flag
        animationRunning[index] = true;

        //sync up translation and rotation speed
        float translationTime = Vector3.Distance(animatedObject.transform.position, nextPosition) / Config.translationMaxDistanceDelta;     //in seconds
        float rotationTime = Quaternion.Angle(animatedObject.transform.rotation, nextRotation) / Config.rotationMaxDegreesDelta;            //in seconds

        float totalTime = Mathf.Max(translationTime, rotationTime);
        float clampedVelocity = Vector3.Distance(animatedObject.transform.position, nextPosition) / totalTime;
        float clampedAngularVelocity = Quaternion.Angle(animatedObject.transform.rotation, nextRotation) / totalTime;

        //animate
        while (animatedObject.transform.position != nextPosition || animatedObject.transform.rotation != nextRotation)
        {
            animatedObject.transform.position = Vector3.MoveTowards(animatedObject.transform.position, nextPosition, clampedVelocity * Time.deltaTime);
            animatedObject.transform.rotation = Quaternion.RotateTowards(animatedObject.transform.rotation, nextRotation, clampedAngularVelocity * Time.deltaTime);
            yield return null;
        }

        //reset flag
        animationRunning[index] = false;
    }

    #endregion
}