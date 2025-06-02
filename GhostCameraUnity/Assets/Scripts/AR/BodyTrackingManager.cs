// Unity AR Foundation을 사용하여 인체 추적 및 귀신 모델 배치
// 이 스크립트는 ARHumanBodyManager를 사용하여 인체를 추적하고,
// 인체의 어깨 위치를 기준으로 귀신 모델을 배치합니다.
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class BodyTrackingManager : MonoBehaviour
{
    [SerializeField]
    private ARHumanBodyManager bodyManager;

    // 귀신 모델 prefab을 Inspector에서 할당
    [SerializeField]
    private GameObject ghostPrefab;

    private GameObject spawnedGhost;

    void Awake()
    {
        if (bodyManager == null)
            bodyManager = GetComponent<ARHumanBodyManager>();
    }

    void OnEnable()
    {
        bodyManager.trackablesChanged.AddListener(OnHumanBodiesChanged);
    }

    void OnDisable()
    {
        bodyManager.trackablesChanged.RemoveListener(OnHumanBodiesChanged);
    }

    void OnHumanBodiesChanged(ARTrackablesChangedEventArgs<ARHumanBody> args)
    {
        foreach (var humanBody in args.added)
        {
            ProcessBody(humanBody);
        }
        foreach (var humanBody in args.updated)
        {
            ProcessBody(humanBody);
        }
    }

    void ProcessBody(ARHumanBody humanBody)
    {
        if (humanBody.joints.IsCreated && humanBody.joints.Length > 0)
        {
            XRHumanBodyJoint leftShoulder;

            if (TryGetJoint(humanBody, HumanBodyJointType.LeftShoulder, out leftShoulder))
            {
                
                Vector3 leftShoulderPos = humanBody.transform.TransformPoint(leftShoulder.anchorPose.position);
                if (spawnedGhost == null)
                {
                    spawnedGhost = Instantiate(ghostPrefab, leftShoulderPos, Quaternion.identity);
                }
                else
                {
                    spawnedGhost.transform.position = leftShoulderPos; // 💡 즉시 위치 업데이트 추가!
                }
            }
        }
    }

    bool TryGetJoint(ARHumanBody humanBody, HumanBodyJointType jointType, out XRHumanBodyJoint joint)
    {
        joint = default;
        bool found = false;
        foreach (var j in humanBody.joints)
        {
            if (j.index == (int)jointType) // ✅ 'index'를 사용해 관절 ID 비교
            {
                joint = j;
                found = true;
                break;
            }
        }
        return found;
    }
}

// 단순 예시용 enum (실제 ARFoundation에서 제공하는 관절 ID를 사용하세요)
public enum HumanBodyJointType
{
    Head = 0,
    Neck = 1,
    Spine = 2,
    Hips = 3,
    LeftHip = 4,
    RightHip = 5,
    LeftKnee = 6,
    RightKnee = 7,
    LeftFoot = 8,
    RightFoot = 9,
    LeftShoulder = 11,
    RightShoulder = 12,
    LeftElbow = 13,
    RightElbow = 14,
    LeftHand = 15,
    RightHand = 16
}