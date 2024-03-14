using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Steal.Components
{
    public class VelocityTracker : MonoBehaviour
    {
        Vector3 rotationLast;
        Vector3 rotationDelta;
        Vector3 _previousvelocity;
        Vector3 _velocity;

        void Start()
        {
            rotationLast = transform.rotation.eulerAngles;
        }

        void Update()
        {
            rotationDelta = transform.rotation.eulerAngles - rotationLast;
            rotationLast = transform.rotation.eulerAngles;
            _velocity = (transform.position - _previousvelocity) / Time.deltaTime;
            _previousvelocity = transform.position;
        }
        public Vector3 angularVelocity
        {
            get
            {
                return rotationDelta;
            }
        }
        public Vector3 velocity
        {
            get
            {
                return _velocity;
            }
        }
    }
}
